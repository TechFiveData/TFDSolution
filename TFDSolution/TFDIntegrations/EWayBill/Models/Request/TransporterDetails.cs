using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFDIntegrations.EWayBill.Models.Request
{
    public class TransporterDetails
    {
        [JsonProperty("transporterId")]
        public string TransporterId { get; set; }


        [JsonProperty("transporterName")]
        public string TransporterName { get; set; }


        [JsonProperty("transDocNo")]
        public string TransportDocumentNumber { get; set; }


        [JsonProperty("transDocDate")]
        public string TransportDocumentDate { get; set; }


        [JsonProperty("transMode")]
        public string TransportMode { get; set; }


        [JsonProperty("vehicleNo")]
        public string VehicleNumber { get; set; }


        [JsonProperty("vehicleType")]
        public string VehicleType { get; set; }
    }
}
