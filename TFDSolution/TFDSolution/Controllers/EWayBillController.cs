using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using iTextSharp.text.pdf.qrcode;
using log4net.Core;
using log4net.Repository.Hierarchy;
using Org.BouncyCastle.Asn1.Ocsp;
using QRCoder;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Services.Description;
using TFDIntegrations.EInvoiceBill.Model;
using TFDIntegrations.EInvoiceBill.Model.Request;
using TFDIntegrations.EInvoiceBill.Services;
using TFDIntegrations.EWayBill.Models;
using TFDIntegrations.EWayBill.Models.Request;
using TFDIntegrations.EWayBill.Models.Response;
using TFDIntegrations.EWayBill.Services;
using TFDSolution.Business;
using TFDSolution.Business.Interface;
using TFDSolution.Common;
using TFDSolution.Transport.Integration;

namespace TFDSolution.Controllers
{
    public class EWayBillController : Controller
    {
        private readonly AuthenticationService _authenticationService;
        private readonly NICAuthenticationService _nICAuthenticationService;
        private readonly EWayBillService _ewayBillService;
        private readonly IRNService _IRNService;
        private readonly IIntegrationBusiness _integrationBusiness;
        private readonly IntegrationConfiguration _integrationConfiguration;
        public EWayBillController()
        {
            _integrationConfiguration = new IntegrationConfiguration();
            _integrationBusiness = new IntegrationBusiness();
            _integrationConfiguration = _integrationBusiness.GetIntegrationConfiguration(Convert.ToString(SessionPersister.LoginedUser.CompanyInfo.CompanyId));
            //_integrationConfiguration.GSTIN = ConfigurationManager.AppSettings["EWayBill.Gstin"];
            //_integrationConfiguration.EWayBill_EWBUsername = ConfigurationManager.AppSettings["EWayBill.Username"];
            //_integrationConfiguration.EWayBill_EWBPassword = ConfigurationManager.AppSettings["EWayBill.Password"];
            _integrationConfiguration.EWayBill_AspId = ConfigurationManager.AppSettings["Integration.AspId"];
            _integrationConfiguration.EWayBill_AspPassword = ConfigurationManager.AppSettings["Integration.AspPassword"];
            _authenticationService = ServiceFactory.CreateAuthenticationService(_integrationConfiguration);
            _ewayBillService = ServiceFactory.CreateEWayBillService(_integrationConfiguration);
            _IRNService = new IRNService(_nICAuthenticationService, _integrationConfiguration);
            _nICAuthenticationService = new NICAuthenticationService(_integrationConfiguration);
        }
        //
        // POST: /EWayBill/Login
        //

        #region e-Wa Bill Integration

