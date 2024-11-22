using CommunityToolkit.Mvvm.ComponentModel;
using DDPM.Easy.Common;
using DDPM.SA.Common;
using DDPM.SA.Common.Settings;
using Dell.Client.Framework.Common;
using nsWinEventHook;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;
using System.Security.Cryptography.Xml;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media.Media3D;
using VcpCore.Common;
using static System.Net.Mime.MediaTypeNames;
using DDPM.SA.Common.Display;
using System.Diagnostics.Eventing.Reader;

namespace DDPM.SA.Plugins.User.EasyArrange
{
    public class ArrangeVM : ObservableObject
    {
        #region Private members
        private readonly object _lockObject = new();
        private readonly object _lockScreenMgr = new();
        private IDisplayService? _displayManagerPlugin;
        private IDeviceManagerSA? _deviceManagerPlugin;

        private bool _isMoving = false; //true when a window is moving
        //Cursor position to VirtualScreen
        private int _xCursor = 0;
        private int _yCursor = 0;

        private Screen _workingScreen; //when (_isMoving==true), will update the Screen of current cursor
        #endregion

        #region Events
        //Invoked,when (_isMoving==true) and cursor position cross screen boundary
        public EventHandler<Screen> WorkingScreenChanged;

        //Invoked when AWS Window visibility changed
        public EventHandler<bool> AwsWindowVisibilityChanged;
        #endregion

        #region Enabled flag

        private bool _isFunctionEnabled = true;

        /// <summary>
        /// Eanble/Disable EasyArrange functions, including Edit, Work,
        /// </summary>
        public bool IsFunctionEnabled
        {
            get => _isFunctionEnabled;
            set => SetProperty(ref _isFunctionEnabled, value);
        }

        #endregion Enabled flag

        #region Option flags

        private bool _isWorkUIEnabled = true;

        public bool IsWorkUIShowing
        {
            get
            {
                if (! IsMoving)
                    return false;
                if (!_isWorkUIEnabled)
                    return false;

                if (EzSettings.IsOnlyAllowWhenShiftKeyPressed)
                {
                    //LogInfo($"@ ArrangeVM.IsWorkUIShowing: IsShiftPressed={IsShiftPressed}");
                    return IsShiftPressed;
                }
                return true;
            }
        }

        /// <summary>
        /// Manually enable/disable WorkWindow function.
        /// For example, when user is editing custom layout (EditWindow is working),
        /// Set this function to false, so user can move widows and not been arranged.
        /// </summary>
        public bool IsWorkUIEnabled
        {
            get => _isWorkUIEnabled;
            set
            {
                SetProperty(ref _isWorkUIEnabled, value);
                OnPropertyChanged("IsWorkUIShowing");
            }
        }

        /// <summary>
        /// The key flag to show UI, and let the moving (OnLocationChanged handler) to continue
        /// </summary>
        public bool IsMoving
        {
            get => _isMoving;
            set
            {
                bool isChanged = (value != _isMoving);
                SetProperty(ref _isMoving, value);
                OnPropertyChanged("IsWorkUIShowing");
                OnPropertyChanged("IsAwsWindowVisible");
                if (isChanged)
                {
                    if (IsMovingChanged != null)
                        IsMovingChanged(this, IsMoving);
                }
                if (value)
                {
                    RefreshWorkWinInfos();
                }
            }
        }

        private bool _isShiftPressed = false;
        public bool IsShiftPressed
        {
            get => _isShiftPressed;
            set
            {
                SetProperty(ref _isShiftPressed, value);
                OnPropertyChanged("IsWorkUIShowing");
                OnPropertyChanged("IsAwsWindowVisible");
            }
        }

        public event EventHandler<bool> IsMovingChanged;

        #endregion Option flags

        #region Cursor position
        /// <summary>
        /// Cursor position (xCursor, yCursor) will be updated by InfoWindow (OnLocationChanged handler).
        /// and then use it to determine if the custor is inside a CellBorder.
        /// </summary>
        public int xCursor
        {
            get { return _xCursor; }
            set
            {
                _xCursor = value;
                OnPropertyChanged("xCursor");
            }
        }
        public int yCursor
        {
            get { return _yCursor; }
            set
            {
                _yCursor = value;
                OnPropertyChanged("yCursor");
            }
        }
        #endregion Cursor position

        #region Screen Scale

        private double _screenScale = 1.00;

        /// <summary>
        /// Update the screen scale when start moving, will be used to fix the coordinates later
        /// </summary>

        public double ScreenScale
        {
            get => _screenScale;
            set => SetProperty(ref _screenScale, value);
        }

        public double RefreshScreenScale()
        {
            var dpiXProperty = typeof(SystemParameters).GetProperty("DpiX", BindingFlags.NonPublic | BindingFlags.Static);
            var varX = (int)dpiXProperty.GetValue(null, null);
            double dpiX = (double)varX / (double)96;
            if (dpiX >= 1.0000)
                ScreenScale = dpiX;
            return ScreenScale;
        }
        #endregion Screen Scale

        #region WorkingScreen
        //Will be updated by InfoWindow
        public Screen WorkingScreen
        {
            get { return _workingScreen; }
            set
            {
                if (value != _workingScreen)
                {
                    _workingScreen = value;
                    if (WorkingScreenChanged != null)
                    {
                        Task.Run(() => WorkingScreenChanged.Invoke(this, _workingScreen));
                    }
                }
            }
        }
        #endregion WorkingScreen

        #region Hovering Cell

        private CellObj? _hoveringCellObj = null;

        /// <summary>
        /// This property will be refreshed by OnLocationChanged handler.
        /// When moving stop, will use it as the target Cell to move the target window into this cell rect.
        /// </summary>
        public CellObj? HoveringCellObj
        {
            get => _hoveringCellObj;
            set
            {
                SetProperty(ref _hoveringCellObj, value);
                OnPropertyChanged();
            }
        }

        private string _hoveringCell = "";

