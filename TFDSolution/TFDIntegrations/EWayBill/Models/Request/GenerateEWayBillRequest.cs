using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFDIntegrations.EWayBill.Models.Request
{
    /// <summary>
    /// Request payload for generating an E-Way Bill.
    /// </summary>
    public class GenerateEWayBillRequest
    {
        [JsonProperty("supplyType")]
        public string SupplyType { get; set; }

        [JsonProperty("subSupplyType")]
        public string SubSupplyType { get; set; }

        [JsonProperty("subSupplyDesc")]
        public string SubSupplyDesc { get; set; }


        [JsonProperty("docType")]
        public string DocumentType { get; set; }


        [JsonProperty("docNo")]
        public string DocumentNumber { get; set; }


        [JsonProperty("docDate")]
        public string DocumentDate { get; set; }


        [JsonProperty("fromGstin")]
        public string FromGstin { get; set; }


        [JsonProperty("fromTrdName")]
        public string FromTradeName { get; set; }


        [JsonProperty("fromAddr1")]
        public string FromAddress1 { get; set; }


        [JsonProperty("fromAddr2")]
        public string FromAddress2 { get; set; }


        [JsonProperty("fromPlace")]
        public string FromPlace { get; set; }


        [JsonProperty("fromPincode")]
        public int FromPincode { get; set; }


        [JsonProperty("actFromStateCode")]
        public int ActualFromStateCode { get; set; }


        [JsonProperty("fromStateCode")]
        public int FromStateCode { get; set; }


        [JsonProperty("toGstin")]
        public string ToGstin { get; set; }


        [JsonProperty("toTrdName")]
        public string ToTradeName { get; set; }


        [JsonProperty("toAddr1")]
        public string ToAddress1 { get; set; }


        [JsonProperty("toAddr2")]
        public string ToAddress2 { get; set; }


        [JsonProperty("toPlace")]
        public string ToPlace { get; set; }


        [JsonProperty("toPincode")]
        public int ToPincode { get; set; }


        [JsonProperty("actToStateCode")]
        public int ActualToStateCode { get; set; }


        [JsonProperty("toStateCode")]
        public int ToStateCode { get; set; }


        [JsonProperty("transactionType")]
        public int TransactionType { get; set; }

        [JsonProperty("shipToGSTIN")]
        public string ShipToGSTIN { get; set; }

        [JsonProperty("shipToTradeName")]
        public string ShipToTradeName { get; set; }


        [JsonProperty("totalValue")]
        public decimal TotalValue { get; set; }


        [JsonProperty("cgstValue")]
        public decimal CgstValue { get; set; }


        [JsonProperty("sgstValue")]
        public decimal SgstValue { get; set; }


        [JsonProperty("igstValue")]
        public decimal IgstValue { get; set; }


        [JsonProperty("cessValue")]
        public decimal CessValue { get; set; }


        [JsonProperty("cessNonAdvolValue")]
        public decimal CessNonAdvolValue { get; set; }


        [JsonProperty("totInvValue")]
        public decimal TotalInvoiceValue { get; set; }


        [JsonProperty("transporterId")]
        public string TransporterId { get; set; }


        [JsonProperty("transporterName")]
        public string TransporterName { get; set; }


        [JsonProperty("transDocNo")]
        public string TransportDocumentNumber { get; set; }


        [JsonProperty("transMode")]
        public string TransportMode { get; set; }


        [JsonProperty("transDistance")]
        public string TransportDistance { get; set; }


        [JsonProperty("vehicleNo")]
        public string VehicleNumber { get; set; }


        [JsonProperty("vehicleType")]
        public string VehicleType { get; set; }


        [JsonProperty("itemList")]
        public List<EWayBillItem> ItemList { get; set; }
    }
}
