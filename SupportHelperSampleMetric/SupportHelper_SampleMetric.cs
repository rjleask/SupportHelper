using System.Collections.Generic;
using SupportHelper;

namespace CustomMetric
{
	/// <summary>
	/// This class will be automatically picked up by the custom metric settings in Settings > Integration > Support helper.
	/// </summary>
	public class SampleCustomMetric : ICustomMetric
	{
		/// <summary>
		/// Define the body of this method to retrieve metric data using the Kentico API or other methods.
		/// </summary>
		/// <remarks>
		/// Single fields must have the key part of the KeyValuePair to be unique integers as strings.
		/// </remarks>
		/// <returns>String representation of metric data.</returns>
		public IEnumerable<KeyValuePair<string, string>> GetCustomMetricData()
		{
			// The data is a single field
			return new[] { new KeyValuePair<string, string>("0", "sample data") };

			// The data is a list of single fields
			//return new[] { new KeyValuePair<string, string>("0", "sample data 1"),
			//				 new KeyValuePair<string, string>("1", "sample data 2") };

			// The data is a list of key value pairs
			//return new[] { new KeyValuePair<string, string>("sample data 1", "test value 1"),
			//			     new KeyValuePair<string, string>("sample data 2", "test value 2") };
		}
	}
}