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
        public bool IsFirstTime = true;

        public WebcamSettings(DeviceInfo? di = null)
        {
            if (di != null)
            {
                Task<string> task = DdpmCommonHelper.DeviceManagerSA!.GetSupportedResolutions(di.ID.ToString());
                var str = task.Result;
                DdpmCommonHelper.WriteUILog(@$"WebcamSettings Json str{str}!");
                if (string.IsNullOrEmpty(str))
                {
                    DdpmCommonHelper.WriteUILog("DTP GetSupportedResolutions fail!");
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
                //if (string.IsNullOrEmpty(str))
                //{
                //    switch (di.ModelNumber)
                //    {
                //        case "WB5023":
                //            str = "[{\"Resolution\":\"1280x720\",\"FPS\":[\"24\",\"30\",\"60\"]},{\"Resolution\":\"1920x1080\",\"FPS\":[\"24\",\"30\",\"60\"]},{\"Resolution\":\"2560x1440\",\"FPS\":[\"24\",\"30\"]}]";
                //            //str = "{\"1280x720\":{\"Resolution\":\"1280x720\",\"FPS\":[\"24\",\"30\",\"60\"]},\"1920x1080\":{\"Resolution\":\"1920x1080\",\"FPS\":[\"24\",\"30\",\"60\"]},\"2560x1440\":{\"Resolution\":\"2560x1440\",\"FPS\":[\"24\",\"30\"]}}";
                //            break;
                //        default:
                //            str = "[{\"Resolution\":\"1280x720\",\"FPS\":[\"24\",\"30\",\"60\"]},{\"Resolution\":\"1920x1080\",\"FPS\":[\"24\",\"30\",\"60\"]},{\"Resolution\":\"2560x1440\",\"FPS\":[\"24\",\"30\"]}]";
                //            //str = "{\"1280x720\":{\"Resolution\":\"1280x720\",\"FPS\":[\"24\",\"30\",\"60\"]},\"1920x1080\":{\"Resolution\":\"1920x1080\",\"FPS\":[\"24\",\"30\",\"60\"]},\"2560x1440\":{\"Resolution\":\"2560x1440\",\"FPS\":[\"24\",\"30\"]}}";
                //            break;
                //    }
                //}

                var resolutions = JsonConvert.DeserializeObject<List<ResolutionItem>>(str)!;
                //var resolutions = JsonConvert.DeserializeObject<Dictionary<string, ResolutionItem>>(str)!;
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

                task = DdpmCommonHelper.DeviceManagerSA!.GetSelectedResolution(di.ID.ToString());
                str = task.Result;
                if (string.IsNullOrEmpty(str))
                {
                    DdpmCommonHelper.WriteUILog("DTP GetSelectedResolution fail!");
                    str = "{\"Resolution\":\"1280x720\",\"FPS\":[\"30\"]}";
                }

                var currentRes = JsonConvert.DeserializeObject<ResolutionItem>(str);
                if (currentRes != null)
                {
                    SelectedResolution = Resolutions.FirstOrDefault(x => x.Value == currentRes.Resolution).Key;
                }

                var customProfiles = di.CustomProfiles.ToObject<List<WebcamProfile>>()?.ToList();
                if (customProfiles != null)
                {
                    for (var l = customProfiles.Count - 1; l >= 0; l--)
                    {
                        CustomProfiles.Add(customProfiles[l].Name, customProfiles[l]);
                    }
                }
                //var presetProfiles = di.PresetProfiles.ToObject<List<WebcamProfile>>()!.ToList();
                //task1 = DdpmCommonHelper.DeviceManagerSA!.GetPresetProfiles(di.ID.ToString());
                //jArray = JArray.FromObject(task1.Result);
                //var presetProfiles = jArray.ToObject<List<WebcamProfile>>()!.ToList();
                //foreach (var profile in presetProfiles.OrderBy(x => x.Name))
                //{
                //    profile.Focus = di.FocusMin;
                //    PresetProfiles.Add(profile.Name, profile);
                //    //ProfileIDs.Add(profile.Name, profile.Id);
                //}

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

        public static bool ExportWebcamSettings(WebcamSettings WebcamSettings, string model)
        {
            try
            {
                DdpmCommonHelper.WriteUILog($"[ExportWebcamSettings] SelectedResolution :{WebcamSettings.Selected_Resolution}");
                string json = JsonConvert.SerializeObject(WebcamSettings, Formatting.Indented);
                DdpmCommonHelper.WriteUILog($"[ExportWebcamSettings] json json:{json}");
                var fileFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @$"Dell\Dell Display and Peripheral Manager\WebcamSettings");
                string info = string.Empty;
                //DDPM.SA.Common.Settings.DDPMFileSecurity.SRemoveSymbolicFolder(fileFolder, out info);   // 20241004 Add for Security
                if (!Directory.Exists(fileFolder))
                    Directory.CreateDirectory(fileFolder);
                if (DDPM.SA.Common.Settings.DDPMFileSecurity.ValidateFilePath(fileFolder, out info))
                {
                    string strPath = Path.Combine(fileFolder, $"{model}.json");
                    //File.WriteAllText(strPath, json);
                    if (DdpmCommonHelper.DeviceManagerSA != null)
                    {
                        return DdpmCommonHelper.DeviceManagerSA.WriteSerializedContentToFile(strPath, json).Result;//1007 apply signature
                    }
                    //return true;
                    throw new Exception($"[ExportWebcamSettings] DeviceManagerSA is null(model:{model})");
                }
                else
                {
                    DdpmCommonHelper.WriteUILog($"[ExportWebcamSettings] ValidateFilePath failed(model:{model}): {info}");
                }
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog($"[ExportWebcamSettings] exception: {ex.Message}");
            }
            return false;
        }

        public static WebcamSettings ImportWebcamSettings(string model, DeviceInfo di)
        {
            DdpmCommonHelper.WriteUILog(@$"[WebcamSettings] ImportWebcamSettings Start  !");
            var filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @$"Dell\Dell Display and Peripheral Manager\WebcamSettings\{model}.json");
            var hasFile = File.Exists(filePath);
            string jsonString = string.Empty;
            if (hasFile)
            {
                string info = string.Empty;
                //DDPM.SA.Common.Settings.DDPMFileSecurity.SRemoveSymbolicFolder(Path.GetDirectoryName(filePath), out info);   // 20241004 Add for Security
                if (DDPM.SA.Common.Settings.DDPMFileSecurity.ValidateFilePath(filePath, out info))
                {
                    if (DdpmCommonHelper.DeviceManagerSA != null)
                    {
                        jsonString = DdpmCommonHelper.DeviceManagerSA.ReadSerializedContentFromFile(filePath).Result;
                        DdpmCommonHelper.WriteUILog(@$"[WebcamSettings] ImportWebcamSettings jsonString:{jsonString}!");
                    }
                    if (!string.IsNullOrEmpty(jsonString))
                        return JsonConvert.DeserializeObject<WebcamSettings>(File.ReadAllText(filePath))!;
                }
                else
                {
                    DdpmCommonHelper.WriteUILog($"[ImportWebcamSettings] ValidateFilePath failed(model:{model}): {info}");
                }
            }
            var ka = new WebcamSettings(di);
            DdpmCommonHelper.WriteUILog(@$"[WebcamSettings] ImportWebcamSettings di jsonString:{JsonConvert.SerializeObject(di)}!");
            //SetJsonToResolution(ka, di);
            ExportWebcamSettings(ka, model);
            return ka;
        }

        //public static void SetJsonToResolution(WebcamSettings ka, DeviceInfo di)
        //{

        //    if (!string.IsNullOrWhiteSpace(di.SelectedResolution))
        //    {
        //        ka.SelectedResolution = di.SelectedResolution;
        //        ka.resolution = JsonConvert.DeserializeObject<ResolutionItem>(ka.SelectedResolution);
        //        DdpmCommonHelper.WriteUILog(@$"[WebcamSettings] ImportWebcamSettings ka.SelectedResolution :{ka.SelectedResolution}!");
        //        var resName = ka.resolution.Resolution switch
        //        {
        //            "1280x720" => "HD",
        //            "720x1280" => "HD",
        //            "1920x1080" => "Full HD",
        //            "1080x1920" => "Full HD",
        //            "2560x1440" => "2K QHD",
        //            "1440x2560" => "2K QHD",
        //            "3840x2160" => "4K UHD",
        //            "2160x3840" => "4K UHD",
        //            _ => "8K UHD"
        //        };
        //        ka.Selected_Resolution = resName;
        //        ka.Selected_FPS = ka.resolution.FPS.ToList().FirstOrDefault();
        //        DdpmCommonHelper.WriteUILog(@$"[WebcamSettings] ImportWebcamSettings resName :{resName}!");
        //        DdpmCommonHelper.WriteUILog(@$"[WebcamSettings] ImportWebcamSettings ka.Selected_FPS :{ka.Selected_FPS}!");
        //    }
        //}
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
        public int AutoWhiteBalance { get; set; }


        public bool IsFocusOn { get; set; }
        public int Focus { get; set; }
        public int Pan { get; set; }
        public int Tilt { get; set; }
        public int Zoom { get; set; }
        public int AntiFlicker { get; set; }
        public int AutoFramingSensitivity { get; set; }
        public int AutoFramingFrameSize { get; set; }
        public bool IsAutoFramingTransitionOn { get; set; }
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
