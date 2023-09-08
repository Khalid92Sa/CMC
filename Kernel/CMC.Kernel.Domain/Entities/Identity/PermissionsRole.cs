
namespace CMC.Kernel.Domain.Entities.Identity
{
    public class PermissionsRole
    {
        public virtual int PermissionId { get; set; }
        public virtual Permission Permission { get; set; }

        public virtual int RoleId { get; set; }
        public virtual Role Role { get; set; }
    }
}
