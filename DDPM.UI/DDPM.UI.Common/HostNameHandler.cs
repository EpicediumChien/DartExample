using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DDPM.UI.Common
{
    public class HostNameHandler
    {
        public static string GetDNSHostName()
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
                DdpmCommonHelper.WriteUILog("$\"[HostNameHandler] GetDNSHostName Exception : " + ex.Message);
            }

            return hostName;
        }
    }
}
