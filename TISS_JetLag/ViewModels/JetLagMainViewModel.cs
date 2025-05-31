using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TISS_JetLag.ViewModels
{
    #region 主頁
    public class JetLagMainViewModel
    {
        public int TopicID { get; set; }
        public string Title { get; set; }
        public string TravelDescription { get; set; }

        public List<string> Symptoms { get; set; }
        public List<string> Causes { get; set; }
        public List<string> Strategies { get; set; }

        public List<string> JetLagAdjustments { get; set; }
        public List<SunlightAdviceViewModel> SunlightAdvices { get; set; }
        public List<JetLagMainViewModel> TopicList { get; set; }
        public TimeZoneSuggestionViewModel TimeZoneSuggestion { get; set; }
    }
    #endregion

    #region 時差建議
    public class SunlightAdviceViewModel
    {
        public string SunlightDirection { get; set; }
        public string TimePeriod { get; set; }
        public string Advice { get; set; }
    }
    #endregion

    #region 地區時差經緯度
    public class TimeZoneSuggestionViewModel
    {
        public string DestinationCountry { get; set; }
        public string DestinationCity { get; set; }
        public double Longitude { get; set; }
        public int TimeZoneOffset { get; set; }

        public string FlightDirection { get; set; }
        public int TimeDifference { get; set; }
        public int SuggestedDays { get; set; }
    }
    #endregion
}