using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using System;
using System.Collections.Generic;
using System.Text;

namespace CMC.Kernel.Core.Constants
{
    public class RegularExpressionsSettings
    {
        public const string OnlyNumbers = @"^\d+$";
        public const string MobileNumber = @"^(077|078|079)\d{7}$";
        public const string EmailAddress = @"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$|^\+?\d{0,2}\-?\d{4,5}\-?\d{5,6}";
    }
}
