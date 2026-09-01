using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFDIntegrations.EWayBill.Models.Request
{
    public class EWayBillItem
    {
        [JsonProperty("productName")]
        public string ProductName { get; set; }


        [JsonProperty("productDesc")]
        public string ProductDescription { get; set; }


        [JsonProperty("hsnCode")]
        public int HsnCode { get; set; }


        [JsonProperty("quantity")]
        public decimal Quantity { get; set; }


        [JsonProperty("qtyUnit")]
        public string QuantityUnit { get; set; }


        [JsonProperty("taxableAmount")]
        public decimal TaxableAmount { get; set; }


        [JsonProperty("sgstRate")]
        public decimal SgstRate { get; set; }


        [JsonProperty("cgstRate")]
        public decimal CgstRate { get; set; }


        [JsonProperty("igstRate")]
        public decimal IgstRate { get; set; }


        [JsonProperty("cessRate")]
        public decimal CessRate { get; set; }
    }

}
