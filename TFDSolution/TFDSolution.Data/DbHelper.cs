using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Data.Common;

namespace TFDSolution.Data
{
    public static class DbHelper
    {
        public static int GetInt32Safe(this IDataRecord reader, string column)
        {
            return reader[column] == DBNull.Value ? 0 : Convert.ToInt32(reader[column]);
        }

        public static decimal GetDecimalSafe(this IDataRecord reader, string column)
        {
            return reader[column] == DBNull.Value ? 0m : Convert.ToDecimal(reader[column]);
        }

        public static DateTime GetDateTime(this IDataRecord reader, string column)
        {
            return Convert.ToDateTime(reader[column]);
        }

        public static bool IsDBNull(this IDataRecord reader, string column)
        {
            return reader[column] == DBNull.Value;
        }
        /// <summary>
        /// Executes a stored procedure with input/output parameters and returns a DataSet.
        /// The output parameters are updated in the provided 'parameters' list.
        /// </summary>
        public static DataSet ExecuteStoredProcedure(DbContext context, string storedProcedure, List<SqlParameter> parameters)
        {
            DataSet dataSet = new DataSet();

            if (context == null)
                throw new ArgumentNullException(nameof(context), "DbContext cannot be null.");

            using (var connection = context.Database.Connection as SqlConnection)
            {
                if (connection == null)
                    throw new InvalidOperationException("Database connection is not a SQL Server connection.");

                using (SqlCommand command = new SqlCommand(storedProcedure, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    if (parameters != null)
                    {
                        foreach (var param in parameters)
                        {
                            if (param.Direction == ParameterDirection.Output || param.Direction == ParameterDirection.InputOutput)
                            {
                                param.Size = 500; // Adjust output parameter size if needed
                            }
                            command.Parameters.Add(param);
                        }
                    }

                    if (connection.State != ConnectionState.Open)
                        connection.Open();

                    using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                    {
                        adapter.Fill(dataSet);
                    }

                    // Output parameters are automatically updated in 'parameters' list
                }
            }

            return dataSet;
        }

        public static DataSet GetDataSet(DbContext context, string sql, CommandType type, Dictionary<string, object> parameters)
        {
            var ds = new DataSet();
            try
            {
                if (context == null)
                    throw new ArgumentNullException(nameof(context), "DbContext cannot be null.");

                //using (var connection = context.Database.Connection as SqlConnection)
                //{
                using (var cmd = context.Database.Connection.CreateCommand())
                {
                    var connection = context.Database.Connection;

                    cmd.CommandText = sql;
                    cmd.CommandType = type;
                    if (parameters != null)
                    {
                        foreach (var kvp in parameters)
                        {
                            var p = cmd.CreateParameter();
                            p.ParameterName = kvp.Key;
                            p.Value = kvp.Value ?? DBNull.Value;
                            cmd.Parameters.Add(p);
                        }
                    }
                    var adapter = DbProviderFactories.GetFactory(connection).CreateDataAdapter();
                    adapter.SelectCommand = cmd;

                    if (cmd.Connection.State != ConnectionState.Open)
                    {
                        cmd.Connection.Open();
                    }

                    adapter.Fill(ds);
                }
                //}
            }
            catch
            {
                ds = new DataSet();
            }
            return ds;
        }
    }
}
