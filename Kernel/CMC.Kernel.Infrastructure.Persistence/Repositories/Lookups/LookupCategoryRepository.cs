using Microsoft.EntityFrameworkCore;
using CMC.Kernel.Core.Enums;
using CMC.Kernel.Core.Persistence;
using CMC.Kernel.Domain.Entities;
using System.Linq;
using System.Threading.Tasks;

namespace CMC.Kernel.Infrastructure.Persistence.Repositories.Lookups
{
    /// <summary>
    /// Lookup Category Repository
    /// </summary>
    public class LookupCategoryRepository : Repository<LookupCategory>, ILookupCategoryRepository
    {
        /// <summary>
        /// Lookup Category Repository Constrcutor 
        /// </summary>
        /// <param name="unitOfWork"></param>
        public LookupCategoryRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }
        /// <summary>
        /// Get Category ID
        /// </summary>
        /// <param name="lookupTypes"></param>
        /// <returns></returns>
        public async Task<int> GetCategoryId(LookupTypes lookupTypes)
        {
            string code = ((int)lookupTypes).ToString();
            var result = await GetAll().Where(l => l.Code == code && l.IsDeleted == false).FirstOrDefaultAsync();
            return result.Id;
        }
    }
}
