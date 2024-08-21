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

        private void CallWindowsSettings_Click(object sender, RoutedEventArgs e)
        {
            DdpmCommonHelper.DeviceManagerSA.CallWindowsDisplaySetting();
        }

        private void RefreshUI()
        {
            VisionEngineViewModel vm = (VisionEngineViewModel)DataContext;
            vm.RefreshUI();
        }
    }
}