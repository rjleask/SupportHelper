<%@ Control Language="C#" AutoEventWireup="true" Inherits="CMSModules_SupportHelper_SupportHelper_Metrics" CodeFile="SupportHelper_Metrics.ascx.cs" %>
<div id="<%=ClientID%>">
    <asp:Panel runat="server" ID="pnlMain" CssClass="TreeMain">
        <asp:Panel runat="server" ID="pnltreeTree" CssClass="TreeTree">
            <cms:UITreeView runat="server" ID="treeElem" ShortID="tv" ShowLines="true" />
            <asp:HiddenField ID="hdnSelectedItem" runat="server" EnableViewState="true" />
            <div style="display: none;">
                <cms:CMSUpdatePanel ID="pnlUpdateSelected" runat="server">
                    <ContentTemplate>
                        <asp:Button ID="btnItemSelected" runat="server" EnableViewState="false" CssClass="HiddenButton" />
                    </ContentTemplate>
                </cms:CMSUpdatePanel>
            </div>
        </asp:Panel>
    </asp:Panel>
</div>
<asp:Literal runat="server" ID="ltlScript" EnableViewState="false"></asp:Literal>
