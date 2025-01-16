using Dell.Client.Framework.UX.WPF.Controls;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Resources;
using Windows.UI.ViewManagement;

namespace DDPM.SA.Common.UI
{
    public static partial class SACommonHelper
    {
        //default theme is dark
        private static OSThemeEnum previousOsTheme = OSThemeEnum.Dark;

        private static Application application;

        /// <summary>
        /// Flag to switch the Light Mode Feature
        /// </summary>
        public static bool ThemeSwitchFlag { get; set; } = false;

        public static void GetResourceDictionary()
        {
            if (!UriParser.IsKnownScheme("pack"))
                application = new System.Windows.Application();
            if (application.Resources != null && application.Resources.MergedDictionaries.Count == 0)
            {
                // Get the current running assembly
                Assembly assembly = Assembly.GetExecutingAssembly();

                // Get the name of the assembly (e.g., AABA.SA.Common)
                string assemblyName = assembly.GetName().Name;
                Uri uri = new Uri($"pack://application:,,,/{assemblyName};component/ModuleStyle.xaml", UriKind.Absolute);
                Application.Current.Resources.MergedDictionaries.Add(
                    new ResourceDictionary
                    {
                        Source = uri
                    });
            }
        }

        public delegate void UpdateAction<T>(ref T resource);

        /// <summary>
        /// Update Freezable
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="resourceKey"></param>
        /// <param name="updateAction"></param>
        public static void UpdateFreezable<T>(string resourceKey, UpdateAction<T> updateAction) where T : Freezable
        {
            // Get the Freezable object from the resources
            if (Application.Current.Resources[resourceKey] is T currentFreezable)
            {
                // Clone the Freezable object to modify it
                T newFreezable = (T)currentFreezable.Clone();

                // Perform the update on the cloned object
                updateAction(ref newFreezable);

                // Replace the old object with the updated one in the resources
                Application.Current.Resources[resourceKey] = newFreezable;

                // [20241122] SonarQube: Remove or correct this useless self-assignment.
                //Application.Current.Resources[resourceKey] = Application.Current.Resources[resourceKey];// Force Refresh
            }
        }

        public static string MappingModel(string modelNumber)
        {
            switch (modelNumber)
            {
                case "KB740":
                case "KB7120W":
                    return "KB740";

                case "KB500":
                case "KB3121W":
                    return "KB500";

                case "KB700":
                case "KB7221W":
                    return "KB700";

                case "MS300":
                case "MS3121W":
                    return "MS300";

                default:
                    break;
            }
            return modelNumber;
        }

        public static string MappingEOLName(string model)
        {
            switch (model)
            {
                case "WK636":
                case "KM713":
                    return $"Dell {model} Wireless Keyboard";
                case "WK717":
                    return $"Dell Premier Wireless Keyboard WK717";
                case "WM116":
                case "WM514":
                case "UV514":
                case "WM126":
                case "WM326":
                case "WM527":
                    return $"Dell {model} Wireless Mouse";
                default:
                    return model;
            }
        }
        public static readonly List<string> EOLKBList = new() { "WK636", "KM713", "WK717", "KM714", "KM717" };
        public static readonly List<string> EOLMouseList = new() { "WM116", "WM514", "UV514", "WM126", "WM326", "WM527" };

        /// <summary>
        /// Check if the specifc peripheral model is EOL model.
        /// Based on "Copy of Peripheral-SupportedDeviceList_20241224.xlsx"
        /// Used by Homepage tooltip text.
        /// *** Major consumer is DDPM UI *** [Dean]0115 move this from UI DdpmCommonHelper to SA SACommonHelper
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static bool IsPeripheralEOLModel(string model)
        {
            switch (model)
            {
                //Keyboard : (Marketname)
                case "WK636": //Dell WK636  Wireless Keyboard
                case "WK717": //Dell Premier Wireless Keyboard WK717
                case "KM713": //Dell KM713  Wireless Keyboard

                //Mouse
                case "WM116": //Dell WM116  Wireless Mouse
                case "WM514": //Dell WM514  Wireless Mouse
                case "UV514": //Dell UV514  Wireless Mouse
                case "WM126": //(Alex 說 IL 還沒能 support, Robert_Lin, 2024-12-24)
                case "WM326": //Dell WM326  Wireless Mouse
                case "WM527": //Dell WM527 Wireless Mouse
                    return true;
                default:
                    return false;
            }
        }

        //[Dean]0115 move this from UI DdpmCommonHelper to SA SACommonHelper, Major consumer is DDPM UI
        public static string MappingName(string model, string name)
        {
            name = name.Replace(model, "").Trim();
            switch (CultureInfo.InstalledUICulture.Name)
            {
                case "ja-JP":
                    if (model == "WB7022")
                        return "Dell Digital Hi-Resolution Webcam";
                    if (model == "U3223QZ")
                        return "Dell Digital Hi-End 32 4K Video Conferencing Monitor";
                    if (model == "U3224KB")
                        return "Dell Digital Hi-End 32 6K Monitor";
                    return name;
                default:
                    return name;
            }
        }
    }
}
