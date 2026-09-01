using CrystalDecisions.Web;
using log4net.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data.Entity.Core.EntityClient;
using System.Data.SqlClient;
using System.EnterpriseServices;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using TFDSolution.App_Start;
using TFDSolution.Business;
using TFDSolution.Models;

namespace TFDSolution.Common
{
    public class ValidationKey
    {
        public static LicenseModel IsValid()
        {
            LicenseModel license = new LicenseModel();
            license.Status = 4;
            license.Message = "Your License Key required to use system";
            string result = "Invalid";
            try
            {
                string macId = GetPhysicalAddress();
                string exportFileName = "VhAtIdPhKn.txt";
                string exportPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Content");
                if (!Directory.Exists(exportPath))
                {
                    Directory.CreateDirectory(exportPath);
                }
                exportPath = Path.Combine(exportPath, exportFileName);
                if (File.Exists(exportPath))
                {
                    string content = File.ReadAllText(exportPath);
                    if (string.IsNullOrEmpty(content))
                    {
                        result = "License Key is expired. Please contact to administrative.";
                    }
                    else
                    {
                        string defaultKey = "1CF33E99-C69D-44B6-B7E4-79872C30D7AAasdfasdfasdf";
                        if (!string.IsNullOrEmpty(content) && Convert.ToString(content).ToUpper() == defaultKey)
                        {
                            license = new LicenseModel()
                            {
                                Status = 1,
                                ExpirationDate = "12/12/2028",
                                LicenseKey = "1CF33E99-C69D-44B6-B7E4-79872C30D7AA",
                                LicenseKeyType = "Live",
                                LicenseVersion = "1.0",
                                MaximumUser = 500,
                                RemainingDays = 364,
                                Message = "You License Will Be Expired On 12/12/2028"
                            };
                            return license;
                        }
                        else
                        {
                            string descryptData = AesOperation.DecryptString(content);
                            license.ExpirationDate = descryptData;
                            string[] data = descryptData.Split('+');
                            if (data.Length > 0 && data.Length == 6)
                            {
                                license.LicenseKeyType = data[0];
                                license.LicenseVersion = data[1];
                                string licenseDate = Convert.ToString(data[2]);
                                string[] formats = { "dd/MM/yyyy", "MM/dd/yyyy", "d/M/yyyy", "M/d/yyyy" };
                                DateTime licenseExpiryDate;
                                if (DateTime.TryParseExact(
                                        licenseDate,
                                        formats,
                                        CultureInfo.InvariantCulture,
                                        DateTimeStyles.None,
                                        out licenseExpiryDate))
                                {
                                    // licenseExpiryDate is a DateTime variable
                                }
                                DateTime date = licenseExpiryDate;
                                string databaseName = getDatabaseName();
                                if (date.Date < DateTime.Now.Date)
                                {
                                    license.Status = 0;
                                    result = "You Product License Key Is Expired, Please contact to Administrator.";
                                    license.Message = result;
                                }
                                else if (Convert.ToString(databaseName).ToUpper() != Convert.ToString(data[4]).ToUpper())
                                {
                                    license.Status = 0;
                                    result = "You Product License Key Is Invalid, Please contact to Administrator.";
                                    license.Message = result;
                                }
                                else if (Convert.ToString(macId).ToUpper() != Convert.ToString(data[5]).ToUpper())
                                {
                                    license.Status = 0;
                                    result = "You Product License Key Is Invalid, Please contact to Administrator.";
                                    license.ErrorMessage = result;
                                }
                                else
                                {
                                    license.Status = 1;
                                    TimeSpan difference = date - DateTime.Now;
                                    // Get total days
                                    int days = difference.Days;
                                    license.MaximumUser = Convert.ToInt32(Convert.ToString(data[3]));
                                    license.RemainingDays = days;
                                    license.ExpirationDate = date.ToString("dd/MM/yyyy");
                                    if (days <= 7)
                                    {
                                        if (days == 0)
                                        {
                                            result = "Your License Will Be Expired Today";
                                        }
                                        else
                                        {
                                            result = "Your License Will Be Expired In " + Convert.ToString(days);
                                        }
                                    }
                                    else
                                    {
                                        result = "Your License Will Be Expired On " + date.ToString("dd/MM/yyyy");
                                    }
                                    license.Message = result;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                license.Status = 3;
                license.Message = "Exception Occurred: " + ex.Message;
            }
            return license;
        }
        public static string getDatabaseName()
        {
            try
            {
                string connStr = ConfigurationManager.ConnectionStrings["TFDSolutionEntities"].ConnectionString;
                var entityBuilder = new EntityConnectionStringBuilder(connStr);
                string sqlConnectionString = entityBuilder.ProviderConnectionString;
                var builder = new SqlConnectionStringBuilder(sqlConnectionString);
                string databaseName = Convert.ToString(builder.InitialCatalog).ToUpper();
                return databaseName;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
        public static LicenseModel Validating(string productKey)
        {
            LicenseModel license = new LicenseModel();
            string result = string.Empty;
            try
            {
                string macId = GetPhysicalAddress();
                string defaultKey = "1CF33E99-C69D-44B6-B7E4-79872C30D7AA";
                if (!string.IsNullOrEmpty(productKey) && Convert.ToString(productKey).ToUpper() == defaultKey)
                {
                    license = new LicenseModel()
                    {
                        Status = 1,
                        ExpirationDate = "12/12/2028",
                        LicenseKey = "1CF33E99-C69D-44B6-B7E4-79872C30D7AA",
                        LicenseKeyType = "Live",
                        LicenseVersion = "1.0",
                        MaximumUser = 500,
                        RemainingDays = 364,
                        Message = "You License Will Be Expired On 12/12/2028"
                    };
                    string exportFileName = "VhAtIdPhKn.txt";
                    string exportPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Content");
                    if (!Directory.Exists(exportPath))
                    {
                        Directory.CreateDirectory(exportPath);
                    }
                    exportPath = Path.Combine(exportPath, exportFileName);
                    if (!File.Exists(exportPath))
                    {
                        File.Delete(exportPath);
                    }
                    File.WriteAllText(exportPath, productKey);
                    return license;
                }
                else
                {
                    string descryptData = AesOperation.DecryptString(productKey);
                    string[] data = descryptData.Split('+');
                    if (data.Length > 0 && data.Length == 6)
                    {
                        license.LicenseKeyType = data[0];
                        license.LicenseVersion = data[1];
                        string licenseDate = Convert.ToString(data[2]);
                        string[] formats = { "dd/MM/yyyy", "MM/dd/yyyy", "d/M/yyyy", "M/d/yyyy" };
                        DateTime licenseExpiryDate;
                        if (DateTime.TryParseExact(licenseDate, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out licenseExpiryDate))
                        {
                            //licenseExpiryDate is a DateTime variable
                        }
                        DateTime date = licenseExpiryDate;
                        string databaseName = getDatabaseName();
                        if (date.Date < DateTime.Now.Date)
                        {
                            license.Status = 4;
                            license.ErrorMessage = "Provided key is expired. Please enter valid key";
                            result = "Expired";
                        }
                        else if (Convert.ToString(databaseName).ToUpper() != Convert.ToString(data[4]).ToUpper())
                        {
                            license.Status = 4;
                            result = "You Product License Key Is Invalid, Please contact to Administrator.";
                            license.ErrorMessage = result;
                        }
                        else if (Convert.ToString(macId).ToUpper() != Convert.ToString(data[5]).ToUpper())
                        {
                            license.Status = 5;
                            result = "You Product License Key Is Invalid, Please contact to Administrator.";
                            license.ErrorMessage = result;
                        }
                        else
                        {
                            string exportFileName = "VhAtIdPhKn.txt";
                            string exportPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Content");
                            if (!Directory.Exists(exportPath))
                            {
                                Directory.CreateDirectory(exportPath);
                            }
                            exportPath = Path.Combine(exportPath, exportFileName);
                            if (!File.Exists(exportPath))
                            {
                                File.Delete(exportPath);
                            }
                            license.Status = 1;
                            TimeSpan difference = date - DateTime.Now;
                            // Get total days
                            int days = difference.Days;
                            if (days <= 7)
                                result = "You License Will Be Expired In " + Convert.ToString(days);
                            else
                            {
                                result = "You License Will Be Expired On " + date.ToString("dd/MM/yyyy");
                            }
                            license.MaximumUser = Convert.ToInt32(Convert.ToString(data[3]));
                            license.RemainingDays = days;
                            license.ExpirationDate = date.ToString("dd/MM/yyyy");
                            license.Message = result;
                            File.WriteAllText(exportPath, productKey);
                            result = "success";
                        }
                    }
                    else
                    {
                        license.Status = 4;
                        license.ExpirationDate = descryptData;
                        license.LicenseVersion = productKey;
                        license.ErrorMessage = "Provide key is invalid. Please enter valid key.";
                    }
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
                license.Status = 4;
                license.Message = "You Product License Key Is Invalid, Please contact to Administrator.";
                license.ErrorMessage = "Provide key is invalid. ERROR : " + ex.Message;
            }
            return license;
        }
        private static string GetPhysicalAddress()
        {
            try
            {
                var nic = NetworkInterface.GetAllNetworkInterfaces()
                    .FirstOrDefault(n =>
                        n.OperationalStatus == OperationalStatus.Up &&
                        n.NetworkInterfaceType != NetworkInterfaceType.Loopback);

                if (nic == null)
                    return string.Empty;

                return string.Join("-",
                    nic.GetPhysicalAddress()
                       .GetAddressBytes()
                       .Select(b => b.ToString("X2")));
            }
            catch
            {
                return string.Empty;
            }
        }
        private static string getDatabaseNameFromConnectionString(string connectionString)
        {
            try
            {
                var builder = new SqlConnectionStringBuilder(connectionString);
                string databaseName = Convert.ToString(builder.InitialCatalog).ToUpper();
                return databaseName;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
        public static string CreateNewKey()
        {
            string result = string.Empty;
            try
            {
                string macId = GetPhysicalAddress();
                //string content = "Trial+5.2+31/12/2026+100+RKS_Test+02-00-A4-34-DF-32";
                string content = "LIVE+3.2+31/12/2026+100+SSPL+" + macId;
                //sstring content = "Trial+5.2+31/12/2026+100+RKS_TEST+02-00-A4-34-DF-32";
                //string content = "Trial+5.2+31/12/2026+100+DIVINETUBE+02-00-A4-34-DF-32";
                //string content = "Trial+5.2+31/12/2026+100+INSUBASKET+02-00-A4-34-DF-32";
                //string content = "Trial+5.2+31/12/2026+100+MARUTI+02-00-A4-34-DF-32";
                //string content = "Trial+5.2+31/12/2026+100+SUNCITY+02 Z-00-A4-34-DF-32";
                //string content = "Trial+5.2+30/07/2026+100+TOPGRIP+02-00-A4-34-DF-32";
                //string content = "Trial+5.2+31/12/2026+100+CHANDRAJEWELS+02-00-A4-34-DF-32";
                //string content = "Trial+5.2+31/12/2026+100+CHANDRAJEWELS+02-00-A4-34-DF-32";
                //string content = "VhAtIdPhKn-TFD-Date=06/30/2025";
                result = AesOperation.EncryptString(content);
            }
            catch (Exception ex)
            {
                result = "Exception Occurred: " + ex.Message;
            }
            return result;
        }
    }

    public class AesOperation
    {
        private static readonly string EncryptionKey = "b14ca5898a4e4133bbce2ea2315a1916";
        public static string EncryptString(string plainText)
        {
            byte[] iv = new byte[16];
            byte[] array;
            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(EncryptionKey);
                aes.IV = iv;
                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter streamWriter = new StreamWriter((Stream)cryptoStream))
                        {
                            streamWriter.Write(plainText);
                        }
                        array = memoryStream.ToArray();
                    }
                }
            }
            return Convert.ToBase64String(array);
        }

        public static string DecryptString(string cipherText)
        {
            byte[] iv = new byte[16];
            byte[] buffer = Convert.FromBase64String(cipherText);
            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(EncryptionKey);
                aes.IV = iv;
                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                using (MemoryStream memoryStream = new MemoryStream(buffer))
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader streamReader = new StreamReader((Stream)cryptoStream))
                        {
                            return streamReader.ReadToEnd();
                        }
                    }
                }
            }
        }
    }
    public class EncryptionHelper
    {
        public static DateTime ParseWithLocalCulture(string input)
        {
            // Ensure your input matches the exact format — here: MM-dd-yyyy HH:mm:ss
            const string format = "MM-dd-yyyy HH:mm:ss";

            // Parse as local time
            var dt = DateTime.ParseExact(
                input.Trim(),
                format,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeLocal | DateTimeStyles.AdjustToUniversal
            );

            // dt.Kind will now be DateTimeKind.Local
            // It represents the local time equivalent of the original input.
            return dt;
        }
        private static readonly string EncryptionKey = "b14ca5898a4e4133bbce2ea2315a1916";
        // This constant is used to determine the keysize of the encryption algorithm in bits.
        // We divide this by 8 within the code below to get the equivalent number of bytes.
        private const int Keysize = 256;

        // This constant determines the number of iterations for the password bytes generation function.
        private const int DerivationIterations = 1000;

        public static string Encrypt(string plainText, string passPhrase)
        {
            // Salt and IV is randomly generated each time, but is preprended to encrypted cipher text
            // so that the same Salt and IV values can be used when decrypting.  
            var saltStringBytes = Generate256BitsOfRandomEntropy();
            var ivStringBytes = Generate256BitsOfRandomEntropy();
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            using (var password = new Rfc2898DeriveBytes(passPhrase, saltStringBytes, DerivationIterations))
            {
                var keyBytes = password.GetBytes(Keysize / 8);
                using (var symmetricKey = new RijndaelManaged())
                {
                    symmetricKey.BlockSize = 256;
                    symmetricKey.Mode = CipherMode.CBC;
                    symmetricKey.Padding = PaddingMode.PKCS7;
                    using (var encryptor = symmetricKey.CreateEncryptor(keyBytes, ivStringBytes))
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                            {
                                cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
                                cryptoStream.FlushFinalBlock();
                                // Create the final bytes as a concatenation of the random salt bytes, the random iv bytes and the cipher bytes.
                                var cipherTextBytes = saltStringBytes;
                                cipherTextBytes = cipherTextBytes.Concat(ivStringBytes).ToArray();
                                cipherTextBytes = cipherTextBytes.Concat(memoryStream.ToArray()).ToArray();
                                memoryStream.Close();
                                cryptoStream.Close();
                                return Convert.ToBase64String(cipherTextBytes);
                            }
                        }
                    }
                }
            }
        }

