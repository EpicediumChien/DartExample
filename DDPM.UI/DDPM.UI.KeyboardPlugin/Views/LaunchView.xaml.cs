using CommunityToolkit.Mvvm.Input;
using DDPM.UI.Common;
using DDPM.UI.Interfaces;
using DDPM.UI.Module.Collaboration;
using DDPM.UI.Module.Illumination;
using DDPM.UI.Module.KeyCustomization;
using DDPM.UI.Plugin.Common;
using DDPM.UI.Plugin.ViewModels;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace DDPM.UI.Plugin.KeyboardPlugin
{
    /// <summary>
    /// KeyboardPlugin.xaml 的互動邏輯
    /// </summary>
    public partial class LaunchView : UserControl
    {
        private readonly KeyboardViewModel? _vm;
        private readonly int[] _rightFrameWidth = { 0, 333, 533, 533 };
        private readonly Style ConnectionStyle1;
        private readonly Style ConnectionStyle2;
        private readonly BitmapImage img1 = new(new Uri($"/DDPM.UI.Resources;component/Resources/Images/Bluetooth.png", UriKind.Relative));
        private readonly BitmapImage img2 = new(new Uri($"/DDPM.UI.Resources;component/Resources/Images/Bluetooth2.png", UriKind.Relative));

        public LaunchView()
        {
            InitializeComponent();
            _vm = (KeyboardViewModel?)Keyboardplugin.PluginIoc?.GetService<IPeripheralViewModel>();

            if (_vm != null)
            {
                DataContext = _vm;
                _vm.VbarItemClickCommand = new RelayCommand<VbarItem>(OnVbarItemClicked!);
                if (_vm.EOLList.Contains(_vm.Model))
                {
                    Battery.Visibility = Visibility.Collapsed;
                    btnRestore.Visibility = Visibility.Collapsed;
                    txtEOL.Text = Strings.EOLMessage;
                    txtEOL.Visibility = Visibility.Visible;
                    SectionA.Visibility = Visibility.Collapsed;
                    SectionF.Visibility = Visibility.Collapsed;
                    EOLDongle.Visibility = Visibility.Visible;
                }
                else
                {
                    BuildModuleGroups();
                }
            }

            txtUnpair.Text = Strings.Unpair;
            txtRestore.Text = Strings.RestoreToDefault;

            ConnectionStyle1 = (Style)FindResource("ConnectionStyle1");
            ConnectionStyle2 = (Style)FindResource("ConnectionStyle2");
            txtDongleHost.Text = Strings.USBWirelessReceiver;

            InitializeKeyImage();
            if (_vm!.IsRestoreEnable)
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
        }

        #region Init for Modules

        private void BuildModuleGroups()
        {
            List<ModuleGroup> groups = new();
            ModuleGroup moduleGroup;

            moduleGroup = new ModuleGroup()
            {
                GroupName = Strings.KeyCustomizationCaption,
                GroupIcon = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Vbar.Keyboard.Key.png")
            };
            moduleGroup.AddHeader(Strings.KeyCustomizationCaption, new KeyCustomizationModule(_vm!));
            groups.Add(moduleGroup);

            if (_vm!.IsCollabsKeysSupported)
            {
                moduleGroup = new ModuleGroup()
                {
                    GroupName = Strings.CollaborationCaption,
                    GroupIcon = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Vbar.Keyboard.Collaboration.png")
                };
                moduleGroup.AddHeader(Strings.CollaborationCaption, new CollaborationModule(_vm));
                groups.Add(moduleGroup);
            }
            if (_vm.IsIlluminationSupported)
            {
                moduleGroup = new ModuleGroup()
                {
                    GroupName = Strings.IlluminationCaption,
                    GroupIcon = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Vbar.Keyboard.Illumination.png")
                };
                moduleGroup.AddHeader(Strings.IlluminationCaption, new IlluminationModule(_vm));
                groups.Add(moduleGroup);
            }

            _vm.ModuleGroups = groups;
        }

        #endregion Init for Modules

        #region Vbar

        private void OnVbarItemClicked(VbarItem newItem)
        {
            //if(newItem.Id == _vm!.VbarSelectedIndex) { return; }

            if (_rightFrameWidth[newItem.Id + 1] != _rightFrameWidth[_vm!.VbarSelectedIndex + 1])
            {
                _vm.RightFrameWidthFrom = _rightFrameWidth[_vm.VbarSelectedIndex + 1];
                _vm.RightFrameWidthTo = _rightFrameWidth[newItem.Id + 1];

                InvokeGotoTwoViewModeAnimation();

                if (newItem.Id == 0)
                {
                    if (_vm.VbarSelectedIndex != -1)
                        InvokeEnlargeAnimation();
                    _vm.ActiveModule?.OnActivated();
                }
                else
                {
                    InvokeShrinkAnimation();
                }
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

            if (_vm!.VbarSelectedIndex > 0) { _vm.IsAllKeysVisible = Visibility.Hidden; }
            else { _vm.IsAllKeysVisible = Visibility.Visible; }
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

            if (_vm.VbarSelectedIndex > 0) { InvokeEnlargeAnimation(); }
            _vm.VbarSelectedIndex = -1;
            _vm.SetLadningMode(true);
            _vm.SelectVBar();
            _vm.ClearSelectedKey();
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
                _vm!.RestoreToDefault();
                ((Border)sender).Visibility = Visibility.Collapsed;
            }
        }

        private void BatteryIndicator_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (_vm!.ConnectionType == "Dongle")
            {
                txtFirmware.Text = $"{Strings.ReceiverFirmwareVersion} {_vm.PhysicalDeviceFWVersion}";
                txtSlot.Text = $"{_vm.CurrentDeviceInfo!.MaxPairingSlots - _vm.CurrentDeviceInfo.PairedDeviceCount} of {_vm.CurrentDeviceInfo.MaxPairingSlots} slots available";
                DongleConnection.Visibility = Visibility.Visible;
            }
            else if (_vm!.ConnectionType == "Bluetooth")
            {
                SetBLConnectionStatus();
                BLConnection.Visibility = Visibility.Visible;
            }
        }

        private void SetBLConnectionStatus()
        {
            var hostIndex = _vm!.VisiblePairedHostName1.ToUpper() == "VISIBLE" ? 1 : (_vm.VisiblePairedHostName2.ToUpper() == "VISIBLE" ? 2 : 3);
            txt1.Style = ConnectionStyle2;
            imgBL1.Source = img2;
            txtBLHost1.Style = ConnectionStyle2;
            txt2.Style = ConnectionStyle2;
            imgBL2.Source = img2;
            txtBLHost2.Style = ConnectionStyle2;

            switch (_vm.Model)
            {
                case "KB700":
                case "KB740":
                    txtBLHost1.Text = string.IsNullOrEmpty(_vm.PairedHostName2) ? Strings.ReadyToBePaired : _vm.PairedHostName2;
                    txtBLHost2.Text = string.IsNullOrEmpty(_vm.PairedHostName3) ? Strings.ReadyToBePaired : _vm.PairedHostName3;
                    if (hostIndex == 2)
                    {
                        txt1.Style = ConnectionStyle1;
                        imgBL1.Source = img1;
                        txtBLHost1.Style = ConnectionStyle1;
                    }
                    else
                    {
                        txt2.Style = ConnectionStyle1;
                        imgBL2.Source = img1;
                        txtBLHost2.Style = ConnectionStyle1;
                    }
                    break;

                case "KB900":
                    txtBLHost1.Text = string.IsNullOrEmpty(_vm.PairedHostName1) ? Strings.ReadyToBePaired : _vm.PairedHostName1;
                    txtBLHost2.Text = string.IsNullOrEmpty(_vm.PairedHostName2) ? Strings.ReadyToBePaired : _vm.PairedHostName2;
                    if (hostIndex == 1)
                    {
                        txt1.Style = ConnectionStyle1;
                        imgBL1.Source = img1;
                        txtBLHost1.Style = ConnectionStyle1;
                    }
                    else
                    {
                        txt2.Style = ConnectionStyle1;
                        imgBL2.Source = img1;
                        txtBLHost2.Style = ConnectionStyle1;
                    }
                    break;

                default:
                    txtBLHost1.Text = _vm.PairedHostName2;
                    txt1.Style = ConnectionStyle1;
                    imgBL1.Source = img1;
                    txtBLHost1.Style = ConnectionStyle1;
                    Host2.Visibility = Visibility.Collapsed;
                    break;
            }
        }

        private void BatteryIndicator_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            DongleConnection.Visibility = Visibility.Collapsed;
            BLConnection.Visibility = Visibility.Collapsed;
        }

        private void InitializeKeyImage()
        {
            switch (_vm!.Model.ToUpper())
            {
                case "KB700":
                case "KB7221W":
                    SectionA.Margin = new Thickness(89.5, 156, 0, 0);
                    this.Resources["F1Width"] = 44.1;
                    this.Resources["F1Height"] = 32.5;
                    this.Resources["SectionAMargin"] = new Thickness(-2.6, 0, 0, 0);
                    this.Resources["F5Margin"] = new Thickness(0.2, 0, 0, 0);
                    this.Resources["F9Margin"] = new Thickness(0.1, 0, 0, 0);
                    break;

                case "KB500":
                case "KB3121W":
                    SectionA.Margin = new Thickness(80.5, 151, 0, 0);
                    this.Resources["F1Width"] = 44.1;
                    this.Resources["F1Height"] = 32.5;
                    this.Resources["SectionAMargin"] = new Thickness(-1, 0, 0, 0);
                    this.Resources["F5Margin"] = new Thickness(0, 0, 0, 0);
                    this.Resources["F9Margin"] = new Thickness(1, 0, 0, 0);
                    break;

                case "KB7120W":
                case "KB740":
                    SectionA.Margin = new Thickness(78, 145, 0, 0);
                    this.Resources["F1Width"] = 45.5;
                    this.Resources["F1Height"] = 37.0;
                    this.Resources["SectionAMargin"] = new Thickness(-2, 0, 0, 0);
                    this.Resources["F5Margin"] = new Thickness(-3, 0, 0, 0);
                    this.Resources["F9Margin"] = new Thickness(-3, 0, 0, 0);
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
                    this.Resources["CalculatorWidth"] = 43.0;
                    this.Resources["CalculatorHeight"] = 32.5;
                    this.Resources["CalculatorMargin"] = new Thickness(7, 0, 0, 0);
                    break;
            }
        }

        private void KeyHoverIn(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var keyName = ((Image)sender).Name;
            _vm!.RefreshKeyImageFile(keyName, true, keyName == _vm.SelectedKey);
        }

        private void KeyHoverOut(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var keyName = ((Image)sender).Name;
            _vm!.RefreshKeyImageFile(keyName, false, keyName == _vm.SelectedKey);
        }

        private void KeyClicked(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var keyName = ((Image)sender).Name;
            if (_vm!.SelectedKey != "")
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
        }
    }
}