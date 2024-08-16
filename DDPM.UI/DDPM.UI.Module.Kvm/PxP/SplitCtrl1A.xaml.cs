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
            vm.PCSwap("PC1", "PC2");
            vm.VideoSwapContent = new PIPSplitCtrl1A();
        }
    }
}