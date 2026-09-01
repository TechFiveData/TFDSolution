using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFDIntegrations.EWayBill.Helpers
{
    public static class TokenManager
    {

        private static string _token;

        private static string _sek;

        private static DateTime _expiry;


        public static void SetToken(
            string token,
            string sek,
            DateTime expiry)
        {
            _token = token;
            _sek = sek;
            _expiry = expiry;
        }


        public static bool IsValid()
        {
            return !string.IsNullOrEmpty(_token)
                &&
                DateTime.Now < _expiry;
        }


        public static string Token
        {
            get { return _token; }
        }


        public static string SEK
        {
            get { return _sek; }
        }
    }
}
