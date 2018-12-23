using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web.Configuration;

using CMS.Activities;
using CMS.Base;
using CMS.ContactManagement;
using CMS.Core;
using CMS.DataEngine;
using CMS.Ecommerce;
using CMS.EmailEngine;
using CMS.EventLog;
using CMS.Helpers;
using CMS.Localization;
using CMS.MediaLibrary;
using CMS.Modules;
using CMS.Scheduler;
using CMS.Search;
using CMS.SiteProvider;
using CMS.Synchronization;
using CMS.WebFarmSync;
using CMS.WinServiceEngine;

using Microsoft.Win32;

namespace SupportHelper
{
    /// <summary>
    /// DataRow that stores Metric metadata.
    /// </summary>
    public class Metric : DataRow
    {
        private const int CUTOFF = 500;

        #region Constructors

        public Metric(DataRowBuilder builder) : base(builder)
        {
        }

        #endregion Constructors

        #region Properties

        public int MetricId { get; set; }

        public string MetricDisplayName
        {
            get
            {
                return ResHelper.GetString(MetricCodeName);
            }
        }

        internal string MetricCodeName { get; set; }

        public Metric MetricParent { get; set; }

        public bool MetricHasChildren
        {
            get
            {
                // The Metrics object is meant to be a three-level structure, so either the parent is null or the grandparent is null
                return (MetricParent == null || MetricParent.MetricParent == null) ? true : false;
            }
        }

        public bool MetricSelected { get; set; }

        public string MetricAssemblyName { get; set; }

        public string MetricClassName { get; set; }

        public string MetricPath { get; internal set; }

