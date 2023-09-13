using System;
using System.Collections.Generic;
using System.Text;

namespace CMC.Kernel.Domain.Entities.Identity
{
    public class Group : FullAuditableEntity<int>
    {
        public string NameEn { get; set; }
        public string NameAr { get; set; }
        public string Code { get; set; }
        public virtual ICollection<GroupPermission> GroupPermissions { get; set; }
        public virtual ICollection<UserGroup> UserGroups { get; set; }
    }
}
