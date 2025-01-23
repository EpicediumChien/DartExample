using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using Windows.Security.Cryptography.Certificates;

namespace DDPM.SA.Obfuscation
{
    public class SettingsAccess
    {
        #region For FW Key Generator
        public static readonly string comKey = "51f371b0181d7a9a7457e48ef639396d8cac1445a762cd012de42ab2a70aa9ab";
        public static readonly byte[] cp1 = { 0x39, 0x65, 0x82, 0x37, 0xA0, 0xDC, 0x2E, 0x5F };
        public static readonly byte[] cp1_keyseed = { 0x1F, 0x3D, 0x92, 0x3C };
        #endregion

        private const int iterations = 100;
        private const int keyLength = 128;
        private static string salt = "841c87c9f5a679dcdba8a9c7f743847d157cd598";

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
        /*public static string ComputeAccessInfo2(byte[] key, string message)//, HashAlgorithmName hashAlgorithm)
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
        }*/

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
        //public static string AppAccessInfo { get; } = AppInfo.id;
        public static string AppAccessVer { get; } = AppInfo.ver;
        public static string AppAccessAddr { get; } = AppInfo.location;

        // ***Important***
        //This function require system/admin privilege
        //And return the setting file's private key for signature generate
        private static (string id, string ver, string location) QueryAppAccessInfo()
        {
            //info = string.Empty;
            if (!IsUserElevated())
            {
                //info = "Caller doesn't has elevated privilege";
                return (string.Empty, string.Empty, string.Empty);
            }

            string softwareName = "Dell Display and Peripheral Manager";

            // target registry path
            string registryKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";

            // open and sequential read to compare.
            //using (RegistryKey key = Registry.LocalMachine.OpenSubKey(registryKey))
            using (RegistryKey key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32)) // DDPM-Setup-2.0.0.40.exe is x86-32bit
            {
                if (key != null)
                {
                    using (RegistryKey uninstallKey = key.OpenSubKey(registryKey))
                    {
                        foreach (string subkeyName in uninstallKey.GetSubKeyNames())
                        {
                            try
                            {
                                using (RegistryKey subkey = uninstallKey.OpenSubKey(subkeyName))
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
            }
            return (string.Empty, string.Empty, string.Empty);
        }
        public static bool QueryRegistryUpdateLock(out bool isUpdateLock, out string info)
        {
            //info = string.Empty;
            if (!IsUserElevated())
            {
                //info = "Caller doesn't has elevated privilege";
                isUpdateLock = false;
                info = "QueryRegistryUpdateLock IsUserElevated";
                return (false);
            }


            // target registry path
            string registryKey = @"SOFTWARE\Dell\Dell Display And Peripheral Manager\";

            try
            {
                // open and sequential read to compare.
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(registryKey))
                //using (RegistryKey key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32)) // DDPM-Setup-2.0.0.40.exe is x86-32bit
                {
                    if (key != null)
                    {
                        string value = key.GetValue("InAppUpdateLock") as string;
                        if (!string.IsNullOrEmpty(value))
                        {
                            isUpdateLock = value.Equals("1") ? true : false;
                            info = $"QueryRegistryUpdateLock ok";
                            return true;
                        }
                        else
                        {
                            info = $"QueryRegistryUpdateLock value == null";
                        }
                    }
                    else
                    {
                        info = $"QueryRegistryUpdateLock key == null";
                    }
                }
            }
            catch (Exception ex)
            {
                info = $"QueryRegistryUpdateLock error:{ex.Message}";
            }
            isUpdateLock = false;
            return (false);
        }
        public static bool DeleteRegistryUpdateLock(out string info)
        {
            //info = string.Empty;
            if (!IsUserElevated())
            {
                //info = "Caller doesn't has elevated privilege";
                info = "DeleteRegistryUpdateLock IsUserElevated";
                return false;
            }

            // target registry path
            string registryKey = @"SOFTWARE\Dell\Dell Display And Peripheral Manager\";

            try
            {
                // open and sequential read to compare.
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(registryKey, true))
                //using (RegistryKey key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32)) // DDPM-Setup-2.0.0.40.exe is x86-32bit
                {
                    if (key != null)
                    {
                        key.DeleteValue("InAppUpdateLock");
                        info = $"DeleteRegistryUpdateLock ok";
                        return true;
                    }
                    else
                    {
                        info = $"DeleteRegistryUpdateLock key == null";
                    }
                }
            }
            catch (Exception ex)
            {
                info = $"DeleteRegistryUpdateLock error:{ex.Message}";
            }
            return false;
        }

        #region XOR-random-number for secret key
        public static byte[] ComputeBytes(byte[] guid, byte[] randomBytes)
        {
            // Perform XOR operation
            try
            {
                byte[] xorResult;
                if (guid.Length > randomBytes.Length)//throw new Exception("The length of the GUID and Random Bytes are not equal.");
                {
                    xorResult = new byte[randomBytes.Length];
                    for (int i = 0; i < randomBytes.Length; i++)
                    {
                        xorResult[i] = (byte)(guid[i] ^ randomBytes[i]);
                    }
                }
                else
                {
                    xorResult = new byte[guid.Length];
                    for (int i = 0; i < guid.Length; i++)
                    {
                        xorResult[i] = (byte)(guid[i] ^ randomBytes[i]);
                    }
                }

                // Convert the XOR result to a hexadecimal string
                string xorHex = BitConverter.ToString(xorResult).Replace("-", "");
#if DEBUG
                Console.WriteLine($"GUID: {guid}");
                Console.WriteLine($"Random Bytes");//: {BitConverter.ToString(randomBytes).Replace("-", "")}");
                Console.WriteLine($"XOR Result");//: {xorHex}");
#endif
                return xorResult;
            }
            catch (Exception e)
            {
#if DEBUG
                Console.WriteLine($"Unable to generate HMAC: {e.Message}");
#endif
                return [];
            }
        }

        public static string GenerateReferenceInfo()
        {
            // Generate a random number
            byte[] randomNumber = new byte[16]; // 128 bits
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
            }

            // Convert the random number to a hexadecimal string
            string hexRandomNumber = BitConverter.ToString(randomNumber).Replace("-", "");

            return hexRandomNumber;
        }

