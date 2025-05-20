using CMC.Kernel.Core.Services;
using CMC.Kernel.Core.Wrappers;
using CMC.Presentation.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CMC.Presentation.Application.Services.Settings
{
    public interface ISettingsService : IApplicationService
    {
        Task<T> GetValue<T>(string key);

        Task<Response> DeleteBackgroundImg();

        Task<Response> UpdateSystemSettings(SettingDTO settingDTO);
    }
}
