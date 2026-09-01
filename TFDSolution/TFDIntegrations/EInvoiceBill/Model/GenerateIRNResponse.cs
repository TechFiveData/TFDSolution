using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TFDIntegrations.EWayBill.Models.Response;

namespace TFDIntegrations.EInvoiceBill.Model
{
    public class EInvoiceErrorResponse
    {
        [JsonProperty("Status")]
        public string Status { get; set; }

        [JsonProperty("Data")]
        public string Data { get; set; }

        [JsonProperty("ErrorDetails")]
        public List<ErrorDetail> ErrorDetails { get; set; }

        [JsonProperty("InfoDtls")]
        public object InfoDtls { get; set; }
    }

    public class GenerateIRNResponse: EInvoiceErrorResponse
    {

        [JsonProperty("Status")]
        public string Status { get; set; }


        [JsonProperty("Irn")]
        public string Irn { get; set; }


        [JsonProperty("AckNo")]
        public string AckNo { get; set; }


        [JsonProperty("AckDt")]
        public string AckDt { get; set; }


        [JsonProperty("SignedInvoice")]
        public string SignedInvoice { get; set; }


        [JsonProperty("SignedQRCode")]
        public string SignedQRCode { get; set; }


        [JsonProperty("EwbNo")]
        public string EwbNo { get; set; }


        [JsonProperty("EwbDt")]
        public string EwbDt { get; set; }


        [JsonProperty("EwbValidTill")]
        public string EwbValidTill { get; set; }

        public string ResponseJson { get; set; }

    }
}
