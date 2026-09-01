using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Data;
using System.Linq;

namespace TFDSolution.Data
{
    public static class EFSqlHelper
    {
        // Execute Scalar
        public static T ExecuteScalar<T>(DbContext dbContext, string storedProc, List<SqlParameter> parameters)
        {
            using (var cmd = CreateCommand(dbContext, storedProc, parameters))
            {
                dbContext.Database.Connection.Open();
                object result = cmd.ExecuteScalar();
                dbContext.Database.Connection.Close();
                return result == null || result == DBNull.Value ? default(T) : (T)Convert.ChangeType(result, typeof(T));
            }
        }

        // Execute NonQuery
        public static int ExecuteNonQuery(DbContext dbContext, string storedProc, List<SqlParameter> parameters)
        {
            using (var cmd = CreateCommand(dbContext, storedProc, parameters))
            {
                dbContext.Database.Connection.Open();
                int result = cmd.ExecuteNonQuery();
                dbContext.Database.Connection.Close();
                return result;
            }
        }

        // Execute DataTable
        public static DataTable ExecuteDataTable(DbContext dbContext, string storedProc, List<SqlParameter> parameters)
        {
            using (var cmd = CreateCommand(dbContext, storedProc, parameters))
            using (var adapter = new SqlDataAdapter((SqlCommand)cmd))
            {
                var table = new DataTable();
                adapter.Fill(table);
                return table;
            }
        }

        // Execute DataSet
        public static DataSet ExecuteDataSet(DbContext dbContext, string storedProc, List<SqlParameter> parameters)
        {
            using (var cmd = CreateCommand(dbContext, storedProc, parameters))
            using (var adapter = new SqlDataAdapter((SqlCommand)cmd))
            {
                var ds = new DataSet();
                adapter.Fill(ds);
                return ds;
            }
        }

        // Execute and Map to List<T>
        public static List<T> ExecuteModel<T>(DbContext dbContext, string storedProc, List<SqlParameter> parameters)
        {
            string sql = BuildExecSql(storedProc, parameters);
            return dbContext.Database.SqlQuery<T>(sql, parameters.ToArray()).ToList();
        }

        // Helper: Build Exec SQL
        private static string BuildExecSql(string spName, List<SqlParameter> parameters)
        {
            var paramNames = parameters?.Select(p => p.ParameterName.StartsWith("@") ? p.ParameterName : "@" + p.ParameterName);
            return $"EXEC {spName} {string.Join(", ", paramNames)}";
        }

        // Helper: Create DbCommand
        private static DbCommand CreateCommand(DbContext dbContext, string storedProc, List<SqlParameter> parameters)
        {
            var conn = dbContext.Database.Connection;
            var cmd = conn.CreateCommand();
            cmd.CommandText = storedProc;
            cmd.CommandType = CommandType.StoredProcedure;
            if (parameters != null)
                foreach (var param in parameters)
                    cmd.Parameters.Add(param);
            return cmd;
        }
/*
//Scalar Result
var userId = EfSqlHelper.ExecuteScalar<int>(db, "sp_GetUserId", new List<SqlParameter>
{
    new SqlParameter("@UserName", "lavina")
});

//DataTable
var dt = EfSqlHelper.ExecuteDataTable(db, "sp_GetAllUsers", null);

//Model Mapping
var users = EfSqlHelper.ExecuteModel<UserViewModel>(db, "sp_GetUsers", new List<SqlParameter>
{
    new SqlParameter("@Status", true)
});

//TVP Example
var dt = new DataTable();
dt.Columns.Add("Id", typeof(int));
dt.Columns.Add("Name", typeof(string));
dt.Rows.Add(1, "Lavina");
var parameters = new List<SqlParameter>
{
    new SqlParameter("@UserTable", SqlDbType.Structured)
    {
        TypeName = "dbo.UserType",
        Value = dt
    }
};
EfSqlHelper.ExecuteNonQuery(db, "sp_SaveUsers", parameters);

//Output Parameters
var outputParam = new SqlParameter("@UserName", SqlDbType.VarChar, 100)
                  {
                      Direction = ParameterDirection.Output
                  };
EfSqlHelper.ExecuteNonQuery(db, "sp_GetUserNameById", new List<SqlParameter>
{
    new SqlParameter("@UserId", 5),
    outputParam
});
string name = outputParam.Value?.ToString();*/
    }
}
