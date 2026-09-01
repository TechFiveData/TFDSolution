using Antlr.Runtime.Misc;
using CrystalDecisions.ReportAppServer.ReportDefModel;
using CrystalDecisions.Web;
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Spreadsheet;
using log4net.Repository.Hierarchy;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.Ocsp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.EnterpriseServices;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.Caching;
using System.Runtime.Remoting;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.DynamicData;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Services.Description;
using System.Web.UI.WebControls;
using TFDSolution.App_Start;
using TFDSolution.Business;
using TFDSolution.Business.Interface;
using TFDSolution.Common;
using TFDSolution.Models;
using TFDSolution.Transport;
using TFDSolution.Transport.Common;
using TFDSolution.Transport.Master;
using WebGrease.Activities;
using WebGrease.Css.Ast;
using static CrystalDecisions.Data.AdoDotNetInterop.InternalXmlSchemaDependencyTree;

namespace TFDSolution.Controllers
{
    [CustomAuthenticationFilter]
    public class HomeController : BaseController
    {
        private readonly IMasterBusiness masterBusines;
        private readonly ITransactionBusiness transactionBusines;
        public readonly IConfigurationBusiness config = new ConfigurationBusiness();
        public HomeController()
        {
            masterBusines = new MasterBusiness();
            transactionBusines = new TransactionBusiness();
            config = new ConfigurationBusiness();

        }
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Calender()
        {
            return View();
        }
        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
        public ActionResult UnAuthorized()
        {
            ViewBag.Message = "Un Authorized Page!";

            return View();
        }
        #region Main Page

        public ActionResult MyPage(string pagename)
        {
            PageModel pageModel = new PageModel();
            //FormMast Forms = masterBusines.getFormDetail(pagename);
            FormMast_Data Forms = masterBusines.GetFormStructure(pagename, string.Empty, SessionPersister.LoginedUser.UserId.Value.ToString(), Convert.ToInt32(SessionPersister.LoginedUser.RoleID));
            CommonBusiness.FormDeleteData(pagename, string.Empty, SessionPersister.LoginedUser.UserId.Value.ToString());
            if (Forms != null)
            {
                pageModel.Design = Forms;
            }
            else
            {
                pageModel.Design = new FormMast_Data();
            }
            if (pageModel.Design != null && pageModel.Design.FormId != Guid.Empty)
            {
                pageModel.PendingData = masterBusines.getFormPendingList(Convert.ToString(pageModel.Design.FormId));
                pageModel.Reports = masterBusines.getFormReport("MIS", Convert.ToString(pageModel.Design.FormId));
            }
            TempData.Remove("PendingId");
            TempData.Remove("PendingItemSrNos");
            CommonBusiness.SaveAuditLog(new Transport.Common.AuditLog()
            {
                Action = "Open",
                CompanyId = SessionPersister.LoginedUser.CompanyInfo.CompanyId,
                FinancialYearId = SessionPersister.LoginedUser.CompanyInfo.DefaultFiancialId,
                IPAddress = SessionPersister.LoginedUser.IPAddress,
                PageName = pageModel.Design.FormTitle,
                RecordId = string.Empty,
                Remark = $"User open {pageModel.Design.FormTitle}",
                UserId = SessionPersister.LoginedUser.UserId.Value
            });

            return View(pageModel);
        }

        public ActionResult HierachyPage(string pagename)
        {
            PageModel pageModel = new PageModel();
            FormMast_Data Forms = masterBusines.GetFormStructure(pagename, string.Empty, SessionPersister.LoginedUser.UserId.Value.ToString(), Convert.ToInt32(SessionPersister.LoginedUser.RoleID));
            if (Forms != null)
            {
                pageModel.Design = Forms;
            }
            else
            {
                pageModel.Design = new FormMast_Data();
            }
            if (pageModel.Design != null && pageModel.Design.FormId != Guid.Empty)
            {
                pageModel.Reports = masterBusines.getFormReport("MIS", Convert.ToString(pageModel.Design.FormId));
            }
            TempData.Remove("PendingId");
            TempData.Remove("PendingItemSrNos");
            return View(pageModel);
        }

