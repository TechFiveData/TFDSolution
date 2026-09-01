using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFDIntegrations.EWayBill.Models.Response
{
    public class CancelEWayBillResponse : EWayErrorResponse
    {
        [JsonProperty("ewayBillNo")]
        public string EWayBillNumber { get; set; }


        [JsonProperty("cancelDate")]
        public string CancelDate { get; set; }


        [JsonProperty("status")]
        public string Status { get; set; }


        [JsonProperty("message")]
        public string Message { get; set; }
    }
}
