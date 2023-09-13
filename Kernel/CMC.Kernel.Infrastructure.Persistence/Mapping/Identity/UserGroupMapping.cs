using CMC.Kernel.Core.Persistence;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using CMC.Kernel.Core.Constants;
using CMC.Kernel.Domain.Entities.Identity;
using System;

namespace CMC.Kernel.Infrastructure.Persistence.Mapping.Identity
{
    public class UserGroupMapping : IEntityTypeConfiguration<UserGroup>, IEntityMapping
    {
        public void Configure(EntityTypeBuilder<UserGroup> builder)
        {
            builder.ToTable("UserGroups", SchemaName.Identity);
            builder.Property(x => x.CreatedOn).HasDefaultValue(DateTime.Now);
            builder.Property(x => x.IsDeleted).HasDefaultValue(false);

            builder.HasOne(x => x.User)
                .WithMany(x => x.UserGroups)
                .HasForeignKey(x => x.UserId)
                .IsRequired();

            builder.HasOne(x => x.Group)
                .WithMany(x => x.UserGroups)
                .HasForeignKey(x => x.GroupID)
                .IsRequired();
            
        }
    }
}
