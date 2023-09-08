using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace CMC.Kernel.Core.Helpers
{
    public class Security
    {
        private static byte[] GetIV()
        {
            return new byte[]   {
                55, 34, 87, 64, 87, 195, 54, 21 , 44, 75,
                          35, 86, 142, 95, 57, 21 };
        }
        private static byte[] IV = GetIV();

        public static string Encrypt(string plainText)
        {
            string key = "0CA736318B1A4090942E657A0EFE315M";
            byte[] EncryptKey = { };
            EncryptKey = System.Text.Encoding.UTF8.GetBytes(key);
            AesCryptoServiceProvider aes = new AesCryptoServiceProvider();
            byte[] inputByte = Encoding.UTF32.GetBytes(plainText);
            MemoryStream mStream = new MemoryStream();
            CryptoStream cStream = new CryptoStream(mStream, aes.CreateEncryptor(EncryptKey, IV), CryptoStreamMode.Write);
            cStream.Write(inputByte, 0, inputByte.Length);
            cStream.FlushFinalBlock();
            return Convert.ToBase64String(mStream.ToArray());
        }
        /// <summary>
        /// To Decrypt values in the system takes an encrypted Text
        /// </summary>
        /// <param name="encryptedText"></param>
        /// <returns></returns>
        public static string Decrypt(string encryptedText)
        {
            try
            {
                string key = "0CA736318B1A4090942E657A0EFE315M";
                byte[] DecryptKey = { };
                byte[] inputByte = new byte[encryptedText.Length];
                DecryptKey = System.Text.Encoding.UTF8.GetBytes(key);
                AesCryptoServiceProvider aes = new AesCryptoServiceProvider();
                inputByte = Convert.FromBase64String(encryptedText);
                MemoryStream ms = new MemoryStream();
                CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(DecryptKey, IV), CryptoStreamMode.Write);
                cs.Write(inputByte, 0, inputByte.Length);
                cs.FlushFinalBlock();
                System.Text.Encoding encoding = System.Text.Encoding.UTF32;
                return encoding.GetString(ms.ToArray());
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public static string Hash(string text)
        {
            return BCrypt.Net.BCrypt.HashPassword(text);
        }
    }
}
