using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFDIntegrations.EWayBill.Models.Request
{
    /// <summary>
    /// Request model for consolidated E-Way Bill generation.
    /// </summary>
    public class ConsolidatedEWayBillRequest
    {
        [JsonProperty("transMode")]
        public string TransportMode { get; set; }


        [JsonProperty("fromPlace")]
        public string FromPlace { get; set; }


        [JsonProperty("fromState")]
        public int FromState { get; set; }


        [JsonProperty("vehicleNo")]
        public string VehicleNumber { get; set; }


        [JsonProperty("ewbNo")]
        public List<long> EWayBillNumbers { get; set; }
    }
}
