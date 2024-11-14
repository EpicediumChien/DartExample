using DDPM.SA.Common;
using DDPM.SA.Common.Alert;
using DDPM.SA.Common.Settings;
using DDPM.UI.Common;
using System.Diagnostics;
using System.IO;
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
        // 10/15 Derek for RWD
        private readonly Int16 breakPoints = 910;

        public UpdatesPage()
        {
            InitializeComponent();

            if (System.Windows.Application.Current?.TryFindResource("breakPoint") is Int16 width)
                breakPoints = width;
        }

        ~UpdatesPage()
        {

        }
        private void CheckUpdate_Click(object sender, RoutedEventArgs e)
        {
            SettingsPageViewModel vm = (SettingsPageViewModel)DataContext;
            vm.CheckUpdate();
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
            List<FWUpdateInfo> fwUpdateInfos = DdpmCommonHelper.DeviceManagerSA.DownloadAndInstall(vm.FWUpdateInfoPackage.FWUpdateInfo, true).Result;
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
                    List<SWUpdateInfo> swUpdateInfos = DdpmCommonHelper.DeviceManagerSA.SW_DownloadAndInstall(vm.SWUpdateInfoPackage.SWUpdateInfo, true).Result;
                }
                if (vm.SWUpdateInfoPackage.SWUpdateInfo.Count <= 0)
                {
                    string exePath = Assembly.GetExecutingAssembly().Location;
                    string folderPath = Path.GetDirectoryName(exePath);
                    Thread t1 = new Thread(() => DdpmCommonHelper.DeviceManagerSA.CallDDPMUI(folderPath));
                    t1.Start();
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

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (this.ActualWidth <= breakPoints - 212 - 50)
                ChangeToVerticalLayout();
            else
                ChangeToHorizontalLayout();

            if (spSAAlert.ActualWidth > 50) //Derek 1107 in debug mode ，ActualWidth maybe 0
                alertBase.Width = spSAAlert.ActualWidth - 50;
        }

        private void ChangeToVerticalLayout()
        {
            spFWUpdate.Orientation = Orientation.Vertical;
        }

        private void ChangeToHorizontalLayout()
        {
            spFWUpdate.Orientation = Orientation.Horizontal;
        }
    }
}