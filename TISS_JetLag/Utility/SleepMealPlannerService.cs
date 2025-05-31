using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TISS_JetLag.ViewModels;

namespace TISS_JetLag.Utility
{
    #region 睡眠及用餐調整計畫
    public static class SleepMealPlannerService
    {
        public static List<SleepMealSegmentViewModel> GenerateInFlightPlan(
            DateTime departureTime, DateTime arrivalTime,
            DateTime arrivalSunriseTime, DateTime arrivalSunsetTime)
        {
            var result = new List<SleepMealSegmentViewModel>();
            var totalFlightMinutes = (int)(arrivalTime - departureTime).TotalMinutes;
            if (totalFlightMinutes <= 0) return result;

            // 改為根據日照時間判斷是否應該提前進入「預備清醒期」
            var bufferBeforeSunrise = 90; // 早上日出前 1.5 小時預醒
            var wakeTarget = arrivalSunriseTime.AddMinutes(-bufferBeforeSunrise);

            // 若抵達時間比預醒時間早，則提前安排醒來；否則靠近降落前
            var wakeStart = (wakeTarget < arrivalTime) ? wakeTarget : arrivalTime.AddMinutes(-45);
            var wakeStartClamp = wakeStart < departureTime ? departureTime.AddMinutes(totalFlightMinutes * 0.7) : wakeStart;

            var mealDuration = 60;
            var sleepStart = departureTime.AddMinutes(mealDuration);
            var sleepEnd = wakeStartClamp;

            result.Add(new SleepMealSegmentViewModel
            {
                TimeRange = $"{departureTime:HH:mm} - {departureTime.AddMinutes(mealDuration):HH:mm}",
                SegmentType = "用餐 / 禁咖啡",
                Description = "上機後立即進餐，避免咖啡因"
            });

            if ((sleepEnd - sleepStart).TotalMinutes > 30)
            {
                result.Add(new SleepMealSegmentViewModel
                {
                    TimeRange = $"{sleepStart:HH:mm} - {sleepEnd:HH:mm}",
                    SegmentType = "睡眠區",
                    Description = "進入深眠期，佩戴眼罩耳塞"
                });
            }

            result.Add(new SleepMealSegmentViewModel
            {
                TimeRange = $"{wakeStartClamp:HH:mm} - {arrivalTime:HH:mm}",
                SegmentType = "清醒 / 曝曬",
                Description = "模擬目的地早晨節律"
            });

            return result;
        }
    }
    #endregion
}