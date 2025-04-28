using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using VcpCore.Common;
using static DDPM.SA.Common.ICLICommandTable;

namespace DDPM.SA.Common
{
    internal interface ICLIResponse
    {
    }

    public class CLI_RESPONSE
    {
        public string Model { get; set; } = "N/A";
        public string SerialNumber { get; set; } = "N/A";
        public string MarketingName { get; set; } = "N/A";
        public string Index { get; set; } = "N/A";
        public string ServiceTag { get; set; } = "N/A";
        public string Command { get; set; } = "N/A";
        public string TargetFeature { get; set; } = "N/A";
        public string Value { get; set; } = "N/A";
        public string Result { get; set; } = "N/A";
        public string Message { get; set; } = "N/A";

        public CLI_RESPONSE(MonitorInfo monitor)
        {
            Model = monitor.modelName;
            SerialNumber = monitor.edid.SerialNumber;
            MarketingName = monitor.MarketingName;
            Index = (monitor.Index + 1).ToString();
            ServiceTag = monitor.edid.ServiceTag;
        }

        public CLI_RESPONSE()
        {
        }

        public string ToJson()
        {
            try
            {
                return JsonConvert.SerializeObject(this, Formatting.Indented);
            }
            catch (Exception ex)
            {
#if DEBUG
                Console.WriteLine($"[CLI_RESPONSE] ToJson exception, message: {ex.Message}");
#endif
                return string.Empty;
            }
        }

        public string OutputLog(object o, CommandLineInput commandLineInput)
        {
            if (!string.IsNullOrEmpty(commandLineInput.LogPath))
            {
                if (!Directory.Exists(Path.GetDirectoryName(commandLineInput.LogPath)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(commandLineInput.LogPath));
                }

                try
                {
                    using (StreamWriter sw = new StreamWriter(commandLineInput.LogPath, true))// 'true':新建或附加.'false',或沒填:新建或覆蓋.
                    {
                        sw.WriteLine(DateTime.Now);
                        sw.WriteLine(JsonConvert.SerializeObject(o, Formatting.Indented));
                    }
                }
                catch (Exception ex) 
                {
#if DEBUG
                    Console.WriteLine($"[CLI_RESPONSE] OutputLog exception, message: {ex.Message}");
#endif
                }
                
            }
//#if DEBUG //Dean 0122 should keep response to console window for CLI
            Console.WriteLine(JsonConvert.SerializeObject(o, Formatting.Indented));
//#endif
            return JsonConvert.SerializeObject(o, Formatting.Indented);
        }
    }

    public class CLI_Read_EDID_RESPONSE : CLI_RESPONSE
    {
        public CLI_Read_EDID_RESPONSE(MonitorInfo monitor) : base(monitor)
        {
        }

        public CLI_Read_EDID_RESPONSE() : base()
        {
        }

        public string AliasDeviceName { get; set; } = "N/A";
        public string Edid { get; set; } = "N/A";
        public string ManufactureID { get; set; } = "N/A";
        public string VendorID { get; set; } = "N/A";
        public string ModelName { get; set; } = "N/A";
        public new string SerialNumber { get; set; } = "N/A";
        public string Week { get; set; } = "N/A";
        public string Month { get; set; } = "N/A";
        public string Year { get; set; } = "N/A";
        public string EdidVersion { get; set; } = "N/A";
        public string VideoInputType { get; set; } = "N/A";
        public string Size { get; set; } = "N/A";
        public string PID { get; set; }
    }

    public class CLI_Get_EDID_RESPONSE : CLI_RESPONSE
    {
        public CLI_Get_EDID_RESPONSE(MonitorInfo monitor) : base(monitor)
        {
        }

        public CLI_Get_EDID_RESPONSE() : base()
        {
        }

        public string EDID_RAW { get; set; } = "N/A";
    }

    public class ConnectedDevices
    {
        public string Index { get; set; }
        public string DeviceType { get; set; }
        public string Model { get; set; }
        public string PID { get; set; }
        public string ServiceTag { get; set; }
        //public string PPID { get; set; }
        public string SerialNumber { get; set; }
        public string FirmwareVersion { get; set; }
        public string Result { get; set; }
        public string Message { get; set; }

