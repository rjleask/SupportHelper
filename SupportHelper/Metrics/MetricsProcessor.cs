using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Mail;
using CMS.Base;

using Newtonsoft.Json;

namespace SupportHelper
{
    public partial class MetricsProcessor
    {
        public enum Category
        {
            Attachments,
            Form,
            System,
            Environment,
            Counters,
            Tasks,
            EventLog
        }

        private readonly List<Attachment> _attachments;
        private readonly Dictionary<string, Tuple<string, IEnumerable<KeyValuePair<string, string>>>> _form;
        private readonly Dictionary<string, Tuple<string, IEnumerable<KeyValuePair<string, string>>>> _system;
        private readonly Dictionary<string, Tuple<string, IEnumerable<KeyValuePair<string, string>>>> _environment;
        private readonly Dictionary<string, Tuple<string, IEnumerable<KeyValuePair<string, string>>>> _counters;
        private readonly Dictionary<string, Tuple<string, IEnumerable<KeyValuePair<string, string>>>> _tasks;
        private readonly Dictionary<string, Tuple<string, IEnumerable<KeyValuePair<string, string>>>> _eventLog;

        #region Properties

        public string JSON
        {
            get
            {
                return GetJson();
            }
        }

        public List<Attachment> Attachments
        {
            get
            {
                return _attachments;
            }
        }

        public Dictionary<string, KeyValuePair<string, string>> FormFields
        {
            get
            {
                return Form.ToDictionary(f => f.Key, f => new KeyValuePair<string, string>(f.Value.Item1, f.Value.Item2.FirstOrDefault().Value));
            }
        }

        private Dictionary<string, Tuple<string, IEnumerable<KeyValuePair<string, string>>>> Form
        {
            get
            {
                return _form;
            }
        }

        private Dictionary<string, Tuple<string, IEnumerable<KeyValuePair<string, string>>>> System
        {
            get
            {
                return _system;
            }
        }

        private Dictionary<string, Tuple<string, IEnumerable<KeyValuePair<string, string>>>> Environment
        {
            get
            {
                return _environment;
            }
        }

        private Dictionary<string, Tuple<string, IEnumerable<KeyValuePair<string, string>>>> Counters
        {
            get
            {
                return _counters;
            }
        }

        private Dictionary<string, Tuple<string, IEnumerable<KeyValuePair<string, string>>>> Tasks
        {
            get
            {
                return _tasks;
            }
        }

        private Dictionary<string, Tuple<string, IEnumerable<KeyValuePair<string, string>>>> EventLog
        {
            get
            {
                return _eventLog;
            }
        }

        #endregion Properties

        #region Constructors

        /// <summary>
        /// Constructor from POST request data.
        /// </summary>
        public MetricsProcessor(IEnumerable<HttpContent> submission)
        {
            // Set up dictionary objects for the JSON output
            _attachments = new List<Attachment>();

            // The structure is: metric codename, metric label, key: value pair(s)
            _form = new Dictionary<string, Tuple<string, IEnumerable<KeyValuePair<string, string>>>>();
            _system = new Dictionary<string, Tuple<string, IEnumerable<KeyValuePair<string, string>>>>();
            _environment = new Dictionary<string, Tuple<string, IEnumerable<KeyValuePair<string, string>>>>();
            _counters = new Dictionary<string, Tuple<string, IEnumerable<KeyValuePair<string, string>>>>();
            _tasks = new Dictionary<string, Tuple<string, IEnumerable<KeyValuePair<string, string>>>>();
            _eventLog = new Dictionary<string, Tuple<string, IEnumerable<KeyValuePair<string, string>>>>();

            foreach (HttpContent item in submission)
            {
                if (item.Headers != null)
                {
                    ProcessSubmissionItem(item);
                }
            }
        }

        #endregion Constructors

        #region Public methods

        /// <summary>
        /// Get a single metric's data. Use <see cref="string"/> for a single form field, <see cref="IEnumerable{string}"/> for a list of single fields, or <see cref="Dictionary{string, string}"/> for a list of key value fields.
        /// </summary>
        /// <typeparam name="T">Metric type.</typeparam>
        /// <param name="category">Metric category.</param>
        /// <param name="metricCodeName">Code name of the metric.</param>
        /// <returns></returns>
        public T GetMetric<T>(Category category, string metricCodeName)
        {
            var tuple = GetMetricInternal(category, metricCodeName);

            if (tuple != null)
            {
                var metricData = tuple.Item2;

                // Single form field
                if (typeof(T) == typeof(string))
                {
                    return (T)(metricData.FirstOrDefault().Value as object);
                }

                // List of single fields
                if (typeof(T) == typeof(IEnumerable<string>))
                {
                    return (T)(metricData.Select(kv => kv.Value) as object);
                }

                // List of key value fields
                if (typeof(T) == typeof(Dictionary<string, string>))
                {
                    return (T)(metricData.ToDictionary(kv => kv.Key, kv => kv.Value) as object);
                }
            }

            return default(T);
        }

