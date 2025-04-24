using DDPM.SA.Common.Settings;
using System.Collections.Generic;
using System.Linq;
using static DDPM.SA.Common.ICLICommandTable;

namespace DDPM.SA.Common
{
    public interface ICLIPeripherals : ICLIBase
    {
        //Write your own function here
    }

    public class CLI_PeripheralRESPONSE
    {
        public string Name { get; set; }
        public string Model { get; set; }
        public string ServiceTag { get; set; }
        public string Guid { get; set; }
        public string Command { get; set; }
        public string TargetFeature { get; set; }
        public string Value { get; set; } = "";
        public string Result { get; set; } = "";
        public string Message { get; set; } = "";

        private IDeviceManagerSA _devMgr;

        public CLI_PeripheralRESPONSE(string id, 
                                      string command, 
                                      string targetFeature, 
                                      string result = "", 
                                      string message = "", 
                                      string name = "N/A", 
                                      string model = "N/A", 
                                      string serviceTag = "N/A")
        {
            Guid = id;
            Command = command;
            TargetFeature = targetFeature;
            Result = result;
            Message = message;
            Name = name ?? "N/A";
            Model = model ?? "N/A";
            ServiceTag = serviceTag ?? "N/A";
        }

        public CLI_PeripheralRESPONSE(IDeviceManagerSA devMgr, DeviceInfo di, string deviceType, string targetFeature)
        {
            _devMgr = devMgr;
            bool retcode = false;
            int retvalue = 0;
            Name = di.Name;
            Model = di.ModelNumber;
            Guid = di.ID.ToString();
            ServiceTag = string.IsNullOrWhiteSpace(di.DockServiceTag) ? "N/A" : di.DockServiceTag;
            //Guid = "DellPeripheral.Webcam.0";
            Command = "GET";
            DDPMSettings data = _devMgr.ReloadAppConfigData().Result;

            if (data == null)
            {
                Value = "N/A";
                Result = "FAIL";
                Message = "Fail to get DDPMSettings";
                return;
            }

            switch (targetFeature)
            {
                case "FIELDOFVIEW":
                    //Guid = "DellPeripheral.Webcam.0";
                    if (_devMgr.GetIsPropertyFOVSupportedByDTP(Guid).Result)
                    {
                        retvalue = _devMgr.GetFieldOfView(Guid).Result;
                        Value = retvalue.ToString();
                        Result = "PASS";
                        Message = "N/A";
                        TargetFeature = targetFeature;
                    }
                    else
                    {
                        Value = "N/A";
                        Result = "FAIL";
                        Message = "Webcam not support Filed of View";
                    }
                    return;
                case "HDR":
                    //Guid = "DellPeripheral.Webcam.0";
                    if (_devMgr.GetIsPropertyHDRSupported(Guid).Result)
                    {
                        retcode = _devMgr.GetIsHDROn(Guid).Result;
                        Value = (retcode) ? "ON" : "OFF";
                        //Value += "," + (data.LockSettings.Lock_Webcam_hdr ? "LOCK" : "UNLOCK");
                        Result = "PASS";
                        Message = "N/A";
                        TargetFeature = targetFeature;
                    }
                    else
                    {
                        Value = "N/A";
                        Result = "FAIL";
                        Message = "Webcam not support HDR";
                    }
                    return;
                case "ANTIFLICKER":
                    //Guid = "DellPeripheral.Webcam.0";
                    if (_devMgr.GetIsPropertyAntiFlickerSupported(Guid).Result)
                    {
                        retvalue = _devMgr.GetAntiFlicker(Guid).Result;
                        if (!string.IsNullOrEmpty(retvalue.ToString()))
                        {
                            switch (retvalue.ToString())
                            {
                                case "1":
                                    Value = "50";
                                    break;
                                case "2":
                                    Value = "60";
                                    break;
                                default:
                                    break;

                            }
                            //Value += "," + (data.LockSettings.Lock_Webcam_AntiFlicker ? "LOCK" : "UNLOCK");
                            Result = "PASS";
                            Message = "N/A";
                            TargetFeature = targetFeature;
                        }
                        else
                        {
                            Value = "N/A";
                            Result = "FAIL";
                            Message = "Interface is return null";
                        }
                    }
                    else
                    {
                        Value = "N/A";
                        Result = "FAIL";
                        Message = "Webcam not support AntiFlicker";
                    }
                    return;
                case "AIAUTOFRAMING":
                    //Guid = "DellPeripheral.Webcam.0";
                    if (_devMgr.GetIsPropertyAutoFramingSupported(Guid).Result)
                    {
                        retcode = _devMgr.GetIsAutoFramingOn(Guid).Result;
                        Value = (retcode) ? "ON" : "OFF";
                        //Value += "," + (data.LockSettings.Lock_Webcam_AIAutoFraming ? "LOCK" : "UNLOCK");
                        Result = "PASS";
                        Message = "N/A";
                        TargetFeature = targetFeature;
                    }
                    else
                    {
                        Value = "N/A";
                        Result = "FAIL";
                        Message = "Webcam not support AI AutoFraming";
                    }
                    return;
                case "MICSWITCH":
                    TargetFeature = targetFeature;
                    if (di.IsMicEnumerationSupported)
                    {
                        retcode = di.IsMicEnumerationOn;
                        Value = (retcode) ? "ON" : "OFF";
                        //Value += "," + (data.LockSettings.Lock_Webcam_MicSwitch ? "LOCK" : "UNLOCK");
                        Result = "PASS";
                        Message = "N/A";
                    }
                    else
                    {
                        Value = "N/A";
                        Result = "FAIL";
                        Message = "Webcam not support MicSwitch";
                    }
                    return;
                case "PRESENCEDETECTION":
                    if (di.IsESISupported)
                    {
                        retcode = _devMgr.GetIsProximitySensorEnable(Guid).Result;
                        Value = (retcode) ? "ON" : "OFF";
                        //Value += "," + (data.LockSettings.Lock_Webcam_PresenceDetection ? "LOCK" : "UNLOCK");
                        Result = "PASS";
                        Message = "N/A";
                        TargetFeature = targetFeature;
                    }
                    else
                    {
                        Value = "N/A";
                        Result = "FAIL";
                        Message = "Webcam not support PresenceDetection";
                    }
                    return;
                case "FWVERSION":
                    TargetFeature = "FIRMWAREVERSION";
                    break;
                case "COLLABCAMERAENABLE":
                    TargetFeature = targetFeature;
                    if (di.IsCollabsKeysSupported)
                    {
                        retcode = di.IsCollaborationCameraEnable;
                        Value = retcode ? "ON" : "OFF";
                        Result = "PASS";
                    }
                    else
                    {
                        Value = "N/A";
                        Result = "FAIL";
                        Message = "Keyboard not support COLLABCAMERAENABLE";
                    }
                    return;
                case "COLLABMICMUTE":
                    TargetFeature = targetFeature;
                    if (di.IsCollabsKeysSupported)
                    {
                        retcode = di.IsCollaborationMicEnable;
                        Value = retcode ? "ON" : "OFF";
                        Result = "PASS";
                    }
                    else
                    {
                        Value = "N/A";
                        Result = "FAIL";
                        Message = "Keyboard not support COLLABMICMUTE";
                    }
                    return;
                case "COLLABSCREENSHARE":
                    TargetFeature = targetFeature;
                    if (di.IsCollabsKeysSupported)
                    {
                        retcode = di.IsCollaborationScreenShareEnable;
                        Value = retcode ? "ON" : "OFF";
                        Result = "PASS";
                    }
                    else
                    {
                        Value = "N/A";
                        Result = "FAIL";
                        Message = "Keyboard not support COLLABSCREENSHARE";
                    }
                    return;
                case "COLLABCHATENABLE":
                    TargetFeature = targetFeature;
                    if (di.IsCollabsKeysSupported)
                    {
                        retcode = di.IsCollaborationChatEnable;
                        Value = retcode ? "ON" : "OFF";
                        Result = "PASS";
                    }
                    else
                    {
                        Value = "N/A";
                        Result = "FAIL";
                        Message = "Keyboard not support COLLABCHATENABLE";
                    }
                    return;
                case "ANCMODE":
                    TargetFeature = targetFeature;
                    if (!di.IsANCSupported)
                    {
                        Value = "N/A";
                        Result = "FAIL";
                        Message = "Audio not support ANCMODE";
                        return;
                    }
                    break;
                case "MICNOISECANCELLATION":
                    TargetFeature = targetFeature;
                    if (GlobalDefinitions.isSupport210)
                    {
                        var isAirAudio = di.LogicalDeviceType.Equals("LogicalAirAudio", System.StringComparison.OrdinalIgnoreCase);
                        if (isAirAudio)
                        {
                            var Airsupport = devMgr.GetAirAudioIsMicNoiseCancellationSupportedAsync(di.ID.ToString()).Result;
                            if (Airsupport)
                            {
                                retcode = devMgr.GetAirAudioMicNoiseCancellationAsync(di.ID.ToString()).Result;
                                Value = (retcode) ? "ON" : "OFF";
                                Result = "PASS";
                                Message = "N/A";
                            }
                            else
                            {
                                Value = "N/A";
                                Result = "FAIL";
                                Message = "AirAudio not support MICNOISECANCELLATION";
                            }
                            return;
                        }
                    }
                    var support = di.IsMicNoiseCancellationSupported;
                    if (support)
                    {
                        retcode = di.MicNoiseCancellation;
                        Value = (retcode) ? "ON" : "OFF";
                        Result = "PASS";
                        Message = "N/A";
                    }
                    else
                    {
                        Value = "N/A";
                        Result = "FAIL";
                        Message = "Audio not support MICNOISECANCELLATION";
                    }
                   
                    return;
                case "WEARDETECTION":
                    TargetFeature = targetFeature;
                    if (!di.IsWearDetectionSupported)
                    {
                        Value = "N/A";
                        Result = "FAIL";
                        Message = "Audio not support WEARDETECTION";
                        return;
                    }
                    break;
                default:
                    TargetFeature = targetFeature;
                    break;
            }
            var properties = Property.DeviceProperties[deviceType];
            if (properties.Contains(TargetFeature))
            {
                Result = "PASS";
                Message = "N/A";
                var type = di.GetType();
                foreach (var prop in type.GetProperties())
                {
                    if (prop.Name.ToUpper() == TargetFeature)
                    {
                        if (TargetFeature == "WEARDETECTION")
                        {
                            bool WearDetectionDTP = devMgr.GetWearDetectionAsync(di.ID.ToString()).Result;
                            if (WearDetectionDTP)
                                Value = "ON";
                            else
                                Value = "OFF";
                        }
                        else if (prop.GetValue(di).ToString().Equals("1") || prop.GetValue(di).ToString().ToUpper().Equals("TRUE"))
                            Value = "ON";
                        else if (prop.GetValue(di).ToString().Equals("0") || prop.GetValue(di).ToString().ToUpper().Equals("FALSE"))
                            Value = "OFF";
                        else if (prop.GetValue(di).ToString().Equals("2") || prop.GetValue(di).ToString().ToUpper().Equals("FALSE"))
                            Value = "OFF";
                        else
                        {
                            Value = prop.GetValue(di).ToString() ?? "";
                        }
                    }
                }
                switch (targetFeature)
                {
                    case "COLLABSCREENSHARE":
                        //Value += "," + (data.LockSettings.Lock_Keyboard_CollabScreenShare ? "LOCK" : "UNLOCK");
                        break;
                    case "ANCMODE":
                        //Value += "," + (data.LockSettings.Lock_Audio_ancMode ? "LOCK" : "UNLOCK");
                        break;
                    case "MICNOISECANCELLATION":
                        //Value += "," + (data.LockSettings.Lock_Audio_micNoiseCancellation ? "LOCK" : "UNLOCK");
                        break;
                    case "WEARDETECTION":
                        //Value += "," + (data.LockSettings.Lock_Audio_wearDetection ? "LOCK" : "UNLOCK");
                        break;
                    case "HDR":
                        //Value += "," + (data.LockSettings.Lock_Webcam_hdr ? "LOCK" : "UNLOCK");
                        break;
                    case "ANTIFLICKER":
                        //Value += "," + (data.LockSettings.Lock_Webcam_AntiFlicker ? "LOCK" : "UNLOCK");
                        break;
                    case "AIAUTOFRAMING":
                        //Value += "," + (data.LockSettings.Lock_Webcam_AIAutoFraming ? "LOCK" : "UNLOCK");
                        break;
                    case "MICSWITCH":
                        //Value += "," + (data.LockSettings.Lock_Webcam_MicSwitch ? "LOCK" : "UNLOCK");
                        break;
                    case "PRESENCEDETECTION":
                        //Value += "," + (data.LockSettings.Lock_Webcam_PresenceDetection ? "LOCK" : "UNLOCK");
                        break;
                }
            }
            else
            {
                Result = "FAIL";
                Message = "invalid TargetFeature";
            }
            TargetFeature = targetFeature;
        }

