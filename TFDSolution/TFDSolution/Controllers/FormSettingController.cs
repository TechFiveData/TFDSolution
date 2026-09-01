using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TFDSolution.Transport.Master;
using TFDSolution.Business;
using System.Runtime.InteropServices;
using TFDSolution.Transport;
using TFDSolution.App_Start;
using TFDSolution.Common;
using System.Configuration;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Reflection;
using System.Data.Entity.Infrastructure;
using System.IO;
using Microsoft.Ajax.Utilities;
using Newtonsoft.Json.Linq;
using System.Runtime.InteropServices.ComTypes;
using TFDSolution.Business.Interface;
using System.Data.Entity;
using TFDSolution.Transport.Common;
using System.Security.AccessControl;
using static CrystalDecisions.Data.AdoDotNetInterop.InternalXmlSchemaDependencyTree;
using DocumentFormat.OpenXml.VariantTypes;

namespace TFDSolution.Controllers
{
    [CustomAuthenticationFilter]
    public class FormSettingController : BaseController
    {
        private readonly ITransactionBusiness _transactionService;

        public readonly MasterBusiness master = new MasterBusiness();
        public FormSettingController()
        {
            _transactionService = new TransactionBusiness();
            master = new MasterBusiness();
        }
        // GET: FormSetting
        public ActionResult Index()
        {
            return View();
        }
        public JsonResult GetFormSettingsRows(string formId, string tabId, string type)
        {
            List<FormSettings> formSettings = new List<FormSettings>();
            if (!string.IsNullOrEmpty(tabId) && type == "Detail")
            {
                formSettings = master.getFormTabSettings(formId, tabId, type);
            }
            else
            {
                formSettings = master.getFormSettings(formId, type);
            }
            return Json(formSettings, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetFormSettingsTab(string formId, string tabId, string type)
        {
            List<FormSettings> formSettings = master.getFormTabSettings(formId, tabId, type);
            return Json(formSettings, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Settings()
        {
            FormMast formMast = new FormMast();
            formMast = master.getFormData("");
            return View(formMast);
        }

        public ActionResult getForms(string FormId)
        {
            List<FormMast> forms = master.getFormList(FormId);
            return Json(forms, JsonRequestBehavior.AllowGet);
        }
        public ActionResult getFormSystem(string FormId)
        {
            List<FormSystemModel> forms = master.getFormSystemList(FormId);
            return Json(forms, JsonRequestBehavior.AllowGet);
        }
        public ActionResult getFormFields(string FormId, string SectionId, string TabId)
        {
            List<FormField> forms = master.getFormFieldList(FormId, SectionId, TabId);
            return new JsonResult
            {
                Data = forms,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                MaxJsonLength = int.MaxValue // set to highest possible
            };
            //return Json(forms, JsonRequestBehavior.AllowGet);
        }

        public ActionResult FormPartial(string _formId)
        {
            Guid formgi = Guid.Empty;
            if (!string.IsNullOrEmpty(_formId))
            {
                formgi = Guid.Parse(_formId);
            }
            FormMast formMast = formMast = master.getFormData(_formId);
            return PartialView("_FormPartial", formMast);
        }
        [HttpGet]
        public ActionResult GetSqlTemplateDDL()
        {
            var tables = master.GetSqlTemplateList();
            return Json(tables, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult GetSqlTemplateFieldList(string templateId)
        {
            var tables = master.GetSqlTemplateDetail(Convert.ToInt32(templateId));
            return Json(tables, JsonRequestBehavior.AllowGet);
        }
        public ActionResult AddForm(FormMast modal)
        {
            modal.UserId = SessionPersister.LoginedUser.UserId.Value;
            modal.CompanyId = SessionPersister.LoginedUser.CompanyInfo.CompanyId;
            ResponseModel response = master.SaveForm(modal);
            //if (response != null && response.IsSuccess.Value)
            //{
            //    SessionPersister.LoginedUser.Forms = master.getUserForms(modal.UserId);
            //}
            return Json(response, JsonRequestBehavior.AllowGet);
        }
        public ActionResult getFormData(string _formId)
        {
            FormMast formMast = formMast = master.getFormData(_formId);
            return Json(formMast, JsonRequestBehavior.AllowGet);
        }
        public ActionResult SettingAction(string _formId)
        {
            string companyId = Convert.ToString(SessionPersister.LoginedUser.CompanyInfo.CompanyId);
            List<clsSelection> clsSelections = master.getSelection("FormSection", companyId, "", "SectionName");
            ViewBag.SectionList = clsSelections;
            clsSelections = master.getSelection("FormFieldTypes", companyId, "", "");
            ViewBag.FieldTypeList = clsSelections;
            FormMast formMast = formMast = master.getFormData(_formId);
            return View(formMast);
        }

        public ActionResult FormPermission()
        {
            return View();
        }

        public ActionResult GetFormFieldTypeList()
        {
            List<FormFieldType> fieldTypes = master.GetFormFieldTypeList();
            // ViewBag.fieldType = fieldType;
            return Json(fieldTypes, JsonRequestBehavior.AllowGet);
        }
        public ActionResult getSectionTab(string formId, string sectionId)
        {
            List<FormTab> tab = master.geSectionTabs(formId, sectionId);
            return Json(tab, JsonRequestBehavior.AllowGet);
        }
        public ActionResult getFieldData(string fieldId)
        {
            FormField field = master.GetFormField(fieldId);
            return Json(field, JsonRequestBehavior.AllowGet);
        }
        public ActionResult SaveField(FormField modal)
        {
            modal.UserId = SessionPersister.LoginedUser.UserId.Value;
            modal.CompanyId = SessionPersister.LoginedUser.CompanyInfo.CompanyId;
            ResponseModel response = master.SaveField(modal);
            //ResponseModel response = new ResponseModel();
            //response.Action = "Save";
            //response.IsSuccess = true;
            //response.Response = "Temp save successfully.";
            return Json(response, JsonRequestBehavior.AllowGet);
        }
        public ActionResult getFormTab(string tabId)
        {
            FormTab response = master.GetFormTab(tabId);
            return Json(response, JsonRequestBehavior.AllowGet);
        }
        public ActionResult SaveTab(FormTab modal)
        {
            ResponseModel response = master.SaveTab(modal);
            return Json(response, JsonRequestBehavior.AllowGet);
        }
        public ActionResult DeleteTab(string tabId)
        {
            ResponseModel response = master.DeleteTab(tabId);
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetFieldOptionSource(string sourceType)
        {
            var tables = master.GetTablesByPrefix(sourceType); // Fetch tables starting with "m_"
            return Json(tables, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetSourceColumns(string sourceType, string tableName, string fieldId, string pageTableName)
        {
            var columns = master.GetSourceColumns(sourceType, tableName, fieldId, pageTableName);
            return Json(columns, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetStoredProcedureRequest(string spName, string pageTableName, string fieldId)
        {
            var requests = master.GetProcedureRequests(spName, pageTableName, fieldId);
            return Json(requests, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeleteFormSetting(DeleteFormSetting modal)
        {
            ResponseModel response = master.DeleteFormSetting(modal);
            return Json(response, JsonRequestBehavior.AllowGet);
        }
        #region Document Numbering

        public ActionResult SaveDocument(DocumentModel modal)
        {
            if (modal == null || modal.FormId == null || string.IsNullOrEmpty(modal.DocName.Trim()) || string.IsNullOrEmpty(modal.DocAlias.Trim()) || string.IsNullOrEmpty(modal.StartNumber.Trim()))
            {
                return Json(new { success = false, message = "All required fields must be filled!" });
            }
            string CompanyId = Convert.ToString(SessionPersister.LoginedUser.CompanyInfo.CompanyId);
            ResponseModel response = master.SaveDocument(modal, CompanyId);
            return Json(response, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetDocument(int docId)
        {
            DocumentModel response = master.GetDocument(docId);
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeleteDocument(int docId)
        {
            ResponseModel response = master.DeleteDocument(docId);
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public ActionResult getDocFormList(string formId)
        {
            string CompanyId = Convert.ToString(SessionPersister.LoginedUser.CompanyInfo.CompanyId);
            List<DocumentModel> tab = master.getDocFormList(formId, CompanyId);
            return Json(tab, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Form Pending
        public ActionResult SaveFormPending(FormPendingModel modal)
        {
            if (modal == null || modal.FormId == null || string.IsNullOrEmpty(modal.FormTabName.Trim()) || string.IsNullOrEmpty(modal.SourceName.Trim()) || modal.SortOrder == 0)
            {
                return Json(new { success = false, message = "All required fields must be filled!" });
            }
            modal.UserId = SessionPersister.LoginedUser.UserId.Value;
            ResponseModel response = master.SaveFormPending(modal);
            return Json(response, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetFormPending(int formPendingId)
        {
            FormPendingModel response = master.GetFormPending(formPendingId);
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeleteFormPending(int formPendingId)
        {
            ResponseModel response = master.DeleteFormPending(formPendingId);
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public ActionResult getFormPendingList(string formId)
        {
            List<FormPendingModel> tab = master.getFormPendingList(formId);
            return Json(tab, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Report
        public ActionResult ReportTab(string formId)
        {
            FormReportModel model = new FormReportModel();
            model.FormId = formId;
            return PartialView("_ReportTab", model);
        }
        public ActionResult GetReports(string formId)
        {
            List<ReportMast> Reports = master.getFormReport("", formId);
            return Json(Reports, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetFormSystemReports(string formId)
        {
            List<ReportMast> Reports = master.getFormSystemReport(formId);
            return Json(Reports, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult SaveReport()
        {
            ResponseModel response = new ResponseModel();
            try
            {
                ReportMast model = new ReportMast();
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Reports");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                //Fetch the File.
                if (Request.Files[0] != null)
                {
                    HttpPostedFileBase postedFile = Request.Files[0];
                    if (postedFile.ContentLength > 52428800) // 50 MB
                    {
                        response.IsSuccess = false;
                        response.Response = "File size exceeds 50 MB limit.\"";
                        return Json(response, JsonRequestBehavior.AllowGet);
                    }
                    //Fetch the File Name.
                    string fileName = postedFile.FileName;
                    path = Path.Combine(path, fileName);
                    if (System.IO.File.Exists(path))
                    {
                        System.IO.File.Delete(path);
                    }
                    //Save the File.
                    postedFile.SaveAs(path);
                    model.ReportPath = "~/Reports/" + fileName;
                    model.ReportName = fileName;
                }
                int reportId = 0;
                int.TryParse(Convert.ToString(Request.Form["ReportId"]), out reportId);
                model.ReportId = reportId;
                model.FormId = Convert.ToString(Request.Form["FormId"]);
                model.ReportSource = Convert.ToString(Request.Form["ReportSource"]);
                model.ReportTitle = Convert.ToString(Request.Form["ReportTitle"]);
                model.ReportType = Convert.ToString(Request.Form["ReportType"]);
                response = master.SaveFormReport(model);
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
                response.IsSuccess = false;
                response.Response = "Exception occurred, Please try again.";
            }
            return Json(response, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public ActionResult SaveSystemReport()
        {
            ResponseModel response = new ResponseModel();
            try
            {
                ReportMast model = new ReportMast();
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Reports");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                // Check if a new file was uploaded
                if (Request.Files.Count > 0 && Request.Files[0] != null && Request.Files[0].ContentLength > 0)
                {
                    HttpPostedFileBase postedFile = Request.Files[0];
                    if (postedFile.ContentLength > 52428800) // 50 MB
                    {
                        response.IsSuccess = false;
                        response.Response = "File size exceeds 50 MB limit.";
                        return Json(response, JsonRequestBehavior.AllowGet);
                    }

                    string fileName = postedFile.FileName;
                    string filePath = Path.Combine(path, fileName);
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }

                    postedFile.SaveAs(filePath);
                    model.ReportPath = "~/Reports/" + fileName;
                    model.ReportName = fileName;
                }
                else
                {
                    // No new file uploaded — keep existing file name sent from frontend
                    string existingReportName = Convert.ToString(Request.Form["ReportName"]);
                    model.ReportName = existingReportName;
                    model.ReportPath = "~/Reports/" + existingReportName;
                }

                int reportId = 0;
                int.TryParse(Convert.ToString(Request.Form["ReportId"]), out reportId);
                model.ReportId = reportId;
                model.FormId = Convert.ToString(Request.Form["FormId"]);
                model.ReportSource = Convert.ToString(Request.Form["ReportSource"]);
                model.ReportTitle = Convert.ToString(Request.Form["ReportTitle"]);

                response = master.SaveFormSystemReport(model);
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
                response.IsSuccess = false;
                response.Response = "Exception occurred, Please try again.";
            }
            return Json(response, JsonRequestBehavior.AllowGet);
        }
        public ActionResult DeleteFormReport(int ReportId)
        {
            ResponseModel response = master.DeleteFormReport(ReportId);
            return Json(response, JsonRequestBehavior.AllowGet);
        }
        public ActionResult DeleteFormSystemReport(int ReportId)
        {
            ResponseModel response = master.DeleteFormSystemReport(ReportId);
            return Json(response, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetFormReport(int ReportId)
        {
            ReportMast response = master.getFormReport(ReportId);
            return Json(response, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetFormSystemReport(int ReportId)
        {
            ReportMast response = master.getFormSystemReport(ReportId);
            return Json(response, JsonRequestBehavior.AllowGet);
        }
        public ActionResult DownloadRepor(string fileName)
        {
            var path = Server.MapPath("~/Reports/" + fileName);
            if (!System.IO.File.Exists(path))
            {
                return Json(new { success = false, message = "File not found" }, JsonRequestBehavior.AllowGet);
            }
            var fileBytes = System.IO.File.ReadAllBytes(path);
            return File(fileBytes, "application/octet-stream", fileName);
        }
        #endregion Report

        #region Form Settings
        public ActionResult PartialFormSettings(string formId)
        {
            List<FormSettings> formSettings = master.getFormSettings(formId);
            return PartialView("_FormSettingsPartial", formSettings);
        }
        public ActionResult SaveFormSettings(List<FormSettings> formSettings)
        {
            ResponseModel response = master.SaveFormSettings(formSettings);
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetFormStockSettings(string formId)
        {
            List<FormStockSettings> formSettings = master.getFormStockSettings(formId);
            return Json(formSettings, JsonRequestBehavior.AllowGet);
        }
        public ActionResult SaveFormStockSettings(List<FormStockSettings> formSettings)
        {
            ResponseModel response = master.SaveFormStockSettings(formSettings);
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        #endregion Form Settings
        [HttpPost]

        public ActionResult UpdateTabSortOrder(SortOrderUpdateRequest sortedTabs)
        {
            ResponseModel response = new ResponseModel();
            // Call your business layer to update the sort order in DB
            var business = new MasterBusiness();
            if (sortedTabs == null || sortedTabs.Rows == null || !sortedTabs.Rows.Any())
            {
                return Json(new { success = false, message = "No Tabs to update." });
            }
            response = master.UpdateTabSortOrder(sortedTabs.Rows);
            if (response.IsSuccess.HasValue && response.IsSuccess.Value)
            {
                ViewBag.SuccessMessage = response.Response;
                return Json(new { success = response.IsSuccess.Value });
            }
            else
            {
                ViewBag.ErrorMessages = response.Response ?? "Record failed to save.";
                return Json(new { success = false });
            }
        }

        public ActionResult UpdateFieldSortOrder(SortOrderUpdateRequest sortedTabs)
        {
            ResponseModel response = new ResponseModel();
            // Call your business layer to update the sort order in DB
            var business = new MasterBusiness();
            if (sortedTabs == null || sortedTabs.Rows == null || !sortedTabs.Rows.Any())
            {
                return Json(new { success = false, message = "No Fields to update." });
            }
            response = master.UpdateFielSortOrder(sortedTabs.Rows);
            if (response.IsSuccess.HasValue && response.IsSuccess.Value)
            {
                ViewBag.SuccessMessage = response.Response;
                return Json(new { success = response.IsSuccess.Value });
            }
            else
            {
                ViewBag.ErrorMessages = response.Response ?? "Record failed to save.";
                return Json(new { success = false });
            }
        }

        #region "Re Ordering"

        #endregion  "Re Ordering"
        #region Field Information
        public ActionResult BindFieldInformation(FormFieldInfoRequest request)
        {
            List<FormFieldInfoModel> model = _transactionService.GetFieldInformation(request);
            return PartialView("_PartialFieldInformation", model);
        }
        public ActionResult SaveFieldInformation(FormFieldInfoModel request)
        {
            request.CreatedBy = Convert.ToString(SessionPersister.LoginedUser.UserId.Value);
            request.UpdatedBy = Convert.ToString(SessionPersister.LoginedUser.UserId.Value);
            ResponseModel response = _transactionService.SaveFieldInformation(request);
            return Json(response, JsonRequestBehavior.AllowGet);
        }
        public ActionResult DeleteFieldInformation(int Id)
        {
            ResponseModel response = _transactionService.DeleteFieldInformation(Id);
            return Json(response, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Item Advance Form Settings
        public ActionResult ItemAdvanceSettings()
        {
            return View();
        }

        public ActionResult getFormItemAdvanceSettings()
        {
            List<FormItemAdvanceSettings> tab = master.getFormItemAdvanceSettings();
            return Json(tab, JsonRequestBehavior.AllowGet);
        }
        public ActionResult getFormItemAdvanceDetail(int ItemAdvanceId)
        {
            FormItemAdvanceSettings result = master.getFormItemAdvanceDetail(ItemAdvanceId);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public ActionResult SaveFormItemAdvance(FormItemAdvanceSettings modal)
        {
            modal.UserId = Convert.ToString(SessionPersister.LoginedUser.UserId.Value);
            modal.CompanyId = Convert.ToString(SessionPersister.LoginedUser.CompanyInfo.CompanyId);
            ResponseModel response = master.SaveFormItemAdvanceSettings(modal);
            return Json(response, JsonRequestBehavior.AllowGet);
        }
        public ActionResult getFormItemAdvanceFields(int ItemAdvanceId)
        {
            List<FormItemAdvanceField> fields = master.getFormItemAdvanceFields(ItemAdvanceId);
            return Json(fields, JsonRequestBehavior.AllowGet);
        }
        public ActionResult getFormItemAdvanceFieldDetail(string FieldId)
        {
            FormItemAdvanceField fieldData = master.getFormItemAdvanceFieldDetail(FieldId);
            return Json(fieldData, JsonRequestBehavior.AllowGet);
        }
        public ActionResult UpdateAdvanceSortOrder(SortOrderFieldUpdate sortedTabs)
        {
            ResponseModel response = new ResponseModel();
            // Call your business layer to update the sort order in DB
            response = master.UpdateItemAdvanceFieldSortOrder(sortedTabs.Rows);
            if (response.IsSuccess.HasValue && response.IsSuccess.Value)
            {
                ViewBag.SuccessMessage = response.Response;
                return Json(new { success = response.IsSuccess.Value });
            }
            else
            {
                ViewBag.ErrorMessages = response.Response ?? "Record failed to save.";
            }
            return Json(new { success = false });
        }
        public ActionResult SaveFormItemAdvanceField(FormItemAdvanceField modal)
        {
            modal.UserId = SessionPersister.LoginedUser.UserId.Value;
            modal.CompanyId = SessionPersister.LoginedUser.CompanyInfo.CompanyId;
            ResponseModel response = master.SaveFormItemAdvanceField(modal);
            return Json(response, JsonRequestBehavior.AllowGet);
        }
        public ActionResult DeleteFormItemAdvanceSetting(DeleteFormSetting modal)
        {
            ResponseModel response = master.DeleteFormItemAdvanceSetting(modal);
            return Json(response, JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
}