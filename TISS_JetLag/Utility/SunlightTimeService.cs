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
        public static async Task<SunlightTimeViewModel> GetSunlightTimeAsync(string cityName, double latitude, double longitude, int timeZoneOffset, DateTime targetDate)
        {
            var client = new HttpClient();
            var url = $"https://api.sunrise-sunset.org/json?lat={latitude}&lng={longitude}&date={targetDate:yyyy-MM-dd}&formatted=0";

            var response = await client.GetStringAsync(url);
            var json = JObject.Parse(response)["results"];

            var sunriseUtc = DateTime.Parse(json["sunrise"].ToString()).ToUniversalTime();
            var sunsetUtc = DateTime.Parse(json["sunset"].ToString()).ToUniversalTime();

            return new SunlightTimeViewModel
            {
                Date = targetDate,
                LocationName = cityName,
                SunriseUtc = sunriseUtc,
                SunsetUtc = sunsetUtc,
                SunriseLocal = sunriseUtc.AddHours(timeZoneOffset),
                SunsetLocal = sunsetUtc.AddHours(timeZoneOffset)
            };
        }
    }
    #endregion
}