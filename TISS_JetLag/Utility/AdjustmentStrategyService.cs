using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TISS_JetLag.ViewModels;

namespace TISS_JetLag.Utility
{
    #region 作息調整建議模組
    public static class AdjustmentStrategyService
    {
        public static List<DailyAdjustmentViewModel> GenerateSleepAdjustmentPlan(
            string flightDirection, int timeDifference, int adjustmentDays,
            TimeSpan baseSleepTime, TimeSpan baseWakeTime,
            TimeSpan targetSunrise, TimeSpan targetSunset)
        {
            var result = new List<DailyAdjustmentViewModel>();
            var shiftMinutes = (int)(timeDifference * 60.0 / adjustmentDays);

            for (int i = 0; i < adjustmentDays; i++)
            {
                var offset = TimeSpan.FromMinutes(shiftMinutes * (i + 1));

                var newSleep = flightDirection == "向東飛行"
                    ? baseSleepTime - offset
                    : baseSleepTime + offset;

                var newWake = flightDirection == "向東飛行"
                    ? baseWakeTime - offset
                    : baseWakeTime + offset;

                // 限制睡眠時間不要超出日落後、也不要比日出早醒
                var adjustedSleep = (newSleep < targetSunset) ? newSleep : targetSunset;
                var adjustedWake = (newWake > targetSunrise) ? newWake : targetSunrise;

                result.Add(new DailyAdjustmentViewModel
                {
                    DayIndex = -(adjustmentDays - i),
                    SuggestedSleepTime = adjustedSleep.ToString(@"hh\:mm"),
                    SuggestedWakeTime = adjustedWake.ToString(@"hh\:mm"),
                    AdjustmentNote = $"每日調整 {(shiftMinutes > 0 ? "+" : "-")}{Math.Abs(shiftMinutes)} 分鐘"
                });
            }

            return result;
        }
    }
    #endregion
}