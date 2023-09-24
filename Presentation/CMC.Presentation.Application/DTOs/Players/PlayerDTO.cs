using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace CMC.Presentation.Application.DTOs.Players
{
    public class PlayerDTO
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public string EmailAddress { get; set; }
        public bool IsEmployee { get; set; }
        public bool IsBlocked { get; set; }
        public string Comment { get; set; }
    }
}
