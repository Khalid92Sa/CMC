using CMC.Kernel.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace CMC.Presentation.Domain.Entities
{
    public class Question : FullAuditableEntity<int>
    {
        public string TextEn { get; set; }
        public string TextAr { get; set; }
        public bool? HasImg { get; set; }
        public int AnswersType { get; set; }
        public virtual int? CategoryID { get; set; }
        public virtual Lookup Category { get; set; }


        public virtual ICollection<Answer> Answers { get; set; }
        public virtual ICollection<CompetitionQuestion> CompetitionQuestions { get; set; }
    }
}
