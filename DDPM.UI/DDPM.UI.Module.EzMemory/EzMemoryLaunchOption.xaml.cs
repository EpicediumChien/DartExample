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
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DDPM.UI.Module.EzMemory
{
    /// <summary>
    /// EzMemoryLaunchOption.xaml 的互動邏輯
    /// </summary>
    public partial class EzMemoryLaunchOption : UserControl
    {
        #region Private Members
        private HomeDevice _homeDevice;
        private IDeviceManagerSA _deviceManagerSA;
        private DDPM.UI.Common.ViewModels.EzMemoryViewModel _vm;
        private readonly DisplayViewModel _vmDisplay;
        private readonly IConsole _console;
        private readonly ILog _log;
        #endregion Private Members
        public EzMemoryLaunchOption(DisplayViewModel vmDisplay)
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

            InitializeComponent();

            InitializePage();
        }

    public void InitializePage()
    {
        _vm.ezPages = _vm.GetEzPages();

        if (_vm.ezPages.ContainsKey(_vm._currentDeviceModel))
        {
            _vm.CurrentAnimationPage = _vm.ezPages[_vm._currentDeviceModel].Count;
            var pageData = _vm.ezPages[_vm._currentDeviceModel][2];
            MainText.Text = pageData.MainText!;
            SubText.Text = pageData.SubText!;
        }
    }

    private void ArrowButton_Click(object sender, RoutedEventArgs e)
        {
            _vm.ProgressValue = 2;
            EzMemoryAssignProgram _ezMemoryAssignProgram = new EzMemoryAssignProgram(_vmDisplay);
            _ezMemoryAssignProgram.DataContext = _vmDisplay;
            DdpmCommonHelper.ModuleOwner?.OpenFullView(_ezMemoryAssignProgram);
        }

        private void FinishBtn_Click(object sender, RoutedEventArgs e)
        {
            DdpmCommonHelper.ModuleOwner?.CloseFullView();
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            DdpmCommonHelper.ModuleOwner?.CloseFullView();
        }
    }
}
