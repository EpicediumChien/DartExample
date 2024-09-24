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
}
