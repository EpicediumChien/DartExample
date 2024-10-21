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
            if (_workScreen != null)
            {
                if (_workScreen.Equals(screen))
                {
                    return;
                }
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

                if (_workSplit != null)
                {
                    if (_workSplit.IsVertical != IsVertical)
                        _workSplit.IsVertical = IsVertical;
                }
            });
        }
        public void SetWorkSplit(ISplitCtrl splitCtrl)
        {
            if (_workSplit != null)
            {

            }
            _workSplit = splitCtrl;

            if (_workSplit == null)
                return;

            this.Dispatcher.Invoke(() =>
            {
                _workSplit = splitCtrl.Clone();

                if (_workSplit.IsAddedCustomLayout) //SplitCtrl0B
                {
                    SplitCtrl0B sp0B = new SplitCtrl0B();

                    //Fix the rcScreen from _workScreen
                    Rect rcScreen = new Rect();
                    rcScreen.X = _workScreen.Bounds.Left / _vm.ScreenScale;
                    rcScreen.Y = _workScreen.Bounds.Top / _vm.ScreenScale;
                    rcScreen.Width = _workScreen.Bounds.Width / _vm.ScreenScale;
                    rcScreen.Height = _workScreen.Bounds.Height / _vm.ScreenScale;

                    //Create Borders and CellBorders to canvas grid
                    sp0B.ApplySettingsToCellList(rcScreen);
                }

                _workSplit.SplitMode = eSplitModes.Work;
                _workSplit.IsEditable = false;
                _workSplit.IsVertical = IsVertical;

                splitContent.Content = _workSplit.UC;
            });
        }

        public void RefreshCellRects(int flag=0)
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
                        System.Threading.Timer timer1 = new System.Threading.Timer((obj) => { RefreshCellRects(1); }, null, 100, Timeout.Infinite);
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
            SetWorkSplit(newSplit);
        }
        private void HandleHoveringAwsCellObjChanged(object? sender, CellObj cellObj)
        {
            ISplitCtrl? localSplit = _workSplit;

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
                        //if (cellBd.rect.Contains(x, y))
                        //{
                        //    cellBd.IsHover = true;
                        //    //Convert to CellObj
                        //    hoverCell = new CellObj(cellBd.Name, cellBd.Border);
                        //    hoverCell.rc = cellBd.rect;
                        //}
                        //else
                        //    cellBd.IsHover = false;
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

            }
        }
    }
}
