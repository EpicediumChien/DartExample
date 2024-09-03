using System.Windows.Controls;

namespace DDPM.UI.Module.EzMemory
{
    /// <summary>
    /// Interaction logic for EzMemoryRightView.xaml
    /// </summary>
    public partial class EzMemoryRightView : UserControl
    {
        private EzMemoryViewModel? vm;

        public EzMemoryRightView(EzMemoryViewModel? vm)
        {
            InitializeComponent();
            DataContext = vm;
        }
    }
}