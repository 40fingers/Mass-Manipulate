using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using System.Diagnostics;

namespace FortyFingers.Library
{
    public static class EncryptionUtils
    {

        // 8 bytes randomly selected for both the Key and the Initialization Vector
        // the IV is used to encrypt the first block of text so that any repetitive 
        // patterns are not apparent
        private static byte[] KEY_64 = new System.Text.UTF8Encoding().GetBytes("4Of1nGr5");

        private static byte[] IV_64 = new System.Text.UTF8Encoding().GetBytes("t1M5t3P3");

        // 24 byte or 192 bit key and IV for TripleDES
        private static byte[] KEY_192 = new System.Text.UTF8Encoding().GetBytes("4Of1nG3r5 W3b 50luT10N5!");

        private static byte[] IV_192 = new System.Text.UTF8Encoding().GetBytes("t1M0_P3T3r_5T3F@n_$)FGr5");

        // Standard DES encryption
        public static string Encrypt(string value)
        {
            if ((value != ""))
            {
                DESCryptoServiceProvider cryptoProvider = new DESCryptoServiceProvider();
                MemoryStream ms = new MemoryStream();
                CryptoStream cs = new CryptoStream(ms, cryptoProvider.CreateEncryptor(KEY_64, IV_64), CryptoStreamMode.Write);
                StreamWriter sw = new StreamWriter(cs);
                sw.Write(value);
                sw.Flush();
                cs.FlushFinalBlock();
                ms.Flush();
                // convert back to a string
                return Convert.ToBase64String(ms.GetBuffer(), 0, (int)ms.Length);
            }
            return value;
        }

        // Standard DES decryption
        public static string Decrypt(string value)
        {
            if ((value != ""))
            {
                DESCryptoServiceProvider cryptoProvider = new DESCryptoServiceProvider();
                // convert from string to byte array
                byte[] buffer = Convert.FromBase64String(value);
                MemoryStream ms = new MemoryStream(buffer);
                CryptoStream cs = new CryptoStream(ms, cryptoProvider.CreateDecryptor(KEY_64, IV_64), CryptoStreamMode.Read);
                StreamReader sr = new StreamReader(cs);
                return sr.ReadToEnd();
            }
            return value;
        }

        // TRIPLE DES encryption
        public static string EncryptTripleDES(string value)
        {
            if ((value != ""))
            {
                TripleDESCryptoServiceProvider cryptoProvider = new TripleDESCryptoServiceProvider();
                MemoryStream ms = new MemoryStream();
                CryptoStream cs = new CryptoStream(ms, cryptoProvider.CreateEncryptor(KEY_192, IV_192), CryptoStreamMode.Write);
                StreamWriter sw = new StreamWriter(cs);
                sw.Write(value);
                sw.Flush();
                cs.FlushFinalBlock();
                ms.Flush();
                // convert back to a string
                return Convert.ToBase64String(ms.GetBuffer(), 0, (int)ms.Length);
            }
            return value;
        }

        // TRIPLE DES decryption
        public static string DecryptTripleDES(string value)
        {
            if ((value != ""))
            {
                TripleDESCryptoServiceProvider cryptoProvider = new TripleDESCryptoServiceProvider();
                // convert from string to byte array
                byte[] buffer = Convert.FromBase64String(value);
                MemoryStream ms = new MemoryStream(buffer);
                CryptoStream cs = new CryptoStream(ms, cryptoProvider.CreateDecryptor(KEY_192, IV_192), CryptoStreamMode.Read);
                StreamReader sr = new StreamReader(cs);
                return sr.ReadToEnd();
            }
            return value;
        }
    }
}