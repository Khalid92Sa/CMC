using CMC.Kernel.Domain.Entities;
using CMC.Kernel.Domain.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace CMC.Presentation.Domain.Entities
{
    public class CompetitionQuestion : FullAuditableEntity<int>
    {
        public virtual int CompetitionId { get; set; }
        public virtual Competition Competition { get; set; }

        public virtual int QuestionId { get; set; }
        public virtual Question Question { get; set; }

        public virtual int? AnswerId { get; set; }
        public virtual Answer Answer { get; set; }

        public virtual int? PlayerId { get; set; }
        public virtual Player Player { get; set; }

        public bool? IsTeam1 { get; set; }
        public int? Point { get; set; }
        public bool? IsCorrectAnswer { get; set; }
    }
}
