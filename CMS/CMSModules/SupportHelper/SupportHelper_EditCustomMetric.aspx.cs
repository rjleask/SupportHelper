using CMS.Base.Web.UI;
using CMS.Helpers;
using CMS.UIControls;
using SupportHelper;
using System;
using System.Text;
using System.Web.UI.WebControls;

[EditedObject(CustomMetricInfo.OBJECT_TYPE,"custommetricID")]
public partial class CMSModules_SupportHelper_SupportHelper_EditCustomMetric : CMSModalPage
{
	protected void Page_Load(object sender, EventArgs e)
	{
		if (!QueryHelper.ValidateHash("hash")) return;

		// Set up UIForm
		formElem.SubmitButton.RegisterHeaderAction = false;
		formElem.SubmitButton.Visible = false;

		// Store selection in case of postback (which clears custom attributes)
		var drpParent = fParent.EditingControl as CMSFormControls_Basic_DropDownListControl;

		string selected = drpParent.SelectedValue == String.Empty ? (EditedObject as CustomMetricInfo).CustomMetricParent : drpParent.SelectedValue;

		drpParent.DropDownList.Items.Clear();

		// Set up form fields
		drpParent.DropDownList.Items.AddRange(new ListItem[] {
			GetCategory("support.category.header.choose", true),
			GetCategory("support.metrics.system"),
			GetCategory("support.metrics.environment"),
			GetCategory("support.metrics.counters"),
			GetCategory("support.metrics.ecommerce"),
			GetCategory("support.metrics.staging"),
			GetCategory("support.metrics.eventlog")
		});


		// Restore selection
		drpParent.SelectedValue = selected;

		CustomMetricInfo cmi = EditedObject as CustomMetricInfo;

		if (cmi?.CustomMetricID != 0)
		{
			SetSaveResourceString("general.saveandclose");
			SetTitle(GetString("support.metric.edit.title"));
		}
		else
		{
			SetSaveResourceString("general.addandclose");
			SetTitle(GetString("support.metric.add.title"));
		}

		Save += EditCustomMetric_Save;
	}

	/// <summary>
	/// Generate category option.
	/// </summary>
	private ListItem GetCategory(string resourceString, bool header = false)
	{
		ListItem li = new ListItem(String.Format("{0}{1}",
									header ? String.Empty : new String((char)160, 3),
									GetString(resourceString)), resourceString);

		if (header)
		{
			li.Attributes["disabled"] = "disabled";
		}

		return li;
	}

	/// <summary>
	/// Handle form validation and modal closing.
	/// </summary>
	private void EditCustomMetric_Save(object sender, EventArgs e)
	{
		if (!formElem.ValidateData())
		{
			// Something is invalid, stop save and let it show the message automatically.
			return;
		}

		formElem.SaveData(String.Empty);

		// Close dialog
		ScriptHelper.RegisterStartupScript(this, typeof(string), "Close", "wopener.location.reload(true); CloseDialog();", true);
	}
}