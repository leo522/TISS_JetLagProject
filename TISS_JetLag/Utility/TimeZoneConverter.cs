using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TISS_JetLag.Utility
{
    #region UTC時區轉換
    public class TimeZoneConverter
    {
        public static DateTime ToUtc(DateTime localTime, int timeZoneOffset)
        {
            return localTime.AddHours(-timeZoneOffset);
        }

        public static DateTime FromUtc(DateTime utcTime, int timeZoneOffset)
        {
            return utcTime.AddHours(timeZoneOffset);
        }
    }
    #endregion
}