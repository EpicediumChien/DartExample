using Dell.Client.Framework.Common;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DDPM.SA.Common
{
    //Robert_Lin, 2024-12-6
    /// <summary>
    /// The common static functions/methods for easy management and maintainace
    /// </summary>
    public static class CommonFunctions
    {
        /// <summary>
        /// Get the screen scale of primary screen when your app startup.
        /// The scale will not be updated when the screen scale changed at runtime.
        /// You need this scale to calculate the correct position/size if you are using
        /// System.Window.Forms.Screen to get size/position.
        /// </summary>
        /// <returns>Screen scale (>= 1.0), if fail to get, it return 1.</returns>
        public static double GetDpiX()
        {
            try
            {
                var dpiXProperty = typeof(SystemParameters).GetProperty("DpiX", BindingFlags.NonPublic | BindingFlags.Static);
                if (dpiXProperty != null)
                {
                    var varX = (int)dpiXProperty.GetValue(null, null);
                    double dpiX = (double)varX / (double)96;
                    return dpiX;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("GetDpiX causes an exception: " + ex.Message);
            }
            return 1.000;
        }

        /// <summary>
        /// Format a Rectangle to string, format: "(0,0)-(1920,1200)1920x1200"
        /// </summary>
        /// <param name="rc"></param>
        /// <returns></returns>
        public static string FormatRectangle(System.Drawing.Rectangle rc)
        {
            return $"({rc.Left},{rc.Top})-({rc.Right},{rc.Bottom}){rc.Width}x{rc.Height}";
        }

        public static bool IsServiceRunning(string ServiceName, ILog Log = null)
        {
            //Bruce 0221 Add check service status 
            Log?.Info($"[IsServiceRunning] {ServiceName} running check");
            bool ret = false;
            try
            {
                using (ServiceController service = new ServiceController(ServiceName))
                {
                    if (service.Status == ServiceControllerStatus.Running)
                    {
                        ret = true;
                    }
                    else
                    {
                        Log?.Info($"[IsServiceRunning] {ServiceName} not running, status: {service.Status}");
                    }
                }
            }
            catch (Exception ex)
            {
                Log?.Info($"[IsServiceRunning] {ServiceName} Error: {ex.Message}");
            }
            //Log?.Info($"[IsServiceRunning] {ServiceName} done. ret : {ret}");
            return ret;
        }

        public static string GetInstalledSoftwareVersion(string softwareName, ILog Log)
        {
            try
            {
                string registryKey32Bit = @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall";
                string registryKey64Bit = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";

                // Check 32-bit apps on a 64-bit system
                Log?.Info($"[GetInstalledSoftwareVersion] Check 32-bit apps on a 64-bit system");
                string version = GetVersionFromRegistry(Registry.LocalMachine.OpenSubKey(registryKey32Bit), softwareName);
                if (!string.IsNullOrEmpty(version))
                    return version;

                // Check 64-bit apps (or all apps on 32-bit systems)
                Log?.Info($"[GetInstalledSoftwareVersion] Check 64-bit apps (or all apps on 32-bit systems)");
                return GetVersionFromRegistry(Registry.LocalMachine.OpenSubKey(registryKey64Bit), softwareName);
            }
            catch (Exception ex)
            {
                Log?.Info($"[GetInstalledSoftwareVersion] Exception: {ex.Message}");
                return string.Empty;
            }
        }

        private static string GetVersionFromRegistry(RegistryKey registryKey, string softwareName)
        {
            if (registryKey == null)
                return null;

            foreach (string subKeyName in registryKey.GetSubKeyNames())
            {
                using (RegistryKey subKey = registryKey.OpenSubKey(subKeyName))
                {
                    string displayName = subKey?.GetValue("DisplayName") as string;
                    if (!string.IsNullOrEmpty(displayName) && displayName.Contains(softwareName, StringComparison.OrdinalIgnoreCase))
                    {
                        return subKey.GetValue("DisplayVersion") as string;
                    }
                }
            }
            return string.Empty;
        }
    }
}
