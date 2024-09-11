using DDPM.SA.Common;
using DDPM.SA.Common.Settings;
using DDPM.UI.Common;
using Dell.Client.Framework.UX.WPF;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DDPM.UI.Plugin.SettingsPlugin
{
    /// <summary>
    /// SettingsPage.xaml 的互動邏輯
    /// </summary>
    public partial class SettingsPage : UserControl
    {
        private SettingsPageViewModel vm
        {
            get { return (SettingsPageViewModel)DataContext; }
        }
        public SettingsPage()
        {
            InitializeComponent();
            DataContext = new SettingsPageViewModel();
            if (DdpmCommonHelper.DeviceManagerSA != null)
            {
                vm.GlobalSettingParam = DdpmCommonHelper.DeviceManagerSA.GetGlobalSettingParam().Result;
                vm.SetUpdateInfoUI(DdpmCommonHelper.DeviceManagerSA.GetFWUpdateInfo(false).Result, DdpmCommonHelper.DeviceManagerSA.SW_GetSWUpdateInfo(false).Result);
                vm.RefreshUI();

                DdpmCommonHelper.DeviceManagerSA.ITSettingsActionEvent += DeviceManagerSA_ITSettingsActionEvent;
                DDPMSettings data = DdpmCommonHelper.DeviceManagerSA.ReloadAppConfigData().Result;
                Dispatcher.Invoke(new Action(() =>
                {
                    SettingsPageViewModel vm = (SettingsPageViewModel)this.DataContext;
                    if (vm != null)
                    {
                        vm.LockMaskVisible = data.LockSettings.Lock_Settings_TelemetryConsent ? Visibility.Visible : Visibility.Collapsed;
                        Trace.WriteLine($"[SettingsPage] Apply TelemetryConsent(check) : {data.LockSettings.Lock_Settings_TelemetryConsent}");
                        vm.LockMaskVisible_Updates = data.LockSettings.Lock_Settings_Updates ? Visibility.Visible : Visibility.Collapsed;
                        Trace.WriteLine($"[SettingsPage] Apply FW/SW Updates(check) : {data.LockSettings.Lock_Settings_Updates}");
                    }
                }));


                GeneralButton_Click(this, null);
            }
        }

        ~SettingsPage()
        {
            if (DdpmCommonHelper.DeviceManagerSA != null)
                DdpmCommonHelper.DeviceManagerSA.ITSettingsActionEvent -= DeviceManagerSA_ITSettingsActionEvent;
        }

        private void DeviceManagerSA_ITSettingsActionEvent(object? sender, SA.Common.ITSettingEventArgs e)
        {
            if (e == null || e.IT_Feature_TriggerList == null || e.target_object == null)
            {
                Trace.WriteLine("Got [SettingsPage][DeviceManagerSA_ITSettingsActionEvent] event but its argument is empty!");
                return;
            }
            int idx = e.IT_Feature_TriggerList.FindIndex(x => x.Trim().Equals("Lock_Settings_TelemetryConsent"));
            if (idx >= 0)
            {
                string feature = e.IT_Feature_TriggerList[idx];
                PropertyInfo propertyInfo = e.target_object.GetType().GetProperty(feature);
                Trace.WriteLine($"Got [SettingsPage][IT settings event] {feature} : {propertyInfo.GetValue(e.target_object)}");
                Dispatcher.Invoke(new Action(() =>
                {
                    SettingsPageViewModel vm = (SettingsPageViewModel)this.DataContext;
                    if (vm != null)
                    {
                        vm.LockMaskVisible = (bool)propertyInfo.GetValue(e.target_object) ? Visibility.Visible : Visibility.Collapsed;
                        Trace.WriteLine($"[SettingsPage] Apply TelemetryConsent(Lock) : {propertyInfo.GetValue(e.target_object)}");
                    }
                }));
            }
            idx = e.IT_Feature_TriggerList.FindIndex(x => x.Trim().Equals("Lock_Settings_Updates"));
            if (idx >= 0)
            {
                string feature = e.IT_Feature_TriggerList[idx];
                PropertyInfo propertyInfo = e.target_object.GetType().GetProperty(feature);
                Trace.WriteLine($"Got [SettingsPage][IT settings event] {feature} : {propertyInfo.GetValue(e.target_object)}");
                Dispatcher.Invoke(new Action(() =>
                {
                    SettingsPageViewModel vm = (SettingsPageViewModel)this.DataContext;
                    if (vm != null)
                    {
                        vm.LockMaskVisible_Updates = (bool)propertyInfo.GetValue(e.target_object) ? Visibility.Visible : Visibility.Collapsed;
                        Trace.WriteLine($"[SettingsPage] Apply FW/SW Updates(Lock) : {propertyInfo.GetValue(e.target_object)}");
                    }
                }));
            }
        }

        private void leftArrow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            IConsole? console = SettingsPlugin.PluginIoc.GetService<IConsole>();
            console?.ShowPluginById(DDPM.UI.Common.Constants.DdpmHomePluginId);
        }
        //0614 將按鈕改成UXTextBlock，事件也變更，讓風格更像figma，不影響功能作動
        private void GeneralButton_Click(object sender, MouseButtonEventArgs e)
        {
            vm.SetSelected(0);
            Settings_General settings_General = new Settings_General();
            vm.OpenFullView(settings_General);
        }
        private void UpdatesButton_Click(object sender, MouseButtonEventArgs e)
        {
            vm.SetSelected(1);
            UpdatesPage updatesPage = new UpdatesPage();
            vm.OpenFullView(updatesPage);
        }
        private void AnalyticsButton_Click(object sender, MouseButtonEventArgs e)
        {
            //Dean 0618 add analytics page
            vm.SetSelected(2);
            vm.FullView = new AnalyticsPage();
        }

        private void WidgetSettingsButton_Click(object sender, MouseButtonEventArgs e)
        {
            vm.SetSelected(3);
            Settings_WidgetSettings settings_WidgetSettings = new Settings_WidgetSettings();
            vm.OpenFullView(settings_WidgetSettings);
        }

        private void AboutButton_Click(object sender, MouseButtonEventArgs e)
        {
            vm.SetSelected(4);
            Settings_About settings_About = new Settings_About();
            vm.OpenFullView(settings_About);
        }
    }
}