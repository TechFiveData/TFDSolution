using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TFDSolution.Data;
using TFDSolution.Transport;
using TFDSolution.Transport.Master;
using TFDSolution.Transport.Common;
using Microsoft.SqlServer.Server;
using System.Data.Entity.Infrastructure;
using System.Globalization;
using System.ComponentModel.Design;
using System.Runtime.Remoting.Contexts;
using System.IO;
using System.Data.Entity.Core.Metadata.Edm;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace TFDSolution.Business
{
    public static class CodeHelper
    {
        public static string ReplaceQuote(string str)
        {
            return string.IsNullOrEmpty(str) ? str : str.Replace("'", "''");
        }
        public static string ExtractTimeSimple(string input)
        {
            string part = input.Contains(" ")
                ? input.Split(' ')[1]          // get time part
                : input;                       // already time-only

            string[] pieces = part.Split(':');
            if (Convert.ToInt32(pieces[0]) > 23)
            {
                return string.Join(":", "00", pieces[0]);
            }
            else
            {
                // take first 3 parts (HH:mm:ss)
                return string.Join(":", pieces[0], pieces[1]);
            }
        }
        public static string GetTime(string input)
        {
            var match = Regex.Match(input, @"\b(\d{2}:\d{2}:\d{2})");
            return match.Success ? match.Groups[1].Value : null;
        }

        public static bool HasParameter(string spName, string paramName, TFDSolutionEntities context)
        {
            var query = @"SELECT COUNT(*) FROM INFORMATION_SCHEMA.PARAMETERS WHERE SPECIFIC_NAME = @ProcName AND PARAMETER_NAME = @ParamName";
            var procName = spName.Contains(".") ? spName.Split('.').Last() : spName;
            var count = context.Database.SqlQuery<int>(
                query,
                new SqlParameter("@ProcName", procName),
                new SqlParameter("@ParamName", "@" + paramName)
            ).FirstOrDefault();

            return count > 0;
        }
    }
    public class CommonBusiness
    {
        public static DateTime ParseWithLocalCulture(string input)
        {
            // Ensure your input matches the exact format — here: MM-dd-yyyy HH:mm:ss
            const string format = "MM-dd-yyyy HH:mm:ss";

            // Parse as local time
            var dt = DateTime.ParseExact(
                input.Trim(),
                format,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeLocal | DateTimeStyles.AdjustToUniversal
            );

            // dt.Kind will now be DateTimeKind.Local
            // It represents the local time equivalent of the original input.
            return dt;
        }
        public static void FormDeleteData(string formName, string uId, string CreatedBy)
        {
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    List<SqlParameter> parameters = new List<SqlParameter>
                        {
                            new SqlParameter("@FormName", formName),
                            new SqlParameter("@CreatedBy", CreatedBy),
                            new SqlParameter("@UId", uId)
                        };
                    var str = context.Database.SqlQuery<string>("exec proc_DeleteData @FormName,@CreatedBy, @UId", parameters.ToArray()).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
            }
        }
        public static void ItemStock(ItemStockModel itemStock)
        {
            try
            {
                using (var content = new TFDSolutionEntities())
                {
                    List<FormStockSettings> formSettings = content.Database.SqlQuery<FormStockSettings>("proc_GetFormStockSettings @FormId",
                        new SqlParameter("@FormId", itemStock.FormId)).ToList();
                    if (!string.IsNullOrEmpty(itemStock.TabId))
                    {
                        formSettings = content.Database.SqlQuery<FormStockSettings>("proc_GetFormStockSettings @FormId,@TabId",
                        new SqlParameter("@FormId", itemStock.FormId),
                        new SqlParameter("@TabId", itemStock.TabId)).ToList();
                    }
                    if (formSettings != null && formSettings.Count > 0)
                    {
                        foreach (var item in formSettings)
                        {
                            if (!string.IsNullOrEmpty(item.StoredProcedure) && item.StoredProcedure.Length > 0)
                            {
                                content.Database.ExecuteSqlCommand(item.StoredProcedure + " @CompanyId, @Id,@DetailId, @Userid,@Status,@Formid",
                                new SqlParameter("@CompanyId", itemStock.CompanyId),
                                new SqlParameter("@Id", itemStock.Id),
                                new SqlParameter("@DetailId", itemStock.DetailId),
                                new SqlParameter("@Userid", itemStock.Userid),
                                new SqlParameter("@Status", itemStock.Status),
                                new SqlParameter("@Formid", itemStock.FormId));
                                if (itemStock.Id > 0)
                                {
                                    try
                                    {
                                        content.Database.ExecuteSqlCommand("Stock_RateIssue @ParentId",
                                        new SqlParameter("@ParentId", itemStock.Id));
                                    }
                                    catch (Exception)
                                    {

                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
            }
        }
        public static string getFormKeySetting(string formId, string KeyName)
        {
            FormSettings settings = new FormSettings();
            try
            {
                using (var content = new TFDSolutionEntities())
                {
                    settings = content.Database.SqlQuery<FormSettings>("proc_getKeyFormSetting @FormId, @KeyName",
                        new SqlParameter("@FormId", formId),
                        new SqlParameter("@KeyName", KeyName)).FirstOrDefault();
                    if (settings != null && !string.IsNullOrEmpty(settings.KeyValue))
                    {
                        return settings.KeyValue.ToLower();
                    }
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.SaveAppLog(new AppLog() { Logger = "TransactionBusiness.getFormKeySetting", Exception = ex.StackTrace, Message = ex.Message, LogLevel = "Error" });
            }
            return string.Empty;
        }

        public static string getFormKeySettingTab(string formId, string tabId, string KeyName)
        {
            FormSettings settings = new FormSettings();
            try
            {
                using (var content = new TFDSolutionEntities())
                {
                    settings = content.Database.SqlQuery<FormSettings>("proc_getKeyFormSettingTab @FormId, @TabId, @KeyName",
                        new SqlParameter("@FormId", formId),
                        new SqlParameter("@TabId", tabId),
                        new SqlParameter("@KeyName", KeyName)).FirstOrDefault();
                    if (settings != null && !string.IsNullOrEmpty(settings.KeyValue))
                    {
                        return settings.KeyValue.ToLower();
                    }
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.SaveAppLog(new AppLog() { Logger = "TransactionBusiness.getFormKeySetting", Exception = ex.StackTrace, Message = ex.Message, LogLevel = "Error" });
            }
            return string.Empty;
        }

        public static bool getFormStockSetting(string formId, string tablId)
        {
            FormStockSettings formSettings = new FormStockSettings();
            try
            {
                using (var content = new TFDSolutionEntities())
                {
                    formSettings = content.Database.SqlQuery<FormStockSettings>("proc_GetFormStockSettings @FormId,@TabId",
                        new SqlParameter("@FormId", formId),
                        new SqlParameter("@TabId", tablId)).FirstOrDefault();
                    return (formSettings != null && !string.IsNullOrEmpty(formSettings.StoredProcedure));
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.SaveAppLog(new AppLog() { Logger = "CommonBusiness.getFormStockSetting", Exception = ex.StackTrace, Message = ex.Message, LogLevel = "Error" });
            }
            return false;
        }
        public static Guid ConvertGUID(string _val)
        {
            try
            {
                if (!string.IsNullOrEmpty(_val))
                {
                    return Guid.Parse(_val);
                }
            }
            catch (Exception)
            {

            }
            return Guid.Empty;
        }
        public static string getNextCode(string tblName)
        {
            try
            {
                using (var content = new TFDSolutionEntities())
                {
                    //int result = content.Database.SqlQuery("exec [m_getNextCode] @_tableName", tblName);
                    m_FieldCode m_FieldCode = content.Database.SqlQuery<m_FieldCode>("m_getNextCode @_tableName",
                        new SqlParameter("@_tableName", tblName)).FirstOrDefault();
                    if (m_FieldCode != null)
                    {
                        return m_FieldCode.LastCode;
                    }
                }
            }
            catch (Exception)
            {
            }
            return "";
        }
        public static string getTransNextCode(string formId, string companyId)
        {
            try
            {
                using (var content = new TFDSolutionEntities())
                {
                    //int result = content.Database.SqlQuery("exec [m_getNextCode] @_tableName", tblName);

                    m_FieldCode m_FieldCode = content.Database.SqlQuery<m_FieldCode>("m_getTransNextCode @p_FormId,@p_companyId",
                        new SqlParameter("@p_FormId", formId),
                        new SqlParameter("@p_companyId", companyId)).FirstOrDefault();
                    if (m_FieldCode != null)
                    {
                        return m_FieldCode.LastCode;
                    }
                }
            }
            catch (Exception)
            {

            }
            return "";
        }
        public static List<clsSelection> GetItemFieldOtherCharges(string FieldId, string ItemId, int AccountId, string FormId, string CompanyId
            , List<PageFieldData> HeaderFieldData, List<PageFieldData> DetailFieldData)
        {
            try
            {
                var permissionTable = new DataTable();
                permissionTable.Columns.Add("FieldName", typeof(string));
                permissionTable.Columns.Add("FieldValue", typeof(string));
                permissionTable.Columns.Add("DataType", typeof(string));
                if (HeaderFieldData != null && HeaderFieldData.Count > 0)
                {
                    foreach (var item in HeaderFieldData)
                    {
                        permissionTable.Rows.Add(item.FieldName, item.FieldValue, "");
                    }
                }
                var detailtable = new DataTable();
                detailtable.Columns.Add("FieldName", typeof(string));
                detailtable.Columns.Add("FieldValue", typeof(string));
                detailtable.Columns.Add("DataType", typeof(string));
                if (DetailFieldData != null && DetailFieldData.Count > 0)
                {
                    foreach (var item in DetailFieldData)
                    {
                        detailtable.Rows.Add(item.FieldName, item.FieldValue, "");
                    }
                }
                using (var context = new TFDSolutionEntities())
                {
                    var param1 = new SqlParameter("@ItemId", ItemId);
                    var param2 = new SqlParameter("@FieldId", FieldId);
                    var param3 = new SqlParameter("@AccountId", AccountId);
                    var param4 = new SqlParameter("@FormId", FormId);
                    var param5 = new SqlParameter("@FieldsData", SqlDbType.Structured)
                    {
                        TypeName = "dbo.Udt_FieldsData",
                        Value = permissionTable
                    };
                    var param6 = new SqlParameter("@DetailFieldsData", SqlDbType.Structured)
                    {
                        TypeName = "dbo.Udt_FieldsData",
                        Value = detailtable
                    };
                    if (CodeHelper.HasParameter("m_ItemOtherCharges", "DetailFieldsData", context))
                    {
                        var TemplateList = context.Database.SqlQuery<clsSelection>("EXEC m_ItemOtherCharges @ItemId,@FieldId,@AccountId,@FormId,@FieldsData,@DetailFieldsData", param1, param2, param3, param4, param5, param6).ToList();
                        return TemplateList;
                    }
                    if (CodeHelper.HasParameter("m_ItemOtherCharges", "FieldsData", context))
                    {
                        var TemplateList = context.Database.SqlQuery<clsSelection>("EXEC m_ItemOtherCharges @ItemId,@FieldId,@AccountId,@FormId,@FieldsData", param1, param2, param3, param4, param5).ToList();
                        return TemplateList;
                    }   
                    else
                    {
                        var TemplateList = context.Database.SqlQuery<clsSelection>("EXEC m_ItemOtherCharges @ItemId,@FieldId,@AccountId,@FormId", param1, param2, param3, param4).ToList();
                        return TemplateList;
                    }
                    
                }
            }
            catch (Exception)
            {

            }
            return new List<clsSelection>();
        }
        public static List<clsSelection> getFieldSelection(string fieldId)
        {
            List<clsSelection> selection = new List<clsSelection>();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    SqlParameter PfieldId = new SqlParameter("@fieldId", fieldId);
                    // Use SqlQuery<string>() since we expect a list of table names (strings)
                    selection = context.Database.SqlQuery<clsSelection>("EXEC m_getFieldSelection @fieldId", PfieldId).ToList();
                }
            }
            catch (Exception)
            {
            }
            return selection;
        }
        public static List<clsSelection> getFieldFormatSelection(string fieldtype)
        {
            List<clsSelection> selection = new List<clsSelection>();
            try
            {
                if (fieldtype == "TimeField")
                {
                    selection.Add(new clsSelection() { Text = "24 Hour (HH:mm:ss)", Value = "HH:mm:ss" });
                    selection.Add(new clsSelection() { Text = "12 Hour (hh:mm AM/PM)", Value = "hh:mm tt" });
                }
                else
                {
                    selection.Add(new clsSelection() { Text = "DD/MM/YYYY", Value = "dd/MM/yyyy" });
                    selection.Add(new clsSelection() { Text = "MM/DD/YYYY", Value = "MM/dd/yyyy" });
                    selection.Add(new clsSelection() { Text = "YYYY-MM-DD", Value = "yyyy-MM-dd" });
                    selection.Add(new clsSelection() { Text = "DD/MM/YYYY 24H", Value = "dd/MM/yyyy HH:mm:ss" });
                    selection.Add(new clsSelection() { Text = "DD/MM/YYYY 12H", Value = "dd/MM/yyyy hh:mm tt" });
                }
            }
            catch (Exception ex)
            {
                LogEx(ex);
            }
            return selection;
        }
        public static FormDocumentNo getDocNumber(string formId, int FinancialYear, string companyId, int DocNoId = 0, int PendingId = 0, string PendingItemSrNos = "")
        {
            FormDocumentNo response = new FormDocumentNo();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    var param1 = new SqlParameter("@FinancialYearId", FinancialYear);
                    var param2 = new SqlParameter("@FormId", formId);
                    var param3 = new SqlParameter("@DocNoId", DocNoId);
                    var param4 = new SqlParameter("@PendingId", PendingId);
                    var param5 = new SqlParameter("@PendingItemSrNos", PendingItemSrNos);
                    var param6 = new SqlParameter("@CompanyId", companyId);
                    // Use SqlQuery<string>() since we expect a list of column names (strings)
                    response = context.Database.SqlQuery<FormDocumentNo>("EXEC proc_getDocNumber @FinancialYearId, @FormId, @DocNoId, @PendingId, @PendingItemSrNos,@CompanyId",
                        param1, param2, param3, param4, param5, param6).FirstOrDefault();
                }
            }
            catch (Exception)
            {
            }
            return response;
        }
        public static async Task<FormTabData> ExecuteDynamicSP(string spName, string compnayId, int fYearId, string userId)
        {
            FormTabData response = new FormTabData();
            List<Dictionary<string, object>> items = new List<Dictionary<string, object>>();
            List<string> itemCol = new List<string>();
            DataTable dtData = new DataTable("Data");
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    using (var command = context.Database.Connection.CreateCommand())
                    {
                        if (context.Database.Connection.State == ConnectionState.Closed)
                            context.Database.Connection.Open();
                        DataTable dtSchema = new DataTable("Schema");
                        command.Parameters.Add(new SqlParameter { ParameterName = "@CompanyId", Value = compnayId });
                        command.Parameters.Add(new SqlParameter { ParameterName = "@FiancialYearId", Value = fYearId });
                        //command.Transaction = context.Database.Connection.BeginTransaction();
                        // ✅ Check and add FinancialYearId
                        if (CodeHelper.HasParameter(spName, "UserId", context))
                        {
                            command.Parameters.Add(new SqlParameter("@UserId", userId));
                        }
                        command.CommandText = spName;
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
                                        obj[columnName] = columnValue;
                                    }
                                    items.Add(obj);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
            response.Columns = itemCol;
            response.Data = items;
            return response;
        }
        public static void SaveAuditLog(AuditLog log)
        {
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    var CompanyId = new SqlParameter("@CompanyId", (object)log.CompanyId);
                    var UserId = new SqlParameter("@UserId", log.UserId);
                    var FYId = new SqlParameter("@FinancialYearId", log.FinancialYearId.HasValue ? log.FinancialYearId.Value : (object)DBNull.Value);
                    var PageName = new SqlParameter("@PageName", log.PageName);
                    var Action = new SqlParameter("@Action", log.Action);
                    var RecordId = new SqlParameter("@RecordId", log.RecordId);
                    var FieldName = new SqlParameter("@FieldName", log.FieldName != null ? (object)log.FieldName : DBNull.Value);
                    var FieldOldValue = new SqlParameter("@FieldOldValue", log.FieldOldValue != null ? (object)log.FieldOldValue : DBNull.Value);
                    var FieldNewValue = new SqlParameter("@FieldNewValue", log.FieldNewValue != null ? (object)log.FieldNewValue : DBNull.Value);
                    var IPAddress = new SqlParameter("@IPAddress", log.IPAddress != null ? (object)log.IPAddress : DBNull.Value);
                    var Remark = new SqlParameter("@Remark", log.Remark != null ? (object)log.Remark : DBNull.Value);
                    // Capture stored procedure result set
                    var result = context.Database.SqlQuery<ResponseModel>(
                        "EXEC proc_InsertAuditLog @CompanyId, @UserId, @FinancialYearId, @PageName, @Action, @RecordId,@FieldName,@FieldOldValue,@FieldNewValue,@IPAddress,@Remark ",
                        CompanyId, UserId, FYId, PageName, Action, RecordId, FieldName, FieldOldValue, FieldNewValue, IPAddress, Remark).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
            }
        }
        public static void LogEx(Exception ex, string CompanyId, string UserId)
        {
            try
            {
                var st = new StackTrace(ex, true);
                var frame = st.GetFrame(0); // first frame with info
                var method = frame.GetMethod();
                string fileName = frame.GetFileName();       // Source file path (if available)
                int lineNumber = frame.GetFileLineNumber();  // Line number in source file
                string methodPath = $"{method.DeclaringType.FullName}.{method.Name}";
                SaveAppLog(new AppLog()
                {
                    Logger = methodPath,
                    Exception = ex.StackTrace,
                    Message = (ex.InnerException != null ? ex.InnerException.Message : ex.Message),
                    CompanyId = CompanyId,
                    LogLevel = "ERROR",
                    UserId = UserId
                });
            }
            catch (Exception)
            {
            }
        }
        public static void LogEx(Exception ex, LoginUserInfo LoginedUser)
        {
            try
            {
                var st = new StackTrace(ex, true);
                var frame = st.GetFrame(0); // first frame with info
                var method = frame.GetMethod();
                string fileName = frame.GetFileName();       // Source file path (if available)
                int lineNumber = frame.GetFileLineNumber();  // Line number in source file
                string methodPath = $"{method.DeclaringType.FullName}.{method.Name}";
                SaveAppLog(new AppLog()
                {
                    Logger = methodPath,
                    Exception = ex.StackTrace,
                    Message = (ex.InnerException != null ? ex.InnerException.Message : ex.Message),
                    CompanyId = Convert.ToString(LoginedUser.CompanyInfo),
                    LogLevel = "ERROR",
                    UserId = Convert.ToString(LoginedUser.UserId)
                });
            }
            catch (Exception)
            {
            }
        }
        public static void LogEx(Exception ex)
        {
            try
            {
                var st = new StackTrace(ex, true);
                var frame = st.GetFrame(0); // first frame with info
                var method = frame.GetMethod();
                string fileName = frame.GetFileName();       // Source file path (if available)
                int lineNumber = frame.GetFileLineNumber();  // Line number in source file
                string methodPath = $"{method.DeclaringType.FullName}.{method.Name}";
                SaveAppLog(new AppLog()
                {
                    Logger = methodPath,
                    Exception = ex.StackTrace,
                    Message = (ex.InnerException != null ? ex.InnerException.Message : ex.Message),
                    CompanyId = "",
                    LogLevel = "ERROR",
                    UserId = ""
                });
            }
            catch (Exception)
            {
            }
        }
        public static void LogInfo(AppLog log, LoginUserInfo LoginedUser)
        {
            try
            {
                SaveAppLog(new AppLog()
                {
                    Logger = log.Logger,
                    Message = log.Message,
                    CompanyId = Convert.ToString(LoginedUser.CompanyInfo.CompanyId),
                    LogLevel = "INFO",
                    UserId = Convert.ToString(LoginedUser.UserId)
                });
            }
            catch (Exception)
            {
            }
        }
        public static void SaveAppLog(AppLog log)
        {
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    var CompanyId = new SqlParameter("@CompanyId", log.CompanyId ?? (object)DBNull.Value);
                    var UserId = new SqlParameter("@UserId", log.UserId ?? (object)DBNull.Value);
                    var Message = new SqlParameter("@Message", log.Message);
                    var LogLevel = new SqlParameter("@LogLevel", log.LogLevel);
                    var Logger = new SqlParameter("@Logger", log.Logger);
                    var Exception = new SqlParameter("@Exception", log.Exception);
                    // Capture stored procedure result set
                    var result = context.Database.SqlQuery<ResponseModel>(
                        "EXEC proc_InsertAppLog @CompanyId, @UserId, @Message, @LogLevel, @Logger, @Exception ",
                        CompanyId, UserId, Message, LogLevel, Logger, Exception).FirstOrDefault();
                }
            }
            catch
            {
            }
        }

        public static void SaveEmailLog(EmailLog log)
        {
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    var param1 = new SqlParameter("@CompanyId", log.CompanyId);
                    var param2 = new SqlParameter("@UserId", log.UserId);
                    var param3 = new SqlParameter("@Email_To", log.ToEmail);
                    var param4 = new SqlParameter("@Email_Subject", log.Subject);
                    var param5 = new SqlParameter("@IsSent", log.IsSent);
                    var param6 = new SqlParameter("@Email_CC", log.CCEmail != null ? (object)log.CCEmail : DBNull.Value);
                    var param7 = new SqlParameter("@Email_BCC", log.BCCEmail != null ? (object)log.BCCEmail : DBNull.Value);
                    var param8 = new SqlParameter("@AttachedFiiles", log.AttachedFileName != null ? (object)log.AttachedFileName : DBNull.Value);
                    var param9 = new SqlParameter("@FormId", log.FormId != null ? (object)log.FormId : DBNull.Value);
                    var param10 = new SqlParameter("@ParentId", log.RecordId);
                    var param11 = new SqlParameter("@ExceptionMessage", log.ExceptionMessage != null ? (object)log.ExceptionMessage : DBNull.Value);
                    context.Database.ExecuteSqlCommand(
                        @"EXEC m_SaveEmailLog 
                            @CompanyId = @CompanyId,
                            @UserId = @UserId,
                            @Email_To = @Email_To,
                            @Email_Subject = @Email_Subject,
                            @IsSent = @IsSent,
                            @Email_CC = @Email_CC,
                            @Email_BCC = @Email_BCC,
                            @AttachedFiiles = @AttachedFiiles,
                            @FormId = @FormId,
                            @ParentId = @ParentId,
                            @ExceptionMessage = @ExceptionMessage",
                        param1, param2, param3, param4, param5, param6, param7, param8, param9, param10, param11
                    );
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.SaveAppLog(new AppLog() { Logger = "TransactionBusiness.SaveEmailLog", Exception = ex.StackTrace, Message = ex.Message, LogLevel = "Error" });
            }
        }
        public static DataTable GetSortingTableForModel(List<FormSortOrderUpdateModel> updatedRows)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("FormId", typeof(Guid));
            dt.Columns.Add("SortOrder", typeof(int));
            if (updatedRows != null && updatedRows.Count > 0)
            {
                foreach (var row in updatedRows)
                {
                    DataRow dr = dt.NewRow();
                    dr["FormId"] = row.FormId;
                    dr["SortOrder"] = row.SortOrder;
                    dt.Rows.Add(dr);
                }
            }
            return dt;

        }
        public static UserCompany UserCompany(Guid UserId)
        {
            UserCompany response = new UserCompany();
            try
            {
                using (var content = new TFDSolutionEntities())
                {
                    m_UserSettings userSettings = content.m_UserSettings.Where(x => x.UserId == UserId).FirstOrDefault();
                    if (userSettings != null)
                    {
                        m_CompanyFinancials financials = content.m_CompanyFinancials.Where(x => x.FiancialYearId == userSettings.DefaultFiancialYearId).FirstOrDefault();
                        m_CompanyMast companyMast = content.m_CompanyMast.Where(x => x.CompanyId == userSettings.CompanyId).FirstOrDefault();
                        response = new UserCompany();
                        response.DefaultFiancialId = userSettings.DefaultFiancialYearId;
                        response.CompanyId = userSettings.CompanyId;
                        if (companyMast != null)
                        {
                            response.CompanyName = companyMast.CompanyName;
                            response.CompanyCode = companyMast.CompanyCode;
                        }
                        if (financials != null)
                        {
                            response.FinancialAlias = financials.Alias;
                            response.FinancialStartDate = financials.StartDate;
                            response.FinancialEndDate = financials.EndDate;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
            }
            return response;
        }
        public static ResponseModel SaveUserSettings(string CompanyId, int FinancialId, Guid userId)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    var userIdParam = new SqlParameter("@UserId", Convert.ToString(userId));
                    var companyIdParam = new SqlParameter("@CompanyId", CompanyId);
                    var defaultYearID = new SqlParameter("@DefaultYearID", FinancialId);
                    response = context.Database.SqlQuery<ResponseModel>("EXEC proc_SetUserCompany @UserId, @CompanyId,@DefaultYearID",
                        userIdParam, companyIdParam, defaultYearID).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                response.Response = "Exception Occured: " + ex.Message;
            }
            return response;
        }

        public static FieldSelectionResponse getCommonSelection(string selectionType, string CompanyId, string whareClause = "", string orderBy = "", string entryType = "")
        {
            FieldSelectionResponse response = new FieldSelectionResponse();
            var results = new List<dynamic>();
            try
            {
                List<clsSelection> selection = new List<clsSelection>();
                using (var context = new TFDSolutionEntities())
                {
                    var param1 = new SqlParameter("@SelectionType", selectionType);
                    var param2 = new SqlParameter("@WhereClause", (object)whareClause ?? DBNull.Value);
                    var param3 = new SqlParameter("@OrderBy", (object)orderBy ?? DBNull.Value);
                    var param4 = new SqlParameter("@EntryType", (object)entryType ?? DBNull.Value);
                    var param5 = new SqlParameter("@CompanyId", (object)CompanyId ?? DBNull.Value);

                    selection = context.Database
                        .SqlQuery<clsSelection>(
                            "EXEC m_getFormSelection @SelectionType,@WhereClause,@OrderBy,@EntryType,@CompanyId",
                            param1, param2, param3, param4, param5
                        ).ToList();

                    //selection = context.m_getSelection(selectionType, whareClause, orderBy, entryType).Select(x => new clsSelection()
                    //{
                    //    Text = x.OptionText,
                    //    Value = x.OptionValue.ToString()
                    //}).ToList();
                    foreach (var item in selection)
                    {
                        results.Add(new
                        {
                            id = item.Value,
                            text = item.Text
                        });
                    }
                }
                response.HasMore = false;
            }
            catch (Exception)
            {
            }
            response.Data = results;
            return response;
        }
        public static FieldSelectionResponse getFieldSelectionLarge(string FieldId, string searchTerm, int page, string fieldValue, string companyId)
        {
            FieldSelectionResponse response = new FieldSelectionResponse();
            try
            {
                int pageSize = 50;
                var results = new List<dynamic>();
                int totalCount = 0;
                using (var context = new TFDSolutionEntities())
                {
                    var connection = context.Database.Connection;
                    if (connection.State == ConnectionState.Closed)
                        connection.Open();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "dbo.m_getFieldSelectionPage";
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter { ParameterName = "@fieldId", Value = (object)FieldId ?? DBNull.Value });
                        command.Parameters.Add(new SqlParameter { ParameterName = "@SearchTerm", Value = (object)searchTerm ?? DBNull.Value });
                        command.Parameters.Add(new SqlParameter { ParameterName = "@PageNumber", Value = page });
                        command.Parameters.Add(new SqlParameter { ParameterName = "@PageSize", Value = pageSize });
                        command.Parameters.Add(new SqlParameter { ParameterName = "@FieldValue", Value = fieldValue });
                        command.Parameters.Add(new SqlParameter { ParameterName = "@CompanyId", Value = companyId });
                        using (var reader = command.ExecuteReader())
                        {
                            // First result set - actual data
                            while (reader.Read())
                            {
                                results.Add(new
                                {
                                    id = reader["Value"],
                                    text = reader["Text"].ToString()
                                });
                            }
                            // Second result set - total count
                            if (reader.NextResult() && reader.Read())
                            {
                                totalCount = Convert.ToInt32(reader["TotalCount"]);
                            }
                        }
                    }
                }
                bool more = (page * pageSize) < totalCount;
                response.HasMore = more;
                response.Data = results;
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
            }
            return response;
        }
        public static FieldSelectionResponse getFieldSelectionLargeSP(SelectionRequest model)
        {
            FieldSelectionResponse response = new FieldSelectionResponse();
            try
            {
                int totalCount = 0;
                var results = new List<dynamic>();
                FieldDependencySQL formFields = new FieldDependencySQL();
                using (var context = new TFDSolutionEntities())
                {
                    var param1 = new SqlParameter("@FieldId", model.FieldId);
                    formFields = context.Database.SqlQuery<FieldDependencySQL>("EXEC m_getFieldSelectionSP @FieldId", param1).FirstOrDefault();
                    if (formFields != null && !string.IsNullOrEmpty(formFields.FieldName))
                    {
                        string sqlQuery = formFields.SQLQuery;
                        if (sqlQuery.Contains("SearchTerm ="))
                            sqlQuery = sqlQuery.Replace("@SearchTerm = {CompanyId}", "@SearchTerm = '" + model.SearchTerm + "'");
                        if (sqlQuery.Contains("{CompanyId}"))
                            sqlQuery = sqlQuery.Replace("{CompanyId}", "'" + model.CompanyId + "'");
                        //if (sqlQuery.Contains("@Head_") == false && sqlQuery.Contains("{ParentId}"))
                        //    sqlQuery = sqlQuery.Replace("{ParentId}", "'" + Convert.ToString(model.ParentId) + "'");
                        if (sqlQuery.Contains("{CreatedBy}"))
                            sqlQuery = sqlQuery.Replace("{CreatedBy}", "'" + model.UserId + "'");
                        if (sqlQuery.Contains("{FiancialYearId}"))
                        {
                            sqlQuery = sqlQuery.Replace("{FiancialYearId}", "'" + model.FiancialYearId + "'");
                        }
                        if (model != null && model.FieldData != null && model.FieldData.Count > 0)
                        {
                            foreach (var field in model.FieldData)
                            {
                                if (sqlQuery.Contains("{" + field.FieldName + "}"))
                                {
                                    sqlQuery = sqlQuery.Replace("{" + field.FieldName + "}", "'" + Convert.ToString(field.FieldValue) + "'");
                                }
                            }
                        }
                        if (model != null && model.HeaderFieldData != null && model.HeaderFieldData.Count > 0)
                        {
                            foreach (var field in model.HeaderFieldData)
                            {
                                if (sqlQuery.Contains("@Head_" + field.FieldName + " ="))
                                    sqlQuery = sqlQuery.Replace("{ParentId}", "'" + Convert.ToString(field.FieldValue) + "'");
                            }
                        }
                        string strSQL = Convert.ToString(sqlQuery).Replace(" ", "");
                        if (strSQL.Contains("={") == false)
                        {
                            DataSet dst = DbHelper.GetDataSet(context, sqlQuery, CommandType.Text, null);
                            if (dst != null && dst.Tables.Count > 0 && dst.Tables[0].Rows.Count > 0)
                            {
                                totalCount = dst.Tables[0].Rows.Count;
                                foreach (DataRow Ditem in dst.Tables[0].Rows)
                                {
                                    string _value = Convert.ToString(Ditem[formFields.DDLValueField]);
                                    string _text = Convert.ToString(Ditem[formFields.DDLTextField]);
                                    results.Add(new { id = _value, text = _text });
                                }
                            }
                        }
                    }
                }
                bool more = false;
                response.HasMore = more;
                response.Data = results;
            }
            catch (Exception)
            {
            }
            return response;
        }

        public static FieldSelectionResponse getCommonSelection(int _Id, string searchTerm, string companyId, int page)
        {
            FieldSelectionResponse response = new FieldSelectionResponse();
            try
            {
                int pageSize = 50;
                var results = new List<dynamic>();
                int totalCount = 0;
                using (var context = new TFDSolutionEntities())
                {
                    var connection = context.Database.Connection;
                    if (connection.State == ConnectionState.Closed)
                        connection.Open();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "dbo.m_getReportSelection";
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter { ParameterName = "@Id", Value = (object)_Id ?? DBNull.Value });
                        command.Parameters.Add(new SqlParameter { ParameterName = "@SearchTerm", Value = (object)searchTerm ?? DBNull.Value });
                        command.Parameters.Add(new SqlParameter { ParameterName = "@CompanyId", Value = companyId });
                        command.Parameters.Add(new SqlParameter { ParameterName = "@PageNumber", Value = page });
                        command.Parameters.Add(new SqlParameter { ParameterName = "@PageSize", Value = pageSize });
                        using (var reader = command.ExecuteReader())
                        {
                            // First result set - actual data
                            while (reader.Read())
                            {
                                results.Add(new
                                {
                                    id = reader["Value"],
                                    text = reader["Text"].ToString()
                                });
                            }
                            // Second result set - total count
                            if (reader.NextResult() && reader.Read())
                            {
                                totalCount = Convert.ToInt32(reader["TotalCount"]);
                            }
                        }
                    }
                }
                bool more = (page * pageSize) < totalCount;
                response.HasMore = more;
                response.Data = results;
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
            }
            return response;
        }

        #region Dataset to Dictionary
        //convert all tables inside a DataSet:
        public static List<List<Dictionary<string, object>>> DataSetToList(DataSet ds)
        {
            var result = new List<List<Dictionary<string, object>>>();

            foreach (DataTable dt in ds.Tables)
            {
                result.Add(DataTableToList(dt));  // reuse the method
            }

            return result;
        }
        //Method: Convert DataTable to List<Dictionary<string, object>>
        public static List<Dictionary<string, object>> DataTableToList(DataTable dt)
        {
            var list = new List<Dictionary<string, object>>();

            foreach (DataRow row in dt.Rows)
            {
                var dict = new Dictionary<string, object>();

                foreach (DataColumn col in dt.Columns)
                {
                    dict[col.ColumnName] = row[col];  // add column name and value
                }

                list.Add(dict);
            }

            return list;
        }
        #endregion

        #region Export in CSV
        public static void ExportDataTableToCsv(DataTable dt, string filePath)
        {
            var sb = new StringBuilder();

            // Add column headers
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                sb.Append(dt.Columns[i].ColumnName);
                if (i < dt.Columns.Count - 1)
                    sb.Append(",");
            }
            sb.AppendLine();

            // Add rows
            foreach (DataRow row in dt.Rows)
            {
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    // Escape quotes
                    string value = row[i].ToString().Replace("\"", "\"\"");
                    // Wrap value in quotes if it contains comma
                    if (value.Contains(",")) value = $"\"{value}\"";

                    sb.Append(value);

                    if (i < dt.Columns.Count - 1)
                        sb.Append(",");
                }
                sb.AppendLine();
            }

            // Write to file
            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
        }
        public static void ExportDataSetToCsv(DataSet ds, string folderPath)
        {
            for (int i = 0; i < ds.Tables.Count; i++)
            {
                string filePath = Path.Combine(folderPath, $"Table_{i + 1}.csv");
                ExportDataTableToCsv(ds.Tables[i], filePath);
            }
        }
        #endregion

        public static ResponseModel SavePageComments(FormCommentModel request)
        {
            ResponseModel response = new ResponseModel();
            int parentId = GetParentID(request);
            if (parentId != 0)
            {
                request.ParentId = parentId;

            }
            else
            {
                request.CommentText = "*" + request.CommentText + "*";
            }
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    string sql = @"INSERT INTO m_FormComments 
                           (FormId, ParentId, DetailId, CommentText, CreatedBy, CreatedOn) 
                           VALUES 
                           (@FormId, @ParentId, @DetailId, @CommentText, @CreatedBy, GETDATE())";

                    context.Database.ExecuteSqlCommand(
                        sql,
                        new SqlParameter("@FormId", request.FormId),
                        new SqlParameter("@ParentId", request.ParentId),
                        new SqlParameter("@DetailId", request.DetailId),
                        new SqlParameter("@CommentText", request.CommentText),
                        new SqlParameter("@CreatedBy", request.UserId)
                    );

                    response.IsSuccess = true;
                    response.Response = "Comment has been saved.";
                }
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Response = "Exception Occurred: " + ex.Message;
            }

            return response;
        }

        public static List<FormCommentModel> GetFormComments(string UserId, string FormId, int ParentId)
        {
            List<FormCommentModel> response = new List<FormCommentModel>();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    var userIdParam = new SqlParameter("@UserId", UserId);
                    var formIdParam = new SqlParameter("@FormId", FormId);
                    var parentIdParam = new SqlParameter("@ParentId", ParentId);
                    response = context.Database
                        .SqlQuery<FormCommentModel>(
                        "EXEC usp_GetFormComments @UserId, @FormId, @ParentId",
                            userIdParam,
                            formIdParam,
                            parentIdParam
                        ).ToList();
                }
            }
            catch (Exception ex)
            {
                LogEx(ex);
            }
            return response;
        }

        public static string GetLanguageCode(CommonEnum.Language lgn)
        {
            // Map common enum names / aliases to language codes
            var aliasToCode = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "English", "en" }, { "En", "en" }, { "EN", "en" },
                { "French", "fr" },  { "Fr", "fr" },  { "FR", "fr" },
                { "Hindi", "hi" },   { "HI", "hi" },
                { "Gujarati", "gu" },{ "GU", "gu" },
                { "German", "de" },  { "DE", "de" },
                { "Spanish", "es" }, { "ES", "es" }
            };

            // Try to get the declared enum name; fall back to ToString()
            string name = Enum.GetName(typeof(CommonEnum.Language), lgn) ?? lgn.ToString();

            // Normalize name by removing non-letters
            string key = System.Text.RegularExpressions.Regex.Replace(name ?? string.Empty, @"[^A-Za-z]", "");

            // Direct alias lookup
            if (!string.IsNullOrWhiteSpace(key) && aliasToCode.TryGetValue(key, out var code))
                return code;

            // Try first two characters as ISO-ish fallback
            if (!string.IsNullOrWhiteSpace(key) && key.Length >= 2)
            {
                var two = key.Substring(0, 2).ToLowerInvariant();
                var allowed = new HashSet<string> { "en", "fr", "hi", "gu", "de", "es" };
                if (allowed.Contains(two))
                    return two;
            }

            // Default fallback
            return "en";
        }

        public static int GetParentID(FormCommentModel request)
        {
            int response = 0;
            try
            {
                if (request.ParentId == 0)
                {
                    using (var context = new TFDSolutionEntities())
                    {
                        var param1 = new SqlParameter("@FormId", (object)request.FormId ?? DBNull.Value);
                        var param2 = new SqlParameter("@CommentText", (object)request.CommentText ?? DBNull.Value);
                        response = context.Database
                            .SqlQuery<int>("EXEC m_getGetParentID @FormId, @CommentText", param1, param2)
                            .FirstOrDefault();   // ✅ single value ke liye
                    }
                }

            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
            }

            return response;
        }

        public static List<ReportMast> getFormSystemReportSelection(string formName)
        {
            List<ReportMast> docList = new List<ReportMast>();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    var formParam1 = new SqlParameter("@FormName", formName);
                    docList = context.Database.SqlQuery<ReportMast>("EXEC proc_GetFormSystemReportSelection @FormName", formParam1).ToList();
                    return docList;
                }
            }
            catch (Exception ex)
            { CommonBusiness.LogEx(ex); }
            return docList;
        }

        public static List<ContactDetail> GetRFQEmailContacts(int parentId)
        {
            List<ContactDetail> contactList = new List<ContactDetail>();

            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    var parentIdParam = new SqlParameter("@ParentId", parentId);

                    string query = @"
                SELECT ParentId,
                       Email,
                       ContactPerson,
                       Designation
                FROM m_MastAccountMaster_Contact
                WHERE RFQEmail = 1
                AND ParentId = @ParentId";

                    contactList = context.Database
                                         .SqlQuery<ContactDetail>(query, parentIdParam)
                                         .ToList();
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
            }

            return contactList;
        }

    }
}

