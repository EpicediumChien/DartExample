using CommunityToolkit.Mvvm.Input;
using DDPM.SA.Common.Settings;
using DDPM.UI.Common;
using DDPM.UI.Common.UserControls;
using DDPM.UI.Interfaces;
using DDPM.UI.Module.HeadsetAudioSettings;
using DDPM.UI.Module.HeadsetAutomatedActions;
using DDPM.UI.Module.HeadsetDeviceSettings;
using DDPM.UI.Plugin.Common;
using DDPM.UI.Plugin.ViewModels;
using DDPM.UI.Resources.Helper;
using Dell.Client.Framework.UX.WPF.Controls;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
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

        private readonly int[] _rightFrameWidth = new int[] { 0, 530, 530, 530 };
        private readonly string AudioSettings = LangHelper.Instance["AudioSettings"];
        private readonly string AutomatedActions = LangHelper.Instance["AutomatedActions"];
        private readonly string DeviceSettings = LangHelper.Instance["DeviceSettings"];
        private readonly Style ConnectionStyle1;
        private readonly Style ConnectionStyle2;

        public LaunchView()
        {
            DdpmCommonHelper.WriteUILog($"[Headset_LaunchView] LaunchView Constructor ... in ");
            _vm = (HeadsetViewModel?)HeadsetPlugin.PluginIoc?.GetService<IPeripheralViewModel>()!;

            if (_vm != null)
            {
                if (false)//!_vm.IsDTPReady)
                {
                    MessageModalDialog messageModalDialog = new(Strings.Error, Strings.DTPUnavailable, "");
                    Window mainWindow = System.Windows.Application.Current.MainWindow;
                    if (mainWindow != null)
                    {
                        messageModalDialog.Owner = mainWindow;
                        messageModalDialog.Left = mainWindow.Left + (mainWindow!.ActualWidth - 417) / 2;
                        messageModalDialog.Top = mainWindow.Top + 300;
                    }
                    Mouse.OverrideCursor = null;
                    messageModalDialog.WindowStartupLocation = WindowStartupLocation.Manual;
                    messageModalDialog.ShowDialog();
                    this.Loaded += LaunchView_Loaded;
                }
                else
                {
                    InitializeComponent();
                    _vm.Reset();
                    DataContext = _vm;
                    _vm.VbarItemClickCommand = new RelayCommand<VbarItem1>(OnVbarItemClicked!);
                    BuildModuleGroups();
                    InitializeButtonImage();
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
                    txtSystemName3.Text = _vm.VisiblePairedHostName1.Trim();
                    txtFirmware.Text = "Dongle " + _vm.PhysicalDeviceFWVersion;
                    txtSlot.Text = $"{_vm.CurrentDeviceInfo!.MaxPairingSlots - _vm.CurrentDeviceInfo.PairedDeviceCount} of {_vm.CurrentDeviceInfo.MaxPairingSlots} slots available";
                    txtAudioBLText.Text = string.Format(LangHelper.Instance["PairedInfo.0"], _vm.CurrentDeviceInfo.TotalNumberOfPairedHostName);
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
                    _vm!.HeadsetGroupChanged += HeadsetGroupChanged;
                    _vm!.BtnRestoreChanged += BtnRestoreChanged;
                }
            }
            DdpmCommonHelper.WriteUILog($"[Headset_LaunchView] LaunchView Constructor ... end ");
        }

        private void HeadsetGroupChanged(object? sender, EventArgs e)
        {
            BuildModuleGroups(_vm.SupportedAnswerCalls);
            vbarList.ItemsSource = null;
            vbarList.ItemsSource = _vm!.VbarItems;
            DdpmCommonHelper.WriteUILog($"[Headset_LaunchView] LaunchView HeadsetGroupChanged SupportedAnswerCalls false");
        }

        private void BtnRestoreChanged(object? sender, EventArgs e)
        {
            if (!_vm.IsRestoreEnable)
            {
                btnRestore.Visibility = Visibility.Visible;
                DdpmCommonHelper.WriteUILog($"[Headset_LaunchView] LaunchView BtnRestoreChanged IsRestoreEnable Visible");
            }
            else
            {
                btnRestore.Visibility = Visibility.Collapsed;
                DdpmCommonHelper.WriteUILog($"[Headset_LaunchView] LaunchView BtnRestoreChanged IsRestoreEnable Collapsed");
            }
        }

        private void LaunchView_UnLoadedStatus(object sender, RoutedEventArgs e)
        {
            DdpmCommonHelper.WriteUILog($"[Headset_LaunchView] LaunchView_UnLoadedStatus ... in ");
            if (_vm == null)
            {
                DdpmCommonHelper.WriteUILog($"[Headset_LaunchView] ~LaunchView_UnLoadedStatus _vm is null");
                return;
            }
            try
            {
                _vm.UloadHeadset_DTPNotify();
                _vm!.HeadsetGroupChanged -= HeadsetGroupChanged;
                _vm!.BtnRestoreChanged -= BtnRestoreChanged;
                if (DdpmCommonHelper.DeviceManagerSA != null)
                {
                    DdpmCommonHelper.DeviceManagerSA.ITSettingsActionEvent -= DeviceManagerSA_ITSettingsActionEvent;
                    DdpmCommonHelper.BitmapImageUpdated -= ImageUpdate;
                    Loaded -= LaunchView_LoadedStatus;
                    Unloaded -= LaunchView_UnLoadedStatus;
                    DdpmCommonHelper.WriteUILog($"[Headset_LaunchView] -= DeviceManagerSA_ITSettingsActionEvent、ImageUpdate、LaunchView_LoadedStatus、LaunchView_UnLoadedStatus");
                }
                else
                {
                    DdpmCommonHelper.WriteUILog($"[Headset_LaunchView] ~LaunchView_UnLoadedStatus DeviceManagerSA is null");
                }
                DdpmCommonHelper.WriteUILog($"[Headset_LaunchView] LaunchView_UnLoadedStatus ... out ");
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog($"[Headset_LaunchView] LaunchView_UnLoadedStatus Exception = {ex.Message}");
            }
        }

        private void LaunchView_LoadedStatus(object sender, RoutedEventArgs e)
        {
            DdpmCommonHelper.WriteUILog($"[Headset_LaunchView] LaunchView_LoadedStatus ... in ");
            if (_vm == null)
            {
                DdpmCommonHelper.WriteUILog($"[Headset_LaunchView] ~LaunchView_LoadedStatus _vm is null");
                return;
            }

            try
            {
                _vm.Invoke_PleaseWaitAsync(_vm.Model, _vm);
                DdpmCommonHelper.WriteUILog($"[Headset_LaunchView] LaunchView_LoadedStatus Invoke_PleaseWaitAsync Check Done");
                //if (!_vm.IsRestoreEnable)
                //{
                //    btnRestore.Visibility = Visibility.Visible;
                //}
                //else
                //{
                //    btnRestore.Visibility = Visibility.Collapsed;
                //}
                DdpmCommonHelper.WriteUILog($"[Headset_LaunchView] LaunchView_LoadedStatus ... out ");
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog($"[Headset_LaunchView] LaunchView_LoadedStatus Exception = {ex.Message}");
            }
        }

        private void ImageUpdate(OSThemeEnum oSThemeEnum)
        {
            ArrowLeft.Source = null;
            ArrowLeft.Source = (BitmapImage)Application.Current.Resources["Arrow_Left"];
        }

        private void LaunchView_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!_vm!.IsDTPReady)
                    DdpmCommonHelper.MyConsole!.ShowHomePage();
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog($"[Headset_LaunchView] LaunchView_Loaded Exception = {ex.Message}");
            }
        }

        private void DeviceManagerSA_ITSettingsActionEvent(object? sender, SA.Common.ITSettingEventArgs e)
        {
            try
            {
                var rst = DdpmCommonHelper.ApplyRestoreFactoryDefaultsEventData(e, "Lock_Audio_RestoreFactoryDefaults");
                Dispatcher.Invoke(new Action(() =>
                {
                    RestoreLockIcon.Visibility = rst.isLocked;
                    txtRestore.IsEnabled = rst.isEnabled;
                }));
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog($"[Headset_LaunchView] LaunchView DeviceManagerSA_ITSettingsActionEvent Exception = {ex.Message}");
            }
        }

        #region Init for Modules

        /// <summary>
        /// Base on specified monitor's capabilities to build the Vbar items, and headers/modules
        /// </summary>
        private void BuildModuleGroups(bool secondVbar = true)
        {
            DdpmCommonHelper.WriteUILog($"[Headset_LaunchView] BuildModuleGroups ... in");
            try
            {
                List<ModuleGroup> groups = new List<ModuleGroup>();
                ModuleGroup moduleGroup;
                moduleGroup = new ModuleGroup()
                {
                    GroupName = AudioSettings,
                    GroupIcon = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Headset_Setting.png"),
                    GroupIconCanvas = DdpmCommonHelper.CanvasIconCreator(VbarIcon.AudioSettings)
                };
                moduleGroup.AddHeader(AudioSettings, new HeadsetAudioSettingsModule(_vm!));
                groups.Add(moduleGroup);

                if (secondVbar)
                {
                    moduleGroup = new ModuleGroup()
                    {
                        GroupName = AutomatedActions,
                        GroupIcon = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Headset_Media.png"),
                        GroupIconCanvas = DdpmCommonHelper.CanvasIconCreator(VbarIcon.HeadsetAutoActions)
                    };
                    moduleGroup.AddHeader(AutomatedActions, new HeadsetAutomatedActionsModule(_vm!));
                    groups.Add(moduleGroup);
                }

                moduleGroup = new ModuleGroup()
                {
                    GroupName = DeviceSettings,
                    GroupIcon = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Headset_Main.png"),
                    GroupIconCanvas = DdpmCommonHelper.CanvasIconCreator(VbarIcon.HeadsetSettings)
                };
                moduleGroup.AddHeader(DeviceSettings, new HeadsetDeviceSettingsModule(_vm!));
                groups.Add(moduleGroup);

                _vm.ModuleGroups = groups;
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog($"[Headset_LaunchView] BuildModuleGroups Exception = {ex.Message}");
            }
        }

        private void InitializeButtonImage()
        {
            try
            {
                switch (_vm!.Model.ToUpper())
                {
                    case "WL7024":
                        RecT.Height = 50;
                        RecB.Height = 120;
                        RecL.Width = 150;
                        RecR.Width = 135;
                        break;
                    case "WL5024":
                        RecT.Height = 100;
                        RecB.Height = 105;
                        RecL.Width = 185;
                        RecR.Width = 185;
                        break;
                    case "WH5024":
                        RecT.Height = 100;
                        RecB.Height = 105;
                        RecL.Width = 185;
                        RecR.Width = 185;
                        break;
                    case "WL3024":
                        RecT.Height = 50;
                        RecB.Height = 120;
                        RecL.Width = 150;
                        RecR.Width = 135;
                        break;
                    case "WH3024":
                        RecT.Height = 90;
                        RecB.Height = 100;
                        RecL.Width = 180;
                        RecR.Width = 170;
                        break;
                    default:
                        RecT.Height = 50;
                        RecB.Height = 120;
                        RecL.Width = 136;
                        RecR.Width = 133;
                        break;
                }
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog($"[Headset_LaunchView] InitializeButtonImage Exception = {ex.Message}");
            }
        }

        #endregion Init for Modules

        #region Vbar

        /// <summary>
        /// Contron Menu slider position
        /// </summary>
        /// <param name="newItem"></param>
        private void OnVbarItemClicked(VbarItem1 newItem)
        {
            DdpmCommonHelper.WriteUILog($"[Headset_LaunchView] OnVbarItemClicked ... in");
            try
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
                btnUnpair.Visibility = Visibility.Collapsed;
                _vm.SetLadningMode(false);
                _vm.SelectVBar();
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog($"[Headset_LaunchView] OnVbarItemClicked Exception = {ex.Message}");
            }
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
                DdpmCommonHelper.WriteUILog($"[Headset_LaunchView] InvokeGotoTwoViewModeAnimation Exception = {ex.Message}");
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
                DdpmCommonHelper.WriteUILog($"[Headset_LaunchView] InvokeShrinkAnimation Exception = {ex.Message}");
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
                DdpmCommonHelper.WriteUILog($"[Headset_LaunchView] InvokeEnlargeAnimation Exception = {ex.Message}");
            }
        }

        #endregion Mode Change

        private void Unpair_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DdpmCommonHelper.WriteUILog($"[Headset_LaunchView] Unpair_Click ... in");
            try
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
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog($"[Headset_LaunchView] Unpair_Click Exception = {ex.Message}");
            }
        }

        private void Mainframe_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DdpmCommonHelper.WriteUILog($"[Headset_LaunchView] Mainframe_MouseLeftButtonDown ... in");
            try
            {
                if (_vm!.VbarSelectedIndex == -1)
                { return; }
                _vm.UpdateResetToDefault();
                if (_vm!.ConnectionType != "WiredAudio")
                {
                    btnUnpair.Visibility = Visibility.Visible;
                }
                if (!_vm.IsRestoreEnable)
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
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog($"[Headset_LaunchView] Mainframe_MouseLeftButtonDown Exception = {ex.Message}");
            }
        }

        private void Restore_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DdpmCommonHelper.WriteUILog($"[Headset_LaunchView] Restore_Click ... in");
            try
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
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog($"[Headset_LaunchView] Restore_Click Exception = {ex.Message}");
            }
        }

        private async void BatteryIndicator_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            DdpmCommonHelper.WriteUILog("[Headset_LaunchView] BatteryIndicator_MouseEnter ... in");
            try
            {
                if (_vm?.ConnectionType == "WiredAudio")
                {
                    DdpmCommonHelper.WriteUILog("[Headset_LaunchView] BatteryIndicator_MouseEnter WiredAudio ... ");
                    return;
                }

                if (_vm?.ConnectionType == "Dongle")
                {
                    txtSystemName3.Text = LangHelper.Instance["USBWirelessReceiver"];
                    txtFirmware.Text = $"{LangHelper.Instance["ReceiverFirmwareVersion"]} {_vm.PhysicalDeviceFWVersion}";
                    txtSlot.Text = $"{_vm.CurrentDeviceInfo!.MaxPairingSlots - _vm.CurrentDeviceInfo.PairedDeviceCount} of {_vm.CurrentDeviceInfo.MaxPairingSlots} slots available";
                    DongleConnection.Visibility = Visibility.Visible;
                    DdpmCommonHelper.WriteUILog("[Headset_LaunchView] BatteryIndicator_MouseEnter Dongle ... ");
                }
                else if (_vm?.ConnectionType == "Bluetooth")
                {
                    string pairedHostName1, pairedHostName2;

                    if (DdpmCommonHelper.DeviceManagerSA != null)
                    {
                        // DTP
                        pairedHostName1 = await DdpmCommonHelper.DeviceManagerSA.GetHeadsetPairedHostName2Async(_vm.CurrentDeviceInfo.ID.ToString());
                        if (string.IsNullOrEmpty(pairedHostName1))
                        {
                            pairedHostName1 = _vm.PairedHostName1;
                            DdpmCommonHelper.WriteUILog($"[Headset_LaunchView] BatteryIndicator_MouseEnter Bluetooth DeviceManagerSA not null, GetHeadsetPairedHostName2Async IsNullOrEmpty,  DTH : {_vm.PairedHostName1} ... ");
                        }
                        else
                        {
                            DdpmCommonHelper.WriteUILog($"[Headset_LaunchView] BatteryIndicator_MouseEnter Bluetooth DeviceManagerSA not null, GetHeadsetPairedHostName2Async DTP pairedHostName1 = {pairedHostName1} ... ");
                        }
                        pairedHostName2 = await DdpmCommonHelper.DeviceManagerSA.GetHeadsetPairedHostName3Async(_vm.CurrentDeviceInfo.ID.ToString());
                        if (string.IsNullOrEmpty(pairedHostName2))
                        {
                            pairedHostName2 = _vm.PairedHostName2;
                            DdpmCommonHelper.WriteUILog($"[Headset_LaunchView] BatteryIndicator_MouseEnter Bluetooth DeviceManagerSA not null, GetHeadsetPairedHostName3Async IsNullOrEmpty,  DTH : {_vm.PairedHostName2} ... ");
                        }
                        else
                        {
                            DdpmCommonHelper.WriteUILog($"[Headset_LaunchView] BatteryIndicator_MouseEnter Bluetooth DeviceManagerSA not null, GetHeadsetPairedHostName2Async DTP pairedHostName2 = {pairedHostName2} ... ");
                        }
                    }
                    else
                    {
                        // DTH
                        pairedHostName1 = _vm.PairedHostName1;
                        pairedHostName2 = _vm.PairedHostName2;
                        DdpmCommonHelper.WriteUILog($"[Headset_LaunchView] BatteryIndicator_MouseEnter Bluetooth DeviceManagerSA null, 1 = {_vm.PairedHostName1} : 2 = {_vm.PairedHostName2} ... ");
                    }

                    if (string.IsNullOrEmpty(pairedHostName1))
                    {
                        Host1.Visibility = Visibility.Collapsed;
                        txtBLHost1.Text = LangHelper.Instance["ReadyToBePaired"];
                    }
                    else
                    {
                        Host1.Visibility = Visibility.Visible;
                        txt1.Style = ConnectionStyle1;
                        txtBLHost1.Style = ConnectionStyle1;
                        txtBLHost1.Text = pairedHostName1;
                    }

                    if (string.IsNullOrEmpty(pairedHostName2))
                    {
                        Host2.Visibility = Visibility.Collapsed;
                        txtBLHost2.Text = LangHelper.Instance["ReadyToBePaired"];
                    }
                    else
                    {
                        Host2.Visibility = Visibility.Visible;
                        txt2.Style = ConnectionStyle1;
                        txtBLHost2.Style = ConnectionStyle1;
                        txtBLHost2.Text = pairedHostName2;
                    }

                    BLConnection.Visibility = Visibility.Visible;

                    DdpmCommonHelper.WriteUILog("[Headset_LaunchView] BatteryIndicator_MouseEnter Bluetooth ... ");
                }
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog($"[Headset_LaunchView] BatteryIndicator_MouseEnter Exception = {ex.Message}");
            }
        }

        private void BatteryIndicator_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            try
            {
                DongleConnection.Visibility = Visibility.Collapsed;
                BLConnection.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog($"[Headset_LaunchView] BatteryIndicator_MouseLeave Exception = {ex.Message}");
            }
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
                DdpmCommonHelper.WriteUILog($"[Headset_LaunchView] LaunchView PushBack Exception = {ex.Message}");
            }
        }
    }
}