using System;

using CMS;
using CMS.Base;
using CMS.Base.Web.UI;
using CMS.Core;
using CMS.Helpers;
using CMS.UIControls;

namespace SupportHelper
{
    /// <summary>
    /// Support helper macro methods container.
    /// </summary>
	[RegisterAllProperties]
	public class SupportHelper : AbstractHierarchicalObject<SupportHelper>
	{
		private const string PAGE_PATH = "~/CMSModules/SupportHelper/SupportHelper.aspx";

		#region Properties

		/// <summary>
		/// Is web.config key SHEnableApp present and set to true.
		/// </summary>
		public bool IsAppEnabled
		{
			get
			{
				return ValidationHelper.GetBoolean(SettingsHelper.AppSettings["SHEnableApp"], true);
			}
		}

		/// <summary>
		/// URL of Support helper application page.
		/// </summary>
		public static string ApplicationUrl
		{
			get
			{
				return URLHelper.AppendQuery(URLHelper.GetAbsoluteUrl(PAGE_PATH), QueryHelper.BuildQueryWithHash("application", "true"));
			}
		}

		/// <summary>
		/// URL of Support helper dialog.
		/// </summary>
		public static string DialogUrl
		{
			get
			{
				return URLHelper.AppendQuery(URLHelper.GetAbsoluteUrl(PAGE_PATH), QueryHelper.BuildQueryWithHash());
			}
		}

		/// <summary>
		/// Script to open Support helper dialog.
		/// </summary>
		public static string DialogScript
		{
			get
			{
				ScriptHelper.RegisterDialogScript(PageContext.CurrentPage);

				return String.Format("modalDialog('{0}', '{1}', 850, 650)", DialogUrl, ResHelper.GetString("contexthelp.submitsupportissue"));
			}
		}

		/// <summary>
		/// Button to open Support helper dialog.
		/// </summary>
		public static string DialogButton
		{
			get
			{
				return String.Format("<button class=\"btn btn-primary support-helper\" onclick=\"{0}; return false;\">{1}</button>", DialogScript, ResHelper.GetString("contexthelp.submitsupportissue"));
			}
		}


		/// <summary>
		/// Script meant to be added to ~\CMSAdminControls\UI\ContextHelp.ascx to add a button to open Support helper dialog.
		/// </summary>
		[NotRegisterProperty]
		public static string ContextHelpScript
		{
			get
			{
				// Hide existing link and construct button for ContextHelp header
				string script = String.Format(@"document.querySelector('[href=""{0}""]').remove();
											if (document.querySelector('.support-helper') == null) {{
												var li = document.createElement(""li"");
												li.innerHTML = ""{1}"";
												document.querySelector('ul.cms-navbar-help').prepend(li)
											}};",
												ApplicationUIHelper.REPORT_BUG_URL,
												ScriptHelper.GetString(DialogButton, false));

				return ScriptHelper.GetScript(script);
			}
		}

		[NotRegisterProperty]
		internal static SupportHelper Instance
		{
			get
			{
				return ObjectFactory<SupportHelper>.StaticSingleton();
			}
		}

		#endregion
	}
}