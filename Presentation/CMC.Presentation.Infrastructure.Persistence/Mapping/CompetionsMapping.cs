using CMC.Kernel.Core.Constants;
using CMC.Kernel.Core.Persistence;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using CMC.Presentation.Domain.Entities;

namespace CMC.Presentation.Infrastructure.Persistence.Mapping
{
    public class CompetionsMapping : IEntityTypeConfiguration<Competition>, IEntityMapping
    {
        public void Configure(EntityTypeBuilder<Competition> builder)
        {
            builder.ToTable("Competitions", SchemaName.CMC);
            builder.HasKey(t => t.Id);
            builder.HasOne(t => t.Host);
            builder.HasMany(x => x.CompetitionQuestions)
                .WithOne(x => x.Competition)
                .HasForeignKey(x => x.CompetitionId);

            builder.Property(x => x.CreatedOn).HasDefaultValue(DateTime.Now);
            builder.Property(x => x.IsDeleted).HasDefaultValue(false);

        }
    }
}