        public ConnectedDevices()
        {
            Index = "N/A";
            DeviceType = "Display";
            Model = "N/A";
            PID = "N/A";
            ServiceTag = "N/A";
            //PPID = "N/A";
            SerialNumber = "N/A";
            ServiceTag = "N/A";
            FirmwareVersion = "N/A";
            Result = "N/A";
            Message = "N/A";
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        public string OutputLog(object o, CommandLineInput commandLineInput)
        {
            if (!string.IsNullOrEmpty(commandLineInput.LogPath))
            {
                if (!Directory.Exists(Path.GetDirectoryName(commandLineInput.LogPath)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(commandLineInput.LogPath));
                }

                try
                {
                    using (StreamWriter sw = new StreamWriter(commandLineInput.LogPath, true))// 'true':新建或附加.'false',或沒填:新建或覆蓋.
                    {
                        sw.WriteLine(DateTime.Now);
                        sw.WriteLine(JsonConvert.SerializeObject(o, Formatting.Indented));
                    }
                }
                catch (Exception ex)
                {
#if DEBUG
                    Console.WriteLine($"[CLI_RESPONSE] OutputLog exception, message: {ex.Message}");
#endif
                }

            }
//#if DEBUG //Dean 0122 should keep response to console window for CLI
            Console.WriteLine(JsonConvert.SerializeObject(o, Formatting.Indented));
//#endif
            return JsonConvert.SerializeObject(o, Formatting.Indented);
        }

    }

    public class CLI_Get_MONITORS_RESPONSE : CLI_RESPONSE
    {
        public List<string> Monitors { get; set; }

        public CLI_Get_MONITORS_RESPONSE()
        {
            Monitors = new List<string>();
        }
    }

    public class CLI_Get_Luminus_RESPONSE : CLI_RESPONSE
    {
        public CLI_Get_Luminus_RESPONSE(MonitorInfo monitor) : base(monitor)
        {
        }

        public CLI_Get_Luminus_RESPONSE() : base()
        {
        }

        public string Luminus { get; set; } = "N/A";
    }

    public class CLI_InputList_RESPONSE : CLI_RESPONSE
    {
        public CLI_InputList_RESPONSE(MonitorInfo monitor) : base(monitor)
        {
        }

        public CLI_InputList_RESPONSE() : base()
        {
        }

        public List<string> InputSourceList { get; set; }
    }

    public class CLI_Get_Properties_HDR_RESPONSE : CLI_RESPONSE
    {
        public string IsSupportedHDR { get; set; }
        public string HDR { get; set; }

        public CLI_Get_Properties_HDR_RESPONSE(CLI_RESPONSE cli_RESPONSE)
        {
            this.Index = cli_RESPONSE.Index;
            this.ServiceTag = cli_RESPONSE.ServiceTag;
            this.Command = cli_RESPONSE.Command;
            this.TargetFeature = cli_RESPONSE.TargetFeature;
            this.SerialNumber = cli_RESPONSE.SerialNumber;
            this.Model = cli_RESPONSE.Model;
            this.MarketingName = cli_RESPONSE.MarketingName;
        }
    }

    public class CLI_Get_Properties_USBCPrioritization_RESPONSE : CLI_RESPONSE
    {
        //public string SupportedUSBCPrioritization { get; set; }
        //public string USBCPrioritizationType { get; set; }

        public CLI_Get_Properties_USBCPrioritization_RESPONSE(CLI_RESPONSE cli_RESPONSE)
        {
            this.Index = cli_RESPONSE.Index;
            this.ServiceTag = cli_RESPONSE.ServiceTag;
            this.Command = cli_RESPONSE.Command;
            this.TargetFeature = cli_RESPONSE.TargetFeature;
            this.SerialNumber = cli_RESPONSE.SerialNumber;
            this.Model = cli_RESPONSE.Model;
            this.MarketingName = cli_RESPONSE.MarketingName;
        }
    }

    public class CLI_Get_Properties_Orientation_RESPONSE : CLI_RESPONSE
    {
        //public string Orientation { get; set; }

        public CLI_Get_Properties_Orientation_RESPONSE(CLI_RESPONSE cli_RESPONSE)
        {
            //this.Orientation = "N/A";
            this.Index = cli_RESPONSE.Index;
            this.ServiceTag = cli_RESPONSE.ServiceTag;
            this.Command = cli_RESPONSE.Command;
            this.TargetFeature = cli_RESPONSE.TargetFeature;
            this.SerialNumber = cli_RESPONSE.SerialNumber;
            this.Model = cli_RESPONSE.Model;
            this.MarketingName = cli_RESPONSE.MarketingName;
        }
    }

