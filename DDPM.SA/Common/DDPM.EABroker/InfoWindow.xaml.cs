using System.Diagnostics;
using System.Text;
using System.Windows;
using nsWinEventHook;
using DDPM.Easy.Common;
using DDPM.Win32Lib;
using DDPM.SA.Common.Display;
using DDPM.SA.Common;
using VcpCore.Common;
using DDPM.SA.Common.Settings;
using System.Windows.Threading; // for DispatcherOperation


namespace DDPM.EABroker
{
    /// <summary>
    /// Interaction logic for InfoWindow.xaml
    /// </summary>
    public partial class InfoWindow : Window
    {
        #region Private members
        private readonly ArrangeVM _vm;
        private WinEventHook _winEventHook = new WinEventHook();
        //Flags to prevent Dispatcher.InvokeAsync() twice
        private DispatcherOperation? _pendingOp_DeterminHoveringCell = null;
        #endregion

        #region Init
        public InfoWindow(ArrangeVM vm)
        {
            InitializeComponent();
            _vm = vm;
            DataContext = _vm;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //Developer Debug Flags Changed:
            //OLD:
            //bool IsInfoWindowVsible = Win32Lib.Win32.IniReadInt(
            //        "DDPMDebug", "DDPM.SA.EAPlugin.InfoWindow.IsVisible", 0, @"C:\temp\DDPMDebug.txt") == 1;
            //NEW:
            bool IsInfoWindowVisible = DDPM.SA.Common.Settings.DevSettings.IsEAInfoWindowVisible();

            if (IsInfoWindowVisible)
            {
                Left = 100;
                Top = 50;
                //Opacity = 1;
            }

            WinEventHook_Start();

            //Hide window from Alt+tab
            System.Windows.Interop.WindowInteropHelper wndHelper = new System.Windows.Interop.WindowInteropHelper(this);
            Win32Lib.Win32.HideWinFromAltTab(wndHelper.Handle);

          //  InitLayoutList();
            //InitPresetLayoutsComboBox();

            if (DevSettings.IsTestMonitorInfoUpdateEnabled())
            {
                testMonitorInfoUpdateButton.Visibility = Visibility.Visible;
            }
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

            _winEventHook.Unhook(); //Derek 2025/03/28
        }
        #endregion Window Event Hook

        #region Window Event Handlers

        private void OnForegroundWindowChangedProc(IntPtr hWndNew, IntPtr hWndOld)
        {
            //Noting to do in this project
            _vm.hWndForeground = hWndNew;
        }

        //Derek 2025/04/01
        //private bool _isDebuggingOnWindowStartMoving = true;
        //private int _isRefresCellsCountAfterStartMoving = 0;

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
                    if (process.MainModule != null && !String.IsNullOrEmpty(process.MainModule.FileName))
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
        }

