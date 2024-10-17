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
        public bool Lock_Setting_ScreenNotification { get; set; } = false;
        #endregion Global Setting

        #region Display Setting
        public bool Lock_Display_ExportSettings { get; set; } = false;
        public bool Lock_Display_BriCont { get; set; } = false;
        public bool Lock_Display_AutoBriTemp { get; set; } = false;
        public bool Lock_Display_NetworkKVM { get; set; } = false;
        public bool Lock_Display_ColorPreset { get; set; } = false;
        public bool Lock_Display_PowerNap { get; set; } = false;
        public bool Lock_Display_ResolutionRefreshRate { get; set; } = false;
        public bool Lock_Display_USBCPrioritization { get; set; } = false;
        public bool Lock_Display_ActiveInputSource { get; set; } = false;        
        public bool Lock_Display_RestoreFactoryDefaults { get; set; } = false;
        public bool Lock_Display_USBKVM { get; set; } = false;
        public bool Lock_Display_EasyArrangeLayout { get; set; } = false;
        #endregion Display Setting

        #region Peripheral Setting
        public bool Lock_Webcam_RestoreFactoryDefaults { get; set; } = false;
        public bool Lock_Audio_RestoreFactoryDefaults { get; set; } = false;
        public bool Lock_Keyboard_RestoreFactoryDefaults { get; set; } = false;
        public bool Lock_Mouse_RestoreFactoryDefaults { get; set; } = false;
        public bool Lock_Pen_RestoreFactoryDefaults { get; set; } = false;
        public bool Lock_Keyboard_CollabScreenShare { get; set; } = false;
        public bool Lock_Audio_ancMode { get; set; } = false;
        public bool Lock_Audio_micNoiseCancellation { get; set; } = false;
        public bool Lock_Audio_wearDetection { get; set; } = false;
        public bool Lock_Webcam_hdr { get; set; } = false;
        public bool Lock_Webcam_AntiFlicker { get; set; } = false;
        public bool Lock_Webcam_AIAutoFraming { get; set; } = false;
        public bool Lock_Webcam_MicSwitch { get; set; } = false;
        public bool Lock_Webcam_PresenceDetection { get; set; } = false;
        #endregion Peripheral Setting

        public GlobalSettingParam global_setting { get; set; } = new GlobalSettingParam();
    }
}