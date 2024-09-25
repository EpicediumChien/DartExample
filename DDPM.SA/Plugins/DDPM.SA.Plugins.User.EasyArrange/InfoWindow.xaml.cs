using DDPM.Easy.Common;
using nsWinEventHook;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using VcpCore.Common;

namespace DDPM.SA.Plugins.User.EasyArrange
{
    /// <summary>
    /// Interaction logic for InfoWindow.xaml
    /// </summary>
    public partial class InfoWindow : Window
    {
        private WinEventHook _winEventHook = new WinEventHook();

        #region Init

        public InfoWindow()
        {
            InitializeComponent();
        }
        public InfoWindow(ArrangeVM vm)
        {
            InitializeComponent();
            DataContext = vm;
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
            else
            {
                //Left = -99999;
                //Top = -99999;
                //Visibility = Visibility.Hidden;
            }
            WinEventHook_Start();

            //Hide window from Alt+tab
            System.Windows.Interop.WindowInteropHelper wndHelper = new System.Windows.Interop.WindowInteropHelper(this);
            Win32Lib.Win32.HideWinFromAltTab(wndHelper.Handle);

            //Test if a open window can create another window in a Dispatcher
            //Result: OK
            //this.Dispatcher.Invoke(() =>
            //{
            //    SaveCustomWindow w = new SaveCustomWindow(this);
            //    w.Show();
            //});

            //InitWorkWindows();
            //this.Dispatcher.Invoke(() =>
            //{
            //    _vmArrange.InitWorkWindows();
            //});

            //RefreshWorkWindows();
        }

        #endregion Init

        #region Exit

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            WinEventHook_Stop();
        }

        #endregion Exit

        #region ViewModel

        private ArrangeVM _vmArrange
        {
            get
            {
                return (ArrangeVM)DataContext;
            }
        }

