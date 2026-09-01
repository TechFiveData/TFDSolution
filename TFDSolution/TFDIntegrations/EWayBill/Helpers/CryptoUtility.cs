using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace TFDIntegrations.EWayBill.Helpers
{
    public static class CryptoHelper
    {
        /// <summary>
        /// Converts byte array to Base64.
        /// </summary>
        public static string ToBase64(byte[] data)
        {
            return Convert.ToBase64String(data);
        }


        /// <summary>
        /// Converts Base64 string to bytes.
        /// </summary>
        public static byte[] FromBase64(string value)
        {
            return Convert.FromBase64String(value);
        }


        /// <summary>
        /// SHA256 hash.
        /// </summary>
        public static string SHA256Hash(string input)
        {
            using (SHA256 sha =
                SHA256.Create())
            {
                byte[] bytes =
                    Encoding.UTF8.GetBytes(input);

                byte[] hash =
                    sha.ComputeHash(bytes);


                return Convert.ToBase64String(hash);
            }
        }


        /// <summary>
        /// Generates random application key.
        /// </summary>
        public static byte[] GenerateAppKey()
        {
            byte[] key = new byte[32];

            using (RandomNumberGenerator rng =
                RandomNumberGenerator.Create())
            {
                rng.GetBytes(key);
            }

            return key;
        }
    }
    public static class CryptoUtility
    {
        public static byte[] GenerateRandomBytes(int length)
        {
            byte[] bytes = new byte[length];

            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }

            return bytes;
        }

        public static string GenerateAppKey()
        {
            return Convert.ToBase64String(
                GenerateRandomBytes(32));
        }
    }
}
