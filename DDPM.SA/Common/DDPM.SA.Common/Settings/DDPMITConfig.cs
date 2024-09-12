namespace DDPM.SA.Common.Settings
{
    public class DDPMITConfig
    {
        public double Version { get; set; } = 1.0;

        #region Global Setting
        //global setting -> Analytics page -> checkbox enable/disable
        public bool Lock_Settings_TelemetryConsent { get; set; } = false;
        public bool Lock_Settings_Updates { get; set; } = false;
        //public bool Lock_Settings_RestoreFactoryDefaults { get; set; } = false;
        public bool Lock_Setting_RestoreDefaults { get; set; } = false;
        #endregion Global Setting

        #region Display Setting
        public bool Lock_Display_ExportSettings { get; set; } = false;
        public bool Lock_Display_BriCont { get; set; } = false;
        public bool Lock_Display_AutoBriTemp { get; set; } = false;
        public bool Lock_Display_NetworkKVM { get; set; } = false;
        public bool Lock_Display_ColorPreset { get; set; } = false;
        public bool Lock_Display_PowerNap { get; set; } = false;
        public bool Lock_Display_ResolutionRefreshRate { get; set; } = false;
        #endregion Display Setting

        #region Peripheral Setting
        public bool Lock_Webcam_RestoreFactoryDefaults { get; set; } = false;
        public bool Lock_Audio_RestoreFactoryDefaults { get; set; } = false;
        public bool Lock_Keyboard_RestoreFactoryDefaults { get; set; } = false;
        public bool Lock_Mouse_RestoreFactoryDefaults { get; set; } = false;
        public bool Lock_Pen_RestoreFactoryDefaults { get; set; } = false;
        #endregion Peripheral Setting

    }
}