using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFDIntegrations.EInvoiceBill.Model
{
    public class ErrorResponse
    {

        [JsonProperty("Status")]
        public int Status { get; set; }


        [JsonProperty("ErrorDetails")]
        public List<ErrorDetail> ErrorDetails { get; set; }

    }


    public class ErrorDetail
    {

        [JsonProperty("ErrorCode")]
        public string ErrorCode { get; set; }


        [JsonProperty("ErrorMessage")]
        public string ErrorMessage { get; set; }

    }
}