        public CLI_PeripheralRESPONSE(DeviceInfo di, string deviceType, string targetFeature)
        {
            Name = di.Name;
            Model = di.ModelNumber;
            Guid = di.ID.ToString();
            Command = "GET";
            DDPMSettings data = _devMgr.ReloadAppConfigData().Result;

            switch (targetFeature)
            {
                case "FWVERSION":
                    TargetFeature = "FIRMWAREVERSION";
                    break;
                case "COLLABCAMERAENABLE":
                    TargetFeature = "ISCOLLABORATIONCAMERAENABLE";
                    break;
                case "COLLABMICMUTE":
                    TargetFeature = "ISCOLLABORATIONMICENABLE";
                    break;
                case "COLLABSCREENSHARE":
                    TargetFeature = "ISCOLLABORATIONSCREENSHAREENABLE";
                    break;
                case "COLLABCHATENABLE":
                    TargetFeature = "ISCOLLABORATIONCHATENABLE";
                    break;
                default:
                    TargetFeature = targetFeature;
                    break;
            }

            var properties = Property.DeviceProperties[deviceType];
            if (properties.Contains(TargetFeature))
            {
                Result = "PASS";
                Message = "N/A";

                var type = di.GetType();
                foreach (var prop in type.GetProperties())
                {
                    if (prop.Name.ToUpper() == TargetFeature)
                    {
                        if (prop.GetValue(di).ToString().Equals("1") || prop.GetValue(di).ToString().ToUpper().Equals("TRUE"))
                            Value = "ENABLE";
                        else if (prop.GetValue(di).ToString().Equals("0") || prop.GetValue(di).ToString().ToUpper().Equals("FALSE"))
                            Value = "DISABLE";
                        else
                        {
                            Value = prop.GetValue(di).ToString() ?? "";
                        }
                    }
                }
                //if (targetFeature.Equals("COLLABSCREENSHARE"))
                //    Value += "," + (data.LockSettings.Lock_Keyboard_CollabScreenShare ? "LOCK" : "UNLOCK");
            }
            else
            {
                Result = "FAIL";
                Message = "invalid TargetFeature";
            }
            TargetFeature = targetFeature;
        }
    }

