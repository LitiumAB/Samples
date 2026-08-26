using Litium.Samples.OrderInspection.LitiumApis.Generated.Admin;

namespace Litium.Samples.OrderInspection.Litium.Sales;

public sealed class OrderSearchService(ISales_sales_orderClient salesOrderClient)
{
    private const int PageSize = 200;
    private const int MaxPageCount = 10;
    private const string ConditionNamespace = "Litium.Data.Queryable.Conditions";
    private const string AssemblyName = "Litium.Abstractions";

    private readonly ISales_sales_orderClient _salesOrderClient = salesOrderClient;

    public async Task<IReadOnlyList<string>> SearchAsync(
        System.DateTimeOffset startDate,
        System.DateTimeOffset endDate,
        string orderStatus,
        string shippingStatus,
        string orderTag,
        CancellationToken cancellationToken = default)
    {
        var filters = CreateFilters(startDate, endDate, orderStatus, shippingStatus, orderTag);
        var orders = new List<SalesOrder>();
        var pageSignatures = new HashSet<string>(StringComparer.Ordinal);
        var skip = 0;
        var pageCount = 0;

        while (true)
        {
            if (++pageCount > MaxPageCount)
            {
                throw new InvalidOperationException("The Admin Web API search exceeded the maximum page count.");
            }

            var page = await _salesOrderClient.Litium_Sales_SalesOrders_SearchAsync(
                new SearchModel
                {
                    Take = PageSize,
                    Skip = skip,
                    Filter = filters
                },
                cancellationToken).ConfigureAwait(false);

            var items = page?.Items?.Where(x => x is not null).ToList() ?? [];

            if (items.Count > 0)
            {
                var pageSignature = string.Join(",", items.Select(x => $"{x.SystemId}:{x.Id}"));
                if (!pageSignatures.Add(pageSignature))
                {
                    throw new InvalidOperationException("The Admin Web API returned the same page repeatedly.");
                }
            }

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

    private static List<FilterModel> CreateFilters(
        System.DateTimeOffset startDate,
        System.DateTimeOffset endDate,
        string orderStatus,
        string shippingStatus,
        string orderTag)
    {
        if (endDate < startDate)
        {
            throw new OrderSearchFormatException("endDate must be greater than or equal to startDate.");
        }

        if (string.IsNullOrWhiteSpace(orderStatus))
        {
            throw new OrderSearchFormatException("orderStatus is required.");
        }

        if (string.IsNullOrWhiteSpace(shippingStatus))
        {
            throw new OrderSearchFormatException("shippingStatus is required.");
        }

        if (string.IsNullOrWhiteSpace(orderTag))
        {
            throw new OrderSearchFormatException("orderTag is required.");
        }

        return
        [
            CreateValueFilter("OrderCustomerFilterCondition", "contains", [orderStatus]),
            CreateValueFilter("OrderCustomerFilterCondition", "contains", [shippingStatus]),
            CreateValueFilter("TaggingFilterCondition", "contains", [orderTag]),
            CreateDateFilter(startDate, endDate)
        ];
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

    private static FilterModel CreateDateFilter(
        System.DateTimeOffset startDate,
        System.DateTimeOffset endDate)
    {
        return new FilterModel
        {
            AdditionalProperties = new Dictionary<string, object>
            {
                ["$type"] = $"{ConditionNamespace}.DateRangeFilterCondition, {AssemblyName}",
                ["operator"] = "daterange",
                ["fromDate"] = startDate,
                ["toDate"] = endDate
            }
        };
    }
}

public sealed class OrderSearchFormatException(string message) : Exception(message);
