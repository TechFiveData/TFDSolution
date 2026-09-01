using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TFDIntegrations.EInvoiceBill.Model;

namespace TFDIntegrations.EWayBill.Models
{
    public class AuthTokenResponse
    {
        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("authtoken")]
        public string AuthToken { get; set; }

        [JsonProperty("sek")]
        public string Sek { get; set; }

        [JsonProperty("ErrorDetails")]
        public ErrorDetails ErrorDetails { get; set; }

        [JsonProperty("InfoDtls")]
        public object InfoDtls { get; set; }
    }
    /// <summary>
    /// Response model returned by NIC authentication API.
    /// </summary>
    public class AuthenticationResponse
    {
        [JsonProperty("Status")]
        public int Status { get; set; }

        [JsonProperty("Data")]
        public AuthenticationData Data { get; set; }

        [JsonProperty("ErrorDetails")]
        public ErrorDetails ErrorDetails { get; set; }

        [JsonProperty("InfoDtls")]
        public object InfoDtls { get; set; }
    }

    /// <summary>
    /// Authentication success response data.
    /// </summary>
    public class AuthenticationData
    {
        [JsonProperty("ClientId")]
        public string ClientId { get; set; }

        [JsonProperty("UserName")]
        public string UserName { get; set; }

        [JsonProperty("AuthToken")]
        public string AuthToken { get; set; }

        [JsonProperty("Sek")]
        public string Sek { get; set; }

        [JsonProperty("TokenExpiry")]
        public DateTime TokenExpiry { get; set; }
    }

    public class ErrorDetails
    {
        [JsonProperty("ErrorCode")]
        public string ErrorCode { get; set; }

        [JsonProperty("ErrorMessage")]
        public string ErrorMessage { get; set; }
    }
}