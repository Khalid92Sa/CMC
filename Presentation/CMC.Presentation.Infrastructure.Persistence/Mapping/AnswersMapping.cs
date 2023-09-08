using CMC.Kernel.Core.Constants;
using CMC.Kernel.Core.Persistence;
using CMC.Presentation.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace CMC.Presentation.Infrastructure.Persistence.Mapping
{
    public class AnswersMapping : IEntityTypeConfiguration<Answer>, IEntityMapping
    {
        public void Configure(EntityTypeBuilder<Answer> builder)
        {
            builder.ToTable("Answers", SchemaName.CMC);
            builder.HasKey(t => t.Id);
            builder.HasOne(a => a.Question)
                .WithMany(a => a.Answers)
                .HasForeignKey(a => a.QuestionId)
                .IsRequired();

            builder.Property(x => x.CreatedOn).HasDefaultValue(DateTime.Now);
            builder.Property(x => x.IsDeleted).HasDefaultValue(false);
        }
    }
}
