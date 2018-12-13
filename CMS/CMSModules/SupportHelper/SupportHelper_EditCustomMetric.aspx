<%@ Page Language="C#" AutoEventWireup="true" Inherits="CMSModules_SupportHelper_SupportHelper_EditCustomMetric"
    Theme="Default" MasterPageFile="~/CMSMasterPages/UI/Dialogs/ModalDialogPage.master" CodeFile="SupportHelper_EditCustomMetric.aspx.cs" %>

<%@ Register Src="~/CMSFormControls/Basic/DropDownListControl.ascx" TagName="DropDownListControl" TagPrefix="cms" %>
<%@ Register Src="~/CMSFormControls/Classes/AssemblyClassSelector.ascx" TagName="AssemblyClassSelector" TagPrefix="cms" %>

<asp:Content runat="server" ContentPlaceHolderID="plcContent" ID="cntContent">
    <cms:UIForm runat="server" ID="formElem" ObjectType="supporthelper.custommetric" RedirectUrlAfterCreate="" DefaultFieldLayout="TwoColumns" RefreshHeader="True">
        <LayoutTemplate>
            <cms:FormField runat="server" ID="fDisplayName" Field="CustomMetricDisplayName" FormControl="LocalizableTextBox" ResourceString="general.displayname" DisplayColon="true" ShowRequiredMark="true"/>
            <cms:FormField runat="server" ID="fCodeName" Field="CustomMetricCodeName" ResourceString="general.codename" DisplayColon="true"/>
            <cms:FormField runat="server" ID="fAssemblyName" Field="CustomMetricAssemblyName" ResourceString="support.metric.edit.dataclass" DisplayColon="true"/>
            <cms:FormField runat="server" ID="fParent" Field="CustomMetricParent" ResourceString="support.metric.edit.parent" DisplayColon="true" ShowRequiredMark="true"/>
            <cms:FormField runat="server" ID="fID" DevelopmentModeOnly="true" Field="CustomMetricID" UseFFI="true"/>
            <cms:FormField runat="server" ID="fSelected" Field="CustomMetricSelected" ResourceString="support.metric.edit.selected" />
        </LayoutTemplate>
    </cms:UIForm>
</asp:Content>

