<%@ Page Language="C#" AutoEventWireup="true" Inherits="CMSModules_SupportHelper_SupportHelper"
    Theme="Default" MasterPageFile="~/CMSMasterPages/UI/Dialogs/ModalDialogPage.master" CodeFile="SupportHelper.aspx.cs" %>

<%@ Register Src="~/CMSFormControls/Documents/DocumentAttachmentsControl.ascx" TagName="Attachments" TagPrefix="cms" %>
<%@ Register Src="~/CMSFormControls/Inputs/EmailInput.ascx" TagName="Email" TagPrefix="cms" %>
<%@ Register Src="~/CMSModules/SupportHelper/SupportHelper_Metrics.ascx" TagName="UniTree" TagPrefix="sup" %>

<asp:Content runat="server" ContentPlaceHolderID="plcContent" ID="cntContent">
    <asp:Panel runat="server" ID="pnlContent">
        <div class="form-horizontal">

            <%-- Category --%>
            <div class="form-group">
                <div class="editing-form-label-cell">
                    <cms:LocalizedLabel CssClass="control-label" ID="lblCategory" runat="server" EnableViewState="false" ResourceString="support.category.label"
                        DisplayColon="true" AssociatedControlID="drpCategory" />
                </div>
                <div class="editing-form-value-cell">
                    <cms:CMSUpdatePanel ID="pnlUp" runat="server" UpdateMode="Conditional">
                        <ContentTemplate>
                            <cms:CMSDropDownList ID="drpCategory" runat="server" AutoPostBack="True" OnSelectedIndexChanged="ShowCustomCategory" CssClass="form-control input-width-80" />
                            <cms:CMSTextBox runat="server" ID="txtCategoryCustom" MaxLength="1000" Visible="false" />
                        </ContentTemplate>
                    </cms:CMSUpdatePanel>
                </div>
            </div>

            <%-- Description --%>
            <div class="form-group">
                <div class="editing-form-label-cell">
                    <cms:LocalizedLabel CssClass="control-label" ID="lblDescription" runat="server" EnableViewState="false" ResourceString="support.description.label" AssociatedControlID="txtDescription" />
                    <cms:LocalizedLabel CssClass="control-label" ID="lblDescriptionExample" runat="server" EnableViewState="false" ResourceString="support.description.example" />
                </div>
                <div class="editing-form-value-cell">
                    <asp:Panel runat="server" ID="txtDescription" CssClass="form-control"></asp:Panel>
                    <asp:HiddenField ID="hdnDescription" runat="server" />
                </div>
            </div>

            <%-- Files --%>
            <div class="form-group">
                <div class="editing-form-label-cell">
                    <cms:LocalizedLabel CssClass="control-label" ID="lblFiles" runat="server" EnableViewState="false" ResourceString="support.files.label" DisplayColon="true" AssociatedControlID="mfuUploader$documentAttachments$newAttachmentElem$mfuDirectUploader$uploadFile" />
                </div>
                <div class="editing-form-value-cell">
                    <cms:CMSForm runat="server" Visible="false" ID="fakeForm"></cms:CMSForm>
                    <cms:Attachments ID="mfuUploader" runat="server"/>
                </div>
            </div>

            <%-- URL --%>
            <div class="form-group">
                <div class="editing-form-label-cell">
                    <cms:LocalizedLabel CssClass="control-label" ID="lblURL" runat="server" EnableViewState="false" ResourceString="support.url.label" DisplayColon="true" AssociatedControlID="txtURL" />
                </div>
                <div class="editing-form-value-cell">
                    <cms:CMSTextBox runat="server" ID="txtURL" MaxLength="1000" />
                </div>
            </div>

            <%-- Contact details --%>
            <div class="form-group">
                <div class="editing-form-label-cell">
                    <cms:LocalizedLabel CssClass="control-label" ID="lblName" runat="server" EnableViewState="false" ResourceString="support.name.label" DisplayColon="true" AssociatedControlID="txtName" />
                </div>
                <div class="editing-form-value-cell">
                    <cms:CMSTextBox runat="server" ID="txtName" MaxLength="1000" />
                </div>
            </div>

            <div class="form-group">
                <div class="editing-form-label-cell">
                    <cms:LocalizedLabel CssClass="control-label" ID="lblOrganization" runat="server" EnableViewState="false" ResourceString="support.organization.label" DisplayColon="true" AssociatedControlID="txtOrganization" />
                </div>
                <div class="editing-form-value-cell">
                    <cms:CMSTextBox runat="server" ID="txtOrganization" MaxLength="1000" />
                </div>
            </div>

            <div class="form-group">
                <div class="editing-form-label-cell">
                    <cms:LocalizedLabel CssClass="control-label" ID="lblEmail" runat="server" EnableViewState="false" ResourceString="support.email.label" DisplayColon="true" AssociatedControlID="txtEmail$txtEmailInput"/>
                </div>
                <div class="editing-form-value-cell">
                    <cms:Email runat="server" ID="txtEmail"/>
                </div>
            </div>

            <%-- Metrics to send --%>
            <div class="form-group">
                <cms:CMSUpdatePanel ID="CMSUpdatePanel1" runat="server" UpdateMode="Conditional">
                    <ContentTemplate>
                        <div class="editing-form-label-cell" style="text-align: right; margin: 0 0 24px 0;">
                            <cms:CMSIcon runat="server" ID="icAdvanced" CssClass="icon-caret-down cms-icon-30" />
                            <cms:LocalizedLinkButton runat="server" ID="lnkAdvanced" OnClick="ExpandMetrics" OnClientClick="return MetricsConfirm();" />
                        </div>
                        <div class="editing-form-label-cell">
                            <div class="ContentTree">
                                <div class="TreeAreaTree" style="margin: 0 0 24px;">
                                    <sup:UniTree ID="metricsControl" runat="server" Localize="true" Visible="false"/>
                                    <asp:HiddenField runat="server" ID="hdnValue" />
                                </div>
                            </div>
                        </div>
                    </ContentTemplate>
                </cms:CMSUpdatePanel>
            </div>
        </div>
    </asp:Panel>
</asp:Content>
