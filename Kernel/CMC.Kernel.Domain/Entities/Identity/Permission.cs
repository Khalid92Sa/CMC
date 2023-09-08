using System.Collections.Generic;

namespace CMC.Kernel.Domain.Entities.Identity
{
    public class Permission : FullAuditableEntity<int>
    {
        public string Name { get; set; }

        public virtual ICollection<Role> Roles { get; set; }
    }
}
