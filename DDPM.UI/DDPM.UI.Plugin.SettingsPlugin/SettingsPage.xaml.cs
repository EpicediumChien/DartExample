using DDPM.SA.Common;
using DDPM.SA.Common.Settings;
using DDPM.UI.Common;
using DDPM.UI.Interfaces;
using DDPM.UI.Plugin.ViewModels;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using Dell.Client.Framework.UX.WPF.Controls;
using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using VcpCore.Common;
using static DDPM.UI.Plugin.SettingsPlugin.GlobalSettingsParam;

namespace DDPM.UI.Plugin.SettingsPlugin
{
    /// <summary>
    /// SettingsPage.xaml 的互動邏輯
    /// </summary>
    public partial class SettingsPage : UserControl
    {
        private ILog? _log;
        private DDPMSettings data = null;
        private SettingsPageViewModel vm
        {
            get { return (SettingsPageViewModel)DataContext; }
        }
        public SettingsPage()
        {
            _log = SettingsPlugin.PluginIoc?.GetService<ILog>();
            _log?.Info("SettingsPage initialize start");
            InitializeComponent();
            SettingsPageViewModel _vm = (SettingsPageViewModel?)SettingsPlugin.PluginIoc?.GetService<ISettingsPageViewModel>();
            data = DdpmCommonHelper.ReadDDPMSettings();
            if (_vm != null)
            {
                DataContext = _vm;
                if (DdpmCommonHelper.DeviceManagerSA != null)
                {
                    vm.Invoke_RefreshData();
                    vm.Invoke_RefreshData_1();
                    DdpmCommonHelper.DeviceManagerSA.ITSettingsActionEvent += DeviceManagerSA_ITSettingsActionEvent;
                    DdpmCommonHelper.DeviceManagerSA.GlobalSettingChangeEvent += GlobalSettingChangeEvent;
                    DdpmCommonHelper.DeviceManagerSA.Peripherals_UpdateNotify += Peripherals_UpdateEvent;
                    //Derek 1210
                    DdpmCommonHelper.DeviceManagerSA.UIUpdateNotify += DeviceManagerSA_UIUpdateNotify;
                }
            }
            _log?.Info("SettingsPage initialize done");
        }

        //Derek 1210 add to support if user has in setting page
        private void DeviceManagerSA_UIUpdateNotify(object? sender, UpdateUINotify e)
        {
            if (e == null || e.UI_Field_Name == null || e == UpdateUINotify.Empty)
                return;

            _log?.Info($"DeviceManagerSA_UIUpdateNotify received msg is {e.UI_Field_Name}");

            if (e.UI_Field_Name.StartsWith("QAMEvent_NavigateToWidgetSettingPage"))
            {
                Dispatcher.Invoke(() =>
                {
                    WidgetSettingsButton_Click(this, null);
                });
            }
        }

        ~SettingsPage()
        {
            if (DdpmCommonHelper.DeviceManagerSA != null)
            {
                DdpmCommonHelper.DeviceManagerSA.ITSettingsActionEvent -= DeviceManagerSA_ITSettingsActionEvent;
                DdpmCommonHelper.DeviceManagerSA.GlobalSettingChangeEvent -= GlobalSettingChangeEvent;
                DdpmCommonHelper.DeviceManagerSA.Peripherals_UpdateNotify -= Peripherals_UpdateEvent;
                DdpmCommonHelper.DeviceManagerSA.UIUpdateNotify -= DeviceManagerSA_UIUpdateNotify;
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
                        _log?.Info($"[SettingsPage] Apply TelemetryConsent(Lock) : {isLocked}");
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
                        data.LockSettings.Lock_Settings_Updates = vm.Lock_UpdatesPage;
                        _log?.Info($"[SettingsPage] Apply FW/SW Updates(Lock) : {isLocked}");
                        vm.RefreshUI();
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
                        data.LockSettings.Lock_Setting_ScreenNotification = vm.Lock_GeneralPage;
                        _log?.Info($"[SettingsPage] Apply General(check) : {isLocked}");
                        vm.RefreshUI();
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
                    //vm.GlobalSettingParam = DdpmCommonHelper.DeviceManagerSA.GetGlobalSettingParam().Result;
                    Global.SettingParam = DdpmCommonHelper.DeviceManagerSA.GetGlobalSettingParam().Result;
                    vm.GlobalSettingParam = Global.SettingParam;
                    vm.RefreshUI();
                }
            }));
        }
        private void Peripherals_UpdateEvent(object? sender, bool e)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                SettingsPageViewModel vm = (SettingsPageViewModel)this.DataContext;
                if (vm != null)
                {
                    vm.CheckUpdate();
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

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //1210 marked by Derek due to spend more time to communicate with SA
            //2024.12.10 08:47:58.546[7364](00001) I------DDPMHOME: [Caller: UserControl_Loaded][SourceLine:163] start to GetIsWidgetSettingPageLoadedByQAMAsync
            //2024.12.10 08:48:17.921[7364](00001) I------DDPMHOME: [Caller: UserControl_Loaded][SourceLine:174] GetIsWidgetSettingPageLoadedByQAMAsync = false
            //try
            //{
            //    DdpmCommonHelper.WriteUILog($"start to GetIsWidgetSettingPageLoadedByQAMAsync");

            //    if (DdpmCommonHelper.DeviceManagerSA!.GetIsWidgetSettingPageLoadedByQAMAsync().Result == true)
            //    {
            //        DdpmCommonHelper.WriteUILog($"GetIsWidgetSettingPageLoadedByQAMAsync = true");

            //        DdpmCommonHelper.DeviceManagerSA!.SetIsWidgetSettingPageLoadedByQAMAsync(false);

            //        WidgetSettingsButton_Click(this, null);
            //    }
            //    else
            //        DdpmCommonHelper.WriteUILog($"GetIsWidgetSettingPageLoadedByQAMAsync = false");
            //}
            //catch (Exception)
            //{
            //    DdpmCommonHelper.WriteUILog($"Catch exception when navigate to Widget Setting");
            //}

            SwitchToWidgetSettingPage();
        }

        private void SwitchToWidgetSettingPage()
        {
            //Derek 1210 use new solution -- workable

            if (DdpmCommonHelper.IsDDPMSwitchToSettingPageByQAM)
            {
                DdpmCommonHelper.WriteUILog($"DdpmCommonHelper.isDDPMSwitchToSettingPageByQAM == true");

                WidgetSettingsButton_Click(this, null);
                DdpmCommonHelper.IsDDPMSwitchToSettingPageByQAM = false;
            }
            else
                DdpmCommonHelper.WriteUILog($"DdpmCommonHelper.isDDPMSwitchToSettingPageByQAM == false");
        }
    }
}