using DDPM.Easy.Common;
using DDPM.SA.Common.Display;
using DDPM.SA.Common.Settings;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using VcpCore.Common;

namespace DDPM.EABroker
{
    /// <summary>
    /// Interaction logic for AwsWindow.xaml
    /// </summary>
    public partial class AwsWindow : Window
    {
        #region Private members
        private readonly ArrangeVM _vm;

        //Width, Height of AwsWindow (should be 788 x 134)
        private double _cxAwsWindow = 788;
        private double _cyAwsWindow = 134;

        //5 Icons
        private Rect _rcIcon1 = new Rect();
        private Rect _rcIcon2 = new Rect();
        private Rect _rcIcon3 = new Rect();
        private Rect _rcIcon4 = new Rect();
        private Rect _rcHoveringIcon = new Rect();
        private Rect _rcHoveringCell = new Rect();


        #endregion Private members

        public ISplitCtrl? HoveringSplit { get; private set; } = null;

        #region Constants
        //The margin of AwsWindow with left boundary of screen
        private const double _leftMargin = 32;
        //The margin of AwsWindow with right boundary of screen
        private const double _rightMargin = 32;

        #endregion Constants

        #region ctor / Init
        public AwsWindow(ArrangeVM vm)
        {
            InitializeComponent();
            _vm = vm;
            DataContext = _vm;
            _vm.AwsWindowVisibilityChanged += HandleAwsWindowVisibilityChanged;
            _vm.WorkScreenChanged += HandleWorkScreenChanged;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //Hide window from Alt+tab
            System.Windows.Interop.WindowInteropHelper wndHelper = new System.Windows.Interop.WindowInteropHelper(this);
            Win32Lib.Win32.HideWinFromAltTab(wndHelper.Handle);
        }
        #endregion ctor / Init

        #region Position Functions
        /// <summary>
        /// Determin the (left,top) of AwsWindow based on cusor position (_vm.xCursor,_vm.yCursor)
        /// </summary>
        /// <returns></returns>
        private System.Windows.Point CalculateAwsPosition()
        {
            //Calculate normal position
            double xCur = (double)_vm.xCursor;
            double yCur = (double)_vm.yCursor;

            xCur /= _vm.ScreenScale;
            yCur /= _vm.ScreenScale;

            double left = xCur - _cxAwsWindow / 2;
            double top = yCur - ArrangeVM.dyAwsShow - _cyAwsWindow;

            //Get current working screen
            Screen? scr = _vm.GetScreenFromCursor();
            if (scr == null)
            {
                _vm.WriteLog("  * GetScreenFromCursor() return null, cannot get Screen from Cusoro positon.");
                left /= _vm.ScreenScale;
                top /= _vm.ScreenScale;
                return new System.Windows.Point(left, top);
            }

            HoveringScreen = scr;

            //Fix left if it crosss screen boundary
            //
            if (left < (scr.Bounds.Left + _leftMargin))
            {
                left = scr.Bounds.Left + _leftMargin;
                Trace.WriteLine("Fix left side");
            }

            double rightBound = scr.Bounds.Right / _vm.ScreenScale - _rightMargin - _cxAwsWindow;
            //rightMargin = rightBound / _vm.ScreenScale;
            if (left > rightBound)
            {
                left = rightBound;
                Trace.WriteLine("Fix right side");
            }

            return new System.Windows.Point(left, top);
        }
        private void Dispatcher_MoveWindow(double x, double y)
        {
            this.Dispatcher.Invoke(() =>
            {
                //this.Visibility = Visibility.Visible;
                Left = x;
                Top = y;
                Topmost = true;
            });
        }
        #endregion

        #region Icons, RecentList
        //AwsIcons are implemented in ArrangeVM