    public class CLI_PeripheralGetRESPONSE
    {
        public string Guid;
        public string Command = "GET";
        public string Result = "PASS";
        public string Message = "N/A";
        public string Name;
        public string Model;
        public string LogicalDeviceType;
        public Dictionary<string, string> Properties;

        public CLI_PeripheralGetRESPONSE(string guid, string message)
        {
            Guid = guid;
            Result = "FAIL";
            Message = message;
        }

        public CLI_PeripheralGetRESPONSE(DeviceInfo di)
        {
            Guid = di.ID.ToString();
            Name = di.Name;
            Model = di.ModelNumber;
            LogicalDeviceType = di.LogicalDeviceType.ToString();
            var properties = Property.DeviceProperties[di.LogicalDeviceType];
            Properties = new();
            var type = di.GetType();
            foreach (var prop in type.GetProperties().OrderBy(x => x.Name))
            {
                if (properties.Contains(prop.Name.ToUpper()))
                {
                    Properties.Add(prop.Name, prop.GetValue(di)?.ToString() ?? "");
                }
            }
        }

        //internal static List<string> KeyboardProperties;//Dean 0626 SAST issue
    }

    internal static class Property
    {
        internal static readonly List<string> Keyboard = new()//Dean 0626 SAST issue
        {
            //"BACKLIGHTINGCONTROLS",
            //"BACKLIGHTINGLEVEL",
            //"BACKLIGHTTABINDEX",
            //"BATTERYLEVEL",
            //"BATTERYSTATUS",
            //"COLLABSKEYSSUPPORTED",
            //"COLORCODE",
            "FIRMWAREVERSION",
            //"INSTANCEID",
            //"ISBATTERYLEVELSUPPORTED",
            //"ISCOLLABORATIONBLINKEFFECTENABLE",
            "ISCOLLABORATIONCAMERAENABLE",
            "ISCOLLABORATIONCHATENABLE",
            //"ISCOLLABORATIONDOUBLETAPENABLE",
            //"ISCOLLABORATIONKEYENABLE",
            "ISCOLLABORATIONMICENABLE",
            "ISCOLLABORATIONSCREENSHAREENABLE",
            //"ISCOLLABSKEYSSUPPORTED",
            //"ISILLUMINATIONSUPPORTED",
            //"PAIREDDEVICECOUNT",
            //"PAIREDHOSTNAME1",
            //"PAIREDHOSTNAME2",
            //"PAIREDHOSTNAME3",
            //"VISIBLEPAIREDHOSTNAME1",
            //"VISIBLEPAIREDHOSTNAME2",
            //"VISIBLEPAIREDHOSTNAME3"
        };