        /// <summary>
        /// Property which is used from DataBinding by SplitCtrls
        /// </summary>
        public string HoveringCell
        {
            get { return _hoveringCell; }
            set => SetProperty(ref _hoveringCell, value);
        }

        public CellObj? DetermineHoveringCellObj(int x, int y)
        {
            CellObj? hoveringCell = null;

            if (IsAwsWindowVisible)
            {
                if (AwsWindow !=null)
                {
                    CellObj? cellObj = AwsWindow.DetermineHoveringCellObj(x, y);
                    if (cellObj != null)
                    {
                        HoveringScreen = AwsWindow.ScreenDeviceName;
                        HoveringWindow = "aws";
                        HoveringCellObj = cellObj;
                        HoveringSplit = AwsWindow.HoveringSplit;

                        hoveringCell = cellObj;
                        foreach (EAWorkWindow workWin in _workWindows2)
                        {
                            if (!workWin.IsUsed)
                                continue;
                            if (workWin.ScreenDeviceName.Equals(HoveringScreen))
                            {
                                if (workWin.IsSameWorkSplit(HoveringSplit))
                                {
                                    workWin.SetWorkSplitHoveringCellName(hoveringCell.Name);
                                }
                                else
                                {
                                    workWin.SetWorkSplitHoveringCellName("");
                                }
                            }
                            else
                            {
                                workWin.SetWorkSplitHoveringCellName("");
                            }
                        }
                        return cellObj;
                    }
                }
            }

            int idxWorkWin = -1;
            foreach (EAWorkWindow workWin in _workWindows2)
            {
                idxWorkWin++;
                if (!workWin.IsUsed)
                    continue;

                CellObj? cellObj = workWin.DetermineHoveringCellObj(x, y);
                if (cellObj != null)
                {
                    HoveringScreen = workWin.ScreenDeviceName;
                    HoveringCellObj = cellObj;
                    HoveringWindow = $"w{idxWorkWin}";
                    return cellObj;
                }
            }
            HoveringCellObj = null;
            return null;
        }

        #endregion Hovering Cell

        #region HoveringScreen

        private string _hoveringScreen = "";

        /// <summary>
        /// The Screen.DeviceName of the hovering cell.
        /// When hover on EAWorkWindow, then HoveringScreen is the Screen of mouse cursor.
        /// When hover on AwsWindow, then the HoveringScreen is the Screen of the selected monitor in
        ///    AwsWindow, Icon0.
        /// </summary>
        public string HoveringScreen
        {
            get => _hoveringScreen;
            set => SetProperty(ref _hoveringScreen, value);
        }

        #endregion HoveringScreen

        #region HoveringSplit
        private ISplitCtrl _hoveringSplit;
        public ISplitCtrl HoveringSplit
        {
            get { return _hoveringSplit; }
            private set
            {
                _hoveringSplit = value;
            }
        }
        #endregion HoveringSplit

        #region Hovering Window
        //Values:
        //"" : no hovering Window;
        //"w0" : WorkWindows[0]; "w1" : WorkWindows[1], ...
        //"aws : AWS Window
        public string HoveringWindow { get; set; } = "";
        #endregion

        #region WorkWindowList
        public const int maxWorkWindowCount = 5;
        private List<EAWorkWindow> _workWindows2 = new List<EAWorkWindow>();
        private int _workWindowUsedCount = 0;

        public void AddWorkWindow(EAWorkWindow workWindow)
        {
            _workWindows2.Add(workWindow);
        }

