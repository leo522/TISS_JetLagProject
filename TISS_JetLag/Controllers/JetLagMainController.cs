using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TISS_JetLag.Models;

namespace TISS_JetLag.Controllers
{
    public class JetLagMainController : Controller
    {
        private TISS_JetLagSolutionsEntities _db = new TISS_JetLagSolutionsEntities();

        #region 旅行疲勞與時差對策主頁
        public ActionResult JetLagSolutions()
        {
            //var topics = _db.TravelFatigueJetLagTopic.ToList();
            //var viewModelList = new List<JetLagMainViewModel>();

            //foreach (var topic in topics)
            //{
            //    var vm = new JetLagMainViewModel
            //    {
            //        TopicID = topic.TopicID,
            //        Title = topic.Title,
            //        TravelDescription = topic.TravelDescription,

            //        Symptoms = _db.TopicSymptom
            //            .Where(ts => ts.TopicID == topic.TopicID)
            //            .Select(ts => ts.Symptom.SymptomName)
            //            .ToList(),

            //        Causes = _db.Cause
            //            .Where(c => c.TopicID == topic.TopicID)
            //            .Select(c => c.CauseDescription)
            //            .ToList(),

            //        Strategies = _db.Strategy
            //            .Where(s => s.TopicID == topic.TopicID)
            //            .Select(s => s.StrategyContent)
            //            .ToList()
            //    };

            //    if (topic.Title.Contains("時差"))
            //    {
            //        vm.JetLagAdjustments = _db.JetLagAdjustment
            //            .Select(j => j.JetLagDirection + "： " + j.AdjustmentDescription)
            //            .ToList();

            //        vm.SunlightAdvices = _db.SunlightAdvice
            //            .Select(s => new SunlightAdviceViewModel
            //            {
            //                SunlightDirection = s.SunlightDirection,
            //                TimePeriod = s.TimePeriod,
            //                Advice = s.Advice
            //            })
            //            .ToList();
            //    }

            //    viewModelList.Add(vm);
            //}

            //return View(viewModelList);



            var vm = new JetLagMainViewModel();
            vm.TopicList = GetJetLagTopics();

            ViewBag.CountryList = _db.CountryTimeZone
                .Where(c => c.CityName != "台北")
                .Select(c => new SelectListItem
                {
                    Value = c.CountryID.ToString(),
                    Text = c.CountryName + " - " + c.CityName
                }).ToList();

            return View(vm);
        }

        [HttpPost]
        public ActionResult JetLagSolutions(int CountryID)
        {
            var destination = _db.CountryTimeZone.Find(CountryID);
            var taiwan = _db.CountryTimeZone.FirstOrDefault(c => c.CityName == "台北");

            var suggestion = new TimeZoneSuggestionViewModel
            {
                DestinationCountry = destination.CountryName,
                DestinationCity = destination.CityName,
                Longitude = destination.Longitude,
                TimeZoneOffset = destination.TimeZoneOffset,
                FlightDirection = (destination.Longitude > taiwan.Longitude) ? "向東飛行" : "向西飛行",
            };
            suggestion.TimeDifference = destination.TimeZoneOffset - taiwan.TimeZoneOffset;
            suggestion.SuggestedDays = (suggestion.FlightDirection == "向東飛行")
                ? (int)Math.Ceiling(Math.Abs(suggestion.TimeDifference) / 1.0)
                : (int)Math.Ceiling(Math.Abs(suggestion.TimeDifference) / 0.75);

            var vm = new JetLagMainViewModel
            {
                TopicList = GetJetLagTopics(),
                TimeZoneSuggestion = suggestion
            };

            ViewBag.CountryList = _db.CountryTimeZone
                .Where(c => c.CityName != "台北")
                .Select(c => new SelectListItem
                {
                    Value = c.CountryID.ToString(),
                    Text = c.CountryName + " - " + c.CityName
                }).ToList();

            return View(vm);
        }
        #endregion


        #region 時差地區
        private List<JetLagMainViewModel> GetJetLagTopics()
        {
            var topics = _db.TravelFatigueJetLagTopic.ToList();
            var result = new List<JetLagMainViewModel>();

            foreach (var topic in topics)
            {
                var vm = new JetLagMainViewModel
                {
                    TopicID = topic.TopicID,
                    Title = topic.Title,
                    TravelDescription = topic.TravelDescription,
                    Symptoms = _db.TopicSymptom.Where(ts => ts.TopicID == topic.TopicID).Select(ts => ts.Symptom.SymptomName).ToList(),
                    Causes = _db.Cause.Where(c => c.TopicID == topic.TopicID).Select(c => c.CauseDescription).ToList(),
                    Strategies = _db.Strategy.Where(s => s.TopicID == topic.TopicID).Select(s => s.StrategyContent).ToList()
                };

                if (topic.Title.Contains("時差"))
                {
                    vm.JetLagAdjustments = _db.JetLagAdjustment.Select(j => j.JetLagDirection + "： " + j.AdjustmentDescription).ToList();
                    vm.SunlightAdvices = _db.SunlightAdvice.Select(s => new SunlightAdviceViewModel
                    {
                        SunlightDirection = s.SunlightDirection,
                        TimePeriod = s.TimePeriod,
                        Advice = s.Advice
                    }).ToList();
                }

                result.Add(vm);
            }

            return result;
        }
        //public ActionResult ZoneSugges()
        //{
        //    ViewBag.CountryList = _db.CountryTimeZone
        //        .Where(c => c.CityName != "台北")
        //        .Select(c => new SelectListItem
        //        {
        //            Value = c.CountryID.ToString(),
        //            Text = c.CountryName + " - " + c.CityName
        //        }).ToList();

        //    return View(); // 初始頁面無 model
        //}

        //[HttpPost]
        //public ActionResult ZoneSugges(int CountryID)
        //{
        //    var destination = _db.CountryTimeZone.Find(CountryID);
        //    var taiwan = _db.CountryTimeZone.FirstOrDefault(c => c.CityName == "台北");

        //    if (destination == null || taiwan == null)
        //        return HttpNotFound();

        //    var vm = new TimeZoneSuggestionViewModel
        //    {
        //        DestinationCountry = destination.CountryName,
        //        DestinationCity = destination.CityName,
        //        Longitude = destination.Longitude,
        //        TimeZoneOffset = destination.TimeZoneOffset
        //    };

        //    // 飛行方向判斷
        //    vm.FlightDirection = (destination.Longitude > taiwan.Longitude) ? "向東飛行" : "向西飛行";

        //    // 計算時差
        //    vm.TimeDifference = destination.TimeZoneOffset - taiwan.TimeZoneOffset;

        //    // 建議調整天數
        //    vm.SuggestedDays = (vm.FlightDirection == "向東飛行")
        //        ? (int)Math.Ceiling(Math.Abs(vm.TimeDifference) / 1.0)
        //        : (int)Math.Ceiling(Math.Abs(vm.TimeDifference) / 0.75);

        //    // 下拉選單也要重新綁定，否則 ViewBag 會是 null
        //    ViewBag.CountryList = _db.CountryTimeZone
        //        .Where(c => c.CityName != "台北")
        //        .Select(c => new SelectListItem
        //        {
        //            Value = c.CountryID.ToString(),
        //            Text = c.CountryName + " - " + c.CityName
        //        }).ToList();

        //    return View("~/Views/JetLagExplanation/Explanation.cshtml", vm); //將結果傳給 Explanation 頁面
        //}
        #endregion
    }
}