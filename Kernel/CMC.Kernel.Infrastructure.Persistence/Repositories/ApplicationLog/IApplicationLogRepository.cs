using CMC.Kernel.Core.Persistence;
using CMC.Kernel.Domain.Logs;
using System.Threading.Tasks;

namespace CMC.Kernel.Infrastructure.Persistence.Repositories
{
    /// <summary>
    /// Interfcae for Application Log Repository
    /// </summary>
    public interface IApplicationLogRepository : IRepository<ApplicationLog>
    {
        /// <summary>
        /// Add Log
        /// </summary>
        /// <param name="applicationLog"></param>
        /// <returns></returns>
        public Task Insert(ApplicationLog applicationLog);
    }
}
