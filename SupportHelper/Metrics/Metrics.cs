using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using CMS.Base;
using CMS.EventLog;
using CMS.Helpers;

namespace SupportHelper
{
    /// <summary>
    /// DataTable that stores Metrics as DataRows.
    /// </summary>
    public class Metrics : DataTable
    {
        #region Fields

        private int metricIdIncrement = 1;
        private readonly string metricPathFormat = "00000000";
        private HashSet<string> mDisabledMetrics;

        #endregion Fields

        #region Properties

        private HashSet<string> DisabledMetrics
        {
            get
            {
                if (mDisabledMetrics == null)
                {
                    string key = ValidationHelper.GetString(SettingsHelper.AppSettings["SHDisabledMetrics"], String.Empty);
                    mDisabledMetrics = new HashSet<string>(key.Split(';'));
                }
                return mDisabledMetrics;
            }
        }

        private string Endpoint
        {
            get
            {
                var url = ValidationHelper.GetString(SettingsHelper.AppSettings["SHSubmitEndpoint"], String.Empty);

                if (!String.IsNullOrEmpty(url))
                {
                    url = URLHelper.GetAbsoluteUrl(url);

                    if (ValidationHelper.IsURL(url))
                    {
                        return url;
                    }
                }

                throw new Exception("Endpoint URL cannot be determined. Check SHSubmitEndpoint key.");
            }
        }

        #endregion Properties

        #region Constructors

        public Metrics()
        {
            Columns.AddRange(
                new[] {
                    new DataColumn("MetricId", typeof(int)),
                    new DataColumn("MetricDisplayName", typeof(string)),
                    new DataColumn("MetricCodeName", typeof(string)),
                    new DataColumn("MetricParent", typeof(Metric)),
                    new DataColumn("MetricHasChildren", typeof(string)),
                    new DataColumn("MetricSelected", typeof(bool)),
                    new DataColumn("MetricDataClass", typeof(string)),
                    new DataColumn("MetricPath", typeof(string)),
                }
            );

            SetMetrics();
        }

        #endregion Constructors

        #region Protected override methods

        protected override Type GetRowType()
        {
            return typeof(Metric);
        }

        protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
        {
            return new Metric(builder);
        }

        #endregion Protected override methods

        #region Private methods

        private Metric GetNewRow()
        {
            Metric row = (Metric)NewRow();

            return row;
        }

        private string BuildPath(Metric metric)
        {
            StringBuilder sb = new StringBuilder();

            Metric m = metric;
            sb.AppendFormat("/{0}", m.MetricId.ToString(metricPathFormat));

            while (m.MetricParent != null)
            {
                sb.Insert(0, String.Format("/{0}", m.MetricParent.MetricId.ToString(metricPathFormat)));
                m = m.MetricParent;
            }

            return sb.ToString();
        }

        private Metric AddMetric(string codeName, Metric parent, string assembly = null, string @class = null, bool selected = true)
        {
            Metric metric = GetNewRow();

            // Deselect if the code name appears in the SHDisabledMetrics web.config key
            if (DisabledMetrics.Contains(codeName))
            {
                selected = false;
            }

            metric.MetricId = metricIdIncrement++;
            metric.MetricCodeName = codeName;
            metric.MetricParent = parent;
            metric.MetricSelected = selected;
            metric.MetricAssemblyName = assembly;
            metric.MetricClassName = @class;
            metric.MetricPath = BuildPath(metric);

            Rows.Add(metric);

            return metric;
        }

