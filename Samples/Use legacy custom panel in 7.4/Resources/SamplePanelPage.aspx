<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="SamplePanelPage.aspx.cs" Inherits="Litium.Accelerator.Mvc.Site.ECommerce.Panels.SamplePanelPage" %>
<%@ Register src="SamplePanel.ascx" tagname="SamplePanel" tagprefix="uc1" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <%-- Render panel's web user control --%>
        <uc1:SamplePanel ID="SamplePanel1" runat="server" />
    </form>
</body>
</html>