using Microsoft.Win32;
using System;
using System.Security.AccessControl;
using System.Security;
using System.Windows.Media.Animation;
using Windows.Devices.Geolocation;
using System.Runtime.InteropServices;
using Dell.Client.Framework.Common;

namespace DDPM.SA.Common.Settings
{
    public enum RegistryHive
    {
        CurrentUser,
        LocalMachine
    }

    public class DDPMRegistryHelper
    {
        //private static Log _log;
        public static void WriteRegistryKey(RegistryHive hive, string keyPath, string keyName, object value)
        {
            ValidateInput(keyPath, keyName);
            string fullPath = GetFullPath(hive, keyPath);
            Registry.SetValue(fullPath, keyName, value);
        }

        public static object ReadRegistryKey(RegistryHive hive, string keyPath, string keyName)
        {
            ValidateInput(keyPath, keyName);
            string fullPath = GetFullPath(hive, keyPath);
            return Registry.GetValue(fullPath, keyName, null);            
        }

        public static void DeleteRegistryKey(RegistryHive hive, string keyPath, string keyName)
        {
            ValidateInput(keyPath, keyName);
            string fullPath = GetFullPath(hive, keyPath);
            using (RegistryKey key = GetBaseKey(hive).OpenSubKey(keyPath, true))
            {
                if (key != null)
                {
                    key.DeleteValue(keyName, false);
                }
            }
        }

        private static bool ValidateInput(string keyPath, string keyName = null)
        {
            if (string.IsNullOrWhiteSpace(keyPath))
            {
                throw new ArgumentException("Key path cannot be null or empty.");
            }

            if (keyName != null && string.IsNullOrWhiteSpace(keyName))
            {
                throw new ArgumentException("Key name cannot be null or empty.");
            }

            //Dean 0912: Registry should not apply file path check
            //Elsa Add Security
            //string FileInfo;
            //if (!DDPMFileSecurity.IsFilePathValid(keyPath, out FileInfo))
            //{
            //    _log.Info($"{nameof(ValidateInput)} {FileInfo}");
            //    throw new ArgumentException($"Invalid file path string - {keyPath}");
            //}
            return true;
        }

        private static string GetFullPath(RegistryHive hive, string keyPath)
        {
            return hive switch
            {
                RegistryHive.CurrentUser => $@"HKEY_CURRENT_USER\{keyPath}",
                RegistryHive.LocalMachine => $@"HKEY_LOCAL_MACHINE\{keyPath}",
                _ => throw new ArgumentOutOfRangeException(nameof(hive), hive, null)
            };
        }

        private static RegistryKey GetBaseKey(RegistryHive hive)
        {
            return hive switch
            {
                RegistryHive.CurrentUser => Registry.CurrentUser,
                RegistryHive.LocalMachine => Registry.LocalMachine,
                _ => throw new ArgumentOutOfRangeException(nameof(hive), hive, null)
            };
        }

        public static bool HasRegistryPermission(RegistryHive hive, string keyPath, RegistryRights rights)
        {
            ValidateInput(keyPath);

            try
            {
                using (RegistryKey key = GetBaseKey(hive).OpenSubKey(keyPath, rights))
                {
                    return key != null;
                }
            }
            catch (SecurityException)
            {
                return false;
            }
        }

        
    }
}
