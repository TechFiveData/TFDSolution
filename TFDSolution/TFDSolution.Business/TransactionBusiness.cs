using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml.Linq;
using TFDSolution.Business.Interface;
using TFDSolution.Data;
using TFDSolution.Transport;
using TFDSolution.Transport.Common;
using TFDSolution.Transport.Master;

namespace TFDSolution.Business
{
    public class TransactionBusiness : ITransactionBusiness
    {
        private readonly IMasterBusiness masterBusines;
        public TransactionBusiness()
        {
            masterBusines = new MasterBusiness();
        }
        #region Purchase Order
        public Task<FormTabData> getPendingRecords(string spName, string compnayId, int fYearId, string UserId)
        {
            try
            {
                return CommonBusiness.ExecuteDynamicSP(spName, compnayId, fYearId, UserId);
            }
            catch (Exception ex)
            {
                CommonBusiness.SaveAppLog(new AppLog() { Logger = "TransactionBusiness.getPendingRecords", Exception = ex.StackTrace, Message = ex.Message, LogLevel = "Error" });
            }
            return null;
        }

        public ResponseModel getPendingSelectedRecords(int formPendingId, string ItemSrNos, string userId, string formTabId)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    using (var command = context.Database.Connection.CreateCommand())
                    {
                        var param1 = new SqlParameter("@formPendingId", formPendingId);
                        var param2 = new SqlParameter("@ItemSrNos", ItemSrNos);
                        var param3 = new SqlParameter { ParameterName = "@UserId", Value = userId };
                        if (CodeHelper.HasParameter("m_ItemOtherCharges", "DetailFieldsData", context))
                        {
                            var param4 = new SqlParameter { ParameterName = "@FormTabId", Value = formTabId };
                            response = context.Database.SqlQuery<ResponseModel>("EXEC get_PendingSelectedData @formPendingId, @ItemSrNos, @UserId, @FormTabId",
                            param1, param2, param3, param4).FirstOrDefault();
                        }
                        else
                        {
                            response = context.Database.SqlQuery<ResponseModel>("EXEC get_PendingSelectedData @formPendingId, @ItemSrNos, @UserId",
                            param1, param2, param3).FirstOrDefault();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Response = "Exception Occured: " + ex.Message;
            }
            return response;
        }