        /// <summary>
        /// Generate metrics.
        /// </summary>
        /// <param name="metrics"></param>
        private void SetMetrics()
        {
            // Root category
            Metric root = AddMetric(MetricDataEnum.support_metrics, null);

            // Top level categories
            Metric system = AddMetric(MetricDataEnum.support_metrics_system, root);
            Metric environment = AddMetric(MetricDataEnum.support_metrics_environment, root);
            Metric counters = AddMetric(MetricDataEnum.support_metrics_counters, root);
            Metric tasks = AddMetric(MetricDataEnum.support_metrics_tasks, root);
            Metric eventlog = AddMetric(MetricDataEnum.support_metrics_eventlog, root);

            #region System

            AddMetric(MetricDataEnum.support_metrics_system_version, system);
            AddMetric(MetricDataEnum.support_metrics_system_appname, system);
            AddMetric(MetricDataEnum.support_metrics_system_instancename, system);
            AddMetric(MetricDataEnum.support_metrics_system_physicalpath, system);
            AddMetric(MetricDataEnum.support_metrics_system_apppath, system);
            AddMetric(MetricDataEnum.support_metrics_system_uiculture, system);
            AddMetric(MetricDataEnum.support_metrics_system_installtype, system);
            AddMetric(MetricDataEnum.support_metrics_system_portaltemplatepage, system);
            AddMetric(MetricDataEnum.support_metrics_system_timesinceapprestart, system);
            AddMetric(MetricDataEnum.support_metrics_system_discoveredassemblies, system);
            AddMetric(MetricDataEnum.support_metrics_system_targetframework, system);
            AddMetric(MetricDataEnum.support_metrics_system_authmode, system);
            AddMetric(MetricDataEnum.support_metrics_system_sessionmode, system);
            AddMetric(MetricDataEnum.support_metrics_system_debugmode, system);
            AddMetric(MetricDataEnum.support_metrics_system_runallmanagedmodules, system);

            #endregion System

            #region Environment

            AddMetric(MetricDataEnum.support_metrics_environment_trustlevel, environment);
            AddMetric(MetricDataEnum.support_metrics_environment_iisversion, environment);
            AddMetric(MetricDataEnum.support_metrics_environment_https, environment);
            AddMetric(MetricDataEnum.support_metrics_environment_windowsversion, environment);
            AddMetric(MetricDataEnum.support_metrics_environment_netversion, environment);
            AddMetric(MetricDataEnum.support_metrics_environment_sqlserverversion, environment);
            AddMetric(MetricDataEnum.support_metrics_environment_azure, environment);
            AddMetric(MetricDataEnum.support_metrics_environment_amazon, environment);
            AddMetric(MetricDataEnum.support_metrics_environment_services, environment);

            #endregion Environment

            #region Counters

            AddMetric(MetricDataEnum.support_metrics_counters_webfarmservers, counters);
            AddMetric(MetricDataEnum.support_metrics_counters_stagingservers, counters);
            AddMetric(MetricDataEnum.support_metrics_counters_pagemostchildren, counters);
            AddMetric(MetricDataEnum.support_metrics_counters_modules, counters);
            AddMetric(MetricDataEnum.support_metrics_counters_medialibraries, counters);
            AddMetric(MetricDataEnum.support_metrics_counters_activities, counters);
            AddMetric(MetricDataEnum.support_metrics_counters_contacts, counters);
            AddMetric(MetricDataEnum.support_metrics_counters_contactgroups, counters);
            AddMetric(MetricDataEnum.support_metrics_counters_omrules, counters);
            AddMetric(MetricDataEnum.support_metrics_counters_products, counters);

            #endregion Counters

            #region Tasks

            AddMetric(MetricDataEnum.support_metrics_tasks_webfarm, tasks);
            AddMetric(MetricDataEnum.support_metrics_tasks_staging, tasks);
            AddMetric(MetricDataEnum.support_metrics_tasks_integration, tasks);
            AddMetric(MetricDataEnum.support_metrics_tasks_scheduled, tasks);
            AddMetric(MetricDataEnum.support_metrics_tasks_search, tasks);
            AddMetric(MetricDataEnum.support_metrics_tasks_email, tasks);

            #endregion Tasks

            #region Event log

            AddMetric(MetricDataEnum.support_metrics_eventlog_macroerrors, eventlog);
            AddMetric(MetricDataEnum.support_metrics_eventlog_stagingerrors, eventlog);
            AddMetric(MetricDataEnum.support_metrics_eventlog_searcherrors, eventlog);
            AddMetric(MetricDataEnum.support_metrics_eventlog_contenterrors, eventlog);
            AddMetric(MetricDataEnum.support_metrics_eventlog_exceptions, eventlog);
            AddMetric(MetricDataEnum.support_metrics_eventlog_upgrade, eventlog);

            #endregion Event log

            var customMetrics = CustomMetricInfoProvider.GetCustomMetrics();

            if (customMetrics.Count > 0)
            {
                foreach (CustomMetricInfo cmi in customMetrics)
                {
                    switch (cmi.CustomMetricParent)
                    {
                        case MetricDataEnum.support_metrics_system:
                            AddMetric(cmi.CustomMetricDisplayName, system, cmi.CustomMetricAssemblyName, cmi.CustomMetricClassName, cmi.CustomMetricSelected);
                            break;

                        case MetricDataEnum.support_metrics_counters:
                            AddMetric(cmi.CustomMetricDisplayName, counters, cmi.CustomMetricAssemblyName, cmi.CustomMetricClassName, cmi.CustomMetricSelected);
                            break;

                        case MetricDataEnum.support_metrics_tasks:
                            AddMetric(cmi.CustomMetricDisplayName, tasks, cmi.CustomMetricAssemblyName, cmi.CustomMetricClassName, cmi.CustomMetricSelected);
                            break;

                        case MetricDataEnum.support_metrics_eventlog:
                            AddMetric(cmi.CustomMetricDisplayName, eventlog, cmi.CustomMetricAssemblyName, cmi.CustomMetricClassName, cmi.CustomMetricSelected);
                            break;

                        case MetricDataEnum.support_metrics_environment:
                        default:
                            // Fall back to Environment when the parent does not match
                            AddMetric(cmi.CustomMetricDisplayName, environment, cmi.CustomMetricAssemblyName, cmi.CustomMetricClassName, cmi.CustomMetricSelected);
                            break;
                    }
                }
            }
        }

