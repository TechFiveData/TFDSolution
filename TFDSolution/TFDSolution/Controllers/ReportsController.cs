using ClosedXML.Excel;
using CrystalDecisions.ReportAppServer;
using CrystalDecisions.Shared;
//using CrystalDecisions.VSDesigner;
using CrystalDecisions.Web;
using DocumentFormat.OpenXml.Spreadsheet;
using Newtonsoft.Json;
using Org.BouncyCastle.Bcpg.OpenPgp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using TFDSolution.App_Start;
using TFDSolution.Business;
using TFDSolution.Business.Interface;
using TFDSolution.Common;
using TFDSolution.Transport;
using TFDSolution.Transport.Master;
using static System.Web.Razor.Parser.SyntaxConstants;

namespace TFDSolution.Controllers
{
    [CustomAuthenticationFilter]
    [LinceseAuthenticationFilter]
    public class ReportsController : BaseController
    {
        private static ConcurrentDictionary<string, Task> _runningTasks = new ConcurrentDictionary<string, Task>();

        private readonly IReportBusiness reportBusiness;
        public readonly MasterBusiness master = new MasterBusiness();
        public ReportsController()
        {
            reportBusiness = new ReportBusiness();
        }
        public ActionResult Index()
        {
            return View();
        }
        #region Item Stock Report