        internal static readonly List<string> Mouse = new()//Dean 0626 SAST issue
        {
            //"BATTERYLEVEL",
            //"BATTERYSTATUS",
            //"COLORCODE",
            //"DPIDELTA",
            //"DPILEVEL",
            //"DPIVALUE",
            //"DPIMAX",
            //"DPIMIN",
            "FIRMWAREVERSION",
            //"INSTANCEID",
            //"ISBATTERYLEVELSUPPORTED",
            //"ISDPILEVELCHANGEPENDING",
            //"ISDPILEVELSUPPORTED",
            //"ISDPIVALUECHANGEPENDING",
            //"ISDPIVALUESUPPORTED",
            //"ISREPORTRATESUPPORTED",
            //"ISTOUCHSCROLLSENSITIVITYSUPPORTED",
            //"MOUSEPRIMARYBUTTON",
            //"PAIREDDEVICECOUNT",
            //"PAIREDHOSTNAME1",
            //"PAIREDHOSTNAME2",
            //"PAIREDHOSTNAME3",
            //"REPORTRATE",
            //"STATUS",
            //"TOUCHSCROLLSENSITIVITYLEVEL",
            //"TOUCHSENSITIVITYLEVELVALUE",
            //"VISIBLEPAIREDHOSTNAME1",
            //"VISIBLEPAIREDHOSTNAME2",
            //"VISIBLEPAIREDHOSTNAME3"
        };