        public void ReloadRecentList(string screenDeviceName)
        {
            //Get the MonitorInfo
            List<MonitorInfo>? attachedMonitors = _vm.GetMonitorsFromDeviceName(screenDeviceName);
            bool isSupportedMonitor = false;
            if (attachedMonitors != null)
            {
                isSupportedMonitor = attachedMonitors.Count > 0;
                _vm.WriteLog($"  * GetAttachedMonitors from screen of cursor, attachedMonitor count={attachedMonitors.Count}");
            }

            //Load RecentList to IconList
            this.Dispatcher.Invoke(() =>
            {
                bool isRecentListLoaded = false;
                if (isSupportedMonitor)
                {
                    EAMonitorSettings eaSettings = _vm.ReadEAMonitorSettings(attachedMonitors[0]);
                    if (eaSettings != null)
                    {
                        if (eaSettings.RecentList != null)
                        {
                            isRecentListLoaded = _vm.RefreshAwsIconsFromRecentList(eaSettings.RecentList);
                        }
                        else
                        {
                            _vm.WriteLog($"  * ReadSettings of Monitor(Model:{attachedMonitors[0].modelName}, ServiceTag:{attachedMonitors[0].edid.ServiceTag}) RecentList is null");
                        }
                    }
                    else
                    {
                        _vm.WriteLog($"  * ReadSettings of Monitor(Model:{attachedMonitors[0].modelName}, ServiceTag:{attachedMonitors[0].edid.ServiceTag}) return null");
                    }
                }
                if (!isRecentListLoaded)
                {
                    _vm.WriteLog("  * Cannot load RecentList from monitor, assume it\'s Non-Dell monitor, will apply defaul RecentList.");
                    //Load Win11 default Snap layout
                    isRecentListLoaded = _vm.RefreshAwsIconsFromRecentList(SplitJson.DefaultRecentList.ToArray());
                }
            });
        }
        #endregion

        #region Moving Support Functions

        //Workaround flag to call RefreshCellRects() again, until this flag set true
        private bool _areCellRectsRefreshed = false;

        public void RefreshCellRects(int flag=0)
        {
            this.Dispatcher.Invoke(() =>
            {
                Dispatcher_RefreshCellRects(flag);
            });
        }


