using CommunityToolkit.Mvvm.Input;
using DDPM.SA.Common;
using DDPM.SA.Common.Settings;
using DDPM.UI.Common;
using DDPM.UI.Interfaces;
using DDPM.UI.Module.HeadsetAudioSettings;
using DDPM.UI.Module.HeadsetAutomatedActions;
using DDPM.UI.Module.HeadsetDeviceSettings;
using DDPM.UI.Plugin.Common;
using DDPM.UI.Plugin.ViewModels;
using System.Diagnostics;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace DDPM.UI.Plugin.HeadsetPlugin
{
    /// <summary>
    /// HeadsetPlugin.xaml 的互動邏輯
    /// </summary>
    public partial class LaunchView : UserControl
    {
        private readonly HeadsetViewModel? _vm;

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

        public LaunchView()
        {
            InitializeComponent();
            _vm = (HeadsetViewModel?)HeadsetPlugin.PluginIoc?.GetService<IPeripheralViewModel>()!;

            if (_vm != null)
            {
                _vm.Reset();
                DataContext = _vm;
                _vm.VbarItemClickCommand = new RelayCommand<VbarItem>(OnVbarItemClicked!);
                BuildModuleGroups();
            }

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
            txtSlot.Text = $"{_vm.CurrentDeviceInfo!.MaxPairingSlots - _vm.CurrentDeviceInfo.PairedDeviceCount} of {_vm.CurrentDeviceInfo.MaxPairingSlots} slots available";
            txtAudioBLText.Text = string.Format(Strings.Paired_Info, _vm.CurrentDeviceInfo.TotalNumberOfPairedHostName);
            if (DdpmCommonHelper.DeviceManagerSA != null)
            {
                DdpmCommonHelper.DeviceManagerSA.ITSettingsActionEvent += DeviceManagerSA_ITSettingsActionEvent;

                DDPMSettings data = DdpmCommonHelper.DeviceManagerSA.ReloadAppConfigData().Result;
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
                        if (data.LockSettings != null)
                        {
                            if (DdpmCommonHelper.GetUINotifyPropertyValue_isAnyLocked(data, "Lock_Audio"))
                            {
                                RestoreLockIcon.Visibility = Visibility.Visible;
                                txtRestore.IsEnabled = false;
                            }
                        }
                    }
                }
            }
        }

        ~LaunchView()
        {
            if (DdpmCommonHelper.DeviceManagerSA != null)
            {
                DdpmCommonHelper.DeviceManagerSA.ITSettingsActionEvent -= DeviceManagerSA_ITSettingsActionEvent;
            }
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
        /// Base on specified monitor's capabiliies to build the Vbar items, and headers/modules
        /// </summary>
        private void BuildModuleGroups()
        {
            List<ModuleGroup> groups = new List<ModuleGroup>();
            ModuleGroup moduleGroup;

            moduleGroup = new ModuleGroup()
            {
                GroupName = AudioSettings,
                GroupIcon = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Headset_Setting.png")
            };
            moduleGroup.AddHeader(AudioSettings, new HeadsetAudioSettingsModule(_vm!));
            groups.Add(moduleGroup);

            //if (!_vm!.IsCollabsKeysSupported)
            //{
            moduleGroup = new ModuleGroup()
            {
                GroupName = AutomatedActions,
                GroupIcon = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Headset_Media.png")
            };
            moduleGroup.AddHeader(AutomatedActions, new HeadsetAutomatedActionsModule(_vm));
            groups.Add(moduleGroup);
            //}
            //if (!_vm.IsIlluminationSupported)
            //{
            moduleGroup = new ModuleGroup()
            {
                GroupName = DeviceSettings,
                GroupIcon = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Headset_Main.png")
            };
            moduleGroup.AddHeader(DeviceSettings, new HeadsetDeviceSettingsModule(_vm));
            groups.Add(moduleGroup);
            //}

            _vm.ModuleGroups = groups;
        }

        #endregion Init for Modules

        #region Vbar

        /// <summary>
        /// Contron Menu slider position
        /// </summary>
        /// <param name="newItem"></param>
        private void OnVbarItemClicked(VbarItem newItem)
        {
            if (newItem.Id == _vm!.VbarSelectedIndex) { return; }

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
            if (_vm!.VbarSelectedIndex == -1) { return; }

            if (_vm!.ConnectionType != "WiredAudio")
            {
                btnUnpair.Visibility = Visibility.Visible;
            }
            btnRestore.Visibility = Visibility.Visible;
            _vm.RightFrameWidthTo = 0;
            _vm.RightFrameWidthFrom = _rightFrameWidth[_vm.VbarSelectedIndex + 1];
            InvokeGotoTwoViewModeAnimation();
            if (_vm.VbarSelectedIndex == 0) { InvokeEnlargeAnimation(); }
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
                txtSlot.Text = $"{_vm.CurrentDeviceInfo!.MaxPairingSlots - _vm.CurrentDeviceInfo.PairedDeviceCount} of {_vm.CurrentDeviceInfo.MaxPairingSlots} slots available";
                DongleConnection.Visibility = Visibility.Visible;
            }
            else if (_vm!.ConnectionType == "Bluetooth")//I can't get Headset connection HostName, FW issue?
            {
                string hostName = Dns.GetHostName();
                //var hostIndex = _vm!.VisiblePairedHostName1.ToUpper() == "VISIBLE" ? 1 : (_vm.VisiblePairedHostName2.ToUpper() == "VISIBLE" ? 2 : 3);
                txt1.Style = ConnectionStyle1;
                //imgBL1.Source = img2;
                txtBLHost1.Style = ConnectionStyle1;
                txt2.Style = ConnectionStyle1;
                //imgBL2.Source = img2;
                txtBLHost2.Style = ConnectionStyle1;
                txt3.Style = ConnectionStyle1;
                txtBLHost1.Text = string.IsNullOrEmpty(_vm.PairedHostName1) ? Strings.ReadyToBePaired : _vm.PairedHostName1;
                txtBLHost2.Text = string.IsNullOrEmpty(_vm.PairedHostName2) ? Strings.ReadyToBePaired : _vm.PairedHostName2;
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
    }
}