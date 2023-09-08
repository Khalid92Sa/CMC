using System;
using System.Collections.Generic;
using System.Text;

namespace CMC.Kernel.Core.Validators
{
    public static class RegularExpressionsValidator
    {
        public static string ArabicName = @"^[\u0621-\u064A ]+$";
        public static string EnglishName = @"^[a-zA-Z ]+$";
        public static string ArabicEnglishName = "^[\u0600-\u065F\u066A-\u06EF\u06FA-\u06FFa-zA-Z ]+[\u0600-\u065F\u066A-\u06EF\u06FA-\u06FFa-zA-Z-_ ]*$";
        public static string EmailAddress = @"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$|^\+?\d{0,2}\-?\d{4,5}\-?\d{5,6}";
        public static string NumbersOnly = "^[0-9]*$";
        public static string ComplexPassword = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).\S{7,}$";
    }
}
