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
    public class TeamsMapping : IEntityTypeConfiguration<Team>, IEntityMapping
    {
        public void Configure(EntityTypeBuilder<Team> builder)
        {
            builder.ToTable("Teams", SchemaName.CMC);
            builder.HasKey(t => t.Id);
            builder.Property(x => x.CreatedOn).HasDefaultValue(DateTime.Now);
            builder.Property(x => x.IsDeleted).HasDefaultValue(false);
            builder.Property(t => t.Player1Id)
           .HasColumnName("Player1");

            builder.Property(t => t.Player2Id)
                .HasColumnName("Player2");

            builder.Property(t => t.Player3Id)
                .HasColumnName("Player3");

            builder.Property(t => t.Player4Id)
                .HasColumnName("Player4");

            builder.HasOne(a => a.Player1)
                .WithMany()
                .HasForeignKey(a => a.Player1Id);

            builder.HasOne(a => a.Player2)
                .WithMany()
                .HasForeignKey(a => a.Player2Id);

            builder.HasOne(a => a.Player3)
                .WithMany()
                .HasForeignKey(a => a.Player3Id);

            builder.HasOne(a => a.Player4)
                .WithMany()
                .HasForeignKey(a => a.Player4Id);
        }
    }
}
