using System;
using System.Collections.Generic;
using System.Text;

namespace CMC.Kernel.Core.Constants
{
    public static class SystemSettings
    {

        public static string ApplicationName = "CMC";



        //OTP Setting.
        public static string OTPNumberOfDigits = "OTPNumberOfDigits";
        public static string OTPMaxNumberOfTrials = "OTPMaxNumberOfTrials";
        public static string OTPCodeExpiryInMinutes = "OTPCodeExpiryInMinutes";
        public static string OTPElapsedTimeInSecond = "OTPElapsedTimeInSecond";
        public static string OTPMaxNumberOfSendSMS = "OTPMaxNumberOfSendSMS";
        public static string OTPBlockPeriodMinutes = "OTPBlockPeriodMinutes";



    }
}
