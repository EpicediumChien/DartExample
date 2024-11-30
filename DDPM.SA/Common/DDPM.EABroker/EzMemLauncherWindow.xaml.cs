using DDPM.Easy.Common;
using nsWinEventHook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
using VcpCore.Common;

namespace DDPM.EABroker
{
    /// <summary>
    /// Interaction logic for EzMemLauncherWindow.xaml
    /// </summary>
    public partial class EzMemLauncherWindow : Window
    {
        #region Private members
        private ISplitCtrl inputSplitCtrl = new SplitCtrl0A();
        #endregion
        public EzMemLauncherWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            System.Windows.Interop.WindowInteropHelper wndHelper = new System.Windows.Interop.WindowInteropHelper(this);
            Win32Lib.Win32.HideWinFromAltTab(wndHelper.Handle);
        }

        #region EzMemLaunch
        public void ShowForEzMemLauncher(MonitorInfo mi, ISplitCtrl isp)
        {
            Screen? screen = Screen.AllScreens.FirstOrDefault(x => x.DeviceName.Equals(mi.DisplayName, StringComparison.OrdinalIgnoreCase));
            if (screen == null)
                return;

            splitCtrl.Visibility = Visibility.Visible;

            inputSplitCtrl = isp.Clone();
            double screenScale = GetScreenScale();

            Rect rcScreen = new Rect();
            rcScreen.X = screen.WorkingArea.Left / screenScale;
            rcScreen.Y = screen.WorkingArea.Top / screenScale;
            rcScreen.Width = screen.WorkingArea.Width / screenScale;
            rcScreen.Height = screen.WorkingArea.Height / screenScale;

            Left = rcScreen.X;
            Top = rcScreen.Y;
            Width = rcScreen.Width;
            Height = rcScreen.Height;

            if (isp.IsOverlapCustomLayout)
            {
                SplitCtrl0B sp0B = (SplitCtrl0B)inputSplitCtrl;
                sp0B.ApplySettingsToCellList(rcScreen);
            }
            inputSplitCtrl.SplitMode = eSplitModes.Work;
            inputSplitCtrl.IsVertical = (rcScreen.Width < rcScreen.Height);
            splitCtrl.Content = inputSplitCtrl.UC;

            ContentRendered += EAEditWindow_ContenRendered;
            Topmost = true;
            Show();
        }

        private void EAEditWindow_ContenRendered(object? sender, EventArgs e)
        {
            RefreshCellRects();
        }

        private double GetScreenScale()
        {
            double screenScale = 1.000;
            var dpiXProperty = typeof(SystemParameters).GetProperty("DpiX", BindingFlags.NonPublic | BindingFlags.Static);
            if (dpiXProperty != null)
            {
                var varX = (int)dpiXProperty.GetValue(null, null);
                double dpiX = (double)varX / (double)96;
                if (dpiX >= 1.0000)
                    screenScale = dpiX;
            }
            return screenScale;
        }
        public Rect GetFrameworkElementRect(FrameworkElement ele)
        {
            if (ele == null)
                return Rect.Empty;

            if ((ele.ActualWidth == 0) && (ele.ActualHeight == 0))
                return Rect.Empty;

            PresentationSource preSrc = PresentationSource.FromVisual(ele);
            if (preSrc == null)
                return Rect.Empty;

            double screenScale = 1.000;
            var dpiXProperty = typeof(SystemParameters).GetProperty("DpiX", BindingFlags.NonPublic | BindingFlags.Static);
            if (dpiXProperty != null)
            {
                var varX = (int)dpiXProperty.GetValue(null, null);
                double dpiX = (double)varX / (double)96;
                if (dpiX >= 1.0000)
                    screenScale = dpiX;
            }

            System.Windows.Point ptTopLeft = ele.PointToScreen(new System.Windows.Point(0, 0));
            double w = ele.ActualWidth * screenScale;
            double h = ele.ActualHeight * screenScale;
            //Trace.WriteLine($"ctrlActual={ele.ActualWidth}x{ele.ActualHeight}; Scale={_vm.ScreenScale} => {w}x{h}");
            return new Rect(ptTopLeft.X, ptTopLeft.Y, w, h);
        }

        public void RefreshCellRects()
        {
            if (inputSplitCtrl == null)
            {
                return;
            }

            bool _areCellRectsRefreshed = true;
            if (inputSplitCtrl.IsAddedCustomLayout)
            {
                SplitCtrl0B sp0B = (SplitCtrl0B)inputSplitCtrl;
                foreach (CellObj objCell in sp0B.CellList)
                {
                    objCell.rc = GetFrameworkElementRect(objCell.CellBd);
                    if (objCell.rc.IsEmpty)
                        _areCellRectsRefreshed = false;
                }
            }
            else
            {
                foreach (CellObj objCell in inputSplitCtrl.CellList)
                {
                    if (objCell.CellBd == null)
                        continue;

                    objCell.rc = GetFrameworkElementRect(objCell.CellBd);

                    if (objCell.rc.IsEmpty)
                        _areCellRectsRefreshed = false;
                }
            }

            if (!_areCellRectsRefreshed)
            {
                //System.Threading.Timer timer1 = new System.Threading.Timer((obj) => { RefreshCellRects(); }, null, 100, Timeout.Infinite);
            }
            else
            {
                string d = "";
            }

        }

        public void ArrangeWindow(IntPtr hWnd, int idxCell)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (inputSplitCtrl == null)
                    return;

                int cellBoderCount = inputSplitCtrl.CellList.Count;
                if ((idxCell < 0) || (idxCell >= cellBoderCount))
                {
                    return;
                }
                CellObj celObj = inputSplitCtrl.CellList[idxCell];
                Rect rcArrange = celObj.rc;
                if (rcArrange.IsEmpty || (rcArrange.Width <= 0))
                {
                    rcArrange = GetFrameworkElementRect(celObj.CellBd);
                    if (rcArrange.IsEmpty)
                    {
                        return;
                    }
                }

                WinEventHook.SetWindowPosition(hWnd, rcArrange);
            }));

        }
        public void Dispatcher_Close()
        {
            this.Dispatcher.Invoke(() =>
            {
                Close();
            });
        }
        #endregion

    }
}
