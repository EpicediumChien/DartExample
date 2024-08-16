using CommunityToolkit.Mvvm.DependencyInjection;
using DDPM.UI.Common;
using DDPM.UI.Interfaces;
using DDPM.UI.Plugin.Common;
using DDPM.UI.Plugin.ViewModels;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace DDPM.UI.Plugin.DockPlugin.Views
{
    /// <summary>
    /// DockPage.xaml 的互動邏輯
    /// </summary>
    public partial class DockPage : UserControl
    {
        public static readonly Ioc PluginIoc = new();
        //private readonly string FWU = "Update Firmware";
        private DockPageViewModel? _vm;
        private ILog? _log;

        public DockPage()
        {
            InitializeComponent();
            _log = DockPlugin.PluginIoc?.GetService<ILog>();
            _log?.Debug("DisplayPage.ctor()");
            _vm = (DockPageViewModel?)DockPlugin.PluginIoc?.GetService<IPeripheralViewModel>()!;

            if (_vm != null)
            {
                _vm.Reset();
                DataContext = _vm;
                //_vm.VbarItemClickCommand = new RelayCommand<VbarItem>(OnVbarItemClicked!);
                //BuildModuleGroups();
            }
            if (_vm.IsEnableUpdate)
            {
                //0614 Bruce 修改按鈕名稱，並判斷是否需要顯示更新按鈕
                btnFWU.Visibility = Visibility.Visible;
            }
            else
            {
                btnFWU.Visibility = Visibility.Collapsed;
            }

            //txtFWUpdate.Text = FWU;

            txtSystemName1.Text = _vm!.VisiblePairedHostName1;
            txtSystemName2.Text = _vm.VisiblePairedHostName1;
            txtSystemName3.Text = _vm.VisiblePairedHostName1;
            txtFirmware.Text = string.Format(Strings.DockDongle1,_vm.PhysicalDeviceFWVersion);
            txtSlot.Text = string.Format(Strings.DockDongle0, _vm.CurrentDeviceInfo!.MaxPairingSlots - _vm.CurrentDeviceInfo.PairedDeviceCount, _vm.CurrentDeviceInfo.MaxPairingSlots);            
        }

        #region RightViewHeader

        private void RightViewHeaderCtrl_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;
        }

        #endregion RightViewHeader

        #region Mode Change

        private void InvokeGotoTwoViewModeAnimation()
        {
            Dispatcher.Invoke(new Action(() =>
            {
                Storyboard sb = (Storyboard)this.FindResource("StoryGotoTwoView");
                if (sb != null)
                {
                    sb.Completed += (o, s) =>
                    {
                    };

                    sb.Begin();
                }
            }));
        }

        private void InvokeShrinkAnimation()
        {
            Dispatcher.Invoke(new Action(() =>
            {
                Storyboard sb = (Storyboard)this.FindResource("StoryShrink");
                if (sb != null)
                {
                    sb.Completed += (o, s) =>
                    {
                    };

                    sb.Begin();
                }
            }));
        }

        private void InvokeEnlargeAnimation()
        {
            Dispatcher.Invoke(new Action(() =>
            {
                Storyboard sb = (Storyboard)this.FindResource("StoryEnlarge");
                if (sb != null)
                {
                    sb.Completed += (o, s) =>
                    {
                    };

                    sb.Begin();
                }
            }));
        }

        #endregion Mode Change

        private void Unpair_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (_vm!.ConnectionType == "Dongle")
            {
                UnpairModalDialog unpairModalDialog = new(eDeviceCategory.KB);
                Window parentWindow = Window.GetWindow(this);
                if (parentWindow != null)
                {
                    unpairModalDialog.Owner = parentWindow;
                }

                bool? dialogResult = unpairModalDialog.ShowDialog();
                if (dialogResult == true)
                {
                    _vm.Unpair();
                }
            }
            else
            {
                Version win10Version = new(10, 0);
                Version currentVersion = Environment.OSVersion.Version;
                if (currentVersion >= win10Version)
                {
                    Process.Start(new ProcessStartInfo("ms-settings:bluetooth")
                    {
                        UseShellExecute = true
                    });
                }
                else
                {
                    Process.Start(new ProcessStartInfo("control", "bthprops.cpl")
                    {
                        UseShellExecute = true
                    });
                }
            }
        }

        private void Mainframe_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            /*if (_vm!.VbarSelectedIndex == -1) { return; }

            _vm.RightFrameWidthTo = 0;
            _vm.RightFrameWidthFrom = _rightFrameWidth[_vm.VbarSelectedIndex + 1];
            InvokeGotoTwoViewModeAnimation();
            btnUnpair.Visibility = Visibility.Visible;
            if (_vm.VbarSelectedIndex == 0) { InvokeEnlargeAnimation(); }
            _vm.VbarSelectedIndex = -1;
            _vm.SetLadningMode(true);
            _vm.SelectVBar();*/
        }

        private void Restore_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            RestoreModalDialog restoreModalDialog = new();
            Window parentWindow = Window.GetWindow(this);
            if (parentWindow != null)
            {
                restoreModalDialog.Owner = parentWindow;
            }

            bool? dialogResult = restoreModalDialog.ShowDialog();
            if (dialogResult == true)
            {
                //MessageBox.Show("OK button was clicked");
            }
        }

        private void BatteryIndicator_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            /*if (_vm!.ConnectionType == "Dongle")
            {
                DongleConnection.Visibility = Visibility.Visible;
            }
            else
            {
                string hostName = Dns.GetHostName();
                if (_vm.VisiblePairedHostName1 == hostName)
                {
                    txt1.Style = ConnectionStyle1;
                    txt2.Style = ConnectionStyle2;
                    imgBL1.Source = img1;
                    imgBL2.Source = img2;
                    txtSystemName1.Style = ConnectionStyle1;
                    txtSystemName2.Style = ConnectionStyle2;
                }
                else
                {
                    txt1.Style = ConnectionStyle2;
                    txt2.Style = ConnectionStyle1;
                    imgBL1.Source = img2;
                    imgBL2.Source = img1;
                    txtSystemName1.Style = ConnectionStyle2;
                    txtSystemName2.Style = ConnectionStyle1;
                }
                BLConnection.Visibility = Visibility.Visible;
            }*/
        }

        private void BatteryIndicator_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            DongleConnection.Visibility = Visibility.Collapsed;
            BLConnection.Visibility = Visibility.Collapsed;
        }

        private void LargeImage_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
        }

        private void FWUpdate_Click(object sender, MouseButtonEventArgs e)
        {
            IConsole? console = DockPlugin.PluginIoc.GetService<IConsole>();
            console?.ShowPluginById(DDPM.UI.Common.Constants.SettingsPluginId);
            /*IShowPluginManager? _showPluginManager = DockPlugin.PluginIoc.GetService<IShowPluginManager>();
            _showPluginManager?.ShowPluginById(DDPM.UI.Common.Constants.SettingsPluginId, "CallFWU");*/
        }
    }
}