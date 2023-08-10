using CMC.Kernel.Domain.Entities.Base;
using System;

namespace CMC.Kernel.Domain.Logs
{
    public class ApplicationLog : Entity<long>
    {
        public DateTime LoggedAt { get; set; }
        public int LogSeverityId { get; set; }
        public string IDUserName { get; set; }
        public string Service { get; set; }
        public string Message { get; set; }
        public string Description { get; set; }
        public string ResultCode { get; set; }
        public string ResultType { get; set; }
        public string MessageRequest { get; set; }
        public string MessageResponse { get; set; }
        public string IPAddress { get; set; }
        public string Device { get; set; }
        public string UserAgent { get; set; }
        public string BrowserName { get; set; }
        public string ApplicationName { get; set; }
    }
}

