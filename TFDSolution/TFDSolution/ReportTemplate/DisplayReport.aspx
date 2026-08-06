<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="DisplayReport.aspx.cs" Inherits="TFDSolution.ReportTemplate.DisplayReport" %>
<%@ Register Assembly="CrystalDecisions.Web, Version=13.0.4000.0, Culture=neutral, PublicKeyToken=692fbea5521e1304"
    Namespace="CrystalDecisions.Web" TagPrefix="cr" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Crystal Report Demo</title>
  <link href="../ReportTemplate/crystalreportviewers13/js/crviewer/images/style.css" rel="stylesheet" />
    <script src="../ReportTemplate/crystalreportviewers13/js/crviewer/crv.js"></script>
    <script src="../ReportTemplate/crystalreportviewers13/js/prompts.js"></script>

</head>
<body>
    <form id="form1" runat="server">
        <div hieght="100px" width="100px">
            <cr:CrystalReportViewer ID="CrystalReportViewer1" runat="server" AutoDataBind="true" 
                EnableDatabaseLogonPrompt="false" EnableParameterPrompt="true" 
                ToolPanelView="None" Width="100%" Height="800px" />
        </div>
    </form>
</body>
</html>
