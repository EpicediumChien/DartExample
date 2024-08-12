using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using DDPM.UI.Common.ViewModels;
using Dell.Client.Framework.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
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

        public DeviceBasePage()
        {
            InitializeComponent();
            DataContext = viewModel;
            viewModel.LeaveLandingMode += OnLeaveLandingMode;
            viewModel.RightViewHeaderChanged += OnRightViewHeaderChanged;

            if (DdpmCommonHelper.MyConsole != null)
            {
                _log = DdpmCommonHelper.MyConsole.CreateLog("BasePage");
                _log.Info("DeviceBasePage ctor");
                _stopwatch.Restart();
            }

            //marketName.Text = Strings.Display;
            //tooltipFwVer.Text = Strings.FirmwareVersion;
            //tooltipServiceTag.Text = Strings.ServiceTag;
            //tooltipManufactureMonth.Text = Strings.ManufactureMonth;
        }

        private void OnRightViewHeaderChanged(object sender, RoutedEventArgs e)
        {
            rightViewHeaderCtrl.SetHeaders(viewModel.RightViewHeaders.ToArray());
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
        #endregion

        public event RoutedEventHandler? LeftArrowClick;

        private void leftArrow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (LeftArrowClick != null)
                LeftArrowClick(sender, e);
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
            if (e.Key == Key.Enter)
            {
                if (LeftArrowClick != null)
                    LeftArrowClick(sender, e);
            }
        }
    }
}