        #endregion Private methods

        #region Public methods

        /// <summary>
        /// Get metric by index.
        /// </summary>
        public Metric this[int idx]
        {
            get
            {
                return (Metric)Rows[idx];
            }
        }

        /// <summary>
        /// Add metrics to submission.
        /// </summary>
        public void SendSubmission(MultipartFormDataContent submission)
        {
            var contextRequestServerVariables = CMSHttpContext.Current.Request.ServerVariables;

            var dataCollection = new ConcurrentBag<Tuple<string, string, IEnumerable<KeyValuePair<string, string>>>>();

            // Generate metrics data based on selected metrics
            Parallel.For(0, Rows.Count, CMSThread.Wrap<int>(i =>
            {
                var metric = Rows[i] as Metric;

                // Skip row if disabled
                if (!metric.MetricSelected)
                {
                    return;
                }

                metric.MetricServerVariables = contextRequestServerVariables;

                var data = metric.MetricData;

                if (data != null)
                {
                    dataCollection.Add(Tuple.Create(metric.MetricCodeName, metric.MetricParent.MetricCodeName, data));
                }
            }, true));

            // Add all to submission
            foreach (var metricData in dataCollection)
            {
                submission.AddSubmissionItem(metricData.Item1, metricData.Item2, metricData.Item3);
            }

            LogSubmission(submission);

            // Send using HttpClient
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = client.PostAsync(Endpoint, submission).Result;

                if (!response.IsSuccessStatusCode)
                {
                    EventLogProvider.LogEvent(EventType.ERROR, "Support helper", "SEND", response.ReasonPhrase);
                    throw new IOException(response.ReasonPhrase);
                }
            }
        }

        #endregion Public methods

        #region Logging

        private void LogSubmission(MultipartFormDataContent submission)
        {
            if (!ValidationHelper.GetBoolean(SettingsHelper.AppSettings["SHLogSubmission"], false))
            {
                return;
            }

            StringBuilder log = new StringBuilder();

            log.AppendFormat("Endpoint: {0}{1}", Endpoint, Environment.NewLine);

            log.AppendFormat("Disabled metrics: {0}{1}", ValidationHelper.GetString(SettingsHelper.AppSettings["SHDisabledMetrics"], String.Empty), Environment.NewLine);

            var metricsProcessor = new MetricsProcessor(submission);

            log.AppendFormat("{0}{1}", metricsProcessor.JSON, Environment.NewLine);

            EventLogProvider.LogInformation("Support helper", "SUBMISSION", log.ToString());
        }

        #endregion Logging
    }
}