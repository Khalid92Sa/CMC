using CMC.Kernel.Core.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CMC.Presentation.Application.Services.Settings
{
    public interface ISettingsService : IApplicationService
    {
        Task<T> GetValue<T>(string key);
    }
}
