using DDPM.SA.Common;
using DDPM.SA.Common.Settings;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Views;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using Dell.Client.Framework.UX.WPF.Controls;
using Dell.Client.Framework.UX.WPF.ResourceManager;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Forms.VisualStyles;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using VcpCore.Common;
using Windows.Devices.Geolocation;
using Windows.Devices.PointOfService;
using static DDPM.UI.Common.Views.DDPMMsgBox;
using Application = System.Windows.Application;
using Path = System.Windows.Shapes.Path;

namespace DDPM.UI.Common
{
    public static partial class DdpmCommonHelper
    {
        /// <summary>
        /// Create an ImageSource object, which load from DDPM.Common.Resources
        /// </summary>
        /// <param name="resourceName">The filename of the image file, for example, "Resources/dell.png"</param>
        /// <returns></returns>
        public static ImageSource GetImageSourceFromCommonResource(string resourceName, string assemblyName = "")
        {
            if (String.IsNullOrWhiteSpace(assemblyName))
                assemblyName = "DDPM.UI.Common";

            try
            {
                Uri oUri = new Uri("pack://application:,,,/" + assemblyName + ";component/" + resourceName, UriKind.RelativeOrAbsolute);
                return BitmapFrame.Create(oUri);
            }
            catch (Exception ex)
            {
            }
            return null;
        }

        //reload hotkey data if jump from USB KVM hotkey page
        public static bool isJumpFromUsbKvm { get; set; } = false;

        //reload inputsourece name if renamed
        public static bool bInputSourceRenamed { get; set; }

        public static bool isHotkeyBypass { get; set; } = false;

        /// <summary>
        /// Splash screen path
        /// </summary>
        public static string SplashPath { get; set; } = "Resources/Images/splash{0}-round.png";

        //DdpmHomePlugin will set this value
        public static IConsole? MyConsole { get; set; }
        public static IShowPluginManager? MyShowPluginManager { get; set; }

        public static IDeviceManagerSA? DeviceManagerSA { get; set; }

        public static DDPMSettings? Settings_Cache { get; set; }

        public static IModuleOwner? ModuleOwner { get; set; }

        public static ILog? Log { get; set; }

        //Derek 1209
        private static bool isDDPMSwitchToSettingPageByQAM = false;
        public static bool IsDDPMSwitchToSettingPageByQAM { get => isDDPMSwitchToSettingPageByQAM; set => isDDPMSwitchToSettingPageByQAM = value; }


        /// <summary>
        /// Flag to switch the Light Mode Feature
        /// </summary>
        public static bool ThemeSwitchFlag { get; set; } = false;

        /// <summary>previousOsTheme
        /// Flag to turn on and off UI Test buttons
        /// </summary>
        public static bool UIDebugModeFlag { get; set; } = false;

        //Robert_Lin, 2024-12-21 added, DdpmHomePLugin must know whether DisplayPlugin is
        // activated or not. When DisplayPlugin is activate, the MainWindow_MoveToNewPosition
        // event will be handled by DisplayPugin, or DdpmHomePlugin shoud take over.
        public static bool IsDisplayPluginActivated { get; set; } = false;

        private static bool isMainWindowAtPrimaryScreen = true;

        public static bool IsMainWindowAtPrimaryScreen { get => isMainWindowAtPrimaryScreen; set => isMainWindowAtPrimaryScreen = value; }

        //default theme is dark
        private static OSThemeEnum previousOsTheme = OSThemeEnum.Dark;
        public static OSThemeEnum PreviousOsTheme { get => previousOsTheme; set => previousOsTheme = value; }

        private static string lastShowOsdScreenDeviceName = "";

        public static string LastShowOsdScreenDeviceName { get => lastShowOsdScreenDeviceName; set => lastShowOsdScreenDeviceName = value; }

        //The last DisplayName (DeviceName) of the screen which show the OSD.

        public enum log_type
        {
            info = 0,
            error
        }

        //Using object from DDPM.SA.Common.UI.SAUICommonHelper [Dean]0115
        public static List<string> EOLKBList = DDPM.SA.Common.UI.SAUICommonHelper.EOLKBList;// new() { "WK636", "KM713", "WK717", "KM714", "KM717" };
        public static List<string> EOLMouseList = DDPM.SA.Common.UI.SAUICommonHelper.EOLMouseList;// new () { "WM116", "WM514", "UV514", "WM126", "WM326", "WM527" };

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <param name="log_type">0 means info, others means error</param>
        public static void WriteUILog(string text, log_type log_type = log_type.info,
            [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
            [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0)
        {
            if (string.IsNullOrEmpty(text))
                text = "";

            text = $"[Caller:{memberName}][SourceLine:{sourceLineNumber}] {text}";
#if DEBUG
            Trace.WriteLine(text);
#endif
            if (Log != null)
            {
                if (log_type == log_type.info)
                    Log.Info(text);
                else
                    Log.Error(text);
            }
        }

        public static bool DDPMMesssageBox(string title, string text, DependencyObject obj = null)
        {
            Window hwnd = null;
            if (obj != null)
            {
                Window parentWindow = Window.GetWindow(obj);
                if (parentWindow != null)
                {
                    hwnd = parentWindow;
                }
            }
            DDPMMsgBox msgBox = new DDPMMsgBox(title, text, hwnd);

            msgBox.ShowDialog();

            //if (result == MessageBoxResult.Yes)
            if (msgBox.result == DDPMMsgBox_btn_result.left)
                return true;
            else
                return false;
        }

        public static void DDPMPureMesssageBox(string title, string text, bool IsCloseButton, Window Owner)
        {
            DDPMMsgBox msgBox = new DDPMMsgBox(title, text, IsCloseButton, Owner);
            msgBox.Height = 172;
            msgBox.ShowDialog();
        }

        public static void DDPMEzMesssageBox(string title, string text, bool IsCloseButton, Window Owner, int width, int height, Thickness titlemargin, Thickness submargin)
        {
            DDPMMsgBox msgBox = new DDPMMsgBox(title, text, IsCloseButton, Owner, width, height, titlemargin, submargin);
            msgBox.ShowDialog();
        }

        public static bool DDPMEzMesssageChangeButtonBox(string title, string text, bool IsCloseButton, Window Owner, int width, int height, Thickness titlemargin, Thickness submargin, Thickness leftbtn, Thickness rightbtn)
        {
            DDPMMsgBox msgBox = new DDPMMsgBox(title, text, IsCloseButton, Owner, width, height, titlemargin, submargin, leftbtn, rightbtn);
            msgBox.ShowDialog();
            if (msgBox.result == DDPMMsgBox_btn_result.left)
                return true;
            else
                return false;
        }


        /// <summary>
        ///Parsing hex value blank separated string to a WORD array
        ///Support format:
        ///1 All Bytes: "02 04 05 08 10 12"
        ///2 All Words: "0002 0004 0105 0208 1006 AE12"
        ///3 Mix: "02 0208 04 05 AE12"
        ///4 Multiple space chars: "  02 04  05     08 10 1006    12"
        /// </summary>
        /// <param name="inStr"></param>
        /// <returns>
        /// null : Invalid format in inStr
        /// empty : no any token found
        /// </returns>
        public static UInt16[] ParsingHexStringToWords(string inStr)
        {
            //1 Split into tokens with white space
            string[] tokens = inStr.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (tokens == null || tokens.Length == 0)
                return new UInt16[0];

            List<UInt16> words = new List<UInt16>();
            //2 for each token will convert to UInt16 integer value
            foreach (string tok in tokens)
            {
                UInt16 wValue;
                if (!UInt16.TryParse(tok, NumberStyles.HexNumber, CultureInfo.CurrentCulture, out wValue))
                {
                    return null;
                }
                words.Add(wValue);
            }
            return words.ToArray();
        }

        public static bool? GetUINotifyPropertyValue_Boolean(string search_key, ITSettingEventArgs event_object)
        {
            if (event_object == null || event_object.IT_Feature_TriggerList == null || event_object.target_object == null)
            {
                Trace.WriteLine("Got [SettingsPage][DeviceManagerSA_ITSettingsActionEvent] event but its argument is empty!");
                return null;
            }
            int idx = event_object.IT_Feature_TriggerList.FindIndex(x => x.Trim().Equals(search_key));
            if (idx >= 0)
            {
                string feature = event_object.IT_Feature_TriggerList[idx];
                PropertyInfo propertyInfo = event_object.target_object.GetType().GetProperty(feature);
                Trace.WriteLine($"Got [SettingsPage][IT settings event] {feature} : {propertyInfo.GetValue(event_object.target_object)}");

                return (bool?)propertyInfo.GetValue(event_object.target_object);
            }
            return null;//null as default if feature not found
        }

        public static bool GetUINotify_IsSynchronizeBetweenMonitors_Locked(DDPMSettings data)
        {
            if (data == null || data.LockSettings == null)
                return false;

            //9/7 Functionality update:
            //When brightness, contrast, or color settings are locked, automatically lock 'synchronize between monitors'.
            //Should unlock if no brightness, contrast, or color settings are locked.
            if (data.LockSettings.Lock_Display_BriCont ||
                data.LockSettings.Lock_Display_ColorPreset ||
                data.LockSettings.Lock_Display_AutoBriTemp)
                return true;

            return false;
        }

        public static bool GetUINotifyPropertyValue_isAnyLocked(DDPMSettings data, string search_key = "")
        {
            if (data == null || data.LockSettings == null)
            {
                Trace.WriteLine("check is any lock but data is null!");
                return false;
            }

            if (search_key == null || string.IsNullOrEmpty(search_key) || search_key.Length == 0)
            {
                bool anyTrue = data.LockSettings.GetType().GetProperties()
                               .Where(p => p.PropertyType == typeof(bool))
                               .Select(p => (bool)p.GetValue(data.LockSettings))
                               .Any(value => value);

                return anyTrue;
            }
            bool result = data.LockSettings.GetType()
                         .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                         .Where(p => p.PropertyType == typeof(bool) && p.Name.Contains(search_key))
                         .Any(p => (bool)p.GetValue(data.LockSettings));
            return result;
        }

        public static (bool isEnabled, Visibility isLocked) ApplyRestoreFactoryDefaultsEventData(ITSettingEventArgs e, string device_lock_string)
        {
            bool isEnabled = false;
            Visibility visibility = Visibility.Collapsed;

            if (DeviceManagerSA == null)
                return (isEnabled, visibility);

            bool? isLocked = GetUINotifyPropertyValue_Boolean("Lock_Setting_RestoreDefaults", e);
            if (isLocked != null)
            {
                isEnabled = !(bool)isLocked;
                visibility = (bool)isLocked ? Visibility.Visible : Visibility.Collapsed;
                Trace.WriteLine($"Apply Global restore factory default(Lock) : {isLocked}");
                if ((bool)isLocked == false)
                {
                    DDPMSettings data = ReadDDPMSettings();// DeviceManagerSA.ReloadAppConfigData().Result;
                    if (data != null)// && data.LockSettings.Lock_Audio_RestoreFactoryDefaults)
                    {
                        bool retrieve = false;
                        if (device_lock_string.Equals("Lock_Display_RestoreFactoryDefaults"))//ok
                            retrieve = data.LockSettings.Lock_Display_RestoreFactoryDefaults;
                        else if (device_lock_string.Equals("Lock_Audio_RestoreFactoryDefaults"))//ok
                            retrieve = data.LockSettings.Lock_Audio_RestoreFactoryDefaults;
                        else if (device_lock_string.Equals("Lock_Webcam_RestoreFactoryDefaults"))//no ui element
                            retrieve = data.LockSettings.Lock_Webcam_RestoreFactoryDefaults;
                        else if (device_lock_string.Equals("Lock_Keyboard_RestoreFactoryDefaults"))//ok
                            retrieve = data.LockSettings.Lock_Keyboard_RestoreFactoryDefaults;
                        else if (device_lock_string.Equals("Lock_Mouse_RestoreFactoryDefaults"))//no ui element
                            retrieve = data.LockSettings.Lock_Mouse_RestoreFactoryDefaults;
                        else if (device_lock_string.Equals("Lock_Pen_RestoreFactoryDefaults"))
                            retrieve = data.LockSettings.Lock_Pen_RestoreFactoryDefaults;

                        if (retrieve)
                        {
                            visibility = Visibility.Visible;
                            isEnabled = false;
                            Trace.WriteLine($"Global restore factory default is unLock, but {device_lock_string} keeping UI lock");
                        }
                    }
                }
            }
            isLocked = GetUINotifyPropertyValue_Boolean(device_lock_string, e);// "Lock_Audio_RestoreFactoryDefaults", e);
            if (isLocked != null)
            {
                Trace.WriteLine($"Apply restore factory default(Lock) to feature {device_lock_string} : {isLocked}");
                DDPMSettings data = ReadDDPMSettings();// DeviceManagerSA.ReloadAppConfigData().Result;
                if (data != null)
                {
                    if (data.LockSettings.Lock_Setting_RestoreDefaults)
                    {
                        visibility = Visibility.Visible;
                        isEnabled = false;
                        Trace.WriteLine($"Global restore factory default is Lock, keeping UI lock");
                    }
                    else
                    {
                        isEnabled = !(bool)isLocked;
                        visibility = (bool)isLocked ? Visibility.Visible : Visibility.Collapsed;
                    }
                }
            }
            return (isEnabled, visibility);
        }

        public static bool WriteDDPMSettings(DDPMSettings data)
        {
            if (data == null || data.LockSettings == null || data.UserSettings == null)
                return false;

            Settings_Cache = data;

            if (DeviceManagerSA == null)
                return false;

            return DeviceManagerSA.SetAppConfigData(data).Result;
        }

        //Default data from cache, load from user subagent if force_reload = true
        public static DDPMSettings ReadDDPMSettings(bool reload_from_SA = false)
        {
            DDPMSettings tmp = null;
            try
            {
                if (reload_from_SA)
                {
                    tmp = DeviceManagerSA == null ? throw new Exception("NULL DevMgr") : DeviceManagerSA.ReloadAppConfigData().Result;
                    if (tmp != null)
                    {
                        Settings_Cache = tmp;
                    }
                    else
                        throw new Exception("Null Data from SA");
                }
                if (Settings_Cache == null)
                    throw new Exception("Null Data Cache");
            }
            catch (Exception ex)
            {
                WriteUILog($"[ReadDDPMSettings] exception: {ex.Message}");

                //20250304 Dean, read it with file directly from UI
                string folder = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Dell");
                string _settings_path = System.IO.Path.Combine(folder, GlobalDefinitions.Folder_Product, GlobalDefinitions.Filename_appsettings_peruser);
                WriteUILog("[ReadDDPMSettings] Read from file directly over UI");

                if (File.Exists(_settings_path))
                {
                    if (DDPMFileSecurity.ValidateFilePath(_settings_path, out string info))
                    {
                        string output = DDPMFileSecurity.GetSerializedJsonString(_settings_path, out info);//, false);
                        var _settings = JsonConvert.DeserializeObject<DDPMSettings>(output);
                        if (_settings != null)
                        {
                            Settings_Cache = _settings;
                            return Settings_Cache;
                        }
                        else
                            WriteUILog("[ReadDDPMSettings] de-serialize got null data");
                    }
                    else
                    {
                        WriteUILog($"[ReadDDPMSettings] ValidateFilePath failed: {info}");
                    }
                }
                else
                {
                    WriteUILog("[ReadDDPMSettings] file is not exist");
                }
            }
            return Settings_Cache;
        }


        public static void updateMergedDictionaries(ResourceManager resourceManager)
        {
            try
            {
                var uxSystemParameters = UXSystemParameters.Instance;
                if (uxSystemParameters == null)
                {
                    WriteUILog("[updateMergedDictionaries] UXSystemParameters.Instance is null.");
                    return;
                }

                OSThemeEnum oSTheme = uxSystemParameters.OSTheme;
                WriteUILog($"[updateMergedDictionaries] Detected OS theme: {oSTheme.ToString()} number: {oSTheme}.");

                if (PreviousOsTheme == oSTheme)
                {
                    WriteUILog($"[updateMergedDictionaries] PreviousOsTheme == oSTheme, then skip.");
                    return;
                }


                //OSThemeEnum oSTheme = UXSystemParameters.Instance.OSTheme;
                //WriteUILog($"[updateMergedDictionaries] Detected OS theme: {oSTheme.ToString()} number: {oSTheme}.");
                //if (PreviousOsTheme == oSTheme)
                //    return;
                string darkModeStyle = @"pack://application:,,,/DDPM.UI.Common;component/ModuleStyle.xaml";
                ResourceDictionary? darkResourceDictionary = Application.Current.Resources.MergedDictionaries.SingleOrDefault(x => x.Source.OriginalString.Equals(darkModeStyle));
                Application.Current.Resources.MergedDictionaries.Remove(darkResourceDictionary);
                darkResourceDictionary = new ResourceDictionary()
                {
                    Source = new Uri(darkModeStyle)
                };
                Application.Current.Resources.MergedDictionaries.Add(darkResourceDictionary);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    switch (oSTheme)
                    {
                        case OSThemeEnum.Light:
                            SwitchToLightMode();
                            break;
                        case OSThemeEnum.Dark:
                        default:
                            SwitchToDarkMode();
                            break;
                    }
                    ;
                    resourceManager.SwapDarkAndLightThemes();
                    resourceManager.StageResources();
                    resourceManager.CommitResources();
                    Application.Current.MainWindow?.InvalidateVisual();
                }, System.Windows.Threading.DispatcherPriority.Loaded);
                // Debug.WriteLine($"updateMergedDictionarie to {oSTheme.ToString()}");
                PreviousOsTheme = oSTheme;
            }
            catch (Exception ex)
            {
                WriteUILog($"[updateMergedDictionaries] An error occurred: {ex.Message}");
                return;
            }
        }

