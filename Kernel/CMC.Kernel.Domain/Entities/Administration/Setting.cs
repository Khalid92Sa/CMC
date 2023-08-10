using CMC.Kernel.Domain.Entities.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace CMC.Kernel.Domain.Entities.Administration
{
    public class Setting : Entity<int>
    {
        public string Key { get; set; }
        public string Value { get; set; }
        public bool IsPublic { get; set; }
    }
}
