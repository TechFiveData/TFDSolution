using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFDIntegrations.EWayBill.Models.Request
{
    public class SupplierDetails
    {
        public string GSTIN { get; set; }

        public string TradeName { get; set; }

        public string Address1 { get; set; }

        public string Address2 { get; set; }

        public string Place { get; set; }

        public int Pincode { get; set; }

        public int StateCode { get; set; }
    }
}
