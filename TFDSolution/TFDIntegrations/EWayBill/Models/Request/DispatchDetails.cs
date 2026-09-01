using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFDIntegrations.EWayBill.Models.Request
{
    public class DispatchDetails
    {
        public string DispatchFromGSTIN { get; set; }

        public string DispatchFromTradeName { get; set; }

        public string DispatchFromAddress { get; set; }

        public string DispatchFromPlace { get; set; }

        public int DispatchFromPincode { get; set; }

        public int DispatchFromStateCode { get; set; }
    }
}
