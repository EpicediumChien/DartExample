using DDPM.UI.Plugin.ViewModels;
using System.Windows.Controls;

namespace DDPM.UI.Module.Illumination
{
    /// <summary>
    /// Interaction logic for IlluminationRightView.xaml
    /// </summary>
    public partial class IlluminationRightView : UserControl
    {
        private readonly KeyboardViewModel _vm;

        public IlluminationRightView(KeyboardViewModel vm)
        {
            InitializeComponent();
            _vm = vm;
        }

        private void BackLightingSlider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            _vm.IsSliderDragging = false;
            _vm.SetDBackLightingLevel();
        }

        private void BackLightingSlider_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            _vm.IsSliderDragging = true;
        }
    }
}