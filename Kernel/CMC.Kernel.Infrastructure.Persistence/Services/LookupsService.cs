using CMC.Kernel.Core.Enums;
using CMC.Kernel.Core.Services;
using CMC.Kernel.Infrastructure.Caching.Model;
using CMC.Kernel.Infrastructure.Persistence.Repositories.Lookups;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CMC.Kernel.Infrastructure.Persistence.Services
{
    /// <summary>
    /// 
    /// </summary>
    public class LookupsService : ILookupsService, IApplicationService
    {
        /// <summary>
        /// 
        /// </summary>
        readonly ILookupRepository _lookupRepository;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="lookupRepository"></param>
        public LookupsService(ILookupRepository lookupRepository)
        {
            _lookupRepository = lookupRepository;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<LookupModel> GetLookupById(int id)
        {
            try
            {
                return await _lookupRepository.GetLookupById(id);
            }
            catch (Exception ex)
            {
                throw ;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="lookupTypes"></param>
        /// <returns></returns>
        public async Task<List<LookupModel>> GetLookupItems(LookupTypes lookupTypes)
        {
            try
            {
                return await _lookupRepository.GetLookupItems(lookupTypes);
            }
            catch (Exception ex)
            {

                throw ;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<List<LookupModel>> GetCities()
        {
            try
            {
                return await _lookupRepository.GetCities();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

    }
}
