using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TFDSolution.Business.Interface;
using TFDSolution.Business;
using TFDSolution.Transport;
using System.ComponentModel.Design;
using TFDSolution.Common;
using TFDSolution.App_Start;

namespace TFDSolution.Controllers
{
    [CustomAuthenticationFilter]
    public class DashboardController : Controller
    {
        public readonly iDashboardBusiness business = new DashboardBusiness();
        public DashboardController()
        {
            business = new DashboardBusiness();
        }
        // GET: Dashboard
        [Route("DashboardPage")]
        public ActionResult Index(string dasbharodName)
        {
            string CompanyId = Convert.ToString(SessionPersister.LoginedUser.CompanyInfo.CompanyId);
            int fyearId = SessionPersister.LoginedUser.CompanyInfo.DefaultFiancialId;
            DashboardModel dashboard = business.GetDashboardData(dasbharodName, CompanyId, fyearId);
            return View(dashboard);
        }
        [HttpGet]
        public JsonResult GetChartData(string period, string sqlScript)
        {
            var result = business.GetChartData(period, sqlScript);

            return Json(new
            {
                IsSuccess = result.IsSuccess,
                Labels = result.Labels,
                Values = result.Values,
                data = result.data,
            }, JsonRequestBehavior.AllowGet);
        }
    }
}