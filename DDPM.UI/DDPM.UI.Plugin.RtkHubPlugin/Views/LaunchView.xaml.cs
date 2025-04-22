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
        private readonly string RtkHubPortInfos = Strings.RtkHub01;

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
                //btnUnpair.Visibility = Visibility.Collapsed;
                //ConnectionStyle1 = (Style)FindResource("ConnectionStyle1");
                //ConnectionStyle2 = (Style)FindResource("ConnectionStyle2");
                //txtSystemName3.Text = _vm.VisiblePairedHostName1;
                //txtFirmware.Text = "Dongle " + _vm.PhysicalDeviceFWVersion;
                //txt1.Text = Strings.USB_C;
                //txtSlot.Text = $"{_vm.CurrentDeviceInfo!.MaxPairingSlots - _vm.CurrentDeviceInfo.PairedDeviceCount} of {_vm.CurrentDeviceInfo.MaxPairingSlots} slots available";
                //txtAudioBLText.Text = string.Format(Strings.Paired_Info, _vm.CurrentDeviceInfo.TotalNumberOfPairedHostName);
                //Loaded += LaunchView_LoadedStatus;
                //Unloaded += LaunchView_UnLoadedStatus;
            }
            DdpmCommonHelper.WriteUILog($"[RtkHub_LaunchView] LaunchView Constructor ... end ");
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
                    GroupName = RtkHubPortInfos,
                    GroupIcon = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Images/DA225_PortInfo.png", "DDPM.UI.Resources"),
                    GroupIconCanvas = DdpmCommonHelper.CanvasIconCreator(VbarIcon.RtkHubPortInfo)
                };
                moduleGroup.AddHeader(RtkHubPortInfos, new RtkHubPortInfoModule(_vm!));
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
            try
            {
                switch (_vm!.Model.ToUpper())
                {
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
                DdpmCommonHelper.WriteUILog($"[RtkHub_LaunchView] InitializeButtonImage Exception = {ex.Message}");
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
            DdpmCommonHelper.WriteUILog($"[RtkHub_LaunchView] OnVbarItemClicked ... in");
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
                btnUpdate.Visibility = Visibility.Collapsed;
                //btnUnpair.Visibility = Visibility.Collapsed;
                _vm.SetLadningMode(false);
                _vm.SelectVBar();
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog($"[RtkHub_LaunchView] OnVbarItemClicked Exception = {ex.Message}");
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
            DdpmCommonHelper.WriteUILog($"[RtkHub_LaunchView] Mainframe_MouseLeftButtonDown ... in");
            try
            {
                if (_vm!.VbarSelectedIndex == -1)
                { return; }
                _vm.UpdateResetToDefault();
                if (!_vm.IsUpdateEnable)
                {
                    btnUpdate.Visibility = Visibility.Visible;
                }
                else
                {
                    btnUpdate.Visibility = Visibility.Collapsed;
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
                DdpmCommonHelper.WriteUILog($"[RtkHub_LaunchView] Mainframe_MouseLeftButtonDown Exception = {ex.Message}");
            }
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