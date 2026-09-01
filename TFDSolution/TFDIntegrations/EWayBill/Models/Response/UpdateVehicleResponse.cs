using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFDIntegrations.EWayBill.Models.Response
{
    public class UpdateVehicleResponse
    {
        [JsonProperty("ewayBillNo")]
        public string EWayBillNumber { get; set; }


        [JsonProperty("vehicleNo")]
        public string VehicleNumber { get; set; }


        [JsonProperty("updateDate")]
        public string UpdateDate { get; set; }


        [JsonProperty("validUpto")]
        public string ValidUpto { get; set; }


        [JsonProperty("message")]
        public string Message { get; set; }
    }
}
