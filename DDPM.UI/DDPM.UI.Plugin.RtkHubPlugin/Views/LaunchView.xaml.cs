using CommunityToolkit.Mvvm.Input;
using DDPM.UI.Common;
using DDPM.UI.Common.UserControls;
using DDPM.UI.Interfaces;
using DDPM.UI.Module.RtkHubPortInfo;
using DDPM.UI.Plugin.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace DDPM.UI.Plugin.RtkHubPlugin
{
    /// <summary>
    /// HeadsetPlugin.xaml 的互動邏輯
    /// </summary>
    public partial class LaunchView : UserControl
    {
        private readonly RtkHubViewModel? _vm;

        private readonly int[] _rightFrameWidth = new int[] { 0, 530, 530, 530 };
        private readonly string AudioSettings = Strings.RtkHub01;

        public LaunchView()
        {
            DdpmCommonHelper.WriteUILog($"[RtkHub_LaunchView] LaunchView Constructor ... in ");
            _vm = (RtkHubViewModel?)RtkHubPlugin.PluginIoc?.GetService<IPeripheralViewModel>()!;

            if (_vm != null)
            {

                InitializeComponent();
                _vm.Reset();
                DataContext = _vm;
                _vm.VbarItemClickCommand = new RelayCommand<VbarItem1>(OnVbarItemClicked!);
                _vm!.ConnectionType = "Port";
                BuildModuleGroups();
                InitializeButtonImage();
                btnUnpair.Visibility = Visibility.Collapsed;
                //ConnectionStyle1 = (Style)FindResource("ConnectionStyle1");
                //ConnectionStyle2 = (Style)FindResource("ConnectionStyle2");
                txtSystemName3.Text = _vm.VisiblePairedHostName1;
                txtFirmware.Text = "Dongle " + _vm.PhysicalDeviceFWVersion;
                //txt1.Text = Strings.USB_C;
                //txtSlot.Text = $"{_vm.CurrentDeviceInfo!.MaxPairingSlots - _vm.CurrentDeviceInfo.PairedDeviceCount} of {_vm.CurrentDeviceInfo.MaxPairingSlots} slots available";
                //txtAudioBLText.Text = string.Format(Strings.Paired_Info, _vm.CurrentDeviceInfo.TotalNumberOfPairedHostName);
                Loaded += LaunchView_LoadedStatus;
                Unloaded += LaunchView_UnLoadedStatus;
            }
            DdpmCommonHelper.WriteUILog($"[RtkHub_LaunchView] LaunchView Constructor ... end ");
        }

        private void LaunchView_UnLoadedStatus(object sender, RoutedEventArgs e)
        {
            //DdpmCommonHelper.WriteUILog($"[RtkHub_LaunchView] LaunchView_UnLoadedStatus ... in ");
            //if (_vm == null)
            //{
            //    DdpmCommonHelper.WriteUILog($"[RtkHub_LaunchView] ~LaunchView_UnLoadedStatus _vm is null");
            //    return;
            //}
            //try
            //{
            //    _vm.UloadHeadset_DTPNotify();
            //    if (DdpmCommonHelper.DeviceManagerSA != null)
            //    {
            //        DdpmCommonHelper.DeviceManagerSA.ITSettingsActionEvent -= DeviceManagerSA_ITSettingsActionEvent;
            //        Loaded -= LaunchView_LoadedStatus;
            //        Unloaded -= LaunchView_UnLoadedStatus;
            //        DdpmCommonHelper.WriteUILog($"[RtkHub_LaunchView] -= DeviceManagerSA_ITSettingsActionEvent、ImageUpdate、LaunchView_LoadedStatus、LaunchView_UnLoadedStatus");
            //    }
            //    else
            //    {
            //        DdpmCommonHelper.WriteUILog($"[RtkHub_LaunchView] ~LaunchView_UnLoadedStatus DeviceManagerSA is null");
            //    }
            //    DdpmCommonHelper.WriteUILog($"[RtkHub_LaunchView] LaunchView_UnLoadedStatus ... out ");
            //}
            //catch (Exception ex)
            //{
            //    DdpmCommonHelper.WriteUILog($"[RtkHub_LaunchView] LaunchView_UnLoadedStatus Exception = {ex.Message}");
            //}
        }

        private void LaunchView_LoadedStatus(object sender, RoutedEventArgs e)
        {
            //DdpmCommonHelper.WriteUILog($"[RtkHub_LaunchView] LaunchView_LoadedStatus ... in ");
            //if (_vm == null)
            //{
            //    DdpmCommonHelper.WriteUILog($"[RtkHub_LaunchView] ~LaunchView_LoadedStatus _vm is null");
            //    return;
            //}

            //try
            //{
            //    DdpmCommonHelper.WriteUILog($"[RtkHub_LaunchView] LaunchView_LoadedStatus Invoke_PleaseWaitAsync Check Done");
            //    DdpmCommonHelper.WriteUILog($"[RtkHub_LaunchView] LaunchView_LoadedStatus ... out ");
            //}
            //catch (Exception ex)
            //{
            //    DdpmCommonHelper.WriteUILog($"[RtkHub_LaunchView] LaunchView_LoadedStatus Exception = {ex.Message}");
            //}
        }

        private void LaunchView_Loaded(object sender, RoutedEventArgs e)
        {
            //try
            //{
            //    if (!_vm!.IsDTPReady)
            //        DdpmCommonHelper.MyConsole!.ShowHomePage();
            //}
            //catch (Exception ex)
            //{
            //    DdpmCommonHelper.WriteUILog($"[RtkHub_LaunchView] LaunchView_Loaded Exception = {ex.Message}");
            //}
        }

        private void DeviceManagerSA_ITSettingsActionEvent(object? sender, SA.Common.ITSettingEventArgs e)
        {
            //try
            //{
            //    var rst = DdpmCommonHelper.ApplyRestoreFactoryDefaultsEventData(e, "Lock_Audio_RestoreFactoryDefaults");
            //    //Dispatcher.Invoke(new Action(() =>
            //    //{
            //    //    RestoreLockIcon.Visibility = rst.isLocked;
            //    //    txtRestore.IsEnabled = rst.isEnabled;
            //    //}));
            //}
            //catch (Exception ex)
            //{
            //    DdpmCommonHelper.WriteUILog($"[RtkHub_LaunchView] LaunchView DeviceManagerSA_ITSettingsActionEvent Exception = {ex.Message}");
            //}
        }

        #region Init for Modules

        /// <summary>
        /// Base on specified monitor's capabilities to build the Vbar items, and headers/modules
        /// </summary>
        private void BuildModuleGroups(bool secondVbar = true)
        {
            DdpmCommonHelper.WriteUILog($"[RtkHub_LaunchView] BuildModuleGroups ... in");
            try
            {
                List<ModuleGroup> groups = new List<ModuleGroup>();
                ModuleGroup moduleGroup;
                moduleGroup = new ModuleGroup()
                {
                    GroupName = AudioSettings,
                    GroupIcon = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Images/DA225_PortInfo.png", "DDPM.UI.Resources"),
                    GroupIconCanvas = DdpmCommonHelper.CanvasIconCreator(VbarIcon.RtkHubPortInfo)
                };
                moduleGroup.AddHeader(AudioSettings, new RtkHubPortInfoModule(_vm!));
                groups.Add(moduleGroup);
                _vm.ModuleGroups = groups;
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog($"[RtkHub_LaunchView] BuildModuleGroups Exception = {ex.Message}");
            }
        }

        private void InitializeButtonImage()
        {
            //try
            //{
            //    switch (_vm!.Model.ToUpper())
            //    {
            //        case "WL7024":
            //            RecT.Height = 50;
            //            RecB.Height = 120;
            //            RecL.Width = 150;
            //            RecR.Width = 135;
            //            break;
            //        case "WL5024":
            //            RecT.Height = 100;
            //            RecB.Height = 105;
            //            RecL.Width = 185;
            //            RecR.Width = 185;
            //            break;
            //        case "WH5024":
            //            RecT.Height = 100;
            //            RecB.Height = 105;
            //            RecL.Width = 185;
            //            RecR.Width = 185;
            //            break;
            //        case "WL3024":
            //            RecT.Height = 50;
            //            RecB.Height = 120;
            //            RecL.Width = 150;
            //            RecR.Width = 135;
            //            break;
            //        case "WH3024":
            //            RecT.Height = 90;
            //            RecB.Height = 100;
            //            RecL.Width = 180;
            //            RecR.Width = 170;
            //            break;
            //        default:
            //            RecT.Height = 50;
            //            RecB.Height = 120;
            //            RecL.Width = 136;
            //            RecR.Width = 133;
            //            break;
            //    }
            //}
            //catch (Exception ex)
            //{
            //    DdpmCommonHelper.WriteUILog($"[Headset_LaunchView] InitializeButtonImage Exception = {ex.Message}");
            //}
        }

        #endregion Init for Modules

        #region Vbar

        /// <summary>
        /// Contron Menu slider position
        /// </summary>
        /// <param name="newItem"></param>
        private void OnVbarItemClicked(VbarItem1 newItem)
        {
            //DdpmCommonHelper.WriteUILog($"[RtkHub_LaunchView] OnVbarItemClicked ... in");
            //try
            //{
            //    if (newItem.Id == _vm!.VbarSelectedIndex)
            //    { return; }

            //    if (_rightFrameWidth[newItem.Id + 1] != _rightFrameWidth[_vm.VbarSelectedIndex + 1])
            //    {
            //        _vm.RightFrameWidthFrom = _rightFrameWidth[_vm.VbarSelectedIndex + 1];
            //        _vm.RightFrameWidthTo = _rightFrameWidth[newItem.Id + 1];

            //        InvokeGotoTwoViewModeAnimation();
            //    }
            //    if (newItem.Id == 0)
            //    {
            //        InvokeShrinkAnimation();
            //    }
            //    else if (_vm.VbarSelectedIndex == 0)
            //    {
            //        InvokeEnlargeAnimation();
            //    }
            //    _vm.VbarSelectedIndex = newItem.Id;
            //    if (_vm.RightViewHeaders != null)
            //    {
            //        rightViewHeaderCtrl.SetHeaders(_vm.RightViewHeaders.ToArray());
            //    }
            //    btnRestore.Visibility = Visibility.Collapsed;
            //    btnUnpair.Visibility = Visibility.Collapsed;
            //    _vm.SetLadningMode(false);
            //    _vm.SelectVBar();
            //}
            //catch (Exception ex)
            //{
            //    DdpmCommonHelper.WriteUILog($"[RtkHub_LaunchView] OnVbarItemClicked Exception = {ex.Message}");
            //}
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
            try
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
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog($"[RtkHub_LaunchView] InvokeGotoTwoViewModeAnimation Exception = {ex.Message}");
            }
        }

        private void InvokeShrinkAnimation()
        {
            try
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
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog($"[RtkHub_LaunchView] InvokeShrinkAnimation Exception = {ex.Message}");
            }
        }

        private void InvokeEnlargeAnimation()
        {
            try
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
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog($"[RtkHub_LaunchView] InvokeEnlargeAnimation Exception = {ex.Message}");
            }
        }

        #endregion Mode Change


        private void Mainframe_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //DdpmCommonHelper.WriteUILog($"[Headset_LaunchView] Mainframe_MouseLeftButtonDown ... in");
            //try
            //{
            //    if (_vm!.VbarSelectedIndex == -1)
            //    { return; }
            //    _vm.UpdateResetToDefault();
            //    if (_vm!.ConnectionType != "WiredAudio")
            //    {
            //        btnUnpair.Visibility = Visibility.Visible;
            //    }
            //    if (!_vm.IsRestoreEnable)
            //    {
            //        btnRestore.Visibility = Visibility.Visible;
            //    }
            //    else
            //    {
            //        btnRestore.Visibility = Visibility.Collapsed;
            //    }
            //    _vm.RightFrameWidthTo = 0;
            //    _vm.RightFrameWidthFrom = _rightFrameWidth[_vm.VbarSelectedIndex + 1];
            //    InvokeGotoTwoViewModeAnimation();
            //    if (_vm.VbarSelectedIndex == 0)
            //    { InvokeEnlargeAnimation(); }
            //    _vm.VbarSelectedIndex = -1;
            //    _vm.SetLadningMode(true);
            //    _vm.SelectVBar();
            //}
            //catch (Exception ex)
            //{
            //    DdpmCommonHelper.WriteUILog($"[Headset_LaunchView] Mainframe_MouseLeftButtonDown Exception = {ex.Message}");
            //}
        }

        private async void BatteryIndicator_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            //DdpmCommonHelper.WriteUILog("[Headset_LaunchView] BatteryIndicator_MouseEnter ... in");
            //try
            //{
            //    if (_vm?.ConnectionType == "WiredAudio")
            //    {
            //        DdpmCommonHelper.WriteUILog("[Headset_LaunchView] BatteryIndicator_MouseEnter WiredAudio ... ");
            //        return;
            //    }

            //    if (_vm?.ConnectionType == "Dongle")
            //    {
            //        txtSystemName3.Text = " " + Strings.USBWirelessReceiver;
            //        txtFirmware.Text = $"{Strings.ReceiverFirmwareVersion} {_vm.PhysicalDeviceFWVersion}";
            //        txtSlot.Text = $"{_vm.CurrentDeviceInfo!.MaxPairingSlots - _vm.CurrentDeviceInfo.PairedDeviceCount} of {_vm.CurrentDeviceInfo.MaxPairingSlots} slots available";
            //        DongleConnection.Visibility = Visibility.Visible;
            //        DdpmCommonHelper.WriteUILog("[Headset_LaunchView] BatteryIndicator_MouseEnter Dongle ... ");
            //    }
            //    else if (_vm?.ConnectionType == "Bluetooth")
            //    {
            //        string pairedHostName1, pairedHostName2;

            //        if (DdpmCommonHelper.DeviceManagerSA != null)
            //        {
            //            // DTP
            //            pairedHostName1 = await DdpmCommonHelper.DeviceManagerSA.GetHeadsetPairedHostName2Async(_vm.CurrentDeviceInfo.ID.ToString());
            //            if (string.IsNullOrEmpty(pairedHostName1))
            //            {
            //                pairedHostName1 = _vm.PairedHostName1;
            //                DdpmCommonHelper.WriteUILog($"[Headset_LaunchView] BatteryIndicator_MouseEnter Bluetooth DeviceManagerSA not null, GetHeadsetPairedHostName2Async IsNullOrEmpty,  DTH : {_vm.PairedHostName1} ... ");
            //            }
            //            else
            //            {
            //                DdpmCommonHelper.WriteUILog($"[Headset_LaunchView] BatteryIndicator_MouseEnter Bluetooth DeviceManagerSA not null, GetHeadsetPairedHostName2Async DTP pairedHostName1 = {pairedHostName1} ... ");
            //            }
            //            pairedHostName2 = await DdpmCommonHelper.DeviceManagerSA.GetHeadsetPairedHostName3Async(_vm.CurrentDeviceInfo.ID.ToString());
            //            if (string.IsNullOrEmpty(pairedHostName2))
            //            {
            //                pairedHostName2 = _vm.PairedHostName2;
            //                DdpmCommonHelper.WriteUILog($"[Headset_LaunchView] BatteryIndicator_MouseEnter Bluetooth DeviceManagerSA not null, GetHeadsetPairedHostName3Async IsNullOrEmpty,  DTH : {_vm.PairedHostName2} ... ");
            //            }
            //            else
            //            {
            //                DdpmCommonHelper.WriteUILog($"[Headset_LaunchView] BatteryIndicator_MouseEnter Bluetooth DeviceManagerSA not null, GetHeadsetPairedHostName2Async DTP pairedHostName2 = {pairedHostName2} ... ");
            //            }
            //        }
            //        else
            //        {
            //            // DTH
            //            pairedHostName1 = _vm.PairedHostName1;
            //            pairedHostName2 = _vm.PairedHostName2;
            //            DdpmCommonHelper.WriteUILog($"[Headset_LaunchView] BatteryIndicator_MouseEnter Bluetooth DeviceManagerSA null, 1 = {_vm.PairedHostName1} : 2 = {_vm.PairedHostName2} ... ");
            //        }

            //        if (string.IsNullOrEmpty(pairedHostName1))
            //        {
            //            Host1.Visibility = Visibility.Collapsed;
            //            txtBLHost1.Text = Strings.ReadyToBePaired;
            //        }
            //        else
            //        {
            //            Host1.Visibility = Visibility.Visible;
            //            txt1.Style = ConnectionStyle1;
            //            txtBLHost1.Style = ConnectionStyle1;
            //            txtBLHost1.Text = pairedHostName1;
            //        }

            //        if (string.IsNullOrEmpty(pairedHostName2))
            //        {
            //            Host2.Visibility = Visibility.Collapsed;
            //            txtBLHost2.Text = Strings.ReadyToBePaired;
            //        }
            //        else
            //        {
            //            Host2.Visibility = Visibility.Visible;
            //            txt2.Style = ConnectionStyle1;
            //            txtBLHost2.Style = ConnectionStyle1;
            //            txtBLHost2.Text = pairedHostName2;
            //        }

            //        BLConnection.Visibility = Visibility.Visible;

            //        DdpmCommonHelper.WriteUILog("[Headset_LaunchView] BatteryIndicator_MouseEnter Bluetooth ... ");
            //    }
            //}
            //catch (Exception ex)
            //{
            //    DdpmCommonHelper.WriteUILog($"[Headset_LaunchView] BatteryIndicator_MouseEnter Exception = {ex.Message}");
            //}
        }

        private void BatteryIndicator_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            //try
            //{
            //    DongleConnection.Visibility = Visibility.Collapsed;
            //    BLConnection.Visibility = Visibility.Collapsed;
            //}
            //catch (Exception ex)
            //{
            //    DdpmCommonHelper.WriteUILog($"[Headset_LaunchView] BatteryIndicator_MouseLeave Exception = {ex.Message}");
            //}
        }

        private void LargeImage_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
        }
        private void PushBack(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                if (sender is Border)
                {
                    Mainframe_MouseLeftButtonDown(this, e);
                }
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog($"[RtkHub_LaunchView] LaunchView PushBack Exception = {ex.Message}");
            }
        }
    }
}