        public static bool isDarkMode()
        {
            return UXSystemParameters.Instance.OSTheme == OSThemeEnum.Dark;
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

                Application.Current.Resources[resourceKey] = Application.Current.Resources[resourceKey];// Force Refresh
            }
        }

        /// <summary>
        /// Create new BitmapImage
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        /// <remarks>
        /// Please set image files "<CopyToOutputDirectory>Always</CopyToOutputDirectory>"
        /// </remarks>
        public static void UpdateBitmapImage(string resourceKey, Uri uri)
        {
            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.UriSource = uri;
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.EndInit();
            Application.Current.Resources[resourceKey] = bitmapImage;
        }

        // Need refine
        public static event Action<OSThemeEnum>? BitmapImageUpdated;

        //Derek 10/30
        public static int GetBreakPoints()
        {
            int breakPoints = 1050;

            if (System.Windows.Application.Current?.TryFindResource("breakPoint") is Int16 width)
                breakPoints = width;

            return breakPoints;
        }

        #region Save UI Element to a .PNG imgae file
        //Robert_Lin, 2024-11-6
        //Two steps to save an FrameworkElement derived object to a .PNG image file
        //Requirements:
        //1 The UI element must be rendered already. You can check it by its ActualWidth and ActualHeight.
        //2 Step 1 CreateBitmapSource() must be called at UI thread, for example, in EzArrangeRightVierw.xaml.cs
        //  SaveLayoutIconsToPngFiles()
        public static BitmapSource? CreateBitmapSource(FrameworkElement ele)
        {
            double pxWidth = ele.ActualWidth + 1;
            double pxHeight = ele.ActualHeight + 1;

            if ((pxWidth <= 0) && (pxHeight <= 0))
                return null;

            System.Windows.Size sizeImg = new System.Windows.Size(pxWidth, pxHeight);
            ele.Measure(sizeImg);
            System.Windows.Rect rectImg = new System.Windows.Rect(new System.Windows.Size(pxWidth, pxHeight));
            ele.Arrange(rectImg);

            //Draw background
            //
            //SolidColorBrush brBackground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0,0,0,0));
            //DrawingVisual drawingVisual = new DrawingVisual();
            //DrawingContext dc = drawingVisual.RenderOpen();
            //dc.DrawRectangle(brBackground, null, rectImg);
            //dc.Close();

            RenderTargetBitmap rtb = new RenderTargetBitmap((int)pxWidth, (int)pxHeight,
               96d, 96d, System.Windows.Media.PixelFormats.Default);


            rtb.Render(ele);
            return rtb;
        }

        public static bool SaveBitmapSourceAsPngFile(BitmapSource bmpSrc, string pathName)
        {
            BitmapFrame bmpFrame = BitmapFrame.Create(bmpSrc);
            PngBitmapEncoder pngEnc = new PngBitmapEncoder();
            pngEnc.Frames.Add(bmpFrame);

            using (FileStream fs = new FileStream(pathName, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                pngEnc.Save(fs);
            }
            return true;
        }

        #endregion

        #region ProductImage
        /// <summary>
        /// Return the filename of the DeviceInfo, not include ".png"
        /// </summary>
        /// <param name="deviceInfo"></param>
        /// <returns></returns>
        public static string DeterminePeripheralProductImageFileName(DeviceInfo? deviceInfo)
        {
            string model = "";
            string colorCode = "";

            if (deviceInfo != null)
            {
                //[#PeripheralModelMap] This mapping table has a duplicate code in
                //1 DdpmCommonHelpers.cs    DeterminePeripheralProductImageFileName()
                //2 HomeDevices             TooltipModelName property
                //3 PeripheralViewModel.cs  MappingModel()
                //If you need to modify, please also modify them.
                /*switch (deviceInfo.ModelNumber)
                {
                    //Keyboard
                    case "KB740":
                    case "KB7120W":
                        model = "KB740";
                        break;
                    case "KB500":
                    case "KB3121W":
                        model = "KB500";
                        break;
                    case "KB700":
                    case "KB7221W":
                        model = "KB700";
                        break;

                    //Mouse
                    case "MS300":
                    case "MS3121W":
                        model = "MS300";
                        break;

                    //Default
                    default:
                        model = deviceInfo.ModelNumber;
                        break;
                } //switch(deviceInfo.ModelNumber)*/
                model = DDPM.SA.Common.UI.SAUICommonHelper.MappingModel(deviceInfo.ModelNumber);

                if (deviceInfo.ColorCode != 0)
                {
                    colorCode = $"_{deviceInfo.ColorCode}";
                }
            }
            return $"{model}{colorCode}";
        }

        //Using this from DDPM.SA.Common.UI.SAUICommonHelper [Dean]0115
        /*public static string MappingEOLName(string model)
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
        }*/

        //Using this from DDPM.SA.Common.UI.SAUICommonHelper [Dean]0115
        /*public static string MappingName(string model, string name)
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
        }*/

        //Collect all the peripheral model name which is EOL model to DDPM.SA.Common.UI.SAUICommonHelper [Dean]0115
        /// <summary>
        /// Check if the specifc peripheral model is EOL model.
        /// Based on "Copy of Peripheral-SupportedDeviceList_20241224.xlsx"
        /// Used by Homepage tooltip text.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        /*public static bool IsPeripheralEOLModel(string model)
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
        }*/
        #endregion

        public static Canvas CanvasIconCreator(VbarIcon vbarIcon, Geometry? clip = null)
        {
            List<UIElement> uIElements = iconPathCreator(vbarIcon);
            Canvas canvas = new Canvas()
            {
                Width = 24,
                Height = 24,
                VerticalAlignment = System.Windows.VerticalAlignment.Center,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                Tag = string.Empty
            };
            if (clip != null)
                canvas.Clip = clip;
            canvas.SetBinding(Canvas.TagProperty, new System.Windows.Data.Binding
            {
                RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(ContentControl), 1),
                Path = new PropertyPath("Foreground")
            });
            foreach (UIElement element in uIElements)
            {
                canvas.Children.Add(element);
            }

