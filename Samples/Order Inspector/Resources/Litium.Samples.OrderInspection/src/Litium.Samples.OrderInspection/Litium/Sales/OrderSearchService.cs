using Litium.Samples.OrderInspection.LitiumApis.Generated.Admin;

namespace Litium.Samples.OrderInspection.Litium.Sales;

public sealed class OrderSearchService(ISales_sales_orderClient salesOrderClient)
{
    private const int PageSize = 200;
    private const string ConditionNamespace = "Litium.Data.Queryable.Conditions";
    private const string AssemblyName = "Litium.Abstractions";

    private readonly ISales_sales_orderClient _salesOrderClient = salesOrderClient;

    public async Task<IReadOnlyList<string>> SearchAsync(string backofficeUrl, CancellationToken cancellationToken = default)
    {
        var filters = ParseFilters(backofficeUrl);
        var orders = new List<SalesOrder>();
        var skip = 0;

        while (true)
        {
            var page = await _salesOrderClient.Litium_Sales_SalesOrders_SearchAsync(
                new SearchModel
                {
                    Take = PageSize,
                    Skip = skip,
                    Filter = filters
                },
                cancellationToken).ConfigureAwait(false);

            var items = page?.Items?.Where(x => x is not null).ToList() ?? [];
            orders.AddRange(items!);

            if (items.Count == 0 || orders.Count >= page?.Total || items.Count < PageSize)
            {
                break;
            }

            skip += items.Count;
        }

        return orders
            .Select(x => x.Id)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.Ordinal)
            .ToList()!;
    }

    private static List<FilterModel> ParseFilters(string backofficeUrl)
    {
        if (!Uri.TryCreate(backofficeUrl, UriKind.Absolute, out var uri))
        {
            throw new OrderSearchFormatException("The supplied value must be an absolute backoffice URL.");
        }

        var encodedFilters = uri.Query
            .TrimStart('?')
            .Split('&', StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Split('=', 2))
            .FirstOrDefault(x => x.Length == 2 && string.Equals(x[0], "filters", StringComparison.OrdinalIgnoreCase))?[1];

        if (string.IsNullOrWhiteSpace(encodedFilters))
        {
            throw new OrderSearchFormatException("The backoffice URL must contain a filters query parameter.");
        }

        // Decode only once here. Repeated decoding can turn encoded ':' (%3A) inside date values
        // into literal separators before ParseFilter splits on ':', which corrupts daterange tokens.
        var filterText = Uri.UnescapeDataString(encodedFilters);
        var result = new List<FilterModel>();
        foreach (var clause in filterText.Split('|', StringSplitOptions.RemoveEmptyEntries))
        {
            result.Add(ParseFilter(clause));
        }

        if (result.Count == 0)
        {
            throw new OrderSearchFormatException("The filters query parameter does not contain any filters.");
        }

        return result;
    }

    private static FilterModel ParseFilter(string clause)
    {
        var parts = clause.Split(':');
        if (parts.Length < 5)
        {
            throw new OrderSearchFormatException($"Invalid filter clause '{clause}'.");
        }

        var field = parts[0];
        var operatorName = parts[2];
        var value = parts[4];

        return field switch
        {
            "__orderStatus" => CreateValueFilter("OrderCustomerFilterCondition", operatorName, [value]),
            "__shipmentStatus" => CreateValueFilter("OrderCustomerFilterCondition", operatorName, [value]),
            "__tags" => CreateValueFilter("TaggingFilterCondition", operatorName, [value]),
            "__orderDate" => CreateDateFilter(parts, operatorName),
            _ => throw new OrderSearchFormatException($"Filter field '{field}' is not supported.")
        };
    }

    private static FilterModel CreateValueFilter(string conditionName, string operatorName, string[] values)
    {
        if (!string.Equals(operatorName, "contains", StringComparison.OrdinalIgnoreCase))
        {
            throw new OrderSearchFormatException($"Operator '{operatorName}' is not supported for this filter.");
        }

        return new FilterModel
        {
            AdditionalProperties = new Dictionary<string, object>
            {
                ["$type"] = $"{ConditionNamespace}.{conditionName}, {AssemblyName}",
                ["operator"] = operatorName,
                ["value"] = values
            }
        };
    }

    private static FilterModel CreateDateFilter(string[] parts, string operatorName)
    {
        if (!string.Equals(operatorName, "daterange", StringComparison.OrdinalIgnoreCase) || parts.Length < 13)
        {
            throw new OrderSearchFormatException("The order date filter must be a daterange with both date bounds.");
        }

        if (!System.DateTimeOffset.TryParse(DecodeUntilStable(parts[11]), out var fromDate) ||
            !System.DateTimeOffset.TryParse(DecodeUntilStable(parts[12]), out var toDate))
        {
            throw new OrderSearchFormatException("The order date filter contains an invalid date range.");
        }

        return new FilterModel
        {
            AdditionalProperties = new Dictionary<string, object>
            {
                ["$type"] = $"{ConditionNamespace}.DateRangeFilterCondition, {AssemblyName}",
                ["operator"] = operatorName,
                ["fromDate"] = fromDate,
                ["toDate"] = toDate
            }
        };
    }

    private static string DecodeUntilStable(string value)
    {
        var decoded = value;
        for (var i = 0; i < 5; i++)
        {
            var next = Uri.UnescapeDataString(decoded);
            if (next == decoded)
            {
                break;
            }

            decoded = next;
        }

        return decoded;
    }
}

public sealed class OrderSearchFormatException(string message) : Exception(message);
