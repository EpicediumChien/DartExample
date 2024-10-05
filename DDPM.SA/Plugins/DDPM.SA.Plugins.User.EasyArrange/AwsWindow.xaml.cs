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
using System.Windows.Forms;
using DDPM.Easy.Common;
using System.Diagnostics;

namespace DDPM.SA.Plugins.User.EasyArrange
{
    /// <summary>
    /// Interaction logic for AwsWindow.xaml
    /// </summary>
    public partial class AwsWindow : Window
    {
        private readonly ArrangeVM _vm;

        public string ScreenDeviceName
        {
            get
            {
                if (HoveringScreen != null)
                {
                    return HoveringScreen.DeviceName;
                }
                return "";
            }
        }
        public Screen? HoveringScreen { get; private set; } = null;

        public AwsWindow(ArrangeVM vm)
        {
            _vm = vm;
            InitializeComponent();
            DataContext = _vm;

        }

        private double leftMargin = 32;
        private double rightMargin = 32;
        private double topMargin = 8;
        private double bottomMargin = 256;

        public void OnStartMoving()
        {
            //Get the Screen contains cursor
            Screen? scr = _vm.GetScreenFromCursor();
            if (scr == null)
                return;

            HoveringScreen = scr;

            //Calcuate the show-up position (xAws, yAws)
            double xAws = (double)_vm.xCursor - _vm.cxAws / 2;
            double yAws = (double)_vm.yCursor - ArrangeVM.dyAwsShow - _vm.cyAws;

            _vm.xAws = xAws / _vm.ScreenScale;
            _vm.yAws = yAws / _vm.ScreenScale;

            Rect rcAws = new Rect(xAws, yAws, _vm.cxAws * _vm.ScreenScale, _vm.cyAws * _vm.ScreenScale);

            //If AwsWindow.Left outside Screen.Bound
            if (rcAws.Left <= scr.Bounds.Left)
                _vm.xAws = scr.Bounds.Left + leftMargin;
            //If AwsWindow.Right outside Screen.Bound
            if (rcAws.Right >= scr.Bounds.Right)
                _vm.xAws = scr.Bounds.Right -_vm.cxAws - rightMargin;
            //If AwsWindow.Top outside Screen.Bound
            if (rcAws.Top <= scr.Bounds.Top)
                _vm.yAws = scr.Bounds.Top + topMargin;
            //If AwsWindow.Bottom over bottom margin of Screen.Bound
            if (rcAws.Bottom >= (scr.Bounds.Bottom - bottomMargin))
                _vm.yAws= scr.Bounds.Bottom - _vm.cyAws - bottomMargin;

            //Move AWS Window
            this.Dispatcher.Invoke(() =>
            {
                //this.Visibility = Visibility.Visible;
                Left = _vm.xAws;
                Top = _vm.yAws;
                Topmost = true;
            });

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //Hide window from Alt+tab
            System.Windows.Interop.WindowInteropHelper wndHelper = new System.Windows.Interop.WindowInteropHelper(this);
            Win32Lib.Win32.HideWinFromAltTab(wndHelper.Handle);

            _vm.AwsIcon1 = new SplitCtrl2A();
            _vm.AwsIcon2 = new SplitCtrl2C();
            _vm.AwsIcon3 = new SplitCtrl3E();
            _vm.AwsIcon4 = new SplitCtrl4A();

            eSplitModes eSplitModes = eSplitModes.AWS;
            _vm.AwsIcon1.SplitMode = eSplitModes;
            _vm.AwsIcon2.SplitMode = eSplitModes;
            _vm.AwsIcon3.SplitMode = eSplitModes;
            _vm.AwsIcon4.SplitMode = eSplitModes;
        }

        public ISplitCtrl? HoveringSplit { get; private set; } = null;

