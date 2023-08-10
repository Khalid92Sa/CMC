using CMC.Kernel.Core.Enums;
using CMC.Kernel.Core.Persistence;
using CMC.Kernel.Domain.Entities;
using System.Threading.Tasks;

namespace CMC.Kernel.Infrastructure.Persistence.Repositories.Lookups
{
    /// <summary>
    /// Lookup Category Repository interface  
    /// </summary>
    public interface ILookupCategoryRepository : IRepository<LookupCategory>
    {
        public Task<int> GetCategoryId(LookupTypes lookupTypes);
    }
}
