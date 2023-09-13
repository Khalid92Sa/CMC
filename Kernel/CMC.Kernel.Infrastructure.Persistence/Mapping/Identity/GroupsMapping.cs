using CMC.Kernel.Core.Constants;
using CMC.Kernel.Domain.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace CMC.Kernel.Infrastructure.Persistence.Mapping.Identity
{
    public class GroupsMapping : EntityMapping<Group, int>
    {
        public override void Configure(EntityTypeBuilder<Group> builder)
        {
            base.Configure(builder);
            builder.ToTable("Groups", SchemaName.Identity);
            builder.Property(x => x.NameEn).HasMaxLength(150);
            builder.Property(x => x.NameAr).HasMaxLength(150);
            builder.Property(x => x.Code).HasMaxLength(150);
            builder.Property(x => x.CreatedOn).HasDefaultValue(DateTime.Now);
            builder.Property(x => x.IsDeleted).HasDefaultValue(false);

            builder.HasMany(x => x.GroupPermissions)
                .WithOne(x => x.Group)
                .HasForeignKey(x => x.GroupId)
                .IsRequired();

            builder.HasMany(x => x.UserGroups)
                .WithOne(x => x.Group)
                .HasForeignKey(x => x.GroupID)
                .IsRequired();
        }
    }
}
