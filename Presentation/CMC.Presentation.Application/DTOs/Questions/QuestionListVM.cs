using System;
using System.Collections.Generic;
using System.Text;

namespace CMC.Presentation.Application.DTOs.Questions
{
    public class QuestionListVM
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public int Time { get; set; }
        public int Points { get; set; }
    }
}
