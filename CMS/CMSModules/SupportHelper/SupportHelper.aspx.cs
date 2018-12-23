using System;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.Base.Web.UI;
using CMS.Base.Web.UI.ActionsConfig;
using CMS.FormEngine.Web.UI;
using CMS.Helpers;
using CMS.SiteProvider;
using CMS.UIControls;

using SupportHelper;

[Title("support.title")]
[UIElement("SupportHelper", "SupportHelperPage", true, false)]
public partial class CMSModules_SupportHelper_SupportHelper : CMSModalPage, ICallbackEventHandler
{
    #region Fields

    private const string SELECT_DESELECT_LINKS = @"&nbsp;(<span class='link' onclick=""SelectAllSubelements($cmsj(this), {0}, {1}); {2}; return false;"">{3}</span>,&nbsp;<span class='link' onclick=""DeselectAllSubelements($cmsj(this), {0}, {1}); {2};return false;"" >{4}</span>)";
    private string mCallbackRef;
    private bool mSubmitValid = true;

    #endregion Fields

    #region Properties

    public CMSForm FakeForm
    {
        get
        {
            if (fakeForm.EditedObject == null || !RequestHelper.IsPostBack())
            {
                fakeForm.EditedObject = DocumentManager.Node;
            }

            return fakeForm;
        }
    }

    private string CallbackRef
    {
        get
        {
            if (String.IsNullOrEmpty(mCallbackRef))
            {
                mCallbackRef = ClientScript.GetCallbackEventReference(this, "hdnValue.value", "callbackHandler", "callbackHandler");
            }

            return mCallbackRef;
        }
    }

    /// <summary>
    /// Metrics containing enabled metrics.
    /// </summary>
    private Metrics Metrics
    {
        get
        {
            if (WindowHelper.GetItem("Metrics") == null || !RequestHelper.IsPostBack())
            {
                try
                {
                    Metrics metrics = new Metrics();
                    WindowHelper.Add("Metrics", metrics);
                }
                catch (Exception ex)
                {
                    AddError(Regex.Replace(CMS.EventLog.EventLogProvider.GetExceptionLogMessage(ex), @"\r\n?|\n", "<br />"));
                    return null;
                }
            }

            return WindowHelper.GetItem("Metrics") as Metrics;
        }

        set
        {
            WindowHelper.Add("Metrics", value);
        }
    }

    private string InfoToggleField => String.Format("{0}__infoTextToggle", ClientID);

    private string WarningToggleField => String.Format("{0}__warningTextToggle", ClientID);

    private string ErrorToggleField => String.Format("{0}__errorTextToggle", ClientID);

    #endregion Properties

    #region Page events

    protected override void OnInit(EventArgs e)
    {
        base.OnInit(e);

        // This must be in OnInit
        metricsControl.OnNodeCreated += CreateMetricsNode;
    }

    protected override void OnPreRender(EventArgs e)
    {
        base.OnPreRender(e);

        SetMessagesPlaceHolder();
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        // Alter page to work as an application
        if (QueryHelper.GetBoolean("application", false))
        {
            PageTitle.Visible = false;
            CurrentMaster.FooterContainer.Visible = false;
            HeaderActions.AddAction(new HeaderAction { Index = -2, Text = GetString("support.submit"), EventName = ComponentEvents.SUBMIT, OnClientClick = "StoreDescriptionForPostback();" });

            // Register save
            ComponentEvents.RequestEvents.RegisterForComponentEvent("Submit", ComponentEvents.SUBMIT, (s, args) =>
            {
                Submit(s, args);
            });
        }
        else
        {
            // Set submit button
            SetSaveResourceString("support.submit");
            SetSaveJavascript("StoreDescriptionForPostback();");
            Save += Submit;
        }

        RegisterCSS();
        SetCategoriesDropdown();
        SetFormControls();
        RegisterScripts();

        // Everything afterward only once
        if (RequestHelper.IsPostBack())
        {
            // Restore description content from hidden field
            txtDescription.Controls.Add(new LiteralControl(hdnDescription.Value));
            return;
        }

        // Remove any stale window objects
        WindowHelper.Remove(WarningToggleField);
        WindowHelper.Remove(ErrorToggleField);
        WindowHelper.Remove("Metrics");

        // Set url
        txtURL.Text = GetURL();

        // Set resource strings
        PageTitle.TitleText = GetString("support.title");
        txtName.Attributes.Add("placeholder", GetString("support.name.placeholder"));
        txtCategoryCustom.Attributes.Add("placeholder", GetString("support.category.custom.placeholder"));
        txtOrganization.Attributes.Add("placeholder", GetString("support.organization.placeholder"));
        txtURL.Attributes.Add("placeholder", GetString("support.url.placeholder"));
        (txtEmail.Controls[0] as CMSTextBox).Attributes.Add("placeholder", GetString("support.email.placeholder"));
        lnkAdvanced.Text = icAdvanced.AlternativeText = GetString("support.metrics.flyopen");
    }

