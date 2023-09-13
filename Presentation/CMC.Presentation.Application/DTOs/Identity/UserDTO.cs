using CMC.Kernel.Core.Enums;
using CMC.Kernel.Infrastructure.Caching.Model;
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
        public string EmailAddress { get; set; }
        public string PhoneNumber { get; set; }
        public int? GroupId { get; set; }
        public List<LookupModel> Groups { get; set; } = new List<LookupModel>();
        public GroupsEnum GroupCode { get; set; }
        public List<string> PermissionCodes { get; set; } = new List<string>();
    }
}
