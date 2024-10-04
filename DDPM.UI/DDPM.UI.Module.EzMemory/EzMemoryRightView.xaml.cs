using DDPM.UI.Common;
using System.Windows.Controls;
using Dell.Client.Framework.UX.WPF;
using Dell.Client.Framework.Common;
using System.Windows;
using DDPM.UI.Plugin.Common.ViewModels;
using DDPM.SA.Common;
using DDPM.UI.Common.Models;

namespace DDPM.UI.Module.EzMemory
{
    /// <summary>
    /// Interaction logic for EzMemoryRightView.xaml
    /// </summary>
    public partial class EzMemoryRightView : UserControl
    {
        #region Private Members
        private HomeDevice _homeDevice;
        private IDeviceManagerSA _deviceManagerSA;
        private DDPM.UI.Common.ViewModels.EzArrangeViewModel _vm;    
        private readonly DisplayViewModel _vmDisplay;
        private readonly IConsole _console;
        private readonly ILog _log;
        #endregion Private Members

        public EzMemoryRightView(DisplayViewModel vmDisplay)
        {
            InitializeComponent();

            _vmDisplay = vmDisplay;
            _homeDevice = vmDisplay.SelectedHomeDevice;
            _console = vmDisplay.Console;
            _deviceManagerSA = HomeDevice.DeviceManagerSA;

            InitializeTextBlocks();
        }

        private void InitializeTextBlocks()
        {
            ProfileTitleTextBlock.Text = "Profile";
            AutomaticStartupTextBlock.Text = "Automatic Startup:";
            AutomaticStartupValueTextBlock.Text = "N/A"; 
            LaunchByTimeTextBlock.Text = "Launch by Time:";
            LaunchByTimeValueTextBlock.Text = "N/A";
            AppDocumentTextBlock.Text = "App/Document:";
            AppDocumentValueTextBlock.Text = "N/A";

            applybtn.Content = "Apply";
        }

        private void EzMemoryStart_Click(object sender, RoutedEventArgs e)
        {
            EzMemoryFirst ezFirst = new EzMemoryFirst(_vmDisplay);
            //ezFirst.DataContext = _vm;
            DdpmCommonHelper.ModuleOwner?.OpenFullView(ezFirst);
        }

        //private void CheckBox_Click_1(object sender, System.Windows.RoutedEventArgs e)
        //{
        //    IConsole? console = DdpmCommonHelper.MyConsole;
        //    if (console != null)
        //    {
        //        if  (ck.IsChecked != null)
        //        {
        //            var args = new EventManagerArgs();
        //            args.Tag = (bool)ck.IsChecked; //true=Show, false=Hide
        //            console.RaiseEvent(ConsoleEventNames.Masthead_ShowAddDeviceIcon, this, args);
        //        }
        //    }
        //}

        //private void ckSettings_Click(object sender, System.Windows.RoutedEventArgs e)
        //{
        //    IConsole? console = DdpmCommonHelper.MyConsole;
        //    if (console != null)
        //    {
        //        if (ckSettings.IsChecked != null)
        //        {
        //            var args = new EventManagerArgs();
        //            args.Tag = (bool)ckSettings.IsChecked; //true=Show, false=Hide
        //            console.RaiseEvent(ConsoleEventNames.Masthead_ShowSettingsIcon, this, args);
        //        }
        //    }
        //}
    }
}