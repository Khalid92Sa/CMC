using CMC.Kernel.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace CMC.Presentation.Domain.Entities
{
    public class Answer : FullAuditableEntity<int>
    {
        public string TextEn { get; set; }
        public string TextAr { get; set; }
        public bool IsAnswer { get; set; }


        public virtual int QuestionId { get; set; }
        public virtual Question Question{ get; set; }

        public virtual ICollection<CompetitionQuestion> CompetitionQuestions { get; set; }

    }
}
