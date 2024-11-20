using DDPM.SA.Common;
using DDPM.SA.Common.Settings;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Views;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using Dell.Client.Framework.UX.WPF.Controls;
using Dell.Client.Framework.UX.WPF.ResourceManager;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net.Http.Headers;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Forms.VisualStyles;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
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

        /// <summary>
        /// Flag to switch the Light Mode Feature
        /// </summary>
        public static bool ThemeSwitchFlag { get; set; } = false;

        /// <summary>
        /// Flag to turn on and off UI Test buttons
        /// </summary>
        public static bool UIDebugModeFlag { get; set; } = false; 

        public static bool DDPMMesssageBox(string title, string text, DependencyObject obj = null)
        {
            //MessageBoxResult result = System.Windows.MessageBox.Show(text, "Confirmation", MessageBoxButton.YesNo);
            DDPMMsgBox msgBox = new DDPMMsgBox(title, text, null);
            if (obj != null)
            {
                Window parentWindow = Window.GetWindow(obj);
                if (parentWindow != null)
                {
                    msgBox.Owner = parentWindow;
                }
            }
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
            msgBox.ShowDialog();
        }

        public static void DDPMEzMesssageBox(string title, string text, bool IsCloseButton, Window Owner, int width, int height, Thickness titlemargin, Thickness submargin)
        {
            DDPMMsgBox msgBox = new DDPMMsgBox(title, text, IsCloseButton, Owner, width, height, titlemargin, submargin);
            msgBox.ShowDialog();
        }

        public static bool IsMainWindowAtPrimaryScreen = true;

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
            if (reload_from_SA)
            {
                if (DeviceManagerSA == null)
                    return null;

                DDPMSettings data = DeviceManagerSA.ReloadAppConfigData().Result;
                if (data != null)
                {
                    Settings_Cache = data;
                }
            }
            return Settings_Cache;
        }
        //default theme is dark
        public static OSThemeEnum previousOsTheme = OSThemeEnum.Dark;
        public static void updateMergedDictionaries(ResourceManager resourceManager)
        {
            OSThemeEnum oSTheme = UXSystemParameters.Instance.OSTheme;
            if (previousOsTheme == oSTheme) return;
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
                };
                resourceManager.SwapDarkAndLightThemes();
                resourceManager.StageResources();
                resourceManager.CommitResources();
                Application.Current.MainWindow?.InvalidateVisual();
                BitmapImageUpdated?.Invoke("AddDeviceVBarRefresh");
            }, System.Windows.Threading.DispatcherPriority.Loaded);
            // Debug.WriteLine($"updateMergedDictionarie to {oSTheme.ToString()}");
            previousOsTheme = oSTheme;
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
        public static event Action<string>? BitmapImageUpdated;

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
        //2 Step 1 CreateBitmapSource() must be called at UI thread, for example, in 


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

            FileStream fs = new FileStream(pathName, FileMode.Create, FileAccess.Write, FileShare.None);
            pngEnc.Save(fs);
            fs.Close();
            return true;
        }

        #endregion

        #region ProductImage
        /// <summary>
        /// Return the filename of the DeviceInfo, not include ".png"
        /// </summary>
        /// <param name="deviceInfo"></param>
        /// <returns></returns>
        public static string DeterminePeripheralProductImageFileName(DeviceInfo deviceInfo)
        {
            string model = "";
            string colorCode = "";

            if (deviceInfo != null)
            {
                switch(deviceInfo.ModelNumber)
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
                } //switch(deviceInfo.ModelNumber)

                if (deviceInfo.ColorCode != 0)
                {
                    colorCode = $"_{deviceInfo.ColorCode}";
                }
            }
            return $"{model}{colorCode}";
        }
        #endregion

        public static Canvas CanvasIconCreator(string iconName)
        {
            List<UIElement> uIElements = iconPathCreator(iconName);
            Canvas canvas = new Canvas()
            {
                Width = 24,
                Height = 24,
                VerticalAlignment = System.Windows.VerticalAlignment.Center,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                Tag = string.Empty
            };
            canvas.SetBinding(Canvas.TagProperty, new System.Windows.Data.Binding
            {
                RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(ContentControl), 1),
                Path = new PropertyPath("Foreground")
            });
            foreach (UIElement element in uIElements) {
                canvas.Children.Add(element);
            }

            return canvas;
        }

        private static List<UIElement> iconPathCreator(string iconName)
        { 
            List<UIElement> uIElements = new List<UIElement>();
            switch (iconName) {
                case "Display.Settings":
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
                case "Display.InputSource":
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
                case "Display.EA":
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
                case "Display.Gaming":
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
                case "Display.KVM":
                    Path kvmLeftRect = new Path()
                    {
                        Data = Geometry.Parse("M1,6 L9,6 Q10,6 10,7 L10,20 Q10,21 9,21 L1,21 Q0,21 0,20 L0,7 Q0,6 1,6 Z"),
                        StrokeThickness = 2,
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
                        StrokeThickness = 2,
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
                case "Display.Others":
                    Path othersRoundCornerRect = new Path()
                    {
                        Data = Geometry.Parse("M1,5.5 H21 A0.5,0.5 0 0 1 21.5,6 V21 A0.5,0.5 0 0 1 21,21.5 H1 A0.5,0.5 0 0 1 0.5,21 V6 A0.5,0.5 0 0 1 1,5.5 Z"),
                        StrokeThickness = 3,
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
            }

            return uIElements;
        }
    }
}