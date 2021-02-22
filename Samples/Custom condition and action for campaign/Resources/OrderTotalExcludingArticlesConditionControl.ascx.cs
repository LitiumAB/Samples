using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
using Litium.Foundation.Modules.ECommerce.Plugins.Campaigns;
using Litium.Studio.UI.ECommerce.Common;
using Litium.Studio.UI.ECommerce.Common.Constants;
using Litium.Products;
using System.Globalization;

namespace Litium.Studio.KC.Samples.Campaigns
{
    public partial class OrderTotalExcludingArticlesConditionControl : UserBaseControl, IUIExtensionPanel
    {
        private const string IS_GREATER_THAN_OR_EQUAL = "IsGreaterThanOrEqual";
        private const string IS_LESS_THAN_OR_EQUAL = "IsLessThanOrEqual";

        protected override void OnInit(EventArgs e)
        {
            Page.RegisterRequiresControlState(this);
            InitializePageControls();
            base.OnInit(e);
        }

        protected override object SaveControlState()
        {
            return m_data;
        }

        protected override void LoadControlState(object state)
        {
            if (state != null)
            {
                m_data = state as OrderTotalExcludingArticlesCondition.Data;
            }
        }

        private ProductListService _productListService = IoC.Resolve<ProductListService>();

        OrderTotalExcludingArticlesCondition.Data Data
        {
            set
            {
                m_data = value;
            }
            get
            {
                if (m_data == null)
                    return m_data = new OrderTotalExcludingArticlesCondition.Data { SelectedIDs = new List<Guid>(), SelectedIdType = OrderTotalExcludingArticlesCondition.Data.SelectionCriteria.ProductLists };
                return m_data;
            }
        }

        private OrderTotalExcludingArticlesCondition.Data m_data;

        /// <summary>
        /// Initializes the validation on page.
        /// </summary>
        private void InitializeValidation()
        {
            c_regExQuantityValidator.CssClass = ParameterConstants.VALIDATOR_STYLE_NAME;
            c_regExQuantityValidator.Text = string.Format("{0} {1}", ParameterConstants.VALIDATOR_IMAGE,
                SystemStrings.GetValue(Foundation.GUI.Constants.SystemStringConstants.VALIDATION_MESSAGE_INVALID_INPUT,
                    FoundationContext.LanguageID, true));

            var digitsSeparator = FoundationContext.Culture.NumberFormat.CurrencyDecimalSeparator;
            c_regExQuantityValidator.ValidationExpression = string.Format(@"^\d*[0-9](|\{0}\d*[0-9])?$", digitsSeparator);

            c_requiredQuantityValidator.CssClass = ParameterConstants.VALIDATOR_STYLE_NAME;
            c_requiredQuantityValidator.Text = string.Format("{0} {1}", ParameterConstants.VALIDATOR_IMAGE,
                SystemStrings.GetValue(Foundation.GUI.Constants.SystemStringConstants.VALIDATION_MESSAGE_REQUIRED_FIELD,
                    FoundationContext.LanguageID, true));
        }

        /// <summary>
        /// Initializes the condition drop down.
        /// </summary>
        private void InitializeConditionDropDown()
        {
            if (c_conditionDropDownList.Items.Count == 0)
            {
                var text = CurrentModule.Strings.GetValue(IS_GREATER_THAN_OR_EQUAL, FoundationContext.LanguageID, true);
                c_conditionDropDownList.Items.Add(new ListItem(text, IS_GREATER_THAN_OR_EQUAL));
                text = CurrentModule.Strings.GetValue(IS_LESS_THAN_OR_EQUAL, FoundationContext.LanguageID, true);
                c_conditionDropDownList.Items.Add(new ListItem(text, IS_LESS_THAN_OR_EQUAL));
            }
        }


        /// <summary>
        /// Initializes the page controls.
        /// </summary>
        private void InitializePageControls()
        {
            InitializeConditionDropDown();
            InitializeProductListDropDown();
            InitializeValidation();
        }

        private void InitializeProductListDropDown()
        {
            if (c_ProductListGrid.Rows.Count == 0)
            {
                c_ProductListGrid.DataSource = _productListService.GetAll().Select(x => new { Id = x.SystemId, x.Localizations[CultureInfo.CurrentCulture].Name });
                c_ProductListGrid.DataBind();
            }
        }

        /// <summary>
        /// Gets the panel data.
        /// </summary>
        /// <returns>Return an object with the panel data.</returns>
        object IUIExtensionPanel.GetPanelData()
        {
            decimal totalPrice;
            decimal.TryParse(c_textBoxPay.Text, out totalPrice);
            Data.TotalPrice = totalPrice;
            Data.IncludeVat = c_includeVat.Checked;
            Data.ShouldBeEqualOrMore = c_conditionDropDownList.Items.FindByValue(IS_GREATER_THAN_OR_EQUAL).Selected;
            Data.SelectedIdType = OrderTotalExcludingArticlesCondition.Data.SelectionCriteria.ProductLists;
            Data.SelectedIDs = new List<Guid>();
            for (var i = 0; i < c_ProductListGrid.Rows.Count; i++)
            {
                var cbSelection = c_ProductListGrid.Rows[i].FindControl("RowSelector") as CheckBox;
                if (cbSelection.Checked)
                {
                    Data.SelectedIDs.Add(Guid.Parse(c_ProductListGrid.DataKeys[i].Value.ToString()));
                }
            }
            return Data;
        }

        /// <summary>
        /// Sets the panel data.
        /// </summary>
        /// <param name="data">The data.</param>
        void IUIExtensionPanel.SetPanelData(object data)
        {
            if (data is OrderTotalExcludingArticlesCondition.Data)
                Data = (OrderTotalExcludingArticlesCondition.Data)data;

            InitializePageControls();

            if (data is OrderTotalExcludingArticlesCondition.Data)
            {
                c_hiddenFieldIDs.Value = string.Join(",", Data.SelectedIDs.ToList().ConvertAll(x => x.ToString()).ToArray());
                c_textBoxPay.Text = Data.TotalPrice.ToString();
                c_includeVat.Checked = Data.IncludeVat;
                if (!Data.ShouldBeEqualOrMore)
                {
                    var item = c_conditionDropDownList.Items.FindByValue(IS_LESS_THAN_OR_EQUAL);
                    if (item != null)
                        item.Selected = true;
                }

                var selectedIds = Data.SelectedIDs.ToList();
                if (selectedIds.Count > 0)
                {
                    for (var i = 0; i < c_ProductListGrid.Rows.Count; i++)
                    {
                        if (selectedIds.IndexOf(Guid.Parse(c_ProductListGrid.DataKeys[i].Value.ToString())) >= 0)
                        {
                            var cbSelection = c_ProductListGrid.Rows[i].FindControl("RowSelector") as CheckBox;
                            cbSelection.Checked = true;
                        }
                    }
                }
            }
        }
    }
}
