using Dell.Client.Framework.Agent;
using Dell.Client.Framework.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using VcpCore.Common;

namespace VcpCore.Plugins
{
    public class RsaEncrypt : BaseAgentPlugin
    {
        private static Logs _logs;
        private IAgent _agent;
        private const string PluginLogId = "RsaEncrypt";

        protected RsaEncrypt(IAgent agent, string logId) : base(agent, logId)
        {
            _logs ??= new Logs(Log);
            _agent = agent;

        }

        public static KeyValuePair<string, string> GetKeyPair()
        {
            RSACryptoServiceProvider RSA = new RSACryptoServiceProvider();
            string publicKey = RSA.ToXmlString(false);
            string privateKey = RSA.ToXmlString(true);
            return new KeyValuePair<string, string>(publicKey, privateKey);
        }

        public static string Encrypt(string content, string encryptKey, int encryptionBufferSize = 117, int decryptionBufferSize = 128)
        {
            var rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(encryptKey);
            byte[] dataEncoded = Encoding.UTF8.GetBytes(content);
            using (var ms = new MemoryStream())
            {
                var buffer = new byte[encryptionBufferSize];
                int pos = 0;
                int copyLength = buffer.Length;
                while (true)
                {
                    if (pos + copyLength > dataEncoded.Length)
                    {
                        copyLength = dataEncoded.Length - pos;
                    }
                    buffer = new byte[copyLength];
                    Array.Copy(dataEncoded, pos, buffer, 0, copyLength);
                    pos += copyLength;

                    try
                    {
                        ms.Write(rsa.Encrypt(buffer, false), 0, decryptionBufferSize);
                    }
                    catch (Exception ex) 
                    {
                        _logs.Info($"[RsaEncrypt] Encrypt failed, message: {ex.Message}");
                    }

                    Array.Clear(buffer, 0, copyLength);
                    if (pos >= dataEncoded.Length)
                    {
                        break;
                    }
                }

                string res = string.Empty;

                try
                {
                     res = Convert.ToBase64String(ms.ToArray());
                }
                catch (Exception ex) 
                {
                    _logs.Info($"Convert.ToBase64String failed, message: {ex.Message}");
                }

                return res;
            }

            //RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            //rsa.FromXmlString(encryptKey);
            //UnicodeEncoding ByteConverter = new UnicodeEncoding();
            //byte[] DataToEncrypt = ByteConverter.GetBytes(content);
            //byte[] resultBytes = rsa.Encrypt(DataToEncrypt, false);
            //return Convert.ToBase64String(resultBytes);
        }

        public static string Decrypt(string content, string decryptKey, int decryptionBufferSize = 128)
        {
            try
            {
                var data = Convert.FromBase64String(content);
                var rsa = new RSACryptoServiceProvider();
                rsa.FromXmlString(decryptKey);
                using (var ms = new MemoryStream(data.Length))
                {
                    byte[] buffer = new byte[decryptionBufferSize];
                    int pos = 0;
                    int copyLength = buffer.Length;

                    while (true)
                    {
                        Array.Copy(data, pos, buffer, 0, copyLength);
                        pos += copyLength;
                        byte[] resp = rsa.Decrypt(buffer, false);
                        ms.Write(resp, 0, resp.Length);
                        Array.Clear(resp, 0, resp.Length);
                        Array.Clear(buffer, 0, copyLength);
                        if (pos >= data.Length)
                        {
                            break;
                        }
                    }
                    return Encoding.UTF8.GetString(ms.ToArray());
                }
            }
            catch (CryptographicException ce)
            {
                throw ce;
            }

            //byte[] dataToDecrypt = Convert.FromBase64String(content);
            //RSACryptoServiceProvider RSA = new RSACryptoServiceProvider();
            //RSA.FromXmlString(decryptKey);
            //byte[] resultBytes = RSA.Decrypt(dataToDecrypt, false);
            //UnicodeEncoding ByteConverter = new UnicodeEncoding();
            //return ByteConverter.GetString(resultBytes);
        }

        private static string Encrypt(string content, out string publicKey, out string privateKey)
        {
            RSACryptoServiceProvider rsaProvider = new RSACryptoServiceProvider();
            publicKey = rsaProvider.ToXmlString(false);
            privateKey = rsaProvider.ToXmlString(true);

            UnicodeEncoding ByteConverter = new UnicodeEncoding();
            byte[] DataToEncrypt = ByteConverter.GetBytes(content);
            byte[] resultBytes = rsaProvider.Encrypt(DataToEncrypt, false);
            return Convert.ToBase64String(resultBytes);
        }
    }
}