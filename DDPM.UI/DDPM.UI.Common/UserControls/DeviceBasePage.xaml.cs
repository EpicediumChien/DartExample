using DDPM.UI.Common.Models;
using DDPM.UI.Common.ViewModels;
using Dell.Client.Framework.Common;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Animation;
using static System.Runtime.InteropServices.JavaScript.JSType;
using UserControl = System.Windows.Controls.UserControl;
using VcpCore.Common;

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
        private readonly int breakPoints = 1050;
        private readonly Int16 vBarWidthNormal = 230, vBarWidthRWD = 70;
        private readonly Int16 rightGridWidth = 660;
        private bool isFirstEntryNonLandingMode = true;

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

            breakPoints = DdpmCommonHelper.GetBreakPoints();
        }

        private void OnRightViewHeaderChanged(object sender, RoutedEventArgs e)
        {
            rightViewHeaderCtrl.SetHeaders(viewModel.RightViewHeaders.ToArray());

            //Check if new selected header is not show (IsShown==false), then change the selection
            if (viewModel.RightViewHeaders.Count > 0 &&
                !viewModel.RightViewHeaders[viewModel.RightViewHeaderSelectedIndex].IsShown)
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

            isFirstEntryNonLandingMode = true;
            ChangeToNonLandingMode();
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
            isFirstEntryNonLandingMode = false;
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
        }

        private void bdLeftArrow_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter &&
                LeftArrowClick != null)
            {
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
            if (viewModel.SelectedHomeDevice != null && viewModel.SelectedHomeDevice.MonitorInfo != null)
            {
                isDdciOn = viewModel.SelectedHomeDevice.MonitorInfo.DDCisON;
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
            //if (this.ActualWidth <= breakPoints)
            //{
            //    ShowVBar(false);
            //    ChangeToVerticalLayout();
            //}
            //else
            //{
            //    ShowVBar(false);
            //    ChangeToHorizontalLayout();
            //}

            if (!viewModel.IsLandingMode)
                ChangeToNonLandingMode();
            else
            { 
                ChangeToLandingMode();
                RightGrid.Visibility = Visibility.Collapsed;

                /*
                // Jim add 20241221 for PIMS-303368 [DDPM Win 2.0][R11 Webcam] The text button "Restore to Default" always showing in DDPM UI under DUT.
                bool blResetToDefault_Visible = true;

                // check bightness
                double Brightness_Value;
                ObjGetVCP obj = DdpmCommonHelper.DeviceManagerSA.GetVCPCapability(viewModel.SelectedHomeDevice.MonitorInfo, 0x10, 0).Result;
                if (obj.result)
                {
                    //if (Brightness_Value < 0)
                    //{
                    //    Brightness_Value = Convert.ToDouble((uint)(long)obj.value);
                    //}
                    //else
                    Brightness_Value = Convert.ToDouble((uint)(long)obj.value);

                    if (Brightness_Value != (double)75.0f)
                        blResetToDefault_Visible = true;
                    else
                        blResetToDefault_Visible = false;

                }

                if (!blResetToDefault_Visible)
                {
                    // check contrast
                    double Contrast_Value;
                    obj = DdpmCommonHelper.DeviceManagerSA.GetVCPCapability(viewModel.SelectedHomeDevice.MonitorInfo, 0x12, 0).Result;
                    if (obj.result)
                    {
                        //if (Brightness_Value < 0)
                        //{
                        //    Brightness_Value = Convert.ToDouble((uint)(long)obj.value);
                        //}
                        //else
                        Contrast_Value = Convert.ToDouble((uint)(long)obj.value);

                        if (Contrast_Value != (double)75.0f)
                            blResetToDefault_Visible = true;
                        else
                            blResetToDefault_Visible = false;

                    }


                    if (blResetToDefault_Visible)
                        ((Border)(viewModel.DefaultLeftView.FindName("btnRestore"))).Visibility = Visibility.Visible;
                    else
                        ((Border)(viewModel.DefaultLeftView.FindName("btnRestore"))).Visibility = Visibility.Collapsed;
                }
                */

            }

            //tbDisplayName.Width = gridDisplayName.ActualWidth;
            //_log?.Info($"tbDisplayName.ActualWidth = {tbDisplayName.ActualWidth}");
            //_log?.Info($"gridDisplayName.ActualWidth = {gridDisplayName.ActualWidth}");

            //Robert_Lin 2025-1-13 Test code to lock Display Settings group
            //if (viewModel.GroupCount > 0)
            //{
            //    SetLockModuleGroup(Constants.GroupName_DisplaySettings, true);
            //}
        }

        private void ChangeToNonLandingMode()
        {
            //横屏 to non landing mode
            if (topStackPanel.Orientation == System.Windows.Controls.Orientation.Horizontal)
            {
                ShowVBar();
                AdjustHorizontalLayoutForNonLandingMode(false);

                //restore vBar
                foreach (var item in viewModel.VbarItems)
                {
                    item.ResetStory();
                }

                vbarListLeft.ItemsSource = null;
                vbarListLeft.ItemsSource = viewModel.VbarItems;
            }
            else
            {
                ShowVBar(false);
                ExtendVBarOnVerticalLayout();
            }

            //show right frame on ToHorizontalLayout
            //show right frame
            RightGrid.Visibility = Visibility.Visible;
            stVbarRightFrame.Orientation = System.Windows.Controls.Orientation.Horizontal;

            //PrintDebugData("ChangeNonLandingMode");

            /*
            // Jim add 20241221 for  PIMS-303368 [DDPM Win 2.0][R11 Webcam] The text button "Restore to Default" always showing in DDPM UI under DUT.
            ((Border)(viewModel.DefaultLeftView.FindName("btnRestore"))).Visibility = Visibility.Collapsed;
            */

            // Jim 20250109 to fix PIMS-340036 [DDPM Win 2.0][R19]Connect two monitors,switch the drop-down menu. One DUT of the Restore to default icons disappears.
            ((System.Windows.Controls.Button)(viewModel.DefaultLeftView.FindName("btnRestore"))).Visibility = Visibility.Collapsed;
            viewModel.SelectedHomeDevice.IsRestoreBtnVisible = Visibility.Collapsed; ;
        }

        private void ExtendVBarOnVerticalLayout()
        {
            //left side
            LeftGrid.Width = this.ActualWidth;
            LeftFrame.Width = this.ActualWidth;

            //right side
            vBarRight.Width = vBarWidthNormal;
            //extend vBar
            foreach (var item in viewModel.VbarItems)
            {
                item.CompleteStory();
            }

            stVbarRightFrame.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;

            vbarListRight.ItemsSource = null;
            vbarListRight.ItemsSource = viewModel.VbarItems;
        }

        //for debug
        private void PrintDebugData(string function)
        {
            _log?.Info($"this.ActualWidth = {this.ActualWidth} in {function}");
            _log?.Info($"vBar.ActualWidth = {vBarRight.ActualWidth}");
            _log?.Info($"RightGrid.ActualWidth = {RightGrid.ActualWidth}");
            _log?.Info($"LeftFrame.ActualWidth = {LeftFrame.ActualWidth}");
        }

        private void ChangeToLandingMode()
        {
            if (topStackPanel.Orientation == System.Windows.Controls.Orientation.Horizontal)
            {
                //vBarRight.Width = vBarWidthNormal;  //show the vBar
                vBarRight.Width = 0;
                LeftFrame.Width = this.ActualWidth - vBarRight.Width - 13;
                LeftFrame.Height = this.ActualHeight - 40;

                //vBar.Margin = new Thickness(0, 0, 0, 10);
            }
        }

        private void ChangeToVerticalLayout()
        {
            topStackPanel.Orientation = System.Windows.Controls.Orientation.Vertical;
            ShowVBar(false);
        }

        private void ChangeToHorizontalLayout()
        {
            topStackPanel.Orientation = System.Windows.Controls.Orientation.Horizontal;

            ShowVBar(false);
        }

        private void vBar_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (topStackPanel.Orientation == System.Windows.Controls.Orientation.Horizontal && !viewModel.IsLandingMode)
            {
                ShowVBar();
                AdjustHorizontalLayoutForNonLandingMode();
            }
        }

        private void vBar_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!isFirstEntryNonLandingMode &&
                topStackPanel.Orientation == System.Windows.Controls.Orientation.Horizontal &&
                !viewModel.IsLandingMode)
            {
                AdjustHorizontalLayoutForNonLandingMode(false);
            }
            else
            {
                isFirstEntryNonLandingMode = false;
            }

            //leave landing mode from Vertical layout
            if (!viewModel.IsLandingMode && topStackPanel.Orientation == System.Windows.Controls.Orientation.Vertical)
                ExtendVBarOnVerticalLayout();
        }

        private void AdjustHorizontalLayoutForNonLandingMode(bool bVBarNormal = true)
        {
            vBarRight.Width = bVBarNormal ? vBarWidthNormal : vBarWidthRWD;
            RightGrid.Width = rightGridWidth;
            //LeftGrid.Width = this.ActualWidth - vBar.Width - RightGrid.Width - 20;
            LeftGrid.Width = this.ActualWidth - RightGrid.Width - 20;
            LeftFrame.Width = LeftGrid.Width;
        }

        private void ShowVBar(bool bShowLeft = true)
        {
            if (bShowLeft) 
            {
                //System.Windows.MessageBox.Show("left");
                vBarLeft.Visibility = Visibility.Visible;
                vBarRight.Visibility = Visibility.Collapsed;
            }
            else 
            {
                //System.Windows.MessageBox.Show("right");
                vBarLeft.Visibility = Visibility.Collapsed;
                vBarRight.Visibility = Visibility.Visible;
            }
        }
    }
}