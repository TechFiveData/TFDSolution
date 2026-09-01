using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading.Tasks;

namespace TFDSolution.Transport.Master
{
    public class ItemStockSerialModel : BaseRequest
    {
        public int ParentId { get; set; }
        public int DetailId { get; set; }
        public int ItemId { get; set; }
        public int ItemSrNo { get; set; }
        public string FormId { get; set; }
        public string FormTabId { get; set; }
        public DateTime DocumentDate { get; set; }
        public int LocationId { get; set; }
        public string LocationName { get; set; }
        public string SerialNo { get; set; }
        public int SerialCount { get; set; }
        public string BatchLotNo { get; set; }
        public List<StockSerialData> Data { get; set; }
        public string Action { get; set; }
        public bool SerialWise { get; set; }
        public string StockEffect { get; set; }
        public List<FormItemAdvanceField> Fields { get; set; }
        public List<FormItemAdvanceField> HeaderFields { get; set; }
        
        public List<ButtonDataItem> ButtonFieldData { get; set; }

        public List<Dictionary<string, object>> DynamicData { get; set; }
    }

    public class StockSerialData
    {
        public int SrNo { get; set; }
        public int Status { get; set; }
        public string SerialNo { get; set; }
        public string BatchLotNo { get; set; }
        public double Quantity { get; set; }
        public double In_Quantity { get; set; }
        public double Out_Quantity { get; set; }
        public string StockEffect { get; set; }
        public double Stock { get; set; }
        public double Rate { get; set; }
        public double Amount { get; set; }
        public Nullable<DateTime> MFGDate { get; set; }
        public Nullable<DateTime> ExpDate { get; set; }
    }
    public class FormFieldInfoRequest
    {
        public string FieldId { get; set; }
        public string CompanyId { get; set; }
        public string FieldName { get; set; }
        public int FieldValue { get; set; }
        public List<PageFieldData> FieldData { get; set; }
    }
    public class FormFieldInfoModel
    {
        public int InformationId { get; set; }
        public string FieldId { get; set; }
        public string TabName { get; set; }
        public string SQLScriptName { get; set; }
        public string CreatedBy { get; set; }
        public int SortOrder { get; set; }
        public string UpdatedBy { get; set; }

        public List<Dictionary<string, object>> Data { get; set; }
    }
}
