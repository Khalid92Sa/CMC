using CMC.Kernel.Core.Persistence;
using CMC.Kernel.Domain.Entities.Identity;
using CMC.Kernel.Infrastructure.Caching.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMC.Kernel.Infrastructure.Persistence.Services
{
    public class GroupPermissionService : IGroupPermissionService
    {
        readonly IRepository<Group> _groupRepository;
        readonly IRepository<GroupPermission> _groupPermissionRepository;
        readonly IRepository<Permission> _permissionRepository;
        readonly IRepository<User> _userRepository;

        public GroupPermissionService(IRepository<Group> groupRepository,
            IRepository<Permission> permissionRepository,
            IRepository<GroupPermission> groupPermissionRepository,
            IRepository<User> userRepository)
        {
            _groupRepository = groupRepository;
            _permissionRepository = permissionRepository;
            _groupPermissionRepository = groupPermissionRepository;
            _userRepository = userRepository;
        }

        /// <summary>
        /// Get groups
        /// </summary>
        /// <returns></returns>
        public async Task<List<LookupModel>> GetGroups()
        {
            var groups = await _groupRepository.GetAll().Select(x => new LookupModel
            {
                Id = x.Id,
                NameEn = x.NameEn,
                NameAr = x.NameAr
            }).ToListAsync();
            return groups;
        }

        /// <summary>
        /// Get permission for group
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        public async Task<List<LookupModel>> GetPermissionByGroupId(int groupId)
        {
            try
            {
                var permissions = await _groupPermissionRepository.GetAll(a => a.GroupId == groupId)
                    .Include(a => a.Permission)
                    .Select(a => new LookupModel()
                    {
                        Id = a.Permission.Id,
                        NameEn = a.Permission.NameEn,
                        Code = a.Permission.Code
                    }).ToListAsync();

                return permissions;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
