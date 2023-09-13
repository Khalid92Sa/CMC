using CMC.Kernel.Core.Services;
using CMC.Kernel.Infrastructure.Caching.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CMC.Kernel.Infrastructure.Persistence.Services
{
    public interface IGroupPermissionService : IApplicationService
    {
        /// <summary>
        /// Get groups
        /// </summary>
        /// <returns></returns>
        Task<List<LookupModel>> GetGroups();

        /// <summary>
        /// Get permission for group
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        Task<List<LookupModel>> GetPermissionByGroupId(int groupId);
    }
}
