using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace CMC.Presentation.Application.DTOs
{
    public class SettingDTO
    {
        public IFormFile BackgroundImg { get; set; }
        public string BackgroundImgPath { get; set; }
    }
}