        public ActionResult geteWayBillPopup(string FormId, int ParentId)
        {
            EWayBillRequest request = new EWayBillRequest();
            string UserId = Convert.ToString(SessionPersister.LoginedUser.UserId.Value);
            string CompanyId = Convert.ToString(SessionPersister.LoginedUser.CompanyInfo.CompanyId);
            int finanancialYearId = Convert.ToInt32(SessionPersister.LoginedUser.CompanyInfo.DefaultFiancialId);
            string PageStatusScript = TFDSolution.Business.CommonBusiness.getFormKeySetting(FormId, "EnableEWayBill");
            if (!string.IsNullOrEmpty(PageStatusScript) && !string.IsNullOrEmpty(_integrationConfiguration.EWayBill_EWBPassword)
                 && !string.IsNullOrEmpty(_integrationConfiguration.EWayBill_EWBUsername))
            {
                request = _integrationBusiness.GetEWayBillRequestData(FormId, ParentId, UserId, CompanyId, finanancialYearId, PageStatusScript);
                if (request == null)
                {
                    request = new EWayBillRequest();
                    request.IsSuccess = false;
                    request.Response = "No data found for EWay Bill generation.";
                }
            }
            else
            {
                request.IsSuccess = false;
                request.Response = "Missing configuration. Please contact to Administrator.";
            }
            return PartialView("_EWayBillPopup", request);
        }
        public async Task<ActionResult> eWayBillAuthentication()
        {
            try
            {
                string username = _integrationConfiguration.EWayBill_EWBUsername;
                string password = _integrationConfiguration.EWayBill_EWBPassword;
                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    return Json(new { success = false, message = "EWay Bill credentials are not configured." }, JsonRequestBehavior.AllowGet);
                }
                TokenInfo token = await _authenticationService.AuthenticateAsync();
                if (token != null)
                {
                    return Json(new { success = true, message = "Authentication successful." }, JsonRequestBehavior.AllowGet);
                }
                return Json(new { success = true, message = "Authentication failed." }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        // POST: Generate EWay Bill
        //
        public async Task<ActionResult> GenerateeWayBill(string FormId, int ParentId, string sqlTableName)
        {
            try
            {
                string UserId = Convert.ToString(SessionPersister.LoginedUser.UserId.Value);
                string CompanyId = Convert.ToString(SessionPersister.LoginedUser.CompanyInfo.CompanyId);
                int finanancialYearId = Convert.ToInt32(SessionPersister.LoginedUser.CompanyInfo.DefaultFiancialId);
                string PageStatusScript = TFDSolution.Business.CommonBusiness.getFormKeySetting(FormId, "EnableEWayBill");
                EWayBillRequest request = _integrationBusiness.GetEWayBillRequestData(FormId, ParentId, UserId, CompanyId, finanancialYearId, PageStatusScript);
                GenerateEWayBillRequest model = new GenerateEWayBillRequest();
                if (request != null && request.Header != null)
                {
                    model.SupplyType = request.Header.supplyType;
                    model.SubSupplyType = request.Header.subSupplyType;
                    model.SubSupplyDesc = request.Header.subSupplyDesc;
                    model.DocumentType = request.Header.docType;
                    model.DocumentNumber = request.Header.docNo;
                    model.DocumentDate = request.Header.docDate;
                    model.FromGstin = request.Header.fromGstin;
                    model.FromTradeName = request.Header.fromTrdName;
                    model.FromAddress1 = request.Header.fromAddr2;
                    model.FromAddress2 = request.Header.fromAddr1;
                    model.FromPlace = request.Header.fromPlace;
                    model.FromPincode = Convert.ToInt32(request.Header.fromPincode);
                    model.ActualFromStateCode = int.TryParse(request.Header.actFromStateCode, out int stateCode) ? stateCode : 0;
                    model.FromStateCode = int.TryParse(request.Header.fromStateCode, out int frmstateCode) ? frmstateCode : 0;
                    //model.FromStateCode = Convert.ToInt32(request.Header.fromStateCode);
                    model.ToGstin = request.Header.toGstin;
                    model.ToTradeName = request.Header.shipToTradeName;
                    model.ToAddress1 = request.Header.toAddr1;
                    model.ToAddress2 = request.Header.toAddr2;
                    model.ToPlace = request.Header.toPlace;
                    model.ToPincode = int.TryParse(request.Header.toPincode, out int toPincode) ? toPincode : 0;
                    model.ActualToStateCode = int.TryParse(request.Header.actToStateCode, out int tostateCode) ? tostateCode : 0;
                    model.ToStateCode = int.TryParse(request.Header.toStateCode, out int tstateCode) ? tstateCode : 0;
                    model.TransactionType = request.Header.transactionType;
                    model.ShipToGSTIN = request.Header.shipToGSTIN;
                    model.ShipToTradeName = request.Header.shipToTradeName;
                    model.TotalValue = request.Header.totalValue;
                    model.CgstValue = request.Header.cgstValue;
                    model.SgstValue = request.Header.sgstValue;
                    model.IgstValue = request.Header.igstValue;
                    model.CessValue = request.Header.cessValue;
                    model.CessNonAdvolValue = request.Header.cessNonAdvolValue;
                    model.TotalInvoiceValue = request.Header.totInvValue;
                    model.TransporterId = request.Header.transporterId;
                    model.TransporterName = request.Header.transporterName;
                    model.TransportDocumentNumber = request.Header.transDocNo;
                    model.TransportMode = Convert.ToString(request.Header.transMode);
                    model.TransportDistance = Convert.ToString(request.Header.transDistance);
                    model.VehicleNumber = request.Header.vehicleNo;
                    model.VehicleType = request.Header.vehicleType;
                    if (request.Items != null && request.Items.Count > 0)
                    {
                        model.ItemList = request.Items.Select(i => new TFDIntegrations.EWayBill.Models.Request.EWayBillItem
                        {
                            ProductName = i.productName,
                            ProductDescription = i.productDesc,
                            HsnCode = Convert.ToInt32(i.hsnCode),
                            Quantity = i.quantity,
                            QuantityUnit = i.qtyUnit,
                            TaxableAmount = i.taxableAmount,
                            CgstRate = i.cgstRate,
                            SgstRate = i.sgstRate,
                            IgstRate = i.igstRate,
                            CessRate = i.cessRate
                        }).ToList();
                    }
                }
                GenerateEWayBillResponse result = await _ewayBillService.GenerateEWayBillAsync(model);
                _integrationBusiness.InsertIntegrationLogs(new IntegrationAPILog
                {
                    FormId = FormId,
                    ParentId = ParentId,
                    CreatedBy = UserId,
                    IntegrationType = "EWayBill",
                    ErrorCode = (result != null && result.error != null) ? result.error.error_cd : null,
                    HttpStatus = null,
                    Reference = (result != null) ? result.EWayBillNumber : null,
                    ApiName = "GenerateEWayBill",
                    ErrorMessage = (result != null && result.error != null) ? result.error.message : null,
                    Success = (result != null && result.status_cd == "1"),
                    RequestJson = Newtonsoft.Json.JsonConvert.SerializeObject(model),
                    ResponseJson = Newtonsoft.Json.JsonConvert.SerializeObject(result),
                    CreatedOn = DateTime.Now
                });
                if (result != null && !string.IsNullOrEmpty(result.EWayBillNumber))
                {
                    _integrationBusiness.UpdateEWayBillData(sqlTableName, ParentId, result.EWayBillNumber, result.EWayBillDate, result.ValidUpto, UserId);
                }
                return Json(new { success = (result != null && result.status_cd != "0"), message = (result != null) ? result.status_cd : null, data = result }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public async Task<JsonResult> CancelEWayBill(string FormId, int ParentId, long ewbNo, string sqlTableName)
        {
            try
            {
                string UserId = Convert.ToString(SessionPersister.LoginedUser.UserId.Value);
                string CompanyId = Convert.ToString(SessionPersister.LoginedUser.CompanyInfo.CompanyId);
                int finanancialYearId = Convert.ToInt32(SessionPersister.LoginedUser.CompanyInfo.DefaultFiancialId);
                CancelEWayBillRequest model = new CancelEWayBillRequest()
                {
                    EWayBillNumber = ewbNo,
                    CancellationReasonCode = 2,
                    CancellationRemark = "Cancelled the order"
                };
                // Authenticate if required
                TokenInfo token = _authenticationService.GetToken();
                if (token == null)
                {
                    token = await _authenticationService.AuthenticateAsync();
                }
                CancelEWayBillResponse result = await _ewayBillService.CancelEWayBillAsync(model);
                _integrationBusiness.InsertIntegrationLogs(new IntegrationAPILog
                {
                    FormId = FormId,
                    ParentId = ParentId,
                    CreatedBy = UserId,
                    IntegrationType = "EWayBill",
                    ErrorCode = null,
                    HttpStatus = null,
                    Reference = Convert.ToString(ewbNo),
                    ApiName = "CancelEWayBill",
                    ErrorMessage = (result != null ? result.Message : null),
                    Success = (result != null && result.Status == "1"),
                    RequestJson = Newtonsoft.Json.JsonConvert.SerializeObject(model),
                    ResponseJson = Newtonsoft.Json.JsonConvert.SerializeObject(result),
                    CreatedOn = DateTime.Now
                });
                if (result.error != null && !string.IsNullOrEmpty(result.error.message))
                {
                    return Json(new { success = false, message = result.error.message, data = result }, JsonRequestBehavior.AllowGet);
                }
                if (!string.IsNullOrEmpty(result.CancelDate))
                {
                    _integrationBusiness.UpdateEWayBillCancel(sqlTableName, ParentId, result.CancelDate, UserId);
                    return Json(new
                    {
                        success = true,
                        message = $"E-Way Bill {result.EWayBillNumber} has been cancelled successfully.",
                        data = result
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { success = (result != null && result.Status != null && result.Status != "0"), message = (result != null && result.Message != null) ? result.Message : null, data = result }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public async Task<JsonResult> DownloadEWayBill(long ewbNo)
        {
            try
            {
                string folder = Server.MapPath("~/ExportReport/eWayBills/");

                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }

                string fileName = $"eWayBill_{ewbNo}.pdf";
                string filePath = Path.Combine(folder, fileName);

                // If the PDF already exists, don't call the API again
                if (System.IO.File.Exists(filePath))
                {
                    return Json(new
                    {
                        success = true,
                        message = "E-Way Bill already exists.",
                        fileName = fileName,
                        filePath = Url.Content("~/ExportReport/eWayBills/" + fileName)
                    }, JsonRequestBehavior.AllowGet);
                }

                // Authenticate if required
                TokenInfo token = _authenticationService.GetToken();
                if (token == null)
                {
                    token = await _authenticationService.AuthenticateAsync();
                }

                // Generate PDF only if it doesn't already exist
                string ewayJson = await _ewayBillService.GetEWayBill(ewbNo);
                byte[] pdf = await _ewayBillService.PrintEWayBill(ewayJson);

                System.IO.File.WriteAllBytes(filePath, pdf);

                return Json(new
                {
                    success = true,
                    message = "E-Way Bill generated and saved successfully.",
                    fileName = fileName,
                    filePath = Url.Content("~/ExportReport/eWayBills/" + fileName)
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
                return Json(new
                {
                    success = false,
                    message = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }

        //
        // POST: Get EWay Bill Details
        //
        [HttpPost]
        public async Task<ActionResult> GetDetails(GetEWayBillRequest model)
        {
            try
            {
                var result = await _ewayBillService.GetEWayBillAsync(model);
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        //
        // POST: Cancel EWay Bill
        //
        [HttpPost]
        public async Task<ActionResult> Cancel(CancelEWayBillRequest model)
        {
            try
            {
                var result = await _ewayBillService.CancelEWayBillAsync(model);
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        //
        // POST: Update Vehicle
        //
        [HttpPost]
        public async Task<ActionResult> UpdateVehicle(UpdateVehicleRequest model)
        {
            try
            {
                var result = await _ewayBillService.UpdateVehicleAsync(model);
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        //
        // POST: Extend Validity
        //
        [HttpPost]
        public async Task<ActionResult> ExtendValidity(ExtendValidityRequest model)
        {
            try
            {
                var result = await _ewayBillService.ExtendValidityAsync(model);
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region e-Invoice Integration
        public ActionResult geteInvoicePopup(string FormId, int ParentId)
        {
            EInvoiceRequest request = new EInvoiceRequest();
            string UserId = Convert.ToString(SessionPersister.LoginedUser.UserId.Value);
            string CompanyId = Convert.ToString(SessionPersister.LoginedUser.CompanyInfo.CompanyId);
            int finanancialYearId = Convert.ToInt32(SessionPersister.LoginedUser.CompanyInfo.DefaultFiancialId);
            string PageStatusScript = TFDSolution.Business.CommonBusiness.getFormKeySetting(FormId, "EnableEInvoiceBill");
            if (!string.IsNullOrEmpty(PageStatusScript) && !string.IsNullOrEmpty(_integrationConfiguration.EWayBill_EWBPassword)
                 && !string.IsNullOrEmpty(_integrationConfiguration.EWayBill_EWBUsername))
            {
                request = _integrationBusiness.GetEInvoiceRequestData(FormId, ParentId, UserId, CompanyId, finanancialYearId, PageStatusScript);
                if (request == null)
                {
                    request = new EInvoiceRequest();
                    request.IsSuccess = false;
                    request.Response = "No data found for EInvoice Bill generation.";
                }
                if (request.ItemList == null) request.ItemList = new List<EInvoiceItem>();
                if (request.DispDtls == null) request.DispDtls = new DispDtls();
                if (request.TranDtls == null) request.TranDtls = new TranDtls();
                if (request.ValDtls == null) request.ValDtls = new ValDtls();
                if (request.BuyerDtls == null) request.BuyerDtls = new BuyerDtls();
                if (request.DocDtls == null) request.DocDtls = new DocDtls();
                if (request.ShipDtls == null) request.ShipDtls = new ShipDtls();
            }
            else
            {
                request.IsSuccess = false;
                request.Response = "Missing configuration. Please contact to Administrator.";
            }
            return PartialView("_EInvoiceBillPopup", request);
        }
        public async Task<JsonResult> DownloadEInvoiceBill(string Irn, string URNno, string downloadType)
        {
            try
            {
                string folder = Server.MapPath("~/ExportReport/eInvoiceBills/" + URNno + "/");
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }
                string fileName = $"eInvoiceBill_{URNno}.pdf";
                string filePath = Path.Combine(folder, fileName);
                if (downloadType != "QRCode")
                {
                    fileName = $"eInvoiceBill_{URNno}.pdf";
                    filePath = Path.Combine(folder, fileName);
                    // If the PDF already exists, don't call the API again
                    if (System.IO.File.Exists(filePath))
                    {
                        return Json(new
                        {
                            success = true,
                            message = "E-Invoice QC Code is available.",
                            fileName = fileName,
                            filePath = Url.Content("~/ExportReport/eInvoiceBills/" + URNno + "/" + fileName)
                        }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        // Authenticate if required
                        TokenInfo token = _nICAuthenticationService.GetToken();
                        if (token == null)
                        {
                            token = await _nICAuthenticationService.AuthenticateAsyncNew();
                        }
                        string QRfilePath = string.Empty;
                        string responseJson = await _IRNService.GetIRNDetails(token.AuthToken, Irn);

                        byte[] pdf = await _IRNService.PrintEInvoiceBill(responseJson, token.AuthToken);
                        System.IO.File.WriteAllBytes(filePath, pdf);

                        return Json(new
                        {
                            success = true,
                            message = "E-Invoice QC Code is available.",
                            fileName = fileName,
                            filePath = Url.Content("~/ExportReport/eInvoiceBills/" + URNno + "/" + fileName)
                        }, JsonRequestBehavior.AllowGet);

                        //DownloadSignedInvoiceResponse signedInvoiceResponse = await _IRNService.DownloadSignedInvoice(Irn, token.AuthToken, Url.Content("~/ExportReport/eInvoiceBills/" + URNno));
                        //// Generate PDF only if it doesn't already exist
                        //if (signedInvoiceResponse?.Data != null && !string.IsNullOrWhiteSpace(signedInvoiceResponse.Data.SignedInvoice))
                        //{
                        //    byte[] pdfBytes = Convert.FromBase64String(signedInvoiceResponse.Data.SignedInvoice);

                        //    QRfilePath = Path.Combine(folder, fileName);
                        //    System.IO.File.WriteAllBytes(QRfilePath, pdfBytes);
                        //}
                    }
                }
                else
                {
                    fileName = $"QR_{URNno}.png";
                    filePath = Path.Combine(folder, fileName);
                    if (System.IO.File.Exists(filePath))
                    {
                        return Json(new
                        {
                            success = true,
                            message = "E-Invoice Bill PDF is available.",
                            fileName = fileName,
                            filePath = Url.Content("~/ExportReport/eInvoiceBills/" + URNno + "/" + fileName)
                        }, JsonRequestBehavior.AllowGet);
                    }
                }
                return Json(new
                {
                    success = false,
                    message = "File not dowloaded. Please try again.",
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
                return Json(new
                {
                    success = false,
                    message = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }
        public async Task<ActionResult> eInvoiceAuthentication()
        {
            try
            {
                string username = _integrationConfiguration.EWayBill_EWBUsername;
                string password = _integrationConfiguration.EWayBill_EWBPassword;
                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    return Json(new { success = false, message = "EInvoice credentials are not configured. Please contact to administrator." }, JsonRequestBehavior.AllowGet);
                }
                TokenInfo token = await _nICAuthenticationService.AuthenticateAsyncNew();
                if (token != null && token.error == null)
                {
                    return Json(new { success = true, message = "Authentication successful." }, JsonRequestBehavior.AllowGet);
                }
                else if (token != null && token.error != null)
                {
                    return Json(new { success = false, message = token.error.message }, JsonRequestBehavior.AllowGet);
                }
                return Json(new { success = false, message = "Authentication failed. Please contact to administrator." }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        public async Task<ActionResult> GenerateeInvoiceBill(string FormId, int ParentId, string sqlTableName, string URNno)
        {
            try
            {
                EInvoiceRequest request = new EInvoiceRequest();
                string UserId = Convert.ToString(SessionPersister.LoginedUser.UserId.Value);
                string CompanyId = Convert.ToString(SessionPersister.LoginedUser.CompanyInfo.CompanyId);
                int finanancialYearId = Convert.ToInt32(SessionPersister.LoginedUser.CompanyInfo.DefaultFiancialId);
                string PageStatusScript = TFDSolution.Business.CommonBusiness.getFormKeySetting(FormId, "EnableEInvoiceBill");
                request = _integrationBusiness.GetEInvoiceRequestData(FormId, ParentId, UserId, CompanyId, finanancialYearId, PageStatusScript);
                // Authenticate if required
                TokenInfo token = _nICAuthenticationService.GetToken();
                if (token == null)
                {
                    token = await _nICAuthenticationService.AuthenticateAsyncNew();
                }
                request.Version = "1.1";
                GenerateIRNResponse result = await _IRNService.GenerateIRNNew(request, token.AuthToken);
                if (result != null && !string.IsNullOrEmpty(result.AckNo) && !string.IsNullOrEmpty(result.Irn))
                {
                    string folder = Server.MapPath("~/ExportReport/eInvoiceBills/" + URNno + "/");
                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }
                    //Save the signed invoice PDF and QR code
                    try
                    {
                        string QRfilePath = Path.Combine(folder, $"QR_{URNno}.png");

                        using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
                        {
                            QRCodeData qrCodeData = qrGenerator.CreateQrCode(result.SignedQRCode, QRCodeGenerator.ECCLevel.Q);
                            PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);
                            byte[] qrBytes = qrCode.GetGraphic(20);

                            System.IO.File.WriteAllBytes(QRfilePath, qrBytes);
                        }
                    }
                    catch (Exception)
                    {

                    }
                    // Download the signed invoice PDF
                    try
                    {
                        string filePath = Path.Combine(folder, $"eInvoiceBill_{URNno}.pdf");
                        // Download the signed invoice PDF
                        //DownloadSignedInvoiceResponse signedInvoiceResponse = await _IRNService.DownloadSignedInvoice(result.Irn, token.AuthToken);
                        //if (!string.IsNullOrWhiteSpace(result.SignedInvoice))
                        //{
                        //    byte[] pdfBytes = Convert.FromBase64String(result.SignedInvoice);

                        //    string filePath = Path.Combine(folder, $"eInvoiceBill_{URNno}.pdf");
                        //    System.IO.File.WriteAllBytes(filePath, pdfBytes);
                        //}
                        byte[] pdf = await _IRNService.PrintEInvoiceBill(result.ResponseJson, token.AuthToken);
                        System.IO.File.WriteAllBytes(filePath, pdf);
                        if (result.SignedInvoice != null)
                        {
                            string[] parts = result.SignedInvoice.Split('.');
                            if (parts.Length >= 2)
                            {
                                string payload = parts[1];
                                // Convert Base64Url to Base64
                                payload = payload.Replace('-', '+').Replace('_', '/');
                                switch (payload.Length % 4)
                                {
                                    case 2: payload += "=="; break;
                                    case 3: payload += "="; break;
                                }
                                string json = Encoding.UTF8.GetString(Convert.FromBase64String(payload));
                            }
                        }
                    }
                    catch (Exception)
                    {

                    }
                }
                _integrationBusiness.InsertIntegrationLogs(new IntegrationAPILog
                {
                    FormId = FormId,
                    ParentId = ParentId,
                    CreatedBy = UserId,
                    IntegrationType = "eInvoiceBill",
                    ErrorCode = (result != null && result.ErrorDetails != null) ? result.ErrorDetails[0].ErrorCode : null,
                    HttpStatus = null,
                    Reference = (result != null) ? result.AckNo : null,
                    ApiName = "GenerateeInvoiceBill",
                    ErrorMessage = (result != null && result.ErrorDetails != null) ? result.ErrorDetails[0].ErrorMessage : null,
                    Success = (result != null && result.Status == "0"),
                    RequestJson = Newtonsoft.Json.JsonConvert.SerializeObject(request),
                    ResponseJson = Newtonsoft.Json.JsonConvert.SerializeObject(result),
                    CreatedOn = DateTime.Now
                });
                if (result != null && !string.IsNullOrEmpty(result.AckNo))
                {
                    _integrationBusiness.UpdateEInvoiceData(sqlTableName, ParentId, result.AckNo, result.AckDt, result.Irn, result.Status.ToString(), UserId);
                }
                return Json(new { success = (result != null && result.Status != "0"), message = (result != null) ? result.Status.ToString() : null, data = result }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public async Task<JsonResult> CancelEInvoice(string FormId, int ParentId, string Irn, string sqlTableName)
        {
            try
            {
                string UserId = Convert.ToString(SessionPersister.LoginedUser.UserId.Value);
                string CompanyId = Convert.ToString(SessionPersister.LoginedUser.CompanyInfo.CompanyId);
                int finanancialYearId = Convert.ToInt32(SessionPersister.LoginedUser.CompanyInfo.DefaultFiancialId);
                CancelInvoiceRequest model = new CancelInvoiceRequest()
                {
                    Irn = Irn,
                    CnlRsn = "2",
                    CnlRem = "Cancelled the order"
                };
                // Authenticate if required
                TokenInfo token = _nICAuthenticationService.GetToken();
                if (token == null)
                {
                    token = await _nICAuthenticationService.AuthenticateAsyncNew();
                }
                CancelIRNResponse result = await _IRNService.CancelInvoice(model, token.AuthToken);
                _integrationBusiness.InsertIntegrationLogs(new IntegrationAPILog
                {
                    FormId = FormId,
                    ParentId = ParentId,
                    CreatedBy = UserId,
                    IntegrationType = "eInvoiceBill",
                    ErrorCode = null,
                    HttpStatus = null,
                    Reference = Convert.ToString(Irn),
                    ApiName = "CanceleInvoiceBill",
                    ErrorMessage = ((result != null && result.ErrorDetails != null) ? result.ErrorDetails[0].ErrorMessage : null),
                    Success = (result != null && result.Status == "\"1\""),
                    RequestJson = Newtonsoft.Json.JsonConvert.SerializeObject(model),
                    ResponseJson = Newtonsoft.Json.JsonConvert.SerializeObject(result),
                    CreatedOn = DateTime.Now
                });
                if (result.ErrorDetails != null && !string.IsNullOrEmpty(result.ErrorDetails[0].ErrorMessage))
                {
                    return Json(new { success = false, message = result.ErrorDetails[0].ErrorMessage, data = result }, JsonRequestBehavior.AllowGet);
                }
                if (!string.IsNullOrEmpty(result.CancelDate))
                {
                    _integrationBusiness.UpdateEInvoiceCancel(sqlTableName, ParentId, result.CancelDate, UserId);
                    return Json(new
                    {
                        success = true,
                        message = $"e-Invoice {result.Irn} has been cancelled successfully.",
                        data = result
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { success = (result != null && result.Status != null && result.Status != "0"), message = (result != null && result.ErrorDetails != null) ? result.ErrorDetails[0].ErrorMessage : null, data = result }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion
    }
}