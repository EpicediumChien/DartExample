namespace DDPM.RemoteManagement.Common.Interfaces
{
    public static class Params
    {

        public static class EventType
        {
            public const int GET = 1;
            public const int SET = 2;

            public const int LOCK = 3;
            public const int UNLOCK = 4;

            public const int FW = 5;

            public const int DISPLAY_CONNECT = 11;
            public const int DISPLAY_DISCONNECT = 12;

            public const int UNKNOWN_ERROR = 99;
        }


        public static class Active
        {
            public const string GET = "get";
            public const string SET = "set";
            public const string LOCK = "lock";
            public const string UNLOCK = "unlock";
            public const string FW = "fw";
        }

        public static class DeviceType
        {
            public const string APP = "app";
            public const string DISPLAY = "display";
            public const string WEBCAM = "webcam";
            public const string AUDIO = "audio";
            public const string KEYBOARD = "keyboard";
            public const string MOUSE = "mouse";
            public const string PEN = "pen";
            public const string DOCK = "dock";
            public const string SPEAKER = "speaker";
            public const string HEADSET = "headset";
            public const string DONGLE = "dongle";
        }

        // 0 : success
        // 1 - 99 : Not Fail
        // 100+ : Fail or Error
        public static class Response
        {
            public const int STATUS_COMMAND_SUCCESS = 0;

            // 51 - 59 : fw update result not fail
            public const int STATUS_FW_UPDATE_AT_MINVERSION_OR_LATER = 51;
            public const int STATUS_FW_UPDATE_AT_LATEST = 52;


            // 101 - 129 : fw update Fail or Error
            public const int STATUS_FW_UPDATE_DEVICE_NOT_FOUND = 101;
            public const int STATUS_FW_UPDATE_PENDING = 102;

            public const int STATUS_FW_UPDATE_ABORTED = 104;
            public const int STATUS_FW_UPDATE_TIMEOUT_ERROR = 105;
            public const int STATUS_FW_UPDATE_CORRUPTION = 106;
            public const int STATUS_FW_UPDATE_FAILED = 107;

            // 500+ : Command Fail or Error
            public const int STATUS_COMMAND_TIMEOUT = 500;
            public const int STATUS_COMMAND_ERROR_FORMAT_OR_PARAMS = 501;
            public const int STATUS_COMMAND_ERROR_RESULT = 502;
            public const int STATUS_COMMAND_ERROR_RESULT_EXCEPTION = 503;

            public static int UNKNOWN_ERROR = 9999;

        }


        public static class Lock
        {
            public const string InAppUpdate = "inappupdate";
            public const string InAppRestoreDefault = "inapprestoredefault";
            public const string RestoreFactoryDefaults = "restorefactorydefaults";
            public const string ScreenNotification = "screennotification ";
            public const string TelemetryConsent = "telemetryconsent";
            public const string InAppBriCont = "inappbricont";
            public const string InAppAutoBriTemp = "inappautobritemp";  // #5.13.7
            //public const string InAppAutoBriTemp = "inappautobritemp";  // #5.13.8
            public const string PrimaryMonitorSync = "primarymonitorsync";
            public const string ResolutionRefreshRate = "resolutionrefreshrate";
            public const string USBCPrioritization = "usbcprioritization";
            public const string InAppUSBKVM = "inappusbkvm";  // #5.13.12
            public const string InAppNetworkKVM = "inappnetworkkvm";
            public const string InAppColorPreset = "inappcolorpreset";
            public const string PowerNap = "powernap";
            public const string InAppExportSettings = "inappexportsettings";
            public const string CollabScreenShare = "collabscreenshare ";
            public const string hdr = "hdr";
            public const string AntiFlicker = "antiflicker";
            public const string MicSwitch = "micswitch";
            public const string AIAutoFraming = "aiautoframing";
            public const string PresenceDetection = "presencedetection";  // #5.13.22
            public const string ancMode = "ancmode";  // #5.13.23
            public const string micNoiseCancellation  = "micnoisecancellation ";  // #5.13.24
            public const string wearDetection = "weardetection";  // #5.13.25

            // add @ 20241116 stephen
            public const string IsCollaborationScreenShareEnable = "IsCollaborationScreenShareEnable"; // CollabScreenShare
            public const string IsHDROn = "IsHDROn";  // hdr
            public const string IsMicEnumerationOn = "IsMicEnumerationOn";  // MicSwitch
            public const string IsAutoFramingOn = "IsAutoFramingOn";  // AIAutoFraming 
            public const string IsProximitySensorEnable = "IsProximitySensorEnable";  // PresenceDetection

        }

        public static class FwUpdateValues
        {
            public const string ForceWithNotice = "forcewithnotice";
            public const string ForceWithNonotice = "forcewithnonotice";
            public const string Defer = "defer";
        }
        public static class Dock
        {
            public const string FWVersion = "FWVersion";
            public const string SilentFWUpdate = "SilentFWUpdate";
        }

        public static class Pen
        {
            public const string FWVersion = "FWVersion";
            public const string RestoreFactoryDefaults = "RestoreFactoryDefaults";
        }

        public static class Mouse
        {
            public const string FWVersion = "FWVersion";
            public const string RestoreFactoryDefaults = "RestoreFactoryDefaults";
        }

        public static class Keyboard
        {
            public const string FWVersion = "FWVersion";
            public const string RestoreFactoryDefaults = "RestoreFactoryDefaults";
            public const string CollabCameraEnable = "CollabCameraEnable";
            public const string CollabMicMute = "CollabMicMute";
            public const string CallabScreenShare = "CallabScreenShare";
            public const string CollabChatEnable = "CollabChatEnable";
        }

        public static class Audio
        {
            public const string FWVersion = "FWVersion";
            public const string RestoreFactoryDefaults = "RestoreFactoryDefaults";
            public const string ancMode = "ancMode";
            public const string micNoiseCancellation = "micNoiseCancellation";
            public const string wearDetection = "wearDetection";
        }

        public static class Webcam
        {
            public const string FWVersion = "FWVersion";
            public const string RestoreFactoryDefaults = "RestoreFactoryDefaults";
            public const string FieldOfView = "FieldOfView";
            public const string hdr = "hdr";
            public const string AntiFlicker = "AntiFlicker";
            public const string MicSwitch = "MicSwitch";
            public const string AIAutoFraming = "AIAutoFraming";
            public const string PresenceDetection = "PresenceDetection";
        }

        public static class App
        {
            public const string Update = "Update";
            public const string UpdateSourceLocation = "UpdateSourceLocation";
            public const string FirmwareUpdate = "FirmwareUpdate";
            public const string UpdateAccess = "UpdateAccess";
            public const string TelemetryConsent = "TelemetryConsent";
            public const string ScreenNotification = "ScreenNotification";
            public const string ExportSettings = "ExportSettings";
            public const string RestoreFactoryDefaults = "RestoreFactoryDefaults";
            public const string ConnectedDevices = "ConnectedDevices";
            public const string DeviceData = "DeviceData";
            public const string DeviceConfiguration = "DeviceConfiguration";
            public const string DiagnosticsReport = "DiagnosticsReport";
        }

        public static class Display
        {

            public const string AutoColorPreset = "AutoColorPreset";
            public const string NetworkKVMVersion = "NetworkKVMVersion";
            public const string ChangeMonitorId = "ChangeMonitorId";
            public const string NetworkKVM = "NetworkKVM";
            public const string InAppNetworkKVM = "InAppNetworkKVM";
            public const string NetworkKVMAutoConnect = "NetworkKVMAutoConnect";
            public const string NetworkKVMContentTransfer = "NetworkKVMContentTransfer";
            public const string NetworkKVMIncomingPort = "NetworkKVMIncomingPort";
            public const string GetNetworkKVMIncomingPort = "GetNetworkKVMIncomingPort";
            public const string NetworkKVMOutgoingPort = "NetworkKVMOutgoingPort";
            public const string GetNetworkKVMOutgoingPort = "GetNetworkKVMOutgoingPort";
            public const string NetworkKVMContentTransferPort = "NetworkKVMContentTransferPort";
            public const string GetNetworkKVMContentTransferPort = "GetNetworkKVMContentTransferPort";
            public const string NetworkKVMAccessReset = "NetworkKVMAccessReset";
            public const string FWVersion = "FWVersion";
            public const string ActiveHours = "ActiveHours";
            public const string AutoBrightness = "AutoBrightness";
            public const string SwapVideo = "SwapVideo";
            public const string SwapUSB = "SwapUSB";
            public const string BrightnessLevel = "BrightnessLevel";
            public const string ContrastLevel = "ContrastLevel";
            public const string ColorPreset = "ColorPreset";
            public const string ActiveInputSource = "ActiveInputSource";
            public const string PxP = "PxP";
            public const string SubInput = "SubInput";
            public const string PxPZoom = "PxPZoom";
            public const string PowerSetting = "PowerSetting";
            public const string OSDLanguage = "OSDLanguage";
            public const string OSDAccess = "OSDAccess";
            public const string AutoBrightnessRangeLevel = "AutoBrightnessRangeLevel";
            public const string AutoTemp = "AutoTemp";
            public const string PrimaryMonitorSync = "PrimaryMonitorSync";
            public const string USBCPrioritization = "USBCPrioritization";
            public const string SpeakerMicrophone = "SpeakerMicrophone";
            public const string SpeakerVolume = "SpeakerVolume";
            public const string Microphone = "Microphone";
            public const string EnergySaver = "EnergySaver";
            public const string PowerNap = "PowerNap";
            public const string ColorManagement = "ColorManagement";
            public const string RestoreColorDefaults = "RestoreColorDefaults";
            public const string CurrentResolutionRefreshRate = "CurrentResolutionRefreshRate";
            public const string Resolution = "Resolution";
            public const string RefreshRate = "RefreshRate";
            public const string ResolutionRefreshRate = "ResolutionRefreshRate";
            public const string EasyArrangeLayout = "EasyArrangeLayout";
            public const string Orientation = "Orientation";
            public const string LockRotate = "LockRotate";
            public const string RotateOSDMenu = "RotateOSDMenu";
            public const string MonitorPower = "MonitorPower";

        }
    }
}
