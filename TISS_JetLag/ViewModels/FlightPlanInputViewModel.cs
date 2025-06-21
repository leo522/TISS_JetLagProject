using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TISS_JetLag.ViewModels
{
    #region 航班資訊建議
    public class FlightPlanInputViewModel
    {
        // 分開管理去程與回程航段
        public List<FlightLegViewModel> OutboundLegs { get; set; } = new List<FlightLegViewModel>();
        public List<FlightLegViewModel> ReturnLegs { get; set; } = new List<FlightLegViewModel>();

        //舊欄位保留 (僅供預設單段用，不再直接運算)
        public string DepartureCity { get; set; }
        public string ArrivalCity { get; set; }
        public DateTime DepartureTimeLocal { get; set; }
        public DateTime ArrivalTimeLocal { get; set; }

        // 以下保留原有欄位
        public int DepartureTimeZoneOffset { get; set; }
        public int ArrivalTimeZoneOffset { get; set; }
        public double DepartureLongitude { get; set; }
        public double ArrivalLongitude { get; set; }

        public string FlightDirection { get; set; }
        public int TimeDifference { get; set; }
        public int SuggestedAdjustmentDays { get; set; }

        public string ResultMessage { get; set; }

        public SunlightTimeViewModel DepartureSunlight { get; set; }
        public SunlightTimeViewModel ArrivalSunlight { get; set; }
        public List<DailyAdjustmentViewModel> SleepAdjustmentSchedule { get; set; } = new List<DailyAdjustmentViewModel>();
        public List<SleepMealSegmentViewModel> InFlightSchedule { get; set; } = new List<SleepMealSegmentViewModel>();
        public List<GanttSegmentViewModel> GanttSchedule { get; set; } = new List<GanttSegmentViewModel>();
    }
    #endregion
}