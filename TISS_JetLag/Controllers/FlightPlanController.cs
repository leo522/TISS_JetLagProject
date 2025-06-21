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
            ViewBag.CityList = GetSelectableAirports();
            return View("FlightAdjustmentPlanner", new FlightPlanInputViewModel());
        }
        #endregion

        #region 計算航班時差調整建議
        [HttpPost]
        public async Task<ActionResult> CalculateFlightAdjustment(FlightPlanInputViewModel model)
        {
            var firstLeg = model.OutboundLegs?.FirstOrDefault();
            var lastLeg = model.ReturnLegs?.LastOrDefault();

            if (firstLeg == null || lastLeg == null)
            {
                ModelState.AddModelError("", "請至少輸入一筆去程與回程的航班資料。");
                ViewBag.CityList = GetSelectableAirports();
                return View("FlightAdjustmentPlanner", model);
            }

            var dep = _db.AirportInfo.FirstOrDefault(a => a.AirportCode == firstLeg.DepartureCity);
            var arr = _db.AirportInfo.FirstOrDefault(a => a.AirportCode == lastLeg.ArrivalCity);

            if (dep == null || arr == null)
            {
                ModelState.AddModelError("", "機場資料有誤，請重新選擇。");
                ViewBag.CityList = GetSelectableAirports();
                return View("FlightAdjustmentPlanner", model);
            }

            // 以去程出發地與回程抵達地為主體
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

            var baseSleepTime = new TimeSpan(23, 0, 0);
            var baseWakeTime = new TimeSpan(7, 0, 0);

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

            // 加入去程航段
            foreach (var leg in model.OutboundLegs)
            {
                var depTz = _db.AirportInfo.FirstOrDefault(a => a.AirportCode == leg.DepartureCity)?.TimeZoneOffset ?? 0;
                var arrTz = _db.AirportInfo.FirstOrDefault(a => a.AirportCode == leg.ArrivalCity)?.TimeZoneOffset ?? 0;

                var depUtc = TimeZoneConverter.ToUtc(leg.DepartureTimeLocal, depTz);
                var arrUtc = TimeZoneConverter.ToUtc(leg.ArrivalTimeLocal, arrTz);

                ganttList.Add(new GanttSegmentViewModel
                {
                    Label = $"✈️ [去程] {leg.DepartureCity} → {leg.ArrivalCity}",
                    Start = depUtc,
                    End = arrUtc,
                    Color = "#F5B041",
                    TooltipText = $"✈️ [去程] {leg.DepartureCity} → {leg.ArrivalCity}\n" +
                        $"起飛（當地）：{leg.DepartureTimeLocal:MM/dd HH:mm} (UTC{depTz:+0;-#})\n" +
                        $"抵達（當地）：{leg.ArrivalTimeLocal:MM/dd HH:mm} (UTC{arrTz:+0;-#})\n" +
                        $"顯示時間軸（UTC）：{depUtc:MM/dd HH:mm} → {arrUtc:MM/dd HH:mm}",
                    Category = "航班"
                });
            }


            foreach (var leg in model.ReturnLegs)
            {
                var depTz = _db.AirportInfo.FirstOrDefault(a => a.AirportCode == leg.DepartureCity)?.TimeZoneOffset ?? 0;
                var arrTz = _db.AirportInfo.FirstOrDefault(a => a.AirportCode == leg.ArrivalCity)?.TimeZoneOffset ?? 0;

                var depUtc = TimeZoneConverter.ToUtc(leg.DepartureTimeLocal, depTz);
                var arrUtc = TimeZoneConverter.ToUtc(leg.ArrivalTimeLocal, arrTz);

                ganttList.Add(new GanttSegmentViewModel
                {
                    Label = $"✈️ [回程] {leg.DepartureCity} → {leg.ArrivalCity}",
                    Start = depUtc,
                    End = arrUtc,
                    Color = "#D6A2E8",
                    TooltipText = $"✈️ [回程] {leg.DepartureCity} → {leg.ArrivalCity}\n" +
                        $"起飛（當地）：{leg.DepartureTimeLocal:MM/dd HH:mm} (UTC{depTz:+0;-#})\n" +
                        $"抵達（當地）：{leg.ArrivalTimeLocal:MM/dd HH:mm} (UTC{arrTz:+0;-#})\n" +
                        $"顯示時間軸（UTC）：{depUtc:MM/dd HH:mm} → {arrUtc:MM/dd HH:mm}",
                    Category = "航班"
                });
            }

            // 機上作息，把出發地的時區偏移抓出來做轉換基準
            var depTzOffset = _db.AirportInfo.FirstOrDefault(a => a.AirportCode == model.DepartureCity)?.TimeZoneOffset ?? 0;
            var departureBaseUtc = TimeZoneConverter.ToUtc(model.DepartureTimeLocal, depTzOffset);

            foreach (var seg in model.InFlightSchedule)
            {
                if (seg.SegmentType.Contains("睡眠") && seg.TimeRange.Contains("-"))
                {
                    var parts = seg.TimeRange.Split('-');

                    if (TimeSpan.TryParse(parts[0].Trim(), out var startSpan) &&
                        TimeSpan.TryParse(parts[1].Trim(), out var endSpan))
                    {
                        var startUtc = departureBaseUtc.Date.Add(startSpan);
                        var endUtc = departureBaseUtc.Date.Add(endSpan);

                        ganttList.Add(new GanttSegmentViewModel
                        {
                            Label = "機上睡眠",
                            Start = startUtc,
                            End = endUtc,
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
            return View("FlightAdjustmentPlanner", model);
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
    }
}

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