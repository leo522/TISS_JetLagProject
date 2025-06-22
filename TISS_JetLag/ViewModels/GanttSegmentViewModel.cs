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
        public string Color { get; set; } //可根據分類決定，若留空則由系統預設配色
        public string TooltipText { get; set; } //Tooltip 說明文字
        public string Category { get; set; } //類別（如：睡眠、起飛、降落、用餐）
        public string LocationLabel { get; set; } //加上時間地區顯示（例如「台灣時間」）
    }
    #endregion
}