    public class CLI_Get_Properties_CurrentResolutionRefreshRate_RESPONSE : CLI_RESPONSE
    {
        //public string CurrentResolutionRefreshRate { get; set; }
        public string BitsPerPixel { get; set; }

        public CLI_Get_Properties_CurrentResolutionRefreshRate_RESPONSE(CLI_RESPONSE cli_RESPONSE)
        {
            this.Index = cli_RESPONSE.Index;
            this.ServiceTag = cli_RESPONSE.ServiceTag;
            this.Command = cli_RESPONSE.Command;
            this.TargetFeature = cli_RESPONSE.TargetFeature;
            this.SerialNumber = cli_RESPONSE.SerialNumber;
            this.Model = cli_RESPONSE.Model;
            this.MarketingName = cli_RESPONSE.MarketingName;
        }
    }

    public class CLI_Get_Properties_SupportedResolutionRefreshRate_RESPONSE
    {
        public string Model { get; set; } = "N/A";
        public string SerialNumber { get; set; } = "N/A";
        public string MarketingName { get; set; } = "N/A";
        public string Index { get; set; } = "N/A";
        public string ServiceTag { get; set; } = "N/A";
        public string Command { get; set; } = "N/A";
        public string TargetFeature { get; set; } = "N/A";
        public string Result { get; set; } = "N/A";
        public string Message { get; set; } = "N/A";
        public List<string> AllResolutionRefreshRate { get; set; }

        public CLI_Get_Properties_SupportedResolutionRefreshRate_RESPONSE(CLI_RESPONSE cli_RESPONSE)
        {
            AllResolutionRefreshRate = new List<string>();
            this.Index = cli_RESPONSE.Index;
            this.ServiceTag = cli_RESPONSE.ServiceTag;
            this.Command = cli_RESPONSE.Command;
            this.TargetFeature = cli_RESPONSE.TargetFeature;
            this.SerialNumber = cli_RESPONSE.SerialNumber;
            this.Model = cli_RESPONSE.Model;
            this.MarketingName = cli_RESPONSE.MarketingName;
        }
    }

    //public class CLI_Get_Properties_SupportedResolutionRefreshRate_RESPONSE : CLI_RESPONSE
    //{
    //    public List<string> AllResolutionRefreshRate { get; set; }

    //    public CLI_Get_Properties_SupportedResolutionRefreshRate_RESPONSE(CLI_RESPONSE cli_RESPONSE)
    //    {
    //        AllResolutionRefreshRate = new List<string>();
    //        this.Index = cli_RESPONSE.Index;
    //        this.ServiceTag = cli_RESPONSE.ServiceTag;
    //        this.Command = cli_RESPONSE.Command;
    //        this.TargetFeature = cli_RESPONSE.TargetFeature;
    //        this.SerialNumber = cli_RESPONSE.SerialNumber;
    //        this.Model = cli_RESPONSE.Model;
    //    }
    //}

    public class CLI_Get_Properties_SupportedOrientation_RESPONSE : CLI_RESPONSE
    {
        public List<string> SupportedOrientation { get; set; }

        public CLI_Get_Properties_SupportedOrientation_RESPONSE(CLI_RESPONSE cli_RESPONSE)
        {
            SupportedOrientation = new List<string>();
            this.Index = cli_RESPONSE.Index;
            this.ServiceTag = cli_RESPONSE.ServiceTag;
            this.Command = cli_RESPONSE.Command;
            this.TargetFeature = cli_RESPONSE.TargetFeature;
            this.SerialNumber = cli_RESPONSE.SerialNumber;
            this.Model = cli_RESPONSE.Model;
            this.MarketingName = cli_RESPONSE.MarketingName;
        }
    }

    public class CLI_FWU_RESPONSE : CLI_RESPONSE
    {
        public string FWVersion { get; set; }
        public List<string> FWUpdateRESPONSE { get; set; }

        public CLI_FWU_RESPONSE(CLI_RESPONSE cli_RESPONSE)
        {
            FWUpdateRESPONSE = new List<string>();
            this.Index = cli_RESPONSE.Index;
            this.ServiceTag = cli_RESPONSE.ServiceTag;
            this.Command = cli_RESPONSE.Command;
            this.TargetFeature = cli_RESPONSE.TargetFeature;
            this.SerialNumber = cli_RESPONSE.SerialNumber;
            this.Model = cli_RESPONSE.Model;
            this.MarketingName = cli_RESPONSE.MarketingName;
        }
    }

