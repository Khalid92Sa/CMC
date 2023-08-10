using CMC.Kernel.Domain.Logs;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CMC.Kernel.Core.Persistence
{
    public interface IHttpLogRepository
    {
        Task<int> AddLogAsync(HttpLog httpLog);
        Task<int> UpdateLogAsync(HttpLog httpLog);
    }
}
