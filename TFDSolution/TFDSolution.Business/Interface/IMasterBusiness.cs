using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TFDSolution.Transport;
using TFDSolution.Transport.Common;
using TFDSolution.Transport.Master;

namespace TFDSolution.Business.Interface
{
    public interface IMasterBusiness
    {
        List<FormSettings> getFormTabSettings(string formId, string tabId, string type = "");
        ResponseModel RunPageStatusScript(string pageStatusScript, string formId, int status, int ParentId, string UserId, string CompanyId);
        List<List<Dictionary<string, object>>> GetPendingMaster(int fromPendingId, string ItemSrNo, string formTabId);
        CompanyData GetCompanyData();
        List<GridField> getHeaderFieldDependency(SubmitFormModel model);
        ResponseModel SaveFormCaption(FormDataModel mast);
        DocumentModel getFormDocument(string formId, string companyId);
        FormDocumentNo getDocNumber(string formId, int FinancialYear, string companyId, int DocNoId = 0, int PendingId = 0, string PendingItemSrNos = "");
        ProcedureSchema GetProcedureRequests(string spName, string pageTableName, string fieldId);
        Task<FormData> GetPageDataPaginationAsync(string pageName, string companyId, int FinacialYearId, int start, int length, string searchValue, string userId);
        FormMast getFormData(string formId);
        //FormMast getFormDetail(string formname);
        Task<FormData> GetPageColumns(string pageName);
        List<FormMast> getForms(Guid? formId);
        List<FormMast> getFormList(string formId);
        List<FormSystemModel> getFormSystemList(string formId);
        List<FormField> getFormFieldList(string formId, string SectionId, string TabId);
        List<ReportMast> getFormReport(string ReportType, string formId);
        DataTable getGridData(string tabId, int ParentId, string userId, int ProcessId = 0);
        ResponseModel DeleteTabFieldDataRecord(int id, string TabId);
        #region "Common"     
        //List<clsSelection> getSelection(string selectionType, string whareClause = "", string orderBy = "", string entryType = "");
        List<clsSelection> getSelection(string selectionType, string CompanyId, string whareClause = "", string orderBy = "", string entryType = "");
        ItemSrNoResult GETNextItemSrNo(int parentId, string TabId, string userId, int ProcessId = 0, int DetailParentId = 0);
        #endregion "Common"

        #region "Company"
        CompanyMast GetCompany(string guid);
        ResponseModel AddCompany(CompanyMast mast);
        List<CompanyMast> GetCompanies();
        List<CompanyFinancial> GetCompanyFinancial(string CompanyId);
        ResponseModel SaveCompanyFinancial(CompanyFinancial mast);
        ResponseModel SaveCompany(CompanyMast mast);
        ResponseModel DeleteCompany(Guid guid);
        #endregion "Company"

        #region "Item"
        ItemMast GetItem(string guid);
        List<ItemMast> GetItemList();
        ResponseModel SaveItem(ItemMast mast);
        ResponseModel DeleteItem(Guid guid);
        #endregion "Item"