            return canvas;
        }

        private static List<UIElement> iconPathCreator(VbarIcon vbarIcon)
        {
            List<UIElement> uIElements = new List<UIElement>();
            switch (vbarIcon)
            {
                case VbarIcon.DisplaySettings:
                    Path displaySettings = new Path()
                    {
                        Data = Geometry.Parse("M24 16.125V2.625H0V16.125H9.375V19.845H5.625V21.435H18.375V19.845H14.625V16.125H24ZM13.02 16.125V19.83H10.98V16.125H13.02ZM1.605 4.125H22.395V14.52H1.605V4.125Z")
                    };
                    displaySettings.SetBinding(Path.FillProperty, new System.Windows.Data.Binding
                    {
                        RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Canvas), 1),
                        Path = new PropertyPath("Tag")
                    });
                    uIElements.Add(displaySettings);
                    break;
                case VbarIcon.DisplayInputSource:
                    Path inputOuterBox = new Path()
                    {
                        Data = Geometry.Parse("M5.14844 17.1559V6.84408C9.94053 3.33241 16.4563 3.33241 21.2484 6.84408V17.1559C16.4563 20.6676 9.94053 20.6676 5.14844 17.1559Z"),
                        StrokeThickness = 1.5,
                        Fill = new SolidColorBrush(Colors.Transparent)
                    };
                    inputOuterBox.SetBinding(Path.StrokeProperty, new System.Windows.Data.Binding
                    {
                        RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Canvas), 1),
                        Path = new PropertyPath("Tag")
                    });
                    uIElements.Add(inputOuterBox);
                    Path inputArrowHead = new Path()
                    {
                        Data = Geometry.Parse("M10.9906 16.9998L10.1406 15.7418L13.4382 11.9976L10.1406 8.25797L10.9906 7L15.3992 11.9942L10.9906 16.9998Z")
                    };
                    inputArrowHead.SetBinding(Path.FillProperty, new System.Windows.Data.Binding
                    {
                        RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Canvas), 1),
                        Path = new PropertyPath("Tag")
                    });
                    uIElements.Add(inputArrowHead);
                    Path inputArrowLine = new Path()
                    {
                        Data = Geometry.Parse("M13.522 11.2852H0V12.7137H13.522V11.2852Z"),
                        StrokeThickness = 1.5,
                    };
                    inputArrowLine.SetBinding(Path.FillProperty, new System.Windows.Data.Binding
                    {
                        RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Canvas), 1),
                        Path = new PropertyPath("Tag")
                    });
                    uIElements.Add(inputArrowLine);
                    break;
                case VbarIcon.DisplayEA:
                    Path eaOuterRect = new Path()
                    {
                        Data = Geometry.Parse("M1,6 H23 A1,1 0 0 1 24,7 V20 A1,1 0 0 1 23,21 H1 A1,1 0 0 1 0,20 V7 A1,1 0 0 1 1,6 Z"),
                        StrokeThickness = 2,
                        Fill = new SolidColorBrush(Colors.Transparent)
                    };
                    eaOuterRect.SetBinding(Path.StrokeProperty, new System.Windows.Data.Binding
                    {
                        RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Canvas), 1),
                        Path = new PropertyPath("Tag")
                    });
                    uIElements.Add(eaOuterRect);
                    Path eaLeftRect = new Path()
                    {
                        Data = Geometry.Parse("M3.7,8.6 H9.9 A0.5,0.5 0 0 1 10.4,9.1 V18.3 A0.5,0.5 0 0 1 9.9,18.8 H3.7 A0.5,0.5 0 0 1 3.2,18.3 V9.1 A0.5,0.5 0 0 1 3.7,8.6 Z"),
                        StrokeThickness = 1.5,
                        Fill = new SolidColorBrush(Colors.Transparent)
                    };
                    eaLeftRect.SetBinding(Path.StrokeProperty, new System.Windows.Data.Binding
                    {
                        RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Canvas), 1),
                        Path = new PropertyPath("Tag")
                    });
                    uIElements.Add(eaLeftRect);
                    Path eaButtonRightRect = new Path()
                    {
                        Data = Geometry.Parse("M11,18.3 V13.6 A0.5,0.5 0 0 1 11.5,13.1 H20.7 A0.5,0.5 0 0 1 21.2,13.6 V18.3 A0.5,0.5 0 0 1 20.7,18.8 H11.5 A0.5,0.5 0 0 1 11,18.3 Z"),
                        StrokeThickness = 1.5,
                        Fill = new SolidColorBrush(Colors.Transparent)
                        //,
                        //RenderTransform = new RotateTransform(90, 9.9, 21.1)
                    };
                    eaButtonRightRect.SetBinding(Path.StrokeProperty, new System.Windows.Data.Binding
                    {
                        RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Canvas), 1),
                        Path = new PropertyPath("Tag")
                    });
                    uIElements.Add(eaButtonRightRect);
                    Path eaTopRightRect = new Path()
                    {
                        Data = Geometry.Parse("M11,13.8 V9.1 A0.5,0.5 0 0 1 11.5,8.6 H20.7 A0.5,0.5 0 0 1 21.2,9.1 V13.8 A0.5,0.5 0 0 1 20.7,14.3 H11.5 A0.5,0.5 0 0 1 11,13.8 Z"),
                        StrokeThickness = 1.5,
                        Fill = new SolidColorBrush(Colors.Transparent)
                        //,
                        //RenderTransform = new RotateTransform(90, 3.9, 21.1)
                    };
                    eaTopRightRect.SetBinding(Path.StrokeProperty, new System.Windows.Data.Binding
                    {
                        RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Canvas), 1),
                        Path = new PropertyPath("Tag"),
                    });
                    uIElements.Add(eaTopRightRect);
                    break;
                case VbarIcon.DisplayGaming:
                    Path handleOutline = new Path()
                    {
                        Data = Geometry.Parse("M4.23152 5.25014L6.49652 3.96014C6.72292 3.83653 6.97398 3.7648 7.23152 3.75014C7.47647 3.74675 7.71852 3.8034 7.93652 3.91514C8.39437 4.17447 8.91037 4.31379 9.43652 4.32014H14.6715C15.1193 4.30426 15.5566 4.18077 15.9465 3.96014C16.1774 3.83437 16.4339 3.76257 16.6965 3.75014C16.9566 3.76699 17.2111 3.83317 17.4465 3.94514L19.6965 5.25014C20.2782 5.56977 20.7569 6.04844 21.0765 6.63014C22.6171 9.56046 23.6084 12.748 24.0015 16.0351V16.1551C24.0251 16.3946 24.0251 16.6357 24.0015 16.8751C24.0079 17.9436 23.6054 18.9739 22.8765 19.7551C22.7384 19.9041 22.5714 20.0235 22.3858 20.106C22.2001 20.1885 21.9997 20.2324 21.7965 20.2351C21.5908 20.2345 21.3875 20.1915 21.1991 20.109C21.0108 20.0264 20.8414 19.906 20.7015 19.7551L17.7015 16.3801C17.215 15.866 16.5486 15.5597 15.8415 15.5251H8.34152C7.98685 15.5383 7.63836 15.622 7.31641 15.7713C6.99447 15.9207 6.70554 16.1328 6.46652 16.3951L3.37652 19.7701C3.23666 19.9209 3.06728 20.0414 2.87891 20.124C2.69054 20.2065 2.48719 20.2495 2.28152 20.2501C2.0722 20.2496 1.86531 20.2054 1.67414 20.1201C1.48297 20.0348 1.31176 19.9105 1.17152 19.7551C0.420777 18.9663 0.00190329 17.9191 0.00151852 16.8301C-0.0213395 16.6007 -0.0213395 16.3696 0.00151852 16.1401C0.377293 12.7632 1.39559 9.48941 3.00152 6.49514C3.31738 5.99665 3.73689 5.57203 4.23152 5.25014ZM16.6965 5.25014C16.112 5.61739 15.4368 5.81475 14.7465 5.82014H9.48152C8.697 5.82542 7.92359 5.63465 7.23152 5.26514L4.96652 6.55514C4.64686 6.72414 4.38552 6.98549 4.21652 7.30514C2.77706 10.1119 1.88132 13.1655 1.57652 16.3051V16.4101C1.55975 16.5847 1.55975 16.7605 1.57652 16.9351C1.56971 17.6272 1.83344 18.2946 2.31152 18.7951L5.44652 15.4201C5.82739 15.0027 6.29055 14.6685 6.80685 14.4388C7.32315 14.209 7.88142 14.0886 8.44652 14.0851H15.8115C16.3768 14.0877 16.9353 14.2077 17.4517 14.4375C17.9682 14.6673 18.4312 15.0019 18.8115 15.4201L21.8115 18.7801C22.2902 18.235 22.5377 17.5248 22.5015 16.8001C22.5163 16.6505 22.5163 16.4998 22.5015 16.3501C22.1935 13.2093 21.2925 10.1553 19.8465 7.35014C19.6786 7.02209 19.4177 6.75079 19.0965 6.57014L16.6965 5.25014Z")
                    };
                    handleOutline.SetBinding(Path.FillProperty, new System.Windows.Data.Binding
                    {
                        RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Canvas), 1),
                        Path = new PropertyPath("Tag"),
                    });
                    uIElements.Add(handleOutline);
                    Path buttonA = new Path()
                    {
                        Data = Geometry.Parse("M15.7215 12.0901C15.7195 12.1879 15.7371 12.2851 15.7731 12.376C15.8091 12.4669 15.863 12.5497 15.9314 12.6196C15.9999 12.6894 16.0816 12.7449 16.1717 12.7828C16.2619 12.8207 16.3587 12.8402 16.4565 12.8401C16.6554 12.8401 16.8462 12.7611 16.9868 12.6205C17.1275 12.4798 17.2065 12.2891 17.2065 12.0901C17.1989 11.8951 17.117 11.7103 16.9775 11.5736C16.8381 11.437 16.6517 11.3588 16.4565 11.3551C16.2616 11.3551 16.0746 11.4326 15.9368 11.5704C15.799 11.7083 15.7215 11.8952 15.7215 12.0901Z")
                    };
                    buttonA.SetBinding(Path.FillProperty, new System.Windows.Data.Binding
                    {
                        RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Canvas), 1),
                        Path = new PropertyPath("Tag"),
                    });
                    uIElements.Add(buttonA);
                    Path buttonY = new Path()
                    {
                        Data = Geometry.Parse("M15.7215 9.09014C15.7195 9.18792 15.7371 9.28511 15.7731 9.37603C15.8091 9.46694 15.863 9.54974 15.9314 9.61959C15.9999 9.68944 16.0816 9.74492 16.1717 9.7828C16.2619 9.82067 16.3587 9.84016 16.4565 9.84014C16.6554 9.84014 16.8462 9.76113 16.9868 9.62047C17.1275 9.47982 17.2065 9.28906 17.2065 9.09014C17.1989 8.89506 17.117 8.71028 16.9775 8.57364C16.8381 8.43699 16.6517 8.35882 16.4565 8.35514C16.3614 8.3531 16.2669 8.37032 16.1786 8.40578C16.0903 8.44124 16.0101 8.4942 15.9428 8.56147C15.8756 8.62874 15.8226 8.70893 15.7872 8.79721C15.7517 8.88549 15.7345 8.98003 15.7365 9.07514L15.7215 9.09014Z")
                    };
                    buttonY.SetBinding(Path.FillProperty, new System.Windows.Data.Binding
                    {
                        RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Canvas), 1),
                        Path = new PropertyPath("Tag"),
                    });
                    uIElements.Add(buttonY);
                    Path buttonX = new Path()
                    {
                        Data = Geometry.Parse("M14.2215 10.5901C14.2215 10.7891 14.3005 10.9798 14.4412 11.1205C14.5818 11.2611 14.7726 11.3401 14.9715 11.3401C15.1692 11.3363 15.3578 11.2561 15.4976 11.1162C15.6374 10.9764 15.7177 10.7879 15.7215 10.5901C15.7107 10.3947 15.6283 10.2102 15.4899 10.0718C15.3515 9.9334 15.1669 9.85092 14.9715 9.84014C14.7726 9.84014 14.5818 9.91916 14.4412 10.0598C14.3005 10.2005 14.2215 10.3912 14.2215 10.5901Z")
                    };
                    buttonX.SetBinding(Path.FillProperty, new System.Windows.Data.Binding
                    {
                        RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Canvas), 1),
                        Path = new PropertyPath("Tag"),
                    });
                    uIElements.Add(buttonX);
                    Path buttonB = new Path()
                    {
                        Data = Geometry.Parse("M17.2215 10.5901C17.2215 10.7891 17.3005 10.9798 17.4412 11.1205C17.5818 11.2611 17.7726 11.3401 17.9715 11.3401C18.1692 11.3363 18.3578 11.2561 18.4976 11.1162C18.6374 10.9764 18.7177 10.7879 18.7215 10.5901C18.7141 10.3936 18.6327 10.2071 18.4936 10.068C18.3545 9.92897 18.1681 9.84757 17.9715 9.84014C17.7726 9.84014 17.5818 9.91916 17.4412 10.0598C17.3005 10.2005 17.2215 10.3912 17.2215 10.5901Z")
                    };
                    buttonB.SetBinding(Path.FillProperty, new System.Windows.Data.Binding
                    {
                        RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Canvas), 1),
                        Path = new PropertyPath("Tag"),
                    });
                    uIElements.Add(buttonB);
                    Path buttonHome = new Path()
                    {
                        Data = Geometry.Parse("M11.0415 7.80014C11.0415 8.05475 11.1427 8.29893 11.3227 8.47897C11.5027 8.659 11.7469 8.76014 12.0015 8.76014C12.2495 8.75621 12.4861 8.65491 12.6601 8.47811C12.834 8.30131 12.9315 8.0632 12.9315 7.81514C12.9316 7.56565 12.8346 7.32591 12.661 7.14671C12.4874 6.96751 12.2509 6.86294 12.0015 6.85514C11.7535 6.85511 11.5153 6.95261 11.3386 7.1266C11.1618 7.30059 11.0605 7.53712 11.0565 7.78514L11.0415 7.80014Z")
                    };
                    buttonHome.SetBinding(Path.FillProperty, new System.Windows.Data.Binding
                    {
                        RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Canvas), 1),
                        Path = new PropertyPath("Tag"),
                    });
                    uIElements.Add(buttonHome);
                    Path cross = new Path()
                    {
                        Data = Geometry.Parse("M8.25152 9.87014H9.75152V11.3551H8.25152V12.8251H6.76652V11.3551H5.26652V9.87014H6.76652V8.35514H8.25152V9.87014Z")
                    };
                    cross.SetBinding(Path.FillProperty, new System.Windows.Data.Binding
                    {
                        RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Canvas), 1),
                        Path = new PropertyPath("Tag"),
                    });
                    uIElements.Add(cross);
                    break;
                case VbarIcon.DisplayKVM:
                    Path kvmLeftRect = new Path()
                    {
                        Data = Geometry.Parse("M1,6 L9,6 Q10,6 10,7 L10,20 Q10,21 9,21 L1,21 Q0,21 0,20 L0,7 Q0,6 1,6 Z"),
                        StrokeThickness = 1,
                        Fill = new SolidColorBrush(Colors.Transparent)
                    };
                    kvmLeftRect.SetBinding(Path.StrokeProperty, new System.Windows.Data.Binding
                    {
                        RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Canvas), 1),
                        Path = new PropertyPath("Tag")
                    });
                    uIElements.Add(kvmLeftRect);
                    Path kvmRightRect = new Path()
                    {
                        Data = Geometry.Parse("M15,6 L23,6 Q24,6 24,7 L24,20 Q24,21 23,21 L15,21 Q14,21 14,20 L14,7 Q14,6 15,6 Z"),
                        StrokeThickness = 1,
                        Fill = new SolidColorBrush(Colors.Transparent)
                    };
                    kvmRightRect.SetBinding(Path.StrokeProperty, new System.Windows.Data.Binding
                    {
                        RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Canvas), 1),
                        Path = new PropertyPath("Tag")
                    });
                    uIElements.Add(kvmRightRect);
                    Path kvmTopLeftLine = new Path()
                    {
                        Data = Geometry.Parse("M2,8.5 L9,8.5"),
                        StrokeThickness = 1
                    };
                    kvmTopLeftLine.SetBinding(Path.StrokeProperty, new System.Windows.Data.Binding
                    {
                        RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Canvas), 1),
                        Path = new PropertyPath("Tag")
                    });
                    uIElements.Add(kvmTopLeftLine);
                    Path kvmTopRightLine = new Path()
                    {
                        Data = Geometry.Parse("M15,8.5 L22,8.5"),
                        StrokeThickness = 1
                    };
                    kvmTopRightLine.SetBinding(Path.StrokeProperty, new System.Windows.Data.Binding
                    {
                        RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Canvas), 1),
                        Path = new PropertyPath("Tag")
                    });
                    uIElements.Add(kvmTopRightLine);
                    Path kvmMiddleLine = new Path()
                    {
                        Data = Geometry.Parse("M10,13.5 L14,13.5"),
                        StrokeThickness = 1
                    };
                    kvmMiddleLine.SetBinding(Path.StrokeProperty, new System.Windows.Data.Binding
                    {
                        RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Canvas), 1),
                        Path = new PropertyPath("Tag")
                    });
                    uIElements.Add(kvmMiddleLine);
                    Path kvmLeftCircle = new Path()
                    {
                        Data = Geometry.Parse("M4,17 A1.5,1.5 0 1,1 7,17 A1.5,1.5 0 1,1 4,17 Z")
                    };
                    kvmLeftCircle.SetBinding(Path.FillProperty, new System.Windows.Data.Binding
                    {
                        RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Canvas), 1),
                        Path = new PropertyPath("Tag")
                    });
                    uIElements.Add(kvmLeftCircle);
                    Path kvmRightCircle = new Path()
                    {
                        Data = Geometry.Parse("M17,17 A1.5,1.5 0 1,1 20,17 A1.5,1.5 0 1,1 17,17 Z")
                    };
                    kvmRightCircle.SetBinding(Path.FillProperty, new System.Windows.Data.Binding
                    {
                        RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Canvas), 1),
                        Path = new PropertyPath("Tag")
                    });
                    uIElements.Add(kvmRightCircle);
                    break;
                case VbarIcon.DisplayOthers:
                    Path othersRoundCornerRect = new Path()
                    {
                        Data = Geometry.Parse("M1,5.5 H21 A0.5,0.5 0 0 1 21.5,6 V21 A0.5,0.5 0 0 1 21,21.5 H1 A0.5,0.5 0 0 1 0.5,21 V6 A0.5,0.5 0 0 1 1,5.5 Z"),
                        StrokeThickness = 1.5,
                        Fill = new SolidColorBrush(Colors.Transparent)
                    };
                    othersRoundCornerRect.SetBinding(Path.StrokeProperty, new System.Windows.Data.Binding
                    {
                        RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Canvas), 1),
                        Path = new PropertyPath("Tag")
                    });
                    uIElements.Add(othersRoundCornerRect);
                    Path othersHorLine1 = new Path()
                    {
                        Data = Geometry.Parse("M4,16.25 H9.5"),
                        StrokeThickness = 1.5,
                        Fill = new SolidColorBrush(Colors.Transparent)
                    };
                    othersHorLine1.SetBinding(Path.StrokeProperty, new System.Windows.Data.Binding
                    {
                        RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Canvas), 1),
                        Path = new PropertyPath("Tag")
                    });
                    uIElements.Add(othersHorLine1);
                    Path othersVerLine1 = new Path()
                    {
                        Data = Geometry.Parse("M6.75,8 V19"),
                        StrokeThickness = 1.5,
                        Fill = new SolidColorBrush(Colors.Transparent)
                    };
                    othersVerLine1.SetBinding(Path.StrokeProperty, new System.Windows.Data.Binding
                    {
                        RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Canvas), 1),
                        Path = new PropertyPath("Tag")
                    });
                    uIElements.Add(othersVerLine1);
                    Path othersHorLine2 = new Path()
                    {
                        Data = Geometry.Parse("M17.5,10.75 H12"),
                        StrokeThickness = 1.5,
                        Fill = new SolidColorBrush(Colors.Transparent)
                    };
                    othersHorLine2.SetBinding(Path.StrokeProperty, new System.Windows.Data.Binding
                    {
                        RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Canvas), 1),
                        Path = new PropertyPath("Tag")
                    });
                    uIElements.Add(othersHorLine2);
                    Path othersVerLine2 = new Path()
                    {
                        Data = Geometry.Parse("M14.75,19 V8"),
                        StrokeThickness = 1.5,
                        Fill = new SolidColorBrush(Colors.Transparent)
                    };
                    othersVerLine2.SetBinding(Path.StrokeProperty, new System.Windows.Data.Binding
                    {
                        RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Canvas), 1),
                        Path = new PropertyPath("Tag")
                    });
                    uIElements.Add(othersVerLine2);
                    break;
                case VbarIcon.KeyboardKeyCustom:
                    Path keyboardBoard = new Path()
                    {
                        Data = Geometry.Parse("M12.615 7.035V5.235H13.29C13.6878 5.235 14.0694 5.07696 14.3507 4.79566C14.632 4.51436 14.79 4.13282 14.79 3.735V1.5H13.155V3.63H12.48C12.0822 3.63 11.7006 3.78804 11.4193 4.06934C11.138 4.35064 10.98 4.73218 10.98 5.13V7.035H0V22.5H24V7.035H12.615ZM22.395 20.865H1.605V8.64H22.395V20.865Z")
                    };
                    keyboardBoard.SetBinding(Path.FillProperty, new System.Windows.Data.Binding
                    {
                        RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Canvas), 1),
                        Path = new PropertyPath("Tag")
                    });
                    uIElements.Add(keyboardBoard);
                    Path keyboardKeys = new Path()
                    {
                        Data = Geometry.Parse("M3.765 9.675H6.105V11.28H3.765V9.675ZM3.765 12.495H6.105V14.085H3.765V12.495ZM3.765 15.27H6.105V16.875H3.765V15.27ZM8.475 9.675H10.815V11.28H8.475V9.675ZM8.475 12.495H10.815V14.085H8.475V12.495ZM8.475 15.27H10.815V16.875H8.475V15.27ZM13.185 9.675H15.525V11.28H13.185V9.675ZM13.185 12.495H15.525V14.085H13.185V12.495ZM13.185 15.27H15.525V16.875H13.185V15.27ZM17.895 9.675H20.235V11.28H17.895V9.675ZM17.895 12.495H20.235V14.085H17.895V12.495ZM17.895 15.27H20.235V16.875H17.895V15.27ZM7.365 18.075H16.62V19.68H7.365V18.075Z")
                    };
                    keyboardKeys.SetBinding(Path.FillProperty, new System.Windows.Data.Binding
                    {
                        RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Canvas), 1),
                        Path = new PropertyPath("Tag")
                    });
                    uIElements.Add(keyboardKeys);
                    break;
                case VbarIcon.KeyboardCollaboration:
                    Path keyboardCola1 = new Path()
                    {
                        Data = Geometry.Parse("M23.91 16.7396C23.8121 15.8509 23.531 14.9921 23.0843 14.2176C22.6377 13.443 22.0352 12.7695 21.315 12.2396L20.82 11.9996L20.37 12.3746C19.7953 12.8347 19.0811 13.0854 18.345 13.0854C17.6089 13.0854 16.8947 12.8347 16.32 12.3746L16.17 12.2546L15.99 12.5396C16.7967 13.0824 17.4812 13.7874 18 14.6096C19.0153 14.7023 20.034 14.4713 20.91 13.9496C21.5017 14.5192 21.9358 15.2324 22.17 16.0196H18.645C18.8491 16.538 18.9999 17.0758 19.095 17.6246H24L23.91 16.7396Z")
                    };
                    keyboardCola1.SetBinding(Path.FillProperty, new System.Windows.Data.Binding
                    {
                        RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Canvas), 1),
                        Path = new PropertyPath("Tag")
                    });
                    uIElements.Add(keyboardCola1);
                    Path keyboardCola2 = new Path()
                    {
                        Data = Geometry.Parse("M18.345 6.53965C18.6517 6.53309 18.9545 6.60915 19.2216 6.75984C19.4888 6.91054 19.7105 7.13032 19.8636 7.39617C20.0166 7.66202 20.0953 7.96413 20.0914 8.27085C20.0875 8.57757 20.0012 8.8776 19.8416 9.1395C19.6819 9.40139 19.4547 9.61551 19.1838 9.75941C18.9129 9.90331 18.6083 9.9717 18.3018 9.9574C17.9954 9.9431 17.6985 9.84664 17.4422 9.67814C17.1859 9.50963 16.9796 9.27528 16.845 8.99965C16.8598 9.2244 16.8598 9.44989 16.845 9.67465C16.8397 10.1482 16.7638 10.6184 16.62 11.0696C17.1752 11.397 17.8142 11.5547 18.458 11.5231C19.1018 11.4915 19.7222 11.272 20.2427 10.8918C20.7632 10.5116 21.161 9.98729 21.3869 9.3836C21.6128 8.77992 21.6569 8.12329 21.5139 7.49479C21.3709 6.86629 21.0469 6.29344 20.582 5.84695C20.1172 5.40047 19.5317 5.0999 18.8979 4.98236C18.2642 4.86483 17.6099 4.93546 17.0158 5.18555C16.4217 5.43564 15.9139 5.85423 15.555 6.38965C16.0219 6.90507 16.3746 7.51337 16.59 8.17465C16.6056 7.73559 16.791 7.31972 17.1072 7.0147C17.4234 6.70969 17.8457 6.53937 18.285 6.53965H18.345Z")
                    };
                    keyboardCola2.SetBinding(Path.FillProperty, new System.Windows.Data.Binding
                    {
                        RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Canvas), 1),
                        Path = new PropertyPath("Tag")
                    });
                    uIElements.Add(keyboardCola2);
                    Path keyboardCola3 = new Path()
                    {
                        Data = Geometry.Parse("M6 14.6096C6.50349 13.7771 7.17844 13.0612 7.98 12.5096C7.90948 12.4217 7.8491 12.3261 7.8 12.2246L7.635 12.3446C7.06035 12.8047 6.34615 13.0554 5.61 13.0554C4.87385 13.0554 4.15965 12.8047 3.585 12.3446L3.18 11.9996L2.685 12.3296C1.96609 12.8608 1.36456 13.5346 0.918034 14.3089C0.471513 15.0832 0.189621 15.9414 0.09 16.8296L0 17.6246H4.905C5.00199 17.0762 5.15272 16.5386 5.355 16.0196H1.875C2.10918 15.2324 2.54327 14.5192 3.135 13.9496C3.99821 14.462 4.99962 14.6927 6 14.6096Z")
                    };
                    keyboardCola3.SetBinding(Path.FillProperty, new System.Windows.Data.Binding
                    {
                        RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Canvas), 1),
                        Path = new PropertyPath("Tag")
                    });
                    uIElements.Add(keyboardCola3);
                    Path keyboardCola4 = new Path()
                    {
                        Data = Geometry.Parse("M7.335 11.0696C7.19116 10.6184 7.11533 10.1482 7.11 9.67465C7.09451 9.44491 7.09451 9.21439 7.11 8.98465C6.97366 9.26808 6.76115 9.50798 6.49624 9.67752C6.23133 9.84706 5.92448 9.93955 5.61 9.94465C5.27514 9.93578 4.95026 9.82876 4.6757 9.63687C4.40113 9.44497 4.18897 9.17667 4.06556 8.86525C3.94215 8.55384 3.91291 8.21304 3.98149 7.88516C4.05007 7.55728 4.21344 7.25676 4.45132 7.02092C4.6892 6.78507 4.99111 6.62429 5.31956 6.55853C5.64802 6.49276 5.98856 6.52492 6.2989 6.651C6.60924 6.77708 6.87572 6.99154 7.06525 7.26774C7.25478 7.54394 7.35901 7.86973 7.365 8.20465C7.57778 7.53327 7.93054 6.91466 8.4 6.38965C8.09934 5.93485 7.69207 5.56044 7.21364 5.29901C6.73522 5.03757 6.20014 4.89704 5.655 4.88965C4.77979 4.88965 3.94042 5.23733 3.32155 5.8562C2.70268 6.47507 2.355 7.31443 2.355 8.18965C2.355 9.06486 2.70268 9.90423 3.32155 10.5231C3.94042 11.142 4.77979 11.4896 5.655 11.4896C6.24411 11.5178 6.82846 11.3717 7.335 11.0696Z")
                    };
                    keyboardCola4.SetBinding(Path.FillProperty, new System.Windows.Data.Binding
                    {
                        RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Canvas), 1),
                        Path = new PropertyPath("Tag")
                    });
                    uIElements.Add(keyboardCola4);
                    Path keyboardCola5 = new Path()
                    {
                        Data = Geometry.Parse("M15 13.7396L14.52 13.4096L14.055 13.7846C13.4815 14.2473 12.7669 14.4997 12.03 14.4997C11.2931 14.4997 10.5785 14.2473 10.005 13.7846L9.54 13.4096L9 13.7396C8.27848 14.2682 7.67506 14.9414 7.22825 15.7162C6.78145 16.491 6.50106 17.3504 6.405 18.2396L6.3 19.1246H17.64L17.535 18.2396C17.4462 17.3552 17.1751 16.4987 16.7388 15.7242C16.3025 14.9497 15.7104 14.274 15 13.7396ZM8.22 17.4597C8.45162 16.6791 8.88039 15.9714 9.465 15.4046C10.2305 15.8649 11.1068 16.108 12 16.108C12.8932 16.108 13.7695 15.8649 14.535 15.4046C15.1196 15.9714 15.5484 16.6791 15.78 17.4597H8.22Z")
                    };
                    keyboardCola5.SetBinding(Path.FillProperty, new System.Windows.Data.Binding
                    {
                        RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Canvas), 1),
                        Path = new PropertyPath("Tag")
                    });
                    uIElements.Add(keyboardCola5);
                    Path keyboardCola6 = new Path()
                    {
                        Data = Geometry.Parse("M12 12.9446C12.5253 12.946 13.0432 12.8215 13.5104 12.5814C13.9776 12.3412 14.3804 11.9926 14.685 11.5646C15.0788 11.0069 15.2933 10.3424 15.3 9.65965C15.3 8.78443 14.9523 7.94507 14.3335 7.3262C13.7146 6.70733 12.8752 6.35965 12 6.35965C11.1248 6.35965 10.2854 6.70733 9.66655 7.3262C9.04768 7.94507 8.7 8.78443 8.7 9.65965C8.70669 10.3424 8.92121 11.0069 9.315 11.5646C9.61961 11.9926 10.0224 12.3412 10.4896 12.5814C10.9568 12.8215 11.4747 12.946 12 12.9446ZM12 7.94965C12.3418 7.94966 12.6759 8.05122 12.9599 8.24144C13.2439 8.43166 13.465 8.70195 13.5951 9.01803C13.7252 9.33411 13.7585 9.68171 13.6907 10.0167C13.6229 10.3518 13.4571 10.6591 13.2144 10.8997C12.9716 11.1404 12.6628 11.3035 12.3272 11.3683C11.9916 11.4332 11.6443 11.3969 11.3294 11.264C11.0145 11.1311 10.7461 10.9077 10.5584 10.6221C10.3707 10.3364 10.272 10.0014 10.275 9.65965C10.279 9.20475 10.4624 8.76984 10.7855 8.44957C11.1086 8.12931 11.5451 7.94963 12 7.94965Z")
                    };
                    keyboardCola6.SetBinding(Path.FillProperty, new System.Windows.Data.Binding
                    {
                        RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Canvas), 1),
                        Path = new PropertyPath("Tag")
                    });
                    uIElements.Add(keyboardCola6);
                    break;
                case VbarIcon.KeyboardIllumination:
                    Path keyboardIlu1 = new Path()
                    {
                        Data = Geometry.Parse("M12 0C5.65504 0 3.96004 4.5 3.96004 7.02C3.89932 7.93709 4.03668 8.8565 4.3628 9.71579C4.68892 10.5751 5.19615 11.3541 5.85004 12C6.48516 12.6596 6.96334 13.454 7.24901 14.324C7.53468 15.194 7.62049 16.1173 7.50004 17.025H9.13504C9.2438 15.931 9.11073 14.8265 8.74524 13.7896C8.37975 12.7528 7.79078 11.809 7.02004 11.025C6.48355 10.5206 6.06949 9.90023 5.80951 9.21129C5.54953 8.52234 5.45052 7.78307 5.52004 7.05C5.52004 6.135 5.83504 1.62 11.97 1.605C18.105 1.59 18.42 6.105 18.42 7.05C18.4896 7.78307 18.3905 8.52234 18.1306 9.21129C17.8706 9.90023 17.4565 10.5206 16.92 11.025C16.1493 11.809 15.5603 12.7528 15.1948 13.7896C14.8293 14.8265 14.6963 15.931 14.805 17.025H16.5C16.3796 16.1173 16.4654 15.194 16.7511 14.324C17.0367 13.454 17.5149 12.6596 18.15 12C18.8119 11.3474 19.3238 10.5585 19.6502 9.68818C19.9766 8.81785 20.1096 7.88691 20.04 6.96C20.04 4.5 18.345 0 12 0Z")
                    };
                    keyboardIlu1.SetBinding(Path.FillProperty, new System.Windows.Data.Binding
                    {
                        RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Canvas), 1),
                        Path = new PropertyPath("Tag")
                    });
                    uIElements.Add(keyboardIlu1);
                    Path keyboardIlu2 = new Path()
                    {
                        Data = Geometry.Parse("M7.60504 18.48H16.395V20.25H7.60504V18.48Z")
                    };
                    keyboardIlu2.SetBinding(Path.FillProperty, new System.Windows.Data.Binding
                    {
                        RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Canvas), 1),
                        Path = new PropertyPath("Tag")
                    });
                    uIElements.Add(keyboardIlu2);
                    Path keyboardIlu3 = new Path()
                    {
                        Data = Geometry.Parse("M16.395 22.965V21.465H7.60504V22.965C9.10504 24.345 14.985 24.345 16.38 22.965H16.395Z")
                    };
                    keyboardIlu3.SetBinding(Path.FillProperty, new System.Windows.Data.Binding
                    {
                        RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Canvas), 1),
                        Path = new PropertyPath("Tag")
                    });
                    uIElements.Add(keyboardIlu3);
                    break;
                case VbarIcon.MouseSettings:
                    Path mouseSettings = new Path()
                    {
                        Data = Geometry.Parse("M21 12.855L3 0L5.7 22.05L9.57 16.8L13.86 24L18.93 21L14.625 13.77L21 12.855ZM16.5 20.355L14.415 21.585L9.72 13.65L6.885 17.49L5.16 3.72L16.5 11.745L11.79 12.42L16.5 20.37V20.355Z")
                    };
                    mouseSettings.SetBinding(Path.FillProperty, new System.Windows.Data.Binding
                    {
                        RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Canvas), 1),
                        Path = new PropertyPath("Tag")
                    });
                    uIElements.Add(mouseSettings);
                    break;
                case VbarIcon.MouseButton:
                    Path mouseButton1 = new Path()
                    {
                        Data = Geometry.Parse("M11.2055 4.02H12.7955V8.31H11.2055V4.02Z")
                    };
                    mouseButton1.SetBinding(Path.FillProperty, new System.Windows.Data.Binding
                    {
                        RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Canvas), 1),
                        Path = new PropertyPath("Tag")
                    });
                    uIElements.Add(mouseButton1);
                    Path mouseButton2 = new Path()
                    {
                        Data = Geometry.Parse("M13.0955 0H10.9055C9.42288 0.00395908 8.00207 0.594702 6.95366 1.64311C5.90525 2.69152 5.31451 4.11233 5.31055 5.595V18.405C5.31451 19.8877 5.90525 21.3085 6.95366 22.3569C8.00207 23.4053 9.42288 23.996 10.9055 24H13.0955C14.5782 23.996 15.999 23.4053 17.0474 22.3569C18.0958 21.3085 18.6866 19.8877 18.6905 18.405V5.595C18.6866 4.11233 18.0958 2.69152 17.0474 1.64311C15.999 0.594702 14.5782 0.00395908 13.0955 0ZM17.1005 18.405C17.0966 19.4646 16.6729 20.4794 15.9222 21.2273C15.1716 21.9751 14.1551 22.395 13.0955 22.395H10.9055C9.84595 22.395 8.82953 21.9751 8.07887 21.2273C7.32822 20.4794 6.90452 19.4646 6.90055 18.405V5.595C6.90452 4.53541 7.32822 3.52057 8.07887 2.77273C8.82953 2.02489 9.84595 1.60499 10.9055 1.605H13.0955C14.1551 1.60499 15.1716 2.02489 15.9222 2.77273C16.6729 3.52057 17.0966 4.53541 17.1005 5.595V18.405Z")
                    };
                    mouseButton2.SetBinding(Path.FillProperty, new System.Windows.Data.Binding
                    {
                        RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Canvas), 1),
                        Path = new PropertyPath("Tag")
                    });
                    uIElements.Add(mouseButton2);
                    break;
                case VbarIcon.PenSettings:
                    Path penSettings1 = new Path()
                    {
                        Data = Geometry.Parse("M19.605 0L2.76 16.845L0.27 23.235L0 24L0.765 23.73L7.17 21.225L24 4.395L19.605 0ZM2.805 21.195L4.095 17.865L6.15 19.89L2.805 21.195ZM7.305 18.78L5.19 16.68L17.19 4.68L19.305 6.765L7.305 18.765V18.78ZM18.36 3.54L19.605 2.295L21.705 4.395L20.475 5.625L18.36 3.54Z")
                    };
                    penSettings1.SetBinding(Path.FillProperty, new System.Windows.Data.Binding
                    {
                        RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Canvas), 1),
                        Path = new PropertyPath("Tag")
                    });
                    uIElements.Add(penSettings1);
                    Path penSettings2 = new Path()
                    {
                        Data = Geometry.Parse("M15.2454 18.4609L14.7241 17.957L14.2029 18.4609L10.283 22.2502H6.79497L3 23.7348V23.7502H10.5862H10.8894L11.1075 23.5394L14.7241 20.0433L18.3408 23.5394L18.8621 24.0433L19.3833 23.5394L23.5213 19.5394L22.4787 18.4609L18.8621 21.957L15.2454 18.4609ZM3 22.3625L3.2872 22.2502H3V22.3625Z")
                    };
                    penSettings2.SetBinding(Path.FillProperty, new System.Windows.Data.Binding
                    {
                        RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Canvas), 1),
                        Path = new PropertyPath("Tag")
                    });
                    uIElements.Add(penSettings2);
                    break;
                case VbarIcon.PenButton:
                    Path penButton1 = new Path()
                    {
                        Data = Geometry.Parse("M18.7881 1L2.645 17.1431L0.25875 23.2669L0 24L0.733125 23.7412L6.87125 21.3406L23 5.21187L18.7881 1ZM2.68812 21.3119L3.92438 18.1206L5.89375 20.0612L2.68812 21.3119ZM7.00062 18.9975L4.97375 16.985L16.4738 5.485L18.5006 7.48313L7.00062 18.9831V18.9975ZM17.595 4.3925L18.7881 3.19937L20.8006 5.21187L19.6219 6.39062L17.595 4.3925Z")
                    };
                    penButton1.SetBinding(Path.FillProperty, new System.Windows.Data.Binding
                    {
                        RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Canvas), 1),
                        Path = new PropertyPath("Tag")
                    });
                    uIElements.Add(penButton1);
                    Path penButton2 = new Path()
                    {
                        Data = Geometry.Parse("M3,13.1753 L5.72668,10.4486 L6.76781,11.4897 L4.04113,14.2164 Z")
                    };
                    penButton2.SetBinding(Path.FillProperty, new System.Windows.Data.Binding
                    {
                        RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Canvas), 1),
                        Path = new PropertyPath("Tag")
                    });
                    uIElements.Add(penButton2);
                    Path penButton3 = new Path()
                    {
                        Data = Geometry.Parse("M6.44531,9.72754 L9.17205,7.00082 L10.2132,8.04195 L7.48646,10.7687 Z")
                    };
                    penButton3.SetBinding(Path.FillProperty, new System.Windows.Data.Binding
                    {
                        RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Canvas), 1),
                        Path = new PropertyPath("Tag")
                    });
                    uIElements.Add(penButton3);
                    Path penButton4 = new Path()
                    {
                        Data = Geometry.Parse("M19.7891,-0.000976562 L23.9915,4.20138 L22.9503,5.24251 L18.7479,1.04115 Z")
                    };
                    penButton4.SetBinding(Path.FillProperty, new System.Windows.Data.Binding
                    {
                        RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Canvas), 1),
                        Path = new PropertyPath("Tag")
                    });
                    uIElements.Add(penButton4);
                    break;
                case VbarIcon.SpeakerPhonePreset:
                    Path speakerPhonePreset = new Path()
                    {
                        Data = Geometry.Parse("M16.95 11.19L14.655 18.465L9.375 0L5.58 11.235H0V12.825H6.72L9.24 5.325L14.58 24L18.12 12.795H23.985V11.235L16.95 11.19Z")
                    };
                    speakerPhonePreset.SetBinding(Path.FillProperty, new System.Windows.Data.Binding
                    {
                        RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Canvas), 1),
                        Path = new PropertyPath("Tag")
                    });
                    uIElements.Add(speakerPhonePreset);
                    break;
                case VbarIcon.AudioSettings:
                    Path audioSettings1 = new Path()
                    {
                        Data = Geometry.Parse("M4.50047 8.24981H0.480469V15.7498H4.50047L9.37547 21.1498L12.6155 21.7648V2.2648L9.37547 2.8498L4.50047 8.24981ZM11.0105 4.1698V19.8448L10.2005 19.6798L5.22047 14.2048H2.07047V9.83981H5.22047L10.2005 4.31981L11.0105 4.1698Z")
                    };
                    audioSettings1.SetBinding(Path.FillProperty, new System.Windows.Data.Binding
                    {
                        RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Canvas), 1),
                        Path = new PropertyPath("Tag")
                    });
                    uIElements.Add(audioSettings1);
                    Path audioSettings2 = new Path()
                    {
                        Data = Geometry.Parse("M16.8605 0.674805L16.0955 2.0098C17.8569 3.01597 19.3209 4.46997 20.3393 6.22439C21.3576 7.97881 21.8939 9.97128 21.8939 11.9998C21.8939 14.0283 21.3576 16.0208 20.3393 17.7752C19.3209 19.5296 17.8569 20.9836 16.0955 21.9898L16.8605 23.3248C18.8592 22.1857 20.5208 20.538 21.6767 18.5489C22.8325 16.5599 23.4414 14.3003 23.4414 11.9998C23.4414 9.69929 22.8325 7.43976 21.6767 5.45069C20.5208 3.46162 18.8592 1.81387 16.8605 0.674805Z")
                    };
                    audioSettings2.SetBinding(Path.FillProperty, new System.Windows.Data.Binding
                    {
                        RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Canvas), 1),
                        Path = new PropertyPath("Tag")
                    });
                    uIElements.Add(audioSettings2);
                    Path audioSettings3 = new Path()
                    {
                        Data = Geometry.Parse("M18.7505 11.9998C18.7634 10.6379 18.4353 9.29445 17.796 8.09186C17.1568 6.88927 16.2266 5.86582 15.0905 5.11481L14.2805 6.41981C15.2094 7.02263 15.9712 7.84998 16.4956 8.82532C17.02 9.80066 17.2899 10.8925 17.2805 11.9998C17.2952 13.0902 17.0382 14.1671 16.5328 15.1334C16.0274 16.0996 15.2895 16.9249 14.3855 17.5348L15.2105 18.8248C16.3178 18.0676 17.2206 17.0481 17.8382 15.8572C18.4559 14.6664 18.7693 13.3412 18.7505 11.9998Z")
                    };
                    audioSettings3.SetBinding(Path.FillProperty, new System.Windows.Data.Binding
                    {
                        RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Canvas), 1),
                        Path = new PropertyPath("Tag")
                    });
                    uIElements.Add(audioSettings3);
                    break;
                case VbarIcon.SpeakerPhoneInteractions:
                    Path interaction1 = new Path()
                    {
                        Data = Geometry.Parse("M19.5 15.45C19.4962 14.613 19.1745 13.8087 18.6 13.2C18.3874 13.0429 18.1427 12.9346 17.8835 12.8827C17.6242 12.8309 17.3567 12.8368 17.1 12.9H16.95C16.6495 12.6285 16.2888 12.4325 15.8975 12.3282C15.5062 12.2238 15.0957 12.2142 14.7 12.3C14.4603 12.1393 14.1901 12.0298 13.9062 11.9781C13.6224 11.9265 13.3309 11.934 13.05 12V8.25C13.05 6.3 12 6 10.95 6C10.3784 5.99873 9.82768 6.21508 9.40978 6.60512C8.99188 6.99516 8.73811 7.52964 8.7 8.1V15.9L7.5 14.85C7.07327 14.4749 6.53911 14.2441 5.97355 14.1902C5.40799 14.1363 4.83985 14.2622 4.35 14.55C4.02245 14.7087 3.75659 14.9713 3.59381 15.2969C3.43103 15.6225 3.38047 15.9927 3.45 16.35C3.6 17.7 5.55 18.9 5.85 19.2L8.85 21.6L10.65 24H17.85V23.25C18.0352 22.625 18.2866 22.0216 18.6 21.45C18.6 21.3 18.75 21.15 18.75 21C19.136 20.249 19.3897 19.4371 19.5 18.6V15.45ZM17.1 20.85L16.35 22.35H11.55L10.2 20.7C9.25947 19.6796 8.20221 18.7734 7.05 18C6.15 17.4 5.25 16.5 5.25 16.05H5.55C5.7357 15.9989 5.93188 15.9999 6.11707 16.0528C6.30226 16.1057 6.46933 16.2085 6.6 16.35L8.25 17.7C8.41883 17.829 8.61889 17.9109 8.82972 17.9372C9.04056 17.9636 9.25461 17.9335 9.45 17.85C9.66705 17.7505 9.85993 17.6051 10.0153 17.4238C10.1707 17.2426 10.2849 17.0297 10.35 16.8V8.25C10.35 8.09087 10.4132 7.93826 10.5257 7.82574C10.6383 7.71321 10.7909 7.65 10.95 7.65C11.4 7.65 11.55 7.65 11.55 8.25V13.2L11.85 13.5C12.072 13.6483 12.333 13.7275 12.6 13.7275C12.867 13.7275 13.128 13.6483 13.35 13.5H13.65C13.8509 13.7287 14.1232 13.8828 14.4228 13.9373C14.7223 13.9917 15.0314 13.9434 15.3 13.8H15.75C15.9509 14.0326 16.2081 14.2098 16.4969 14.3148C16.7857 14.4198 17.0966 14.4492 17.4 14.4H17.55C17.7408 14.7174 17.8443 15.0797 17.85 15.45V18.3C17.7797 19.1929 17.5243 20.0613 17.1 20.85Z")
                    };
                    interaction1.SetBinding(Path.FillProperty, new System.Windows.Data.Binding
                    {
                        RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Canvas), 1),
                        Path = new PropertyPath("Tag")
                    });
                    uIElements.Add(interaction1);
                    Path interaction2 = new Path()
                    {
                        Data = Geometry.Parse("M10.95 1.65C12.6209 1.65 14.2233 2.31375 15.4048 3.49523C16.5863 4.67671 17.25 6.27914 17.25 7.95H18.9C18.9 5.84153 18.0624 3.81942 16.5715 2.3285C15.0806 0.837587 13.0585 0 10.95 0C8.84153 0 6.81941 0.837587 5.3285 2.3285C3.83759 3.81942 3 5.84153 3 7.95H4.65C4.65 6.27914 5.31375 4.67671 6.49523 3.49523C7.67671 2.31375 9.27914 1.65 10.95 1.65Z")
                    };
                    interaction2.SetBinding(Path.FillProperty, new System.Windows.Data.Binding
                    {
                        RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Canvas), 1),
                        Path = new PropertyPath("Tag")
                    });
                    uIElements.Add(interaction2);
                    Path interaction3 = new Path()
                    {
                        Data = Geometry.Parse("M10.95 3.15C9.67696 3.15 8.45606 3.65571 7.55589 4.55589C6.65571 5.45606 6.15 6.67696 6.15 7.95H7.65C7.65 7.07479 7.99768 6.23542 8.61655 5.61655C9.23542 4.99768 10.0748 4.65 10.95 4.65C11.8252 4.65 12.6646 4.99768 13.2835 5.61655C13.9023 6.23542 14.25 7.07479 14.25 7.95H15.75C15.75 6.67696 15.2443 5.45606 14.3441 4.55589C13.4439 3.65571 12.223 3.15 10.95 3.15Z")
                    };
                    interaction3.SetBinding(Path.FillProperty, new System.Windows.Data.Binding
                    {
                        RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Canvas), 1),
                        Path = new PropertyPath("Tag")
                    });
                    uIElements.Add(interaction3);
                    Path interaction4 = new Path()
                    {
                        Data = Geometry.Parse("M19.5 15.45C19.4962 14.613 19.1745 13.8087 18.6 13.2C18.3874 13.0429 18.1427 12.9346 17.8835 12.8827C17.6242 12.8309 17.3567 12.8368 17.1 12.9H16.95C16.6495 12.6285 16.2888 12.4325 15.8975 12.3282C15.5062 12.2238 15.0957 12.2142 14.7 12.3C14.4603 12.1393 14.1901 12.0298 13.9062 11.9781C13.6224 11.9265 13.3309 11.934 13.05 12V8.25C13.05 6.3 12 6 10.95 6C10.3784 5.99873 9.82768 6.21508 9.40978 6.60512C8.99188 6.99516 8.73811 7.52964 8.7 8.1V15.9L7.5 14.85C7.07327 14.4749 6.53911 14.2441 5.97355 14.1902C5.40799 14.1363 4.83985 14.2622 4.35 14.55C4.02245 14.7087 3.75659 14.9713 3.59381 15.2969C3.43103 15.6225 3.38047 15.9927 3.45 16.35C3.6 17.7 5.55 18.9 5.85 19.2L8.85 21.6L10.65 24H17.85V23.25C18.0352 22.625 18.2866 22.0216 18.6 21.45C18.6 21.3 18.75 21.15 18.75 21C19.136 20.249 19.3897 19.4371 19.5 18.6V15.45ZM17.1 20.85L16.35 22.35H11.55L10.2 20.7C9.25947 19.6796 8.20221 18.7734 7.05 18C6.15 17.4 5.25 16.5 5.25 16.05H5.55C5.7357 15.9989 5.93188 15.9999 6.11707 16.0528C6.30226 16.1057 6.46933 16.2085 6.6 16.35L8.25 17.7C8.41883 17.829 8.61889 17.9109 8.82972 17.9372C9.04056 17.9636 9.25461 17.9335 9.45 17.85C9.66705 17.7505 9.85993 17.6051 10.0153 17.4238C10.1707 17.2426 10.2849 17.0297 10.35 16.8V8.25C10.35 8.09087 10.4132 7.93826 10.5257 7.82574C10.6383 7.71321 10.7909 7.65 10.95 7.65C11.4 7.65 11.55 7.65 11.55 8.25V13.2L11.85 13.5C12.072 13.6483 12.333 13.7275 12.6 13.7275C12.867 13.7275 13.128 13.6483 13.35 13.5H13.65C13.8509 13.7287 14.1232 13.8828 14.4228 13.9373C14.7223 13.9917 15.0314 13.9434 15.3 13.8H15.75C15.9509 14.0326 16.2081 14.2098 16.4969 14.3148C16.7857 14.4198 17.0966 14.4492 17.4 14.4H17.55C17.7408 14.7174 17.8443 15.0797 17.85 15.45V18.3C17.7797 19.1929 17.5243 20.0613 17.1 20.85Z")
                    };
                    interaction4.SetBinding(Path.FillProperty, new System.Windows.Data.Binding
                    {
                        RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Canvas), 1),
                        Path = new PropertyPath("Tag")
                    });
                    uIElements.Add(interaction4);
                    Path interaction5 = new Path()
                    {
                        Data = Geometry.Parse("M10.95 1.65C12.6209 1.65 14.2233 2.31375 15.4048 3.49523C16.5863 4.67671 17.25 6.27914 17.25 7.95H18.9C18.9 5.84153 18.0624 3.81942 16.5715 2.3285C15.0806 0.837587 13.0585 0 10.95 0C8.84153 0 6.81941 0.837587 5.3285 2.3285C3.83759 3.81942 3 5.84153 3 7.95H4.65C4.65 6.27914 5.31375 4.67671 6.49523 3.49523C7.67671 2.31375 9.27914 1.65 10.95 1.65Z")
                    };
                    interaction5.SetBinding(Path.FillProperty, new System.Windows.Data.Binding
                    {
                        RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Canvas), 1),
                        Path = new PropertyPath("Tag")
                    });
                    uIElements.Add(interaction5);
                    Path interaction6 = new Path()
                    {
                        Data = Geometry.Parse("M10.95 3.15C9.67696 3.15 8.45606 3.65571 7.55589 4.55589C6.65571 5.45606 6.15 6.67696 6.15 7.95H7.65C7.65 7.07479 7.99768 6.23542 8.61655 5.61655C9.23542 4.99768 10.0748 4.65 10.95 4.65C11.8252 4.65 12.6646 4.99768 13.2835 5.61655C13.9023 6.23542 14.25 7.07479 14.25 7.95H15.75C15.75 6.67696 15.2443 5.45606 14.3441 4.55589C13.4439 3.65571 12.223 3.15 10.95 3.15Z")
                    };
                    interaction6.SetBinding(Path.FillProperty, new System.Windows.Data.Binding
                    {
                        RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Canvas), 1),
                        Path = new PropertyPath("Tag")
                    });
                    uIElements.Add(interaction6);
                    break;
                case VbarIcon.HeadsetAutoActions:
                    Path autoActions1 = new Path()
                    {
                        Data = Geometry.Parse("M9.135 18.7795L17.28 13.6045L9.135 8.41453V18.7795ZM10.74 11.2795L14.295 13.6045L10.74 15.8695V11.2795Z")
                    };
                    autoActions1.SetBinding(Path.FillProperty, new System.Windows.Data.Binding
                    {
                        RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Canvas), 1),
                        Path = new PropertyPath("Tag")
                    });
                    uIElements.Add(autoActions1);
                    Path autoActions2 = new Path()
                    {
                        Data = Geometry.Parse("M11.22 4.60453L9 1.39453H0V22.6045H24V4.60453H11.22ZM22.395 20.9995H1.605V2.99953H8.175L10.395 6.19453H22.395V20.9995Z")
                    };
                    autoActions2.SetBinding(Path.FillProperty, new System.Windows.Data.Binding
                    {
                        RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Canvas), 1),
                        Path = new PropertyPath("Tag")
                    });
                    uIElements.Add(autoActions2);
                    break;
                case VbarIcon.HeadsetSettings:
                    Path headsetSettings = new Path()
                    {
                        Data = Geometry.Parse("M11.9998 0.000115429C10.7941 -0.04595 9.5913 0.148082 8.46125 0.570936C7.33121 0.993789 6.29648 1.63703 5.4172 2.46327C4.53792 3.28951 3.83163 4.28228 3.33938 5.38387C2.84713 6.48545 2.57874 7.67389 2.5498 8.88012V15.4351H3.7048V15.6901C3.7048 16.1097 3.78745 16.5252 3.94801 16.9128C4.10857 17.3004 4.34392 17.6526 4.6406 17.9493C4.93728 18.246 5.2895 18.4813 5.67713 18.6419C6.06477 18.8025 6.48023 18.8851 6.8998 18.8851H8.3998V10.5001H6.8998C6.34342 10.5032 5.79749 10.6515 5.31603 10.9304C4.83458 11.2093 4.4343 11.609 4.1548 12.0901V8.88012C4.18361 7.88457 4.41041 6.90473 4.82198 5.99778C5.23355 5.09083 5.82163 4.27493 6.55191 3.59771C7.2822 2.92049 8.14005 2.3955 9.07542 2.05339C10.0108 1.71127 11.0049 1.55888 11.9998 1.60512C12.9949 1.54831 13.9912 1.69368 14.9287 2.03244C15.8661 2.37121 16.7251 2.89636 17.454 3.57621C18.183 4.25606 18.7666 5.07651 19.1698 5.98808C19.5729 6.89966 19.7872 7.88345 19.7998 8.88012V12.0901C19.5242 11.6155 19.131 11.2199 18.6581 10.9414C18.1852 10.6629 17.6485 10.5109 17.0998 10.5001H15.5998V18.8851H17.0998C17.6562 18.882 18.2021 18.7337 18.6836 18.4549C19.165 18.176 19.5653 17.7762 19.8448 17.2951V19.8601L14.9998 22.3951H12.7948V21.0001H11.2048V24.0001H15.4198L21.4198 20.8201V8.82012C21.3971 7.61726 21.1334 6.43117 20.6444 5.33196C20.1554 4.23276 19.4509 3.24276 18.5727 2.42049C17.6945 1.59821 16.6603 0.96036 15.5314 0.544637C14.4024 0.128913 13.2016 -0.0562429 11.9998 0.000115429ZM5.3098 13.6951C5.30915 13.2886 5.46426 12.8972 5.74325 12.6015C6.02223 12.3057 6.4039 12.1281 6.8098 12.1051V17.2801C6.4039 17.2571 6.02223 17.0795 5.74325 16.7838C5.46426 16.488 5.30915 16.0967 5.3098 15.6901V13.6951ZM18.6898 15.6901C18.6905 16.0967 18.5353 16.488 18.2564 16.7838C17.9774 17.0795 17.5957 17.2571 17.1898 17.2801V12.1051C17.5957 12.1281 17.9774 12.3057 18.2564 12.6015C18.5353 12.8972 18.6905 13.2886 18.6898 13.6951V15.6901Z")
                    };
                    headsetSettings.SetBinding(Path.FillProperty, new System.Windows.Data.Binding
                    {
                        RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Canvas), 1),
                        Path = new PropertyPath("Tag")
                    });
                    uIElements.Add(headsetSettings);
                    break;
                case VbarIcon.WebcamControl:
                    Path webcamControl = new Path();
                    webcamControl.Data = Geometry.Parse("M19.5 9.525H21V24H19.5V9.525Z");
                    webcamControl.SetBinding(Path.FillProperty, new System.Windows.Data.Binding
                    {
                        RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Canvas), 1),
                        Path = new PropertyPath("Tag")
                    });
                    uIElements.Add(webcamControl);
                    webcamControl = new Path();
                    webcamControl.Data = Geometry.Parse("M19.5 0H21V4.725H19.5V0Z");
                    webcamControl.SetBinding(Path.FillProperty, new System.Windows.Data.Binding
                    {
                        RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Canvas), 1),
                        Path = new PropertyPath("Tag")
                    });
                    uIElements.Add(webcamControl);
                    webcamControl = new Path();
                    webcamControl.Data = Geometry.Parse("M11.205 20.775H12.795V24H11.205V20.775Z");
                    webcamControl.SetBinding(Path.FillProperty, new System.Windows.Data.Binding
                    {
                        RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Canvas), 1),
                        Path = new PropertyPath("Tag")
                    });
                    uIElements.Add(webcamControl);
                    webcamControl = new Path();
                    webcamControl.Data = Geometry.Parse("M11.205 0H12.795V15.975H11.205V0Z");
                    webcamControl.SetBinding(Path.FillProperty, new System.Windows.Data.Binding
                    {
                        RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Canvas), 1),
                        Path = new PropertyPath("Tag")
                    });
                    uIElements.Add(webcamControl);
                    webcamControl = new Path();
                    webcamControl.Data = Geometry.Parse("M3 0H4.5V4.725H3V0Z");
                    webcamControl.SetBinding(Path.FillProperty, new System.Windows.Data.Binding
                    {
                        RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Canvas), 1),
                        Path = new PropertyPath("Tag")
                    });
                    uIElements.Add(webcamControl);
                    webcamControl = new Path();
                    webcamControl.Data = Geometry.Parse("M3 9.525H4.5V24H3V9.525Z");
                    webcamControl.SetBinding(Path.FillProperty, new System.Windows.Data.Binding
                    {
                        RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Canvas), 1),
                        Path = new PropertyPath("Tag")
                    });
                    uIElements.Add(webcamControl);
                    webcamControl = new Path();
                    webcamControl.Data = Geometry.Parse("M16.5 6.33V7.92H24V6.33H16.5Z");
                    webcamControl.SetBinding(Path.FillProperty, new System.Windows.Data.Binding
                    {
                        RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Canvas), 1),
                        Path = new PropertyPath("Tag")
                    });
                    uIElements.Add(webcamControl);
                    webcamControl = new Path();
                    webcamControl.Data = Geometry.Parse("M7.5 6.33H0V7.92H7.5V6.33Z");
                    webcamControl.SetBinding(Path.FillProperty, new System.Windows.Data.Binding
                    {
                        RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Canvas), 1),
                        Path = new PropertyPath("Tag")
                    });
                    uIElements.Add(webcamControl);
                    webcamControl = new Path();
                    webcamControl.Data = Geometry.Parse("M8.205 17.58V19.17H15.705V17.58H8.205Z");
                    webcamControl.SetBinding(Path.FillProperty, new System.Windows.Data.Binding
                    {
                        RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Canvas), 1),
                        Path = new PropertyPath("Tag")
                    });
                    uIElements.Add(webcamControl);
                    break;
                case VbarIcon.WebcamColorImg:
                    Path webcamColorImg = new Path();
                    webcamColorImg.Data = Geometry.Parse("M18.6,10.29C18.2688,9.95349 18.0192,9.54556 17.8702,9.09753C17.7212,8.64951 17.6769,8.1733 17.7406,7.70548C17.8043,7.23765 17.9744,6.79066 18.2378,6.3988C18.5012,6.00695 18.8509,5.68067 19.26,5.445C19.7165,5.1643 20.0938,4.77177 20.3563,4.30456C20.6188,3.83735 20.7577,3.31089 20.76,2.775C20.775,1.02 18.57,0 16.74,0C9,0 3,5.58 3,12.96C3.00198,15.5976 3.80778,18.172 5.31,20.34C6.11133,21.4897 7.18299,22.4249 8.43066,23.0631C9.67833,23.7013 11.0638,24.023 12.465,24C13.5594,24.0319 14.6492,23.8469 15.6717,23.4556C16.6942,23.0643 17.6291,22.4744 18.4226,21.72C19.216,20.9656 19.8522,20.0616 20.2946,19.0602C20.737,18.0587 20.9767,16.9796 21,15.885C20.9596,13.7785 20.0984,11.7711 18.6,10.29ZM12.465,22.395C11.3357,22.3997 10.2222,22.129 9.22109,21.6062C8.22,21.0835 7.36151,20.3245 6.72,19.395L6.645,19.29C5.36975,17.4241 4.68034,15.22 4.665,12.96C4.665,6.495 9.855,1.605 16.74,1.605C17.985,1.605 19.155,2.265 19.155,2.775C19.1555,3.02992 19.091,3.28076 18.9677,3.50384C18.8443,3.72693 18.6662,3.91489 18.45,4.05C17.7474,4.47121 17.1656,5.06677 16.7609,5.77897C16.3561,6.49117 16.1423,7.29584 16.14,8.115C16.1404,9.31568 16.6022,10.4703 17.43,11.34C18.0231,11.9292 18.493,12.6305 18.8123,13.403C19.1317,14.1756 19.294,15.004 19.29,15.84C19.2866,16.7228 19.1057,17.5959 18.758,18.4074C18.4104,19.2189 17.9032,19.9522 17.2665,20.5637C16.6298,21.1752 15.8766,21.6524 15.0517,21.967C14.2269,22.2816 13.3472,22.4272 12.465,22.395Z");
                    webcamColorImg.SetBinding(Path.FillProperty, new System.Windows.Data.Binding
                    {
                        RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Canvas), 1),
                        Path = new PropertyPath("Tag")
                    });
                    uIElements.Add(webcamColorImg);
                    webcamColorImg = new Path();
                    webcamColorImg.Data = Geometry.Parse("M11.19,6.3C11.193,6.52015 11.1304,6.73621 11.0103,6.92072C10.8901,7.10522 10.7178,7.24982 10.5153,7.33614C10.3127,7.42246 10.0891,7.44659 9.87279,7.40547C9.65649,7.36435 9.45731,7.25983 9.30057,7.10521C9.14383,6.95059 9.03661,6.75284 8.99255,6.53713C8.94849,6.32141 8.96958,6.09746 9.05314,5.89376C9.13669,5.69006 9.27894,5.51581 9.46179,5.39316C9.64464,5.27052 9.85983,5.20502 10.08,5.205C10.3718,5.20497 10.6519,5.31986 10.8596,5.52479C11.0674,5.72972 11.1861,6.00822 11.19,6.3Z");
                    webcamColorImg.SetBinding(Path.FillProperty, new System.Windows.Data.Binding
                    {
                        RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Canvas), 1),
                        Path = new PropertyPath("Tag")
                    });
                    uIElements.Add(webcamColorImg);
                    webcamColorImg = new Path();
                    webcamColorImg.Data = Geometry.Parse("M16.065,3.93C16.0678,4.15203 16.004,4.36981 15.8817,4.55518C15.7595,4.74055 15.5845,4.885 15.3793,4.96985C15.1741,5.0547 14.9481,5.07606 14.7307,5.03117C14.5132,4.98627 14.3142,4.87718 14.1594,4.718C14.0046,4.55882 13.901,4.35687 13.8622,4.13824C13.8234,3.91961 13.851,3.69436 13.9415,3.4916C14.032,3.28884 14.1813,3.11788 14.37,3.00083C14.5587,2.88378 14.7781,2.82602 15,2.835C15.2839,2.8465 15.5527,2.96646 15.7508,3.17018C15.9489,3.37389 16.0614,3.64584 16.065,3.93Z");
                    webcamColorImg.SetBinding(Path.FillProperty, new System.Windows.Data.Binding
                    {
                        RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Canvas), 1),
                        Path = new PropertyPath("Tag")
                    });
                    uIElements.Add(webcamColorImg);
                    webcamColorImg = new Path();
                    webcamColorImg.Data = Geometry.Parse("M8.52,11.16C8.52,10.9405 8.4549,10.7259 8.33293,10.5433C8.21096,10.3608 8.0376,10.2185 7.83478,10.1345C7.63195,10.0505 7.40877,10.0285 7.19345,10.0713C6.97813,10.1142 6.78035,10.2199 6.62511,10.3751C6.46988,10.5303 6.36416,10.7281 6.32133,10.9434C6.2785,11.1588 6.30048,11.382 6.38449,11.5848C6.46851,11.7876 6.61078,11.961 6.79332,12.0829C6.97586,12.2049 7.19046,12.27 7.41,12.27C7.70439,12.27 7.98672,12.1531 8.19489,11.9449C8.40305,11.7367 8.52,11.4544 8.52,11.16Z");
                    webcamColorImg.SetBinding(Path.FillProperty, new System.Windows.Data.Binding
                    {
                        RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Canvas), 1),
                        Path = new PropertyPath("Tag")
                    });
                    uIElements.Add(webcamColorImg);
                    webcamColorImg = new Path();
                    webcamColorImg.Data = Geometry.Parse("M13.695,19.95C13.698,20.1702 13.6354,20.3862 13.5153,20.5707C13.3951,20.7552 13.2228,20.8998 13.0203,20.9861C12.8177,21.0725 12.5941,21.0966 12.3778,21.0555C12.1615,21.0143 11.9623,20.9098 11.8056,20.7552C11.6488,20.6006 11.5396,20.4028 11.4976,20.1843C11.4555,19.9658 11.4814,19.7401 11.569,19.5375C11.6567,19.335 11.8027,19.1626 11.9977,19.0419C12.1927,18.9212 12.413,18.8535 12.634,18.8546C12.8871,18.8544 13.1344,18.9711 13.2847,19.178C13.435,19.3849 13.4801,19.6705 13.4197,19.95C13.399,19.9575 13.3951,19.957 13.395,19.95Z");
                    webcamColorImg.SetBinding(Path.FillProperty, new System.Windows.Data.Binding
                    {
                        RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Canvas), 1),
                        Path = new PropertyPath("Tag")
                    });
                    uIElements.Add(webcamColorImg);
                    break;
                case VbarIcon.WebcamDetection:
                    Path webcamDetection = new Path();
                    webcamDetection.Data = Geometry.Parse("M5.21979 3.495C7.30643 2.35493 9.62471 1.70356 11.9998 1.59C14.375 1.70868 16.6949 2.34883 18.7948 3.465L19.4998 3.855L20.2648 2.46L19.5748 2.07C17.2361 0.82295 14.6477 0.115633 11.9998 0C9.37306 0.123269 6.8086 0.841317 4.49979 2.1L3.80979 2.49L4.49979 3.885L5.20479 3.495H5.21979Z");
                    webcamDetection.SetBinding(Path.FillProperty, new System.Windows.Data.Binding
                    {
                        RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Canvas), 1),
                        Path = new PropertyPath("Tag")
                    });
                    uIElements.Add(webcamDetection);
                    webcamDetection = new Path();
                    webcamDetection.Data = Geometry.Parse("M22.7848 8.535C21.5491 6.81913 19.9317 5.4138 18.06 4.42984C16.1884 3.44587 14.1138 2.91018 11.9998 2.865C9.87957 2.90361 7.79828 3.44114 5.92453 4.43405C4.05078 5.42695 2.43729 6.8473 1.21479 8.58L0.779785 9.24L2.11479 10.125L2.54979 9.45C3.643 7.95872 5.06293 6.73734 6.70088 5.87937C8.33884 5.02139 10.1514 4.54956 11.9998 4.5C13.8471 4.5551 15.6577 5.02933 17.2948 5.88686C18.9319 6.74439 20.3527 7.96279 21.4498 9.45L21.8998 10.11L23.2198 9.225L22.7848 8.565V8.535Z");
                    webcamDetection.SetBinding(Path.FillProperty, new System.Windows.Data.Binding
                    {
                        RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Canvas), 1),
                        Path = new PropertyPath("Tag")
                    });
                    uIElements.Add(webcamDetection);
                    webcamDetection = new Path();
                    webcamDetection.Data = Geometry.Parse("M11.9998 5.55C9.60666 5.54944 7.29561 6.42235 5.50043 8.00487C3.70525 9.58739 2.54938 11.7707 2.24979 14.145C2.05122 16.2161 2.45764 18.3003 3.41979 20.145L3.82479 20.835L5.18979 20.04L4.79979 19.35C4.01879 17.8076 3.68619 16.077 3.83979 14.355C4.07884 12.3611 5.04211 10.5246 6.54653 9.19438C8.05094 7.86419 9.99164 7.13308 11.9998 7.14C13.6562 7.11963 15.2793 7.60575 16.6519 8.53329C18.0244 9.46082 19.0809 10.7855 19.6798 12.33C20.1148 13.83 20.4448 15.66 19.1548 16.38C18.845 16.5671 18.4996 16.6877 18.1407 16.7342C17.7817 16.7807 17.417 16.752 17.0698 16.65C16.7056 16.5689 16.3847 16.355 16.1698 16.05C16.0735 15.7902 15.9934 15.5247 15.9298 15.255C15.8533 14.5611 15.6273 13.892 15.2674 13.2938C14.9074 12.6956 14.4221 12.1826 13.8448 11.79C13.0191 11.2832 12.0299 11.1143 11.0828 11.3182C10.1357 11.5222 9.3037 12.0833 8.75979 12.885C7.73979 14.565 8.36979 18.06 10.0648 20.085C11.5713 21.7213 13.449 22.9714 15.5398 23.73L16.2898 24L16.8298 22.5L16.0798 22.23C14.2454 21.5753 12.598 20.4838 11.2798 19.05C9.95979 17.55 9.50979 14.715 10.1248 13.71C10.4566 13.2472 10.9566 12.9329 11.5176 12.8348C12.0785 12.7366 12.6555 12.8624 13.1248 13.185C13.9348 13.62 14.2048 14.685 14.4148 15.615C14.4875 16.0132 14.6084 16.401 14.7748 16.77C15.1866 17.4588 15.847 17.9635 16.6198 18.18C17.1761 18.3392 17.7588 18.3853 18.3332 18.3158C18.9077 18.2462 19.4625 18.0624 19.9648 17.775C22.5298 16.35 21.5698 12.99 21.2398 11.88C20.5501 9.99442 19.2891 8.37108 17.6327 7.23636C15.9764 6.10165 14.0072 5.51212 11.9998 5.55Z");
                    webcamDetection.SetBinding(Path.FillProperty, new System.Windows.Data.Binding
                    {
                        RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Canvas), 1),
                        Path = new PropertyPath("Tag")
                    });
                    uIElements.Add(webcamDetection);
                    webcamDetection = new Path();
                    webcamDetection.Data = Geometry.Parse("M12.2698 9.9C13.2834 9.89986 14.2674 10.2419 15.0624 10.8708C15.8573 11.4997 16.4167 12.3785 16.6498 13.365L16.8298 14.145L18.3298 13.77L18.1498 13.005C17.8277 11.6594 17.0591 10.4627 15.9694 9.61003C14.8798 8.75737 13.5334 8.29911 12.1498 8.31C7.73979 8.31 5.09979 11.115 5.09979 15.81C5.09979 16.2 5.21978 19.785 9.17978 23.415L9.77979 23.94L10.8448 22.77L10.2598 22.23C8.28078 20.6352 7.00036 18.3326 6.68979 15.81C6.61147 15.0428 6.69935 14.2676 6.94742 13.5374C7.1955 12.8072 7.59796 12.1389 8.12742 11.5781C8.65688 11.0174 9.30092 10.5772 10.0157 10.2876C10.7305 9.99807 11.4993 9.86585 12.2698 9.9Z");
                    webcamDetection.SetBinding(Path.FillProperty, new System.Windows.Data.Binding
                    {
                        RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Canvas), 1),
                        Path = new PropertyPath("Tag")
                    });
                    uIElements.Add(webcamDetection);
                    webcamDetection = new Path();
                    webcamDetection.Data = Geometry.Parse("M19.1848 19.905C19.1848 19.905 14.4598 20.145 13.0048 16.095L12.7498 15.345L11.2498 15.885L11.5048 16.635C12.0887 18.1079 13.1114 19.3656 14.4342 20.2376C15.757 21.1096 17.316 21.5538 18.8998 21.415C20.0919 21.3065 21.2099 20.8607 22.0988 20.1685L21.7748 19.415L19.1848 19.905Z");
                    webcamDetection.SetBinding(Path.FillProperty, new System.Windows.Data.Binding
                    {
                        RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Canvas), 1),
                        Path = new PropertyPath("Tag")
                    });
                    uIElements.Add(webcamDetection);
                    break;
                case VbarIcon.WebcamCapture:
                    Path webcamCapture = new System.Windows.Shapes.Path();
                    webcamCapture.Data = Geometry.Parse("M22.395 22.395H18.405V24H24V18.405H22.395V22.395Z");
                    webcamCapture.SetBinding(Path.FillProperty, new System.Windows.Data.Binding
                    {
                        RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Canvas), 1),
                        Path = new PropertyPath("Tag")
                    });
                    uIElements.Add(webcamCapture);
                    webcamCapture = new System.Windows.Shapes.Path();
                    webcamCapture.Data = Geometry.Parse("M1.605 18.405H0V24H5.595V22.395H1.605V18.405Z");
                    webcamCapture.SetBinding(Path.FillProperty, new System.Windows.Data.Binding
                    {
                        RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Canvas), 1),
                        Path = new PropertyPath("Tag")
                    });
                    uIElements.Add(webcamCapture);
                    webcamCapture = new System.Windows.Shapes.Path();
                    webcamCapture.Data = Geometry.Parse("M1.605 1.605H5.595V0H0V5.595H1.605V1.605Z");
                    webcamCapture.SetBinding(Path.FillProperty, new System.Windows.Data.Binding
                    {
                        RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Canvas), 1),
                        Path = new PropertyPath("Tag")
                    });
                    uIElements.Add(webcamCapture);
                    webcamCapture = new System.Windows.Shapes.Path();
                    webcamCapture.Data = Geometry.Parse("M18.405 0V1.605H22.395V5.595H24V0H18.405Z");
                    webcamCapture.SetBinding(Path.FillProperty, new System.Windows.Data.Binding
                    {
                        RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Canvas), 1),
                        Path = new PropertyPath("Tag")
                    });
                    uIElements.Add(webcamCapture);
                    webcamCapture = new System.Windows.Shapes.Path();
                    webcamCapture.Data = Geometry.Parse("M18.525 12.795H21.525V11.205H18.525C18.3454 9.74781 17.6839 8.39243 16.6458 7.35425C15.6076 6.31606 14.2522 5.65461 12.795 5.475V2.475H11.205V5.475C9.74781 5.65461 8.39243 6.31606 7.35425 7.35425C6.31606 8.39243 5.65461 9.74781 5.475 11.205H2.475V12.795H5.475C5.65461 14.2522 6.31606 15.6076 7.35425 16.6458C8.39243 17.6839 9.74781 18.3454 11.205 18.525V21.525H12.795V18.525C14.2522 18.3454 15.6076 17.6839 16.6458 16.6458C17.6839 15.6076 18.3454 14.2522 18.525 12.795ZM16.92 11.205H12.795V7.08C13.8281 7.24864 14.7821 7.73758 15.5223 8.47775C16.2624 9.21792 16.7514 10.1719 16.92 11.205ZM11.205 7.08V11.205H7.08C7.24864 10.1719 7.73758 9.21792 8.47775 8.47775C9.21792 7.73758 10.1719 7.24864 11.205 7.08ZM7.08 12.795H11.205V16.92C10.1719 16.7514 9.21792 16.2624 8.47775 15.5223C7.73758 14.7821 7.24864 13.8281 7.08 12.795ZM12.795 16.92V12.795H16.92C16.7514 13.8281 16.2624 14.7821 15.5223 15.5223C14.7821 16.2624 13.8281 16.7514 12.795 16.92Z");
                    webcamCapture.SetBinding(Path.FillProperty, new System.Windows.Data.Binding
                    {
                        RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Canvas), 1),
                        Path = new PropertyPath("Tag")
                    });
                    uIElements.Add(webcamCapture);
                    break;
                case VbarIcon.WebcamMicrophone:
                    Path WebcamMicrophone = new System.Windows.Shapes.Path();
                    WebcamMicrophone.Data = Geometry.Parse("M16.5 10.005V12.63C16.5587 13.2553 16.4858 13.8859 16.286 14.4813C16.0863 15.0767 15.7641 15.6237 15.3402 16.0871C14.9164 16.5505 14.4002 16.9201 13.825 17.172C13.2497 17.424 12.6281 17.5527 12 17.55C11.372 17.5527 10.7504 17.424 10.1751 17.172C9.59986 16.9201 9.0837 16.5505 8.65984 16.0871C8.23599 15.6237 7.91383 15.0767 7.71408 14.4813C7.51433 13.8859 7.44142 13.2553 7.50004 12.63V10.005H6.00004V12.63C5.88984 14.1768 6.3828 15.7062 7.37555 16.8975C8.3683 18.0888 9.78373 18.8494 11.325 19.02V22.02H7.68004V23.52H16.32V22.02H12.765V19.02C14.3064 18.8494 15.7218 18.0888 16.7145 16.8975C17.7073 15.7062 18.2002 14.1768 18.09 12.63V10.005H16.5Z");
                    WebcamMicrophone.SetBinding(Path.FillProperty, new System.Windows.Data.Binding
                    {
                        RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Canvas), 1),
                        Path = new PropertyPath("Tag")
                    });
                    uIElements.Add(WebcamMicrophone);
                    WebcamMicrophone = new System.Windows.Shapes.Path();
                    WebcamMicrophone.Data = Geometry.Parse("M12 16.305C12.4717 16.3228 12.9416 16.2369 13.3764 16.0533C13.8112 15.8697 14.2005 15.5929 14.5166 15.2424C14.8328 14.8919 15.0681 14.4763 15.2061 14.0249C15.344 13.5735 15.3812 13.0973 15.315 12.63V3.92998C15.3583 3.4756 15.3037 3.01723 15.1549 2.58572C15.0062 2.15422 14.7666 1.7596 14.4525 1.42843C14.1384 1.09727 13.757 0.837251 13.334 0.665886C12.9109 0.49452 12.4561 0.415786 12 0.434984C11.5434 0.413274 11.0874 0.490296 10.6631 0.660799C10.2389 0.831302 9.85645 1.09128 9.54182 1.423C9.22718 1.75472 8.98777 2.15038 8.83992 2.58302C8.69207 3.01565 8.63924 3.47508 8.68504 3.92998V12.63C8.61885 13.0973 8.65604 13.5735 8.794 14.0249C8.93196 14.4763 9.16732 14.8919 9.48348 15.2424C9.79963 15.5929 10.1889 15.8697 10.6237 16.0533C11.0585 16.2369 11.5284 16.3228 12 16.305ZM10.23 3.92998C10.23 2.63998 10.845 1.97998 12 1.97998C13.5 1.97998 13.77 3.04498 13.77 3.92998V12.63C13.77 14.76 12.48 14.76 12 14.76C11.52 14.76 10.23 14.76 10.23 12.63V3.92998Z");
                    WebcamMicrophone.SetBinding(Path.FillProperty, new System.Windows.Data.Binding
                    {
                        RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Canvas), 1),
                        Path = new PropertyPath("Tag")
                    });
                    uIElements.Add(WebcamMicrophone);
                    break;
                case VbarIcon.RtkHubPortInfo:
                    Path RtkHubPortInfo = new Path()
                    {
                        Data = Geometry.Parse("M2.25 2.25H9.75V17.25H2.25V2.25ZM0 0H2.25H9.75H12V2.25V17.25V19.5H9.75H7.31892C7.37652 19.6552 7.45045 19.8172 7.54508 19.9772C7.9149 20.6022 8.69602 21.375 10.6148 21.375C12.5335 21.375 13.3147 20.6022 13.6845 19.9772C13.891 19.628 13.9989 19.2694 14.0537 18.9917C14.0807 18.855 14.0935 18.7441 14.0995 18.6728C14.1025 18.6374 14.1038 18.6123 14.1043 18.5995L14.1046 18.5922L14.1046 18.5976L14.1046 18.5955L14.1046 18.5923L14.1046 18.5897C14.1046 18.5897 14.1046 18.5905 14.1046 18.5922L14.1046 6.9H14.1189L14.1082 6.8999L14.1046 6.89971L14.1046 6.89876L14.1046 6.89658L14.1046 6.89113L14.1047 6.87595L14.1056 6.82903C14.1065 6.79064 14.1083 6.73817 14.1116 6.6736C14.1184 6.54479 14.1316 6.36601 14.1577 6.15346C14.2092 5.73422 14.3142 5.15518 14.5341 4.56006C14.7521 3.97008 15.106 3.30215 15.6924 2.77369C16.3026 2.22372 17.1112 1.875 18.1138 1.875C19.1163 1.875 19.925 2.2237 20.5354 2.77352C21.1219 3.30185 21.476 3.96965 21.6943 4.55959C21.9145 5.15466 22.0197 5.73367 22.0714 6.15291C22.0977 6.36545 22.111 6.54423 22.1178 6.67304C22.1212 6.7376 22.123 6.79007 22.124 6.82846L22.1248 6.87538L22.125 6.89057L22.125 6.89601L22.125 6.89819V6.89914C22.125 6.89958 22.125 6.9 21 6.9H22.125V14.7692H24V17.0192V19.9039V22.1539L22.3846 22.1539V22.5V23.6539V24H20.8846H20.6538H19.1538V23.6539V22.5V22.1539L17.5385 22.1539V19.9039V17.0192V14.7692H19.7885H19.875L19.875 6.90248L19.875 6.90224L19.8746 6.88394C19.8742 6.8652 19.8732 6.83388 19.8709 6.79181C19.8665 6.70733 19.8572 6.58143 19.8384 6.42834C19.7999 6.11633 19.7247 5.72034 19.5841 5.34041C19.4417 4.95535 19.2547 4.64815 19.0294 4.44523C18.828 4.2638 18.5544 4.125 18.1138 4.125C17.6732 4.125 17.3998 4.26378 17.1987 4.44506C16.9737 4.64785 16.7869 4.95492 16.6446 5.33994C16.5042 5.71982 16.4293 6.11578 16.391 6.42779C16.3722 6.58087 16.363 6.70677 16.3586 6.79124C16.3564 6.83332 16.3554 6.86464 16.3549 6.88338L16.3546 6.90167L16.3546 6.90089L16.3546 6.902L16.3546 6.90255V18.6H15.2296C14.1046 18.6 14.1046 18.5994 14.1046 18.5987L14.1046 18.6H15.2296C16.3546 18.6 16.3546 18.6007 16.3546 18.6013L16.3546 18.6028L16.3545 18.6059L16.3545 18.6134L16.3542 18.6327C16.3539 18.6474 16.3534 18.666 16.3526 18.6881C16.3508 18.7324 16.3476 18.7911 16.3415 18.8623C16.3295 19.0043 16.3063 19.1982 16.2612 19.427C16.1717 19.8806 15.9912 20.497 15.6209 21.1228C14.8371 22.4478 13.3108 23.625 10.6148 23.625C7.91876 23.625 6.39249 22.4478 5.60861 21.1228C5.25781 20.5299 5.07734 19.9455 4.98326 19.5H2.25H0V17.25V2.25V0ZM21.75 17.0192H19.7885V19.9039H21.75V17.0192Z")
                    };
                    RtkHubPortInfo.SetBinding(Path.FillProperty, new System.Windows.Data.Binding
                    {
                        RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Canvas), 1),
                        Path = new PropertyPath("Tag")
                    });
                    uIElements.Add(RtkHubPortInfo);
                    break;
                case VbarIcon.DisplayWebcam:
                    Path displayWebcam = new Path()
                    {
                        Data = Geometry.Parse("M15.0001 18C17.0811 17.2895 18.8424 15.8635 19.9704 13.9759C21.0984 12.0884 21.5199 9.8617 21.1598 7.69245C20.7997 5.5232 19.6814 3.55216 18.004 2.13028C16.3266 0.708398 14.199 -0.0720215 12.0001 -0.0720215C9.80117 -0.0720215 7.6736 0.708398 5.99623 2.13028C4.31886 3.55216 3.20056 5.5232 2.84046 7.69245C2.48035 9.8617 2.90182 12.0884 4.02982 13.9759C5.15781 15.8635 6.91911 17.2895 9.00011 18V22.335H4.32011V24H19.6801V22.395H15.0001V18ZM13.4101 22.395H10.5901V18.45C11.0574 18.5153 11.5283 18.5504 12.0001 18.555C12.4719 18.5504 12.9429 18.5153 13.4101 18.45V22.395ZM12.0001 16.965C10.4811 16.965 8.9963 16.5145 7.73333 15.6707C6.47036 14.8268 5.48599 13.6273 4.90471 12.224C4.32343 10.8206 4.17134 9.27645 4.46768 7.78668C4.76401 6.2969 5.49546 4.92846 6.56953 3.85439C7.6436 2.78032 9.01204 2.04887 10.5018 1.75254C11.9916 1.4562 13.5358 1.60829 14.9391 2.18958C16.3425 2.77086 17.5419 3.75522 18.3858 5.01819C19.2297 6.28116 19.6801 7.76601 19.6801 9.28497C19.6761 11.3206 18.8657 13.2718 17.4263 14.7112C15.9869 16.1506 14.0358 16.961 12.0001 16.965Z"),
                        StrokeThickness = 1.5,
                        Fill = new SolidColorBrush(Colors.Transparent)
                    };
                    displayWebcam.SetBinding(Path.StrokeProperty, new System.Windows.Data.Binding
                    {
                        RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Canvas), 1),
                        Path = new PropertyPath("Tag")
                    });
                    uIElements.Add(displayWebcam);
                    displayWebcam = new Path()
                    {
                        Data = Geometry.Parse("M18.0001 8.47497H12.7951V3.28497H12.0001C10.8134 3.28497 9.65338 3.63686 8.66669 4.29615C7.67999 4.95544 6.91096 5.89251 6.45683 6.98887C6.0027 8.08523 5.88388 9.29163 6.1154 10.4555C6.34691 11.6194 6.91835 12.6885 7.75747 13.5276C8.59658 14.3667 9.66568 14.9382 10.8296 15.1697C11.9935 15.4012 13.1999 15.2824 14.2962 14.8282C15.3926 14.3741 16.3296 13.6051 16.9889 12.6184C17.6482 11.6317 18.0001 10.4717 18.0001 9.28497V8.47497ZM12.0001 13.68C10.8907 13.6989 9.81514 13.2976 8.98937 12.5564C8.1636 11.8153 7.64873 10.7892 7.5481 9.68419C7.44747 8.57918 7.76854 7.47699 8.44685 6.59888C9.12516 5.72077 10.1105 5.13172 11.2051 4.94997V10.08H16.3201C16.1343 11.0903 15.6004 12.0036 14.8112 12.6613C14.022 13.319 13.0274 13.6794 12.0001 13.68Z")
                    };
                    displayWebcam.SetBinding(Path.FillProperty, new System.Windows.Data.Binding
                    {
                        RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Canvas), 1),
                        Path = new PropertyPath("Tag")
                    });
                    uIElements.Add(displayWebcam);
                    break;
            }

            return uIElements;
        }

        public static double GetScalingFactor(Window window)
        {
            // Get the PresentationSource for the window
            var source = PresentationSource.FromVisual(window);

            if (source != null && source.CompositionTarget != null)
            {
                // Get the matrix that represents the DPI scaling
                var transform = source.CompositionTarget.TransformToDevice;

                // Extract the scaling factors (X)
                return transform.M11;
            }

            // Default scaling is 1.0 (100%)
            return 1.0;
        }

        public static object ReadRegistryData(RegistryHive hive, string keyPath, string keyName)
        {
            object obj = null;
            try
            {
                WriteUILog($"[ReadRegistryData] read to ({keyPath}); KeyName: ({keyName})");
                if (DeviceManagerSA != null)
                {
                    return DeviceManagerSA.ReadRegistryData(hive, keyPath, keyName).Result;
                }
            }
            catch (Exception ex)
            {
                WriteUILog($"[ReadRegistryData] over DeviceManager exception: ({ex.ToString()})");
            }
            WriteUILog($"[ReadRegistryData] read registry from UI directly");
            try
            {
                obj = DDPMRegistryHelper.ReadRegistryKey(hive, keyPath, keyName);
            }
            catch (Exception ex)
            {
                WriteUILog($"[ReadRegistryData] over UI exception: ({ex.ToString()})");
            }
            if (obj == null)
            {
                WriteUILog($"[ReadRegistryData] keyName: {keyName} is null");
                return null;
            }
            return obj;
        }

        public static void WriteRegistryData(RegistryHive hive, string regPath, string regKey, object data)
        {
            _ = Task.Run(() =>
            {
                try
                {
                    bool result = false;
                    WriteUILog($"[WriteRegistryData] write to ({regPath}); Ket: ({regKey})");
                    if (DeviceManagerSA != null)
                    {
                        result = DeviceManagerSA.WriteRegistryData(hive, regPath, regKey, data).Result;
                        WriteUILog($"[WriteRegistryData] write to ({regPath}): ({result})");
                    }
                    else
                    {
                        WriteUILog($"[WriteRegistryData] write to ({regPath}): DeviceManagerSA is null");
                    }
                }
                catch (Exception ex)
                {
                    WriteUILog($"[WriteRegistryData] over DeviceManager exception: ({ex.ToString()})");
                }
            });
        }

        public enum GlobalSettingsType
        {
            Consent,
            BatteryLevel,
            KeyboardLockKey,
            WB7022CoverState,
            MuteState,
            DisplayCPAndEM,
            EnableQAW,
            EnableQAWReminder
        }

        public static void Set_GlobalSettings(GlobalSettingsType property, bool data)
        {
            if (DeviceManagerSA == null)
            {
                WriteUILog($"[Set_GlobalSettings] DeviceManagerSA is null");
                return;
            }
            _ = Task.Run(() =>
            {
                bool result = false;
                switch (property)
                {
                    case GlobalSettingsType.Consent:
                        result = DeviceManagerSA.Set_GlobalSetting_EnableTelemetryConsent(data).Result;
                        break;
                    case GlobalSettingsType.BatteryLevel:
                        result = DeviceManagerSA.Set_GlobalSetting_DisplayLowBatteryLevel(data).Result;
                        break;
                    case GlobalSettingsType.KeyboardLockKey:
                        result = DeviceManagerSA.Set_GlobalSetting_DisplayKeyboardLockKey(data).Result;
                        break;
                    case GlobalSettingsType.WB7022CoverState:
                        result = DeviceManagerSA.Set_GlobalSetting_DisplayWB7022CoverState(data).Result;
                        break;
                    case GlobalSettingsType.MuteState:
                        result = DeviceManagerSA.Set_GlobalSetting_DisplayMuteState(data).Result;
                        break;
                    case GlobalSettingsType.DisplayCPAndEM:
                        result = DeviceManagerSA.Set_GlobalSetting_DisplayColorPresetAndEasyMemory(data).Result;
                        break;
                    case GlobalSettingsType.EnableQAW:
                        result = DeviceManagerSA.Set_GlobalSetting_EnableQuickAccessWidget(data).Result;
                        break;
                    case GlobalSettingsType.EnableQAWReminder:
                        result = DeviceManagerSA.Set_GlobalSetting_EnableQuickAccessWidget_Reminder(data).Result;
                        break;
                    default:
                        WriteUILog($"[Set_GlobalSettings] unknow setting type in global settings from UI ({nameof(property)})");
                        return;
                }
                WriteUILog($"[Set_GlobalSettings] set ({nameof(property)}) to {data}: result({result})");
            });
        }

        //Robert_Lin 2025-3-27 for tracking the memory usage
        /// <summary>
        /// Return current process memory usage in MB. To output to string can reference below example:
        /// _log?.Info($"Memory usage: {DdpmCommonHelper.GetProcessMemoryUsageMB():F2} MB");
        /// </summary>
        /// <returns></returns>
        public static double GetProcessMemoryUsageMB()
        {
            using (Process process = Process.GetCurrentProcess())
            {
                long usageBytes = process.WorkingSet64;
                double usageMB = usageBytes / (1024.0 * 1024.0);
                return usageMB;
            }
        }
    }

    public class BindingProxy : Freezable
    {
        // Define a DependencyProperty to hold the data
        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register(
                nameof(Data), typeof(object), typeof(BindingProxy), new UIPropertyMetadata(null));

        public object Data
        {
            get => GetValue(DataProperty);
            set => SetValue(DataProperty, value);
        }

        protected override Freezable CreateInstanceCore()
        {
            return new BindingProxy();
        }
    }

}