using DdmLibrary.Utility;

namespace DDPM.SA.Common.Settings
{
    public class DDPMImpExpSettings
    {
        public DDPMAppSettings AppSettings { get; set; }
        public DDPMUserSettings UserSettings { get; set; }
        public DDPMMonitorSettings MonitorSettings { get; set; }
    }

    public class DDMImpSettings
    {
        public DDMAppSettings AppSettings { get; set; }
        public DDMUserSettings UserSettings { get; set; }
        public DDMMonitorSettings MonitorSettings { get; set; }

    }

    public class ImpVCPSequence
    {
        public uint ALSConfig { get; set; } = 0;
    }
}