        public void CreateWorkWindows2()
        {
            if (_workWindows2.Count >= maxWorkWindowCount)
                return;

            int idx = _workWindows2.Count;
            //[0]
            Task.Run(new Action(() => { CreateWorkWindowAndAddToList(idx); }));
            idx++;
            while (_workWindows2.Count < idx)
            {
                Thread.Sleep(10);
                Trace.WriteLine($"WorkWindows.Count={_workWindows2.Count}");
            }
            if (_workWindows2.Count >= maxWorkWindowCount)
                return;

            //[1]
            Task.Run(new Action(() => { CreateWorkWindowAndAddToList(idx); }));
            idx++;
            while (_workWindows2.Count < idx)
            {
                Thread.Sleep(10);
                Trace.WriteLine($"WorkWindows.Count={_workWindows2.Count}");
            }
            if (_workWindows2.Count >= maxWorkWindowCount)
                return;

            //[2]
            Task.Run(new Action(() => { CreateWorkWindowAndAddToList(idx); }));
            idx++;
            while (_workWindows2.Count < idx)
            {
                Thread.Sleep(10);
                Trace.WriteLine($"WorkWindows.Count={_workWindows2.Count}");
            }
            if (_workWindows2.Count >= maxWorkWindowCount)
                return;

            //[3]
            Task.Run(new Action(() => { CreateWorkWindowAndAddToList(idx); }));
            idx++;
            while (_workWindows2.Count < idx)
            {
                Thread.Sleep(10);
                Trace.WriteLine($"WorkWindows.Count={_workWindows2.Count}");
            }
            if (_workWindows2.Count >= maxWorkWindowCount)
                return;

            //[4]
            Task.Run(new Action(() => { CreateWorkWindowAndAddToList(idx); }));
            idx++;
            while (_workWindows2.Count < idx)
            {
                Thread.Sleep(10);
                Trace.WriteLine($"WorkWindows.Count={_workWindows2.Count}");
            }
            if (_workWindows2.Count >= maxWorkWindowCount)
                return;


            ////Wait until previous thread has finished, _workWindows count has been added
            //while (_workWindows2.Count < 5)
            //{
            //    Thread.Sleep(10);
            //    Trace.WriteLine($"WorkWindows.Count={_workWindows2.Count}");
            //}
            Trace.WriteLine($"WorkWindows.Count={_workWindows2.Count}");
            /*
            try
            {
                //for (int i = 0; i < maxWorkWindowCount; i++)
                //{
                //    Thread thread = new Thread(() =>
                //    {
                //        //7 Create a new WorkWindow
                //        EAWorkWindow workWin = new EAWorkWindow(this);
                //        workWin.Show();

                //        _workWindows2.Add(workWin);
                //        System.Windows.Threading.Dispatcher.Run();
                //    });
                //    thread.SetApartmentState(ApartmentState.STA);
                //    thread.Start();

                //    //Wait until previous thread has finished, _workWindows count has been added
                //    while (_workWindows2.Count <= i)
                //    {
                //        Thread.Sleep(10);
                //    }
                //    thread.Join(200); //Wait until thread finished or timeout
                //}

                int thread1done = 0;

                Thread thread1 = new Thread(() =>
                {
                    //7 Create a new WorkWindow
                    EAWorkWindow workWin = new EAWorkWindow(this);
                    workWin.Show();

                    _workWindows2.Add(workWin);
                    Trace.WriteLine($"T1.WorkWindows.Count={_workWindows2.Count}");
                    thread1done++;
                    System.Windows.Threading.Dispatcher.Run();
                });
                thread1.SetApartmentState(ApartmentState.STA);
                thread1.IsBackground = true;
                thread1.Start();
                while (thread1done <= 0)
                {
                    Thread.Sleep(10);
                }

                int thread2done = 0;
                Thread thread2 = new Thread(() =>
                {
                    while (thread1done <= 0)
                    {
                        Thread.Sleep(10);
                    }

                    //7 Create a new WorkWindow
                    EAWorkWindow workWin = new EAWorkWindow(this);
                    workWin.Show();

                    _workWindows2.Add(workWin);
                    Trace.WriteLine($"T2.WorkWindows.Count={_workWindows2.Count}");
                    thread2done++;
                    System.Windows.Threading.Dispatcher.Run();
                });
                thread2.SetApartmentState(ApartmentState.STA);
                thread2.Start();
                while (thread2done <= 0)
                {
                    Thread.Sleep(10);
                }

                int thread3done = 0;
                Thread thread3 = new Thread(() =>
                {
                    //7 Create a new WorkWindow
                    EAWorkWindow workWin = new EAWorkWindow(this);
                    workWin.Show();

                    _workWindows2.Add(workWin);
                    Trace.WriteLine($"T3.WorkWindows.Count={_workWindows2.Count}");
                    thread3done++;
                    System.Windows.Threading.Dispatcher.Run();
                });
                thread3.SetApartmentState(ApartmentState.STA);
                thread3.Start();
                while (thread3done <= 0)
                {
                    Thread.Sleep(10);
                }

                int thread4done = 0;
                Thread thread4 = new Thread(() =>
                {
                    //7 Create a new WorkWindow
                    EAWorkWindow workWin = new EAWorkWindow(this);
                    workWin.Show();

                    _workWindows2.Add(workWin);
                    Trace.WriteLine($"T4.WorkWindows.Count={_workWindows2.Count}");
                    thread4done++;
                    System.Windows.Threading.Dispatcher.Run();
                });
                thread4.SetApartmentState(ApartmentState.STA);
                thread4.Start();
                while (thread4done <= 0)
                {
                    Thread.Sleep(10);
                }

                int thread5done = 0;
                Thread thread5 = new Thread(() =>
                {
                    //7 Create a new WorkWindow
                    EAWorkWindow workWin = new EAWorkWindow(this);
                    workWin.Show();

                    _workWindows2.Add(workWin);
                    Trace.WriteLine($"T5.WorkWindows.Count={_workWindows2.Count}");
                    thread5done++;
                    System.Windows.Threading.Dispatcher.Run();
                });
                thread5.SetApartmentState(ApartmentState.STA);
                thread5.Start();
                while (thread5done <= 0)
                {
                    Thread.Sleep(10);
                }

                //Wait until previous thread has finished, _workWindows count has been added
                while (_workWindows2.Count < 5)
                {
                    Thread.Sleep(10);
                    Trace.WriteLine($"WorkWindows.Count={_workWindows2.Count}");
                }
                Trace.WriteLine($"WorkWindows.Count={_workWindows2.Count}");


            }
            catch (Exception ex1)
            {
                LogInfo("@ ArrangeVM.CreateWorkWindows2() EXCEPTION, Message: " + ex1.Message);
                return;
            }
            */
        }

