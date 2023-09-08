using CMC.Kernel.Core.Constants;
using CMC.Kernel.Core.Persistence;
using CMC.Kernel.Domain.Entities.Identity;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using CMC.Presentation.Domain.Entities;

namespace CMC.Presentation.Infrastructure.Persistence.Mapping
{
    public class CompetitionQuestionsMapping : IEntityTypeConfiguration<CompetitionQuestion>, IEntityMapping
    {
        public void Configure(EntityTypeBuilder<CompetitionQuestion> builder)
        {
            builder.HasKey(x => x.Id);
            builder.ToTable("CompetionQuestions", SchemaName.CMC);

            builder.HasOne(a => a.Question)
                .WithMany(a => a.CompetitionQuestions)
                .HasForeignKey(a => a.QuestionId);

            builder.HasOne(a => a.Answer)
                .WithMany(a => a.CompetitionQuestions)
                .HasForeignKey(a => a.AnswerId);

            builder.HasOne(a => a.Player)
                .WithMany(a => a.CompetitionQuestions)
                .HasForeignKey(a => a.PlayerId);
        }
    }
}
