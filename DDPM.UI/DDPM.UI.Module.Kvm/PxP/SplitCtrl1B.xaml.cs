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
