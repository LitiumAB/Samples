namespace Litium.SampleApps.Erp.Loggers
{
    /// <summary>
    /// Custom interface for logging messages
    /// </summary>
    public interface ILogger<TType> where TType : class
    {
        /// <summary>
        /// Log a specified entry
        /// </summary>
        /// <param name="entry">the log entry</param>
        void Log(LogEntry entry);
    }
}
