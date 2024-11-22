using CommunityToolkit.Mvvm.Input;
using DDPM.SA.Common;
using DDPM.UI.Common;
using DDPM.UI.Interfaces;
using DDPM.UI.Module.SpeakerAudioPreset;
using DDPM.UI.Module.SpeakerAudioSettings;
using DDPM.UI.Module.SpeakerInteractions;
using DDPM.UI.Plugin.Common;
using DDPM.UI.Plugin.ViewModels;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Net;
using System.Windows;
using System.Windows.Controls;
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

        private readonly int[] _rightFrameWidth = new int[] { 0, 483, 483, 483 };
        //private readonly string Restore = "Restore to default";
        //private readonly string Unpair = "Unpair";
        private readonly string AudioPreset = Strings.SoundBarAudioPreset;
        private readonly string AudioSettings = Strings.SoundBarAudioSettings;
        private readonly string Interactions = Strings.SoundBarInteractions;
        
        private readonly Style ConnectionStyle1;
        private readonly Style ConnectionStyle2;
        private readonly BitmapImage img1 = new(new Uri($"/DDPM.UI.Resources;component/Resources/Images/Bluetooth.png", UriKind.Relative));
        private readonly BitmapImage img2 = new(new Uri($"/DDPM.UI.Resources;component/Resources/Images/Bluetooth2.png", UriKind.Relative));

        //private readonly string PenSettings = "Pen Settings";
        //private readonly string PenButton = "Button\nCustomization";

        public LaunchView()
        {
            InitializeComponent();
            _vm = (SoundBarViewModel?)SoundBarPlugin.PluginIoc?.GetService<IPeripheralViewModel>()!;

            if (_vm != null)
            {
                _vm.Reset();
                DataContext = _vm;
                _vm.VbarItemClickCommand = new RelayCommand<VbarItem>(OnVbarItemClicked!);
                BuildModuleGroups();
            }

            //txtUnpair.Text = Unpair;
            //txtRestore.Text = Restore;

            ConnectionStyle1 = (Style)FindResource("ConnectionStyle1");
            ConnectionStyle2 = (Style)FindResource("ConnectionStyle2");
            txtSystemName1.Text = _vm!.VisiblePairedHostName1;
            txtSystemName2.Text = _vm.VisiblePairedHostName1;
            txtSystemName3.Text = _vm.VisiblePairedHostName1;
            txtFirmware.Text = string.Format(Strings.DockDongle1, _vm.PhysicalDeviceFWVersion);
            txtSlot.Text = string.Format(Strings.DockDongle0, _vm.CurrentDeviceInfo!.MaxPairingSlots - _vm.CurrentDeviceInfo.PairedDeviceCount, _vm.CurrentDeviceInfo.MaxPairingSlots);
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
            _vm.ChangeImage(_vm.Model, "Default");
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

        private void Mainframe_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (_vm!.VbarSelectedIndex == -1) { return; }

            btnRestore.Visibility = Visibility.Visible;
            _vm.ChangeImage(_vm.Model, "Default");
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
                //((Border)sender).Visibility = Visibility.Collapsed;
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