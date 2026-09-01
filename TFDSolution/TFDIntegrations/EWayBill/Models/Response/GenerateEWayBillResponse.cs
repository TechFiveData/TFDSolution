using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFDIntegrations.EWayBill.Models.Response
{
    /// <summary>
    /// Response returned after successful E-Way Bill generation.
    /// </summary>
    /// 
    public class GenerateEWayBillResponse : EWayErrorResponse
    {
        [JsonProperty("ewayBillNo")]
        public string EWayBillNumber { get; set; }


        [JsonProperty("ewayBillDate")]
        public string EWayBillDate { get; set; }


        [JsonProperty("validUpto")]
        public string ValidUpto { get; set; }


        [JsonProperty("alert")]
        public string Alert { get; set; }
    }
    public class EWayErrorResponse
    {
        [JsonProperty("status_cd")]
        public string status_cd { get; set; }
        public GstError error { get; set; }
    }

    public class GstError
    {
        [JsonProperty("error_cd")]
        public string error_cd { get; set; }

        [JsonProperty("message")]
        public string message { get; set; }
    }
}