        public ActionResult ParticalMasterPopup(string FormId, string pagename, string act, int Id = 0, int ParentId = 0)
        {
            PageModel pageModel = new PageModel();
            try
            {
                //Add session 
                var userDetails = (LoginUserInfo)Session["LoginUserInfo"];
                Guid companyID = userDetails.CompanyInfo.CompanyId;
                CompanyMast CompanyMast = masterBusines.GetCompany(Convert.ToString(companyID));
                List<clsSelection> _currency = masterBusines.getSelection("Currency", Convert.ToString(companyID));
                string currenctyId = Convert.ToString(CompanyMast.DefaultCurrencyId);
                FormMast_Data Design = masterBusines.GetFormStructure(pagename, Convert.ToString(Id), SessionPersister.LoginedUser.UserId.Value.ToString(), Convert.ToInt32(SessionPersister.LoginedUser.RoleID));
                int formPendingId = 0;
                string ItemSrNos = string.Empty;
                if (string.IsNullOrEmpty(act) && Id > 0)
                {
                    act = "E";
                }
                else if (string.IsNullOrEmpty(act) && Id == 0)
                {
                    act = "A";
                }
                if (string.IsNullOrEmpty(act) || (!string.IsNullOrEmpty(act) && (act.ToUpper() != "E" && act.ToUpper() != "A")))
                {
                    act = "V";
                }
                pageModel.PageAction = act.ToUpper();
                string docNo = string.Empty, URLNo = string.Empty, status = "0", _exchangeRate = "";
                int docId = 0;
                if (Design != null)
                {
                    pageModel.Design = Design;
                }
                pageModel.DocNumberSetting = masterBusines.getDocFormList(Convert.ToString(pageModel.Design.FormId), Convert.ToString(companyID));
                if (Id > 0)
                {
                    pageModel.Reports = masterBusines.getFormReport("Transaction", Convert.ToString(pageModel.Design.FormId));
                    masterBusines.FillFieldValue(Id, Design, Convert.ToString(userDetails.UserId));
                    Design.Data = masterBusines.GetDynamicData(pagename, Id, Convert.ToString(userDetails.UserId));
                    if (Design.Data != null && Design.Data.Count() > 0)
                    {
                        foreach (var item in Design.Data.ToList())
                        {
                            if (item["URNNo"] != null)
                                URLNo = Convert.ToString(item["URNNo"]);
                            if (item["Status"] != null)
                                status = Convert.ToString(item["Status"]);
                            if (item.ContainsKey("ExchangeRate"))
                                _exchangeRate = Convert.ToString(item["ExchangeRate"]);
                            if (item.ContainsKey("CurrencyId"))
                            {
                                if (item["CurrencyId"] != null)
                                {
                                    currenctyId = Convert.ToString(item["CurrencyId"]);
                                }
                            }
                        }
                    }
                    docNo = "";
                    if (status != "1" && (act == "E"))
                    {
                        act = "V";
                    }
                }
                else
                {
                    if (Design.Data != null && Design.Data.Count() > 0)
                    {
                        foreach (var item in Design.Data)
                        {
                            if (item.TryGetValue("ExchangeRate", out var exchangeValue)
                                 && !string.IsNullOrWhiteSpace(Convert.ToString(exchangeValue)))
                                _exchangeRate = Convert.ToString(exchangeValue);

                            if (item.TryGetValue("CurrencyId", out var currencyValue) && !string.IsNullOrWhiteSpace(Convert.ToString(currencyValue)))
                            {
                                currenctyId = Convert.ToString(currencyValue);
                            }
                        }
                    }

                    FormDocumentNo formDocument = masterBusines.getDocNumber(Convert.ToString(pageModel.Design.FormId),
                        SessionPersister.LoginedUser.CompanyInfo.DefaultFiancialId,
                        Convert.ToString(SessionPersister.LoginedUser.CompanyInfo.CompanyId),
                        0, formPendingId, ItemSrNos);
                    URLNo = CommonBusiness.getTransNextCode(Convert.ToString(pageModel.Design.FormId), Convert.ToString(SessionPersister.LoginedUser.CompanyInfo.CompanyId));
                    if (formDocument != null)
                    {
                        docId = formDocument.DocNoSettingId;
                        docNo = formDocument.DocNo;
                    }
                    if (pageModel != null && pageModel.Design != null)
                    {
                        DeletePhotos(URLNo, pageModel.Design.FormName, pageModel.Design.ParentFormName);
                    }
                }
                if (_currency != null)
                {
                    var objcurrency = _currency.Where(x => x.Value == currenctyId).FirstOrDefault();
                    if (objcurrency == null)
                    {
                        objcurrency = _currency.FirstOrDefault();
                    }
                    if (objcurrency != null)
                    {
                        currenctyId = Convert.ToString(objcurrency.Value);
                        ViewBag.currentCurrencyName = objcurrency.Text;
                        ViewBag.currentExchangeCurrencyName = "0";
                        if (!string.IsNullOrEmpty(_exchangeRate) && _exchangeRate != "")
                        {
                            ViewBag.currentExchangeCurrencyName = "₹" + _exchangeRate;
                        }
                        else
                        {
                            int _currencyId = Convert.ToInt32(currenctyId);
                            ResponseModel response = getCurrencyRate(_currencyId);
                            if (response != null && (response.IsSuccess.HasValue ? response.IsSuccess.Value : false))
                            {
                                ViewBag.currentExchangeCurrencyName = response.Response;
                            }
                            else
                            {
                                ViewBag.currentExchangeCurrencyName = "";
                            }
                        }
                    }
                }
                ViewBag.CurrencyList = _currency;
                ViewBag.PrimaryId = Id;
                ViewBag.DocNo = docNo;
                ViewBag.URNNo = URLNo;
                ViewBag.Status = status;
                ViewBag.DocId = docId;
                ViewBag.currenctyId = currenctyId;
                ViewBag.ParentId = ParentId;
                //Delete attachments
                if (Id <= 0)
                {
                    string uploadFolder = Server.MapPath((Path.Combine("~/FileManager/" + pageModel.Design.ParentFormName + "/" + pageModel.Design.FormName + "/", URLNo)));
                    if (System.IO.Directory.Exists(uploadFolder))
                    {
                        System.IO.Directory.Delete(uploadFolder, true);
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.ExceptionMessage = ex.Message;
            }
            return PartialView("_ParticalMasterPopup", pageModel);
        }

        public async Task<ActionResult> getPageData(string tblName)
        {
            FormData model = new FormData();
            string CompanyId = Convert.ToString(SessionPersister.LoginedUser.CompanyInfo.CompanyId);
            int fiancialYearId = SessionPersister.LoginedUser.CompanyInfo.DefaultFiancialId;
            model = await masterBusines.GetDynamicDataFromSPAsync(tblName, CompanyId, fiancialYearId);
            return new JsonResult
            {
                Data = model,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                MaxJsonLength = int.MaxValue // set to highest possible
            };
        }

        public async Task<ActionResult> getPageColumns(string tblName)
        {
            FormData model = await masterBusines.GetPageColumns(tblName);
            return new JsonResult
            {
                Data = model,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                MaxJsonLength = int.MaxValue // set to highest possible
            };
        }

        public async Task<JsonResult> getPageDataOnly(string tblName)
        {
            FormData model = new FormData();

            var draw = Request.Form["draw"];
            var start = Convert.ToInt32(Request.Form["start"]);
            var length = Convert.ToInt32(Request.Form["length"]);
            var searchValue = Request.Form["search[value]"];

            string CompanyId = Convert.ToString(SessionPersister.LoginedUser.CompanyInfo.CompanyId);
            string UserId = Convert.ToString(SessionPersister.LoginedUser.UserId.Value);
            int fiancialYearId = SessionPersister.LoginedUser.CompanyInfo.DefaultFiancialId;
            model = await masterBusines.GetPageDataPaginationAsync(tblName, CompanyId, fiancialYearId, start, length, searchValue, UserId);

            return Json(new
            {
                draw = draw,
                recordsTotal = model.TotalRecords,
                recordsFiltered = model.TotalRecords,
                data = model.Data
            }, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> getPageChildDataOnly(string tblName, int ParentId)
        {
            FormData model = new FormData();

            var draw = Request.Form["draw"];
            var start = Convert.ToInt32(Request.Form["start"]);
            var length = Convert.ToInt32(Request.Form["length"]);
            var searchValue = Request.Form["search[value]"];
            if (length == -1)
            {
                length = 10000;
            }
            string CompanyId = Convert.ToString(SessionPersister.LoginedUser.CompanyInfo.CompanyId);
            string UserId = Convert.ToString(SessionPersister.LoginedUser.UserId.Value);
            int fiancialYearId = SessionPersister.LoginedUser.CompanyInfo.DefaultFiancialId;
            model = await masterBusines.GetPageChildDataAsync(tblName, CompanyId, fiancialYearId, start, length, searchValue, UserId, ParentId);

            return Json(new
            {
                draw = draw,
                recordsTotal = model.TotalRecords,
                recordsFiltered = model.TotalRecords,
                data = model.Data
            }, JsonRequestBehavior.AllowGet);
        }
        public async Task<ActionResult> getPageTabData(string tabId, int ParentId, int ProcessId, int DetailParentId)
        {
            FormTabData model = new FormTabData();
            model = await masterBusines.GetDynamicTabDataFromSPAsync(tabId, ParentId, Convert.ToString(SessionPersister.LoginedUser.UserId.Value), ProcessId, DetailParentId);
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> getPageTabDataExport(string tabId, int ParentId, int ProcessId)
        {
            FormTabData model = new FormTabData();
            model = await masterBusines.GetDynamicTabDataFromSPAsync(tabId, ParentId, Convert.ToString(SessionPersister.LoginedUser.UserId.Value), ProcessId, 0);
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> getPageInlineTabData(string tabId, int ParentId, int ProcessId, string FormId, string SectionId, string PageAction)
        {
            PageTabModel model = new PageTabModel();
            model.FormFields = masterBusines.getFormFieldList(FormId, SectionId, tabId);
            model.Design = masterBusines.GetFormTab(tabId);
            FormTabData tabData = await masterBusines.GetDynamicTabData(tabId, ParentId, Convert.ToString(SessionPersister.LoginedUser.UserId.Value), ProcessId);
            model.Data = tabData.Data;
            model.PageAction = PageAction;
            model.IsDefault = 0;
            model.UserId = Convert.ToString(SessionPersister.LoginedUser.UserId.Value);
            model.CompanyId = Convert.ToString(SessionPersister.LoginedUser.CompanyInfo.CompanyId);
            model.FinancialYearId = SessionPersister.LoginedUser.CompanyInfo.DefaultFiancialId;
            return PartialView("_InlineTabGrid", model);
        }

        #endregion

        #region Page Detail
        [HttpPost]
        public JsonResult UploadPhoto(HttpPostedFileBase file, string URNNo, string FormName, string ParentFormName)
        {
            try
            {
                if (file != null && file.ContentLength > 0)
                {
                    string uploadFolder = Path.Combine("~/FileManager", ParentFormName, FormName);

                    string folderPath = Server.MapPath(uploadFolder);
                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }
                    //string fileName = Path.GetFileName(file.FileName);
                    //string filePath = Path.Combine(folderPath, fileName);
                    string newFileName = URNNo + ".png";
                    string filePath = Path.Combine(folderPath, newFileName);
                    int count = 1;
                    while (System.IO.File.Exists(filePath))
                    {
                        newFileName = URNNo + "_" + count + ".png";
                        filePath = Path.Combine(folderPath, newFileName);
                        count++;
                    }
                    file.SaveAs(filePath);

                    return Json(new { success = true, filePath = uploadFolder + "/" + newFileName });
                }
                else
                {
                    return Json(new { success = false, message = "No file selected." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        public ActionResult DeletePhoto(string fileName, string formName, string parentFormName)
        {
            try
            {
                string uploadFolder = Path.Combine("~/FileManager", parentFormName, formName);
                string fullPath = Path.Combine(Server.MapPath(uploadFolder), fileName);
                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                }
                return Json(new { success = true, message = "Photo has been deleted." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        public JsonResult GetFilesByPrefix(string prefix, string formName, string parentFormName)
        {
            try
            {
                // Build relative upload folder path
                string uploadFolder = Path.Combine("~/FileManager", parentFormName, formName);
                // Map to server path
                string folderPath = Server.MapPath(uploadFolder);

                // Ensure folder exists
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                // Get all files in the folder that start with the given prefix
                var files = Directory.EnumerateFiles(folderPath)
                                     .Select(Path.GetFileName)
                                     .Where(f => f.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                                     .ToList();

                return Json(files, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
                // Optional: log exception 'ex' here
                return Json(new string[0], JsonRequestBehavior.AllowGet);
            }
        }
        public void DeletePhotos(string prefix, string formName, string parentFormName)
        {
            try
            {
                string uploadFolder = Path.Combine("~/FileManager", parentFormName, formName);
                // Map to server path
                string folderPath = Server.MapPath(uploadFolder);
                // Ensure folder exists
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                // Get all files in the folder that start with the given prefix
                var files = Directory.EnumerateFiles(folderPath)
                                     .Select(Path.GetFileName)
                                     .Where(f => f.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                                     .ToList();

                if (files != null & files.Count > 0)
                {
                    foreach (var item in files)
                    {
                        string filePath = Path.Combine(Server.MapPath(uploadFolder), Path.GetFileName(item));
                        if (System.IO.File.Exists(filePath))
                        {
                            System.IO.File.Delete(filePath);
                        }
                    }
                }
            }
            catch (Exception)
            {

            }
        }
        public ActionResult getDependencyData(SubmitFormModel model)
        {
            ResponseModel response = new ResponseModel();
            model.UserId = SessionPersister.LoginedUser.UserId.Value;
            model.CompanyId = SessionPersister.LoginedUser.CompanyInfo.CompanyId;
            model.FiancialYearId = SessionPersister.LoginedUser.CompanyInfo.DefaultFiancialId;
            List<GridField> fields = masterBusines.getFieldDependency(model);
            return Json(fields, JsonRequestBehavior.AllowGet);
        }

        public ActionResult getHeaderDependencyData(SubmitFormModel model)
        {
            ResponseModel response = new ResponseModel();
            model.UserId = SessionPersister.LoginedUser.UserId.Value;
            model.CompanyId = SessionPersister.LoginedUser.CompanyInfo.CompanyId;
            model.FiancialYearId = SessionPersister.LoginedUser.CompanyInfo.DefaultFiancialId;
            List<GridField> fields = masterBusines.getHeaderFieldDependency(model);
            return Json(fields, JsonRequestBehavior.AllowGet);
        }


        public ActionResult getTabGridData(string tabId, int ParentId)
        {
            PageListData model = new PageListData();
            DataTable Data1 = masterBusines.getGridData(tabId, ParentId, Convert.ToString(SessionPersister.LoginedUser.UserId.Value));
            string json = JsonConvert.SerializeObject(Data1);
            return Json(json, JsonRequestBehavior.AllowGet);
        }

        public ActionResult NextRecordExecution(int ParentId, string spName)
        {
            ResponseModel response = transactionBusines.ExecuteNextStoredProcedure(ParentId, spName, Convert.ToString(SessionPersister.LoginedUser.CompanyInfo.CompanyId), Convert.ToString(SessionPersister.LoginedUser.UserId.Value));
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult SavePageData(SubmitFormModel model)
        {
            model.UserId = SessionPersister.LoginedUser.UserId.Value;
            model.CompanyId = SessionPersister.LoginedUser.CompanyInfo.CompanyId;
            model.FiancialYearId = SessionPersister.LoginedUser.CompanyInfo.DefaultFiancialId;
            ResponseModel response = masterBusines.SavePageData(model);
            return Json(response, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult SavePageCharges(PageOtherCharges model)
        {
            model.UserId = SessionPersister.LoginedUser.UserId.Value;
            ResponseModel response = masterBusines.SavePageCharges(model);
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public ActionResult getPageTotal(string formId, int parentId)
        {
            string UserId = Convert.ToString(SessionPersister.LoginedUser.UserId.Value);
            PageTotalData response = masterBusines.getPageTotal(formId, parentId, UserId);
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SaveOtherChargesItem(ItemOtherChargeData model)
        {
            string UserId = Convert.ToString(SessionPersister.LoginedUser.UserId.Value);
            model.UserId = UserId;
            ResponseModel response = masterBusines.SaveOtherChargesItem(model);
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ParticalPageTotal(string formId, int parentId, string pageAction)
        {
            string UserId = Convert.ToString(SessionPersister.LoginedUser.UserId.Value);
            PageTotalData response = masterBusines.getPageTotal(formId, parentId, UserId);
            response.PageAction = pageAction;
            return PartialView("_ParticalPageTotal", response);
        }

        public ActionResult getDocumentNumber(string formId, int docId)
        {
            string CompanyId = Convert.ToString(SessionPersister.LoginedUser.CompanyInfo.CompanyId);
            FormDocumentNo formDocument = masterBusines.getDocNumber(formId, SessionPersister.LoginedUser.CompanyInfo.DefaultFiancialId, CompanyId, docId, 0, "");
            return Json(formDocument, JsonRequestBehavior.AllowGet);
        }
        public ActionResult PageDetail(string pagename, string uId, string act)
        {
            PageModel pageModel = new PageModel();
            var actionName = string.Empty;
            var URNText = string.Empty;
            try
            {
                //startagain:
                //Add session 
                var userDetails = (LoginUserInfo)Session["LoginUserInfo"];
                Guid companyID = userDetails.CompanyInfo.CompanyId;
                CompanyMast CompanyMast = masterBusines.GetCompany(Convert.ToString(companyID));
                List<clsSelection> _currency = masterBusines.getSelection("Currency", Convert.ToString(companyID));
                string currenctyId = Convert.ToString(CompanyMast.DefaultCurrencyId);
                FormMast_Data Design = masterBusines.GetFormStructure(pagename, uId, SessionPersister.LoginedUser.UserId.Value.ToString(), Convert.ToInt32(SessionPersister.LoginedUser.RoleID));
                int formPendingId = 0;
                string ItemSrNos = string.Empty;
                //if (!string.IsNullOrEmpty(uId) && uId != "-1" && Design.Data == null)
                //{
                //    act = "A";
                //    uId = "-1";
                //    goto startagain;
                //}
                if (TempData["PendingId"] != null && TempData["PendingItemSrNos"] != null)
                {
                    formPendingId = Convert.ToInt32(Convert.ToString(TempData["PendingId"]));
                    ItemSrNos = Convert.ToString(TempData["PendingItemSrNos"]);
                    TempData["PendingId"] = formPendingId;
                    TempData["PendingItemSrNos"] = ItemSrNos;
                    if (formPendingId > 0 && ItemSrNos != string.Empty)
                    {
                        masterBusines.FillPendingMaster(formPendingId, ItemSrNos, Design);

                        Design.Data = masterBusines.GetDynamicPendingData(formPendingId, ItemSrNos);
                    }
                }

                if (!string.IsNullOrEmpty(uId) && uId == "-1")
                {
                    uId = string.Empty;
                }
                if (string.IsNullOrEmpty(act) && !string.IsNullOrEmpty(uId))
                {
                    act = "E";
                    actionName = "Edit";
                }
                else if (string.IsNullOrEmpty(act) && string.IsNullOrEmpty(uId))
                {
                    act = "A";
                    actionName = "Add";
                }
                if (string.IsNullOrEmpty(act) || (!string.IsNullOrEmpty(act) && (act.ToUpper() != "E" && act.ToUpper() != "A")))
                {
                    act = "V";
                    actionName = "View";
                }
                pageModel.PageAction = act.ToUpper();
                string docNo = string.Empty, URLNo = string.Empty, status = "0", _exchangeRate = "";
                int docId = 0;
                if (Design != null)
                {
                    pageModel.Design = Design;
                }
                pageModel.DocNumberSetting = masterBusines.getDocFormList(Convert.ToString(pageModel.Design.FormId), Convert.ToString(companyID));
                if (!string.IsNullOrEmpty(uId))
                {
                    pageModel.Reports = masterBusines.getFormReport("Transaction", Convert.ToString(pageModel.Design.FormId));
                    masterBusines.FillFieldValue(Convert.ToInt32(uId), Design, Convert.ToString(userDetails.UserId));
                    Design.Data = masterBusines.GetDynamicData(pagename, Convert.ToInt32(uId), Convert.ToString(userDetails.UserId));
                    if (Design.Data != null && Design.Data.Count() > 0)
                    {
                        foreach (var item in Design.Data.ToList())
                        {
                            if (item["URNNo"] != null)
                                URLNo = Convert.ToString(item["URNNo"]);
                            if (item["Status"] != null)
                                status = Convert.ToString(item["Status"]);
                            if (item.ContainsKey("ExchangeRate"))
                                _exchangeRate = Convert.ToString(item["ExchangeRate"]);
                            if (item.ContainsKey("CurrencyId"))
                            {
                                if (item["CurrencyId"] != null)
                                {
                                    currenctyId = Convert.ToString(item["CurrencyId"]);
                                }
                            }
                        }
                    }
                    docNo = "";
                    if (status != "1" && (act == "E"))
                    {
                        act = "V";
                    }
                }
                else
                {
                    if (Design.Data != null && Design.Data.Count() > 0)
                    {
                        foreach (var item in Design.Data)
                        {
                            if (item.TryGetValue("ExchangeRate", out var exchangeValue)
                                 && !string.IsNullOrWhiteSpace(Convert.ToString(exchangeValue)))
                                _exchangeRate = Convert.ToString(exchangeValue);

                            if (item.TryGetValue("CurrencyId", out var currencyValue) && !string.IsNullOrWhiteSpace(Convert.ToString(currencyValue)))
                            {
                                currenctyId = Convert.ToString(currencyValue);
                            }
                        }
                    }

                    FormDocumentNo formDocument = masterBusines.getDocNumber(Convert.ToString(pageModel.Design.FormId),
                        SessionPersister.LoginedUser.CompanyInfo.DefaultFiancialId
                        , Convert.ToString(SessionPersister.LoginedUser.CompanyInfo.CompanyId), 0, formPendingId, ItemSrNos);
                    URLNo = CommonBusiness.getTransNextCode(Convert.ToString(pageModel.Design.FormId), Convert.ToString(SessionPersister.LoginedUser.CompanyInfo.CompanyId));
                    if (formDocument != null)
                    {
                        docId = formDocument.DocNoSettingId;
                        docNo = formDocument.DocNo;
                    }
                    if (pageModel != null && pageModel.Design != null)
                    {
                        DeletePhotos(URLNo, pageModel.Design.FormName, pageModel.Design.ParentFormName);
                    }
                }
                if (string.IsNullOrEmpty(uId))
                {
                    uId = "0";
                }
                if (_currency != null)
                {
                    var objcurrency = _currency.Where(x => x.Value == currenctyId).FirstOrDefault();
                    if (objcurrency == null)
                    {
                        objcurrency = _currency.FirstOrDefault();
                    }
                    if (objcurrency != null)
                    {
                        currenctyId = Convert.ToString(objcurrency.Value);
                        ViewBag.currentCurrencyName = objcurrency.Text;
                        ViewBag.currentExchangeCurrencyName = "0";
                        if (!string.IsNullOrEmpty(_exchangeRate) && _exchangeRate != "" && _exchangeRate != "0")
                        {
                            ViewBag.currentExchangeCurrencyName = "₹" + _exchangeRate;
                        }
                        else
                        {
                            int _currencyId = Convert.ToInt32(currenctyId);
                            ResponseModel response = getCurrencyRate(_currencyId);
                            if (response != null && (response.IsSuccess.HasValue ? response.IsSuccess.Value : false))
                            {
                                ViewBag.currentExchangeCurrencyName = response.Response;
                            }
                            else
                            {
                                ViewBag.currentExchangeCurrencyName = "";
                            }
                        }
                    }
                }
                else
                {

                }
                ViewBag.CurrencyList = _currency;
                ViewBag.PrimaryId = uId;
                ViewBag.DocNo = docNo;
                ViewBag.URNNo = URLNo;
                ViewBag.Status = status;
                ViewBag.DocId = docId;
                ViewBag.currenctyId = currenctyId;
                //Delete attachments
                if (string.IsNullOrEmpty(uId) || (!string.IsNullOrEmpty(uId) && uId == "0"))
                {
                    string uploadFolder = Server.MapPath((Path.Combine("~/FileManager/" + pageModel.Design.ParentFormName + "/" + pageModel.Design.FormName + "/", URLNo)));
                    if (System.IO.Directory.Exists(uploadFolder))
                    {
                        System.IO.Directory.Delete(uploadFolder, true);
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.ExceptionMessage = ex.Message;
            }
            if (string.IsNullOrEmpty(act) && !string.IsNullOrEmpty(uId))
            {
                act = "E";
                actionName = "Edit";
                URNText = $"for {ViewBag.URNNo}";
            }
            else if (string.IsNullOrEmpty(act) && string.IsNullOrEmpty(uId))
            {
                act = "A";
                actionName = "Add";
            }
            if (string.IsNullOrEmpty(act) || (!string.IsNullOrEmpty(act) && (act.ToUpper() != "E" && act.ToUpper() != "A")))
            {
                act = "V";
                actionName = "View";
                URNText = $"for {ViewBag.URNNo}";
            }
            CommonBusiness.SaveAuditLog(new Transport.Common.AuditLog()
            {
                Action = actionName,
                CompanyId = SessionPersister.LoginedUser.CompanyInfo.CompanyId,
                FinancialYearId = SessionPersister.LoginedUser.CompanyInfo.DefaultFiancialId,
                IPAddress = SessionPersister.LoginedUser.IPAddress,
                PageName = pageModel.Design.FormTitle,
                RecordId = string.Empty,
                Remark = $"User click {actionName} button and open to {pageModel.Design.FormTitle} {URNText}",
                UserId = SessionPersister.LoginedUser.UserId.Value
            });
            return View(pageModel);
        }
        public ActionResult getCurrencyID(string formId, int cId)
        {
            string docNo = "";
            //Add session 
            var userDetails = (LoginUserInfo)Session["LoginUserInfo"];
            Guid companyID = userDetails.CompanyInfo.CompanyId;
            CompanyMast CompanyMast = new CompanyMast();
            CompanyMast = masterBusines.GetCurrencyByComapanyID(companyID);
            ViewBag.CurrencyList = masterBusines.getSelection("Currency", Convert.ToString(companyID));

            foreach (var item in ViewBag.CurrencyList)
            {
                if (item.Value == cId.ToString())
                {
                    ViewBag.currentCurrencyName = item.Text;
                    docNo = item.Text;
                    break; // Exit loop after match
                }
            }

            return Json(docNo, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetPageStatusView(string FormId, int Id)
        {
            List<FormStatusDataView> model = transactionBusines.GetFormStatusDataViews(FormId, Id);
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        #endregion

        [HttpGet]
        public ActionResult GetPageRecord(int id, string tblName)
        {
            SubmitFormModel model = new SubmitFormModel();
            model = masterBusines.GetFormFieldDataRecord(tblName, id);
            model.UserId = SessionPersister.LoginedUser.UserId.Value;
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult DeletePageRecord(int id, string tblName, string URNNo, string ParentFormName, string FormName)
        {
            ResponseModel response = masterBusines.DeleteFormFieldDataRecord(id, tblName);
            if (response != null && response.IsSuccess.HasValue && response.IsSuccess.Value)
            {
                CommonBusiness.SaveAuditLog(new Transport.Common.AuditLog()
                {
                    Action = "Delete",
                    CompanyId = SessionPersister.LoginedUser.CompanyInfo.CompanyId,
                    FinancialYearId = SessionPersister.LoginedUser.CompanyInfo.DefaultFiancialId,
                    IPAddress = SessionPersister.LoginedUser.IPAddress,
                    PageName = FormName,
                    RecordId = id.ToString(),
                    Remark = "Delete " + URNNo + " URN Transaction",
                    UserId = SessionPersister.LoginedUser.UserId.Value
                });
                if (!string.IsNullOrEmpty(URNNo) && !string.IsNullOrEmpty(ParentFormName) && !string.IsNullOrEmpty(FormName))
                {
                    //Delete attachments
                    string uploadFolder = Server.MapPath((Path.Combine("~/FileManager/" + ParentFormName + "/" + FormName + "/", URNNo)));
                    if (System.IO.Directory.Exists(uploadFolder))
                    {
                        System.IO.Directory.Delete(uploadFolder, true);
                    }
                }
            }
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult DeleteTabRecord(int id, string TabId, string FormTitle)
        {
            ResponseModel response = masterBusines.DeleteTabFieldDataRecord(id, TabId);
            if (response != null && response.IsSuccess.HasValue && response.IsSuccess.Value)
            {
                CommonBusiness.SaveAuditLog(new Transport.Common.AuditLog()
                {
                    Action = "Delete",
                    CompanyId = SessionPersister.LoginedUser.CompanyInfo.CompanyId,
                    FinancialYearId = SessionPersister.LoginedUser.CompanyInfo.DefaultFiancialId,
                    IPAddress = SessionPersister.LoginedUser.IPAddress,
                    PageName = FormTitle,
                    RecordId = id.ToString(),
                    Remark = "Delete Tab Detail",
                    UserId = SessionPersister.LoginedUser.UserId.Value
                });
            }
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ParticalDetailPopup(string FormId, string SectionId, string TabId, string ActionType, int Id = 0, int ProcessId = 0, int ParentId = 0, int DetailParentId = 0)
        {
            PageTabModel model = new PageTabModel();
            int ItemSrNo = 1;
            int RowIndex = 1;
            model.FormFields = masterBusines.getFormFieldList(FormId, SectionId, TabId);
            model.Design = masterBusines.GetFormTab(TabId);
            model.Id = Id;
            if (Id > 0)
            {
                masterBusines.FillTabFieldValue(Id, model);
            }
            else
            {
                ItemSrNoResult _srNo = masterBusines.GETNextItemSrNo(ParentId, TabId, SessionPersister.LoginedUser.UserId.Value.ToString(), ProcessId, DetailParentId);
                if (_srNo != null)
                {
                    ItemSrNo = _srNo.ItemSrNo;
                    RowIndex = _srNo.RowIndex;
                }
            }
            ViewBag.ItemSrNo = ItemSrNo;
            ViewBag.RowIndex = RowIndex;
            ViewBag.ProcessId = ProcessId;
            ViewBag.DetailParentId = DetailParentId;
            model.PageAction = ActionType;
            List<FormSettings> formSettings = masterBusines.getFormSettings(FormId);
            model.FormTabId = TabId;

            CommonBusiness.SaveAuditLog(new Transport.Common.AuditLog()
            {
                Action = ActionType,
                CompanyId = SessionPersister.LoginedUser.CompanyInfo.CompanyId,
                FinancialYearId = SessionPersister.LoginedUser.CompanyInfo.DefaultFiancialId,
                IPAddress = SessionPersister.LoginedUser.IPAddress,
                PageName = "",
                RecordId = string.Empty,
                Remark = $"User open on add item detail with itemsrno {ItemSrNo}",
                UserId = SessionPersister.LoginedUser.UserId.Value
            });
            return PartialView("_ParticalTabPopup", model);
        }
        public ActionResult GetItemOtherCharges(int chargesId, int ParentId, int DetailId, string FormId)
        {
            OtherChargesModel response = config.GetItemOtherCharges(chargesId, ParentId, DetailId, FormId);
            return Json(response, JsonRequestBehavior.AllowGet);
        }
        public ActionResult getNextItemSrNo(string TabId, int Id = 0, int ProcessId = 0)
        {
            ItemSrNoResult _srNo = masterBusines.GETNextItemSrNo(Id, TabId, SessionPersister.LoginedUser.UserId.Value.ToString(), ProcessId);
            return Json(_srNo, JsonRequestBehavior.AllowGet);

        }

        #region Pending    
        public async Task<ActionResult> getPendingData(string param)
        {
            FormTabData model = new FormTabData();
            string CompanyId = SessionPersister.LoginedUser.CompanyInfo.CompanyId.ToString();
            string UserId = Convert.ToString(SessionPersister.LoginedUser.UserId);
            model = await transactionBusines.getPendingRecords(param, CompanyId, SessionPersister.LoginedUser.CompanyInfo.DefaultFiancialId, UserId);
            return new JsonResult
            {
                Data = model,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                MaxJsonLength = int.MaxValue // set to highest possible
            };
        }

        public async Task<ActionResult> SelectedPendingData(int formPendingId, string ItemSrNos)
        {
            ResponseModel response = transactionBusines.getPendingSelectedRecords(formPendingId, ItemSrNos, SessionPersister.LoginedUser.UserId.Value.ToString());
            TempData["PendingId"] = formPendingId;
            TempData["PendingItemSrNos"] = ItemSrNos;
            return Json(response, JsonRequestBehavior.AllowGet);
        }
        #endregion

        public ActionResult UpdateRecordStatus(string formId, int status, int ParentId, int Id, string CancelReason)
        {
            ResponseModel response = new ResponseModel();
            string UserId = Convert.ToString(SessionPersister.LoginedUser.UserId.Value);
            string CompanyId = Convert.ToString(SessionPersister.LoginedUser.CompanyInfo.CompanyId);
            string PageStatusScript = TFDSolution.Business.CommonBusiness.getFormKeySetting(formId, "PageStatusScript");
            if (!string.IsNullOrEmpty(PageStatusScript) && !string.IsNullOrEmpty(PageStatusScript))
            {
                response = masterBusines.RunPageStatusScript(PageStatusScript, formId, status, ParentId, UserId, CompanyId);
                if (response != null && response.IsSuccess.HasValue && !response.IsSuccess.Value)
                {
                    return Json(response, JsonRequestBehavior.AllowGet);
                }
            }
            response = masterBusines.UpdateRecordStatus(formId, status, ParentId, Id, CancelReason, UserId);
            if (status == 2)
            {
                string SendInquiryToVendor = TFDSolution.Business.CommonBusiness.getFormKeySetting(formId, "SendInquiryToVendor");
                if (!string.IsNullOrEmpty(SendInquiryToVendor) && SendInquiryToVendor == "y")
                {
                    var emailrequest = new EmailRequest();
                    emailrequest.UserId = UserId;
                    emailrequest.UserName = Convert.ToString(SessionPersister.LoginedUser.LastName) + " " + Convert.ToString(SessionPersister.LoginedUser.FirstName);
                    emailrequest.CompanyId = CompanyId;
                    emailrequest.FormId = formId;
                    emailrequest.ParentId = ParentId;
                    //string baseUrl = $"{Request.Url.Scheme}://{Request.Url.Authority}";
                    string baseUrl = $"{Request.Url.Scheme}://{Request.Url.Authority}{Request.ApplicationPath}".TrimEnd('/');
                    emailrequest.BaseURL = baseUrl;
                    Task.Run(() => SendInquiryEmails(ParentId, formId, emailrequest));
                }
            }
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        #region RFQ Link Send

        public async Task SendInquiryEmails(int ParentId, string formId, EmailRequest emailrequest)
        {
            try
            {
                var vendors = await masterBusines.GetVendorInquiryList(ParentId, formId);
                var request = new EmailRequest();
                foreach (var group in vendors)
                {
                    try
                    {
                        var expiryDate = DateTime.Now.AddDays(20).ToString("yyyy-MM-dd");
                        var queryString = $"PendingId={group.FormPendingId}&DetailId={group.Ids}&ExpiryDate={expiryDate}";
                        var encrptData = AesOperation.EncryptString(queryString);
                        //var link = $"~/default/SubmitYourQuote?data={encrptData}";
                        string baseUrl = $"{Request.Url.Scheme}://{Request.Url.Authority}{Request.ApplicationPath}";
                        var link = $"{baseUrl}/default/SubmitYourQuote?data={encrptData}";
                        var formInquiryDetails = await masterBusines.GetFormInquiryDetail(Convert.ToInt32(group.FormPendingId), group.Ids);
                        var master = formInquiryDetails.master ?? new List<Dictionary<string, object>>();
                        var details = formInquiryDetails.details ?? new List<Dictionary<string, object>>();
                        if (master.Any() && details.Any())
                        {
                            await GetPurchaseInquiryEmailBody(master, details, link, ParentId, formId, emailrequest);
                        }
                    }
                    catch (Exception ex)
                    {
                        CommonBusiness.LogEx(ex, request.UserId, request.CompanyId);
                    }
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex, emailrequest.UserId, emailrequest.CompanyId);
            }
        }

        private async Task<ResponseModel> GetPurchaseInquiryEmailBody(
            List<Dictionary<string, object>> master,
            List<Dictionary<string, object>> details,
            string quotationLink,
            int ParentId,
            string formId,
            EmailRequest request)
        {
            try
            {
                // Generate Item Rows
                StringBuilder rowsBuilder = new StringBuilder();
                foreach (var item in details)
                {
                    string itemSrNo = item.ContainsKey("ItemSrNo") ? Convert.ToString(item["ItemSrNo"]) : "";
                    string itemName = item.ContainsKey("ITCode") ? Convert.ToString(item["ITCode"]) : "";
                    string uom = item.ContainsKey("UOM") ? Convert.ToString(item["UOM"]) : "";
                    string qty = item.ContainsKey("Quantity") ? Convert.ToString(item["Quantity"]) : "0";
                    string reqDate = item.ContainsKey("RequiredDate") && item["RequiredDate"] != null ? Convert.ToString(item["RequiredDate"]) : "";

                    rowsBuilder.Append($@"
            <tr>
                <td align='center'>{itemSrNo}</td>
                <td>{itemName}</td>
                <td align='center'>{qty}&nbsp;{uom}</td>
                <td>{reqDate}</td>
            </tr>");
                }

                string rows = rowsBuilder.ToString();

                var headerData = master.FirstOrDefault();

                if (headerData != null)
                {
                    string URNNo = headerData.ContainsKey("URNNo")
                        ? Convert.ToString(headerData["URNNo"])
                        : "";

                    string acCode = headerData.ContainsKey("AcCode")
                        ? Convert.ToString(headerData["AcCode"])
                        : "";

                    string docDate = headerData.ContainsKey("DocDate") &&
                                     headerData["DocDate"] != null
                        ? Convert.ToDateTime(headerData["DocDate"]).ToString("dd-MMM-yyyy")
                        : "";

                    string companyName = headerData.ContainsKey("CompanyName")
                        ? Convert.ToString(headerData["CompanyName"])
                        : "";

                    string companyMobile = headerData.ContainsKey("CompanyMobile")
                        ? Convert.ToString(headerData["CompanyMobile"])
                        : "";

                    string companyEmail = headerData.ContainsKey("CompanyEmail")
                        ? Convert.ToString(headerData["CompanyEmail"])
                        : "";

                    string companyWebStite = headerData.ContainsKey("CompanyWebStite")
                        ? Convert.ToString(headerData["CompanyWebStite"])
                        : "";

                    string registerAddress1 = headerData.ContainsKey("RegisterAddress1")
                        ? Convert.ToString(headerData["RegisterAddress1"])
                        : "";
                    string registerPincode = headerData.ContainsKey("RegisterPincode")
                        ? Convert.ToString(headerData["RegisterPincode"])
                        : "";
                    string ToEmail = headerData.ContainsKey("VendorEmail")
                        ? Convert.ToString(headerData["VendorEmail"])
                        : "";
                    request.Subject = "Purchase Inquiry – Request for Quotation (URN # " + URNNo + ")";
                    // Read HTML Template
                    string templatePath = HostingEnvironment.MapPath(
                        "~/Content/EmailTemplate/RFQ_EmailTemplate.html");
                    string emailBody = System.IO.File.ReadAllText(templatePath);
                    // Replace Placeholders
                    emailBody = emailBody.Replace("{acCode}", acCode);
                    emailBody = emailBody.Replace("{companyName}", companyName);
                    emailBody = emailBody.Replace("{URNNo}", URNNo);
                    emailBody = emailBody.Replace("{docDate}", docDate);
                    emailBody = emailBody.Replace("{rows}", rows);
                    emailBody = emailBody.Replace("{quotationLink}", quotationLink);
                    emailBody = emailBody.Replace("{companyMobile}", companyMobile);
                    emailBody = emailBody.Replace("{companyEmail}", companyEmail);
                    emailBody = emailBody.Replace("{companyWebStite}", companyWebStite);
                    emailBody = emailBody.Replace("{registerAddress1}", registerAddress1);
                    emailBody = emailBody.Replace("{registerPincode}", registerPincode);
                    emailBody = emailBody.Replace("{baseURL}", request.BaseURL);
                    request.MessageBody = emailBody;
                    ResponseModel response = new ResponseModel();
                    //request.ToEmail = "amit.gorvadiya1512@gmail.com"; // For testing purpose, replace with actual email in production
                    //response = MailHelper.SendEmailDefault(request);
                    //await AddSentEmailAuditLog(formId, ParentId, URNNo, request.UserId);
                    int acCodeId = headerData.TryGetValue("AcCode_Id", out var value) ? Convert.ToInt32(value) : 0;
                    List<ContactDetail> contacts = CommonBusiness.GetRFQEmailContacts(acCodeId);
                    if (contacts != null && contacts.Any())
                    {
                        foreach (var item in contacts)
                        {
                            request.ToEmail = item.Email;
                            request.CCEmail = ToEmail;
                            response = MailHelper.SendEmailDefault(request);

                            await AddSentEmailAuditLog(formId, ParentId, URNNo, request.UserId);
                        }
                    }
                    else
                    {
                        request.ToEmail = ToEmail;
                        response = MailHelper.SendEmailDefault(request);
                        await AddSentEmailAuditLog(formId, ParentId, URNNo, request.UserId);
                    }
                    return response;
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex, request.UserId, request.CompanyId);
            }
            return new ResponseModel
            {
                IsSuccess = false,
                Response = "No header data found."
            };
        }

        private async Task<ResponseModel> GetPurchaseInquiryEmailBody1(List<Dictionary<string, object>> master, List<Dictionary<string, object>> details, string quotationLink, int ParentId, string formId)
        {
            var request = new EmailRequest();
            var rows = "";
            int srNo = 1;
            foreach (var item in details)
            {
                string itemSrNo = item.ContainsKey("ItemSrNo") ? Convert.ToString(item["ItemSrNo"]) : "";
                string itemName = item.ContainsKey("ITCode") ? Convert.ToString(item["ITCode"]) : "";
                string qty = item.ContainsKey("Quantity") ? Convert.ToString(item["Quantity"]) : "0";
                string reqDate = item.ContainsKey("RequiredDate")
                    ? Convert.ToDateTime(item["RequiredDate"]).ToString("dd-MMM-yyyy")
                    : "";
                rows += $@"
                        <tr>
                            <td align='center'>{itemSrNo}</td>
                            <td>{itemName}</td>
                            <td align='center'>{qty}</td>
                            <td>{reqDate}</td>
                        </tr>";
                srNo++;
            }

            var headerData = master.FirstOrDefault();
            var itemHtml = string.Empty;
            var URNNo = string.Empty;
            if (headerData != null)
            {
                URNNo = headerData.ContainsKey("URNNo") ? Convert.ToString(headerData["URNNo"]) : "";
                var acCode = headerData.ContainsKey("AcCode") ? Convert.ToString(headerData["AcCode"]) : "";
                var docDate = headerData.ContainsKey("DocDate") && headerData["DocDate"] != null
                    ? Convert.ToDateTime(headerData["DocDate"]).ToString("dd-MMM-yyyy")
                    : "";
                var docNo = headerData.ContainsKey("DocNo") ? Convert.ToString(headerData["DocNo"]) : "";
                var companyName = headerData.ContainsKey("CompanyName") ? Convert.ToString(headerData["CompanyName"]) : "";
                var companyMobile = headerData.ContainsKey("CompanyMobile") ? Convert.ToString(headerData["CompanyMobile"]) : "";
                var companyEmail = headerData.ContainsKey("CompanyEmail") ? Convert.ToString(headerData["CompanyEmail"]) : "";
                var companyWebStite = headerData.ContainsKey("CompanyWebStite") ? Convert.ToString(headerData["CompanyWebStite"]) : "";
                var registerAddress1 = headerData.ContainsKey("RegisterAddress1") ? Convert.ToString(headerData["RegisterAddress1"]) : "";
                var registerPincode = headerData.ContainsKey("RegisterPincode") ? Convert.ToString(headerData["RegisterPincode"]) : "";
                request.ToEmail = headerData.ContainsKey("VendorEmail") ? Convert.ToString(headerData["VendorEmail"]) : "";
                request.ToEmail = "ceo";
                request.Subject = "Purchase Inquiry – Request for Quotation";
                request.MessageBody = $@"
                            <!DOCTYPE html>
                                <html>
                                <head>
                                    <meta charset=""utf-8"" />
                                    <title>Vendor Inquiry</title>
	                                <style>
		                                table th {{
		                                background-color: skyblue !important;
		                                }}
	                                </style>
                                </head>
                                <body style=""font-family: Arial, sans-serif; font-size: 14px; color: #333;"">

                                    <p>Dear <b>{acCode}</b>,</p>

                                    <p>
                                        Greetings from <b>{companyName}</b>.
                                    </p>

                                    <p>
                                        Please find below inquiry details:
                                    </p>

                                    <!-- Inquiry Header Details -->
                                    <table cellpadding=""6"" cellspacing=""0""
                                           style=""border-collapse: collapse; margin-bottom: 15px;"">
                                        <tr>
                                            <td><b>Inquiry #</b></td>
                                            <td>: {URNNo}</td>
                                        </tr>
                                        <tr>
                                            <td><b>Inquiry Date</b></td>
                                            <td>: {docDate}</td>
                                        </tr>
                                    </table>

                                    <p>
                                        We would like to request your quotation for the following items:
                                    </p>

                                    <!-- Item Details -->
                                    <table border=""1"" cellpadding=""8"" cellspacing=""0""
                                           style=""border-collapse: collapse; width: 100%;"">
                                        <thead style=""background-color: #f2f2f2;"">
                                            <tr>
                                                <th>Sr. No.</th>
                                                <th>Product Name</th>
                                                <th>Quantity</th>
                                                <th>Required Date</th>
                                            </tr>
                                        </thead>
                                        <tbody>
                                           {rows}
                                        </tbody>
                                    </table>

                                    <br />

                                    <p>
                                        Kindly submit your quotation by clicking the below link:
                                    </p>

                                    <!-- Quotation Submission Link -->
                                    <p style=""margin-top: 15px; margin-bottom: 20px;"">
                                        <a href=""{quotationLink}""
                                           style=""background-color: #007bff;
                                                  color: #ffffff;
                                                  padding: 10px 18px;
                                                  text-decoration: none;
                                                  border-radius: 4px;
                                                  display: inline-block;"">
                                            Submit Quotation
                                        </a>
                                    </p>
                                 <p>
                                    Kindly submit your quotation by clicking the below link.
                                    Please note that this link will remain active for the next
                                    <b>20 days</b> from the date of this email.
                                </p>
    

                                    <p>
                                        <a href=""{quotationLink}"">
                                            [Quotation Submission Link]
                                        </a>
                                    </p>

                                  <p>
                                        Alternatively, you can copy and paste the below URL into your browser:
                                    </p>

                                    <p>
                                        If you need any additional information, feel free to contact us.
                                    </p>

                                    <p>
                                        Thanks & Regards,<br />
                                        {companyName}<br />
                                        {companyMobile}<br />
                                        {companyEmail}<br />
                                        {companyWebStite}<br />
                                        {registerAddress1}<br />
                                        {registerPincode}
                                    </p>
                                </body>
                                </html>";
            }
            ResponseModel response = MailHelper.SendEmailDefault(request);
            await AddSentEmailAuditLog(formId, ParentId, URNNo, request.UserId);
            return response;
        }

        private async Task AddSentEmailAuditLog(string formId, int ParentId, string URNNo, string UserId)
        {
            ResponseModel response = await masterBusines.AddSentEmailAuditLog(formId, ParentId, UserId, URNNo);
        }

        #endregion

        public ActionResult DeleteItemDetailRecord(string ids, string TabId, string URNNo, int ParentId)
        {
            string UserId = Convert.ToString(SessionPersister.LoginedUser.UserId.Value);
            string IPAddress = SessionPersister.LoginedUser.IPAddress;
            ResponseModel response = masterBusines.DeleteItemDetailRecord(ids, TabId, ParentId, URNNo, UserId, IPAddress);
            return Json(response, JsonRequestBehavior.AllowGet);
        }
        public ActionResult OpenReportView(int ReportId, string FileName, int Id)
        {
            ReportMast report = masterBusines.getFormReport(ReportId);
            ResponseModel response = new ResponseModel();
            if (report != null)
            {
                response = ReportHelper.ExportReport(Id, report, FileName, true);
            }
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        #region BOM
        public ActionResult SaveBOMProcess(BomProcessModel model)
        {
            model.CreatedBy = SessionPersister.LoginedUser.UserId.Value;
            ResponseModel response = transactionBusines.SaveBOMProcess(model);
            return Json(response, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetBOMProcess(BomProcessModel model)
        {
            model.CreatedBy = SessionPersister.LoginedUser.UserId.Value;
            List<BomProcessModel> response = transactionBusines.GetBOMProcess(model);
            return Json(response, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetBOMProcessById(BomProcessModel model)
        {
            model.CreatedBy = SessionPersister.LoginedUser.UserId.Value;
            BomProcessModel response = transactionBusines.GetBOMProcessById(model);
            return Json(response, JsonRequestBehavior.AllowGet);
        }
        public ActionResult DeleteBOMProcess(BomProcessModel model)
        {
            model.CreatedBy = SessionPersister.LoginedUser.UserId.Value;
            ResponseModel response = transactionBusines.DeleteBOMProcess(model);
            return Json(response, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Next button Procedure Call Function
        //[HttpPost]
        //public ActionResult NextProcedureButtonCall(int id, string NextProcedureValue)
        //{

        //    var userDetails = (LoginUserInfo)Session["LoginUserInfo"];
        //    Guid userId = userDetails.UserId.Value;
        //    ResponseModel response = masterBusines.NextProcedureButtonCall(id, NextProcedureValue, userId);
        //    return Json(response, JsonRequestBehavior.AllowGet);
        //}
        #endregion

        #region Exchange currency 
        public ActionResult ExchnageCurrency(int id)
        {
            ResponseModel response = getCurrencyRate(id);
            return Json(response, JsonRequestBehavior.AllowGet);
        }
        #endregion

        private ResponseModel getCurrencyRate(int id)
        {
            ResponseModel response = new ResponseModel();
            response.IsSuccess = true;
            response.Response = "₹0";
            ObjectCache cache = MemoryCache.Default;
            string cacheKey = "ExchnageCurrencies";
            API_ExchangeRates rates = cache[cacheKey] as API_ExchangeRates;
            if (rates == null)
            {
                rates = masterBusines.ExchnageCurrencies();
                CacheItemPolicy policy = new CacheItemPolicy
                {
                    AbsoluteExpiration = DateTimeOffset.Now.AddDays(1)
                };
                cache.Set(cacheKey, rates, policy);
            }
            ExchangeRateResponse objcurrency = masterBusines.GetCurrencyDetails(id);
            if (objcurrency != null)
            {
                if (rates != null && rates.conversion_rates != null)
                {
                    var property = typeof(ConversionRates).GetProperty(objcurrency.alias, BindingFlags.Public | BindingFlags.Instance);
                    if (property != null)
                    {
                        var value = property.GetValue(rates.conversion_rates);
                        double rateDate = Convert.ToDouble(value);
                        if (rateDate > 0)
                            rateDate = (1 / rateDate);
                        response.Response = "₹" + Convert.ToString(Math.Round(rateDate, 2));
                    }
                }
            }
            return response;
        }

        #region Item Spefication
        public ActionResult PartialItemSpecification(SpecificationRequest request)
        {
            request.UserId = Convert.ToString(SessionPersister.LoginedUser.UserId);
            ItemSpeficationModel model = transactionBusines.GetItemSpefications(request);
            return PartialView("_ItemSpefication", model);
        }

        public ActionResult SaveItemSpecification(SpecificationRequest request)
        {
            request.UserId = Convert.ToString(SessionPersister.LoginedUser.UserId);
            request.CompanyId = Convert.ToString(SessionPersister.LoginedUser.CompanyInfo.CompanyId);
            request.FiancialYearId = SessionPersister.LoginedUser.CompanyInfo.DefaultFiancialId;
            ResponseModel response = transactionBusines.SaveItemSpecification(request);
            return Json(response, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Attachments
        public ActionResult ParticalAttachment(PageAttachmentRequest request)
        {
            request.UploadedBy = Convert.ToString(SessionPersister.LoginedUser.UserId);
            List<PageAttachmentModel> model = transactionBusines.GetAttachments(request);
            ViewBag.RecordStatus = Convert.ToString(request.RecordStatus);
            ViewBag.PageAction = request.PageAction;
            return PartialView("_Attachments", model);
        }
        [HttpPost]
        public JsonResult UploadAttachment(HttpPostedFileBase file, string URNNo, string FormId, int ParentId, string ParentFormName, string FormName, string FileName, string fileType, string fileSize,
            int detailId, string fieldId, int itemSrNo)
        {
            try
            {
                if (file != null && file.ContentLength > 0)
                {
                    string uploadFolder = Path.Combine("~/FileManager/" + ParentFormName + "/" + FormName + "/", URNNo);
                    string folderPath = Server.MapPath(uploadFolder);
                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }
                    //string fileName = Path.GetFileName(file.FileName);
                    //string filePath = Path.Combine(folderPath, fileName);
                    string newFileName = FileName + "." + fileType;
                    string filePath = Path.Combine(folderPath, newFileName);
                    int count = 1;
                    while (System.IO.File.Exists(filePath))
                    {
                        newFileName = FileName + "_" + count + "." + fileType;
                        filePath = Path.Combine(folderPath, newFileName);
                        count++;
                    }
                    file.SaveAs(filePath);
                    PageAttachmentRequest request = new PageAttachmentRequest();
                    request.ParentId = ParentId;
                    request.URNNo = URNNo;
                    request.FilePath = uploadFolder + "/" + newFileName;
                    request.UploadedBy = Convert.ToString(SessionPersister.LoginedUser.UserId);
                    request.FileType = fileType;
                    request.FormId = FormId;
                    request.FileName = newFileName;
                    request.FileSize = fileSize;
                    request.ItemSrNo = itemSrNo;
                    request.DetailId = detailId;
                    request.FieldId = fieldId;
                    ResponseModel response = transactionBusines.SaveAttachment(request);
                    return Json(new { success = response.IsSuccess, message = response.Response }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { success = false, message = "No file selected." }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult DownloadAttachment(string fileName)
        {
            string path = Server.MapPath(fileName);
            if (!System.IO.File.Exists(path))
            {
                return Json(new { success = false, message = "File not found" }, JsonRequestBehavior.AllowGet);
            }
            var fileBytes = System.IO.File.ReadAllBytes(path);
            return File(fileBytes, "application/octet-stream", fileName);
        }
        public ActionResult DeleteAttachment(int Id)
        {
            try
            {
                ResponseModel response = transactionBusines.DeleteAttachment(Id);
                if (response != null && response.IsSuccess.HasValue && response.IsSuccess.Value)
                {
                    string path = Server.MapPath(response.Response);
                    if (System.IO.File.Exists(path))
                    {
                        System.IO.File.Delete(path);
                    }
                }
                return Json(new { success = true, message = "Document has been deleted." }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region Detail - Button
        public async Task<ActionResult> PartialDetailButton(ItemDetailButtonRequest request)
        {
            request.UserId = Convert.ToString(SessionPersister.LoginedUser.UserId.Value);
            request.CompanyId = Convert.ToString(SessionPersister.LoginedUser.CompanyInfo.CompanyId);
            ItemDetailButtonModel buttonModel = new ItemDetailButtonModel();
            PageTabModel model = new PageTabModel();
            FormTabData tabData = await masterBusines.GetDetailButtonData(request);
            buttonModel.Data = tabData;
            buttonModel.ParentId = request.ParentId;
            buttonModel.DetailId = request.DetailId;
            buttonModel.PageAction = request.PageAction;
            buttonModel.FormId = request.FormId;
            buttonModel.ItemSrNo = request.ItemSrNo;
            buttonModel.TemplateId = request.TemplateId;
            buttonModel.FieldData = request.FieldData;
            return PartialView("_PartialDetailButton", buttonModel);
        }

        [HttpPost]
        public async Task<ActionResult> SavePageReadingData()
        {
            SubmitItemDetailButton model;

            // 1️⃣ Read raw request stream
            using (var reader = new StreamReader(Request.InputStream))
            {
                var json = await reader.ReadToEndAsync();

                // 2️⃣ Deserialize manually
                model = Newtonsoft.Json.JsonConvert
                            .DeserializeObject<SubmitItemDetailButton>(json);
            }
            model.UserId = SessionPersister.LoginedUser.UserId.Value;
            model.CompanyId = SessionPersister.LoginedUser.CompanyInfo.CompanyId;
            model.FiancialYearId = SessionPersister.LoginedUser.CompanyInfo.DefaultFiancialId;
            ResponseModel response = masterBusines.SaveDetailButtonData(model);
            return Json(response);
        }

        #endregion

        #region Send Email Auto
        private void SendTransactionEmailAsync(EmailRequest request)
        {
            try
            {
                var userId = Convert.ToString(SessionPersister.LoginedUser.UserId.Value);
                var userName = SessionPersister.LoginedUser.FirstName + " " +
                               SessionPersister.LoginedUser.LastName;
                var companyName = SessionPersister.LoginedUser.CompanyInfo.CompanyName;
                var companyId = Convert.ToString(SessionPersister.LoginedUser.CompanyInfo.CompanyId);
                var basePath = Server.MapPath("~/ExportReport/");

                Task.Run(() =>
                {
                    request.UserId = userId;
                    request.UserName = userName;
                    request.CompanyName = companyName;
                    request.CompanyId = companyId;

                    ProcessEmail(request, basePath);
                });
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
            }
        }
        private void ProcessEmail(EmailRequest request, string basePath)
        {
            ResponseModel response = new ResponseModel();

            if (request.ReportId > 0)
            {
                ReportMast report = masterBusines.getFormReport(request.ReportId);

                if (report != null)
                {
                    ResponseModel ReportResponse =
                        ReportHelper.ExportReport(request.ParentId,
                                                  report,
                                                  request.ReportFileName,
                                                  true);

                    if (ReportResponse.IsSuccess == true)
                    {
                        request.FullFilePath =
                            Path.Combine(basePath, ReportResponse.Response);

                        response = MailHelper.SendEmail(request);
                    }
                }
            }
            else
            {
                response = MailHelper.SendEmail(request);
            }
        }

        #endregion
    }
}