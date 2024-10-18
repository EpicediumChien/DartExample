namespace DDPM.SA.Common.Settings
{
    public class DDPMSettings
    {
        private static readonly object Lock = new object();

        //public DDPMAppSettings AppSettings { get; set; }

        public DDPMUserSettings UserSettings { get; set; }

        public DDPMITConfig LockSettings { get; set; }

        public DDPMSettings(DDPMAppSettings appSettings, DDPMUserSettings userSettings, DDPMITConfig lockSettings)
        {
            //AppSettings = appSettings;
            UserSettings = userSettings;
            LockSettings = lockSettings;
        }
    }
}