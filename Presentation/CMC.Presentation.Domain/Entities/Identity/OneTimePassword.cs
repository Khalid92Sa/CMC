using CMC.Kernel.Domain.Entities.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace CMC.Presentation.Domain.Entities.Identity
{
    public class OneTimePassword : Entity<int>
    {
        public string SecurityCode { get; set; }
        public int NoOfTrials { get; set; }
        public int NoOfGenerations { get; set; }
        public string MobileNumber { get; set; }
        public DateTime? UnlockedDate { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }
}
