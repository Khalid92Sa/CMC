using System.Collections.Generic;

namespace CMC.Kernel.Domain.Entities.Identity
{
    public class Permission : FullAuditableEntity<int>
    {
        public string NameEn { get; set; }
        public string NameAr { get; set; }
        public string Code { get; set; }

        public virtual ICollection<GroupPermission> GroupPermissions { get; set; }
    }
}
