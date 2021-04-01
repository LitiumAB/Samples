<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ReducePriceByAmountActionControl.ascx.cs" Inherits="Litium.Studio.KC.Samples.Campaigns.ReducePriceByAmountActionControl" %>
<style type="text/css">
    .productlist-grid th, .productlist-grid td {
        padding: 5px;
    }
</style>
<div style="float:left; margin-right: 25px;">
    <div>
        <div class="litBoldHeader">
            <Foundation:ModuleString ID="ModuleStringDiscountAmount" Name="ReducePriceByAmountActionAmount" runat="server" />
            <img src="~/LitiumStudio/ECommerce/Images/transparent.gif" runat="server" id="Img3"
                style="vertical-align: middle;" class="litIconRequired" alt="" />
        </div>
        <div>
            <div style="display: inline;">
                <asp:TextBox ID="c_textboxAmount" runat="server" Width="200" />&nbsp;
            </div>
            <div style="display: inline; margin-left: 10px;">
                <asp:CheckBox CssClass="litInputCheckbox" ID="c_applyOnCampaignCheckbox" runat="server" />
                <asp:Label ID="Label1" AssociatedControlID="c_applyOnCampaignCheckbox" runat="server">
                    <Foundation:ModuleString ID="ModuleStringApplyOnCampaignPrice" Name="ApplyOnCampaignPrice" runat="server" />
                </asp:Label>
            </div>
        </div>
        <div class="litBoldHeader">
            <Foundation:ModuleString ID="ModuleStringMaxDiscountsPerItemInOrder" Name="MaxDiscountsPerItemInOrder" runat="server" />            
        </div>
        <div>
            <div style="display: inline;">
                <asp:TextBox ID="c_textboxMaxDiscountsPerItemInOrder" runat="server" Width="200" />&nbsp;
            </div>            
        </div>
        <div>
            <asp:RequiredFieldValidator Display="Dynamic" ID="c_percentageRequiredValidator"
                ValidationGroup="addCampaign" ControlToValidate="c_textboxAmount" runat="server"
                ErrorMessage="*" />
            <asp:RegularExpressionValidator ID="c_percentageRegexValidator" ControlToValidate="c_textboxAmount"
                ValidationGroup="addCampaign" runat="server" ValidationExpression="^\d*[0-9](|.\d*[0-9]|,\d*[0-9])?$"
                ErrorMessage="*" />
        </div>
    </div>
    <div class="litBoldHeader">
    <asp:Label ID="Label2" runat="server">Product List</asp:Label>
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
</div>
<div style="float: left;">
        <div class="litBoldHeader">
            <Foundation:ModuleString ID="ModuleString3" Name="CampaignDescription" runat="server" />
            <img src="~/LitiumStudio/ECommerce/Images/transparent.gif" visible="false" runat="server"
                id="Img4" style="vertical-align: middle;" class="litIconRequired" alt="" /></div>
        <div class="litText" style="width: 280px;">
            <asp:Label ID="c_labelDescription" runat="server"></asp:Label>
        </div>
    </div>
    <div class="clearer" />
<asp:HiddenField runat="server" ID="c_hiddenFieldIDs" />