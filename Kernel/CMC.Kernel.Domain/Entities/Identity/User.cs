using System.Collections.Generic;

namespace CMC.Kernel.Domain.Entities.Identity
{
    public class User : FullAuditableEntity<int>
    {
        public string Name { get; set; }
        public string EmailAddress { get; set; }
        public string PhoneNumber { get; set; }
        public bool IsActive { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }

        public virtual ICollection<Role> Role { get; set; }
        public virtual ICollection<UserRole> UserRoles { get; set; }
    }
}
