using DDPM.UI.Common;
using DDPM.UI.Plugin.ViewModels;
using System.Windows.Controls;
using System.Windows.Input;

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

        private DateTime _lastKeyUpTime;
        private readonly TimeSpan _debounceInterval = TimeSpan.FromMilliseconds(600); // Adjust as needed
        private void UXSlider_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            try
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
                            _vm.SetDBackLightingLevel();
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
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.Module.MouseSettings\\IlluminationRightView.xaml.cs UXSlider_KeyUp ex:" + ex.Message);
            }
        }

        private void UXSlider_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            try
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
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.Module.MouseSettings\\IlluminationRightView.xaml.cs UXSlider_PreviewKeyDown ex:" + ex.Message);
            }
        }
    }
}