        private void OnWindowEndMovingProc(IntPtr hWnd, bool isCanceled = false)
        {
            //_vm.IsShiftPressed = WinEventHook.IsShiftPressed();
            if (!_vm.IsMoving)
            {
                return;
            }

            bool isActiveWindowVisible = false;
            if (_vm.IsAwsEnabled)
            {
                isActiveWindowVisible = _vm.IsAwsWindowVisible;
            }
            else
            {
                isActiveWindowVisible = _vm.IsWorkWindowVisible;
            }
            _vm.IsMoving = false;
            _vm.StartMovingMsg = "";

            CellObj? hoveringCellObj = _vm.HoveringCellObj;
            if (hoveringCellObj == null)
            {
                return;
            }
            if (!isActiveWindowVisible)
                return;


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

            if (_vm.HoveringWindow.Equals("scr") || _vm.HoveringWindow.Equals("aws"))
            {
                rcArrange = _vm.GetHoveringRectFromAwsBuddyWindow();
                if (rcArrange.IsEmpty && _vm.AwsWindow != null)
                    rcArrange = _vm.AwsWindow.CalculateHoveringCellArrangeRect();
                if (rcArrange.IsEmpty)
                    return;
            }


            //Inflate the rect, because the rcArrange not include the border thickness(=6) of CellBorder
            if (_vm.IsWithoutGap)
            {
                
                double extendedFrameBoundsHorz = 4;

                rcArrange.Inflate(6, 5);
                double arrWidth = rcArrange.Width;
                double arrHeight = rcArrange.Height;
                rcArrange.X -= extendedFrameBoundsHorz;
                rcArrange.Width += extendedFrameBoundsHorz;
                rcArrange.Y += 1;
                rcArrange.Height += 1;

                //To prevent the rcArrange acrouss screen boundary after Inflated
                if (_vm.WorkScreen != null)
                {
                    //Check top, do not out of screen working area
                    if (rcArrange.Top < _vm.WorkScreen.WorkingArea.Top)
                    {
               //         rcArrange.Y = _vm.WorkScreen.WorkingArea.Top;

                    }
                    //Check if Left border across screen boundary? (Unused section)
                    int scrLeft = _vm.WorkScreen.Bounds.Left;
                    if ((rcArrange.Left < scrLeft) && (rcArrange.Right > scrLeft))
                    {
                        //Robert_Lin 2025-2-5 to prevent raArrange.Width assign a <0 value
                        //OLD:
                        //double dx = scrLeft - rcArrange.Left;
                        //rcArrange.X = scrLeft;
                        //rcArrange.Width -= dx;
                        //NEW:
                        double dx = scrLeft - rcArrange.Left;
                        //Change - to +
                        double w = rcArrange.Width + dx;
                    //    rcArrange.X = scrLeft;
                        //if (w >= 0)
                        //    rcArrange.Width = w;
                    }
                }
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
            //_vm.IsShiftPressed = WinEventHook.IsShiftPressed();
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

            //NEW: using Dispatcher.InvokeAsync
            //
            ///*
            _pendingOp_DeterminHoveringCell?.Abort();
            //Dispatcher.BeginInvoke(new Action(() =>
            _pendingOp_DeterminHoveringCell = Dispatcher.InvokeAsync(() =>
            {
                _pendingOp_DeterminHoveringCell = null;

                CellObj orgCell = _vm.HoveringCellObj;
                CellObj? newCell = _vm.DetermineHoveringCellObj(x, y);

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
            }, DispatcherPriority.Loaded);
            //*/

            //OLD: using Dispatcher.BeginInvoke
            //
            /*
            Dispatcher.BeginInvoke(new Action(() =>
            {

                CellObj? orgCell = _vm.HoveringCellObj;
                CellObj? newCell = _vm.DetermineHoveringCellObj(x, y);

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
            }));
            */
        }

        #endregion Window Event Handlers

        #region Layouts
        private void refreshMonitorsButton_Click(object sender, RoutedEventArgs e)
        {
            List<MonitorInfo>? monitors = _vm.GetMonitors();

            if (monitors == null) return;

            cbMonitors.ItemsSource = monitors;
        }

        private void SetSelectedLayoutButton_Click(object sender, RoutedEventArgs e)
        {
            if (cbMonitors.SelectedItem == null) return;
            MonitorInfo? monitorInfo = cbMonitors.SelectedItem as MonitorInfo;

            if (monitorInfo == null) return;

            string eaIdText = tbEAId.Text;
            if (String.IsNullOrEmpty(eaIdText)) return;
            int eaId = 0;
            if (int.TryParse(eaIdText, out eaId))
            {
                _vm.SetEASelectedLayout(monitorInfo, eaId);
            }
        }

        private void refreshRecentListButton_Click(object sender, RoutedEventArgs e)
        {
            if (cbMonitors.SelectedItem == null) return;

            MonitorInfo? monitorInfo = cbMonitors.SelectedItem as MonitorInfo;
            if (monitorInfo == null) return;

            EAMonitorSettings? eaSettings = _vm.ReadEAMonitorSettings(monitorInfo);
            if (eaSettings == null) return;

            SplitJson spjSelected = eaSettings.SelectedSplit;
            StringBuilder? sb = new StringBuilder();

            string mark = "*";
            foreach (SplitJson spjRecent in eaSettings.RecentList)
            {
                if (spjRecent.IsEquals(spjSelected))
                {
                    sb.Append(mark);
                }
                sb.Append($"[{spjRecent.EAID}]{spjRecent.CellCount}{spjRecent.SplitKey}");
                sb.Append("   ");
            }
            string strOut = sb.ToString();
            strOut = strOut.Trim();
            txtRecentList.Text = strOut;
            sb.Clear();
            sb = null;
        }

        private void refreshCustomListButton_Click(object sender, RoutedEventArgs e)
        {
            SplitJson[] customList = _vm.ReadCustomList();
            StringBuilder? sb = new StringBuilder();

            foreach (SplitJson spjCustom in customList)
            {
                sb.Append($"[{spjCustom.EAID}]{spjCustom.CellCount}{spjCustom.SplitKey}");
                sb.Append("   ");
            }
            string strOut = sb.ToString();
            strOut = strOut.Trim();
            txtCustomList.Text = strOut;
            sb.Clear();
            sb = null;
        }

        //Derek 2025/04/01 due to reference = 0
        //private void reloadCustomLayoutsButton_Click(object sender, RoutedEventArgs e)
        //{

        //}
        //private void sekectLayoutButton_Click(object sender, RoutedEventArgs e)
        //{
        //    //object selItem = lbLayouts.SelectedItem;
        //    //if (selItem != null)
        //    //{
        //    //    System.Windows.Interop.WindowInteropHelper wndHelper = new System.Windows.Interop.WindowInteropHelper(this);
        //    //    Screen scr = Screen.FromHandle(wndHelper.Handle);
        //    //}
        //}
        #endregion

        private void IsSpanMultipleMonitorsCheckbox_Click(object sender, RoutedEventArgs e)
        {

        }

        private void editOverlapButton_Click(object sender, RoutedEventArgs e)
        {
            if (_vm == null)
                return;

            MonitorInfo? mi = _vm.GetSelectedMonitorInfo();
            if (mi == null)
                return;

            EAArgs eaArgs = new EAArgs()
            {
                Command = "EditCommand",
                SplitJson = new SplitJson()
                {
                    CellCount = 0,
                    SplitKey = 'B',
                    Settings = new List<double>(),
                    EAID = 0
                }
            };
            _vm.Invoke_EditCommand(mi, eaArgs);
        }

        /// <summary>
        /// Robert_Lin 2025-3-19 added to terminate the Dispatcher.Run() loop in EABroker.InitAllWindows()
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (System.Windows.Threading.Dispatcher.CurrentDispatcher != null)
            {
                 System.Windows.Threading.Dispatcher.CurrentDispatcher.InvokeShutdown();
            }
        }

        private void testMonitorInfoUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (_vm != null)
            {
                EAArgs eaArgs = new EAArgs()
                {
                    Command = EAEMConstants.EACommand_TestMonitorInfoUpdated,
                };
                _vm.SendEANotifyToUI(eaArgs);
            }
        }

        private void makeExceptionButton_Click(object sender, RoutedEventArgs e)
        {
            _vm.WriteLog("MakeExceptionButton_Click");
            throw new Exception("MakeExceptionButton_Click");
        }
    }
}
