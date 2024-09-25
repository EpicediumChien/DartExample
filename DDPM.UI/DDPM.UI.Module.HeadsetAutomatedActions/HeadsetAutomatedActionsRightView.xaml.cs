using DDPM.UI.Common;
using DDPM.UI.Plugin.ViewModels;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using UserControl = System.Windows.Controls.UserControl;
using DDPM.SA.Common.Settings;

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
            {
                DdpmCommonHelper.DeviceManagerSA.ITSettingsActionEvent -= DeviceManagerSA_ITSettingsActionEvent;
            }
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

    public class BooleanToForegroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? Brushes.White : Brushes.Gray;
            }
            return Brushes.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}