        public static string GenerateReferenceTicket(DateTimeOffset utcNow)
        {
            //generate hMAC using timestamp and random number with a XOR predefined GUID
            long timestamp = utcNow.ToUnixTimeSeconds();
            // Convert the timestamp to a hexadecimal string
            string hexTimestamp = Convert.ToString(timestamp, 16);

            return hexTimestamp;
        }

        public static string GenerateSignature(string referenceTicket, string referenceInfo, string content)
        {
            string hexTimestamp = referenceTicket;
            string hexRandomNumber = referenceInfo;
            // Combine the timestamp and random number
            string combined = hexTimestamp + hexRandomNumber;

            //Console.WriteLine("*** Timestamp (Hex): " + hexTimestamp + "," + hexTimestamp.Length);
            //Console.WriteLine("*** Random Number (Hex): " + hexRandomNumber + "," + hexRandomNumber.Length);
            //Console.WriteLine("*** Combined: " + combined + "," + combined.Length);

            string token = content;
            byte[] secToken = Encoding.UTF8.GetBytes(token);
            //Console.WriteLine($"*** GenerateRandomNumber {secToken.Length} {hexRandomNumber.Length}");
            //password = random number XOR string
            byte[] byteArray = ComputeBytes(Encoding.UTF8.GetBytes(salt), Encoding.UTF8.GetBytes(hexRandomNumber));

            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
                byteArray,
                Encoding.UTF8.GetBytes(hexRandomNumber),
                10000,
                HashAlgorithmName.SHA512,
                512/8);//output 64 bytes
            
