using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TFDSolution.Business.Interface;
using TFDSolution.Data;
using TFDSolution.Transport;
using TFDSolution.Transport.Master;

namespace TFDSolution.Business
{
    public class DashboardBusiness : iDashboardBusiness
    {
        public List<DashboardModel> GetDashboard(string UserId, string CompanyId)
        {
            try
            {
                using (var context = new TFDSolutionEntities())
                {
                    var param1 = new SqlParameter("@CompanyId", CompanyId);
                    var param2 = new SqlParameter("@UserId", UserId);
                    return context.Database.SqlQuery<DashboardModel>("EXEC m_GetDashboard_User @CompanyId,@UserId", param1, param2).ToList();
                }
            }
            catch (Exception)
            {

            }
            return new List<DashboardModel>();
        }
        public DashboardModel GetDashboardData(string dashboardName, string CompanyId, int financialYear)
        {
            var model = new DashboardModel();

            try
            {
                using (var context = new TFDSolutionEntities())
                using (var command = context.Database.Connection.CreateCommand())
                {
                    command.CommandText = "dbo.m_GetDashboardData";
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@DashboardName", (object)dashboardName ?? DBNull.Value));

                    if (command.Connection.State == ConnectionState.Closed)
                        command.Connection.Open();

                    using (var reader = command.ExecuteReader())
                    {
                        // Read DashboardModel
                        if (reader.Read())
                        {
                            model = MapDashboard(reader);
                        }
                        // Move to next result set (Widgets)
                        if (reader.NextResult())
                        {
                            var widgets = new List<Dashboard_Widgets>();
                            while (reader.Read())
                            {
                                widgets.Add(MapWidget(reader, CompanyId, financialYear));
                            }
                            model.Settings = widgets;
                        }
                    }
                }
                model.IsSuccess = true;
            }
            catch (Exception ex)
            {
                model.IsSuccess = false;
                model.Response = "Exception Occurred: " + ex.Message;
            }
            return model;
        }
        private DashboardModel MapDashboard(IDataRecord reader)
        {
            return new DashboardModel
            {
                IsSuccess = reader["IsSuccess"] as bool? ?? false,
                // Example mapping (adjust based on actual fields returned by SP)
                DashboardId = reader["DashboardId"] as int? ?? 0,
                SortOrder = reader["SortOrder"] as int? ?? 0,
                DashboardName = reader["DashboardName"] as string,
                DashboardTitle = reader["DashboardTitle"] as string,
                Response = reader["Response"] as string
            };
        }

