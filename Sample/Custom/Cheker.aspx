<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Cheker.aspx.cs" Inherits="SitefinityWebApp.Custom.Cheker" %>
<%@ Register Assembly="Telerik.Sitefinity" Namespace="Telerik.Sitefinity.Web.UI" TagPrefix="sf" %>   
<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            Debug Services
        </div>

        <sf:ResourceLinks ID="ResourceLinks1" runat="server">
            <sf:ResourceFile Name="Sitefinity.LibraryItemsDownloader.LibraryItemsDownloadService.js" Type="JavaScript" AssemblyInfo="Sitefinity.LibraryItemsDownloader.Program, Sitefinity.LibraryItemsDownloader" Static="true"></sf:ResourceFile>
        </sf:ResourceLinks>
    </form>
</body>
</html>
