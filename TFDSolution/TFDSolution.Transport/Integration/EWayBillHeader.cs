using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFDSolution.Transport.Integration
{
    public class EWayBillRequest
    {
        public EWayBillHeader Header { get; set; }

        public List<EWayBillItem> Items { get; set; }

        public bool IsSuccess { get; set; }

        public string Response { get; set; }
    }
    public class EWayBillHeader
    {
        public string supplyType { get; set; }
        public string subSupplyType { get; set; }
        public string subSupplyDesc { get; set; }

        public string docType { get; set; }
        public string docNo { get; set; }
        public string docDate { get; set; }

        public string fromGstin { get; set; }
        public string fromTrdName { get; set; }
        public string fromAddr1 { get; set; }
        public string fromAddr2 { get; set; }
        public string fromPlace { get; set; }
        public string fromPincode { get; set; }
        public string actFromStateCode { get; set; }
        public string fromStateCode { get; set; }

        public string toGstin { get; set; }
        public string toTrdName { get; set; }
        public string toAddr1 { get; set; }
        public string toAddr2 { get; set; }
        public string toPlace { get; set; }
        public string toPincode { get; set; }
        public string actToStateCode { get; set; }
        public string toStateCode { get; set; }

        public int transactionType { get; set; }

        public string shipToGSTIN { get; set; }
        public string shipToTradeName { get; set; }

        public decimal otherValue { get; set; }
        public decimal totalValue { get; set; }
        public decimal cgstValue { get; set; }
        public decimal sgstValue { get; set; }
        public decimal igstValue { get; set; }
        public decimal cessValue { get; set; }
        public decimal cessNonAdvolValue { get; set; }
        public decimal totInvValue { get; set; }

        public string transporterId { get; set; }
        public string transporterName { get; set; }

        public string transDocNo { get; set; }
        public int transMode { get; set; }
        public int transDistance { get; set; }
        public string transDocDate { get; set; }

        public string vehicleNo { get; set; }
        public string vehicleType { get; set; }
    }

    public class EWayBillItem
    {
        public string productName { get; set; }
        public string productDesc { get; set; }
        public string hsnCode { get; set; }

        public decimal quantity { get; set; }
        public string qtyUnit { get; set; }

        public decimal cgstRate { get; set; }
        public decimal sgstRate { get; set; }
        public decimal igstRate { get; set; }

        public decimal cessRate { get; set; }
        public decimal cessNonadvol { get; set; }

        public decimal taxableAmount { get; set; }
    }
}
