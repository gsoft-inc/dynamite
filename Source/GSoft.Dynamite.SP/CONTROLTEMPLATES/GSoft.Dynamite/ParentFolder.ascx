<%@ Assembly Name="$SharePoint.Project.AssemblyFullName$" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="asp" Namespace="System.Web.UI" Assembly="System.Web.Extensions, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" %>
<%@ Import Namespace="Microsoft.SharePoint" %> 
<%@ Register Tagprefix="WebPartPages" Namespace="Microsoft.SharePoint.WebPartPages" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ParentFolder.ascx.cs" Inherits="GSoft.Dynamite.CONTROLTEMPLATES.GSoft.Dynamite.ParentFolder" %>

<a class="ms-promotedActionButton parent-folder-link" title="<%=this.ParentFolderLabel%>" href="<%=this.ParentFolderUrl%>">
    <span style="height: 16px; width: 16px; position: relative; display: inline-block; overflow: hidden;" class="s4-clust ms-promotedActionButton-icon">
        <img src="/_layouts/15/images/spcommon.png?rev=23" alt="Follow" style="position: absolute; left: -215px; top: -120px;">
    </span>
    <span class="ms-promotedActionButton-text language-switcher-label"><%=this.ParentFolderLabel%></span>
</a>

<%-- Global JS initialization --%>
<script type="text/javascript">
    GSoft.Dynamite.Utilities.CurrentWebUrl = "<%=this.CurrentWebAbsolutePath%>";
    GSoft.Dynamite.Utilities.ParentFolderUrl = "<%=this.ParentFolderServerRelativePath%>";
    GSoft.Dynamite.Utilities.initializeParentFolderLink();
</script>
