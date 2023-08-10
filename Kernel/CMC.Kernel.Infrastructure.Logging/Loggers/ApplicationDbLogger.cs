using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using CMC.Kernel.Core.Constants;
using CMC.Kernel.Core.Enums;
using CMC.Kernel.Core.Helpers;
using CMC.Kernel.Core.Infrastructure;
using CMC.Kernel.Domain.Logs;
using CMC.Kernel.Infrastructure.Persistence.Repositories;
using System;
using System.Threading.Tasks;
using UAParser;

namespace CMC.Kernel.Infrastructure.Logging.Loggers
{
    /// <summary>
    /// Application Db Logger
    /// </summary>
    public class ApplicationDbLogger : IApplicationLogger
    {
        #region Fields
        private readonly IHttpContextAccessor _httpAccessor;
        private readonly ILogger<ApplicationDbLogger> _logger;
        private IApplicationLogRepository _applicationLogRepository { set; get; }
        #endregion

        #region Ctor
        public ApplicationDbLogger(IApplicationLogRepository applicationLogRepository, IHttpContextAccessor httpAccessor, ILogger<ApplicationDbLogger> logger)
        {
            _applicationLogRepository = applicationLogRepository;
            _httpAccessor = httpAccessor;
            _logger = logger;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Log Debug
        /// </summary>
        /// <param name="message"></param>
        /// <param name="details"></param>
        /// <param name="service"></param>
        /// <param name="request"></param>
        /// <param name="response"></param>
        /// <returns></returns>
        public async Task LogDebug(string message, object details = null, string service = null, object request = null, object response = null, bool isAPI = false)
        {

            string strDetails = details != null ? JsonConvert.SerializeObject(details) : null;
            string strRequest = request != null ? JsonConvert.SerializeObject(request) : null;
            string strResponse = response != null ? JsonConvert.SerializeObject(response) : null;

            ApplicationLog applicationLog = PrepareLog();
            applicationLog.LogSeverityId = (int)LogServerity.Debug;
            applicationLog.Service = service;
            applicationLog.Message = message;
            applicationLog.Description = strDetails;
            applicationLog.MessageRequest = strRequest;
            applicationLog.MessageResponse = strResponse;

            await _applicationLogRepository.Insert(applicationLog);
        }

        /// <summary>
        /// Log Error or Excpetion
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="service"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task LogError(Exception ex, string service, object request = null, string IDNumber = null, bool isAPI = false)
        {
            try
            {
                string error = JsonConvert.SerializeObject(ex);
                string strRequest = request != null ? JsonConvert.SerializeObject(request) : null;

                ApplicationLog applicationLog = PrepareLog(isAPI);
                if (!string.IsNullOrEmpty(IDNumber))
                    applicationLog.IDUserName = IDNumber;
                applicationLog.LogSeverityId = (int)LogServerity.Error;
                applicationLog.Service = service;
                applicationLog.Message = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                applicationLog.Description = error;
                applicationLog.MessageRequest = strRequest;
                applicationLog.ResultType = LogServerity.Error.ToString();

                await _applicationLogRepository.Insert(applicationLog);
            }
            catch (Exception exc)
            {
                _logger.LogError(ex, "LogError", service, request, IDNumber, isAPI);
                ApplicationLog applicationLog = PrepareLog(isAPI);
                if (!string.IsNullOrEmpty(IDNumber))
                    applicationLog.IDUserName = IDNumber;
                applicationLog.LogSeverityId = (int)LogServerity.Error;
                applicationLog.Service = service;
                applicationLog.Message = exc.Message;
                applicationLog.Description = exc.InnerException != null ? exc.InnerException.Message : exc.Message;
                applicationLog.ResultType = LogServerity.Error.ToString();
                await _applicationLogRepository.Insert(applicationLog);
            }
        }

        /// <summary>
        /// Log Information
        /// </summary>
        /// <param name="message"></param>
        /// <param name="details"></param>
        /// <param name="service"></param>
        /// <param name="request"></param>
        /// <param name="response"></param>
        /// <param name="statusCode"></param>
        /// <param name="resultType"></param>
        /// <returns></returns>
        public async Task LogInformation(string message, object details = null, string service = null, object request = null, object response = null, HttpStatusCode statusCode = HttpStatusCode.None, LogServerity resultType = LogServerity.Info, string IDNumnber = null, bool isAPI = false)
        {
            string strDetails = details != null ? JsonConvert.SerializeObject(details) : null;
            string strRequest = request != null ? JsonConvert.SerializeObject(request) : null;
            string strResponse = response != null ? JsonConvert.SerializeObject(response) : null;

            ApplicationLog applicationLog = PrepareLog(isAPI);
            if (!string.IsNullOrEmpty(IDNumnber))
                applicationLog.IDUserName = IDNumnber;
            applicationLog.LogSeverityId = (int)LogServerity.Info;
            applicationLog.Service = service;
            applicationLog.Message = message;
            applicationLog.Description = strDetails;
            applicationLog.MessageRequest = strRequest;
            applicationLog.MessageResponse = strResponse;
            applicationLog.ResultCode = ((int)statusCode).ToString();
            applicationLog.ResultType = resultType.ToString();


            await _applicationLogRepository.Insert(applicationLog);
        }

        /// <summary>
        /// Log Warning
        /// </summary>
        /// <param name="message"></param>
        /// <param name="details"></param>
        /// <param name="service"></param>
        /// <returns></returns>
        public async Task LogWarning(string message, object details = null, string service = null, bool isAPI = false)
        {
            string strDetails = details != null ? JsonConvert.SerializeObject(details) : null;

            ApplicationLog applicationLog = PrepareLog(isAPI);
            applicationLog.LogSeverityId = (int)LogServerity.Info;
            applicationLog.Service = service;
            applicationLog.Message = message;
            applicationLog.Description = strDetails;
            applicationLog.ResultType = LogServerity.Warning.ToString();

            await _applicationLogRepository.Insert(applicationLog);
        }

        /// <summary>
        /// Prepare public Log info
        /// </summary>
        /// <returns></returns>
        private ApplicationLog PrepareLog(bool isAPI = false)
        {
            if (!isAPI)
            {
                if (_httpAccessor.HttpContext == null)
                {
                    return new ApplicationLog()
                    {
                        ApplicationName = SystemSettings.ApplicationName,
                        LoggedAt = DateTime.Now
                    };
                }
                string idUsername = !string.IsNullOrEmpty(_httpAccessor.HttpContext.Session.GetString("IDNumber")) ? Security.Decrypt(_httpAccessor.HttpContext.Session.GetString("IDNumber")) :
                    !string.IsNullOrEmpty(_httpAccessor.HttpContext.Session.GetString("MobileNumber")) ? Security.Decrypt(_httpAccessor.HttpContext.Session.GetString("MobileNumber")) : "";
                var userAgent = _httpAccessor.HttpContext.Request.Headers["User-Agent"];
                string uaString = Convert.ToString(userAgent[0]);
                var uaParser = Parser.GetDefault();
                ClientInfo c = uaParser.Parse(uaString);
                string browsr = c.UserAgent.Family;
                string device = c.Device.Family;
                string ipAddress = _httpAccessor.HttpContext.Connection.RemoteIpAddress.ToString();


                return new ApplicationLog()
                {
                    ApplicationName = SystemSettings.ApplicationName,
                    LoggedAt = DateTime.Now,
                    IPAddress = ipAddress,
                    IDUserName = idUsername,
                    BrowserName = browsr,
                    Device = device,
                    UserAgent = uaString,
                };
            }
            else
            {
                return new ApplicationLog()
                {
                    ApplicationName = SystemSettings.ApplicationName,
                    LoggedAt = DateTime.Now
                };
            }
        }
        #endregion
    }
}