        private void Dispatcher_RefreshCellRects(int flag=0)
        {
            _areCellRectsRefreshed = true;
            if (!_vm.AreAwsIconsLoaded)
            {
                Trace.WriteLine("@ Dispatcher_RefreshCellRects(), AwsIcons are not loaded");
                return;
            }

            Trace.WriteLine("@ Dispatcher_RefreshCellRects()");
            _rcIcon1 = _vm.GetFrameworkElementRect(_vm.AwsIcon1.UC);
            _vm.WriteLog($"@ RefreshCellRects() - Icon1: {ArrangeVM.FormatRect(_rcIcon1)}");
            foreach (CellObj objCell in _vm.AwsIcon1.CellList)
            {
                if (objCell.bd == null)
                    continue;

                objCell.rc = _vm.GetFrameworkElementRect(objCell.bd);
                if (!objCell.rc.IsEmpty)
                {
                    //_areCellRectsRefreshed &= true;
                    _vm.HoveringAwsIcon = _vm.AwsIcon1;
                }
                else
                {
                    _areCellRectsRefreshed = false;
                }
            }
            if (_vm.AwsIcon1.IsAddedCustomLayout)
            {
                Rect rcIcon = _vm.GetFrameworkElementRect(_vm.AwsIcon1.UC);
                if (rcIcon.IsEmpty)
                {
                    _areCellRectsRefreshed = false;
                }
                else
                {
                    SplitCtrl0B splitCtrl0B = (SplitCtrl0B)_vm.AwsIcon1;
                    splitCtrl0B.ApplySettingsToCellList(new Rect(rcIcon.Left, rcIcon.Top, rcIcon.Width, rcIcon.Height));
                }
            }

            _rcIcon2 = _vm.GetFrameworkElementRect(_vm.AwsIcon2.UC);
            _vm.WriteLog($"@ RefreshCellRects() - Icon2: {ArrangeVM.FormatRect(_rcIcon2)}");
            foreach (CellObj objCell in _vm.AwsIcon2.CellList)
            {
                if (objCell.bd == null)
                    continue;

                objCell.rc = _vm.GetFrameworkElementRect(objCell.bd);
                //Trace.WriteLine($"Cell({objCell.Name})={ArrangeVM.FormatRect(objCell.rc)}");
                if (!objCell.rc.IsEmpty)
                {
                    //_areCellRectsRefreshed = true;
                    _vm.HoveringAwsIcon = _vm.AwsIcon2;
                }
                else
                {
                    _areCellRectsRefreshed = false;
                }
            }
            if (_vm.AwsIcon2.IsAddedCustomLayout)
            {
                Rect rcIcon = _vm.GetFrameworkElementRect(_vm.AwsIcon2.UC);
                if (!rcIcon.IsEmpty)
                {
                    SplitCtrl0B splitCtrl0B = (SplitCtrl0B)_vm.AwsIcon2;
                    splitCtrl0B.ApplySettingsToCellList(new Rect(rcIcon.Left, rcIcon.Top, rcIcon.Width, rcIcon.Height));
                }
                else
                {
                    _areCellRectsRefreshed = false;
                }
            }

            _rcIcon3 = _vm.GetFrameworkElementRect(_vm.AwsIcon3.UC);
            _vm.WriteLog($"@ RefreshCellRects() - Icon3: {ArrangeVM.FormatRect(_rcIcon3)}");
            foreach (CellObj objCell in _vm.AwsIcon3.CellList)
            {
                if (objCell.bd == null)
                    continue;

                objCell.rc = _vm.GetFrameworkElementRect(objCell.bd);
                //Trace.WriteLine($"Cell({objCell.Name})={ArrangeVM.FormatRect(objCell.rc)}");
                if (!objCell.rc.IsEmpty)
                {
                    //_areCellRectsRefreshed = true;
                    _vm.HoveringAwsIcon = _vm.AwsIcon3;
                }
                else
                {
                    _areCellRectsRefreshed = false;
                }
            }

            if (_vm.AwsIcon3.IsAddedCustomLayout)
            {
                Rect rcIcon = _vm.GetFrameworkElementRect(_vm.AwsIcon2.UC);
                if (!rcIcon.IsEmpty)
                {
                    SplitCtrl0B splitCtrl0B = (SplitCtrl0B)_vm.AwsIcon3;
                    splitCtrl0B.ApplySettingsToCellList(new Rect(rcIcon.Left, rcIcon.Top, rcIcon.Width, rcIcon.Height));
                }
                else
                {
                    _areCellRectsRefreshed = false;
                }
            }

            _rcIcon4 = _vm.GetFrameworkElementRect(_vm.AwsIcon4.UC);
            _vm.WriteLog($"@ RefreshCellRects() - Icon4: {ArrangeVM.FormatRect(_rcIcon4)}");
            foreach (CellObj objCell in _vm.AwsIcon4.CellList)
            {
                if (objCell.bd == null)
                    continue;

                objCell.rc = _vm.GetFrameworkElementRect(objCell.bd);
                if (!objCell.rc.IsEmpty)
                {
                    //_areCellRectsRefreshed = true;
                    _vm.HoveringAwsIcon = _vm.AwsIcon4;
                }
                else
                {
                    _areCellRectsRefreshed = false;
                }

            }
            if (_vm.AwsIcon4.IsAddedCustomLayout)
            {
                Rect rcIcon = _vm.GetFrameworkElementRect(_vm.AwsIcon4.UC);
                if (!rcIcon.IsEmpty)
                {
                    SplitCtrl0B splitCtrl0B = (SplitCtrl0B)_vm.AwsIcon3;
                    splitCtrl0B.ApplySettingsToCellList(new Rect(rcIcon.Left, rcIcon.Top, rcIcon.Width, rcIcon.Height));
                }
                else
                {
                    _areCellRectsRefreshed = false;
                }
            }

            if (!_areCellRectsRefreshed)
            {
                if (flag == 0)
                {
                    System.Threading.Timer timer1 = new System.Threading.Timer((obj) => { RefreshCellRects(1); }, null, 100, Timeout.Infinite);
                }
            }
        }

