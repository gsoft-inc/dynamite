<%@ Assembly Name="$SharePoint.Project.AssemblyFullName$" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="asp" Namespace="System.Web.UI" Assembly="System.Web.Extensions, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="BasicWallUserControl.ascx.cs" Inherits="GSoft.Dynamite.Examples.Parts.BasicWall.BasicWallUserControl" %>

<div class="basic wall">
    <div class="new-wall-post">
        <asp:TextBox ID="NewPost" CssClass="text-box" TextMode="MultiLine" runat="server"></asp:TextBox>
        <asp:Button ID="NewPostSubmit" CssClass="button" runat="server" />
    </div>
    <div>
        <asp:Repeater ID="PostRepeater" runat="server">
            <HeaderTemplate>
            
            </HeaderTemplate>
            <ItemTemplate>
                <div class="wall-post">
                    <div class="wall-post-author">
                        <%# DataBinder.Eval(Container.DataItem, "AuthorName")%>:
                    </div>
                    <div class="wall-post-text">
                        <%# DataBinder.Eval(Container.DataItem, "Text") %>
                    </div>
                    <div class="wall-post-tags">
                        <%# this.TagsHtml(Container.DataItem) %>
                    </div>
                    <div class="wall-post-replies">
                        <asp:Repeater ID="ReplyRepeater" runat="server">
                            <HeaderTemplate>
                            </HeaderTemplate>
                            <ItemTemplate>
                                <div class="wall-reply">
                                    <div class="wall-reply-author">
                                        &#8594;&nbsp;<%# DataBinder.Eval(Container.DataItem, "AuthorName")%>:
                                    </div>
                                    <div class="wall-reply-text">
                                        <%# DataBinder.Eval(Container.DataItem, "Text") %>
                                    </div>
                                    <div class="wall-reply-tags">
                                        <%# this.TagsHtml(Container.DataItem) %>
                                    </div>
                                </div>
                            </ItemTemplate>
                            <FooterTemplate>
                            </FooterTemplate>
                        </asp:Repeater>
                    </div>
                    <div class="new-wall-reply-open-form">
                        <a href="#">
                            &#8594;&nbsp;<asp:Literal ID="NewReplyOpenForm" runat="server"></asp:Literal>
                        </a>
                    </div>
                    <div class="new-wall-reply" style="display: none;" runat="server">
                        <asp:TextBox ID="NewReply" CssClass="text-box" TextMode="MultiLine" runat="server"></asp:TextBox>
                        <asp:Button ID="NewReplySubmit" CssClass="button" Text="Reply" UseSubmitBehavior="false" CommandName="NewReply" runat="server" />
                    </div>
                </div>
            </ItemTemplate>
            <FooterTemplate>
            </FooterTemplate>
        </asp:Repeater>
    </div>
</div>
