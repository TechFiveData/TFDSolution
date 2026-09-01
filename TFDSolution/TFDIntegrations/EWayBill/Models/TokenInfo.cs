using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TFDIntegrations.EInvoiceBill.Model;
using TFDIntegrations.EWayBill.Common;
using TFDIntegrations.EWayBill.Models.Response;

namespace TFDIntegrations.EWayBill.Models
{
    /// <summary>
    /// Stores NIC E-Way Bill authentication session information.
    /// </summary>
    public class TokenInfo : EWayErrorResponse
    {
        /// <summary>
        /// GSTIN for which token was generated.
        /// </summary>
        public string Gstin { get; set; }


        /// <summary>
        /// API username.
        /// </summary>
        public string Username { get; set; }


        /// <summary>
        /// Authentication token returned by NIC.
        /// </summary>
        public string AuthToken { get; set; }


        /// <summary>
        /// Session encryption key returned by NIC.
        /// Used for payload encryption/decryption.
        /// </summary>
        public string Sek { get; set; }


        /// <summary>
        /// Token creation time.
        /// </summary>
        public DateTime CreatedOn { get; set; }


        /// <summary>
        /// Token expiry time.
        /// </summary>
        public DateTime ExpiryOn { get; set; }


        /// <summary>
        /// Checks whether the current token has expired.
        /// </summary>
        public bool IsExpired()
        {
            return DateTime.Now >= ExpiryOn;
        }


        /// <summary>
        /// Returns remaining token validity.
        /// </summary>
        public TimeSpan RemainingValidity()
        {
            if (IsExpired())
            {
                return TimeSpan.Zero;
            }

            return ExpiryOn - DateTime.Now;
        }
    }
}
