using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CMC.Kernel.Core.Constants;
using CMC.Kernel.Domain.Entities.Administration;

namespace CMC.Kernel.Infrastructure.Persistence.Mapping
{
    /// <summary>
    /// Setting Mapping
    /// </summary>
    public class SettingMapping : EntityMapping<Setting, int>
    {
        public override void Configure(EntityTypeBuilder<Setting> builder)
        {
            base.Configure(builder);
            builder.ToTable("Settings", SchemaName.Common);
        }
    }
}