        public ActionResult StockSummary()
        {
            return View();
        }
        public ActionResult getStockSummary(DateTime fromDate, DateTime ToDate)
        {
            List<Rep_StockSummaryModel> response = reportBusiness.getItemStockSummary(fromDate, ToDate);
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        #endregion

        public ActionResult getReportTemplates()
        {
            string UserId = Convert.ToString(SessionPersister.LoginedUser.UserId.Value);
            List<ReportTemplateModel> response = reportBusiness.getReportTamplates(UserId);
            return Json(response, JsonRequestBehavior.AllowGet);
        }
        public ActionResult SaveReportTemplate(ReportTemplateModel model)
        {
            ResponseModel response = reportBusiness.SaveReportTamplate(model);
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public ActionResult PartialReportTemplate(int ReportId)
        {
            ReportTemplateModel model = reportBusiness.getReportTamplateById(ReportId);
            return PartialView("_ReportTemplate", model);
        }

        #region MIS Report
        public ActionResult MISReport()
        {
            return View();
        }
        public ActionResult MISReportView(int ReportId)
        {
            string UserId = Convert.ToString(SessionPersister.LoginedUser.UserId.Value);
            string sessionKey = $"MIS_REPORT_{ReportId}_{UserId}";
            string sessionKeyFiltered = $"MIS_REPORT_FILTERDATA_{ReportId}_{UserId}";
            string sessionKeyColumn = $"MIS_REPORT_{ReportId}_COLUMN_{UserId}";
            Session.Remove(sessionKey);
            Session.Remove(sessionKeyFiltered);
            Session.Remove(sessionKeyColumn);
            MemoryCache.Default.Remove(sessionKey);
            ReportTemplateModel model = reportBusiness.getReportTamplateById(ReportId);
            return View(model);
        }
        public ActionResult getReportData(ReportFilterRequest model)
        {
            // Example: fetch from DB dynamically
            model.UserId = Convert.ToString(SessionPersister.LoginedUser.UserId.Value);
            model.CompanyId = Convert.ToString(SessionPersister.LoginedUser.CompanyInfo.CompanyId);
            model.FinacialYearId = SessionPersister.LoginedUser.CompanyInfo.DefaultFiancialId;
            ReportData report = reportBusiness.executeMISReport(model);
            return Json(new { data = report }, JsonRequestBehavior.AllowGet);
        }
        public void ExportToExcelJob(ReportFilterRequest model)
        {
            try
            {
                model.UserId = Convert.ToString(SessionPersister.LoginedUser.UserId.Value);
                model.CompanyId = Convert.ToString(SessionPersister.LoginedUser.CompanyInfo.CompanyId);
                model.CompanyName = Convert.ToString(SessionPersister.LoginedUser.CompanyInfo.CompanyName);
                model.FinacialYearId = SessionPersister.LoginedUser.CompanyInfo.DefaultFiancialId;
                DataSet dst = reportBusiness.executeMISReportExport(model);
                if (dst != null && dst.Tables.Count > 0 && dst.Tables[0].Rows.Count > 0)
                {
                    string cleanTitle = model.ReportTitle.Replace(" ", "");
                    model.ReportTitle = cleanTitle;
                    string folderPath = Server.MapPath("~/TempDownload");
                    // Ensure the directory exists
                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }
                    // Generate unique file name
                    string fileName = $"{model.ReportTitle}_{DateTime.Now:MMddyyyyHHmmss}.csv";
                    string fullFilePath = Path.Combine(folderPath, fileName);
                    // Delete the file if it already exists
                    if (System.IO.File.Exists(fullFilePath))
                    {
                        System.IO.File.Delete(fullFilePath);
                    }
                    // Delete any old files with the same report title prefix
                    var matchingFiles = Directory.EnumerateFiles(folderPath).Where(f => Path.GetFileName(f).StartsWith(model.ReportTitle, StringComparison.OrdinalIgnoreCase)).ToList();
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
                    CsvExporter.ExportDataSetToCsv(dst, folderPath, fileName, model);
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
            }
        }

        #region Export Report
        [HttpGet]
        public ActionResult CheckExportStatus(string fileName)
        {
            if (!string.IsNullOrEmpty(fileName))
            {
                string folderPath = Server.MapPath("~/TempDownload");
                string fullFilePath = Path.Combine(folderPath, fileName);
                return Json(new { IsReady = System.IO.File.Exists(fullFilePath), FileName = fileName }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { IsReady = false }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<ActionResult> ExportReport(ReportFilterRequest model, string type)
        {
            var response = new ResponseModel
            {
                IsSuccess = true,
                Response = "Export started successfully."
            };

            try
            {
                var user = SessionPersister.LoginedUser;
                model.UserId = Convert.ToString(user.UserId.Value);
                model.CompanyId = Convert.ToString(user.CompanyInfo.CompanyId);
                model.CompanyName = user.CompanyInfo.CompanyName;
                model.FinacialYearId = user.CompanyInfo.DefaultFiancialId;


                model.ReportName = model.ReportTitle;
                model.ReportTitle = model.ReportTitle.Replace(" ", "");
                string folderPath = Server.MapPath("~/TempDownload");

                EnsureDirectoryExists(folderPath);
                CleanupOldFiles(folderPath, model.ReportTitle);

                string fileName = $"{model.ReportTitle}_{DateTime.Now:MMddyyyyHHmmss}.{type}";
                string fullFilePath = Path.Combine(folderPath, fileName);
                response.Action = fileName;
                if (System.IO.File.Exists(fullFilePath))
                    System.IO.File.Delete(fullFilePath);

                string sessionKey = $"MIS_REPORT_{model.ReportId}_{model.UserId}";
                string sessionKeyFiltered = $"MIS_REPORT_FILTERDATA_{model.ReportId}_{model.UserId}";
                DataSet dst = new DataSet();
                string cacheKey = $"MIS_REPORT_{model.ReportId}_{SessionPersister.LoginedUser.UserId}";

                if (_runningTasks.TryGetValue(cacheKey, out Task runningTask))
                {
                    await runningTask; // ⏳ WAIT
                }
                var fullCache = Session[sessionKeyFiltered] as ReportSessionCache;
                var fulldata = Session[sessionKey] as ReportSessionCache;
                var cache = MemoryCache.Default;
                var result = cache.Get(sessionKey) as ReportSessionCache;
                if (result != null)
                {
                    fulldata = result;
                }
                if (fullCache != null && fullCache.Data != null && fullCache.Data.Count > 0
                    && fullCache.ColumnSearches != null && fullCache.ColumnSearches.Any())
                {
                    dst = ConvertToDataSet(fullCache.Data);
                    if (dst == null || dst.Tables.Count == 0 || dst.Tables[0].Rows.Count == 0)
                    {
                        response.IsSuccess = false;
                        response.Response = "No data found to export.";
                    }
                    if (type == "pdf")
                    {
                        PdfExporter.ExportDataSetToPdf(dst, folderPath, fileName, model);
                    }
                    else
                    {
                        CsvExporter.ExportDataSetToCsv(dst, folderPath, fileName, model);
                    }
                    response.IsSuccess = true;
                    response.Response = fileName;
                }
                else if (fulldata != null && fulldata.Data != null && fulldata.Data.Count > 0)
                {
                    dst = ConvertToDataSet(fulldata.Data);
                    if (dst == null || dst.Tables.Count == 0 || dst.Tables[0].Rows.Count == 0)
                    {
                        response.IsSuccess = false;
                        response.Response = "No data found to export.";
                    }
                    if (type == "pdf")
                    {
                        PdfExporter.ExportDataSetToPdf(dst, folderPath, fileName, model);
                    }
                    else
                    {
                        CsvExporter.ExportDataSetToCsv(dst, folderPath, fileName, model);
                    }
                    response.IsSuccess = true;
                    response.Response = fileName;
                }
                else
                {
                    // Fire-and-forget background task
                    _ = Task.Run(() => SafeGenerateExport(model, folderPath, fileName, type));
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
        [HttpPost]
        public ActionResult SendTransactionEmail(EmailRequest request)
        {
            request.UserId = Convert.ToString(SessionPersister.LoginedUser.UserId.Value);
            request.UserName = Convert.ToString(SessionPersister.LoginedUser.FirstName) + " " + Convert.ToString(SessionPersister.LoginedUser.LastName);
            request.CompanyName = Convert.ToString(SessionPersister.LoginedUser.CompanyInfo.CompanyName);
            request.CompanyId = Convert.ToString(SessionPersister.LoginedUser.CompanyInfo.CompanyId);
            ResponseModel response = new ResponseModel();
            response = MailHelper.SendEmail(request);
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        private async Task SafeGenerateExport(ReportFilterRequest model, string folderPath, string fileName, string type)
        {
            try
            {
                await Task.Yield(); // ensures async execution starts properly
                GenerateExport(model, folderPath, fileName, type);
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
            }
        }

        private ResponseModel GenerateExport(ReportFilterRequest model, string folderPath, string fileName, string type)
        {
            var response = new ResponseModel();

            try
            {
                DataSet dst = dst = reportBusiness.executeMISReportExport(model);
                if (dst == null || dst.Tables.Count == 0 || dst.Tables[0].Rows.Count == 0)
                {
                    response.IsSuccess = false;
                    response.Response = "No data found to export.";
                    return response;
                }
                if (type == "pdf")
                {
                    PdfExporter.ExportDataSetToPdf(dst, folderPath, fileName, model);
                }
                else
                {
                    CsvExporter.ExportDataSetToCsv(dst, folderPath, fileName, model);
                }
                response.IsSuccess = true;
                response.Response = fileName;
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
                response.IsSuccess = false;
                response.Response = $"Error occurred during export: {ex.Message}";
            }

            return response;
        }

        private static void EnsureDirectoryExists(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }

        private static void CleanupOldFiles(string folderPath, string reportTitle)
        {
            var oldFiles = Directory
                .EnumerateFiles(folderPath)
                .Where(f => Path.GetFileName(f)
                    .StartsWith(reportTitle, StringComparison.OrdinalIgnoreCase))
                .ToList();

            foreach (var file in oldFiles)
            {
                try
                {
                    System.IO.File.Delete(file);
                }
                catch
                {
                    // Log delete failure if needed
                }
            }
        }
        #endregion

        #endregion

        public JsonResult getReportDataLargeColumns(int draw, int start, int length, string search, List<PageFieldData> FieldData, int ReportId)
        {
            string sessionKey = $"MIS_REPORT_{ReportId}_{Convert.ToString(SessionPersister.LoginedUser.UserId.Value)}";
            string sessionKeyfilterd = $"MIS_REPORT_FILTERDATA_{ReportId}_{Convert.ToString(SessionPersister.LoginedUser.UserId.Value)}";
            string sessionKeyColumn = $"MIS_REPORT_{ReportId}_COLUMN_{Convert.ToString(SessionPersister.LoginedUser.UserId.Value)}";
            Session.Remove(sessionKey);
            Session.Remove(sessionKeyfilterd);
            ReportFilterRequest model = new ReportFilterRequest();
            model.FieldData = FieldData;
            model.ReportId = ReportId;
            model.UserId = Convert.ToString(SessionPersister.LoginedUser.UserId.Value);
            model.CompanyId = Convert.ToString(SessionPersister.LoginedUser.CompanyInfo.CompanyId);
            model.FinacialYearId = SessionPersister.LoginedUser.CompanyInfo.DefaultFiancialId;
            model.SearchTerm = search;
            // DataTables params
            var draw1 = Request.Form["draw"];
            var start1 = Convert.ToInt32(Request.Form["start"]);
            var length1 = Convert.ToInt32(Request.Form["length"]);
            var searchValue = Request.Form["search[value]"];

            // Fix: PageNumber calculation
            model.PageNumber = (start / length) + 1;
            model.PageSize = length;
            model.SearchTerm = search;
            ReportData report = new ReportData();
            var cached = Session[sessionKeyColumn] as ReportData;
            if (cached != null)
            {
                report = cached;
            }
            else
            {
                report = reportBusiness.executeMISReportColumns(model);
                Session[sessionKeyColumn] = report;
            }
            // Create JsonResult manually
            return new JsonResult
            {
                Data = new
                {
                    draw = draw,
                    recordsTotal = report.TotalRecords,
                    recordsFiltered = report.TotalRecords,
                    data = report.Data,
                    columns = report.columns,
                    secondColumns = report.secondColumns
                },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                MaxJsonLength = int.MaxValue   // avoid truncation for large datasets
            };
        }

        #region Option2
        private DataSet ConvertToDataSet(List<Dictionary<string, object>> data, string tableName = "Table1")
        {
            DataSet dataSet = new DataSet();
            DataTable table = new DataTable(tableName);
            if (data == null || data.Count == 0)
            {
                dataSet.Tables.Add(table);
                return dataSet;
            }
            // Create columns from all keys
            foreach (var key in data.SelectMany(d => d.Keys).Distinct())
            {
                table.Columns.Add(key, typeof(object));
            }

            // Add rows
            foreach (var dict in data)
            {
                DataRow row = table.NewRow();
                foreach (var kv in dict)
                {
                    row[kv.Key] = kv.Value ?? DBNull.Value;
                }
                table.Rows.Add(row);
            }
            dataSet.Tables.Add(table);
            return dataSet;
        }
        private ReportFilterRequest BuildRequestModel(int start, int length, string search, List<PageFieldData> FieldData, int ReportId)
        {
            ReportFilterRequest model = new ReportFilterRequest();
            model.FieldData = FieldData;
            model.ReportId = ReportId;
            model.UserId = Convert.ToString(SessionPersister.LoginedUser.UserId.Value);
            model.CompanyId = Convert.ToString(SessionPersister.LoginedUser.CompanyInfo.CompanyId);
            model.FinacialYearId = SessionPersister.LoginedUser.CompanyInfo.DefaultFiancialId;
            model.SearchTerm = search;
            // Fix: PageNumber calculation
            model.PageNumber = (start / length) + 1;
            model.PageSize = length;
            model.SearchTerm = search;
            return model;
        }
        private Dictionary<string, string> GetColumnSearches()
        {
            Dictionary<string, string> columnSearches = new Dictionary<string, string>();
            for (int i = 0; ; i++)
            {
                string columnData = Request.Form[$"columns[{i}][data]"];
                if (columnData == null) break;
                string searchValues = Request.Form[$"columns[{i}][search][value]"];
                if (!string.IsNullOrWhiteSpace(searchValues))
                {
                    columnSearches.Add(columnData, searchValues);
                }
            }
            return columnSearches;
        }
        public async Task<JsonResult> getReportDataLarge2(int draw, int start, int length, string search, List<PageFieldData> FieldData, int ReportId)
        {
            string sessionKey = $"MIS_REPORT_{ReportId}_{SessionPersister.LoginedUser.UserId.Value}";
            string sessionKeyFiltered = $"MIS_REPORT_FILTERDATA_{ReportId}_{SessionPersister.LoginedUser.UserId.Value}";
            var draw1 = Request.Form["draw"];
            var start1 = Convert.ToInt32(Request.Form["start"]);
            var length1 = Convert.ToInt32(Request.Form["length"]);
            var model = BuildRequestModel(start1, length1, search, FieldData, ReportId);

            // Column search
            var columnSearches = GetColumnSearches();

            // Try FULL DATA session
            var fullCache = Session[sessionKey] as ReportSessionCache;

            var cache = MemoryCache.Default;
            var result = cache.Get(sessionKey) as ReportSessionCache;
            if (result != null)
            {
                fullCache = result;
            }
            // 🔹 LOAD FULL DATA ONCE
            if (fullCache == null || !IsSameBaseRequest2(fullCache.Request, model))
            {
                var report = reportBusiness.executeMISReport(model);
                if (report.Data == null)
                {
                    report.Data = new List<Dictionary<string, object>>();
                }
                fullCache = new ReportSessionCache
                {
                    Request = model,
                    Data = report.Data,                // FULL DATA
                    TotalRecords = report.TotalRecords,
                    Columns = report.columns,
                    ColumnSearches = columnSearches
                };

                Session[sessionKey] = fullCache;

                // Background refresh (optional)
                StartBackgroundJob(model, sessionKey);

            }
            else if (fullCache.Data != null && fullCache.Data.Count != fullCache.TotalRecords)
            {
                StartBackgroundJob(model, sessionKey);
            }
            if (fullCache.Data == null)
            {
                fullCache.Data = new List<Dictionary<string, object>>();
            }
            var filteredData = fullCache.Data;
            if (fullCache.Data != null
                && columnSearches != null && columnSearches.Count > 0)
            {
                // 🔹 APPLY FILTERS ON FULL DATA
                filteredData = ApplyFilters(fullCache.Data, search, columnSearches);
                int counttotal = fullCache.TotalRecords;
                if (columnSearches != null && columnSearches.Count > 0)
                {
                    counttotal = filteredData.Count;
                }
                // Save FILTERED result
                Session[sessionKeyFiltered] = new ReportSessionCache
                {
                    Request = model,
                    Data = filteredData,
                    TotalRecords = counttotal,
                    Columns = fullCache.Columns,
                    ColumnSearches = columnSearches
                };
                // 🔹 PAGINATION (AFTER FILTERING)
                var pagedData = filteredData
                    .Skip(start)
                    .Take(length)
                    .ToList();
                return new JsonResult
                {
                    Data = new
                    {
                        draw = draw,
                        recordsTotal = counttotal,
                        recordsFiltered = counttotal,
                        data = pagedData,
                        columns = fullCache.Columns
                    },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    MaxJsonLength = int.MaxValue
                };
            }
            else
            {
                var pagedData = filteredData
                   .Skip(start)
                   .Take(length)
                   .ToList();
                // Save FILTERED result
                Session[sessionKeyFiltered] = new ReportSessionCache
                {

                    Request = model,
                    Data = pagedData,
                    TotalRecords = fullCache.TotalRecords,
                    Columns = fullCache.Columns,
                    ColumnSearches = columnSearches
                };
                return new JsonResult
                {
                    Data = new
                    {
                        draw = draw,
                        recordsTotal = fullCache.TotalRecords,
                        recordsFiltered = fullCache.TotalRecords,
                        data = fullCache.Data,
                        columns = fullCache.Columns
                    },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    MaxJsonLength = int.MaxValue
                };
            }
        }
        public void StartBackgroundJob(ReportFilterRequest model, string sessionKey)
        {
            Task.Run(async () =>
            {
                try
                {
                    var task = Task.Run(() => LoadDataInBackground(model, sessionKey));
                    _runningTasks[sessionKey] = task;
                }
                catch (Exception ex)
                {
                    CommonBusiness.LogEx(ex);
                    // log exception
                }
            });
        }
        private async Task LoadDataInBackground(ReportFilterRequest model, string cacheKey)
        {
            string connectionstring =
                ConfigurationManager.ConnectionStrings["TFDSolutionEntities"].ConnectionString;

            var report = await reportBusiness.ExecuteMISReportAsync(model, connectionstring);
            var cache = MemoryCache.Default;

            cache.Set(
                cacheKey,
                new ReportSessionCache
                {
                    Request = model,
                    Data = report.Data,
                    TotalRecords = report.TotalRecords,
                    Columns = report.columns
                },
                DateTimeOffset.Now.AddMinutes(30)
            );
            //Session[cacheKey] = new ReportSessionCache
            //{
            //    Request = model,
            //    Data = report.Data,
            //    TotalRecords = report.TotalRecords,
            //    Columns = report.columns
            //};
        }
        private void LoadAllReportData(ReportFilterRequest model, string sessionKey)
        {
            model.PageSize = 10000000;
            var report = reportBusiness.executeMISReport(model);

            Session[sessionKey] = new ReportSessionCache
            {
                Request = model,
                Data = report.Data,
                TotalRecords = report.TotalRecords,
                Columns = report.columns
            };
        }
        private List<Dictionary<string, object>> ApplyFilters(List<Dictionary<string, object>> source, string globalSearch, Dictionary<string, string> columnSearches)
        {
            var query = source.AsEnumerable();

            // Global search
            if (!string.IsNullOrWhiteSpace(globalSearch))
            {
                query = query.Where(row =>
                    row.Values.Any(v =>
                        v != null &&
                        v.ToString().IndexOf(globalSearch, StringComparison.OrdinalIgnoreCase) >= 0));
            }

            // Column search
            if (columnSearches.Any())
            {
                query = query.Where(row =>
                    columnSearches.All(f =>
                        row.ContainsKey(f.Key) &&
                        row[f.Key] != null &&
                        row[f.Key].ToString()
                            .IndexOf(f.Value, StringComparison.OrdinalIgnoreCase) >= 0));
            }

            return query.ToList();
        }
        private bool IsSameBaseRequest2(ReportFilterRequest oldReq, ReportFilterRequest newReq)
        {
            return
                oldReq.ReportId == newReq.ReportId &&
                oldReq.UserId == newReq.UserId &&
                oldReq.CompanyId == newReq.CompanyId &&
                oldReq.FinacialYearId == newReq.FinacialYearId &&
                Newtonsoft.Json.JsonConvert.SerializeObject(oldReq.FieldData)
                == Newtonsoft.Json.JsonConvert.SerializeObject(newReq.FieldData);
        }
        #endregion

        #region Account Ledger
        public ActionResult AccountLedger()
        {
            string sessionKey = $"LEDGER_REPORT_{SessionPersister.LoginedUser.UserId.Value}";
            string sessionKeyFiltered = $"LEDGER_REPORT_FILTERDATA_{SessionPersister.LoginedUser.UserId.Value}";
            Session.Remove(sessionKey);
            Session.Remove(sessionKeyFiltered);
            List<ReportMast> reports = CommonBusiness.getFormSystemReportSelection("AccountLedger");
            ViewBag.ReportSelection = reports;
            string pageTitle = "Account Ledger";
            if (TempData["AccountLedgerRedirection"] != null)
            {
                AccountLedgerOpenModel model = TempData["AccountLedgerRedirection"] as AccountLedgerOpenModel;
                ViewBag.AccountId = model.AccountId;
                pageTitle = model.AccountLedger;
                ViewBag.AccountLedger = model.AccountLedger;
                ViewBag.FromDate = model.FromDate.ToString("yyyy-MM-dd");
                ViewBag.ToDate = model.ToDate.ToString("yyyy-MM-dd");
            }
            ViewBag.PageTitle = pageTitle;
            return View();
        }

        public ActionResult RedirectToLedger(AccountLedgerOpenModel model)
        {
            if (model.AccountId > 0)
            {
                model.AccountLedger = Convert.ToString(model.AccountLedger).Trim();
                TempData["AccountLedgerRedirection"] = model;
                return Json(new { success = true, redirectUrl = Url.Action("AccountLedger") });
            }
            return Json(new { success = false, message = "Please select ledger" });
        }
        public async Task<JsonResult> GetLedgerReport(int draw, int start, int length, string search, LedgerReportRequest model)
        {
            LedgerReportResult reportResult = new LedgerReportResult();
            string sessionKey = $"LEDGER_REPORT_{SessionPersister.LoginedUser.UserId.Value}";
            string sessionKeyFiltered = $"LEDGER_REPORT_FILTERDATA_{SessionPersister.LoginedUser.UserId.Value}";
            var draw1 = Request.Form["draw"];
            var start1 = Convert.ToInt32(Request.Form["start"]);
            var length1 = Convert.ToInt32(Request.Form["length"]);
            model.PageNumber = (start / length) + 1;
            model.PageSize = 1000000;
            model.CompanyId = Convert.ToString(SessionPersister.LoginedUser.CompanyInfo.CompanyId);
            model.UserId = Convert.ToString(SessionPersister.LoginedUser.UserId.Value);
            model.FinacialYearId = Convert.ToInt32(SessionPersister.LoginedUser.CompanyInfo.DefaultFiancialId);
            // Try FULL DATA session
            var fullCache = Session[sessionKey] as LedgerReportSessionCache;
            var cache = MemoryCache.Default;
            var result = cache.Get(sessionKey) as LedgerReportSessionCache;
            if (result != null)
            {
                fullCache = result;
            }

            // 🔹 LOAD FULL DATA ONCE
            LedgerReportResult report = await reportBusiness.RunLedgerReport(model);
            fullCache = new LedgerReportSessionCache
            {
                Request = model,
                Data = report.Data,                // FULL DATA
                TotalRecords = report.TotalRecords
            };

            Session[sessionKey] = fullCache;

            // Background refresh (optional)
            StartLedgerBackgroundJob(model, sessionKey);
            var filteredData = fullCache.Data;
            if (fullCache.Data != null && search != null && search.Length > 0)
            {
                // 🔹 APPLY FILTERS ON FULL DATA
                filteredData = ApplyLedgerFilters(fullCache.Data, search);
                int counttotal = fullCache.TotalRecords;
                if (search != null && search.Length > 0)
                {
                    counttotal = filteredData.Count;
                }
                // Save FILTERED result
                Session[sessionKeyFiltered] = new LedgerReportSessionCache
                {
                    Request = model,
                    Data = filteredData,
                    TotalRecords = counttotal
                };
                // 🔹 PAGINATION (AFTER FILTERING)
                var pagedData = filteredData
                    .Skip(start)
                    .Take(length)
                    .ToList();
                return new JsonResult
                {
                    Data = new
                    {
                        draw = draw,
                        recordsTotal = counttotal,
                        recordsFiltered = counttotal,
                        data = pagedData,
                        TotalRecords = fullCache.TotalRecords,
                    },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    MaxJsonLength = int.MaxValue
                };
            }
            else
            {
                var pagedData = filteredData
                   .Skip(start)
                   .Take(length)
                   .ToList();
                // Save FILTERED result
                Session[sessionKeyFiltered] = new LedgerReportSessionCache
                {
                    Request = model,
                    Data = pagedData,
                    TotalRecords = fullCache.TotalRecords
                };
                return new JsonResult
                {
                    Data = new
                    {
                        draw = draw,
                        recordsTotal = fullCache.TotalRecords,
                        recordsFiltered = fullCache.TotalRecords,
                        data = fullCache.Data,
                        TotalRecords = fullCache.TotalRecords,
                    },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    MaxJsonLength = int.MaxValue
                };
            }
        }
        private List<LedgerReportData> ApplyLedgerFilters(List<LedgerReportData> source, string globalSearch)
        {
            var query = source.AsQueryable();
            return query.ToList();
        }

        private bool IsSameBaseLedgerRequest(LedgerReportRequest oldReq, LedgerReportRequest newReq)
        {
            return
                oldReq.FromDate == newReq.FromDate &&
                oldReq.AccountId == newReq.AccountId &&
                oldReq.ToDate == newReq.ToDate &&
                oldReq.CompanyId == newReq.CompanyId &&
                oldReq.FinacialYearId == newReq.FinacialYearId;
        }
        public void StartLedgerBackgroundJob(LedgerReportRequest model, string sessionKey)
        {
            Task.Run(async () =>
            {
                try
                {
                    var task = Task.Run(() => LoadLedgerDataInBackground(model, sessionKey));
                    _runningTasks[sessionKey] = task;
                }
                catch (Exception ex)
                {
                    CommonBusiness.LogEx(ex);
                    // log exception
                }
            });
        }
        private async Task LoadLedgerDataInBackground(LedgerReportRequest model, string cacheKey)
        {
            int pageZie = model.PageSize;
            model.PageSize = 10000000;
            var report = await reportBusiness.RunLedgerReport(model);
            var cache = MemoryCache.Default;
            model.PageSize = pageZie;
            cache.Set(
                cacheKey,
                new LedgerReportSessionCache
                {
                    Request = model,
                    Data = report.Data,
                    TotalRecords = report.TotalRecords
                },
                DateTimeOffset.Now.AddMinutes(30)
            );
        }

        public async Task<ActionResult> ExportAccountLedgerReport(LedgerReportRequest model)
        {
            var response = new ResponseModel
            {
                IsSuccess = true,
                Response = "Export started successfully."
            };

            try
            {
                var user = SessionPersister.LoginedUser;
                //model.UserId = Convert.ToString(user.UserId.Value);
                model.CompanyId = Convert.ToString(user.CompanyInfo.CompanyId);
                model.CompanyName = user.CompanyInfo.CompanyName;
                model.FinacialYearId = user.CompanyInfo.DefaultFiancialId;

                string folderPath = Server.MapPath("~/TempDownload");

                EnsureDirectoryExists(folderPath);
                CleanupOldFiles(folderPath, "ACCOUNTLEDGER");

                string fileName = $"ACCOUNTLEDGER_REPORT_" + Guid.NewGuid().ToString();
                string fullFilePath = Path.Combine(folderPath, fileName);
                response.Action = fileName + ".xlsx";
                if (System.IO.File.Exists(fullFilePath))
                    System.IO.File.Delete(fullFilePath);

                string sessionKey = $"LEDGER_REPORT_{SessionPersister.LoginedUser.UserId.Value}";
                string sessionKeyFiltered = $"LEDGER_REPORT_FILTERDATA_{SessionPersister.LoginedUser.UserId.Value}";
                DataSet dst = new DataSet();
                string cacheKey = $"ACCOUNTLEDGER_REPORT_{SessionPersister.LoginedUser.UserId}";

                if (_runningTasks.TryGetValue(cacheKey, out Task runningTask))
                {
                    await runningTask; // ⏳ WAIT
                }
                var fullCache = Session[sessionKeyFiltered] as LedgerReportSessionCache;
                var fulldata = Session[sessionKey] as LedgerReportSessionCache;
                var cache = MemoryCache.Default;
                var result = cache.Get(sessionKey) as LedgerReportSessionCache;
                if (result != null)
                {
                    fulldata = result;
                }
                fulldata.Request.CompanyName = model.CompanyName;
                dst = ConvertToDataSet(fulldata.Data);
                if (dst == null || dst.Tables.Count == 0 || dst.Tables[0].Rows.Count == 0)
                {
                    response.IsSuccess = false;
                    response.Response = "No data found to export.";
                }
                //CsvExporter.ExportLedgerToCsv(fulldata, folderPath, fileName);
                CsvExporter.ExportLedgerExcel(fulldata, folderPath, fileName);
                response.IsSuccess = true;
                response.Response = fileName + ".xlsx";
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Response = $"Error starting export: {ex.Message}";
            }

            // Return immediately (don’t wait for export)
            return Json(response, JsonRequestBehavior.AllowGet);
        }
        public static DataSet ConvertToDataSet<T>(List<T> data, string tableName = "Ledger")
        {
            DataSet ds = new DataSet();
            DataTable dt = new DataTable(tableName);

            if (data == null || data.Count == 0)
            {
                ds.Tables.Add(dt);
                return ds;
            }

            var props = typeof(T).GetProperties();

            // Create columns
            foreach (var prop in props)
            {
                Type colType = Nullable.GetUnderlyingType(prop.PropertyType)
                               ?? prop.PropertyType;
                dt.Columns.Add(prop.Name, colType);
            }

            // Add rows
            foreach (var item in data)
            {
                DataRow row = dt.NewRow();
                foreach (var prop in props)
                    row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;

                dt.Rows.Add(row);
            }

            ds.Tables.Add(dt);
            return ds;
        }

        #endregion

        #region TrialBalanceReport
        public ActionResult TrialBalance()
        {
            string sessionKey = $"TRIALBALANCE_REPORT_{SessionPersister.LoginedUser.UserId.Value}";
            string sessionKeyFiltered = $"TRIALBALANCE_REPORT_FILTERDATA_{SessionPersister.LoginedUser.UserId.Value}";
            Session.Remove(sessionKey);
            Session.Remove(sessionKeyFiltered);
            List<ReportMast> reports = CommonBusiness.getFormSystemReportSelection("TrialBalance");
            // Pass it to the view using ViewBag
            ViewBag.ReportSelection = reports;
            return View();
        }
        private List<TrailBalanceReportData> ApplyTrialBalanceFilters(List<TrailBalanceReportData> source, string globalSearch)
        {
            var query = source.AsQueryable();
            return query.ToList();
        }
        private List<ProfitLossReportData> ApplyProfitLossFilters(List<ProfitLossReportData> source, string globalSearch)
        {
            var query = source.AsQueryable();
            return query.ToList();
        }
        private List<BalanceSheetReportData> ApplyBalanceSheetFilters(List<BalanceSheetReportData> source, string globalSearch)
        {
            var query = source.AsQueryable();
            return query.ToList();
        }
        public async Task<JsonResult> GetTrialBalanceReport(int draw, int start, int length, string search, LedgerReportRequest model)
        {
            LedgerReportResult reportResult = new LedgerReportResult();
            string sessionKey = $"TRIALBALANCE_REPORT_{SessionPersister.LoginedUser.UserId.Value}";
            string sessionKeyFiltered = $"TRIALBALANCE_REPORT_FILTERDATA_{SessionPersister.LoginedUser.UserId.Value}";
            var draw1 = Request.Form["draw"];
            var start1 = Convert.ToInt32(Request.Form["start"]);
            var length1 = Convert.ToInt32(Request.Form["length"]);
            model.PageNumber = (start / length) + 1;
            model.PageSize = 1000000;
            model.CompanyId = Convert.ToString(SessionPersister.LoginedUser.CompanyInfo.CompanyId);
            model.UserId = Convert.ToString(SessionPersister.LoginedUser.UserId.Value);
            model.FinacialYearId = Convert.ToInt32(SessionPersister.LoginedUser.CompanyInfo.DefaultFiancialId);
            // Try FULL DATA session
            var fullCache = Session[sessionKey] as TrailBalanceReportSessionCache;
            var cache = MemoryCache.Default;
            var result = cache.Get(sessionKey) as TrailBalanceReportSessionCache;
            if (result != null)
            {
                fullCache = result;
            }

            // 🔹 LOAD FULL DATA ONCE
            TrailBalanceReportResult report = await reportBusiness.RunTrialBalanceReport(model);
            fullCache = new TrailBalanceReportSessionCache
            {
                Request = model,
                Data = report.Data,                // FULL DATA
                TotalRecords = report.TotalRecords
            };

            Session[sessionKey] = fullCache;

            // Background refresh (optional)
            StartLedgerBackgroundJob(model, sessionKey);
            var filteredData = fullCache.Data;
            if (fullCache.Data != null && search != null && search.Length > 0)
            {
                // 🔹 APPLY FILTERS ON FULL DATA
                filteredData = ApplyTrialBalanceFilters(fullCache.Data, search);
                int counttotal = fullCache.TotalRecords;
                if (search != null && search.Length > 0)
                {
                    counttotal = filteredData.Count;
                }
                // Save FILTERED result
                Session[sessionKeyFiltered] = new TrailBalanceReportSessionCache
                {
                    Request = model,
                    Data = filteredData,
                    TotalRecords = counttotal
                };
                // 🔹 PAGINATION (AFTER FILTERING)
                var pagedData = filteredData
                    .Skip(start)
                    .Take(length)
                    .ToList();
                return new JsonResult
                {
                    Data = new
                    {
                        draw = draw,
                        recordsTotal = counttotal,
                        recordsFiltered = counttotal,
                        data = pagedData,
                        TotalRecords = fullCache.TotalRecords,
                    },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    MaxJsonLength = int.MaxValue
                };
            }
            else
            {
                var pagedData = filteredData
                   .Skip(start)
                   .Take(length)
                   .ToList();
                // Save FILTERED result
                Session[sessionKeyFiltered] = new TrailBalanceReportSessionCache
                {
                    Request = model,
                    Data = pagedData,
                    TotalRecords = fullCache.TotalRecords
                };
                return new JsonResult
                {
                    Data = new
                    {
                        draw = draw,
                        recordsTotal = fullCache.TotalRecords,
                        recordsFiltered = fullCache.TotalRecords,
                        data = fullCache.Data,
                        TotalRecords = fullCache.TotalRecords,
                    },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    MaxJsonLength = int.MaxValue
                };
            }
        }

        public async Task<ActionResult> ExportTrialBalanceReport(LedgerReportRequest model)
        {
            var response = new ResponseModel { IsSuccess = true, Response = "Export started successfully." };
            try
            {
                var user = SessionPersister.LoginedUser;
                //model.UserId = Convert.ToString(user.UserId.Value);
                model.CompanyId = Convert.ToString(user.CompanyInfo.CompanyId);
                model.CompanyName = user.CompanyInfo.CompanyName;
                model.FinacialYearId = user.CompanyInfo.DefaultFiancialId;

                string folderPath = Server.MapPath("~/TempDownload");

                EnsureDirectoryExists(folderPath);
                CleanupOldFiles(folderPath, "TRIALBALANCE");

                string fileName = $"TRIALBALANCE_REPORT_" + Guid.NewGuid().ToString();
                string fullFilePath = Path.Combine(folderPath, fileName);
                response.Action = fileName + ".xlsx";
                if (System.IO.File.Exists(fullFilePath))
                    System.IO.File.Delete(fullFilePath);

                string sessionKey = $"TRIALBALANCE_REPORT_{SessionPersister.LoginedUser.UserId.Value}";
                string sessionKeyFiltered = $"TRIALBALANCE_REPORT_FILTERDATA_{SessionPersister.LoginedUser.UserId.Value}";
                DataSet dst = new DataSet();
                string cacheKey = $"TRIALBALANCE_REPORT_{SessionPersister.LoginedUser.UserId}";

                if (_runningTasks.TryGetValue(cacheKey, out Task runningTask))
                {
                    await runningTask; // ⏳ WAIT
                }
                var fullCache = Session[sessionKeyFiltered] as TrailBalanceReportSessionCache;
                var fulldata = Session[sessionKey] as TrailBalanceReportSessionCache;
                var cache = MemoryCache.Default;
                var result = cache.Get(sessionKey) as TrailBalanceReportSessionCache;
                if (result != null)
                {
                    fulldata = result;
                }
                fulldata.Request.CompanyName = model.CompanyName;
                dst = ConvertToDataSet(fulldata.Data);
                if (dst == null || dst.Tables.Count == 0 || dst.Tables[0].Rows.Count == 0)
                {
                    response.IsSuccess = false;
                    response.Response = "No data found to export.";
                }
                //CsvExporter.ExportLedgerToCsv(fulldata, folderPath, fileName);
                CsvExporter.ExportTrailBalanceExcel(fulldata, folderPath, fileName);
                response.IsSuccess = true;
                response.Response = fileName + ".xlsx";
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Response = $"Error starting export: {ex.Message}";
            }

            // Return immediately (don’t wait for export)
            return Json(response, JsonRequestBehavior.AllowGet);
        }
        public async Task<ActionResult> ExportProfitLossReport(LedgerReportRequest model)
        {
            var response = new ResponseModel { IsSuccess = true, Response = "Export started successfully." };
            try
            {
                var user = SessionPersister.LoginedUser;
                //model.UserId = Convert.ToString(user.UserId.Value);
                model.CompanyId = Convert.ToString(user.CompanyInfo.CompanyId);
                model.CompanyName = user.CompanyInfo.CompanyName;
                model.FinacialYearId = user.CompanyInfo.DefaultFiancialId;

                string folderPath = Server.MapPath("~/TempDownload");

                EnsureDirectoryExists(folderPath);
                CleanupOldFiles(folderPath, "PROFITLOSS");

                string fileName = $"PROFITLOSS_REPORT_" + Guid.NewGuid().ToString();
                string fullFilePath = Path.Combine(folderPath, fileName);
                response.Action = fileName + ".xlsx";
                if (System.IO.File.Exists(fullFilePath))
                    System.IO.File.Delete(fullFilePath);

                string sessionKey = $"PROFITLOSS_REPORT_{SessionPersister.LoginedUser.UserId.Value}";
                string sessionKeyFiltered = $"PROFITLOSS_REPORT_FILTERDATA_{SessionPersister.LoginedUser.UserId.Value}";
                DataSet dst = new DataSet();
                string cacheKey = $"PROFITLOSS_REPORT_{SessionPersister.LoginedUser.UserId}";

                if (_runningTasks.TryGetValue(cacheKey, out Task runningTask))
                {
                    await runningTask; // ⏳ WAIT
                }
                var fullCache = Session[sessionKeyFiltered] as ProfitLossReportSessionCache;
                var fulldata = Session[sessionKey] as ProfitLossReportSessionCache;
                var cache = MemoryCache.Default;
                var result = cache.Get(sessionKey) as ProfitLossReportSessionCache;
                if (result != null)
                {
                    fulldata = result;
                }
                fulldata.Request.CompanyName = model.CompanyName;
                dst = ConvertToDataSet(fulldata.Data);
                if (dst == null || dst.Tables.Count == 0 || dst.Tables[0].Rows.Count == 0)
                {
                    response.IsSuccess = false;
                    response.Response = "No data found to export.";
                }
                //CsvExporter.ExportLedgerToCsv(fulldata, folderPath, fileName);
                CsvExporter.ExportProfitLossExcel(fulldata, folderPath, fileName);
                response.IsSuccess = true;
                response.Response = fileName + ".xlsx";
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Response = $"Error starting export: {ex.Message}";
            }

            // Return immediately (don’t wait for export)
            return Json(response, JsonRequestBehavior.AllowGet);
        }
        public async Task<JsonResult> GetProfitLossReport(int draw, int start, int length, string search, LedgerReportRequest model)
        {
            LedgerReportResult reportResult = new LedgerReportResult();
            string sessionKey = $"PROFITLOSS_REPORT_{SessionPersister.LoginedUser.UserId.Value}";
            string sessionKeyFiltered = $"PROFITLOSS_REPORT_FILTERDATA_{SessionPersister.LoginedUser.UserId.Value}";
            var draw1 = Request.Form["draw"];
            var start1 = Convert.ToInt32(Request.Form["start"]);
            var length1 = Convert.ToInt32(Request.Form["length"]);
            model.PageNumber = (start / length) + 1;
            model.PageSize = 1000000;
            model.CompanyId = Convert.ToString(SessionPersister.LoginedUser.CompanyInfo.CompanyId);
            model.UserId = Convert.ToString(SessionPersister.LoginedUser.UserId.Value);
            model.FinacialYearId = Convert.ToInt32(SessionPersister.LoginedUser.CompanyInfo.DefaultFiancialId);
            // Try FULL DATA session
            var fullCache = Session[sessionKey] as ProfitLossReportSessionCache;
            var cache = MemoryCache.Default;
            var result = cache.Get(sessionKey) as ProfitLossReportSessionCache;
            if (result != null)
            {
                fullCache = result;
            }

            // 🔹 LOAD FULL DATA ONCE
            ProfitLossReportResult report = await reportBusiness.RunProfitLossReport(model);
            fullCache = new ProfitLossReportSessionCache
            {
                Request = model,
                Data = report.Data,                // FULL DATA
                TotalRecords = 10
            };

            Session[sessionKey] = fullCache;

            // Background refresh (optional)
            StartLedgerBackgroundJob(model, sessionKey);
            var filteredData = fullCache.Data;
            if (fullCache.Data != null && search != null && search.Length > 0)
            {
                // 🔹 APPLY FILTERS ON FULL DATA
                filteredData = ApplyProfitLossFilters(fullCache.Data, search);
                int counttotal = fullCache.TotalRecords;
                if (search != null && search.Length > 0)
                {
                    counttotal = filteredData.Count;
                }
                // Save FILTERED result
                Session[sessionKeyFiltered] = new ProfitLossReportSessionCache
                {
                    Request = model,
                    Data = filteredData,
                    TotalRecords = counttotal
                };
                // 🔹 PAGINATION (AFTER FILTERING)
                var pagedData = filteredData
                    .Skip(start)
                    .Take(length)
                    .ToList();
                return new JsonResult
                {
                    Data = new
                    {
                        draw = draw,
                        recordsTotal = counttotal,
                        recordsFiltered = counttotal,
                        data = pagedData,
                        totalRecords = fullCache.TotalRecords,
                        totalIncome = report.TotalIncome,
                        totalExpense = report.TotalExpense,
                        netProfit = report.NetProfit,
                    },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    MaxJsonLength = int.MaxValue
                };
            }
            else
            {
                var pagedData = filteredData
                   .Skip(start)
                   .Take(length)
                   .ToList();
                // Save FILTERED result
                Session[sessionKeyFiltered] = new ProfitLossReportSessionCache
                {
                    Request = model,
                    Data = pagedData,
                    TotalRecords = fullCache.TotalRecords
                };
                return new JsonResult
                {
                    Data = new
                    {
                        draw = draw,
                        recordsTotal = fullCache.TotalRecords,
                        recordsFiltered = fullCache.TotalRecords,
                        data = fullCache.Data,
                        totalRecords = fullCache.TotalRecords,
                        totalIncome = report.TotalIncome,
                        totalExpense = report.TotalExpense,
                        netProfit = report.NetProfit,
                    },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    MaxJsonLength = int.MaxValue
                };
            }
        }
        public async Task<ActionResult> ExportBalanceSheetReport(LedgerReportRequest model)
        {
            var response = new ResponseModel { IsSuccess = true, Response = "Export started successfully." };
            try
            {
                var user = SessionPersister.LoginedUser;
                //model.UserId = Convert.ToString(user.UserId.Value);
                model.CompanyId = Convert.ToString(user.CompanyInfo.CompanyId);
                model.CompanyName = user.CompanyInfo.CompanyName;
                model.FinacialYearId = user.CompanyInfo.DefaultFiancialId;

                string folderPath = Server.MapPath("~/TempDownload");

                EnsureDirectoryExists(folderPath);
                CleanupOldFiles(folderPath, "BALANCESHEET");

                string fileName = $"BALANCESHEET_REPORT_" + Guid.NewGuid().ToString();
                string fullFilePath = Path.Combine(folderPath, fileName);
                response.Action = fileName + ".xlsx";
                if (System.IO.File.Exists(fullFilePath))
                    System.IO.File.Delete(fullFilePath);

                string sessionKey = $"BALANCESHEET_REPORT_{SessionPersister.LoginedUser.UserId.Value}";
                string sessionKeyFiltered = $"BALANCESHEET_REPORT_FILTERDATA_{SessionPersister.LoginedUser.UserId.Value}";
                DataSet dst = new DataSet();
                string cacheKey = $"BALANCESHEET_REPORT_{SessionPersister.LoginedUser.UserId}";

                if (_runningTasks.TryGetValue(cacheKey, out Task runningTask))
                {
                    await runningTask; // ⏳ WAIT
                }
                var fullCache = Session[sessionKeyFiltered] as BalanceSheetReportSessionCache;
                var fulldata = Session[sessionKey] as BalanceSheetReportSessionCache;
                var cache = MemoryCache.Default;
                var result = cache.Get(sessionKey) as BalanceSheetReportSessionCache;
                if (result != null)
                {
                    fulldata = result;
                }
                fulldata.Request.CompanyName = model.CompanyName;
                dst = ConvertToDataSet(fulldata.Data);
                if (dst == null || dst.Tables.Count == 0 || dst.Tables[0].Rows.Count == 0)
                {
                    response.IsSuccess = false;
                    response.Response = "No data found to export.";
                }
                //CsvExporter.ExportLedgerToCsv(fulldata, folderPath, fileName);
                CsvExporter.ExportBalanceSheetExcel(fulldata, folderPath, fileName);
                response.IsSuccess = true;
                response.Response = fileName + ".xlsx";
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Response = $"Error starting export: {ex.Message}";
            }

            // Return immediately (don’t wait for export)
            return Json(response, JsonRequestBehavior.AllowGet);
        }
        public async Task<JsonResult> GetBalanceSheetReport(int draw, int start, int length, string search, LedgerReportRequest model)
        {
            LedgerReportResult reportResult = new LedgerReportResult();
            string sessionKey = $"BALANCESHEET_REPORT_{SessionPersister.LoginedUser.UserId.Value}";
            string sessionKeyFiltered = $"BALANCESHEET_REPORT_FILTERDATA_{SessionPersister.LoginedUser.UserId.Value}";
            var draw1 = Request.Form["draw"];
            var start1 = Convert.ToInt32(Request.Form["start"]);
            var length1 = Convert.ToInt32(Request.Form["length"]);
            model.PageNumber = (start / length) + 1;
            model.PageSize = 1000000;
            model.CompanyId = Convert.ToString(SessionPersister.LoginedUser.CompanyInfo.CompanyId);
            model.UserId = Convert.ToString(SessionPersister.LoginedUser.UserId.Value);
            model.FinacialYearId = Convert.ToInt32(SessionPersister.LoginedUser.CompanyInfo.DefaultFiancialId);
            // Try FULL DATA session
            var fullCache = Session[sessionKey] as BalanceSheetReportSessionCache;
            var cache = MemoryCache.Default;
            var result = cache.Get(sessionKey) as BalanceSheetReportSessionCache;
            if (result != null)
            {
                fullCache = result;
            }

            // 🔹 LOAD FULL DATA ONCE
            BalanceSheetReportResult report = await reportBusiness.RunBalanceSheetReport(model);
            fullCache = new BalanceSheetReportSessionCache
            {
                Request = model,
                Data = report.Data,                // FULL DATA
                TotalRecords = 10
            };

            Session[sessionKey] = fullCache;

            // Background refresh (optional)
            StartLedgerBackgroundJob(model, sessionKey);
            var filteredData = fullCache.Data;
            if (fullCache.Data != null && search != null && search.Length > 0)
            {
                // 🔹 APPLY FILTERS ON FULL DATA
                filteredData = ApplyBalanceSheetFilters(fullCache.Data, search);
                int counttotal = fullCache.TotalRecords;
                if (search != null && search.Length > 0)
                {
                    counttotal = filteredData.Count;
                }
                // Save FILTERED result
                Session[sessionKeyFiltered] = new BalanceSheetReportSessionCache
                {
                    Request = model,
                    Data = filteredData,
                    TotalRecords = counttotal
                };
                // 🔹 PAGINATION (AFTER FILTERING)
                var pagedData = filteredData
                    .Skip(start)
                    .Take(length)
                    .ToList();
                return new JsonResult
                {
                    Data = new
                    {
                        draw = draw,
                        recordsTotal = counttotal,
                        recordsFiltered = counttotal,
                        data = pagedData,
                        TotalRecords = fullCache.TotalRecords,
                        totalAsset = report.TotalAsset,
                        totalLiability = report.TotalLiability,
                        difference = report.Difference
                    },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    MaxJsonLength = int.MaxValue
                };
            }
            else
            {
                var pagedData = filteredData
                   .Skip(start)
                   .Take(length)
                   .ToList();
                // Save FILTERED result
                Session[sessionKeyFiltered] = new BalanceSheetReportSessionCache
                {
                    Request = model,
                    Data = pagedData,
                    TotalRecords = fullCache.TotalRecords
                };
                return new JsonResult
                {
                    Data = new
                    {
                        draw = draw,
                        recordsTotal = fullCache.TotalRecords,
                        recordsFiltered = fullCache.TotalRecords,
                        data = fullCache.Data,
                        TotalRecords = fullCache.TotalRecords,
                        totalAsset = report.TotalAsset,
                        totalLiability = report.TotalLiability,
                        difference = report.Difference,
                    },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    MaxJsonLength = int.MaxValue
                };
            }
        }

        #endregion

        #region Common
        public ActionResult OpenSystemReportView(int ReportId, string TableName, List<SpecificationField> reportParameters)
        {
            ReportMast report = master.getFormSystemReport(ReportId);
            ResponseModel response = new ResponseModel();
            if (report != null)
            {
                reportParameters.Add(new SpecificationField()
                {
                    FieldName = "RequestUserId",
                    FieldValue = Convert.ToString(SessionPersister.LoginedUser.UserId)
                });
                string FileName = report.ReportTitle.Replace(" ", "") + "_" + Convert.ToString(SessionPersister.LoginedUser.UserId);
                report.SQLTableName = TableName;
                response = ReportHelper.ExportSystemReport(report, FileName, reportParameters, true);
            }
            return Json(response, JsonRequestBehavior.AllowGet);
        }
        #endregion@reg

        #region Audit Logs
        public ActionResult SystemAuditLogs()
        {
            List<ReportMast> reports = CommonBusiness.getFormSystemReportSelection("AuditLogs");
            // Pass it to the view using ViewBag
            ViewBag.ReportSelection = reports;
            return View();
        }
        #endregion

        #region ProfitLossReport
        public ActionResult ProfitLoss()
        {
            List<ReportMast> reports = CommonBusiness.getFormSystemReportSelection("ProfitLoss");
            // Pass it to the view using ViewBag
            ViewBag.ReportSelection = reports;
            return View();
        }
        #endregion

        #region BalanceSheetReport
        public ActionResult BalanceSheet()
        {
            List<ReportMast> reports = CommonBusiness.getFormSystemReportSelection("BalanceSheet");
            // Pass it to the view using ViewBag
            ViewBag.ReportSelection = reports;
            return View();
        }
        #endregion

        #region GSTR1Report
        public ActionResult GSTR1Report()
        {
            List<ReportMast> reports = CommonBusiness.getFormSystemReportSelection("GSTR1");
            // Pass it to the view using ViewBag
            ViewBag.ReportSelection = reports;
            return View();
        }
        #endregion
        #region GSTR2Report
        public ActionResult GSTR2Report()
        {
            List<ReportMast> reports = CommonBusiness.getFormSystemReportSelection("GSTR2");
            // Pass it to the view using ViewBag
            ViewBag.ReportSelection = reports;
            return View();
        }
        #endregion
        #region GSTR3BReport
        public ActionResult GSTR3BReport()
        {
            List<ReportMast> reports = CommonBusiness.getFormSystemReportSelection("GSTR3B");
            // Pass it to the view using ViewBag
            ViewBag.ReportSelection = reports;
            return View();
        }
        #endregion
    }
}