    public class CLI_SWU_RESPONSE : CLI_RESPONSE
    {
        public string SWVersion { get; set; }
        public string SWname { get; set; }
        public List<string> SWUpdateRESPONSE { get; set; }

        public CLI_SWU_RESPONSE(CLI_RESPONSE cli_RESPONSE)
        {
            SWUpdateRESPONSE = new List<string>();
            this.Index = cli_RESPONSE.Index;
            this.ServiceTag = cli_RESPONSE.ServiceTag;
            this.Command = cli_RESPONSE.Command;
            this.TargetFeature = cli_RESPONSE.TargetFeature;
            this.SerialNumber = cli_RESPONSE.SerialNumber;
            this.Model = cli_RESPONSE.Model;
            this.MarketingName = cli_RESPONSE.MarketingName;
        }
    }

    #region ColorProfile
    // jim modify 20240608
    public class CLI_Set_AllActiveColorPreset_RESPONSE : CLI_RESPONSE
    {
        public CLI_Set_AllActiveColorPreset_RESPONSE(MonitorInfo monitor) : base(monitor)
        {
        }

        public CLI_Set_AllActiveColorPreset_RESPONSE() : base()
        {
        }

        public string Set_AllActiveColorPreset { get; set; }
    }

    // jim modify 20240608
    public class CLI_Set_AllMonitorProfile_RESPONSE : CLI_RESPONSE
    {
        public CLI_Set_AllMonitorProfile_RESPONSE(MonitorInfo monitor) : base(monitor)
        {
        }

        public CLI_Set_AllMonitorProfile_RESPONSE() : base()
        {
        }

        public string Set_AllMonitorProfile { get; set; }
    }

    // jim modify 20240608
    public class CLI_Get_AllMonitorProfile_RESPONSE : CLI_RESPONSE
    {
        public CLI_Get_AllMonitorProfile_RESPONSE(MonitorInfo monitor) : base(monitor)
        {
        }

        public CLI_Get_AllMonitorProfile_RESPONSE() : base()
        {
        }

        public string Get_AllMonitorProfile { get; set; }
    }

    #endregion ColorProfile

    #region Pxp - Robert_Lin added 2024-6-12

    public class CLI_RESPONSE_PxpMode : CLI_RESPONSE
    {
        public CLI_RESPONSE_PxpMode(MonitorInfo monitor) : base(monitor)
        {
        }

        public CLI_RESPONSE_PxpMode() : base()
        {
        }

        public string[] SupportedModes { get; set; }
        public string CurrentMode { get; set; }
        public UInt16 CurrentModeCode { get; set; }
    }

    public class CLI_RESPONSE_SubInput : CLI_RESPONSE
    {
        public CLI_RESPONSE_SubInput(MonitorInfo monitor) : base(monitor)
        {
        }

        public CLI_RESPONSE_SubInput() : base()
        {
        }

        // public int SubInputCount { get; set; } = 0;
        public string Sub1InputSource { get; set; }
        public string Sub2InputSource { get; set; }
        public string Sub3InputSource { get; set; }
    }

    #endregion Pxp - Robert_Lin added 2024-6-12

    public class Get_DeviceData
    {
        public Get_DeviceData(MonitorInfo monitor)
        {
            Model = monitor.modelName;
            SerialNumber = monitor.edid.SerialNumber;
            MarketingName = monitor.MarketingName;
            Index = (monitor.Index + 1).ToString();
            ServiceTag = monitor.edid.ServiceTag;
        }

        public Get_DeviceData()
        {
        }

