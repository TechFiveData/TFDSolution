using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFDIntegrations.EWayBill.Common
{
    /// <summary>
    /// Generic API response wrapper.
    /// </summary>
    public class ApiResponse<T>
    {
        [JsonProperty("Status")]
        public int Status { get; set; }


        [JsonProperty("ErrorCode")]
        public string ErrorCode { get; set; }


        [JsonProperty("ErrorMessage")]
        public string ErrorMessage { get; set; }


        [JsonProperty("Data")]
        public T Data { get; set; }


        public bool IsSuccess
        {
            get
            {
                return Status == 1;
            }
        }
    }
}
