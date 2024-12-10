using DdmLibrary.Utility;

namespace DDPM.SA.Common.Settings
{
    public class DDPMImpExpSettings
    {
        public DDPMAppSettings AppSettings { get; set; } = new DDPMAppSettings();
        public DDPMUserSettings UserSettings { get; set; } = new DDPMUserSettings();
        public DDPMMonitorSettings MonitorSettings { get; set; } = new DDPMMonitorSettings();
    }

    public class DDMImpSettings
    {
        public DDMAppSettings AppSettings { get; set; } = new DDMAppSettings();
        public DDMUserSettings UserSettings { get; set; } = new DDMUserSettings();
        public DDMMonitorSettings MonitorSettings { get; set; } = new DDMMonitorSettings();

    }

    public class ImpVCPSequence
    {
        public uint ALSConfig { get; set; } = 0;
    }
}