using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using CMC.Kernel.Core.Enums;
using CMC.Kernel.Core.Persistence;
using CMC.Kernel.Domain.Logs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CMC.Kernel.Infrastructure.Logging.Loggers
{
    /// <summary>
    /// HttpLogger
    /// </summary>
    public class HttpLogger : IAsyncActionFilter
    {
        /// <summary>
        /// define http Log Repository
        /// </summary>
        private IHttpLogRepository _httpLogRepository { set; get; }
        /// <summary>
        /// Http Logger constructor 
        /// </summary>
        /// <param name="httpLogRepository"></param>
        public HttpLogger(IHttpLogRepository httpLogRepository)
        {
            _httpLogRepository = httpLogRepository;
        }
        /// <summary>
        /// On Action Execution Async Logger
        /// </summary>
        /// <param name="context"></param>
        /// <param name="next"></param>
        /// <returns></returns>
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var log = new HttpLog();
            HttpContext httpContext = context.HttpContext;

            //Main details of request
            log.RequestedOn = DateTime.Now;
            var ip = httpContext.Connection.RemoteIpAddress;
            log.IPAddress = ip == null ? null : ip.ToString();
            if (log.IPAddress == "::1")
                log.IPAddress = "127.0.0.1";
            log.RequestType = context.HttpContext.Request.Method.ToUpper();
            log.Url = string.Concat(context.HttpContext.Request.Host, httpContext.Request.Path);
            log.ServiceName = context.RouteData.Values["Controller"].ToString();
            log.ActionName = context.RouteData.Values["Action"].ToString();
            // request parameters / objects
            log.RequestQueries = JsonConvert.SerializeObject(FormatQueries(httpContext.Request.QueryString.ToString()));
            log.RequestHeader = JsonConvert.SerializeObject(FormatHeaders(httpContext.Request.Headers));
            log.RequestObject = JsonConvert.SerializeObject(await ReadBodyFromRequest(httpContext.Request));
            if (log.RequestObject == "\"\"")
            {
                var actionArgumentKey = context.ActionArguments.Keys.FirstOrDefault();
                if (actionArgumentKey == null)
                    log.RequestObject = "{}";
                else
                {
                    var actionArgument = context.ActionArguments[actionArgumentKey];
                    if (actionArgument != null)
                        log.RequestObject = JsonConvert.SerializeObject(actionArgument);
                }
            }
            var actionExectuedContext = await next();
            if (actionExectuedContext.Result != null)
            {
                var response = actionExectuedContext.HttpContext.Response;
                log.ResponseContentType = response.ContentType;
                log.ResponseStatus = response.StatusCode.ToString();
                log.ResponseObject = JsonConvert.SerializeObject(actionExectuedContext.Result);
            }
            else if (actionExectuedContext.Exception != null)
            {
                LogError(log, actionExectuedContext.Exception);
            }

            // Resposne
            log.RespondedOn = DateTime.Now;
            TimeSpan ts = log.RespondedOn.Value - log.RequestedOn;
            log.ActionPeriodTime = ts.TotalMilliseconds;

            await _httpLogRepository.AddLogAsync(log);
        }

        private void LogError(HttpLog log, Exception exception)
        {
            log.IsException = true;
            log.ResponseStatus = ((int)HttpStatusCode.BadRequest).ToString();
            log.ExceptionMessage = exception.Message;
            log.ExceptionStackTrace = exception.StackTrace;
            log.ExceptionDetails = JsonConvert.SerializeObject(exception);
        }

        private Dictionary<string, string> FormatHeaders(IHeaderDictionary headers)
        {
            Dictionary<string, string> pairs = new Dictionary<string, string>();
            foreach (var header in headers)
            {
                pairs.Add(header.Key, header.Value);
            }
            return pairs;
        }

        private List<KeyValuePair<string, string>> FormatQueries(string queryString)
        {
            List<KeyValuePair<string, string>> pairs =
                 new List<KeyValuePair<string, string>>();
            string key, value;
            foreach (var query in queryString.TrimStart('?').Split("&"))
            {
                var items = query.Split("=");
                key = items.Count() >= 1 ? items[0] : string.Empty;
                value = items.Count() >= 2 ? items[1] : string.Empty;
                if (!String.IsNullOrEmpty(key))
                {
                    pairs.Add(new KeyValuePair<string, string>(key, value));
                }
            }
            return pairs;
        }

        private async Task<string> ReadBodyFromRequest(HttpRequest request)
        {
            // Ensure the request's body can be read multiple times 
            // (for the next middlewares in the pipeline).
            request.EnableBuffering();
            using var streamReader = new StreamReader(request.Body, leaveOpen: true);
            var requestBody = await streamReader.ReadToEndAsync();
            // Reset the request's body stream position for 
            // next middleware in the pipeline.
            request.Body.Position = 0;
            return requestBody;
        }


    }
}
