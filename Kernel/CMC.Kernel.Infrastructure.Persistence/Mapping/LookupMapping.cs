using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CMC.Kernel.Core.Constants;
using CMC.Kernel.Core.Persistence;
using CMC.Kernel.Domain.Entities;
using System;

namespace CMC.Kernel.Infrastructure.Persistence.Mapping
{
    /// <summary>
    /// Lookup Mapping 
    /// </summary>
    public class LookupMapping : IEntityTypeConfiguration<Lookup>, IEntityMapping
    {
        public void Configure(EntityTypeBuilder<Lookup> builder)
        {
            builder.ToTable("Lookups", SchemaName.Common);
            builder.HasKey(t => t.Id);
            builder.HasOne(a => a.LookupCategory).WithMany(a => a.Lookups).HasForeignKey(a => a.CategoryID).IsRequired();
            builder.Property(x => x.CreatedOn).HasDefaultValue(DateTime.Now);
        }
    }
}
