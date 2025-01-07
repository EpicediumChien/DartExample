using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using DDPM.SA.Common;
using Dell.Client.Framework.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DDPM.SA.Common.Settings
{
    /// <summary>
    /// This webcam setting is designed for user mode process to read/write data,
    /// if you use this class under system mode process the target folder might be empty.
    /// </summary>
    public class WebcamSettings
    {
        private static readonly string target_folder = 
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @$"Dell\Dell Display and Peripheral Manager\WebcamSettings");

        public string VideoCaptureFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
        public bool WebcamCountdown = false;
        public bool WebcamGrid = false;
        public string SelectedResolution = "";
        public Dictionary<string, List<string>> SupportedFPSs = new();
        public Dictionary<string, string> SelectedFPSs = new();
        public Dictionary<string, string> Resolutions = new();

        public bool IsFocusOn { get; set; } = false;
        public int Focus { get; set; } = -1;
        public int Pan { get; set; } = -1;
        public int Tilt { get; set; } = -1;
        public int Zoom { get; set; } = -1;
        public int AntiFlicker { get; set; } = -1;
        public bool IsAutoFramingTransitionOn { get; set; } = false;
        public int AutoFramingSensitivity { get; set; } = -1;
        public int AutoFramingFrameSize { get; set; } = -1;
        public int AutoWhiteBalance { get; set; } = -1;
        public string SelectedProfile { get; set; } = string.Empty;
        public string SelectedProfileName { get; set; } = string.Empty;
        public Dictionary<string, WebcamProfile> PresetProfiles = new();
        public Dictionary<string, WebcamProfile> CustomProfiles = new();

        public string CurrentResolution { get => Resolutions[SelectedResolution]; }
        public string CurrentFPS { get => SelectedFPSs[SelectedResolution]; }
        public bool IsFirstTime = true;

        public WebcamSettings(DeviceInfo di = null, IDeviceManagerSA devMgr = null, ILog log = null)
        { 
            if(di != null && devMgr != null)
            {
                UpdateSupportedResolutions(di, devMgr, log);
            }
        }

        public void UpdateSupportedResolutions(DeviceInfo di = null, IDeviceManagerSA devMgr = null, ILog log = null)
        {
            if (di != null && devMgr != null)
            {
                Task<string> task = devMgr.GetSupportedResolutions(di.ID.ToString());
                var str = task.Result;
                log?.Info(@$"WebcamSettings Json str{str}!");
                if (string.IsNullOrEmpty(str))
                {
                    log?.Info("DTP GetSupportedResolutions fail!");
                    switch (di.ModelNumber)
                    {
                        case "WB5023":
                            str = "[{\"Resolution\":\"1280x720\",\"FPS\":[\"24\",\"30\",\"60\"]},{\"Resolution\":\"1920x1080\",\"FPS\":[\"24\",\"30\",\"60\"]},{\"Resolution\":\"2560x1440\",\"FPS\":[\"24\",\"30\"]}]";
                            //str = "{\"1280x720\":{\"Resolution\":\"1280x720\",\"FPS\":[\"24\",\"30\",\"60\"]},\"1920x1080\":{\"Resolution\":\"1920x1080\",\"FPS\":[\"24\",\"30\",\"60\"]},\"2560x1440\":{\"Resolution\":\"2560x1440\",\"FPS\":[\"24\",\"30\"]}}";
                            break;
                        default:
                            str = "[{\"Resolution\":\"1280x720\",\"FPS\":[\"24\",\"30\",\"60\"]},{\"Resolution\":\"1920x1080\",\"FPS\":[\"24\",\"30\",\"60\"]},{\"Resolution\":\"2560x1440\",\"FPS\":[\"24\",\"30\"]}]";
                            //str = "{\"1280x720\":{\"Resolution\":\"1280x720\",\"FPS\":[\"24\",\"30\",\"60\"]},\"1920x1080\":{\"Resolution\":\"1920x1080\",\"FPS\":[\"24\",\"30\",\"60\"]},\"2560x1440\":{\"Resolution\":\"2560x1440\",\"FPS\":[\"24\",\"30\"]}}";
                            break;
                    }
                }

                var resolutions = JsonConvert.DeserializeObject<List<ResolutionItem>>(str)!;
                if (resolutions != null)
                {
                    foreach (var res in resolutions.OrderByDescending(x => x.Resolution))
                    {
                        var resName = res.Resolution switch
                        {
                            "1280x720" => "HD",
                            "720x1280" => "HD",
                            "1920x1080" => "Full HD",
                            "1080x1920" => "Full HD",
                            "2560x1440" => "2K QHD",
                            "1440x2560" => "2K QHD",
                            "3840x2160" => "4K UHD",
                            "2160x3840" => "4K UHD",
                            _ => "8K UHD"
                        };
                        SupportedFPSs.Add(resName, res.FPS);
                        SelectedFPSs.Add(resName, "30");
                        Resolutions.Add(resName, res.Resolution);
                    }
                }

                task = devMgr.GetSelectedResolution(di.ID.ToString());
                str = task.Result;
                if (string.IsNullOrEmpty(str))
                {
                    log?.Error("DTP GetSelectedResolution fail!");
                    str = "{\"Resolution\":\"1280x720\",\"FPS\":[\"30\"]}";
                }

                var currentRes = JsonConvert.DeserializeObject<ResolutionItem>(str);
                if (currentRes != null)
                {
                    SelectedResolution = Resolutions.FirstOrDefault(x => x.Value == currentRes.Resolution).Key;
                    SelectedFPSs[SelectedResolution] = currentRes.FPS?.Count > 0 ? currentRes.FPS[0] : "30";
                }

                var customProfiles = di.CustomProfiles.ToObject<List<WebcamProfile>>()?.ToList();
                if (customProfiles != null)
                {
                    for (var l = customProfiles.Count - 1; l >= 0; l--)
                    {
                        CustomProfiles.Add(customProfiles[l].Name, customProfiles[l]);
                    }
                }

                WebcamProfile profile = new();
                profile.Name = "Default";
                profile.Description = "default";
                profile.IsHDROn = false;
                profile.Brightness = 128;
                profile.Contrast = 128;
                profile.Saturation = 128;
                profile.Sharpness = 128;
                profile.IsAutoFramingOn = false;
                profile.FieldOfView = 78;
                profile.IsAutoWhiteBalanceOn = true;
                profile.AutoWhiteBalance = 5000;
                PresetProfiles.Add(profile.Name, profile);

                profile = new();
                profile.Name = "Smooth";
                profile.Description = "Smooth";
                profile.IsHDROn = true;
                profile.Brightness = 160;
                profile.Contrast = 128;
                profile.Saturation = 128;
                profile.Sharpness = 0;
                profile.IsAutoFramingOn = false;
                profile.FieldOfView = 78;
                profile.IsAutoWhiteBalanceOn = true;
                profile.AutoWhiteBalance = 5000;
                PresetProfiles.Add(profile.Name, profile);

                profile = new();
                profile.Name = "Vibrant";
                profile.Description = "Vibrant";
                profile.IsHDROn = true;
                profile.Brightness = 192;
                profile.Contrast = 167;
                profile.Saturation = 152;
                profile.Sharpness = 181;
                profile.IsAutoFramingOn = false;
                profile.FieldOfView = 78;
                profile.IsAutoWhiteBalanceOn = true;
                profile.AutoWhiteBalance = 5000;
                PresetProfiles.Add(profile.Name, profile);

                profile = new();
                profile.Name = "Warm";
                profile.Description = "Warm";
                profile.IsHDROn = true;
                profile.Brightness = 169;
                profile.Contrast = 166;
                profile.Saturation = 134;
                profile.Sharpness = 168;
                profile.IsAutoFramingOn = false;
                profile.FieldOfView = 78;
                profile.IsAutoWhiteBalanceOn = true;
                profile.AutoWhiteBalance = 5950;
                PresetProfiles.Add(profile.Name, profile);

                switch (di.ModelNumber.ToUpper())
                {
                    case "U3223QZ":
                        PresetProfiles["Default"].FieldOfView = 90;
                        PresetProfiles["Smooth"].IsHDROn = false;
                        PresetProfiles["Smooth"].FieldOfView = 90;
                        PresetProfiles["Smooth"].Sharpness = 250;
                        PresetProfiles["Vibrant"].IsHDROn = false;
                        PresetProfiles["Vibrant"].FieldOfView = 90;
                        PresetProfiles["Vibrant"].Brightness = 200;
                        PresetProfiles["Vibrant"].Contrast = 162;
                        PresetProfiles["Vibrant"].Saturation = 128;
                        PresetProfiles["Vibrant"].Sharpness = 180;
                        PresetProfiles["Warm"].IsHDROn = false;
                        PresetProfiles["Warm"].FieldOfView = 90;
                        PresetProfiles["Warm"].Brightness = 204;
                        PresetProfiles["Warm"].Contrast = 147;
                        PresetProfiles["Warm"].Saturation = 155;
                        PresetProfiles["Warm"].Sharpness = 128;
                        break;
                    case "U3224KB":
                    case "U3224KBA":
                        PresetProfiles["Default"].FieldOfView = 90;
                        PresetProfiles["Smooth"].FieldOfView = 90;
                        PresetProfiles["Smooth"].Sharpness = 250;
                        PresetProfiles["Vibrant"].FieldOfView = 90;
                        PresetProfiles["Vibrant"].Brightness = 200;
                        PresetProfiles["Vibrant"].Contrast = 162;
                        PresetProfiles["Vibrant"].Saturation = 128;
                        PresetProfiles["Vibrant"].Sharpness = 180;
                        PresetProfiles["Warm"].FieldOfView = 90;
                        PresetProfiles["Warm"].Brightness = 204;
                        PresetProfiles["Warm"].Contrast = 147;
                        PresetProfiles["Warm"].Saturation = 155;
                        PresetProfiles["Warm"].Sharpness = 128;
                        break;
                    case "WB7022":
                        PresetProfiles["Default"].FieldOfView = 90;
                        PresetProfiles["Smooth"].FieldOfView = 90;
                        PresetProfiles["Vibrant"].FieldOfView = 90;
                        PresetProfiles["Warm"].FieldOfView = 90;
                        break;
                    default:
                        break;
                }

                SelectedProfileName = "Default";
            }
        }

        //Target folder should be: Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @$"Dell\Dell Display and Peripheral Manager\WebcamSettings");
        public static bool ExportWebcamSettings(WebcamSettings WebcamSettings, string model, IDeviceManagerSA devMgr = null, ILog log = null)
        {
            if(devMgr == null)
            {
                log?.Error("[ExportWebcamSettings] The input devMgr is null");
                return false;
            }
            try
            {
                log?.Info($"[ExportWebcamSettings] SelectedResolution :{WebcamSettings.SelectedResolution}");
                string json = JsonConvert.SerializeObject(WebcamSettings, Formatting.Indented);
                log?.Info($"[ExportWebcamSettings] json json:{json}");
                var fileFolder = target_folder;
                string info = string.Empty;
                if (!Directory.Exists(fileFolder))
                    Directory.CreateDirectory(fileFolder);
                if (DDPM.SA.Common.Settings.DDPMFileSecurity.ValidateFilePath(fileFolder, out info))
                {
                    string strPath = Path.Combine(fileFolder, $"{model}.json");
                    bool result = devMgr.WriteSerializedContentToFile(strPath, json).Result;//1007 apply signature
                    
                    if(!result)
                        log?.Error($"[ExportWebcamSettings] DeviceManagerSA is null(model:{model})");
                    return result;
                }
                else
                {
                    log?.Error($"[ExportWebcamSettings] ValidateFilePath failed(model:{model}): {info}");
                }
            }
            catch (Exception ex)
            {
                log?.Error($"[ExportWebcamSettings] exception: {ex.Message}");
            }
            return false;
        }

        //Target folder should be Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @$"Dell\Dell Display and Peripheral Manager\WebcamSettings\{model}.json");
        public static WebcamSettings ImportWebcamSettings(string model, DeviceInfo di, IDeviceManagerSA devMgr = null, ILog log = null)
        {
            if (devMgr != null)
            {
                log?.Info(@$"[WebcamSettings] ImportWebcamSettings Start  !");
                var filePath = target_folder;
                var hasFile = File.Exists(filePath);
                string jsonString = string.Empty;
                if (hasFile && devMgr != null)
                {
                    string info = string.Empty;
                    //DDPM.SA.Common.Settings.DDPMFileSecurity.SRemoveSymbolicFolder(Path.GetDirectoryName(filePath), out info);   // 20241004 Add for Security
                    if (DDPM.SA.Common.Settings.DDPMFileSecurity.ValidateFilePath(filePath, out info))
                    {
                        jsonString = devMgr.ReadSerializedContentFromFile(filePath).Result;
                        log?.Info(@$"[WebcamSettings] ImportWebcamSettings jsonString:{jsonString}!");
                        if (!string.IsNullOrEmpty(jsonString))
                            return JsonConvert.DeserializeObject<WebcamSettings>(File.ReadAllText(filePath))!;
                        else
                            log?.Error($"[ImportWebcamSettings][ReadSerializedContentFromFile] empty string output(model:{model})");
                    }
                    else
                    {
                        log?.Error($"[ImportWebcamSettings] ValidateFilePath failed(model:{model}): {info}");
                    }
                }
            }
            else
            {
                log?.Error("[ExportWebcamSettings] The input devMgr is null");
            }
            //Init a new data
            var wc = new WebcamSettings(di, devMgr, log);
            log?.Info(@$"[WebcamSettings] ImportWebcamSettings di jsonString:{JsonConvert.SerializeObject(di)}!");
            if(!ExportWebcamSettings(wc, model, devMgr, log))
            {
                log?.Info(@$"[WebcamSettings][ImportWebcamSettings] try to use ExportWebcamSettings to init file fail");
            }
            return wc;
        }
    }

    public class ResolutionItem
    {
        public string Resolution = string.Empty;
        public List<string> FPS = new();
    }

    public class WebcamProfile
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Priority { get; set; } = -1;
        public bool IsHDROn { get; set; } = false;
        public int Brightness { get; set; } = -1;
        public int Contrast { get; set; } = -1;
        public int Saturation { get; set; } = -1;
        public int Sharpness { get; set; } = -1;
        public bool IsAutoFramingOn { get; set; } = false;
        public int FieldOfView { get; set; } = -1;
        public bool IsAutoWhiteBalanceOn { get; set; } = false;
        public int AutoWhiteBalance { get; set; } = -1;
        public bool IsFocusOn { get; set; } = false;
        public int Focus { get; set; } = -1;
        public int Pan { get; set; } = -1;
        public int Tilt { get; set; } = -1;
        public int Zoom { get; set; } = -1;
        public int AntiFlicker { get; set; } = -1;
        public int AutoFramingSensitivity { get; set; } = -1;
        public int AutoFramingFrameSize { get; set; } = -1;
        public bool IsAutoFramingTransitionOn { get; set; } = false;
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
