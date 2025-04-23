using DDPM.SA.Common;
using DDPM.SA.Common.Alert;
using DDPM.SA.Common.Settings;
using DDPM.UI.Common;
using Dell.Client.Framework.Common;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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
        private ILog? _log;

        public UpdatesPage()
        {
            _log = SettingsPlugin.PluginIoc?.GetService<ILog>();
            _log?.Info("UpdatesPage initialize go");
            InitializeComponent();
            if (System.Windows.Application.Current?.TryFindResource("breakPoint") is Int16 width)
                breakPoints = width;
            SettingsPageViewModel vm = (SettingsPageViewModel?)SettingsPlugin.PluginIoc?.GetService<ISettingsPageViewModel>();
            if (vm != null)
            {
                DataContext = vm;
                if (DdpmCommonHelper.DeviceManagerSA != null)
                {
                    DdpmCommonHelper.DeviceManagerSA.DeviceChanged += vm.DeviceChanged;
                }
            }
            _log?.Info("UpdatesPage initialize done");
        }

        ~UpdatesPage()
        {
            SettingsPageViewModel vm = (SettingsPageViewModel)DataContext;
            if (DdpmCommonHelper.DeviceManagerSA != null && vm != null)
            {
                DdpmCommonHelper.DeviceManagerSA.DeviceChanged -= vm.DeviceChanged;
            }
        }
        private void CheckUpdate_Click(object sender, RoutedEventArgs e)
        {
            SettingsPageViewModel vm = (SettingsPageViewModel)DataContext;
            if (vm != null)
            {
                vm.CheckUpdate();
            }
            else
            {
                _log?.Info("CheckUpdate_Click vm is null");
            }
            _log?.Info("CheckUpdate_Click finish");
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
                    //DownloadUXTextBlock.Visibility = Visibility.Visible;

                    Thread t1 = new Thread(() => CallFWU(vm));
                    t1.Start();
                }
            }
        }
        private void CallFWU(SettingsPageViewModel vm)
        {
            _log?.Info("CallFWU start");
            if (vm != null)
            {
                if (vm.FWUpdateInfoPackage.FWUpdateInfo.Count > 0)
                {
                    _log?.Info("CallFWU DownloadAndInstall go");
                    List<FWUpdateInfo> fwUpdateInfos = DdpmCommonHelper.DeviceManagerSA.DownloadAndInstall(vm.FWUpdateInfoPackage.FWUpdateInfo, true, true, "").Result;
                    _log?.Info("CallFWU DownloadAndInstall finish");

                }
                if (vm.SWUpdateInfoPackage.SWUpdateInfo.Count > 0)
                {
                    _log?.Info("CallFWU SW_DownloadAndInstall go");
                    List<SWUpdateInfo> swUpdateInfos = DdpmCommonHelper.DeviceManagerSA.SW_DownloadAndInstall(vm.SWUpdateInfoPackage.SWUpdateInfo, true, "").Result;
                    _log?.Info("CallFWU SW_DownloadAndInstall finish");
                }
                else
                {
                    _log?.Info("CallFWU restart DDPM go");
                    string? exePath = Assembly.GetExecutingAssembly().Location;
                    if (!string.IsNullOrEmpty(exePath))
                    {
                        string? folderPath = Path.GetDirectoryName(exePath);
                        if (!string.IsNullOrEmpty(folderPath))
                        {
                            if (DdpmCommonHelper.DeviceManagerSA != null)
                            {
                                Thread t1 = new Thread(() => DdpmCommonHelper.DeviceManagerSA.CallDDPMUI(folderPath));
                                t1.Start();
                                _log?.Info("CallFWU restart DDPM finish");
                            }
                            else
                            {
                                _log?.Info("CallFWU restart DDPM DeviceManagerSA is null");
                            }
                        }
                        else
                        {
                            _log?.Info("CallFWU restart DDPM folderPath is null");
                        }
                    }
                    else
                    {
                        _log?.Info("CallFWU restart DDPM exePath is null");
                    }
                }
            }
            else
            {
                _log?.Info("CallFWU vm is null");
            }
            Dispatcher.BeginInvoke(new Action(() =>
            {
                UpdatesPageUI.IsEnabled = true;
                DownloadUXBusyIndicator.IsActive = false;
                DownloadUXBusyIndicator.Visibility = Visibility.Collapsed;
                //DownloadUXTextBlock.Visibility = Visibility.Collapsed;
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

        private void HandlePreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (!e.Handled)
            {
                e.Handled = true;
                var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
                eventArg.RoutedEvent = UIElement.MouseWheelEvent;
                eventArg.Source = sender;
                var parent = ((Control)sender).Parent as UIElement;
                parent.RaiseEvent(eventArg);
            }
        }

        private void ShowDellURL_Click(object sender, MouseButtonEventArgs e)
        {
            string url = "https://www.dell.com/support/home";
            try
            {
                DDPM.SA.Common.Settings.DDPMFileSecurity.StartProcessSafely(
                    null,
                    new ProcessStartInfo
                    {
                        FileName = url,
                        UseShellExecute = true
                    });
            }
            catch
            {
            }
        }
    }
}