        public CellObj? DetermineHoveringCellObj(int x, int y)
        {
            bool isHandled = false;

            DpiScale dpiScale = VisualTreeHelper.GetDpi(this);
            double scale = dpiScale.PixelsPerDip;

            _vm.AwsIcon1.HoveringCell = "";
            _vm.AwsIcon2.HoveringCell = "";
            _vm.AwsIcon3.HoveringCell = "";
            _vm.AwsIcon4.HoveringCell = "";

            //if (_vm.AwsIcon1.CtrlClass.Equals("SplitCtrl2A"))
            //{
            //    SplitCtrl2A ctrl = _vm.AwsIcon1 as SplitCtrl2A;
            //    foreach (CellBorder cellBd in ctrl.CellBorders)
            //    {
            //        if (cellBd.rect.Contains(x, y))
            //        {
            //            _vm.AwsIcon1.HoveringCell = cellBd.CellName;
            //            HoveringSplit = _vm.AwsIcon1;
            //            cellBd.IsHover = true;
            //        }
            //        else
            //        {
            //            cellBd.IsHover = false;
            //        }
            //    }
            //}
            foreach (CellObj objCell in _vm.AwsIcon1.CellList)
            {
                if (objCell.rc.Contains(x, y))
                {
                    _vm.AwsIcon1.HoveringCell = objCell.Name;
                    HoveringSplit = _vm.AwsIcon1;
                    _rcHoveringIcon = _rcIcon1;
                    _rcHoveringCell = objCell.rc;
                    return objCell;
                }
            }
            //if (_vm.AwsIcon2.CtrlClass.Equals("SplitCtrl2C"))
            //{
            //    SplitCtrl2C ctrl = _vm.AwsIcon2 as SplitCtrl2C;
            //    foreach (CellBorder cellBd in ctrl.CellBorders)
            //    {
            //        if (cellBd.rect.Contains(x, y))
            //        {
            //            _vm.AwsIcon1.HoveringCell = cellBd.CellName;
            //            cellBd.IsHover = true;
            //        }
            //        else
            //        {
            //            cellBd.IsHover = false;
            //        }
            //    }
            //}

            foreach (CellObj objCell in _vm.AwsIcon2.CellList)
            {
                if (objCell.rc.Contains(x, y))
                {
                    _vm.AwsIcon2.HoveringCell = objCell.Name;
                    HoveringSplit = _vm.AwsIcon2;
                    _rcHoveringIcon = _rcIcon2;
                    _rcHoveringCell = objCell.rc;
                    return objCell;
                }
            }
            foreach (CellObj objCell in _vm.AwsIcon3.CellList)
            {
                if (objCell.rc.Contains(x, y))
                {
                    _vm.AwsIcon3.HoveringCell = objCell.Name;
                    HoveringSplit = _vm.AwsIcon3;
                    _rcHoveringIcon = _rcIcon3;
                    _rcHoveringCell = objCell.rc;
                    return objCell;
                }
            }
            foreach (CellObj objCell in _vm.AwsIcon4.CellList)
            {
                if (objCell.rc.Contains(x, y))
                {
                    _vm.AwsIcon4.HoveringCell = objCell.Name;
                    HoveringSplit = _vm.AwsIcon4;
                    _rcHoveringIcon = _rcIcon4;
                    _rcHoveringCell = objCell.rc;
                    return objCell;
                }
            }
            HoveringSplit = null;
            return null;
        }

        private Rect _rcIcon1 = new Rect();
        private Rect _rcIcon2 = new Rect();
        private Rect _rcIcon3 = new Rect();
        private Rect _rcIcon4 = new Rect();
        private Rect _rcHoveringIcon = new Rect();
        private Rect _rcHoveringCell = new Rect();

