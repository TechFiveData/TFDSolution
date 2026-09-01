using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TFDSolution.Transport.Integration;

namespace TFDIntegrations.EInvoiceBill.Model.Request
{
    public class GenerateIRNRequest
    {

        [JsonProperty("Version")]
        public string Version { get; set; }


        [JsonProperty("Irn")]
        public string Irn { get; set; }


        [JsonProperty("TranDtls")]
        public TranDtls TranDtls { get; set; }


        [JsonProperty("DocDtls")]
        public DocDtls DocDtls { get; set; }


        [JsonProperty("SellerDtls")]
        public SellerDtls SellerDtls { get; set; }


        [JsonProperty("BuyerDtls")]
        public BuyerDtls BuyerDtls { get; set; }


        [JsonProperty("ItemList")]
        public List<EInvoiceItem> ItemList { get; set; }


        [JsonProperty("ValDtls")]
        public ValDtls ValDtls { get; set; }

    }
    public class CancelInvoiceRequest
    {
        public string Irn { get; set; }
        public string CnlRsn { get; set; }
        public string CnlRem { get; set; }
    }
}
