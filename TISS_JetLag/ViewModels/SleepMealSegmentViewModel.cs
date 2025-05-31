using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TISS_JetLag.ViewModels
{
    #region 睡眠調整建議
    public class SleepMealSegmentViewModel
    {
        public string TimeRange { get; set; }          // 例如：23:25 - 01:30
        public string SegmentType { get; set; }        // 如：用餐、睡眠、清醒
        public string Description { get; set; }        // 顯示用說明
    }
    #endregion
}