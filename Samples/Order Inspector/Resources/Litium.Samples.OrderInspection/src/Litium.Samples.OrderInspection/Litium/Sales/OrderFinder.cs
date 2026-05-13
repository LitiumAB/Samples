using Litium.Samples.OrderInspection.LitiumApis.Generated.Admin;

namespace Litium.Samples.OrderInspection.Litium.Sales
{
    public class OrderFinder
    {
        private readonly ISales_sales_orderClient _salesOrderClient;

        public OrderFinder(ISales_sales_orderClient salesOrderClient)
        {
            _salesOrderClient = salesOrderClient;
        }


        public async Task<IReadOnlyList<SalesOrder>> FindOrdersByTagAsync(
            string tags,
            bool matchAll = false,
            CancellationToken cancellationToken = default)
        {
            var requestedTags = ParseTags(tags);
            if (requestedTags.Count == 0)
            {
                return [];
            }

            return await GetAllOrdersAsync(requestedTags, cancellationToken, matchAll).ConfigureAwait(false);
        }


        private async Task<IReadOnlyList<SalesOrder>> GetAllOrdersAsync(
            IReadOnlyCollection<string> requestedTags,
            CancellationToken cancellationToken,
            bool matchAll = false)
        {
            const int pageSize = 200;
            var skip = 0;
            var orders = new List<SalesOrder>();

            List<FilterModel> tagFilters;
            if (matchAll)
            {
                // One filter per tag, all must match (server-side AND)
                tagFilters = requestedTags
                    .Select(tag => new FilterModel
                    {
                        AdditionalProperties = new Dictionary<string, object>
                        {
                            ["$type"] = "Litium.Data.Queryable.Conditions.TaggingFilterCondition, Litium.Abstractions",
                            ["operator"] = "contains",
                            ["value"] = new[] { tag }
                        }
                    })
                    .ToList();
            }
            else
            {
                // Single filter, any tag match (server-side OR)
                tagFilters = new List<FilterModel>
                {
                    new FilterModel
                    {
                        AdditionalProperties = new Dictionary<string, object>
                        {
                            ["$type"] = "Litium.Data.Queryable.Conditions.TaggingFilterCondition, Litium.Abstractions",
                            ["operator"] = "contains",
                            ["value"] = requestedTags.ToArray()
                        }
                    }
                };
            }

            while (true)
            {
                var page = await _salesOrderClient
                    .Litium_Sales_SalesOrders_SearchAsync(
                        new SearchModel
                        {
                            Take = pageSize,
                            Skip = skip,
                            Filter = tagFilters
                        },
                        cancellationToken)
                    .ConfigureAwait(false);

                var items = page?.Items?.ToList() ?? [];
                if (items.Count == 0)
                {
                    break;
                }

                orders.AddRange(items);
                skip += items.Count;

                if (skip >= page!.Total)
                {
                    break;
                }
            }

            return orders;
        }

        private static HashSet<string> ParseTags(string tags)
        {
            return tags
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
        }
    }
}