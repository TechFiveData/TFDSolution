using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Razor.Generator;
using System.Web.UI.WebControls;
using CrystalDecisions.ReportAppServer;
//using CrystalDecisions.VSDesigner;
using DocumentFormat.OpenXml.Office2016.Excel;
using Newtonsoft.Json;
using TFDSolution.Business;
using TFDSolution.Business.Interface;
using TFDSolution.Common;
using TFDSolution.Models;
using TFDSolution.Transport;
using TFDSolution.Transport.Common;
using TFDSolution.Transport.Master;

namespace TFDSolution.Controllers
{
    public class TransactionController : Controller
    {
        // GET: Transaction
        private readonly ITransactionBusiness _transactionService;
        private readonly IMasterBusiness _masterBusiness;
        public TransactionController()
        {
            _transactionService = new TransactionBusiness();
            _masterBusiness = new MasterBusiness();
        }
        public async Task<ActionResult> AmendmentProcess(AmendmentRequest request)
        {
            request.UserId = Convert.ToString(SessionPersister.LoginedUser.UserId);
            request.CompanyId = Convert.ToString(SessionPersister.LoginedUser.CompanyInfo.CompanyId);
            request.FinacialYearId = SessionPersister.LoginedUser.CompanyInfo.DefaultFiancialId;
            var response = await _transactionService.AmendmentProcessAsync(request);
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> ReplicationProcess(AmendmentRequest request)
        {
            request.UserId = Convert.ToString(SessionPersister.LoginedUser.UserId);
            request.CompanyId = Convert.ToString(SessionPersister.LoginedUser.CompanyInfo.CompanyId);
            request.FinacialYearId = SessionPersister.LoginedUser.CompanyInfo.DefaultFiancialId;
            var response = await _transactionService.ReplicationProcessAsync(request);
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        #region Delivery Schedule
        public ActionResult PartialItemDeliverySchedule(DeliveryScheduleModel request)
        {
            request.UserId = Convert.ToString(SessionPersister.LoginedUser.UserId);
            DeliveryScheduleModel model = _transactionService.GetItemDeliverySchedule(request);
            model.Fields = _masterBusiness.getFormItemAdvanceFields(3);
            return PartialView("_ItemDeliverySchedule", model);
        }

        [HttpPost]
        public ActionResult SaveDeliverySchedule(DeliveryScheduleModel request)
        {
            request.UserId = Convert.ToString(SessionPersister.LoginedUser.UserId);
            request.CompanyId = Convert.ToString(SessionPersister.LoginedUser.CompanyInfo.CompanyId);
            request.FinacialYearId = SessionPersister.LoginedUser.CompanyInfo.DefaultFiancialId;
            var response = _transactionService.SaveItemDeliverySchedule(request);
            return Json(response, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Stock with Serial Number
        public ActionResult PartialItemStockSerial(ItemStockSerialModel request)
        {
            request.UserId = Convert.ToString(SessionPersister.LoginedUser.UserId);
            request.CompanyId = Convert.ToString(SessionPersister.LoginedUser.CompanyInfo.CompanyId);
            request.FinacialYearId = SessionPersister.LoginedUser.CompanyInfo.DefaultFiancialId;
            ItemStockSerialModel model = new ItemStockSerialModel();
            if (request.Action != "V")
            {
                if (request.Action == "E" && request.SerialCount > 0)
                {
                    model = _transactionService.GetItemStockSerialNumbers(request);
                }
                else if (request.Action == "A")
                {
                    model = _transactionService.GetItemStockSerialNumbers(request);
                }
                else
                {
                    model = _transactionService.GetItemStockSerial(request);
                }
            }
            else
            {
                model = _transactionService.GetItemStockSerial(request);
            }
            model.Action = request.Action;
            model.SerialCount = request.SerialCount;
            model.BatchLotNo = request.BatchLotNo;  
            model.Fields = _masterBusiness.getFormItemAdvanceFields(2);
            model.FormId = request.FormId;
            return PartialView("_ItemStockSerial", model);
        }

        [HttpPost]
        public ActionResult SaveStockSerialNumber(ItemStockSerialModel request)
        {
            request.UserId = Convert.ToString(SessionPersister.LoginedUser.UserId);
            request.CompanyId = Convert.ToString(SessionPersister.LoginedUser.CompanyInfo.CompanyId);
            request.FinacialYearId = SessionPersister.LoginedUser.CompanyInfo.DefaultFiancialId;
            var response = _transactionService.SaveItemStockSerial(request);
            return Json(response, JsonRequestBehavior.AllowGet);
        }
        #endregion

        public ActionResult SendTransactionEmail(EmailRequest request)
        {
            request.UserId = Convert.ToString(SessionPersister.LoginedUser.UserId.Value);
            request.UserName = Convert.ToString(SessionPersister.LoginedUser.FirstName) + " " + Convert.ToString(SessionPersister.LoginedUser.LastName);
            request.CompanyName = Convert.ToString(SessionPersister.LoginedUser.CompanyInfo.CompanyName);
            request.CompanyId = Convert.ToString(SessionPersister.LoginedUser.CompanyInfo.CompanyId);
            ResponseModel response = new ResponseModel();
            if (request.ReportId > 0)
            {
                ReportMast report = _masterBusiness.getFormReport(request.ReportId);
                if (report != null)
                {
                    ResponseModel ReportResponse = ReportHelper.ExportReport(request.ParentId, report, request.ReportFileName, true);
                    if (!(ReportResponse.IsSuccess.HasValue ? ReportResponse.IsSuccess.Value : false))
                    {
                        response.IsSuccess = false;
                        response.Response = ReportResponse.Response;
                    }
                    else
                    {
                        request.FullFilePath = Server.MapPath("~/ExportReport/" + ReportResponse.Response);
                        response = MailHelper.SendEmail(request);
                    }
                }
                else
                {
                    response.IsSuccess = false;
                    response.Response = request.ReportFileName + " report not loading. Please contact to administrator";
                }
            }
            else
            {
                response = MailHelper.SendEmail(request);
            }
            CommonBusiness.SaveAuditLog(new Transport.Common.AuditLog()
            {
                Action = "SendEmail",
                CompanyId = SessionPersister.LoginedUser.CompanyInfo.CompanyId,
                FinancialYearId = SessionPersister.LoginedUser.CompanyInfo.DefaultFiancialId,
                IPAddress = SessionPersister.LoginedUser.IPAddress,
                PageName = string.Empty,
                RecordId = request.ReportId.ToString(),
                Remark = response.Response,
                UserId = SessionPersister.LoginedUser.UserId.Value
            });
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        #region Field Information
        public ActionResult BindFieldInformationView(FormFieldInfoRequest request)
        {
            request.CompanyId = Convert.ToString(SessionPersister.LoginedUser.CompanyInfo.CompanyId);
            List<FormFieldInfoModel> model = _transactionService.GetFieldInformation(request);
            return PartialView("_FieldInformation", model);
        }
        #endregion

        #region Export Import
        [HttpPost]
        public async Task<ActionResult> ExportTemplate(ExportTemplateRequest model)
        {
            var response = new ResponseModel
            {
                IsSuccess = true,
                Response = "Export started successfully."
            };
            try
            {
                string filenm = model.TabTitle.Replace(" ", "") + "_" + DateTime.Now.ToString("MMddyyyyHHmmss");
                string folderPath = Server.MapPath("~/TempDownload");
                CommonHelper.EnsureDirectoryExists(folderPath);
                CommonHelper.CleanupOldFiles(folderPath, filenm);
                string fileName = $"{filenm}.csv";
                if (model.ExportType == "excel")
                {
                    fileName = $"{filenm}.xlsx";
                }
                string fullFilePath = Path.Combine(folderPath, fileName);
                response.Action = fileName;
                if (System.IO.File.Exists(fullFilePath))
                    System.IO.File.Delete(fullFilePath);

                List<FormField> fields = _transactionService.DownloadImportTemplate(model.TabId);
                if (model.ExportType == "excel")
                {
                    CsvExporter.ExportTemplateToExcel(fields, folderPath, fileName, model.TabTitle);
                }
                else
                {
                    CsvExporter.ExportTemplateToCsv(fields, folderPath, fileName);
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
                response.IsSuccess = false;
                response.Response = $"Error starting export: {ex.Message}";
            }
            // Return immediately (don’t wait for export)
            return Json(response, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult UploadImportFiles()
        {
            string newFileName = "";
            try
            {
                var files = Request.Files;
                if (files.Count > 0)
                {
                    for (int i = 0; i < files.Count; i++)
                    {
                        var file = files[i];
                        var path = Server.MapPath("~/TempDownload/");
                        if (!Directory.Exists(path))
                            Directory.CreateDirectory(path);

                        string originalFileName = Path.GetFileNameWithoutExtension(file.FileName);
                        string extension = Path.GetExtension(file.FileName);
                        //extension = ".csv";
                        // Example: MyFile_20251108_162030.txt
                        newFileName = $"{originalFileName}_{DateTime.Now:yyyyMMdd_HHmmss}{extension}";

                        string filePath = Path.Combine(path, newFileName);
                        file.SaveAs(filePath);
                    }
                }
                return Json(new { IsSuccess = true, message = "Files uploaded successfully!", NewFileName = newFileName });
            }
            catch (Exception ex)
            {
                return Json(new { IsSuccess = false, message = "Error: " + ex.Message, NewFileName = newFileName });
            }
        }

        public JsonResult ValidateImportedFile(ExportTemplateRequest model)
        {
            try
            {
                string folderPath = Server.MapPath("~/TempDownload");
                string fullFilePath = Path.Combine(folderPath, model.FileName);
                if (System.IO.File.Exists(fullFilePath))
                {
                    DataTable dt = new DataTable();
                    bool isCsv = !string.IsNullOrEmpty(model.FileName) && Path.GetExtension(model.FileName).Equals(".csv", StringComparison.OrdinalIgnoreCase);
                    if (isCsv)
                    {
                        dt = CsvExporter.ReadCsvToDataTable(fullFilePath);
                    }
                    else
                    {
                        dt = CsvExporter.ReadExcelToDataTable(fullFilePath);
                    }
                    if (dt != null && dt.Rows.Count == 0)
                    {
                        return Json(new { IsSuccess = false, message = "Record could not be retrieved. Please try again.", NewFileName = model.FileName });
                    }
                    string json = JsonConvert.SerializeObject(dt);
                    ResponseModel response = _transactionService.ValidateImportTemplate(model, json);
                    return Json(new { IsSuccess = response.IsSuccess, message = response.Response, NewFileName = model.FileName });
                }
                else
                {
                    return Json(new { IsSuccess = false, message = "Uploaded File does not exists. Please upload again.", NewFileName = model.FileName });
                }
            }
            catch (Exception ex)
            {
                return Json(new { IsSuccess = false, message = "Error: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult> ImportData(ExportTemplateRequest model)
        {
            var response = new ResponseModel
            {
                IsSuccess = true,
                Response = "Export started successfully."
            };
            try
            {
                string folderPath = Server.MapPath("~/TempDownload");
                string fullFilePath = Path.Combine(folderPath, model.FileName);
                if (System.IO.File.Exists(fullFilePath))
                {
                    DataTable dt = new DataTable();
                    bool isCsv = !string.IsNullOrEmpty(model.FileName) && Path.GetExtension(model.FileName).Equals(".csv", StringComparison.OrdinalIgnoreCase);
                    if (isCsv)
                    {
                        dt = CsvExporter.ReadCsvToDataTable(fullFilePath);
                    }
                    else
                    {
                        dt = CsvExporter.ReadExcelToDataTable(fullFilePath);
                    }
                    if (dt != null && dt.Rows.Count == 0)
                    {
                        response.IsSuccess = false;
                        response.Response = "Record could not be retrieved. Please try again.";
                    }
                    LoginUserInfo user = SessionPersister.LoginedUser;
                    SubmitFormModel request = new SubmitFormModel();
                    request.UserId = user.UserId.Value;
                    request.CompanyId = user.CompanyInfo.CompanyId;
                    request.FiancialYearId = user.CompanyInfo.DefaultFiancialId;
                    request.FormId = model.FormId;
                    request.TabId = model.TabId;
                    request.URNNo = model.URNNo;
                    request.ProcessId = model.ProcessId;
                    request.HeaderFieldData = model.HeaderFieldData;
                    response = _transactionService.SaveImportData(request, dt);
                }
                else
                {
                    response.IsSuccess = false;
                    response.Response = "Uploaded File does not exists. Please upload again.";
                }
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Response = $"Error starting export: {ex.Message}";
            }
            // Return immediately (don’t wait for export)
            return Json(response, JsonRequestBehavior.AllowGet);
        }
        #endregion

        public async Task<ActionResult> ExportPageList(FormDataModel model)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                model.UserId = Convert.ToString(SessionPersister.LoginedUser.UserId.Value);
                model.CompanyId = Convert.ToString(SessionPersister.LoginedUser.CompanyInfo.CompanyId);
                model.CompanyName = Convert.ToString(SessionPersister.LoginedUser.CompanyInfo.CompanyName);
                model.FinacialYearId = SessionPersister.LoginedUser.CompanyInfo.DefaultFiancialId;
                DataSet dst = await _masterBusiness.ExportPageData(model);
                if (dst != null && dst.Tables.Count > 0 && dst.Tables[0].Rows.Count > 0)
                {
                    string cleanTitle = model.FormTitle.Replace(" ", "");
                    model.FormTitle = cleanTitle;
                    string folderPath = Server.MapPath("~/TempDownload");

                    // Ensure the directory exists
                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }
                    // Generate unique file name
                    string fileName = $"{model.FormTitle}_{DateTime.Now:MMddyyyyHHmmss}.csv";
                    if (model.ExportType == "excel")
                    {
                        fileName = $"{model.FormTitle}_{DateTime.Now:MMddyyyyHHmmss}.xlsx";
                    }
                    if (model.ExportType == "pdf")
                    {
                        fileName = $"{model.FormTitle}_{DateTime.Now:MMddyyyyHHmmss}.pdf";
                    }
                    response.Action = fileName;
                    string fullFilePath = Path.Combine(folderPath, fileName);
                    // Delete the file if it already exists
                    if (System.IO.File.Exists(fullFilePath))
                    {
                        System.IO.File.Delete(fullFilePath);
                    }
                    // Delete any old files with the same report title prefix
                    var matchingFiles = Directory
                        .EnumerateFiles(folderPath)
                        .Where(f => Path.GetFileName(f).StartsWith(model.FormTitle, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                    foreach (var oldFile in matchingFiles)
                    {
                        try
                        {
                            System.IO.File.Delete(oldFile);
                        }
                        catch (Exception ex)
                        {
                            CommonBusiness.LogEx(ex);
                        }
                    }
                    // Export the dataset to CSV
                    if (model.ExportType == "excel")
                    {
                        CsvExporter.ExportPageDataToExcel(dst.Tables[0], folderPath, fileName, model);
                    }
                    else if (model.ExportType == "pdf")
                    {
                        PdfExporter.ExportPageDataToPdf(dst.Tables[0], folderPath, fileName, model);
                    }
                    else
                    {
                        CsvExporter.ExportPageDataToCsv(dst.Tables[0], folderPath, fileName, model);
                    }
                    // Set response
                    response.IsSuccess = true;
                    response.Response = fileName;
                }
                else
                {
                    response.Response = "Does not have a data to export.";
                }
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Response = "Error occurred in export: " + ex.Message;
            }
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> getPageTabDataExport(FormDataModel model)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                model.UserId = Convert.ToString(SessionPersister.LoginedUser.UserId.Value);
                model.CompanyId = Convert.ToString(SessionPersister.LoginedUser.CompanyInfo.CompanyId);
                model.CompanyName = Convert.ToString(SessionPersister.LoginedUser.CompanyInfo.CompanyName);
                model.FinacialYearId = SessionPersister.LoginedUser.CompanyInfo.DefaultFiancialId;
                DataSet dst = await _masterBusiness.ExportPageTabData(model);
                if (dst != null && dst.Tables.Count > 0 && dst.Tables[0].Rows.Count > 0)
                {
                    string cleanTitle = model.FormTitle.Replace(" ", "");
                    model.FormTitle = cleanTitle;
                    string folderPath = Server.MapPath("~/TempDownload");

                    // Ensure the directory exists
                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }
                    // Generate unique file name
                    string fileName = $"{model.FormTitle}_{Convert.ToString(model.TabName).Replace(" ", "")}_{DateTime.Now:MMddyyyyHHmmss}.csv";
                    if (model.ExportType == "excel")
                    {
                        fileName = $"{model.FormTitle}_{Convert.ToString(model.TabName).Replace(" ", "")}_{DateTime.Now:MMddyyyyHHmmss}.xlsx";
                    }
                    response.Action = fileName;
                    string fullFilePath = Path.Combine(folderPath, fileName);
                    // Delete the file if it already exists
                    if (System.IO.File.Exists(fullFilePath))
                    {
                        System.IO.File.Delete(fullFilePath);
                    }
                    // Delete any old files with the same report title prefix
                    var matchingFiles = Directory
                        .EnumerateFiles(folderPath)
                        .Where(f => Path.GetFileName(f).StartsWith(model.FormTitle, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                    foreach (var oldFile in matchingFiles)
                    {
                        try
                        {
                            System.IO.File.Delete(oldFile);
                        }
                        catch (Exception ex)
                        {
                            CommonBusiness.LogEx(ex);
                            // Optionally log or handle delete failure
                        }
                    }
                    // Export the dataset to CSV
                    // Export the dataset to CSV
                    if (model.ExportType == "excel")
                    {
                        CsvExporter.ExportPageDataToExcel(dst.Tables[0], folderPath, fileName, model);
                    }
                    else
                    {
                        CsvExporter.ExportPageDataToCsv(dst.Tables[0], folderPath, fileName, model);
                    }
                    // Set response
                    response.IsSuccess = true;
                    response.Response = fileName;
                }
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Response = "Error occurred in export: " + ex.Message;
            }
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public ActionResult AddFormComment(FormCommentModel request)
        {
            request.UserId = Convert.ToString(SessionPersister.LoginedUser.UserId);
            var response = CommonBusiness.SavePageComments(request);
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetUserComments(FormCommentModel model)
        {
            List<FormCommentModel> response = new List<FormCommentModel>();
            try
            {
                string userId = Convert.ToString(SessionPersister.LoginedUser.UserId.Value);
                response = CommonBusiness.GetFormComments(
                                userId,
                                model.FormId,
                                model.ParentId
                           );
            }
            catch (Exception ex)
            {
                return Json(new { IsSuccess = false, Message = ex.Message });
            }

            return Json(new { IsSuccess = true, Data = response }, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> getLanguageConversation(string fieldId, string fieldValue)
        {
            List<SpecificationField> fields = new List<SpecificationField>();
            try
            {
                List<LanguageFieldModel> languageFields = _transactionService.GetLanguageConversationFields(fieldId);
                if (languageFields != null && languageFields.Count > 0)
                {
                    string translateVale = string.Empty;
                    foreach (var item in languageFields)
                    {
                        if (!string.IsNullOrEmpty(fieldValue) && fieldValue.Length > 0)
                        {
                            translateVale = await CommonHelper.TranslateCode("en", item.LanguageCode, fieldValue);
                        }
                        SpecificationField field = new SpecificationField
                        {
                            FieldName = item.FieldName,
                            FieldCaption = item.FieldName,
                            FieldValue = translateVale
                        };
                        fields.Add(field);
                    }
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
            }
            return Json(fields, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> UpdateTabRecordSortOrder(string FormId ,string tabId,int ParentId, List<TabRecordSortOrderModel> rows)
        {
            // Implementation for updating tab record sort order
            var response = _transactionService.UpdateTabRecordSortOrder(FormId, tabId, ParentId, rows);
            return Json(response, JsonRequestBehavior.AllowGet);
        }
    }
}