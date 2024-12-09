using DDPM.SA.Common.Display;
using System;
using System.Collections.Generic;

namespace DDPM.SA.Common.Settings
{
    public class DDPMSimpleMonitorRecord
    {
        public string ModelName { get; set; } = string.Empty;
        public string ServiceTag { get; set; } = string.Empty;
    }

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

        public double Version { get; set; } = 1.0; //consider how to control the setting's version in the feature
        public int Language { get; set; }
        public bool IsSynchronizemonitor { get; set; } = false;
        public bool IsSynchronizemonitor_Scheduled { get; set; } = false;
        public string Schedule { get; set; } = string.Empty;
        public FrequencyDateTime TelementryFrequency { get; set; } = new FrequencyDateTime() { Month1stDay = DateTime.Now, PerDay = DateTime.Now, Weekly = DateTime.Now, };

        //FW Update
        public FWUpdateInfoPackage DelayFWUpdateInfoPackage { get; set; }

        //0606 Bruce 新增鎖定自動旋轉方向
        public bool LockRotate { get; set; }

        //FW Update
        public bool LockFWU_UI { get; set; }

        public DokcUODUpdateInfoPackage UODFWUInfoPackage { get; set; }
        public List<string> SupportedMonitorList { get; set; }

        public SWUpdateInfoPackage DelaySWUpdateInfoPackage { get; set; }

        public string VideoCaptureFolder { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);

        //Robert_Lin added for Display / Easy Arrange / Settings (EzSettings module)
        //These settings are per-user settings and will apply to all monitors

        #region EzSettings

        //Recent Hotkey settings: will save to Hotkey settings, implemented by Gavin Liu

        //Robert_Lin, 2024-10-11 EasyArrange Custom Layouts, move from MonitorSettings
        public SplitJson[] EACustomList { get; set; }

        public EzSettings EzSettings { get; set; } = new EzSettings();

        #endregion EzSettings

        #region EasyMemory

        public List<EAProfileDDPM> EAProfile { get; set; }// = new EAProfile();

        #endregion EasyMemory

        public HotkeySettings HotkeySettings { get; set; } = new HotkeySettings();
        public bool isDisplayConsentPage { get; set; } = false;//DDPMW-1254
        public DDPMSimpleMonitorRecord lastUISelectedMonitor {  get; set; } = new DDPMSimpleMonitorRecord();
    }
}