using Litium.Samples.OrderInspection.LitiumApis.Generated.Admin;

namespace Litium.Samples.OrderInspection.Litium.Sales
{
    internal static class TransactionRowMapper
    {
        public static TransactionRow FromShipmentRow(ShipmentRow shipmentRow)
        {
            return new TransactionRow
            {
                OrderRowSystemId = shipmentRow.OrderRowSystemId,
                OriginOrderRowSystemId = shipmentRow.OriginOrderRowSystemId,
                RowType = GetRowType(shipmentRow.OrderRowType, shipmentRow.ProductType),
                ShipmentRowSystemId = shipmentRow.SystemId,
                Description = shipmentRow.Description,
                UnitPriceIncludingVat = shipmentRow.UnitPriceIncludingVat,
                UnitPriceExcludingVat = shipmentRow.UnitPriceExcludingVat,
                Quantity = shipmentRow.Quantity,
                VatRate = shipmentRow.VatRate,
                TotalIncludingVat = shipmentRow.TotalIncludingVat,
                TotalExcludingVat = shipmentRow.TotalExcludingVat,
                TotalVat = shipmentRow.TotalVat,
                AdditionalInfo = shipmentRow.AdditionalInfo?.ToDictionary(s => s.Key, s => s.Value),
                ArticleNumber = shipmentRow.ArticleNumber,
                VatDetails = shipmentRow.VatDetails?.Select(CloneVatDetail).ToList(),
                TaxDetails = shipmentRow.TaxDetails?.Select(CloneTaxDetail).ToList()
            };
        }

        private static VatDetail CloneVatDetail(VatDetail vatDetail)
        {
            return new VatDetail
            {
                VatRate = vatDetail.VatRate,
                AmountIncludingVat = vatDetail.AmountIncludingVat,
                Vat = vatDetail.Vat,
                AdditionalProperties = vatDetail.AdditionalProperties is null
                    ? null
                    : vatDetail.AdditionalProperties.ToDictionary(x => x.Key, x => x.Value)
            };
        }

        private static TaxDetail CloneTaxDetail(TaxDetail taxDetail)
        {
            return new TaxDetail
            {
                Name = taxDetail.Name,
                Type = taxDetail.Type,
                TaxableAmount = taxDetail.TaxableAmount,
                TaxRate = taxDetail.TaxRate,
                Tax = taxDetail.Tax,
                AdditionalProperties = taxDetail.AdditionalProperties is null
                    ? null
                    : taxDetail.AdditionalProperties.ToDictionary(x => x.Key, x => x.Value)
            };
        }

        private static TransactionRowRowType GetRowType(ShipmentRowOrderRowType orderRowType, ShipmentRowProductType productType)
        {
            return orderRowType switch
            {
                ShipmentRowOrderRowType.ShippingFee => TransactionRowRowType.ShippingFee,
                ShipmentRowOrderRowType.Tax => TransactionRowRowType.Tax,
                ShipmentRowOrderRowType.Fee => TransactionRowRowType.Fee,
                ShipmentRowOrderRowType.Discount => TransactionRowRowType.Discount,
                ShipmentRowOrderRowType.RoundingOffAdjustment => TransactionRowRowType.RoundingOffAdjustment,
                _ => productType switch
                {
                    ShipmentRowProductType.DigitalGoods => TransactionRowRowType.DigitalGoods,
                    ShipmentRowProductType.PhysicalGoods => TransactionRowRowType.PhysicalGoods,
                    ShipmentRowProductType.Service => TransactionRowRowType.Service,
                    _ => TransactionRowRowType.Unknown,
                },
            };
        }
    }
}