    /// <summary>
    /// Validate inputs (category, custom category, URL, email) and submit submission data.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Submit(object sender, EventArgs e)
    {
        // Category is a header (not set)
        AddError(ValidationHelper.GetString(drpCategory.SelectedValue, "header").Contains("header"), lblCategory, GetString("support.category.validation"));
        // Custom category is empty
        AddError(txtCategoryCustom.Visible && String.IsNullOrEmpty(ValidationHelper.GetString(txtCategoryCustom.Text, null)), lblCategory, GetString("support.category.custom.validation"));

        // URL is invalid
        AddError(!String.IsNullOrEmpty(txtURL.Text) && !ValidationHelper.IsURL(txtURL.Text) && !URLHelper.IsLocalUrl(txtURL.Text), lblURL, GetString("support.url.validation"));

        // Email is invalid
        AddError(!String.IsNullOrEmpty(txtEmail.Value.ToString()) && !txtEmail.IsValid(), lblEmail, GetString("general.correctemailformat"));

        // Email is empty
        AddError(String.IsNullOrEmpty(txtEmail.Value.ToString()), lblEmail, GetString("general.requireemail"));

        if (mSubmitValid)
        {
            try
            {
                // Create POST fields
                MultipartFormDataContent submission = new MultipartFormDataContent();
                FillSubmission(ref submission);
                Metrics.SendSubmission(submission);
            }
            catch (Exception ex)
            {
                // Log exception messages
                AddError(Regex.Replace(CMS.EventLog.EventLogProvider.GetExceptionLogMessage(ex), @"\r\n?|\n", "<br />"));

                mSubmitValid = false;
            }
        }

        if (!mSubmitValid)
        {
            // Add header message to the error box
            MessagesPlaceHolder.PrependError(GetString("basicform.errorvalidationerror|general.errorvalidationerror"), "<br /><br />");
            return;
        }

        // Alter close to work as an application
        if (QueryHelper.GetBoolean("application", false))
        {
            RedirectToInformation("support.success");
        }
        else
        {
            // Close dialog
            ScriptHelper.RegisterStartupScript(this, typeof(string), "Close", "CloseDialog();", true);
        }
    }

    #endregion Page events

    #region Control events

    /// <summary>
    /// Check to see if the custom category needs to be shown.
    /// </summary>
    protected void ShowCustomCategory(object sender, EventArgs e)
    {
        txtCategoryCustom.Visible = drpCategory.SelectedValue == drpCategory.Items[drpCategory.Items.Count - 1].Value;
    }

    private TreeNode CreateMetricsNode(DataRow childNode, TreeNode createdNode)
    {
        // Get data
        if (childNode != null)
        {
            Metric metric = (Metric)childNode;

            MetricsProvider mp = new MetricsProvider();

            int id = ValidationHelper.GetInteger(metric.MetricId, 0);
            bool selected = ValidationHelper.GetBoolean(metric.MetricSelected, false);
            string displayName = HTMLHelper.HTMLEncode(ResHelper.LocalizeString(ValidationHelper.GetString(metric.MetricDisplayName, String.Empty)));
            string itemClass = "ContentTreeItem";
            string onClickDeclaration = String.Format(" var chkElem_{0} = document.getElementById('chk_{0}'); ", id);
            string onClickCommon = String.Format("  hdnValue.value = {0} + ';' + chkElem_{0}.checked; {1};", id, CallbackRef);
            string onClickSpan = String.Format(" chkElem_{0}.checked = !chkElem_{0}.checked; ", id);

            // Get button links
            string links = null;

            string nodeText;

            string format = @"<span class='checkbox tree-checkbox'><input type='checkbox' id='chk_{0}' name='chk_{0}'{1}{2} onclick=""{3}"" /><label for='chk_{0}'>&nbsp;</label><span class='{4}' onclick=""{5} return false;""><span class='Name'>{6}</span></span></span>{7}";

            // Parent or grandparent
            if (ValidationHelper.GetString(metric.MetricPath, String.Empty).Length < 27)
            {
                links = String.Format(SELECT_DESELECT_LINKS, id, "true", CallbackRef, GetString("uiprofile.selectall"), GetString("uiprofile.deselectall"));
                format = @"<span><span class='{4}'><span style='cursor: initial;'>{6}</span></span></span>{7}";
            }

            nodeText = String.Format(format,
                                        id,
                                        String.Empty,
                                        selected ? " checked='checked'" : String.Empty,
                                        onClickDeclaration + onClickCommon,
                                        itemClass,
                                        onClickDeclaration + onClickSpan + onClickCommon,
                                        displayName,
                                        links);

            createdNode.NavigateUrl = "#";

            createdNode.Text = nodeText;
        }

        return createdNode;
    }

