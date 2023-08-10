using CMC.Kernel.Core.Persistence;
using CMC.Kernel.Domain.Logs;
using System.Threading.Tasks;

namespace CMC.Kernel.Infrastructure.Persistence.Repositories
{
    /// <summary>
    /// 
    /// </summary>
    public class HttpLogRepository : Repository<HttpLog>, IHttpLogRepository
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="unitOfWork"></param>
        public HttpLogRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="requestLogging"></param>
        /// <returns></returns>
        public async Task<int> AddLogAsync(HttpLog requestLogging)
        {
            await InsertAsync(requestLogging);
            return await UnitOfWork.SaveChangesAsync();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="requestLogging"></param>
        /// <returns></returns>
        public async Task<int> UpdateLogAsync(HttpLog requestLogging)
        {
            Update(requestLogging);
            return await UnitOfWork.SaveChangesAsync();
        }
    }
}
