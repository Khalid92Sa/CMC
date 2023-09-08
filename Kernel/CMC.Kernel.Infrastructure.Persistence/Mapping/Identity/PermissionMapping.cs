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
            builder.Property(x => x.Name).HasMaxLength(128);
            builder.Property(x => x.CreatedOn).HasDefaultValue(DateTime.Now);
            builder.Property(x=>x.IsDeleted).HasDefaultValue(false);
        }
    }
}
