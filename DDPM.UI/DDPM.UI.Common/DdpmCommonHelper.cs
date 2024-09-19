using DDPM.SA.Common;
using DDPM.SA.Common.Settings;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Views;
using Dell.Client.Framework.UX.WPF;
using Dell.Client.Framework.UX.WPF.Controls;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using static DDPM.UI.Common.Views.DDPMMsgBox;

namespace DDPM.UI.Common
{
    public static class DdpmCommonHelper
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

        //DdpmHomePlugin will set this value
        public static IConsole? MyConsole { get; set; }

        public static IDeviceManagerSA? DeviceManagerSA { get; set; }
        public static DDPMSettings? Settings_Cache { get; set; }

        public static IModuleOwner? ModuleOwner { get; set; }

        public static bool DDPMMesssageBox(string title, string text)
        {
            //MessageBoxResult result = System.Windows.MessageBox.Show(text, "Confirmation", MessageBoxButton.YesNo);
            DDPMMsgBox msgBox = new DDPMMsgBox(title, text, null);
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
                    DDPMSettings data = DeviceManagerSA.ReloadAppConfigData().Result;
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
                DDPMSettings data = DeviceManagerSA.ReloadAppConfigData().Result;
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
    }
}