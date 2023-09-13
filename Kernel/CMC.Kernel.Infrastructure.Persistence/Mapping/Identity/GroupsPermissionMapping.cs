using CMC.Kernel.Core.Constants;
using CMC.Kernel.Domain.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace CMC.Kernel.Infrastructure.Persistence.Mapping.Identity
{
    public class GroupsPermissionMapping : EntityMapping<GroupPermission, int>
    {
        public override void Configure(EntityTypeBuilder<GroupPermission> builder)
        {
            base.Configure(builder);
            builder.ToTable("GroupPermissions", SchemaName.Identity);
            builder.Property(x => x.CreatedOn).HasDefaultValue(DateTime.Now);
            builder.Property(x => x.IsDeleted).HasDefaultValue(false);

            builder.HasOne(x => x.Group)
                .WithMany(x => x.GroupPermissions)
                .HasForeignKey(x => x.GroupId)
                .IsRequired();

            builder.HasOne(x => x.Permission)
                .WithMany(x => x.GroupPermissions)
                .HasForeignKey(x => x.PermissionId)
                .IsRequired();
        }
    }
}
