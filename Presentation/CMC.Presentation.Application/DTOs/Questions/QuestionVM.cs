using CMC.Kernel.Infrastructure.Caching.Model;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace CMC.Presentation.Application.DTOs.Questions
{
    public class QuestionVM
    {
        public int? Id { get; set; }
        public string TextEn { get; set; }
        public string TextAr { get; set; }
        public int Time { get; set; }
        public int Points { get; set; }
        public int? CategoryId { get; set; }
        public int? AnswertType { get; set; }
        public IFormFile Img { get; set; }
        public string ImgPath { get; set; }
        public List<LookupModel> Categories { get; set; } = new List<LookupModel>();
        public List<AnswerOptions> Answers { get; set; } = new List<AnswerOptions>();
    }

    public class AnswerOptions
    {
        public int? Id { get; set;}
        public string TextEn { get; set;} 
        public string TextAr { get; set;} 
        public bool IsAnswer { get; set; }
        public bool IsImg { get; set; }
        public string ImgPath { get; set; }
        public IFormFile Img { get; set; }
    }
}