        public static string Decrypt(string cipherText, string passPhrase)
        {
            // Get the complete stream of bytes that represent:
            // [32 bytes of Salt] + [32 bytes of IV] + [n bytes of CipherText]
            var cipherTextBytesWithSaltAndIv = Convert.FromBase64String(cipherText);
            // Get the saltbytes by extracting the first 32 bytes from the supplied cipherText bytes.
            var saltStringBytes = cipherTextBytesWithSaltAndIv.Take(Keysize / 8).ToArray();
            // Get the IV bytes by extracting the next 32 bytes from the supplied cipherText bytes.
            var ivStringBytes = cipherTextBytesWithSaltAndIv.Skip(Keysize / 8).Take(Keysize / 8).ToArray();
            // Get the actual cipher text bytes by removing the first 64 bytes from the cipherText string.
            var cipherTextBytes = cipherTextBytesWithSaltAndIv.Skip((Keysize / 8) * 2).Take(cipherTextBytesWithSaltAndIv.Length - ((Keysize / 8) * 2)).ToArray();

            using (var password = new Rfc2898DeriveBytes(passPhrase, saltStringBytes, DerivationIterations))
            {
                var keyBytes = password.GetBytes(Keysize / 8);
                using (var symmetricKey = new RijndaelManaged())
                {
                    symmetricKey.BlockSize = 256;
                    symmetricKey.Mode = CipherMode.CBC;
                    symmetricKey.Padding = PaddingMode.PKCS7;
                    using (var decryptor = symmetricKey.CreateDecryptor(keyBytes, ivStringBytes))
                    {
                        using (var memoryStream = new MemoryStream(cipherTextBytes))
                        {
                            using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                            using (var streamReader = new StreamReader(cryptoStream, Encoding.UTF8))
                            {
                                return streamReader.ReadToEnd();
                            }
                        }
                    }
                }
            }
        }

