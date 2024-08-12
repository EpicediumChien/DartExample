using DDPM.Easy.Common;
using Dell.Client.Framework.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DDPM.SA.Plugins.User.EasyArrange
{
    /// <summary>
    /// Interaction logic for EAWorkWindow.xaml
    /// </summary>
    public partial class EAWorkWindow : Window
    {
        private ArrangeVM VM;

        #region ctor
        public EAWorkWindow(ArrangeVM vm)
        {
            InitializeComponent();
            VM = vm;
            DataContext = vm;
        }
        #endregion

        #region Init
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //Hide window from Alt+tab
            System.Windows.Interop.WindowInteropHelper wndHelper = new System.Windows.Interop.WindowInteropHelper(this);
            Win32Lib.Win32.HideWinFromAltTab(wndHelper.Handle);
        }
        #endregion

        #region Working SplitCtrl
        private ISplitCtrl? _workingSplit = null;
        private bool _isSplitCtrl0A = false;

        public bool SetWorkingSplit(int cellCount, char splitKey, List<double>? settings = null)
        {
            this.Dispatcher.Invoke(() =>
            {

                if ((cellCount == 0) && (splitKey == 'A'))
                {
                    _workingSplit = null;
                }
                else
                {
                    _workingSplit = ISplitCtrl.Create(cellCount, splitKey);
                }
                if (_workingSplit != null)
                {
                    _workingSplit.SplitMode = eSplitModes.Work;
                    _workingSplit.IsEditable = false;
                    splitCtrl.Content = _workingSplit;
                }
                else
                {
                    splitCtrl.Content = null;
                }
            });

            return true;

        }
        #endregion

        #region ILog for EAPlugin
        public ILog Log { get; set; }
        #endregion

        public void RefreshCellRects()
        {
            if (_workingSplit == null)
                return;

            DpiScale dpiScale = VisualTreeHelper.GetDpi(this);
            double scale = dpiScale.PixelsPerDip;


            foreach (CellObj objCell in _workingSplit.CellList)
            {
                if (objCell.bd == null)
                    continue;

                objCell.rc = GetBorderRect(objCell.bd);
            }
        }

        private Rect GetBorderRect(Border ctrl)
        {
            if (ctrl == null)
                return Rect.Empty;

            if ((ctrl.ActualWidth == 0) && (ctrl.ActualHeight == 0))
                return Rect.Empty;

            PresentationSource preSrc = PresentationSource.FromVisual(ctrl);
            if (preSrc == null)
                return Rect.Empty;

            System.Windows.Point ptTopLeft = ctrl.PointToScreen(new System.Windows.Point(0, 0));
            double w = ctrl.ActualWidth * VM.ScreenScale;
            double h = ctrl.ActualHeight * VM.ScreenScale;
            return new Rect(ptTopLeft.X, ptTopLeft.Y, w, h);
        }

        public CellObj? DetermineHoveringCellObj(int x, int y)
        {
            if (_workingSplit == null)
                return null;

            DpiScale dpiScale = VisualTreeHelper.GetDpi(this);
            double scale = dpiScale.PixelsPerDip;

            foreach (CellObj objCell in _workingSplit.CellList)
            {
                if (objCell.rc.Contains(x, y))
                {
                    _workingSplit.HoveringCell = objCell.Name;
                    return objCell;
                }
            }
            _workingSplit.HoveringCell = "";
            return null;
        }
    }
}
