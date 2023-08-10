using Microsoft.EntityFrameworkCore;
using CMC.Kernel.Core.Persistence;
using CMC.Kernel.Domain.Entities.Administration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMC.Kernel.Infrastructure.Persistence.Repositories.Settings
{
    /// <summary>
    /// 
    /// </summary>
    public class SettingRepository : Repository<Setting>, ISettingRepository
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="unitOfWork"></param>
        public SettingRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Key"></param>
        /// <returns></returns>
        public async Task<string> GetSettingValue(string Key)
        {
            var setting = await GetAll().Where(a => a.Key == Key).SingleOrDefaultAsync();
            if (setting != null)
                return setting.Value;
            return null;
        }
    }
}
