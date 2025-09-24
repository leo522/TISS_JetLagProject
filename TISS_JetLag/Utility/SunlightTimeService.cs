using Innovative.SolarCalculator;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using TISS_JetLag.ViewModels;

namespace TISS_JetLag.Utility
{
    #region 日照時區服務
    public static class SunlightTimeService
    {
        public static Task<SunlightTimeViewModel> GetSunlightTimeAsync(string cityName, double latitude, double longitude, string timeZoneId, DateTime targetDate)
        {
            var solar = new SolarTimes(targetDate, latitude, longitude);
            var sunriseUtc = solar.Sunrise;
            var sunsetUtc = solar.Sunset;

            var tz = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);

            return Task.FromResult(new SunlightTimeViewModel
            {
                Date = targetDate,
                LocationName = cityName,
                SunriseUtc = sunriseUtc,
                SunsetUtc = sunsetUtc,
                SunriseLocal = TimeZoneInfo.ConvertTimeFromUtc(sunriseUtc, tz),
                SunsetLocal = TimeZoneInfo.ConvertTimeFromUtc(sunsetUtc, tz)
            });
        }
    }
    #endregion
}