using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TISS_JetLag.Models;

namespace TISS_JetLag.Controllers
{
    public class JetLagExplanationController : Controller
    {
        private TISS_JetLagSolutionsEntities _db = new TISS_JetLagSolutionsEntities();

        #region 時差解釋
        public ActionResult Explanation()
        {
            return View();
        }
        #endregion
    }
}