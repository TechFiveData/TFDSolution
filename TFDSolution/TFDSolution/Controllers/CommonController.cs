using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using TFDSolution.App_Start;
using TFDSolution.Business;
using TFDSolution.Common;
using TFDSolution.Transport;
using TFDSolution.Transport.Master;
using Microsoft.Ajax.Utilities;
using DocumentFormat.OpenXml.Spreadsheet;

namespace TFDSolution.Controllers
{
    public class CommonController : BaseController
    {
        // GET: Common
        public ActionResult Index()
        {
            return View();
        }

        public JsonResult getNextCode(string tblName)
        {
            string _code = CommonBusiness.getNextCode(tblName);
            return Json(_code, JsonRequestBehavior.AllowGet);
        }
        public JsonResult getTranNextCode(string formName, string companyId)
        {
            string _code = CommonBusiness.getTransNextCode(formName, companyId);
            return Json(_code, JsonRequestBehavior.AllowGet);
        }
        public ActionResult getFieldSelection(string fieldId)
        {
            List<clsSelection> clsSelections = CommonBusiness.getFieldSelection(fieldId);
            return Json(clsSelections, JsonRequestBehavior.AllowGet);
        }
        public ActionResult getFieldFormatSelection(string fieldtype)
        {
            List<clsSelection> clsSelections = CommonBusiness.getFieldFormatSelection(fieldtype);
            return Json(clsSelections, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetItemFieldOtherCharges(string FieldId, string ItemId, int AccId, string FormId, List<PageFieldData> HeaderFieldData, 
            List<PageFieldData> DetailFieldData)
        {
            string companyId = Convert.ToString(SessionPersister.LoginedUser.CompanyInfo.CompanyId);
            List<clsSelection> clsSelections = CommonBusiness.GetItemFieldOtherCharges(FieldId, ItemId, AccId, FormId, companyId, HeaderFieldData, DetailFieldData);
            return Json(clsSelections, JsonRequestBehavior.AllowGet);
        }

        public JsonResult UpdateUserSetting(string CompanyId, int FinancialId)
        {
            string UserId = Convert.ToString(SessionPersister.LoginedUser.UserId.Value);
            ResponseModel response = CommonBusiness.SaveUserSettings(CompanyId, FinancialId, SessionPersister.LoginedUser.UserId.Value);
            if (response != null && response.IsSuccess.Value)
            {
                SessionPersister.LoginedUser.CompanyInfo = CommonBusiness.UserCompany(SessionPersister.LoginedUser.UserId.Value);
            }
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public JsonResult FillSelctionPaging(string FieldId, string searchTerm, int page, string fieldValue)
        {
            string CompanyId = Convert.ToString(SessionPersister.LoginedUser.CompanyInfo.CompanyId);
            FieldSelectionResponse response = CommonBusiness.getFieldSelectionLarge(FieldId, searchTerm, page, fieldValue, CompanyId);
            return Json(new { results = response.Data, pagination = new { more = response.HasMore } }, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// This is form without login pages.
        /// </summary>
        /// <param name="FieldId"></param>
        /// <param name="CompanyId"></param>
        /// <param name="searchTerm"></param>
        /// <param name="page"></param>
        /// <param name="fieldValue"></param>
        /// <returns></returns>
        public JsonResult FillSelctionPagingDefault(string FieldId, string CompanyId, string searchTerm, int page, string fieldValue)
        {
            FieldSelectionResponse response = CommonBusiness.getFieldSelectionLarge(FieldId, searchTerm, page, fieldValue, CompanyId);
            return Json(new { results = response.Data, pagination = new { more = response.HasMore } }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult FillSelctionCommon(string _type, string _whareClause, string _orderType, string _entryType)
        {
            string companyId = Convert.ToString(SessionPersister.LoginedUser.CompanyInfo.CompanyId);
            FieldSelectionResponse response = CommonBusiness.getCommonSelection(_type, companyId, _whareClause, _orderType, _entryType);
            return Json(new { results = response.Data, pagination = new { more = response.HasMore } }, JsonRequestBehavior.AllowGet);
        }

        //FillReportSelction
        public JsonResult FillReportSelction(int _Id, string searchTerm, int page)
        {
            string companyId = Convert.ToString(SessionPersister.LoginedUser.CompanyInfo.CompanyId);
            FieldSelectionResponse response = CommonBusiness.getCommonSelection(_Id, searchTerm, companyId, page);
            return Json(new { results = response.Data, pagination = new { more = response.HasMore } }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult FillSelctionPagingSP(string FieldId, string searchTerm = "", int page = 1, string fieldValue = "", 
            List<PageFieldData> pageFieldData = null, List<PageFieldData> headerFieldData = null)
        {
            SelectionRequest request = new SelectionRequest();
            request.FieldId = FieldId;
            request.SearchTerm = searchTerm;
            request.FieldValue = fieldValue;
            request.UserId = Convert.ToString(SessionPersister.LoginedUser.UserId.Value);
            request.FieldData = pageFieldData;
            request.HeaderFieldData = headerFieldData;
            request.UserId = Convert.ToString(SessionPersister.LoginedUser.UserId.Value);
            request.CompanyId = Convert.ToString(SessionPersister.LoginedUser.CompanyInfo.CompanyId);
            request.FiancialYearId = SessionPersister.LoginedUser.CompanyInfo.DefaultFiancialId;
            FieldSelectionResponse response = CommonBusiness.getFieldSelectionLargeSP(request);
            return Json(new { results = response.Data, pagination = new { more = response.HasMore } }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult FillSelctionPagingSPDefault(string FieldId, string CompanyId, string UserId, int DefaultFiancialId,
            string searchTerm = "", int page = 1,
            string fieldValue = "", List<PageFieldData> pageFieldData = null)
        {
            SelectionRequest request = new SelectionRequest();
            request.FieldId = FieldId;
            request.SearchTerm = searchTerm;
            request.FieldValue = fieldValue;
            request.UserId = UserId;
            request.FieldData = pageFieldData;
            request.CompanyId = CompanyId;
            request.FiancialYearId = DefaultFiancialId;
            FieldSelectionResponse response = CommonBusiness.getFieldSelectionLargeSP(request);
            return Json(new { results = response.Data, pagination = new { more = response.HasMore } }, JsonRequestBehavior.AllowGet);
        }
    }
}