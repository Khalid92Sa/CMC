using CMC.Kernel.Domain.Entities.Base;
using System;

namespace CMC.Kernel.Domain.Entities
{
    public class FullAuditableEntity<T> : Entity<T>, IAuditableEntity, ISoftDeletedEntity
    {
        public int CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public int? DeletedBy { get; set; }
        public DateTime? DeletedOn { get; set; }
        public bool? IsDeleted { get; set; }
    }
}