            string hMAC1 = ComputeHMACSHA512(secToken, hash, HashAlgorithmName.SHA512);
            //Console.WriteLine($"*** Message 1: {hMAC1}");

            return hMAC1;
        }

        public static bool VerifySignature(string referenceTicket, string referenceInfo, string content, string signature)
        {
            CultureInfo provider = CultureInfo.InvariantCulture;
            string format = "M/d/yyyy h:mm:ss tt zzz";
            long timestamp = DateTimeOffset.ParseExact(referenceTicket, format, provider, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal).ToUnixTimeSeconds();

            string hexTimestamp = Convert.ToString(timestamp, 16);

            //string hexTimestamp = referenceTicket;
            string hexRandomNumber = referenceInfo;
            // Combine the timestamp and random number
            string combined = hexTimestamp + hexRandomNumber;
#if DEBUG
            Console.WriteLine("*** Timestamp (Hex)");//: " + hexTimestamp + "," + hexTimestamp.Length);
            Console.WriteLine("*** Random Number (Hex)");//: " + hexRandomNumber + "," + hexRandomNumber.Length);
            Console.WriteLine("*** Combined");//: " + combined + "," + combined.Length);
#endif
            string token = content;
            byte[] secToken = Encoding.UTF8.GetBytes(token);
#if DEBUG
            Console.WriteLine($"*** GenerateRandomNumber");// {secToken.Length} {hexRandomNumber.Length}");
#endif
            byte[] random = VerifyTimestamp(referenceTicket, combined);
            //    byte[] byteArray = Encoding.UTF8.GetBytes(token);
            byte[] byteArray = ComputeBytes(Encoding.UTF8.GetBytes(salt), random);

            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
                byteArray,
                random,
                10000,
                HashAlgorithmName.SHA512,
                512 / 8);//output 64 bytes

            string hMAC2 = ComputeHMACSHA512(secToken, hash, HashAlgorithmName.SHA512);
#if DEBUG
            Console.WriteLine($"*** Message 2");
#endif

            return hMAC2.ToUpper().Equals(signature.ToUpper());
        }

        private static byte[] VerifyTimestamp(string utctimestamp, string combined)
        {
            CultureInfo provider = CultureInfo.InvariantCulture;
            string format = "M/d/yyyy h:mm:ss tt zzz";
            long timestamp = DateTimeOffset.ParseExact(utctimestamp, format, provider, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal).ToUnixTimeSeconds();

            string hexTimestamp = Convert.ToString(timestamp, 16);

            int lastIndex = combined.LastIndexOf(hexTimestamp) + hexTimestamp.Length;// + 1;

            //Console.WriteLine($"*** Reversed secret random number: {timestamp.ToString()} {lastIndex} {combined.Substring(lastIndex)}");
            string str = combined.Substring(lastIndex);

            return Encoding.UTF8.GetBytes(str);
        }

        private static string ComputeHMACSHA512(byte[] key, byte[] message, HashAlgorithmName hashAlgorithm)
        {
            try
            {
                switch (hashAlgorithm.Name)
                {
                    case "SHA512":
                        {
                            using (var hmacsha512 = new HMACSHA512(key))
                            {
                                byte[] hashBytes = hmacsha512.ComputeHash(message);
                                //Console.WriteLine($"*** ComputeHMACSHA512: {BitConverter.ToString(hashBytes).Replace("-", "").ToLower()}");
                                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
                            }
                        }
                    case "SHA256":
                        {
                            using (var hmacsha256 = new HMACSHA256(key))
                            {
                                byte[] hashBytes = hmacsha256.ComputeHash(message);
                                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
                            }
                        }
                    default:
                        throw new Exception("*** Underlying HMAC mechanism must leverage HMACSHA256 or higher");
                }
            }
            catch (Exception e)
            {
#if DEBUG
                Console.WriteLine($"*** Unable to generate HMAC: {e.Message}");
#endif
                return "";
            }
        }
        #endregion
    }
}
