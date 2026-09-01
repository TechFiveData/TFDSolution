using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace TFDIntegrations.EWayBill.Helpers
{
    public static class AESHelper
    {
        /// <summary>
        /// Encrypts plain text using AES.
        /// </summary>
        public static string Encrypt(
            string plainText,
            byte[] key,
            byte[] iv)
        {
            if (string.IsNullOrEmpty(plainText))
                throw new ArgumentNullException(nameof(plainText));

            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream =
                        new CryptoStream(
                            memoryStream,
                            aes.CreateEncryptor(),
                            CryptoStreamMode.Write))
                    {
                        byte[] data =
                            Encoding.UTF8.GetBytes(plainText);

                        cryptoStream.Write(
                            data,
                            0,
                            data.Length);

                        cryptoStream.FlushFinalBlock();
                    }

                    return Convert.ToBase64String(
                        memoryStream.ToArray());
                }
            }
        }


        /// <summary>
        /// Decrypts AES encrypted text.
        /// </summary>
        public static string Decrypt(
            string cipherText,
            byte[] key,
            byte[] iv)
        {
            if (string.IsNullOrEmpty(cipherText))
                throw new ArgumentNullException(nameof(cipherText));


            byte[] buffer =
                Convert.FromBase64String(cipherText);


            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;


                using (MemoryStream memoryStream =
                    new MemoryStream(buffer))
                {
                    using (CryptoStream cryptoStream =
                        new CryptoStream(
                            memoryStream,
                            aes.CreateDecryptor(),
                            CryptoStreamMode.Read))
                    {
                        using (StreamReader reader =
                            new StreamReader(cryptoStream))
                        {
                            return reader.ReadToEnd();
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Generates random AES key.
        /// </summary>
        public static byte[] GenerateKey(int size = 32)
        {
            byte[] key = new byte[size];

            using (RandomNumberGenerator rng =
                RandomNumberGenerator.Create())
            {
                rng.GetBytes(key);
            }

            return key;
        }


        /// <summary>
        /// Generates random IV.
        /// </summary>
        public static byte[] GenerateIV()
        {
            byte[] iv = new byte[16];

            using (RandomNumberGenerator rng =
                RandomNumberGenerator.Create())
            {
                rng.GetBytes(iv);
            }

            return iv;
        }
        public static string Encrypt1(string plainText, byte[] key, byte[] iv)
        {
            using (var aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using (var encryptor = aes.CreateEncryptor())
                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    using (var sw = new StreamWriter(cs))
                    {
                        sw.Write(plainText);
                    }

                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }

        public static string Decryp1t(string cipherText, byte[] key, byte[] iv)
        {
            byte[] buffer = Convert.FromBase64String(cipherText);

            using (var aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using (var decryptor = aes.CreateDecryptor())
                using (var ms = new MemoryStream(buffer))
                using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                using (var sr = new StreamReader(cs))
                {
                    return sr.ReadToEnd();
                }
            }
        }

        public static byte[] GenerateKey()
        {
            byte[] key = new byte[32];

            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(key);
            }

            return key;
        }

        public static byte[] GenerateIV1()
        {
            byte[] iv = new byte[16];

            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(iv);
            }

            return iv;
        }
    }
}