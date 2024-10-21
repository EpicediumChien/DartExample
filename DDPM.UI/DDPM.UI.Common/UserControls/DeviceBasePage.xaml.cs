using DDPM.UI.Common.Models;
using DDPM.UI.Common.ViewModels;
using Dell.Client.Framework.Common;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using UserControl = System.Windows.Controls.UserControl;

namespace DDPM.UI.Common.UserControls
{
    /// <summary>
    /// Interaction logic for DeviceBasePage.xaml
    /// </summary>
    public partial class DeviceBasePage : UserControl
    {
        private ILog _log;
        private Stopwatch _stopwatch = new Stopwatch();

        private DeviceBasePageViewModel viewModel = new DeviceBasePageViewModel();
        public DeviceBasePageViewModel ViewModel { get { return viewModel; } }

        //Derek 10/17 for RWD
        private readonly Int16 breakPoints = 537;

        public DeviceBasePage()
        {
            InitializeComponent();
            DataContext = viewModel;
            viewModel.LeaveLandingMode += OnLeaveLandingMode;
            viewModel.RightViewHeaderChanged += OnRightViewHeaderChanged;
            viewModel.SelectedHomeDeviceChanged += OnSelectedHomeDeviceChanged;

            if (DdpmCommonHelper.MyConsole != null)
            {
                _log = DdpmCommonHelper.MyConsole.CreateLog("BasePage");
                _log.Info("DeviceBasePage ctor");
                viewModel.InitLog();
                _stopwatch.Restart();
            }

            //marketName.Text = Strings.Display;
            //tooltipFwVer.Text = Strings.FirmwareVersion;
            //tooltipServiceTag.Text = Strings.ServiceTag;
            //tooltipManufactureMonth.Text = Strings.ManufactureMonth;

            if (System.Windows.Application.Current?.TryFindResource("breakPoint") is Int16 width)
                breakPoints = width;
        }

        private void OnRightViewHeaderChanged(object sender, RoutedEventArgs e)
        {
            rightViewHeaderCtrl.SetHeaders(viewModel.RightViewHeaders.ToArray());

            //Check if new selected header is not show (IsShown==false), then change the selection
            if (viewModel.RightViewHeaders.Count > 0)
            {
                if (!viewModel.RightViewHeaders[viewModel.RightViewHeaderSelectedIndex].IsShown)
                {
                    int newIndex = 0;
                    ModuleGroup? mg = viewModel.SelectedGroup;
                    if (mg != null)
                    {
                        newIndex = mg.HeaderSelectedIndex;
                    }
                    //Try to select [0] (NOTE. It's assume that the Headers[0] will be always isShown)
                    if (viewModel.RightViewHeaders[newIndex].IsShown)
                        viewModel.RightViewHeaderSelectedIndex = newIndex;
                }
            }
            rightViewHeaderCtrl.SelectedIndex = viewModel.RightViewHeaderSelectedIndex;
        }

        public void SetModuleGroupList(List<ModuleGroup> groupList)
        {
            viewModel.ModuleGroups = groupList;
            DataContext = null;
            DataContext = viewModel;
        }

        #region Leave from LandingMode
        private void OnLeaveLandingMode(object sender, RoutedEventArgs e)
        {
            //Transit to TwoView mode
            InvokeGotoTwoViewModeAnimation();

            //    //Set the RightViewHeader seklection to 0
            //    //_ivm.RightViewHeaderSelectedIndex = 0;

            //    RightFrame.Visibility = Visibility.Visible;
            //System.Windows.MessageBox.Show("OnLeaveLandingMode");
            LeftFrame.Width = viewModel.LeftFrameWidth;
        }

        private void OnSelectedHomeDeviceChanged(object sender, EventArgs e)
        {
            if (viewModel.DefaultLeftView != null)
            {
                viewModel.DefaultLeftView.DataContext = null;
                viewModel.DefaultLeftView.DataContext = viewModel.SelectedHomeDevice;
            }
        }

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
        #endregion Leave from LandingMode

        public event RoutedEventHandler? LeftArrowClick;

        private void leftArrow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (LeftArrowClick != null)
                LeftArrowClick(sender, e);

            //System.Windows.MessageBox.Show("leftArrow_MouseLeftButtonDown");
        }

