using DDPM.Easy.Common;
using DDPM.SA.Common.Display;
using DDPM.SA.Common.Settings;
using Dell.Client.Framework.Common;
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

        //AwsWindow Rect on VirtualScreen
        private Rect _rcAwsWindow = new Rect();

        //5 Icons
        private Rect _rcIcon0 = new Rect();
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

        private const double _topMargin = 16;
        private const double _bottomMargin = 128;
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

            //Fix left if it across screen boundary
            //
            if (left < (scr.Bounds.Left + _leftMargin))
            {
                left = scr.Bounds.Left + _leftMargin;
                Trace.WriteLine("Fix left side");
            }

            //Fix right
            double rightBound = scr.Bounds.Right / _vm.ScreenScale - _rightMargin - _cxAwsWindow;
            //rightMargin = rightBound / _vm.ScreenScale;
            if (left > rightBound)
            {
                left = rightBound;
                Trace.WriteLine("Fix right side");
            }

            //If AwsWindow.Top outside Screen.Bound
            double topBound = scr.Bounds.Top / _vm.ScreenScale + _topMargin;
            if (top <= topBound)
                top = topBound;

            //Fix bottom
            double bottomBound = scr.Bounds.Bottom / _vm.ScreenScale - _bottomMargin - _cyAwsWindow;
            //If AwsWindow.Bottom over bottom margin of Screen.Bound
            if (top >= bottomBound)
                top = bottomBound;



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
            _vm.WriteLog($"@ AwsWindow.ReloadRecentList({screenDeviceName})");
            Stopwatch sw0 = Stopwatch.StartNew();

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
                    Stopwatch sw1 = Stopwatch.StartNew();
                    EAMonitorSettings eaSettings = _vm.ReadEAMonitorSettings(attachedMonitors[0]);
                    sw1.Stop();
                    _vm.WriteLog($"  * ReadEAMonitorSettings() elapsed {sw1.ElapsedMilliseconds} msec.");

                    if (eaSettings != null)
                    {
                        if (eaSettings.RecentList != null)
                        {
                            Stopwatch sw2 = Stopwatch.StartNew();
                            isRecentListLoaded = _vm.RefreshAwsIconsFromRecentList(eaSettings.RecentList);
                            sw2.Stop();
                            _vm.WriteLog($"  * RefreshAwsIconsFromRecentList() elapsed {sw2.ElapsedMilliseconds} msec.");

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
                sw0.Stop();
                _vm.WriteLog($"  * AwsWindow.ReloadRecentList() elapsed {sw0.ElapsedMilliseconds} msec.");

                Dispatcher_RefreshCellRects();
            });
        }

        private void RefreshAwsIconRects()
        {
            _rcIcon0 = _vm.GetFrameworkElementRect(_vm.AwsIcon0.UC);
            _rcIcon1 = _vm.GetFrameworkElementRect(_vm.AwsIcon1.UC);
            _rcIcon2 = _vm.GetFrameworkElementRect(_vm.AwsIcon2.UC);
            _rcIcon3 = _vm.GetFrameworkElementRect(_vm.AwsIcon3.UC);
            _rcIcon4 = _vm.GetFrameworkElementRect(_vm.AwsIcon4.UC);
        }

        private void RefreshCellBordersInAwsIcons()
        {
            if (_vm.AwsIcon0 != null)
            {
                Rect rcIcon = _vm.GetFrameworkElementRect(_vm.AwsIcon0.UC);
                if (rcIcon.IsEmpty)
                {
                    _areCellRectsRefreshed = false;
                }
                else
                {
                    SplitCtrl0B splitCtrl0B = (SplitCtrl0B)_vm.AwsIcon0;
                    splitCtrl0B.ApplySettingsToCellList(new Rect(rcIcon.Left, rcIcon.Top, rcIcon.Width, rcIcon.Height));
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
            if (_vm.AwsIcon3.IsAddedCustomLayout)
            {
                Rect rcIcon = _vm.GetFrameworkElementRect(_vm.AwsIcon3.UC);
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

            if (_vm.AwsIcon4.IsAddedCustomLayout)
            {
                Rect rcIcon = _vm.GetFrameworkElementRect(_vm.AwsIcon4.UC);
                if (!rcIcon.IsEmpty)
                {
                    SplitCtrl0B splitCtrl0B = (SplitCtrl0B)_vm.AwsIcon4;
                    splitCtrl0B.ApplySettingsToCellList(new Rect(rcIcon.Left, rcIcon.Top, rcIcon.Width, rcIcon.Height));
                }
                else
                {
                    _areCellRectsRefreshed = false;
                }
            }

        }
        #endregion

        #region Hovering

         public CellObj? DetermineHoveringCellObj(int x, int y)
         {
            bool isHandled = false;
            _vm.WriteLog($"@ DetermineHoveringCellObj({x},{y}), _areCellRectsRefreshed={_areCellRectsRefreshed}");

            if (! _vm.AreAwsIconsLoaded)
            {
                _vm.WriteLog($"@ DetermineHoveringCellObj, AwsIcons are not loaded.");
                _vm.AwsWindowHoverMsg = "AwsIcons are not loaded";
                return null;
            }

            if (!IsCursorInsideAwsWindow(x, y))
            {
                _vm.AwsWindowHoverMsg = $"Cursor({x},{y}) not inside AwsWindow";
                return null;
            }


            DpiScale dpiScale = VisualTreeHelper.GetDpi(this);
            double scale = dpiScale.PixelsPerDip;
            CellObj? hoverCell = null;

            _vm.AwsIcon0.HoveringCell = "";
            _vm.AwsIcon1.HoveringCell = "";
            _vm.AwsIcon2.HoveringCell = "";
            _vm.AwsIcon3.HoveringCell = "";
            _vm.AwsIcon4.HoveringCell = "";

            //
            //  AWSIcon0
            //
            hoverCell = DeterminAwsIconHoveringCellObj(_vm.AwsIcon0, x, y);
            if (hoverCell != null)
            {
                _vm.HoveringAwsIcon = _vm.AwsIcon0;
                _vm.AwsIcon0.HoveringCell = hoverCell.Name;
                _rcHoveringIcon = _rcIcon0;
                _rcHoveringCell = hoverCell.rc;
                _vm.AwsWindowHoverMsg = $"Hovering Icon0.{hoverCell.Name}";

                //_vm.AwsIcon0.HoveringCell = "";
                _vm.AwsIcon1.HoveringCell = "";
                _vm.AwsIcon2.HoveringCell = "";
                _vm.AwsIcon3.HoveringCell = "";
                _vm.AwsIcon4.HoveringCell = "";
                return hoverCell;
            }

            //
            //  AWSIcon1
            //
            hoverCell = DeterminAwsIconHoveringCellObj(_vm.AwsIcon1, x, y);
            if (hoverCell != null)
            {
                _vm.HoveringAwsIcon = _vm.AwsIcon1;
                _vm.AwsIcon1.HoveringCell = hoverCell.Name;
                _rcHoveringIcon = _rcIcon1;
                _rcHoveringCell = hoverCell.rc;
                _vm.AwsWindowHoverMsg = $"Hovering Icon1.{hoverCell.Name}";

                _vm.AwsIcon0.HoveringCell = "";
                //_vm.AwsIcon1.HoveringCell = "";
                _vm.AwsIcon2.HoveringCell = "";
                _vm.AwsIcon3.HoveringCell = "";
                _vm.AwsIcon4.HoveringCell = "";

                return hoverCell;
            }

            /*
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
            if (_vm.AwsIcon1.IsAddedCustomLayout)
            {
                SplitCtrl0B sp0B = (SplitCtrl0B)_vm.AwsIcon1;
                foreach (CellBorder cb in sp0B.CellBorders)
                {
                    if (hoverCell == null)
                    {
                        if (cb.rect.Contains(x, y))
                        {
                            cb.Dispatcher_SetIsHover(true);

                            hoverCell = new CellObj(cb.CellName);
                            hoverCell.rc = cb.rect;

                            _vm.AwsIcon1.HoveringCell = cb.CellName;
                            _vm.HoveringAwsIcon = _vm.AwsIcon1;
                            _rcHoveringIcon = _rcIcon1;
                            _rcHoveringCell = cb.rect;
                        }
                        else
                        {
                            cb.Dispatcher_SetIsHover(false);
                        }
                    }
                    else
                    {
                        cb.Dispatcher_SetIsHover(false);
                    }
                }
            }
            */


            //
            //  AWSIcon2
            //
            hoverCell = DeterminAwsIconHoveringCellObj(_vm.AwsIcon2, x, y);
            if (hoverCell != null)
            {
                _vm.HoveringAwsIcon = _vm.AwsIcon2;
                _vm.AwsIcon2.HoveringCell = hoverCell.Name;
                _rcHoveringIcon = _rcIcon2;
                _rcHoveringCell = hoverCell.rc;
                _vm.AwsWindowHoverMsg = $"Hovering Icon2.{hoverCell.Name}";

                _vm.AwsIcon0.HoveringCell = "";
                _vm.AwsIcon1.HoveringCell = "";
                //_vm.AwsIcon2.HoveringCell = "";
                _vm.AwsIcon3.HoveringCell = "";
                _vm.AwsIcon4.HoveringCell = "";
                return hoverCell;
            }
            /*
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
            if (_vm.AwsIcon2.IsAddedCustomLayout)
            {
                SplitCtrl0B sp0B = (SplitCtrl0B)_vm.AwsIcon2;
                foreach (CellBorder cb in sp0B.CellBorders)
                {
                    if (hoverCell == null)
                    {
                        if (cb.rect.Contains(x, y))
                        {
                            cb.Dispatcher_SetIsHover(true);

                            hoverCell = new CellObj(cb.CellName);
                            hoverCell.rc = cb.rect;

                            _vm.AwsIcon2.HoveringCell = cb.CellName;
                            _vm.HoveringAwsIcon = _vm.AwsIcon2;
                            _rcHoveringIcon = _rcIcon2;
                            _rcHoveringCell = cb.rect;
                        }
                        else
                        {
                            cb.Dispatcher_SetIsHover(false);
                        }
                    }
                    else
                    {
                        cb.Dispatcher_SetIsHover(false);
                    }
                }
            }
            */

            //
            //  AWSIcon3
            //
            hoverCell = DeterminAwsIconHoveringCellObj(_vm.AwsIcon3, x, y);
            if (hoverCell != null)
            {
                _vm.HoveringAwsIcon = _vm.AwsIcon3;
                _vm.AwsIcon3.HoveringCell = hoverCell.Name;
                _rcHoveringIcon = _rcIcon3;
                _rcHoveringCell = hoverCell.rc;
                _vm.AwsWindowHoverMsg = $"Hovering Icon3.{hoverCell.Name}";

                _vm.AwsIcon0.HoveringCell = "";
                _vm.AwsIcon1.HoveringCell = "";
                _vm.AwsIcon2.HoveringCell = "";
                //_vm.AwsIcon3.HoveringCell = "";
                _vm.AwsIcon4.HoveringCell = "";

                return hoverCell;
            }
            /*
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
            if (_vm.AwsIcon3.IsAddedCustomLayout)
            {
                SplitCtrl0B sp0B = (SplitCtrl0B)_vm.AwsIcon3;
                foreach (CellBorder cb in sp0B.CellBorders)
                {
                    if (hoverCell == null)
                    {
                        if (cb.rect.Contains(x, y))
                        {
                            cb.Dispatcher_SetIsHover(true);

                            hoverCell = new CellObj(cb.CellName);
                            hoverCell.rc = cb.rect;

                            _vm.AwsIcon3.HoveringCell = cb.CellName;
                            _vm.HoveringAwsIcon = _vm.AwsIcon3;
                            _rcHoveringIcon = _rcIcon3;
                            _rcHoveringCell = cb.rect;
                        }
                        else
                        {
                            cb.Dispatcher_SetIsHover(false);
                        }
                    }
                    else
                    {
                        cb.Dispatcher_SetIsHover(false);
                    }
                }
            }
            */

            //
            //  AWSIcon4
            //
            hoverCell = DeterminAwsIconHoveringCellObj(_vm.AwsIcon4, x, y);
            if (hoverCell != null)
            {
                _vm.HoveringAwsIcon = _vm.AwsIcon4;
                _vm.AwsIcon4.HoveringCell = hoverCell.Name;
                _rcHoveringIcon = _rcIcon4;
                _rcHoveringCell = hoverCell.rc;
                _vm.AwsWindowHoverMsg = $"Hovering Icon4.{hoverCell.Name}";

                _vm.AwsIcon0.HoveringCell = "";
                _vm.AwsIcon1.HoveringCell = "";
                _vm.AwsIcon2.HoveringCell = "";
                _vm.AwsIcon3.HoveringCell = "";
                //_vm.AwsIcon4.HoveringCell = "";

                return hoverCell;
            }
            /*
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
            if (_vm.AwsIcon4.IsAddedCustomLayout)
            {
                SplitCtrl0B sp0B = (SplitCtrl0B)_vm.AwsIcon4;
                foreach (CellBorder cb in sp0B.CellBorders)
                {
                    if (hoverCell == null)
                    {
                        if (cb.rect.Contains(x, y))
                        {
                            cb.Dispatcher_SetIsHover(true);

                            hoverCell = new CellObj(cb.CellName);
                            hoverCell.rc = cb.rect;

                            _vm.AwsIcon4.HoveringCell = cb.CellName;
                            _vm.HoveringAwsIcon = _vm.AwsIcon4;
                            _rcHoveringIcon = _rcIcon4;
                            _rcHoveringCell = cb.rect;
                        }
                        else
                        {
                            cb.Dispatcher_SetIsHover(false);
                        }
                    }
                    else
                    {
                        cb.Dispatcher_SetIsHover(false);
                    }
                }
            }
            */
            if (hoverCell == null)
                _vm.HoveringAwsIcon = null;

            _vm.AwsIcon0.HoveringCell = "";
            _vm.AwsIcon1.HoveringCell = "";
            _vm.AwsIcon2.HoveringCell = "";
            _vm.AwsIcon3.HoveringCell = "";
            _vm.AwsIcon4.HoveringCell = "";
            _vm.AwsWindowHoverMsg = $"No hovering Cell detected.";
            return hoverCell;
        }

        private CellObj? DeterminAwsIconHoveringCellObj(ISplitCtrl awsIcon, int x, int y)
        {
            CellObj? hoverCell = null;

            if (awsIcon.IsAddedCustomLayout)
            {
                SplitCtrl0B sp0B = (SplitCtrl0B)awsIcon;
                foreach(CellObj objCell in awsIcon.CellList)
                {
                    if (hoverCell == null)
                    {
                        if (objCell.rc.Contains(x, y))
                        {
                            objCell.CellBd.Dispatcher_SetIsHover(true);

                            hoverCell = objCell;
                            hoverCell.rc = objCell.rc;

                            _vm.AwsIcon1.HoveringCell = objCell.Name;
                            _vm.HoveringAwsIcon = awsIcon;
                            //_rcHoveringIcon = _rcIcon1;
                            _rcHoveringCell = objCell.rc;
                        }
                        else
                        {
                            objCell.CellBd.Dispatcher_SetIsHover(false);
                        }
                    }
                    else
                    {
                        objCell.CellBd.Dispatcher_SetIsHover(false);
                    }
                }
                //foreach (CellBorder cb in sp0B.CellBorders)
                //{
                //    if (hoverCell == null)
                //    {
                //        if (cb.rect.Contains(x, y))
                //        {
                //            cb.Dispatcher_SetIsHover(true);

                //            hoverCell = new CellObj(cb.CellName);
                //            hoverCell.rc = cb.rect;

                //            _vm.AwsIcon1.HoveringCell = cb.CellName;
                //            _vm.HoveringAwsIcon = _vm.AwsIcon1;
                //            _rcHoveringIcon = _rcIcon1;
                //            _rcHoveringCell = cb.rect;
                //        }
                //        else
                //        {
                //            cb.Dispatcher_SetIsHover(false);
                //        }
                //    }
                //    else
                //    {
                //        cb.Dispatcher_SetIsHover(false);
                //    }
                //}
            }
            else //if (awsIcon.CellCount == 5)
            {
                foreach (CellObj objCell in awsIcon.CellList)
                {
                    if (objCell.rc.Contains(x, y))
                    {
                        hoverCell = objCell;
                        return objCell;
                    }
                }
            }
            //else
            //{
            //    foreach (CellObj objCell in awsIcon.CellList)
            //    {
            //        if (objCell.rc.Contains(x, y))
            //        {
            //            hoverCell = objCell;
            //            return objCell;
            //        }
            //    }
            //}
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

        private bool IsCursorInsideAwsWindow(int x, int y)
        {
            if (_vm.rcAwsWindow.IsEmpty)
                return false;
            return _vm.rcAwsWindow.Contains(x, y);
        }
        #endregion Hovering

        #region RefreshCellRects
        //Workaround flag to call RefreshCellRects() again, until this flag set true
        private bool _areCellRectsRefreshed = false;

        public void RefreshCellRects(int flag = 0)
        {
            this.Dispatcher.Invoke(() =>
            {
                Dispatcher_RefreshCellRects(flag);

                if (_vm.rcAwsWindow.IsEmpty)
                {
                    _vm.rcAwsWindow = _vm.GetFrameworkElementRect(this);
                }
            });
        }

        private void Dispatcher_RefreshCellRects(int flag = 0)
        {
            _areCellRectsRefreshed = true;
            if (!_vm.AreAwsIconsLoaded)
            {
                Trace.WriteLine("@ Dispatcher_RefreshCellRects(), AwsIcons are not loaded");
                return;
            }

            Trace.WriteLine("@ Dispatcher_RefreshCellRects()");

            if (!UI_RefreshAwsIconCellRects(_vm.AwsIcon0))
            {
                _areCellRectsRefreshed = false;
            }




            _rcIcon1 = _vm.GetFrameworkElementRect(_vm.AwsIcon1.UC);
            _vm.WriteLog($"@ RefreshCellRects() - Icon1: {ArrangeVM.FormatRect(_rcIcon1)}");

            /*
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
                //Rect rcIcon = _vm.GetFrameworkElementRect(_vm.AwsIcon1.UC);
                //if (rcIcon.IsEmpty)
                //{
                //    _areCellRectsRefreshed = false;
                //}
                //else
                //{
                //    SplitCtrl0B splitCtrl0B = (SplitCtrl0B)_vm.AwsIcon1;
                //    splitCtrl0B.ApplySettingsToCellList(new Rect(rcIcon.Left, rcIcon.Top, rcIcon.Width, rcIcon.Height));
                //}
                SplitCtrl0B splitCtrl0B = (SplitCtrl0B)_vm.AwsIcon1;
                foreach (CellBorder cb in splitCtrl0B.CellBorders)
                {
                    cb.rect = _vm.GetFrameworkElementRect(cb);
                }
            }
            */
            if (!UI_RefreshAwsIconCellRects(_vm.AwsIcon1))
            {
                _areCellRectsRefreshed = false;
            }

            _rcIcon2 = _vm.GetFrameworkElementRect(_vm.AwsIcon2.UC);
            _vm.WriteLog($"@ RefreshCellRects() - Icon2: {ArrangeVM.FormatRect(_rcIcon2)}");

            /*
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
                //Rect rcIcon = _vm.GetFrameworkElementRect(_vm.AwsIcon2.UC);
                //if (!rcIcon.IsEmpty)
                //{
                //    SplitCtrl0B splitCtrl0B = (SplitCtrl0B)_vm.AwsIcon2;
                //    splitCtrl0B.ApplySettingsToCellList(new Rect(rcIcon.Left, rcIcon.Top, rcIcon.Width, rcIcon.Height));
                //}
                //else
                //{
                //    _areCellRectsRefreshed = false;
                //}
                SplitCtrl0B splitCtrl0B = (SplitCtrl0B)_vm.AwsIcon2;
                foreach (CellBorder cb in splitCtrl0B.CellBorders)
                {
                    cb.rect = _vm.GetFrameworkElementRect(cb);
                }
            }
            */
            if (!UI_RefreshAwsIconCellRects(_vm.AwsIcon2))
            {
                _areCellRectsRefreshed = false;
            }


            _rcIcon3 = _vm.GetFrameworkElementRect(_vm.AwsIcon3.UC);
            _vm.WriteLog($"@ RefreshCellRects() - Icon3: {ArrangeVM.FormatRect(_rcIcon3)}");

            /*
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
                //Rect rcIcon = _vm.GetFrameworkElementRect(_vm.AwsIcon3.UC);
                //if (!rcIcon.IsEmpty)
                //{
                //    SplitCtrl0B splitCtrl0B = (SplitCtrl0B)_vm.AwsIcon3;
                //    splitCtrl0B.ApplySettingsToCellList(new Rect(rcIcon.Left, rcIcon.Top, rcIcon.Width, rcIcon.Height));
                //}
                //else
                //{
                //    _areCellRectsRefreshed = false;
                //}
                SplitCtrl0B splitCtrl0B = (SplitCtrl0B)_vm.AwsIcon3;
                foreach (CellBorder cb in splitCtrl0B.CellBorders)
                {
                    cb.rect = _vm.GetFrameworkElementRect(cb);
                }
            }
            */
            if (!UI_RefreshAwsIconCellRects(_vm.AwsIcon3))
            {
                _areCellRectsRefreshed = false;
            }

            _rcIcon4 = _vm.GetFrameworkElementRect(_vm.AwsIcon4.UC);
            _vm.WriteLog($"@ RefreshCellRects() - Icon4: {ArrangeVM.FormatRect(_rcIcon4)}");

            /*
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
                //Rect rcIcon = _vm.GetFrameworkElementRect(_vm.AwsIcon4.UC);
                //if (!rcIcon.IsEmpty)
                //{
                //    SplitCtrl0B splitCtrl0B = (SplitCtrl0B)_vm.AwsIcon4;
                //    splitCtrl0B.ApplySettingsToCellList(new Rect(rcIcon.Left, rcIcon.Top, rcIcon.Width, rcIcon.Height));
                //}
                //else
                //{
                //    _areCellRectsRefreshed = false;
                //}
                SplitCtrl0B splitCtrl0B = (SplitCtrl0B)_vm.AwsIcon4;
                foreach (CellBorder cb in splitCtrl0B.CellBorders)
                {
                    cb.rect = _vm.GetFrameworkElementRect(cb);
                }
            }
            */
            if (!UI_RefreshAwsIconCellRects(_vm.AwsIcon4))
            {
                _areCellRectsRefreshed = false;
            }

            if (!_areCellRectsRefreshed)
            {
                if (flag == 0)
                {
                    System.Threading.Timer timer1 = new System.Threading.Timer((obj) => { RefreshCellRects(1); }, null, 100, Timeout.Infinite);
                }
            }
            _vm.OnPropertyChanged_AwsIconInfos();
        }

        private bool UI_RefreshAwsIconCellRects(ISplitCtrl awsIcon)
        {
            bool _areCellRectsRefreshed = true;

            if (awsIcon.IsAddedCustomLayout)
            {
                SplitCtrl0B splitCtrl0B = (SplitCtrl0B)awsIcon;
                foreach(CellObj objCell in splitCtrl0B.CellList)
                {
                    objCell.rc = _vm.GetFrameworkElementRect(objCell.CellBd);
                    if (objCell.rc.IsEmpty)
                    {
                        _areCellRectsRefreshed = false;
                    }
                }
                //foreach (CellBorder cb in splitCtrl0B.CellBorders)
                //{
                //    cb.rect = _vm.GetFrameworkElementRect(cb);
                //    if (cb.rect.IsEmpty)
                //    {
                //        _areCellRectsRefreshed = false;
                //    }
                //}
            }
            else
            {
                foreach (CellObj objCell in awsIcon.CellList)
                {
                    if (objCell.CellBd == null)
                        continue;

                    objCell.rc = _vm.GetFrameworkElementRect(objCell.CellBd);
                    if (objCell.rc.IsEmpty)
                    {
                        _areCellRectsRefreshed = false;
                    }
                }
            }
            //else
            //{
            //    foreach (CellObj objCell in awsIcon.CellList)
            //    {
            //        if (objCell.bd == null)
            //            continue;

            //        objCell.rc = _vm.GetFrameworkElementRect(objCell.bd);
            //        if (objCell.rc.IsEmpty)
            //        {
            //            _areCellRectsRefreshed = false;
            //        }
            //    }
            //}
            return _areCellRectsRefreshed;
        }


        #endregion

        #region ViewModel Event Handlers
        private void HandleAwsWindowVisibilityChanged(object? sender, bool isVisible)
        {
            _vm.WriteLog($"@AwsWindow.HandleAwsWindowVisibilityChanged(isVisible={isVisible})");
            if (!isVisible)
                return;

            //Get the Screen of the cursor
            Screen showScreen = _vm.GetScreenFromCursor();
            //Check if the showScreen is WorkScreen of AwsWindow
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
            RefreshAwsIconRects();
            RefreshCellBordersInAwsIcons();
            RefreshCellRects();
            RefreshIcon0();
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

        #region Icon0 - Monitors
        private void RefreshIcon0()
        {
            this.Dispatcher.Invoke(() =>
            {
                if (icon0Canvas.ActualWidth == 0)
                    return;

                System.Drawing.Rectangle rcVirtualScreen = SystemInformation.VirtualScreen;
                double cxView = 1.000;
                double cyView = 1.000;
                double ratioX = icon0Canvas.ActualWidth / (double)rcVirtualScreen.Width;
                double ratioY = icon0Canvas.ActualHeight / (double)rcVirtualScreen.Height;
                bool isHorzFit = (ratioX < ratioY);
                double ratio = ratioX; //Default is top-down
                double ratioBorder;
                if (isHorzFit)
                {
                    ratio = ratioY; //Left-right
                    icon0Canvas.Height = rcVirtualScreen.Height * icon0Canvas.ActualWidth / rcVirtualScreen.Width;
                    ratioBorder = icon0Canvas.ActualWidth / rcVirtualScreen.Width;
                }
                else
                {
                    ratio = ratioX; //top-down
                    icon0Canvas.Width = rcVirtualScreen.Width * icon0Canvas.ActualHeight / rcVirtualScreen.Height; ;
                    ratioBorder = icon0Canvas.ActualHeight / rcVirtualScreen.Height;
                }

                icon0Canvas.Children.Clear();
                _vm.AwsIcon0.CellList.Clear();
                int idxScr = 0;

                foreach (Screen scr in Screen.AllScreens)
                {
                    //AddMsg($"[{idxScr}] {scr.DeviceName} {(scr.Primary ? "Primary" : "")}");
                    //AddMsg($"   {FormatRecttangle(scr.Bounds)}");

                    //Border bd = new Border();
                    //bd.Width = scr.Bounds.Width * ratioBorder;
                    //bd.Height = scr.Bounds.Height * ratioBorder;
                    //bd.Style = FindResource("CellBorderStyle") as Style;

                    CellBorder cellBd = new CellBorder();
                    cellBd.Width = scr.Bounds.Width * ratioBorder;
                    cellBd.Height = scr.Bounds.Height * ratioBorder;

                    TextBlock text = new TextBlock();
                    text.Text = $"{idxScr + 1}";
                    text.Style = FindResource("MonitorIdTextStyle") as Style;
                    cellBd.AddChild(text);

                    double left = (scr.Bounds.Left - rcVirtualScreen.Left) * ratioBorder;
                    double top = (scr.Bounds.Top - rcVirtualScreen.Top) * ratioBorder;

                    //AddMsg($"    Border at ({left},{top}) {bd.Width}x{bd.Height}");

                    System.Windows.Point topLeft = new System.Windows.Point(left, top);
                    topLeft = icon0Canvas.PointToScreen(topLeft);
                    cellBd.rect = new Rect(topLeft.X, topLeft.Y, cellBd.Width, cellBd.Height);

                    icon0Canvas.Children.Add(cellBd);
                    Canvas.SetLeft(cellBd, left);
                    Canvas.SetTop(cellBd, top);

                    CellObj cellObj = new CellObj(text.Text, cellBd);
                    cellObj.rc = new Rect(left, top, cellBd.Width, cellBd.Height);
                    _vm.AwsIcon0.CellList.Add(cellObj);

                    if (isHorzFit)
                    {
                        icon0Canvas.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
                        icon0Canvas.VerticalAlignment = VerticalAlignment.Center;
                    }
                    else
                    {
                        icon0Canvas.VerticalAlignment = VerticalAlignment.Stretch;
                        icon0Canvas.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                    }


                    idxScr++;
                }
                _vm.OnPropertyChanged_AwsIconInfos();
            });


        }
        #endregion

        private void Window_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null)
            {
                bool isVisible = (bool)e.NewValue;
                if (isVisible)
                {
                    _vm.rcAwsWindow = _vm.GetFrameworkElementRect(this);
                }
            }

        }

        private void rootGrid_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null)
            {
                bool isVisible = (bool)e.NewValue;
                if (isVisible)
                {
                    _vm.rcAwsWindow = _vm.GetFrameworkElementRect(this);
                }
            }
        }
    }
}
