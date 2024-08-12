using DDPM.SA.Common;
using DDPM.SA.Common.Popup;
using DDPM.UI.Common;
using Dell.Client.Framework.UX.WPF.Dialogs.WPF;
using DPeMPublic.Common.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using static System.Net.Mime.MediaTypeNames;
using PopupBase = DDPM.UI.Common.UserControls.PopupBase;

namespace DDPM.UI.Plugin.SettingsPlugin
{
    /// <summary>
    /// UpdatesPage.xaml 的互動邏輯
    /// </summary>
    public partial class UpdatesPage : UserControl
    {
        public UpdatesPage()
        {
            InitializeComponent();
            DdpmCommonHelper.DeviceManagerSA.FWU_UILock_Notify += _FWUpdatePlugin_UIFreezes_Notify;
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
        void CallFWU(SettingsPageViewModel vm)
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
