<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="OrderTotalExcludingArticlesConditionControl.ascx.cs" Inherits="Litium.Studio.KC.Samples.Campaigns.OrderTotalExcludingArticlesConditionControl" %>

<style type="text/css">
    .productlist-grid th, .productlist-grid td {
        padding: 5px;
    }
</style>
<div>
    <div class="litBoldHeader">
        <Foundation:ModuleString ID="ModuleStringOrderTotal" Name="OrderTotalExcludingArticlesConditionOrderTotal" runat="server" />
    </div>
    <asp:DropDownList ID="c_conditionDropDownList" runat="server" Style="width: 204px;" />
    <asp:TextBox ID="c_textBoxPay" runat="server" CssClass="lsCampaignConditionsQuantityField" />&nbsp;
         
    <div class="litBoldHeader" style="display: inline;">
        <img src="~/LitiumStudio/ECommerce/Images/transparent.gif" runat="server" id="Img2"
            style="vertical-align: middle;" class="litIconRequired" alt="" />
    </div>
    <asp:RequiredFieldValidator Display="Dynamic" ID="c_requiredQuantityValidator" ControlToValidate="c_textBoxPay"
        ValidationGroup="addCampaign" runat="server" ErrorMessage="*" />
        <asp:RegularExpressionValidator ID="c_regExQuantityValidator" ControlToValidate="c_textBoxPay"
        ValidationGroup="addCampaign" runat="server" ValidationExpression="^\d*[0-9](|.\d*[0-9]|,\d*[0-9])?$"
        ErrorMessage="*" />
          
    <div>
        <div class="litBoldHeader" style="display: inline;">
            <asp:CheckBox CssClass="litInputCheckbox" ID="c_includeVat" runat="server" />
            <asp:Label ID="c_includeVatLabel" AssociatedControlID="c_includeVat" runat="server">
                <Foundation:ModuleString ID="ModuleString11" Name="TotalIncludingVAT" runat="server" />
            </asp:Label>
        </div>
    </div>
</div>
<div class="litBoldHeader">
    <Foundation:ModuleString ID="ModuleStringOrderTotalExcludingArticlesConditionExcludeArticles" Name="OrderTotalExcludingArticlesConditionExcludeArticles" runat="server" />
</div>
<div class="litBoldHeader">
    <asp:GridView ID="c_ProductListGrid" CssClass="productlist-grid"
        runat="server" DataKeyNames="Id" AutoGenerateColumns="false">
        <Columns>
            <asp:BoundField HeaderText="Name" DataField="Name" />
            <asp:TemplateField HeaderText="Select" ItemStyle-HorizontalAlign="Center">
                <ItemTemplate>
                    <asp:CheckBox ID="RowSelector" runat="server"/>
                </ItemTemplate>
            </asp:TemplateField>
        </Columns>
    </asp:GridView>
</div>
<div>
    <div class="litBoldHeader">
        <Foundation:ModuleString ID="ModuleString3" Name="CampaignDescription" runat="server" />
        <img src="~/LitiumStudio/ECommerce/Images/transparent.gif" visible="false" runat="server"
             id="Img4" style="vertical-align: middle;" class="litIconRequired" alt="" /></div>
    <div class="litText" style="width: 280px;">
        <Foundation:ModuleString ID="ModuleString1" Name="OrderTotalExcludingArticlesConditionDescription" runat="server" />
    </div>
</div>
<div class="clearer" />
<asp:HiddenField runat="server" ID="c_hiddenFieldIDs" />