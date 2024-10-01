using DDPM.SA.Common;
using DDPM.SA.Common.Settings;
using DDPM.UI.Common;
using DDPM.UI.Common.Models;
using DDPM.UI.Plugin.Common.ViewModels;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ProgressBar = System.Windows.Controls.ProgressBar;
using UserControl = System.Windows.Controls.UserControl;

namespace DDPM.UI.Module.EzMemory
{
    /// <summary>
    /// EzMemoryAssignProgram.xaml 的互動邏輯
    /// </summary>
    public partial class EzMemoryAssignProgram : UserControl
    {
        #region Private Members
        private HomeDevice _homeDevice;
        private IDeviceManagerSA _deviceManagerSA;
        private DDPM.UI.Common.ViewModels.EzMemoryViewModel _vm;  
        private readonly DisplayViewModel _vmDisplay;
        private readonly IConsole _console;
        private readonly ILog _log;
        #endregion Private Members
        public EzMemoryAssignProgram(DisplayViewModel vmDisplay)
        {
            _vmDisplay = vmDisplay;
            _homeDevice = vmDisplay.SelectedHomeDevice;
            _console = vmDisplay.Console;
            _deviceManagerSA = HomeDevice.DeviceManagerSA;

            InitializeComponent();

            if (_homeDevice.vmEzMemory == null)
            {
                _homeDevice.vmEzMemory = new DDPM.UI.Common.ViewModels.EzMemoryViewModel(_homeDevice);
            }
            _vm = _homeDevice.vmEzMemory;
            DataContext = _homeDevice.vmEzMemory;

            //Screen? currentScreen = GetAttachedScreen(_homeDevice.MonitorInfo.DisplayName);
            //_vm.IsVertical = (currentScreen != null) ? (currentScreen.Bounds.Width < currentScreen.Bounds.Height) : false;

            InitializePage();
        }
        public void InitializePage()
        {
            //_vm._currentTotalPage = 0;
            //_vm._currentPageIndex = 0;
            //_vm.ProgressValue = 1;
            _vm.ezPages = _vm.GetEzPages();
            //RightGridPage2.Visibility = Visibility.Collapsed;

            if (_vm.ezPages.ContainsKey(_vm._currentDeviceModel))
            {
                _vm.CurrentAnimationPage = _vm.ezPages[_vm._currentDeviceModel].Count;
                var pageData = _vm.ezPages[_vm._currentDeviceModel][1];
                MainText.Text = pageData.MainText!;
                SubText.Text = pageData.SubText!;
            }
        }

        /// <summary>
        /// Next Page
        /// </summary>
        public void NextPage()
        {
            _vm._currentPageIndex++;
            EzMemoryAssignProgram _ezMemoryAssignProgram = new EzMemoryAssignProgram(_vmDisplay);
            _ezMemoryAssignProgram.DataContext = _vmDisplay;
            DdpmCommonHelper.ModuleOwner?.OpenFullView(_ezMemoryAssignProgram);
            //UpdatePageContent();
        }

        /// <summary>
        /// Previous Page
        /// </summary>
        public void PreviousPage()
        {
            if (_vm._currentPageIndex > 0)
            {
                _vm._currentPageIndex--;
                UpdatePageContent();
            }
        }
        /// <summary>
        /// Update Page Content
        /// </summary>
        public void UpdatePageContent()
        {
            if (_vm._currentPageIndex >= 3)
            {
                CancelBtn_Click(null!, null!);
            }
            var pageData = _vm.ezPages["EzMemory"][_vm._currentPageIndex];
            MainText.Text = pageData.MainText!;
            SubText.Text = pageData.SubText!;
            ControlPageGrid(_vm._currentPageIndex);
        }
        public void ControlPageGrid(int _currentPageIndex)
        {
            //if (_vm._currentPageIndex == 0)
            //{
            //    RightGridPage1.Visibility = Visibility.Visible;
            //    RightGridPage2.Visibility = Visibility.Collapsed;
            //}
            //if (_vm._currentPageIndex == 1)
            //{
            //    RightGridPage1.Visibility = Visibility.Collapsed;
            //    RightGridPage2.Visibility = Visibility.Visible;
            //}
        }
        private void ArrowButton_Click(object sender, RoutedEventArgs e)
        {
            EzMemoryFirst ezMemoryFirst = new EzMemoryFirst(_vmDisplay);
            ezMemoryFirst.DataContext = _vmDisplay;
            DdpmCommonHelper.ModuleOwner?.OpenFullView(ezMemoryFirst);
            //if (_vm._currentPageIndex == 0)
            //{
            //    DdpmCommonHelper.ModuleOwner?.CloseFullView();
            //    return;
            //}
            //PreviousPage();
            //DoProgressAnimation(false);
        }
        private void NextBtn_Click(object sender, RoutedEventArgs e)
        {
            EzMemoryAddApplication _ezMemoryAddApplication = new EzMemoryAddApplication(_vmDisplay);
            _ezMemoryAddApplication.DataContext = _vmDisplay;
            DdpmCommonHelper.ModuleOwner?.OpenFullView(_ezMemoryAddApplication);
            //NextPage();
            //DoProgressAnimation(true);
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            _vm.ProgressValue = 1;
            DdpmCommonHelper.ModuleOwner?.CloseFullView();
            return;
        }


        private void DoProgressAnimation(bool isForward)
        {
            double newProgressValue;
            if (isForward)
            {
                // Move
                newProgressValue = Math.Min(_vm.ProgressValue + 1, _vm.CurrentAnimationPage);
            }
            else
            {
                // Back
                newProgressValue = Math.Max(_vm.ProgressValue - 1, 1);
            }

            DoubleAnimation progressAnimation = new DoubleAnimation
            {
                From = _vm.ProgressValue,
                To = newProgressValue,
                Duration = new Duration(TimeSpan.FromSeconds(0.5)), // Time
                FillBehavior = FillBehavior.HoldEnd
            };

            EzMemoryProgressbar.BeginAnimation(ProgressBar.ValueProperty, progressAnimation);

            // refresh ProgressValue
            _vm.ProgressValue = newProgressValue;
        }
    }
}
