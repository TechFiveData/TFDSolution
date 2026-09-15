using CrystalDecisions.ReportAppServer.DataDefModel;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using log4net.Repository.Hierarchy;
using Org.BouncyCastle.Asn1.Ocsp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Caching;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.UI.WebControls;
using TFDSolution.Business;
using TFDSolution.Business.Interface;
using TFDSolution.Common;
using TFDSolution.Models;
using TFDSolution.Transport;
using TFDSolution.Transport.Master;
using WebGrease.Css.Ast;

namespace TFDSolution.Controllers
{
    public class DefaultController : Controller
    {
        private readonly IMasterBusiness masterBusines;
        private readonly ITransactionBusiness transactionBusines;
        public DefaultController()
        {
            masterBusines = new MasterBusiness();
            transactionBusines = new TransactionBusiness();
        }
        // GET: Default
        public ActionResult Index()
        {
            LicenseModel model = ValidationKey.IsValid();
            string keyy = ValidationKey.CreateNewKey();
            if (model != null && model.Status == 1)
            {
                return RedirectToAction("Index", "Login");
            }
            return View(model);
        }
        [HttpPost]
        public ActionResult Index(LicenseModel model)
        {
            if (ModelState.IsValid)
            {
                LicenseModel license = ValidationKey.Validating(model.LicenseKey);
                if (license.Status == 1)
                {
                    TempData["Authorized"] = license;
                    return RedirectToAction("AuthenticationSuccess");
                }
                else
                {
                    model.Status = license.Status;
                    model.ErrorMessage = license.ErrorMessage;
                    model.Message = license.Message;
                    ModelState.AddModelError("", license.Message);
                }
            }
            return View(model);
        }

