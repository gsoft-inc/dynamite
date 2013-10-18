<%@ Assembly Name="$SharePoint.Project.AssemblyFullName$" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ScriptDependencies.ascx.cs" Inherits="GSoft.Dynamite.Examples.Parts.ControlTemplates.ScriptDependencies" %>

<!-- GSoft.Dynamite.Examples javascript dependencies -->

<%-- Dependencies and plugins kept separate from main project Javascript links - be careful not to import jQuery twice in the same page --%>
<SharePoint:ScriptLink ID="JQueryScriptLink" Language="javascript" Name="~sitecollection/_layouts/GSoft.Dynamite.Examples/Js/lib/jquery-1.7.1.min.js" Localizable="false" OnDemand="false" runat="server"/>

<%-- jQuery no conflicts mode to free up the $ symbol --%>
<SharePoint:ScriptLink ID="JQueryNoConflictScriptLink" Language="javascript" Name="~sitecollection/_layouts/GSoft.Dynamite.Examples/Js/jquery-no-conflict.js" Localizable="false" OnDemand="false" runat="server"/>