using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFDIntegrations.EInvoiceBill.Model
{
    public class SignedQRCodeResponse
    {

        [JsonProperty("Irn")]
        public string Irn { get; set; }


        [JsonProperty("SellerGstin")]
        public string SellerGstin { get; set; }


        [JsonProperty("BuyerGstin")]
        public string BuyerGstin { get; set; }


        [JsonProperty("DocNo")]
        public string DocNo { get; set; }


        [JsonProperty("DocDt")]
        public string DocDt { get; set; }


        [JsonProperty("TotInvVal")]
        public decimal TotInvVal { get; set; }


        [JsonProperty("QRCode")]
        public string QRCode { get; set; }

    }
    public class DownloadSignedInvoiceResponse
    {
        public int Status { get; set; }
        public DownloadSignedInvoiceData Data { get; set; }
        public ErrorDetails ErrorDetails { get; set; }
    }

    public class DownloadSignedInvoiceData
    {
        public string SignedInvoice { get; set; }
        public string Irn { get; set; }
    }

    public class ErrorDetails
    {
        public string ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
    }
}