        internal static readonly List<string> Webcam = new()//Dean 0626 SAST issue
        {
            "FIRMWAREVERSION",
            //"INSTANCEID",
            //"ISBATTERYLEVELSUPPORTED",
         };

        internal static readonly List<string> Pen = new()
        {
            "FIRMWAREVERSION",
            //"INSTANCEID",
            //"",
        };

        internal static readonly List<string> Headset = new()//Dean 0626 SAST issue
        {
            "FIRMWAREVERSION",
            //"INSTANCEID",
            //"MUTESTATUS",
            "ANCMODE",
            "MICNOISECANCELLATION",
            "WEARDETECTION",
            "ISWEARDETECTIONCHECKED",
            "ISWEARDETECTIONSUPPORTED",
            "ISANCSUPPORTED",
        };

        internal static readonly List<string> AirAudio = new()//Kidd 20250122 add
        {
            "FIRMWAREVERSION",
            //"INSTANCEID",
            //"MUTESTATUS",
            "ANCMODE",
            "MICNOISECANCELLATION",
            "WEARDETECTION",
            "ISWEARDETECTIONCHECKED",
            "ISWEARDETECTIONSUPPORTED",
            "ISANCSUPPORTED",
        };

        internal static readonly List<string> Audio = new()//Dean 0626 SAST issue
        {
            "FIRMWAREVERSION",
            //"INSTANCEID",
            //"MUTESTATUS",
            "ANCMODE",
            "MICNOISECANCELLATION",
            "WEARDETECTION",
            "ISWEARDETECTIONCHECKED",
            "ISWEARDETECTIONSUPPORTED",
            "ISANCSUPPORTED",
        };

        internal static readonly List<string> Dock = new()//Dean 0626 SAST issue
        {
            "FIRMWAREVERSION",
            //"INSTANCEID",
            //"",
        };

        internal static readonly Dictionary<string, List<string>> DeviceProperties = new()//Dean 0626 SAST issue
        {
            { "KEYBOARD", Keyboard },
            { "MOUSE", Mouse },
            { "WEBCAM", Webcam },
            { "PEN", Pen },
            { "HEADSET", Headset },
            { "AUDIO", Audio },
            { "DOCK", Dock },
            { "LOGICALWIREDAUDIO", Audio },
            { "AIRAUDIO", AirAudio },
            { "LOGICALAIRAUDIO", AirAudio }
        };
    }
}