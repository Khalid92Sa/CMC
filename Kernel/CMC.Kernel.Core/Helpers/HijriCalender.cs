using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace CMC.Kernel.Core.Helpers
{
    public static class HijriCalender
    {
        public static DateTime ToGregorian(string HijriDate)
        {
            CultureInfo cultureInfo = new CultureInfo("ar-SA");
            DateTime.TryParseExact(HijriDate, "yyyy/MM/dd", cultureInfo.DateTimeFormat, DateTimeStyles.AllowInnerWhite, out var result);
            return result;
        }

        public static string ToHijri(DateTime GregorianDate)
        {
            CultureInfo cultureInfo = new CultureInfo("ar-SA");
            return GregorianDate.ToString("yyyy/MM/dd", cultureInfo.DateTimeFormat);
        }
    }
}
