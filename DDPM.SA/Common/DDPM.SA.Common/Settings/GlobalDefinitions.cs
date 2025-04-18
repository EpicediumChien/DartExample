//#define SUPPORT_200
//#define SUPPORT_210
#define REMOVE_EA_SPLITTERS //Robert_Lin 2025-4-15, Define this symbol to remove all (unused VSplitters and HSplitters)
#define DISABLE_LOCK

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDPM.SA.Common.Settings
{
    public class GlobalDefinitions
    {
#if SUPPORT_200
        public const bool isSupport200 = true;
        public const bool isSupport210 = false;
#elif SUPPORT_210
        public const bool isSupport200 = false;
        public const bool isSupport210 = true;
#else
        public const bool isSupport200 = false;//for 2.0.1
        public const bool isSupport210 = false;
#endif

        //2025/4/18 Dean: this flag is used to disable lock functionality
#if DISABLE_LOCK
        public const bool isDisableLock = true;
#else
        public const bool isDisableLock = false;
#endif
        public const bool enableCurrentColorCache = true;
        public const string major_url = "https://clientperipherals.dell.com/DDPM/";
        public const string percent_two_url = "https://downloads.dell.com";
        public const string Display_FWU_URL_Folder = @$"/Windows/Display/Firmware/";
        public const string SW_URL_Folder = @$"/Windows/Application/";
        public const string Dongle_BeforeGen2_Name = "Dell Universal Receiver";
        public const string Display_ICC_URL_Folder = @"ICC/";

        //DDPMW-2896, Dean add 2025/4/15
        public const string HttpAgentNamePreset = "Dell Display and Peripheral Agent ";

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

        //Log location global variables, prefix could be "%programdata%" or "%localappdata%" by the design
        public const string LogDDPM = @"\Dell\Dell Display and Peripheral Manager";
        public const string LogSwUpdater = @"\Dell\Dell Display and Peripheral Manager\DdpmSwUpdater";
        public const string LogFwUpdater = @"\Dell\Dell Display and Peripheral Manager\FWUpdateLog";
        public const string LogDPeM = @"\Dell\Dell Peripheral Manager\DPeMSDK\Log";
        public const string LogDPM = @"\Dell\Dell Peripheral Manager\DPM\Log";
        public const string LogDPMService = @"\Dell\Dell Peripheral Manager\DPMService\Log";
        public const string LogDTP = @"\Dell\DTP\Logs";
        public const string LogDTH = @"\Dell\Dell TechHub";
        public const string LogDDPMSYSSA = @"\Dell\DDPM.Subagent";
        public const string LogDDPMGUI = @"\Dell\Dell Display and Peripheral Manager\Log\DDPM.GUI";
        public const string LogDDPMUSERSA = @"\Dell\Dell Display and Peripheral Manager\Log\DDPM.Subagent.User";

        //Migration
        public const string MigrationInput = "Migration";

        //DDM(NKVM) executable file name
#if SUPPORT_200
        public const string DDMExeName = "DDM.exe";
        public const string DDMProcessName = "DDM";
#else
        public const string DDMExeName = "DDPM-NKVM.exe";
        public const string DDMProcessName = "DDPM-NKVM";
#endif
        //Overall settings param
        public const string Folder_Product = "Dell Display and Peripheral Manager";
        public static readonly string Folder_ProgramData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Dell");
        //Sys settings param
        public const string Filename_appsettings_IT = "DDPM.Configs.json";
        public const string Filename_appsettings_Info = "DDPM.Infos.json";
        //User settings param
        public const string Filename_GlobalSetting_peruser = "GlobalSetting.json";
        public const string Filename_appsettings_peruser = "DDPM.Configs.json";
        //DPeM service
        public const string DPeMServiceName = "DPMService";
        //Software installed name
        public const string InstalledName_DPeM = "Dell Peripheral Core";
        public const string InstalledName_NKVM = "DDPMW-NKVM";
    }

    public class InfoObject
    {
        //string: info value, bool: isActived
        public List<string> Infos { get; set; } = new List<string>();
    }
}
