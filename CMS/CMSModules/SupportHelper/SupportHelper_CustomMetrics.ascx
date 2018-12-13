<%@ Control Language="C#" AutoEventWireup="true" CodeFile="SupportHelper_CustomMetrics.ascx.cs" Inherits="CMSModules_SupportHelper_SupportHelper_CustomMetrics" %>
<%@ Register src="~/CMSAdminControls/UI/UniGrid/UniGrid.ascx" tagname="UniGrid" tagprefix="cms" %>
<%@ Register Namespace="CMS.UIControls.UniGridConfig" TagPrefix="ug" Assembly="CMS.UIControls" %>

<cms:LocalizedButton ID="btnAddNew" runat="server" ResourceString="support.metric.add.title"></cms:LocalizedButton>
<cms:LocalizedLabel ID="lblNoClasses" runat="server" ResourceString="support.ui.custommetric.noclasses" Visible="false"></cms:LocalizedLabel>
<br /><br />
<cms:UniGrid ID="customMetricsGrid" runat="server" ObjectType="SupportHelper.CustomMetric" OrderBy="CustomMetricID" ShowExportMenu="false" HideControlForZeroRows="true">

    <GridActions>
        <ug:Action Name="#edit" Caption="$General.Edit$" ExternalSourceName="edit" FontIconClass="icon-edit" FontIconStyle="allow"/>
        <ug:Action Name="#delete" Caption="$General.Delete$" ExternalSourceName="delete" FontIconClass="icon-bin" FontIconStyle="critical"/>
    </GridActions>

    <GridColumns>
        <ug:Column Source="CustomMetricDisplayName" Caption="$support.metric.codename$" ExternalSourceName="codename"/>
        <ug:Column Source="CustomMetricClassName" Caption="$support.metric.dataclass$"/>
        <ug:Column Source="CustomMetricParent" Caption="$support.metric.parent$" ExternalSourceName="parent"/>
        <ug:Column Source="CustomMetricSelected" Caption="$support.metric.selected$" ExternalSourceName="#yesno" Width="100%"/>
    </GridColumns>

    <PagerConfig ShowPageSize="false"/>

</cms:UniGrid>

<asp:HiddenField runat="server" ID="hdnCmdArg" />


