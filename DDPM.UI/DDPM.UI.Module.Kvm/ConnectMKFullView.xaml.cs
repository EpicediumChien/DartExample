using DDPM.SA.Common.Display;
using DDPM.UI.Common;
using DDPM.UI.Plugin.DdpmHomePlugin;
using Dell.Client.Framework.UX.WPF;
using System.Windows;
using System.Windows.Controls;

namespace DDPM.UI.Module.Kvm
{
    /// <summary>
    /// Interaction logic for ConnectMKFullView.xaml
    /// </summary>
    public partial class ConnectMKFullView : UserControl
    {
        private KvmViewModel vm
        {
            get
            {
                return (KvmViewModel)DataContext;
            }
        }

        public ConnectMKFullView()
        {
            InitializeComponent();
        }

        private void BackInputFullView(object sender, RoutedEventArgs e)
        {
            InputSourceFullView inputSourceFullView = new InputSourceFullView();
            inputSourceFullView.DataContext = vm;
            DdpmCommonHelper.ModuleOwner?.OpenFullView(inputSourceFullView);
            if (vm.ToProgressValue > 3 || vm.ToProgressValue < 0)
            {
                vm.ToProgressValue = 2;
            }
            vm.FromProgressValue = vm.ToProgressValue;
            vm.ToProgressValue = vm.ToProgressValue - 1;
        }

        private void OpenPxPFullView(object sender, RoutedEventArgs e)
        {
            KVMPIPPBPFullView kvmPIPPBPFullView = new KVMPIPPBPFullView(vm);
            kvmPIPPBPFullView.DataContext = vm;
            DdpmCommonHelper.ModuleOwner?.OpenFullView(kvmPIPPBPFullView);
            if (vm.ToProgressValue > 3 || vm.ToProgressValue < 0)
            {
                vm.ToProgressValue = 2;
            }
            vm.FromProgressValue = vm.ToProgressValue;
            vm.ToProgressValue = vm.ToProgressValue + 1;
        }

        private void CloseUSBKVM(object sender, RoutedEventArgs e)
        {
            DdpmCommonHelper.ModuleOwner?.CloseFullView();
            vm.FromProgressValue = 0;
            vm.ToProgressValue = 1;
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //splitGrid.ActualWidth
        }

        private void USBKVMFinish(object sender, RoutedEventArgs e)
        {
            try
            {
                vm.FinishtoSetPCs();
                vm.isOnUSBKVM(true);
                vm.isOnNKVM(false);
                vm.isOnNoKVM(false);
                vm.USBKVMisON = true;
                vm.NKVMisON = false;
                //Return to DdpmHomePage              
                IConsole? console = DdpmHomePlugin.PluginIoc.GetService<IConsole>();
                console?.ShowPluginById(DDPM.UI.Common.Constants.DdpmHomePluginId);
                vm.FromProgressValue = 0;
                vm.ToProgressValue = 1;
            }
            catch (Exception ex)
            {
                //Return to DdpmHomePage
                IConsole? console = DdpmHomePlugin.PluginIoc.GetService<IConsole>();
                console?.ShowPluginById(DDPM.UI.Common.Constants.DdpmHomePluginId);
                vm.FromProgressValue = 0;
                vm.ToProgressValue = 1;
            }
        }
    }
}