        #region "Party"
        PartyMst GetParty(string guid);
        List<PartyMst> GetPartyList();
        ResponseModel SaveParty(PartyMst mast);
        ResponseModel DeleteParty(Guid guid);
        #endregion "Party"
        #region "Forms&Fields"
        List<FormTab> geSectionTabs(string formId, string sectionId);
        ResponseModel SaveForm(FormMast mast);
        List<FormMast> getUserForms(System.Guid UserId, int RoleId = 0);
        List<FormMast> getChildFormListWithPermission(int RoleID, string formId);
        List<FormFieldData> GetFormFieldData(string tblName);
        ResponseModel SavePageData(SubmitFormModel model);
        ResponseModel SavePageCharges(PageOtherCharges model);
        SubmitFormModel GetFormFieldDataRecord(string tblName, int id);
        ResponseModel DeleteFormFieldDataRecord(int id, string tblName);
        List<FormFieldType> GetFormFieldTypeList();
        List<string> GetTablesByPrefix(string sourceType);
        List<SqlTemplateDto> GetSqlTemplateList();
        List<string> GetSqlTemplateDetail(int TemplateId);
        ProcedureSchema GetSourceColumns(string sourceType, string tableName, string fieldId, string pageTableName);
        FormTab GetFormTab(string tabId);
        ResponseModel SaveField(FormField mast);
        FormField GetFormField(string fieldId);
        FormMast_Data GetFormStructure(string formName, string uId, string CreatedBy, int RoleId);
        ResponseModel SaveTab(FormTab mast);
        ResponseModel DeleteFormSetting(DeleteFormSetting modal);
        List<object> getPageGridData(string pageName, string companyId, int FinacialYearId);
        //Task<IEnumerable<IDictionary<string, object>>> GetDynamicDataFromSPAsync(string pageName);
        Task<FormData> GetDynamicDataFromSPAsync(string pageName, string companyId, int FinacialYearId);
        //Task<FormTabData> GetDynamicTabDataFromSPAsync(string tabId, int ParentId, string userId, int ProcessId = 0);
        Task<FormTabData> GetDynamicTabDataFromSPAsync(string tabId, int ParentId, string userId, int ProcessId = 0, int DetailParentId = 0);
        Task<FormMast_Data> FillFieldValue(int uid, FormMast_Data Formdata, string UserId);
        FormMast_Data FillPendingMaster(int fromPendingId, string ItemSrNo, string formTabId, FormMast_Data Formdata);
        Task<PageTabModel> FillTabFieldValue(int uid, PageTabModel Tabdata);
        PageTotalData getPageTotal(string formId, int parentId, string userId);
        ResponseModel SaveOtherChargesItem(ItemOtherChargeData model);
        ResponseModel UpdateRecordStatus(string formId, int status, int ParentId, int Id, string cancelReason, string UserId);
        List<FormField> getFormFieldList(string pageName);
        ResponseModel SaveDocument(DocumentModel docModal, string CompanyId);
        List<DocumentModel> getDocFormList(string formId, string companyId);
        ResponseModel DeleteDocument(int DocuId);
        DocumentModel GetDocument(int DocuId);
        IEnumerable<IDictionary<string, object>> GetDynamicData(string pageName, int recordId, string UserId);
        IEnumerable<IDictionary<string, object>> GetDynamicPendingData(int formPendingId, string ItemSrNos, string formTabId);
        ResponseModel UpdateFielSortOrder(List<FormSortOrderUpdateModel> updatedFields);
        ResponseModel SaveFormPending(FormPendingModel pendingModal);
        List<FormPendingModel> getFormPendingList(string formId);
        FormPendingModel GetFormPending(int FormPendingId);
        ResponseModel DeleteFormPending(int FormPendingId);
        ResponseModel DeleteFormReport(int FormReportId);
        ResponseModel SaveFormReport(ReportMast model);
        ResponseModel SaveFormSystemReport(ReportMast model);
        ReportMast getFormReport(int reportId);
        List<ReportMast> getFormSystemReport(string formId);
        ReportMast getFormSystemReport(int reportId);
        #endregion "Forms&Fields"

        #region "User"
        UserDetail GetUser(string guid);
        List<UserMast> GetUserList();
        ResponseModel SaveUserDetails(UserDetail mast);
        ResponseModel SaveUserWithCompanies(UserDetail mast);
        ResponseModel DeleteUser(Guid guid);
        #endregion "User"

        #region "Roles & Permission"
        List<RoleMast> GetRoleList();
        ResponseModel DeleteRole(int id);
        RoleDetail GetRolePermissionDetails(int RoleId);
        ResponseModel SaveRolePermissionsRoleWise(RoleDetail model);

        #endregion "Roles & Permission"

        #region Form Settings
        List<FormSettings> getFormSettings(string formId, string type = null);
        ResponseModel SaveFormSettings(List<FormSettings> model);
        ResponseModel SaveFormStockSettings(List<FormStockSettings> model);
        List<FormStockSettings> getFormStockSettings(string formId);

