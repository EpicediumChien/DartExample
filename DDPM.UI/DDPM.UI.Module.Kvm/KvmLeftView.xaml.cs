using DDPM.UI.Common;
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
            vm.Invoke_RefreshData();
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
