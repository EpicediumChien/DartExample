using CommunityToolkit.Mvvm.Input;
using DDPM.SA.Common;
using DDPM.SA.Common.Settings;
using DDPM.UI.Common;
using DDPM.UI.Common.UserControls;
using DDPM.UI.Interfaces;
using DDPM.UI.Module.ButtonSettings;
using DDPM.UI.Module.MouseSettings;
using DDPM.UI.Plugin.Common;
using DDPM.UI.Plugin.ViewModels;
using Dell.Client.Framework.UX.WPF.Controls;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Windows.Devices.Power;

namespace DDPM.UI.Plugin.MousePlugin
{
    /// <summary>
    /// MousePlugin.xaml 的互動邏輯
    /// </summary>
    public partial class LaunchView : UserControl
    {
        private readonly MouseViewModel? _vm;
        private readonly int[] _rightFrameWidth = { 0, 530, 330 };
        private readonly Style ConnectionStyle1 = new();
        private readonly Style ConnectionStyle2 = new();
        private readonly BitmapImage img1 = new(new Uri($"/DDPM.UI.Resources;component/Resources/Images/Bluetooth.png", UriKind.Relative));
        private readonly BitmapImage img2 = new(new Uri($"/DDPM.UI.Resources;component/Resources/Images/Bluetooth2.png", UriKind.Relative));

        private SolidColorBrush buttonColorFocusedT = new(System.Windows.Media.Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF));
        private SolidColorBrush buttonColorFocusedF = new(System.Windows.Media.Color.FromArgb(0x80, 0xFF, 0xFF, 0xFF));

        private double leftBorderDefaultWidth = 0.0;
        private double capAreaDefaultWidth = 0.0;

