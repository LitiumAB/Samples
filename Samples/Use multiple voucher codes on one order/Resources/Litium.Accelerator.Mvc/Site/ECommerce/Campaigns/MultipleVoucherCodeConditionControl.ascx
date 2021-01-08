<%@ Control Language="C#" AutoEventWireup="true"
    Inherits="MultipleVoucherCodeConditionControl" Codebehind="MultipleVoucherCodeConditionControl.ascx.cs" %>
<div>
    <div class="lsTabContainer">
        <Telerik:RadTabStrip runat="server" MultiPageID="CreateViews" ID="tabs">
            <Tabs>
                <Telerik:RadTab runat="server" Text="Ange" Selected="True"/>
                <Telerik:RadTab runat="server" Text="Generera"/>
            </Tabs>
        </Telerik:RadTabStrip>
        <Telerik:RadMultiPage runat="server" ID="CreateViews">
            <Telerik:RadPageView runat="server" Selected="True">
                    <div>
                        <div class="litBoldHeader">
                            <Foundation:ModuleString ID="ModuleString1" Name="NumberOfTimehVoucherCodeBeUsed" runat="server" />
                            <img src="~/Litium/ECommerce/Images/transparent.gif" runat="server" id="Img4" class="litIconRequired" style="vertical-align: middle;"  alt="" />
                        </div>
                        <asp:TextBox ID="c_textBoxUseageFrequencyStatic" runat="server" Width="200" ValidationGroup="voucherStatic" />&nbsp;<br/>
                       <asp:CustomValidator ID="UseageFrequencyStaticCustomValidator" runat="server" OnServerValidate="OnServerValidate"  ControlToValidate="c_textBoxUseageFrequencyStatic" ValidationGroup="voucherStatic" cssClass="litErrorMsg"><img src="Images/transparent.gif" style="vertical-align:middle;" alt="" class="litIconError"> <Foundation:SystemString runat="server" Name="ValidatorMissingValue"/></asp:CustomValidator>
                   </div>
                    <div style="clear: both;"></div>
                    <div>
                        <div class="litBoldHeader">
                            <Foundation:ModuleString Name="StaticVoucherCodes" runat="server" />
                            <img src="~/Litium/ECommerce/Images/transparent.gif" runat="server" id="Img2" style="vertical-align: middle;"  alt="" class="litIconRequired" />
                        </div>
                        <asp:TextBox ID="StaticVoucherCodes" runat="server" Width="200" ValidationGroup="voucherStatic" />
                        &nbsp;
                        <asp:Button type="button" ID="ButtonStaticAddVoucherCode" runat="server" OnClick="ButtonAddVoucherCodes" ValidationGroup="voucherStatic" /><br/>
                        <asp:CustomValidator ID="StaticVoucherCodesCustomValidator" runat="server" OnServerValidate="OnServerValidate"  ControlToValidate="StaticVoucherCodes" ValidationGroup="voucherStatic" cssClass="litErrorMsg"><img src="Images/transparent.gif" style="vertical-align:middle;" alt="" class="litIconError"> <Foundation:SystemString runat="server" Name="ValidatorMissingValue"/></asp:CustomValidator>
                    </div>
            </Telerik:RadPageView>
            <Telerik:RadPageView runat="server">
                    <div>
                        <div class="litBoldHeader">
                            <Foundation:ModuleString ID="ModuleStringNumberOfTimesAVoucherCodeBeUsed" Name="NumberOfTimesEachVoucherCodeBeUsed" runat="server" />
                            <img src="~/Litium/ECommerce/Images/transparent.gif" runat="server" id="Img1" class="litIconRequired" style="vertical-align: middle;"  alt="" />
                        </div>
                        <asp:TextBox ID="c_textBoxUseageFrequency" runat="server" Width="200" ValidationGroup="voucherCodeCondition" />&nbsp;<br/>
                        <asp:CustomValidator ID="UseageFrequencyCustomValidator" runat="server" OnServerValidate="OnServerValidate"  ControlToValidate="c_textBoxUseageFrequency" ValidationGroup="voucherStatic" cssClass="litErrorMsg"><img src="Images/transparent.gif" style="vertical-align:middle;" alt="" class="litIconError"> <Foundation:SystemString runat="server" Name="ValidatorMissingValue"/></asp:CustomValidator>
                     </div>
                    <div style="clear: both;"></div>
                    <div>
                        <div class="litBoldHeader">
                            <Foundation:ModuleString ID="ModuleStringNumberOfVoucherCodes" Name="NumberOfVoucherCodes" runat="server" />
                            <img src="~/Litium/ECommerce/Images/transparent.gif" runat="server" id="Img3" style="vertical-align: middle;"  alt="" class="litIconRequired" />
                        </div>
                        <asp:TextBox ID="c_textBoxNumberOfVoucherCodes" runat="server" Width="200" ValidationGroup="voucherCodeCondition" />&nbsp;
                        <asp:Button type="button" ID="c_buttonNumberOfVoucherCodes" runat="server" OnClick="ButtonNumberOfVoucherCodes" ValidationGroup="voucherCodeCondition" /><br/>
                        <asp:CustomValidator ID="NumberOfVoucherCodesCustomValidator" runat="server" OnServerValidate="OnServerValidate"  ControlToValidate="c_textBoxNumberOfVoucherCodes" ValidationGroup="voucherStatic" cssClass="litErrorMsg"><img src="Images/transparent.gif" style="vertical-align:middle;" alt="" class="litIconError"> <Foundation:SystemString runat="server" Name="ValidatorMissingValue"/></asp:CustomValidator>
                   </div>
            </Telerik:RadPageView>
        </Telerik:RadMultiPage>
    </div>
    <div>
        <div style="width: 450px;">
	        <div class="litBoldHeader" style="float: left;">
            <Foundation:ModuleString ID="c_moduleStringArticles" Name="VoucherCodes" runat="server" />
			</div>
			<div style="float: right;margin-top: 11px;">
				<asp:LinkButton CssClass="litContentPager" runat="server" OnClick="ExportToExcell"><Foundation:ModuleString Name="ExportVoucherCodes" runat="server" /></asp:LinkButton>
			</div>
            <div style="clear: both;" class="lscampaignsvouchercodecontainer">
                <LS:GridHelperAjax runat="server" ID="m_vouacherCodesGridHelper" AssociatedControlID="c_voucherCodesGrid"
                    OnNeedData="voucherCodesGrid_NeedData" PageSize="24" />
                <Telerik:RadGrid ID="c_voucherCodesGrid" CssClass="ArticleGrid" runat="server"
                    Style="height: 200px; width: 450px; border: solid 1px #CFCDCC;"
                    OnItemDataBound="VoucherCodesGridOnItemDataBound" EnableViewState="true" AllowMultiRowSelection="True">
                    <MasterTableView runat="server" CssClass="AutoShrink" ShowHeader="false">
                        <Columns>
                            <Telerik:GridTemplateColumn>
                                <ItemTemplate>
                                    <div class="lsConditionVoucherCodesRowContainer lsText">
                                        <asp:Panel ID="c_panelVoucherCode" runat="server"></asp:Panel>
                                    </div>
                                </ItemTemplate>
                            </Telerik:GridTemplateColumn>
                        </Columns>
                    </MasterTableView>
                </Telerik:RadGrid>
            </div>
        </div>
        <div style="text-align: right;">
            <asp:Button type="button" ID="RemoveSelected" runat="server" OnClick="ButtonRemoveSelected" />
            &nbsp;
            <asp:Button type="button" ID="Clear" runat="server" OnClick="ButtonClearVoucherCodes" />
        </div>

    </div>
</div>
