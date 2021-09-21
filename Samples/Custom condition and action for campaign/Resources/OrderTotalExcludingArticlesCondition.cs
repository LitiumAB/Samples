using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Litium.Foundation.Modules.ECommerce;
using Litium.Foundation.Modules.ECommerce.Carriers;
using Litium.Foundation.Modules.ECommerce.Plugins.Campaigns;
using Litium.Foundation.Modules.ECommerce.Plugins.Campaigns.ConditionTypes;
using Litium.Foundation.Modules.ExtensionMethods;
using Litium.Foundation.Security;

namespace Litium.Studio.KC.Samples.Campaigns
{
    /// <summary>
    /// Order total condition
    /// </summary>
    public class OrderTotalExcludingArticlesCondition : CartConditionType
    {
        private const string PATH = "~/Site/ECommerce/Campaigns/OrderTotalExcludingArticlesConditionControl.ascx";
        Data m_data;

        /// <summary>
        /// Initializes the specified data.
        /// </summary>
        /// <param name="data">The data.</param>
        protected override void Initialize(object data)
        {
            base.Initialize(data);
            m_data = (Data)data;
        }

        /// <summary>
        /// Gets the panel path.
        /// </summary>
        /// <value>The panel path.</value>
        public override string PanelPath { get { return PATH; } }

        /// <summary>
        /// Gets a value indicating whether condition depends on order total.
        /// If true, an order total calculation will be done before invoking the condition.
        /// </summary>
        /// <value>
        ///     <c>true</c> if condition depends on order total otherwise, <c>false</c>.
        /// </value>
        public override bool DependsOnOrderTotal
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Evaluates the specified condition.
        /// </summary>
        /// <param name="conditionArgs">The condition args.</param>
        /// <returns></returns>
        protected override IEnumerable<OrderCarrier> Evaluate(ConditionArgs conditionArgs)
        {
            decimal subTotal = 0;
            switch (m_data.SelectedIdType)
            {
                case Data.SelectionCriteria.ProductLists:
                    subTotal = (m_data.IncludeVat) ?
                         conditionArgs.OrderRows.Where(x => !m_data.SelectedIDs.Intersect(x.ProductSetIDs).Any()).Select(y => y.OrderRowCarrier.TotalPriceWithVAT).Sum() :
                         conditionArgs.OrderRows.Where(x => !m_data.SelectedIDs.Intersect(x.ProductSetIDs).Any()).Select(y => y.OrderRowCarrier.TotalPrice).Sum();
                    break;
            }

            return (from order in conditionArgs.OrderCarriers
                    where
                        (m_data.ShouldBeEqualOrMore && (subTotal >= m_data.TotalPrice))
                        || (!m_data.ShouldBeEqualOrMore && (subTotal <= m_data.TotalPrice))
                    select order);
        }

        /// <summary>
        /// Condition data.
        /// </summary>
        [Serializable]
        public class Data
        {
            /// <summary>
            /// Gets or sets the total price.
            /// </summary>
            /// <value>The total price.</value>
            public decimal TotalPrice { get; set; }
            /// <summary>
            /// Gets or sets a value indicating whether [include vat].
            /// </summary>
            /// <value><c>true</c> if [include vat]; otherwise, <c>false</c>.</value>
            public bool IncludeVat { get; set; }
            /// <summary>
            /// Gets or sets a value indicating whether [should be more].
            /// </summary>
            /// <value><c>true</c> if [should be more]; otherwise, <c>false</c>.</value>
            public bool ShouldBeEqualOrMore { get; set; }

            /// <summary>
            /// Define the type of Ids selected for the <see cref="Data.SelectedIDs"/> property.
            /// </summary>
            public enum SelectionCriteria
            {
                /// <summary>
                /// Product list ids.
                /// </summary>
                ProductLists
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
        }

        /// <summary>
        /// Gets the description.
        /// </summary>
        /// <param name="languageID">The language ID.</param>
        /// <returns></returns>
        public override string GetDescription(Guid languageID)
        {
            string description = string.Empty;
            try
            {
                SecurityToken token = ModuleECommerce.Instance.AdminToken;
                StringBuilder sb = new StringBuilder();
                if (m_data != null)
                {
                    string articleName = string.Empty;
                    if (m_data.IncludeVat)
                    {
                        var withVatDescription = ModuleECommerce.Instance.Strings.GetValue("OrderTotalConditionDescriptionWithVAT", languageID, true);
                        sb.AppendFormat(withVatDescription, m_data.ShouldBeEqualOrMore ? " >= " : " <= ", m_data.TotalPrice);
                    }
                    else
                    {
                        var withoutVatdescription = ModuleECommerce.Instance.Strings.GetValue("OrderTotalConditionDescriptionWithoutVAT", languageID, true);
                        sb.AppendFormat(withoutVatdescription, m_data.ShouldBeEqualOrMore ? " >= " : " <= ", m_data.TotalPrice);
                    }
                    sb.AppendLine();
                    switch (m_data.SelectedIdType)
                    {
                        case Data.SelectionCriteria.ProductLists:
                            sb.Append(ModuleECommerce.Instance.Strings.GetValue("OrderTotalExcludingArticlesConditionExcludeProductSets", languageID, true));
                            foreach (Guid item in m_data.SelectedIDs)
                            {
                                var productSet = item.GetProductList();
                                if (productSet != null)
                                {
                                    sb.Append(Environment.NewLine);
                                    sb.Append(productSet.GetDisplayName(languageID));
                                }
                            }
                            break;
                    }
                    description = sb.ToString();
                }
                else
                {
                    description = base.GetDescription(languageID);
                }
            }
            catch (Exception)
            {
                description = base.GetDescription(languageID);
            }

            return description;
        }
    }
}
