using CMC.Kernel.Core.Constants;
using CMC.Kernel.Domain.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace CMC.Kernel.Infrastructure.Persistence.Mapping.Identity
{
    public class PermissionMapping : EntityMapping<Permission, int>
    {
        public override void Configure(EntityTypeBuilder<Permission> builder)
        {
            base.Configure(builder);
            builder.ToTable("Permissions", SchemaName.Identity);
            builder.Property(x => x.NameEn).HasMaxLength(400);
            builder.Property(x => x.NameAr).HasMaxLength(400);
            builder.Property(x => x.Code).HasMaxLength(400);
            builder.Property(x => x.CreatedOn).HasDefaultValue(DateTime.Now);
            builder.Property(x=>x.IsDeleted).HasDefaultValue(false);

            builder.HasMany(x => x.GroupPermissions)
                .WithOne(x => x.Permission)
                .HasForeignKey(x => x.PermissionId)
                .IsRequired();
        }
    }
}
