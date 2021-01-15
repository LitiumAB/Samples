using Litium.SampleApps.Erp.Loggers;
using System;

namespace Litium.SampleApps.ErpDemo.NLogs
{
    public class NLogger<TType> : ILogger<TType> where TType : class
    {
        private readonly NLog.Logger _nlogger;
        public NLogger()
        {
            NLog.Config.ConfigurationItemFactory.Default.JsonConverter = new JsonNetSerializer();
            _nlogger = NLog.LogManager.GetLogger(typeof(TType).FullName);
        }

        public void Log(LogEntry entry)
        {
            switch (entry.Severity)
            {
                case LoggingEventType.Trace:
                    _nlogger.Trace(entry.Message);
                    break;
                case LoggingEventType.Debug:
                    _nlogger.Debug(entry.Message);
                    break;
                case LoggingEventType.Information:
                    _nlogger.Info(entry.Message);
                    break;
                case LoggingEventType.Warning:
                    _nlogger.Warn(entry.Message);
                    break;
                case LoggingEventType.Error:
                    _nlogger.Error(entry.Exception, entry.Message);
                    break;
                case LoggingEventType.Fatal:
                    _nlogger.Fatal(entry.Exception, entry.Message);
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"LogEntry.Severity out of range. (Severity='{entry.Severity}')");
            }
        }
    }
}