using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.UI.WebControls;
using Litium.Foundation.Modules.ECommerce.Plugins.Campaigns;
using Litium.Products;
using Litium.Studio.KC.Samples.Campaigns;
using Litium.Studio.UI.ECommerce.Common;
using Litium.Studio.UI.ECommerce.Common.Constants;
using Telerik.Web.UI;
using SystemStringConstants = Litium.Foundation.GUI.Constants.SystemStringConstants;

namespace Litium.Studio.KC.Samples.Campaigns
{
    public partial class ReducePriceByAmountActionControl : UserBaseControl, IUIExtensionPanel
    {
        #region Control State

        protected override void OnInit(EventArgs e)
        {
            Page.RegisterRequiresControlState(this);
            InitializePageControls();
            base.OnInit(e);
        }

        protected override object SaveControlState()
        {
            return _data;
        }

        protected override void LoadControlState(object state)
        {
            if (state != null)
            {
                _data = state as ReducePriceByAmountAction.Data;
            }
        }

        #endregion Control State

        #region Constants

        protected static class DialogType
        {
            public const string SELECT_PRODUCT_SET = "5";
        }

        #endregion

        #region Property

        ReducePriceByAmountAction.Data Data
        {
            set
            {
                _data = value;
            }
            get
            {
                if (_data == null)
                    return _data = new ReducePriceByAmountAction.Data { SelectedIDs = new List<Guid>(), SelectedIdType = ReducePriceByAmountAction.Data.SelectionCriteria.ProductList, MaxDiscountsPerItemInOrder = -1 };
                return _data;
            }
        }

        private ReducePriceByAmountAction.Data _data;

        #endregion Property

        #region Private Metods

        /// <summary>
        /// Initializes the validation on page.
        /// </summary>
        private void InitializeValidation()
        {
            c_percentageRegexValidator.CssClass = ParameterConstants.VALIDATOR_STYLE_NAME;
            c_percentageRegexValidator.Text = string.Format("{0} {1}", ParameterConstants.VALIDATOR_IMAGE,
                SystemStrings.GetValue(SystemStringConstants.VALIDATION_MESSAGE_INVALID_INPUT,
                    FoundationContext.LanguageID, true));

            var requiredMessage = string.Format("{0} {1}", ParameterConstants.VALIDATOR_IMAGE,
                SystemStrings.GetValue(SystemStringConstants.VALIDATION_MESSAGE_REQUIRED_FIELD,
                    FoundationContext.LanguageID, true));

            c_percentageRequiredValidator.CssClass = ParameterConstants.VALIDATOR_STYLE_NAME;
            c_percentageRequiredValidator.Text = requiredMessage;
        }

        /// <summary>
        /// Initialize description of the action.
        /// </summary>
        private void InitializeDescription()
        {
            var description = CurrentModule.Strings.GetValue("ReducePriceByAmountActionDescriptionGeneral",
                                                                FoundationContext.LanguageID, true);
            c_labelDescription.Text = description;
        }

        #endregion Private Metods

        #region Protected Methods

        protected override void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                InitializePageControls();
            }
        }

        private ProductListService _productListService = IoC.Resolve<ProductListService>();
        /// <summary>
        /// Initializes the page controls.
        /// </summary>
        private void InitializePageControls()
        {
            InitializeValidation();
            InitializeDescription();
            InitializeProductListDropDown();
        }

        private void InitializeProductListDropDown()
        {
            if (c_ProductListGrid.Rows.Count == 0)
            {
                c_ProductListGrid.DataSource = _productListService.GetAll().Select(x => new { Id = x.SystemId, x.Localizations[CultureInfo.CurrentCulture].Name });
                c_ProductListGrid.DataBind();
            }
        }


        protected void GridPageIndexChanged(object source, GridPageChangedEventArgs e)
        {
            c_hiddenFieldIDs.Value = string.Join(",", Data.SelectedIDs.ToList().ConvertAll(x => x.ToString()).ToArray());
        }

        [Serializable]
        internal sealed class ReducePriceByPercentageResult
        {
            public Guid ID { get; set; }

            public string DisplayName { get; set; }

            public string ArticleNumber { get; set; }
        }

        #endregion Protected Methods

        #region Implementation of IUIExtensionPanel

        /// <summary>
        /// Gets the panel data.
        /// </summary>
        /// <returns>Return an object with the panel data.</returns>
        object IUIExtensionPanel.GetPanelData()
        {
            decimal amount;
            Data.ApplyOnCampaignPriceCumulatively = c_applyOnCampaignCheckbox.Checked;
            decimal.TryParse(c_textboxAmount.Text, out amount);
            Data.DiscountAmountWithVAT = amount;
            int maxDiscountsPerItemInOrder;
            if (int.TryParse(c_textboxMaxDiscountsPerItemInOrder.Text, out maxDiscountsPerItemInOrder))
                Data.MaxDiscountsPerItemInOrder = maxDiscountsPerItemInOrder;
            else
                Data.MaxDiscountsPerItemInOrder = -1;
            Data.SelectedIdType = ReducePriceByAmountAction.Data.SelectionCriteria.ProductList;
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
            if (data is ReducePriceByAmountAction.Data)
                Data = (ReducePriceByAmountAction.Data)data;

            InitializePageControls();

            if (data is ReducePriceByAmountAction.Data)
            {
                c_applyOnCampaignCheckbox.Checked = Data.ApplyOnCampaignPriceCumulatively;
                c_textboxAmount.Text = Data.DiscountAmountWithVAT.ToString();
                c_textboxMaxDiscountsPerItemInOrder.Text = Data.MaxDiscountsPerItemInOrder > 0 ? Data.MaxDiscountsPerItemInOrder.ToString() : string.Empty;
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

        #endregion
    }
}
