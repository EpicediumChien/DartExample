using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDPM.SA.Plugins.CMAManager
{
    public static class Params
    {

        public static class EventType
        {
            public const int GET = 1;
            public const int SET = 2;
            public const int FW = 5;

            public const int DISPLAY_CONNECT = 11;
            public const int DISPLAY_DISCONNECT = 12;
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
            public const string DISPLAY = "DISPLAY";
            public const string APP = "APP";
            public const string WEBCAM = "WEBCAM";
            public const string AUDIO = "AUDIO";
            public const string KEYBOARD = "KEYBOARD";
            public const string MOUSE = "MOUSE";
            public const string PEN = "PEN";
            public const string DOCK = "DOCK";
        }

        public static class Response
        {
            public const int STATUS_COMMAND_SUCCESS = 0;
            public const int STATUS_COMMAND_TIMEOUT = 1;
            public const int STATUS_COMMAND_ERROR_FORMAT_OR_PARAMS = 11;

            public const int STATUS_FW_UPDATE_DEVICE_NOT_FOUND = 101;
            public const int STATUS_FW_UPDATE_PENDING = 102;

            public const int STATUS_FW_UPDATE_ABORTED = 104;
            public const int STATUS_FW_UPDATE_TIMEOUT_ERROR = 105;
            public const int STATUS_FW_UPDATE_CORRUPTION = 106;
            public const int STATUS_FW_UPDATE_FAILED = 107;
            public const int STATUS_FW_UPDATE_AT_MINVERSION_OR_LATER = 108;
            public const int STATUS_FW_UPDATE_AT_LATEST = 109;

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