        public ActionResult AuthenticationSuccess()
        {
            LicenseModel model = ValidationKey.IsValid();
            if (model.Status == 0)
            {
                RedirectToAction("Index");
            }
            return View(model);
        }
        public static QueryParamModel GetQueryParameters(string queryString)
        {
            QueryParamModel model = new QueryParamModel();

            // Split by &
            var parameters = queryString.Split('&');
            foreach (var param in parameters)
            {
                var keyValue = param.Split('=');
                if (keyValue.Length != 2)
                    continue;

                var key = keyValue[0];
                var value = keyValue[1];
                switch (key)
                {
                    case "PendingId":
                        model.PendingId = Convert.ToInt32(value);
                        break;
                    case "DetailId":
                        model.DetailId = value;
                        model.DetailIds = value
                                            .Split(',')
                                            .Select(x => Convert.ToInt32(x))
                                            .ToList();
                        break;
                    case "ExpiryDate":
                        model.ExpiryDate = Convert.ToDateTime(value);
                        break;
                }
            }
            return model;
        }
        public ActionResult SubmitYourQuote(string data)
        {
            string pagename = "PurchaseQuotation", uId = "-1";
            PageModel pageModel = new PageModel();
            try
            {
                data = data.Replace(" ", "+");
                string decrypted = AesOperation.DecryptString(data);
                QueryParamModel model = GetQueryParameters(decrypted);

                int formPendingId = model.PendingId, RoleID = 0, DefaultFiancialId = 0;
                string ItemSrNos = model.DetailId;
                pageModel.PendingId = formPendingId;
                pageModel.ItemSrNos = ItemSrNos;
                List<List<Dictionary<string, object>>> records = masterBusines.GetPendingMaster(formPendingId, ItemSrNos, "");
                string companyID = records[0].FirstOrDefault()?["COMPANYID"]?.ToString();
                string UserId = records[0].FirstOrDefault()?["CREATEDBY"]?.ToString();
                string IsPosted = records[0].FirstOrDefault()?["STATUS"]?.ToString();
                var value = records[0].FirstOrDefault()?["FIANCIALYEARID"];
                if (value != null)
                {
                    int.TryParse(value.ToString(), out DefaultFiancialId);
                }
                CompanyMast CompanyMast = masterBusines.GetCompany(Convert.ToString(companyID));
                if (IsPosted == "6" && records[1].Count > 0)
                {
                    string URNno = records[1].FirstOrDefault()?["URNNO"]?.ToString();
                    string DocDate = records[1].FirstOrDefault()?["CREATEDON"]?.ToString();

                    RFQDataModel rFQDataModel = new RFQDataModel();
                    rFQDataModel.URNno = URNno;
                    rFQDataModel.DocDate = DocDate;
                    rFQDataModel.CompanyMast = CompanyMast;
                    TempData["ModelData"] = rFQDataModel;
                    return RedirectToAction("RequestAlreadySubmitted");
                }
                pageModel.Design = new FormMast_Data();
                ResponseModel responsePend = transactionBusines.getPendingSelectedRecords(formPendingId, ItemSrNos, UserId, "");
                List<clsSelection> _currency = masterBusines.getSelection("Currency", Convert.ToString(companyID));
                string currenctyId = Convert.ToString(CompanyMast.DefaultCurrencyId);
                FormMast_Data Design = masterBusines.GetFormStructure(pagename, uId, UserId.ToString(), RoleID);
                if (formPendingId > 0 && ItemSrNos != string.Empty)
                {
                    masterBusines.FillPendingMaster(formPendingId, ItemSrNos, "", Design);
                    Design.Data = masterBusines.GetDynamicPendingData(formPendingId, ItemSrNos, "");
                }
                string act = "A";
                string docNo = string.Empty, URLNo = string.Empty, status = "0", _exchangeRate = "";
                int docId = 0;
                if (Design != null)
                {
                    pageModel.Design = Design;
                }
                pageModel.DocNumberSetting = masterBusines.getDocFormList(Convert.ToString(pageModel.Design.FormId), Convert.ToString(companyID));
                if (!string.IsNullOrEmpty(uId) && uId != "-1")
                {
                    masterBusines.FillFieldValue(Convert.ToInt32(uId), Design, Convert.ToString(UserId));
                    Design.Data = masterBusines.GetDynamicData(pagename, Convert.ToInt32(uId), Convert.ToString(UserId));
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

                    FormDocumentNo formDocument = masterBusines.getDocNumber(Convert.ToString(pageModel.Design.FormId), DefaultFiancialId, Convert.ToString(companyID), 0, formPendingId, ItemSrNos);
                    URLNo = CommonBusiness.getTransNextCode(Convert.ToString(pageModel.Design.FormId), Convert.ToString(companyID));
                    if (formDocument != null)
                    {
                        docId = formDocument.DocNoSettingId;
                        docNo = formDocument.DocNo;
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
                ViewBag.CurrencyList = _currency;
                ViewBag.PrimaryId = uId;
                ViewBag.DocNo = docNo;
                ViewBag.URNNo = URLNo;
                ViewBag.Status = status;
                ViewBag.DocId = docId;
                ViewBag.currenctyId = currenctyId;
                pageModel.Design.CompanyId = Guid.Parse(companyID);
                pageModel.Design.UserId = Guid.Parse(UserId);
                pageModel.Design.FinancialYearId = DefaultFiancialId;
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
            if (pageModel != null && pageModel.Design == null)
            {
                pageModel.Design = new FormMast_Data();
            }
            return View(pageModel);
        }
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

        public ActionResult getHeaderDependencyDataDefault(SubmitFormModel model)
        {
            ResponseModel response = new ResponseModel();
            List<GridField> fields = masterBusines.getHeaderFieldDependency(model);
            return Json(fields, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> getPageInlineTabDataDefault(string tabId, int ParentId, int ProcessId, string FormId, string SectionId,
            string PageAction, string UserId, string CompanyId, int FiancialYearId)
        {
            PageTabModel model = new PageTabModel();
            model.FormFields = masterBusines.getFormFieldList(FormId, SectionId, tabId);
            model.Design = masterBusines.GetFormTab(tabId);
            FormTabData tabData = await masterBusines.GetDynamicTabData(tabId, ParentId, UserId, ProcessId);
            model.Data = tabData.Data;
            model.PageAction = PageAction;
            model.IsDefault = 1;
            model.CompanyId = CompanyId;
            model.UserId = UserId;
            model.FinancialYearId = FiancialYearId;
            return PartialView("~/Views/Home/_InlineTabGrid.cshtml", model);
        }
        public ActionResult getDependencyData(SubmitFormModel model)
        {
            ResponseModel response = new ResponseModel();
            model.UserId = model.UserId;
            model.CompanyId = model.CompanyId;
            model.FiancialYearId = model.FiancialYearId;
            List<GridField> fields = masterBusines.getFieldDependency(model);
            return Json(fields, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SavePageData(SubmitFormModel model)
        {
            model.UserId = model.UserId;
            model.CompanyId = model.CompanyId;
            model.FiancialYearId = model.FiancialYearId;
            ResponseModel response = masterBusines.SavePageData(model);
            //TempData["QuoteSubmittedData"] = model;
            return Json(response, JsonRequestBehavior.AllowGet);
        }
        public ActionResult RequestAlreadySubmitted()
        {
            RFQDataModel rFQDataModel = (RFQDataModel)TempData["ModelData"];
            TempData.Keep("ModelData");
            return View(rFQDataModel);
        }

        public ActionResult QuoteSubmitted(string uRNNo, int Id, int PendingId, string ItemSrNos)
        {
            RFQDataModel rFQDataModel = new RFQDataModel(); 
            //SubmitFormModel model = (SubmitFormModel)TempData["QuoteSubmittedData"];
            //TempData.Keep("QuoteSubmittedData");
            List<List<Dictionary<string, object>>> records = masterBusines.GetPendingMaster(PendingId, ItemSrNos, "");
            string URNno = uRNNo;
            string IsPosted = records[0].FirstOrDefault()?["STATUS"]?.ToString();
            string companyID = records[0].FirstOrDefault()?["COMPANYID"]?.ToString();
            string formName = records[0].FirstOrDefault()?["TOFORMNAME"]?.ToString();
            CompanyMast CompanyMast = masterBusines.GetCompany(Convert.ToString(companyID));
            if (IsPosted == "6" && records[1].Count > 0)
            {
                URNno = records[1].FirstOrDefault()?["URNNO"]?.ToString();

            }
            EmailRequest request = new EmailRequest();
            string baseUrl = $"{Request.Url.Scheme}://{Request.Url.Authority}{Request.ApplicationPath}";
            var link = $"{baseUrl}/Home/PageDetail?pagename=" + formName + "&uId=${Id}&act=V";
            request.BaseURL = link;
            Task.Run(() => SendQuoteSubmissionEmail(records, request));
            string submittedDate = DateTime.Now.ToString("dd/MM/yyyy hh:mm tt");
            rFQDataModel.URNno = URNno;
            rFQDataModel.CompanyMast = CompanyMast;
            ViewBag.SubmittedDate = submittedDate;
            return View(rFQDataModel);
        }

        private void SendQuoteSubmissionEmail(List<List<Dictionary<string, object>>> records, EmailRequest request)
        {
            try
            {
                IUserBusines userBusines = new UserBusines();
                string companyID = records[0].FirstOrDefault()?["COMPANYID"]?.ToString();

                string fromURNNo = records[0].FirstOrDefault()?["URNNO"]?.ToString();
                string vendorName = records[0].FirstOrDefault()?["ACCODE"]?.ToString();
                string fromPIDate = records[0].FirstOrDefault()?["CREATEDON"]?.ToString();
                string UserId = records[0].FirstOrDefault()?["CREATEDBY"]?.ToString();
                request.CompanyId = companyID;
                request.UserId = UserId;
                CompanyMast CompanyMast = masterBusines.GetCompany(Convert.ToString(companyID));
                if (records[1].Count > 0)
                {
                    string URNno = records[1].FirstOrDefault()?["URNNO"]?.ToString();
                    string DocDate = records[1].FirstOrDefault()?["CREATEDON"]?.ToString();
                    string subject = "Quotation Submitted Against Purchase Inquiry #" + fromURNNo;

                    Guid _UserId = Guid.Parse(request.UserId);
                    UserMast user = userBusines.GetUserInfo(_UserId);

                    request.Subject = subject;
                    // Read HTML Template
                    string templatePath = HostingEnvironment.MapPath(
                        "~/Content/EmailTemplate/RFQ_QuoteSubmittedEmail.html");
                    string emailBody = System.IO.File.ReadAllText(templatePath);
                    // Replace Placeholders
                    emailBody = emailBody.Replace("{VendorName}", vendorName);
                    emailBody = emailBody.Replace("{companyName}", CompanyMast.CompanyName);
                    emailBody = emailBody.Replace("{PINumber}", fromURNNo);
                    emailBody = emailBody.Replace("{PIDate}", fromPIDate);
                    emailBody = emailBody.Replace("{QuotationNumber}", URNno);
                    emailBody = emailBody.Replace("{QuotationDate}", DocDate);
                    emailBody = emailBody.Replace("{SubmissionDateTime}", DateTime.Now.ToString("dd/MM/yyyy hh:mm tt"));
                    emailBody = emailBody.Replace("{CurrentYear}", DateTime.Now.Year.ToString());
                    emailBody = emailBody.Replace("{companyMobile}", CompanyMast.Phone);
                    emailBody = emailBody.Replace("{companyEmail}", CompanyMast.Email);
                    emailBody = emailBody.Replace("{companyWebStite}", CompanyMast.WebStite);
                    emailBody = emailBody.Replace("{registerAddress1}", CompanyMast.RegisterAddress1);
                    emailBody = emailBody.Replace("{registerPincode}", CompanyMast.RegisterAddress2);
                    emailBody = emailBody.Replace("{ERP_Link}", request.BaseURL);
                    request.MessageBody = emailBody;
                    ResponseModel response = new ResponseModel();
                    request.ToEmail = user.EmailId;
                    request.CCEmail = CompanyMast.Email;
                    request.CompanyName = CompanyMast.CompanyName;
                    request.UserName = user.UserName;
                    request.UserId = UserId;
                    response = MailHelper.SendEmailDefault(request);
                    //await AddSentEmailAuditLog(formId, ParentId, URNNo, request.UserId);

                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex, request.UserId, request.CompanyId);
            }
        }
    }
}