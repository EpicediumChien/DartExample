using DDPM.UI.Common;
using System.Windows;
using System.Windows.Controls;

namespace DDPM.UI.Module.Kvm
{
    /// <summary>
    /// Interaction logic for KvmLeftView.xaml
    /// </summary>
    public partial class KvmLeftView : UserControl
    {
        private KvmViewModel vm
        {
            get
            {
                return (KvmViewModel)DataContext;
            }
        }

        public KvmLeftView(KvmViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
            vm.ConnectionType = vm.KvmModule.SelectedHomeDevice.ConnectionType;
            vm.Text1 = vm.KvmModule.SelectedHomeDevice.Text1;
            //vm.Invoke_RefreshData();
        }

        private void EditInput_Click(object sender, RoutedEventArgs e)
        {
            vm.EditInput = Visibility.Visible;
            vm.SetInput = Visibility.Collapsed;
            InputSourceFullView _inputSourceFullView = new InputSourceFullView();
            _inputSourceFullView.DataContext = vm;
            DdpmCommonHelper.ModuleOwner?.OpenFullView(_inputSourceFullView);
        }

        private void EditPXP_Click(object sender, RoutedEventArgs e)
        {
            vm.SetPXP = Visibility.Collapsed;
            vm.EditPXP = Visibility.Visible;
            KVMPIPPBPFullView kvmPIPPBPFullView = new KVMPIPPBPFullView(vm);
            kvmPIPPBPFullView.DataContext = vm;
            DdpmCommonHelper.ModuleOwner?.OpenFullView(kvmPIPPBPFullView);
        }
    }
}