using CMC.Kernel.Core.Persistence;
using CMC.Kernel.Domain.Entities.Administration;
using System.Threading.Tasks;

namespace CMC.Kernel.Infrastructure.Persistence.Repositories.Settings
{
    public interface ISettingRepository : IRepository<Setting>
    {
        public Task<string> GetSettingValue(string Key);
    }
}
