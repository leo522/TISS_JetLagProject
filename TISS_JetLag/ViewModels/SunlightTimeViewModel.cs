using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TISS_JetLag.ViewModels
{
    #region 日照時間建議
    public class SunlightTimeViewModel
    {
        public DateTime Date { get; set; }
        public string LocationName { get; set; }
        public DateTime SunriseUtc { get; set; }
        public DateTime SunsetUtc { get; set; }
        public DateTime SunriseLocal { get; set; }
        public DateTime SunsetLocal { get; set; }
    }
    #endregion
}