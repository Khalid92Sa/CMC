using CMC.Kernel.Core.Persistence;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using CMC.Kernel.Core.Constants;
using CMC.Kernel.Domain.Entities.Identity;
using System;

namespace CMC.Kernel.Infrastructure.Persistence.Mapping.Identity
{
    public class UserMapping : IEntityTypeConfiguration<User>, IEntityMapping
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("Users", SchemaName.Identity);
            builder.Property(x => x.CreatedOn).HasDefaultValue(DateTime.Now);
            builder.Property(x => x.IsDeleted).HasDefaultValue(false);
            builder.Property(x => x.IsActive).HasDefaultValue(true);
            builder.HasMany(x => x.UserRoles)
                .WithOne(x => x.User)
                .HasForeignKey(x => x.UserId);
        }
    }
}