        public string Index { get; set; }
        public string DeviceType { get; set; } = "Display";
        public string Model { get; set; }
        public string MarketingName { get; set; }
        public string SerialNumber { get; set; }
        public string ServiceTag { get; set; }
        public string Manufacturer { get; set; } = "N/A";
        public string ManufacturingYear { get; set; } = "N/A";
        public string ManufacturingWeek { get; set; } = "N/A";
        public string FirmwareVersion { get; set; } = "N/A";
        public string MonitorActiveHour { get; set; } = "N/A";
        public string DisplayTechnologyType { get; set; } = "N/A";
        public string ScreenSize { get; set; } = "N/A";
        public string OptimalResolution { get; set; } = "N/A";
        public string Resolution { get; set; } = "N/A";
        public string ActiveInputSource { get; set; } = "N/A";
        public string ColorPreset { get; set; } = "N/A";
        public string Orientation { get; set; } = "N/A";
        public string BrightnessLevel { get; set; } = "N/A";
        public string ContrastLevel { get; set; } = "N/A";
        public string LuminanceLevel { get; set; } = "N/A";
        public string AutoBrightness { get; set; } = "N/A";
        public string AutoBrightnessRangeLevel { get; set; } = "N/A";
        public string AutoColorTemp { get; set; } = "N/A";
        public string PrimaryMonitorForSync { get; set; } = "N/A";
        public string AspectRatio { get; set; } = "N/A";
        public string USB_CPrioritization { get; set; } = "N/A";
        public string ColorManagement { get; set; } = "N/A";
        public string SpeakerMicrophone { get; set; } = "N/A";
        public string SpeakerVolume { get; set; } = "N/A";
        public string MicrophoneControl { get; set; } = "N/A";
        //public string Uniformity { get; set; } = "N/A";
        public string PowerNap { get; set; } = "N/A";
        public string OSD_language { get; set; } = "N/A";
        public string PID { get; set; } = "N/A";

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        public string OutputLog(object o, CommandLineInput commandLineInput)
        {
            if (!string.IsNullOrEmpty(commandLineInput.LogPath))
            {
                if (!Directory.Exists(Path.GetDirectoryName(commandLineInput.LogPath)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(commandLineInput.LogPath));
                }

                try
                {
                    using (StreamWriter sw = new StreamWriter(commandLineInput.LogPath, true))// 'true':新建或附加.'false',或沒填:新建或覆蓋.
                    {
                        sw.WriteLine(DateTime.Now);
                        sw.WriteLine(JsonConvert.SerializeObject(o, Formatting.Indented));
                    }
                }
                catch (Exception ex)
                {
#if DEBUG
                    Console.WriteLine($"[CLI_RESPONSE] OutputLog exception, message: {ex.Message}");
#endif
                }
                
            }
//#if DEBUG //Dean 0122 should keep response to console window for CLI
            Console.WriteLine(JsonConvert.SerializeObject(o, Formatting.Indented));
//#endif
            return JsonConvert.SerializeObject(o, Formatting.Indented);
        }
    }

    public class PeripheralResponse
    {
        public PeripheralResponse()
        {

        }

        public PeripheralResponse(int index, DeviceInfo deviceInfo)
        {
            ID = deviceInfo.ID.ToString();
            Index = index.ToString();
            Model = deviceInfo.ModelNumber;
            //ServiceTag = string.IsNullOrEmpty( deviceInfo.DockServiceTag) ? "N/A":deviceInfo.DockServiceTag;
            FirmwareVersion = deviceInfo.FirmwareVersion;
            BatteryStatus = deviceInfo.BatteryStatus;
            DeviceType = deviceInfo.LogicalDeviceType;
            Connectiontype = (deviceInfo.PhysicalDeviceType.ToString().Contains("Dongle") || deviceInfo.PhysicalDeviceType.ToString().Contains("Bluetooth") || deviceInfo.PhysicalDeviceType.ToString().Contains("Pen")) ? "Wireless" : "Wired";
        }

        public string ID { get; set; } = "N/A";
        public string Index { get; set; } = "N/A";
        public string Model { get; set; } = "N/A";
        public string Manufacturer { get; set; } = "N/A";
        public string PID { get; set; } = "N/A";
        public string ServiceTag { get; set; } = "N/A";
        public string PPID { get; set; } = "N/A";
        public string SerialNumber { get; set; } = "N/A";
        public string ManufacturingYear { get; set; } = "N/A";
        public string ManufacturingWeek { get; set; } = "N/A";
        public string FirmwareVersion { get; set; } = "N/A";
        public string Connectiontype { get; set; } = "N/A";
        public string BatteryStatus { get; set; } = "N/A";
        public string DeviceType { get; set; } = "N/A";

        public string ToJson()
        {
            try
            {
                return JsonConvert.SerializeObject(this, Formatting.Indented);
            }
            catch (Exception ex)
            {
#if DEBUG
                Console.WriteLine($"[PeripheralResponse] ToJson exception, message: {ex.Message}");
#endif
                return string.Empty;
            }
        }
    }

    public class DeviceDataWebcamResponse : PeripheralResponse
    {
        public DeviceDataWebcamResponse(int index, DeviceInfo deviceInfo) : base(index, deviceInfo)
        {
        }

        public DeviceDataWebcamResponse() : base()
        {
        }

        public string FieldOfView { get; set; } = "N/A";
        public string HDR { get; set; } = "N/A";
        public string AntiFlicker { get; set; } = "N/A";
        public string MicSwitch { get; set; } = "N/A";
        public string AIAutoFraming { get; set; } = "N/A";
        public string PresenceDetection { get; set; } = "N/A";
    }

