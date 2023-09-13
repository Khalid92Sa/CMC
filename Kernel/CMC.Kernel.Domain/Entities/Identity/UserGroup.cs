using System.Collections.Generic;

namespace CMC.Kernel.Domain.Entities.Identity
{
    public class UserGroup : FullAuditableEntity<int>
    {
        public virtual int UserId { get; set; }
        public virtual User User { get; set; }

        public virtual int GroupID { get; set; }
        public virtual Group Group { get; set; }
    }
}
