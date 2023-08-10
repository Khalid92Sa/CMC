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
    public class HttpLogMapping : IEntityTypeConfiguration<HttpLog>, IEntityMapping
    {
        public void Configure(EntityTypeBuilder<HttpLog> builder)
        {
            builder.ToTable("HttpLog", SchemaName.Log);
            builder.HasKey(t => t.Id);
            builder.Property(p => p.ServiceName).HasMaxLength(100);
            builder.Property(p => p.ActionName).HasMaxLength(100);
            builder.Property(p => p.Url).HasMaxLength(500);
            builder.Property(p => p.IPAddress).HasMaxLength(20);
            builder.Property(p => p.RequestType).HasMaxLength(10);
            builder.Property(p => p.RequestHeader).HasColumnType("nvarchar(MAX)");
            builder.Property(p => p.RequestQueries).HasColumnType("nvarchar(MAX)");
            builder.Property(p => p.RequestObject).HasColumnType("nvarchar(MAX)");
            builder.Property(p => p.ResponseObject).HasColumnType("nvarchar(MAX)");
            builder.Property(p => p.ResponseStatus).HasMaxLength(10);
            builder.Property(p => p.ResponseHeader).HasColumnType("nvarchar(MAX)");
            builder.Property(p => p.ResponseContentType).HasColumnType("nvarchar(MAX)");
            builder.Property(p => p.RequestedOn).HasColumnType("datetime");
            builder.Property(p => p.RespondedOn).HasColumnType("datetime");
            builder.Property(p => p.ExceptionMessage).HasColumnType("nvarchar(MAX)");
            builder.Property(p => p.ExceptionStackTrace).HasColumnType("nvarchar(MAX)");
            builder.Property(p => p.ExceptionDetails).HasColumnType("nvarchar(MAX)");
        }
    }
}
