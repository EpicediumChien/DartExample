using Dell.Client.Framework.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using DDPM.SA.Common;
using Dell.Client.Framework.Common;
using Microsoft.VisualBasic.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VcpCore.Common;
using IndiLogic.DPeM.Broker;

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

        //import  
        public ResolutionItem resolution = null;
        public string SelectedcurrentFPS = string.Empty;
        public string ImportSelectedResolution { get; set; } = string.Empty;
        public List<string> WebcamProfileNames = new List<string>() { "Default", "Smooth", "Vibrant", "Warm" };
        //
        public string CurrentResolution { get => Resolutions[SelectedResolution]; }
        public string CurrentFPS { get => SelectedFPSs[SelectedResolution]; }
        public bool IsFirstTime = true;
        public WebcamProfile NONE = new();

        public WebcamSettings(DeviceInfo di = null, IDeviceManagerSA devMgr = null, ILog log = null)
        {
            if (di != null && devMgr != null)
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
                log?.Info(@$"WebcamSettings Json str {str}, device id is {di.ID.ToString()}");
                if (string.IsNullOrEmpty(str))
                {
                    log?.Info("DTP GetSupportedResolutions fail!");
                    //switch (di.ModelNumber)
                    //{
                    //    case "WB5023":
                    //        str = "[{\"Resolution\":\"1280x720\",\"FPS\":[\"24\",\"30\",\"60\"]},{\"Resolution\":\"1920x1080\",\"FPS\":[\"24\",\"30\",\"60\"]},{\"Resolution\":\"2560x1440\",\"FPS\":[\"24\",\"30\"]}]";
                    //        //str = "{\"1280x720\":{\"Resolution\":\"1280x720\",\"FPS\":[\"24\",\"30\",\"60\"]},\"1920x1080\":{\"Resolution\":\"1920x1080\",\"FPS\":[\"24\",\"30\",\"60\"]},\"2560x1440\":{\"Resolution\":\"2560x1440\",\"FPS\":[\"24\",\"30\"]}}";
                    //        break;
                    //    default:
                    //        str = "[{\"Resolution\":\"1280x720\",\"FPS\":[\"24\",\"30\",\"60\"]},{\"Resolution\":\"1920x1080\",\"FPS\":[\"24\",\"30\",\"60\"]},{\"Resolution\":\"2560x1440\",\"FPS\":[\"24\",\"30\"]}]";
                    //        //str = "{\"1280x720\":{\"Resolution\":\"1280x720\",\"FPS\":[\"24\",\"30\",\"60\"]},\"1920x1080\":{\"Resolution\":\"1920x1080\",\"FPS\":[\"24\",\"30\",\"60\"]},\"2560x1440\":{\"Resolution\":\"2560x1440\",\"FPS\":[\"24\",\"30\"]}}";
                    //        break;
                    //}
                    str = "[{\"Resolution\":\"1280x720\",\"FPS\":[\"24\",\"30\",\"60\"]},{\"Resolution\":\"1920x1080\",\"FPS\":[\"24\",\"30\",\"60\"]},{\"Resolution\":\"2560x1440\",\"FPS\":[\"24\",\"30\"]}]";
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
                    GetMigrationData(di, devMgr, log, ref task, ref str);
                }

                log?.Info(@$"Get customProfiles");
                var customProfiles = di.CustomProfiles.ToObject<List<WebcamProfile>>()?.ToList();
                if (customProfiles != null)
                {
                    for (var l = customProfiles.Count - 1; l >= 0; l--)
                    {
                        CustomProfiles.TryAdd(customProfiles[l].Name, customProfiles[l]);
                        log?.Info(@$"Get customProfiles {JsonConvert.SerializeObject(customProfiles[l])}");
                    }
                }
                log?.Info(@$"Get PresetProfiles {CustomProfiles.Keys}");
                PresetProfiles.Clear();
                var presetProfiles = di.PresetProfiles?.ToObject<List<WebcamProfile>>()?.ToList();
                log?.Error($"presetProfiles count {presetProfiles?.Count}!");
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
                PresetProfiles.TryAdd(profile.Name, profile);

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
                PresetProfiles.TryAdd(profile.Name, profile);

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
                PresetProfiles.TryAdd(profile.Name, profile);

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
                PresetProfiles.TryAdd(profile.Name, profile);

                switch (di.ModelNumber.ToUpper())
                {
                    case "P2424HEB":
                    case "P2724DEB":
                    case "P3424WEB":
                        PresetProfiles["Default"].IsFocusOn = true;
                        PresetProfiles["Warm"].IsFocusOn = true;
                        PresetProfiles["Vibrant"].IsFocusOn = true;
                        PresetProfiles["Smooth"].IsFocusOn = true;
                        PresetProfiles["Smooth"].Sharpness = 32;
                        break;
                    case "U3223QZ":
                        PresetProfiles["Default"].FieldOfView = 90;
                        PresetProfiles["Default"].IsFocusOn = true;
                        PresetProfiles["Smooth"].IsHDROn = false;
                        PresetProfiles["Smooth"].FieldOfView = 90;
                        PresetProfiles["Smooth"].Sharpness = 250;
                        PresetProfiles["Smooth"].IsFocusOn = true;
                        PresetProfiles["Vibrant"].IsHDROn = false;
                        PresetProfiles["Vibrant"].FieldOfView = 90;
                        PresetProfiles["Vibrant"].Brightness = 200;
                        PresetProfiles["Vibrant"].Contrast = 162;
                        PresetProfiles["Vibrant"].Saturation = 128;
                        PresetProfiles["Vibrant"].Sharpness = 180;
                        PresetProfiles["Vibrant"].IsFocusOn = true;
                        PresetProfiles["Warm"].IsHDROn = false;
                        PresetProfiles["Warm"].FieldOfView = 90;
                        PresetProfiles["Warm"].Brightness = 204;
                        PresetProfiles["Warm"].Contrast = 147;
                        PresetProfiles["Warm"].Saturation = 155;
                        PresetProfiles["Warm"].Sharpness = 128;
                        PresetProfiles["Warm"].IsFocusOn = true;
                        break;
                    case "U3224KB":
                    case "U3224KBA":
                        PresetProfiles["Default"].FieldOfView = 90;
                        PresetProfiles["Default"].IsFocusOn = true;
                        PresetProfiles["Default"].Focus = 1;
                        PresetProfiles["Smooth"].FieldOfView = 90;
                        PresetProfiles["Smooth"].Brightness = 128;
                        PresetProfiles["Smooth"].Sharpness = 25;
                        PresetProfiles["Smooth"].IsFocusOn = true;
                        PresetProfiles["Smooth"].Saturation = 100;
                        PresetProfiles["Smooth"].IsHDROn = false;
                        PresetProfiles["Vibrant"].FieldOfView = 90;
                        PresetProfiles["Vibrant"].Brightness = 128;
                        PresetProfiles["Vibrant"].Contrast = 191;
                        PresetProfiles["Vibrant"].Saturation = 157;
                        PresetProfiles["Vibrant"].Sharpness = 163;
                        PresetProfiles["Vibrant"].IsHDROn = false;
                        PresetProfiles["Vibrant"].IsFocusOn = true;
                        PresetProfiles["Warm"].FieldOfView = 90;
                        PresetProfiles["Warm"].Brightness = 93;
                        PresetProfiles["Warm"].Contrast = 128;
                        PresetProfiles["Warm"].Saturation = 129;
                        PresetProfiles["Warm"].Sharpness = 63;
                        PresetProfiles["Warm"].IsHDROn = false;
                        PresetProfiles["Warm"].IsFocusOn = true;
                        PresetProfiles["Warm"].IsAutoWhiteBalanceOn = false;
                        PresetProfiles["Warm"].AutoWhiteBalance = 5830;

                        break;
                    case "WB5023":
                        PresetProfiles["Default"].IsFocusOn = true;
                        PresetProfiles["Warm"].IsFocusOn = true;
                        PresetProfiles["Vibrant"].IsFocusOn = true;
                        PresetProfiles["Smooth"].IsFocusOn = true;
                        break;
                    case "WB3023":
                        PresetProfiles["Default"].IsFocusOn = true;
                        PresetProfiles["Default"].Focus = 1;
                        PresetProfiles["Default"].AutoFramingSensitivity = 0;
                        PresetProfiles["Default"].AutoFramingFrameSize = 0;
                        PresetProfiles["Default"].IsAutoFramingTransitionOn = false;
                        PresetProfiles["Warm"].IsFocusOn = true;
                        PresetProfiles["Warm"].AutoFramingSensitivity = 0;
                        PresetProfiles["Warm"].AutoFramingFrameSize = 0;
                        PresetProfiles["Warm"].IsAutoFramingTransitionOn = false;
                        PresetProfiles["Vibrant"].IsFocusOn = true;
                        PresetProfiles["Vibrant"].AutoFramingSensitivity = 0;
                        PresetProfiles["Vibrant"].AutoFramingFrameSize = 0;
                        PresetProfiles["Vibrant"].IsAutoFramingTransitionOn = false;
                        PresetProfiles["Smooth"].IsFocusOn = true;
                        PresetProfiles["Smooth"].AutoFramingSensitivity = 0;
                        PresetProfiles["Smooth"].AutoFramingFrameSize = 0;
                        PresetProfiles["Smooth"].IsAutoFramingTransitionOn = false;
                        break;
                    case "WB7022":
                        PresetProfiles["Default"].FieldOfView = 90;
                        PresetProfiles["Default"].IsFocusOn = true;
                        PresetProfiles["Smooth"].FieldOfView = 90;
                        PresetProfiles["Smooth"].IsFocusOn = true;
                        PresetProfiles["Vibrant"].FieldOfView = 90;
                        PresetProfiles["Vibrant"].IsFocusOn = true;
                        PresetProfiles["Warm"].FieldOfView = 90;
                        PresetProfiles["Warm"].IsFocusOn = true;
                        break;
                    default:
                        break;
                }
                log?.Error($"if (presetProfiles != null) {PresetProfiles.Keys}");
                SetDPeMDefaultSettings(presetProfiles, PresetProfiles, log);
                var HasNewDefault = CustomProfiles.Values.Any(x => x.Name.Contains(di.ProfileName)) && WebcamProfileNames.Any(y => y == di.ProfileName);
                SelectedProfileName = di.ProfileName != string.Empty ? HasNewDefault == true ? di.ProfileName + "*" : di.ProfileName : "Default";
            }
        }

        private void GetMigrationData(DeviceInfo di, IDeviceManagerSA devMgr, ILog log, ref Task<string> task, ref string str)
        {
            log?.Info(@$"task = devMgr.GetSelectedResolution(di.ID.ToString());");
            try
            {
                task = devMgr.GetSelectedResolution(di.ID.ToString());
                str = task.Result;
                log?.Info(@$"GetSelectedResolution str:{str}");
                if (string.IsNullOrEmpty(str))
                {
                    log?.Error("DTP GetSelectedResolution fail!");
                    str = "{\"Resolution\":\"1280x720\",\"FPS\":[\"30\"]}";
                }
            }
            catch (Exception ex)
            {
                log?.Info(@$"GetSelectedResolution Ex:{ex.Message}");
            }
            try
            {
                log?.Info(@$"currentRes str:{str}");
                var currentRes = JsonConvert.DeserializeObject<ResolutionItem>(str);
                if (currentRes != null)
                {
                    string resName = GetResolutionName(log, currentRes);
                    SelectedResolution = Resolutions.FirstOrDefault(x => x.Key == resName).Key;
                    log?.Info(@$"currentRes SelectedResolution:{SelectedResolution}");
                    if (SelectedFPSs.ContainsKey(SelectedResolution))
                    {
                        SelectedFPSs.Remove(SelectedResolution);
                        SelectedFPSs.Add(SelectedResolution, currentRes.FPS?.Count > 0 ? currentRes.FPS[0] : "30");
                        log?.Info(@$"currentRes SelectedFPSs:{JsonConvert.SerializeObject(SelectedFPSs)}");
                        log?.Info(@$"currentRes CurrentFPS:{CurrentFPS}");
                    }
                    log?.Info(@$"currentRes currentRes.FPS:{JsonConvert.SerializeObject(currentRes.FPS)}");

                }
            }
            catch (Exception e)
            {
                log?.Info(@$"currentRes Ex:{e.Message}");
            }
        }

        /// <summary>
        /// //Check And Set SelectedcurrentFPS in DeviceInfo on migration if SelectedcurrentFPS is empty to defualt "30"
        /// </summary>
        /// <param name="di"></param>
        /// <returns></returns>
        private static string SetCurrentFPS(WebcamSettings di, string resName, ILog log)
        {
            log?.Info(@$"di.SelectedResolution Json str {di.SelectedResolution}");
            if (!string.IsNullOrEmpty(di.SelectedResolution))
            {
                try
                {
                    if (di.SelectedResolution.Equals(resName))
                    {
                        log?.Info(@$"di.SelectedcurrentFPS  {di.SelectedcurrentFPS}");
                        if (!string.IsNullOrEmpty(di.SelectedcurrentFPS))
                        {
                            return di.SelectedcurrentFPS;
                        }
                    }
                }
                catch (Exception ex)
                {

                }
            }
            return "30";
        }

        private void SetDPeMDefaultSettings(List<WebcamProfile> presetProfiles, Dictionary<string, WebcamProfile> DefaultPresetProfiles, ILog log)
        {
            if (presetProfiles != null)
            {
                foreach (var PresetProfile in presetProfiles)
                {
                    WebcamProfile webcamProfile = null;
                    bool IsGetFile = DefaultPresetProfiles.TryGetValue(PresetProfile.Name, out webcamProfile);
                    if (!IsGetFile)
                    {
                        continue;
                    }
                    WebcamProfile webcamProfile1 = PresetProfile.Clone();
                    webcamProfile1.Description = string.Empty;
                    webcamProfile1.Id = string.Empty;
                    WebcamProfile webcamProfile2 = webcamProfile.Clone();
                    webcamProfile2.Description = string.Empty;
                    webcamProfile2.Id = string.Empty;
                    log?.Info(@$"webcamProfile1:{JsonConvert.SerializeObject(webcamProfile1)}");
                    log?.Info(@$"webcamProfile2:{JsonConvert.SerializeObject(webcamProfile2)}");
                    if (JsonConvert.SerializeObject(webcamProfile1).Equals(JsonConvert.SerializeObject(webcamProfile2)))
                    {
                        log?.Info(@$"SetDPeMDefaultSettings continue");
                        continue;
                    }
                    WebcamProfile newprofile = PresetProfile;
                    newprofile.Name = newprofile.Name + "*";
                    CustomProfiles.TryAdd(newprofile.Name, newprofile);
                    log?.Info(@$"Get newprofile {JsonConvert.SerializeObject(newprofile)}");
                }
                //var FindSelectedProfile = PresetProfiles.ToList().Where(x => x.Value.Description == di.ProfileDescription).FirstOrDefault();
                //SelectedProfileName = FindSelectedProfile.Value.Name;
            }
        }

        private static string GetResolutionName(ILog log, ResolutionItem currentRes)
        {
            log?.Info(@$"currentRes currentRes:{currentRes}");
            var resName = currentRes.Resolution switch
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
            return resName;
        }

        //Target folder should be: Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @$"Dell\Dell Display and Peripheral Manager\WebcamSettings");
        public static bool ExportWebcamSettings(WebcamSettings WebcamSettings, string model, IDeviceManagerSA devMgr = null, ILog log = null)
        {
            if (devMgr == null)
            {
                log?.Error("[ExportWebcamSettings] The input devMgr is null");
                return false;
            }
            try
            {
                log?.Info($"[ExportWebcamSettings] SelectedResolution :{WebcamSettings.SelectedResolution}");
                WebcamSettings.SelectedcurrentFPS = WebcamSettings.CurrentFPS;
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

                    if (!result)
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

        private static WebcamSettings ReAlignWebcamResolution(WebcamSettings input, string model, DeviceInfo di, IDeviceManagerSA devMgr = null, ILog log = null)
        {
            //leo fixed start 2025/01/07
            //Always re-read the resolution and FPS information.
            try
            {
                WebcamSettings tmp = input;// JsonConvert.DeserializeObject<WebcamSettings>(jsonString) ?? new WebcamSettings(di, devMgr, log);
                tmp.SupportedFPSs.Clear();
                tmp.SelectedFPSs.Clear();
                tmp.Resolutions.Clear();
                Task<string> task = devMgr.GetSupportedResolutions(di.ID.ToString());
                var str = task.Result;
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
                        //Check And Set SelectedcurrentFPS in WebCamSetting.json if SelectedcurrentFPS is empty to defualt "30"

                        tmp.SupportedFPSs.Add(resName, res.FPS);
                        tmp.Resolutions.Add(resName, res.Resolution);
                        string selectFps = SetCurrentFPS(input, resName, log);
                        tmp.SelectedFPSs.Add(resName, selectFps);
                    }
                    //if (!ExportWebcamSettings(tmp, model, devMgr, log))
                    //{
                    //    log?.Info("[WebcamSettings][ImportWebcamSettings] resolutions change, ExportWebcamSettings to file fail");
                    //}
                    //else
                    //    log?.Info("[WebcamSettings][ImportWebcamSettings] resolutions change, ExportWebcamSettings to file OK");
                }
                return tmp;
            }
            catch (Exception ex)
            {
                log?.Error("[ImportWebcamSettings][DeserializeObject] exception : " + ex.Message);
            }
            //leo fixed end
            return input;
        }

        //Target folder should be Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @$"Dell\Dell Display and Peripheral Manager\WebcamSettings\{model}.json");
        public static WebcamSettings ImportWebcamSettings(string model, DeviceInfo di, IDeviceManagerSA devMgr = null, ILog log = null)
        {
            WebcamSettings tmp = null;

            try
            {
                if (devMgr != null)
                {
                    log?.Info(@$"[WebcamSettings] ImportWebcamSettings Start  !");
                    var filePath = Path.Combine(target_folder, $"{model}.json");
                    var hasFile = File.Exists(filePath);
                    log?.Info(@$"[WebcamSettings] file {filePath} exist = {hasFile}");

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
                            {
                                tmp = JsonConvert.DeserializeObject<WebcamSettings>(jsonString) ?? new WebcamSettings(di, devMgr, log);
                                tmp = ReAlignWebcamResolution(tmp, model, di, devMgr, log);
                                //return tmp;
                            }
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
                if (tmp == null)
                {
                    tmp = new WebcamSettings(di, devMgr, log);
                    log?.Info(@$"[WebcamSettings] ImportWebcamSettings DeviceInfo:{di}!");
                    //tmp = ReAlignWebcamResolution(tmp, model, di, devMgr, log);
                }
                if (!ExportWebcamSettings(tmp, model, devMgr, log))
                {
                    log?.Info(@$"[WebcamSettings][ImportWebcamSettings] try to use ExportWebcamSettings to init file fail");
                }
            }
            catch (Exception ex)
            {
                log?.Info("DDPM.SA.Common\\Settings\\WebcamSettings.cs ImportWebcamSettings ex:" + ex.Message);
            }
            return tmp;
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
        public int Priority { get; set; } = 0; //參考IL基本值
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
        public int Focus { get; set; } = 0;//參考IL基本值
        public int Pan { get; set; } = 0;//參考IL基本值
        public int Tilt { get; set; } = 0;//參考IL基本值
        public int Zoom { get; set; } = 100;//參考IL基本值
        public int AntiFlicker { get; set; } = 2;//參考IL基本值
        public int AutoFramingSensitivity { get; set; } = 1;//參考IL基本值
        public int AutoFramingFrameSize { get; set; } = 1;//參考IL基本值
        public bool IsAutoFramingTransitionOn { get; set; } = true;//參考IL基本值
        public WebcamProfile Clone()
        {
            return (WebcamProfile)MemberwiseClone();
        }
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
