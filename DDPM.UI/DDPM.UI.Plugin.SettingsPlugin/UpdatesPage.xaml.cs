using DDPM.SA.Common;
using DDPM.SA.Common.Settings;
using DDPM.UI.Common;
using Dell.Client.Framework.Common;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace DDPM.UI.Plugin.SettingsPlugin
{
    /// <summary>
    /// UpdatesPage.xaml 的互動邏輯
    /// </summary>
    public partial class UpdatesPage : UserControl
    {
        private static Log _log;
        public UpdatesPage()
        {
            InitializeComponent();
            if (DdpmCommonHelper.DeviceManagerSA != null)
            {
                DdpmCommonHelper.DeviceManagerSA.FWU_UILock_Notify += _FWUpdatePlugin_UIFreezes_Notify;
                //DdpmCommonHelper.DeviceManagerSA.UIUpdateNotify += DeviceManagerSA_UIUpdateNotify;
                DdpmCommonHelper.DeviceManagerSA.ITSettingsActionEvent += DeviceManagerSA_ITSettingsActionEvent;
            }
        }

        ~UpdatesPage()
        {
            if (DdpmCommonHelper.DeviceManagerSA != null)
            {
                DdpmCommonHelper.DeviceManagerSA.FWU_UILock_Notify -= _FWUpdatePlugin_UIFreezes_Notify;
                //DdpmCommonHelper.DeviceManagerSA.UIUpdateNotify -= DeviceManagerSA_UIUpdateNotify;
                DdpmCommonHelper.DeviceManagerSA.ITSettingsActionEvent -= DeviceManagerSA_ITSettingsActionEvent;
            }
        }

        /*private void DeviceManagerSA_UIUpdateNotify(object? sender, UpdateUINotify e)
        {
            if (e == null || string.IsNullOrEmpty(e.UI_Field_Name))
            {
                Trace.WriteLine("Got [DeviceManagerSA_UIUpdateNotifyEvent] event but its argument is empty!");
                return;
            }
            //Catch event if belong to telemetry consent
            if (e.UI_Field_Name.ToUpper().Trim().Equals("INAPPUPDATE"))
            {
                DDPMSettings data = DdpmCommonHelper.DeviceManagerSA.ReloadAppConfigData().Result;
                Dispatcher.Invoke(new Action(() =>
                {
                    SettingsPageViewModel vm = (SettingsPageViewModel)this.DataContext;
                    if (vm != null)
                    {
                        //Trace.WriteLine($"Apply InAppUpdate(check) : {data.UserSettings}");
                    }
                }));
            }
        }*/

        private void DeviceManagerSA_ITSettingsActionEvent(object? sender, SA.Common.ITSettingEventArgs e)
        {
            if (e == null || e.IT_Feature_TriggerList == null || e.target_object == null)
            {
                Trace.WriteLine("Got [DeviceManagerSA_ITSettingsActionEvent] event but its argument is empty!");
                return;
            }
            int idx = e.IT_Feature_TriggerList.FindIndex(x => x.Trim().Equals("Lock_Settings_Updates"));
            if (idx >= 0)
            {
                string feature = e.IT_Feature_TriggerList[idx];
                PropertyInfo propertyInfo = e.target_object.GetType().GetProperty(feature);
                Trace.WriteLine($"Got [IT settings event] {feature} : {propertyInfo.GetValue(e.target_object)}");
                Dispatcher.Invoke(new Action(() =>
                {
                    SettingsPageViewModel vm = (SettingsPageViewModel)this.DataContext;
                    if (vm != null)
                    {
                        //vm.isTabStoppable = !(bool)propertyInfo.GetValue(e.target_object);
                        //vm.ShowLockMask = (bool)propertyInfo.GetValue(e.target_object);
                        //Trace.WriteLine($"Apply TelemetryConsent(Lock) : {propertyInfo.GetValue(e.target_object)}");

                        //do your lock UI here
                    }
                }));
            }
        }

        private void _FWUpdatePlugin_UIFreezes_Notify(object sender, bool lockStatus)
        {
            Dispatcher.Invoke(() =>
            {
                SettingsPageViewModel vm = (SettingsPageViewModel)DataContext;
                vm.RefreshUI();
            });
        }

        private void CheckUpdate_Click(object sender, RoutedEventArgs e)
        {
            SettingsPageViewModel vm = (SettingsPageViewModel)DataContext;

            vm.SetUpdateInfoUI(DdpmCommonHelper.DeviceManagerSA.GetFWUpdateInfo(false).Result, DdpmCommonHelper.DeviceManagerSA.SW_GetSWUpdateInfo(false).Result);
            vm.RefreshUI();
        }

        private void DownloadAndInstall_Click(object sender, RoutedEventArgs e)
        {
            SettingsPageViewModel vm = (SettingsPageViewModel)DataContext;
            if (vm.IsCanUpdate())
            {
                vm.SetVMUpdate();
                if (vm.FWUpdateInfoPackage.FWUpdateInfo.Count > 0 || vm.SWUpdateInfoPackage.SWUpdateInfo.Count > 0)
                {
                    UpdatesPageUI.IsEnabled = false;
                    DownloadUXBusyIndicator.IsActive = true;
                    DownloadUXBusyIndicator.Visibility = Visibility.Visible;
                    DownloadUXTextBlock.Visibility = Visibility.Visible;

                    Thread t1 = new Thread(() => CallFWU(vm));
                    t1.Start();
                }
            }
        }

        private void CallFWU(SettingsPageViewModel vm)
        {
            List<FWUpdateInfo> fwUpdateInfos = DdpmCommonHelper.DeviceManagerSA.DownloadAndInstall(vm.FWUpdateInfoPackage.FWUpdateInfo).Result;
            bool b = false;
            foreach (FWUpdateInfo fwUpdateInfo in fwUpdateInfos)
            {
                if (fwUpdateInfo.FWUErrorCode == FWUErrorCode.NoError)
                {
                    b = true;
                }
            }
            if (b || vm.SWUpdateInfoPackage.SWUpdateInfo.Count > 0)
            {
                if (vm.SWUpdateInfoPackage.SWUpdateInfo.Count > 0)
                {
                    List<SWUpdateInfo> swUpdateInfos = DdpmCommonHelper.DeviceManagerSA.SW_DownloadAndInstall(vm.SWUpdateInfoPackage.SWUpdateInfo).Result;
                }
                string path = "DDPM.exe";
                string processName = "DDPM";
                Process[] processes = Process.GetProcessesByName(processName);
                if (processes.Length > 0)
                {
                    foreach (Process process in processes)
                    {
                        // Close process by sending a close message to its main window.
                        process.CloseMainWindow();
                        // Free resources associated with process.
                        process.Close();
                    }
                }
                if (vm.SWUpdateInfoPackage.SWUpdateInfo.Count <= 0)
                {
                    Process.Start(path);
                }
            }
            Dispatcher.BeginInvoke(new Action(() =>
            {
                UpdatesPageUI.IsEnabled = true;
                DownloadUXBusyIndicator.IsActive = false;
                DownloadUXBusyIndicator.Visibility = Visibility.Collapsed;
                DownloadUXTextBlock.Visibility = Visibility.Collapsed;
            }));
        }
    }
}