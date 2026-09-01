using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFDSolution.Transport.Master
{
    public class FormPendingModel
    {
        public int FormPendingId { get; set; }
        public string FormId { get; set; }
        public string FormTabName { get; set; }
        public string SourceName { get; set; }
        public string SourceRequest { get; set; }
        public int SortOrder { get; set; }
        public Guid UserId { get; set; }
        public string ParentFormName { get; set; }
        public string FormName { get; set; }
        public string FromFormId { get; set; }
        public string FromFormTabId { get; set; }
        public string ParentFormId { get; set; }

        public string FormTabId { get; set; }
    }
    public class FormModel
    {
        public string FormId { get; set; }
        public string FormTitle { get; set; }
        public string FormName { get; set; }
        public string ParentFormName { get; set; }
        public string ParentFormId { get; set; }
        public bool IsActive { get; set; }
        public int UserApprovals { get; set; }
        public int SrNo { get; set; }
        public string UserId { get; set; }
    }

    public class FormUserApprovalModel
    {
        public int SrNo { get; set; }
        public string FormId { get; set; }
        public string FormTitle { get; set; }
        public string FormName { get; set; }
        public string ParentFormName { get; set; }
        public string ParentFormId { get; set; }
        public int Priority { get; set; }
        public string UserName { get; set; }
        public string UserId { get; set; }
        public string ApprovalType { get; set; }
    }
    public class UserApprovalSettingModel
    {
        public string FormId { get; set; }
        public string CreatedBy { get; set; }
        public string ApprovalType { get; set; }
        public List<UserApprovalModel> UserPriorities { get; set; }
    }
    public class UserApprovalModel
    {
        public string UserId { get; set; }
        public int Priority { get; set; }
    }
}
