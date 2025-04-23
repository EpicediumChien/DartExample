using CommunityToolkit.Mvvm.Input;
using DDPM.SA.Common;
using DDPM.SA.Common.Settings;
using DDPM.UI.Common;
using DDPM.UI.Common.UserControls;
using DDPM.UI.Interfaces;
using DDPM.UI.Module.HeadsetAudioForSB725Settings;
using DDPM.UI.Module.HeadsetAudioSettings;
using DDPM.UI.Module.HeadsetAutomatedActions;
using DDPM.UI.Module.HeadsetDeviceSettings;
using DDPM.UI.Plugin.Common;
using DDPM.UI.Plugin.ViewModels;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF.Controls;
using Microsoft.VisualBasic.Logging;
using System;
using System.Diagnostics;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace DDPM.UI.Plugin.AirAudioPlugin
{
    /// <summary>
    /// HeadsetPlugin.xaml 的互動邏輯
    /// </summary>
    public partial class LaunchView : UserControl
    {
        private readonly AirAudioViewModel? _vm;
        private IDeviceManagerSA _deviceManager;
        private readonly int[] _rightFrameWidth = new int[] { 0, 533, 533, 533 };
        //private readonly string Restore = "Restore to default";
        //private readonly string Unpair = "Unpair";
        private readonly string AudioSettings = Strings.HeadsetAudioSettings;
        private readonly string AutomatedActions = Strings.HeadsetAutomatedActions;
        private readonly string DeviceSettings = Strings.HeadsetDeviceSettings;
        private readonly Style ConnectionStyle1;
        private readonly Style ConnectionStyle2;
        private readonly BitmapImage img1 = new(new Uri($"/DDPM.UI.Resources;component/Resources/Images/Bluetooth.png", UriKind.Relative));
        private readonly BitmapImage img2 = new(new Uri($"/DDPM.UI.Resources;component/Resources/Images/Bluetooth2.png", UriKind.Relative));
        private ModuleGroup moduleGroup;

        public LaunchView()
        {

            _vm = (AirAudioViewModel?)AirAudioPlugin.PluginIoc?.GetService<IPeripheralViewModel>()!;

            if (_vm != null)
            {
                if (_vm.CurrentDeviceInfo == null)
                    return;

                _deviceManager = _vm._deviceManager;
                InitializeComponent();
                _vm.Reset();
                DataContext = _vm;
                _vm.VbarItemClickCommand = new RelayCommand<VbarItem1>(OnVbarItemClicked!);
                BuildModuleGroups();
                if (_vm!.ConnectionType == "WiredAudio")
                {
                    btnUnpair.Visibility = Visibility.Collapsed;
                }

                //txtUnpair.Text = Unpair;
                //txtRestore.Text = Restore;

                ConnectionStyle1 = (Style)FindResource("ConnectionStyle1");
                ConnectionStyle2 = (Style)FindResource("ConnectionStyle2");
                //txtSystemName1.Text = Dns.GetHostName(); ;// _vm!.VisiblePairedHostName1;
                //txtSystemName2.Text = _vm.VisiblePairedHostName1;
                txtSystemName3.Text = _vm.VisiblePairedHostName1;
                txtFirmware.Text = "Dongle " + _vm.PhysicalDeviceFWVersion;
                txtSlot.Text = $"{_vm.CurrentDeviceInfo?.MaxPairingSlots - _vm.CurrentDeviceInfo?.PairedDeviceCount} of {_vm.CurrentDeviceInfo?.MaxPairingSlots} slots available";
                txtAudioBLText.Text = string.Format(Strings.Paired_Info, _vm.CurrentDeviceInfo?.TotalNumberOfPairedHostName);
                if (_deviceManager != null)
                {
                    _deviceManager.ITSettingsActionEvent += DeviceManagerSA_ITSettingsActionEvent;

                    DDPMSettings data = _deviceManager.ReloadAppConfigData().Result;
                    if (data != null)
                    {
                        if (data.LockSettings.Lock_Setting_RestoreDefaults)
                        {
                            RestoreLockIcon.Visibility = Visibility.Visible;
                            txtRestore.IsEnabled = false;
                        }
                        else
                        {
                            txtRestore.IsEnabled = !data.LockSettings.Lock_Audio_RestoreFactoryDefaults;
                            RestoreLockIcon.Visibility = data.LockSettings.Lock_Audio_RestoreFactoryDefaults ? Visibility.Visible : Visibility.Collapsed;

                            //Lock Functionality 9/7
                            //When a 1 or more settings are locked, automatically lock 'Restore to default'/'factory reset' control [Audio]
                            if (data.LockSettings != null &&
                                DdpmCommonHelper.GetUINotifyPropertyValue_isAnyLocked(data, "Lock_Audio"))
                            {
                                RestoreLockIcon.Visibility = Visibility.Visible;
                                txtRestore.IsEnabled = false;
                            }
                        }
                    }
                }
                DdpmCommonHelper.BitmapImageUpdated += ImageUpdate;
                Loaded += LaunchView_LoadedStatus;
                Unloaded += LaunchView_UnLoadedStatus;
            }
        }


        private async void LaunchView_UnLoadedStatus(object sender, RoutedEventArgs e)
        {
            if (_vm == null)
                return;
            _vm.UloadAirAudio_DTPNotify();
            if (_deviceManager != null)
            {
                _deviceManager.ITSettingsActionEvent -= DeviceManagerSA_ITSettingsActionEvent;
                DdpmCommonHelper.BitmapImageUpdated -= ImageUpdate;
                Loaded -= LaunchView_LoadedStatus;
                Unloaded -= LaunchView_UnLoadedStatus;
                DdpmCommonHelper.WriteUILog($"[Headset] ~LaunchView");
            }

            moduleGroup?.Dispose();
        }

        private void LaunchView_LoadedStatus(object sender, RoutedEventArgs e)
        {
            if (_vm == null)
                return;

            try
            {
                _vm.Invoke_PleaseWaitAsync(_vm.Model, _vm);
                DdpmCommonHelper.WriteUILog($"[Headset] LaunchView_LoadedStatus Invoke_PleaseWaitAsync Check Done");
                if (_vm.IsRestoreEnable)
                {
                    btnRestore.Visibility = Visibility.Visible;
                }
                else
                {
                    btnRestore.Visibility = Visibility.Collapsed;
                }
                DdpmCommonHelper.WriteUILog($"[Headset] LaunchView_LoadedStatus IsRestoreEnable Check Done");
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog($"[Headset] LaunchView_LoadedStatus Exception = {ex.Message}");
            }
        }

        private void ImageUpdate(OSThemeEnum oSThemeEnum)
        {
            ArrowLeft.Source = null;
            ArrowLeft.Source = (BitmapImage)Application.Current.Resources["Arrow_Left"];
        }

        private void LaunchView_Loaded(object sender, RoutedEventArgs e)
        {
            if (!_vm!.IsDTPReady)
                DdpmCommonHelper.MyConsole!.ShowHomePage();
        }

        private void DeviceManagerSA_ITSettingsActionEvent(object? sender, SA.Common.ITSettingEventArgs e)
        {
            var rst = DdpmCommonHelper.ApplyRestoreFactoryDefaultsEventData(e, "Lock_Audio_RestoreFactoryDefaults");
            Dispatcher.Invoke(new Action(() =>
            {
                RestoreLockIcon.Visibility = rst.isLocked;
                txtRestore.IsEnabled = rst.isEnabled;
            }));
        }

        #region Init for Modules

        /// <summary>
        /// Base on specified monitor's capabilities to build the Vbar items, and headers/modules
        /// </summary>
        private void BuildModuleGroups(bool secondVbar = true)
        {
            List<ModuleGroup> groups = new List<ModuleGroup>();
            moduleGroup = new ModuleGroup()
            {
                GroupName = AudioSettings,
                GroupIcon = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Headset_Setting.png"),
                GroupIconCanvas = DdpmCommonHelper.CanvasIconCreator(VbarIcon.AudioSettings)
            };
            moduleGroup.AddHeader(AudioSettings, new HeadsetAudioForSB725SettingsModule(_vm!));
            groups.Add(moduleGroup);
            _vm.ModuleGroups = groups;
        }

        #endregion Init for Modules

        #region Vbar

        /// <summary>
        /// Contron Menu slider position
        /// </summary>
        /// <param name="newItem"></param>
        private void OnVbarItemClicked(VbarItem1 newItem)
        {
            if (newItem.Id == _vm!.VbarSelectedIndex)
            { return; }

            if (_rightFrameWidth[newItem.Id + 1] != _rightFrameWidth[_vm.VbarSelectedIndex + 1])
            {
                _vm.RightFrameWidthFrom = _rightFrameWidth[_vm.VbarSelectedIndex + 1];
                _vm.RightFrameWidthTo = _rightFrameWidth[newItem.Id + 1];

                InvokeGotoTwoViewModeAnimation();
            }
            if (newItem.Id == 0)
            {
                InvokeShrinkAnimation();
            }
            else if (_vm.VbarSelectedIndex == 0)
            {
                InvokeEnlargeAnimation();
            }
            _vm.VbarSelectedIndex = newItem.Id;
            if (_vm.RightViewHeaders != null)
            {
                rightViewHeaderCtrl.SetHeaders(_vm.RightViewHeaders.ToArray());
            }
            btnRestore.Visibility = Visibility.Collapsed;
            btnUnpair.Visibility = Visibility.Collapsed;
            _vm.SetLadningMode(false);
            _vm.SelectVBar();
        }

        #endregion Vbar

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
                //UnpairModalDialog unpairModalDialog = new(eDeviceCategory.KB);
                UnpairModalDialog unpairModalDialog = new(eDeviceCategory.Headset);
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
#pragma warning disable CA1416
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
#pragma warning restore CA1416
            }
        }

        private void Mainframe_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (_vm!.VbarSelectedIndex == -1)
            { return; }
            _vm.UpdateResetToDefault();
            if (_vm!.ConnectionType != "WiredAudio")
            {
                btnUnpair.Visibility = Visibility.Visible;
            }
            if (_vm.IsRestoreEnable)
            {
                btnRestore.Visibility = Visibility.Visible;
            }
            else
            {
                btnRestore.Visibility = Visibility.Collapsed;
            }
            _vm.RightFrameWidthTo = 0;
            _vm.RightFrameWidthFrom = _rightFrameWidth[_vm.VbarSelectedIndex + 1];
            InvokeGotoTwoViewModeAnimation();
            if (_vm.VbarSelectedIndex == 0)
            { InvokeEnlargeAnimation(); }
            _vm.VbarSelectedIndex = -1;
            _vm.SetLadningMode(true);
            _vm.SelectVBar();
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
                _vm!.RestoreToDefault();
                //if (_vm.IsRestoreEnable)
                //{
                //    btnRestore.Visibility = Visibility.Visible;
                //}
                //else
                //{
                btnRestore.Visibility = Visibility.Collapsed;
                //}
                //MessageBox.Show("OK button was clicked");
            }
        }

        private void BatteryIndicator_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (_vm!.ConnectionType == "WiredAudio")
            {
                return;
            }
            if (_vm!.ConnectionType == "Dongle")
            {
                //string pp = DdpmCommonHelper.DeviceManagerSA.GetFirmwareVersionAsyncForDongle(_vm!.CurrentDeviceID.ToString()).Result;
                //string ppp = DdpmCommonHelper.DeviceManagerSA.GetFirmwareVersionAsync(_vm!.CurrentDeviceID.ToString()).Result;
                txtSystemName3.Text = " " + Strings.USBWirelessReceiver;
                txtFirmware.Text = $"{Strings.ReceiverFirmwareVersion} {_vm.PhysicalDeviceFWVersion}";
                txtSlot.Text = $"{_vm.CurrentDeviceInfo.MaxPairingSlots - _vm.CurrentDeviceInfo.PairedDeviceCount} of {_vm.CurrentDeviceInfo.MaxPairingSlots} slots available";
                DongleConnection.Visibility = Visibility.Visible;
            }
            else if (_vm!.ConnectionType == "Bluetooth")//I can't get Headset connection HostName, FW issue?
            {
                //_vm.PairedHostName1 = DdpmCommonHelper.DeviceManagerSA.GetHeadsetPairedHostName2Async(_vm.CurrentDeviceInfo.ID.ToString()).Result;
                //_vm.PairedHostName2 = DdpmCommonHelper.DeviceManagerSA.GetHeadsetPairedHostName3Async(_vm.CurrentDeviceInfo.ID.ToString()).Result;
                //_deviceManager.GetFirmwareVersionAsync(CurrentDeviceID.ToString()).Result;
                //string hostName = Dns.GetHostName();
                string PairedHostName1 = string.Empty;
                string PairedHostName2 = string.Empty;
                if (string.IsNullOrEmpty(_vm.PairedHostName1))
                    PairedHostName1 = _vm.isAirAudio == false ? _deviceManager.GetHeadsetPairedHostName2Async(_vm.CurrentDeviceInfo.ID.ToString()).Result : null; //DTP
                else
                    PairedHostName1 = _vm.PairedHostName1; //DTH
                if (string.IsNullOrEmpty(_vm.PairedHostName2))
                    PairedHostName2 = _vm.isAirAudio == false ? _deviceManager.GetHeadsetPairedHostName3Async(_vm.CurrentDeviceInfo.ID.ToString()).Result : null; //DTP
                else
                    PairedHostName2 = _vm.PairedHostName2;  //DTH

                if (string.IsNullOrEmpty(PairedHostName1))
                {
                    txt1.Style = ConnectionStyle2;
                    txtBLHost1.Style = ConnectionStyle2;
                    //imgBL1.Source = img2;
                }
                else
                {
                    txt1.Style = ConnectionStyle1;
                    txtBLHost1.Style = ConnectionStyle1;
                    //imgBL1.Source = img2;
                }
                if (string.IsNullOrEmpty(PairedHostName2))
                {
                    txt2.Style = ConnectionStyle2;
                    txtBLHost2.Style = ConnectionStyle2;
                    //imgBL2.Source = img2;
                }
                else
                {
                    txt2.Style = ConnectionStyle1;
                    txtBLHost2.Style = ConnectionStyle1;
                    //imgBL2.Source = img2;
                }
                txtBLHost1.Text = string.IsNullOrEmpty(PairedHostName1) ? Strings.ReadyToBePaired : PairedHostName1;
                txtBLHost2.Text = string.IsNullOrEmpty(PairedHostName2) ? Strings.ReadyToBePaired : PairedHostName2;
                BLConnection.Visibility = Visibility.Visible;
            }
        }

        private void BatteryIndicator_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            DongleConnection.Visibility = Visibility.Collapsed;
            BLConnection.Visibility = Visibility.Collapsed;
        }

        private void LargeImage_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
        }
        private void PushBack(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is Border)
            {
                Mainframe_MouseLeftButtonDown(this, e);
            }
        }
    }
}