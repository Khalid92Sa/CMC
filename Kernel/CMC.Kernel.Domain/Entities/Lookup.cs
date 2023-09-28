using CMC.Kernel.Domain.Entities.Base;
using System;
using System.Collections.Generic;

namespace CMC.Kernel.Domain.Entities
{
    public class Lookup : Entity<int>, IAuditableEntity
    {
        public string Code { get; set; }
        public string NameEn { get; set; }
        public string NameAr { get; set; }
        public string OtherCode { get; set; }
        public bool IsHighRisk { get; set; }
        public int Sort { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool IsDeleted { get; set; }
        public int? DeletedBy { get; set; }
        public DateTime? DeletedOn { get; set; }

        public virtual int CategoryID { get; set; }
        public virtual LookupCategory LookupCategory { get; set; }
    }
}
