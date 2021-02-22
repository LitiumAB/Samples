﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Litium.Foundation.Modules.ECommerce;
using Litium.Foundation.Modules.ECommerce.Plugins.Campaigns;
using Litium.Foundation.Modules.ECommerce.Plugins.Campaigns.Actions;
using Litium.Foundation.Modules.ECommerce.Plugins.Campaigns.Conditions;
using Litium.Foundation.Modules.ProductCatalog.Articles;
using Litium.Foundation.Modules.ProductCatalog.Assortments;
using Litium.Foundation.Modules.ProductCatalog.Products;
using Litium.Foundation.Modules.ProductCatalog.ProductSets;
using Litium.Foundation.Security;
using Litium.Products;

namespace Litium.Studio.KC.Samples.Campaigns
{
    /// <summary>
    /// Reduces the article campaign price by a percentage.
    /// </summary>
    public class ReducePriceByAmountAction : CampaignAction
    {
        Data _mData;
        private const string Path = "~/Site/ECommerce/Campaigns/ReducePriceByAmountActionControl.ascx";

        /// <summary>
        /// Action data.
        /// </summary>
        [Serializable]
        public class Data
        {
            /// <summary>
            /// Define the type of Ids selected for the <see cref="Data.SelectedIDs"/> property.
            /// </summary>
            public enum SelectionCriteria
            {                
                /// <summary>
                /// product set ids.
                /// </summary>
                ProductList
            }

            /// <summary>
            /// Gets or sets the selection criteria.
            /// </summary>
            /// <value>The selection criteria.</value>
            public SelectionCriteria SelectedIdType { get; set; }

            /// <summary>
            /// Gets or sets list of selected Ids
            /// </summary>
            /// <value>The selected I ds.</value>
            public List<Guid> SelectedIDs { get; set; }

            /// <summary>
            /// Gets or sets the discount amount.
            /// </summary>
            /// <value>The discount percentage.</value>
            public decimal DiscountAmountWithVAT { get; set; }

            /// <summary>
            /// If true, if there is already a campaign price, this campaign will discount that price.
            /// If there is no campaign price when this campaign is processed, list price will be used.
            /// </summary>
            /// <value>The apply on campaign price cumulatively.</value>
            public bool ApplyOnCampaignPriceCumulatively { get; set; }

            /// <summary>
            /// Gets or sets the max discounts per item in order.
            /// </summary>
            /// <value>
            /// The max discounts per item in order.
            /// </value>
            public int MaxDiscountsPerItemInOrder { get; set; }
        }

        /// <summary>
        /// Initializes the specified data.
        /// </summary>
        /// <param name="data">The data.</param>
        protected override void Initialize(object data)
        {
            base.Initialize(data);
            _mData = (Data)data;
        }

        /// <summary>
        /// Gets the conditions that is required for this action.
        /// </summary>
        /// <returns>
        /// A list of <see cref="ICondition"/> that should be added as conditions for the campaign.
        /// </returns>
        protected override IList<ICondition> GetConditions()
        {
            ICondition condition = null;
            switch (_mData.SelectedIdType)
            {                
                case Data.SelectionCriteria.ProductList:
                    condition = new ProductSetCondition();
                    condition.Initialize(new ProductSetCondition.Data { ProductSetIDs = _mData.SelectedIDs, FilterRows = true });
                    break;
            }
            return new[] { condition };
        }

        /// <summary>
        /// Processes the reduce price by amount action.
        /// </summary>
        /// <remarks>
        /// This action might split order rows, or move quantities between similar order rows.
        /// </remarks>
        /// <param name="actionArgs">The action args.</param>
        /// <returns>
        ///     <c>True</c> if actions is applied, otherwise <c>False</c>.
        /// </returns>
        protected override bool Process(ActionArgs actionArgs)
        {
            if (_mData.MaxDiscountsPerItemInOrder > 0)
            {
                //first give discounts to autogenerated items, ordered by lowest priced items.
                //var orderedRows = actionArgs.FilteredOrderRows.OrderByDescending().ThenBy(x => x.OrderRowCarrier.UnitListPrice).ToList();
                var articleCount = (from o in actionArgs.FilteredOrderRows
                                    group o by o.ArticleID into g
                                    select new { ArticleId = g.Key, Quantity = 0 }).ToDictionary(x => x.ArticleId, y => y.Quantity);

                //order by autogenerated, so auto generated are considered last, then order by quantity, so lower quantities taken first to avoid unnecessary splits.
                var orderedRows = actionArgs.FilteredOrderRows.OrderByDescending(y => y.OrderRowCarrier.IsAutoGenerated).ThenBy(x => x.Quantity).ToList();

                for (int i = 0; i < orderedRows.Count(); i++)
                {
                    FilteredOrderRow item = orderedRows[i];
                    if (articleCount[item.ArticleID] < _mData.MaxDiscountsPerItemInOrder)
                    {
                        if (articleCount[item.ArticleID] + item.Quantity > _mData.MaxDiscountsPerItemInOrder)
                        {
                            int discountQty = _mData.MaxDiscountsPerItemInOrder - articleCount[item.ArticleID];
                            //find a order row already not considered..
                            var nextAvailableItem = orderedRows.FirstOrDefault(x => x.OrderRowCarrier.CampaignID != actionArgs.CampaignID && x.OrderRowCarrier.ID != item.OrderRowCarrier.ID
                                && x.ProductID == item.ProductID && x.ArticleID == item.ArticleID);
                            //move items which will not attract discounts to, this already not considered row.
                            if (nextAvailableItem != null)
                            {
                                int qtyToMove = (int)(item.OrderRowCarrier.Quantity - discountQty);
                                nextAvailableItem.OrderRowCarrier.Quantity += qtyToMove;
                                item.OrderRowCarrier.Quantity = discountQty;
                            }                           
                        }
                        ApplyCampaignToOrderRow(actionArgs.CampaignID, item);
                        articleCount[item.ArticleID] += (int)item.Quantity;
                    }
                }
            }
            else
            {
                foreach (var item in actionArgs.FilteredOrderRows)
                {
                    ApplyCampaignToOrderRow(actionArgs.CampaignID, item);
                }
            }
            return actionArgs.FilteredOrderRows.Count > 0;
        }


