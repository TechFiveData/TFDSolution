using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace TFDSolution.Transport.Master
{
    public class TransactionModel
    {
    }

    public class AccountLedgerOpenModel
    {
        public int AccountId { get; set; }
        public string AccountLedger { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }
    public class AmendmentRequest : BaseRequest
    {
        public int TransactionId { get; set; }
        public string Reason { get; set; }
        public string FormId { get; set; }
    }

    public class DeliveryScheduleModel : BaseRequest
    {
        public int ParentId { get; set; }
        public int DetailId { get; set; }
        public int ItemId { get; set; }
        public string FormId { get; set; }
        public List<DeliveryScheduleData> Data { get; set; }
        public List<FormItemAdvanceField> Fields { get; set; }
        public List<ButtonDataItem> ButtonFieldData { get; set; }
    }

    public class DeliveryScheduleData
    {
        public int SrNo { get; set; }
        public string ScheduledDate { get; set; }
        public int Quantity { get; set; }
        public string Remarks { get; set; }
    }

    public class DashboardSettingModel
    {
        public int SrNo { get; set; }
        public int DashboardId { get; set; }
        public string DashboardTitle { get; set; }   
        public string DashboardName { get; set; }
        public bool IsActive { get; set; }
        public int SortOrder { get; set; }
        public string AssignedUsers { get; set; }
        public string UserId { get; set; }
    }

    public class DashboardDetailSettingModel
    {
        public int SrNo { get; set; }
        public int Id { get; set; }
        public int DashboardId { get; set; }
        public string Title { get; set; }
        public string WidgetType { get; set; }
        public string BackgroundColor { get; set; }
        public bool IsActive { get; set; }
        public int SortOrder { get; set; }
        public string FontColor { get; set; }
        public string SQLDataScript { get; set; }
        public string RedirectURL { get; set; }
        public string DisplaySize { get; set; }
        public string ChartType { get; set; }
    }
}