        #endregion ViewModel

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
            _vmArrange.hWndForeground = hWndNew;
        }

        private bool _isDebuggingOnWindowStartMoving = true;

        private void OnWindowStartMovingProc(IntPtr hWnd)
        {
            //if (_isDebuggingOnWindowStartMoving)
            //    _log?.Info($"Enter OnWindowStartMovingProc(), hWnd=0x{hWnd:X}");

            if (!_vmArrange.IsFunctionEnabled)
                return;

            _vmArrange.hWndForeground = hWnd;

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
                            _vmArrange.PathNameForeground = pathName;

                            if (_isDebuggingOnWindowStartMoving)
                                _vmArrange.LogInfo($"Process.PathName={pathName}");

                            if (!_vmArrange.IsAllowToMoveFromPathName(pathName))
                            {
                                _vmArrange.IsMoving = false;
                                return;
                            }
                        }
                    }
                }
                catch (Exception e1)
                {
                    _vmArrange.StartMovingMsg = $"GetProcessPathName causes an exception: {e1.Message}";
                    _vmArrange.LogInfo($"@OnWindowStartMovingProc, {_vmArrange.StartMovingMsg}");

                    //Temporary allow to continue moving
                    _vmArrange.IsMoving = true;
                    //Robert_Lin Debug, let it contine
                    //return;
                }
            }
            else
            {
                _vmArrange.StartMovingMsg = $"GetProcessFromWindowHandle err: {msg}";
                _vmArrange.LogInfo($"@OnWindowStartMovingProc, {_vmArrange.StartMovingMsg}");
            }

            _vmArrange.RefreshScreenScale();
            Trace.WriteLine($"ScreenScale={_vmArrange.ScreenScale}");
            _vmArrange.IsMoving = true;
            _vmArrange.StartMovingMsg = "OK";
            _vmArrange.IsShiftPressed = WinEventHook.IsShiftPressed();
            //_vmArrange.DetermineWorkWindowVisibility();

            _vmArrange.RefreshCellRects();
        }

        private void OnWindowEndMovingProc(IntPtr hWnd, bool isCanceled = false)
        {
            bool isWorkUIShowing = _vmArrange.IsWorkUIShowing;

            if (!_vmArrange.IsMoving)
                return;

            _vmArrange.IsMoving = false;
            _vmArrange.StartMovingMsg = "";

            _vmArrange.IsShiftPressed = WinEventHook.IsShiftPressed();
            //_vmArrange.DetermineWorkWindowVisibility();

            if (!isWorkUIShowing)
                return;

            if (_vmArrange.HoveringCellObj == null)
                return;

            //Check if user cancel the window moving by pressing [Esc] key
            //Assumption:
            // When user moving window, the mouse [LeftButton] is pressed and hold.
            // When user canceling the moving, he/she press [Esc] key and the
            //     mouse [LeftButton] is strll pressed and hold.
            //
            if (WinEventHook.IsUserCancelMoving())
                return;

            Rect rcArrange = _vmArrange.HoveringCellObj.rc;

            //Inflate the rect, because the rcArrange not include the border thickness(=6) of CellBorder
            rcArrange.Inflate(6, 6);
            WinEventHook.SetWindowPosition(hWnd, rcArrange);
        }

        private void OnLocationChangedProc(int x, int y)
        {
            if (_vmArrange == null)
                return;

            _vmArrange.xCursor = x;
            _vmArrange.yCursor = y;

            //_vmArrange.DetermineWorkWindowVisibility();
            _vmArrange.IsShiftPressed = WinEventHook.IsShiftPressed();

            if (!_vmArrange.IsWorkUIShowing)
                return;

            CellObj orgCell = _vmArrange.HoveringCellObj;
            CellObj? newCell = _vmArrange.DetermineHoveringCellObj(x, y);

            if (orgCell != _vmArrange.HoveringCellObj)
            {
                string strOrg = "null";
                if (orgCell != null)
                    strOrg = orgCell.Name;
                string strNew = "null";
                if (_vmArrange.HoveringCellObj != null)
                    strNew = _vmArrange.HoveringCellObj.Name;

                Trace.WriteLine($" * HoveringCell: {strOrg}->{strNew}");
            }
            if (_vmArrange.HoveringCellObj != null)
            {
                _vmArrange.HoveringCell = _vmArrange.HoveringCellObj.Name;
            }
            else
            {
                _vmArrange.HoveringCell = "";
            }
            //if (_workingSplit != null)
            //    _workingSplit.VM.HoveringCell = vm.HoveringCell;

            //Set WorkWins to topmost
        }

        #endregion Window Event Handlers

        #region Init WorkWindows
        public void InitWorkWindows()
        {
            this.Dispatcher.Invoke(() =>
            {
                //_vmArrange.InitWorkWindows();
                _vmArrange.CreateWorkWindows2();
            });
        }
        #endregion
        //public void RefreshWorkWindows()
        //{
        //    this.Dispatcher.Invoke(() =>
        //    {
        //        List<MonitorInfo>? monitors = _vmArrange.GetMonitors();
        //        /*
        //        if (monitors == null)
        //        {
        //            _vmArrange.LogInfo("  * Monitors is null");
        //            return;
        //        }
        //        if (monitors.Count <= 0)
        //        {
        //            _vmArrange.LogInfo("  * Monitors is empty");
        //            return;
        //        }*/

        //        //Rebuild a new WorkWindows
        //        Dictionary<string, EAWorkWindow> tempWorkWindows = new Dictionary<string, EAWorkWindow>();

        //        var dpiXProperty = typeof(SystemParameters).GetProperty("DpiX", BindingFlags.NonPublic | BindingFlags.Static);
        //        var varX = (int)dpiXProperty.GetValue(null, null);
        //        double dpiX = (double)varX / (double)96;
        //        _vmArrange.LogInfo($"  * dpiX={dpiX}");

        //        //Refresh with new AllScreens
        //        _vmArrange.LogInfo($"  * Refreshing WorkWindows... AllScreens.Count={System.Windows.Forms.Screen.AllScreens.Length}");
        //        int idxScr = 0;
        //        foreach (Screen scr in System.Windows.Forms.Screen.AllScreens)
        //        {
        //            bool isVertical = (scr.Bounds.Width < scr.Bounds.Height);
        //            double left = scr.WorkingArea.Left / (double)dpiX;
        //            double top = scr.WorkingArea.Top / (double)dpiX;
        //            double width = scr.WorkingArea.Width / (double)dpiX;
        //            double height = scr.WorkingArea.Height / (double)dpiX;
        //            _vmArrange.LogInfo($"    - Screen[{idxScr}] {scr.DeviceName}   IsPrimary={scr.Primary}");
        //            _vmArrange.LogInfo($"      WorkingArea: ({left},{top}){width}x{height}");

        //            EAWorkWindow workWin = null;

        //            //Try to find if the WorkWindow work for current scr is exist
        //            if (_vmArrange.WorkWindows.TryGetValue(scr.DeviceName, out workWin))
        //            {
        //                _vmArrange.LogInfo($"      Changed: (Exist => refresh WorkingArea)");
        //                //The scr have an existing WorkWindow, no need to create new
        //                //Just to renew some screen properties
        //                //workWin.Left = left;
        //                //workWin.Top = top;
        //                //workWin.Width = width;
        //                //workWin.Height = height;
        //                workWin.ChangeWindowPos(left, top, width, height);

        //                //Refresh screen orientation (not been implemented)

        //                //Add to temp workwindows
        //                tempWorkWindows.Add(scr.DeviceName, workWin);
        //                //Remove from old dictionary
        //                _vmArrange.WorkWindows.Remove(scr.DeviceName);
        //            }
        //            else
        //            {
        //                _vmArrange.LogInfo($"      Changed: (Added => Add new WorkWindow)");

        //                //Cannot find the WorkWindow which is work for scr => scr is a new screen
        //                //We will need to create a new WorkWindow work for scr
        //                List<MonitorInfo> attachedMonitors = monitors.FindAll(x => x.DisplayName.Equals(scr.DeviceName, StringComparison.OrdinalIgnoreCase));

        //                //If there is no any Dell Monitor attached on this Screen, then do not need to create a
        //                // Workwindow for it

        //                //if ((attachedMonitors == null) || (attachedMonitors.Count <= 0))
        //                //    continue;

        //                //Read settings for this monitor
        //                int cellCount = 2;
        //                char splitKey = 'A';
        //                List<double> settings = new List<double>();

        //                //New a WorkWindow in local
        //                EAWorkWindow addedWorkWin = new EAWorkWindow(_vmArrange, scr, attachedMonitors);
        //                addedWorkWin.Left = left;
        //                addedWorkWin.Top = top;
        //                addedWorkWin.Width = width;
        //                addedWorkWin.Height = height;
        //                addedWorkWin.Show();
        //                addedWorkWin.SetWorkingSplit(cellCount, splitKey, settings);
        //                //addedWorkWin.Show();
                        
        //                tempWorkWindows.Add(scr.DeviceName, addedWorkWin);
        //            }
        //            idxScr++;
        //        } //foreach (Screen scr)

        //        //Case_3 Exist->NotExist, a display has been unplugged
        //        _vmArrange.LogInfo($"  * Removing unplugged WorkWindows... Count={_vmArrange.WorkWindows.Count}");
        //        foreach (KeyValuePair<string, EAWorkWindow> pair in _vmArrange.WorkWindows)
        //        {
        //            _vmArrange.WorkWindows.Remove(pair.Key);
        //            //Close the workWin
        //            pair.Value.DispatcherClose();
        //        }

        //        _vmArrange.WorkWindows = tempWorkWindows;
        //    });
        //}

        //private void chkIsFading_Click(object sender, RoutedEventArgs e)
        //{
        //    if (sender != null)
        //    {
        //        System.Windows.Controls.CheckBox checkBox = sender as System.Windows.Controls.CheckBox;
        //        if (checkBox != null)
        //        {
        //            bool isChecked = checkBox.IsChecked == true;
        //            foreach (KeyValuePair<string, EAWorkWindow> pair in _vmArrange.WorkWindows)
        //            {
        //                EAWorkWindow workWin = pair.Value as EAWorkWindow;
        //                if (workWin != null)
        //                {
        //                    workWin.IsFading = isChecked;
        //                }
        //            }
        //        }
        //    }
        //}
    }
}