        public void RefreshCellRects()
        {
            this.Dispatcher.Invoke(() =>
            {
                _rcIcon1 = GetFrameworkElementRect(_vm.AwsIcon1.UC);
                _rcIcon2 = GetFrameworkElementRect(_vm.AwsIcon2.UC);
                _rcIcon3 = GetFrameworkElementRect(_vm.AwsIcon3.UC);
                _rcIcon4 = GetFrameworkElementRect(_vm.AwsIcon4.UC);

                if (_vm.AwsIcon1.CtrlClass.Equals("SplitCtrl2A"))
                {
                    SplitCtrl2A ctrl = _vm.AwsIcon1 as SplitCtrl2A;
                    foreach (CellBorder cellBd in ctrl.CellBorders)
                    {
                        cellBd.rect = GetFrameworkElementRect(cellBd);
                    }
                }
                foreach (CellObj objCell in _vm.AwsIcon1.CellList)
                {
                    if (objCell.bd == null)
                        continue;

                    objCell.rc = GetBorderRect(objCell.bd);
                    //Trace.WriteLine($"Cell({objCell.Name})={ArrangeVM.FormatRect(objCell.rc)}");
                }
                if (_vm.AwsIcon2.CtrlClass.Equals("SplitCtrl2C"))
                {
                    SplitCtrl2C ctrl = _vm.AwsIcon2 as SplitCtrl2C;
                    foreach (CellBorder cellBd in ctrl.CellBorders)
                    {
                        cellBd.rect = GetFrameworkElementRect(cellBd);
                    }
                }
                foreach (CellObj objCell in _vm.AwsIcon2.CellList)
                {
                    if (objCell.bd == null)
                        continue;

                    objCell.rc = GetBorderRect(objCell.bd);
                    //Trace.WriteLine($"Cell({objCell.Name})={ArrangeVM.FormatRect(objCell.rc)}");
                }
                foreach (CellObj objCell in _vm.AwsIcon3.CellList)
                {
                    if (objCell.bd == null)
                        continue;

                    objCell.rc = GetBorderRect(objCell.bd);
                    //Trace.WriteLine($"Cell({objCell.Name})={ArrangeVM.FormatRect(objCell.rc)}");
                }
                foreach (CellObj objCell in _vm.AwsIcon4.CellList)
                {
                    if (objCell.bd == null)
                        continue;

                    objCell.rc = GetBorderRect(objCell.bd);
                    //Trace.WriteLine($"Cell({objCell.Name})={ArrangeVM.FormatRect(objCell.rc)}");
                }
            });
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
            double w = ctrl.ActualWidth * _vm.ScreenScale;
            double h = ctrl.ActualHeight * _vm.ScreenScale;
            Trace.WriteLine($"ctrlActual={ctrl.ActualWidth}x{ctrl.ActualHeight}; Scale={_vm.ScreenScale} => {w}x{h}");
            return new Rect(ptTopLeft.X, ptTopLeft.Y, w, h);
        }

        private Rect GetFrameworkElementRect(FrameworkElement ele)
        {
            if (ele == null)
                return Rect.Empty;

            if ((ele.ActualWidth == 0) && (ele.ActualHeight == 0))
                return Rect.Empty;

            PresentationSource preSrc = PresentationSource.FromVisual(ele);
            if (preSrc == null)
                return Rect.Empty;

            System.Windows.Point ptTopLeft = ele.PointToScreen(new System.Windows.Point(0, 0));
            double w = ele.ActualWidth * _vm.ScreenScale;
            double h = ele.ActualHeight * _vm.ScreenScale;
            Trace.WriteLine($"ctrlActual={ele.ActualWidth}x{ele.ActualHeight}; Scale={_vm.ScreenScale} => {w}x{h}");
            return new Rect(ptTopLeft.X, ptTopLeft.Y, w, h);
        }
        
        public Rect CalculateHoveringCellArrangeRect()
        {
            if (HoveringScreen == null)
                return Rect.Empty;
            if (_rcHoveringIcon.IsEmpty)
                return Rect.Empty;
            if (_rcHoveringCell.IsEmpty)
                return Rect.Empty;

            Rect rcOut = new Rect();
            rcOut.X = HoveringScreen.WorkingArea.Left +
                (_rcHoveringCell.Left - _rcHoveringIcon.Left) / (_rcHoveringIcon.Width) * HoveringScreen.WorkingArea.Width;
            rcOut.Y = HoveringScreen.WorkingArea.Top +
                (_rcHoveringCell.Top - _rcHoveringIcon.Top) / (_rcHoveringIcon.Height) * HoveringScreen.WorkingArea.Height;
            rcOut.Width = _rcHoveringCell.Width / _rcHoveringIcon.Width * HoveringScreen.WorkingArea.Width;
            rcOut.Height = _rcHoveringCell.Height / _rcHoveringIcon.Height * HoveringScreen.WorkingArea.Height;
            return rcOut;
        }
    }
}