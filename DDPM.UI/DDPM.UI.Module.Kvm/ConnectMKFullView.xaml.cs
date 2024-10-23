using DDPM.UI.Common;
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
            vm.FromProgressValue = vm.ToProgressValue;
            vm.ToProgressValue = vm.ToProgressValue - 1;
        }

        private void OpenPxPFullView(object sender, RoutedEventArgs e)
        {
            KVMPIPPBPFullView kvmPIPPBPFullView = new KVMPIPPBPFullView(vm);
            kvmPIPPBPFullView.DataContext = vm;
            DdpmCommonHelper.ModuleOwner?.OpenFullView(kvmPIPPBPFullView);
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
    }
}