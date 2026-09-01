using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFDIntegrations.EWayBill.Models
{
    public class AuthRequest
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string AppKey { get; set; }
        public string EncodedAppKey { get; set; }
    }
    public class AuthResponse
    {
        public string Status { get; set; }
        public string AuthToken { get; set; }
        public string Sek { get; set; }
        public string TokenExpiry { get; set; }
        public string ErrorMessage { get; set; }
    }
    public class ApiResult<T>
    {
        public bool Success { get; set; }

        public string Message { get; set; }

        public T Data { get; set; }

        public string RawResponse { get; set; }
    }
}
