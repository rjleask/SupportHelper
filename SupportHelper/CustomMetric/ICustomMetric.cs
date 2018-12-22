using System.Collections.Generic;

namespace SupportHelper
{
	/// <summary>
	/// Defines a common interface for custom metrics.
	/// </summary>
	public interface ICustomMetric
	{
		/// <summary>
		/// Gets the custom metric data.
		/// </summary>
		IEnumerable<KeyValuePair<string, string>> GetCustomMetricData();
	}
}