        public LaunchView()
        {
            DdpmCommonHelper.WriteUILog($"Mouse UI LaunchView Begin timestamp: {DateTime.Now:hh:mm:ss.ffffff}");
            try
            {
                Loaded += LaunchView_Loaded;
                InitializeComponent();
                _vm = (MouseViewModel?)Mouseplugin.PluginIoc?.GetService<IPeripheralViewModel>()!;
                if (_vm == null)
                {
                    DdpmCommonHelper.WriteUILog("Mouse ViewModel is null");
                    return;
                }

                // for Light mode check by leo
                if (UXSystemParameters.Instance.OSTheme == OSThemeEnum.Light)
                {
                    buttonColorFocusedT = new(System.Windows.Media.Color.FromArgb(0xFF, 0x0, 0x0, 0x0));
                    buttonColorFocusedF = new(System.Windows.Media.Color.FromArgb(0x80, 0x0, 0x0, 0x0));
                }
                UXSystemParameters.Instance.ParameterChangedEvent += MSUXSystemParametersChanged;


                DataContext = _vm;
                _vm.VbarItemClickCommand = new RelayCommand<VbarItem1>(OnVbarItemClicked!);

                if (_vm.EOLMouseList.Contains(_vm.Model))
                {
                    //Battery.Visibility = Visibility.Collapsed;
                    //btnRestore.Visibility = Visibility.Collapsed;
                    //txtEOL.Text = Strings.EOLMessage;
                    txtEOL.Visibility = Visibility.Visible;
                    SectionA.Visibility = Visibility.Collapsed;
                    SectionB.Visibility = Visibility.Collapsed;
                    _vm.IsBatteryUnavailable = true;
                    //EOLDongle.Visibility = Visibility.Visible;
                }
                else
                {
                    _vm.IsBatteryUnavailable = false;
                }
                BuildModuleGroups();

                //txtUnpair.Text = Strings.Unpair;
                //txtRestore.Text = Strings.RestoreToDefault;
                ConnectionStyle1 = (Style)FindResource("ConnectionStyle1");
                ConnectionStyle2 = (Style)FindResource("ConnectionStyle2");
                //txtDongleHost.Text = Strings.USBWirelessReceiver;

                //txtActionCaption.Text = ActionCaption;
                // txtAllApp.Text = AllAppCaption;
                SetAppFocus();
                // txtWord.Text = WordCaption;
                //txtExcel.Text = ExcelCaption;
                //txtPowerPoint.Text = PowerPointCaption;
                //txtOutlook.Text = OutlookCaption;

                InitializeButtonImage();
                //if(_vm.IsRestoreEnable) {
                //  btnRestore.Visibility = Visibility.Visible;
                //}
                //else {
                //  btnRestore.Visibility = Visibility.Collapsed;
                //}
                _vm.IsAllButtonsVisible = Visibility.Visible;
                _vm.ActiveModule = null;

                if (_vm.ConnectionType == "Wired")
                    btnUnpair.Visibility = Visibility.Collapsed;

                //lock/unlock, no ui element currently
                if (DdpmCommonHelper.DeviceManagerSA != null)
                {
                    DdpmCommonHelper.DeviceManagerSA.ITSettingsActionEvent += DeviceManagerSA_ITSettingsActionEvent;


                    DDPMSettings data = DdpmCommonHelper.DeviceManagerSA.ReloadAppConfigData().Result;
                    if (data != null)
                    {
                        if (data.LockSettings.Lock_Setting_RestoreDefaults)
                        {
                            //RestoreLockIcon.Visibility = Visibility.Visible;
                            //txtRestore.IsEnabled = false;
                        }
                        else
                        {
                            //txtRestore.IsEnabled = !data.LockSettings.Lock_Mouse_RestoreFactoryDefaults;
                            //RestoreLockIcon.Visibility = data.LockSettings.Lock_Mouse_RestoreFactoryDefaults ? Visibility.Visible : Visibility.Collapsed;
                        }
                        //Lock Functionality 9/7
                        //When a 1 or more settings are locked, automatically lock 'Restore to default'/'factory reset' control [Mouse]
                        if (data.LockSettings != null &&
                            DdpmCommonHelper.GetUINotifyPropertyValue_isAnyLocked(data, "Lock_Mouse"))
                        {
                            //RestoreLockIcon.Visibility = Visibility.Visible;
                            //txtRestore.IsEnabled = false;
                        }
                    }
                }
                Unloaded += LaunchView_Unloaded;
                //if (DdpmCommonHelper.DeviceManagerSA != null)
                //{
                //    DdpmCommonHelper.DeviceManagerSA.StartCopilotRegistryMonitor();
                //    DdpmCommonHelper.DeviceManagerSA.DeviceChanged += DeviceManagerSA_DeviceChanged;
                //}
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.MousePlugin\\Views\\LaunchView.xaml.cs  LaunchView() ex:" + ex.Message);
            }
            leftBorderDefaultWidth = 1045;
            capAreaDefaultWidth = AppCaptionArea.Width;
            LeftBorder.SizeChanged -= CapAreaSizeChange;
            LeftBorder.SizeChanged += CapAreaSizeChange;
            DdpmCommonHelper.WriteUILog($"Mouse UI LaunchView End timestamp: {DateTime.Now:hh:mm:ss.ffffff}");
        }

