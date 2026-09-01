using CrystalDecisions.ReportAppServer;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing.Design;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using TFDSolution.App_Start;
using TFDSolution.Business;
using TFDSolution.Business.Interface;
using TFDSolution.Common;
using TFDSolution.Models;
using TFDSolution.Transport;
using TFDSolution.Transport.Master;

namespace TFDSolution.Controllers
{
    [CustomAuthenticationFilter]
    public class ConfigurationController : BaseController
    {
        public readonly IConfigurationBusiness config = new ConfigurationBusiness();
        public readonly MasterBusiness master = new MasterBusiness();

        public ConfigurationController()
        {
            master = new MasterBusiness();
            config = new ConfigurationBusiness();
        }
        // GET: Configuration
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult OtherCharges()
        {
            //ViewBag.ChargesType = master.getSelection("ChargesType", "", "ChargesType");
            //ViewBag.TaxSlab = master.getSelection("TaxSlab", "", "TaxSlab");
            //ViewBag.SectionList = master.getSelection("FormSection", "", "SectionName");
            return View();
        }
        public ActionResult getOtherCharges()
        {
            List<OtherChargesModel> forms = config.GetSqlTemplateDetail(Convert.ToString(SessionPersister.LoginedUser.CompanyInfo.CompanyId));
            return Json(forms, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetOtherChargeDetails(int chargesId)
        {
            OtherChargesModel response = config.GetOtherChargeDetails(chargesId);
            return Json(response, JsonRequestBehavior.AllowGet);
        }
        public ActionResult DeleteOtherCharges(int chargesId)
        {
            ResponseModel response = config.DeleteOtherCharge(chargesId);
            return Json(response, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ReplicationOtherCharge(int chargesId)
        {
            ResponseModel response = config.ReplicationOtherCharge(chargesId, Convert.ToString(SessionPersister.LoginedUser.UserId.Value));
            return Json(response, JsonRequestBehavior.AllowGet);
        }
        public ActionResult UpdateOtherChargesStatus(int chargesId, int Status)
        {
            OtherChargesModel request = new OtherChargesModel();
            request.Status = Status;
            request.ChargesId = chargesId;
            request.UserId = SessionPersister.LoginedUser.UserId.Value;
            request.CompanyId = SessionPersister.LoginedUser.CompanyInfo.CompanyId.ToString();
            ResponseModel response = config.UpdateOtherChargesStatus(request);
            return Json(response, JsonRequestBehavior.AllowGet);
        }
        public ActionResult SaveOtherCharges(OtherChargesModel model)
        {
            model.CompanyId = SessionPersister.LoginedUser.CompanyInfo.CompanyId.ToString();
            model.UserId = SessionPersister.LoginedUser.UserId.Value;
            ResponseModel response = config.SaveOtherCharges(model);
            return Json(response, JsonRequestBehavior.AllowGet);
        }
        public ActionResult License()
        {
            LicenseModel model = ValidationKey.IsValid();
            return View(model);
        }

        #region Utilites
        public ActionResult MultiApproval()
        {
            MultiApprovalModel model = new MultiApprovalModel();
            model.ParentForms = master.getFormList("");
            return View(model);
        }

        public ActionResult PartialMultiApproval(MultipleApprovalRequest request)
        {
            return PartialView("_PartialMultiApproval", request);
        }
        public async Task<ActionResult> GetMultiApprovalData(MultipleApprovalRequest request)
        {
            FormData model = new FormData();
            request.CompanyId = Convert.ToString(SessionPersister.LoginedUser.CompanyInfo.CompanyId);
            model = await config.GetDynamicMultipleApproval(request);
            return new JsonResult
            {
                Data = model,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                MaxJsonLength = int.MaxValue // set to highest possible
            };
        }
        public ActionResult UpdateRecordStatus(MultipleApprovalRequest request)
        {
            request.CompanyId = Convert.ToString(SessionPersister.LoginedUser.CompanyInfo.CompanyId);
            request.UserId = Convert.ToString(SessionPersister.LoginedUser.UserId);
            ResponseModel response = config.UpdateRecordStatus(request);
            return Json(response, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Dashboard Settings
        public ActionResult DashboardSetting()
        {
            return View();
        }
        public ActionResult getDashboardSetting()
        {
            List<DashboardSettingModel> response = config.getDashboardSettings();
            return Json(response, JsonRequestBehavior.AllowGet);
        }
        public ActionResult SaveDashboardSetting(DashboardSettingModel model)
        {
            model.UserId = Convert.ToString(SessionPersister.LoginedUser.UserId.Value);
            ResponseModel response = config.SaveDashboardSetting(model);
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public ActionResult EmailDashboardSetting(int templateId)
        {
            DashboardSettingModel model = config.getDashboardSettingById(templateId);
            if (model == null)
            {
                model = new DashboardSettingModel();
                model.DashboardId = 0;
            }
            return View(model);
        }
        public ActionResult DeleteDashboard(int Id)
        {
            ResponseModel response = config.DeleteDashboardSetting(Id);
            return Json(response, JsonRequestBehavior.AllowGet);
        }
        public ActionResult DashboardSettingAction(int dashboardId)
        {
            DashboardSettingModel model = config.getDashboardSettingById(dashboardId);
            if (model == null)
            {
                model = new DashboardSettingModel();
                model.DashboardId = 0;
            }
            return View(model);
        }
        public ActionResult GetDashboardDetailSettings(int Id)
        {
            DashboardDetailSettingModel response = config.GetDashboardDetail(Id);
            return Json(response, JsonRequestBehavior.AllowGet);
        }
        public ActionResult SaveDashboardDetailSettings(DashboardDetailSettingModel model)
        {
            ResponseModel response = config.SaveDashboardDetailSetting(model);
            return Json(response, JsonRequestBehavior.AllowGet);
        }
        public ActionResult getDashboardSettingList(int dashboardId)
        {
            List<DashboardDetailSettingModel> response = config.GetDashboardDetailSettings(dashboardId);
            return Json(response, JsonRequestBehavior.AllowGet);
        }
        public ActionResult DeleteDashboardSetting(int Id)
        {
            ResponseModel response = config.DeleteDashboardDetailSetting(Id);
            return Json(response, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Email Template Settings
        public ActionResult EmailTemplate()
        {
            return View();
        }
        public ActionResult getEmailTemplates()
        {
            List<EmailTemplateModel> response = config.getEmailTamplates();
            return Json(response, JsonRequestBehavior.AllowGet);
        }
        public ActionResult SaveEmailTemplate(EmailTemplateModel model)
        {
            model.UserId = Convert.ToString(SessionPersister.LoginedUser.UserId.Value);
            ResponseModel response = config.SaveEmailTamplate(model);
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public ActionResult EmailTemplateAction(int templateId)
        {
            EmailTemplateModel model = config.getEmailTamplateById(templateId);
            if (model == null)
            {
                model = new EmailTemplateModel();
                model.TemplateId = 0;
            }
            return View(model);
        }
        public ActionResult getEmailTemplateByForm(string formId, int AccountId, int ParentId)
        {
            EmailTemplateModel response = config.getEmailTamplateByForm(formId, AccountId, ParentId);
            return Json(response, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Multi Approval Settings
        public ActionResult UserApprovalSettings()
        {
            ViewBag.ParentForms = master.getFormList("");
            return View();
        }

        [HttpPost]
        public ActionResult getFormUserApproval(FormModel request)
        {
            List<FormModel> response = config.getFormUserApproval(request);
            return Json(response, JsonRequestBehavior.AllowGet);
        }
        public ActionResult getFormUserApprovalSettings(string formId)
        {
            List<FormUserApprovalModel> response = config.getFormUserApprovalSettings(formId);
            return Json(response, JsonRequestBehavior.AllowGet);
        }
        public ActionResult SaveUserApprovalSettings(UserApprovalSettingModel model)
        {
            model.CreatedBy = Convert.ToString(SessionPersister.LoginedUser.UserId.Value);
            ResponseModel response = config.SaveUserApprovalSettings(model);
            return Json(response, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Static Form Report
        public ActionResult FormSystemReportSettings()
        {
            return View();
        }
        #endregion
    }
}