using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Data.Entity;


namespace TFDSolution.Data
{
    public abstract class BaseRepository<TContext> where TContext : DbContext, new()
    {
        protected TContext dbContext;

        protected BaseRepository()
        {
            dbContext = new TContext();
        }

        public T ExecuteScalar<T>(string storedProc, List<SqlParameter> parameters)
        {
            return EFSqlHelper.ExecuteScalar<T>(dbContext, storedProc, parameters);
        }

        public int ExecuteNonQuery(string storedProc, List<SqlParameter> parameters)
        {
            return EFSqlHelper.ExecuteNonQuery(dbContext, storedProc, parameters);
        }

        public DataTable ExecuteDataTable(string storedProc, List<SqlParameter> parameters)
        {
            return EFSqlHelper.ExecuteDataTable(dbContext, storedProc, parameters);
        }

        public DataSet ExecuteDataSet(string storedProc, List<SqlParameter> parameters)
        {
            return EFSqlHelper.ExecuteDataSet(dbContext, storedProc, parameters);
        }

        public List<T> ExecuteModel<T>(string storedProc, List<SqlParameter> parameters)
        {
            return EFSqlHelper.ExecuteModel<T>(dbContext, storedProc, parameters);
        }
    }
}
