using System;
using System.Collections.Generic;
using System.Text;

namespace CMC.Kernel.Domain.Entities.Base
{
    public interface IAuditableEntity
    {
        public int CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
    }
}
