using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DDPM.SA.Obfuscation
{
    public class SettingsAccess
    {
        private const int iterations = 100;
        private const int keyLength = 128;

        /*
            //Example:
            // The secret key for HMAC. In a real-world application, this should be kept secret.
		    byte[] secretKey = Encoding.UTF8.GetBytes("your-secret-key"); //This key can be replaced with InstallShield installer's AppId
		    // The message to hash.
		    string message = "The quick brown fox jumps over the lazy dog"; //This message can be replaced with Json serialized string
		    // Generate the HMAC.
		    string hmac = GenerateAccessString(secretKey, message);
		    Console.WriteLine($"HMAC:{hmac}"); 
		    // Verify the HMAC.bool isVerified = VerifyAccessString(secretKey, message, hmac);
		    Console.WriteLine($"HMAC Verified:{isVerified}");
         */
        public static string GenerateAccessString(byte[] key, string message)
        {
            using (var HMACSha512 = new HMACSHA512(key))
            {
                byte[] messageBytes = Encoding.UTF8.GetBytes(message);
                byte[] hashMessage = HMACSha512.ComputeHash(messageBytes);
                return Convert.ToBase64String(hashMessage);
            }
        }
        
        public static string GenerateAccessString2(string key, string message)
        {
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            byte[] output = DeriveKey(key, messageBytes, iterations, keyLength);
            return Convert.ToBase64String(output);
        }

        /*public static bool VerifyAccessString(byte[] key, string message, string HMAC)
        {
            using (var HMACSha512 = new HMACSHA512(key))
            {
                byte[] messageBytes = Encoding.UTF8.GetBytes(message);
                byte[] hashMessage = HMACSha512.ComputeHash(messageBytes);
                string computedHmac = Convert.ToBase64String(hashMessage);
                return HMAC == computedHmac;
            }
        }*/

        //HashAlgorithmName.SHA512 as default
        public static string ComputeAccessInfo2(byte[] key, string message)//, HashAlgorithmName hashAlgorithm)
        {
            try
            {
                //   key = DeriveKey();
                HashAlgorithmName hashAlgorithm = HashAlgorithmName.SHA512;
                byte[] messageBytes = Encoding.UTF8.GetBytes(message);

                switch (hashAlgorithm.Name)
                {
                    case "SHA512":
                        {
                            using (var hmacsha512 = new HMACSHA512(key))
                            {
                                byte[] hashBytes = hmacsha512.ComputeHash(messageBytes);
                                Console.WriteLine(BitConverter.ToString(hashBytes).Replace("-", "").ToLower());

                                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
                            }
                        }
                    case "SHA256":
                        {
                            using (var hmacsha256 = new HMACSHA256(key))
                            {
                                byte[] hashBytes = hmacsha256.ComputeHash(messageBytes);
                                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
                            }
                        }
                    default:
                        throw new Exception("Underlying HMAC mechanism must leverage HMACSHA256 or higher");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Unable to generate HMAC: {e.Message}");
                return "";
            }
        }

        private static byte[] DeriveKey(string password, byte[] salt, int iterations, int keyLength)
        {
            using (var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(password)))
            {
                //  foreach (byte key in salt) { Console.WriteLine($"{key}"); }
                var derivedKey = new byte[keyLength];
                var blockCount = (int)Math.Ceiling((double)keyLength / hmac.HashSize);
                //var blockCount = (int)Math.Ceiling((double)keyLength );
                var buffer = new byte[hmac.HashSize / 8];
                var temp = new byte[hmac.HashSize / 8];

                for (int i = 1; i <= blockCount; i++)
                {
                    var counter = BitConverter.GetBytes(i);
                    if (BitConverter.IsLittleEndian)
                    {
                        Array.Reverse(counter);
                    }

                    hmac.TransformBlock(salt, 0, salt.Length, salt, 0);
                    hmac.TransformFinalBlock(counter, 0, counter.Length);
                    Array.Copy(hmac.Hash, temp, temp.Length);

                    Array.Copy(temp, 0, buffer, 0, temp.Length);

                    for (int j = 1; j < iterations; j++)
                    {
                        temp = hmac.ComputeHash(temp);
                        for (int k = 0; k < buffer.Length; k++)
                        {
                            buffer[k] ^= temp[k];
                        }
                    }

                    Array.Copy(buffer, 0, derivedKey, (i - 1) * buffer.Length, buffer.Length);
                }
                /* foreach (byte b in derivedKey)
                 {
                     Console.WriteLine(b + " ");
                 }*/
                // Console.WriteLine($"Derived Key: {Encoding.UTF8.GetString(derivedKey)} ");
                return derivedKey.Take(keyLength / 2).ToArray();
            }
        }

        private static bool IsUserElevated()
        {
            using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
            {
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
        }

        private static (string id, string ver, string location) AppInfo { get; } = QueryAppAccessInfo();
        public static string AppAccessInfo { get; } = AppInfo.id;
        public static string AppAccessVer { get; } = AppInfo.ver;
        public static string AppAccessAddr { get; } = AppInfo.location;

        // ***Important***
        //This function require system/admin privilege
        //And return the setting file's private key for signature generate
        private static (string id, string ver, string location) QueryAppAccessInfo()
        {
            //info = string.Empty;
            if(!IsUserElevated())
            {
                //info = "Caller doesn't has elevated privilege";
                return (string.Empty, string.Empty, string.Empty);
            }

            string softwareName = "Dell Display and Peripheral Manager";

            // target registry path
            string registryKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";

            // open and sequential read to compare.
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(registryKey))
            {
                if (key != null)
                {
                    foreach (string subkeyName in key.GetSubKeyNames())
                    {
                        try
                        {
                            using (RegistryKey subkey = key.OpenSubKey(subkeyName))
                            {
                                if (subkey != null)
                                {
                                    // get value from DisplayName
                                    string displayName = subkey.GetValue("DisplayName") as string;
                                    if (displayName != null && displayName.Trim().Equals(softwareName))
                                    {
                                        // get value from uninstall string
                                        //string data = subkey.GetValue("UninstallString") as string;
                                        //if(data != null && data.Length >= 36) //format like "{fgsetyu5-5da6-5ges-9sed-s6h8deqa6358}"
                                        {
                                            //string output = data.ToUpper().Replace("MSIEXEC.EXE", "").Replace("{", "").Replace("}", "").Replace("-", "").Replace("/X", "").Trim();
                                            string output = subkeyName.ToUpper().Replace("{", "").Replace("}", "").Replace("-", "").Trim();
                                            if (output != null && output.Length == 32)
                                            {
                                                string ver = subkey.GetValue("DisplayVersion") as string;
                                                string addr = subkey.GetValue("InstallLocation") as string;
                                                //info key original method: GenerateAccessString(Encoding.UTF8.GetBytes(output), softwareName)
                                                string infoKey = Convert.ToBase64String(DeriveKey(output, Encoding.UTF8.GetBytes(softwareName), iterations, keyLength));
                                                
                                                return (infoKey, ver, addr); //this id is used as DDPM settings private key
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        catch
                        {
                            //do nothing
                        }
                    }
                }
            }
            return (string.Empty, string.Empty,string.Empty);
        }
    }
}
