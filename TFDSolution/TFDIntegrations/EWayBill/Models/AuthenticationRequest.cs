using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFDIntegrations.EWayBill.Models
{
    /// <summary>
    /// Request model for NIC E-Way Bill API authentication.
    /// </summary>
    public class AuthenticationRequest
    {
        /// <summary>
        /// API action.
        /// Example: ACCESSTOKEN
        /// </summary>
        [JsonProperty("action")]
        public string Action { get; set; }


        /// <summary>
        /// E-Way Bill API username.
        /// </summary>
        [JsonProperty("username")]
        public string Username { get; set; }


        /// <summary>
        /// E-Way Bill API password.
        /// </summary>
        [JsonProperty("password")]
        public string Password { get; set; }


        /// <summary>
        /// Application key used during authentication flow.
        /// </summary>
        [JsonProperty("app_key")]
        public string AppKey { get; set; }
    }
}
