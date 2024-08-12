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
        }

        private void OpenPxPFullView(object sender, RoutedEventArgs e)
        {
            KVMPIPPBPFullView kvmPIPPBPFullView = new KVMPIPPBPFullView(vm);
            kvmPIPPBPFullView.DataContext = vm;
            DdpmCommonHelper.ModuleOwner?.OpenFullView(kvmPIPPBPFullView);
        }

        private void CloseUSBKVM(object sender, RoutedEventArgs e)
        {
            DdpmCommonHelper.ModuleOwner?.CloseFullView();
        }
    }
}
