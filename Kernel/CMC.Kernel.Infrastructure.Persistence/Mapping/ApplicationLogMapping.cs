using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CMC.Kernel.Core.Constants;
using CMC.Kernel.Core.Persistence;
using CMC.Kernel.Domain.Logs;
using System;
using System.Collections.Generic;
using System.Text;

namespace CMC.Kernel.Infrastructure.Persistence.Mapping
{
    public class ApplicationLogMapping : IEntityTypeConfiguration<ApplicationLog>, IEntityMapping
    {
        public void Configure(EntityTypeBuilder<ApplicationLog> builder)
        {
            builder.ToTable("ApplicationLog", SchemaName.Log);
            builder.HasKey(t => t.Id);
            builder.Property(p => p.LoggedAt).HasColumnType("datetime").IsRequired();
            builder.Property(p => p.IDUserName).HasMaxLength(50);
            builder.Property(p => p.Service).HasMaxLength(500);
            builder.Property(p => p.ResultCode).HasMaxLength(50);
            builder.Property(p => p.ResultType).HasMaxLength(50);
            builder.Property(p => p.IPAddress).HasMaxLength(50);
            builder.Property(p => p.Device).HasMaxLength(200);
            builder.Property(p => p.BrowserName).HasMaxLength(50);
            builder.Property(p => p.UserAgent).HasMaxLength(500);
            builder.Property(p => p.ApplicationName).HasMaxLength(50);
        }
    }
}
