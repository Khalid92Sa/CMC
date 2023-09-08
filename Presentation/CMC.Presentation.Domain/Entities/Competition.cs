using CMC.Kernel.Domain.Entities;
using CMC.Kernel.Domain.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace CMC.Presentation.Domain.Entities
{
    public class Competition : FullAuditableEntity<int>
    {
        public string Name { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public int? HostID { get; set; }
        public virtual User Host { get; set; }


        public virtual ICollection<CompetitionQuestion> CompetitionQuestions { get; set; }
    }
}
