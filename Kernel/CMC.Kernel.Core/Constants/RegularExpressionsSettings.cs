using System;
using System.Collections.Generic;
using System.Text;

namespace CMC.Kernel.Core.Constants
{
    public class RegularExpressionsSettings
    {
        public const string IDIqamaNumber = @"^[1-2][0-9]{9}$";
        public const string SaudiMobileNumber = @"^(05)\d{8}$";
        public const string EmailAddress = @"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$|^\+?\d{0,2}\-?\d{4,5}\-?\d{5,6}";
    }
}
