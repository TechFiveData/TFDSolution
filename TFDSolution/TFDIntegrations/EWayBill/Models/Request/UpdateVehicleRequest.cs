using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFDIntegrations.EWayBill.Models.Request
{
    /// <summary>
    /// Request model for updating vehicle details.
    /// </summary>
    public class UpdateVehicleRequest
    {
        [JsonProperty("ewbNo")]
        public long EWayBillNumber { get; set; }


        [JsonProperty("vehicleNo")]
        public string VehicleNumber { get; set; }


        [JsonProperty("fromPlace")]
        public string FromPlace { get; set; }


        [JsonProperty("fromState")]
        public int FromState { get; set; }


        [JsonProperty("remainingDistance")]
        public int RemainingDistance { get; set; }


        [JsonProperty("transMode")]
        public string TransportMode { get; set; }


        [JsonProperty("transDocNo")]
        public string TransportDocumentNumber { get; set; }


        [JsonProperty("transDocDate")]
        public string TransportDocumentDate { get; set; }


        [JsonProperty("vehicleType")]
        public string VehicleType { get; set; }
    }
}
