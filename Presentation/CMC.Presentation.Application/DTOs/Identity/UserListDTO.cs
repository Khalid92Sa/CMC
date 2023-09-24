using System;
using System.Collections.Generic;
using System.Text;

namespace CMC.Presentation.Application.DTOs.Identity
{
    public class UserListDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public string EmailAddress { get; set; }
        public string GroupName { get; set; }
        public bool IsActive { get; set; }
    }
}
