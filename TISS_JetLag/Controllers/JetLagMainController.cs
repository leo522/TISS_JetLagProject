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
            var topics = _db.TravelFatigueJetLagTopic.ToList();
            var viewModelList = new List<JetLagMainViewModel>();

            foreach (var topic in topics)
            {
                var vm = new JetLagMainViewModel
                {
                    TopicID = topic.TopicID,
                    Title = topic.Title,
                    TravelDescription = topic.TravelDescription,

                    Symptoms = _db.TopicSymptom
                        .Where(ts => ts.TopicID == topic.TopicID)
                        .Select(ts => ts.Symptom.SymptomName)
                        .ToList(),

                    Causes = _db.Cause
                        .Where(c => c.TopicID == topic.TopicID)
                        .Select(c => c.CauseDescription)
                        .ToList(),

                    Strategies = _db.Strategy
                        .Where(s => s.TopicID == topic.TopicID)
                        .Select(s => s.StrategyContent)
                        .ToList()
                };

                if (topic.Title.Contains("時差"))
                {
                    vm.JetLagAdjustments = _db.JetLagAdjustment
                        .Select(j => j.JetLagDirection + "： " + j.AdjustmentDescription)
                        .ToList();

                    vm.SunlightAdvices = _db.SunlightAdvice
                        .Select(s => new SunlightAdviceViewModel
                        {
                            SunlightDirection = s.SunlightDirection,
                            TimePeriod = s.TimePeriod,
                            Advice = s.Advice
                        })
                        .ToList();
                }

                viewModelList.Add(vm);
            }

            return View(viewModelList);
        }
        #endregion
    }
}