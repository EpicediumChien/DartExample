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
        private static string HostNameCheck(string? input)
        {
            input = input ?? "_ERROR";

            Match match = Regex.Match(input, @"^[a-zA-Z0-9]+(-[a-zA-Z0-9]+)*$");

            if (!match.Success || input.Length > 63 || input.Length < 1)
            {
                input = "_ERROR";
            }

            return input;
        }

        private static string GetHostNameByDNS()
        {
            string hostName = "_ERROR";

            try
            {
                hostName = Dns.GetHostName();
                hostName = HostNameCheck(hostName);
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("$\"[HostNameHandler] GetHostNameByDNS Exception : " + ex.Message);
            }

            return hostName;
        }

        private static string GetHostNameByMachineName()
        {
            string machineName = "_ERROR";

            try
            {
                machineName = HostNameCheck(Environment.MachineName);
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("$\"[HostNameHandler] GetHostNameByMachineName Exception : " + ex.Message);
            }

            return machineName;
        }

        private static string GetHostNameByWMI()
        {
            string hostName = "_ERROR";

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

            hostName = HostNameCheck(hostName);

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
