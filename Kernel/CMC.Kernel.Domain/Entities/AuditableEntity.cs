using CMC.Kernel.Domain.Entities.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace CMC.Kernel.Domain.Entities
{
    public class AuditableEntity<T> : Entity<T>, IAuditableEntity
    {
        public int CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
    }
}