         public CellObj? DetermineHoveringCellObj(int x, int y)
         {
            bool isHandled = false;
            _vm.WriteLog($"@ DetermineHoveringCellObj({x},{y}), _areCellRectsRefreshed={_areCellRectsRefreshed}");

            if (! _vm.AreAwsIconsLoaded)
            {
                _vm.WriteLog($"@ DetermineHoveringCellObj, AwsIcons are not loaded.");
                return null;
            }

            DpiScale dpiScale = VisualTreeHelper.GetDpi(this);
            double scale = dpiScale.PixelsPerDip;

            _vm.AwsIcon1.HoveringCell = "";
            _vm.AwsIcon2.HoveringCell = "";
            _vm.AwsIcon3.HoveringCell = "";
            _vm.AwsIcon4.HoveringCell = "";

            foreach (CellObj objCell in _vm.AwsIcon1.CellList)
            {
                if (objCell.rc.Contains(x, y))
                {
                    _vm.AwsIcon1.HoveringCell = objCell.Name;
                    _vm.HoveringAwsIcon = _vm.AwsIcon1;
                    _rcHoveringIcon = _rcIcon1;
                    _rcHoveringCell = objCell.rc;
                    return objCell;
                }
            }

            foreach (CellObj objCell in _vm.AwsIcon2.CellList)
            {
                if (objCell.rc.Contains(x, y))
                {
                    _vm.AwsIcon2.HoveringCell = objCell.Name;
                    _vm.HoveringAwsIcon = _vm.AwsIcon2;
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
                    _vm.HoveringAwsIcon = _vm.AwsIcon3;
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
                    _vm.HoveringAwsIcon = _vm.AwsIcon4;
                    _rcHoveringIcon = _rcIcon4;
                    _rcHoveringCell = objCell.rc;
                    return objCell;
                }
            }
            _vm.HoveringAwsIcon = null;
            return null;
        }

        public Screen HoveringScreen { get; set; }
        public Rect CalculateHoveringCellArrangeRect()
        {
            if (_vm.WorkScreen == null)
                return Rect.Empty;
            if (_rcHoveringIcon.IsEmpty)
                return Rect.Empty;
            if (_rcHoveringCell.IsEmpty)
                return Rect.Empty;

            Rect rcScreen = ArrangeVM.RectFromRectangle(_vm.WorkScreen.WorkingArea);

            Rect rcOut = new Rect();
            rcOut.X = rcScreen.Left +
                (_rcHoveringCell.Left - _rcHoveringIcon.Left) / (_rcHoveringIcon.Width) * rcScreen.Width;
            rcOut.Y = rcScreen.Top +
                (_rcHoveringCell.Top - _rcHoveringIcon.Top) / (_rcHoveringIcon.Height) * rcScreen.Height;
            rcOut.Width = _rcHoveringCell.Width / _rcHoveringIcon.Width * rcScreen.Width;
            rcOut.Height = _rcHoveringCell.Height / _rcHoveringIcon.Height * rcScreen.Height;
            return rcOut;
        }
        #endregion

        #region ViewModel Event Handlers
        private void HandleAwsWindowVisibilityChanged(object? sender, bool isVisible)
        {
            _vm.WriteLog($"@AwsWindow.HandleAwsWindowVisibilityChanged(isVisible={isVisible})");
            if (!isVisible)
                return;

            //Trace.WriteLine($"Actual={ActualWidth}x{ActualHeight}, Size={Width}x{Height}");

            _vm.WriteLog($"@ AwsWindow.HandleAwsWindowVisibilityChanged(), Cursor=({_vm.xCursor},{_vm.yCursor})");
            System.Windows.Point ptAws = CalculateAwsPosition();
            _vm.xAwsWindow = ptAws.X;
            _vm.yAwsWindow = ptAws.Y;

            Dispatcher_MoveWindow(ptAws.X, ptAws.Y);

            //Get current working screen
            Screen? scr = _vm.GetScreenFromCursor();
            if (scr != null)
            {
                ReloadRecentList(scr.DeviceName);
            }


        }

        private void HandleWorkScreenChanged(object? sender, Screen newScreen)
        {
            _vm.WriteLog($"@AwsWindow.HandleWorkScreenChanged(newScreen={newScreen.DeviceName})");

            //Trace.WriteLine($"Actual={ActualWidth}x{ActualHeight}, Size={Width}x{Height}");

            _vm.WriteLog($"@ AwsWindow.HandleWorkScreenChanged(), Cursor=({_vm.xCursor},{_vm.yCursor})");
            System.Windows.Point ptAws = CalculateAwsPosition();
            _vm.xAwsWindow = ptAws.X;
            _vm.yAwsWindow = ptAws.Y;

            Dispatcher_MoveWindow(ptAws.X, ptAws.Y);

            //Get current working screen
            Screen? scr = _vm.GetScreenFromCursor();
            if (scr != null)
            {
                ReloadRecentList(scr.DeviceName);
            }
        }

        #endregion ViewModel Event Handlers
    }
}
