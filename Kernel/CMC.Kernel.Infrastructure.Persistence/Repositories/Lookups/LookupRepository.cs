using Microsoft.EntityFrameworkCore;
using CMC.Kernel.Core.Enums;
using CMC.Kernel.Core.Infrastructure;
using CMC.Kernel.Core.Persistence;
using CMC.Kernel.Domain.Entities;
using CMC.Kernel.Infrastructure.Caching.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CMC.Kernel.Infrastructure.Persistence.Repositories.Lookups
{
    /// <summary>
    /// Lookup Repository
    /// </summary>
    public class LookupRepository : Repository<Lookup>, ILookupRepository
    {
        /// <summary>
        /// Cache Repository 
        /// </summary>
        private readonly ICacheRepository _cacheRepository;
        /// <summary>
        /// Constructor for  Lookup Repository
        /// </summary>
        /// <param name="unitOfWork"></param>
        /// <param name="cacheRepository"></param>
        public LookupRepository(IUnitOfWork unitOfWork, ICacheRepository cacheRepository) : base(unitOfWork)
        {
            _cacheRepository = cacheRepository;
        }
        /// <summary>
        /// Get Lookup Items by type 
        /// </summary>
        /// <param name="lookupTypes"></param>
        /// <returns></returns>
        public async Task<List<LookupModel>> GetLookupItems(LookupTypes lookupTypes)
        {
            List<LookupModel> lookupModels = await GetLookups(lookupTypes);
            return lookupModels;
        }
        public async Task<List<LookupModel>> GetLookupItems(List<int>IDs)
        {
            try
            {
                List<LookupModel> lookupModels = new List<LookupModel>();
                var result = await GetAll().Where(l => IDs.Contains(l.Id) && l.IsDeleted == false).Select(a => new LookupModel
                {
                    Id = a.Id,
                    Code = a.Code,
                    NameAr = a.NameAr,
                    NameEn = a.NameEn
                }).ToListAsync();
                return result;

            }
            catch (Exception)
            {

                throw;
            }
           

           
        }


        /// <summary>
        /// Get Lookup By Name And Category Code
        /// </summary>
        /// <param name="name"></param>
        /// <param name="IsEnglish"></param>
        /// <param name="lookupType"></param>
        /// <returns></returns>
        public async Task<LookupModel> GetLookupByNameAndCategoryCode(string name, bool isEnglish, LookupTypes lookupType)
        {
            var result = await _cacheRepository.GetObjectAsync(lookupType.ToString(),
                async delegate ()
                {
                    return await GetLookups(lookupType);
                });

            LookupModel lookup = null;

            if (isEnglish)
                lookup = result.Where(a => a.NameEn == name).SingleOrDefault();
            else
                lookup = result.Where(a => a.NameAr == name).SingleOrDefault();
            //In case selected language is English but there is no value in NameEn, then get NameAr
            if (lookup == null)
            {
                if (isEnglish)
                    lookup = result.Where(a => a.NameAr == name).SingleOrDefault();
                else
                    lookup = result.Where(a => a.NameEn == name).SingleOrDefault();
            }
            return lookup;
        }
        /// <summary>
        /// Get Lookup By Code And Category Code
        /// </summary>
        /// <param name="code"></param>
        /// <param name="lookupTypes"></param>
        /// <returns></returns>
        public async Task<LookupModel> GetLookupByCodeAndCategoryCode(string code, LookupTypes lookupTypes)
        {
            var result = await _cacheRepository.GetObjectAsync(lookupTypes.ToString(),
                         async delegate ()
                         {
                             return await GetLookups(lookupTypes);
                         });
            return result.Where(a => a.Code == code).SingleOrDefault();
        }

        /// <summary>
        /// Get Cached Lookups
        /// </summary>
        /// <param name="lookupTypes"></param>
        /// <returns></returns>
        private async Task<List<LookupModel>> GetCachedLookups(LookupTypes lookupTypes)
        {
            var result = await _cacheRepository.GetObjectAsync(lookupTypes.ToString(),
                        async delegate ()
                        {
                            return await GetLookups(lookupTypes);
                        });
            return result;
        }
        /// <summary>
        /// Get Lookups
        /// </summary>
        /// <param name="lookupTypes"></param>
        /// <returns></returns>
        private async Task<List<LookupModel>> GetLookups(LookupTypes lookupTypes)
        {
            try
            {
                string categoryCode = ((int)lookupTypes).ToString();
                var result = await GetAll().Include(l=>l.LookupCategory).Where(l => l.LookupCategory.Code == categoryCode && l.IsDeleted == false).Select(a => new LookupModel
                {
                    Id = a.Id,
                    Code = a.Code,
                    NameAr = a.NameAr,
                    NameEn = a.NameEn,
                    CategoryId = a.CategoryID,
                    Img = a.Img
                }).ToListAsync();
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception($"LookupCode:{lookupTypes.ToString()} ____ {ex.Message}");
            }
            
        }
        /// <summary>
        /// Get Lookup By ID
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public async Task<LookupModel> GetLookupById(int id)
        {
            try
            {
                var result = await GetAll().Where(l => l.Id == id && l.IsDeleted == false).Select(a => new LookupModel
                {
                    Id = a.Id,
                    Code = a.Code,
                    NameAr = a.NameAr,
                    NameEn = a.NameEn,
                    OtherCode = a.OtherCode,
                    CategoryId = a.CategoryID,
                    Img = a.Img
                }).SingleOrDefaultAsync();
                return result;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
