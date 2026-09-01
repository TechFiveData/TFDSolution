using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFDIntegrations.EWayBill.Models.Response
{
    public class GetEWayBillResponse : EWayErrorResponse
    {
        [JsonProperty("ewayBillNo")]
        public string EWayBillNumber { get; set; }


        [JsonProperty("ewayBillDate")]
        public string EWayBillDate { get; set; }


        [JsonProperty("validUpto")]
        public string ValidUpto { get; set; }


        [JsonProperty("status")]
        public string Status { get; set; }


        [JsonProperty("supplyType")]
        public string SupplyType { get; set; }


        [JsonProperty("subSupplyType")]
        public string SubSupplyType { get; set; }


        [JsonProperty("docType")]
        public string DocumentType { get; set; }


        [JsonProperty("docNo")]
        public string DocumentNumber { get; set; }


        [JsonProperty("docDate")]
        public string DocumentDate { get; set; }


        [JsonProperty("fromGstin")]
        public string FromGSTIN { get; set; }


        [JsonProperty("fromTrdName")]
        public string FromTradeName { get; set; }


        [JsonProperty("toGstin")]
        public string ToGSTIN { get; set; }


        [JsonProperty("toTrdName")]
        public string ToTradeName { get; set; }


        [JsonProperty("totalValue")]
        public decimal TotalValue { get; set; }


        [JsonProperty("totInvValue")]
        public decimal TotalInvoiceValue { get; set; }


        [JsonProperty("itemList")]
        public List<GetEWayBillItem> Items { get; set; }
    }


    public class GetEWayBillItem
    {
        [JsonProperty("productName")]
        public string ProductName { get; set; }


        [JsonProperty("hsnCode")]
        public int HSNCode { get; set; }


        [JsonProperty("quantity")]
        public decimal Quantity { get; set; }


        [JsonProperty("qtyUnit")]
        public string QuantityUnit { get; set; }


        [JsonProperty("taxableAmount")]
        public decimal TaxableAmount { get; set; }
    }
}
