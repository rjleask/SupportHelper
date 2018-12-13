using System;
using System.Data;
using System.Text;
using System.Web.UI.WebControls;

using CMS.Base.Web.UI;
using CMS.FormEngine.Web.UI;
using CMS.Helpers;

public partial class CMSModules_SupportHelper_SupportHelper_CustomMetrics : FormEngineUserControl
{
    #region Required members

    public override object Value { get; set; }

    #endregion Required members

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        ScriptHelper.RegisterDialogScript(Page);

        var sb = new StringBuilder();

        // Dialog for editing custom metric
        sb.AppendFormat(@"
			function Edit_{0} (custommetricID, hash) {{
				modalDialog('{1}?custommetricID=' + custommetricID + '&hash=' + hash, 'editMetric', 660, 440);
				return false;
			}}",
            ClientID,
            ResolveUrl("~/CMSModules/SupportHelper/SupportHelper_EditCustomMetric.aspx")
        );

        // Transfer hidden field for deleting custom metric
        sb.AppendFormat(@"
			function Delete(arg) {{
				if (confirm('{1}'))
				{{
					document.getElementById('{0}').value = arg;
				}}
			}}",
            hdnCmdArg.ClientID,
            GetString("General.ConfirmDelete")
        );

        // Register script for editing attachment
        ScriptHelper.RegisterClientScriptBlock(this, typeof(string), "CustomMetric", sb.ToString(), true);

        // Setup new custom Metric button
        if (CustomMetricsAvailable())
        {
            btnAddNew.OnClientClick = String.Format("Edit_{0}(0, '{1}');return false;", ClientID, QueryHelper.GetHash("?custommetricID=0"));
        }
        else
        {
            lblNoClasses.Visible = !(btnAddNew.Visible = false);
        }

        customMetricsGrid.OnExternalDataBound += customMetricsGrid_OnExternalDataBound;

        if (RequestHelper.IsPostBack())
        {
            customMetricsGrid.HandleAction("#delete", ValidationHelper.GetInteger(Request.Form[hdnCmdArg.UniqueID], 0).ToString());
        }
    }

    /// <summary>
    /// Determines if any classes inheriting the <see cref="ICustomMetric"/> interface are registered.
    /// </summary>
    private bool CustomMetricsAvailable()
    {
        return CMS.Base.ClassHelper.GetAssemblyNames("", new CMS.Base.ClassTypeSettings("ICustomMetric")).Count > 0;
    }

    /// <summary>
    /// Set external data formatting.
    /// </summary>
    private object customMetricsGrid_OnExternalDataBound(object sender, string sourceName, object parameter)
    {
        switch (sourceName.ToLowerInvariant())
        {
            case "edit":
                var edit = sender as CMSGridActionButton;

                if (edit != null)
                {
                    // Prepare parameters
                    DataRowView drv = (DataRowView)((GridViewRow)parameter).DataItem;
                    var id = drv["CustomMetricID"];

                    // Create security hash
                    string validationHash = QueryHelper.GetHash(String.Format("?custommetricID={0}", id));

                    edit.OnClientClick = String.Format("Edit_{0}('{1}', '{2}');return false;", ClientID, id, validationHash);
                }

                break;

            case "delete":
                var delete = sender as CMSGridActionButton;

                if (delete != null)
                {
                    delete.OnClientClick = String.Format("Delete({0});", delete.CommandArgument);
                }

                break;

            case "codename":
            case "parent":
                return GetString(ValidationHelper.GetString(parameter, String.Empty));
        }

        return parameter;
    }
}