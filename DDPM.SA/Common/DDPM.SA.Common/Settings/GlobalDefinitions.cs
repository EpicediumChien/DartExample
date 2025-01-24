using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDPM.SA.Common.Settings
{
    public class GlobalDefinitions
    {
        public const string major_url = "https://clientperipherals.dell.com/DDPM/";
        public const string percent_two_url = "https://downloads.dell.com";
        public const string Display_FWU_URL_Folder = @$"/Windows/Display/Firmware/";
        public const string SW_URL_Folder = @$"/Windows/Application/";
        public const string Dongle_BeforeGen2_Name = "Dell Universal Receiver";
        public const string Display_ICC_URL_Folder = @"ICC/";

        //for log print comparison
        private const string production_server = "clientperipherals.dell.com";
        private const string staging_server = "clientperipherals-uat.dell.com";
        private const string download_server = "downloads.dell.com";

        public static string GetLogPrintServerName(string url)
        {
            if (url.Contains(production_server, StringComparison.OrdinalIgnoreCase))
            {
                return "PRODUCTION SERVER";
            }
            else if (url.Contains(staging_server, StringComparison.OrdinalIgnoreCase))
            {
                return "STAGING SERVER";
            }
            else if (url.Contains(download_server, StringComparison.OrdinalIgnoreCase))
            {
                return "DELL DOWNLOADS SERVER";
            }
            else
            {
                return "Unknown address";
            }
        }
    }
}
