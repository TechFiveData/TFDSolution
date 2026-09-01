using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TFDSolution.Business;
using TFDSolution.Transport.Common;
using TFDSolution.Transport.Master;

namespace TFDSolution.App_Start.Filters
{
    public class AuditLogAttribute : ActionFilterAttribute
    {

        public string PageName { get; set; }
        public string ActionName { get; set; }
        public string Remark { get; set; }

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            try
            {
                var request = filterContext.HttpContext.Request;
                var session = filterContext.HttpContext.Session;
                LoginUserInfo loginUser = (LoginUserInfo)session["LoginUserInfo"];
                 
                var userId = loginUser?.UserId != null ? (Guid)loginUser.UserId : Guid.Empty;
                var companyId = loginUser?.CompanyInfo?.CompanyId != null ? (Guid)loginUser.CompanyInfo.CompanyId : Guid.Empty;
                var financialYearId = loginUser?.CompanyInfo?.DefaultFiancialId != null ? (int)loginUser.CompanyInfo.DefaultFiancialId :0;
                var ipAddress = filterContext.HttpContext.Request.UserHostAddress;

                var page = PageName ?? filterContext.ActionDescriptor.ControllerDescriptor.ControllerName+"/"+ ActionName ?? filterContext.ActionDescriptor.ActionName;
                var action = request.RequestType + ":" + request.RawUrl;
                var remark = Remark ?? "Auto Log";
                var log = new AuditLog
                {
                    LogDateTimeStamp = DateTime.Now,
                    UserId = userId,
                    CompanyId = companyId,
                    FinancialYearId = financialYearId,
                    PageName = page,
                    Action = action,
                    RecordId = string.Empty,
                    Remark = remark,
                    IPAddress = ipAddress
                };

                //CommonBusiness.SaveAuditLog(log);

            }
            catch
            {
                // Avoid throwing exception from logging
            }

            base.OnActionExecuted(filterContext);
        }

    }
}