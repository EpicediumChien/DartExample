using DDPM.UI.Common;
using DDPM.UI.Plugin.ViewModels;
using System.Windows;
using UserControl = System.Windows.Controls.UserControl;

namespace DDPM.UI.Module.MouseSettings
{
    /// <summary>
    /// Interaction logic for MouseSettingsRightView.xaml
    /// </summary>
    public partial class MouseSettingsRightView : UserControl
    {
        private readonly MouseViewModel _vm;

        public MouseSettingsRightView(MouseViewModel vm)
        {
            InitializeComponent();
            _vm = vm;

            txtDPIMessage.Text = Strings.DPIMessage;
            txtPollingRateMessage.Text = Strings.PollingRateMessage;
        }

        private void DPISlider_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            _vm.IsSliderDragging = true;
        }

        private void DPISlider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            _vm.IsSliderDragging = false;
            _vm.SetDPIValue();
        }

        private void TouchScrollSlider_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            _vm.IsSliderDragging = true;
            _vm.IsTouchScrollHilighted = Visibility.Visible;
            _vm.OnPropertyChanged(nameof(_vm.IsTouchScrollHilighted));
        }

        private void TouchScrollSlider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            _vm.IsSliderDragging = false;
            _vm.SetTouchScrollSensitivityLevel();
            _vm.IsTouchScrollHilighted = Visibility.Collapsed;
            _vm.OnPropertyChanged(nameof(_vm.IsTouchScrollHilighted));
        }
    }
}