        private Dashboard_Widgets MapWidget(IDataRecord reader, string CompanyId, int financialYear)
        {
            return new Dashboard_Widgets
            {
                BackgroundColor = reader["BackgroundColor"] as string,
                FontColor = reader["FontColor"] as string,
                IsActive = reader["IsActive"] as bool? ?? false,
                RedirectURL = reader["RedirectURL"] as string,
                Title = reader["Title"] as string,
                SQLDataScript = reader["SQLDataScript"] as string,
                SortOrder = reader["SortOrder"] as int? ?? 0,
                WidgetType = reader["WidgetType"] as string,
                DisplaySize = reader["DisplaySize"] as string,
                ChartType = reader["ChartType"] as string,
                Data = GetDynamicWidgetData(reader["SQLDataScript"] as string, CompanyId, financialYear),
                ResultData = GetDynamicScriptData(reader["SQLDataScript"] as string, CompanyId, financialYear)
            };
        }
        public IEnumerable<IDictionary<string, object>> GetDynamicWidgetData(string storedProcedureName, string CompanyId, int financialYear) //Task<IEnumerable<IDictionary<string, object>>>
        {
            List<Dictionary<string, object>> items = new List<Dictionary<string, object>>();
            try
            {
                if (!string.IsNullOrEmpty(storedProcedureName))
                {
                    using (var context = new TFDSolutionEntities())
                    {
                        using (var command = context.Database.Connection.CreateCommand())
                        {
                            if (context.Database.Connection.State == ConnectionState.Closed)
                                context.Database.Connection.Open();

                            command.Parameters.Add(new SqlParameter { ParameterName = "@CompanyId", Value = CompanyId });
                            // ✅ Check and add FinancialYearId
                            if (HasParameter(storedProcedureName, "FinancialYearId", context))
                            {
                                command.Parameters.Add(new SqlParameter("@FinancialYearId", financialYear));
                            }
                            command.CommandText = storedProcedureName;
                            command.CommandType = CommandType.StoredProcedure;
                            using (var reader = command.ExecuteReader())
                            {
                                if (reader.HasRows)
                                {
                                    while (reader.Read())
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
                                // If there's an additional result set (table), read and attach it
                                if (reader.NextResult())
                                {
                                    var secondTable = new List<Dictionary<string, object>>();
                                    if (reader.HasRows)
                                    {
                                        while (reader.Read())
                                        {
                                            Dictionary<string, object> obj2 = new Dictionary<string, object>();
                                            for (int i = 0; i < reader.FieldCount; i++)
                                            {
                                                string columnName = reader.GetName(i);
                                                object columnValue = reader.GetValue(i);
                                                obj2[columnName] = columnValue;
                                            }
                                            secondTable.Add(obj2);
                                        }
                                    }

                                    if (secondTable.Count > 0)
                                    {
                                        // Add a wrapper entry to preserve the second table structure.
                                        // Consumers can detect "__SecondResultSet" key and process the nested list.
                                        items.Add(new Dictionary<string, object>
                                        {
                                            { "__SecondResultSet", (object)secondTable }
                                        });
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
            return items;
        }
        private bool HasParameter(string spName, string paramName, TFDSolutionEntities context)
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
        public DashboardResultData GetDynamicScriptData(string storedProcedureName, string companyId, int financialYear)
        {
            var result = new DashboardResultData
            {
                Data = new List<Dictionary<string, object>>(),
                SummaryColumn = new List<string>()
            };
            try
            {
                if (string.IsNullOrWhiteSpace(storedProcedureName))
                    return result;

                using (var context = new TFDSolutionEntities())
                using (var command = context.Database.Connection.CreateCommand())
                {
                    command.CommandText = storedProcedureName;
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@CompanyId", companyId));
                    // ✅ Check and add FinancialYearId
                    if (HasParameter(storedProcedureName, "FinancialYearId", context))
                    {
                        command.Parameters.Add(new SqlParameter("@FinancialYearId", financialYear));
                    }
                    if (command.Connection.State != ConnectionState.Open)
                        command.Connection.Open();

                    using (var reader = command.ExecuteReader())
                    {
                        // First Result Set
                        result.Data = ReadResultSet(reader);

                        // Second Result Set (Summary)
                        if (reader.NextResult())
                        {
                            var secondResult = ReadResultSet(reader);

                            // If summary is just column names
                            if (secondResult.Count > 0)
                            {
                                result.SummaryColumn = secondResult
                                .SelectMany(dict => dict.Values)
                                .Where(v => v != null)
                                .Select(v => v.ToString())
                                .Distinct()
                                .ToList();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
            }
            return result;
        }

        private List<Dictionary<string, object>> ReadResultSet(DbDataReader reader)
        {
            var list = new List<Dictionary<string, object>>();
            while (reader.Read())
            {
                var row = new Dictionary<string, object>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                }
                list.Add(row);
            }
            return list;
        }

        public ChartDataResult GetChartData(string period, string sqlScript)
        {
            var labels = new List<string>();
            var values = new List<decimal>();
            var data = new List<object>();
            try
            {
                using (var context = new TFDSolutionEntities())
                using (var command = context.Database.Connection.CreateCommand())
                {
                    command.CommandText = sqlScript;
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@Period", period));

                    if (command.Connection.State != ConnectionState.Open)
                        command.Connection.Open();

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            data.Add(new
                            {
                                XAxisLabel = reader["XAxisLabel"].ToString(),
                                Category = reader["Category"].ToString(),
                                YAxisValue = reader["YAxisValue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["YAxisValue"]),
                                SortKey = reader["SortKey"].ToString()
                            });
                            labels.Add(reader["XAxisLabel"].ToString());
                            values.Add(Convert.ToDecimal(reader["YAxisValue"]));
                        }
                    }
                }
                return new ChartDataResult
                {
                    IsSuccess = true,
                    Labels = labels,
                    Values = values,
                    data = data
                };
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
            }
            return new ChartDataResult
            {
                IsSuccess = false,
                Labels = labels,
                Values = values
            };
        }

    }
}
