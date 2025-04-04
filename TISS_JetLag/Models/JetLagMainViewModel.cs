using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TISS_JetLag.Models
{
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
    }

    public class SunlightAdviceViewModel
    {
        public string SunlightDirection { get; set; }
        public string TimePeriod { get; set; }
        public string Advice { get; set; }
    }
}