using System;
using System.Collections.Generic;
using System.Text;

namespace CMC.Kernel.Domain.Entities.Identity
{
    public class GroupPermission : FullAuditableEntity<int>
    {
        public int GroupId { get; set; }
        public virtual Group Group { get; set; }
        public int PermissionId { get; set; }
        public virtual Permission Permission { get; set; }
    }
}
