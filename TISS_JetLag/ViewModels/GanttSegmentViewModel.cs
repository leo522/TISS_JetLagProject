using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TISS_JetLag.ViewModels
{
    #region 甘特圖
    public class GanttSegmentViewModel
    {
        public string Label { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string Color { get; set; } //可改為根據分類決定
        public string TooltipText { get; set; } //新增 tooltip 說明文字
        public string Category { get; set; } //新增類別（如：起床/睡眠/登機）
    }
    #endregion
}