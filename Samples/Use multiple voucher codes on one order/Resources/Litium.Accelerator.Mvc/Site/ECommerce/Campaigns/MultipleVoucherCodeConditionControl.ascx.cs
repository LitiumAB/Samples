using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.UI.WebControls;
using Litium.Accelerator.Mvc.Campaigns;
using Litium.Foundation.Modules.ECommerce.Campaigns;
using Litium.Foundation.Modules.ECommerce.Plugins.Campaigns;
using Litium.Foundation.Modules.ECommerce.Plugins.Campaigns.Conditions;
using Litium.Infrastructure.Excel;
using Litium.Studio.Extenssions;
using Litium.Studio.UI.Common;
using Litium.Studio.UI.ECommerce.Common;
using Telerik.Web.UI;

public partial class MultipleVoucherCodeConditionControl : UserBaseControl, IUIExtensionPanel
{
	private MultipleVoucherCodeCondition.Data _data;

    #region Constants
    private const string GENERATE = "Generate";
    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the Data of control.
    /// </summary>
    MultipleVoucherCodeCondition.Data Data
    {
	    get
	    {
			return _data ?? (_data = new MultipleVoucherCodeCondition.Data { VoucherCodes = new Dictionary<string, int>() });
        }
		set
		{
			_data = value;
		}
    }

	#endregion

    #region Private methods

    /// <summary>
    /// Initialize controls properties.
    /// </summary>
    private void InitControlsProperties()
    {
        //c_textBoxNumberOfVoucherCodes.Attributes.Add("readonly", "readoly");
        c_buttonNumberOfVoucherCodes.Text = String.Format("{0}...", CurrentModule.Strings.GetValue(GENERATE, FoundationContext.LanguageID, true));
        ButtonStaticAddVoucherCode.Text = "ButtonAdd".AsSystemString();
        Clear.Text = "Clear".AsSystemString();
        RemoveSelected.Text = "RemoveSelected".AsSystemString();
        tabs.Tabs[0].Text = "CreateVoucherStatic".AsModuleString();
        tabs.Tabs[1].Text = "CreateVoucherGenerate".AsModuleString();
    }

    private void InitValidators()
    {
       
    }

    #endregion

    #region Protected methods

    protected override void Page_Load(object sender, EventArgs e)
    {
        InitControlsProperties();
        InitValidators();
    }

    protected void ButtonNumberOfVoucherCodes(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(c_textBoxUseageFrequency.Text) | string.IsNullOrEmpty(c_textBoxNumberOfVoucherCodes.Text))
        {
            if (string.IsNullOrEmpty(c_textBoxUseageFrequency.Text))
            {
                UseageFrequencyCustomValidator.IsValid = false;
            }
            if (string.IsNullOrEmpty(c_textBoxNumberOfVoucherCodes.Text))
            {
                NumberOfVoucherCodesCustomValidator.IsValid = false;
            }
            return;
        }
        int quantityOfVoucherCodes;
        if (int.TryParse(c_textBoxNumberOfVoucherCodes.Text, out quantityOfVoucherCodes))
        {
            List<string> result = VoucherCodeGenerator.GenerateVoucherCodes(quantityOfVoucherCodes);
            var voucherCodes = new Dictionary<string, int>();
            int repeatCount = 1;
            int.TryParse(c_textBoxUseageFrequency.Text,out repeatCount);
            result.ForEach(x=>voucherCodes.Add(x,repeatCount));

            foreach (var item in voucherCodes)
            {
                Data.VoucherCodes[item.Key] = item.Value;
            }
        }
        
