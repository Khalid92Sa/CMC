using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CMC.Kernel.Core.Constants;
using CMC.Kernel.Core.Persistence;
using CMC.Kernel.Domain.Entities;
using System;

namespace CMC.Kernel.Infrastructure.Persistence.Mapping
{
    /// <summary>
    /// Lookup Category Mapping
    /// </summary>
    public class LookupCategoryMapping : IEntityTypeConfiguration<LookupCategory>, IEntityMapping
    {
        public void Configure(EntityTypeBuilder<LookupCategory> builder)
        {
            builder.ToTable("LookupCategories", SchemaName.Common);
            builder.HasKey(t => t.Id);
            builder.HasMany(a => a.Lookups).WithOne(a => a.LookupCategory);
            builder.Property(x => x.CreatedOn).HasDefaultValue(DateTime.Now);
            builder.Property(x => x.IsDeleted).HasDefaultValue(false);
        }
    }
}