        internal IEnumerable<KeyValuePair<string, string>> MetricData
        {
            get
            {
                if (MetricClassName != null && MetricAssemblyName != null)
                {
                    try
                    {
                        return (ClassHelper.GetClass(MetricAssemblyName, MetricClassName) as ICustomMetric).GetCustomMetricData();
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }

                return GetMetricData();
            }
        }

        internal System.Collections.Specialized.NameValueCollection MetricServerVariables { get; set; }

        #endregion Properties

        #region Methods

        private IEnumerable<KeyValuePair<string, string>> GetMetricData()
        {
            string stringData = null;
            IEnumerable<KeyValuePair<string, string>> tupleData = null;

            // Gather data for each row, return special message if data is null
            switch (MetricCodeName)
            {
                // Categories
                case MetricDataEnum.support_metrics:
                case MetricDataEnum.support_metrics_system:
                case MetricDataEnum.support_metrics_environment:
                case MetricDataEnum.support_metrics_counters:
                case MetricDataEnum.support_metrics_ecommerce:
                case MetricDataEnum.support_metrics_tasks:
                case MetricDataEnum.support_metrics_eventlog:
                    return null;

                #region System

                case MetricDataEnum.support_metrics_system_version:
                    stringData = CMSVersion.GetVersion(true, true, true, true);
                    break;

                case MetricDataEnum.support_metrics_system_appname:
                    stringData = SettingsHelper.AppSettings["CMSApplicationName"];
                    break;

                case MetricDataEnum.support_metrics_system_instancename:
                    stringData = SystemContext.InstanceName;
                    break;

                case MetricDataEnum.support_metrics_system_physicalpath:
                    stringData = SystemContext.WebApplicationPhysicalPath;
                    break;

                case MetricDataEnum.support_metrics_system_apppath:
                    stringData = SystemContext.ApplicationPath;
                    break;

                case MetricDataEnum.support_metrics_system_uiculture:
                    stringData = LocalizationContext.CurrentUICulture.CultureName;
                    break;

                case MetricDataEnum.support_metrics_system_installtype:
                    stringData = SystemContext.IsWebApplicationProject ? "Web App" : "Web site";
                    break;

                case MetricDataEnum.support_metrics_system_portaltemplatepage:
                    stringData = URLHelper.PortalTemplatePage;
                    break;

                case MetricDataEnum.support_metrics_system_timesinceapprestart:
                    stringData = (DateTime.Now - CMSApplication.ApplicationStart).ToString(@"dd\:hh\:mm\:ss");
                    break;

                case MetricDataEnum.support_metrics_system_discoveredassemblies:
                    tupleData = AssemblyDiscoveryHelper.GetAssemblies(true).Select((a, i) => GetKeyValuePair(i, a.FullName));
                    break;

                case MetricDataEnum.support_metrics_system_targetframework:
                    HttpRuntimeSection httpRuntime = ConfigurationManager.GetSection("system.web/httpRuntime") as HttpRuntimeSection;
                    stringData = httpRuntime.TargetFramework;
                    break;

                case MetricDataEnum.support_metrics_system_authmode:
                    AuthenticationSection Authentication = ConfigurationManager.GetSection("system.web/authentication") as AuthenticationSection;
                    stringData = Authentication != null ? Authentication.Mode.ToString() : null;
                    break;

                case MetricDataEnum.support_metrics_system_sessionmode:
                    SessionStateSection SessionState = ConfigurationManager.GetSection("system.web/sessionState") as SessionStateSection;
                    stringData = SessionState != null ? SessionState.Mode.ToString() : null;
                    break;

                case MetricDataEnum.support_metrics_system_debugmode:
                    CompilationSection Compilation = ConfigurationManager.GetSection("system.web/compilation") as CompilationSection;
                    stringData = Compilation != null ? Compilation.Debug.ToString() : null;
                    break;

                case MetricDataEnum.support_metrics_system_runallmanagedmodules:
                    var xmlDoc = new System.Xml.XmlDocument();
                    xmlDoc.Load(URLHelper.GetPhysicalPath("~/Web.config"));
                    var attribute = xmlDoc.SelectSingleNode("/configuration/system.webServer/modules").Attributes["runAllManagedModulesForAllRequests"];
                    if (attribute != null)
                    {
                        stringData = attribute.Value;
                    }

                    break;

                #endregion System

                #region Environment

                case MetricDataEnum.support_metrics_environment_trustlevel:
                    stringData = SystemContext.CurrentTrustLevel.ToString();
                    break;

                case MetricDataEnum.support_metrics_environment_iisversion:
                    stringData = MetricServerVariables["SERVER_SOFTWARE"];
                    break;

                case MetricDataEnum.support_metrics_environment_https:
                    stringData = MetricServerVariables["HTTPS"];
                    break;

                case MetricDataEnum.support_metrics_environment_windowsversion:
                    using (RegistryKey versionKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion"))
                    {
                        if (versionKey != null)
                        {
                            var productName = versionKey.GetValue("ProductName");
                            var currentBuild = versionKey.GetValue("CurrentBuild");
                            var releaseId = versionKey.GetValue("ReleaseId");

                            stringData = String.Format("{0}, build {1}, release {2}", productName.ToString(), currentBuild.ToString(), releaseId.ToString());
                        }
                    }
                    break;

                case MetricDataEnum.support_metrics_environment_netversion:
                    using (RegistryKey ndpKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full\"))
                    {
                        if (ndpKey != null && ndpKey.GetValue("Release") != null)
                        {
                            var releaseKey = (int)ndpKey.GetValue("Release");
                            if (releaseKey >= 461808) { stringData = "4.7.2 or later"; }
                            else
                            if (releaseKey >= 461308) { stringData = "4.7.1"; }
                            else
                            if (releaseKey >= 460798) { stringData = "4.7"; }
                            else
                            if (releaseKey >= 394802) { stringData = "4.6.2"; }
                            else
                            if (releaseKey >= 394254) { stringData = "4.6.1"; }
                            else
                            if (releaseKey >= 393295) { stringData = "4.6"; }
                            else
                            if (releaseKey >= 379893) { stringData = "4.5.2"; }
                            else
                            if (releaseKey >= 378675) { stringData = "4.5.1"; }
                            else
                            if (releaseKey >= 378389) { stringData = "4.5"; }
                        }
                    }
                    break;

                case MetricDataEnum.support_metrics_environment_sqlserverversion:
                    var dtm = new TableManager(null);
                    stringData = dtm.DatabaseServerVersion;
                    break;

                case MetricDataEnum.support_metrics_environment_azure:
                    var azureStats = new Dictionary<string, string>(4)
                    {
                        { "Is a Cloud Service", (SettingsHelper.AppSettings["CMSAzureProject"] == "true").ToString("false") },
                        { "Is file system on Azure", (SettingsHelper.AppSettings["CMSExternalStorageName"] == "azure").ToString("false") },
                        { "Azure storage account", SettingsHelper.AppSettings["CMSAzureAccountName"] ?? String.Empty },
                        { "Azure CDN endpoint", SettingsHelper.AppSettings["CMSAzureCDNEndpoint"] ?? String.Empty }
                    };

                    tupleData = azureStats.Select(s => GetKeyValuePair(s.Key, s.Value));
                    break;

                case MetricDataEnum.support_metrics_environment_amazon:
                    var amazonStats = new Dictionary<string, string>(3)
                        {
                            { "Is file system on Amazon", (SettingsHelper.AppSettings["CMSExternalStorageName"] == "amazon").ToString() },
                            { "Amazon bucket name", SettingsHelper.AppSettings["CMSAmazonBucketName"] ?? String.Empty },
                            { "Amazon public access", SettingsHelper.AppSettings["CMSAmazonPublicAccess"] ?? String.Empty },
                        };

                    tupleData = amazonStats.Select(s => GetKeyValuePair(s.Key, s.Value));
                    break;

                case MetricDataEnum.support_metrics_environment_services:
                    tupleData = ServiceManager.GetServices().Select(s => GetKeyValuePair(s.ServiceName, s.Status));
                    break;

                #endregion Environment

                #region Counters

                case MetricDataEnum.support_metrics_counters_webfarmservers:
                    stringData = CoreServices.WebFarm.GetEnabledServerNames().Count().ToString();
                    break;

                case MetricDataEnum.support_metrics_counters_stagingservers:
                    stringData = ServerInfoProvider.GetServers().Count().ToString();
                    break;

                case MetricDataEnum.support_metrics_counters_pagemostchildren:
                    CMS.DocumentEngine.TreeProvider tree = new CMS.DocumentEngine.TreeProvider();

                    var pageWithMostChildren = tree.SelectNodes().OnCurrentSite().Published()
                                                    .ToDictionary(n => n, n => n.Children.Count)
                                                    .Aggregate((l, r) => l.Value > r.Value ? l : r);

                    tupleData = new[] { GetKeyValuePair(URLHelper.GetAbsoluteUrl("~" + pageWithMostChildren.Key.NodeAliasPath), pageWithMostChildren.Value) };
                    break;

                case MetricDataEnum.support_metrics_counters_modules:
                    stringData = ResourceInfoProvider.GetCount().ToString();
                    break;

                case MetricDataEnum.support_metrics_counters_medialibraries:
                    stringData = MediaLibraryInfoProvider.GetCount(w => w.WhereNull("LibraryGroupID")).ToString();
                    break;

                case MetricDataEnum.support_metrics_counters_activities:
                    stringData = ActivityInfoProvider.GetCount().ToString();
                    break;

                case MetricDataEnum.support_metrics_counters_contacts:
                    stringData = ContactInfoProvider.GetCount().ToString();
                    break;

                case MetricDataEnum.support_metrics_counters_contactgroups:
                    stringData = ContactGroupInfoProvider.GetCount().ToString();
                    break;

                case MetricDataEnum.support_metrics_counters_omrules:
                    stringData = RuleInfoProvider.GetCount().ToString();
                    break;

                case MetricDataEnum.support_metrics_counters_products:
                    stringData = SKUInfoProvider.GetSKUs(SiteContext.CurrentSiteID).WhereNull("SKUOptionCategoryID").Count().ToString();
                    break;

                #endregion Counters

                #region Tasks

                case MetricDataEnum.support_metrics_tasks_webfarm:
                    stringData = WebFarmTaskInfoProvider.GetWebFarmTasks()
                           .WhereLessThan("TaskCreated", DateTime.Now.AddDays(-1))
                           .Count().ToString();
                    break;

                case MetricDataEnum.support_metrics_tasks_staging:
                    stringData = StagingTaskInfoProvider.GetTasks()
                           .WhereLessThan("TaskTime", DateTime.Now.AddDays(-1))
                           .Count().ToString();
                    break;

                case MetricDataEnum.support_metrics_tasks_integration:
                    stringData = IntegrationTaskInfoProvider.GetIntegrationTasks()
                           .WhereLessThan("TaskTime", DateTime.Now.AddDays(-1))
                           .Count().ToString();
                    break;

                case MetricDataEnum.support_metrics_tasks_scheduled:
                    stringData = TaskInfoProvider.GetTasks()
                            .WhereTrue("TaskDeleteAfterLastRun")
                           .WhereLessThan("TaskNextRunTime", DateTime.Now.AddDays(-1))
                           .Count().ToString();
                    break;

                case MetricDataEnum.support_metrics_tasks_search:
                    stringData = SearchTaskInfoProvider.GetSearchTasks()
                           .WhereLessThan("SearchTaskCreated", DateTime.Now.AddDays(-1))
                           .Count().ToString();
                    break;

                case MetricDataEnum.support_metrics_tasks_email:
                    stringData = EmailInfoProvider.GetEmailCount("EmailStatus = 1 AND EmailLastSendResult IS NOT NULL").ToString();
                    break;

                #endregion Tasks

                #region Event log

                case MetricDataEnum.support_metrics_eventlog_macroerrors:
                    var macroErrors = EventLogProvider.GetEvents()
                                    .WhereEquals("Source", "MacroResolver")
                                    .WhereGreaterThan("EventTime", DateTime.Now.Subtract(TimeSpan.FromDays(7)))
                                    .OrderByDescending("EventTime")
                                    .TopN(10);

                    tupleData = macroErrors.Select(e =>
                                    GetKeyValuePair(string.Format("{0} from {1} at {2} in {3}", e.EventCode, e.Source, e.EventTime, e.EventMachineName),
                                                                                        e.EventDescription.Length > CUTOFF ? e.EventDescription.Substring(0, CUTOFF) : e.EventDescription)
                                );
                    break;

                case MetricDataEnum.support_metrics_eventlog_stagingerrors:
                    var stagingErrors = EventLogProvider.GetEvents()
                                    .WhereEquals("Source", "staging")
                                    .WhereIn("EventType", new[] { "E", "W" })
                                    .WhereGreaterThan("EventTime", DateTime.Now.Subtract(TimeSpan.FromDays(7)))
                                    .OrderByDescending("EventTime")
                                    .TopN(10);

                    tupleData = stagingErrors.Select(e =>
                                    GetKeyValuePair(string.Format("{0} from {1} at {2} in {3}", e.EventCode, e.Source, e.EventTime, e.EventMachineName),
                                                                                        e.EventDescription.Length > CUTOFF ? e.EventDescription.Substring(0, CUTOFF) : e.EventDescription)
                                );
                    break;

                case MetricDataEnum.support_metrics_eventlog_searcherrors:
                    var searchErrors = EventLogProvider.GetEvents()
                                    .WhereEquals("Source", "search")
                                    .WhereIn("EventType", new[] { "E", "W" })
                                    .WhereGreaterThan("EventTime", DateTime.Now.Subtract(TimeSpan.FromDays(7)))
                                    .OrderByDescending("EventTime")
                                    .TopN(10);

                    tupleData = searchErrors.Select(e =>
                                    GetKeyValuePair(string.Format("{0} from {1} at {2} in {3}", e.EventCode, e.Source, e.EventTime, e.EventMachineName),
                                                                                        e.EventDescription.Length > CUTOFF ? e.EventDescription.Substring(0, CUTOFF) : e.EventDescription)
                                );
                    break;

                case MetricDataEnum.support_metrics_eventlog_contenterrors:
                    var contentErrors = EventLogProvider.GetEvents()
                                    .WhereEquals("Source", "content")
                                    .WhereIn("EventType", new[] { "E", "W" })
                                    .WhereGreaterThan("EventTime", DateTime.Now.Subtract(TimeSpan.FromDays(7)))
                                    .OrderByDescending("EventTime")
                                    .TopN(10);

                    tupleData = contentErrors.Select(e =>
                                    GetKeyValuePair(string.Format("{0} from {1} at {2} in {3}", e.EventCode, e.Source, e.EventTime, e.EventMachineName),
                                                                                        e.EventDescription.Length > CUTOFF ? e.EventDescription.Substring(0, CUTOFF) : e.EventDescription)
                                );
                    break;

                case MetricDataEnum.support_metrics_eventlog_exceptions:
                    var exceptions = EventLogProvider.GetEvents()
                                    .WhereEquals("EventCode", "exception")
                                    .WhereGreaterThan("EventTime", DateTime.Now.Subtract(TimeSpan.FromDays(7)))
                                    .OrderByDescending("EventTime")
                                    .TopN(10);

                    tupleData = exceptions.Select(e =>
                                    GetKeyValuePair(string.Format("{0} from {1} at {2} in {3}", e.EventCode, e.Source, e.EventTime, e.EventMachineName),
                                                                                        e.EventDescription.Length > CUTOFF ? e.EventDescription.Substring(0, CUTOFF) : e.EventDescription)
                                );
                    break;

                case MetricDataEnum.support_metrics_eventlog_upgrade:

                    EventLogInfo upgrade = EventLogProvider.GetEvents().WhereLike("Source", "upgrade%").FirstObject;

                    if (upgrade != null && !string.IsNullOrEmpty(upgrade.Source.Split(' ')[2]))
                    {
                        var parameters = new QueryDataParameters
                        {
                            { "@versionnumber", upgrade.Source.Split(' ')[2] }
                        };

                        var events = ConnectionHelper.ExecuteQuery("SupportHelper.CustomMetric.checkupgrade", parameters);

                        if (events.Tables[0] != null)
                        {
                            tupleData = (from DataRow row in events.Tables[0].Rows select row)
                                        .Select(r => new EventLogInfo(r)).Select(e =>
                                            GetKeyValuePair(string.Format("{0} from {1} at {2} in {3}", e.EventCode, e.Source, e.EventTime, e.EventMachineName),
                                                                                                e.EventDescription.Length > CUTOFF ? e.EventDescription.Substring(0, CUTOFF) : e.EventDescription)
                                    );
                        }
                    }
                    break;

                    #endregion Event log
            }

            if (tupleData != null && tupleData.Count() > 0)
            {
                return tupleData;
            }

            if (stringData != null)
            {
                return new[] { GetKeyValuePair(0, stringData) };
            }

            return new[] { GetKeyValuePair(0, ResHelper.GetStringFormat("support.metrics.invalid", MetricDisplayName, MetricCodeName)) };
        }

        private KeyValuePair<string, string> GetKeyValuePair(object key, object value)
        {
            return new KeyValuePair<string, string>(key.ToString(), value.ToString());
        }

        public override string ToString()
        {
            return MetricCodeName;
        }

        #endregion Methods
    }
}