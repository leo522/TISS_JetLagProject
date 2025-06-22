using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using TISS_JetLag.ViewModels;

namespace TISS_JetLag.Utility
{
    #region 驗證抵達時間不得早於出發時間
    public static class FlightValidator
    {
        public static ValidationResult ValidateTimeOrder(DateTime arrivalTime, ValidationContext context)
        {
            var instance = context.ObjectInstance as FlightLegViewModel;
            if (instance == null)
                return ValidationResult.Success;

            if (arrivalTime < instance.DepartureTimeLocal)
            {
                return new ValidationResult("抵達時間不得早於出發時間");
            }

            return ValidationResult.Success;
        }
    }
    #endregion
}