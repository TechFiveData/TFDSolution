using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TFDIntegrations.EWayBill.Helpers;

namespace TFDIntegrations.EWayBill.Services
{
    /// <summary>
    /// Handles E-Way Bill API payload encryption/decryption.
    /// //This service is responsible for converting the API JSON payload into an encrypted payload and decrypting the API response.
    /// </summary>
    public class EncryptionService
    {
        /// <summary>
        /// Encrypts JSON payload using SEK.
        /// </summary>
        public string EncryptRequest(string jsonData, string sek)
        {
            try
            {
                if (string.IsNullOrEmpty(jsonData))
                    throw new ArgumentNullException(nameof(jsonData));


                if (string.IsNullOrEmpty(sek))
                    throw new ArgumentNullException(nameof(sek));


                byte[] key =
                    Convert.FromBase64String(sek);


                byte[] iv =
                    new byte[16];


                return AESHelper.Encrypt(
                    jsonData,
                    key,
                    iv);
            }
            catch (Exception ex)
            {
                throw new Exception("Encryption failed: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Decrypts API encrypted response.
        /// </summary>
        public string DecryptResponse(string encryptedData, string sek)
        {
            try
            {
                if (string.IsNullOrEmpty(encryptedData))
                    throw new ArgumentNullException(nameof(encryptedData));


                byte[] key =
                    Convert.FromBase64String(sek);


                byte[] iv =
                    new byte[16];


                return AESHelper.Decrypt(
                    encryptedData,
                    key,
                    iv);
            }
            catch (Exception ex)
            {
                throw new Exception(
                    "Decryption failed: "
                    + ex.Message,
                    ex);
            }
        }

        /// <summary>
        /// Converts object to encrypted API payload.
        /// </summary>
        public string EncryptObject<T>(T model, string sek)
        {
            string json = JsonHelper.Serialize(model);
            return json;
        }



        /// <summary>
        /// Decrypts API response into object.
        /// </summary>
        public T DecryptObject<T>(
            string encryptedData,
            string sek)
        {
            string json = DecryptResponse(
                    encryptedData,
                    sek);


            return JsonHelper.Deserialize<T>(
                json);
        }
    }
}
