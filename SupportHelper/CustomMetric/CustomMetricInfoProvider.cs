using CMS.DataEngine;

namespace SupportHelper
{
    /// <summary>
    /// Class providing <see cref="CustomMetricInfo"/> management.
    /// </summary>
    public partial class CustomMetricInfoProvider : AbstractInfoProvider<CustomMetricInfo, CustomMetricInfoProvider>
    {
        /// <summary>
        /// Creates an instance of <see cref="CustomMetricInfoProvider"/>.
        /// </summary>
        public CustomMetricInfoProvider()
            : base(CustomMetricInfo.TYPEINFO)
        {
        }

        /// <summary>
        /// Returns a query for all the <see cref="CustomMetricInfo"/> objects.
        /// </summary>
        public static ObjectQuery<CustomMetricInfo> GetCustomMetrics()
        {
            return ProviderObject.GetObjectQuery();
        }

        /// <summary>
        /// Returns <see cref="CustomMetricInfo"/> with specified ID.
        /// </summary>
        /// <param name="id"><see cref="CustomMetricInfo"/> ID.</param>
        public static CustomMetricInfo GetCustomMetricInfo(int id)
        {
            return ProviderObject.GetInfoById(id);
        }

        /// <summary>
        /// Returns <see cref="CustomMetricInfo"/> with specified name.
        /// </summary>
        /// <param name="name"><see cref="CustomMetricInfo"/> name.</param>
        public static CustomMetricInfo GetCustomMetricInfo(string name)
        {
            return ProviderObject.GetInfoByCodeName(name);
        }

        /// <summary>
        /// Sets (updates or inserts) specified <see cref="CustomMetricInfo"/>.
        /// </summary>
        /// <param name="infoObj"><see cref="CustomMetricInfo"/> to be set.</param>
        public static void SetCustomMetricInfo(CustomMetricInfo infoObj)
        {
            ProviderObject.SetInfo(infoObj);
        }

        /// <summary>
        /// Deletes specified <see cref="CustomMetricInfo"/>.
        /// </summary>
        /// <param name="infoObj"><see cref="CustomMetricInfo"/> to be deleted.</param>
        public static void DeleteCustomMetricInfo(CustomMetricInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }

        /// <summary>
        /// Deletes <see cref="CustomMetricInfo"/> with specified ID.
        /// </summary>
        /// <param name="id"><see cref="CustomMetricInfo"/> ID.</param>
        public static void DeleteCustomMetricInfo(int id)
        {
            CustomMetricInfo infoObj = GetCustomMetricInfo(id);
            DeleteCustomMetricInfo(infoObj);
        }
    }
}