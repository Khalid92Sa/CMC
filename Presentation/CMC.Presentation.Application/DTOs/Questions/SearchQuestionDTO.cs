using CMC.Kernel.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace CMC.Presentation.Application.DTOs.Questions
{
    public class SearchQuestionDTO : PagedRequest
    {
        public int CategoryId { get; set; }
        public string QuestionText { get; set; }
    }
}
