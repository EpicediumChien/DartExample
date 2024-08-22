using System.Collections.Generic;

namespace DDPM.SA.Common.Settings
{
    public class DDPMUserSettings
    {
        public enum Themes
        {
            Dark,
            Light
        }

        /*
         * DDPM Language Requirement
            •	English
            •	French
            •	Traditional Chinese
            •	Simplified Chinese
            •	Spanish
            •	German
            •	Russian
            •	Portuguese (Portugal & Brazil)
            •	Japanese
            •	Korean
        */

        public enum Languages
        {
            en,
            de,
            es,
            fr,
            ja_JP,
            pt_BR,
            ru_RU,
            zh_CN,
            zh_TW,
            ko_KR,

            //it_IT,
            preferred
        }

        //private static DDPMUserSettings userSettings;

        public double Version { get; set; }
        public int Language { get; set; }
        public bool IsSynchronizemonitor { get; set; } = false;
        public string Schedule { get; set; } = string.Empty;

        //Input
        //public string strInputSourceList { get; set; }
        //FW Update
        public FWUpdateInfoPackage DelayFWUpdateInfoPackage { get; set; }

        //0606 Bruce 新增鎖定自動旋轉方向
        public bool LockRotate { get; set; }

        //USBKVM
        //public string strUSBKVMPCsList { get; set; }
        public bool isTelemetryConsentOn { get; set; } = true; //global setting -> Analytics page -> checkbox on/off
        //public bool isTelemetryConsentAllow { get; set; } = true; //global setting -> Analytics page -> checkbox enable/disable
        //FW Update
        public bool LockFWU_UI { get; set; }

        public DokcUODUpdateInfoPackage UODFWUInfoPackage { get; set; }
        public List<string> SupportedMonitorList { get; set; }

        //public bool isOnUSBKVM { get; set; } = false;
        //public bool isOnNKVM { get; set; } = false;
        public SWUpdateInfoPackage DelaySWUpdateInfoPackage { get; set; }

        //20240820 Jim add Lock(Unlock) for Auto Color Preset
        public bool IsAutoColorPreset_Lock { get; set; } = false;

        //20240820 Jim add for Color Management
        public bool ColorManagement_off { get; set; } = false;
        public bool ColorManagement_bymonitor { get; set; } = false;
        public bool ColorManagement_byhost { get; set; } = false;
        
    }
}