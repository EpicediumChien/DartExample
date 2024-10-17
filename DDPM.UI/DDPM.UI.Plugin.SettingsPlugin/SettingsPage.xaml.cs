using DDPM.SA.Common;
using DDPM.SA.Common.Settings;
using DDPM.UI.Common;
using DDPM.UI.Interfaces;
using DDPM.UI.Plugin.ViewModels;
using Dell.Client.Framework.UX.WPF;
using System.Diagnostics;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Cryptography;
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
            SettingsPageViewModel _vm = (SettingsPageViewModel?)SettingsPlugin.PluginIoc?.GetService<ISettingsPageViewModel>();

            if (_vm != null)
            {
                DataContext = _vm;
                if (DdpmCommonHelper.DeviceManagerSA != null)
                {
                    vm.Invoke_RefreshData();
                    DdpmCommonHelper.DeviceManagerSA.ITSettingsActionEvent += DeviceManagerSA_ITSettingsActionEvent;
                    DdpmCommonHelper.DeviceManagerSA.GlobalSettingChangeEvent += GlobalSettingChangeEvent;
                }
            }
        }

        ~SettingsPage()
        {
            if (DdpmCommonHelper.DeviceManagerSA != null)
            {
                DdpmCommonHelper.DeviceManagerSA.ITSettingsActionEvent -= DeviceManagerSA_ITSettingsActionEvent;
                DdpmCommonHelper.DeviceManagerSA.GlobalSettingChangeEvent -= GlobalSettingChangeEvent;
            }
        }

        private void DeviceManagerSA_ITSettingsActionEvent(object? sender, SA.Common.ITSettingEventArgs e)
        {
            bool? isLocked = DdpmCommonHelper.GetUINotifyPropertyValue_Boolean("Lock_Settings_TelemetryConsent", e);
            if (isLocked != null)
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    SettingsPageViewModel vm = (SettingsPageViewModel)this.DataContext;
                    if (vm != null)
                    {
                        vm.Lock_AnalyticsPage = (bool)isLocked;
                        Trace.WriteLine($"[SettingsPage] Apply TelemetryConsent(Lock) : {isLocked}");
                    }
                }));
            }

            isLocked = DdpmCommonHelper.GetUINotifyPropertyValue_Boolean("Lock_Settings_Updates", e);
            if (isLocked != null)
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    SettingsPageViewModel vm = (SettingsPageViewModel)this.DataContext;
                    if (vm != null)
                    {
                        vm.Lock_UpdatesPage = (bool)isLocked;
                        Trace.WriteLine($"[SettingsPage] Apply FW/SW Updates(Lock) : {isLocked}");
                    }
                }));
            }
            isLocked = DdpmCommonHelper.GetUINotifyPropertyValue_Boolean("Lock_Setting_ScreenNotification", e);
            if (isLocked != null)
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    SettingsPageViewModel vm = (SettingsPageViewModel)this.DataContext;
                    if (vm != null)
                    {
                        vm.Lock_GeneralPage = (bool)isLocked;
                        Trace.WriteLine($"[SettingsPage] Apply General(check) : {isLocked}");
                    }
                }));
            }
        }
        private void GlobalSettingChangeEvent(object? sender, EventArgs e)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                SettingsPageViewModel vm = (SettingsPageViewModel)this.DataContext;
                if (vm != null)
                {
                    vm.GlobalSettingParam = DdpmCommonHelper.DeviceManagerSA.GetGlobalSettingParam().Result;
                    vm.RefreshUI();
                }
            }));
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
        }
        private void UpdatesButton_Click(object sender, MouseButtonEventArgs e)
        {
            vm.SetSelected(1);
        }
        private void AnalyticsButton_Click(object sender, MouseButtonEventArgs e)
        {
            //Dean 0618 add analytics page
            vm.SetSelected(2);
            
        }

        private void WidgetSettingsButton_Click(object sender, MouseButtonEventArgs e)
        {
            vm.SetSelected(3);
            
        }

        private void AboutButton_Click(object sender, MouseButtonEventArgs e)
        {
            vm.SetSelected(4);
            
        }
    }
}