using DDPM.UI.Common;
using DDPM.UI.Plugin.ViewModels;
using System.Windows;
using System.Windows.Input;
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

        private DateTime _lastKeyUpTime;
        private readonly TimeSpan _debounceInterval = TimeSpan.FromMilliseconds(500); // Adjust as needed
        private void DPISlider_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Left:
                case Key.Right:
                case Key.Up:
                case Key.Down:
                    DateTime now = DateTime.Now;

                    if (now - _lastKeyUpTime > _debounceInterval)
                    {
                        _lastKeyUpTime = now;
                        _vm.IsSliderDragging = false;
                        //_vm.DPIValue = (int)DPISlider.Value;
                        _vm.SetDPIValue();
                    }
                    else
                    {
                        e.Handled = true;
                    }
                    break;
                default:
                    return;
            }
        }

        private void DPISlider_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Left:
                case Key.Right:
                case Key.Up:
                case Key.Down:
                    _vm.IsSliderDragging = true;
                    break;
                default:
                    return;
            }
        }
    }
}