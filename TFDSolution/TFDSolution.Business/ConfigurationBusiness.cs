using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;
using TFDSolution.Business.Interface;
using TFDSolution.Data;
using TFDSolution.Transport;
using TFDSolution.Transport.Common;
using TFDSolution.Transport.Master;

namespace TFDSolution.Business
{
    public class ConfigurationBusiness : IConfigurationBusiness
    {
        #region Other Charges
        public ResponseModel UpdateOtherChargesStatus(OtherChargesModel request)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    var formParam1 = new SqlParameter("@ChargesId", request.ChargesId);
                    var formParam3 = new SqlParameter("@Status", request.Status);
                    var formParam6 = new SqlParameter("@UserId", request.UserId);
                    response = context.Database.SqlQuery<ResponseModel>("EXEC proc_UpdateOtherChargesStatus " +
                        "@ChargesId, @Status, @UserId", formParam1, formParam3, formParam6).FirstOrDefault();
                    if (response != null && response.IsSuccess.HasValue && response.IsSuccess.Value)
                    {
                        response.Response = "Status has been updated successfully";
                    }
                    else
                    {
                        response.IsSuccess = false;
                        response.Response = "Unexpected error occurred while updating status.";
                    }
                    CommonBusiness.SaveAppLog(new AppLog()
                    {
                        CompanyId = request.CompanyId,
                        Logger = "ConfigurationBusiness.UpdateOtherChargesStatus",
                        LogLevel = "Info",
                        Message = response.Response + " for Other Charges: " + request.@ChargesId + ", Status: " + request.Status + "",
                        UserId = Convert.ToString(request.UserId),
                    });
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
                response.IsSuccess = false;
                response.Response = "Exception occurred: " + Convert.ToString(ex.Message);
            }
            return response;
        }
        public ResponseModel ReplicationOtherCharge(int chargesId, string UserId)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    var oId = new SqlParameter("@ChargesId", chargesId);
                    var UId = new SqlParameter("@UserId", UserId);
                    var result = context.Database.SqlQuery<ResponseModel>("EXEC proc_OtherChargesReplication @ChargesId,@UserId", oId, UId).FirstOrDefault();
                    if (result != null)
                    {
                        response.Action = result.Action;
                        response.IsSuccess = result.IsSuccess;
                        response.Response = result.Response;
                    }
                    else
                    {
                        response.Action = "Error";
                        response.IsSuccess = false;
                        response.Response = "Replication process not completed. Please try again.";
                    }
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
                response.Action = "Error";
                response.IsSuccess = false;
                response.Response = "Replication process not completed. Please try again.";
            }
            return response;
        }
        public List<OtherChargesModel> GetSqlTemplateDetail(string companyId)
        {
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    var param1 = new SqlParameter("@_CompanyId", companyId);
                    var TemplateList = context.Database.SqlQuery<OtherChargesModel>("EXEC m_getOtherCharges @_CompanyId", param1).ToList();
                    return TemplateList;
                }
            }
            catch (Exception)
            {

            }
            return new List<OtherChargesModel>();
        }
        public ResponseModel DeleteOtherCharge(int chargesId)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    var oId = new SqlParameter("@ChargesId", chargesId);
                    var result = context.Database.SqlQuery<ResponseModel>("EXEC proc_DeleteOtherCharges @ChargesId", @oId).FirstOrDefault();
                    if (result != null)
                    {
                        response.Action = result.Action;
                        response.IsSuccess = result.IsSuccess;
                        response.Response = result.Response;
                    }
                    else
                    {
                        response.Action = "Error";
                        response.IsSuccess = false;
                        response.Response = "Unexpected error occurred.";
                    }
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
                response.Action = "Error";
                response.IsSuccess = false;
                response.Response = "Unexpected error occurred. Error: " + ex.Message;
            }
            return response;
        }
        //DataTable Matching udt_OtherChargesDetail
        public DataTable GetOtherChargesDetailDataTable(List<OtherChargesDetail> details)
        {
            var table = new DataTable();
            table.Columns.Add("OtherChargesDetailId", typeof(int));
            table.Columns.Add("ChargesId", typeof(int));
            table.Columns.Add("SrNo", typeof(int));
            table.Columns.Add("Alias", typeof(string));
            table.Columns.Add("TaxChargeName", typeof(string));
            table.Columns.Add("LedgerId", typeof(int));
            table.Columns.Add("PerAmount", typeof(string));
            table.Columns.Add("BasedOn", typeof(string));
            table.Columns.Add("ChargesValue", typeof(double));
            table.Columns.Add("Formula", typeof(string));
            table.Columns.Add("DecimalPoint", typeof(int));
            table.Columns.Add("Effect", typeof(string));
            table.Columns.Add("ChangeValue", typeof(bool));
            table.Columns.Add("AddInCost", typeof(bool));
            table.Columns.Add("UserId", typeof(Guid)); // This is both CreatedBy/UpdatedBy
            table.Columns.Add("IsDisplayOnItemsPopup", typeof(bool));

            foreach (var item in details)
            {
                table.Rows.Add(
                    item.OtherChargesDetailId,
                    item.ChargesId,
                    item.SrNo,
                    item.Alias,
                    item.TaxChargeName,
                    item.LedgerId,
                    item.PerAmount,
                    item.BasedOn,
                    item.ChargesValue,
                    item.Formula,
                    item.DecimalPoint,
                    item.Effect,
                    item.ChangeValue,
                    item.AddInCost,
                    item.UserId,
                    item.IsDisplayOnItemsPopup
                );
            }

            return table;
        }
        public ResponseModel SaveOtherCharges(OtherChargesModel model)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    var result = context.Database.SqlQuery<ResponseModel>(
                        "EXEC dbo.proce_SaveOtherCharges @ChargesId, @FormId, @Description, @ShortName, @LedgerId, @TaxSlabId, @ChargesTypeId, @CompanyId, @UserId, @MultipleFormId",
                        new SqlParameter("@ChargesId", model.ChargesId),
                        new SqlParameter("@FormId", model.FormId),
                        new SqlParameter("@Description", model.Description),
                        new SqlParameter("@ShortName", (object)model.ShortName ?? DBNull.Value),
                        new SqlParameter("@LedgerId", model.LedgerId),
                        new SqlParameter("@TaxSlabId", model.TaxSlabId),
                        new SqlParameter("@ChargesTypeId", model.ChargesTypeId),
                        new SqlParameter("@CompanyId", (object)model.CompanyId ?? DBNull.Value),
                        new SqlParameter("@UserId", model.UserId),
                        new SqlParameter("@MultipleFormId", model.MultipleFormId)
                        ).FirstOrDefault();
                    if (result != null && result.Id > 0)
                    {
                        if (model != null && model.Details != null && model.Details.Count > 0)
                        {
                            foreach (var item in model.Details)
                            {
                                item.UserId = model.UserId;
                                item.ChargesId = result.Id;
                            }
                            SaveOtherChargesDetails(model.Details);
                        }
                    }
                    response.IsSuccess = true;
                    response.Response = "Record has been save successfully";
                }
            }
            catch (Exception)
            {
                response.Response = "Exception occured, Please try again.";
            }
            return response;
        }
        public OtherChargesModel GetOtherChargeDetails(int chargesId)
        {
            OtherChargesModel model = new OtherChargesModel();
            try
            {
                using (var context = new TFDSolutionEntities()) // Your EDMX context
                {
                    var cmd = context.Database.Connection.CreateCommand();
                    cmd.CommandText = "proc_GetOtherChargeDetail";
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@ChargesId", chargesId));
                    context.Database.Connection.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        // Result set 1: Orders
                        model = ((System.Data.Entity.Infrastructure.IObjectContextAdapter)context)
                                        .ObjectContext
                                        .Translate<OtherChargesModel>(reader).FirstOrDefault();
                        if (model != null)
                        {
                            // Move to result set 2
                            reader.NextResult();
                            List<OtherChargesDetail> details = ((System.Data.Entity.Infrastructure.IObjectContextAdapter)context)
                                                   .ObjectContext
                                                   .Translate<OtherChargesDetail>(reader).ToList();
                            model.Details = details;
                        }
                        // Now you can use all three lists
                    }
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
            }
            return model;
        }
        public void SaveOtherChargesDetails(List<OtherChargesDetail> details)
        {
            var dt = GetOtherChargesDetailDataTable(details);
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    var connection = context.Database.Connection;
                    if (connection.State == ConnectionState.Closed)
                        connection.Open();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "dbo.proc_SaveOtherChargesDetail";
                        command.CommandType = CommandType.StoredProcedure;

                        var param = new SqlParameter();
                        param.ParameterName = "@Details";
                        param.SqlDbType = SqlDbType.Structured;
                        param.SourceColumn = "dbo.udt_OtherChargesDetail";
                        param.Value = dt;
                        command.Parameters.Add(param);

                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.SaveAppLog(new AppLog() { Logger = "ConfigurationBusiness.SaveOtherChargesDetails", Exception = ex.StackTrace, Message = ex.Message, LogLevel = "Error" });
            }
        }

        public OtherChargesModel GetItemOtherCharges(int chargesId, int ParentId, int DetailId, string FormId)
        {
            OtherChargesModel model = new OtherChargesModel();
            try
            {
                using (var context = new TFDSolutionEntities()) // Your EDMX context
                {
                    var cmd = context.Database.Connection.CreateCommand();
                    cmd.CommandText = "proc_GetItemOtherCharges";
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@ChargesId", chargesId));
                    cmd.Parameters.Add(new SqlParameter("@ParentId", ParentId));
                    cmd.Parameters.Add(new SqlParameter("@DetailId", DetailId));
                    if (CodeHelper.HasParameter("proc_GetItemOtherCharges", "FormId", context))
                        cmd.Parameters.Add(new SqlParameter("@FormId", FormId));
                    context.Database.Connection.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        // Result set 1: Orders
                        model = ((System.Data.Entity.Infrastructure.IObjectContextAdapter)context)
                                        .ObjectContext
                                        .Translate<OtherChargesModel>(reader).FirstOrDefault();
                        if (model != null)
                        {
                            // Move to result set 2
                            reader.NextResult();
                            List<OtherChargesDetail> details = ((System.Data.Entity.Infrastructure.IObjectContextAdapter)context)
                                                   .ObjectContext
                                                   .Translate<OtherChargesDetail>(reader).ToList();
                            model.Details = details;
                        }
                        // Now you can use all three lists
                    }
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.SaveAppLog(new AppLog() { Logger = "TransactionBusiness.GetItemOtherCharges", Exception = ex.StackTrace, Message = ex.Message, LogLevel = "Error" });
            }
            return model;
        }
        #endregion
        public async Task<FormData> GetDynamicMultipleApproval(MultipleApprovalRequest request)
        {
            FormData response = new FormData();
            List<Dictionary<string, object>> items = new List<Dictionary<string, object>>();
            List<string> itemCol = new List<string>();
            List<PageGridData> pageGrids = new List<PageGridData>();
            try
            {
                string storedProcedureName = "m_Multiple_Approval";
                using (var context = new TFDSolutionEntities())
                {
                    //dynamic re = GetTaskExecution(context, pageName);
                    using (var command = context.Database.Connection.CreateCommand())
                    {
                        if (context.Database.Connection.State == ConnectionState.Closed)
                            context.Database.Connection.Open();
                        DataTable dtSchema = new DataTable("Schema");
                        command.Parameters.Add(new SqlParameter { ParameterName = "@CompanyId", Value = request.CompanyId });
                        command.Parameters.Add(new SqlParameter { ParameterName = "@FormId", Value = request.FormId });
                        command.Parameters.Add(new SqlParameter { ParameterName = "@UserId", Value = request.UserId });
                        command.Parameters.Add(new SqlParameter { ParameterName = "@EntryType", Value = request.EntryType });
                        command.CommandText = storedProcedureName;
                        command.CommandType = CommandType.StoredProcedure;
                        using (var reader = command.ExecuteReader())
                        {
                            dtSchema = reader.GetSchemaTable();
                            if (dtSchema != null)
                            {
                                foreach (DataRow schemarow in dtSchema.Rows)
                                {
                                    itemCol.Add(schemarow.ItemArray[0].ToString());
                                }
                            }
                            if (reader.HasRows)
                            {
                                while (await reader.ReadAsync())
                                {
                                    Dictionary<string, object> obj = new Dictionary<string, object>();
                                    for (int i = 0; i < reader.FieldCount; i++)
                                    {
                                        string columnName = reader.GetName(i);
                                        object columnValue = reader.GetValue(i);
                                        if (columnValue.GetType() == typeof(DateTime))
                                        {
                                            DateTime dt = Convert.ToDateTime(columnValue);
                                            bool hasOnlyTime = dt.TimeOfDay != TimeSpan.Zero;
                                            if (hasOnlyTime)
                                            {
                                                TimeSpan timeOnly = dt.TimeOfDay;
                                                columnValue = dt.ToString("HH:mm");
                                            }
                                            else
                                            {
                                                columnValue = (dt == new DateTime(1900, 1, 1)) ? "" : dt.ToString("dd/MM/yyyy");
                                            }
                                        }
                                        obj[columnName] = columnValue;
                                    }
                                    items.Add(obj);
                                }
                            }
                            //if (reader.NextResult())
                            //{
                            //    while (reader.Read())
                            //    {
                            //        string columnName = reader["FieldName"] != DBNull.Value ? (string)reader["FieldName"] : string.Empty;
                            //        string columnTitle = reader["FieldTitle"] != DBNull.Value ? (string)(reader["FieldTitle"]) : string.Empty;
                            //        pageGrids.Add(new PageGridData()
                            //        {
                            //            ColumnName = columnName,
                            //            ColumnTitle = columnTitle,
                            //        });
                            //    }
                            //}
                            //if (reader.NextResult() && reader.Read())
                            //{
                            //    response.TotalRecords = Convert.ToInt32(reader["TotalCount"]);
                            //}
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                response.Response = "Exception occured: " + Convert.ToString(ex.Message);
                response.IsSuccess = false;
            }
            response.Data = items;
            response.Columns = itemCol;
            response.PageData = pageGrids;
            return response;
        }
        public ResponseModel UpdateRecordStatus(MultipleApprovalRequest request)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    var formParam1 = new SqlParameter("@RecorId", request.RecorId);
                    var formParam2 = new SqlParameter("@EntryType", request.EntryType);
                    var formParam3 = new SqlParameter("@Status", request.Status);
                    var formParam4 = new SqlParameter("@FromId", request.FormId);
                    var formParam5 = new SqlParameter("@CancelReason", (object)request.CancelReason ?? DBNull.Value);
                    var formParam6 = new SqlParameter("@UserId", request.UserId);
                    response = context.Database.SqlQuery<ResponseModel>("EXEC proc_UpdateDataStatus @RecorId, @EntryType, @Status, @FromId, @CancelReason, @UserId",
                        formParam1, formParam2, formParam3, formParam4, formParam5, formParam6).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Response = "Exception occurred: " + Convert.ToString(ex.Message);
            }
            return response;
        }

        #region Email Templates
        public List<EmailTemplateModel> getEmailTamplates()
        {
            List<EmailTemplateModel> model = new List<EmailTemplateModel>();
            try
            {
                //m_getEmailTemplates
                using (var context = new TFDSolutionEntities())
                {
                    model = context.Database.SqlQuery<EmailTemplateModel>("EXEC m_getEmailTemplates").ToList();
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
            }
            return model;
        }
        public ResponseModel SaveEmailTamplate(EmailTemplateModel model)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    response = context.Database.SqlQuery<ResponseModel>(
                        "EXEC dbo.proc_m_SaveEmailTemplate @TemplateId, @TemplateName, @Subject, @Body, @CC, @BCC, @FromEmail, @ReplyTo,@IsHtml, @IsActive, @UserId, @FormIds, @Signature",
                        new SqlParameter("@TemplateId", model.TemplateId),
                        new SqlParameter("@TemplateName", model.TemplateName),
                        new SqlParameter("@Subject", model.Subject),
                        new SqlParameter("@Body", (object)model.Body ?? DBNull.Value),
                        new SqlParameter("@CC", (object)model.CC ?? DBNull.Value),
                        new SqlParameter("@BCC", (object)model.BCC ?? DBNull.Value),
                        new SqlParameter("@FromEmail", (object)model.FromEmail ?? DBNull.Value),
                        new SqlParameter("@ReplyTo", (object)model.ReplyTo ?? DBNull.Value),
                        new SqlParameter("@IsHtml", (object)model.IsHtml ?? DBNull.Value),
                        new SqlParameter("@IsActive", (object)model.IsActive ?? DBNull.Value),
                        new SqlParameter("@UserId", model.UserId),
                        new SqlParameter("@FormIds", (object)model.FormIds ?? DBNull.Value),
                        new SqlParameter("@Signature", (object)model.Signature ?? DBNull.Value)
                        ).FirstOrDefault();
                    if (response != null && (response.IsSuccess.HasValue ? response.IsSuccess.Value : false))
                        response.Response = "Record has been save successfully";
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
                response.Response = "Exception occured, Please try again.";
            }
            return response;
        }
        public EmailTemplateModel getEmailTamplateById(int templateId)
        {
            EmailTemplateModel response = new EmailTemplateModel();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    var param1 = new SqlParameter("@TemplateId", templateId);
                    response = context.Database.SqlQuery<EmailTemplateModel>("EXEC m_getEmailTemplateById @TemplateId", param1).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
            }
            return response;
        }

        public EmailTemplateModel getEmailTamplateByForm(string formId, int AccountId, int ParentId)
        {
            EmailTemplateModel response = new EmailTemplateModel();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    var param1 = new SqlParameter("@FormId", formId);
                    var param2 = new SqlParameter("@AccountId", AccountId);
                    var param3 = new SqlParameter("@ParentId", ParentId);
                    response = context.Database.SqlQuery<EmailTemplateModel>("EXEC m_getFormEmailSettings @FormId, @AccountId, @ParentId", param1, param2, param3).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.SaveAppLog(new AppLog() { Logger = "ConfigurationBusiness.getEmailTamplateByForm", Exception = ex.StackTrace, Message = ex.Message, LogLevel = "Error" });
            }
            return response;
        }
        #endregion

        #region Email Templates
        public List<DashboardSettingModel> getDashboardSettings()
        {
            List<DashboardSettingModel> model = new List<DashboardSettingModel>();
            try
            {
                //m_getEmailTemplates
                using (var context = new TFDSolutionEntities())
                {
                    model = context.Database.SqlQuery<DashboardSettingModel>("EXEC m_getDashboards").ToList();
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.SaveAppLog(new AppLog() { Logger = "ConfigurationBusiness.getDashboardSettings", Exception = ex.StackTrace, Message = ex.Message, LogLevel = "Error" });
            }
            return model;
        }
        public List<DashboardDetailSettingModel> GetDashboardDetailSettings(int dashboardId)
        {
            List<DashboardDetailSettingModel> model = new List<DashboardDetailSettingModel>();

            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    var param = new SqlParameter("@DashboardId", dashboardId);
                    model = context.Database.SqlQuery<DashboardDetailSettingModel>("EXEC m_getDashboardSettings @DashboardId", param).ToList();
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.SaveAppLog(new AppLog() { Logger = "ConfigurationBusiness.GetDashboardDetailSettings", Exception = ex.StackTrace, Message = ex.Message, LogLevel = "Error" });
            }

            return model;
        }
        public ResponseModel SaveDashboardSetting(DashboardSettingModel model)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    response = context.Database.SqlQuery<ResponseModel>(
                        "EXEC dbo.proc_m_SaveDashboard @DashboardId, @DashboardTitle, @DashboardName, @AssignedUsers, @IsActive, @SortOrder",
                        new SqlParameter("@DashboardId", model.DashboardId),
                        new SqlParameter("@DashboardTitle", model.DashboardTitle),
                        new SqlParameter("@DashboardName", model.DashboardName),
                        new SqlParameter("@AssignedUsers", (object)model.AssignedUsers ?? DBNull.Value),
                        new SqlParameter("@IsActive", (object)model.IsActive ?? DBNull.Value),
                        new SqlParameter("@SortOrder", (object)model.SortOrder ?? DBNull.Value)
                        ).FirstOrDefault();
                    if (response != null && (response.IsSuccess.HasValue ? response.IsSuccess.Value : false))
                        response.Response = "Record has been save successfully";
                }
            }
            catch (Exception)
            {
                response.Response = "Exception occurred, Please try again.";
            }
            return response;
        }
        public DashboardDetailSettingModel GetDashboardDetail(int Id)
        {
            DashboardDetailSettingModel response = new DashboardDetailSettingModel();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    response = context.Database.SqlQuery<DashboardDetailSettingModel>("EXEC dbo.proc_getDashboardSetting @Id", new SqlParameter("@Id", Id)).FirstOrDefault();
                }
            }
            catch (Exception)
            {
            }
            return response;
        }
        public ResponseModel DeleteDashboardSetting(int Id)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    response = context.Database.SqlQuery<ResponseModel>("EXEC dbo.proc_m_DeleteDashboard @Id", new SqlParameter("@Id", Id)).FirstOrDefault();
                    if (response != null && (response.IsSuccess.HasValue ? response.IsSuccess.Value : false))
                        response.Response = "Record has been save successfully";
                }
            }
            catch (Exception)
            {
                response.Response = "Exception occurred, Please try again.";
            }
            return response;
        }
        public ResponseModel DeleteDashboardDetailSetting(int Id)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    response = context.Database.SqlQuery<ResponseModel>("EXEC dbo.proc_m_DeleteDashboardSettings @Id", new SqlParameter("@Id", Id)).FirstOrDefault();
                    if (response != null && (response.IsSuccess.HasValue ? response.IsSuccess.Value : false))
                        response.Response = "Record has been save successfully";
                }
            }
            catch (Exception)
            {
                response.Response = "Exception occurred, Please try again.";
            }
            return response;
        }
        public ResponseModel SaveDashboardDetailSetting(DashboardDetailSettingModel model)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    response = context.Database.SqlQuery<ResponseModel>(
                        "EXEC dbo.proc_m_SaveDashboardSettings @Id, @DashboardId, @Title, @WidgetType, @BackgroundColor, @FontColor, @SQLDataScript, @IsActive, @SortOrder, @RedirectURL, @DisplaySize, @ChartType",
                        new SqlParameter("@Id", model.Id),
                        new SqlParameter("@DashboardId", model.DashboardId),
                        new SqlParameter("@Title", model.Title),
                        new SqlParameter("@WidgetType", model.WidgetType),
                        new SqlParameter("@BackgroundColor", model.BackgroundColor),
                        new SqlParameter("@FontColor", (object)model.FontColor ?? DBNull.Value),
                        new SqlParameter("@SQLDataScript", (object)model.SQLDataScript ?? DBNull.Value),
                        new SqlParameter("@IsActive", (object)model.IsActive ?? DBNull.Value),
                        new SqlParameter("@SortOrder", (object)model.SortOrder ?? DBNull.Value),
                        new SqlParameter("@RedirectURL", (object)model.RedirectURL ?? DBNull.Value),
                        new SqlParameter("@DisplaySize", (object)model.DisplaySize ?? DBNull.Value),
                        new SqlParameter("@ChartType", (object)model.ChartType ?? DBNull.Value)
                        ).FirstOrDefault();
                    if (response != null && (response.IsSuccess.HasValue ? response.IsSuccess.Value : false))
                        response.Response = "Record has been save successfully";
                }
            }
            catch (Exception)
            {
                response.Response = "Exception occurred, Please try again.";
            }
            return response;
        }
        public DashboardSettingModel getDashboardSettingById(int dashboardId)
        {
            DashboardSettingModel response = new DashboardSettingModel();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    var param1 = new SqlParameter("@DashboardId", dashboardId);
                    response = context.Database.SqlQuery<DashboardSettingModel>("EXEC m_getDashboardById @DashboardId", param1).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.SaveAppLog(new AppLog() { Logger = "ConfigurationBusiness.getDashboardSettingById", Exception = ex.StackTrace, Message = ex.Message, LogLevel = "Error" });
            }
            return response;
        }

        #endregion

        #region User Form Approvals
        public List<FormModel> getFormUserApproval(FormModel request)
        {
            List<FormModel> model = new List<FormModel>();
            try
            {
                //m_getEmailTemplates
                using (var context = new TFDSolutionEntities())
                {
                    var param1 = new SqlParameter("@ParentFormId", (object)request.ParentFormId ?? DBNull.Value);
                    var param2 = new SqlParameter("@FormId", (object)request.FormId ?? DBNull.Value);
                    var param3 = new SqlParameter("@UserId", (object)request.UserId ?? DBNull.Value);
                    model = context.Database.SqlQuery<FormModel>("EXEC m_getFormUserApproval @ParentFormId, @FormId, @UserId", param1, param2, param3).ToList();
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
            }
            return model;
        }
        public List<FormUserApprovalModel> getFormUserApprovalSettings(string formId)
        {
            List<FormUserApprovalModel> model = new List<FormUserApprovalModel>();
            try
            {
                //m_getEmailTemplates
                using (var context = new TFDSolutionEntities())
                {
                    var param1 = new SqlParameter("@FormId", (object)formId ?? DBNull.Value);
                    model = context.Database.SqlQuery<FormUserApprovalModel>("EXEC m_getFormUserApprovalSettings @FormId", param1).ToList();
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.SaveAppLog(new AppLog() { Logger = "ConfigurationBusiness.getFormUserApprovalSettings", Exception = ex.StackTrace, Message = ex.Message, LogLevel = "Error" });
            }
            return model;
        }
        public ResponseModel SaveUserApprovalSettings(UserApprovalSettingModel model)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                var permissionTable = new DataTable();
                permissionTable.Columns.Add("UserId", typeof(string));
                permissionTable.Columns.Add("Priority", typeof(int));
                if (model.UserPriorities != null && model.UserPriorities.Count > 0)
                {
                    foreach (var item in model.UserPriorities)
                    {
                        permissionTable.Rows.Add(item.UserId, item.Priority);
                    }
                }
                using (var context = new TFDSolutionEntities())
                {
                    response = context.Database.SqlQuery<ResponseModel>(
                        "EXEC dbo.proc_m_UserApprovalSettings @FormId, @CreatedBy, @ApprovalType, @UserPriorityData",
                        new SqlParameter("@FormId", model.FormId),
                        new SqlParameter("@CreatedBy", model.CreatedBy),
                        new SqlParameter("@ApprovalType", model.ApprovalType),
                        new SqlParameter("@UserPriorityData", SqlDbType.Structured)
                        {
                            TypeName = "dbo.Udt_UserPriorityData",
                            Value = permissionTable
                        }
                        ).FirstOrDefault();
                    if (response != null && (response.IsSuccess.HasValue ? response.IsSuccess.Value : false))
                        response.Response = "Record has been save successfully";
                }
            }
            catch (Exception)
            {
                response.Response = "Exception occured, Please try again.";
            }
            return response;
        }
        #endregion
    }
}