using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TISS_JetLag.ViewModels
{
    #region 作息調整建議
    public class DailyAdjustmentViewModel
    {
        public int DayIndex { get; set; } // 調整第幾天（Day -3 / -2...）
        public string SuggestedSleepTime { get; set; }
        public string SuggestedWakeTime { get; set; }
        public string AdjustmentNote { get; set; } // 如：提早 30 分鐘
    }
    #endregion
}