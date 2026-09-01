using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace TFDSolution.Models
{
    public class LicenseModel
    {
        public string LicenseKeyType { get; set; }
        public string LicenseVersion { get; set; }
        public string LicenseKey { get; set; }
        public int MaximumUser { get; set; }
        public int RemainingDays { get; set; }
        public string ExpirationDate { get; set; }
        public int Status { get; set; }
        public string Message { get; set; }
        public string ErrorMessage { get; set; }
        
    }
}