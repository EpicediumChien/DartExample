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
using nsWinEventHook;
using DDPM.Easy.Common;
using DDPM.Win32Lib;


namespace DDPM.EABroker
{
    /// <summary>
    /// Interaction logic for InfoWindow.xaml
    /// </summary>
    public partial class InfoWindow : Window
    {
        private readonly ArrangeVM _vm;
        private WinEventHook _winEventHook = new WinEventHook();

        #region Init
        public InfoWindow(ArrangeVM vm)
        {
            InitializeComponent();
            _vm = vm;
            DataContext = _vm;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            bool IsInfoWindowVsible = Win32Lib.Win32.IniReadInt(
                "DDPMDebug", "DDPM.SA.EAPlugin.InfoWindow.IsVisible", 0, @"C:\temp\DDPMDebug.txt") == 1;
            if (IsInfoWindowVsible)
            {
                Left = 100;
                Top = 50;
                //Opacity = 1;
            }

            WinEventHook_Start();

            //Hide window from Alt+tab
            System.Windows.Interop.WindowInteropHelper wndHelper = new System.Windows.Interop.WindowInteropHelper(this);
            Win32Lib.Win32.HideWinFromAltTab(wndHelper.Handle);

            InitLayoutList();
            //InitPresetLayoutsComboBox();
        }
        #endregion Init

        #region Exit
        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            WinEventHook_Stop();
        }
        #endregion Exit

        #region Window Event Hook
        private void WinEventHook_Start()
        {
            _winEventHook.OnStartMoving += OnWindowStartMovingProc;
            _winEventHook.OnEndMoving += OnWindowEndMovingProc;
            _winEventHook.OnLocationChanged += OnLocationChangedProc;
            _winEventHook.OnForegroundWindowChanged += OnForegroundWindowChangedProc;
            _winEventHook.Hook();
        }

        private void WinEventHook_Stop()
        {
            _winEventHook.Unhook();
            _winEventHook.OnStartMoving -= OnWindowStartMovingProc;
            _winEventHook.OnEndMoving -= OnWindowEndMovingProc;
            _winEventHook.OnLocationChanged -= OnLocationChangedProc;
            _winEventHook.OnForegroundWindowChanged -= OnForegroundWindowChangedProc;
        }
        #endregion Window Event Hook

        #region Window Event Handlers

        private void OnForegroundWindowChangedProc(IntPtr hWndNew, IntPtr hWndOld)
        {
            //Noting to do in this project
            _vm.hWndForeground = hWndNew;
        }

        private bool _isDebuggingOnWindowStartMoving = true;
        private int _isRefresCellsCountAfterStartMoving = 0;

        private void OnWindowStartMovingProc(IntPtr hWnd)
        {
            _vm.hWndForeground = hWnd;
            _vm.RefreshScreenScale();

            //Step_1, determine the moving window is allowed to move
            //
            Process process;
            string msg;
            if (WinEventHook.GetProcessFromWindowHandle(hWnd, out process, out msg))
            {
                //Try to get the PathName of the process
                try
                {
                    if (process.MainModule != null)
                    {
                        if (!String.IsNullOrEmpty(process.MainModule.FileName))
                        {
                            string pathName = process.MainModule.FileName;
                            _vm.PathNameForeground = pathName;

                            if (ArrangeVM.IsEAExcludedPathName(pathName))
                            {
                                _vm.StartMovingMsg = "Moving window is in EAExcluded list";
                                _vm.IsMoving = false;
                                return;
                            }
                            else
                            {
                                _vm.StartMovingMsg = "OK";
                                _vm.IsMoving = true;
                            }
                        }
                    }
                }
                catch (Exception e1)
                {
                    _vm.StartMovingMsg = $"GetProcessPathName causes an exception: {e1.Message}";
                    _vm.WriteLog("GetProcessPathName() causes EXCEPTION", e1);

                    //Robert_Lin, 2024-10-26, Option 1: when moving an administrator window
                    // the WorkWindow will display the selected layout on the target screen
                    //=> comment-out below will "Not display"
                    //_vm.IsMoving = true;

                    //Robert_Lin 2024-10-26,Option 2:
                    // Comment out below 2 statements will show Workwindow when user moving a
                    // administrator window. (but Admin window will not be moved)
                    _vm.IsMoving = false;
                    return;
                }
            }
            else
            {
                _vm.StartMovingMsg = $"GetProcessFromWindowHandle err: {msg}";
                _vm.WriteLog($"@OnWindowStartMovingProc, {_vm.StartMovingMsg}");
            }

            //Get current Screen from cursor
            _vm.RefreshWorkScreen();

            //Step_2, Set flags to show windows
            //

            _vm.IsShiftPressed = WinEventHook.IsShiftPressed();


            if (_vm.IsAwsWindowVisible)
            {
                //AwsWindowVisibilityChange will trigger to call this method
                //_vm.AwsWindow.ReloadRecentList(_vm.WorkScreen.DeviceName);
            }

            //Temporary always update
            //if (_vmArrange.IsAwsWindowVisible)
            //{
            //    if (_vmArrange.AwsWindow != null)
            //    {
            //        _vmArrange.AwsWindow.OnStartMoving();
            //    }
            //}

            _vm.RefreshCellRects();
            //_vmArrange.RefreshCellRects();
            //_isRefresCellsCountAfterStartMoving = 0;
        }

