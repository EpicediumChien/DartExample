using CommunityToolkit.Mvvm.Input;
using DDPM.SA.Common.Settings;
using DDPM.UI.Common;
using DDPM.UI.Common.UserControls;
using DDPM.UI.Interfaces;
using DDPM.UI.Module.PenButtonSettings;
using DDPM.UI.Module.PenSettings;
using DDPM.UI.Plugin.Common;
using DDPM.UI.Plugin.ViewModels;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace DDPM.UI.Plugin.PenPlugin
{
    /// <summary>
    /// PenPlugin.xaml 的互動邏輯
    /// </summary>
    public partial class LaunchView : UserControl
    {
        private readonly PenViewModel? _vm;

        private readonly int[] _rightFrameWidth = { 0, 530, 330 };
        private ModuleGroup moduleGroup;

        public LaunchView()
        {
            DdpmCommonHelper.WriteUILog($"Pen UI LaunchView Begin timestamp: {DateTime.Now:hh:mm:ss.ffffff}");
            try
            {
                Loaded += LaunchView_Loaded;
                InitializeComponent();
                _vm = (PenViewModel?)Penplugin.PluginIoc?.GetService<IPeripheralViewModel>()!;
                if (_vm == null)
                {
                    DdpmCommonHelper.WriteUILog("Pen ViewModel is null");
                    return;
                }

                _vm.Reset();
                DataContext = _vm;
                _vm.VbarItemClickCommand = new RelayCommand<VbarItem1>(OnVbarItemClicked!);
                BuildModuleGroups();

                //txtUnpair.Text = Strings.Unpair;
                //txtRestore.Text = Strings.RestoreToDefault;

                InitializeButtonImage();
                //if (_vm.IsRestoreEnable)
                //{
                //    btnRestore.Visibility = Visibility.Visible;
                //}
                //else
                //{
                //    btnRestore.Visibility = Visibility.Collapsed;
                //}
                _vm.IsAllButtonsVisible = Visibility.Visible;
                _vm.ActiveModule = null;

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
                            txtRestore.IsEnabled = !data.LockSettings.Lock_Pen_RestoreFactoryDefaults;
                            RestoreLockIcon.Visibility = data.LockSettings.Lock_Pen_RestoreFactoryDefaults ? Visibility.Visible : Visibility.Collapsed;

                            //Lock Functionality 9/7
                            //When a 1 or more settings are locked, automatically lock 'Restore to default'/'factory reset' control [Pen]
                            if (data.LockSettings != null &&
                                DdpmCommonHelper.GetUINotifyPropertyValue_isAnyLocked(data, "Lock_Pen"))
                            {
                                RestoreLockIcon.Visibility = Visibility.Visible;
                                txtRestore.IsEnabled = false;
                            }
                        }
                    }
                }
                if (_vm.Model == "PN5122W")
                    imgInfo.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.PenPlugin\\Views\\LaunchView.xaml.cs  LaunchView() ex:" + ex.Message);
            }
            DdpmCommonHelper.WriteUILog($"Pen UI LaunchView End timestamp: {DateTime.Now:hh:mm:ss.ffffff}");
        }

        private void LaunchView_Loaded(object sender, RoutedEventArgs e)
        {
            DdpmCommonHelper.WriteUILog($"Pen UI Loaded timestamp: {DateTime.Now:hh:mm:ss.ffffff}");
        }

        ~LaunchView()
        {
            if (DdpmCommonHelper.DeviceManagerSA != null)
            {
                DdpmCommonHelper.DeviceManagerSA.ITSettingsActionEvent -= DeviceManagerSA_ITSettingsActionEvent;
            }

            moduleGroup?.Dispose();
        }

        private void DeviceManagerSA_ITSettingsActionEvent(object? sender, SA.Common.ITSettingEventArgs e)
        {
            var rst = DdpmCommonHelper.ApplyRestoreFactoryDefaultsEventData(e, "Lock_Pen_RestoreFactoryDefaults");
            Dispatcher.Invoke(new Action(() =>
            {
                RestoreLockIcon.Visibility = rst.isLocked;
                txtRestore.IsEnabled = rst.isEnabled;
            }));
        }

        private void InitializeButtonImage()
        {
            switch (_vm?.Model?.ToUpper())
            {
                case "PN7522W":
                    imgTopButtonBackground.Visibility = Visibility.Visible;
                    SectionB.Margin = new Thickness(195, 320, 0, 0);
                    SectionC.Margin = new Thickness(155, 343.5, 0, 0);
                    this.Resources["B2Width"] = 45.0;
                    this.Resources["B2Height"] = 36.0;

                    PolyT.Points = new PointCollection
                    {
                       new Point(0, 0),
                        new Point(620, 0),
                        new Point(620, 146),
                        new Point(597, 111),
                        new Point(61, 410),
                        new Point(0, 466)
                    };
                    PolyB.Points = new PointCollection
                    {
                        new Point(0, 540),
                        new Point(620, 540),
                        new Point(620,153),
                        new Point(83, 450),
                        new Point(30,466),
                        new Point(0, 467)
                    };
                    break;

                case "PN9315A":
                    imgTopButtonBackground.Visibility = Visibility.Visible;
                    SectionB.Margin = new Thickness(191, 325, 0, 0);
                    SectionC.Margin = new Thickness(151, 348, 0, 0);
                    this.Resources["B2Width"] = 55.0;
                    this.Resources["B2Height"] = 46.0;

                    PolyT.Points = new PointCollection
                    {
                       new Point(0, 0),
                        new Point(620, 0),
                        new Point(620, 154),
                        new Point(597, 117),
                        new Point(52, 422),
                        new Point(0, 475)
                    };
                    PolyB.Points = new PointCollection
                    {
                        new Point(0, 540),
                        new Point(620, 540),
                        new Point(620,157),
                        new Point(80, 459),
                        new Point(30,472),
                        new Point(0, 475)
                    };
                    break;

                case "PN5122W":
                    SectionB.Margin = new Thickness(210, 272, 0, 0);
                    SectionC.Margin = new Thickness(159, 301, 0, 0);
                    this.Resources["B2Width"] = 63.0;
                    this.Resources["B2Height"] = 52.0;

                    PolyT.Points = new PointCollection
                    {
                        new Point(0, 0),
                        new Point(620, 0),
                        new Point(620, 112),
                        new Point(597, 74),
                        new Point(53, 376),
                        new Point(0, 429)
                    };
                    PolyB.Points = new PointCollection
                    {
                        new Point(0, 540),
                        new Point(620, 540),
                        new Point(620,116),
                        new Point(83, 415),
                        new Point(30,429),
                        new Point(0, 430)
                    };
                    break;
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

            moduleGroup = new ModuleGroup()
            {
                GroupName = Strings.PenSettingsCaption,
                GroupIcon = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Images/PencilMark.png", "DDPM.UI.Resources"),
                GroupIconCanvas = DdpmCommonHelper.CanvasIconCreator(VbarIcon.PenSettings)
            };
            moduleGroup.AddHeader(Strings.PenSettingsCaption, new PenSettingsModule(_vm));
            groups.Add(moduleGroup);

            moduleGroup = new ModuleGroup()
            {
                GroupName = Strings.ButtonCustomizationCaption,
                GroupIcon = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Images/PenButton.png", "DDPM.UI.Resources"),
                GroupIconCanvas = DdpmCommonHelper.CanvasIconCreator(VbarIcon.PenButton)
            };
            moduleGroup.AddHeader(Strings.ButtonCustomizationCaption, new PenButtonSettingsModule(_vm));
            groups.Add(moduleGroup);

            _vm.ModuleGroups = groups;
        }

        #endregion Init for Modules

        #region Vbar

        private void OnVbarItemClicked(VbarItem1 newItem)
        {
            if (_vm == null)
                return;

            try
            {
                if (newItem.Id == _vm.VbarSelectedIndex)
                { return; }

                if (_rightFrameWidth[newItem.Id + 1] != _rightFrameWidth[_vm.VbarSelectedIndex + 1])
                {
                    _vm.RightFrameWidthFrom = _rightFrameWidth[_vm.VbarSelectedIndex + 1];
                    _vm.RightFrameWidthTo = _rightFrameWidth[newItem.Id + 1];

                    InvokeGotoTwoViewModeAnimation();

                    if (newItem.Id == 0)
                    {
                        InvokeShrinkAnimation();
                    }
                    else
                    {
                        if (_vm.VbarSelectedIndex == 0)
                        {
                            InvokeEnlargeAnimation();
                        }
                        _vm.ActiveModule?.OnActivated();
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

                if (_vm.VbarSelectedIndex == 0)
                { _vm.IsAllButtonsVisible = Visibility.Hidden; }
                else
                { _vm.IsAllButtonsVisible = Visibility.Visible; }
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.PenPlugin\\Views\\LaunchView.xaml.cs OnVbarItemClicked() ex:" + ex.Message);
            }
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

        private void Unpair_Click(object sender, RoutedEventArgs e)
        {
            if (_vm == null)
                return;

            try
            {
                if (_vm.Model == "PN5122W")
                {
                    UnpairModalDialog unpairModalDialog = new(eDeviceCategory.Pen);
                    Window parentWindow = Window.GetWindow(this);
                    if (parentWindow != null)
                    {
                        unpairModalDialog.Owner = parentWindow;
                    }

                    bool? dialogResult = unpairModalDialog.ShowDialog();
                    if (dialogResult == true)
                    {
                        _vm.UnpairPen();
                    }
                    return;
                }
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
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.PenPlugin\\Views\\LaunchView.xaml.cs Unpair_Click() ex:" + ex.Message);
            }
        }

        private void Mainframe_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                if (_vm.VbarSelectedIndex == -1)
                { return; }

                _vm.RightFrameWidthTo = 0;
                _vm.RightFrameWidthFrom = _rightFrameWidth[_vm.VbarSelectedIndex + 1];
                InvokeGotoTwoViewModeAnimation();
                btnUnpair.Visibility = Visibility.Visible;
                if (_vm.IsRestoreEnable)
                {
                    btnRestore.Visibility = Visibility.Visible;
                }
                else
                {
                    btnRestore.Visibility = Visibility.Collapsed;
                }

                if (_vm.VbarSelectedIndex == 0)
                { InvokeEnlargeAnimation(); }

                _vm.VbarSelectedIndex = -1;
                _vm.SetLadningMode(true);
                _vm.SelectVBar();
                _vm.ClearSelectedButton();
                _vm.IsAllButtonsVisible = Visibility.Visible;
                _vm.SelectedBehavior = "";
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.PenPlugin\\Views\\LaunchView.xaml.cs Mainframe_MouseLeftButtonDown() ex:" + ex.Message);
            }
        }

        private void Restore_Click(object sender, RoutedEventArgs e)
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
                    _vm.RestoreToDefault();
                }
                btnRestore.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.PenPlugin\\Views\\LaunchView.xaml.cs Restore_Click() ex:" + ex.Message);
            }
        }

        private void ButtonHoverIn(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (_vm == null)
                return;

            try
            {
                var btnName = ((Image)sender).Name;
                _vm.RefreshButtonImageFile(btnName, true, btnName == _vm.SelectedButton);
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.PenPlugin\\Views\\LaunchView.xaml.cs ButtonHoverIn() ex:" + ex.Message);
            }
        }

        private void ButtonHoverOut(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (_vm == null)
                return;

            try
            {
                var btnName = ((Image)sender).Name;
                _vm.RefreshButtonImageFile(btnName, false, btnName == _vm.SelectedButton);
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.PenPlugin\\Views\\LaunchView.xaml.cs ButtonHoverOut() ex:" + ex.Message);
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
                DdpmCommonHelper.WriteUILog("DDPM.UI.PenPlugin\\Views\\LaunchView.xaml.cs ButtonClicked() ex:" + ex.Message);
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
                DdpmCommonHelper.WriteUILog("DDPM.UI.PenPlugin\\Views\\LaunchView.xaml.cs PushBack() ex:" + ex.Message);
            }
        }
    }
}