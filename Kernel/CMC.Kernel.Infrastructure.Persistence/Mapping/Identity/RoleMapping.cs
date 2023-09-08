using CMC.Kernel.Core.Persistence;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using CMC.Kernel.Core.Constants;
using CMC.Kernel.Domain.Entities.Identity;
using System;

namespace CMC.Kernel.Infrastructure.Persistence.Mapping.Identity
{
    public class RoleMapping : IEntityTypeConfiguration<Role>, IEntityMapping
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            builder.ToTable("Roles", SchemaName.Identity);
            builder.Property(x => x.CreatedOn).HasDefaultValue(DateTime.Now);
            builder.Property(x => x.IsDeleted).HasDefaultValue(false);
        }
    }
}
