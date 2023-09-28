using CMC.Kernel.Core.Constants;
using CMC.Kernel.Core.Persistence;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using CMC.Kernel.Domain.Entities;
using System;

namespace CMC.Kernel.Infrastructure.Persistence.Mapping
{
    public class AttachmentsMapping : IEntityTypeConfiguration<Attachment>, IEntityMapping
    {
        public void Configure(EntityTypeBuilder<Attachment> builder)
        {
            builder.ToTable("Attachments", SchemaName.Common);
            builder.HasKey(t => t.Id);
            builder.Property(x => x.CreatedOn).HasDefaultValue(DateTime.Now);
            builder.Property(x => x.IsDeleted).HasDefaultValue(false);
        }
    }
}
