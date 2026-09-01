using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFDIntegrations.EInvoiceBill.Model
{
    public class AuthResponse
    {
        [JsonProperty("Status")]
        public int Status { get; set; }


        [JsonProperty("ClientId")]
        public string ClientId { get; set; }


        [JsonProperty("UserName")]
        public string UserName { get; set; }


        [JsonProperty("AuthToken")]
        public string AuthToken { get; set; }


        [JsonProperty("Sek")]
        public string Sek { get; set; }


        [JsonProperty("TokenExpiry")]
        public string TokenExpiry { get; set; }
    }
}
