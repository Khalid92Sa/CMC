using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CMC.Kernel.Core.Persistence;
using CMC.Kernel.Domain.Entities;
using CMC.Kernel.Domain.Entities.Base;

namespace CMC.Kernel.Infrastructure.Persistence.Mapping
{

    /// <summary>
    /// Entity Mapping
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="IdType"></typeparam>
    public abstract class EntityMapping<T, IdType> : IEntityTypeConfiguration<T>, IEntityMapping where T : Entity<IdType>
    {
        /// <summary>
        /// Configure 
        /// </summary>
        /// <param name="builder"></param>
        public virtual void Configure(EntityTypeBuilder<T> builder)
        {
            builder.Property(x => x.Id).ValueGeneratedOnAdd();
        }
    }
    /// <summary>
    /// Auditable Entity Mapping
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="IdType"></typeparam>
    public abstract class AuditableEntityMapping<T, IdType> : IEntityTypeConfiguration<T>, IEntityMapping where T : AuditableEntity<IdType>
    {
        /// <summary>
        /// Configure
        /// </summary>
        /// <param name="builder"></param>
        public virtual void Configure(EntityTypeBuilder<T> builder)
        {
            builder.Property(x => x.Id).ValueGeneratedOnAdd();
        }
    }
    /// <summary>
    /// Full Auditable Entity Mapping
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="IdType"></typeparam>
    public abstract class FullAuditableEntityMapping<T, IdType> : IEntityTypeConfiguration<T>, IEntityMapping where T : FullAuditableEntity<IdType>
    {
        /// <summary>
        /// Configure
        /// </summary>
        /// <param name="builder"></param>
        public virtual void Configure(EntityTypeBuilder<T> builder)
        {
            builder.Property(x => x.Id).ValueGeneratedOnAdd();
        }
    }
}