        private void LaunchView_Loaded(object sender, RoutedEventArgs e)
        {
            DdpmCommonHelper.WriteUILog($"Mouse UI Loaded timestamp: {DateTime.Now:hh:mm:ss.ffffff}");
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
                        case "CopilotEnableChanged":
                            if (e.device_peripherals != null)
                            {
                                _vm.IsCopilotEnabled = e.device_peripherals.Message != "false";
                                if (!_vm.IsCopilotEnabled)
                                    _vm.RemoveCopilotAction();
                                Dispatcher.Invoke(new Action(() =>
                                {
                                    if (_vm.VbarSelectedIndex == 1)
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
                DdpmCommonHelper.WriteUILog("DDPM.UI.MousePlugin\\Views\\LaunchView.xaml.cs  DeviceManagerSA_DeviceChanged ex:" + ex.Message);
            }
        }

        private void LaunchView_Unloaded(object sender, RoutedEventArgs e)
        {
            if (DdpmCommonHelper.DeviceManagerSA != null)
            {
                DdpmCommonHelper.DeviceManagerSA.StopCopilotRegistryMonitor();
                DdpmCommonHelper.DeviceManagerSA.DeviceChanged -= DeviceManagerSA_DeviceChanged;
            }
        }

        private void MSUXSystemParametersChanged(object? sender, PropertyChangedEventArgs e)
        {

            this.Dispatcher.Invoke(
                          DispatcherPriority.Normal,
                          (System.Windows.Forms.MethodInvoker)delegate ()
                          {
                              if (UXSystemParameters.Instance.OSTheme == OSThemeEnum.Light)
                              {
                                  buttonColorFocusedT = new(System.Windows.Media.Color.FromArgb(0xFF, 0x0, 0x0, 0x0));
                                  buttonColorFocusedF = new(System.Windows.Media.Color.FromArgb(0x80, 0x0, 0x0, 0x0));
                              }
                              else
                              {
                                  buttonColorFocusedT = new(System.Windows.Media.Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF));
                                  buttonColorFocusedF = new(System.Windows.Media.Color.FromArgb(0x80, 0xFF, 0xFF, 0xFF));
                              }
                              SetAppFocus();
                              // Your code here 
                          });
        }

        ~LaunchView()
        {
            if (DdpmCommonHelper.DeviceManagerSA != null)
            {
                DdpmCommonHelper.DeviceManagerSA.ITSettingsActionEvent -= DeviceManagerSA_ITSettingsActionEvent;
            }
            LeftBorder.SizeChanged -= CapAreaSizeChange;
        }

        private void DeviceManagerSA_ITSettingsActionEvent(object? sender, SA.Common.ITSettingEventArgs e)
        {
            try
            {
                var rst = DdpmCommonHelper.ApplyRestoreFactoryDefaultsEventData(e, "Lock_Mouse_RestoreFactoryDefaults");
                Dispatcher.Invoke(new Action(() =>
                {
                    //no ui element currently
                    //RestoreLockIcon.Visibility = rst.isLocked;
                    //txtRestore.IsEnabled = rst.isEnabled;

                    //Lock Functionality 9/7
                    //When a 1 or more settings are locked, automatically lock 'Restore to default'/'factory reset' control [Mouse]
                    DDPMSettings data = DdpmCommonHelper.DeviceManagerSA.ReloadAppConfigData().Result;
                    if (data != null && data.LockSettings != null &&
                    DdpmCommonHelper.GetUINotifyPropertyValue_isAnyLocked(data, "Lock_Mouse"))
                    {
                        //RestoreLockIcon.Visibility = Visibility.Visible;
                        //txtRestore.IsEnabled = false;
                    }
                }));
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.MousePlugin\\Views\\LaunchView.xaml.cs  DeviceManagerSA_ITSettingsActionEvent ex:" + ex.Message);
            }
        }

        #region Init for Modules

        /// <summary>
        /// Base on specified monitor's capabiliies to build the Vbar items, and headers/modules
        /// </summary>
        private void BuildModuleGroups()
        {
            if (_vm == null)
                return;

            List<ModuleGroup> groups = new();
            ModuleGroup moduleGroup;

            moduleGroup = new ModuleGroup()
            {
                GroupName = Strings.MouseSettingsCaption,
                GroupIcon = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Vbar.Mouse.Cursor.png"),
                GroupIconCanvas = DdpmCommonHelper.CanvasIconCreator(VbarIcon.MouseSettings)
            };
            moduleGroup.AddHeader(Strings.MouseSettingsCaption, new MouseSettingsModule(_vm));
            groups.Add(moduleGroup);

            if (_vm.Model != "MS700" && !_vm.EOLMouseList.Contains(_vm.Model))
            {
                moduleGroup = new ModuleGroup()
                {
                    GroupName = Strings.ButtonCustomizationCaption,
                    GroupIcon = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Vbar.Mouse.Mouse.png"),
                    GroupIconCanvas = DdpmCommonHelper.CanvasIconCreator(VbarIcon.MouseButton)
                };
                moduleGroup.AddHeader(Strings.ButtonCustomizationCaption, new ButtonSettingsModule(_vm));
                groups.Add(moduleGroup);
            }

            _vm.ModuleGroups = groups;
        }

        #endregion Init for Modules

        #region Vbar

        private void OnVbarItemClicked(VbarItem1 newItem)
        {
            if (_vm == null || newItem.Id == _vm.VbarSelectedIndex)
            { return; }

            if (_rightFrameWidth[newItem.Id + 1] != _rightFrameWidth[_vm.VbarSelectedIndex + 1])
            {
                _vm.RightFrameWidthFrom = _rightFrameWidth[_vm.VbarSelectedIndex + 1];
                _vm.RightFrameWidthTo = _rightFrameWidth[newItem.Id + 1];

                InvokeGotoTwoViewModeAnimation();
            }

            if (newItem.Id == 1)
            {
                AppCaptionArea.Visibility = Visibility.Visible;
                SetAppFocus();
                _vm.ActiveModule?.OnActivated();
            }
            else
            {
                AppCaptionArea.Visibility = Visibility.Collapsed;
            }

            _vm.VbarSelectedIndex = newItem.Id;

            if (_vm.RightViewHeaders != null)
            {
                rightViewHeaderCtrl.SetHeaders(_vm.RightViewHeaders.ToArray());
            }
            btnUnpair.Visibility = Visibility.Collapsed;
            //btnRestore.Visibility = Visibility.Collapsed;
            _vm.SetLadningMode(false);
            _vm.SelectVBar();

            if (_vm.VbarSelectedIndex == 0)
            { _vm.IsAllButtonsVisible = Visibility.Hidden; }
            else
            { _vm.IsAllButtonsVisible = Visibility.Visible; }
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

        #endregion Mode Change

        private void App_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                UXTextBlock button = (UXTextBlock)sender;
                var app = button.Name.Replace("txt", "");
                if (_vm.SelectedApp == app)
                { return; }

                _vm.SelectedApp = app;
                //_vm.OnPropertyChanged(nameof(_vm.SelectedApp));
                SetAppFocus();
                _vm.ActiveModule?.OnActivated();
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.MousePlugin\\Views\\LaunchView.xaml.cs  App_MouseLeftButtonDown ex:" + ex.Message);
            }
        }

        private void Unpair_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                if (_vm?.ConnectionType == "Dongle")
                {
                    UnpairModalDialog unpairModalDialog = new(eDeviceCategory.Mouse);
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
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.MousePlugin\\Views\\LaunchView.xaml.cs  Unpair_Click ex:" + ex.Message);
            }
        }

