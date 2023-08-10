using System;
using System.Collections.Generic;
using System.Text;

namespace CMC.Kernel.Domain.Entities.Identity
{
    public class Login
    {
        public string SessionId { get; set; }
        public string IDNumber { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public int? ModifyBy { get; set; }
        public DateTime? ModifyOn { get; set; }
        public bool LoggedIn { get; set; }
    }
}
