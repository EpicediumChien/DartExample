using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using DDPM.SA.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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

        public bool IsFocusOn { get; set; }
        public int Focus { get; set; }
        public int Pan { get; set; }
        public int Tilt { get; set; }
        public int Zoom { get; set; }
        public int AntiFlicker { get; set; }
        public bool IsAutoFramingTransitionOn { get; set; }
        public int AutoFramingSensitivity { get; set; }
        public int AutoFramingFrameSize { get; set; }
        public int AutoWhiteBalance { get; set; }

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
                var str = task.Result;
                if (string.IsNullOrEmpty(str))
                {
                    Task<DeviceHelper> task1 = DdpmCommonHelper.DeviceManagerSA.GetDevices(true);
                    var dis = task1.Result.deviceInfo;
                    foreach (var item in dis)
                    {
                        if (item.ID == di.ID)
                        {
                            di = item;
                            str = di.SupportedResolutions;
                            break;
                        }
                    }
                }
                if (string.IsNullOrEmpty(str))
                    return;

                //var resolutions = JsonConvert.DeserializeObject<List<ResolutionItem>>(str)!;
                var resolutions = JsonConvert.DeserializeObject<Dictionary<string, ResolutionItem>>(str)!;
                foreach (var res in resolutions.OrderByDescending(x => x.Key))
                {
                    var resName = res.Key switch
                    {
                        "1280x720" => "HD",
                        "1920x1080" => "Full HD",
                        "2560x1440" => "2K QHD",
                        "3840x2160" => "4K QHD",
                        _ => "8K UHD"
                    };
                    SupportedFPSs.Add(resName, res.Value.FPS);
                    Resolutions.Add(resName, res.Value.Resolution);
                }
                task = DdpmCommonHelper.DeviceManagerSA!.GetSelectedResolution(di.ID.ToString());
                var currentRes = JsonConvert.DeserializeObject<ResolutionItem>(task.Result)!;
                SelectedResolution = Resolutions.FirstOrDefault(x => x.Value == currentRes.Resolution).Key;
                SelectedFPSs.Add(SelectedResolution, currentRes.FPS[0]);

                var customProfiles = di.CustomProfiles.ToObject<List<WebcamProfile>>()!.ToList();
                //Task<JArray> task1 = DdpmCommonHelper.DeviceManagerSA!.GetCustomProfiles(di.ID.ToString());
                //var jArray = JArray.FromObject(task1.Result);
                //var customProfiles = jArray.ToObject<List<WebcamProfile>>()!.ToList();
                for (var l = customProfiles.Count - 1; l >= 0; l--)
                {
                    CustomProfiles.Add(customProfiles[l].Name, customProfiles[l]);
                }
                var presetProfiles = di.PresetProfiles.ToObject<List<WebcamProfile>>()!.ToList();
                //task1 = DdpmCommonHelper.DeviceManagerSA!.GetPresetProfiles(di.ID.ToString());
                //jArray = JArray.FromObject(task1.Result);
                //var presetProfiles = jArray.ToObject<List<WebcamProfile>>()!.ToList();
                //foreach (var profile in presetProfiles.OrderBy(x => x.Name))
                //{
                //    profile.Focus = di.FocusMin;
                //    PresetProfiles.Add(profile.Name, profile);
                //    //ProfileIDs.Add(profile.Name, profile.Id);
                //}

                switch (di.ModelNumber.ToUpper())
                {
                    case "U3223QZ":
                    case "U3224KB":

                        break;
                    default:
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
                        PresetProfiles.Add(profile.Name, profile);
                        break;
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
                //return true;
            }
            catch (Exception)
            {
            }
            return false;
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
        public bool IsHDROn { get; set; }
        public int Brightness { get; set; }
        public int Contrast { get; set; }
        public int Saturation { get; set; }
        public int Sharpness { get; set; }
        public bool IsAutoFramingOn { get; set; }
        public int FieldOfView { get; set; }
        public bool IsAutoWhiteBalanceOn { get; set; }


        public bool IsFocusOn { get; set; }
        public int Focus { get; set; }
        public int Pan { get; set; }
        public int Tilt { get; set; }
        public int Zoom { get; set; }
        public int AntiFlicker { get; set; }
        public int AutoFramingSensitivity { get; set; }
        public int AutoFramingFrameSize { get; set; }
        public bool IsAutoFramingTransitionOn { get; set; }
        public int AutoWhiteBalance { get; set; }
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
