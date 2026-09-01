using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TFDSolution.App_Start;
using TFDSolution.Business;
using TFDSolution.Business.Interface;

namespace TFDSolution.Controllers
{
    [CustomAuthenticationFilter]
    public class DynamicReportController : BaseController
    {
        private readonly IDynamicallyReportBusiness ReportBusiness;
        // GET: DynamicReport
        public DynamicReportController()
        {
            ReportBusiness = new DynamicallyReportBusiness();
        }
        public ActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public ActionResult GetSourceColumns(string tableName)
        {
            var columns = ReportBusiness.GetSourceColumns(tableName);
            return Json(columns, JsonRequestBehavior.AllowGet);
        }
    }
}