using DDPM.UI.Common;
using DDPM.UI.Plugin.ViewModels;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using UserControl = System.Windows.Controls.UserControl;
using DDPM.SA.Common.Settings;
using Dell.Client.Framework.UX.WPF.Controls;
using System.ComponentModel;

namespace DDPM.UI.Module.HeadsetAutomatedActions
{
    /// <summary>
    /// Interaction logic for HeadsetAudioSettingsRightView.xaml
    /// </summary>
    public partial class HeadsetAutomatedActionsRightView : UserControl
    {
        private readonly HeadsetViewModel _vm;

        public HeadsetAutomatedActionsRightView(HeadsetViewModel vm)
        {
            InitializeComponent();
            _vm = vm;

            //lock/unlock init, 9/23 add
            if (DdpmCommonHelper.DeviceManagerSA != null)
            {
                DdpmCommonHelper.DeviceManagerSA.ITSettingsActionEvent += DeviceManagerSA_ITSettingsActionEvent;

                DDPMSettings data = DdpmCommonHelper.DeviceManagerSA.ReloadAppConfigData().Result;
                if (data != null)
                {
                    if (data.LockSettings.Lock_Audio_wearDetection)
                    {
                        _vm.isWearLocked = System.Windows.Visibility.Visible;
                        _vm.isWearTabStopped = false;
                    }
                    else
                    {
                        _vm.isWearLocked = System.Windows.Visibility.Collapsed;
                        _vm.isWearTabStopped = true;
                    }
                }
            }
        }

        ~HeadsetAutomatedActionsRightView()
        {
            if (DdpmCommonHelper.DeviceManagerSA != null)
                DdpmCommonHelper.DeviceManagerSA.ITSettingsActionEvent -= DeviceManagerSA_ITSettingsActionEvent;
        }

        private void DeviceManagerSA_ITSettingsActionEvent(object? sender, SA.Common.ITSettingEventArgs e)
        {
            bool? rst = DdpmCommonHelper.GetUINotifyPropertyValue_Boolean("Lock_Audio_wearDetection", e);
            Dispatcher.Invoke(new Action(() =>
            {
                if (_vm != null)
                {
                    bool locked = false;
                    if (rst != null && rst == true)
                        locked = true;

                    _vm.isWearLocked = locked ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
                    _vm.isWearTabStopped = !locked;
                }
            }));
        }
    }

    public class BooleanToForegroundConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is bool isChecked && values[1] is bool isDarkTheme)
            {
                if (isDarkTheme)
                    return isChecked ? Brushes.White : Brushes.Gray;
                else
                    return isChecked ? Brushes.Black : Brushes.Gray;
            }
            return Brushes.Gray;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BooleanToForegroundConverter2 : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length == 4)
            {
                if (values[0] is bool isDarkTheme && values[1] is bool isChecked && values[2] is bool IsChecked2 && values[3] is bool quickPauseStatus)
                {

                    if (!IsChecked2 || !quickPauseStatus)
                        return Brushes.Gray;
                    //if (!IsChecked2)
                    //    return Brushes.Gray;
                    if (isDarkTheme)
                        return isChecked ? Brushes.White : Brushes.White;
                    else
                        return isChecked ? Brushes.White : Brushes.Black;
                }
            }
            else if (values.Length == 3)
            {
                if (values[0] is bool isDarkTheme && values[1] is bool isChecked && values[2] is bool IsChecked2)
                {
                    if (!IsChecked2)
                        return Brushes.Gray;
                    if (isDarkTheme)
                        return isChecked ? Brushes.White : Brushes.White;
                    else
                        return isChecked ? Brushes.White : Brushes.Black;
                }
            }
            else { return Brushes.Gray; }
            return Brushes.Gray;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BooleanToForegroundConverter3 : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length == 3 && 
                values[0] is bool isChecked && 
                values[1] is bool isDarkTheme && 
                values[2] is bool quickPauseStatus)
            {
                if (!quickPauseStatus)
                    return Brushes.Gray;
                if (isDarkTheme)
                    return isChecked ? Brushes.White : Brushes.Gray;
                else
                    return isChecked ? Brushes.Black : Brushes.Gray;
            }
            return Brushes.Gray;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BooleanToForegroundConverter4 : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2 || !(values[0] is bool isSupported) || !(values[1] is bool isDarkTheme))
                return Brushes.Gray;

            if (!isSupported)
                return Brushes.Gray;

            return isDarkTheme ? Brushes.White : Brushes.Black;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class BooleanToOpacityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (values.Length == 1
                    && (bool)values[0])
                    return 1.0;
                if (values.Length > 1
                    && values[0] is bool isChecked
                    && values[1] is bool quickPauseStatus)
                {
                    if (isChecked && quickPauseStatus)
                        return 1.0;
                }
                return 0.6;
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog($"[BooleanToOpacityConverter] Exception thrown: {ex.Message}. Stack trace: {ex.StackTrace}");
                return 0.6;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}