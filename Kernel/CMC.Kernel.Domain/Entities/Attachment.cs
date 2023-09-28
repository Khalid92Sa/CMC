using CMC.Kernel.Domain.Entities.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace CMC.Kernel.Domain.Entities
{
    public class Attachment : Entity<int>, IAuditableEntity
    {
        public byte[] FileData { get; set; }
        public string FileName { get; set;}
        public int EntityType { get; set; }
        public int EntityId { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool? IsDeleted { get; set; }
        public int? DeletedBy { get; set; }
        public DateTime? DeletedOn { get; set; }
    }
}
