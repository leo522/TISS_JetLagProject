using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using TISS_JetLag.Utility;

namespace TISS_JetLag.ViewModels
{
    #region 多段航班輸入欄位
    public class FlightLegViewModel
    {
        [Required(ErrorMessage = "請選擇出發機場")]
        [StringLength(10)]
        public string DepartureCity { get; set; }

        [Required(ErrorMessage = "請選擇抵達機場")]
        [StringLength(10)]
        public string ArrivalCity { get; set; }

        [Required(ErrorMessage = "請輸入出發時間")]
        [DataType(DataType.DateTime)]
        public DateTime? DepartureTimeLocal { get; set; }

        [Required(ErrorMessage = "請輸入抵達時間")]
        [DataType(DataType.DateTime)]
        public DateTime? ArrivalTimeLocal { get; set; }

        public int DepartureTimeZoneOffset { get; set; }
        public int ArrivalTimeZoneOffset { get; set; }
        public double DepartureLongitude { get; set; }
        public double ArrivalLongitude { get; set; }
    }
    #endregion
}