        c_voucherCodesGrid.DataSource = null;
        c_voucherCodesGrid.Rebind();
    }

	protected PagedResult voucherCodesGrid_NeedData(object sender, int startrowindex, int pagesize, List<Litium.Studio.UI.Common.SortExpression> sortexpressions)
    {
		return new PagedResult<KeyValuePair<string, int>>
		{
			Count = Data.VoucherCodes.Count,
			Data = Data.VoucherCodes,
			Reset = pagesize > startrowindex
		};
    }

    protected void VoucherCodesGridOnItemDataBound(object sender, GridItemEventArgs e)
    {
        if (e.Item is GridDataItem)
        {
            var voucherCodes = (KeyValuePair<string, int>) e.Item.DataItem;
			if (!string.IsNullOrEmpty(voucherCodes.Key))
            {
                var div = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
				div.Controls.Add(new Literal { Text = string.Format("{0} ({1})", voucherCodes.Key, voucherCodes.Value) });
                e.Item.FindControl("c_panelVoucherCode").Controls.Add(div);
            }
        }
    }

    #endregion

    #region IUIExtensionPanel members

    object IUIExtensionPanel.GetPanelData()
    {
        return Data;
    }

    /// <summary>
    /// Sets the panel data.
    /// </summary>
    /// <param name="data">The data.</param>
    void IUIExtensionPanel.SetPanelData(object data)
    {
		Data = data as MultipleVoucherCodeCondition.Data ?? new MultipleVoucherCodeCondition.Data { VoucherCodes = new Dictionary<string, int>() };
        c_voucherCodesGrid.DataSource = null;
		c_voucherCodesGrid.Rebind();
    }
    
    #endregion

	protected void ExportToExcell(object sender, EventArgs e)
	{
		using (var dataTable = new DataTable { TableName = "Codes" })
		{
			dataTable.Columns.Add("Code", typeof (string));
			dataTable.Columns.Add("Count", typeof (int));
			foreach (var item in Data.VoucherCodes)
			{
				var row = dataTable.NewRow();
				row["Code"] = item.Key;
				row["Count"] = item.Value;

				dataTable.Rows.Add(row);
			}

			// Stream the Excel spreadsheet to the client in a format
			// compatible with Excel 97/2000/XP/2003/2007/2010.
			Response.Clear();
			Response.ContentType = "application/vnd.ms-excel";
			Response.AddHeader("Content-Disposition", "attachment; filename=VoucherCodes.xlsx");
            dataTable.SaveToStream(Response.OutputStream);
			Response.End();
		}
	}

    protected void ButtonAddVoucherCodes(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(c_textBoxUseageFrequencyStatic.Text) | string.IsNullOrEmpty(StaticVoucherCodes.Text))
        {
            if (string.IsNullOrEmpty(c_textBoxUseageFrequencyStatic.Text))
            {
                UseageFrequencyStaticCustomValidator.IsValid = false;
            }
            if (string.IsNullOrEmpty(StaticVoucherCodes.Text))
            {
                StaticVoucherCodesCustomValidator.IsValid = false;
            }
            return;
        }
        if (Data.VoucherCodes == null)
        {
            Data.VoucherCodes = new Dictionary<string, int>();
        }

        int i;
        if (int.TryParse(c_textBoxUseageFrequencyStatic.Text, out i) && !string.IsNullOrWhiteSpace(StaticVoucherCodes.Text))
        {
            Data.VoucherCodes[StaticVoucherCodes.Text] = i;

            c_voucherCodesGrid.DataSource = null;
            c_voucherCodesGrid.Rebind();
        }
    }

    protected void ButtonClearVoucherCodes(object sender, EventArgs e)
    {
        Data.VoucherCodes = new Dictionary<string, int>();

        c_voucherCodesGrid.DataSource = null;
        c_voucherCodesGrid.Rebind();
    }

    protected void ButtonRemoveSelected(object sender, EventArgs e)
    {
        var items = c_voucherCodesGrid.SelectedItems.OfType<GridDataItem>().OrderBy(x => x.DataSetIndex).Select(x => x.DataSetIndex).ToList();
        var dataItems = Data.VoucherCodes.ToList();
        for (int i = items.Count; i > 0; i--)
        {
            var index = items[i-1];
            dataItems.RemoveAt(index);
        }
        Data.VoucherCodes = dataItems.ToDictionary(x => x.Key, x => x.Value);
        c_voucherCodesGrid.DataSource = null;
        c_voucherCodesGrid.Rebind();
    }

    protected void OnServerValidate(object source, ServerValidateEventArgs args)
    {
    }
}