using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TISS_JetLag.Models;

namespace TISS_JetLag.Controllers
{
    public class TimeZoneSuggestionController : Controller
    {
        private TISS_JetLagSolutionsEntities _db = new TISS_JetLagSolutionsEntities();

        #region 時差地區
        public ActionResult ZoneSugges()
        {
            ViewBag.CountryList = _db.CountryTimeZone
                .Where(c => c.CityName != "台北")
                .Select(c => new SelectListItem
                {
                    Value = c.CountryID.ToString(),
                    Text = c.CountryName + " - " + c.CityName
                }).ToList();

            return View(); // 初始頁面無 model
        }

        [HttpPost]
        public ActionResult ZoneSugges(int CountryID)
        {
            var destination = _db.CountryTimeZone.Find(CountryID);
            var taiwan = _db.CountryTimeZone.FirstOrDefault(c => c.CityName == "台北");

            if (destination == null || taiwan == null)
                return HttpNotFound();

            var vm = new TimeZoneSuggestionViewModel
            {
                DestinationCountry = destination.CountryName,
                DestinationCity = destination.CityName,
                Longitude = destination.Longitude,
                TimeZoneOffset = destination.TimeZoneOffset
            };

            // 飛行方向判斷
            vm.FlightDirection = (destination.Longitude > taiwan.Longitude) ? "向東飛行" : "向西飛行";

            // 計算時差
            vm.TimeDifference = destination.TimeZoneOffset - taiwan.TimeZoneOffset;

            // 建議調整天數
            vm.SuggestedDays = (vm.FlightDirection == "向東飛行")
                ? (int)Math.Ceiling(Math.Abs(vm.TimeDifference) / 1.0)
                : (int)Math.Ceiling(Math.Abs(vm.TimeDifference) / 0.75);

            // 下拉選單也要重新綁定，否則 ViewBag 會是 null
            ViewBag.CountryList = _db.CountryTimeZone
                .Where(c => c.CityName != "台北")
                .Select(c => new SelectListItem
                {
                    Value = c.CountryID.ToString(),
                    Text = c.CountryName + " - " + c.CityName
                }).ToList();

            return View("~/Views/JetLagExplanation/Explanation.cshtml", vm); //將結果傳給 Explanation 頁面
        }
        #endregion
    }
}