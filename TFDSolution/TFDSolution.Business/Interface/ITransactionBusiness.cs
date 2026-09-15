using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TFDSolution.Transport;
using TFDSolution.Transport.Common;
using TFDSolution.Transport.Master;

namespace TFDSolution.Business.Interface
{
    public interface ITransactionBusiness
    {
        Task<FormTabData> getPendingRecords(string spName, string compnayId, int fYearId, string UserId);
        ResponseModel getPendingSelectedRecords(int formPendingId, string ItemSrNos, string userId, string FormTabId);
        ResponseModel SaveBOMProcess(BomProcessModel model);
        List<BomProcessModel> GetBOMProcess(BomProcessModel model);
        BomProcessModel GetBOMProcessById(BomProcessModel model);
        ResponseModel DeleteBOMProcess(BomProcessModel model);
        ResponseModel ExecuteNextStoredProcedure(int ParentId, string spName, string CompanyId, string userId);
        #region "Calender Event"
        ResponseModel SaveEvent(FollowUpEventModel model);
        ResponseModel SaveFollowUpDateTime(int followUpId, DateTime followUpDate, string userId);
        ResponseModel SaveTRNFollowUp(FollowUpEventModel model);
        ResponseModel SaveNextFollowUpEvent(FollowUpEventModel model);
        ResponseModel DeleteFollowUp(int followUpId, string userId);
        List<FollowUpEventModel> GetAllEvents(int FollowUpEventID = 0);
        #endregion "Calender Event"
        ItemSpeficationModel GetItemSpefications(SpecificationRequest request);
        ResponseModel SaveItemSpecification(SpecificationRequest request);

        Task<ResponseModel> AmendmentProcessAsync(AmendmentRequest request);
        #region Delivery Scheduler

        Task<ResponseModel> SaveItemDeliverySchedule(DeliveryScheduleModel request);

        DeliveryScheduleModel GetItemDeliverySchedule(DeliveryScheduleModel request);
        #endregion

        #region Delivery Scheduler

        Task<ResponseModel> SaveItemStockSerial(ItemStockSerialModel request);
        ItemStockSerialModel GetItemStockSerial(ItemStockSerialModel request);
        ItemStockSerialModel GetItemStockSerialNumbers(ItemStockSerialModel request);
        #endregion

        Task<ResponseModel> ReplicationProcessAsync(AmendmentRequest request);

        #region Attachment
        ResponseModel SaveAttachment(PageAttachmentRequest request);
        List<PageAttachmentModel> GetAttachments(PageAttachmentRequest request);

        ResponseModel DeleteAttachment(int Id);
        #endregion

        List<FormFieldInfoModel> GetFieldInformation(FormFieldInfoRequest request);
        List<FormFieldInfoModel> GetFieldInforSettings(FormFieldInfoRequest request);
        ResponseModel SaveFieldInformation(FormFieldInfoModel request);
        ResponseModel DeleteFieldInformation(int Id);
        ResponseModel UpdateTabRecordSortOrder(string FormId, string tabId, int ParentId, List<TabRecordSortOrderModel> rows);
        List<FormField> DownloadImportTemplate(string tabId);
        ResponseModel ValidateImportTemplate(ExportTemplateRequest request, string jsonData);
        ResponseModel SaveImportData(SubmitFormModel model, DataTable dt);
        List<FormStatusDataView> GetFormStatusDataViews(string formId, int Id);
        List<LanguageFieldModel> GetLanguageConversationFields(string fieldId);
    }
}