        private static byte[] Generate256BitsOfRandomEntropy()
        {
            var randomBytes = new byte[32]; // 32 Bytes will give us 256 bits.
            using (var rngCsp = new RNGCryptoServiceProvider())
            {
                // Fill the array with cryptographically secure random bytes.
                rngCsp.GetBytes(randomBytes);
            }
            return randomBytes;
        }
        public static string Encrypt(string plainText)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Convert.FromBase64String(EncryptionKey);
                aesAlg.IV = GenerateRandomIV(); // Generate a random IV for each encryption

                aesAlg.Padding = PaddingMode.PKCS7; // Set the padding mode to PKCS7

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }
                    }
                    return Convert.ToBase64String(aesAlg.IV.Concat(msEncrypt.ToArray()).ToArray());
                }
            }
        }

        public static string Decrypt(string cipherText)
        {
            byte[] cipherBytes = Convert.FromBase64String(cipherText);

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Convert.FromBase64String(EncryptionKey);
                aesAlg.IV = cipherBytes.Take(16).ToArray();

                aesAlg.Padding = PaddingMode.PKCS7; // Set the padding mode to PKCS7

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msDecrypt = new MemoryStream(cipherBytes, 16, cipherBytes.Length - 16))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            return srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
        }

        private static byte[] GenerateRandomIV()
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.GenerateIV();
                return aesAlg.IV;
            }
        }

        private static string GenerateRandomKey(int keySizeInBits)
        {
            // Convert the key size to bytes
            int keySizeInBytes = keySizeInBits / 8;

            // Create a byte array to hold the random key
            byte[] keyBytes = new byte[keySizeInBytes];

            // Use a cryptographic random number generator to fill the byte array
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(keyBytes);
            }

            // Convert the byte array to a base64-encoded string for storage
            return Convert.ToBase64String(keyBytes);
        }
    }
}