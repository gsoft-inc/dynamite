<%@ Assembly Name="$SharePoint.Project.AssemblyFullName$" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ProjectScripts.ascx.cs" Inherits="GSoft.Dynamite.Examples.Parts.ControlTemplates.ProjectScripts" %>

<!-- GSoft.Dynamite.Examples javascript code -->

<%-- Layouts folder master script file for the project (unavalaible to end-users for easier maintenance) --%>
<SharePoint:ScriptLink ID="ProjectScriptLink" Language="javascript" Name="~sitecollection/_layouts/GSoft.Dynamite.Examples/Js/GSoft.Dynamite.Examples.js" Localizable="false" OnDemand="false" runat="server"/>

<%-- Customizable script file available in Style Library for power users. Place after main script file to allow users to override the default functionality --%>
<SharePoint:ScriptLink ID="ProjectCustomScriptLink" Language="javascript" Name="~sitecollection/Style Library/GSoft.Dynamite.Examples.Custom.js" Localizable="false" OnDemand="false" runat="server"/>

<%-- Global component initialization --%>
<script type="text/javascript">
    GSoft.Dynamite.Examples.BasicWall.initialize();    
</script>