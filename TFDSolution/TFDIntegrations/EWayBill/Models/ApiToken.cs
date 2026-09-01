using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFDIntegrations.EWayBill.Models
{
    public class ApiToken
    {
        public int Id { get; set; }

        public string AuthToken { get; set; }

        public string Sek { get; set; }

        public DateTime ExpiryDate { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}
