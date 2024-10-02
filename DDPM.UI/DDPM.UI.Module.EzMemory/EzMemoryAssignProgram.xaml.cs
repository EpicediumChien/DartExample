using DDPM.SA.Common;
using DDPM.SA.Common.Settings;
using DDPM.UI.Common;
using DDPM.UI.Common.Models;
using DDPM.UI.Plugin.Common.ViewModels;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using System;
using System.Collections.Generic;
using System.Globalization;
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
            //這裡加入分割視窗的個數
            //_vm.IsRightGridPage2Visible = false;
            //_vm.SelectedValue = 3;
            _vm.IsRightGridPage2Visible = true;
            //_vm.SelectedValue = 3;
            _vm.ezPages = _vm.GetEzPages();

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

        }
        private void ArrowButton_Click(object sender, RoutedEventArgs e)
        {
            EzMemoryFirst ezMemoryFirst = new EzMemoryFirst(_vmDisplay);
            DdpmCommonHelper.ModuleOwner?.OpenFullView(ezMemoryFirst);
        }
        private void NextBtn_Click(object sender, RoutedEventArgs e)
        {
            _vm._currentPageIndex++;
            EzMemoryLaunchOption _ezMemoryLaunchOption = new EzMemoryLaunchOption(_vmDisplay);
            DdpmCommonHelper.ModuleOwner?.OpenFullView(_ezMemoryLaunchOption);
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

        private void AddButton1_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.Button button = sender as System.Windows.Controls.Button;
            _vm.ButtonName = button.Name;
            EzMemoryAddApplication _ezMemoryAddApplication = new EzMemoryAddApplication(_vmDisplay);
            DdpmCommonHelper.ModuleOwner?.OpenFullView(_ezMemoryAddApplication);
        }
    }
    public class WindowGridVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int selectedValue && parameter is string gridIndexString && int.TryParse(gridIndexString, out int gridIndex))
            {
                // 如果 selectedValue 大於等於 gridIndex，則顯示 (Visible)，否則隱藏 (Collapsed)
                return selectedValue >= gridIndex ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
