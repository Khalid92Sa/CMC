
using Serilog;

namespace CMC.Kernel.Core.Helpers
{
    public static class SerlilogExtensions
    {
        public static ILogger LogInformation(this ILogger logger, string message, string requestTraceId = "")
        {
            string value = string.Empty;
            if (!string.IsNullOrEmpty(requestTraceId))
            {
                value = requestTraceId;
            }
            else
            {
                try
                {
                    value = System.Diagnostics.Activity.Current.TraceId.ToString();
                }
                catch { }
            }
            logger.ForContext("reqtraceId", value).Information(message);
            return logger;

        }

        public static ILogger LogException(this ILogger logger, string message, string requestTraceId = "")
        {
            string value = string.Empty;
            if (!string.IsNullOrEmpty(requestTraceId))
            {
                value = requestTraceId;
            }
            else
            {
                try
                {
                    value = System.Diagnostics.Activity.Current.TraceId.ToString();
                }
                catch { }
            }
            logger.ForContext("reqtraceId", value).Information(message);
            return logger;
        }

        public static ILogger LogException(this ILogger logger, System.Exception exception, string requestTraceId = "")
        {
            string value = string.Empty;
            if (!string.IsNullOrEmpty(requestTraceId))
            {
                value = requestTraceId;
            }
            else
            {
                try
                {
                    value = System.Diagnostics.Activity.Current.TraceId.ToString();
                }
                catch { }
            }
            logger.ForContext("reqtraceId", value).Error(exception, "");
            return logger;
        }
    }
}
