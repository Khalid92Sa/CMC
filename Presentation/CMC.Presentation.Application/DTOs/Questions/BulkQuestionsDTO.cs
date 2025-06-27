using System;
using System.Collections.Generic;
using System.Text;

namespace CMC.Presentation.Application.DTOs.Questions
{
    public class BulkQuestionsDTO
    {
        public List<QuestionVM> Questions { get; set; } = new List<QuestionVM>();

        public int? DefaultCategoryId { get; set; }
    }

    public class ExcelValidationResult
    {
        public bool IsValid { get; set; }
        public List<QuestionVM> Questions { get; set; } = new List<QuestionVM>();
        public List<string> Errors { get; set; } = new List<string>();
        public List<string> Warnings { get; set; } = new List<string>();
    }
}
