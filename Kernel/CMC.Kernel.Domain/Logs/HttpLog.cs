using CMC.Kernel.Domain.Entities.Base;
using System;

namespace CMC.Kernel.Domain.Logs
{
    public class HttpLog : Entity<long>
    {
        public DateTime RequestedOn { get; set; }
        public string ServiceName { get; set; }
        public string ActionName { get; set; }
        public string Url { get; set; }
        public string IPAddress { get; set; }
        public string RequestType { get; set; }
        public string RequestHeader { get; set; }
        public string RequestQueries { get; set; }
        public string RequestObject { get; set; }
        public string ResponseObject { get; set; }
        public string ResponseStatus { get; set; }
        public string ResponseHeader { get; set; }
        public string ResponseContentType { get; set; }
        public DateTime? RespondedOn { get; set; }
        public double? ActionPeriodTime { get; set; }
        public bool? IsException { get; set; }
        public string ExceptionMessage { get; set; }
        public string ExceptionStackTrace { get; set; }
        public string ExceptionDetails { get; set; }
    }
}
