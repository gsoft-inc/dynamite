<%@ Assembly Name="$SharePoint.Project.AssemblyFullName$" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="asp" Namespace="System.Web.UI" Assembly="System.Web.Extensions, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" %>
<%@ Import Namespace="Microsoft.SharePoint" %> 
<%@ Register Tagprefix="WebPartPages" Namespace="Microsoft.SharePoint.WebPartPages" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="GoogleAnalyticsSearchSend.ascx.cs" Inherits="GSoft.Dynamite.CONTROLTEMPLATES.GSoft.Dynamite.GoogleAnalyticsSearchSend" %>

<script>
    // Keep track of the value because the search and the hashchanged triggers the event
    var searchQuery = "";

    function HashChanged(sender, hashs) {
        if (hashs["k"] && hashs["k"] != searchQuery) {
            searchQuery = hashs["k"];
            var searchQueryUrl = window.location.pathname + "?k=" + encodeURIComponent(searchQuery);
            ga('send', 'pageview', searchQueryUrl);
        }
    }
    
    if (typeof ga != 'undefined') {
        ajaxNavigate.add_navigate(HashChanged);
    }
</script>