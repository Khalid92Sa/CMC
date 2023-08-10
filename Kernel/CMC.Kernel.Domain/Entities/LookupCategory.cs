using CMC.Kernel.Domain.Entities.Base;
using System;
using System.Collections.Generic;

namespace CMC.Kernel.Domain.Entities
{
    public class LookupCategory : Entity<int>, IAuditableEntity
    {
        public string Code { get; set; }
        public string NameEn { get; set; }
        public string NameAr { get; set; }
        public string Description { get; set; }
        public bool IsDeleted { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }

        public virtual ICollection<Lookup> Lookups { get; set; }
    }
}
