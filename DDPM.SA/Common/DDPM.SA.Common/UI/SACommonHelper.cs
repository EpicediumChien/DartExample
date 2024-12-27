using Dell.Client.Framework.UX.WPF.Controls;
using System;
using System.Collections.Generic;
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
                    return modelNumber;
            }
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
    }
}
