<%@ Assembly Name="$SharePoint.Project.AssemblyFullName$" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="asp" Namespace="System.Web.UI" Assembly="System.Web.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" %>
<%@ Import Namespace="Microsoft.SharePoint" %> 
<%@ Register Tagprefix="WebPartPages" Namespace="Microsoft.SharePoint.WebPartPages" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="JavaScriptImports.ascx.cs" Inherits="GSoft.Dynamite.CONTROLTEMPLATES.GSoft.Dynamite.JavaScriptImports" %>

<%-- 3rd party JS libraries --%>
<SharePoint:ScriptLink ID="JqueryScriptLink" Language="javascript" Name="GSoft.Dynamite/Lib/jquery-1.10.2.min.js" Localizable="false" OnDemand="false" runat="server" />
<SharePoint:ScriptLink ID="JqueryPlaceholderShim" Language="javascript" Name="GSoft.Dynamite/Lib/jquery.html5-placeholder-shim.js" Localizable="false" OnDemand="false" runat="server" />
<SharePoint:ScriptLink ID="JqueryNoConflictScriptLink" Language="javascript" Name="GSoft.Dynamite/Lib/jquery-noconflict.js" Localizable="false" OnDemand="false" runat="server" />

<SharePoint:ScriptLink ID="KnockoutScriptLink" Language="javascript" Name="GSoft.Dynamite/Lib/knockout-3.0.0.js" Localizable="false" OnDemand="false" runat="server"/>
<SharePoint:ScriptLink ID="MomentScriptLink" Language="javascript" Name="GSoft.Dynamite/Lib/moment-with-langs.min.js" Localizable="false" OnDemand="false" runat="server"/>
<SharePoint:ScriptLink ID="UnderscoreScriptLink" Language="javascript" Name="GSoft.Dynamite/Lib/underscore-1.7.0.min.js" Localizable="false" OnDemand="false" runat="server"/>

<%-- Dynamite JS libraries --%>
<SharePoint:ScriptLink ID="DynamiteCoreScriptLink" Language="javascript" Name="GSoft.Dynamite/GSoft.Dynamite.Core.js" Localizable="false" OnDemand="false" runat="server" />
<SharePoint:ScriptLink ID="KnockoutBindingHandlersScriptLink" Language="javascript" Name="GSoft.Dynamite/GSoft.Dynamite.Knockout.BindindHandlers.js" Localizable="false" OnDemand="false" runat="server" />
<SharePoint:ScriptLink ID="KnockoutExtensionsScriptLink" Language="javascript" Name="GSoft.Dynamite/GSoft.Dynamite.Knockout.Extensions.js" Localizable="false" OnDemand="false" runat="server" />

<%-- Reusable Knockout.js HTML templates --%>
<script type="text/html" id="tabs-template">
    <ul class="edit-mode-tabs float-left full-width">
        <!-- ko foreach: tabs -->
        <li class="edit-mode-tab-title float-left" data-bind="
            text: resourceString,
            click: function (data) { $root.toggleTab(data) }, 
            css: { 'edit-mode-tab-title-selected': isSelected }">
        </li>
        <!-- /ko -->
        <li class="minimize" data-bind="click: $root.toggleAllTabs">
            -
        </li>
    </ul>
</script>

<%-- Global JS initialization --%>

<script type="text/javascript">
    GSoft.Dynamite.Utils.CurrentWebUrl = "<asp:Literal ID="CurrentWebUrlLiteral" runat="server" />";
    GSoft.Dynamite.Utils.ParentFolderUrl = "<asp:Literal ID="ParentFolderUrlLiteral" runat="server" />";
    GSoft.Dynamite.Utils.initializeParentFolderLink();
</script>
