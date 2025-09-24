using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TISS_JetLag.Utility
{
    #region UTC時區轉換
    public class TimeZoneConverter
    {
        public static DateTime ToUtc(DateTime localTime, string windowsTimeZoneId)
        {
            try
            {
                var tz = TimeZoneInfo.FindSystemTimeZoneById(windowsTimeZoneId);

                // 取得 offset，先判斷轉換會不會超出範圍
                var offset = tz.GetUtcOffset(localTime);

                var safeUtc = localTime - offset;

                if (safeUtc < DateTime.MinValue)
                    return DateTime.MinValue;
                if (safeUtc > DateTime.MaxValue)
                    return DateTime.MaxValue;

                return TimeZoneInfo.ConvertTimeToUtc(localTime, tz);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"轉換至 UTC 失敗，原始時間：{localTime}, 時區：{windowsTimeZoneId}", ex);
            }
        }

        public static DateTime FromUtc(DateTime utcTime, string windowsTimeZoneId)
        {
            var tz = TimeZoneInfo.FindSystemTimeZoneById(windowsTimeZoneId);
            return TimeZoneInfo.ConvertTimeFromUtc(utcTime, tz);
        }
    }
    #endregion

    #region 判斷 DST 狀態
    public class DstHelper
    {
        public static bool IsCurrentlyInDaylightSaving(string windowsTimeZoneId, DateTime utcDateTime)
        {
            try
            {
                TimeZoneInfo tz = TimeZoneInfo.FindSystemTimeZoneById(windowsTimeZoneId);
                DateTime localTime = TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, tz);
                return tz.IsDaylightSavingTime(localTime);
            }
            catch (TimeZoneNotFoundException)
            {
                return false;
            }
        }

        public static TimeSpan GetCurrentOffset(string windowsTimeZoneId, DateTime utcDateTime)
        {
            try
            {
                TimeZoneInfo tz = TimeZoneInfo.FindSystemTimeZoneById(windowsTimeZoneId);
                return tz.GetUtcOffset(utcDateTime);
            }
            catch
            {
                return TimeSpan.Zero;
            }
        }
    }
    #endregion
}