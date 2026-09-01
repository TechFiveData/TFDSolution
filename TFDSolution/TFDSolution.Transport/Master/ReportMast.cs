using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls.WebParts;

namespace TFDSolution.Transport.Master
{
    public class ReportMast
    {
        public int ReportId { get; set; }
        public string ReportName { get; set; }
        public string ReportTitle { get; set; }
        public string ReportPath { get; set; }
        public string FormId { get; set; }
        public string ReportType { get; set; }
        public string ReportSource { get; set; }
        public string FormName { get; set; }
        public string FormTitle { get; set; }
        public string SQLTableName { get; set; }
    }

    public class MultiApprovalModel
    {
        public List<FormMast> ParentForms { get; set; }
    }
    public class MultipleApprovalRequest
    {
        public string FormId { get; set; }
        public string CompanyId { get; set; }
        public string UserId { get; set; }
        public string EntryType { get; set; }
        public string RecorId { get; set; }
        public int Status { get; set; }
        public string CancelReason { get; set; }
    }

    public class ContactDetail
    {
        public int ParentId { get; set; }
        public string Email { get; set; }
        public string ContactPerson { get; set; }
        public string Designation { get; set; }
    }
}
