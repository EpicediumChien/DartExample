using CommunityToolkit.Mvvm.Input;
using DDPM.UI.Common;
using DDPM.UI.Common.UserControls;
using DDPM.UI.Interfaces;
using DDPM.UI.Module.SpeakerAudioPreset;
using DDPM.UI.Module.SpeakerAudioSettings;
using DDPM.UI.Module.SpeakerInteractions;
using DDPM.UI.Plugin.Common;
using DDPM.UI.Plugin.ViewModels;
using Dell.Client.Framework.UX.WPF.Controls;
using System.Diagnostics;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace DDPM.UI.Plugin.SoundBarPlugin
{
    /// <summary>
    /// SoundBarPlugin.xaml 的互動邏輯
    /// </summary>
    public partial class LaunchView : UserControl
    {
        private readonly SoundBarViewModel? _vm;

        private readonly int[] _rightFrameWidth = new int[] { 0, 480, 480, 480 };
        //private readonly string Restore = "Restore to default";
        //private readonly string Unpair = "Unpair";
        private readonly string AudioPreset = Strings.SoundBarAudioPreset;
        private readonly string AudioSettings = Strings.SoundBarAudioSettings;
        private readonly string Interactions = Strings.SoundBarInteractions;

        private readonly Style ConnectionStyle1;
        private readonly Style ConnectionStyle2;
        private readonly BitmapImage img1 = new(new Uri($"/DDPM.UI.Resources;component/Resources/Images/Bluetooth.png", UriKind.Relative));
        private readonly BitmapImage img2 = new(new Uri($"/DDPM.UI.Resources;component/Resources/Images/Bluetooth2.png", UriKind.Relative));
        private ModuleGroup moduleGroup;

        public LaunchView()
        {
            DdpmCommonHelper.WriteUILog($"[SoundBar_LaunchView] LaunchView Constructor ... in ");
            _vm = (SoundBarViewModel?)SoundBarPlugin.PluginIoc?.GetService<IPeripheralViewModel>()!;
            if (_vm != null)
            {
                if (false)//(!_vm.IsDTPReady)
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
                    //txtUnpair.Text = Unpair;
                    //txtRestore.Text = Restore;
                    ConnectionStyle1 = (Style)FindResource("ConnectionStyle1");
                    ConnectionStyle2 = (Style)FindResource("ConnectionStyle2");
                    txtSystemName1.Text = _vm!.VisiblePairedHostName1;
                    txtSystemName2.Text = _vm.VisiblePairedHostName1;
                    txtSystemName3.Text = _vm.VisiblePairedHostName1;
                    txtFirmware.Text = string.Format(Strings.DockDongle1, _vm.PhysicalDeviceFWVersion);
                    txtSlot.Text = string.Format(Strings.DockDongle0, _vm.CurrentDeviceInfo!.MaxPairingSlots - _vm.CurrentDeviceInfo.PairedDeviceCount, _vm.CurrentDeviceInfo.MaxPairingSlots);
                    DdpmCommonHelper.BitmapImageUpdated += ImageUpdate;
                    Loaded += LaunchView_LoadedStatus;
                    Unloaded += LaunchView_UnLoadedStatus;
                }
            }
            DdpmCommonHelper.WriteUILog($"[SoundBar_LaunchView] LaunchView Constructor ... end ");
        }

        private void LaunchView_UnLoadedStatus(object sender, RoutedEventArgs e)
        {
            DdpmCommonHelper.WriteUILog($"[SoundBar_LaunchView] LaunchView_UnLoadedStatus ... in ");
            if (_vm == null)
            {
                DdpmCommonHelper.WriteUILog($"[SoundBar_LaunchView] ~LaunchView_UnLoadedStatus _vm is null");
                return;
            }
            try
            {
                _vm.UloadSpeaker_DTPNotify();
                if (DdpmCommonHelper.DeviceManagerSA != null)
                {
                    DdpmCommonHelper.BitmapImageUpdated -= ImageUpdate;
                    Loaded -= LaunchView_LoadedStatus;
                    Unloaded -= LaunchView_UnLoadedStatus;
                    DdpmCommonHelper.WriteUILog($"[SoundBar_LaunchView] -= ImageUpdate、LaunchView_LoadedStatus、LaunchView_UnLoadedStatus");
                }
                else
                {
                    DdpmCommonHelper.WriteUILog($"[SoundBar_LaunchView] ~LaunchView_UnLoadedStatus DeviceManagerSA is null");
                }
                moduleGroup?.Dispose();
                DdpmCommonHelper.WriteUILog($"[SoundBar_LaunchView] LaunchView_UnLoadedStatus ... out ");
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog($"[SoundBar_LaunchView] LaunchView_UnLoadedStatus Exception = {ex.Message}");
            }
        }

        private void LaunchView_LoadedStatus(object sender, RoutedEventArgs e)
        {
            DdpmCommonHelper.WriteUILog($"[SoundBar_LaunchView] LaunchView_LoadedStatus ... in ");
            if (_vm == null)
            {
                DdpmCommonHelper.WriteUILog($"[SoundBar_LaunchView] ~LaunchView_LoadedStatus _vm is null");
                return;
            }

            try
            {
                //await _vm.Invoke_PleaseWaitAsync(_vm.Model, _vm);
                if (!_vm.IsRestoreEnable)
                {
                    btnRestore.Visibility = Visibility.Visible;
                }
                else
                {
                    btnRestore.Visibility = Visibility.Collapsed;
                }
                DdpmCommonHelper.WriteUILog($"[SoundBar_LaunchView] LaunchView_LoadedStatus ... out ");
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog($"[SoundBar_LaunchView] LaunchView_LoadedStatus Exception = {ex.Message}");
            }
        }

        private void ImageUpdate(OSThemeEnum oSThemeEnum)
        {
            ArrowLeft.Source = null;
            ArrowLeft.Source = (BitmapImage)Application.Current.Resources["Arrow_Left"];
        }

        private void LaunchView_Loaded(object sender, RoutedEventArgs e)
        {
            DdpmCommonHelper.WriteUILog($"[SoundBar_LaunchView] LaunchView LaunchView_Loaded ... in ");
            if (!_vm!.IsDTPReady)
                DdpmCommonHelper.MyConsole!.ShowHomePage();

            DdpmCommonHelper.WriteUILog($"[SoundBar_LaunchView] LaunchView LaunchView_Loaded ... out ");
        }

        #region Init for Modules

        /// <summary>
        /// Base on specified monitor's capabiliies to build the Vbar items, and headers/modules
        /// </summary>
        private void BuildModuleGroups()
        {
            DdpmCommonHelper.WriteUILog($"[SoundBar_LaunchView] BuildModuleGroups ... in ");
            try
            {
                List<ModuleGroup> groups = new List<ModuleGroup>();

                moduleGroup = new ModuleGroup()
                {
                    GroupName = AudioPreset,
                    GroupIcon = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Speaker_AudioPreset.png"),
                    GroupIconCanvas = DdpmCommonHelper.CanvasIconCreator(VbarIcon.SpeakerPhonePreset)
                };
                moduleGroup.AddHeader(AudioPreset, new SpeakerAudioPresetModule(_vm!));
                groups.Add(moduleGroup);

                moduleGroup = new ModuleGroup()
                {
                    GroupName = AudioSettings,
                    GroupIcon = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Speaker_AudioSettings.png"),
                    GroupIconCanvas = DdpmCommonHelper.CanvasIconCreator(VbarIcon.AudioSettings)
                };
                moduleGroup.AddHeader(AudioSettings, new SpeakerAudioSettingsModule(_vm!));
                groups.Add(moduleGroup);

                moduleGroup = new ModuleGroup()
                {
                    GroupName = Interactions,
                    GroupIcon = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Speaker_Interactions.png"),
                    GroupIconCanvas = DdpmCommonHelper.CanvasIconCreator(VbarIcon.SpeakerPhoneInteractions)
                };
                moduleGroup.AddHeader(Interactions, new SpeakerInteractionsModule(_vm!));
                groups.Add(moduleGroup);

                _vm!.ModuleGroups = groups;
                DdpmCommonHelper.WriteUILog($"[SoundBar_LaunchView] BuildModuleGroups ... out ");
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog($"[SoundBar_LaunchView] BuildModuleGroups Exception = {ex.Message}");
            }
        }

        private void ArrowLeftImageUpdate(string resourceKey)
        {
            if (resourceKey == "Arrow_Left")
            {
                ArrowLeft.Source = null;
                ArrowLeft.Source = (BitmapImage)Application.Current.Resources[resourceKey];
            }
        }

        private void InitializeButtonImage()
        {
            try
            {
                switch (_vm!.Model.ToUpper())
                {
                    case "SP3022":
                        RecT.Height = 165;
                        RecB.Height = 170;
                        RecL.Width = 50;
                        RecR.Width = 50;
                        break;
                    case "SB522A":
                        RecT.Height = 220;
                        RecB.Height = 220;
                        RecL.Width = 30;
                        RecR.Width = 30;
                        break;
                    default:
                        RecT.Height = 165;
                        RecB.Height = 170;
                        RecL.Width = 50;
                        RecR.Width = 50;
                        break;
                }
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog($"[SoundBar_LaunchView] InitializeButtonImage Exception = {ex.Message}");
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
            DdpmCommonHelper.WriteUILog($"[SoundBar_LaunchView] OnVbarItemClicked ... in ");
            try
            {
                if (newItem.Id == _vm!.VbarSelectedIndex)
                { return; }
                if (_vm.Model == "SB522A" && newItem.Id == 2)
                {
                    _vm.ChangeImage(_vm.Model, "NoLight");
                }
                else
                {
                    _vm.ChangeImage(_vm.Model, "Default");
                }
                if (_rightFrameWidth[newItem.Id + 1] != _rightFrameWidth[_vm.VbarSelectedIndex + 1])
                {
                    _vm.RightFrameWidthFrom = _rightFrameWidth[_vm.VbarSelectedIndex + 1];
                    _vm.RightFrameWidthTo = _rightFrameWidth[newItem.Id + 1];

                    InvokeGotoTwoViewModeAnimation();
                    InvokeEnlargeAnimation();
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
                //btnUnpair.Visibility = Visibility.Collapsed;
                _vm.SetLadningMode(false);
                _vm.SelectVBar();
                DdpmCommonHelper.WriteUILog($"[SoundBar_LaunchView] OnVbarItemClicked ... out ");
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog($"[SoundBar_LaunchView] OnVbarItemClicked Exception = {ex.Message}");
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
                DdpmCommonHelper.WriteUILog($"[SoundBar_LaunchView] InvokeGotoTwoViewModeAnimation Exception = {ex.Message}");
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
                DdpmCommonHelper.WriteUILog($"[SoundBar_LaunchView] InvokeShrinkAnimation Exception = {ex.Message}");
            }
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
            try
            {
                if (_vm!.ConnectionType == "Dongle")
                {
                    //UnpairModalDialog unpairModalDialog = new(eDeviceCategory.KB);
                    UnpairModalDialog unpairModalDialog = new(eDeviceCategory.Soundbar);
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
                DdpmCommonHelper.WriteUILog($"[SoundBar_LaunchView] Unpair_Click Exception = {ex.Message}");
            }
        }

        private void Mainframe_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                if (_vm!.VbarSelectedIndex == -1)
                { return; }
                _vm.UpdateResetToDefault();
                if (!_vm.IsRestoreEnable)
                {
                    btnRestore.Visibility = Visibility.Visible;
                }
                else
                {
                    btnRestore.Visibility = Visibility.Collapsed;
                }
                _vm.ChangeImage(_vm.Model, "Default");
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
                DdpmCommonHelper.WriteUILog($"[SoundBar_LaunchView] Mainframe_MouseLeftButtonDown Exception = {ex.Message}");
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
                    _vm!.RestoreToDefault();
                    btnRestore.Visibility = Visibility.Collapsed;
                    //((Border)sender).Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog($"[SoundBar_LaunchView] Restore_Click Exception = {ex.Message}");
            }
        }

        private void BatteryIndicator_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            DdpmCommonHelper.WriteUILog($"[SoundBar_LaunchView] BatteryIndicator_MouseEnter ... in");
            try
            {
                if (_vm!.ConnectionType == "WiredAudio")
                {
                    return;
                }
                if (_vm!.ConnectionType == "Dongle")
                {
                    DongleConnection.Visibility = Visibility.Visible;
                }
                else
                {
                    string hostName = HostNameHandler.GetHostName();
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
                }
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog($"[SoundBar_LaunchView] BatteryIndicator_MouseEnter Exception = {ex.Message}");
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
                DdpmCommonHelper.WriteUILog($"[SoundBar_LaunchView] BatteryIndicator_MouseLeave Exception = {ex.Message}");
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
                DdpmCommonHelper.WriteUILog($"[SoundBar_LaunchView] PushBack Exception = {ex.Message}");
            }
        }

    }
}