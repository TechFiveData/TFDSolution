using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFDIntegrations.EInvoiceBill.Model
{
    public class IRNDetails
    {
        public int InvoiceId { get; set; }

        public string IRN { get; set; }

        public string AckNo { get; set; }

        public DateTime AckDate { get; set; }

        public string SignedQRCode { get; set; }

        public string SignedInvoice { get; set; }

        public string Status { get; set; }
    }
}
