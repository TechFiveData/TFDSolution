using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFDIntegrations.EWayBill.Models.Request
{
    /// <summary>
    /// Request model for cancelling an E-Way Bill.
    /// </summary>
    public class CancelEWayBillRequest
    {
        [JsonProperty("ewbNo")]
        public long EWayBillNumber { get; set; }


        [JsonProperty("cancelRsnCode")]
        public int CancellationReasonCode { get; set; }


        [JsonProperty("cancelRmrk")]
        public string CancellationRemark { get; set; }
    }
}
