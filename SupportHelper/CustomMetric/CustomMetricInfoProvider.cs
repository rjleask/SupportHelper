using CMS.DataEngine;

namespace SupportHelper
{
	/// <summary>
	/// Class providing CustomMetricInfo management.
	/// </summary>
	public partial class CustomMetricInfoProvider : AbstractInfoProvider<CustomMetricInfo, CustomMetricInfoProvider>
	{
		#region "Constructors"

		/// <summary>
		/// Constructor
		/// </summary>
		public CustomMetricInfoProvider()
			: base(CustomMetricInfo.TYPEINFO)
		{
		}

		#endregion


		#region "Public methods - Basic"

		/// <summary>
		/// Returns a query for all the CustomMetricInfo objects.
		/// </summary>
		public static ObjectQuery<CustomMetricInfo> GetCustomMetrics()
		{
			return ProviderObject.GetCustomMetricsInternal();
		}


		/// <summary>
		/// Returns CustomMetricInfo with specified ID.
		/// </summary>
		/// <param name="id">CustomMetricInfo ID</param>
		public static CustomMetricInfo GetCustomMetricInfo(int id)
		{
			return ProviderObject.GetCustomMetricInfoInternal(id);
		}


		/// <summary>
		/// Returns CustomMetricInfo with specified name.
		/// </summary>
		/// <param name="name">CustomMetricInfo name</param>
		public static CustomMetricInfo GetCustomMetricInfo(string name)
		{
			return ProviderObject.GetCustomMetricInfoInternal(name);
		}


		/// <summary>
		/// Sets (updates or inserts) specified CustomMetricInfo.
		/// </summary>
		/// <param name="infoObj">CustomMetricInfo to be set</param>
		public static void SetCustomMetricInfo(CustomMetricInfo infoObj)
		{
			ProviderObject.SetCustomMetricInfoInternal(infoObj);
		}


		/// <summary>
		/// Deletes specified CustomMetricInfo.
		/// </summary>
		/// <param name="infoObj">CustomMetricInfo to be deleted</param>
		public static void DeleteCustomMetricInfo(CustomMetricInfo infoObj)
		{
			ProviderObject.DeleteCustomMetricInfoInternal(infoObj);
		}


		/// <summary>
		/// Deletes CustomMetricInfo with specified ID.
		/// </summary>
		/// <param name="id">CustomMetricInfo ID</param>
		public static void DeleteCustomMetricInfo(int id)
		{
			CustomMetricInfo infoObj = GetCustomMetricInfo(id);
			DeleteCustomMetricInfo(infoObj);
		}

		#endregion


		#region "Internal methods - Basic"
	
		/// <summary>
		/// Returns a query for all the CustomMetricInfo objects.
		/// </summary>
		protected virtual ObjectQuery<CustomMetricInfo> GetCustomMetricsInternal()
		{
			return GetObjectQuery();
		}    


		/// <summary>
		/// Returns CustomMetricInfo with specified ID.
		/// </summary>
		/// <param name="id">CustomMetricInfo ID</param>        
		protected virtual CustomMetricInfo GetCustomMetricInfoInternal(int id)
		{	
			return GetInfoById(id);
		}


		/// <summary>
		/// Returns CustomMetricInfo with specified name.
		/// </summary>
		/// <param name="name">CustomMetricInfo name</param>        
		protected virtual CustomMetricInfo GetCustomMetricInfoInternal(string name)
		{
			return GetInfoByCodeName(name);
		} 


		/// <summary>
		/// Sets (updates or inserts) specified CustomMetricInfo.
		/// </summary>
		/// <param name="infoObj">CustomMetricInfo to be set</param>        
		protected virtual void SetCustomMetricInfoInternal(CustomMetricInfo infoObj)
		{
			SetInfo(infoObj);
		}


		/// <summary>
		/// Deletes specified CustomMetricInfo.
		/// </summary>
		/// <param name="infoObj">CustomMetricInfo to be deleted</param>        
		protected virtual void DeleteCustomMetricInfoInternal(CustomMetricInfo infoObj)
		{
			DeleteInfo(infoObj);
		}	

		#endregion
	}
}