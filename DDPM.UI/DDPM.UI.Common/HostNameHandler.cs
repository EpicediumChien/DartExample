using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Networking;

namespace DDPM.UI.Common
{
    public class HostNameHandler
    {

        private static string GetHostNameByDNS()
        {
            string hostName = string.Empty;

            try
            {
                hostName = Dns.GetHostName();

                Match match = Regex.Match(hostName, @"^[a-zA-Z0-9]+(-[a-zA-Z0-9]+)*$");

                if (!match.Success || hostName.Length > 63 || hostName.Length < 1)
                {
                    hostName = "_ERROR";
                }
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("$\"[HostNameHandler] GetHostNameByDNS Exception : " + ex.Message);
            }

            return hostName;
        }

        private static string GetHostNameByMachineName()
        {
            string machineName = string.Empty;

            try
            {
                machineName = Environment.MachineName;
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("$\"[HostNameHandler] GetHostNameByMachineName Exception : " + ex.Message);
            }

            return machineName;
        }

        private static string GetHostNameByWMI()
        {
            string hostName = string.Empty;

            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT CSName FROM Win32_OperatingSystem");
                foreach (ManagementObject queryObj in searcher.Get())
                {
                    if(queryObj["CSName"] != null)
                    {
                        hostName = queryObj["CSName"].ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("$\"[HostNameHandler] GetHostNameByWMI Exception : " + ex.Message);
            }

            return hostName;
        }

        public static string GetHostName()
        {
            string hostName = string.Empty;

            hostName = GetHostNameByMachineName();

            return hostName;
        }
    }
}
