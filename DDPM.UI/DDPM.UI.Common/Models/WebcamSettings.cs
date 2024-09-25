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
                var fileFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @$"Dell Display and Peripheral Manager\WebcamSettings");
                if (!Directory.Exists(fileFolder))
                    Directory.CreateDirectory(fileFolder);

                File.WriteAllText(Path.Combine(fileFolder, $"{model}.json"), json);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static WebcamSettings ImportWebcamSettings(string model)
        {
            var filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @$"Dell Display and Peripheral Manager\WebcamSettings\{model}.json");
            var hasFile = File.Exists(filePath);
            if (hasFile)
            {
                return JsonConvert.DeserializeObject<WebcamSettings>(File.ReadAllText(filePath))!;
            }
            else
            {
                var ka = new WebcamSettings();
                ExportWebcamSettings(ka, model);
                return ka;
            }
        }
    }

    public class WebcamProfile
    {
        public string Id = "";
        public string Name = "";
        public string Description = "";
        public int Priority = 0;
        public bool IsFocusOn;
        public int Focus;
        public int Pan;
        public int Tilt = 0;
        public int Zoom;
        public int Brightness;
        public int Contrast;
        public int AntiFlicker;
        public int Saturation;
        public int Sharpness;
        public bool IsAutoWhiteBalanceOn;
        public int AutoWhiteBalance;
        public bool IsAutoFramingOn;
        public int AutoFramingSensitivity;
        public int AutoFramingFrameSize;
        public bool IsAutoFramingTransitionOn;
        public int FieldOfView;
        public bool IsHDROn;

    }
}