        private void Mainframe_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                if (_vm == null || _vm.VbarSelectedIndex == -1)
                { return; }

                _vm.RightFrameWidthTo = 0;
                _vm.RightFrameWidthFrom = _rightFrameWidth[_vm.VbarSelectedIndex + 1];
                InvokeGotoTwoViewModeAnimation();
                if (_vm.ConnectionType != "Wired")
                    btnUnpair.Visibility = Visibility.Visible;

                AppCaptionArea.Visibility = Visibility.Collapsed;
                //if(_vm.IsRestoreEnable) {
                //  btnRestore.Visibility = Visibility.Visible;
                //}
                //else {
                //  btnRestore.Visibility = Visibility.Collapsed;
                //}

                _vm.VbarSelectedIndex = -1;
                _vm.SetLadningMode(true);
                _vm.SelectVBar();
                _vm.ClearSelectedButton();
                SetAppFocus();
                _vm.IsAllButtonsVisible = Visibility.Visible;
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.MousePlugin\\Views\\LaunchView.xaml.cs  Mainframe_MouseLeftButtonDown ex:" + ex.Message);
            }
        }

        private void BatteryIndicator_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (_vm == null)
                return;
            try
            {
                if (_vm.ConnectionType == "Dongle")
                {
                    txtFirmware.Text = $"{Strings.ReceiverFirmwareVersion} {_vm.PhysicalDeviceFWVersion}";
                    txtSlot.Text = $"{(_vm.CurrentDeviceInfo?.MaxPairingSlots ?? 6) - (_vm.CurrentDeviceInfo?.PairedDeviceCount ?? 2)} of {_vm.CurrentDeviceInfo?.MaxPairingSlots ?? 6} slots available";
                    DongleConnection.Visibility = Visibility.Visible;
                }
                else if (_vm.ConnectionType == "Bluetooth")
                {
                    SetBLConnectionStatus();
                    BLConnection.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.MousePlugin\\Views\\LaunchView.xaml.cs  BatteryIndicator_MouseEnter ex:" + ex.Message);
            }
        }

        private void BatteryIndicator_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            DongleConnection.Visibility = Visibility.Collapsed;
            BLConnection.Visibility = Visibility.Collapsed;
        }

        private void SetBLConnectionStatus()
        {
            try
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
                    case "MS700":
                        txt3.Visibility = Visibility.Visible;
                        Host3.Visibility = Visibility.Visible;
                        txtBLHost1.Text = string.IsNullOrEmpty(_vm.PairedHostName1) ? Strings.ReadyToBePaired : _vm.PairedHostName1;
                        txtBLHost2.Text = string.IsNullOrEmpty(_vm.PairedHostName2) ? Strings.ReadyToBePaired : _vm.PairedHostName2;
                        txtBLHost3.Text = string.IsNullOrEmpty(_vm.PairedHostName3) ? Strings.ReadyToBePaired : _vm.PairedHostName3;
                        if (txtBLHost1.Text.Equals(hostName, StringComparison.CurrentCultureIgnoreCase))
                        {
                            txt1.Style = ConnectionStyle1;
                            txtBLHost1.Style = ConnectionStyle1;
                            _vm.ImgBL1 = true;
                        }
                        else if (txtBLHost2.Text.Equals(hostName, StringComparison.CurrentCultureIgnoreCase))
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

                    case "MS5320W":
                    case "MS7421W":
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

                    case "MS900":
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
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.MousePlugin\\Views\\LaunchView.xaml.cs  SetBLConnectionStatus ex:" + ex.Message);
            }
        }

        private void Restore_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
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
                    _vm?.RestoreToDefault();
                    ((Border)sender).Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.MousePlugin\\Views\\LaunchView.xaml.cs  SetBLConnectionStatus ex:" + ex.Message);
            }
        }

        private void InitializeButtonImage()
        {
            try
            {
                switch (_vm?.Model.ToUpper())
                {
                    case "MS300":
                        SectionA.Margin = new Thickness(210, 64, 0, 0);
                        RecT.Height = 21;
                        RecB.Height = 60;
                        RecL.Width = 136;
                        RecR.Width = 133;
                        break;

                    case "MS355":
                        SectionA.Margin = new Thickness(210, 91, 0, 0);
                        RecT.Height = 48;
                        RecB.Height = 85;
                        RecL.Width = 142;
                        RecR.Width = 140;
                        break;

                    case "MS7421W":
                        SectionA.Margin = new Thickness(210, 98, 0, 0);
                        SectionB.Margin = new Thickness(127, 170, 0, 0);
                        this.Resources["B4Margin"] = new Thickness(0, -8, 0, 0);
                        RecT.Height = 55;
                        RecB.Height = 73;
                        RecL.Width = 133;
                        RecR.Width = 135;
                        break;

                    case "MS3320W":
                        SectionA.Margin = new Thickness(210, 108, 0, 0);
                        RecT.Height = 62;
                        RecB.Height = 60;
                        RecL.Width = 140;
                        RecR.Width = 135;
                        break;

                    case "MS5120W":
                        SectionA.Margin = new Thickness(210, 108, 0, 0);
                        SectionB.Margin = new Thickness(127, 164, 0, 0);
                        this.Resources["B4Margin"] = new Thickness(0, -6, 0, 0);
                        RecT.Height = 62;
                        RecB.Height = 60;
                        RecL.Width = 137;
                        RecR.Width = 138;
                        break;

                    case "MS5320W":
                        SectionA.Margin = new Thickness(210, 81, 0, 0);
                        SectionB.Margin = new Thickness(126, 150, 0, 0);
                        this.Resources["B4Margin"] = new Thickness(0, -6, 0, 0);
                        RecT.Height = 33;
                        RecB.Height = 53;
                        RecL.Width = 110;
                        RecR.Width = 138;
                        break;

                    case "MS900":
                        SectionA.Margin = new Thickness(230, 50, 0, 0);
                        SectionB.Margin = new Thickness(156, 126, 0, 0);
                        this.Resources["B4Margin"] = new Thickness(0, -15, 0, 0);
                        RecT.Height = 13;
                        RecB.Height = 88;
                        RecL.Width = 104;
                        RecR.Width = 115;
                        break;

                    case "MS3220":
                    case "MS3220T":
                        SectionA.Margin = new Thickness(210, 106, 0, 0);
                        SectionB.Margin = new Thickness(140, 173, 0, 0);
                        this.Resources["B4Margin"] = new Thickness(0, -6, 0, 0);
                        RecT.Height = 72;
                        RecB.Height = 70;
                        RecL.Width = 152;
                        RecR.Width = 150;
                        break;

                    case "MS700":
                        SectionA.Visibility = Visibility.Collapsed;
                        RecT.Height = 21;
                        RecB.Height = 82;
                        RecL.Width = 142;
                        RecR.Width = 140;
                        break;
                    default:
                        RecT.Height = 50;
                        RecB.Height = 50;
                        RecL.Width = 130;
                        RecR.Width = 130;
                        break;
                }
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.MousePlugin\\Views\\LaunchView.xaml.cs  InitializeButtonImage ex:" + ex.Message);
            }
        }

        private void ButtonHoverIn(object sender, System.Windows.Input.MouseEventArgs e)
        {
            try
            {
                var btnName = ((Image)sender).Name;
                _vm?.RefreshButtonImageFile(btnName, true, btnName == _vm.SelectedButton);
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.MousePlugin\\Views\\LaunchView.xaml.cs  ButtonHoverIn ex:" + ex.Message);
            }
        }

        private void ButtonHoverOut(object sender, System.Windows.Input.MouseEventArgs e)
        {
            try
            {
                var btnName = ((Image)sender).Name;
                _vm?.RefreshButtonImageFile(btnName, false, btnName == _vm.SelectedButton);
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.MousePlugin\\Views\\LaunchView.xaml.cs ButtonHoverOut ex:" + ex.Message);
            }
        }

        private void ButtonClicked(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (_vm == null)
                return;

            try
            {
                if (_vm.SelectedButton != "")
                { _vm.RefreshButtonImageFile(_vm.SelectedButton); }

                var btnName = ((Image)sender).Name;
                _vm.SelectedButton = btnName;
                _vm.RefreshButtonImageFile(btnName, false, true);

                if (_vm.VbarSelectedIndex == 1)
                {
                    _vm.ActiveModule!.OnActivated();
                }
                else
                {
                    OnVbarItemClicked(_vm.VbarItems[1]);
                }
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.MousePlugin\\Views\\LaunchView.xaml.cs ButtonClicked ex:" + ex.Message);
            }
        }

        private void SetAppFocus()
        {
            if (_vm == null)
                return;

            try
            {
                bdrAllApp.Visibility = Visibility.Collapsed;
                bdrWord.Visibility = Visibility.Collapsed;
                bdrExcel.Visibility = Visibility.Collapsed;
                bdrPowerPoint.Visibility = Visibility.Collapsed;
                bdrOutlook.Visibility = Visibility.Collapsed;
                txtAllApp.Foreground = buttonColorFocusedF;
                txtWord.Foreground = buttonColorFocusedF;
                txtExcel.Foreground = buttonColorFocusedF;
                txtPowerPoint.Foreground = buttonColorFocusedF;
                txtOutlook.Foreground = buttonColorFocusedF;
                switch (_vm.SelectedApp)
                {
                    case "Word":
                        bdrWord.Visibility = Visibility.Visible;
                        txtWord.Foreground = buttonColorFocusedT;
                        break;

                    case "Excel":
                        bdrExcel.Visibility = Visibility.Visible;
                        txtExcel.Foreground = buttonColorFocusedT;
                        break;

                    case "PowerPoint":
                        bdrPowerPoint.Visibility = Visibility.Visible;
                        txtPowerPoint.Foreground = buttonColorFocusedT;
                        break;

                    case "Outlook":
                        bdrOutlook.Visibility = Visibility.Visible;
                        txtOutlook.Foreground = buttonColorFocusedT;
                        break;

                    default:
                        bdrAllApp.Visibility = Visibility.Visible;
                        txtAllApp.Foreground = buttonColorFocusedT;
                        break;
                }
                _vm.RefreshButtonInfo();
                _vm.CheckRestoreStatus();
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.MousePlugin\\Views\\LaunchView.xaml.cs SetAppFocus ex:" + ex.Message);
            }
        }

        private void App_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            try
            {
                UXTextBlock button = (UXTextBlock)sender;
                button.Foreground = buttonColorFocusedT;
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.MousePlugin\\Views\\LaunchView.xaml.cs SetAppFocus ex:" + ex.Message);
            }
        }

        private void App_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            try
            {
                UXTextBlock button = (UXTextBlock)sender;
                if (button.Name != $"txt{_vm?.SelectedApp}")
                {
                    button.Foreground = buttonColorFocusedF;
                }
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.MousePlugin\\Views\\LaunchView.xaml.cs App_MouseLeave ex:" + ex.Message);
            }
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
            try
            {
                if (sender is Border)
                {
                    Mainframe_MouseLeftButtonDown(this, e);
                }
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.MousePlugin\\Views\\LaunchView.xaml.cs PushBack ex:" + ex.Message);
            }
        }

        private void CapAreaSizeChange(object sender, SizeChangedEventArgs e)
        {
            if (LeftBorder != null && leftBorderDefaultWidth != 0 && capAreaDefaultWidth != 0)
            {
                double scalingFactor = LeftBorder.ActualWidth / leftBorderDefaultWidth;
                AppCaptionArea.LayoutTransform = new ScaleTransform(scalingFactor, scalingFactor);
            }
        }

        //Robert_Lin 2025-3-7 added to handle [Enter] key press event on backArrow image
        private void backArrow_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                e.Handled = true;
                // Ensure DataContext is of the correct type
                if (_vm != null)
                {
                    // Check if the command can be executed
                    if (_vm.GoBackClickedCommand.CanExecute(null))
                    {
                        // Execute the command
                        _vm.GoBackClickedCommand.Execute(null);
                    }
                }
            }
        }

        //Robert_Lin 2025-3-8 added for Narrator, press [Enter] key on SectionA,B images
        private void ButtonPreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            //Re-route the key event to ButtonClicked method with MouseLeftButtonDown event
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                e.Handled = true;
                var mouseDevice = InputManager.Current.PrimaryMouseDevice;
                if (mouseDevice != null)
                {
                    MouseButtonEventArgs mouseButtonEventArgs = new(mouseDevice, 0, MouseButton.Left)
                    {
                        RoutedEvent = UIElement.MouseLeftButtonDownEvent
                    };
                    ButtonClicked(sender, mouseButtonEventArgs);
                }
            }
        }
    }
}