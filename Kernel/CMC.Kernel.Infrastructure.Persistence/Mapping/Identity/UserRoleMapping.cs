using CMC.Kernel.Core.Persistence;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using CMC.Kernel.Core.Constants;
using CMC.Kernel.Domain.Entities.Identity;

namespace CMC.Kernel.Infrastructure.Persistence.Mapping.Identity
{
    public class UserRoleMapping : IEntityTypeConfiguration<UserRole>, IEntityMapping
    {
        public void Configure(EntityTypeBuilder<UserRole> builder)
        {
            builder.HasKey(x => new { x.UserId, x.RoleId });
            builder.ToTable("UserRole", SchemaName.Identity);
        }
    }
}