        /// <summary>
        /// Gets the type of the action.
        /// </summary>
        /// <value>The type of the action.</value>
        public override ActionType ActionType
        {
            get { return ActionType.Product; }
        }

        /// <summary>
        /// Gets the panel path.
        /// </summary>
        /// <value>The panel path.</value>
        public override string PanelPath
        {
            get { return Path; }
        }

        private ProductListService _productListService = IoC.Resolve<ProductListService>();
        /// <summary>
        /// Gets the description.
        /// </summary>
        /// <param name="languageID">The language ID.</param>
        /// <returns></returns>
        public override string GetDescription(Guid languageID)
        {
            string description;
            try
            {
                var sb = new StringBuilder();
                if (_mData != null)
                {
                    SecurityToken token = ModuleECommerce.Instance.AdminToken;
                    switch (_mData.SelectedIdType)
                    {
                        case Data.SelectionCriteria.ProductList:
                            description = ModuleECommerce.Instance.Strings.GetValue("ReducePriceByAmountActionDescriptionProductSets", languageID, true);
                            sb.AppendFormat(description, _mData.DiscountAmountWithVAT);
                            foreach (Guid item in _mData.SelectedIDs)
                            {
                                var productList = _productListService.Get<ProductList>(item);
                                if (productList != null)
                                {
                                    sb.Append(Environment.NewLine);
                                    sb.Append("- " + productList.Localizations[CultureInfo.CurrentCulture]);
                                }
                            }
                            break;
                    }
                    if (_mData.ApplyOnCampaignPriceCumulatively)
                    {
                        sb.Append(Environment.NewLine);
                        sb.Append(Environment.NewLine);
                        sb.Append(ModuleECommerce.Instance.Strings.GetValue("ReducePriceByAmountActionApplyOnCampaignPrice", languageID, true));
                    }
                    if (_mData.MaxDiscountsPerItemInOrder > 0)
                    {
                        sb.Append(Environment.NewLine);
                        sb.Append(Environment.NewLine);
                        sb.Append(ModuleECommerce.Instance.Strings.GetValue("MaxDiscountsPerOrder", languageID, true));
                        sb.Append(" = ");
                        sb.Append(_mData.MaxDiscountsPerItemInOrder);
                    }
                    description = sb.ToString();
                }
                else
                {
                    description = base.GetDescription(languageID);
                }
            }
            catch
            {
                description = base.GetDescription(languageID);
            }
            return description;
        }

        /// <summary>
        /// Applies the campaign to order row.
        /// </summary>
        /// <param name="campaignId">The campaign id.</param>
        /// <param name="item">The item.</param>
        private void ApplyCampaignToOrderRow(Guid campaignId, FilteredOrderRow item)
        {
            var discountAmount = Decimal.Divide(_mData.DiscountAmountWithVAT, 1 + item.OrderRowCarrier.VATPercentage);
            if (_mData.ApplyOnCampaignPriceCumulatively)
            {
                item.OrderRowCarrier.UnitCampaignPrice = (item.OrderRowCarrier.CampaignID == Guid.Empty) ?
                                                                item.OrderRowCarrier.UnitListPrice - discountAmount :
                                                                            item.OrderRowCarrier.UnitCampaignPrice - discountAmount;
            }
            else
            {
                item.OrderRowCarrier.UnitCampaignPrice = item.OrderRowCarrier.UnitListPrice - discountAmount;
            }

            if (item.OrderRowCarrier.UnitCampaignPrice < 0)
                item.OrderRowCarrier.UnitCampaignPrice = 0;
            item.OrderRowCarrier.CampaignID = campaignId;
        }
    }
}
