using DDPM.UI.Common;
using System.Windows.Input;

namespace DDPM.UI.Module.Kvm
{
    /// <summary>
    /// Interaction logic for SplitCtrl1B.xaml
    /// </summary>
    public partial class PIPSplitCtrl1B : System.Windows.Controls.UserControl
    {
        private KvmViewModel vm
        {
            get
            {
                return (KvmViewModel)DataContext;
            }
        }

        public PIPSplitCtrl1B()
        {
            InitializeComponent();
        }

        private void PIP_PCSWAP(object sender, MouseButtonEventArgs e)
        {
            if (vm != null)
            {
                vm.PCSwap("PC1", "PC2");
                if (!vm.isPxPFullView)
                {
                    if (DdpmCommonHelper.DeviceManagerSA.VideoSwap(vm.KvmModule.SelectedHomeDevice.MonitorInfo, 0, 1).Result)
                    {
                        vm.UpdatePCList();
                    }
                }
                else
                {
                    vm.VideoSwapContent = new PIPSplitCtrl1B();
                    vm.VideoSwapContent_Left = new PIPSplitCtrl1B();
                }
            }
        }
    }
}