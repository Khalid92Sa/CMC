using System;
using System.Collections.Generic;
using System.Text;

namespace CMC.Kernel.Domain.Entities.Base
{
    public interface ISoftDeletedEntity
    {
        public int? DeletedBy { get; set; }
        public DateTime? DeletedOn { get; set; }
        public bool? IsDeleted { get; set; }
    }
}
