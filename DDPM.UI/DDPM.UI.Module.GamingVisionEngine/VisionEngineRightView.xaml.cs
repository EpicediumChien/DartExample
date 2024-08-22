using DDPM.UI.Common;
using System.Windows;
using System.Windows.Controls;

namespace DDPM.UI.Module.GamingVisionEngine
{
    /// <summary>
    /// Interaction logic for VisionEngineRightView.xaml
    /// </summary>
    public partial class VisionEngineRightView : UserControl
    {
        public VisionEngineRightView()
        {
            InitializeComponent();

        }

        private void RefreshUI()
        {
            VisionEngineViewModel vm = (VisionEngineViewModel)DataContext;
            vm.RefreshUI();
        }

        private void UXCheckBox_Click(object sender, RoutedEventArgs e)
        {
            VisionEngineViewModel vm = (VisionEngineViewModel)DataContext;
            vm.VisionEngine_Debouncer.Debounce(null);
        }
    }
}