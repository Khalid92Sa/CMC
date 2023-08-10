using System;
using System.Collections.Generic;
using System.Text;

namespace CMC.Presentation.Application.DTOs.Identity
{
    public class LoginDTO
    {
        public int UserID { get; set; }
        public string NationalID { get; set; }
        public string MobileNumber { get; set; }
        public string UserFullName { get; set; }
        public string BirthDate { get; set; }
        public string Captcha { get; set; }
        public string MatchCaptcha { get; set; }
        public bool TCAccepted { get; set; }
        public bool IsLogin { get; set; }
        public bool IsMortgage { get; set; }
        public bool IsRIB { get; set; }
        public string RIBCode { get; set; }
    }
}
