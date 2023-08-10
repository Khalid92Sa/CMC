using Microsoft.EntityFrameworkCore;
using CMC.Kernel.Core.Services;
using CMC.Kernel.Infrastructure.Persistence.Repositories.Settings;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CMC.Presentation.Application.Services.Settings
{
    public class SettingsService : BaseServiceHandler, ISettingsService
    {
        private readonly ISettingRepository _settingRepository;

        public SettingsService(ISettingRepository settingRepository)
        {
            _settingRepository = settingRepository;
        }

        /// <summary>
        /// This method is to get Setting value by its key then return it converted to the specified type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<T> GetValue<T>(string key)
        {
            try
            {
                var setting = await _settingRepository.GetAll(s => s.Key.ToLower() == key.ToLower()).AsNoTracking().FirstOrDefaultAsync();
                if (setting != null)
                    return (T)Convert.ChangeType(setting.Value,typeof(T));
                else
                    return default(T);
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
