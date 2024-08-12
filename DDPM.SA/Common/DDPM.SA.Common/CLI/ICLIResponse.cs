using DDPM.SA.Common.Display;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static DDPM.SA.Common.ICLICommandTable;

namespace DDPM.SA.Common
{
    internal interface ICLIResponse
    {
    }

    public class CLI_RESPONSE
    {
        public string Model { get; set; }
        public string SerialNumber { get; set; }
        public string Index { get; set; }
        public List<string> GUID { get; set; }
        public string ServiceTag { get; set; }
        public string Command { get; set; }
        public string TargetFeature { get; set; }
        public string Value { get; set; }
        public string Result { get; set; }
        public string Message { get; set; }
        public CLI_RESPONSE()
        {
            Model = "N/A";
            SerialNumber = "N/A";
            Command = "N/A";
            TargetFeature = "N/A";
            Result = "N/A";
            Index = "N/A";
            ServiceTag = "N/A";
            Value = "N/A";
            Message = "N/A";
            GUID = new List<string>();
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
                using (StreamWriter sw = new StreamWriter(commandLineInput.LogPath, true))// 'true':新建或附加.'false',或沒填:新建或覆蓋.     
                {
                    sw.WriteLine(DateTime.Now);
                    sw.WriteLine(JsonConvert.SerializeObject(o, Formatting.Indented));
                }
            }
            System.Console.WriteLine(JsonConvert.SerializeObject(o, Formatting.Indented));
            return JsonConvert.SerializeObject(o, Formatting.Indented);
        }
    }

    public class CLI_Read_EDID_RESPONSE : CLI_RESPONSE
    {
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
    }

    public class CLI_Get_EDID_RESPONSE : CLI_RESPONSE
    {
        public string EDID_RAW { get; set; } = "N/A";
    }

    public class CLI_Get_FW_RESPONSE : CLI_RESPONSE
    {
        public string FWVer { get; set; } = "N/A";
    }
	
	public class ConnectedDevices : CLI_RESPONSE
    {
        //public string ID { get; set; }
        public string Manufacturer { get; set; }
        //public string PID { get; set; }
        public string ManufacturingYear { get; set; }
        public string ManufacturingWeek { get; set; }
        public string FirmwareVersion { get; set; }
    }

    public class CLI_Get_MONITORS_RESPONSE : CLI_RESPONSE
    {
        public List<string> Monitors { get; set; }

        public CLI_Get_MONITORS_RESPONSE()
        {
            Monitors = new List<string>();
        }
    }

    public class CLI_Get_Brightness_RESPONSE : CLI_RESPONSE
    {
        public string Brightness { get; set; } = "N/A";
    }

    public class CLI_Get_Contrast_RESPONSE : CLI_RESPONSE
    {
        public string Contrast { get; set; } = "N/A";
    }

    public class CLI_Get_Luminus_RESPONSE : CLI_RESPONSE
    {
        public string Luminus { get; set; } = "N/A";
    }

    public class CLI_Input_RESPONSE : CLI_RESPONSE
    {
        public string ActiveInputSource { get; set; }
    }

    public class CLI_InputList_RESPONSE : CLI_RESPONSE
    {
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
        }
    }

    public class CLI_Get_Properties_USBCPrioritization_RESPONSE : CLI_RESPONSE
    {
        public string SupportedUSBCPrioritization { get; set; }
        public string USBCPrioritizationType { get; set; }
        public CLI_Get_Properties_USBCPrioritization_RESPONSE(CLI_RESPONSE cli_RESPONSE)
        {
            this.Index = cli_RESPONSE.Index;
            this.ServiceTag = cli_RESPONSE.ServiceTag;
            this.Command = cli_RESPONSE.Command;
            this.TargetFeature = cli_RESPONSE.TargetFeature;
            this.SerialNumber = cli_RESPONSE.SerialNumber;
            this.Model = cli_RESPONSE.Model;
        }
    }

    public class CLI_Get_Properties_Orientation_RESPONSE : CLI_RESPONSE
    {
        public string Orientation { get; set; }
        public CLI_Get_Properties_Orientation_RESPONSE(CLI_RESPONSE cli_RESPONSE)
        {
            this.Orientation = "N/A";
            this.Index = cli_RESPONSE.Index;
            this.ServiceTag = cli_RESPONSE.ServiceTag;
            this.Command = cli_RESPONSE.Command;
            this.TargetFeature = cli_RESPONSE.TargetFeature;
            this.SerialNumber = cli_RESPONSE.SerialNumber;
            this.Model = cli_RESPONSE.Model;
        }
    }

    public class CLI_Get_Properties_CurrentResolutionRefreshRate_RESPONSE : CLI_RESPONSE
    {
        public string CurrentResolutionRefreshRate { get; set; }
        public string BitsPerPixel { get; set; }
        public CLI_Get_Properties_CurrentResolutionRefreshRate_RESPONSE(CLI_RESPONSE cli_RESPONSE)
        {
            this.Index = cli_RESPONSE.Index;
            this.ServiceTag = cli_RESPONSE.ServiceTag;
            this.Command = cli_RESPONSE.Command;
            this.TargetFeature = cli_RESPONSE.TargetFeature;
            this.SerialNumber = cli_RESPONSE.SerialNumber;
            this.Model = cli_RESPONSE.Model;
        }
    }
    public class CLI_Get_Properties_SupportedResolutionRefreshRate_RESPONSE : CLI_RESPONSE
    {
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
        }
    }

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
        }
    }

    public class CLI_FWU_RESPONSE : CLI_RESPONSE
    {
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
        }
    }


    public class CLI_Set_Input_RESPONSE : CLI_RESPONSE
    {
        public string Set_ActiveInputSource { get; set; }
    }


    #region ColorProfile

    public class CLI_Get_ActiveColorPresetList_RESPONSE : CLI_RESPONSE
    {
        public string Get_ActiveColorPresetList { get; set; }
    }


    public class CLI_Get_AllSupportedColorPresetList_RESPONSE : CLI_RESPONSE
    {
        public List<string> Get_AllSupportedColorPresetList { get; set; }
    }

    public class CLI_Set_SupportedColorPreset_RESPONSE : CLI_RESPONSE
    {
        public string Set_SupportedColorPreset { get; set; }
    }

    // jim modify 20240608
    public class CLI_Set_AllActiveColorPreset_RESPONSE : CLI_RESPONSE
    {
        public string Set_AllActiveColorPreset { get; set; }
    }

    // jim modify 20240608
    public class CLI_Set_AllMonitorProfile_RESPONSE : CLI_RESPONSE
    {
        public string Set_AllMonitorProfile { get; set; }
    }

    // jim modify 20240608
    public class CLI_Get_AllMonitorProfile_RESPONSE : CLI_RESPONSE
    {
        public string Get_AllMonitorProfile { get; set; }
    }

    #endregion

    #region Pxp - Robert_Lin added 2024-6-12
    public class CLI_RESPONSE_PxpMode : CLI_RESPONSE
    {
        public string[] SupportedModes { get; set; }
        public string CurrentMode { get; set; }
        public UInt16 CurrentModeCode { get; set; }
    }

    public class CLI_RESPONSE_SubInput : CLI_RESPONSE
    {
        public int SubInputCount { get; set; } = 0;
        public string Sub1InputSource { get; set; }
        public string Sub2InputSource { get; set; }
        public string Sub3InputSource { get; set; }
    }
    #endregion

    public class Get_DeviceData : CLI_RESPONSE
    {
        public string Manufacturer { get; set; }
        public string ManufacturingYear { get; set; }
        public string ManufacturingWeek { get; set; }
        public string FirmwareVersion { get; set; }
        public string MonitorActiveHour { get; set; }
        public string DisplayTechnologyType { get; set; }
        public string ScreenSize { get; set; }
        public string OptimalResolution { get; set; }
        public string Resolution { get; set; }
        public string ActiveInputSource { get; set; }
        public string ColorPreset { get; set; }
        public string ScreenOrientation { get; set; }
        public string BrightnessLevel { get; set; }
        public string ContrastLevel { get; set; }
        public string LuminanceLevel { get; set; }
        public string AutoBrightness { get; set; }
        public string AutoBrightnessRangeLevel { get; set; }
        public string AutoColorTemp { get; set; }
        public string PrimaryMonitorForSync { get; set; }
        public string AspectRatio { get; set; }
        public string USB_CPrioritization { get; set; }
        public string ColorManagement { get; set; }
        public string SpeakerMicrophone_enable { get; set; }
        public string SpeakerMicrophone_lock { get; set; }
        public string SpeakerVolume { get; set; }
        public string MicrophoneControl { get; set; }
        public string Uniformity { get; set; }
        public string PowerNap { get; set; }
        public string OSD_language { get; set; }
    }

}
