using System;

namespace Litium.SampleApps.Erp.Loggers
{
    /// <summary>
    /// Represents immutable log entry
    /// </summary>
    public class LogEntry
    {
        public readonly LoggingEventType Severity;
        public readonly string Message;
        public readonly Exception Exception;

        /// <summary>
        /// Initializes a new instance of the <see cref="LogEntry"> class
        /// </summary>
        /// <param name="severity">the log severiry</param>
        /// <param name="message">the log message</param>
        /// <param name="exception">the exception</param>
        public LogEntry(LoggingEventType severity, string message, Exception exception = null)
        {   
            Severity = severity;
            Message = message;
            Exception = exception;
        }
    }
}
