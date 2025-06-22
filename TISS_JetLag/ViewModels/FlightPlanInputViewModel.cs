using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using TISS_JetLag.ViewModels;

namespace TISS_JetLag.ViewModels
{
    #region 航班資訊建議
    public class FlightPlanInputViewModel
    {
        [Required]
        public List<FlightLegViewModel> OutboundLegs { get; set; } = new List<FlightLegViewModel>(); //去程

        [Required]
        public List<FlightLegViewModel> ReturnLegs { get; set; } = new List<FlightLegViewModel>(); //回程

        public string DepartureCity { get; set; }
        public string ArrivalCity { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime DepartureTimeLocal { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime ArrivalTimeLocal { get; set; }

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