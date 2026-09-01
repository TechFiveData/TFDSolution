using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFDIntegrations.EWayBill.Common
{
    public class ErrorResponse
    {
        [JsonProperty("ErrorCode")]
        public string ErrorCode { get; set; }


        [JsonProperty("ErrorMessage")]
        public string ErrorMessage { get; set; }


        [JsonProperty("Status")]
        public int Status { get; set; }
    }
}
