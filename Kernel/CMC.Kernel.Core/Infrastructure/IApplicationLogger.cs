using CMC.Kernel.Core.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CMC.Kernel.Core.Infrastructure
{
    public interface IApplicationLogger
    {
        Task LogError(Exception ex, string service, object request = null, string IDNumber = null, bool isAPI = false);
        Task LogWarning(string message, object details = null, string service = null, bool isAPI = false);
        Task LogInformation(string message, object details = null, string service = null, object request = null, object response = null, HttpStatusCode statusCode = HttpStatusCode.None, LogServerity resultType = LogServerity.Info, string IDNumnber = null, bool isAPI = false);
        Task LogDebug(string message, object details = null, string service = null, object request = null, object response = null, bool isAPI = false);
    }
}
