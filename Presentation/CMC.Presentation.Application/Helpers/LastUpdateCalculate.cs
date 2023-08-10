using System;

namespace CMC.Presentation.Application.Helpers
{
    public class LastUpdateCalculate
    {
        /// <summary>
        /// English
        /// </summary>
        /// <param name="Date"></param>
        /// <returns></returns>
        public static string calcualte(DateTime Date)
        {
            const int SECOND = 1;
            const int MINUTE = 60 * SECOND;
            const int HOUR = 60 * MINUTE;
            const int DAY = 24 * HOUR;
            const int MONTH = 30 * DAY;

            var ts = new TimeSpan(DateTime.Now.Ticks - Date.Ticks);
            double delta = Math.Abs(ts.TotalSeconds);

            if (delta < 1 * MINUTE)
                return ts.Seconds == 1 ? " one second ago" : " " + ts.Seconds + " seconds ago";

            if (delta < 2 * MINUTE)
                return " a minute ago";

            if (delta < 45 * MINUTE)
                return ts.Minutes + " minutes ago";

            if (delta < 90 * MINUTE)
                return " an hour ago";

            if (delta < 24 * HOUR)
                return ts.Hours + " hours ago";

            if (delta < 48 * HOUR)
                return " yesterday";

            if (delta < 30 * DAY)
                return ts.Days + " days ago";

            if (delta < 12 * MONTH)
            {
                int months = Convert.ToInt32(Math.Floor((double)ts.Days / 30));
                return months <= 1 ? " one month ago" : " " + months + " months ago";
            }
            else
            {
                int years = Convert.ToInt32(Math.Floor((double)ts.Days / 365));
                return years <= 1 ? " one year ago" : " " + years + " years ago";
            }
        }

        /// <summary>
        /// Arabic
        /// </summary>
        /// <param name="Date"></param>
        /// <returns></returns>
        public static string calcualte_ar(DateTime Date)
        {
            const int SECOND = 1;
            const int MINUTE = 60 * SECOND;
            const int HOUR = 60 * MINUTE;
            const int DAY = 24 * HOUR;
            const int MONTH = 30 * DAY;

            var ts = new TimeSpan(DateTime.Now.Ticks - Date.Ticks);
            double delta = Math.Abs(ts.TotalSeconds);

            if (ts.Seconds == 0)
                ts = new TimeSpan(0, 0, 1);

            if (delta < 1 * MINUTE)
                return ts.Seconds == 1 ? " قبل ثانية" : " قبل " + ts.Seconds + " ثواني";

            if (delta < 2 * MINUTE)
                return " قبل دقيقة";

            if (delta < 45 * MINUTE)
                return " قبل " + ts.Minutes + " دقائق";

            if (delta < 90 * MINUTE)
                return " قبل ساعة";

            if (delta < 24 * HOUR)
                return " قبل " + ts.Hours + " ساعات";

            if (delta < 48 * HOUR)
                return " أمس";

            if (delta < 30 * DAY)
                return " قبل " + ts.Days + " أيام";

            if (delta < 12 * MONTH)
            {
                int months = Convert.ToInt32(Math.Floor((double)ts.Days / 30));
                return months <= 1 ? " قبل شهر" : " قبل " + months + " أشهر";
            }
            else
            {
                int years = Convert.ToInt32(Math.Floor((double)ts.Days / 365));
                return years <= 1 ? " قبل سنة" : " قبل " + years + " سنوات";
            }
        }
    }
}
