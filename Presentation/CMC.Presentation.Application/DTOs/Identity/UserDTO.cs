using System;
using System.Collections.Generic;
using System.Text;

namespace CMC.Presentation.Application.DTOs.Identity
{
    public class UserDTO
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        public string UserName { get; set; }
        public List<RoleDTO> Roles { get; set; } = new List<RoleDTO>();
        public bool IsActive { get; set; }
        public string EmailAddress { get; set; }
        public string PhoneNumber { get; set; }
    }
}
