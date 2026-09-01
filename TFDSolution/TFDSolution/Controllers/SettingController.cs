using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TFDSolution.App_Start;
using TFDSolution.Business.Interface;
using TFDSolution.Business;
using System.Web.UI.WebControls;

namespace TFDSolution.Controllers
{
    [CustomAuthenticationFilter]
    public class SettingController : Controller
    {
        private readonly IDynamicallyReportBusiness ReportBusiness;
        public SettingController()
        {
            ReportBusiness = new DynamicallyReportBusiness();
        }
        // GET: Setting
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Users()
        {
            return View();
        }

      
        // GET: DynamicReport
        
        public ActionResult DynamicReport()
        {
            return View("~/Views/DynamicReport/Index.cshtml");
        }
        [HttpGet]
        public ActionResult GetSourceColumns(string tableName)
        {
            var columns = ReportBusiness.GetSourceColumns(tableName);
            return Json(columns, JsonRequestBehavior.AllowGet);
        }

        #region Approvals
        public ActionResult Approvals()
        {
            return View();
        }
        #endregion

    }
}