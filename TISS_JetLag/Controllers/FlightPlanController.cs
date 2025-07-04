﻿using Newtonsoft.Json;
using System;
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
            var cityList = GetSelectableAirports();
            ViewBag.CityList = cityList;
            ViewBag.CityListJson = JsonConvert.SerializeObject(cityList);
            return View("FlightAdjustmentPlanner", new FlightPlanInputViewModel());
        }
        #endregion

        #region 計算航班時差調整建議
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CalculateFlightAdjustment(FlightPlanInputViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    ModelState.AddModelError("", "表單驗證失敗，請確認輸入欄位。");
                    return ReturnFormWithCityList(model);
                }

                if ((model.OutboundLegs?.Count ?? 0) + (model.ReturnLegs?.Count ?? 0) > 5)
                {
                    ModelState.AddModelError("", "航段總數不可超過 5 段。");
                    return ReturnFormWithCityList(model);
                }

                var airportDict = _db.AirportInfo.ToDictionary(a => a.AirportCode);

                var firstLeg = model.OutboundLegs?.FirstOrDefault();
                var lastLeg = model.ReturnLegs?.LastOrDefault() ?? model.OutboundLegs?.LastOrDefault();

                if (firstLeg == null || lastLeg == null)
                {
                    ModelState.AddModelError("", "請至少輸入一筆有效的航班資料。");
                    return ReturnFormWithCityList(model);
                }

                if (!airportDict.TryGetValue(firstLeg.DepartureCity, out var dep))
                {
                    ModelState.AddModelError("", $"出發機場代碼錯誤：{firstLeg.DepartureCity}");
                    return ReturnFormWithCityList(model);
                }

                if (!airportDict.TryGetValue(lastLeg.ArrivalCity, out var arr))
                {
                    ModelState.AddModelError("", $"抵達機場代碼錯誤：{lastLeg.ArrivalCity}");
                    return ReturnFormWithCityList(model);
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

                // 使用者輸入作息時間（有值則用，否則預設）
                var baseSleepTime = model.UserSleepTime ?? new TimeSpan(23, 0, 0);
                var baseWakeTime = model.UserWakeTime ?? new TimeSpan(7, 0, 0);

                model.ResultMessage = $"您從 {model.DepartureCity} 飛往 {model.ArrivalCity}，屬於【{model.FlightDirection}】，" + $"時差為 {model.TimeDifference} 小時，建議提前調整 {model.SuggestedAdjustmentDays} 天作息。" +$"（依您平常作息：{baseSleepTime:hh\\:mm} - {baseWakeTime:hh\\:mm}）";

                var today = DateTime.Today;

                model.DepartureSunlight = await SunlightTimeService.GetSunlightTimeAsync(
                    model.DepartureCity, dep.Latitude, dep.Longitude, dep.TimeZoneOffset, today);

                model.ArrivalSunlight = await SunlightTimeService.GetSunlightTimeAsync(
                    model.ArrivalCity, arr.Latitude, arr.Longitude, arr.TimeZoneOffset, today.AddDays(1));

                // 若查詢日出日落失敗，則以 6:00 / 18:00 為預設時間
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

                void AddFlightLegs(List<FlightLegViewModel> legs, string labelPrefix, string color)
                {
                    foreach (var leg in legs)
                    {
                        if (!airportDict.TryGetValue(leg.DepartureCity, out var depInfo) ||
                            !airportDict.TryGetValue(leg.ArrivalCity, out var arrInfo))
                            continue;

                        var depUtc = TimeZoneConverter.ToUtc(leg.DepartureTimeLocal, depInfo.TimeZoneOffset);
                        var arrUtc = TimeZoneConverter.ToUtc(leg.ArrivalTimeLocal, arrInfo.TimeZoneOffset);

                        ganttList.Add(new GanttSegmentViewModel
                        {
                            Label = $"{labelPrefix} {leg.DepartureCity} → {leg.ArrivalCity}",
                            Start = depUtc,
                            End = arrUtc,
                            Color = color,
                            TooltipText = $"{labelPrefix} {leg.DepartureCity} → {leg.ArrivalCity}\n" + $"起飛（當地）：{leg.DepartureTimeLocal:MM/dd HH:mm} (UTC{depInfo.TimeZoneOffset:+0;-#})\n" + $"抵達（當地）：{leg.ArrivalTimeLocal:MM/dd HH:mm} (UTC{arrInfo.TimeZoneOffset:+0;-#})\n" + $"顯示時間軸（UTC）：{depUtc:MM/dd HH:mm} → {arrUtc:MM/dd HH:mm}",
                            Category = "航班"
                        });
                    }
                }

                AddFlightLegs(model.OutboundLegs, "✈️ [去程]", "#F5B041");
                AddFlightLegs(model.ReturnLegs, "✈️ [回程]", "#D6A2E8");

                var depTzOffset = dep.TimeZoneOffset;
                var departureBaseUtc = TimeZoneConverter.ToUtc(model.DepartureTimeLocal, depTzOffset);

                foreach (var seg in model.InFlightSchedule)
                {
                    if (seg.SegmentType.Contains("睡眠") && seg.TimeRange.Contains("-"))
                    {
                        var parts = seg.TimeRange.Split('-');
                        if (TimeSpan.TryParse(parts[0].Trim(), out var startSpan) &&
                            TimeSpan.TryParse(parts[1].Trim(), out var endSpan))
                        {
                            ganttList.Add(new GanttSegmentViewModel
                            {
                                Label = "機上睡眠",
                                Start = departureBaseUtc.Date.Add(startSpan),
                                End = departureBaseUtc.Date.Add(endSpan),
                                Color = "#A9CCE3",
                                TooltipText = seg.Description,
                                Category = "機上作息"
                            });
                        }
                    }
                }

                model.GanttSchedule = ganttList;

                if (ganttList.Any())
                {
                    var minTime = ganttList.Min(g => g.Start).AddHours(-2);
                    var maxTime = ganttList.Max(g => g.End).AddHours(2);
                    ViewBag.GanttMin = minTime.ToString("yyyy-MM-ddTHH:mm:ss");
                    ViewBag.GanttMax = maxTime.ToString("yyyy-MM-ddTHH:mm:ss");
                }

                ViewBag.CityList = GetSelectableAirports();
                ViewBag.CityListJson = JsonConvert.SerializeObject(ViewBag.CityList);
                TempData["DebugInfo"] = $"時差：{model.TimeDifference}，方向：{model.FlightDirection}，航段數：{(model.OutboundLegs?.Count ?? 0) + (model.ReturnLegs?.Count ?? 0)}，甘特圖數量：{model.GanttSchedule?.Count ?? 0}";

                return View("FlightAdjustmentPlanner", model);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
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

        #region 錯誤時重回表單的共用方法
        private ActionResult ReturnFormWithCityList(FlightPlanInputViewModel model)
        {
            var list = GetSelectableAirports();
            ViewBag.CityList = list;
            ViewBag.CityListJson = JsonConvert.SerializeObject(list);
            return View("FlightAdjustmentPlanner", model);
        }
        #endregion
    }
}