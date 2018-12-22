using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Net.Mime;
using System.Threading.Tasks;
using System.Web.Http;

using CMS;
using CMS.DataEngine;
using CMS.EmailEngine;
using SupportHelper;

[assembly: RegisterModule(typeof(SupportHelperDestinationModule))]

/// <summary>
/// Sample controller on the supportsubmission/submit route.
/// </summary>
public class SubmitController : ApiController
{
	private const string SUPPORT_EMAIL = "support@kentico.com";
	private const string TEMPLATE_TEXT = "<table style=\"border-spacing: 0;\">{0}</table>";
	private const string SUBJECT_FORMAT = "Kentico {0} Support issue: {1}";
	private const string FORM_FIELD_FORMAT = "<tr><td id=\"{2}\" style=\"text-align: right;padding: 0.3em;font-family: sans-serif;border-right: 1px solid rgb(224, 224, 224);vertical-align: top;color: #007dcc;\"><strong>{0}</strong></td><td  style=\"padding: 0.3em;vertical-align: top;\">{1}</td></tr>";

	private static MetricsProcessor metricsProcessor;

	/// <summary>
	/// Handle HTTP POST request.
	/// </summary>
	[HttpPost]
	public async Task<HttpResponseMessage> Post()
	{
		// Ensure the request is valid
		if (!Request.Content.IsMimeMultipartContent("form-data"))
		{
			RedirectInvalidRequest();
		}

		// Take data from the request and convert to JSON data
		var data = await Request.Content.ReadAsMultipartAsync();

		metricsProcessor = new MetricsProcessor(data.Contents);

		BuildAndSendEmail();

		return Request.CreateResponse(HttpStatusCode.OK);
	}

	/// <summary>
	/// Customize this method to redirect invalid requests.
	/// </summary>
	[HttpGet]
	public void RedirectInvalidRequest()
	{
		throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
	}

	private static void BuildAndSendEmail()
	{
		var version = metricsProcessor.GetMetric<string>(MetricsProcessor.Category.Form, "support.metrics.system.version");
		var category = metricsProcessor.GetMetric<string>(MetricsProcessor.Category.Form, "support.category");
		var email = metricsProcessor.GetMetric<string>(MetricsProcessor.Category.Form, "support.email");

        string from;
        string cc;
		GetEmails(email, out from, out cc);

		var subject = String.Format(SUBJECT_FORMAT, version, category);

		var body = GetBody();

		EmailMessage supportEmail = new EmailMessage
		{
			EmailFormat = EmailFormatEnum.Html,
			From = from,
			CcRecipients = cc,
			Subject = subject,
			Recipients = SUPPORT_EMAIL,
			Body = body,
		};

		// Create attachment from metrics JSON
		var metricsAttachment = Attachment.CreateAttachmentFromString(metricsProcessor.JSON, "Instance metrics.json");
		metricsAttachment.ContentType = new ContentType("application/json");

		// Attach metrics JSON and other attachments to email
		supportEmail.Attachments.Add(metricsAttachment);
		metricsProcessor.Attachments.ForEach(a => supportEmail.Attachments.Add(a));

		// Send email
		EmailSender.SendEmail(supportEmail);
	}

	private static void GetEmails(string emails, out string from, out string cc)
	{
		int separator = emails.IndexOf(';');
		from = emails;
		cc = null;

		if (separator > 0)
		{
			from = emails.Substring(0, separator);
			cc = emails.Substring(separator + 1, emails.Length - separator - 1);
		}
	}

	private static string GetBody()
	{
		return String.Format(TEMPLATE_TEXT,
								String.Join("", metricsProcessor.FormFields.Select(field =>
									String.Format(FORM_FIELD_FORMAT, field.Value.Key, field.Value.Value, field.Key)
									)
								)
							);
	}

}

/// <summary>
/// Small module to map the destination route when the system initializes.
/// </summary>
public class SupportHelperDestinationModule : Module
{
	public SupportHelperDestinationModule() : base("SupportHelperDestinationModule", true)
	{
	}

	protected override void OnInit()
	{
		// This must match the SHSubmitEndpoint web.config key in the source instance.
		GlobalConfiguration.Configuration.Routes.MapHttpRoute("support", "supportsubmission/submit", new { controller = "submit" });
	}
}
