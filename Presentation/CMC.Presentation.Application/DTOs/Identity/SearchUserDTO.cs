using CMC.Kernel.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace CMC.Presentation.Application.DTOs.Identity
{
    public class SearchUserDTO : PagedRequest
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string RoleName { get; set; }
        public int? RoleId { get; set; }
    }
}
