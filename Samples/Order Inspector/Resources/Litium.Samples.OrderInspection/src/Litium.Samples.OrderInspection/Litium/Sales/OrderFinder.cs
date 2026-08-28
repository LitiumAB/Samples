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

        public async Task<IReadOnlyList<SalesOrder>> FindOrdersAsync(
           System.DateTimeOffset? startDate,
           System.DateTimeOffset? endDate,
           CancellationToken cancellationToken = default)
        {
            if (startDate.HasValue && endDate.HasValue && startDate.Value > endDate.Value)
            {
                return [];
            }

            const int pageSize = 200;
            var skip = 0;
            var orders = new List<SalesOrder>();

            var filters = new List<FilterModel>();
            if (startDate.HasValue || endDate.HasValue)
            {
                filters.Add(new FilterModel
                {
                    AdditionalProperties = new Dictionary<string, object?>
                    {
                        ["$type"] = "Litium.Data.Queryable.Conditions.DateRangeFilterCondition, Litium.Abstractions",
                        ["operator"] = "daterange",
                        ["fromDate"] = startDate,
                        ["toDate"] = endDate
                    }
                });
            }

            while (true)
            {
                var page = await _salesOrderClient
                    .Litium_Sales_SalesOrders_SearchAsync(
                        new SearchModel
                        {
                            Take = pageSize,
                            Skip = skip,
                            Filter = filters.Count > 0 ? filters : null
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

        public async Task<IReadOnlyList<SalesOrder>> FindOrdersByDateRangeOrderStateAsync(
           string? orderState,
           System.DateTimeOffset? startDate,
           System.DateTimeOffset? endDate,
           CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(orderState))
            {
                return [];
            }

            if (startDate.HasValue && endDate.HasValue && startDate.Value > endDate.Value)
            {
                return [];
            }

            const int pageSize = 200;
            var skip = 0;
            var orders = new List<SalesOrder>();
            var normalizedOrderState = orderState.Trim().ToLowerInvariant();

            var filters = new List<FilterModel>
            {
                new FilterModel
                {
                    AdditionalProperties = new Dictionary<string, object?>
                    {
                        ["$type"] = "Litium.Data.Queryable.Conditions.FieldFilterCondition, Litium.Abstractions",
                        ["id"] = "__orderStatus",
                        ["operator"] = "contains",
                        ["value"] = new[] { normalizedOrderState }
                    }
                }
            };

            if (startDate.HasValue || endDate.HasValue)
            {
                filters.Add(new FilterModel
                {
                    AdditionalProperties = new Dictionary<string, object?>
                    {
                        ["$type"] = "Litium.Data.Queryable.Conditions.DateRangeFilterCondition, Litium.Abstractions",
                        ["operator"] = "daterange",
                        ["fromDate"] = startDate,
                        ["toDate"] = endDate
                    }
                });
            }

            while (true)
            {
                var page = await _salesOrderClient
                    .Litium_Sales_SalesOrders_SearchAsync(
                        new SearchModel
                        {
                            Take = pageSize,
                            Skip = skip,
                            Filter = filters
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

        public async Task<IReadOnlyList<SalesOrder>> FindOrdersByDateRangeTagsAsync(
           string tags,
           System.DateTimeOffset? startDate,
           System.DateTimeOffset? endDate,
           bool matchAll = false,
           CancellationToken cancellationToken = default)
        {
            var requestedTags = ParseTags(tags);

            if (startDate.HasValue && endDate.HasValue && startDate.Value > endDate.Value)
            {
                return [];
            }

            const int pageSize = 200;
            var skip = 0;
            var orders = new List<SalesOrder>();

            var filters = new List<FilterModel>();
            if (startDate.HasValue || endDate.HasValue)
            {
                filters.Add(new FilterModel
                {
                    AdditionalProperties = new Dictionary<string, object?>
                    {
                        ["$type"] = "Litium.Data.Queryable.Conditions.DateRangeFilterCondition, Litium.Abstractions",
                        ["operator"] = "daterange",
                        ["fromDate"] = startDate,
                        ["toDate"] = endDate
                    }
                });
            }

            if (requestedTags.Count > 0)
            {
                if (matchAll)
                {
                    filters.AddRange(requestedTags.Select(tag => new FilterModel
                    {
                        AdditionalProperties = new Dictionary<string, object?>
                        {
                            ["$type"] = "Litium.Data.Queryable.Conditions.TaggingFilterCondition, Litium.Abstractions",
                            ["operator"] = "contains",
                            ["value"] = new[] { tag }
                        }
                    }));
                }
                else
                {
                    filters.Add(new FilterModel
                    {
                        AdditionalProperties = new Dictionary<string, object?>
                        {
                            ["$type"] = "Litium.Data.Queryable.Conditions.TaggingFilterCondition, Litium.Abstractions",
                            ["operator"] = "contains",
                            ["value"] = requestedTags.ToArray()
                        }
                    });
                }
            }

            while (true)
            {
                var page = await _salesOrderClient
                    .Litium_Sales_SalesOrders_SearchAsync(
                        new SearchModel
                        {
                            Take = pageSize,
                            Skip = skip,
                            Filter = filters
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
            if(string.IsNullOrWhiteSpace(tags))
            {
                return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            }

            return tags
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
        }
    }
}