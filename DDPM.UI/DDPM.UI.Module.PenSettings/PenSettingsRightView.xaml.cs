using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using DDPM.UI.Plugin.ViewModels;
using Dell.Client.Framework.UX.WPF.Controls;
using UserControl = System.Windows.Controls.UserControl;

namespace DDPM.UI.Module.PenSettings {
  /// <summary>
  /// Interaction logic for PenSettingsRightView.xaml
  /// </summary>
  public partial class PenSettingsRightView : UserControl {
    private readonly PenViewModel _vm;

    private readonly string Caption = "Pen Settings";
    private readonly string TipSensitivity = "Tip Sensitivity";
    private readonly string TipTooltip = "Moving the slider to the right will gradually\ndecrease the sensitivity to pressure and\nyou need to apply firmer pen pressure.";
    private readonly string TiltSensitivity = "Tilt Sensitivity";
    private readonly string TiltTooltip = "Moving the slider to the right will gradually\nincrease the tilting effect, and you need to\napply less tilting angle.";
    private readonly string PairWithTile = "Pair with Tile";
    private readonly string PairTooltip = "Pair your pen to your mobile device using\nthe Tile app.";
    private readonly string GetStarted = "Get started";

    public PenSettingsRightView(PenViewModel vm) {
      InitializeComponent();
      _vm = vm;

      txtCaption.Text = Caption;
      txtTipSensitivity.Text = TipSensitivity;
      txtTipTooltip.Text = TipTooltip;
      txtTiltSensitivity.Text = TiltSensitivity;
      txtTiltTooltip.Text = TiltTooltip;
      txtPairWithTile.Text = PairWithTile;
      txtPairTooltip.Text = PairTooltip;
      btnPair.Content = GetStarted;
    }

    private void Slider_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e) {
      _vm.IsSliderDragging = true;
    }

    private void TipSlider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e) {
      _vm.IsSliderDragging = false;
      //_vm.SetDPIValue();
    }


    private void TiltSlider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e) {
      _vm.IsSliderDragging = false;
      //_vm.SetTouchScrollSensitivityLevel();
    }

    private void btnPair_Click(object sender, RoutedEventArgs e) {

    }
  }
}
