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
    public class PlayersMapping : IEntityTypeConfiguration<Player>, IEntityMapping
    {
        public void Configure(EntityTypeBuilder<Player> builder)
        {
            builder.ToTable("Players", SchemaName.CMC);
            builder.HasKey(t => t.Id);
           
            builder.Property(x => x.CreatedOn).HasDefaultValue(DateTime.Now);
            builder.Property(x => x.IsDeleted).HasDefaultValue(false);

            builder.HasMany(a => a.Competitions)
                .WithOne(a => a.WinningPlayer)
                .HasForeignKey(a => a.WinningPlayerId);
        }
    }
}
