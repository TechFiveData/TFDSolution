using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFDSolution.Transport.Integration
{
    public class IntegrationConfiguration
    {
        public string GSTIN { get; set; }
        public string EWayBill_AspId { get; set; }
        public string EWayBill_AspPassword { get; set; }
        public string EWayBill_EWBUsername { get; set; }
        public string EWayBill_EWBPassword { get; set; }
        public string CompanyName { get; set; }
    }
}
