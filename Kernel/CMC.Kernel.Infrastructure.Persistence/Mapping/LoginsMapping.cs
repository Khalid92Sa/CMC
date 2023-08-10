using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CMC.Kernel.Core.Constants;
using CMC.Kernel.Core.Persistence;
using CMC.Kernel.Domain.Entities.Identity;
namespace CMC.Kernel.Infrastructure.Persistence.Mapping
{
    /// <summary>
    /// Login table Mapping
    /// </summary>
    public  class LoginsMapping : IEntityTypeConfiguration<Login>, IEntityMapping
    {
        public void Configure(EntityTypeBuilder<Login> builder)
        {
            builder.ToTable("Logins", SchemaName.Identity);
            builder.HasKey(t => t.SessionId);
        }
    }
}
