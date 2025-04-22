using DDPM.UI.Common;
using DDPM.UI.Plugin.Common;
using DDPM.UI.Plugin.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DDPM.UI.Module.RtkHubPortInfo
{
    /// <summary>
    /// Interaction logic for RtkHubPortInfoRightView.xaml
    /// </summary>
    public partial class RtkHubPortInfoRightView : UserControl
    {
        private readonly RtkHubViewModel _vm;
        private LearnMorePage modalDialog;
        public RtkHubPortInfoRightView(RtkHubViewModel vm)
        {
            InitializeComponent();
            _vm = vm;
        }

        //Elsa add for tooltip issue fix
        private double GetScreenScaleX()
        {
            var source = PresentationSource.FromVisual(this);
            if (source?.CompositionTarget != null)
            {
                return source.CompositionTarget.TransformToDevice.M11;
            }
            return 1;
        }

        //Elsa add for tooltip issue fix
        private void toolTip_Opened(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.ToolTip? target = sender as System.Windows.Controls.ToolTip;
            if (target == null)
                return;
            double screenScaleX = GetScreenScaleX();
            Window mainWindow = System.Windows.Application.Current.MainWindow;
            double winRightX = (mainWindow.Left + mainWindow!.ActualWidth) * screenScaleX;
            double mousepositionX = System.Windows.Forms.Cursor.Position.X;
            double mouseaddtooltip = mousepositionX + target.ActualWidth * screenScaleX;
            if (winRightX > mouseaddtooltip)
            {
                target.HorizontalOffset = 2;
            }
            else
            {
                target.HorizontalOffset = -1 * target.ActualWidth + 14;
            }
        }
    }
}