using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Web;

namespace TFDSolution.Transport.Common
{
    public class AuditLog
    {
        public int LogId { get; set; }
        public DateTime LogDateTimeStamp { get; set; }
        public Guid CompanyId { get; set; }
        public Guid UserId { get; set; }
        public int? FinancialYearId { get; set; }
        public string PageName { get; set; }
        public string Action { get; set; }
        public string RecordId { get; set; }
        public string FieldName { get; set; }
        public string FieldOldValue { get; set; }
        public string FieldNewValue { get; set; }
        public string Remark { get; set; }
        public string IPAddress { get; set; }
    }


    public class EmailLog
    {
        public string FormId { get; set; }
        public string CompanyId { get; set; }
        public string UserId { get; set; }
        public int RecordId { get; set; }
        public string ToEmail { get; set; }
        public string CCEmail { get; set; }
        public string BCCEmail { get; set; }
        public string Subject { get; set; }
        public string AttachedFileName { get; set; }
        public bool IsSent { get; set; }

        public string ExceptionMessage { get; set; }

    }

    public class AppLog
    {
        public string LogLevel { get; set; }
        public string Logger { get; set; }
        public string Message { get; set; }
        public string Exception { get; set; }
        public string UserId { get; set; }
        public string CompanyId { get; set; }

    }
}