        #endregion Public methods

        #region Private methods

        private Tuple<string, IEnumerable<KeyValuePair<string, string>>> GetMetricInternal(Category category, string metric)
        {
            Tuple<string, IEnumerable<KeyValuePair<string, string>>> tuple = null;

            switch (category)
            {
                case Category.Form:
                    Form.TryGetValue(metric, out tuple);
                    break;

                case Category.System:
                    System.TryGetValue(metric, out tuple);
                    break;

                case Category.Environment:
                    Environment.TryGetValue(metric, out tuple);
                    break;

                case Category.Counters:
                    Counters.TryGetValue(metric, out tuple);
                    break;

                case Category.Tasks:
                    Tasks.TryGetValue(metric, out tuple);
                    break;

                case Category.EventLog:
                    EventLog.TryGetValue(metric, out tuple);
                    break;

                default:
                    throw new NotSupportedException("If you are trying to get an attachment, use property Attachments instead.");
            }

            return tuple;
        }

        private void ProcessSubmissionItem(HttpContent item)
        {
            switch (item.Headers.ContentDisposition.DispositionType)
            {
                case "form-urlencoded":
                    var category = item.Headers.GetValues("category").First();
                    var field = item.Headers.GetValues("codeName").First();
                    var label = item.Headers.ContentDisposition.Name.Trim('\"');
                    var value = item.ReadAsFormDataAsync().Result;

                    AddToCategory(category, field, label, value);
                    break;

                case "attachment":
                    var fileName = item.Headers.ContentDisposition.FileName.Trim('\"');
                    var bytes = item.ReadAsByteArrayAsync().Result;

                    AddToAttachments(fileName, bytes);
                    break;
            }
        }

        private void AddToAttachments(string fileName, byte[] bytes)
        {
            Attachments.Add(new Attachment(new MemoryStream(bytes), fileName));
        }

        private void AddToCategory(string category, string field, string label, NameValueCollection value)
        {
            var labelValue = Tuple.Create(label, value.Cast<string>()
                                                      .Select(key => new KeyValuePair<string, string>(key, value[key]))
                                        );

            switch (category)
            {
                case "form":
                    Form.Add(field, labelValue);
                    break;

                case "support.metrics.system":
                    System.Add(field, labelValue);
                    break;

                case "support.metrics.environment":
                    Environment.Add(field, labelValue);
                    break;

                case "support.metrics.counters":
                    Counters.Add(field, labelValue);
                    break;

                case "support.metrics.tasks":
                    Tasks.Add(field, labelValue);
                    break;

                case "support.metrics.eventlog":
                    EventLog.Add(field, labelValue);
                    break;
            }
        }

        private string GetJson()
        {
            var metrics = new
            {
                system = System.ToDictionary(t => t.Value.Item1, t => GetDynamicValue(t.Value.Item2)),
                environment = Environment.ToDictionary(t => t.Value.Item1, t => GetDynamicValue(t.Value.Item2)),
                counters = Counters.ToDictionary(t => t.Value.Item1, t => GetDynamicValue(t.Value.Item2)),
                tasks = Tasks.ToDictionary(t => t.Value.Item1, t => GetDynamicValue(t.Value.Item2)),
                eventLog = EventLog.ToDictionary(t => t.Value.Item1, t => GetDynamicValue(t.Value.Item2)),
            };

            return JsonConvert.SerializeObject(metrics, Formatting.Indented);
        }

        private object GetDynamicValue(IEnumerable<KeyValuePair<string, string>> keyValuePairs)
        {
            var totalCount = keyValuePairs.Count();

            // Single form field
            if (totalCount == 1 && keyValuePairs.FirstOrDefault().Key.ToInteger(-1) == 0)
            {
                return keyValuePairs.FirstOrDefault().Value;
            }

            // List of single fields
            if (totalCount == keyValuePairs.Where(kv => kv.Key.ToInteger(-1) != -1).Count())
            {
                return keyValuePairs.Select(kv => kv.Value);
            }

            // List of key value fields
            return keyValuePairs.ToDictionary(kv => kv.Key, kv => kv.Value);
        }

        #endregion Private methods
    }
}