        private void OnWindowEndMovingProc(IntPtr hWnd, bool isCanceled = false)
        {
            _vm.IsShiftPressed = WinEventHook.IsShiftPressed();
            if (!_vm.IsMoving)
            {
                return;
            }

            _vm.IsMoving = false;
            _vm.StartMovingMsg = "";

            CellObj? hoveringCellObj = _vm.HoveringCellObj;
            if (hoveringCellObj == null)
            {
                return;
            }

            Win32.RECT rcWnd = new Win32.RECT();
            Win32._GetWindowRect(hWnd, out rcWnd);
            _vm.rcWndForeground = new Rect((double)rcWnd.X, (double)rcWnd.Y, (double) rcWnd.Width, (double)rcWnd.Height);


            //bool isWorkUIShowing = _vm.IsWorkWindowVisible;

            //_vmArrange.DetermineWorkWindowVisibility();

            //if (!isWorkUIShowing)
            //    return;

            //if (_vm.HoveringCellObj == null)
            //    return;

            //Check if user cancel the window moving by pressing [Esc] key
            //Assumption:
            // When user moving window, the mouse [LeftButton] is pressed and hold.
            // When user canceling the moving, he/she press [Esc] key and the
            //     mouse [LeftButton] is strll pressed and hold.
            //
            if (WinEventHook.IsUserCancelMoving())
                return;

            Rect rcArrange = hoveringCellObj.rc;

            if (_vm.HoveringWindow.Equals("aws"))
            {
                rcArrange = _vm.GetHoveringRectFromAwsBuddyWindow(); 
                if (rcArrange.IsEmpty)
                    rcArrange = _vm.AwsWindow.CalculateHoveringCellArrangeRect();
            }

            if (rcArrange.IsEmpty)
                return;

            //Inflate the rect, because the rcArrange not include the border thickness(=6) of CellBorder
            if (_vm.IsWithoutGap)
            {
                rcArrange.Inflate(6, 6);
            }
            if (!rcArrange.IsEmpty)
            {
                WinEventHook.SetWindowPosition(hWnd, rcArrange);
                if (_vm != null)
                {
                    _vm.SendTelemetry_EasyArrangeLayout();
                }
            }
        }


        private void OnLocationChangedProc(int x, int y) 
        {
            _vm.IsShiftPressed = WinEventHook.IsShiftPressed();
            double deltaX = Math.Abs(x - _vm.xCursor);
            double deltaY = Math.Abs(y - _vm.yCursor);

            //if ((x == _vm.xCursor) || (y == _vm.yCursor))
            //{
            //    return;
            //}
            if ((deltaX < 2.00) && (deltaY < 2.00))
                return;

            _vm.xCursor = x; _vm.yCursor = y;
            //Screen? cursorScreen = _vm.GetScreenFromCursor();
            Screen? cursorScreen = Screen.FromPoint(new System.Drawing.Point(x, y));
            _vm.WorkScreen = cursorScreen;

            if (!_vm.IsMoving)
                return;

            Dispatcher.BeginInvoke(new Action(() =>
            {
            //}));
            //this.Dispatcher.Invoke(() =>
            //{


                CellObj orgCell = _vm.HoveringCellObj;
             CellObj? newCell = _vm.DetermineHoveringCellObj(x, y);
            //CellObj? newCell = null; // _vm.DetermineHoveringCellObj(x, y);

            if (orgCell != _vm.HoveringCellObj)
            {
                string strOrg = "null";
                if (orgCell != null)
                    strOrg = orgCell.Name;
                string strNew = "null";
                if (_vm.HoveringCellObj != null)
                    strNew = _vm.HoveringCell;

                Trace.WriteLine($" * HoveringCell: {strOrg}->{strNew}");
            }
                //if (_vm.HoveringCellObj != null)
                //{
                //    _vm.HoveringCell = _vm.HoveringCellObj.Name;
                //}
                //else
                //{
                //    _vmArrange.HoveringCell = "";
                //}
                //if (_workingSplit != null)
                //    _workingSplit.VM.HoveringCell = vm.HoveringCell;

                //Set WorkWins to topmost

            }));

        }

        #endregion Window Event Handlers

        #region Layouts
        private void InitLayoutList()
        {
            foreach (ISplitCtrl isp in ISplitCtrl.Splits_EA)
            {
                lbLayouts.Items.Add($"({isp.EAID}) {isp.CtrlClass}");
            }
        }
        private void reloadCustomLayoutsButton_Click(object sender, RoutedEventArgs e)
        {

        }
        private void sekectLayoutButton_Click(object sender, RoutedEventArgs e)
        {
            object selItem = lbLayouts.SelectedItem;
            if (selItem != null)
            {
                System.Windows.Interop.WindowInteropHelper wndHelper = new System.Windows.Interop.WindowInteropHelper(this);
                Screen scr = Screen.FromHandle(wndHelper.Handle);
            }
        }
        #endregion

    }
}
