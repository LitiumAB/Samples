using System;

namespace Litium.SampleApps.Erp.Loggers
{
    public static class LoggerExtensions
    {
        public static void Trace<T>(this ILogger<T> logger, string message) where T : class
        {
            logger.Log(new LogEntry(LoggingEventType.Trace, message));
        }

        public static void Debug<T>(this ILogger<T> logger, string message) where T : class
        {
            logger.Log(new LogEntry(LoggingEventType.Debug, message));
        }

        public static void Info<T>(this ILogger<T> logger, string message) where T : class
        {
            logger.Log(new LogEntry(LoggingEventType.Information, message));
        }

        public static void Warn<T>(this ILogger<T> logger, string message) where T : class
        {
            logger.Log(new LogEntry(LoggingEventType.Warning, message));
        }

        public static void Error<T>(this ILogger<T> logger, string message, Exception exception) where T : class
        {
            logger.Log(new LogEntry(LoggingEventType.Error, message, exception));
        }

        public static void Fatal<T>(this ILogger<T> logger, string message, Exception exception) where T : class
        {
            logger.Log(new LogEntry(LoggingEventType.Fatal, message, exception));
        }
    }
}
