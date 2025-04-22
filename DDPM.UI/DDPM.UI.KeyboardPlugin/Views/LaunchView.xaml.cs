using CommunityToolkit.Mvvm.Input;
using DDPM.SA.Common;
using DDPM.SA.Common.Settings;
using DDPM.UI.Common;
using DDPM.UI.Common.UserControls;
using DDPM.UI.Interfaces;
using DDPM.UI.Module.Collaboration;
using DDPM.UI.Module.Illumination;
using DDPM.UI.Module.KeyCustomization;
using DDPM.UI.Plugin.Common;
using DDPM.UI.Plugin.ViewModels;
using System.Diagnostics;
using System.Drawing;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using Image = System.Windows.Controls.Image;

namespace DDPM.UI.Plugin.KeyboardPlugin
{
    /// <summary>
    /// KeyboardPlugin.xaml 的互動邏輯
    /// </summary>
    public partial class LaunchView : UserControl
    {
        private readonly KeyboardViewModel? _vm;
        private readonly int[] _rightFrameWidth = { 0, 330, 530, 530 };
        private readonly Style ConnectionStyle1;
        private readonly Style ConnectionStyle2;
        //private readonly BitmapImage img1 = new(new Uri($"/DDPM.UI.Resources;component/Resources/Images/Bluetooth.png", UriKind.Relative));
        //private readonly BitmapImage img2 = new(new Uri($"/DDPM.UI.Resources;component/Resources/Images/Bluetooth2.png", UriKind.Relative));
        private ModuleGroup moduleGroup;

