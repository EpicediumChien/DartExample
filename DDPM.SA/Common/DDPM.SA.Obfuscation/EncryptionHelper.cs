using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DDPM.SA.Obfuscation
{
    public class EncryptionHelper
    {
        public static byte[] EncryptJsonToBytes(string json_string, string secretKey)
        {
            byte[] encryptedData = EncryptStringToBytes_Aes2(json_string, secretKey);
            return encryptedData;
        }

        public static string DecryptJsonFromFile(byte[] cipherText, string secretKey)
        {
            byte[] encryptedData = cipherText;
            string decryptedJson = DecryptStringFromBytes_Aes2(encryptedData, secretKey);
            return decryptedJson;
        }

        private static byte[] EncryptStringToBytes_Aes(string plainText, string secretKey)
        {
            using (Aes aesAlg = Aes.Create())
            {
                using (var key = new Rfc2898DeriveBytes(secretKey, new byte[16]))
                {
                    aesAlg.Key = key.GetBytes(aesAlg.KeySize / 8);
                    aesAlg.IV = key.GetBytes(aesAlg.BlockSize / 8);
                }

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }
                        return msEncrypt.ToArray();
                    }
                }
            }
        }

        private static string DecryptStringFromBytes_Aes(byte[] cipherText, string secretKey)
        {
            using (Aes aesAlg = Aes.Create())
            {
                using (var key = new Rfc2898DeriveBytes(secretKey, new byte[16]))
                {
                    aesAlg.Key = key.GetBytes(aesAlg.KeySize / 8);
                    aesAlg.IV = key.GetBytes(aesAlg.BlockSize / 8);
                }

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
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

        private static byte[] EncryptStringToBytes_Aes2(string plainText, string secretKey)
        {
            using (Aes aesAlg = Aes.Create())
            {
                using (var key = new Rfc2898DeriveBytes(secretKey, new byte[16], 10000, HashAlgorithmName.SHA512))
                {
                    aesAlg.Key = key.GetBytes(aesAlg.KeySize / 8);
                    aesAlg.IV = key.GetBytes(aesAlg.BlockSize / 8);
                }

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }
                        return msEncrypt.ToArray();
                    }
                }
            }
        }

        private static string DecryptStringFromBytes_Aes2(byte[] cipherText, string secretKey)
        {
            using (Aes aesAlg = Aes.Create())
            {
                using (var key = new Rfc2898DeriveBytes(secretKey, new byte[16], 10000, HashAlgorithmName.SHA512))
                {
                    aesAlg.Key = key.GetBytes(aesAlg.KeySize / 8);
                    aesAlg.IV = key.GetBytes(aesAlg.BlockSize / 8);
                }

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
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
    }
}
