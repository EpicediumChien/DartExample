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
            vm.PCSwap("PC1", "PC2");
            vm.VideoSwapContent = new PIPSplitCtrl1B();
        }
    }
}