    public class DeviceDataAudioResponse : PeripheralResponse
    {
        public DeviceDataAudioResponse(int index, DeviceInfo deviceInfo) : base(index, deviceInfo)
        {
        }

        public DeviceDataAudioResponse() : base()
        {
        }

        public string ANCMode { get; set; } = "N/A";
        public string MicNoiseCancellation { get; set; } = "N/A";
        public string WearDetection { get; set; } = "N/A";
    }

    public class DeviceDataKeyboardResponse : PeripheralResponse
    {
        public DeviceDataKeyboardResponse(int index, DeviceInfo deviceInfo) : base(index, deviceInfo)
        {
            CollabCameraEnable = deviceInfo.IsCollabsKeysSupported ? (deviceInfo.IsCollaborationCameraEnable ? "ON" : "OFF") : "N/A";
            CollabMicMute = deviceInfo.IsCollabsKeysSupported ? (deviceInfo.IsCollaborationMicEnable ? "ON" : "OFF") : "N/A";
            CollabScreenShare = deviceInfo.IsCollabsKeysSupported ? (deviceInfo.IsCollaborationScreenShareEnable ? "ON" : "OFF") : "N/A";
            CollabChatEnable = deviceInfo.IsCollabsKeysSupported ? (deviceInfo.IsCollaborationChatEnable ? "ON" : "OFF") : "N/A";
        }

        public DeviceDataKeyboardResponse() : base()
        {
        }

        public string CollabCameraEnable { get; set; } = "N/A";
        public string CollabMicMute { get; set; } = "N/A";
        public string CollabScreenShare { get; set; } = "N/A";
        public string CollabChatEnable { get; set; } = "N/A";
    }

    public class DeviceDataMouseResponse : PeripheralResponse
    {
        public DeviceDataMouseResponse(int index, DeviceInfo deviceInfo) : base(index, deviceInfo)
        {
        }

        public DeviceDataMouseResponse() : base()
        {
        }
    }

    public class DeviceDataPenResponse : PeripheralResponse
    {
        public DeviceDataPenResponse(int index, DeviceInfo deviceInfo) : base(index, deviceInfo)
        {
        }

        public DeviceDataPenResponse() : base()
        {
        }
    }

    public class DeviceDataDockResponse : PeripheralResponse
    {
        public DeviceDataDockResponse(int index, DeviceInfo deviceInfo) : base(index, deviceInfo)
        {
        }

        public DeviceDataDockResponse() : base()
        {
        }
    }

    public class SetDeviceConfigResponse : CLI_RESPONSE3
    {
        public new List<string> Message { get; set; } = new List<string>();
    }

    public class CLI_RESPONSE2
    {
        public string Index { get; set; }
        public Guid ID { get; set; }
        public string FirmwareVersion { get; set; }
        public string Model { get; set; }
        public string Connectiontype { get; set; }
        public string BatteryStatus { get; set; }
        public string ServiceTag { get; set; } = "N/A";

        public CLI_RESPONSE2()
        {
            Index = "N/A";
            FirmwareVersion = "N/A";
            Model = "N/A";
            Connectiontype = "N/A";
            BatteryStatus = "N/A";
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        public string OutputLog(object o, CommandLineInput commandLineInput)
        {
            if (!string.IsNullOrEmpty(commandLineInput.LogPath))
            {
                if (!Directory.Exists(Path.GetDirectoryName(commandLineInput.LogPath)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(commandLineInput.LogPath));
                }

                try
                {
                    using (StreamWriter sw = new StreamWriter(commandLineInput.LogPath, true))// 'true':新建或附加.'false',或沒填:新建或覆蓋.
                    {
                        sw.WriteLine(DateTime.Now);
                        sw.WriteLine(JsonConvert.SerializeObject(o, Formatting.Indented));
                    }
                }
                catch (Exception ex)
                {
#if DEBUG
                    Console.WriteLine($"[CLI_RESPONSE] OutputLog exception, message: {ex.Message}");
#endif
                }                
            }
//#if DEBUG //Dean 0122 should keep response to console window for CLI
            Console.WriteLine(JsonConvert.SerializeObject(o, Formatting.Indented));
//#endif
            return JsonConvert.SerializeObject(o, Formatting.Indented);
        }
    }

    public class CLI_RESPONSE3
    {
        public string Command { get; set; }
        public string TargetFeature { get; set; }
        public string Result { get; set; }
        public string Message { get; set; }

