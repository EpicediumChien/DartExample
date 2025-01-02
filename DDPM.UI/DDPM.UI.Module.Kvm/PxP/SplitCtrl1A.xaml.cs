using DDPM.UI.Common;
using System.Windows.Input;
using UserControl = System.Windows.Controls.UserControl;

namespace DDPM.UI.Module.Kvm
{
    /// <summary>
    /// Interaction logic for SplitCtrl1A.xaml
    /// </summary>
    public partial class PIPSplitCtrl1A : UserControl
    {
        private KvmViewModel vm
        {
            get
            {
                return (KvmViewModel)DataContext;
            }
        }

        public PIPSplitCtrl1A()
        {
            InitializeComponent();
        }

        private void PIP_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (vm != null)
            {
                vm.PCSwap("PC1", "PC2");
                if (!vm.isPxPFullView)
                {
                    DdpmCommonHelper.DeviceManagerSA.VideoSwap(vm.KvmModule.SelectedHomeDevice.MonitorInfo, 0, 1);
                }
                vm.VideoSwapContent = new PIPSplitCtrl1A();
                vm.VideoSwapContent_Left = new PIPSplitCtrl1A();
            }
        }
    }
}