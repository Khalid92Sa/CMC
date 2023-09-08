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
    public class QuestionsMapping : IEntityTypeConfiguration<Question>, IEntityMapping
    {
        public void Configure(EntityTypeBuilder<Question> builder)
        {
            builder.ToTable("Questions", SchemaName.CMC);
            builder.HasKey(t => t.Id);
            builder.HasMany(x => x.Answers)
                .WithOne(x => x.Question)
                .HasForeignKey(x => x.QuestionId);

            builder.Property(x => x.CreatedOn).HasDefaultValue(DateTime.Now);
            builder.Property(x => x.IsDeleted).HasDefaultValue(false);
        }
    }
}
