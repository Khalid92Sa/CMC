using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace CMC.Presentation.Application.DTOs.Questions
{
    public class CategoryDTO
    {
        public int? Id { get; set; }
        public string NameEn { get; set; }
        public string NameAr { get; set; }
        public IFormFile Img { get; set; }
        public string ImgPath { get; set; }
        public byte[] ImgBinary { get; set; }
        public List<QuestionVM> Questions { get; set; } = new List<QuestionVM>();
    }
}