        //RightViewHeaderCtrl cannot notify SelectedIndex property changed to ViewModel.
        //SO we will handling the SelectionChanged event
        private void rightViewHeaderCtrl_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            viewModel.RightViewHeaderSelectedIndex = rightViewHeaderCtrl.SelectedIndex;
            /*
            int newSelId =
            if (newSelId != displaySettingsSelIdx)
            {
                if (_ivm != null)
                {
                    _ivm.RightViewHeaderSelectedIndex = newSelId;
                }
                displaySettingsSelIdx = newSelId;
                SwitchLeftRightView();
            }
            */
        }

        public void SetLeftFrameWidth(double width)
        {
            viewModel.LeftFrameWidth = width;
        }

        public void SetHomeDevices(List<HomeDevice> devices)
        {
            viewModel.HomeDevices = devices;
        }

        public void SetSelectedHomeDevice(HomeDevice? device)
        {
            viewModel.SelectedHomeDevice = device;
        }

        public void SelectGroupByIndex(int groupIndex)
        {
            if ((groupIndex < 0) || (groupIndex >= viewModel.GroupCount))
                return;

            //If we are in LandingMode, then will transit to TwoViewMode
            if (viewModel.IsLandingMode)
                InvokeGotoTwoViewModeAnimation();

            viewModel.GroupSelectedIndex = groupIndex;
        }

        public void SetDefaultLeftView(UserControl leftView)
        {
            viewModel.DefaultLeftView = leftView;
            viewModel.DefaultLeftView.DataContext = viewModel.SelectedHomeDevice;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            _stopwatch.Stop();
            if (_log != null)
            {
                _log.Info($"DeviceBasePage_Loaded, Elapsed {_stopwatch.Elapsed.TotalMilliseconds} msec.");
            }

            LeftFrame.Width = this.ActualWidth - 20;
            rightFrameSV.Height = RightGrid.Height;
        }

        private void bdLeftArrow_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (LeftArrowClick != null)
                    LeftArrowClick(sender, e);
            }
        }

        #region Lock/Unlock
        public bool SetLockModuleGroup(string groupName, bool isLocked)
        {
            //Find the group index from groupName
            int groupIndex = viewModel.FindGroupIndexByGroupName(groupName);
            if (groupIndex < 0)
                return false;
            //Set the IsLocked for the VbatItem
            VbarItem1 vbarItem = viewModel.VbarItems[groupIndex];
            vbarItem.IsLocked = isLocked;

            //Get the IsDdcciOn flag from SelectedHomeDevice
            bool isDdciOn = true;
            if (viewModel.SelectedHomeDevice != null)
            {
                if (viewModel.SelectedHomeDevice.MonitorInfo != null)
                {
                    isDdciOn = viewModel.SelectedHomeDevice.MonitorInfo.DDCisON;
                }
            }


            //If we are not in Landing mode which has selected group
            if ((!viewModel.IsLandingMode) && isLocked)
            {
                //If DDC/CI is on
                if (isDdciOn)
                {
                    //If current locked group is currently selected
                    //then always change selection to the first group (DisplaySettings)
                    if (groupIndex == viewModel.GroupSelectedIndex)
                    {
                        //If current Selected Monitor DDC/CI is off => switch
                        viewModel.GroupSelectedIndex = 0;
                    }
                }
                else
                {
                    //DDC/CI is off => Only EasyArrange can be selected and should has been selected
                    //If we just lock EasyArrange, then should return to homepage
                    if (groupName.Equals(Constants.GroupName_EasyArrange))
                    {
                        viewModel.GotoHomepage();
                    }
                }
            }
            return true;
        }
        #endregion

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //_log.Info($"this.ActualWidth = {this.ActualWidth}");

            //if (!isLandingMode && LeftFrame.ActualWidth <= 200)
            if (!viewModel.IsLandingMode && this.ActualWidth <= breakPoints + 150)
            {
                ChangeToVerticalLayout();
            }
            else
            {
                ChangeToHorizontalLayout();
            }

            if (!viewModel.IsLandingMode)
            {
                RightGrid.Width = this.ActualWidth / 2;
                LeftFrame.Width = this.ActualWidth / 2;
            }
            else
            {
                LeftFrame.Width = this.ActualWidth - 20;
            }
        }

        private void ChangeToVerticalLayout()
        {
            topStackPanel.Orientation = System.Windows.Controls.Orientation.Vertical;
            //vBar.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            rightFrameSV.Height = this.ActualHeight - LeftFrame.ActualHeight - 20;
        }

        private void ChangeToHorizontalLayout()
        {
            topStackPanel.Orientation = System.Windows.Controls.Orientation.Horizontal;
            //vBar.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
            rightFrameSV.Height = RightGrid.Height;
        }
    }
}