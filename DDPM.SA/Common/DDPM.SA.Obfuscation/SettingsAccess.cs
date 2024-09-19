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

        public static bool VerifyAccessString(byte[] key, string message, string HMAC)
        {
            using (var HMACSha512 = new HMACSHA512(key))
            {
                byte[] messageBytes = Encoding.UTF8.GetBytes(message);
                byte[] hashMessage = HMACSha512.ComputeHash(messageBytes);
                string computedHmac = Convert.ToBase64String(hashMessage);
                return HMAC == computedHmac;
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
                                                return (GenerateAccessString(Encoding.UTF8.GetBytes(output), softwareName), ver, addr); //this id is used as DDPM settings private key
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
        public static string NKVM_Log { get; } = QueryNKVMLogInfo();
        private static string QueryNKVMLogInfo()
        {
            string ret = string.Empty;
            // target registry path
            string registryKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\DDPMW-NKVM";
            try
            {
                // open and sequential read to compare.
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(registryKey))
                {
                    if (key != null)
                    {
                        // get value from GUID
                        string GUID = key.GetValue("GUID") as string;
                        if (GUID != null)
                        {
                            ret = GUID;
                        }
                    }
                }
            }
            catch
            {

            }
            return ret;
        }
    }
}
