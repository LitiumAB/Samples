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

            var allOrders = await GetAllOrdersAsync(requestedTags, cancellationToken).ConfigureAwait(false);
            var ordersWithTags = new List<(SalesOrder Order, HashSet<string> Tags)>();

            foreach (var order in allOrders)
            {
                var orderTags = await _salesOrderClient
                    .Litium_Sales_SalesOrders_GetAllTagsAsync(order.SystemId, cancellationToken)
                    .ConfigureAwait(false);

                var normalizedOrderTags = new HashSet<string>(orderTags ?? [], StringComparer.OrdinalIgnoreCase);
                if (normalizedOrderTags.Overlaps(requestedTags))
                {
                    ordersWithTags.Add((order, normalizedOrderTags));
                }
            }

            if (!matchAll)
            {
                return ordersWithTags.Select(x => x.Order).ToList();
            }

            return ordersWithTags
                .Where(x => requestedTags.All(tag => x.Tags.Contains(tag)))
                .Select(x => x.Order)
                .ToList();
        }

        private async Task<IReadOnlyList<SalesOrder>> GetAllOrdersAsync(
            IReadOnlyCollection<string> requestedTags,
            CancellationToken cancellationToken)
        {
            const int pageSize = 200;
            var skip = 0;
            var orders = new List<SalesOrder>();

            // Use the same typed condition format as Backoffice order-grid tag filtering.
            var tagFilter = new FilterModel
            {
                AdditionalProperties = new Dictionary<string, object>
                {
                    ["$type"] = "Litium.Data.Queryable.Conditions.TaggingFilterCondition, Litium.Abstractions",
                    ["operator"] = "contains",
                    ["value"] = requestedTags.ToArray()
                }
            };

            while (true)
            {
                var page = await _salesOrderClient
                    .Litium_Sales_SalesOrders_SearchAsync(
                        new SearchModel
                        {
                            Take = pageSize,
                            Skip = skip,
                            Filter = [tagFilter]
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