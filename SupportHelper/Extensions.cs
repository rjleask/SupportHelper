using CMS.Helpers;
using System.Collections.Generic;
using System.Net.Http;

namespace SupportHelper
{
	public static class Extensions
	{
		/// <summary>
		/// Adds <see cref="byte[]"/> submission item to given MultipartFormDataContent submission.
		/// </summary>
		public static void AddSubmissionItem(this MultipartFormDataContent submission, string metricCodeName, byte[] data, string contentType, string fileName)
		{
			var bytesContent = new ByteArrayContent(data);
			bytesContent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment")
			{
				FileName = fileName,
				Name = ResHelper.GetString(metricCodeName)
			};
			bytesContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);

			AddSubmissionItemInternal(submission, metricCodeName, bytesContent);
		}

		/// <summary>
		/// Adds <see cref="string"/> submission item to given MultipartFormDataContent submission.
		/// </summary>
		public static void AddSubmissionItem(this MultipartFormDataContent submission, string metricCodeName, string category, string data)
		{
			AddSubmissionItem(submission, metricCodeName, category, new [] { new KeyValuePair<string, string>("0", data) });
		}

		internal static void AddSubmissionItem(this MultipartFormDataContent submission, string metricCodeName, string category, IEnumerable<KeyValuePair<string, string>> data)
		{
			var formContent = new FormUrlEncodedContent(data);
			formContent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-urlencoded")
			{
				Name = ResHelper.GetString(metricCodeName),
			};
			formContent.Headers.Add("category", category);

			AddSubmissionItemInternal(submission, metricCodeName, formContent);
		}

		private static void AddSubmissionItemInternal(MultipartFormDataContent submission, string metricCodeName, HttpContent content)
		{
			content.Headers.Add("codeName", metricCodeName);

			submission.Add(content);
		}
	}
}