    protected void ExpandMetrics(object sender, EventArgs e)
    {
        // Switch the mode
        metricsControl.Visible = !metricsControl.Visible;

        if (!metricsControl.Visible)
        {
            lnkAdvanced.Text = icAdvanced.AlternativeText = GetString("support.metrics.flyopen");
            lnkAdvanced.OnClientClick = "return MetricsConfirm();";
            icAdvanced.CssClass = "icon-caret-down cms-icon-30";

            WindowHelper.Remove(WarningToggleField);
        }
        else
        {
            lnkAdvanced.Text = icAdvanced.AlternativeText = GetString("support.metrics.flyclose");
            lnkAdvanced.OnClientClick = String.Empty;
            icAdvanced.CssClass = "icon-caret-up cms-icon-30";

            AddWarning(GetString("support.metrics.warning"));
        }
    }

    #endregion Control events

    #region Setup methods

    private void RegisterCSS()
    {
        CssRegistration.RegisterCssBlock(this, "SupportHelper", String.Format(@"
			.ck-link-form .ck-input-text {{
				width: inherit !important;
			}}
            .cms-bootstrap .editing-form-value-cell i {{
                margin: inherit;
            }}
			.cms-bootstrap .ck-editor__editable a:hover,
			.cms-bootstrap .ck-editor__editable .link:hover,
			.cms-bootstrap .ck-editor__editable a:focus,
			.cms-bootstrap .ck-editor__editable .link:focus {{
				color: black;
				text-decoration: none;
			}}
			.cms-bootstrap .ck-editor__editable h2,
			.cms-bootstrap .ck-editor__editable h3,
			.cms-bootstrap .ck-editor__editable h4,
			.cms-bootstrap .ck-editor__editable strong {{
				color: black;
			}}
			#{0} {{
				height: 300px;
				max-width: 500px !important;
				border: 2px solid #bdbbbb;
				border-radius: 3px;
			}}
			#{0}:focus {{
				border-bottom: 2px solid #1175ae;
			}}
			#{1} {{
				margin: 7px 0 0;
			}}
			#{2} {{
				display: block;
			}}
			.PLCFix > div {{
				top: 70px;
			}}
		", txtDescription.ClientID, txtCategoryCustom.ClientID, drpCategory.ClientID));
    }

    /// <summary>
    /// Show or persist messages by checking the hidden fields that Javascript toggles when the boxes are closed.
    /// </summary>
    private void SetMessagesPlaceHolder()
    {
        string mErrorToggleField = "0";
        string mWarningToggleField = "0";

        if (!RequestHelper.IsPostBack() || Request.Form[InfoToggleField] == "1")
        {
            ShowInformation(GetString("support.ui.tip"));
        }

        if (!String.IsNullOrEmpty(MessagesPlaceHolder.WarningText) || (Request.Form[WarningToggleField] == "1" && WindowHelper.GetItem(WarningToggleField) != null))
        {
            var warningText = String.IsNullOrEmpty(MessagesPlaceHolder.WarningText) ? WindowHelper.GetItem(WarningToggleField) : MessagesPlaceHolder.WarningText;

            mWarningToggleField = "1";
            WindowHelper.Add(WarningToggleField, warningText);
            ShowWarning(warningText.ToString());
        }
        else
        {
            MessagesPlaceHolder.WarningText = null;
        }

        if (!String.IsNullOrEmpty(MessagesPlaceHolder.ErrorText) || (Request.Form[ErrorToggleField] == "1" && WindowHelper.GetItem(ErrorToggleField) != null))
        {
            var errorText = String.IsNullOrEmpty(MessagesPlaceHolder.ErrorText) ? WindowHelper.GetItem(ErrorToggleField) : MessagesPlaceHolder.ErrorText;

            if (!mSubmitValid)
            {
                WindowHelper.Add(ErrorToggleField, errorText);
            }

            mErrorToggleField = "1";
            ShowError(errorText.ToString());
        }

        ScriptHelper.RegisterHiddenField(this, InfoToggleField, Request.Form[InfoToggleField] ?? "1");
        ScriptHelper.RegisterHiddenField(this, WarningToggleField, mWarningToggleField);
        ScriptHelper.RegisterHiddenField(this, ErrorToggleField, mErrorToggleField);
    }

    private void SetCategoriesDropdown()
    {
        // Store selection in case of postback (which clears custom attributes)
        int selected = drpCategory.SelectedIndex;
        drpCategory.Items.Clear();

        drpCategory.Items.AddRange(new ListItem[] {
            BuildCategoryItem("support.category.header.choose", true),
            BuildCategoryItem("support.category.header.application", true),
            BuildCategoryItem("support.category.admin"),
            BuildCategoryItem("support.category.upgrade"),
            BuildCategoryItem("support.category.header.content", true),
            BuildCategoryItem("support.category.pages"),
            BuildCategoryItem("support.category.workflows"),
            BuildCategoryItem("support.category.forms"),
            BuildCategoryItem("support.category.medialibraries"),
            BuildCategoryItem("support.category.localization"),
            BuildCategoryItem("support.category.header.om", true),
            BuildCategoryItem("support.category.contacts"),
            BuildCategoryItem("support.category.campaigns"),
            BuildCategoryItem("support.category.newsletters"),
            BuildCategoryItem("support.category.header.ecommerce", true),
            BuildCategoryItem("support.category.products"),
            BuildCategoryItem("support.category.orders"),
            BuildCategoryItem("support.category.discounts"),
            BuildCategoryItem("support.category.store"),
            BuildCategoryItem("support.category.header.social", true),
            BuildCategoryItem("support.category.events"),
            BuildCategoryItem("support.category.forums"),
            BuildCategoryItem("support.category.blogs"),
            BuildCategoryItem("support.category.header.development", true),
            BuildCategoryItem("support.category.cssjavascript"),
            BuildCategoryItem("support.category.formcontrols"),
            BuildCategoryItem("support.category.webparts"),
            BuildCategoryItem("support.category.modules"),
            BuildCategoryItem("support.category.mvc"),
            BuildCategoryItem("support.category.header.configuration", true),
            BuildCategoryItem("support.category.permissions"),
            BuildCategoryItem("support.category.staging"),
            BuildCategoryItem("support.category.users"),
            BuildCategoryItem("support.category.webfarm"),
            BuildCategoryItem("support.category.sites"),
            BuildCategoryItem("support.category.integrations"),
            BuildCategoryItem("support.category.spacer", true),
            BuildCategoryItem("support.category.custom")
        });

        // Restore selection
        drpCategory.SelectedIndex = selected;
    }

    /// <summary>
    /// Perform necessary steps for uploader, description, url, email, and metrics.
    /// </summary>
    private void SetFormControls()
    {
        // Set description
        lblDescription.Attributes.Add("onclick", String.Format("document.getElementById('{0}').focus()", txtDescription.ClientID));

        // Set file uploader
        DocumentManager.Mode = FormModeEnum.Insert;
        DocumentManager.NewNodeClassName = "CMS.Root";
        DocumentManager.Node.NodeParentID = 1;
        mfuUploader.Form = FakeForm;
        mfuUploader.ResourcePrefix = "support";
        mfuUploader.SetValue("paging", false);
        mfuUploader.SetValue("extensions", "custom");
        mfuUploader.SetValue("allowed_extensions", "config;csv;pdf;doc;docx;ppt;pptx;xls;xlsx;htm;html;xml;bmp;gif;jpg;jpeg;png;wav;wma;wmv;mp3;mp4;mpg;mpeg;mov;avi;swf;rar;zip;7z;txt;rtf;xlf");
        mfuUploader.MessagesPlaceHolder.ContainerCssClass += "PLCFix";

        // Set email
        txtEmail.AllowMultipleAddresses = true;

        // Set metrics control (UniTree)
        if (Metrics == null)
        {
            metricsControl.StopProcessing = true;
            icAdvanced.Visible = false;
            lnkAdvanced.Visible = false;
        }
        else
        {
            metricsControl.Metrics = Metrics;
            metricsControl.UsePostBack = false;
            metricsControl.ReloadData();
        }
    }

    private void RegisterScripts()
    {
        ScriptHelper.RegisterJQuery(this);
        StringBuilder sbs = new StringBuilder();
        StringBuilder sbc = new StringBuilder();

        // Persist messages placeholder
        sbc.Append(String.Format(@"function SetMessages(id) {{
										document.getElementById(id).value = 0;
									}}
									setTimeout(function() {{
										if (document.querySelector('.alert-info .close') != null) {{
											document.querySelector('.alert-info .close').addEventListener('click', function() {{ SetMessages('{0}') }})
										}};
										if (document.querySelector('.alert-warning .close') != null) {{
											document.querySelector('.alert-warning .close').addEventListener('click', function() {{ SetMessages('{1}') }})
										}};
										if (document.querySelector('.alert-error .close') != null) {{
											document.querySelector('.alert-error .close').addEventListener('click', function() {{ SetMessages('{2}') }})
										}};
									}}, 0);", InfoToggleField, WarningToggleField, ErrorToggleField));

        // Store CKEditor content
        sbc.Append(String.Format(@"function StoreDescriptionForPostback() {{
										document.getElementById('{0}').value = document.getElementById('{1}').innerHTML;
									}}", hdnDescription.ClientID, txtDescription.ClientID));

        // CKEditor
        ScriptHelper.RegisterScriptFile(this, "CMSModules/SupportHelper/ckeditor_balloon_11_2_0.js", false);
        sbs.Append(String.Format(@"if(!document.querySelector('#{0}').hasAttribute('contenteditable')) {{
			BalloonEditor.create(document.querySelector('#{0}'), {{toolbar: [ 'heading', '|', 'bold', 'italic', 'link', 'bulletedList', 'numberedList', 'blockQuote' ]}}).catch (error => {{console.error(error);}})}};", txtDescription.ClientID));

        // Get current page link if in Pages
        if (!RequestHelper.IsPostBack())
        {
            sbs.Append(String.Format("document.getElementById('{0}').value = parent.document.getElementById('m_c_lnkLiveSite').href;", txtURL.ClientID));
        }

        // Confirm metrics
        sbs.Append(String.Format("function MetricsConfirm() {{return confirm('{0}');}}", GetString("support.metrics.warning")));

        // Metrics control
        sbs.Append(String.Format("var hdnValue = document.getElementById('{0}'); function callbackHandler(content, context) {{}}", hdnValue.ClientID));

        string script = string.Format(@"
			function SelectAllSubelements(elem, id, hasChkBox) {{
					hdnValue.value = 's;' + id + ';' + (hasChkBox? 1 : 0);
					var tab = elem.parents('table');
					tab.find('input:enabled[type=checkbox]').prop('checked', true);
					var node = tab.next();
					if ((node.length > 0)&&(node[0].nodeName.toLowerCase() == 'div')) {{
						node.find('input:enabled[type=checkbox]').prop('checked', true);
					}}
			}}
			function DeselectAllSubelements(elem, id, hasChkBox) {{
				if ((confirm('{0}'))) {{
					hdnValue.value = 'd;' + id + ';' + (hasChkBox? 1 : 0);
					var tab = elem.parents('table');
					tab.find('input:enabled[type=checkbox]').removeAttr('checked');
					var node = tab.next();
					if ((node.length > 0)&&(node[0].nodeName.toLowerCase() == 'div')) {{
						node.find('input:enabled[type=checkbox]').removeAttr('checked');
					}}
				}}
			}}", GetString("support.deselectallconfirmation"));

        sbc.Append(script);

        ScriptHelper.RegisterClientScriptBlock(this, typeof(string), "ClientScripts", sbc.ToString(), true);
        ScriptHelper.RegisterStartupScript(this, typeof(string), "StartupScripts", sbs.ToString(), true);

        ScriptHelper.RegisterCompletePageScript(this);
        ScriptHelper.RegisterLoader(this);
        ScriptHelper.RegisterTooltip(this);
        ScriptHelper.RegisterEditScript(this);
    }

    /// <summary>
    /// Get site link from current context. Javascript checks Live site button for full path.
    /// </summary>
    private string GetURL()
    {
        return URLHelper.GetAbsoluteUrl(SiteContext.CurrentSite.DomainName);
    }

    #endregion Setup methods

    #region Action methods

    private ListItem BuildCategoryItem(string resourceString, bool header = false)
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

    private void DeSelectAllMetrics(int parentId, bool select = true)
    {
        // Get parent
        Metric parent = (from Metric m in Metrics.Rows where m.MetricId == ValidationHelper.GetInteger(parentId, 0) select m).First();
        parent.MetricSelected = select;

        // Get children
        var children = from Metric m in Metrics.Rows where (m.MetricParent == null ? 0 : m.MetricParent.MetricId) == ValidationHelper.GetInteger(parent.MetricId, 0) select m;

        foreach (Metric child in children)
        {
            child.MetricSelected = select;

            // Get grandchildren
            var gchildren = from Metric m in Metrics.Rows where (m.MetricParent == null ? 0 : m.MetricParent.MetricId) == ValidationHelper.GetInteger(child.MetricId, 0) select m;

            foreach (Metric gchild in gchildren)
            {
                gchild.MetricSelected = select;
            }
        }
    }

    private MultipartFormDataContent FillSubmission(ref MultipartFormDataContent submission)
    {
        // Add category
        string category = ValidationHelper.GetString(drpCategory.SelectedValue, "Unknown").Contains("custom") ? ValidationHelper.GetString(txtCategoryCustom.Text, String.Empty) : GetString(drpCategory.SelectedValue);
        submission.AddSubmissionItem("support.category", "form", category);

        // Add description
        submission.AddSubmissionItem("support.description", "form", HTMLHelper.RemoveScripts(hdnDescription.Value));

        // Add files
        var files = CMS.DocumentEngine.AttachmentInfoProvider.GetAttachments().WhereEquals("AttachmentFormGUID", mfuUploader.Form.FormGUID);

        if (files.Count > 0)
        {
            int i = 0;
            foreach (var attachment in files)
            {
                string name = ValidationHelper.GetString(attachment.AttachmentName, String.Empty);
                submission.AddSubmissionItem(String.Format("{0}_{1}", "support.files", i++), attachment.AttachmentBinary, attachment.AttachmentMimeType, name);
            }
        }

        // Add URL
        submission.AddSubmissionItem("support.url", "form", txtURL.Text);

        // Add name
        submission.AddSubmissionItem("support.name", "form", txtName.Text);

        // Add organization
        submission.AddSubmissionItem("support.organization", "form", txtOrganization.Text);

        // Add email
        submission.AddSubmissionItem("support.email", "form", txtEmail.Text);

        return submission;
    }

    private void AddError(bool condition, LocalizedLabel label, string validationResult)
    {
        if (condition)
        {
            string anchorScript = label != null ? String.Format("onclick=\"{0}\";", MessagesPlaceHolder.GetAnchorScript(label.ClientID, null)) : String.Empty;

            AddError(label.GetText(), validationResult, anchorScript);

            mSubmitValid = false;
        }
    }

    private void AddError(string label, string validationResult, string anchorScript = null)
    {
        string message = String.Format("<span class=\"Anchor\" {0} >{1}</span> {2}", anchorScript, label, validationResult);

        AddError(message);
    }

    #endregion Action methods

    #region Callback handling

    public string GetCallbackResult()
    {
        return String.Empty;
    }

    // Update metrics
    public void RaiseCallbackEvent(string eventArgument)
    {
        string[] args = eventArgument.Split(";".ToArray(), StringSplitOptions.RemoveEmptyEntries);

        int id = 0;

        switch (args.Length)
        {
            case 2:
                // Basic checkbox click
                id = ValidationHelper.GetInteger(args[0], 1) - 1;

                // De/select item
                Metrics[id].MetricSelected = ValidationHelper.GetBoolean(args[1], false);
                break;

            case 3:
                // Mass deselect
                id = ValidationHelper.GetInteger(args[1], 0);

                if (args[0] == "s")
                {
                    DeSelectAllMetrics(id);
                }
                else if (args[0] == "d")
                {
                    DeSelectAllMetrics(id, false);
                }
                break;
        }
    }

    #endregion Callback handling
}