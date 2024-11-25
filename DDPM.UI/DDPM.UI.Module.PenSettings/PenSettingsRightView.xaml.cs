using DDPM.UI.Plugin.ViewModels;
using System.Windows;
using UserControl = System.Windows.Controls.UserControl;
using DDPM.UI.Common;
using DDPM.UI.Plugin.Common;
using static Dell.Client.Framework.UX.WPF.WinApi;
using System.Reflection.Metadata;
using System.Windows.Media;

namespace DDPM.UI.Module.PenSettings
{
    /// <summary>
    /// Interaction logic for PenSettingsRightView.xaml
    /// </summary>
    public partial class PenSettingsRightView : UserControl
    {
        private readonly PenViewModel _vm;


        public PenSettingsRightView(PenViewModel vm)
        {
            InitializeComponent();
            _vm = vm;

            txtCaption.Text = Strings.PenSettings;
            txtTipSensitivity.Text = Strings.TipSensitivity;
            txtTipTooltip.Text = Strings.TipTooltip;
            txtTiltSensitivity.Text = Strings.TiltSensitivity;
            txtTiltTooltip.Text = Strings.TiltTooltip;
            txtPairWithTile.Text = Strings.PairWithTile;
            txtPairTooltip.Text = Strings.PairTooltip;
            btnGetStart.Content = Strings.GetStarted2;

            //DoubleCollection tickMarks = new DoubleCollection();
            //tickMarks.Add(0);
            //tickMarks.Add(12.5);
            //tickMarks.Add(25);
            //tickMarks.Add(60);
            //tickMarks.Add(62.5);
            //tickMarks.Add(75);
            //tickMarks.Add(100);
            //TipSlider.Ticks = tickMarks;
        }

        private void Slider_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            _vm.IsSliderDragging = true;
        }

        private void TipSlider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            _vm.IsSliderDragging = false;
            _vm.SetTipSensitivity();
        }

        private void TiltSlider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            _vm.IsSliderDragging = false;
            _vm.SetTiltSensitivity();
        }

    private void btnGetStart_Click(object sender, RoutedEventArgs e) {
      Window parentWindow = Window.GetWindow(this);
      double windowLeft = 0;
      double windowTop = 0;
      PairTileModalDialog modalDialog = new(parentWindow.ActualWidth, parentWindow.ActualHeight);
      if(parentWindow != null) {
        modalDialog.Owner = parentWindow;
        windowLeft = parentWindow.Left;
        windowTop = parentWindow.Top;
      }
      modalDialog.WindowStartupLocation = WindowStartupLocation.Manual;
      modalDialog.Left = windowLeft;
      modalDialog.Top = windowTop;
      modalDialog.ShowDialog();
    }
  }
}