        private Task CreateWorkWindowAndAddToList(int idx)
        {
            Thread thread = new Thread(() =>
            {
                LogInfo($"Before new EAWorkWindow({idx})");
                EAWorkWindow workWin = new EAWorkWindow(this);
                LogInfo($"After new EAWorkWindow({idx})");
                workWin.Show();

                _workWindows2.Add(workWin);

                System.Windows.Threading.Dispatcher.Run();
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.IsBackground = true;
            thread.Start();

            return Task.CompletedTask;
        }

        private EAWorkWindow workWin0;
        private EAWorkWindow workWin1;
        private EAWorkWindow workWin2;
        private EAWorkWindow workWin3;
        private EAWorkWindow workWin4;
        /// <summary>
        /// Called from STA Thread
        /// </summary>
        public void STA_CreateWorkWindowsAddToList()
        {
            LogInfo($"Before new EAWorkWindow(0)");
            workWin0 = new EAWorkWindow(this);
            LogInfo($"After new EAWorkWindow(0)");
            workWin0.Show();
            AddWorkWindow(workWin0);

            LogInfo($"Before new EAWorkWindow(1)");
            workWin1 = new EAWorkWindow(this);
            LogInfo($"After new EAWorkWindow(1)");
            workWin1.Show();
            AddWorkWindow(workWin1);

            LogInfo($"Before new EAWorkWindow(2)");
            workWin2 = new EAWorkWindow(this);
            LogInfo($"After new EAWorkWindow(2)");
            workWin2.Show();
            AddWorkWindow(workWin2);

            LogInfo($"Before new EAWorkWindow(3)");
            EAWorkWindow workWin3 = new EAWorkWindow(this);
            LogInfo($"After new EAWorkWindow(3)");
            workWin3.Show();
            AddWorkWindow(workWin3);

            LogInfo($"Before new EAWorkWindow(4)");
            EAWorkWindow workWin4 = new EAWorkWindow(this);
            LogInfo($"After new EAWorkWindow(4)");
            workWin4.Show();
            AddWorkWindow(workWin4);
        }

        private EAWorkWindow _eaWin;
        public void STA_CreateAndAddWorkWindowToList()
        {
            if (WorkWindowCount >= maxWorkWindowCount)
                return;
            _eaWin = new EAWorkWindow(this);
            _eaWin.Show();
            AddWorkWindow(_eaWin);

        }

        public void ResetWorkWindows2()
        {
            foreach (EAWorkWindow workWin in _workWindows2)
            {
                if (workWin != null)
                {
                    if (workWin.IsUsed)
                    {
                        //Robert_Lin, 2024-10-3 don't close the window, we need to reused it
                        //workWin.DispatcherClose();
                        workWin.IsUsed = false;
                    }
                }
            }
        }

        private EAWorkWindow? GetUnusedWorkWindow()
        {
            foreach (EAWorkWindow workWin in _workWindows2)
            {
                if (workWin != null)
                    if (!workWin.IsUsed)
                        return workWin;
            }
            return null;
        }


        public int RefreshWorkWindows2()
        {
            bool isSupportNonDellMonitors = false;

            LogInfo("@ ArrangeVM.RefreshWorkWindows2()");

            //1 Get all supported monitors from DeviceManager
            //
            List<MonitorInfo>? monitors = GetMonitors();
            if (monitors == null)
            {
                LogInfo($"  * Monitors is null.");
                if (!isSupportNonDellMonitors)
                    return -1;
            }
            else
            {
                if (monitors.Count <= 0)
                {
                    LogInfo($"  * Monitors is empty.");
                    if (!isSupportNonDellMonitors)
                        return -1;
                }
                LogInfo($"  * Monitors.Count={monitors.Count}");
            }

            //2 Prepare to refresh WorkWindows
            //
            RefreshScreenScale();
            LogInfo($"  * ScreenScale={ScreenScale}");

            ResetWorkWindows2();

            int usedCount = 0;

            //Rebuild WorkWindows in a temp list
            Dictionary<string, EAWorkWindow> tempWorkWindows = new Dictionary<string, EAWorkWindow>();

            //Refresh with new AllScreens
            LogInfo($"  * Refreshing WorkWindows... AllScreens.Count={System.Windows.Forms.Screen.AllScreens.Length}");
            int idxScr = 0;
            int addCount = 0;
            foreach (Screen scr in System.Windows.Forms.Screen.AllScreens)
            {
                //Catch all variables to local to prevent overwrite in multi-thread environemnt
                double left = scr.WorkingArea.Left / ScreenScale;
                double top = scr.WorkingArea.Top / ScreenScale;
                double width = scr.WorkingArea.Width / ScreenScale;
                double height = scr.WorkingArea.Height / ScreenScale;
                bool isVertical = (width < height);
                LogInfo($"    - Screen[{idxScr}] {scr.DeviceName}   IsPrimary={scr.Primary}");
                LogInfo($"      WorkingArea: ({left},{top}){width}x{height}");

                //3 Find all monitors which have the same DeviceName (DisplayName)
                LogInfo($"  * Find attached monitor of current screen[{idxScr}]:");
                List<MonitorInfo> attachedMonitors = monitors.FindAll(x => x.DisplayName.Equals(scr.DeviceName, StringComparison.OrdinalIgnoreCase));

                //4 If there is no any Dell Monitor attached on this Screen, then do not need to create a
                // Workwindow for it
                if ((attachedMonitors == null) || (attachedMonitors.Count <= 0))
                {
                    LogInfo($"    - No attached Monitor for this screen[{idxScr}] => No WorkWindow to create for it.");
                    idxScr++;
                    if (!isSupportNonDellMonitors) 
                        continue;
                }
                else
                {
                    //Dump attached monitors
                    LogInfo($"    - Dump AttachedMonitors for screen[{idxScr}]");
                    int idxMonitor = 0;
                    foreach (MonitorInfo mi in attachedMonitors)
                    {
                        LogInfo($"        [{mi.Index}] Name=[{mi.AliasDeviceName}], Model=[{mi.modelName}], ServiceTag=[{mi.edid.ServiceTag}], MarketName=[{mi.MarketingName}]");
                        idxMonitor++;
                    }
                }
                //5 Select the first monitor to read its settings
                MonitorInfo? miWork = null;
                if ((attachedMonitors != null) && (attachedMonitors.Count > 0)) 
                {
                    miWork = attachedMonitors[0];
                }

                //6 Find an available WorkWindow work for it
                //
                EAWorkWindow? workWin = GetUnusedWorkWindow();
                if (workWin == null)
                {
                    LogInfo($"  * Fail to  GetUnusedWorkWindow for Screen[{idxScr}]");
                    continue;
                }
                else
                {

                    workWin.IsUsed = true;
                    workWin.SetScreen(scr);
                    if (miWork != null) 
                        workWin.AttachedMonitor = miWork;
                    usedCount++;
                }

                //10 Read settings for the target monitor
                EAMonitorSettings? eaSettings = null;
                if (miWork != null)
                {
                    LogInfo($"  * ReadEAMonitorSettings({miWork.modelName}/{miWork.edid.ServiceTag})");
                    eaSettings = ReadEAMonitorSettings(miWork);
                }
                if (eaSettings == null)
                {
                    //Should be never to here
                    if (isSupportNonDellMonitors)
                    {
                        //int cellCount = 2;
                        //char splitKey = 'C';
                        //List<double> settings = new List<double>() { 7, 3 };
                        //int cellCount = 2;
                        //char splitKey = 'A';
                        //List<double> settings = new List<double>() { 1, 1 };
                        int cellCount = 4;
                        char splitKey = 'A';
                        List<double> settings = new List<double>() { 1, 1, 1, 1, 1 };
                        workWin.SetWorkingSplit(cellCount, splitKey, settings);
                    }
                }
                else
                {
                    //11 Apply settings to workWin
                    int cellCount = eaSettings.SelectedSplit.CellCount;
                    char splitKey = eaSettings.SelectedSplit.SplitKey;
                    List<double> settings = eaSettings.SelectedSplit.Settings;

                    //Debug, force using non-default layout
                    //cellCount = 4;
                    //splitKey = 'A';
                    workWin.SetWorkingSplit(cellCount, splitKey, settings);

                    LogInfo($"  * SetWorkSplit: {eaSettings.SelectedSplit.ToString()}");

                }

                workWin.ChangeWindowPos(left, top, width, height);

                idxScr++;
            } //foreach(Screen scr)

            _workWindowUsedCount = usedCount;
            OnPropertyChanged("WorkWindowCount");
            return usedCount;
        }

        public int RefreshWorkWindowUsedCount()
        {
            int usedCount = 0;
            foreach (EAWorkWindow workWin in _workWindows2)
            {
                if (workWin.IsUsed)
                    usedCount++;
            }
            _workWindowUsedCount = usedCount;
            OnPropertyChanged("WorkWindowCount");
            return _workWindowUsedCount;
        }
        public EAWorkWindow? FindWorkWindowByDisplayName2(string displayName)
        {
            if (_workWindows2 == null)
                return null;

            Trace.WriteLine($"WorkWindows Count={_workWindows2.Count}");
            foreach (EAWorkWindow workWin in _workWindows2)
            {
                if (workWin.IsUsed)
                {
                    if (!String.IsNullOrWhiteSpace(workWin.ScreenDeviceName))
                    {
                        if (workWin.ScreenDeviceName.Equals(displayName))
                            return workWin;
                    }
                }
            }
            return null;
        }
        #endregion WorkWindowList

        #region WorkWindows

        //private Dictionary<string, EAWorkWindow> _workWindows_Unused = new Dictionary<string, EAWorkWindow>();
        private ObservableCollection<string> _workWinCellInfos = new ObservableCollection<string>();


        //public Dictionary<string, EAWorkWindow> WorkWindows_Unused
        //{
        //    get => _workWindows;
        //    set
        //    {
        //        SetProperty(ref _workWindows, value);
        //        OnPropertyChanged("WorkWindowCount");
        //    }
        //}

        //public void AddWorkWindow(string key, EAWorkWindow workWin)
        //{
        //    _workWindows.Add(key, workWin);

        //    OnPropertyChanged("WorkWindows");
        //    OnPropertyChanged("WorkWindowCount");
        //    //RefreshWorkWinInfos();
        //}

        public int WorkWindowCount
        {
            get
            {
                //return WorkWindows.Count; 
                return _workWindowUsedCount;
            }
        }

        //public void ClearWorkWindows()
        //{
        //    foreach (KeyValuePair<string, EAWorkWindow> keyValuePair in _workWindows)
        //    {
        //        if (keyValuePair.Value != null)
        //            keyValuePair.Value.DispatcherClose();
        //    }
        //    _workWindows.Clear();
        //    RefreshWorkWinInfos();
        //}

        //public void RemoveWorkWindow(string key)
        //{
        //    EAWorkWindow workWindow;
        //    if (WorkWindows.TryGetValue(key, out workWindow))
        //    {
        //        workWindow.DispatcherClose();
        //        WorkWindows.Remove(key);
        //        RefreshWorkWinInfos();
        //    }
        //}

        public ObservableCollection<string> WorkWinCellInfos
        {
            get => _workWinCellInfos;
            set
            {
                SetProperty(ref _workWinCellInfos, value);
            }
        }

        public void RefreshWorkWinInfos()
        {
            lock (_lockObject)
            {
                ObservableCollection<string> newInfo = new ObservableCollection<string>();
                //foreach (KeyValuePair<string, EAWorkWindow> kv in _workWindows)
                //{
                //    EAWorkWindow workWin = kv.Value as EAWorkWindow;
                //    if (workWin != null)
                //    {
                //        string workInfo = $"{workWin.WindowName}={workWin.CellListJson}";
                //        newInfo.Add(workInfo);
                //    }
                //}
                foreach (EAWorkWindow workWin in _workWindows2)
                {
                    if (workWin == null)
                        continue;
                    if (!workWin.IsUsed)
                        continue;

                    string workInfo = $"{workWin.WindowName}={workWin.CellListJson}";
                    newInfo.Add(workInfo);
                }
                WorkWinCellInfos = newInfo;
            }
        }

        public void RefreshCellRects()
        {
            if (IsAwsWindowVisible)
            {
                if (AwsWindow != null)
                {
                    AwsWindow.RefreshCellRects();
                }
            }
            //foreach (KeyValuePair<string, EAWorkWindow> keyValuePair in _workWindows)
            //{
            //    EAWorkWindow workWin = keyValuePair.Value;
            //    if (workWin != null) 
            //        workWin.Invoke_RefreshCellRects();
            //}
            foreach (EAWorkWindow workWin in _workWindows2)
            {
                if (workWin == null)
                    continue;
                if (!workWin.IsUsed)
                    continue;
                workWin.RefreshCellRects();
            }
            RefreshWorkWinInfos();
        }

        //Find the WorkWindow in WorkWindows by DisplayName, for exmaple "\\.\DISPLAY1"
        //public EAWorkWindow? FindWorkWindowByDisplayName(string displayName)
        //{
        //    if (_workWinCellInfos == null)
        //        return null;

        //    EAWorkWindow workWindow = null;
        //    if (_workWindows.TryGetValue(displayName, out workWindow))
        //    {
        //        return workWindow;
        //    }
        //    return null;
        //}

        /// <summary>
        /// Add/Build EAWorkWindows for each supported Monitors, with their settings, add into WorkWindows.
        /// </summary>
        /// <returns>>0 : number of WorkWindows are added.</returns>
        //public int InitWorkWindows()
        //{
        //    LogInfo("@ ArrangeVM.InitWorkWindows()");

        //    //1 Get all supported monitors from DeviceManager
        //    //
        //    List<MonitorInfo>? monitors = GetMonitors();
        //    if (monitors == null)
        //    {
        //        LogInfo($"  * Monitors is null.");
        //        return -1;
        //    }
        //    if (monitors.Count <= 0)
        //    {
        //        LogInfo($"  * Monitors is empty.");
        //        return -1;
        //    }
        //    LogInfo($"  * Monitors.Count={monitors.Count}");

        //    //2 Prepare to build WorkWindows
        //    //
        //    RefreshScreenScale();
        //    var dpiXProperty = typeof(SystemParameters).GetProperty("DpiX", BindingFlags.NonPublic | BindingFlags.Static);
        //    var varX = (int)dpiXProperty.GetValue(null, null);
        //    double dpiX = (double)varX / (double)96;
        //    LogInfo($"  * ScreenScale={ScreenScale}");

        //    ClearWorkWindows();

        //    //Rebuild WorkWindows in a temp list
        //    Dictionary<string, EAWorkWindow> tempWorkWindows = new Dictionary<string, EAWorkWindow>();

        //    //Refresh with new AllScreens
        //    LogInfo($"  * Refreshing WorkWindows... AllScreens.Count={System.Windows.Forms.Screen.AllScreens.Length}");
        //    int idxScr = 0;
        //    int addCount = 0;
        //    foreach (Screen scr in System.Windows.Forms.Screen.AllScreens)
        //    {
        //        //Catch all variables to local to prevent overwrite in multi-thread environemnt
        //        double left = scr.WorkingArea.Left / (double)dpiX;
        //        double top = scr.WorkingArea.Top / (double)dpiX;
        //        double width = scr.WorkingArea.Width / (double)dpiX;
        //        double height = scr.WorkingArea.Height / (double)dpiX;
        //        bool isVertical = (width < height);
        //        LogInfo($"    - Screen[{idxScr}] {scr.DeviceName}   IsPrimary={scr.Primary}");
        //        LogInfo($"      WorkingArea: ({left},{top}){width}x{height}");

        //        //3 Find all monitors which have the same DeviceName (DisplayName)
        //        LogInfo($"  * Find attached monitor of current screen[{idxScr}]:");
        //        List<MonitorInfo> attachedMonitors = monitors.FindAll(x => x.DisplayName.Equals(scr.DeviceName, StringComparison.OrdinalIgnoreCase));

        //        //4 If there is no any Dell Monitor attached on this Screen, then do not need to create a
        //        // Workwindow for it
        //        if ((attachedMonitors == null) || (attachedMonitors.Count <= 0))
        //        {
        //            LogInfo($"    - No attached Monitor for this screen[{idxScr}] => No WorkWindow to create for it.");
        //            idxScr++;
        //            continue;
        //        }

        //        //Dump attached monitors
        //        LogInfo($"    - Dump AttachedMonitors for screen[{idxScr}]");
        //        int idxMonitor = 0;
        //        foreach (MonitorInfo mi in attachedMonitors)
        //        {
        //            LogInfo($"        [{mi.Index}] Name=[{mi.AliasDeviceName}], Model=[{mi.modelName}], ServiceTag=[{mi.edid.ServiceTag}], MarketName=[{mi.MarketingName}]");
        //            idxMonitor++;
        //        }
        //        //5 Select the first monitor to read its settings
        //        MonitorInfo miWork = attachedMonitors[0];
        //        //_deviceManagerPlugin.ShowOSD(miWork, OSDType.DisplayChanged);

        //        //6 Create a WorkWindow work for it
        //        //
        //        Thread thread = new Thread(() =>
        //        {
        //            //7 Create a new WorkWindow
        //            EAWorkWindow workWin = new EAWorkWindow(this, scr, attachedMonitors);

        //            //8 Move workWin to fit the screen
        //            workWin.Left = left;
        //            workWin.Top = top;
        //            workWin.Width = width;
        //            workWin.Height = height;
        //            workWin.IsVertical = isVertical;

        //            //9 Show to workWin

        //            workWin.Show();
        //            //10 Read settings for the target monitor
        //            LogInfo($"  * ReadEAMonitorSettings({miWork.modelName}/{miWork.edid.ServiceTag})");
        //            EAMonitorSettings? eaSettings = ReadEAMonitorSettings(miWork);
        //            if (eaSettings == null)
        //            {
        //                //Should be never to here
        //            }
        //            else
        //            {
        //                //11 Apply settings to workWin
        //                int cellCount = eaSettings.SelectedSplit.CellCount;
        //                char splitKey = eaSettings.SelectedSplit.SplitKey;
        //                List<double> settings = eaSettings.SelectedSplit.Settings;
        //                LogInfo($"  * SetWorkSplit: {eaSettings.SelectedSplit.ToString()}");
        //                workWin.SetWorkingSplit(cellCount, splitKey, settings);
        //            }
        //            tempWorkWindows.Add(scr.DeviceName, workWin);
        //            LogInfo($"  * Add WorkWindow for [{idxScr}]{scr.DeviceName}.");

        //            System.Windows.Threading.Dispatcher.Run();
        //        });

        //        addCount++;
        //        thread.SetApartmentState(ApartmentState.STA);
        //        thread.Start();
        //        //thread.Join(2000); //Wait until thread finished
        //        idxScr++;
        //    } //foreach(Screen scr)

        //    //Wait for all WorkWindows are added into tempWorkWindows
        //    while (tempWorkWindows.Count < addCount)
        //    {
        //        Thread.Sleep(10);
        //    }
        //    WorkWindows = tempWorkWindows;
        //    return WorkWindowCount;
        //}
        #endregion WorkWindows

        #region Foreground Window Info

        private IntPtr _hWndForeground = IntPtr.Zero;

        public IntPtr hWndForeground
        {
            get => _hWndForeground;
            set => SetProperty(ref _hWndForeground, value);
        }

        private string _pathNameForeground = "";

        public string PathNameForeground
        {
            get => _pathNameForeground;
            set => SetProperty(ref _pathNameForeground, value);
        }

        private string _startMovingMsg = "";

        public string StartMovingMsg
        {
            get => _startMovingMsg;
            set => SetProperty(ref _startMovingMsg, value);
        }

         public bool IsAllowToMoveFromPathName(string pathName)
        {
            string fileName = System.IO.Path.GetFileName(pathName);
            if (fileName.Equals("DDPM.Subagent.User.exe", StringComparison.OrdinalIgnoreCase))
                return false;
            if (fileName.Equals("DDPM.exe", StringComparison.OrdinalIgnoreCase))
                return false;

            return true;
        }

        #endregion Foreground Window Info

        #region DCF Features

        public ILog Log { get; set; }

        public void LogInfo(string message)
        {
            if (Log != null)
            {
                string logDatetime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff");
                Log.Info($"[{logDatetime}] {message}");
            }
        }

        #endregion DCF Features

        #region Helper Functions

        public static string FormatRect(System.Windows.Rect rc)
        {
            return $"({rc.Left},{rc.Top})-({rc.Right},{rc.Bottom}){rc.Width}x{rc.Height}";
        }

        //
        /// <summary>
        /// Get the Rect of the specific FrameworkElement.
        /// It will reference ScreenScale property to fix the coordinate.
        /// </summary>
        /// <param name="ele"></param>
        /// <returns>Return the Rect based on Screen coordinate</returns>
        public Rect GetFrameworkElementRect(FrameworkElement ele)
        {
            if (ele == null)
                return Rect.Empty;

            if ((ele.ActualWidth == 0) && (ele.ActualHeight == 0))
                return Rect.Empty;

            PresentationSource preSrc = PresentationSource.FromVisual(ele);
            if (preSrc == null)
                return Rect.Empty;

            System.Windows.Point ptTopLeft = ele.PointToScreen(new System.Windows.Point(0, 0));
            double w = ele.ActualWidth * ScreenScale;
            double h = ele.ActualHeight * ScreenScale;
            //Trace.WriteLine($"ctrlActual={ele.ActualWidth}x{ele.ActualHeight}; Scale={_vm.ScreenScale} => {w}x{h}");
            return new Rect(ptTopLeft.X, ptTopLeft.Y, w, h);
        }

        public static ISplitCtrl? SplitCtrlFromSplitJson(SplitJson spJson, eSplitModes splitMode)
        {
            ISplitCtrl? splitCtrl = ISplitCtrl.Create(spJson.CellCount, spJson.SplitKey);
            if (splitCtrl == null)
                return null;
            if (spJson.Settings != null)
            {
                splitCtrl.Settings = new List<double>(spJson.Settings);
            }
            splitCtrl.FriendlyName = spJson.CustomName;

            splitCtrl.SplitMode = splitMode;
            if ((spJson.CellCount == 0) && (spJson.SplitKey == 'B'))
            {
                if (splitMode == eSplitModes.AWS)
                {
                    splitMode = eSplitModes.Work;
                }
            }
           return splitCtrl;
        }
        #endregion Helper Functions

        #region DDPM.SA Interfaces
        public IDeviceManagerSA? DeviceManager
        {
            get => _deviceManagerPlugin;
            set => _deviceManagerPlugin = value;
        }

        public IDisplayService? DisplayManager
        {
            get => _displayManagerPlugin;
            set => _displayManagerPlugin = value;
        }

        public EAMonitorSettings? ReadEAMonitorSettings(MonitorInfo mi)
        {
            if (_deviceManagerPlugin == null) return null;
            return DeviceManager?.ReadEAMonitorSettings(mi).Result;
        }

        public bool WriteEAMonitorSettings(MonitorInfo mi, EAMonitorSettings eaSettings)
        {
            if (_deviceManagerPlugin == null) return false;
            return _deviceManagerPlugin.WriteEAMonitorSettings(mi, eaSettings).Result;
        }
        public List<MonitorInfo>? GetMonitors()
        {
            if (_displayManagerPlugin == null)
                return null;

            return _displayManagerPlugin.GetMonitors().Result;
        }

        public List<MonitorInfo>? GetMonitorsFromDisplayName(string displayName)
        {
            List<MonitorInfo>? monitors = GetMonitors();
            if (monitors == null) return null;

            //Find all monitors which have the same DeviceName (DisplayName)
            List<MonitorInfo> attachedMonitors = monitors.FindAll(x => x.DisplayName.Equals(displayName, StringComparison.OrdinalIgnoreCase));
            return attachedMonitors;
        }
        #endregion DDPM.SA Interfaces

        #region WorkWindow FadeOut

        //private bool _isFading = false;
        //public bool IsFading
        //{
        //    get => _isFading;
        //    set => SetProperty(ref _isFading, value);
        //}

        #endregion WorkWindow FadeOut

        #region AWS Window
        //private bool _isAwsEnabled = EzSettings.IsAwsEnabled;

        //AWS Window width x height
        //public const double cxAws = 788.000;
        //public const double cyAws = 134.000;
        //The gap between AWS window bottom to cursor when AWS showing up
        public const double dyAwsShow = 96;

        //AWS Window position
        private double _xAws = 0;
        private double _yAws = 0;

        //The last Visibility state of AwsWindow
        private bool _isAwsWindowVisible = false;

        public bool IsAwsEnabled
        {
            get
            {
                if (EzSettings != null)
                    return EzSettings.IsAwsEnabled;
                return false;
            }
            //set
            //{
            //    SetProperty(ref _isAwsEnabled, value);
            //    OnPropertyChanged("IsAwsWindowVisible");
            //}
        }
        public bool IsAwsWindowVisible
        {
            get
            {
                bool newValue = _isAwsWindowVisible;

                if (!IsMoving)
                {
                    newValue = false;
                }
                else
                {
                    if (!IsAwsEnabled)
                        newValue = false;
                    else
                    {
                        if (EzSettings.IsOnlyAllowWhenShiftKeyPressed)
                        {
                            newValue = IsShiftPressed;
                        }
                        else
                        {
                            newValue = true;
                        }
                    }
                }
                if (newValue != _isAwsWindowVisible)
                {
                    _isAwsWindowVisible = newValue;
                    if (AwsWindowVisibilityChanged != null)
                    {
                        Task.Run(() => AwsWindowVisibilityChanged.Invoke(this, newValue));
                    }
                }
                return _isAwsWindowVisible;
            }
        }

        public AwsWindow AwsWindow { get; set; }

        /// <summary>
        /// Return the Screen from current mouse cursor position
        /// Need to update ArrangeVM.xCursor and yCursor at first
        /// </summary>
        /// <returns></returns>
        public Screen? GetScreenFromCursor()
        {
            foreach (System.Windows.Forms.Screen scr in Screen.AllScreens)
            {
                if (scr.Bounds.Contains(xCursor, yCursor))
                {
                    return scr;
                }
            }
            return null;
        }

        public double cxAws { get => 788.000; }
        public double cyAws { get => 134.000; }

        public double xAws
        {
            get => _xAws;
            set => SetProperty(ref _xAws, value);
        }
        public double yAws
        {
            get => _yAws;
            set => SetProperty(ref _yAws, value);
        }

        #endregion

        #region AWS Icons

        private ISplitCtrl _awsIcon1;
        private ISplitCtrl _awsIcon2;
        private ISplitCtrl _awsIcon3;
        private ISplitCtrl _awsIcon4;

        public ISplitCtrl AwsIcon1
        {
            get => _awsIcon1;
            set => SetProperty(ref _awsIcon1, value);
        }
        public ISplitCtrl AwsIcon2
        {
            get => _awsIcon2;
            set => SetProperty(ref _awsIcon2, value);
        }
        public ISplitCtrl AwsIcon3
        {
            get => _awsIcon3;
            set => SetProperty(ref _awsIcon3, value);
        }
        public ISplitCtrl AwsIcon4
        {
            get => _awsIcon4;
            set => SetProperty(ref _awsIcon4, value);
        }

        public void RefreshAwsWindowIcons()
        {

        }
        #endregion AWS Icons


        #region Screen Manager
        private List<EAScreen> _EAScreens = new List<EAScreen>();

        /// <summary>
        /// Rebuild the Screen list from Forms.Screen.AllScreens, and find their attached MonitorInfos from DisplayManager
        /// </summary>
        public void RefreshEAScreens()
        {
            if (_displayManagerPlugin == null)
                return;

            lock (_lockScreenMgr)
            {
                List<MonitorInfo>? dellMonitors = GetMonitors();
                List<EAScreen> tempScreens = new List<EAScreen>();

                foreach (Screen scr in System.Windows.Forms.Screen.AllScreens)
                {
                    List<MonitorInfo> attachedMonitors = dellMonitors.FindAll(x => x.DisplayName.Equals(scr.DeviceName, StringComparison.OrdinalIgnoreCase));
                    EAScreen eaScr = new EAScreen(scr, attachedMonitors);
                    tempScreens.Add(eaScr);
                } //foreach Screen
                _EAScreens.Clear();
                _EAScreens = tempScreens;
            }
        }

        public List<MonitorInfo>? GetMonitorsFromDeviceName(string deviceName)
        {
            List<MonitorInfo>? allMonitors = GetMonitors();
            if (allMonitors == null)
            {
                Trace.WriteLine($"@ GetMonitorsFromDeviceName({deviceName}): GetMonitors() return null");
                return null;
            }
            Trace.WriteLine($"@ GetMonitorsFromDeviceName({deviceName}): Monitors.Count={allMonitors.Count}");
            return allMonitors.FindAll(x => x.DisplayName.Equals(deviceName, StringComparison.OrdinalIgnoreCase));
        }
        #endregion
        public void DetermineWorkWindowVisibility()
        {
            //foreach (EAWorkWindow workWin in _workWindows2)
            //{
            //    if (workWin.IsUsed)
            //    {
            //        workWin.DetermineWindowVisibility();
            //    }
            //}
        }

        #region EzSettings
        //EzSettings should be updated with assign a new object, for example
        //  (ArrangeVM) vm.EzSettings = new EzSettings() { xxx=xxxx, ...}
        private EzSettings _ezSettings = new EzSettings() { /*IsOnlyAllowWhenShiftKeyPressed = false*/ };
        public EzSettings EzSettings 
        {
            get => _ezSettings;
            set
            {
                SetProperty(ref _ezSettings, value);
                OnPropertyChanged("IsOnlyShift");
                OnPropertyChanged("IsAwsEnabled");
                OnPropertyChanged("IsAwsWindowVisible");
            }
        }

        public bool IsOnlyShift
        {
            get { return EzSettings.IsOnlyAllowWhenShiftKeyPressed; }
        }
        #endregion

        //public bool ReloadMonitorSettings(MonitorInfo monitorInfo)
        //{
        //    foreach (EAWorkWindow workWin in _workWindows2)
        //    {
        //        if (workWin.IsUsed)
        //        {
        //            if (workWin.ScreenDeviceName.Equals(monitorInfo.DisplayName))
        //                return workWin.ReloadMonitorSettings();
        //        }
        //    }
        //    return false;
        //}


        /// <summary>
        /// Check if the specify pathName is exclued by EasyArrange and EasyMemory
        /// </summary>
        /// <param name="pathName"></param>
        /// <returns></returns>
        public static bool IsEAExcludedPathName(string pathName)
        {
            if (String.IsNullOrWhiteSpace(pathName)) 
                return true;
            string fileName = System.IO.Path.GetFileName(pathName);
            if (fileName.Equals("DDPM.Subagent.User.exe", StringComparison.OrdinalIgnoreCase))
                return true;
            if (fileName.Equals("DDPM.exe", StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        }

        public void TraceSplitJsonList(List<SplitJson> splitJsonList, int maxCount = 5)
        {
            int idx = 0;
            foreach (SplitJson splitJson in splitJsonList)
            {
                Trace.WriteLine($"[{idx}] {splitJson.ToString()}");
                idx++;
            }
        }
 
    }
}