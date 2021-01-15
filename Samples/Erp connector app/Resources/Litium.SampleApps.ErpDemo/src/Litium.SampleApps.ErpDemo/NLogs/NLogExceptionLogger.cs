using Litium.Connect.Erp;
using Litium.SampleApps.Erp.Loggers;
using System.Threading.Tasks;
using System.Web.Http.ExceptionHandling;

namespace Litium.SampleApps.ErpDemo.NLogs
{
    public class NLogExceptionLogger : ExceptionLogger
    {
        private readonly ILogger<ExceptionLogger> _logger;

        public NLogExceptionLogger(ILogger<ExceptionLogger> logger)
        {
            _logger = logger;
        }
        public async override Task LogAsync(ExceptionLoggerContext context, System.Threading.CancellationToken cancellationToken)
        {
            if(context.Exception is ApiException)
            {
                _logger.Error("An unhandled exception occurred.", context.Exception);
            }
            
            await base.LogAsync(context, cancellationToken);
        }

        public override void Log(ExceptionLoggerContext context)
        {
            if (context.Exception is ApiException)
            {
                _logger.Error("An unhandled exception occurred.", context.Exception);
            }
            base.Log(context);
        }
    }
}