        public CLI_RESPONSE3()
        {
            Command = "N/A";
            TargetFeature = "N/A";
            Result = "N/A";
            Message = "N/A";
        }

        public string ToJson()
        {
            try
            {
                return JsonConvert.SerializeObject(this, Formatting.Indented);
            }
            catch (Exception ex)
            {
#if DEBUG
                Console.WriteLine($"[CLI_RESPONSE3] ToJson exception, message: {ex.Message}");
#endif
                return string.Empty;
            }
        }

        public string OutputLog(object o, CommandLineInput commandLineInput)
        {
            if (!string.IsNullOrEmpty(commandLineInput.LogPath))
            {
                if (!Directory.Exists(Path.GetDirectoryName(commandLineInput.LogPath)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(commandLineInput.LogPath));
                }
                
                try
                {
                    using (StreamWriter sw = new StreamWriter(commandLineInput.LogPath, true))// 'true':新建或附加.'false',或沒填:新建或覆蓋.
                    {
                        sw.WriteLine(DateTime.Now);
                        sw.WriteLine(JsonConvert.SerializeObject(o, Formatting.Indented));
                    }
                }
                catch (Exception ex)
                {
#if DEBUG
                    Console.WriteLine($"[CLI_RESPONSE] OutputLog exception, message: {ex.Message}");
#endif
                }
            }
//#if DEBUG //Dean 0122 should keep response to console window for CLI
            Console.WriteLine(JsonConvert.SerializeObject(o, Formatting.Indented));
//#endif
            return JsonConvert.SerializeObject(o, Formatting.Indented);
        }
    }

    public class Get_Capabilitystring : CLI_RESPONSE
    {
        public Get_Capabilitystring(MonitorInfo monitor) : base(monitor)
        {
        }

        public Get_Capabilitystring() : base()
        {
        }

        public string CapabilityString { get; set; }
    }

    public class Apply_Configuration : CLI_RESPONSE
    {
        public Apply_Configuration(MonitorInfo monitor) : base(monitor)
        {
        }

        public Apply_Configuration() : base()
        {
        }

        //public string OptimalResolution { get; set; } = "N/A";
        public string Resolution { get; set; } = "N/A";
        public string ActiveInputSource { get; set; } = "N/A";
        public string ColorPreset { get; set; } = "N/A";
        public string ScreenOrientation { get; set; } = "N/A";
        public string BrightnessLevel { get; set; } = "N/A";
        public string ContrastLevel { get; set; } = "N/A";
        public string LuminanceLevel { get; set; } = "N/A";
        public string AutoBrightness { get; set; } = "N/A";
        public string AutoBrightnessRangeLevel { get; set; } = "N/A";
        public string AutoColorTemp { get; set; } = "N/A";
        public string PrimaryMonitorForSync { get; set; } = "N/A";
        public string AspectRatio { get; set; } = "N/A";
        public string USB_CPrioritization { get; set; } = "N/A";
        public string ColorManagement { get; set; } = "N/A";
        public string SpeakerMicrophone { get; set; } = "N/A";
        public string SpeakerVolume { get; set; } = "N/A";
        public string MicrophoneControl { get; set; } = "N/A";
        //public string Uniformity { get; set; } = "N/A";
        public string PowerNap { get; set; } = "N/A";
        public string OSD_language { get; set; } = "N/A";
    }

    public class NKVM_RESPONSE
    {
        //public string Model { get; set; }
        //public string SerialNumber { get; set; }
        //public string Index { get; set; }
        //public List<string> GUID { get; set; }
        //public string ServiceTag { get; set; }
        public string Command { get; set; }
        public string TargetFeature { get; set; }
        public string Value { get; set; }
        public string Result { get; set; }
        public string Message { get; set; }

