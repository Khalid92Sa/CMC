using CMC.Kernel.Core.Persistence;
using CMC.Kernel.Domain.Logs;
using System;
using System.Threading.Tasks;

namespace CMC.Kernel.Infrastructure.Persistence.Repositories
{
    /// <summary>
    /// Application Log Repository
    /// </summary>
    public class ApplicationLogRepository : Repository<ApplicationLog>, IApplicationLogRepository
    {
        /// <summary>
        /// Application Log Repository constructor 
        /// </summary>
        /// <param name="unitOfWork"></param>
        public ApplicationLogRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }
        /// <summary>
        /// Add Log
        /// </summary>
        /// <param name="applicationLog"></param>
        /// <returns></returns>
        public async Task Insert(ApplicationLog applicationLog)
        {
            try
            {
                await InsertAsync(applicationLog);
                await UnitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
