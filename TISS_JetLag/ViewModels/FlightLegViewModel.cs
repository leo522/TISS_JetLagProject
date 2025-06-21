using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TISS_JetLag.ViewModels
{
    #region 多段航班輸入欄位
    public class FlightLegViewModel
    {
        public string DepartureCity { get; set; }
        public string ArrivalCity { get; set; }
        public DateTime DepartureTimeLocal { get; set; }
        public DateTime ArrivalTimeLocal { get; set; }

        public int DepartureTimeZoneOffset { get; set; }
        public int ArrivalTimeZoneOffset { get; set; }
        public double DepartureLongitude { get; set; }
        public double ArrivalLongitude { get; set; }
    }
    #endregion
}