        #endregion

        #region currency
        CompanyMast GetCurrencyByComapanyID(Guid guid);
        #endregion

        #region BankDetails
        List<CompanyBank> GetBankDetails(string guid);
        ResponseModel SavebankDetails(CompanyBank modal);
        CompanyBank getBankDetailsForEdit(Guid BankId);
        ResponseModel DeleteCompanyBankDetails(Guid BankId);
        #endregion
        #region UnitDetails
        List<CompanyUnit> GetUnitDetails(string guid);
        ResponseModel SaveUnitDetails(CompanyUnit modal);
        CompanyUnit getUnitDetailsForEdit(Guid CompanyUnitId);
        ResponseModel DeleteCompanyUnitDetails(Guid CompanyUnitId);
        #endregion

        #region "SortingOrder"
        ResponseModel UpdateFormSortOrder(List<FormSortOrderUpdateModel> updatedRows);
        ResponseModel UpdateTabSortOrder(List<FormSortOrderUpdateModel> updatedTabs);
        #endregion

        #region next Button StoreProcedure Get
        //string GetIsNextButtonStoreProcedure(string TabId);
        //ResponseModel NextProcedureButtonCall(int id, string NextProcedureValue,Guid userId);
        #endregion

        List<GridField> getFieldDependency(SubmitFormModel model);
        #region Exchange Currency
        ResponseModel ExchnageCurrency(int id);
        API_ExchangeRates ExchnageCurrencies();
        ExchangeRateResponse GetCurrencyDetails(int id);
        #endregion
        Task<FormTabData> GetDetailButtonData(ItemDetailButtonRequest request);
        Task<FormTabData> GetDynamicTabData(string tabId, int ParentId, string userId, int ProcessId = 0);
        Task<DataSet> ExportPageData(FormDataModel model);
        Task<DataSet> ExportPageTabData(FormDataModel model);
        Task<List<UserNotificationModel>> getUserNotification(string UserId, int record);
        Task<UserNotificationModel> GetNotificationDetail(int notificationId, string UserId);
        ResponseModel SavePageReadingData(SubmitItemDetailButton request);
        ResponseModel SaveDetailButtonData(SubmitItemDetailButton request);

        List<FormItemAdvanceSettings> getFormItemAdvanceSettings();
        List<FormItemAdvanceField> getFormItemAdvanceFields(int ItemAdvanceId);
        FormItemAdvanceField getFormItemAdvanceFieldDetail(string FieldId);
        FormItemAdvanceSettings getFormItemAdvanceDetail(int ItemAdvanceId);
        ResponseModel SaveFormItemAdvanceField(FormItemAdvanceField model);
        ResponseModel DeleteFormItemAdvanceSetting(DeleteFormSetting modal);
        ResponseModel UpdateItemAdvanceFieldSortOrder(List<FormSortOrderUpdateModel> updatedFields);
        ResponseModel SaveFormItemAdvanceSettings(FormItemAdvanceSettings model);
        Task<FormData> GetPageChildDataAsync(string pageName, string companyId, int FinacialYearId, int start, int length, string searchValue, string userId, int ParentId);
        int CheckFormSettingForSendEmail(string formId);
        Task<List<FormInquiryData>> GetVendorInquiryList(int parentId,string formId);
        Task<ResponseModel> AddSentEmailAuditLog(string formId, int ParentId, string UserId, string URNNo);
        Task<(List<Dictionary<string, object>> master, List<Dictionary<string, object>> details)> GetFormInquiryDetail(int formPendingId, string itemSrNo);
        ResponseModel DeleteItemDetailRecord(string ids, string TabId, int ParentId, string URNNo, string UserId, string IPAddress);
        Task<List<AuditLogUserNameData>> getUsersForAuditLog(string UserId, int RoleId);
        Task<List<UserAuditLogList>> getUsersAuditLogList(string UserId, string FromDate, string ToDate, string companyId);
    }
}