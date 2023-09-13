using CMC.Kernel.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace CMC.Presentation.Application.DTOs.Identity
{
    public class SearchUserDTO : PagedRequest
    {
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public int? GroupId { get; set; }
    }
}
