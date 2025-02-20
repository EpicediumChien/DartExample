using DDPM.SA.Common.Settings;
using DDPM.UI.Common;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DDPM.UI.Module.DisplayProperties
{
    /// <summary>
    /// Interaction logic for DisplayPropertiesRightView.xaml
    /// </summary>
    public partial class DisplayPropertiesRightView : UserControl
    {
        //0604 Bruce 將change select item和swich click事件拿掉，改用ViemModel的set去做設定功能
        public DisplayPropertiesRightView()
        {
            InitializeComponent();
            //DisplayPropertiesViewModel vm = (DisplayPropertiesViewModel)DataContext;
            
            if (DdpmCommonHelper.DeviceManagerSA != null)
            {
                DdpmCommonHelper.DeviceManagerSA.ITSettingsActionEvent += DeviceManagerSA_ITSettingsActionEvent;
            }
            // Set the input method to English for the entire UserControl
            InputMethod.SetPreferredImeState(this, InputMethodState.Off);

        }
        ~DisplayPropertiesRightView()
        {
            if (DdpmCommonHelper.DeviceManagerSA != null)
            {
                DdpmCommonHelper.DeviceManagerSA.ITSettingsActionEvent -= DeviceManagerSA_ITSettingsActionEvent;
            }
        }
        private void DeviceManagerSA_ITSettingsActionEvent(object? sender, SA.Common.ITSettingEventArgs e)
        {
            bool? isLocked = DdpmCommonHelper.GetUINotifyPropertyValue_Boolean("Lock_Display_ResolutionRefreshRate", e);
            if (isLocked != null)
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    DisplayPropertiesViewModel vm = (DisplayPropertiesViewModel)this.DataContext;
                    if (vm != null)
                    {
                        vm.Lock_RefreshRate = (bool)isLocked;
                        Trace.WriteLine($"[SettingsPage] DisplayProperty RefreshRate(Lock) : {isLocked}");
                    }
                }));
            }
            isLocked = DdpmCommonHelper.GetUINotifyPropertyValue_Boolean("Lock_Display_USBCPrioritization", e);
            if (isLocked != null)
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    DisplayPropertiesViewModel vm = (DisplayPropertiesViewModel)this.DataContext;
                    if (vm != null)
                    {
                        vm.Lock_USBCrioritization = (bool)isLocked;
                        Trace.WriteLine($"[SettingsPage] USBC Prioritization(Lock) : {isLocked}");
                    }
                }));
            }
        }

        private void CallWindowsSettings_Click(object sender, RoutedEventArgs e)
        {
            DdpmCommonHelper.DeviceManagerSA.CallWindowsDisplaySetting();
            //Refresh();
        }

        private void RefreshUI()
        {
            DisplayPropertiesViewModel vm = (DisplayPropertiesViewModel)DataContext;
            vm.RefreshUI();
        }
    }
}