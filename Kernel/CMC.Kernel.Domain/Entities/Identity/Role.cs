using System.Collections.Generic;

namespace CMC.Kernel.Domain.Entities.Identity
{
    public class Role : FullAuditableEntity<int>
    {
        public string Name { get; set; }
        public virtual ICollection<Permission> Permissions { get; set; }
        public virtual ICollection<User> Users { get; set; }
    }
}
