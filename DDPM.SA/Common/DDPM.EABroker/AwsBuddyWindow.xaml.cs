using DDPM.Easy.Common;
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
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace DDPM.EABroker
{
    /// <summary>
    /// Interaction logic for AwsBuddyWindow.xaml
    /// </summary>
    public partial class AwsBuddyWindow : Window
    {
        #region Private members
        private readonly ArrangeVM _vm;
        private Screen? _workScreen = null;
        private ISplitCtrl? _workSplit = null;
        private CellObj? _hoveringCell = null;
        #endregion

        #region ctor / Init
        public AwsBuddyWindow(ArrangeVM vm)
        {
            InitializeComponent();
            _vm = vm;
            DataContext = _vm;

            _vm.AwsBuddyWindowVisibilityChanged += HandleAwsBuddyWindowVisibilityChanged;
            _vm.HoveringAwsIconChanged += HandleHoveringAwsIconChanged;
            _vm.HoveringAwsCellObjChanged += HandleHoveringAwsCellObjChanged;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //Hide window from Alt+tab
            System.Windows.Interop.WindowInteropHelper wndHelper = new System.Windows.Interop.WindowInteropHelper(this);
            Win32Lib.Win32.HideWinFromAltTab(wndHelper.Handle);
        }
        #endregion ctor / Init

        private bool IsVertical
        {
            get
            {
                if (_workScreen == null)
                    return false;

                return (_workScreen.Bounds.Width < _workScreen.Bounds.Height);
            }
        }

        public void MoveToScreen(Screen screen)
        {
            if (_workScreen != null && _workScreen.Equals(screen))
            {
                return;
            }

            _workScreen = screen;
            //WorkScreen is changed

            this.Dispatcher.Invoke(() =>
            {
                double screenScale = _vm.ScreenScale;
                Left = _workScreen.WorkingArea.Left / screenScale;
                Top = _workScreen.WorkingArea.Top / screenScale;
                Width = _workScreen.WorkingArea.Width / screenScale;
                Height = _workScreen.WorkingArea.Height / screenScale;

                if (_workSplit != null && _workSplit.IsVertical != IsVertical)
                {
                    _workSplit.IsVertical = IsVertical;
                }
            });
        }

        /// <summary>
        /// Assign a SplitCtrl to show on AwsBuddyWindow.
        /// This method must be called from UI/Dispatcher thread.
        /// </summary>
        /// <param name="splitCtrl"></param>
        /// <param name="hoverCellName"></param>
        public void SetWorkSplit(ISplitCtrl splitCtrl, string hoverCellName="")
        {
            if (splitCtrl == null)
                return;


            Dispatcher.InvokeAsync(() => 
          //  this.Dispatcher.BeginInvoke(() =>
            {
                //If workSplit not been assigned, or changed
                bool needToRefreshWorkSplit = (_workSplit == null) || (splitCtrl.EAID != _workSplit.EAID);
                ISplitCtrl? localSplit = _workSplit;

                if (needToRefreshWorkSplit)
                {
                    //Create a new ISplitCtrl to the AwsBuddyWindow
                    localSplit = splitCtrl.Clone();
                    // localSplit.IsVertical = splitCtrl.IsVertical;
                    localSplit.IsVertical = IsVertical;

                    if (localSplit.IsOverlapCustomLayout) //SplitCtrl0B
                    {
                        SplitCtrl0B sp0B = (SplitCtrl0B)localSplit;

                        if (_workScreen == null)
                            _workScreen = _vm.WorkScreen;

                        double awsBuddyOverlapScale = 1.000; /// _vm.ScreenScale;
                        //Fix the rcScreen from _workScreen
                        Rect rcScreen = new Rect();
                        rcScreen.X = _workScreen.WorkingArea.Left * awsBuddyOverlapScale;
                        rcScreen.Y = _workScreen.WorkingArea.Top * awsBuddyOverlapScale;
                        rcScreen.Width = _workScreen.WorkingArea.Width * awsBuddyOverlapScale;
                        rcScreen.Height = _workScreen.WorkingArea.Height * awsBuddyOverlapScale;

                        //Check if it's migrate from DDM
                        if ((localSplit.Settings != null) && (localSplit.Settings.Count >= 8))
                        {
                            if ((localSplit.Settings[2] == 1.000) && (localSplit.Settings[3] == 1.0000))
                            {
                                rcScreen.Width /= _vm.ScreenScale;
                                rcScreen.Height /= _vm.ScreenScale;
                            }
                        }


                        //Create Borders and CellBorders to canvas grid
                        sp0B.ApplySettingsToCellList(rcScreen);

                    }
                    localSplit.SplitMode = eSplitModes.Work;
                    localSplit.IsEditable = false;
                    localSplit.IsVertical = IsVertical;
                    localSplit.HoveringCell = hoverCellName;
                    splitContent.Content = localSplit;


                    _workSplit = localSplit;
                    _workSplit.IsVertical = IsVertical;
                }//Check if workSplit is changed
                else
                {
                    if (_workSplit != null)
                    {
                        _workSplit.HoveringCell = hoverCellName;
                    }
                }
                /*
                if (localSplit != null)
                {
                    //Set the hovering Cell to Hover state
                    if (localSplit.IsAddedCustomLayout)
                    {
                        localSplit.HoveringCell = hoverCellName;
                        foreach (CellObj objCell in localSplit.CellList)
                        {
                            if (objCell.Name.Equals(hoverCellName))
                            {

                                objCell.CellBd.Dispatcher_SetIsHover(true);
                            }
                            else
                                objCell.CellBd.Dispatcher_SetIsHover(false);
                        }
                    }
                    else
                    {
                        localSplit.HoveringCell = hoverCellName;
                    }
                }
                */

                //Robert_Lin, 2024-10-23 Temporary comment-out

                //if (localSplit.IsAddedCustomLayout) //SplitCtrl0B
                //{
                //    SplitCtrl0B sp0B = (SplitCtrl0B)localSplit;

                //    if (_workScreen == null)
                //        _workScreen = _vm.WorkScreen;

                //    //Fix the rcScreen from _workScreen
                //    Rect rcScreen = new Rect();
                //    rcScreen.X = _workScreen.Bounds.Left / _vm.ScreenScale;
                //    rcScreen.Y = _workScreen.Bounds.Top / _vm.ScreenScale;
                //    rcScreen.Width = _workScreen.Bounds.Width / _vm.ScreenScale;
                //    rcScreen.Height = _workScreen.Bounds.Height / _vm.ScreenScale;

                //    //Create Borders and CellBorders to canvas grid
                //    sp0B.ApplySettingsToCellList(rcScreen);

                //    foreach(CellBorder cb in sp0B.CellBorders)
                //    {
                //        if (cb.CellName.Equals(hoverCellName))
                //            cb.Dispatcher_SetIsHover(true);
                //        else
                //            cb.Dispatcher_SetIsHover(false);
                //    }
                //}

                //localSplit.SplitMode = eSplitModes.Work;
                //localSplit.IsEditable = false;
                //localSplit.IsVertical = IsVertical;
                //localSplit.HoveringCell = hoverCellName;
                //splitContent.Content = localSplit;

                //Delay to call RefreshCellRects
              //      RefreshCellRects();
            }, DispatcherPriority.Loaded);
        }

        public void RefreshCellRects(int flag)
        {
            if (_workSplit == null)
                return;

            this.Dispatcher.Invoke(() =>
            {
                bool _areCellRectsRefreshed = true;

                ISplitCtrl localSplit = _workSplit;
                if (localSplit == null)
                    return;
                if (localSplit.IsAddedCustomLayout)
                {
                    SplitCtrl0B sp0B = (SplitCtrl0B)localSplit;
                    foreach (CellBorder cellBd in localSplit.CellBorders)
                    {
                        cellBd.rect = _vm.GetFrameworkElementRect(cellBd);

                        CellObj? cellObj = localSplit.CellList.Find(x => x.Name.Equals(cellBd.CellName));
                        if (cellObj != null)
                        {
                            cellObj.rc = cellBd.rect;

                            if (cellObj.rc.IsEmpty)
                                _areCellRectsRefreshed = false;
                        }
                    }
                }
                else
                {
                    foreach (CellObj objCell in localSplit.CellList)
                    {
                        if (objCell.bd == null)
                            continue;

                        objCell.rc = _vm.GetFrameworkElementRect(objCell.bd);

                        Trace.WriteLine($"Cell({objCell.Name})={ArrangeVM.FormatRect(objCell.rc)}");

                        if (objCell.rc.IsEmpty)
                            _areCellRectsRefreshed = false;
                    }

                }
                if (!_areCellRectsRefreshed)
                {
                    if (flag > 0)
                    {
                        System.Threading.Timer? timer1 = null;
                        try
                        {
                            timer1 = new System.Threading.Timer((obj) => { RefreshCellRects(1); }, null, 100, Timeout.Infinite);
                        }
                        catch (Exception e)
                        {
                            _vm.WriteLog($"[AwsBuddyWindow] create RefreshCellRects(1) timer exception: {e.Message}");
                        }
                        finally
                        {
                            timer1?.Dispose();
                            timer1 = null;
                        }                        
                    }
                }
                else
                {
                    string d = "";
                }

            });
        }

        #region VisibilityChanged event handler
        private void HandleAwsBuddyWindowVisibilityChanged(object? sender, bool isVisible)
        {
            _vm.WriteLog($"@AwsWindow.HandleAwsWindowVisibilityChanged(isVisible={isVisible})");
            if (!isVisible)
                return;


            MoveToScreen(_vm.WorkScreen);
            //SetWorkSplit(_vm.HoveringSplit);
            RefreshCellRects();

        }
        public CellObj? HoveringCellObj
        {
            get { return _hoveringCell; }
        }
        #endregion
        private void HandleHoveringAwsIconChanged(object? sender, ISplitCtrl newSplit)
        {
    //        SetWorkSplit(newSplit);
        }
        private void HandleHoveringAwsCellObjChanged(object? sender, CellObj cellObj)
        {
            ISplitCtrl? localSplit = _workSplit;

            return;
            /*
            if (localSplit != null)
            {
                string hoveringCellName = "";
                if (cellObj != null) 
                    hoveringCellName = cellObj.Name;

                localSplit.HoveringCell = hoveringCellName;
                _hoveringCell = null;

                if (localSplit.IsAddedCustomLayout)
                {
                    SplitCtrl0B sp0B = (SplitCtrl0B)localSplit;
                    foreach (CellBorder cellBd in sp0B.CellBorders)
                    {
                        if (cellBd.rect.Contains(x, y))
                        {
                            cellBd.IsHover = true;
                            //Convert to CellObj
                            hoverCell = new CellObj(cellBd.Name, cellBd.Border);
                            hoverCell.rc = cellBd.rect;
                        }
                        else
                            cellBd.IsHover = false;
                        if (cellObj != null)
                        {
                            if (cellBd.CellName.Equals(hoveringCellName))
                            {
                                cellBd.Dispatcher_SetIsHover(true);
                                _hoveringCell = new CellObj(cellBd.CellName);
                                _hoveringCell.rc = cellBd.rect;
                                _vm.HoveringCellObj = _hoveringCell;
                            }
                            else
                            {
                                cellBd.Dispatcher_SetIsHover(false);
                            }
                        }
                        else
                        {
                            cellBd.Dispatcher_SetIsHover(false);
                        }
                    }
                }

            }*/
        }

        public Rect GetHoveringCellRect()
        {
            if (_workSplit == null)
            {
                return Rect.Empty;
            }

            if (String.IsNullOrEmpty(_workSplit.HoveringCell))
            {
                return Rect.Empty;
            }
            CellObj? hoveringCellObj = _workSplit.CellList.Find(x => x.Name.Equals(_workSplit.HoveringCell));
            if (hoveringCellObj == null)
            {
                return Rect.Empty;
            }
            return hoveringCellObj.rc;
        }


        public void RefreshCellRects()
        {
            if (_workSplit == null)
                return;

            this.Dispatcher.Invoke(() =>
            {
                bool _areCellRectsRefreshed = true;
                ISplitCtrl localSplit = _workSplit as ISplitCtrl;
                if (localSplit == null)
                    return;
                // if (localSplit.IsAddedCustomLayout)
                // {
                //    SplitCtrl0B sp0B = (SplitCtrl0B)_workSplit;
                //    foreach (CellBorder cellBd in localSplit.CellBorders)
                //    {
                //        cellBd.rect = _vm.GetFrameworkElementRect(cellBd);

                //        CellObj? cellObj = localSplit.CellList.Find(x => x.Name.Equals(cellBd.CellName));
                //        if (cellObj != null)
                //        {
                //            cellObj.rc = cellBd.rect;

                //            if (cellObj.rc.IsEmpty)
                //                _areCellRectsRefreshed = false;
                //        }
                //    }
                //}
                //else // if (_workingSplit.CellCount==5)
                //{
                foreach (CellObj objCell in localSplit.CellList)
                {
                    if (objCell.CellBd == null)
                    {
                        continue;
                    }

                    objCell.rc = _vm.GetFrameworkElementRect(objCell.CellBd);

                    if (objCell.rc.IsEmpty)
                    {
                        _areCellRectsRefreshed = false;
                    }
                }
                //}
                //else
                //{
                //    //foreach (CellObj objCell in _workingSplit.CellList)
                //    //{
                //    //    if (objCell.bd == null)
                //    //        continue;

                //    //    objCell.rc = _vm.GetFrameworkElementRect(objCell.bd);

                //    //    Trace.WriteLine($"Cell({objCell.Name})={ArrangeVM.FormatRect(objCell.rc)}");

                //    //    if (objCell.rc.IsEmpty)
                //    //        _areCellRectsRefreshed = false;
                //    //}

                //}
                if (!_areCellRectsRefreshed)
                {
                    //System.Threading.Timer timer1 = new System.Threading.Timer(refreshCellRects_TimerCallback, null, 100, Timeout.Infinite);
                    System.Threading.Timer? timer1 = null;
                    try
                    {
                        timer1 = new System.Threading.Timer((obj) => { RefreshCellRects(); }, null, 100, Timeout.Infinite);
                    }
                    catch (Exception e)
                    {
                        _vm.WriteLog($"[AwsBuddyWindow] create RefreshCellRects() timer exception: {e.Message}");
                    }
                    finally
                    {
                        timer1?.Dispose();
                        timer1 = null;
                    }
                }
                else
                {
                    string d = "";
                }

            });
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (System.Windows.Threading.Dispatcher.CurrentDispatcher != null)
            {
                System.Windows.Threading.Dispatcher.CurrentDispatcher.InvokeShutdown();
            }
        }
    }


}
