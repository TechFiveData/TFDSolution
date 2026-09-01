using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFDIntegrations.EInvoiceBill.Model
{
    public class CancelIRNResponse: EInvoiceErrorResponse
    {
        public string Irn { get; set; }
        public string CancelDate { get; set; }

    }
}