        #endregion
        public ResponseModel ExecuteNextStoredProcedure(int ParentId, string spName, string CompanyId, string userId)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    var param1 = new SqlParameter("@ParentId", ParentId);
                    var param2 = new SqlParameter("@CompanyId", CompanyId);
                    var param3 = new SqlParameter { ParameterName = "@UserId", Value = userId };
                    context.Database.ExecuteSqlCommand("EXEC " + spName + " @ParentId, @CompanyId, @UserId", param1, param2, param3);
                    response.IsSuccess = true;
                    response.Response = "Action has been successfully performed";
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.SaveAppLog(new AppLog() { Logger = "TransactionBusiness.ExecuteNextStoredProcedure", Exception = ex.StackTrace, Message = ex.Message, LogLevel = "Error" });
                response.Response = "Exception Occured: " + ex.Message;
                response.IsSuccess = false;
            }
            return response;
        }
        public ResponseModel SaveBOMProcess(BomProcessModel model)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    response = context.Database.SqlQuery<ResponseModel>(
                "EXEC dbo.proc_SaveBOMProcess @Id, @FormId, @ParentId, @CreatedBy, @ProcessId, @LocationId, @QCEnable",
                new SqlParameter("@Id", model.Id),
                new SqlParameter("@FormId", model.FormId),
                new SqlParameter("@ParentId", model.ParentId),
                new SqlParameter("@CreatedBy", (object)model.CreatedBy ?? DBNull.Value),
                new SqlParameter("@ProcessId", model.ProcessId),
                new SqlParameter("@LocationId", model.LocationId),
                new SqlParameter("@QCEnable", (object)model.QCEnable ?? DBNull.Value)).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.SaveAppLog(new AppLog() { Logger = "TransactionBusiness.SaveBOMProcess", Exception = ex.StackTrace, Message = ex.Message, LogLevel = "Error" });
                response.Response = "Exception occured, Please try again.";
            }
            return response;
        }

        public List<BomProcessModel> GetBOMProcess(BomProcessModel model)
        {
            List<BomProcessModel> response = new List<BomProcessModel>();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    response = context.Database.SqlQuery<BomProcessModel>(
                "EXEC dbo.proc_GetBOMProcess @FormId, @ParentId, @CreatedBy",
                new SqlParameter("@FormId", model.FormId),
                new SqlParameter("@ParentId", model.ParentId),
                new SqlParameter("@CreatedBy", (object)model.CreatedBy ?? DBNull.Value)).ToList();
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.SaveAppLog(new AppLog() { Logger = "TransactionBusiness.GetBOMProcess", Exception = ex.StackTrace, Message = ex.Message, LogLevel = "Error" });
                response = new List<BomProcessModel>();
            }
            return response;
        }
        public BomProcessModel GetBOMProcessById(BomProcessModel model)
        {
            BomProcessModel response = new BomProcessModel();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    response = context.Database.SqlQuery<BomProcessModel>(
                    "EXEC dbo.proc_GetBOMProcessBy @FormId, @ParentId, @ProcessId, @CreatedBy",
                    new SqlParameter("@FormId", model.FormId),
                    new SqlParameter("@ParentId", model.ParentId),
                    new SqlParameter("@ProcessId", model.ProcessId),
                    new SqlParameter("@CreatedBy", (object)model.CreatedBy ?? DBNull.Value)).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.SaveAppLog(new AppLog() { Logger = "TransactionBusiness.GetBOMProcessById", Exception = ex.StackTrace, Message = ex.Message, LogLevel = "Error" });
                response = new BomProcessModel();
            }
            return response;
        }

        public ResponseModel DeleteBOMProcess(BomProcessModel model)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    // Prepare parameters for the stored procedure
                    var parameters = new[] { new SqlParameter("@FormId", model.FormId), new SqlParameter("@UserId", model.CreatedBy), new SqlParameter("@Id", model.ProcessId), new SqlParameter("@ParentId", model.ParentId) };
                    response = context.Database.SqlQuery<ResponseModel>("EXEC proc_DeleteBOMProcess @FormId, @UserId, @Id, @ParentId", parameters).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.SaveAppLog(new AppLog() { Logger = "TransactionBusiness.GetBOMProcessById", Exception = ex.StackTrace, Message = ex.Message, LogLevel = "Error" });
                response.IsSuccess = false;
                response.Response = $"Error: {ex.Message}";
            }
            return response;
        }

        #region "Calender Event"
        public ResponseModel SaveEvent(FollowUpEventModel model)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    response = context.Database.SqlQuery<ResponseModel>(
                "EXEC dbo.Proc_SaveCalendarEvent @EventId, @Title, @StartDate, @EndDate, @Url"
                //,                new SqlParameter("@EventId", (object)model.F ?? DBNull.Value),
                //new SqlParameter("@Title", model.Title),
                //new SqlParameter("@StartDate", model.StartDate),
                //new SqlParameter("@EndDate", (object)model.EndDate ?? DBNull.Value),
                //new SqlParameter("@Url", model.Url)
                ).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.SaveAppLog(new AppLog() { Logger = "TransactionBusiness.SaveEvent", Exception = ex.StackTrace, Message = ex.Message, LogLevel = "Error" });
                response.Response = "Exception occured, Please try again.";
            }
            return response;
        }
        public ResponseModel SaveNextFollowUpEvent(FollowUpEventModel model)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    var result = context.Database.SqlQuery<ResponseModel>(
                        @"EXEC dbo.Proc_SaveNextFollowUpEvent @ParentFollowUpId, 
                    @FollowUpId, 
                    @FollowUpDate,
                    @Form_URNNo, 
                    @EventPriority, 
                    @AssignedTo, 
                    @ContactPerson, 
                    @Interaction, 
                    @Response, 
                    @CreatedBy, 
                    @CoAgentId,
                    @Status,
                    @ModeOfCommunication,
                    @ContactNumber",
                        new SqlParameter("@ParentFollowUpId", model.ParentFollowUpId),
                        new SqlParameter("@FollowUpId", model.FollowUpId),
                        new SqlParameter("@FollowUpDate", model.FollowUpDate),
                        new SqlParameter("@Form_URNNo", string.IsNullOrEmpty(model.Form_URNNo) ? (object)DBNull.Value : model.Form_URNNo),
                        new SqlParameter("@EventPriority", model.EventPriority),
                        new SqlParameter("@AssignedTo", string.IsNullOrEmpty(model.AssignedTo) ? "" : model.AssignedTo),
                        new SqlParameter("@ContactPerson", model.ContactPerson),
                        new SqlParameter("@Interaction", string.IsNullOrEmpty(model.Interaction) ? (object)DBNull.Value : model.Interaction),
                        new SqlParameter("@Response", string.IsNullOrEmpty(model.Response) ? (object)DBNull.Value : model.Response),
                        new SqlParameter("@CreatedBy", model.CreatedBy),
                        new SqlParameter("@CoAgentId", model.CoAgent.HasValue ? (object)model.CoAgent.Value : DBNull.Value),
                        new SqlParameter("@Status", model.Status),
                        new SqlParameter("@ModeOfCommunication", model.ModeOfCommunication),
                        new SqlParameter("@ContactNumber", model.ContactNumber)
                        ).FirstOrDefault();

                    if (result != null)
                    {
                        response = result;
                    }
                    else
                    {
                        response.Response = "No response returned from the database.";
                        response.IsSuccess = false;
                    }
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.SaveAppLog(new AppLog() { Logger = "TransactionBusiness.SaveEvent", Exception = ex.StackTrace, Message = ex.Message, LogLevel = "Error" });
                response.Response = "Exception occurred, Please try again. " + ex.Message;
                response.IsSuccess = false;
            }
            return response;
        }
        public ResponseModel SaveTRNFollowUp(FollowUpEventModel model)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    var result = context.Database.SqlQuery<ResponseModel>(
                        @"EXEC dbo.Proc_SaveTRNFollowUp @ParentFormId, 
                        @FormId,
                        @AccountID,
                        @FollowUpId, 
                        @FollowUpDate,
                        @Form_URNNo, 
                        @EventPriority, 
                        @AssignedTo, 
                        @ContactPerson,
                        @ContactNumber,
                        @Interaction, 
                        @Response, 
                        @CreatedBy, 
                        @CoAgentId,
                        @Status,
                        @ModeOfCommunication
                       ",
                        new SqlParameter("@ParentFormId", model.Form_ParentId),
                        new SqlParameter("@FormId", string.IsNullOrEmpty(model.FormId) ? "" : model.FormId),
                        new SqlParameter("@AccountID", model.AccountId),
                        new SqlParameter("@FollowUpId", model.FollowUpId),
                        new SqlParameter("@FollowUpDate", model.FollowUpDate),
                        new SqlParameter("@Form_URNNo", string.IsNullOrEmpty(model.Form_URNNo) ? (object)DBNull.Value : model.Form_URNNo),
                        new SqlParameter("@EventPriority", model.EventPriority),
                        new SqlParameter("@AssignedTo", string.IsNullOrEmpty(model.AssignedTo) ? "" : model.AssignedTo),
                        new SqlParameter("@ContactPerson", model.ContactPerson),
                        new SqlParameter("@ContactNumber", model.ContactNumber),
                        new SqlParameter("@Interaction", string.IsNullOrEmpty(model.Interaction) ? (object)DBNull.Value : model.Interaction),
                        new SqlParameter("@Response", string.IsNullOrEmpty(model.Response) ? (object)DBNull.Value : model.Response),
                        new SqlParameter("@CreatedBy", model.CreatedBy),
                        new SqlParameter("@CoAgentId", model.CoAgent.HasValue ? (object)model.CoAgent.Value : DBNull.Value),
                        new SqlParameter("@Status", model.Status),
                        new SqlParameter("@ModeOfCommunication", model.ModeOfCommunication)
                        ).FirstOrDefault();

                    if (result != null)
                    {
                        response = result;
                    }
                    else
                    {
                        response.Response = "No response returned from the database.";
                        response.IsSuccess = false;
                    }
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.SaveAppLog(new AppLog() { Logger = "TransactionBusiness.SaveTRNFollowUp", Exception = ex.StackTrace, Message = ex.Message, LogLevel = "Error" });
                response.Response = "Exception occurred, Please try again. " + ex.Message;
                response.IsSuccess = false;
            }
            return response;
        }
        public ResponseModel SaveFollowUpDateTime(int followUpId, DateTime followUpDate, string userId)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    var result = context.Database.SqlQuery<ResponseModel>(
                        @"EXEC dbo.Proc_UpdateFollowUpDateTime  @FollowUpId,  @FollowUpDate, @CreatedBy",
                        new SqlParameter("@FollowUpId", followUpId),
                        new SqlParameter("@FollowUpDate", followUpDate),
                        new SqlParameter("@CreatedBy", userId)
                    ).FirstOrDefault();

                    if (result != null)
                    {
                        response = result;
                    }
                    else
                    {
                        response.Response = "No response returned from the database.";
                        response.IsSuccess = false;
                    }
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.SaveAppLog(new AppLog() { Logger = "TransactionBusiness.SaveFollowUpDateTime", Exception = ex.StackTrace, Message = ex.Message, LogLevel = "Error" });
                response.Response = "Exception occurred, Please try again. " + ex.Message;
                response.IsSuccess = false;
            }
            return response;
        }
        public List<FollowUpEventModel> GetAllEvents(int FollowUpEventID = 0)
        {
            List<FollowUpEventModel> response = new List<FollowUpEventModel>();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    response = context.Database.SqlQuery<FollowUpEventModel>(
                        "EXEC dbo.Proc_GetAllFollowUpEvents @FollowUpId",
                        new SqlParameter("@FollowUpId", (object)FollowUpEventID ?? DBNull.Value)
                    ).ToList();
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.SaveAppLog(new AppLog() { Logger = "TransactionBusiness.GetAllEvents", Exception = ex.StackTrace, Message = ex.Message, LogLevel = "Error" });
                response = new List<FollowUpEventModel>();
            }
            return response;
        }

        public ResponseModel DeleteFollowUp(int followUpId, string userId)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    var result = context.Database.SqlQuery<ResponseModel>(
                        @"EXEC dbo.Proc_DeleteFollowUpEvent  @FollowUpId,  @UserId",
                        new SqlParameter("@FollowUpId", followUpId),
                        new SqlParameter("@UserId", userId)
                    ).FirstOrDefault();

                    if (result != null)
                    {
                        response = result;
                    }
                    else
                    {
                        response.Response = "No response returned from the database.";
                        response.IsSuccess = false;
                    }
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.SaveAppLog(new AppLog() { Logger = "TransactionBusiness.DeleteFollowUp", Exception = ex.StackTrace, Message = ex.Message, LogLevel = "Error" });
                response.Response = "Exception occurred, Please try again. " + ex.Message;
                response.IsSuccess = false;
            }
            return response;
        }


        #endregion "Calender Event"

        #region Item Specification
        public ResponseModel SaveItemSpecification(SpecificationRequest request)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                var permissionTable = new DataTable();
                permissionTable.Columns.Add("FieldName", typeof(string));
                permissionTable.Columns.Add("FieldCaption", typeof(string));
                permissionTable.Columns.Add("FieldValue", typeof(string));
                permissionTable.Columns.Add("FieldRemarks", typeof(string));

                if (request.SpecificationFields != null && request.SpecificationFields.Count > 0)
                {
                    foreach (var item in request.SpecificationFields)
                    {
                        permissionTable.Rows.Add(item.FieldName, item.FieldCaption, item.FieldValue, item.FieldRemarks);
                    }
                }

                using (var context = new TFDSolutionEntities())
                {
                    // Prepare parameters for the stored procedure
                    var parameters = new List<SqlParameter>
                    {
                        new SqlParameter("@ParentId", (object)request.ParentId ?? DBNull.Value),
                        new SqlParameter("@DetailId", (object)request.DetailId ?? DBNull.Value),
                        new SqlParameter("@CompanyId", (object)request.CompanyId ?? DBNull.Value),
                        new SqlParameter("@FiancialYearId", request.FiancialYearId),
                        new SqlParameter("@IT_CODE", request.ItemId),
                        new SqlParameter("@UserId", request.UserId),
                        new SqlParameter("@FormId", request.FormId),
                        new SqlParameter("@SpecificationTable", SqlDbType.Structured)
                        {
                            TypeName = "dbo.UDT_ItemSpecification",
                            Value = permissionTable
                        }
                    };
                    response = context.Database.SqlQuery<ResponseModel>("EXEC Proc_SaveItemSpecification @ParentId,@DetailId,@CompanyId,@FiancialYearId,@IT_CODE,@UserId,@FormId,@SpecificationTable",
                        parameters.ToArray()).FirstOrDefault();

                }
            }
            catch (Exception ex)
            {
                CommonBusiness.SaveAppLog(new AppLog() { Logger = "TransactionBusiness.SaveItemSpecification", Exception = ex.StackTrace, Message = ex.Message, LogLevel = "Error" });
                response.IsSuccess = false;
                response.Response = $"Error: {ex.Message}";
            }
            return response;
        }
        public ItemSpeficationModel GetItemSpefications(SpecificationRequest request)
        {
            ItemSpeficationModel response = new ItemSpeficationModel();
            response.ItemId = request.ItemId;
            response.ParentId = request.ParentId;
            response.DetailId = request.DetailId;
            List<FormField> Fields = new List<FormField>();
            List<SpecificationField> Data = new List<SpecificationField>();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    var conn = context.Database.Connection;
                    if (conn.State != ConnectionState.Open)
                        conn.Open();
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "m_getProductSpecifications";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@IT_CODE", request.ItemId));
                        cmd.Parameters.Add(new SqlParameter("@ParentId", request.ParentId));
                        cmd.Parameters.Add(new SqlParameter("@DetailId", request.DetailId));
                        cmd.Parameters.Add(new SqlParameter("@CreatedBy", request.UserId));
                        cmd.Parameters.Add(new SqlParameter("@FormId", request.FormId));
                        using (var reader = cmd.ExecuteReader())
                        {
                            // First result set: User main details
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    Fields.Add(new FormField
                                    {
                                        FormId = reader["FormId"] as string,
                                        FormTabId = reader["FormTabId"] as string,
                                        FieldId = reader["FieldId"] as string,
                                        FieldName = reader["FieldName"] as string,
                                        FieldCaption = reader["FieldCaption"] as string,
                                        FieldType = reader["FieldType"] as string,
                                        FieldValue = reader["FieldValue"] as string,
                                        FieldRemarks = reader["FieldRemarks"] as string,
                                    });
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.SaveAppLog(new AppLog() { Logger = "TransactionBusiness.GetItemSpefications", Exception = ex.StackTrace, Message = ex.Message, LogLevel = "Error" });
            }
            response.Data = Data;
            response.Fields = Fields;
            return response;
        }
        #endregion

        #region Item Delivery Schedule
        public async Task<ResponseModel> SaveItemDeliverySchedule(DeliveryScheduleModel request)
        {
            ResponseModel response = new ResponseModel();
            response.Response = "Delivery Schedule save failed.";
            try
            {
                var permissionTable = new DataTable();
                permissionTable.Columns.Add("ScheduledDate", typeof(DateTime));
                permissionTable.Columns.Add("ScheduledQTY", typeof(string));
                permissionTable.Columns.Add("Remarks", typeof(string));
                if (request.Data != null && request.Data.Count > 0)
                {
                    foreach (var item in request.Data)
                    {
                        DateTime date = new DateTime();
                        DateTime.TryParse(item.ScheduledDate, out date);
                        permissionTable.Rows.Add(date, item.Quantity, item.Remarks);
                    }
                }
                var fieldTable = new DataTable();
                if (request.ButtonFieldData != null && request.ButtonFieldData.Count > 0)
                {
                    fieldTable.Columns.Add("FieldName", typeof(string));
                    fieldTable.Columns.Add("FieldValue", typeof(string));
                    fieldTable.Columns.Add("DataType", typeof(string));
                    foreach (var item in request.ButtonFieldData)
                    {
                        if (item.ItemRowData != null && item.ItemRowData.Count > 0)
                        {
                            foreach (var item1 in item.ItemRowData)
                            {
                                fieldTable.Rows.Add(item1.FieldName, item1.FieldValue, "");
                            }
                        }
                    }
                }
                using (var context = new TFDSolutionEntities())
                {
                    // Prepare parameters for the stored procedure
                    var parameters = new List<SqlParameter>
                    {
                        new SqlParameter("@ParentId", (object)request.ParentId ?? DBNull.Value),
                        new SqlParameter("@DetailId", (object)request.DetailId ?? DBNull.Value),
                        new SqlParameter("@CompanyId", (object)request.CompanyId ?? DBNull.Value),
                        new SqlParameter("@FinancialYearId", request.FinacialYearId),
                        new SqlParameter("@IT_CODE", request.ItemId),
                        new SqlParameter("@UserId", request.UserId),
                        new SqlParameter("@FormId", request.FormId),
                        new SqlParameter("@ScheduledTable", SqlDbType.Structured)
                        {
                            TypeName = "dbo.UDT_ItemDeliverySchedule",
                            Value = permissionTable
                        },
                        new SqlParameter("@FieldsData", SqlDbType.Structured)
                        {
                             TypeName = "dbo.Udt_FieldsData",
                             Value = fieldTable
                        }
                    };
                    response = context.Database.SqlQuery<ResponseModel>("EXEC Proc_SaveItemDeliverySchedule @ParentId,@DetailId,@CompanyId,@FinancialYearId,@IT_CODE,@UserId,@FormId,@ScheduledTable",
                        parameters.ToArray()).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.SaveAppLog(new AppLog() { Logger = "TransactionBusiness.SaveItemDeliverySchedule", Exception = ex.StackTrace, Message = ex.Message, LogLevel = "Error" });
                response.IsSuccess = false;
                response.Response = $"Error: {ex.Message}";
            }

            return response;
        }
        public DeliveryScheduleModel GetItemDeliverySchedule(DeliveryScheduleModel request)
        {
            DeliveryScheduleModel response = new DeliveryScheduleModel();
            response.ItemId = request.ItemId;
            response.ParentId = request.ParentId;
            response.DetailId = request.DetailId;
            List<FormField> Fields = new List<FormField>();
            List<DeliveryScheduleData> Data = new List<DeliveryScheduleData>();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    var conn = context.Database.Connection;
                    if (conn.State != ConnectionState.Open)
                        conn.Open();
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "m_getProductDeliveryScheduled";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@IT_CODE", request.ItemId));
                        cmd.Parameters.Add(new SqlParameter("@ParentId", request.ParentId));
                        cmd.Parameters.Add(new SqlParameter("@DetailId", request.DetailId));
                        cmd.Parameters.Add(new SqlParameter("@CreatedBy", request.UserId));
                        cmd.Parameters.Add(new SqlParameter("@FormId", request.FormId));
                        using (var reader = cmd.ExecuteReader())
                        {
                            // First result set: Delivery Schedule details
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    Data.Add(new DeliveryScheduleData
                                    {
                                        SrNo = reader["SrNo"] != DBNull.Value
                                                   ? Convert.ToInt32(reader["SrNo"])
                                                   : 0,
                                        ScheduledDate = reader["ScheduledDate"] != DBNull.Value
                                                  ? reader["ScheduledDate"].ToString()
                                                  : string.Empty,

                                        Quantity = reader["Quantity"] != DBNull.Value
                                                   ? Convert.ToInt32(reader["Quantity"])
                                                   : 0,

                                        Remarks = reader["Remarks"] != DBNull.Value
                                                  ? reader["Remarks"].ToString()
                                                  : string.Empty
                                    });
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.SaveAppLog(new AppLog() { Logger = "TransactionBusiness.GetItemDeliverySchedule", Exception = ex.StackTrace, Message = ex.Message, LogLevel = "Error" });
            }
            response.Data = Data;
            return response;
        }
        #endregion

        #region Amendment Transaction
        public async Task<ResponseModel> AmendmentProcessAsync(AmendmentRequest request)
        {
            var response = new ResponseModel();
            response.Response = "Amendment process failed.";
            try
            {
                // TODO: Add your asynchronous processing logic here
                using (var context = new TFDSolutionEntities())
                {
                    string URLNo = CommonBusiness.getTransNextCode(Convert.ToString(request.FormId), request.CompanyId);
                    response = context.Database.SqlQuery<ResponseModel>(
                        @"EXEC Proc_CreateAmendment  @FormId,  @TransId, @CreatedBy,@URNNo, @FinacialYearId, @Reason",
                        new SqlParameter("@FormId", request.FormId),
                        new SqlParameter("@TransId", request.TransactionId),
                        new SqlParameter("@CreatedBy", request.UserId),
                        new SqlParameter("@URNNo", URLNo),
                        new SqlParameter("@FinacialYearId", request.FinacialYearId),
                        new SqlParameter("@Reason", request.Reason)
                    ).FirstOrDefault();
                    if (response.IsSuccess.HasValue && response.IsSuccess.Value)
                        response.Response = "Amend process has been completed (URN# : " + URLNo + ")";
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.SaveAppLog(new AppLog() { Logger = "TransactionBusiness.AmendmentProcessAsync", Exception = ex.StackTrace, Message = ex.Message, LogLevel = "Error" });
                // TODO: Log the exception
                response.IsSuccess = false;
                response.Response = "An error occurred while processing the amendment.";
            }
            return response;
        }
        #endregion

        #region Replication Transaction
        public async Task<ResponseModel> ReplicationProcessAsync(AmendmentRequest request)
        {
            var response = new ResponseModel();
            response.Response = "Replication process failed.";
            try
            {
                // TODO: Add your asynchronous processing logic here
                using (var context = new TFDSolutionEntities())
                {
                    string URLNo = CommonBusiness.getTransNextCode(Convert.ToString(request.FormId), request.CompanyId);
                    FormDocumentNo formDocument = CommonBusiness.getDocNumber(Convert.ToString(request.FormId), request.FinacialYearId, request.CompanyId, 0, 0, "");
                    response = context.Database.SqlQuery<ResponseModel>(
                        @"EXEC Proc_CreateReplica  @FormId,  @TransId, @CreatedBy,@URNNo,@DocNo, @FinacialYearId, @Reason",
                        new SqlParameter("@FormId", request.FormId),
                        new SqlParameter("@TransId", request.TransactionId),
                        new SqlParameter("@CreatedBy", request.UserId),
                        new SqlParameter("@URNNo", URLNo),
                        new SqlParameter("@DocNo", formDocument.DocNo ?? ""),
                        new SqlParameter("@FinacialYearId", request.FinacialYearId),
                        new SqlParameter("@Reason", request.Reason)
                    ).FirstOrDefault();
                    if (response.IsSuccess.HasValue && response.IsSuccess.Value)
                        response.Response = "Replication process has been completed (URN# : " + URLNo + ")";
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.SaveAppLog(new AppLog() { Logger = "TransactionBusiness.ReplicationProcessAsync", Exception = ex.StackTrace, Message = ex.Message, LogLevel = "Error" });
                // TODO: Log the exception
                response.IsSuccess = false;
                response.Response = "An error occurred while processing the replication.";
            }
            return response;
        }
        #endregion

        #region Item Stock Serial Number
        public async Task<ResponseModel> SaveItemStockSerial(ItemStockSerialModel request)
        {
            ResponseModel response = new ResponseModel();
            response.Response = "Item Stock Serial save failed.";
            try
            {
                var DynamicFieldTable = new DataTable();
                DynamicFieldTable.Columns.Add("FieldName", typeof(string));
                DynamicFieldTable.Columns.Add("FieldValue", typeof(string));
                DynamicFieldTable.Columns.Add("SrNo", typeof(int));

                var permissionTable = new DataTable();
                permissionTable.Columns.Add("SrNo", typeof(string));
                permissionTable.Columns.Add("SerialNo", typeof(string));
                permissionTable.Columns.Add("BatchLotNo", typeof(string));
                permissionTable.Columns.Add("Quantity", typeof(double));
                permissionTable.Columns.Add("Rate", typeof(double));
                permissionTable.Columns.Add("Amount", typeof(double));
                permissionTable.Columns.Add("Status", typeof(int));
                permissionTable.Columns.Add("MFGDate", typeof(DateTime));
                permissionTable.Columns.Add("ExpDate", typeof(DateTime));
                if (request.Data != null && request.Data.Count > 0)
                {
                    foreach (var item in request.Data)
                    {
                        permissionTable.Rows.Add(item.SrNo, item.SerialNo, item.BatchLotNo, item.Quantity, item.Rate, item.Amount, item.Status, item.MFGDate, item.ExpDate);
                    }
                }
                if (request.ButtonFieldData != null && request.ButtonFieldData.Count > 0)
                {
                    foreach (var item in request.ButtonFieldData)
                    {
                        if (item.ItemRowData != null && item.ItemRowData.Count > 0)
                        {
                            foreach (var field in item.ItemRowData)
                            {
                                DynamicFieldTable.Rows.Add(field.FieldName, field.FieldValue, item.SrNo);
                            }
                        }
                    }
                }
                using (var context = new TFDSolutionEntities())
                {
                    // Prepare parameters for the stored procedure
                    var parameters = new List<SqlParameter>
                    {
                        new SqlParameter("@ParentId", (object)request.ParentId ?? DBNull.Value),
                        new SqlParameter("@DetailId", (object)request.DetailId ?? DBNull.Value),
                        new SqlParameter("@CompanyId", (object)request.CompanyId ?? DBNull.Value),
                        new SqlParameter("@FinancialYearId", request.FinacialYearId),
                        new SqlParameter("@IT_CODE", request.ItemId),
                        new SqlParameter("@UserId", request.UserId),
                        new SqlParameter("@FormId", request.FormId),
                        new SqlParameter("@DocumentDate", request.DocumentDate),
                        new SqlParameter("@LocationId", request.LocationId),
                        new SqlParameter("@LocationName", request.LocationName),
                        new SqlParameter("@FormTabId", request.FormTabId),
                        new SqlParameter("@ItemSrNo", request.ItemSrNo),
                        new SqlParameter("@StockSerialTable", SqlDbType.Structured)
                        {
                            TypeName = "dbo.UDT_ItemStockSerial",
                            Value = permissionTable
                        },
                        new SqlParameter("@DynamicFieldsTable", SqlDbType.Structured)
                        {
                            TypeName = "dbo.Udt_DynamicFieldsData",
                            Value = DynamicFieldTable
                        }
                    };
                    response = context.Database.SqlQuery<ResponseModel>("EXEC Proc_SaveItemStockSerial @ParentId,@DetailId,@CompanyId,@FinancialYearId,@IT_CODE,@UserId,@FormId,@DocumentDate,@LocationId,@LocationName,@FormTabId,@ItemSrNo,@StockSerialTable,@DynamicFieldsTable",
                        parameters.ToArray()).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.SaveAppLog(new AppLog() { Logger = "TransactionBusiness.SaveItemStockSerial", Exception = ex.StackTrace, Message = ex.Message, LogLevel = "Error" });
                response.IsSuccess = false;
                response.Response = $"Error: {ex.Message}";
            }

            return response;
        }
        public ItemStockSerialModel GetItemStockSerialNumbers(ItemStockSerialModel request)
        {
            ItemStockSerialModel response = new ItemStockSerialModel();
            response.ItemId = request.ItemId;
            response.ParentId = request.ParentId;
            response.DetailId = request.DetailId;
            List<StockSerialData> Data = new List<StockSerialData>();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    var fieldTable = new DataTable();
                    fieldTable.Columns.Add("FieldName", typeof(string));
                    fieldTable.Columns.Add("FieldValue", typeof(string));
                    fieldTable.Columns.Add("DataType", typeof(string));
                    if (request.Fields != null && request.Fields.Count > 0)
                    {
                        foreach (var item in request.Fields)
                        {
                            fieldTable.Rows.Add(item.FieldName, item.FieldValue, "");
                        }
                    }
                    var headerfieldTable = new DataTable();
                    headerfieldTable.Columns.Add("FieldName", typeof(string));
                    headerfieldTable.Columns.Add("FieldValue", typeof(string));
                    headerfieldTable.Columns.Add("DataType", typeof(string));
                    if (request.HeaderFields != null && request.HeaderFields.Count > 0)
                    {
                        foreach (var item in request.HeaderFields)
                        {
                            headerfieldTable.Rows.Add(item.FieldName, item.FieldValue, "");
                        }
                    }
                    var conn = context.Database.Connection;
                    if (conn.State != ConnectionState.Open)
                        conn.Open();
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "m_getStockSerialNumbers";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@FormId", request.FormId));
                        cmd.Parameters.Add(new SqlParameter("@IT_CODE", request.ItemId));
                        cmd.Parameters.Add(new SqlParameter("@ParentId", request.ParentId));
                        cmd.Parameters.Add(new SqlParameter("@DetailId", request.DetailId));
                        cmd.Parameters.Add(new SqlParameter("@CreatedBy", request.UserId));
                        cmd.Parameters.Add(new SqlParameter("@BatchLotNo", request.BatchLotNo));
                        cmd.Parameters.Add(new SqlParameter("@SerialCount", request.SerialCount));
                        cmd.Parameters.Add(new SqlParameter("@CompanyId", request.CompanyId));
                        cmd.Parameters.Add(new SqlParameter("@FinancialYearId", request.FinacialYearId));
                        cmd.Parameters.Add(new SqlParameter("@LocationId", request.LocationId));
                        cmd.Parameters.Add(new SqlParameter("@TabId", request.FormTabId));
                        cmd.Parameters.Add(new SqlParameter("@ItemSrNo", request.ItemSrNo));
                        cmd.Parameters.Add(new SqlParameter("@FieldsData", SqlDbType.Structured)
                        {
                            TypeName = "dbo.Udt_FieldsData",
                            Value = fieldTable
                        });
                        cmd.Parameters.Add(new SqlParameter("@HeaderFieldsData", SqlDbType.Structured)
                        {
                            TypeName = "dbo.Udt_FieldsData",
                            Value = headerfieldTable
                        });
                        using (var reader = cmd.ExecuteReader())
                        {
                            // First result set: Delivery Schedule details
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    Data.Add(new StockSerialData
                                    {
                                        SrNo = reader["SrNo"] != DBNull.Value
                                                   ? Convert.ToInt32(reader["SrNo"])
                                                   : 0,
                                        Status = reader["Status"] != DBNull.Value
                                                   ? Convert.ToInt32(reader["Status"])
                                                   : 0,
                                        In_Quantity = reader["In_Quantity"] != DBNull.Value
                                                   ? Convert.ToDouble(reader["In_Quantity"])
                                                   : 0,
                                        Out_Quantity = reader["Out_Quantity"] != DBNull.Value
                                                   ? Convert.ToDouble(reader["Out_Quantity"]) : 0,
                                        Quantity = reader["Quantity"] != DBNull.Value
                                                   ? Convert.ToDouble(reader["Quantity"])
                                                   : 0,
                                        BatchLotNo = reader["BatchLotNo"] != DBNull.Value
                                                  ? reader["BatchLotNo"].ToString()
                                                  : string.Empty,
                                        SerialNo = reader["SerialNo"] != DBNull.Value
                                                  ? reader["SerialNo"].ToString()
                                                  : string.Empty,
                                        StockEffect = reader["StockEffect"] != DBNull.Value
                                                  ? reader["StockEffect"].ToString()
                                                  : string.Empty,
                                        Stock = reader["Stock"] != DBNull.Value
                                                   ? Convert.ToDouble(reader["Stock"])
                                                   : 0,
                                        Rate = reader["Rate"] != DBNull.Value
                                                   ? Convert.ToDouble(reader["Rate"])
                                                   : 0,
                                        Amount = reader["Amount"] != DBNull.Value
                                                   ? Convert.ToDouble(reader["Amount"])
                                                   : 0,
                                        MFGDate = reader["MFGDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["MFGDate"]),
                                        ExpDate = reader["ExpDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["ExpDate"]),
                                    });
                                }
                            }
                            #region Table 2
                            reader.NextResult();
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    var stockEffect = reader.GetString(reader.GetOrdinal("StockEffect"));
                                    bool serialWise = reader.GetBoolean(reader.GetOrdinal("SerialWise"));
                                    response.StockEffect = stockEffect;
                                    response.SerialWise = serialWise;
                                    // Process each OtherCharge
                                }
                            }
                            #endregion

                            #region Table 3
                            reader.NextResult();
                            if (reader.HasRows)
                            {
                                var table = new List<Dictionary<string, object>>();
                                while (reader.Read())
                                {
                                    var row = new Dictionary<string, object>();
                                    for (int i = 0; i < reader.FieldCount; i++)
                                    {
                                        row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                                    }
                                    table.Add(row);
                                }
                                response.DynamicData = table;
                            }
                            #endregion
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.SaveAppLog(new AppLog() { Logger = "TransactionBusiness.GetItemStockSerialNumbers", Exception = ex.StackTrace, Message = ex.Message, LogLevel = "Error" });
            }
            response.Data = Data;
            return response;
        }
        public ItemStockSerialModel GetItemStockSerial(ItemStockSerialModel request)
        {
            ItemStockSerialModel response = new ItemStockSerialModel();
            response.ItemId = request.ItemId;
            response.ParentId = request.ParentId;
            response.DetailId = request.DetailId;
            List<StockSerialData> Data = new List<StockSerialData>();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    var fieldTable = new DataTable();
                    fieldTable.Columns.Add("FieldName", typeof(string));
                    fieldTable.Columns.Add("FieldValue", typeof(string));
                    fieldTable.Columns.Add("DataType", typeof(string));
                    if (request.Fields != null && request.Fields.Count > 0)
                    {
                        foreach (var item in request.Fields)
                        {
                            fieldTable.Rows.Add(item.FieldName, item.FieldValue, "");
                        }
                    }
                    var conn = context.Database.Connection;
                    if (conn.State != ConnectionState.Open)
                        conn.Open();
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "m_getItemStockSerial";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@IT_CODE", request.ItemId));
                        cmd.Parameters.Add(new SqlParameter("@ParentId", request.ParentId));
                        cmd.Parameters.Add(new SqlParameter("@DetailId", request.DetailId));
                        cmd.Parameters.Add(new SqlParameter("@CreatedBy", request.UserId));
                        cmd.Parameters.Add(new SqlParameter("@FormId", request.FormId));
                        cmd.Parameters.Add(new SqlParameter("@TabId", request.FormTabId));
                        cmd.Parameters.Add(new SqlParameter("@LocationId", request.LocationId));
                        cmd.Parameters.Add(new SqlParameter("@ItemSrNo", request.ItemSrNo));
                        cmd.Parameters.Add(new SqlParameter("@FieldsData", SqlDbType.Structured)
                        {
                            TypeName = "dbo.Udt_FieldsData",
                            Value = fieldTable
                        });
                        using (var reader = cmd.ExecuteReader())
                        {
                            // First result set: Delivery Schedule details
                            #region Table 1
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    Data.Add(new StockSerialData
                                    {
                                        SrNo = reader["SrNo"] != DBNull.Value
                                                   ? Convert.ToInt32(reader["SrNo"])
                                                   : 0,
                                        Status = reader["Status"] != DBNull.Value
                                                   ? Convert.ToInt32(reader["Status"])
                                                   : 0,
                                        In_Quantity = reader["In_Quantity"] != DBNull.Value
                                                   ? Convert.ToDouble(reader["In_Quantity"])
                                                   : 0,
                                        Out_Quantity = reader["Out_Quantity"] != DBNull.Value
                                                   ? Convert.ToDouble(reader["Out_Quantity"]) : 0,
                                        Quantity = reader["Quantity"] != DBNull.Value
                                                   ? Convert.ToDouble(reader["Quantity"])
                                                   : 0,
                                        BatchLotNo = reader["BatchLotNo"] != DBNull.Value
                                                  ? reader["BatchLotNo"].ToString()
                                                  : string.Empty,
                                        SerialNo = reader["SerialNo"] != DBNull.Value
                                                  ? reader["SerialNo"].ToString()
                                                  : string.Empty,
                                        StockEffect = reader["StockEffect"] != DBNull.Value
                                                  ? reader["StockEffect"].ToString()
                                                  : string.Empty,
                                        Stock = reader["Stock"] != DBNull.Value
                                                   ? Convert.ToDouble(reader["Stock"])
                                                   : 0,
                                        Rate = reader["Rate"] != DBNull.Value
                                                   ? Convert.ToDouble(reader["Rate"])
                                                   : 0,
                                        Amount = reader["Amount"] != DBNull.Value
                                                   ? Convert.ToDouble(reader["Amount"])
                                                   : 0,
                                        MFGDate = reader["MFGDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["MFGDate"]),
                                        ExpDate = reader["ExpDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["ExpDate"]),
                                    });
                                }
                            }
                            #endregion

                            #region Table 2
                            reader.NextResult();
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    var stockEffect = reader.GetString(reader.GetOrdinal("StockEffect"));
                                    bool serialWise = reader.GetBoolean(reader.GetOrdinal("SerialWise"));
                                    response.StockEffect = stockEffect;
                                    response.SerialWise = serialWise;
                                    // Process each OtherCharge
                                }
                            }
                            #endregion

                            #region Table 3
                            reader.NextResult();
                            if (reader.HasRows)
                            {
                                var table = new List<Dictionary<string, object>>();
                                while (reader.Read())
                                {
                                    var row = new Dictionary<string, object>();
                                    for (int i = 0; i < reader.FieldCount; i++)
                                    {
                                        row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                                    }
                                    table.Add(row);
                                }
                                response.DynamicData = table;
                            }
                            #endregion
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.SaveAppLog(new AppLog() { Logger = "TransactionBusiness.GetItemStockSerial", Exception = ex.StackTrace, Message = ex.Message, LogLevel = "Error" });
            }
            response.Data = Data;
            return response;
        }
        #endregion

        #region Attachment
        public List<PageAttachmentModel> GetAttachments(PageAttachmentRequest request)
        {
            List<PageAttachmentModel> response = new List<PageAttachmentModel>();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    var param1 = new SqlParameter("@FormId", request.FormId);
                    var param2 = new SqlParameter("@ParentId", request.ParentId);
                    var param3 = new SqlParameter("@UploadedBy", request.UploadedBy);
                    var param4 = new SqlParameter("@DetailId", request.DetailId);
                    var param5 = new SqlParameter("@FieldId", (object)request.FieldId ?? DBNull.Value);
                    var param6 = new SqlParameter("@ItemSrNo", request.ItemSrNo);
                    response = context.Database.SqlQuery<PageAttachmentModel>("EXEC m_GetAttachments @FormId, @ParentId, @UploadedBy, @DetailId, @FieldId, @ItemSrNo",
                        param1, param2, param3, param4, param5, param6).ToList();
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.SaveAppLog(new AppLog() { Logger = "TransactionBusiness.GetAttachments", Exception = ex.StackTrace, Message = ex.Message, LogLevel = "Error" });
                response = new List<PageAttachmentModel>();
            }
            return response;
        }
        public ResponseModel SaveAttachment(PageAttachmentRequest request)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    // Prepare parameters for the stored procedure
                    var parameters = new List<SqlParameter>
                    {
                        new SqlParameter("@FormId", (object)request.FormId ?? DBNull.Value),
                        new SqlParameter("@ParentId", (object)request.ParentId ?? DBNull.Value),
                        new SqlParameter("@UploadedBy", (object)request.UploadedBy ?? DBNull.Value),
                        new SqlParameter("@URNNo", request.URNNo),
                        new SqlParameter("@FileName", request.FileName),
                        new SqlParameter("@FileType", request.FileType),
                        new SqlParameter("@FileSize", request.FileSize),
                        new SqlParameter("@FilePath", request.FilePath),
                        new SqlParameter("@DetailId", request.DetailId),
                        new SqlParameter("@FieldId", request.FieldId),
                        new SqlParameter("@ItemSrNo", request.ItemSrNo)
                    };
                    response = context.Database.SqlQuery<ResponseModel>("EXEC proc_SaveAttachment @FormId,@ParentId,@UploadedBy,@URNNo,@FileName,@FileType,@FileSize,@FilePath,@DetailId,@FieldId,@ItemSrNo",
                        parameters.ToArray()).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.SaveAppLog(new AppLog() { Logger = "TransactionBusiness.SaveAttachment", Exception = ex.StackTrace, Message = ex.Message, LogLevel = "Error" });
                response.IsSuccess = false;
                response.Response = $"Error: {ex.Message}";
            }
            return response;
        }
        public ResponseModel DeleteAttachment(int Id)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    // Prepare parameters for the stored procedure
                    var parameters = new List<SqlParameter> { new SqlParameter("@Id", Id) };
                    response = context.Database.SqlQuery<ResponseModel>("EXEC proc_DeleteAttachment @Id", parameters.ToArray()).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.SaveAppLog(new AppLog() { Logger = "TransactionBusiness.DeleteAttachment", Exception = ex.StackTrace, Message = ex.Message, LogLevel = "Error" });
                response.IsSuccess = false;
                response.Response = $"Error: {ex.Message}";
            }
            return response;
        }
        #endregion

        #region MyRegion
        public List<FormFieldInfoModel> GetFieldInformation(FormFieldInfoRequest request)
        {
            List<FormFieldInfoModel> response = new List<FormFieldInfoModel>();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    var param1 = new SqlParameter("@FieldId", request.FieldId);
                    response = context.Database.SqlQuery<FormFieldInfoModel>("EXEC proc_GetFieldInformation @FieldId", param1).ToList();

                    if (response != null && response.Count > 0)
                    {
                        foreach (var item in response)
                        {
                            if (!string.IsNullOrEmpty(item.SQLScriptName))
                            {
                                var param3 = new SqlParameter("@SPName", item.SQLScriptName);
                                var param4 = new SqlParameter("@FieldId", DBNull.Value);
                                List<ProcedureRequest> requestParam = context.Database.SqlQuery<ProcedureRequest>("EXEC proc_GetSPRequestSchema @SPName, @FieldId", param3, param4).ToList();

                                var conn = context.Database.Connection;
                                if (conn.State != ConnectionState.Open)
                                    conn.Open();
                                using (var cmd = conn.CreateCommand())
                                {
                                    cmd.CommandText = item.SQLScriptName;
                                    cmd.CommandType = CommandType.StoredProcedure;
                                    cmd.Parameters.Add(new SqlParameter("@CompanyId", request.CompanyId ?? (object)DBNull.Value));
                                    if (requestParam != null && requestParam.Count > 0)
                                    {
                                        // Build a quick lookup to avoid nested loop
                                        var fieldDataMap = request.FieldData
                                            .ToDictionary(f => f.FieldName, f => f.FieldValue, StringComparer.OrdinalIgnoreCase);
                                        foreach (var req in requestParam)
                                        {
                                            string paramName = req.PARAMETER_NAME;
                                            string fieldName = paramName.Replace("@", "");
                                            if (fieldDataMap.TryGetValue(fieldName, out var value))
                                            {
                                                cmd.Parameters.Add(new SqlParameter(paramName, value ?? (object)DBNull.Value));
                                            }
                                        }
                                    }
                                    using (var reader = cmd.ExecuteReader())
                                    {
                                        var table = new List<Dictionary<string, object>>();
                                        while (reader.Read())
                                        {
                                            var row = new Dictionary<string, object>();
                                            for (int i = 0; i < reader.FieldCount; i++)
                                            {
                                                row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                                            }
                                            table.Add(row);
                                        }
                                        item.Data = table;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.SaveAppLog(new AppLog() { Logger = "TransactionBusiness.GetFieldInformation", Exception = ex.StackTrace, Message = ex.Message, LogLevel = "Error" });
            }
            return response;
        }
        public List<FormFieldInfoModel> GetFieldInforSettings(FormFieldInfoRequest request)
        {
            List<FormFieldInfoModel> response = new List<FormFieldInfoModel>();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    var param1 = new SqlParameter("@FieldId", request.FieldId);
                    response = context.Database.SqlQuery<FormFieldInfoModel>("EXEC proc_GetFieldInformation @FieldId", param1).ToList();
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.SaveAppLog(new AppLog() { Logger = "TransactionBusiness.GetFieldInforSettings", Exception = ex.StackTrace, Message = ex.Message, LogLevel = "Error" });
            }
            return response;
        }

        public ResponseModel SaveFieldInformation(FormFieldInfoModel request)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    // Prepare parameters for the stored procedure
                    var parameters = new List<SqlParameter>
                    {
                        new SqlParameter("@InformationId", request.InformationId),
                        new SqlParameter("@FieldId", request.FieldId),
                        new SqlParameter("@SQLScriptName", request.SQLScriptName),
                        new SqlParameter("@TabName", request.TabName),
                        new SqlParameter("@UserId", request.CreatedBy),
                    };
                    response = context.Database.SqlQuery<ResponseModel>("EXEC proc_SaveFieldInformation @InformationId,@FieldId,@SQLScriptName,@TabName,@UserId",
                        parameters.ToArray()).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Response = $"Error: {ex.Message}";
            }
            return response;
        }
        public ResponseModel DeleteFieldInformation(int Id)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    // Prepare parameters for the stored procedure
                    var parameters = new List<SqlParameter> { new SqlParameter("@Id", Id) };
                    response = context.Database.SqlQuery<ResponseModel>("EXEC proc_DeleteFieldInformation @Id", parameters.ToArray()).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Response = $"Error: {ex.Message}";
            }
            return response;
        }
        #endregion

        #region Import & Export
        /// <summary>
        /// Export Template
        /// </summary>
        /// <param name="tabId"></param>
        /// <returns></returns>
        public List<FormField> DownloadImportTemplate(string tabId)
        {
            List<FormField> fields = new List<FormField>();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    var parameters = new[] { new SqlParameter("@TabId", tabId) };
                    fields = context.Database.SqlQuery<FormField>("EXEC m_ExportTemplate @TabId", parameters).ToList();
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
            }
            return fields;
        }

        public ResponseModel ValidateImportTemplate(ExportTemplateRequest request, string jsonData)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    var parameters = new[] {
                        new SqlParameter("@FormId", request.FormId),
                        new SqlParameter("@TabId", request.TabId),
                        new SqlParameter("@JsonData", jsonData)
                    };
                    response = context.Database.SqlQuery<ResponseModel>("EXEC proc_ValidateImportData @FormId, @TabId, @JsonData", parameters).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
            }
            return response;
        }

        public ResponseModel SaveImportData(SubmitFormModel model, DataTable dt)
        {
            ResponseModel response = new ResponseModel();
            string SQL = string.Empty, SQLTableName = string.Empty;
            try
            {
                int DetailId = 0, ParentId = 0;
                using (var context = new TFDSolutionEntities())
                {
                    var param1 = new SqlParameter("@FormId", model.FormId);
                    var param2 = new SqlParameter("@TabId", model.TabId);
                    List<FormField> fields = context.Database.SqlQuery<FormField>("EXEC m_FormTabImportTemplate @FormId,@TabId", param1, param2).ToList();
                    string DefaultSQL = "ParentId,URNNo,Status,CompanyId,CreatedOn,CreatedBy,FiancialYearId";
                    string DefaultSQLVal = "-1,'" + model.URNNo + "',1,'" + Convert.ToString(model.CompanyId) + "',GETDATE(),'" + Convert.ToString(model.UserId) + "'," + model.FiancialYearId + "";
                    if (model.ProcessId > 0)
                    {
                        DefaultSQL = DefaultSQL + ",ProcessId";
                        DefaultSQLVal = DefaultSQLVal + "," + model.ProcessId;
                    }
                    string ItemId = string.Empty, FieldId = string.Empty;
                    int AccId = 0;
                    PageFieldData HeaderFieldData = model.HeaderFieldData.Where(p => p.FieldName == "AcCode_Id").FirstOrDefault();
                    if (HeaderFieldData != null && !string.IsNullOrEmpty(HeaderFieldData.FieldValue))
                    {
                        AccId = Convert.ToInt32(HeaderFieldData.FieldValue);
                    }
                    DefaultSQL = DefaultSQL + ",ItemSrNo,RowIndex";
                    var fieldNames = new List<string>();
                    var fieldValues = new List<string>();
                    var tableRows = new List<string>();
                    List<PageFieldData> pageFieldData = new List<PageFieldData>();
                    if (fields != null && fields.Count > 0)
                    {
                        SQLTableName = fields.FirstOrDefault().SQLTableName;
                        foreach (var field in fields)
                        {
                            fieldNames.Add(field.FieldName);
                        }
                        string valueToAdd = "";
                        if (dt != null && dt.Rows.Count > 0)
                        {
                            // Try to find a matching value in dt for the field
                            int itemRow = 0;
                            foreach (DataRow row in dt.Rows)
                            {
                                pageFieldData = new List<PageFieldData>();
                                foreach (var col in dt.Columns.Cast<DataColumn>())
                                {
                                    string value = row[col] == DBNull.Value ? string.Empty : row[col].ToString();
                                    if (!string.IsNullOrEmpty(value))
                                    {
                                        var fieldData = fields.Where(x => x.FieldCaption == col.ColumnName).FirstOrDefault();
                                        if (fieldData != null)
                                        {
                                            pageFieldData.Add(new PageFieldData()
                                            {
                                                FieldName = fieldData.FieldName,
                                                FieldValue = row[col].ToString(),
                                                FieldText = col.ColumnName,
                                                FieldType = fieldData.FieldType
                                            });
                                        }
                                    }
                                }
                                itemRow = itemRow + 1;
                                fieldValues = new List<string>();
                                fieldValues.Add(Convert.ToString(itemRow));
                                fieldValues.Add(Convert.ToString(itemRow));
                                bool isData = false;
                                foreach (var field in fields)
                                {
                                    isData = false;
                                    if (field.FieldName.Contains("_Id") == false)
                                    {
                                        //if ((field.IsDependencyField.HasValue ? field.IsDependencyField.Value : false) == true)
                                        //{
                                        //    SubmitFormModel dependencyRequestModel = new SubmitFormModel();
                                        //    dependencyRequestModel.FromFieldName = field.FieldName;
                                        //    dependencyRequestModel.PageType = "Detail";
                                        //    dependencyRequestModel.FormId = model.FormId;
                                        //    dependencyRequestModel.Id = model.ParentId;
                                        //    dependencyRequestModel.HeaderFieldData = model.HeaderFieldData;
                                        //    dependencyRequestModel.FieldData = pageFieldData;
                                        //    dependencyRequestModel.TabId = model.TabId;
                                        //    ResponseModel dependencyresponse = new ResponseModel();
                                        //    model.UserId = model.UserId.Value;
                                        //    model.CompanyId = model.CompanyId.Value;
                                        //    model.FiancialYearId = model.FiancialYearId;
                                        //    List<GridField> dependencyfields = masterBusines.getFieldDependency(dependencyRequestModel);
                                        //    if (dependencyfields != null && dependencyfields.Count > 0)
                                        //    {
                                        //        valueToAdd = "NULL";
                                        //        fieldValues.Add(valueToAdd);
                                        //        pageFieldData.Add(new PageFieldData()
                                        //        {
                                        //            FieldName = field.FieldName,
                                        //            FieldValue = valueToAdd,
                                        //            FieldText = field.FieldCaption,
                                        //            FieldType = field.FieldType
                                        //        });
                                        //    }
                                        //}
                                        foreach (var col in dt.Columns.Cast<DataColumn>())
                                        {
                                            if (col.ColumnName == field.FieldCaption)
                                            {
                                                valueToAdd = row[col].ToString();
                                                isData = true;
                                                if (field.FieldType == "DateTimeField" &&
                                                            (valueToAdd != null && !string.IsNullOrEmpty(Convert.ToString(valueToAdd))))
                                                {
                                                    try
                                                    {
                                                        //string input = Convert.ToString(valueToAdd);
                                                        //string[] formats = { "dd-MM-yyyy", "dd-MM-yyyy HH:mm", "yyyy-MM-dd HH:mm:ss", "yyyy-MM-dd HH:mm:ss.f" };

                                                        //DateTime date = DateTime.ParseExact(
                                                        //    input,
                                                        //    formats,
                                                        //    System.Globalization.CultureInfo.InvariantCulture,
                                                        //    System.Globalization.DateTimeStyles.None
                                                        //);

                                                        string input = Convert.ToString(valueToAdd);

                                                        string[] formats = { "dd-MM-yyyy", "dd-MM-yyyy HH:mm", "dd-MM-yyyy HH:mm:ss", "yyyy-MM-dd HH:mm:ss", "yyyy-MM-dd HH:mm:ss.f" };

                                                        DateTime date = DateTime.ParseExact(
                                                            input,
                                                            formats,
                                                            System.Globalization.CultureInfo.InvariantCulture,
                                                            System.Globalization.DateTimeStyles.None
                                                        );


                                                        fieldValues.Add($"'{date.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)}'");
                                                    }
                                                    catch (Exception)
                                                    {
                                                        fieldValues.Add("NULL");
                                                    }
                                                }
                                                else if (field.FieldType == "TimeField" &&
                                                    (valueToAdd != null && !string.IsNullOrEmpty(Convert.ToString(valueToAdd))))
                                                {
                                                    try
                                                    {
                                                        string input = Convert.ToString(valueToAdd);
                                                        input = input.Replace(".", ":");
                                                        input = input.ToLower().Replace("am", "").Replace("pm", "").Trim();
                                                        input = DateTime.Now.ToString("yyyy-MM-dd") + " " + CodeHelper.ExtractTimeSimple(input);

                                                        fieldValues.Add($"'{input}'");
                                                    }
                                                    catch (Exception)
                                                    {
                                                        fieldValues.Add("NULL");
                                                    }
                                                }
                                                else
                                                {
                                                    if (pageFieldData != null
                                                        && pageFieldData.FirstOrDefault(p => p.FieldName == field.FieldName) != null)
                                                    {
                                                        var existingField = pageFieldData.FirstOrDefault(p => p.FieldName == field.FieldName);
                                                        if (existingField != null)
                                                        {
                                                            fieldValues.Add($"'{existingField.FieldValue}'");
                                                        }
                                                    }
                                                    else if ((field.IsDependencyField.HasValue ? field.IsDependencyField.Value : false) == true)
                                                    {
                                                        SubmitFormModel dependencyRequestModel = new SubmitFormModel();
                                                        dependencyRequestModel.FromFieldName = field.FieldName;
                                                        dependencyRequestModel.PageType = "Detail";
                                                        dependencyRequestModel.FormId = model.FormId;
                                                        dependencyRequestModel.Id = model.ParentId;
                                                        dependencyRequestModel.HeaderFieldData = model.HeaderFieldData;
                                                        dependencyRequestModel.FieldData = pageFieldData;
                                                        dependencyRequestModel.TabId = model.TabId;
                                                        dependencyRequestModel.CompanyId = model.CompanyId.Value;
                                                        dependencyRequestModel.FieldId = field.FieldId;
                                                        ResponseModel dependencyresponse = new ResponseModel();
                                                        model.UserId = model.UserId.Value;
                                                        model.CompanyId = model.CompanyId.Value;
                                                        model.FiancialYearId = model.FiancialYearId;
                                                        List<GridField> dependencyfields = masterBusines.getFieldDependency(dependencyRequestModel);
                                                        if (dependencyfields != null && dependencyfields.Count > 0)
                                                        {
                                                            foreach (var item in dependencyfields)
                                                            {
                                                                if (item.FieldValue != null && item.FieldValue.Count > 0)
                                                                {
                                                                    if (item.FieldValue.Count == 1)
                                                                    {
                                                                        pageFieldData.Add(new PageFieldData()
                                                                        {
                                                                            FieldName = item.FieldName,
                                                                            FieldValue = Convert.ToString(item.FieldValue[0].FieldValue),
                                                                            FieldText = item.FieldTitle
                                                                        });
                                                                        fieldValues.Add($"'{Convert.ToString(item.FieldValue[0].FieldValue)}'");
                                                                    }
                                                                    else
                                                                    {
                                                                        bool _Isfilled = false;
                                                                        foreach (var item1 in item.FieldValue)
                                                                        {
                                                                            if (field.FieldName == item1.FieldName)
                                                                            {
                                                                                pageFieldData.Add(new PageFieldData()
                                                                                {
                                                                                    FieldName = field.FieldName,
                                                                                    FieldValue = Convert.ToString(item1.FieldValue),
                                                                                    FieldText = item1.FieldText
                                                                                });
                                                                                fieldValues.Add($"'{Convert.ToString(item1.FieldValue)}'");
                                                                                _Isfilled = true;
                                                                            }
                                                                        }
                                                                        if (!_Isfilled)
                                                                        {
                                                                            foreach (var item1 in item.FieldValue)
                                                                            {
                                                                                if (field.FieldName.Contains("_Id") && (item1.FieldName.ToLower().Contains("id")))
                                                                                {
                                                                                    pageFieldData.Add(new PageFieldData()
                                                                                    {
                                                                                        FieldName = field.FieldName,
                                                                                        FieldValue = Convert.ToString(item1.FieldValue),
                                                                                        FieldText = item1.FieldText
                                                                                    });
                                                                                    fieldValues.Add($"'{Convert.ToString(item1.FieldValue)}'");
                                                                                    _Isfilled = true;
                                                                                }
                                                                            }
                                                                        }
                                                                        if (!_Isfilled)
                                                                        {
                                                                            foreach (var item1 in item.FieldValue)
                                                                            {
                                                                                if (field.FieldName.Contains("_Id") == false && (item1.FieldName.ToLower().Contains("id")) == false)
                                                                                {
                                                                                    pageFieldData.Add(new PageFieldData()
                                                                                    {
                                                                                        FieldName = field.FieldName,
                                                                                        FieldValue = Convert.ToString(item1.FieldValue),
                                                                                        FieldText = item1.FieldText
                                                                                    });
                                                                                    fieldValues.Add($"'{Convert.ToString(item1.FieldValue)}'");
                                                                                    _Isfilled = true;
                                                                                }
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                        else
                                                        {
                                                            fieldValues.Add($"N'{valueToAdd}'");
                                                        }
                                                    }
                                                    else
                                                    {
                                                        if (!string.IsNullOrEmpty(field.FieldFormula))
                                                        {
                                                            string finalForm = field.FieldFormula;
                                                            foreach (var item in pageFieldData)
                                                            {
                                                                finalForm = finalForm.Replace("{" + item.FieldName + "}", item.FieldValue);
                                                            }
                                                            if (finalForm.Contains("{") == false)
                                                            {
                                                                try
                                                                {
                                                                    decimal value = Convert.ToDecimal(new DataTable().Compute(finalForm, null));
                                                                    valueToAdd = Math.Round(value, field.FieldDecimal).ToString();
                                                                }
                                                                catch (Exception)
                                                                {
                                                                    valueToAdd = "0";
                                                                }
                                                                fieldValues.Add($"N'{valueToAdd}'");
                                                            }
                                                        }
                                                        else
                                                        {
                                                            fieldValues.Add($"N'{valueToAdd}'");
                                                        }
                                                    }
                                                }

                                                if (pageFieldData.Where(ex => ex.FieldName == field.FieldName).FirstOrDefault() == null)
                                                {
                                                    pageFieldData.Add(new PageFieldData()
                                                    {
                                                        FieldName = field.FieldName,
                                                        FieldValue = valueToAdd,
                                                        FieldText = field.FieldCaption,
                                                        FieldType = field.FieldType
                                                    });
                                                }
                                                break;
                                            }
                                        }
                                        if (isData == false)
                                        {
                                            if (pageFieldData != null
                                                       && pageFieldData.FirstOrDefault(p => p.FieldName == field.FieldName) != null)
                                            {
                                                var existingField = pageFieldData.FirstOrDefault(p => p.FieldName == field.FieldName);
                                                if (existingField != null)
                                                {
                                                    fieldValues.Add($"'{existingField.FieldValue}'");
                                                }
                                                else
                                                {
                                                    fieldValues.Add("NULL");
                                                }
                                            }
                                            else
                                            {
                                                fieldValues.Add("NULL");
                                            }
                                        }
                                    }
                                    else
                                    {
                                        var caption = field.FieldCaption.Replace(" ", "").Trim().ToUpper();
                                        // For fields ending with _Id, we assume integer type                                        
                                        if (field.DDLSourceType == "Table")
                                        {
                                            try
                                            {
                                                string strValue = Convert.ToString(row[field.FieldCaption]).Trim();
                                                string sqlRef = "SELECT " + field.DDLValueField + " FROM " + field.DDLSourceName + " WITH(NOLOCK) WHERE " + field.DDLTextField + " = @FieldValue";
                                                var paramField = new SqlParameter("@FieldValue", strValue);
                                                var refValue = context.Database.SqlQuery<int?>(sqlRef, paramField).FirstOrDefault();
                                                if (refValue != null && refValue.HasValue)
                                                {
                                                    if (field.FieldName.ToUpper().Contains("ITCODE")
                                                        || field.FieldName.ToUpper().Contains("IT_CODE")
                                                        || (field.DDLSourceName == "m_MastProduct_Master"))
                                                    {
                                                        ItemId = Convert.ToString(refValue.Value);
                                                    }
                                                    if (field.DDLSourceName == "m_MastAccountMaster")
                                                    {
                                                        AccId = Convert.ToInt32(Convert.ToString(refValue.Value));
                                                    }
                                                    fieldValues.Add(Convert.ToString(refValue.Value));
                                                    if (pageFieldData != null
                                                       && pageFieldData.FirstOrDefault(p => p.FieldName == field.FieldName) != null)
                                                    {
                                                        var existingField = pageFieldData.FirstOrDefault(p => p.FieldName == field.FieldName);
                                                        if (existingField != null)
                                                        {
                                                            fieldValues.Add($"'{refValue.Value}'");
                                                        }
                                                        else
                                                        {
                                                            fieldValues.Add("NULL");
                                                        }
                                                    }
                                                    else
                                                    {
                                                        if (pageFieldData.Where(ex => ex.FieldName == field.FieldName).FirstOrDefault() == null)
                                                        {
                                                            pageFieldData.Add(new PageFieldData()
                                                            {
                                                                FieldName = field.FieldName,
                                                                FieldValue = Convert.ToString(refValue.Value),
                                                                FieldText = field.FieldCaption,
                                                                FieldType = field.FieldType
                                                            });
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    fieldValues.Add("0");
                                                }
                                            }
                                            catch (Exception)
                                            {
                                                fieldValues.Add("0");
                                            }
                                        }
                                        else if (field.DDLSourceType == "StoredProcedure" && field.DDLSourceName == "Selection_PO_Approval")
                                        {
                                            try
                                            {
                                                string strValue = Convert.ToString(row[field.FieldCaption]).Trim();
                                                string sqlRef = "select distinct CAST(UserId AS NVARCHAR(37)) from m_UserMast WHERE CONCAT(FirstName,'-',LastName) LIKE '%' + @FieldValue + '%'";
                                                var paramField = new SqlParameter("@FieldValue", strValue);
                                                string refValue = context.Database.SqlQuery<String>(sqlRef, paramField).FirstOrDefault();
                                                if (refValue != null)
                                                {
                                                    if (field.FieldName.ToUpper().Contains("ITCODE")
                                                       || field.FieldName.ToUpper().Contains("IT_CODE")
                                                       || (field.DDLSourceName == "m_MastProduct_Master"))
                                                    {
                                                        ItemId = Convert.ToString(refValue);
                                                    }
                                                    fieldValues.Add($"'{Convert.ToString(refValue)}'");
                                                }
                                                else
                                                {
                                                    fieldValues.Add("0");
                                                }
                                            }
                                            catch (Exception)
                                            {
                                                fieldValues.Add("0");
                                            }
                                        }
                                        else if ((field.IsDependencyField.HasValue ? field.IsDependencyField.Value : false) == false)
                                        {
                                            try
                                            {
                                                string searchTerm = Convert.ToString(row[field.FieldCaption]);
                                                if (field.DDLSourceType == "StoredProcedure")
                                                {

                                                    List<PageFieldData> headerFieldData = new List<PageFieldData>();
                                                    //FillSelctionPagingSP
                                                    SelectionRequest request = new SelectionRequest();
                                                    request.FieldId = field.FieldId;
                                                    request.SearchTerm = searchTerm;
                                                    request.FieldValue = "";
                                                    request.UserId = Convert.ToString(model.UserId.Value);
                                                    request.FieldData = pageFieldData;
                                                    request.HeaderFieldData = model.HeaderFieldData;
                                                    request.UserId = Convert.ToString(model.UserId.Value);
                                                    request.CompanyId = Convert.ToString(model.CompanyId.Value);
                                                    request.FiancialYearId = model.FiancialYearId;
                                                    FieldSelectionResponse selectionLargeSPresponse = CommonBusiness.getFieldSelectionLargeSP(request);
                                                    if (selectionLargeSPresponse != null && selectionLargeSPresponse.Data != null && selectionLargeSPresponse.Data.Count > 0)
                                                    {
                                                        fieldValues.Add(Convert.ToString(selectionLargeSPresponse.Data[0].id));
                                                        pageFieldData.Add(new PageFieldData()
                                                        {
                                                            FieldName = field.FieldName,
                                                            FieldValue = Convert.ToString(selectionLargeSPresponse.Data[0].id),
                                                            FieldText = field.FieldCaption,
                                                            FieldType = field.FieldType
                                                        });
                                                        SubmitFormModel dependencyRequestModel = new SubmitFormModel();
                                                        dependencyRequestModel.FromFieldName = field.FieldName;
                                                        dependencyRequestModel.PageType = "Detail";
                                                        dependencyRequestModel.FormId = model.FormId;
                                                        dependencyRequestModel.Id = model.ParentId;
                                                        dependencyRequestModel.HeaderFieldData = model.HeaderFieldData;
                                                        dependencyRequestModel.FieldData = pageFieldData;
                                                        dependencyRequestModel.TabId = model.TabId;
                                                        dependencyRequestModel.FieldId = field.FieldId;
                                                        dependencyRequestModel.CompanyId = model.CompanyId.Value;
                                                        ResponseModel dependencyresponse = new ResponseModel();
                                                        dependencyRequestModel.UserId = model.UserId.Value;
                                                        dependencyRequestModel.CompanyId = model.CompanyId.Value;
                                                        dependencyRequestModel.FiancialYearId = model.FiancialYearId;
                                                        List<GridField> dependencyfields = masterBusines.getFieldDependency(dependencyRequestModel);
                                                        if (dependencyfields != null && dependencyfields.Count > 0)
                                                        {
                                                            foreach (var item in dependencyfields)
                                                            {
                                                                if (item.FieldValue != null && item.FieldValue.Count > 0)
                                                                {
                                                                    if (item.FieldValue.Count == 1)
                                                                    {
                                                                        pageFieldData.Add(new PageFieldData()
                                                                        {
                                                                            FieldName = item.FieldName,
                                                                            FieldValue = Convert.ToString(item.FieldValue[0].FieldValue),
                                                                            FieldText = item.FieldTitle
                                                                        });
                                                                        //fieldValues.Add($"'{Convert.ToString(item.FieldValue[0].FieldValue)}'");
                                                                    }
                                                                    else
                                                                    {
                                                                        bool _Isfilled = false;
                                                                        foreach (var item1 in item.FieldValue)
                                                                        {
                                                                            if (field.FieldName == item1.FieldName)
                                                                            {
                                                                                pageFieldData.Add(new PageFieldData()
                                                                                {
                                                                                    FieldName = field.FieldName,
                                                                                    FieldValue = Convert.ToString(item1.FieldValue),
                                                                                    FieldText = item1.FieldText
                                                                                });
                                                                                fieldValues.Add($"'{Convert.ToString(item1.FieldValue)}'");
                                                                                _Isfilled = true;
                                                                            }
                                                                        }
                                                                        if (!_Isfilled)
                                                                        {
                                                                            foreach (var item1 in item.FieldValue)
                                                                            {
                                                                                if (field.FieldName.Contains("_Id") && (item1.FieldName.ToLower().Contains("id")))
                                                                                {
                                                                                    pageFieldData.Add(new PageFieldData()
                                                                                    {
                                                                                        FieldName = field.FieldName,
                                                                                        FieldValue = Convert.ToString(item1.FieldValue),
                                                                                        FieldText = item1.FieldText
                                                                                    });
                                                                                    //fieldValues.Add($"'{Convert.ToString(item1.FieldValue)}'");
                                                                                    _Isfilled = true;
                                                                                }
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        fieldValues.Add(Convert.ToString("0"));
                                                        pageFieldData.Add(new PageFieldData()
                                                        {
                                                            FieldName = field.FieldName,
                                                            FieldValue = Convert.ToString("0"),
                                                            FieldText = field.FieldCaption,
                                                            FieldType = field.FieldType
                                                        });
                                                    }
                                                }
                                                else
                                                {
                                                    //FillSelctionPaging
                                                    FieldSelectionResponse selectionLargeResponse = CommonBusiness.getFieldSelectionLarge(field.FieldId, searchTerm, 1, "", Convert.ToString(model.CompanyId.Value));
                                                    if (selectionLargeResponse != null && selectionLargeResponse.Data != null && selectionLargeResponse.Data.Count > 0)
                                                    {
                                                        fieldValues.Add(Convert.ToString(selectionLargeResponse.Data[0].id));
                                                        pageFieldData.Add(new PageFieldData()
                                                        {
                                                            FieldName = field.FieldName,
                                                            FieldValue = Convert.ToString(selectionLargeResponse.Data[0].id),
                                                            FieldText = field.FieldCaption,
                                                            FieldType = field.FieldType
                                                        });
                                                    }
                                                }
                                            }
                                            catch (Exception)
                                            {
                                                fieldValues.Add("0");
                                            }
                                        }
                                        else if ((field.IsDependencyField.HasValue ? field.IsDependencyField.Value : false) == true)
                                        {
                                            SubmitFormModel dependencyRequestModel = new SubmitFormModel();
                                            dependencyRequestModel.FromFieldName = field.FieldName;
                                            dependencyRequestModel.PageType = "Detail";
                                            dependencyRequestModel.FormId = model.FormId;
                                            dependencyRequestModel.Id = model.ParentId;
                                            dependencyRequestModel.HeaderFieldData = model.HeaderFieldData;
                                            dependencyRequestModel.FieldData = pageFieldData;
                                            dependencyRequestModel.TabId = model.TabId;
                                            dependencyRequestModel.FieldId = field.FieldId;
                                            dependencyRequestModel.CompanyId = model.CompanyId.Value;
                                            ResponseModel dependencyresponse = new ResponseModel();
                                            model.UserId = model.UserId.Value;
                                            model.CompanyId = model.CompanyId.Value;
                                            model.FiancialYearId = model.FiancialYearId;
                                            List<GridField> dependencyfields = masterBusines.getFieldDependency(dependencyRequestModel);
                                            if (dependencyfields != null && dependencyfields.Count > 0)
                                            {
                                                foreach (var item in dependencyfields)
                                                {
                                                    if (item.FieldValue != null && item.FieldValue.Count > 0)
                                                    {
                                                        if (item.FieldValue.Count == 1)
                                                        {
                                                            pageFieldData.Add(new PageFieldData()
                                                            {
                                                                FieldName = item.FieldName,
                                                                FieldValue = Convert.ToString(item.FieldValue[0].FieldValue),
                                                                FieldText = item.FieldTitle
                                                            });
                                                            fieldValues.Add($"'{Convert.ToString(item.FieldValue[0].FieldValue)}'");
                                                        }
                                                        else
                                                        {
                                                            bool _Isfilled = false;
                                                            foreach (var item1 in item.FieldValue)
                                                            {
                                                                if (field.FieldName == item1.FieldName)
                                                                {
                                                                    pageFieldData.Add(new PageFieldData()
                                                                    {
                                                                        FieldName = field.FieldName,
                                                                        FieldValue = Convert.ToString(item1.FieldValue),
                                                                        FieldText = item1.FieldText
                                                                    });
                                                                    fieldValues.Add($"'{Convert.ToString(item1.FieldValue)}'");
                                                                    _Isfilled = true;
                                                                }
                                                            }
                                                            if (!_Isfilled)
                                                            {
                                                                foreach (var item1 in item.FieldValue)
                                                                {
                                                                    if (field.FieldName.Contains("_Id") && (item1.FieldName.ToLower().Contains("id")))
                                                                    {
                                                                        pageFieldData.Add(new PageFieldData()
                                                                        {
                                                                            FieldName = field.FieldName,
                                                                            FieldValue = Convert.ToString(item1.FieldValue),
                                                                            FieldText = item1.FieldText
                                                                        });
                                                                        fieldValues.Add($"'{Convert.ToString(item1.FieldValue)}'");
                                                                        _Isfilled = true;
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                if (field.FieldName.Contains("_Id") == false)
                                                {
                                                    if (pageFieldData != null
                                                           && pageFieldData.FirstOrDefault(p => p.FieldName == field.FieldName) != null)
                                                    {
                                                        var existingField = pageFieldData.FirstOrDefault(p => p.FieldName == field.FieldName);
                                                        if (existingField != null)
                                                        {
                                                            fieldValues.Add($"'{existingField.FieldValue}'");
                                                        }
                                                    }
                                                    else
                                                    {
                                                        fieldValues.Add("0");
                                                    }
                                                }
                                                else
                                                {
                                                    fieldValues.Add("0");
                                                }
                                            }
                                        }
                                        #region Dependency Bindings

                                        //List<FieldDependencySQL> formFields = new List<FieldDependencySQL>();

                                        //var param1 = new SqlParameter("@tabId", model.TabId);
                                        //var param2 = new SqlParameter("@FromFieldName", field.FieldName);
                                        //var param3 = new SqlParameter("@FieldId", (field.FieldId != null ? Convert.ToString(field.FieldId) : ""));
                                        //formFields = context.Database.SqlQuery<FieldDependencySQL>("EXEC proc_getDependencyFields @tabId, @FromFieldName, @FieldId", param1, param2, param3).ToList();
                                        //if (formFields != null && formFields.Count > 0)
                                        //{
                                        //    foreach (var item in formFields)
                                        //    {

                                        //    }
                                        //}
                                        //string sqlProc = $@" SELECT 'EXEC {field.DDLSourceName} ' + STRING_AGG(p.name + ' = _' + p.name, ', ')
                                        //        WITHIN GROUP (ORDER BY p.parameter_id) FROM sys.parameters p WHERE p.object_id = OBJECT_ID('{field.DDLSourceName}');";

                                        //string sqlQuery = context.Database.SqlQuery<string>(sqlProc).FirstOrDefault();
                                        //if (!string.IsNullOrEmpty(sqlQuery))
                                        //{
                                        //    if (sqlQuery.Contains("_@CompanyId"))
                                        //        sqlQuery = sqlQuery.Replace("_@CompanyId", "'" + Convert.ToString(model.CompanyId) + "'");
                                        //    if (sqlQuery.Contains("_@CreatedBy"))
                                        //        sqlQuery = sqlQuery.Replace("_@CreatedBy", "'" + Convert.ToString(model.UserId) + "'");
                                        //    if (sqlQuery.Contains("@Head_") == false && sqlQuery.Contains("_@ParentId"))
                                        //        sqlQuery = sqlQuery.Replace("_@ParentId", "'" + Convert.ToString(model.ParentId) + "'");

                                        //    foreach (var item in fields)
                                        //    {
                                        //        if (sqlQuery.Contains("_@" + item.FieldName + ""))
                                        //        {
                                        //            foreach (var col in dt.Columns.Cast<DataColumn>())
                                        //            {
                                        //                if (col.ColumnName == item.FieldCaption)
                                        //                {
                                        //                    string colValue = Convert.ToString(row[col]);
                                        //                    sqlQuery = sqlQuery.Replace("_@" + item.FieldName + "", "'" + Convert.ToString(colValue) + "'");
                                        //                }
                                        //            }
                                        //        }
                                        //    }
                                        //    if (sqlQuery.Contains("_@") == false)
                                        //    {
                                        //        DataSet dst = DbHelper.GetDataSet(context, sqlQuery, CommandType.Text, null);
                                        //        if (dst != null && dst.Tables.Count > 0)
                                        //        {
                                        //            if (dst.Tables[0].Rows.Count > 0)
                                        //            {
                                        //                List<PageFieldData> fieldDatas = new List<PageFieldData>();
                                        //                foreach (DataRow Ditem in dst.Tables[0].Rows)
                                        //                {
                                        //                    foreach (DataColumn col in Ditem.Table.Columns)
                                        //                    {
                                        //                        string columnName = col.ColumnName;
                                        //                        string value = Convert.ToString(Ditem[columnName]);
                                        //                        fieldDatas.Add(new PageFieldData()
                                        //                        {
                                        //                            FieldName = col.ColumnName,
                                        //                            FieldValue = value
                                        //                        });
                                        //                    }
                                        //                }
                                        //            }
                                        //        }
                                        //    }
                                        //}
                                    }
                                    #endregion
                                }
                                tableRows.Add("(" + DefaultSQLVal + "," + string.Join(",", fieldValues) + ") ");
                            }
                        }
                        else
                        {
                            response.IsSuccess = false;
                            response.Response = "Record could not be retrieved. Please try again.";
                            return response;
                        }
                    }

                    DefaultSQL = DefaultSQL + "," + string.Join(",", fieldNames);
                    DefaultSQLVal = string.Empty;
                    string[] columns = DefaultSQL.Split(',');
                    foreach (var item in tableRows)
                    {
                        string[] Colvalues = item.Trim()
                                                  .TrimStart('(')
                                                  .TrimEnd(')')
                                                  .Split(',');

                        for (int i = 0; i < columns.Length; i++)
                        {
                            string columnName = columns[i].Trim();
                            // Current column is xxx_Id
                            if (columnName.EndsWith("_Id", StringComparison.OrdinalIgnoreCase))
                            {
                                string fieldName = columnName.Substring(0, columnName.Length - 3);
                                // Find corresponding FieldName column
                                int fieldIndex = Array.FindIndex(columns, c => c.Trim().Equals(fieldName, StringComparison.OrdinalIgnoreCase));
                                if (fieldIndex >= 0)
                                {
                                    if (Colvalues.Length >= i && Colvalues[i] != null)
                                    {
                                        string idValue = Colvalues[i].Trim();
                                        // Remove quotes if value is quoted
                                        string cleanIdValue = idValue.Trim('\'');
                                        if (cleanIdValue == "0")
                                        {
                                            Colvalues[fieldIndex] = "NULL";
                                        }
                                    }
                                }
                            }
                        }
                        if (DefaultSQLVal == string.Empty)
                        {
                            DefaultSQLVal = string.Join(",", Colvalues);
                        }
                        else
                        {
                            DefaultSQLVal = DefaultSQLVal + "),(" + string.Join(",", Colvalues);
                        }
                    }
                    DefaultSQLVal = "(" + string.Join(",", DefaultSQLVal) + ")";
                    SQL = @"INSERT INTO " + SQLTableName + " (" + DefaultSQL + ") " + "OUTPUT INSERTED.Id " + "VALUES " + DefaultSQLVal + ";";
                    int primaryId = context.Database.SqlQuery<int>(SQL).FirstOrDefault();
                    if (primaryId > 0)
                    {
                        DataTable dtDetails = new DataTable();

                        string sql = $@"SELECT * FROM {SQLTableName} WHERE ParentId IN (0, -1) AND CreatedBy = @UserId AND CompanyId = @CompanyId";
                        using (SqlConnection con = (SqlConnection)context.Database.Connection)
                        {
                            if (con.State != ConnectionState.Open)
                                con.Open();

                            using (SqlCommand cmd = new SqlCommand(sql, con))
                            {
                                cmd.Parameters.Add("@UserId", SqlDbType.UniqueIdentifier).Value = model.UserId;
                                cmd.Parameters.Add("@CompanyId", SqlDbType.UniqueIdentifier).Value = model.CompanyId;

                                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                                {
                                    da.Fill(dtDetails);
                                }
                            }
                        }
                        if (dtDetails?.Rows.Count > 0)
                        {
                            foreach (DataRow row in dtDetails.Rows)
                            {
                                int id = Convert.ToInt32(row["Id"]);
                                int _itemId = 0;
                                decimal ItemAmount = 0;
                                if (row.Table.Columns.Contains("ITCode_Id") && row["ITCode_Id"] != DBNull.Value &&
                                    !string.IsNullOrWhiteSpace(row["ITCode_Id"]?.ToString()))
                                {
                                    _itemId = Convert.ToInt32(row["ITCode_Id"]);
                                }

                                if (row.Table.Columns.Contains("Amount") && row["Amount"] != DBNull.Value &&
                                    !string.IsNullOrWhiteSpace(row["Amount"]?.ToString()))
                                {
                                    ItemAmount = Convert.ToDecimal(row["Amount"]);
                                }
                                if (ItemAmount > 0 && _itemId > 0)
                                {
                                    #region Other Charges
                                    List<clsSelection> clsSelections = CommonBusiness.GetItemFieldOtherCharges(FieldId, _itemId.ToString(), AccId, model.FormId, Convert.ToString(model.CompanyId), model.HeaderFieldData, new List<PageFieldData>());
                                    if (clsSelections != null && clsSelections.Count > 0)
                                    {
                                        PageOtherCharges modelOtherCharges = new PageOtherCharges();
                                        List<ItemOtherCharges> FieldData = new List<ItemOtherCharges>();
                                        modelOtherCharges.DetailId = id;
                                        modelOtherCharges.ParentId = model.ParentId;
                                        modelOtherCharges.FormId = model.FormId;
                                        modelOtherCharges.UserId = model.UserId.Value;
                                        decimal totalAmount = ItemAmount;
                                        int chargesId = Convert.ToInt32(clsSelections[0].Value);
                                        IConfigurationBusiness config = new ConfigurationBusiness();
                                        OtherChargesModel otherChargesResponse = config.GetItemOtherCharges(chargesId, ParentId, DetailId, model.FormId);
                                        //Store calculated values by Alias
                                        Dictionary<string, decimal> aliasValues = new Dictionary<string, decimal>();
                                        //A = Item Total
                                        Dictionary<string, decimal> values = new Dictionary<string, decimal>();
                                        values["A"] = totalAmount;
                                        aliasValues["A"] = totalAmount;
                                        decimal grandTotal = totalAmount;
                                        if (otherChargesResponse != null && otherChargesResponse.Details != null &&
                                            otherChargesResponse.Details.Count > 0)
                                        {
                                            foreach (var item in otherChargesResponse.Details.OrderBy(x => x.SrNo))
                                            {
                                                decimal calculatedValue = 0;
                                                if (item.PerAmount == "Amount")
                                                {
                                                    calculatedValue = Convert.ToDecimal(item.ChargesValue);
                                                }
                                                else
                                                {
                                                    calculatedValue = EvaluateFormula(
                                                        Convert.ToString(item.Formula).ToUpper(),
                                                        item.ChargesValue,
                                                        values);
                                                }
                                                values[item.Alias] = calculatedValue;
                                                item.StoredAmount = Convert.ToDouble(calculatedValue);

                                                FieldData.Add(new ItemOtherCharges()
                                                {
                                                    Amount = calculatedValue,
                                                    OtherChargesDetailId = item.OtherChargesDetailId,
                                                    Percentage = Convert.ToDecimal(item.ChargesValue),
                                                    TaxChargeName = item.TaxChargeName
                                                });
                                            }
                                        }
                                        modelOtherCharges.FieldData = FieldData;
                                        ResponseModel responseModel = masterBusines.SavePageCharges(modelOtherCharges);
                                        if (responseModel != null && responseModel.IsSuccess.Value)
                                        {
                                            string sql1 = $"UPDATE [{SQLTableName}] SET ChargesId = @ChargesId WHERE Id = @Id";

                                            using (var context1 = new TFDSolutionEntities())
                                            {
                                                context1.Database.ExecuteSqlCommand(
                                                    sql1,
                                                    new SqlParameter("@ChargesId", chargesId),
                                                    new SqlParameter("@Id", id)
                                                );
                                            }

                                            //string connectionString = ConfigurationManager.ConnectionStrings["YourConnectionString"].ConnectionString;
                                            //using (SqlConnection con = new SqlConnection(connectionString))
                                            //{
                                            //    con.Open();

                                            //    using (SqlCommand cmd = new SqlCommand(sql, con))
                                            //    {
                                            //        cmd.Parameters.AddWithValue("@ChargesId", chargesId);
                                            //        cmd.Parameters.AddWithValue("@Id", id);

                                            //        cmd.ExecuteNonQuery();
                                            //    }
                                            //}
                                            //context.Database.ExecuteSqlCommand(sql1, chargesId, id);
                                        }
                                    }
                                    #endregion
                                }
                                // Your logic here
                            }
                        }
                    }

                    response.Id = primaryId;
                    response.IsSuccess = true;
                }
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Response = "Import Failed. Please check the file and try again.";
                CommonBusiness.SaveAppLog(new AppLog()
                {
                    Logger = "TransactionBusiness.SaveImportData",
                    Exception = SQL,
                    Message = ex.Message,
                    LogLevel = "Error",
                    CompanyId = Convert.ToString(model.CompanyId),
                    UserId = Convert.ToString(model.UserId)
                });
            }
            return response;
        }

        #endregion
        private decimal EvaluateFormula(string formula, double percentage, Dictionary<string, decimal> values)
        {
            formula = formula.TrimStart('=');

            //Replace aliases with actual values
            foreach (var pair in values)
            {
                formula = Regex.Replace(
                    formula,
                    $@"\b{pair.Key}\b",
                    pair.Value.ToString(CultureInfo.InvariantCulture));
            }

            //Replace current alias (percentage)
            formula = Regex.Replace(
                formula,
                @"\b[A-Z]\b",
                percentage.ToString(CultureInfo.InvariantCulture));

            var table = new DataTable();

            return Convert.ToDecimal(table.Compute(formula, ""));
        }
        public List<FormStatusDataView> GetFormStatusDataViews(string formId, int Id)
        {
            List<FormStatusDataView> result = new List<FormStatusDataView>();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    var parameters = new[] { new SqlParameter("@FormId", formId), new SqlParameter("@ParentId", Id) };
                    result = context.Database.SqlQuery<FormStatusDataView>("EXEC m_PageRecordStatusDetail @FormId,@ParentId", parameters).ToList();
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.SaveAppLog(new AppLog() { Logger = "TransactionBusiness.GetFormStatusDataViews", Exception = ex.StackTrace, Message = ex.Message, LogLevel = "Error" });
            }
            return result;
        }

        public List<LanguageFieldModel> GetLanguageConversationFields(string fieldId)
        {
            List<LanguageFieldModel> fields = new List<LanguageFieldModel>();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    var parameters = new[] { new SqlParameter("@FieldId", fieldId) };
                    fields = context.Database.SqlQuery<LanguageFieldModel>(@"SELECT FieldName, LanguageCode FROM m_FormLanguageFields WITH(NOLOCK) WHERE FieldId = @FieldId",
                        parameters).ToList();
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
            }
            return fields;
        }

        public ResponseModel UpdateTabRecordSortOrder(string FormId, string tabId, int ParentId, List<TabRecordSortOrderModel> rows)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    // Create DataTable for TVP
                    DataTable dt = new DataTable();
                    dt.Columns.Add("Id", typeof(int));
                    dt.Columns.Add("SortOrder", typeof(int));
                    if (rows != null)
                    {
                        foreach (var item in rows)
                        {
                            dt.Rows.Add(item.Id, item.SortOrder);
                        }
                    }
                    SqlParameter tabIdParam = new SqlParameter("@TabId", SqlDbType.NVarChar)
                    {
                        Value = tabId
                    };
                    SqlParameter parentIdParam = new SqlParameter("@ParentId", SqlDbType.Int)
                    {
                        Value = ParentId
                    };
                    SqlParameter rowsParam = new SqlParameter("@Rows", SqlDbType.Structured)
                    {
                        TypeName = "dbo.TabRecordSortOrderType",
                        Value = dt
                    };
                    context.Database.ExecuteSqlCommand(
                        "EXEC dbo.m_UpdateDetailRowIndex @TabId, @ParentId, @Rows",
                        tabIdParam,
                        parentIdParam,
                        rowsParam);

                    response.IsSuccess = true;
                    response.Response = "Row order updated successfully.";
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
                response.IsSuccess = false;
                response.Response = ex.Message;
            }
            return response;
        }
    }
}
