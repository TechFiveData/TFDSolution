using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFDIntegrations.EWayBill.Models.Request
{
    /// <summary>
    /// Request model for fetching E-Way Bill details.
    /// </summary>
    public class GetEWayBillRequest
    {
        [JsonProperty("ewbNo")]
        public long EWayBillNumber { get; set; }
    }
}
