using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TFDSolution.Business.Interface;
using TFDSolution.Data;
using TFDSolution.Transport.Master;

namespace TFDSolution.Business
{
    public class DynamicallyReportBusiness: IDynamicallyReportBusiness
    {
        public ProcedureSchema GetSourceColumns(string tableName)
        {
            ProcedureSchema schema = new ProcedureSchema();
            try
            {

                using (var context = new TFDSolutionEntities())
                {
                    
                    var param2 = new SqlParameter("@SPName", tableName);
                    // Use SqlQuery<string>() since we expect a list of column names (strings)
                    var columns = context.Database.SqlQuery<string>("EXEC proc_GetSPResultSchema @SPName", param2).ToList();
                    schema.Columns = columns;

                    var param3 = new SqlParameter("@SPName", tableName);
                    var param4 = new SqlParameter("@FieldId", "");
                    // Use SqlQuery<string>() since we expect a list of column names (strings)
                    var request = context.Database.SqlQuery<ProcedureRequest>("EXEC proc_GetSPRequestSchema @SPName, @FieldId", param3, param4).ToList();
                    schema.Request = request;

                    var param5 = new SqlParameter("@TableName", "");
                    // Use SqlQuery<string>() since we expect a list of column names (strings)
                    var SPcolumns = context.Database.SqlQuery<string>("EXEC proc_GetColumnsDynamic @TableName", param5).ToList();
                    schema.SPColumns = SPcolumns;

                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
            }
            return schema;
        }
    }
}
