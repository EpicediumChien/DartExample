using DDPM.Easy.Common;
using DDPM.SA.Common.Display;
using DDPM.SA.Common.Settings;
using Dell.Client.Framework.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
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
        readonly double _cxIcon = 120;
        readonly double _cyIcon = 129;
        private Rect _rcIcon0 = new Rect();
        private Rect _rcIcon1 = new Rect();
        private Rect _rcIcon2 = new Rect();
        private Rect _rcIcon3 = new Rect();
        private Rect _rcIcon4 = new Rect();
        private Rect _rcHoveringIcon = new Rect();
        private Rect _rcHoveringCell = new Rect();

        //Flags to prevent Dispatcher.InvokeAsync() twice
        private DispatcherOperation? _pendingOp_RefreshCellRects = null;
        private DispatcherOperation? _pendingOp_AwsWinVisibleChanged = null;
        private DispatcherOperation? _pendingOp_WorkScreenChanged = null;
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

            ISplitCtrl? isp0B = ISplitCtrl.Create(0, 'B');
            isp0B.SplitMode = eSplitModes.AWS;
            _vm.AwsIcon0 = isp0B;

            //ISplitCtrl? isp2B = ISplitCtrl.Create(2, 'B');
            //isp2B.SplitMode = eSplitModes.AWS;
            //_vm.AwsIcon0 = isp2B;


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
            Trace.WriteLine($"scr.Bounds.Left={scr.Bounds.Left}");
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

            //Get the MonitorInfo
            List<MonitorInfo>? attachedMonitors = _vm.GetMonitorsFromDeviceName(screenDeviceName);
            bool isSupportedMonitor = false;
            if (attachedMonitors != null)
            {
                isSupportedMonitor = attachedMonitors.Count > 0;
                _vm.WriteLog($"  * GetAttachedMonitors from screen of cursor, attachedMonitor count={attachedMonitors.Count}");
            }

            //Load RecentList to IconList
            bool isRecentListLoaded = false;
            if (isSupportedMonitor)
            {
                EAMonitorSettings eaSettings = _vm.ReadEAMonitorSettings(attachedMonitors[0]);
                if (eaSettings != null)
                {
                    if (eaSettings.RecentList != null)
                    {
                        isRecentListLoaded = _vm.RefreshAwsIconsFromRecentList(eaSettings.RecentList);
                        UpdateLayout();

                    }
                    else
                    {
                        _vm.WriteLog($"  * ReadSettings of Monitor(Model:{attachedMonitors[0].modelName}, ServiceTag:{attachedMonitors[0].edid.ServiceTag}) RecentList is null");
                    }
                }
                else
                {
                    //_vm.WriteLog($"  * ReadSettings of Monitor(Model:{attachedMonitors[0].modelName}, ServiceTag:{attachedMonitors[0].edid.ServiceTag}) return null");
                    _vm.WriteLog("  * ReadSettings of Monitor return null");
                }
            }
            if (!isRecentListLoaded)
            {
                _vm.WriteLog("  * Cannot load RecentList from monitor, assume it\'s Non-Dell monitor, will apply defaul RecentList.");
                //Load Win11 default Snap layout
                isRecentListLoaded = _vm.RefreshAwsIconsFromRecentList(SplitJson.DefaultRecentList.ToArray());
            }

        //    Dispatcher_RefreshCellRects();
        }

        //Robert_Lin 2025-4-25, unused, use RefreshAwsIconsRectFromUI() instead
        /// <summary>
        /// Calculate the Rect of Icons (AwsIcon0~AwsIcon4) and then store to ArrangeVM.rcIcon0~rcIcon4
        /// The coordinates are based on VirtualScreen
        /// </summary>
        private void RefreshAwsIconRects_Unused()
        {
            double dx = (_cxAwsWindow - _cxIcon * 5) / 10;
            double dy = (_cyAwsWindow - _cyIcon) / 2;

            double xWindow = _vm.xAwsWindow;
            double yWindow = _vm.yAwsWindow;

            _rcIcon0 = new Rect(_vm.ScreenScale * (xWindow + dx), _vm.ScreenScale * (yWindow + dy), _vm.ScreenScale * _cxIcon, _vm.ScreenScale *_cyIcon);
            _rcIcon1 = new Rect(_vm.ScreenScale * (xWindow + 3 * dx + _cxIcon), _vm.ScreenScale * (yWindow + dy), _vm.ScreenScale * _cxIcon, _vm.ScreenScale * _cyIcon);
            _rcIcon2 = new Rect(_vm.ScreenScale * (xWindow + 5 * dx + 2 * _cxIcon), _vm.ScreenScale * (yWindow + dy), _vm.ScreenScale * _cxIcon, _vm.ScreenScale * _cyIcon);
            _rcIcon3 = new Rect(_vm.ScreenScale * (xWindow + 7 * dx + 3 * _cxIcon), _vm.ScreenScale * (yWindow + dy), _vm.ScreenScale * _cxIcon, _vm.ScreenScale * _cyIcon);
            _rcIcon4 = new Rect(_vm.ScreenScale * (xWindow + 9 * dx + 4 * _cxIcon), _vm.ScreenScale * (yWindow + dy), _vm.ScreenScale * _cxIcon, _vm.ScreenScale * _cyIcon);

            //_rcIcon0 = _vm.GetFrameworkElementRect(_vm.AwsIcon0.UC);
            //_rcIcon1 = _vm.GetFrameworkElementRect(_vm.AwsIcon1.UC);
            //_rcIcon2 = _vm.GetFrameworkElementRect(_vm.AwsIcon2.UC);
            //_rcIcon3 = _vm.GetFrameworkElementRect(_vm.AwsIcon3.UC);
            //_rcIcon4 = _vm.GetFrameworkElementRect(_vm.AwsIcon4.UC);
            _vm.rcIcont0 = _rcIcon0;
            _vm.rcIcont1 = _rcIcon1;
            _vm.rcIcont2 = _rcIcon2;
            _vm.rcIcont3 = _rcIcon3;
            _vm.rcIcont4 = _rcIcon4;
        }

        /// <summary>
        /// Get the Rects of AwsIcons (to _vm_rcIcon0~rcIcon4) from UI element names (awsIcon0~awsIcon4)
        /// by calling to GetFrameworkElementRect()
        /// This method can be called from UI (Dispatcher) thread only.
        /// Specify callerName to record in log file when error.
        /// </summary>
        /// <returns>return true when no any error.</returns>
        private bool RefreshAwsIconsRectFromUI([CallerMemberName] string callerName="")
        {
            const string methodName = "RefreshAwsIconsRectFromUI";
            bool ret = true;
            try
            {
                //Refresh the Rects of AwsIcons
                //OUTPUT: _vm.rcIcon0, _vm.rcIcon1, _vm.rcIcon2, _vm.rcIcon3, _vm.rcIcon4
                _vm.rcIcont0 = _vm.GetFrameworkElementRect(awsIcon0);
                if (_vm.rcIcont0.IsEmpty)
                {
                    ret = false;
                    _vm.WriteLog($"{methodName}({callerName}), GetFrameworkElementRect(awsIcon0) return EMPTY.");
                }

                _vm.rcIcont1 = _vm.GetFrameworkElementRect(awsIcon1);
                if (_vm.rcIcont1.IsEmpty)
                {
                    ret = false;
                    _vm.WriteLog($"{methodName}({callerName}), GetFrameworkElementRect(awsIcon1) return EMPTY.");
                }

                _vm.rcIcont2 = _vm.GetFrameworkElementRect(awsIcon2);
                if (_vm.rcIcont2.IsEmpty)
                {
                    ret = false;
                    _vm.WriteLog($"{methodName}({callerName}), GetFrameworkElementRect(awsIcon2) return EMPTY.");
                }

                _vm.rcIcont3 = _vm.GetFrameworkElementRect(awsIcon3);
                if (_vm.rcIcont3.IsEmpty)
                {
                    ret = false;
                    _vm.WriteLog($"{methodName}({callerName}), GetFrameworkElementRect(awsIcon3) return EMPTY.");
                }

                _vm.rcIcont4 = _vm.GetFrameworkElementRect(awsIcon4);
                if (_vm.rcIcont4.IsEmpty)
                {
                    ret = false;
                    _vm.WriteLog($"{methodName}({callerName}), GetFrameworkElementRect(awsIcon4) return EMPTY.");
                }
            }
            catch (Exception ex)
            {
                ret = false;
                _vm.WriteLog($"{methodName}({callerName}), Exception", ex);
            }
            return ret;
        }

        private void RefreshCellBordersInAwsIcons_Unused()
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
                    //Robert_Lin 2025-4-1 trial workaround. It seems not solve the problem.
                    splitCtrl0B.ApplySettingsToCellList(new Rect(rcIcon.Left, rcIcon.Top, rcIcon.Width*_vm.ScreenScale, rcIcon.Height*_vm.ScreenScale));
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
        public CellObj? DetermineHoverigCellObj_Icon0(int x, int y)
        {
            //if (!IsCursorInsideAwsWindow(x, y))
            //{
            //    _vm.AwsWindowHoverMsg = $"Cursor({x},{y}) not inside AwsWindow";
            //    return null;
            //}
            if (_vm.AwsIcon0 == null)
                return null;

            CellObj? hoverCell = null;
            hoverCell = DeterminAwsIconHoveringCellObj(_vm.AwsIcon0, x, y);
            if (hoverCell != null)
            {
                _vm.HoveringAwsIcon = _vm.AwsIcon0;
                _vm.AwsIcon0.HoveringCell = hoverCell.Name;
                _rcHoveringIcon = _rcIcon0;
                _rcHoveringCell = hoverCell.rc;
                _vm.AwsWindowHoverMsg = $"Hovering Icon0.{hoverCell.Name}";
                HoverCellInAwsIcon0(_vm.AwsIcon0.HoveringCell);

                //_vm.AwsIcon0.HoveringCell = "";
       //         _vm.AwsIcon1.HoveringCell = "";
       //         _vm.AwsIcon2.HoveringCell = "";
       //         _vm.AwsIcon3.HoveringCell = "";
        //        _vm.AwsIcon4.HoveringCell = "";
                return hoverCell;
            }
            else
            {
                HoverCellInAwsIcon0("");
            }
            return null;
        }

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

            //if (!IsCursorInsideAwsWindow(x, y))
            //{
            //    _vm.AwsWindowHoverMsg = $"Cursor({x},{y}) not inside AwsWindow";
            //    return null;
            //}


            //DpiScale dpiScale = VisualTreeHelper.GetDpi(this);
            //double scale = dpiScale.PixelsPerDip;
            CellObj? hoverCell = null;

            //_vm.AwsIcon0.HoveringCell = "";
            //_vm.AwsIcon1.HoveringCell = "";
            //_vm.AwsIcon2.HoveringCell = "";
            //_vm.AwsIcon3.HoveringCell = "";
            //_vm.AwsIcon4.HoveringCell = "";

            //
            //  AWSIcon0
            //
            //if (!_rcIcon0.IsEmpty)
            //{
            //    if (_rcIcon0.Contains(x, y))
            //    {
            //        hoverCell = DeterminAwsIconHoveringCellObj(_vm.AwsIcon0, x, y);
            //        //        foreach(CellObj objCell in _vm.AwsIcon0.CellList)
            //        //        {
            //        //            if (objCell.rc.Contains(x, y))
            //        //            {
            //        //                this.Dispatcher.Invoke(() =>
            //        //                {
            //        //                });
            //        //            }
            //        //        }
            //    }
            //}
            //hoverCell = DeterminAwsIconHoveringCellObj(_vm.AwsIcon0, x, y);
            //if (hoverCell != null)
            //{
            //    _vm.HoveringAwsIcon = _vm.AwsIcon0;
            //    _vm.AwsIcon0.HoveringCell = hoverCell.Name;
            //    _rcHoveringIcon = _rcIcon0;
            //    _rcHoveringCell = hoverCell.rc;
            //    _vm.AwsWindowHoverMsg = $"Hovering Icon0.{hoverCell.Name}";
            //    HoverCellInAwsIcon0(_vm.AwsIcon0.HoveringCell);

            //    //_vm.AwsIcon0.HoveringCell = "";
            //    _vm.AwsIcon1.HoveringCell = "";
            //    _vm.AwsIcon2.HoveringCell = "";
            //    _vm.AwsIcon3.HoveringCell = "";
            //    _vm.AwsIcon4.HoveringCell = "";
            //    return hoverCell;
            //}
            //else
            //{
            //    HoverCellInAwsIcon0("");
            //}

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
            if (awsIcon == null)
                return null;

            if (awsIcon.IsOverlapCustomLayout)
            {
                SplitCtrl0B sp0B = (SplitCtrl0B)awsIcon;
                foreach(CellObj objCell in awsIcon.CellList)
                {
                    if (hoverCell == null)
                    {
                        if (objCell.rc.Contains(x, y))
                        {
                         //   objCell.CellBd.Dispatcher_SetIsHover(true);
                         //   objCell.CellBd.IsHover = true;

                            hoverCell = objCell;
                            hoverCell.rc = objCell.rc;

                            //_vm.AwsIcon1.HoveringCell = objCell.Name;
                            //_vm.HoveringAwsIcon = awsIcon;
                            ////_rcHoveringIcon = _rcIcon1;
                            //_rcHoveringCell = objCell.rc;
                        }
                        else
                        {
                        //    objCell.CellBd.Dispatcher_SetIsHover(false);
                        }
                    }
                    else
                    {
                    //    objCell.CellBd.Dispatcher_SetIsHover(false);
                    }
                }
                if (hoverCell != null) 
                    return hoverCell;

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

        public Screen? HoveringScreen { get; set; } = null;
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
            _ = Dispatcher.BeginInvoke(new Action(() =>
            {
                Dispatcher_RefreshCellRects(flag);

                if (_vm.rcAwsWindow.IsEmpty)
                {
                    _vm.rcAwsWindow = _vm.GetFrameworkElementRect(this);
                }
            }));
        }

        //Unused, Robert_Lin 2025-4-25
        private void Dispatcher_RefreshCellRects(int flag = 0)
        {
            _areCellRectsRefreshed = true;
            if (!_vm.AreAwsIconsLoaded)
            {
                Trace.WriteLine("@ Dispatcher_RefreshCellRects(), AwsIcons are not loaded");
                return;
            }

            Trace.WriteLine("@ Dispatcher_RefreshCellRects()");

            //Robert_Lin 2025-4-17 comment-out
            //The Cell rects has been created when the CellList is creating in Dispatcher_RefreshIcon0()
            //But UI_RefreshAwsIconCellRects() is called when LocationChanged, and when it's running, the AwsIcon may
            //not been displayed, so it will get an empty rect to CellList.CellObj.rc
            //if (!UI_RefreshAwsIconCellRects(_vm.AwsIcon0, _vm.rcIcont0))
            //{
            //    _areCellRectsRefreshed = false;
            //}

     //       _rcIcon1 = _vm.GetFrameworkElementRect(_vm.AwsIcon1.UC);
            //_vm.WriteLog($"@ RefreshCellRects() - Icon1: {ArrangeVM.FormatRect(_rcIcon1)}");

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
    //        if (!UI_RefreshAwsIconCellRects(_vm.AwsIcon1, _vm.rcIcont1))
    //        {
    //            _areCellRectsRefreshed = false;
    //        }

    //        _rcIcon2 = _vm.GetFrameworkElementRect(_vm.AwsIcon2.UC);
            //_vm.WriteLog($"@ RefreshCellRects() - Icon2: {ArrangeVM.FormatRect(_rcIcon2)}");

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
     //       if (!UI_RefreshAwsIconCellRects(_vm.AwsIcon2, _vm.rcIcont2))
     //       {
     //           _areCellRectsRefreshed = false;
     //       }


     //       _rcIcon3 = _vm.GetFrameworkElementRect(_vm.AwsIcon3.UC);
            //_vm.WriteLog($"@ RefreshCellRects() - Icon3: {ArrangeVM.FormatRect(_rcIcon3)}");

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
     //       if (!UI_RefreshAwsIconCellRects(_vm.AwsIcon3, _vm.rcIcont3))
     //       {
     //           _areCellRectsRefreshed = false;
     //       }

     //       _rcIcon4 = _vm.GetFrameworkElementRect(_vm.AwsIcon4.UC);
            //_vm.WriteLog($"@ RefreshCellRects() - Icon4: {ArrangeVM.FormatRect(_rcIcon4)}");

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
    //        if (!UI_RefreshAwsIconCellRects(_vm.AwsIcon4, _vm.rcIcont4))
    //        {
    //            _areCellRectsRefreshed = false;
    //        }

            if (!_areCellRectsRefreshed && flag == 0)
            {
                _vm.WriteLog($"Create timer for RefreshCellRects(0)");

                //Derek 2025/03/29
                System.Threading.Timer? timer1 = null;
                try
                {
                    timer1 = new System.Threading.Timer((obj) => { RefreshCellRects(0); }, null, 100, Timeout.Infinite);
                }
                catch (Exception e)
                {
                    _vm.WriteLog($"[AwsWindow] Create timer for RefreshCellRects(0) exception {e.Message}");
                }
                finally
                {
                    timer1?.Dispose();
                    timer1 = null;
                }
                
            }
            _vm.OnPropertyChanged_AwsIconInfos();
        }        

        private bool UI_RefreshAwsIconCellRects(ISplitCtrl awsIcon, Rect rcIcon)
        {
            bool isCellRectsRefreshed = true;

            //Robert_Lin 2025-4-24 Overlap layouts has added code the RefreshCells by themself.
            //It's no need to get here.
            /*
            if (awsIcon.IsOverlapCustomLayout)
            {
                SplitCtrl0B splitCtrl0B = (SplitCtrl0B)awsIcon;
                foreach(CellObj objCell in splitCtrl0B.CellList)
                {
                    objCell.rc = _vm.GetFrameworkElementRect(objCell.CellBd);
                    if (objCell.rc.IsEmpty)
                    {
                        isCellRectsRefreshed = false;
                    }
                }
                //foreach (CellBorder cb in splitCtrl0B.CellBorders)
                //{
                //    cb.rect = _vm.GetFrameworkElementRect(cb);
                //    if (cb.rect.IsEmpty)
                //    {
                //        isCellRectsRefreshed = false;
                //    }
                //}
            }
            */
            //Robert_Lin 2025-4-24, for Preset layouts, thay can RefreshCells by themselft,
            //It's no need to get Rects here
            /*
            else
            {
                if ((awsIcon.CellCount == 2) || (awsIcon.CellCount == 3))
                    return true;
                if (awsIcon.CtrlClass.Equals("SplitCtrl4A"))
                    return true;

                foreach (CellObj objCell in awsIcon.CellList)
                {
                    if (objCell.CellBd == null)
                        continue;

                    objCell.rc = _vm.GetFrameworkElementRect(objCell.CellBd);

                    //var transformToWnd = awsIcon.UC.TransformToVisual(this);
                    //var posIconToWnd = transformToWnd.Transform(new System.Windows.Point(0, 0));

                    ////Transform CellBorder position to related to AwsIcon
                    //var transformToIcon = objCell.CellBd.TransformToVisual(awsIcon.UC);
                    //var posCellToSplit = transformToIcon.Transform(new System.Windows.Point(0, 0));

                    ////Get the position of Virtual Screen
                    //var posVscr = new System.Windows.Point(SystemParameters.VirtualScreenLeft, SystemParameters.VirtualScreenTop);
                    ////Calculate the relate pos of Cell to Virtual Screen
                    //var posCellToVscr = new System.Windows.Point(posCellToSplit.X + rcIcon.Left, posCellToSplit.Y + rcIcon.Top);

                    if (objCell.rc.IsEmpty)
                    {
                        isCellRectsRefreshed = false;
                    }
                }
            } */
            //else
            //{
            //    foreach (CellObj objCell in awsIcon.CellList)
            //    {
            //        if (objCell.bd == null)
            //            continue;

            //        objCell.rc = _vm.GetFrameworkElementRect(objCell.bd);
            //        if (objCell.rc.IsEmpty)
            //        {
            //            isCellRectsRefreshed = false;
            //        }
            //    }
            //}
            return isCellRectsRefreshed;
        }


        #endregion

        #region ViewModel Event Handlers
        private void HandleAwsWindowVisibilityChanged(object? sender, bool isVisible)
        {
            const string methodName = "AwsWidow.HandleAwsWindowVisibilityChanged";

            _vm.WriteLog($"@{methodName}(isVisible={isVisible})");
            if (!isVisible)
                return;

            _pendingOp_AwsWinVisibleChanged?.Abort();
            _pendingOp_AwsWinVisibleChanged = Dispatcher.InvokeAsync(() =>
            {
                //Step_1, determine the new position of AwsWindow
                //OUTPUT: (_vm.xAwsWindow, _vm.yAwsWindow)
                //And move AwsWindow to the new position

                //Get the Screen of the cursor
                Screen showScreen = _vm.GetScreenFromCursor();
                _vm.WorkScreen = showScreen;

                //Check if the showScreen is WorkScreen of AwsWindow
                _vm.WriteLog($"@ {methodName}(), Cursor=({_vm.xCursor},{_vm.yCursor})");
                System.Windows.Point ptAws = CalculateAwsPosition();
                _vm.xAwsWindow = ptAws.X;
                _vm.yAwsWindow = ptAws.Y;

                //Move window to the new WorkScreen
                Left = ptAws.X;
                Top = ptAws.Y;
                Topmost = true;

                //Refresh the Rects of AwsIcons
                bool ret = RefreshAwsIconsRectFromUI(methodName);

                //OUTPUT: _vm.rcIcon0, _vm.rcIcon1, _vm.rcIcon2, _vm.rcIcon3, _vm.rcIcon4
                // RefreshAwsIconRects();
                //if (_vm.AwsIcon0 != null)
                //    _vm.rcIcont0 = _vm.GetFrameworkElementRect(_vm.AwsIcon0.UC);
                //if (_vm.AwsIcon1 != null)
                //    _vm.rcIcont1 = _vm.GetFrameworkElementRect(_vm.AwsIcon1.UC);
                //if (_vm.AwsIcon2 != null)
                //    _vm.rcIcont2 = _vm.GetFrameworkElementRect(_vm.AwsIcon2.UC);
                //if (_vm.AwsIcon3 != null)
                //    _vm.rcIcont3 = _vm.GetFrameworkElementRect(_vm.AwsIcon3.UC);
                //if(_vm.AwsIcon4 != null)
                //    _vm.rcIcont4 = _vm.GetFrameworkElementRect(_vm.AwsIcon4.UC);

                //Get current working screen
                //Screen? scr = _vm.GetScreenFromCursor();
                if (showScreen != null)
                {
                    ReloadRecentList(showScreen.DeviceName);
                }

        //        RefreshAwsIconRects();
        //        RefreshCellBordersInAwsIcons();
                //RefreshCellRects();
        //        Dispatcher_RefreshCellRects();
                Dispatcher_RefreshIcon0();

            }, DispatcherPriority.Loaded);


        }

        /// <summary>
        /// It's called when ArrangeVM.WorkScreen value is changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="newScreen"></param>
        private void HandleWorkScreenChanged(object? sender, Screen newScreen)
        {
            const string methodName = "AwsWindow.HandleWorkScreenChanged";

            _vm.WriteLog($"{methodName}(newScreen={newScreen.DeviceName})");

            if (!_vm.IsAwsWindowVisible)
                return;

            _pendingOp_WorkScreenChanged?.Abort();
            _pendingOp_WorkScreenChanged = Dispatcher.InvokeAsync(() =>
            {
                _pendingOp_WorkScreenChanged = null;
                _vm.WriteLog($"@{methodName}(), Cursor=({_vm.xCursor},{_vm.yCursor})");
                System.Windows.Point ptAws = CalculateAwsPosition();
                _vm.xAwsWindow = ptAws.X;
                _vm.yAwsWindow = ptAws.Y;

                //Move window to the new WorkScreen
                Left = ptAws.X;
                Top = ptAws.Y;
                Topmost = true;

                //Refresh the Rects of AwsIcons
                //OUTPUT: _vm.rcIcon0, _vm.rcIcon1, _vm.rcIcon2, _vm.rcIcon3, _vm.rcIcon4
                // RefreshAwsIconRects();
                bool ret = RefreshAwsIconsRectFromUI(methodName);
                //if (_vm.AwsIcon0 != null)
                //    _vm.rcIcont0 = _vm.GetFrameworkElementRect(_vm.AwsIcon0.UC);
                //else
                //{
                //    _vm.WriteLog($"{methodName}(), GetFrameworkElementRect(AwsIcon0) return null.");
                //}
                //if (_vm.AwsIcon1 != null)
                //    _vm.rcIcont1 = _vm.GetFrameworkElementRect(_vm.AwsIcon1.UC);
                //else
                //{
                //    _vm.WriteLog($"{methodName}(), GetFrameworkElementRect(AwsIcon1) return null.");
                //}
                //if (_vm.AwsIcon2 != null)
                //    _vm.rcIcont2 = _vm.GetFrameworkElementRect(_vm.AwsIcon2.UC);
                //else
                //{
                //    _vm.WriteLog($"{methodName}(), GetFrameworkElementRect(AwsIcon2) return null.");
                //}
                //if (_vm.AwsIcon3 != null)
                //    _vm.rcIcont3 = _vm.GetFrameworkElementRect(_vm.AwsIcon3.UC);
                //else
                //{
                //    _vm.WriteLog($"{methodName}(), GetFrameworkElementRect(AwsIcon3) return null.");
                //}
                //if (_vm.AwsIcon4 != null)
                //    _vm.rcIcont4 = _vm.GetFrameworkElementRect(_vm.AwsIcon4.UC);
                //else
                //{
                //    _vm.WriteLog($"{methodName}(), GetFrameworkElementRect(AwsIcon4) return null.");
                //}



                //Get current working screen
                Screen? scr = _vm.GetScreenFromCursor();
                if (scr != null)
                {
                    ReloadRecentList(scr.DeviceName);
                }

        //        RefreshAwsIconRects();
        //        RefreshCellBordersInAwsIcons();
        //        Dispatcher_RefreshCellRects();
                Dispatcher_RefreshIcon0();

            }, DispatcherPriority.Loaded);
        }

        #endregion ViewModel Event Handlers

        #region Icon0 - Monitors
        //Robert_Lin, this method is workable, it will be removed due to
        //1 The purpose of this method is create the CellBorders for AwsIcon0, it's OK
        //2 But it also calculate
        private void Dispatcher_RefreshIcon0()
        {
            try
            {
                if (icon0Canvas.ActualWidth == 0)
                    return;

                System.Drawing.Rectangle rcVirtualScreen = SystemInformation.VirtualScreen;
                if ((rcVirtualScreen.Width <= 0) || (rcVirtualScreen.Height <= 0))
                    return;

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
                    CellBorder cellBd = new CellBorder();
                    cellBd.Width = scr.Bounds.Width * ratioBorder;
                    cellBd.Height = scr.Bounds.Height * ratioBorder;
                    cellBd.CellName = $"{idxScr + 1}";

                    TextBlock text = new TextBlock();
                    text.Text = $"{idxScr + 1}";
                    text.Style = FindResource("MonitorIdTextStyle") as Style;
                    cellBd.AddChild(text);

                    double left = (scr.Bounds.Left - rcVirtualScreen.Left) * ratioBorder;
                    double top = (scr.Bounds.Top - rcVirtualScreen.Top) * ratioBorder;

                    System.Windows.Point topLeft = new System.Windows.Point(left, top);
                    topLeft = icon0Canvas.PointToScreen(topLeft);
                    //cellBd.rect = new Rect(topLeft.X,
                    //    topLeft.Y, cellBd.Width, cellBd.Height);

                    icon0Canvas.Children.Add(cellBd);
                    Canvas.SetLeft(cellBd, left);
                    Canvas.SetTop(cellBd, top);

                    CellObj cellObj = new CellObj(text.Text, cellBd);
                    //cellObj.rc = new Rect(left, top, cellBd.Width, cellBd.Height);
                    //double scalerc = 1.00;///_vm.ScreenScale;
                    double xrc = topLeft.X;// (topLeft.X  - left * scalerc);
                    double yrc = topLeft.Y; // (topLeft.Y  + top * scalerc);
                    double wrc = cellBd.Width;
                    double hrc = cellBd.Height;
                    cellObj.rc = new Rect(xrc, yrc, wrc, hrc);

                    _vm.AwsIcon0.CellList.Add(cellObj);

                    idxScr++;
                }

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



                _vm.OnPropertyChanged_AwsIconInfos();
            }
            catch (Exception)
            {
            }
        }

        private void HoverCellInAwsIcon0(string hoverName)
        {
            _ = Dispatcher.BeginInvoke(new Action(() =>
            {
                foreach (var item in icon0Canvas.Children)
                {
                    CellBorder cellBorder = item as CellBorder;
                    if (cellBorder != null)
                    {
                        if (cellBorder.CellName.Equals(hoverName))
                            cellBorder.Dispatcher_SetIsHover(true);
                        else
                            cellBorder.Dispatcher_SetIsHover(false);

                    }
                }
            }));
        }

        private void UI_RefreshIcon0_SplitCtrl0B()
        {
            System.Drawing.Rectangle rcVirtualScreen = SystemInformation.VirtualScreen;
            Trace.WriteLine($"VirtualScreen: {rcVirtualScreen.Width}x{rcVirtualScreen.Height}");
            Trace.WriteLine($"AwsIcon0: {_vm.AwsIcon0.UC.ActualWidth}x{_vm.AwsIcon0.UC.ActualHeight}; {_vm.AwsIcon0.UC.Width}");
            Trace.WriteLine($"AwsIcon1: {_vm.AwsIcon1.UC.ActualWidth}x{_vm.AwsIcon1.UC.ActualHeight}; {_vm.AwsIcon1.UC.Width}");

            double cxView = 120;// _vm.AwsIcon1.UC.ActualWidth;
            double cyView = 90; // _vm.AwsIcon1.UC.ActualHeight;

            double ratioX = cxView / (double)rcVirtualScreen.Width;
            double ratioY = cyView / (double)rcVirtualScreen.Height;
            bool isHorzFit = (ratioX < ratioY);
            Trace.WriteLine($"ratioX={ratioX}, ratioY={ratioY}, IsHorzFit={isHorzFit}");

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

            //icon0Canvas.Children.Clear();
            _vm.AwsIcon0.CellList.Clear();
            int idxScr = 0;

            foreach (Screen scr in Screen.AllScreens)
            {
                Trace.WriteLine($"Screen[{idxScr}] ({scr.Bounds.X},{scr.Bounds.Y}) {scr.Bounds.Width}x{scr.Bounds.Height}");
                string cellName = $"{idxScr + 1}";

                //AddMsg($"[{idxScr}] {scr.DeviceName} {(scr.Primary ? "Primary" : "")}");
                //AddMsg($"   {FormatRecttangle(scr.Bounds)}");

                //Border bd = new Border();
                //bd.Width = scr.Bounds.Width * ratioBorder;
                //bd.Height = scr.Bounds.Height * ratioBorder;
                //bd.Style = FindResource("CellBorderStyle") as Style;

                CellBorder cellBd = new CellBorder();
                cellBd.Width = scr.Bounds.Width * ratio;
                cellBd.Height = scr.Bounds.Height * ratio;
                cellBd.CellName = cellName;

                TextBlock text = new TextBlock();
                text.Text = cellName;
                text.Style = FindResource("MonitorIdTextStyle") as Style;
                cellBd.AddChild(text);

                double left = (scr.Bounds.Left - rcVirtualScreen.Left) * ratio;
                double top = (scr.Bounds.Top - rcVirtualScreen.Top) * ratio;

                //AddMsg($"    Border at ({left},{top}) {bd.Width}x{bd.Height}");

                System.Windows.Point topLeft = new System.Windows.Point(left, top);
                topLeft = icon0Canvas.PointToScreen(topLeft);
                cellBd.rect = new Rect(topLeft.X, topLeft.Y, cellBd.Width, cellBd.Height);

                //icon0Canvas.Children.Add(cellBd);
                //Canvas.SetLeft(cellBd, left);
                //Canvas.SetTop(cellBd, top);

                CellObj cellObj = new CellObj(text.Text, cellBd);
                cellObj.rc = new Rect(left, top, cellBd.Width, cellBd.Height);
                Trace.WriteLine($"Cell[{cellName}] {ArrangeVM.FormatRect(cellObj.rc)}");
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
        }
        #endregion

        private void Window_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.IsVisible)
            {
                _vm.rcAwsWindow = _vm.GetFrameworkElementRect(this);

                _pendingOp_RefreshCellRects?.Abort();
                _pendingOp_RefreshCellRects = Dispatcher.InvokeAsync(() =>
                {
                    _pendingOp_RefreshCellRects = null;
                    Dispatcher_RefreshCellRects();
                }, DispatcherPriority.Loaded);
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

        //Derek 2025/03/29 remove it due to no one use it
        //public void OnWindowStartMoving()
        //{
        //    //RefreshIcon0();
        //}

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _vm.WriteLog($"Awswindows Window_Closing start, sender = {sender.ToString()}");

            try
            {
                HoveringScreen = null;

                if (_vm != null)
                {
                    _vm.AwsWindowVisibilityChanged -= HandleAwsWindowVisibilityChanged;
                    _vm.WorkScreenChanged -= HandleWorkScreenChanged;
                }


                if (System.Windows.Threading.Dispatcher.CurrentDispatcher != null)
                {
                    System.Windows.Threading.Dispatcher.CurrentDispatcher.InvokeShutdown();
                }
            }
            catch (Exception ex)
            {
                _vm.WriteLog($"Awswindows Window_Closing catch excepton {ex.Message}");
            }
            
        }
    }
}
