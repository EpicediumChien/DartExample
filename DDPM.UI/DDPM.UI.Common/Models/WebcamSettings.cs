using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using DDPM.SA.Common;
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

        public string SelectedProfile = "";
        public string SelectedProfileName = "";
        public Dictionary<string, WebcamProfile> PresetProfiles = new();
        public Dictionary<string, WebcamProfile> CustomProfiles = new();

        public string CurrentResolution { get => Resolutions[SelectedResolution]; }
        public string CurrentFPS { get => SelectedFPSs[SelectedResolution]; }

        public WebcamSettings(DeviceInfo? di = null)
        {
            if (di != null && di.ID != new Guid())
            {
                Task<string> task = DdpmCommonHelper.DeviceManagerSA!.GetSupportedResolutions(di.ID.ToString());
                var resolutions = JsonConvert.DeserializeObject<List<ResolutionItem>>(task.Result)!;
                foreach (var res in resolutions)
                {
                    var resName = res.Resolution switch
                    {
                        "1280x720" => "HD",
                        "1920x1080" => "Full HD",
                        "2560x1440" => "2K QHD",
                        "3840x2160" => "2K QHD",
                        _ => "8K UHD"
                    };
                    SupportedFPSs.Add(resName, res.FPS);
                    Resolutions.Add(resName, res.Resolution);
                }
                task = DdpmCommonHelper.DeviceManagerSA!.GetSelectedResolution(di.ID.ToString());
                var currentRes = JsonConvert.DeserializeObject<ResolutionItem>(task.Result)!;
                SelectedResolution = Resolutions.FirstOrDefault(x => x.Value == currentRes.Resolution).Key;
                SelectedFPSs.Add(SelectedResolution, currentRes.FPS[0]);

                var customProfiles = di.CustomProfiles.ToObject<List<WebcamProfile>>()!.ToList();
                for (var l = customProfiles.Count - 1; l >= 0; l--)
                {
                    CustomProfiles.Add(customProfiles[l].Name, customProfiles[l]);
                }
                foreach (var profile in di.PresetProfiles.ToObject<List<WebcamProfile>>()!.ToList().OrderBy(x => x.Name))
                {
                    profile.Focus = di.FocusMin;
                    PresetProfiles.Add(profile.Name, profile);
                    //ProfileIDs.Add(profile.Name, profile.Id);
                }
                SelectedProfileName = PresetProfiles.Values.ToList()[0].Name;
            }
        }

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

        public static WebcamSettings ImportWebcamSettings(string model, DeviceInfo di)
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
            var ka = new WebcamSettings(di);
            ExportWebcamSettings(ka, model);
            return ka;
        }
    }

    public class ResolutionItem
    {
        public string Resolution = string.Empty;
        public List<string> FPS = new();
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
