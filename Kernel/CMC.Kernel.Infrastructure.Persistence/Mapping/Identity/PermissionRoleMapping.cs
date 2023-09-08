using CMC.Kernel.Core.Constants;
using CMC.Kernel.Core.Persistence;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using CMC.Kernel.Domain.Entities.Identity;

namespace CMC.Kernel.Infrastructure.Persistence.Mapping.Identity
{
    public class PermissionRoleMapping : IEntityTypeConfiguration<PermissionsRole>, IEntityMapping
    {
        public void Configure(EntityTypeBuilder<PermissionsRole> builder)
        {
            builder.HasKey(x => new { x.PermissionId, x.RoleId });
            builder.ToTable("PermissionsRole", SchemaName.Identity);
        }
    }
}
