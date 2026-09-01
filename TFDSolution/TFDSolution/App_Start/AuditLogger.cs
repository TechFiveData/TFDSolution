using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TFDSolution.Transport.Common;
namespace TFDSolution.App_Start
{
    public static class AuditLogger
    {
        public static List<AuditLog> GetFieldLevelChanges<T>(T oldObj, T newObj, string pageName, string actionName, string recordId, Guid companyId, Guid userId, int? financialYearId, string ipAddress)
        {
            var logs = new List<AuditLog>();
            var props = typeof(T).GetProperties();

            foreach (var prop in props)
            {
                var oldValue = prop.GetValue(oldObj)?.ToString();
                var newValue = prop.GetValue(newObj)?.ToString();

                if (oldValue != newValue)
                {
                    logs.Add(new AuditLog
                    {
                        PageName = pageName,
                        Action = actionName,
                        RecordId = recordId,
                        FieldName = prop.Name,
                        FieldOldValue = oldValue,
                        FieldNewValue = newValue,
                        CompanyId = companyId,
                        UserId = userId,
                        FinancialYearId = financialYearId,
                        IPAddress = ipAddress
                    });
                }
            }

            return logs;
        }

/*--Implementation--
        var logs = AuditLogger.GetFieldLevelChanges(model, model, "User Page", "save", model.UserId.ToString(),
  (Guid)Session["CompanyId"], (Guid)Session["UserId"], (int?)Session["FinancialYearId"], Request.UserHostAddress);

            foreach (var log in logs)
            {

                CommonBusiness.SaveAuditLog(log);
            }
*/
}

}