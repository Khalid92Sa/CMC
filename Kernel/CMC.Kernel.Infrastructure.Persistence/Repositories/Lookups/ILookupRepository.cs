using CMC.Kernel.Core.Enums;
using CMC.Kernel.Core.Persistence;
using CMC.Kernel.Domain.Entities;
using CMC.Kernel.Infrastructure.Caching.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CMC.Kernel.Infrastructure.Persistence.Repositories.Lookups
{
    public interface ILookupRepository : IRepository<Lookup>
    {
        public Task<List<LookupModel>> GetLookupItems(LookupTypes lookupTypes);
        public Task<LookupModel> GetLookupById(int id);
        public Task<LookupModel> GetLookupByNameAndCategoryCode(string name, bool isEnglish, LookupTypes lookupTypes);
        public Task<LookupModel> GetLookupByCodeAndCategoryCode(string name, LookupTypes lookupTypes);
        public Task<List<LookupModel>> GetLookupItems(List<int> IDs);

    }
}
