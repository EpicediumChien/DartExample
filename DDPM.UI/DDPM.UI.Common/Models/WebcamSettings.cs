using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DDPM.UI.Common
{
    public class WebcamSettings
    {
        public string VideoCaptureFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
        public bool WebcamCountdown = false;
        public bool WebcamGrid = false;
        public string SelectedResolution = "";
        public Dictionary<string, List<string>> SupportedFPSs = new();
        public Dictionary<string, string> SelectedFPSs = new();
        public Dictionary<string, string> Resolutions = new();

        public string SelectedProfileName = "";
        public Dictionary<string, WebcamProfile> PresetProfiles = new();
        public Dictionary<string, WebcamProfile> CustomProfiles = new();

        public string CurrentResolution { get => Resolutions[SelectedResolution]; }
        public string CurrentFPS { get => SelectedFPSs[SelectedResolution]; }

        public static bool ExportWebcamSettings(WebcamSettings WebcamSettings, string model)
        {
            try
            {
                string json = JsonConvert.SerializeObject(WebcamSettings, Formatting.Indented);
                var fileFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @$"Dell\Dell Display and Peripheral Manager\WebcamSettings");
                string info = string.Empty;
                DDPM.SA.Common.Settings.DDPMFileSecurity.SRemoveSymbolicFolder(fileFolder, out info);   // 20241004 Add for Security
                if (!Directory.Exists(fileFolder))
                    Directory.CreateDirectory(fileFolder);
                string strPath = Path.Combine(fileFolder, $"{model}.json");
                //File.WriteAllText(strPath, json);
                if (DdpmCommonHelper.DeviceManagerSA != null)
                {
                    return DdpmCommonHelper.DeviceManagerSA.WriteSerializedContentToFile(strPath, json).Result;//1007 apply signature
                }                
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static WebcamSettings ImportWebcamSettings(string model)
        {
            var filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @$"Dell\Dell Display and Peripheral Manager\WebcamSettings\{model}.json");
            var hasFile = File.Exists(filePath);
            string jsonString = string.Empty;
            if (hasFile)
            {
                string info = string.Empty;                
                DDPM.SA.Common.Settings.DDPMFileSecurity.SRemoveSymbolicFolder(Path.GetDirectoryName(filePath), out info);   // 20241004 Add for Security
                if (DdpmCommonHelper.DeviceManagerSA != null)
                {
                    jsonString = DdpmCommonHelper.DeviceManagerSA.ReadSerializedContentFromFile(filePath).Result;
                }
                if (!string.IsNullOrEmpty(jsonString))
                    return JsonConvert.DeserializeObject<WebcamSettings>(File.ReadAllText(filePath))!;
            }
            var ka = new WebcamSettings();
            ExportWebcamSettings(ka, model);
            return ka;
        }
    }

    public class WebcamProfile
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public int Priority { get; set; }
        public bool IsFocusOn { get; set; }
        public int Focus { get; set; }
        public int Pan { get; set; }
        public int Tilt { get; set; }
        public int Zoom { get; set; }
        public int Brightness { get; set; }
        public int Contrast { get; set; }
        public int AntiFlicker { get; set; }
        public int Saturation { get; set; }
        public int Sharpness { get; set; }
        public bool IsAutoWhiteBalanceOn { get; set; }
        public int AutoWhiteBalance { get; set; }
        public bool IsAutoFramingOn { get; set; }
        public int AutoFramingSensitivity { get; set; }
        public int AutoFramingFrameSize { get; set; }
        public bool IsAutoFramingTransitionOn { get; set; }
        public int FieldOfView { get; set; }
        public bool IsHDROn { get; set; }

    }

    public enum OperationModule
    {
        CameraControl,
        ColorAndImage,
        Other
    }

    public class WebcamOperation
    {
        public OperationModule OPModule;
        public required string Property;
        public required object OldValue;
        public required object NewValue;
    }
}
