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
using System.Windows;
using System.Windows.Media.Media3D;
using VcpCore.Common;
using static System.Net.Mime.MediaTypeNames;

namespace DDPM.SA.Plugins.User.EasyArrange
{
    public class ArrangeVM : ObservableObject
    {
        private readonly object _lockObject = new();
        private IDisplayService? _displayManagerPlugin;
        private IDeviceManagerSA? _deviceManagerPlugin;

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
        private bool _isMoving = false;

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
                    LogInfo($"@ ArrangeVM.IsWorkUIShowing: IsShiftPressed={IsShiftPressed}");
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
            }
        }

        public event EventHandler<bool> IsMovingChanged;

        #endregion Option flags

        #region Cursor position

        /// <summary>
        /// Cursor position (xCursor, yCursor) will be updated by (OnLocationChanged handler).
        /// and then use it to determine if the custor is inside a CellBorder.
        /// </summary>
        //xCursor
        private int _xCursor = 0;

        public int xCursor
        {
            get { return _xCursor; }
            set
            {
                _xCursor = value;
                OnPropertyChanged("xCursor");
            }
        }

        //yCursor
        private int _yCursor = 0;

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

        #region Hovering Cell

        private CellObj? _hoveringCellObj = null;

        /// <summary>
        /// This property will be refreshed by OnLocationChanged handler.
        /// When moving stop, will use it as the target Cell to move the target window into this cell rect.
        /// </summary>
        public CellObj? HoveringCellObj
        {
            get => _hoveringCellObj;
            set => SetProperty(ref _hoveringCellObj, value);
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
            //foreach (KeyValuePair<string, EAWorkWindow> keyValuePair in _workWindows)
            //{
            //    EAWorkWindow workWin = keyValuePair.Value;
            //    if (workWin == null) continue;

            //    CellObj? cellObj = workWin.DetermineHoveringCellObj(x, y);
            //    if (cellObj != null)
            //    {
            //        HoveringScreen = keyValuePair.Key;
            //        HoveringCellObj = cellObj;
            //        return cellObj;
            //    }
            //}
            
            foreach (EAWorkWindow workWin in _workWindows2)
            {
                if (!workWin.IsUsed)
                    continue;


                CellObj? cellObj = workWin.DetermineHoveringCellObj(x, y);
                if (cellObj != null)
                {
                    HoveringScreen = workWin.ScreenDeviceName;
                    HoveringCellObj = cellObj;
                    return cellObj;
                }
            }
            HoveringCellObj = null;
            return null;
        }

        #endregion Hovering Cell

        #region HoveringScreen

        private string _hoveringScreen = "";

        public string HoveringScreen
        {
            get => _hoveringScreen;
            set => SetProperty(ref _hoveringScreen, value);
        }

        #endregion HoveringScreen

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
                        workWin.DispatcherClose();
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
            LogInfo("@ ArrangeVM.RefreshWorkWindows2()");

            //1 Get all supported monitors from DeviceManager
            //
            List<MonitorInfo>? monitors = GetMonitors();
            if (monitors == null)
            {
                LogInfo($"  * Monitors is null.");
                return -1;
            }
            if (monitors.Count <= 0)
            {
                LogInfo($"  * Monitors is empty.");
                return -1;
            }
            LogInfo($"  * Monitors.Count={monitors.Count}");

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
                    continue;
                }

                //Dump attached monitors
                LogInfo($"    - Dump AttachedMonitors for screen[{idxScr}]");
                int idxMonitor = 0;
                foreach (MonitorInfo mi in attachedMonitors)
                {
                    LogInfo($"        [{mi.Index}] Name=[{mi.AliasDeviceName}], Model=[{mi.modelName}], ServiceTag=[{mi.edid.ServiceTag}], MarketName=[{mi.MarketingName}]");
                    idxMonitor++;
                }
                //5 Select the first monitor to read its settings
                MonitorInfo miWork = attachedMonitors[0];

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
                    workWin.AttachedMonitor = miWork;
                    usedCount++;
                }

                //10 Read settings for the target monitor
                LogInfo($"  * ReadEAMonitorSettings({miWork.modelName}/{miWork.edid.ServiceTag})");
                EAMonitorSettings? eaSettings = ReadEAMonitorSettings(miWork);
                if (eaSettings == null)
                {
                    //Should be never to here
                }
                else
                {
                    //11 Apply settings to workWin
                    int cellCount = eaSettings.SelectedSplit.CellCount;
                    char splitKey = eaSettings.SelectedSplit.SplitKey;
                    List<double> settings = eaSettings.SelectedSplit.Settings;
                    LogInfo($"  * SetWorkSplit: {eaSettings.SelectedSplit.ToString()}");
                    workWin.SetWorkingSplit(cellCount, splitKey, settings);


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
                Log.Info(message);
            }
        }

        #endregion DCF Features

        #region Helper Functions

        public static string FormatRect(System.Windows.Rect rc)
        {
            return $"({rc.Left},{rc.Top})-({rc.Right},{rc.Bottom}){rc.Width}x{rc.Height}";
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
        private EzSettings _ezSettings = new EzSettings();
        public EzSettings EzSettings 
        {
            get => _ezSettings;
            set
            {
                SetProperty(ref _ezSettings, value);
                OnPropertyChanged("IsOnlyShift");
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


    }
}