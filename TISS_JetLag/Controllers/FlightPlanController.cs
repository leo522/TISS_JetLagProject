﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using TISS_JetLag.Models;
using TISS_JetLag.Utility;
using TISS_JetLag.ViewModels;

namespace TISS_JetLag.Controllers
{
    public class FlightPlanController : Controller
    {
        private TISS_JetLagSolutionsEntities _db = new TISS_JetLagSolutionsEntities();

        #region 航班時差調整主頁
        public ActionResult FlightAdjustmentPlanner()
        {
            ViewBag.CityList = GetSelectableAirports();
            return View("FlightAdjustmentPlanner", new FlightPlanInputViewModel());
        }
        #endregion

        #region 計算航班時差調整建議
        [HttpPost]
        public async Task<ActionResult> CalculateFlightAdjustment(FlightPlanInputViewModel model)
        {
            var firstLeg = model.FlightLegs?.FirstOrDefault();
            var lastLeg = model.FlightLegs?.LastOrDefault();

            if (firstLeg == null || lastLeg == null)
            {
                ModelState.AddModelError("", "請至少輸入一筆航班資料。");
                ViewBag.CityList = GetSelectableAirports();
                return View("FlightAdjustmentPlanner", model);
            }

            //城市為主
            //var dep = _db.CountryTimeZone.FirstOrDefault(c => c.CityName == firstLeg.DepartureCity);
            //var arr = _db.CountryTimeZone.FirstOrDefault(c => c.CityName == lastLeg.ArrivalCity);

            // 使用機場資料庫來獲取城市資訊
            var dep = _db.AirportInfo.FirstOrDefault(a => a.AirportCode == firstLeg.DepartureCity);
            var arr = _db.AirportInfo.FirstOrDefault(a => a.AirportCode == lastLeg.ArrivalCity);

            if (dep == null || arr == null)
            {
                ModelState.AddModelError("", "機場資料有誤，請重新選擇。");
                ViewBag.CityList = GetSelectableAirports();
                return View("FlightAdjustmentPlanner", model);
            }

            model.DepartureCity = firstLeg.DepartureCity;
            model.ArrivalCity = lastLeg.ArrivalCity;
            model.DepartureTimeLocal = firstLeg.DepartureTimeLocal;
            model.ArrivalTimeLocal = lastLeg.ArrivalTimeLocal;
            model.DepartureTimeZoneOffset = dep.TimeZoneOffset;
            model.ArrivalTimeZoneOffset = arr.TimeZoneOffset;
            model.DepartureLongitude = dep.Longitude;
            model.ArrivalLongitude = arr.Longitude;

            model.FlightDirection = (arr.Longitude > dep.Longitude) ? "向東飛行" : "向西飛行";
            model.TimeDifference = arr.TimeZoneOffset - dep.TimeZoneOffset;

            model.SuggestedAdjustmentDays = (model.FlightDirection == "向東飛行")
                ? (int)Math.Ceiling(Math.Abs(model.TimeDifference) / 1.0)
                : (int)Math.Ceiling(Math.Abs(model.TimeDifference) / 0.75);

            model.ResultMessage = $"您從 {model.DepartureCity} 飛往 {model.ArrivalCity}，屬於【{model.FlightDirection}】，" +
                                  $"時差為 {model.TimeDifference} 小時，建議提前調整 {model.SuggestedAdjustmentDays} 天作息。";

            var today = DateTime.Today;

            model.DepartureSunlight = await SunlightTimeService.GetSunlightTimeAsync(
             model.DepartureCity, dep.Latitude, dep.Longitude, dep.TimeZoneOffset, today);

            model.ArrivalSunlight = await SunlightTimeService.GetSunlightTimeAsync(
                model.ArrivalCity, arr.Latitude, arr.Longitude, arr.TimeZoneOffset, today.AddDays(1));

            // 基本作息時間
            var baseSleepTime = new TimeSpan(23, 0, 0);
            var baseWakeTime = new TimeSpan(7, 0, 0);

            // 日出日落時間參數（提供給調整與機上作息）
            var sunriseTime = model.ArrivalSunlight?.SunriseLocal.TimeOfDay ?? new TimeSpan(6, 0, 0);
            var sunsetTime = model.ArrivalSunlight?.SunsetLocal.TimeOfDay ?? new TimeSpan(18, 0, 0);

            model.SleepAdjustmentSchedule = AdjustmentStrategyService.GenerateSleepAdjustmentPlan(
                model.FlightDirection,
                model.TimeDifference,
                model.SuggestedAdjustmentDays,
                baseSleepTime,
                baseWakeTime,
                sunriseTime,
                sunsetTime);

            model.InFlightSchedule = SleepMealPlannerService.GenerateInFlightPlan(
                model.DepartureTimeLocal,
                model.ArrivalTimeLocal,
                model.ArrivalSunlight?.SunriseLocal ?? model.ArrivalTimeLocal.AddHours(2),
                model.ArrivalSunlight?.SunsetLocal ?? model.ArrivalTimeLocal.AddHours(10));

            // 建立甘特圖
            var ganttList = new List<GanttSegmentViewModel>();
            var baseDate = model.DepartureTimeLocal.Date.AddDays(-model.SuggestedAdjustmentDays);

            foreach (var sleep in model.SleepAdjustmentSchedule)
            {
                if (TimeSpan.TryParse(sleep.SuggestedSleepTime, out var sleepTime) &&
                    TimeSpan.TryParse(sleep.SuggestedWakeTime, out var wakeTime))
                {
                    var day = baseDate.AddDays(sleep.DayIndex);
                    ganttList.Add(new GanttSegmentViewModel
                    {
                        Label = $"作息調整：Day {sleep.DayIndex}",
                        Start = day.Add(sleepTime),
                        End = day.Add(wakeTime),
                        Color = "#7FB3D5",
                        TooltipText = $"建議睡眠時間：{sleep.SuggestedSleepTime} - {sleep.SuggestedWakeTime}",
                        Category = "作息調整"
                    });
                }
            }

            foreach (var leg in model.FlightLegs)
            {
                ganttList.Add(new GanttSegmentViewModel
                {
                    Label = $"✈️ {leg.DepartureCity} → {leg.ArrivalCity}",
                    Start = leg.DepartureTimeLocal,
                    End = leg.ArrivalTimeLocal,
                    Color = "#F5B041",
                    TooltipText = $"起飛：{leg.DepartureTimeLocal:MM/dd HH:mm}，抵達：{leg.ArrivalTimeLocal:MM/dd HH:mm}",
                    Category = "航班"
                });
            }

            foreach (var seg in model.InFlightSchedule)
            {
                if (seg.SegmentType.Contains("睡眠") && seg.TimeRange.Contains("-"))
                {
                    var parts = seg.TimeRange.Split('-');
                    if (DateTime.TryParse($"{model.DepartureTimeLocal.Date:yyyy-MM-dd} {parts[0].Trim()}", out var start) &&
                        DateTime.TryParse($"{model.DepartureTimeLocal.Date:yyyy-MM-dd} {parts[1].Trim()}", out var end))
                    {
                        ganttList.Add(new GanttSegmentViewModel
                        {
                            Label = "機上睡眠",
                            Start = start,
                            End = end,
                            Color = "#A9CCE3",
                            TooltipText = seg.Description,
                            Category = "機上作息"
                        });
                    }
                }
            }

            model.GanttSchedule = ganttList;

            //計算甘特圖時間軸範圍
            if (ganttList.Any())
            {
                var minTime = ganttList.Min(g => g.Start).AddHours(-2);
                var maxTime = ganttList.Max(g => g.End).AddHours(2);
                ViewBag.GanttMin = minTime.ToString("yyyy-MM-ddTHH:mm:ss");
                ViewBag.GanttMax = maxTime.ToString("yyyy-MM-ddTHH:mm:ss");
            }

            ViewBag.CityList = GetSelectableAirports();
            return View("FlightAdjustmentPlanner", model);
        }
        #endregion

        #region 城市清單（下拉用）
        //private List<SelectListItem> GetSelectableCities()
        //{
        //    return _db.CountryTimeZone.OrderBy(c => c.CountryName).ToList()
        //    .Select(c => new SelectListItem
        //    {
        //        Value = c.CityName,
        //        Text = $"{c.CountryName} - {c.CityName}" // C# 字串插值可正常執行
        //    }).ToList();
        //}

        #endregion

        #region 機場清單（下拉用）
        private List<SelectListItem> GetSelectableAirports()
        {
            return _db.AirportInfo.OrderBy(a => a.CountryName).ThenBy(a => a.CityName).ToList()
                .Select(a => new SelectListItem
                {
                    Value = a.AirportCode,
                    Text = $"{a.CountryName} - {a.CityName} ({a.AirportCode}) - {a.AirportName}"
                }).ToList();
        }
        #endregion
    }
}