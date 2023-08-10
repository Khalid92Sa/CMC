using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using CMC.Kernel.Core.Constants;
using CMC.Kernel.Core.Persistence;
using CMC.Presentation.Domain.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace CMC.Presentation.Infrastructure.Persistence.Mapping.Identity
{
    public class OneTimePasswordMapping : IEntityTypeConfiguration<OneTimePassword>, IEntityMapping
    {
        public void Configure(EntityTypeBuilder<OneTimePassword> builder)
        {
            builder.ToTable("OneTimePassword", SchemaName.Identity);
            builder.HasKey(t => t.Id);
            builder.Property(t => t.NoOfTrials).IsRequired();
        }
    }
}