        public LaunchView()
        {
            DdpmCommonHelper.WriteUILog($"Keyboard UI LaunchView Begin timestamp: {DateTime.Now:hh:mm:ss.ffffff}");
            InitializeComponent();
            _vm = (KeyboardViewModel?)Keyboardplugin.PluginIoc?.GetService<IPeripheralViewModel>();
            if (_vm == null)
            {
                DdpmCommonHelper.WriteUILog("Keyboard ViewModel is null");
                return;
            }

            DataContext = _vm;
            _vm.VbarItemClickCommand = new RelayCommand<VbarItem1>(OnVbarItemClicked!);

            if (_vm.EOLKBList.Contains(_vm.Model))
            {
                //Battery.Visibility = Visibility.Collapsed;
                btnRestore.Visibility = Visibility.Collapsed;
                //txtEOL.Text = Strings.EOLMessage;
                txtEOL.Visibility = Visibility.Visible;
                SectionA.Visibility = Visibility.Collapsed;
                SectionF.Visibility = Visibility.Collapsed;
                _vm.IsBatteryUnavailable = true;
                //EOLDongle.Visibility = Visibility.Visible;
            }
            else
            {
                _vm.IsBatteryUnavailable = false;
                BuildModuleGroups();
            }

            //txtUnpair.Text = Strings.Unpair;
            //txtRestore.Text = Strings.RestoreToDefault;

            ConnectionStyle1 = (Style)FindResource("ConnectionStyle1");
            ConnectionStyle2 = (Style)FindResource("ConnectionStyle2");
            //txtDongleHost.Text = Strings.USBWirelessReceiver;

            InitializeKeyImage();
            if (_vm.IsRestoreEnable)
            {
                btnRestore.Visibility = Visibility.Visible;
            }
            else
            {
                btnRestore.Visibility = Visibility.Collapsed;
            }
            _vm.IsAllKeysVisible = Visibility.Visible;
            _vm.ActiveModule = null;

            if (_vm.ConnectionType == "Wired")
                btnUnpair.Visibility = Visibility.Collapsed;

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
                        txtRestore.IsEnabled = !data.LockSettings.Lock_Keyboard_RestoreFactoryDefaults;
                        RestoreLockIcon.Visibility = data.LockSettings.Lock_Keyboard_RestoreFactoryDefaults ? Visibility.Visible : Visibility.Collapsed;
                        //Lock Functionality 9/7
                        //When a 1 or more settings are locked, automatically lock 'Restore to default'/'factory reset' control [Keyboard]
                        if (data.LockSettings != null &&
                            DdpmCommonHelper.GetUINotifyPropertyValue_isAnyLocked(data, "Lock_Keyboard"))
                        {
                            RestoreLockIcon.Visibility = Visibility.Visible;
                            txtRestore.IsEnabled = false;
                        }
                    }
                }
            }
            Unloaded += LaunchView_Unloaded;
            Loaded += LaunchView_Loaded;
            //if (DdpmCommonHelper.DeviceManagerSA != null)
            //{
            //    DdpmCommonHelper.DeviceManagerSA.StartCopilotRegistryMonitor();
            //    DdpmCommonHelper.DeviceManagerSA.DeviceChanged += DeviceManagerSA_DeviceChanged;
            //}
            DdpmCommonHelper.WriteUILog($"Keyboard UI LaunchView End timestamp: {DateTime.Now:hh:mm:ss.ffffff}");
        }

        private void LaunchView_Loaded(object sender, RoutedEventArgs e)
        {
            DdpmCommonHelper.WriteUILog($"Keyboard UI Loaded timestamp: {DateTime.Now:hh:mm:ss.ffffff}");
            if (DdpmCommonHelper.DeviceManagerSA != null)
            {
                DdpmCommonHelper.DeviceManagerSA.StartCopilotRegistryMonitor();
                DdpmCommonHelper.DeviceManagerSA.DeviceChanged += DeviceManagerSA_DeviceChanged;
            }
        }

        private void DeviceManagerSA_DeviceChanged(object? sender, SA.Common.DeviceChangedEventArgs e)
        {
            try
            {
                if (_vm != null && e.type == DeviceChangedType.Peripherals_SettingsChange)
                {
                    switch (e.changedProperty)
                    {
                        case "RestoreToDefault":
                            if (e.device_peripherals?.ModelNumber == _vm.Model)
                                Dispatcher.Invoke(new Action(() =>
                                {
                                    btnRestore.Visibility = Visibility.Collapsed;
                                }));
                            break;
                        case "CopilotEnableChanged":
                            if (e.device_peripherals != null)
                            {
                                _vm.IsCopilotEnabled = e.device_peripherals.Message != "false";
                                if (!_vm.IsCopilotEnabled)
                                    _vm.RemoveCopilotAction();
                                Dispatcher.Invoke(new Action(() =>
                                {
                                    if (_vm.VbarSelectedIndex == 0)
                                        _vm.ActiveModule!.OnActivated();
                                }));
                            }
                            break;
                        default:
                            return;
                    }
                }
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.KeyboardPlugin\\Views\\LaunchView.xaml.cs  DeviceManagerSA_DeviceChanged ex:" + ex.Message);
            }
        }

        private void LaunchView_Unloaded(object sender, RoutedEventArgs e)
        {
            if (DdpmCommonHelper.DeviceManagerSA != null)
            {
                DdpmCommonHelper.DeviceManagerSA.StopCopilotRegistryMonitor();
                DdpmCommonHelper.DeviceManagerSA.DeviceChanged -= DeviceManagerSA_DeviceChanged;
            }

            moduleGroup?.Dispose();
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
            var rst = DdpmCommonHelper.ApplyRestoreFactoryDefaultsEventData(e, "Lock_Keyboard_RestoreFactoryDefaults");
            Dispatcher.Invoke(new Action(() =>
            {
                RestoreLockIcon.Visibility = rst.isLocked;
                txtRestore.IsEnabled = rst.isEnabled;
                //Lock Functionality 9/7
                //When a 1 or more settings are locked, automatically lock 'Restore to default'/'factory reset' control [Display]
                DDPMSettings data = DdpmCommonHelper.DeviceManagerSA.ReloadAppConfigData().Result;
                if (data != null &&
                    data.LockSettings != null &&
                    DdpmCommonHelper.GetUINotifyPropertyValue_isAnyLocked(data, "Lock_Keyboard"))
                {
                    RestoreLockIcon.Visibility = Visibility.Visible;
                    txtRestore.IsEnabled = false;
                }
            }));
        }

        #region Init for Modules

        private void BuildModuleGroups()
        {
            if (_vm == null)
                return;

            List<ModuleGroup> groups = new();
            moduleGroup = new ModuleGroup()
            {
                GroupName = Strings.KeyCustomizationCaption,
                GroupIcon = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Vbar.Keyboard.Key.png"),
                GroupIconCanvas = DdpmCommonHelper.CanvasIconCreator(VbarIcon.KeyboardKeyCustom)
            };
            moduleGroup.AddHeader(Strings.KeyCustomizationCaption, new KeyCustomizationModule(_vm));
            groups.Add(moduleGroup);

            if (_vm.IsCollabsKeysSupported)
            {
                moduleGroup = new ModuleGroup()
                {
                    GroupName = Strings.CollaborationCaption,
                    GroupIcon = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Vbar.Keyboard.Collaboration.png"),
                    GroupIconCanvas = DdpmCommonHelper.CanvasIconCreator(VbarIcon.KeyboardCollaboration)
                };
                moduleGroup.AddHeader(Strings.CollaborationCaption, new CollaborationModule(_vm));
                groups.Add(moduleGroup);
            }
            if (_vm.IsIlluminationSupported)
            {
                moduleGroup = new ModuleGroup()
                {
                    GroupName = Strings.IlluminationCaption,
                    GroupIcon = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Vbar.Keyboard.Illumination.png"),
                    GroupIconCanvas = DdpmCommonHelper.CanvasIconCreator(VbarIcon.KeyboardIllumination)
                };
                moduleGroup.AddHeader(Strings.IlluminationCaption, new IlluminationModule(_vm));
                groups.Add(moduleGroup);
            }

            _vm.ModuleGroups = groups;
        }

        #endregion Init for Modules

        #region Vbar

        private void OnVbarItemClicked(VbarItem1 newItem)
        {
            if (_vm == null)
            { return; }

            if (_rightFrameWidth[newItem.Id + 1] != _rightFrameWidth[_vm.VbarSelectedIndex + 1])
            {
                _vm.RightFrameWidthFrom = _rightFrameWidth[_vm.VbarSelectedIndex + 1];
                _vm.RightFrameWidthTo = _rightFrameWidth[newItem.Id + 1];

                InvokeGotoTwoViewModeAnimation();

                // << 20250225 updated by Hess for PIMS-340217
                //if (newItem.Id == 0)
                //{
                //    if (_vm.VbarSelectedIndex != -1)
                //        InvokeEnlargeAnimation();
                //    _vm.ActiveModule?.OnActivated();
                //}
                //else
                //{
                //    InvokeShrinkAnimation();
                //}
                // >>
            }

            _vm.VbarSelectedIndex = newItem.Id;

            if (_vm.RightViewHeaders != null)
            {
                rightViewHeaderCtrl.SetHeaders(_vm.RightViewHeaders.ToArray());
            }
            btnUnpair.Visibility = Visibility.Collapsed;
            btnRestore.Visibility = Visibility.Collapsed;
            _vm.SetLadningMode(false);
            _vm.SelectVBar();

            if (_vm.VbarSelectedIndex > 0)
            { _vm.IsAllKeysVisible = Visibility.Hidden; }
            else
            { _vm.IsAllKeysVisible = Visibility.Visible; }
        }

        #endregion Vbar

        #region RightViewHeader

        private void RightViewHeaderCtrl_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            //int newSelId = rightViewHeaderCtrl.SelectedIndex;
            //if (newSelId != displaySettingsSelIdx) {
            //  if (_vm != null) {
            //    _vm.RightViewHeaderSelectedIndex = newSelId;
            //  }
            //  displaySettingsSelIdx = newSelId;
            //  SwitchLeftRightView();
            //}
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

        //private void InvokeShrinkAnimation()
        //{
        //    Dispatcher.Invoke(new Action(() =>
        //    {
        //        Storyboard sb = (Storyboard)this.FindResource("StoryShrink");
        //        if (sb != null)
        //        {
        //            sb.Completed += (o, s) =>
        //            {
        //            };

        //            sb.Begin();
        //        }
        //    }));
        //}

        //private void InvokeEnlargeAnimation()
        //{
        //    Dispatcher.Invoke(new Action(() =>
        //    {
        //        Storyboard sb = (Storyboard)this.FindResource("StoryEnlarge");
        //        if (sb != null)
        //        {
        //            sb.Completed += (o, s) =>
        //            {
        //            };

        //            sb.Begin();
        //        }
        //    }));
        //}

        #endregion Mode Change

        private void Unpair_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (_vm == null)
                return;

            if (_vm.ConnectionType == "Dongle")
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
            if (_vm == null)
                return;

            if (_vm.VbarSelectedIndex == -1)
            { return; }

            _vm.RightFrameWidthTo = 0;
            _vm.RightFrameWidthFrom = _rightFrameWidth[_vm.VbarSelectedIndex + 1];
            InvokeGotoTwoViewModeAnimation();
            if (_vm.ConnectionType != "Wired")
                btnUnpair.Visibility = Visibility.Visible;

            if (_vm.IsRestoreEnable)
            {
                btnRestore.Visibility = Visibility.Visible;
            }
            else
            {
                btnRestore.Visibility = Visibility.Collapsed;
            }

            // << 20250225 updated by Hess for PIMS-340217
            //if (_vm.VbarSelectedIndex > 0)
            //{ InvokeEnlargeAnimation(); }
            // >>

            _vm.VbarSelectedIndex = -1;
            _vm.SetLadningMode(true);
            _vm.SelectVBar();
            _vm.ClearSelectedKey();
            _vm.ActiveModule = null;
            _vm.IsAllKeysVisible = Visibility.Visible;
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
                _vm?.RestoreToDefault();
                ((Border)sender).Visibility = Visibility.Collapsed;
            }
        }

        private void BatteryIndicator_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (_vm == null || _vm.CurrentDeviceInfo == null)
                return;

            if (_vm.ConnectionType == "Dongle")
            {
                txtFirmware.Text = $"{Strings.ReceiverFirmwareVersion} {_vm.PhysicalDeviceFWVersion}";
                txtSlot.Text = $"{_vm.CurrentDeviceInfo.MaxPairingSlots - _vm.CurrentDeviceInfo.PairedDeviceCount} of {_vm.CurrentDeviceInfo.MaxPairingSlots} slots available";
                DongleConnection.Visibility = Visibility.Visible;
            }
            else if (_vm.ConnectionType == "Bluetooth")
            {
                SetBLConnectionStatus();
                BLConnection.Visibility = Visibility.Visible;
            }
        }

        private void SetBLConnectionStatus()
        {
            if (_vm == null)
                return;

            string hostName = HostNameHandler.GetHostName();

            txt1.Style = ConnectionStyle2;
            txtBLHost1.Style = ConnectionStyle2;
            txt2.Style = ConnectionStyle2;
            txtBLHost2.Style = ConnectionStyle2;
            txt3.Style = ConnectionStyle2;
            txtBLHost3.Style = ConnectionStyle2;

            _vm.ImgBL1 = false;
            _vm.ImgBL2 = false;
            _vm.ImgBL3 = false;

            switch (_vm.Model)
            {
                case "KB700":
                case "KB740":
                case "KB7120W":
                case "KB7221W":
                    Host1.Visibility = Visibility.Collapsed;
                    txtBLHost2.Text = string.IsNullOrEmpty(_vm.PairedHostName2) ? Strings.ReadyToBePaired : _vm.PairedHostName2;
                    txtBLHost3.Text = string.IsNullOrEmpty(_vm.PairedHostName3) ? Strings.ReadyToBePaired : _vm.PairedHostName3;
                    if (txtBLHost2.Text.Equals(hostName, StringComparison.CurrentCultureIgnoreCase))
                    {
                        txt2.Style = ConnectionStyle1;
                        txtBLHost2.Style = ConnectionStyle1;
                        _vm.ImgBL2 = true;
                    }
                    else
                    {
                        txt3.Style = ConnectionStyle1;
                        txtBLHost3.Style = ConnectionStyle1;
                        _vm.ImgBL3 = true;
                    }
                    break;

                case "KB900":
                    Host3.Visibility = Visibility.Collapsed;
                    txtBLHost1.Text = string.IsNullOrEmpty(_vm.PairedHostName2) ? Strings.ReadyToBePaired : _vm.PairedHostName2;
                    txtBLHost2.Text = string.IsNullOrEmpty(_vm.PairedHostName3) ? Strings.ReadyToBePaired : _vm.PairedHostName3;
                    if (txtBLHost1.Text.Equals(hostName, StringComparison.CurrentCultureIgnoreCase))
                    {
                        txt1.Style = ConnectionStyle1;
                        txtBLHost1.Style = ConnectionStyle1;
                        _vm.ImgBL1 = true;
                    }
                    else
                    {
                        txt2.Style = ConnectionStyle1;
                        txtBLHost2.Style = ConnectionStyle1;
                        _vm.ImgBL2 = true;
                    }
                    break;

                default:
                    Host1.Visibility = Visibility.Collapsed;
                    Host3.Visibility = Visibility.Collapsed;
                    txt2.Style = ConnectionStyle1;
                    txtBLHost2.Text = hostName;
                    txtBLHost2.Style = ConnectionStyle1;
                    _vm.ImgBL2 = true;
                    break;
            }
            if (txtBLHost1.Text.Length > 15 && txtBLHost1.Text != Strings.ReadyToBePaired)
                txtBLHost1.Text = txtBLHost1.Text.Substring(0, 15);
            if (txtBLHost2.Text.Length > 15 && txtBLHost2.Text != Strings.ReadyToBePaired)
                txtBLHost2.Text = txtBLHost2.Text.Substring(0, 15);
            if (txtBLHost3.Text.Length > 15 && txtBLHost3.Text != Strings.ReadyToBePaired)
                txtBLHost3.Text = txtBLHost3.Text.Substring(0, 15);
        }

        private void BatteryIndicator_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            DongleConnection.Visibility = Visibility.Collapsed;
            BLConnection.Visibility = Visibility.Collapsed;
        }

        private void InitializeKeyImage()
        {
            if (_vm == null)
                return;

            switch (_vm.Model.ToUpper())
            {
                case "KB700":
                case "KB7221W":
                    SectionA.Margin = new Thickness(89.5, 156, 0, 0);
                    this.Resources["F1Width"] = 44.1;
                    this.Resources["F1Height"] = 32.5;
                    this.Resources["SectionAMargin"] = new Thickness(-2.6, 0, 0, 0);
                    this.Resources["F5Margin"] = new Thickness(0.2, 0, 0, 0);
                    this.Resources["F9Margin"] = new Thickness(0.1, 0, 0, 0);
                    RecT.Height = 145;
                    RecB.Height = 145;
                    RecL.Width = 20;
                    RecR.Width = 20;
                    break;

                case "KB500":
                case "KB3121W":
                    SectionA.Margin = new Thickness(80.5, 151, 0, 0);
                    this.Resources["F1Width"] = 44.1;
                    this.Resources["F1Height"] = 32.5;
                    this.Resources["SectionAMargin"] = new Thickness(-1, 0, 0, 0);
                    this.Resources["F5Margin"] = new Thickness(0, 0, 0, 0);
                    this.Resources["F9Margin"] = new Thickness(1, 0, 0, 0);
                    RecT.Height = 139;
                    RecB.Height = 148;
                    RecL.Width = 11;
                    RecR.Width = 9;
                    break;

                case "KB7120W":
                case "KB740":
                    SectionA.Margin = new Thickness(78, 145, 0, 0);
                    this.Resources["F1Width"] = 45.5;
                    this.Resources["F1Height"] = 37.0;
                    this.Resources["SectionAMargin"] = new Thickness(-2, 0, 0, 0);
                    this.Resources["F5Margin"] = new Thickness(-3, 0, 0, 0);
                    this.Resources["F9Margin"] = new Thickness(-3, 0, 0, 0);
                    RecT.Height = 129;
                    RecB.Height = 118;
                    RecL.Width = 19;
                    RecR.Width = 15;
                    break;

                case "KB555":
                    SectionA.Margin = new Thickness(75, 146, 0, 0);
                    this.Resources["F1Width"] = 42.9;
                    this.Resources["F1Height"] = 32.5;
                    this.Resources["SectionAMargin"] = new Thickness(-2, 0, 0, 0);
                    this.Resources["F5Margin"] = new Thickness(-3, 0, 0, 0);
                    this.Resources["F9Margin"] = new Thickness(-2.6, 0, 0, 0);
                    this.Resources["PrtScWidth"] = 42.9;
                    this.Resources["PrtScHeight"] = 32.5;
                    this.Resources["PrtScMargin"] = new Thickness(88, 0, 0, 0);
                    this.Resources["SectionBMargin"] = new Thickness(0, 0, 0, 0);
                    this.Resources["CalculatorWidth"] = 43.0;
                    this.Resources["CalculatorHeight"] = 32.5;
                    this.Resources["CalculatorMargin"] = new Thickness(-117, 0, 0, 0);
                    RecT.Height = 133;
                    RecB.Height = 131;
                    RecL.Width = 25;
                    RecR.Width = 15;
                    break;

                case "KB525C":
                    SectionA.Margin = new Thickness(80, 168, 0, 0);
                    this.Resources["F1Width"] = 44.0;
                    this.Resources["F1Height"] = 32.5;
                    this.Resources["SectionAMargin"] = new Thickness(-1.5, 0, 0, 0);
                    this.Resources["F5Margin"] = new Thickness(2, 0, 0, 0);
                    this.Resources["F9Margin"] = new Thickness(2, 0, 0, 0);
                    this.Resources["PrtScWidth"] = 41.5;
                    this.Resources["PrtScHeight"] = 32.5;
                    this.Resources["PrtScMargin"] = new Thickness(2, 0, 0, 0);
                    this.Resources["SectionBMargin"] = new Thickness(-3, 0, 0, 0);
                    RecT.Height = 155;
                    RecB.Height = 128;
                    RecL.Width = 10;
                    RecR.Width = 10;
                    break;

                case "KB900":
                    SectionA.Margin = new Thickness(93, 158, 0, 0);
                    this.Resources["F1Width"] = 43.0;
                    this.Resources["F1Height"] = 32.5;
                    this.Resources["SectionAMargin"] = new Thickness(-2.5, 0, 0, 0);
                    this.Resources["F5Margin"] = new Thickness(0.8, 0, 0, 0);
                    this.Resources["F9Margin"] = new Thickness(1, 0, 0, 0);
                    this.Resources["PrtScWidth"] = 40.0;
                    this.Resources["PrtScHeight"] = 32.5;
                    this.Resources["PrtScMargin"] = new Thickness(5.8, 0, 0, 0);
                    this.Resources["SectionBMargin"] = new Thickness(-3, 0, 0, 0);
                    this.Resources["CalculatorWidth"] = 40.0;
                    this.Resources["CalculatorHeight"] = 32.5;
                    this.Resources["CalculatorMargin"] = new Thickness(7, 0, 0, 0);
                    RecT.Height = 148;
                    RecB.Height = 148;
                    RecL.Width = 25;
                    RecR.Width = 25;
                    break;
            }
        }

        private void KeyHoverIn(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var keyName = ((Image)sender).Name;
            _vm?.RefreshKeyImageFile(keyName, true, keyName == _vm.SelectedKey);
        }

        private void KeyHoverOut(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var keyName = ((Image)sender).Name;
            _vm?.RefreshKeyImageFile(keyName, false, keyName == _vm.SelectedKey);
        }

        private void KeyClicked(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (_vm == null)
                return;

            var keyName = ((Image)sender).Name;
            if (_vm.SelectedKey != "")
                _vm.RefreshKeyImageFile(_vm.SelectedKey);

            _vm.SelectedKey = keyName;
            _vm.RefreshKeyImageFile(keyName, false, true);

            if (_vm.VbarSelectedIndex == 0)
            {
                _vm.ActiveModule!.OnActivated();
            }
            else
            {
                OnVbarItemClicked(_vm.VbarItems[0]);
            }
            e.Handled = true;
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //ChangeDevNameWidth();
        }

        private void ChangeDevNameWidth()
        {
            //devName.Width = this.ActualWidth - RightGrid.ActualWidth - VbarGrid.ActualWidth - 100;
        }

        private void RightFrame_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //ChangeDevNameWidth();
        }

        private void PushBack(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is Border)
            {
                Mainframe_MouseLeftButtonDown(this, e);
            }
        }

        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }
    }
}