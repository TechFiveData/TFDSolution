using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TFDSolution.Common
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
}