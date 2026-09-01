using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using TFDSolution.Business.Interface;
using TFDSolution.Data;
using TFDSolution.Transport;
using TFDSolution.Transport.Common;
using TFDSolution.Transport.Master;

namespace TFDSolution.Business
{
    public class ReportBusiness : IReportBusiness
    {
        public List<Rep_StockSummaryModel> getItemStockSummary(DateTime fromDate, DateTime ToDate)
        {
            List<Rep_StockSummaryModel> reportResults = new List<Rep_StockSummaryModel>();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    SqlParameter param1 = new SqlParameter("@FromDate", fromDate);
                    SqlParameter param2 = new SqlParameter("@ToDate", ToDate);
                    reportResults = context.Database.SqlQuery<Rep_StockSummaryModel>("EXEC report_ItemStock @FromDate,@ToDate",
                        param1, param2).ToList();
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.SaveAppLog(new AppLog() { Logger = "MasterBusiness.getItemStockSummary", Exception = ex.StackTrace, Message = ex.Message, LogLevel = "Error" });
            }
            return reportResults;
        }

        public List<ReportTemplateModel> getReportTamplates(string UserId)
        {
            List<ReportTemplateModel> reportResults = new List<ReportTemplateModel>();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    SqlParameter param1 = new SqlParameter("@UserId", UserId);
                    //SqlParameter param2 = new SqlParameter("@ToDate", ToDate);
                    reportResults = context.Database.SqlQuery<ReportTemplateModel>("EXEC m_getMISReports @UserId", param1).ToList();
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
            }
            return reportResults;
        }

        public ResponseModel SaveReportTamplate(ReportTemplateModel mast)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    var filterTable = new DataTable();
                    filterTable.Columns.Add("SrNo", typeof(int));
                    filterTable.Columns.Add("ReportId", typeof(int));
                    filterTable.Columns.Add("FieldName", typeof(string));
                    filterTable.Columns.Add("FieldCaption", typeof(string));
                    filterTable.Columns.Add("FieldType", typeof(string));
                    filterTable.Columns.Add("FieldDefaultValue", typeof(string));
                    filterTable.Columns.Add("DropdwonTextField", typeof(string));
                    filterTable.Columns.Add("DropdwonValueField", typeof(string));
                    filterTable.Columns.Add("SourceType", typeof(string));
                    if (mast.Header != null && mast.Header.Count > 0)
                    {
                        foreach (var item in mast.Header)
                        {
                            filterTable.Rows.Add(item.SrNo, mast.ReportId, item.FieldName, item.FieldCaption,
                                item.FieldType, item.FieldDefaultValue
                                , item.DropdwonTextField, item.DropdwonValueField, item.SourceType);
                        }
                    }
                    var paramTable = new DataTable();
                    paramTable.Columns.Add("SrNo", typeof(int));
                    paramTable.Columns.Add("ReportId", typeof(int));
                    paramTable.Columns.Add("ParameterName", typeof(string));
                    paramTable.Columns.Add("ParamDataType", typeof(string));
                    paramTable.Columns.Add("ParamValueName", typeof(string));
                    paramTable.Columns.Add("DefaultValue", typeof(string));
                    if (mast.ProcedureMappings != null && mast.ProcedureMappings.Count > 0)
                    {
                        foreach (var item in mast.ProcedureMappings)
                        {
                            paramTable.Rows.Add(item.SrNo, mast.ReportId, item.ParameterName, item.ParamDataType, item.ParamValueName, item.DefaultValue);
                        }
                    }
                    var parameters = new List<SqlParameter>
                    {
                        new SqlParameter("@ReportId", mast.ReportId),
                        new SqlParameter("@ReportTitle", mast.ReportTitle),
                        new SqlParameter("@ReportName", mast.ReportName),
                        new SqlParameter("@IsActive", (object)mast.IsActive ?? DBNull.Value),
                        new SqlParameter("@SQLQueryName", mast.SQLQueryName),
                        new SqlParameter("@AssignedUsers", (object)mast.AssignedUsers ?? DBNull.Value),
                        new SqlParameter("@FilterTable", SqlDbType.Structured)
                        {
                            TypeName = "dbo.UDT_MISReportHeader",
                            Value = filterTable
                        },
                        new SqlParameter("@ParamMappingTable", SqlDbType.Structured) {
                            TypeName = "dbo.UDT_MISReportParamMapping",
                            Value = paramTable
                        }
                    };
                    response = context.Database.SqlQuery<ResponseModel>("EXEC proc_m_MISReport @ReportId,@ReportTitle,@ReportName,@IsActive,@SQLQueryName,@AssignedUsers,@FilterTable,@ParamMappingTable",
                        parameters.ToArray()).FirstOrDefault();
                }
            }
            catch (Exception)
            {
                response.Response = "Record not save successfully.";
                response.IsSuccess = false;
            }
            return response;
        }

        public ReportTemplateModel getReportTamplateById(int ReportId)
        {
            ReportTemplateModel response = new ReportTemplateModel();
            response.ReportId = ReportId;
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    var conn = context.Database.Connection;
                    if (conn.State != ConnectionState.Open)
                        conn.Open();
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "m_getMISReportById";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@ReportId", ReportId));
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                response = new ReportTemplateModel
                                {
                                    ReportId = ReportId,
                                    ReportName = reader["ReportName"] as string,
                                    ReportTitle = reader["ReportTitle"] as string,
                                    AssignedUsers = reader["AssignedUsers"] as string,
                                    SQLQueryName = reader["SQLQueryName"] as string,
                                    IsActive = reader["IsActive"] != DBNull.Value && (bool)reader["IsActive"],
                                    CreatedOn = reader["CreatedOn"] != DBNull.Value ? (DateTime)reader["CreatedOn"] : DateTime.MinValue,
                                    UpdatedOn = reader["UpdatedOn"] as DateTime?
                                };
                            }
                            // Second result set: Header
                            if (reader.NextResult() && response != null)
                            {
                                response.Header = new List<ReportTemplateHeader>();
                                while (reader.Read())
                                {
                                    response.Header.Add(new ReportTemplateHeader
                                    {
                                        SrNo = reader["SrNo"] != DBNull.Value ? Convert.ToInt32(reader["SrNo"]) : default(int),
                                        Id = reader["Id"] != DBNull.Value ? Convert.ToInt32(reader["Id"]) : default(int),
                                        FieldCaption = reader["FieldCaption"] as string,
                                        FieldName = reader["FieldName"] as string,
                                        FieldType = reader["FieldType"] as string,
                                        FieldDefaultValue = reader["FieldDefaultValue"] as string,
                                        DropdwonTextField = reader["DropdwonTextField"] as string,
                                        DropdwonValueField = reader["DropdwonValueField"] as string,
                                        SourceType = reader["SourceType"] as string
                                    });
                                }
                            }
                            // Third result set: UserCompanies
                            if (reader.NextResult() && response != null)
                            {
                                response.ProcedureMappings = new List<ReportTemplateProcedureMapping>();
                                while (reader.Read())
                                {
                                    response.ProcedureMappings.Add(new ReportTemplateProcedureMapping
                                    {
                                        SrNo = reader["SrNo"] != DBNull.Value ? Convert.ToInt32(reader["SrNo"]) : default(int),
                                        ParameterName = reader["ParameterName"] as string,
                                        ParamDataType = reader["ParamDataType"] as string,
                                        ParamValueName = reader["ParamValueName"] as string,
                                        DefaultValue = reader["DefaultValue"] as string
                                    });
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.SaveAppLog(new AppLog() { Logger = "MasterBusiness.getReportTamplateById", Exception = ex.StackTrace, Message = ex.Message, LogLevel = "Error" });
            }
            return response;
        }

        public ReportData executeMISReportColumns(ReportFilterRequest model)
        {
            ReportData response = new ReportData();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    List<FieldDependencySQL> formFields = new List<FieldDependencySQL>();
                    var param1 = new SqlParameter("@ReportId", model.ReportId);
                    formFields = context.Database.SqlQuery<FieldDependencySQL>("EXEC m_executeMISReport @ReportId", param1).ToList();
                    if (formFields != null && formFields.Count > 0)
                    {
                        foreach (var item in formFields)
                        {
                            string sqlQuery = item.SQLQuery;
                            //bool paginationExist = false;
                            if (sqlQuery.Contains("{CompanyId}"))
                                sqlQuery = sqlQuery.Replace("{CompanyId}", "'" + Convert.ToString(model.CompanyId) + "'");
                            if (sqlQuery.Contains("{CreatedBy}"))
                                sqlQuery = sqlQuery.Replace("{CreatedBy}", "'" + Convert.ToString(model.UserId) + "'");
                            if (sqlQuery.Contains("@PageNumber={0}"))
                            {
                                sqlQuery = sqlQuery.Replace("@PageNumber={0}", "@PageNumber='" + model.PageNumber + "'");
                                //paginationExist = true;
                            }
                            if (sqlQuery.Contains("@PageSize={0}"))
                                sqlQuery = sqlQuery.Replace("@PageSize={0}", "@PageSize='" + model.PageSize + "'");
                            if (sqlQuery.Contains("@SearchTerms={0}"))
                                sqlQuery = sqlQuery.Replace("@SearchTerms={0}", "@SearchTerms='" + model.SearchTerm + "'");
                            if (model != null && model.FieldData != null && model.FieldData.Count > 0)
                            {
                                foreach (var field in model.FieldData)
                                {
                                    if (sqlQuery.Contains("{" + field.FieldName + "}"))
                                        sqlQuery = sqlQuery.Replace("{" + field.FieldName + "}", "'" + Convert.ToString(field.FieldValue) + "'");
                                }
                            }

                            if (sqlQuery.Contains("{") == false)
                            {
                                DataSet dst = DbHelper.GetDataSet(context, sqlQuery, CommandType.Text, null);
                                if (dst != null && dst.Tables.Count > 0)
                                {
                                    DataTable dt = dst.Tables[0];
                                    List<ColumnDef> columns = dt.Columns
                                        .Cast<DataColumn>()
                                        .Select(col => new ColumnDef
                                        {
                                            data = col.ColumnName,
                                            title = col.ColumnName
                                        })
                                        .ToList();
                                    response.columns = columns;
                                    columns = new List<ColumnDef>();
                                    if (dst.Tables.Count > 1)
                                    {
                                        dt = dst.Tables[1];
                                        response.secondColumns = new List<string>();
                                        if (dt != null && dt.Rows.Count > 0)
                                        {
                                            foreach (DataRow row in dt.Rows)
                                            {
                                                foreach (object value in row.ItemArray)
                                                {
                                                    if (value != null && value != DBNull.Value)
                                                    {
                                                        string column = value.ToString().Trim();

                                                        if (!string.IsNullOrEmpty(column))
                                                        {
                                                            response.secondColumns.Add(column);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            response.IsSuccess = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Response = ex.Message;
            }
            return response;
        }
        public ReportData executeMISReport(ReportFilterRequest model)
        {
            ReportData response = new ReportData();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    List<FieldDependencySQL> formFields = new List<FieldDependencySQL>();
                    var param1 = new SqlParameter("@ReportId", model.ReportId);
                    formFields = context.Database.SqlQuery<FieldDependencySQL>("EXEC m_executeMISReport @ReportId", param1).ToList();
                    if (formFields != null && formFields.Count > 0)
                    {
                        foreach (var item in formFields)
                        {
                            bool paginationExist = false;
                            string sqlQuery = item.SQLQuery;
                            if (sqlQuery.Contains("{CompanyId}"))
                                sqlQuery = sqlQuery.Replace("{CompanyId}", "'" + Convert.ToString(model.CompanyId) + "'");
                            if (sqlQuery.Contains("{CreatedBy}"))
                                sqlQuery = sqlQuery.Replace("{CreatedBy}", "'" + Convert.ToString(model.UserId) + "'");
                            if (sqlQuery.Contains("@PageNumber={0}"))
                            {
                                sqlQuery = sqlQuery.Replace("@PageNumber={0}", "@PageNumber='" + model.PageNumber + "'");
                                paginationExist = true;
                            }
                            if (sqlQuery.Contains("@PageSize={0}"))
                                sqlQuery = sqlQuery.Replace("@PageSize={0}", "@PageSize='" + model.PageSize + "'");
                            if (sqlQuery.Contains("@SearchTerms={0}"))
                                sqlQuery = sqlQuery.Replace("@SearchTerms={0}", "@SearchTerms='" + model.SearchTerm + "'");
                            if (model != null && model.FieldData != null && model.FieldData.Count > 0)
                            {
                                foreach (var field in model.FieldData)
                                {
                                    if (sqlQuery.Contains("{" + field.FieldName + "}"))
                                        sqlQuery = sqlQuery.Replace("{" + field.FieldName + "}", "'" + Convert.ToString(field.FieldValue) + "'");
                                }
                            }
                            if (sqlQuery.Contains("{") == false)
                            {
                                DataSet dst = DbHelper.GetDataSet(context, sqlQuery, CommandType.Text, null);
                                if (dst != null && dst.Tables.Count > 0)
                                {
                                    if (dst.Tables[0].Rows.Count > 0)
                                    {
                                        response.Data = CommonBusiness.DataTableToList(dst.Tables[0]);
                                    }
                                    DataTable dt = dst.Tables[0];
                                    List<ColumnDef> columns = dt.Columns
                                        .Cast<DataColumn>()
                                        .Select(col => new ColumnDef
                                        {
                                            data = col.ColumnName,
                                            title = col.ColumnName
                                        })
                                        .ToList();

                                    response.columns = columns;

                                    if (paginationExist && dst.Tables.Count > 1 && dst.Tables[1].Rows.Count > 0)
                                    {
                                        response.TotalRecords = Convert.ToInt32(dst.Tables[1].Rows[0]["TotalRecords"]);
                                    }
                                    else if (paginationExist == false && dst.Tables.Count > 0 && dst.Tables[0].Rows.Count > 0)
                                    {
                                        response.TotalRecords = dst.Tables[0].Rows.Count;
                                    }
                                    else
                                    {
                                        response.TotalRecords = 0; // fallback if no data
                                    }
                                }
                            }
                            response.IsSuccess = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Response = ex.Message;
            }
            return response;
        }

        public async Task<ReportData> ExecuteMISReportAsync(ReportFilterRequest model, string connectionstring)
        {
            ReportData response = new ReportData();

            try
            {

                using (var context = new TFDSolutionEntities())
                {
                    if (context.Database.Connection.State == ConnectionState.Closed)
                    {
                        context.Database.Connection.Open();
                    }
                    var param1 = new SqlParameter("@ReportId", model.ReportId);
                    var formFields = await context.Database.SqlQuery<FieldDependencySQL>("EXEC m_executeMISReport @ReportId", param1).ToListAsync();

                    foreach (var item in formFields)
                    {
                        bool paginationExist = false;
                        string sqlQuery = item.SQLQuery;
                        if (sqlQuery.Contains("{CompanyId}"))
                            sqlQuery = sqlQuery.Replace("{CompanyId}", "'" + Convert.ToString(model.CompanyId) + "'");
                        if (sqlQuery.Contains("{CreatedBy}"))
                            sqlQuery = sqlQuery.Replace("{CreatedBy}", "'" + Convert.ToString(model.UserId) + "'");
                        if (sqlQuery.Contains("@PageNumber={0}"))
                        {
                            sqlQuery = sqlQuery.Replace("@PageNumber={0}", "@PageNumber='1'");
                            paginationExist = true;
                        }
                        if (sqlQuery.Contains("@PageSize={0}"))
                            sqlQuery = sqlQuery.Replace("@PageSize={0}", "@PageSize='10000000'");
                        if (sqlQuery.Contains("@SearchTerms={0}"))
                            sqlQuery = sqlQuery.Replace("@SearchTerms={0}", "@SearchTerms='" + model.SearchTerm + "'");
                        if (model != null && model.FieldData != null && model.FieldData.Count > 0)
                        {
                            foreach (var field in model.FieldData)
                            {
                                if (sqlQuery.Contains("{" + field.FieldName + "}"))
                                    sqlQuery = sqlQuery.Replace("{" + field.FieldName + "}", "'" + Convert.ToString(field.FieldValue) + "'");
                            }
                        }
                        if (sqlQuery.Contains("{") == false)
                        {
                            DataSet dst = DbHelper.GetDataSet(context, sqlQuery, CommandType.Text, null);
                            if (dst != null && dst.Tables.Count > 0)
                            {
                                if (dst.Tables[0].Rows.Count > 0)
                                {
                                    response.Data = CommonBusiness.DataTableToList(dst.Tables[0]);
                                }
                                DataTable dt = dst.Tables[0];
                                List<ColumnDef> columns = dt.Columns
                                    .Cast<DataColumn>()
                                    .Select(col => new ColumnDef
                                    {
                                        data = col.ColumnName,
                                        title = col.ColumnName
                                    })
                                    .ToList();

                                response.columns = columns;

                                if (paginationExist && dst.Tables.Count > 1 && dst.Tables[1].Rows.Count > 0)
                                {
                                    response.TotalRecords = Convert.ToInt32(dst.Tables[1].Rows[0]["TotalRecords"]);
                                }
                                else if (paginationExist == false && dst.Tables.Count > 0 && dst.Tables[0].Rows.Count > 0)
                                {
                                    response.TotalRecords = dst.Tables[0].Rows.Count;
                                }
                                else
                                {
                                    response.TotalRecords = 0; // fallback if no data
                                }
                            }
                        }
                        response.IsSuccess = true;
                    }
                }
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Response = ex.Message;
            }

            return response;
        }

        public DataSet executeMISReportExport(ReportFilterRequest model)
        {
            DataSet dst = new DataSet();
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    List<FieldDependencySQL> formFields = new List<FieldDependencySQL>();
                    var param1 = new SqlParameter("@ReportId", model.ReportId);
                    formFields = context.Database.SqlQuery<FieldDependencySQL>("EXEC m_executeMISReport @ReportId", param1).ToList();
                    if (formFields != null && formFields.Count > 0)
                    {
                        foreach (var item in formFields)
                        {
                            string sqlQuery = item.SQLQuery;
                            if (sqlQuery.Contains("{CompanyId}"))
                                sqlQuery = sqlQuery.Replace("{CompanyId}", "'" + Convert.ToString(model.CompanyId) + "'");
                            if (sqlQuery.Contains("{CreatedBy}"))
                                sqlQuery = sqlQuery.Replace("{CreatedBy}", "'" + Convert.ToString(model.UserId) + "'");
                            if (sqlQuery.Contains("@PageNumber={0}"))
                            {
                                sqlQuery = sqlQuery.Replace("@PageNumber={0}", "@PageNumber='1'");
                            }
                            if (sqlQuery.Contains("@PageSize={0}"))
                                sqlQuery = sqlQuery.Replace("@PageSize={0}", "@PageSize='10000000'");
                            if (sqlQuery.Contains("@SearchTerms={0}"))
                                sqlQuery = sqlQuery.Replace("@SearchTerms={0}", "@SearchTerms='" + model.SearchTerm + "'");
                            if (model != null && model.FieldData != null && model.FieldData.Count > 0)
                            {
                                foreach (var field in model.FieldData)
                                {
                                    if (sqlQuery.Contains("{" + field.FieldName + "}"))
                                        sqlQuery = sqlQuery.Replace("{" + field.FieldName + "}", "'" + Convert.ToString(field.FieldValue) + "'");
                                }
                            }
                            if (sqlQuery.Contains("{") == false)
                            {
                                dst = DbHelper.GetDataSet(context, sqlQuery, CommandType.Text, null);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.SaveAppLog(new AppLog() { Logger = "ReportBusiness.executeMISReportExport", Exception = ex.StackTrace, Message = ex.Message, LogLevel = "Error" });
            }
            return dst;
        }

        #region Account Ledger
        public async Task<LedgerReportResult> RunLedgerReport(LedgerReportRequest model)
        {
            var response = new LedgerReportResult
            {
                Data = new List<LedgerReportData>(),
                TotalRecords = 0,
            };

            try
            {
                using (var context = new TFDSolutionEntities())
                using (var conn = context.Database.Connection)
                {
                    if (conn.State != ConnectionState.Open)
                        conn.Open();

                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "usp_GetAccountLedgerData";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = 120; // important for reports

                        cmd.Parameters.Add(new SqlParameter("@FromDate", SqlDbType.DateTime) { Value = model.FromDate });
                        cmd.Parameters.Add(new SqlParameter("@ToDate", SqlDbType.DateTime) { Value = model.ToDate });
                        cmd.Parameters.Add(new SqlParameter("@Account_Id", SqlDbType.Int) { Value = model.AccountId });
                        cmd.Parameters.Add(new SqlParameter("@CompanyId", SqlDbType.NVarChar) { Value = model.CompanyId });
                        cmd.Parameters.Add(new SqlParameter("@FinancialYearId", SqlDbType.Int) { Value = model.FinacialYearId });
                        cmd.Parameters.Add(new SqlParameter("@PageNumber", SqlDbType.Int) { Value = model.PageNumber });
                        cmd.Parameters.Add(new SqlParameter("@PageSize", SqlDbType.Int) { Value = model.PageSize });

                        using (var reader = cmd.ExecuteReader())
                        {
                            // ===== Result Set 1 : Ledger Data =====
                            while (reader.Read())
                            {
                                response.Data.Add(new LedgerReportData
                                {
                                    CompanyId = reader["CompanyId"] as string,
                                    FormId = reader["FormId"] as string,
                                    CreatedOn = reader.IsDBNull("CreatedOn") ? (DateTime?)null : reader.GetDateTime("CreatedOn"),
                                    CreatedBy = reader["CreatedBy"] as string,
                                    ParentId = reader.GetInt32Safe("ParentId"),
                                    DetailId = reader.GetInt32Safe("DetailId"),
                                    Account_Id = reader.GetInt32Safe("Account_Id"),
                                    RowNo = reader.GetInt32Safe("RowNo"),
                                    Id = reader.GetInt32Safe("Id"),
                                    AccountName = reader["AccountName"] as string,
                                    URNNo = reader["URNNo"] as string,
                                    DocNo = reader["DocNo"] as string,
                                    DocDate = reader.IsDBNull("DocDate") ? (DateTime?)null : reader.GetDateTime("DocDate"),
                                    Debit_Amount = reader.GetDecimalSafe("Debit_Amount"),
                                    Credit_Amount = reader.GetDecimalSafe("Credit_Amount"),
                                    Balance = reader.GetDecimalSafe("Balance"),
                                    CurrencyId = reader.GetInt32Safe("CurrencyId"),
                                    ExchangeRate = reader.GetDecimalSafe("ExchangeRate"),
                                    Remarks = reader["Remarks"] as string,
                                    FormTitle = reader["FormTitle"] as string,
                                    FormName = reader["FormName"] as string,
                                    Currency = reader["Currency"] as string,
                                    UserName = reader["UserName"] as string,
                                    FinancialYearId = reader.GetInt32Safe("FinancialYearId")
                                });
                            }

                            // ===== Result Set 2 : Total Records =====
                            if (reader.NextResult() && reader.Read())
                            {
                                response.TotalRecords = reader.GetInt32Safe("TotalRecords");
                            }
                        }
                    }
                }

                response.IsSuccess = true;
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
                response.IsSuccess = false;
                response.Response = ex.Message;
            }
            if ((response.IsSuccess.HasValue && response.IsSuccess.Value) && response.Data.Any())
            {
                await SaveLedgerReportHistoryAsync(model, response);
            }
            return response;
        }

        public async Task SaveLedgerReportHistoryAsync(LedgerReportRequest model, LedgerReportResult response)
        {
            var dt = ConvertLedgerDataToDataTable(model, response);

            using (var context = new TFDSolutionEntities())
            using (var conn = (SqlConnection)context.Database.Connection)
            {
                if (conn.State != ConnectionState.Open)
                    await conn.OpenAsync();

                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // Delete old records for first page
                        if (model.PageNumber == 0 || model.PageNumber == 1)
                        {
                            using (var deleteCmd = conn.CreateCommand())
                            {
                                deleteCmd.Transaction = transaction;

                                deleteCmd.CommandText = @"DELETE FROM t_AccountLedgerData_Hist WHERE RequestUserId = @RequestUserId";

                                deleteCmd.Parameters.Add(
                                    new SqlParameter("@RequestUserId", SqlDbType.NVarChar)
                                    {
                                        Value = model.UserId,
                                    });

                                await deleteCmd.ExecuteNonQueryAsync();
                            }
                        }

                        // Bulk Insert
                        using (var bulkCopy = new SqlBulkCopy(
                            conn,
                            SqlBulkCopyOptions.Default,
                            transaction))
                        {
                            bulkCopy.DestinationTableName = "t_AccountLedgerData_Hist";

                            bulkCopy.BatchSize = 5000;
                            bulkCopy.BulkCopyTimeout = 120;

                            // Column Mapping
                            foreach (DataColumn col in dt.Columns)
                            {
                                bulkCopy.ColumnMappings.Add(
                                    col.ColumnName,
                                    col.ColumnName);
                            }

                            await bulkCopy.WriteToServerAsync(dt);
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }
        private DataTable ConvertLedgerDataToDataTable(LedgerReportRequest model, LedgerReportResult response)
        {
            var dt = new DataTable();

            // Request Parameters
            dt.Columns.Add("RequestFromDate", typeof(DateTime));
            dt.Columns.Add("RequestToDate", typeof(DateTime));
            dt.Columns.Add("RequestAccountId", typeof(int));
            dt.Columns.Add("RequestCompanyId", typeof(string));
            dt.Columns.Add("RequestFinancialYearId", typeof(int));
            dt.Columns.Add("RequestPageNumber", typeof(int));
            dt.Columns.Add("RequestPageSize", typeof(int));
            dt.Columns.Add("RequestTotalRecords", typeof(int));
            dt.Columns.Add("RequestUserId", typeof(string));

            // Ledger Result Columns
            dt.Columns.Add("CompanyId", typeof(string));
            dt.Columns.Add("FormId", typeof(string));
            dt.Columns.Add("CreatedOn", typeof(DateTime));
            dt.Columns.Add("CreatedBy", typeof(string));
            dt.Columns.Add("ParentId", typeof(int));
            dt.Columns.Add("DetailId", typeof(int));
            dt.Columns.Add("Account_Id", typeof(int));
            dt.Columns.Add("RowNo", typeof(int));
            dt.Columns.Add("Id", typeof(int));
            dt.Columns.Add("AccountName", typeof(string));
            dt.Columns.Add("URNNo", typeof(string));
            dt.Columns.Add("DocNo", typeof(string));
            dt.Columns.Add("DocDate", typeof(DateTime));
            dt.Columns.Add("Debit_Amount", typeof(decimal));
            dt.Columns.Add("Credit_Amount", typeof(decimal));
            dt.Columns.Add("Balance", typeof(decimal));
            dt.Columns.Add("CurrencyId", typeof(int));
            dt.Columns.Add("ExchangeRate", typeof(decimal));
            dt.Columns.Add("Remarks", typeof(string));
            dt.Columns.Add("FormTitle", typeof(string));
            dt.Columns.Add("FormName", typeof(string));
            dt.Columns.Add("Currency", typeof(string));
            dt.Columns.Add("UserName", typeof(string));
            dt.Columns.Add("FinancialYearId", typeof(int));

            // Fill DataTable
            foreach (var item in response.Data)
            {
                dt.Rows.Add(
                    model.FromDate,
                    model.ToDate,
                    model.AccountId,
                    model.CompanyId,
                    model.FinacialYearId,
                    model.PageNumber,
                    model.PageSize,
                    response.TotalRecords,
                    model.UserId,
                    item.CompanyId,
                    item.FormId,
                    item.CreatedOn ?? (object)DBNull.Value,
                    item.CreatedBy,
                    item.ParentId,
                    item.DetailId,
                    item.Account_Id,
                    item.RowNo,
                    item.Id,
                    item.AccountName,
                    item.URNNo,
                    item.DocNo,
                    item.DocDate ?? (object)DBNull.Value,
                    item.Debit_Amount,
                    item.Credit_Amount,
                    item.Balance,
                    item.CurrencyId,
                    item.ExchangeRate,
                    item.Remarks,
                    item.FormTitle,
                    item.FormName,
                    item.Currency,
                    item.UserName,
                    item.FinancialYearId
                );
            }

            return dt;
        }

        public async Task SaveProfitLossReportHistoryAsync(LedgerReportRequest model, ProfitLossReportResult response)
        {
            var dt = ConvertProfitLossDataToDataTable(model, response);
            using (var context = new TFDSolutionEntities())
            using (var conn = (SqlConnection)context.Database.Connection)
            {
                if (conn.State != ConnectionState.Open)
                    await conn.OpenAsync();

                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // Delete old records for first page
                        if (model.PageNumber == 0 || model.PageNumber == 1)
                        {
                            using (var deleteCmd = conn.CreateCommand())
                            {
                                deleteCmd.Transaction = transaction;
                                deleteCmd.CommandText = @"DELETE FROM t_ProfitAndLossData_Hist WHERE RequestUserId = @RequestUserId";
                                deleteCmd.Parameters.Add(
                                    new SqlParameter("@RequestUserId", SqlDbType.NVarChar)
                                    {
                                        Value = model.UserId,
                                    });
                                await deleteCmd.ExecuteNonQueryAsync();
                            }
                        }
                        // Bulk Insert
                        using (var bulkCopy = new SqlBulkCopy(conn, SqlBulkCopyOptions.Default, transaction))
                        {
                            bulkCopy.DestinationTableName = "t_ProfitAndLossData_Hist";
                            bulkCopy.BatchSize = 5000;
                            bulkCopy.BulkCopyTimeout = 120;
                            // Column Mapping
                            foreach (DataColumn col in dt.Columns)
                            {
                                bulkCopy.ColumnMappings.Add(col.ColumnName, col.ColumnName);
                            }
                            await bulkCopy.WriteToServerAsync(dt);
                        }
                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                    }
                }
            }
        }
        private DataTable ConvertProfitLossDataToDataTable(LedgerReportRequest model, ProfitLossReportResult response)
        {
            var dt = new DataTable();
            // Request Parameters
            dt.Columns.Add("RequestFromDate", typeof(DateTime));
            dt.Columns.Add("RequestToDate", typeof(DateTime));
            dt.Columns.Add("RequestAccountId", typeof(int));
            dt.Columns.Add("RequestCompanyId", typeof(string));
            dt.Columns.Add("RequestFinancialYearId", typeof(int));
            dt.Columns.Add("RequestPageNumber", typeof(int));
            dt.Columns.Add("RequestPageSize", typeof(int));
            dt.Columns.Add("RequestTotalRecords", typeof(int));
            dt.Columns.Add("RequestUserId", typeof(string));
            dt.Columns.Add("CompanyId", typeof(string));
            dt.Columns.Add("FormId", typeof(string));
            dt.Columns.Add("CreatedOn", typeof(DateTime));
            dt.Columns.Add("CreatedBy", typeof(string));
            dt.Columns.Add("ParentId", typeof(int));
            dt.Columns.Add("DetailId", typeof(int));
            dt.Columns.Add("Account_Id", typeof(int));
            dt.Columns.Add("RowNo", typeof(int));
            dt.Columns.Add("Id", typeof(int));
            dt.Columns.Add("AccountName", typeof(string));
            dt.Columns.Add("Level", typeof(string));
            dt.Columns.Add("Particular", typeof(string));
            dt.Columns.Add("Debit", typeof(decimal));
            dt.Columns.Add("Credit", typeof(decimal));
            dt.Columns.Add("Amount", typeof(decimal));
            dt.Columns.Add("TotalIncome", typeof(decimal));
            dt.Columns.Add("TotalExpense", typeof(decimal));
            dt.Columns.Add("NetProfit", typeof(decimal));
            dt.Columns.Add("SortPath", typeof(string));
            dt.Columns.Add("RowType", typeof(string));
            dt.Columns.Add("SortOrder", typeof(string));
            dt.Columns.Add("FormTitle", typeof(string));
            dt.Columns.Add("FormName", typeof(string));
            dt.Columns.Add("Currency", typeof(string));
            dt.Columns.Add("UserName", typeof(string));
            dt.Columns.Add("FinancialYearId", typeof(int));
            // Fill DataTable
            foreach (var item in response.Data)
            {
                dt.Rows.Add(
                    model.FromDate,
                    model.ToDate,
                    model.AccountId,
                    model.CompanyId,
                    model.FinacialYearId,
                    model.PageNumber,
                    model.PageSize,
                    response.TotalRecords,
                    model.UserId,
                    item.CompanyId,
                    item.FormId,
                    item.CreatedOn ?? (object)DBNull.Value,
                    item.CreatedBy,
                    item.ParentId,
                    item.DetailId,
                    item.Account_Id,
                    0,
                    0,
                    item.AccountName,
                    item.Level,
                    item.Particular,
                    item.Debit,
                    item.Credit,
                    item.Amount,
                    response.TotalIncome,
                    response.TotalExpense,
                    response.NetProfit,
                    item.SortPath,
                    item.RowType,
                    item.SortOrder,
                    item.FormTitle,
                    item.FormName,
                    item.Currency,
                    item.UserName,
                    item.FinancialYearId
                );
            }
            return dt;
        }
        public async Task SaveBalanceSheetReportHistoryAsync(LedgerReportRequest model, BalanceSheetReportResult response)
        {
            var dt = ConvertBalanceSheetDataToDataTable(model, response);

            using (var context = new TFDSolutionEntities())
            using (var conn = (SqlConnection)context.Database.Connection)
            {
                if (conn.State != ConnectionState.Open)
                    await conn.OpenAsync();

                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // Delete old records for first page
                        if (model.PageNumber == 0 || model.PageNumber == 1)
                        {
                            using (var deleteCmd = conn.CreateCommand())
                            {
                                deleteCmd.Transaction = transaction;

                                deleteCmd.CommandText = @"DELETE FROM t_BalanceSheet_Hist WHERE RequestUserId = @RequestUserId";

                                deleteCmd.Parameters.Add(
                                    new SqlParameter("@RequestUserId", SqlDbType.NVarChar)
                                    {
                                        Value = model.UserId,
                                    });

                                await deleteCmd.ExecuteNonQueryAsync();
                            }
                        }

                        // Bulk Insert
                        using (var bulkCopy = new SqlBulkCopy(
                            conn,
                            SqlBulkCopyOptions.Default,
                            transaction))
                        {
                            bulkCopy.DestinationTableName = "t_BalanceSheet_Hist";

                            bulkCopy.BatchSize = 5000;
                            bulkCopy.BulkCopyTimeout = 120;

                            // Column Mapping
                            foreach (DataColumn col in dt.Columns)
                            {
                                bulkCopy.ColumnMappings.Add(
                                    col.ColumnName,
                                    col.ColumnName);
                            }

                            await bulkCopy.WriteToServerAsync(dt);
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }
        private DataTable ConvertBalanceSheetDataToDataTable(LedgerReportRequest model, BalanceSheetReportResult response)
        {
            var dt = new DataTable();

            // Request Parameters
            dt.Columns.Add("RequestFromDate", typeof(DateTime));
            dt.Columns.Add("RequestToDate", typeof(DateTime));
            dt.Columns.Add("RequestAccountId", typeof(int));
            dt.Columns.Add("RequestCompanyId", typeof(string));
            dt.Columns.Add("RequestFinancialYearId", typeof(int));
            dt.Columns.Add("RequestPageNumber", typeof(int));
            dt.Columns.Add("RequestPageSize", typeof(int));
            dt.Columns.Add("RequestTotalRecords", typeof(int));
            dt.Columns.Add("RequestUserId", typeof(string));
            dt.Columns.Add("CompanyId", typeof(string));
            dt.Columns.Add("FormId", typeof(string));
            dt.Columns.Add("CreatedOn", typeof(DateTime));
            dt.Columns.Add("CreatedBy", typeof(string));
            dt.Columns.Add("ParentId", typeof(int));
            dt.Columns.Add("DetailId", typeof(int));
            dt.Columns.Add("Account_Id", typeof(int));
            dt.Columns.Add("RowNo", typeof(int));
            dt.Columns.Add("Id", typeof(int));
            dt.Columns.Add("AccountName", typeof(string));
            dt.Columns.Add("Level", typeof(string));
            dt.Columns.Add("Particular", typeof(string));
            dt.Columns.Add("Debit", typeof(decimal));
            dt.Columns.Add("Credit", typeof(decimal));
            dt.Columns.Add("Balance", typeof(decimal));
            dt.Columns.Add("BalanceType", typeof(string));
            dt.Columns.Add("TotalAsset", typeof(decimal));
            dt.Columns.Add("TotalLiability", typeof(decimal));
            dt.Columns.Add("Difference", typeof(decimal));
            dt.Columns.Add("SortPath", typeof(string));
            dt.Columns.Add("RowType", typeof(string));
            dt.Columns.Add("SortOrder", typeof(string));
            dt.Columns.Add("FormTitle", typeof(string));
            dt.Columns.Add("FormName", typeof(string));
            dt.Columns.Add("Currency", typeof(string));
            dt.Columns.Add("UserName", typeof(string));
            dt.Columns.Add("FinancialYearId", typeof(int));

            // Fill DataTable
            foreach (var item in response.Data)
            {
                dt.Rows.Add(
                    model.FromDate,
                    model.ToDate,
                    model.AccountId,
                    model.CompanyId,
                    model.FinacialYearId,
                    model.PageNumber,
                    model.PageSize,
                    response.TotalRecords,
                    model.UserId,
                    item.CompanyId,
                    item.FormId,
                    item.CreatedOn ?? (object)DBNull.Value,
                    item.CreatedBy,
                    item.ParentId,
                    item.DetailId,
                    item.Account_Id,
                    0,
                    0,
                    item.AccountName,
                    item.Level,
                    item.Particular,
                    item.Debit,
                    item.Credit,
                    item.Balance,
                     item.BalanceType,
                    response.TotalAsset,
                    response.TotalLiability,
                    response.Difference,
                    item.SortPath,
                    item.RowType,
                    item.SortOrder,
                    item.FormTitle,
                    item.FormName,
                    item.Currency,
                    item.UserName,
                    item.FinancialYearId
                );
            }

            return dt;
        }
        #endregion

        #region Trail Balance
        public async Task<TrailBalanceReportResult> RunTrialBalanceReport(LedgerReportRequest model)
        {
            var response = new TrailBalanceReportResult
            {
                Data = new List<TrailBalanceReportData>(),
                TotalRecords = 0,
            };

            try
            {
                using (var context = new TFDSolutionEntities())
                using (var conn = context.Database.Connection)
                {
                    if (conn.State != ConnectionState.Open)
                        conn.Open();

                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "usp_GetAccountTrailBalance";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = 120;
                        cmd.Parameters.Add(new SqlParameter("@FromDate", SqlDbType.DateTime) { Value = model.FromDate });
                        cmd.Parameters.Add(new SqlParameter("@ToDate", SqlDbType.DateTime) { Value = model.ToDate });
                        cmd.Parameters.Add(new SqlParameter("@CompanyId", SqlDbType.NVarChar) { Value = model.CompanyId });
                        cmd.Parameters.Add(new SqlParameter("@FinancialYearId", SqlDbType.Int) { Value = model.FinacialYearId });
                        cmd.Parameters.Add(new SqlParameter("@PageNumber", SqlDbType.Int) { Value = model.PageNumber });
                        cmd.Parameters.Add(new SqlParameter("@PageSize", SqlDbType.Int) { Value = model.PageSize });
                        if (CodeHelper.HasParameter("usp_GetAccountTrailBalance", "ReportType", context))
                        {
                            cmd.Parameters.Add(new SqlParameter("@ReportType", SqlDbType.NVarChar) { Value = model.ReportType });
                        }
                        using (var reader = cmd.ExecuteReader())
                        {
                            // ===== Result Set 1 : Ledger Data =====
                            while (reader.Read())
                            {
                                response.Data.Add(new TrailBalanceReportData
                                {
                                    Level = reader.GetInt32Safe("Level"),
                                    Account_Id = reader.GetInt32Safe("Account_Id"),
                                    Particular = reader["Particular"] as string,
                                    Op_Debit = reader.GetDecimalSafe("Op_Debit"),
                                    Op_Credit = reader.GetDecimalSafe("Op_Credit"),
                                    Tr_Debit = reader.GetDecimalSafe("Tr_Debit"),
                                    Tr_Credit = reader.GetDecimalSafe("Tr_Credit"),
                                    Cl_Debit = reader.GetDecimalSafe("Cl_Debit"),
                                    Cl_Credit = reader.GetDecimalSafe("Cl_Credit"),
                                    RowType = reader["RowType"] as string,
                                    SortPath = reader["SortPath"] as string,
                                    SortOrder = reader.GetInt32Safe("SortOrder"),
                                });
                            }
                            // ===== Result Set 2 : Total Records =====
                            if (reader.NextResult() && reader.Read())
                            {
                                response.TotalRecords = reader.GetInt32Safe("TotalRecords");
                            }
                        }
                    }
                }
                response.IsSuccess = true;
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
                response.IsSuccess = false;
                response.Response = ex.Message;
            }
            if ((response.IsSuccess.HasValue && response.IsSuccess.Value) && response.Data.Any())
            {
                await SaveTrailBalanceReportHistoryAsync(model, response);
            }
            return response;
        }
        public async Task<ProfitLossReportResult> RunProfitLossReport(LedgerReportRequest model)
        {
            var response = new ProfitLossReportResult
            {
                Data = new List<ProfitLossReportData>(),
                TotalExpense = 0,
                TotalIncome = 0,
                NetProfit = 0,
                TotalRecords = 0
            };

            try
            {
                using (var context = new TFDSolutionEntities())
                using (var conn = context.Database.Connection)
                {
                    if (conn.State != ConnectionState.Open)
                        conn.Open();

                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "usp_GetAccountProfitLoss";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = 120;
                        cmd.Parameters.Add(new SqlParameter("@FromDate", SqlDbType.DateTime) { Value = model.FromDate });
                        cmd.Parameters.Add(new SqlParameter("@ToDate", SqlDbType.DateTime) { Value = model.ToDate });
                        cmd.Parameters.Add(new SqlParameter("@CompanyId", SqlDbType.NVarChar) { Value = model.CompanyId });
                        if (CodeHelper.HasParameter("usp_GetAccountProfitLoss", "ReportType", context))
                        {
                            cmd.Parameters.Add(new SqlParameter("@ReportType", SqlDbType.NVarChar) { Value = model.ReportType });
                        }
                        using (var reader = cmd.ExecuteReader())
                        {
                            // ===== Result Set 1 : Ledger Data =====
                            while (reader.Read())
                            {
                                response.Data.Add(new ProfitLossReportData
                                {
                                    Level = reader.GetInt32Safe("Level"),
                                    AccountName = reader["AccountName"] as string,
                                    Particular = reader["Particular"] as string,
                                    Credit = reader.GetDecimalSafe("Credit"),
                                    Debit = reader.GetDecimalSafe("Debit"),
                                    Amount = reader.GetDecimalSafe("Amount"),
                                    RowType = reader["RowType"] as string,
                                    SortPath = reader["SortPath"] as string,
                                    SortOrder = reader.GetInt32Safe("SortOrder"),
                                    CompanyId = reader["CompanyId"] as string,
                                    FormId = reader["FormId"] as string,
                                    CreatedOn = reader.IsDBNull("CreatedOn") ? (DateTime?)null : reader.GetDateTime("CreatedOn"),
                                    CreatedBy = reader["CreatedBy"] as string,
                                    ParentId = reader.GetInt32Safe("ParentId"),
                                    DetailId = reader.GetInt32Safe("DetailId"),
                                    Account_Id = reader.GetInt32Safe("Account_Id"),
                                    FormTitle = reader["FormTitle"] as string,
                                    FormName = reader["FormName"] as string,
                                    Currency = reader["Currency"] as string,
                                    UserName = reader["UserName"] as string,
                                    FinancialYearId = reader.GetInt32Safe("FinancialYearId")
                                });
                            }

                            // ===== Result Set 2 : Total Records =====
                            if (reader.NextResult() && reader.Read())
                            {
                                response.TotalIncome = reader.GetDecimalSafe("TotalIncome");
                                response.NetProfit = reader.GetDecimalSafe("NetProfit");
                                response.TotalExpense = reader.GetDecimalSafe("TotalExpense");
                                response.TotalRecords = response.Data.Count;
                            }
                        }
                    }
                }

                response.IsSuccess = true;
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
                response.IsSuccess = false;
                response.Response = ex.Message;
            }
            if ((response.IsSuccess.HasValue && response.IsSuccess.Value) && response.Data.Any())
            {
                await SaveProfitLossReportHistoryAsync(model, response);
            }
            return response;
        }
        public async Task<BalanceSheetReportResult> RunBalanceSheetReport(LedgerReportRequest model)
        {
            var response = new BalanceSheetReportResult
            {
                Data = new List<BalanceSheetReportData>(),
                TotalAsset = 0,
                TotalLiability = 0,
                Difference = 0,
                TotalRecords = 0
            };

            try
            {
                using (var context = new TFDSolutionEntities())
                using (var conn = context.Database.Connection)
                {
                    if (conn.State != ConnectionState.Open)
                        conn.Open();

                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "usp_GetAccountBalanceSheet";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = 120;
                        cmd.Parameters.Add(new SqlParameter("@FromDate", SqlDbType.DateTime) { Value = model.FromDate });
                        cmd.Parameters.Add(new SqlParameter("@ToDate", SqlDbType.DateTime) { Value = model.ToDate });
                        cmd.Parameters.Add(new SqlParameter("@CompanyId", SqlDbType.NVarChar) { Value = model.CompanyId });
                        if (CodeHelper.HasParameter("usp_GetAccountBalanceSheet", "ReportType", context))
                        {
                            cmd.Parameters.Add(new SqlParameter("@ReportType", SqlDbType.NVarChar) { Value = model.ReportType });
                        }
                        using (var reader = cmd.ExecuteReader())
                        {
                            // ===== Result Set 1 : Ledger Data =====
                            while (reader.Read())
                            {
                                response.Data.Add(new BalanceSheetReportData
                                {
                                    Level = reader.GetInt32Safe("Level"),
                                    AccountName = reader["AccountName"] as string,
                                    Particular = reader["Particular"] as string,
                                    Credit = reader.GetDecimalSafe("Credit"),
                                    Debit = reader.GetDecimalSafe("Debit"),
                                    Balance = reader.GetDecimalSafe("Balance"),
                                    BalanceType = reader["BalanceType"] as string,
                                    RowType = reader["RowType"] as string,
                                    SortPath = reader["SortPath"] as string,
                                    SortOrder = reader.GetInt32Safe("SortOrder"),
                                    CompanyId = reader["CompanyId"] as string,
                                    FormId = reader["FormId"] as string,
                                    CreatedOn = reader.IsDBNull("CreatedOn") ? (DateTime?)null : reader.GetDateTime("CreatedOn"),
                                    CreatedBy = reader["CreatedBy"] as string,
                                    ParentId = reader.GetInt32Safe("ParentId"),
                                    DetailId = reader.GetInt32Safe("DetailId"),
                                    Account_Id = reader.GetInt32Safe("Account_Id"),
                                    FormTitle = reader["FormTitle"] as string,
                                    FormName = reader["FormName"] as string,
                                    Currency = reader["Currency"] as string,
                                    UserName = reader["UserName"] as string,
                                    FinancialYearId = reader.GetInt32Safe("FinancialYearId")
                                });
                            }

                            // ===== Result Set 2 : Total Records =====
                            if (reader.NextResult() && reader.Read())
                            {
                                response.TotalAsset = reader.GetDecimalSafe("TotalAsset");
                                response.TotalLiability = reader.GetDecimalSafe("TotalLiability");
                                response.Difference = reader.GetDecimalSafe("Difference");
                                response.TotalRecords = response.Data.Count;
                            }
                        }
                    }
                }

                response.IsSuccess = true;
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
                response.IsSuccess = false;
                response.Response = ex.Message;
            }
            if ((response.IsSuccess.HasValue && response.IsSuccess.Value) && response.Data.Any())
            {
                await SaveBalanceSheetReportHistoryAsync(model, response);
            }
            return response;
        }

        public async Task SaveTrailBalanceReportHistoryAsync(LedgerReportRequest model, TrailBalanceReportResult response)
        {
            var dt = ConvertTrialBalanceDataToDataTable(model, response);

            using (var context = new TFDSolutionEntities())
            using (var conn = (SqlConnection)context.Database.Connection)
            {
                if (conn.State != ConnectionState.Open)
                    await conn.OpenAsync();

                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // Delete old records for first page
                        if (model.PageNumber == 0 || model.PageNumber == 1)
                        {
                            using (var deleteCmd = conn.CreateCommand())
                            {
                                deleteCmd.Transaction = transaction;

                                deleteCmd.CommandText = @"DELETE FROM t_TrialBalance_Hist WHERE RequestUserId = @RequestUserId";

                                deleteCmd.Parameters.Add(
                                    new SqlParameter("@RequestUserId", SqlDbType.NVarChar)
                                    {
                                        Value = model.UserId,
                                    });

                                await deleteCmd.ExecuteNonQueryAsync();
                            }
                        }
                        // Bulk Insert
                        using (var bulkCopy = new SqlBulkCopy(conn, SqlBulkCopyOptions.Default, transaction))
                        {
                            bulkCopy.DestinationTableName = "t_TrialBalance_Hist";
                            bulkCopy.BatchSize = 5000;
                            bulkCopy.BulkCopyTimeout = 120;
                            // Column Mapping
                            foreach (DataColumn col in dt.Columns)
                            {
                                bulkCopy.ColumnMappings.Add(col.ColumnName, col.ColumnName);
                            }
                            await bulkCopy.WriteToServerAsync(dt);
                        }
                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                    }
                }
            }
        }
        private DataTable ConvertTrialBalanceDataToDataTable(LedgerReportRequest model, TrailBalanceReportResult response)
        {
            var dt = new DataTable();
            // Request Parameters
            dt.Columns.Add("RequestFromDate", typeof(DateTime));
            dt.Columns.Add("RequestToDate", typeof(DateTime));
            dt.Columns.Add("RequestAccountId", typeof(int));
            dt.Columns.Add("RequestCompanyId", typeof(string));
            dt.Columns.Add("RequestFinancialYearId", typeof(int));
            dt.Columns.Add("RequestPageNumber", typeof(int));
            dt.Columns.Add("RequestPageSize", typeof(int));
            dt.Columns.Add("RequestTotalRecords", typeof(int));
            dt.Columns.Add("RequestUserId", typeof(string));
            dt.Columns.Add("Account_Id", typeof(int));
            dt.Columns.Add("Level", typeof(string));
            dt.Columns.Add("Particular", typeof(string));
            dt.Columns.Add("Op_Debit", typeof(decimal));
            dt.Columns.Add("Op_Credit", typeof(decimal));
            dt.Columns.Add("Tr_Debit", typeof(decimal));
            dt.Columns.Add("Tr_Credit", typeof(decimal));
            dt.Columns.Add("Cl_Debit", typeof(decimal));
            dt.Columns.Add("Cl_Credit", typeof(decimal));
            dt.Columns.Add("SortPath", typeof(string));
            dt.Columns.Add("RowType", typeof(string));
            dt.Columns.Add("SortOrder", typeof(int));

            // Fill DataTable
            foreach (var item in response.Data)
            {
                dt.Rows.Add(
                    model.FromDate,
                    model.ToDate,
                    model.AccountId,
                    model.CompanyId,
                    model.FinacialYearId,
                    model.PageNumber,
                    model.PageSize,
                    response.TotalRecords,
                    model.UserId,
                    item.Account_Id,
                    item.Level,
                    item.Particular,
                    item.Op_Debit,
                    item.Op_Credit,
                    item.Tr_Debit,
                    item.Tr_Credit,
                    item.Cl_Debit,
                    item.Cl_Credit,
                    item.SortPath,
                    item.RowType,
                    item.SortOrder
                );
            }

            // Add Total Row
            dt.Rows.Add(
                model.FromDate,
                model.ToDate,
                model.AccountId,
                model.CompanyId,
                model.FinacialYearId,
                model.PageNumber,
                model.PageSize,
                response.TotalRecords,
                model.UserId,
                0,
                0,
                "Total",
                response.Data.Where(x => x.RowType == "LEDGER").Sum(x => x.Op_Debit),
                response.Data.Where(x => x.RowType == "LEDGER").Sum(x => x.Op_Credit),
                response.Data.Where(x => x.RowType == "LEDGER").Sum(x => x.Tr_Debit),
                response.Data.Where(x => x.RowType == "LEDGER").Sum(x => x.Tr_Credit),
                response.Data.Where(x => x.RowType == "LEDGER").Sum(x => x.Cl_Debit),
                response.Data.Where(x => x.RowType == "LEDGER").Sum(x => x.Cl_Credit),
                "",
                "",
                response.Data.Max(x => x.SortOrder) + 1
            );
            return dt;
        }
        #endregion
    }
}