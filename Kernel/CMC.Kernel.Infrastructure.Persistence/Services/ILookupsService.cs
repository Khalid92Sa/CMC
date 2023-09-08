using CMC.Kernel.Core.Enums;
using CMC.Kernel.Infrastructure.Caching.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CMC.Kernel.Infrastructure.Persistence.Services
{
    public interface ILookupsService
    {
        Task<LookupModel> GetLookupById(int id);
        Task<List<LookupModel>> GetLookupItems(LookupTypes lookupTypes);
    }
}
