using CMS;
using CMS.DataEngine;
using CMS.MacroEngine;

[assembly: RegisterModule(typeof(SupportHelper.SupportHelperModule))]

namespace SupportHelper
{
	public class SupportHelperModule : Module
	{
		public SupportHelperModule()
			: base("SupportHelperModule", true)
		{
		}

		protected override void OnInit()
		{
			MacroContext.GlobalResolver.SetHiddenNamedSourceData("SupportHelper", SupportHelper.Instance);
		}
	}
}