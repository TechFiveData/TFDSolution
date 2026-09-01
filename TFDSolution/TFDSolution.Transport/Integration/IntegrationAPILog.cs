using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFDSolution.Transport.Integration
{
    public class IntegrationAPILog
    {

        public string FormId { get; set; }

        public string CreatedBy { get; set; }

        public int? ParentId { get; set; }

        public string IntegrationType { get; set; }

        public string Reference { get; set; }

        public string ApiName { get; set; }

        public string RequestJson { get; set; }

        public string ResponseJson { get; set; }

        public int? HttpStatus { get; set; }

        public bool? Success { get; set; }

        public string ErrorCode { get; set; }

        public string ErrorMessage { get; set; }

        public DateTime? CreatedOn { get; set; }
    }
}
