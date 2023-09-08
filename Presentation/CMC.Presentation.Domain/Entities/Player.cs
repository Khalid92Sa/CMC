using CMC.Kernel.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Security.Policy;
using System.Text;

namespace CMC.Presentation.Domain.Entities
{
    public class Player : FullAuditableEntity<int>
    {
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public string EmailAddress { get; set; }
        public bool IsEmployee { get; set; }

        public virtual ICollection<CompetitionQuestion> CompetitionQuestions { get; set; }
    }
}