        public NKVM_RESPONSE()
        {
            //Model = "N/A";
            //SerialNumber = "N/A";
            Command = "N/A";
            TargetFeature = "N/A";
            Result = "N/A";
            //Index = "N/A";
            //ServiceTag = "N/A";
            Value = "N/A";
            Message = "N/A";
            //GUID = new List<string>();
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        public string OutputLog(object o, CommandLineInput commandLineInput)
        {
            if (!string.IsNullOrEmpty(commandLineInput.LogPath))
            {
                if (!Directory.Exists(Path.GetDirectoryName(commandLineInput.LogPath)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(commandLineInput.LogPath));
                }

                try
                {
                    using (StreamWriter sw = new StreamWriter(commandLineInput.LogPath, true))// 'true':新建或附加.'false',或沒填:新建或覆蓋.
                    {
                        sw.WriteLine(DateTime.Now);
                        sw.WriteLine(JsonConvert.SerializeObject(o, Formatting.Indented));
                    }
                }
                catch (Exception ex)
                {
#if DEBUG
                    Console.WriteLine($"[CLI_RESPONSE] OutputLog exception, message: {ex.Message}");
#endif
                }
            }
//#if DEBUG //Dean 0122 should keep response to console window for CLI
            Console.WriteLine(JsonConvert.SerializeObject(o, Formatting.Indented));
//#endif
            return JsonConvert.SerializeObject(o, Formatting.Indented);
        }
    }

    public class MONITORCOUNT_RESPONSE
    {
        //public string Model { get; set; }
        //public string SerialNumber { get; set; }
        //public string Index { get; set; }
        //public List<string> GUID { get; set; }
        //public string ServiceTag { get; set; }
        public string Command { get; set; }
        public string TargetFeature { get; set; }
        public string Value { get; set; }
        public string Result { get; set; }
        public string Message { get; set; }

        public MONITORCOUNT_RESPONSE()
        {
            //Model = "N/A";
            //SerialNumber = "N/A";
            Command = "N/A";
            TargetFeature = "N/A";
            Result = "N/A";
            //Index = "N/A";
            //ServiceTag = "N/A";
            Value = "N/A";
            Message = "N/A";
            //GUID = new List<string>();
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        public string OutputLog(object o, CommandLineInput commandLineInput)
        {
            if (!string.IsNullOrEmpty(commandLineInput.LogPath))
            {
                if (!Directory.Exists(Path.GetDirectoryName(commandLineInput.LogPath)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(commandLineInput.LogPath));
                }

                try
                {
                    using (StreamWriter sw = new StreamWriter(commandLineInput.LogPath, true))// 'true':新建或附加.'false',或沒填:新建或覆蓋.
                    {
                        sw.WriteLine(DateTime.Now);
                        sw.WriteLine(JsonConvert.SerializeObject(o, Formatting.Indented));
                    }
                }
                catch (Exception ex)
                {
#if DEBUG
                    Console.WriteLine($"[CLI_RESPONSE] OutputLog exception, message: {ex.Message}");
#endif
                }                
            }
//#if DEBUG //Dean 0122 should keep response to console window for CLI
            Console.WriteLine(JsonConvert.SerializeObject(o, Formatting.Indented));
//#endif
            return JsonConvert.SerializeObject(o, Formatting.Indented);
        }
    }

    public class APP_RESPONSE
    {
        //public string Model { get; set; }
        //public string SerialNumber { get; set; }
        //public string Index { get; set; }
        //public List<string> GUID { get; set; }
        //public string ServiceTag { get; set; }
        public string Command { get; set; }
        public string TargetFeature { get; set; }
        public string Value { get; set; }
        public string Result { get; set; }
        public string Message { get; set; }

        public APP_RESPONSE()
        {
            //Model = "N/A";
            //SerialNumber = "N/A";
            Command = "N/A";
            TargetFeature = "N/A";
            Result = "N/A";
            //Index = "N/A";
            //ServiceTag = "N/A";
            Value = "N/A";
            Message = "N/A";
            //GUID = new List<string>();
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        public string OutputLog(object o, CommandLineInput commandLineInput)
        {
            if (!string.IsNullOrEmpty(commandLineInput.LogPath))
            {
                if (!Directory.Exists(Path.GetDirectoryName(commandLineInput.LogPath)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(commandLineInput.LogPath));
                }

                try
                {
                    using (StreamWriter sw = new StreamWriter(commandLineInput.LogPath, true))// 'true':新建或附加.'false',或沒填:新建或覆蓋.
                    {
                        sw.WriteLine(DateTime.Now);
                        sw.WriteLine(JsonConvert.SerializeObject(o, Formatting.Indented));
                    }
                }
                catch (Exception ex)
                {
#if DEBUG
                    Console.WriteLine($"[CLI_RESPONSE] OutputLog exception, message: {ex.Message}");
#endif
                }                
            }
//#if DEBUG //Dean 0122 should keep response to console window for CLI
            Console.WriteLine(JsonConvert.SerializeObject(o, Formatting.Indented));
//#endif
            return JsonConvert.SerializeObject(o, Formatting.Indented);
        }
    }
}