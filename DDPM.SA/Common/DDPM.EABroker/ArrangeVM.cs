using CommunityToolkit.Mvvm.ComponentModel;
using DDPM.SA.Common;
using DDPM.SA.Common.Interfaces;
using DDPM.SA.Common.Settings;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using VcpCore.Common;
using DDPM.Easy.Common;
using DDPM.SA.Common.Display;
using DDPM.SA.Common.Settings;
using System.Diagnostics;
using System.Collections.ObjectModel;

namespace DDPM.EABroker
{
    public class ArrangeVM : ObservableObject
    {
        #region Private members
        //DCF Agent Related
        private IAgent? _agent = null;
        private ILog? _log = null;

        //DDPM.SA.Plugin.User interfaces
        private IDeviceManagerSA? _deviceManagerSA = null;
        private IDisplayService? _displayService = null;
        private IEasyArrangeService? _easyArrangeService = null;

        //Window Moving
        private IntPtr _hWndForeground = IntPtr.Zero;
        private string _pathNameForeground = "";
        private string _startMovingMsg = "";
        private bool _isMoving = false;

        //Moving Status - Cursor, Screen
        private int _xCursor = 0;
        private int _yCursor = 0;
        private double _screenScale = 1;
        private Screen? _workScreen = null;

        //EzSettings
        private bool _isOnlyShift = false; //EzSettings.IsOnlyAllowWhenShiftKeyPressed
        private bool _isAwsEnabled = true; //EzSettings.IsAwsEnabled
        private bool _isWithoutGap = true; //EzSettings.IsWidthoutGap
        private bool _isSpanMultiMonitors = false; //EzSettings.IsSpanAcrossMultiMonitors
        private bool _isShiftPressed = false;

        //Hovering
        private CellObj? _hoveringCellObj = null;

        //AWS Window
        private AwsWindow _awsWindow;
        //The last Visibility state of AwsWindow, used to detect when Visibility changed
        private bool _isAwsWindowVisible = true;
        //The last AwsWindow (left,top) position
        private double _xAwsWindow = 0;
        private double _yAwsWindow = 0;

        //AWS Icons
        private ISplitCtrl _awsIcon1;
        private ISplitCtrl _awsIcon2;
        private ISplitCtrl _awsIcon3;
        private ISplitCtrl _awsIcon4;

        private ISplitCtrl? _hoveringAwsIcon; //Point to one of {_awsIcon1 ~ _awsIcon4 }


        //WorkWindows
        private List<EAWorkWindow> _workWindows = new List<EAWorkWindow>();
        private int _workWindowUsedCount = 0;
        private bool _isWorkUIEnabled = true;
        private bool _isWorkWindowVisible = false;
        private ObservableCollection<string> _workWinCellInfos = new ObservableCollection<string>();
        #endregion Private members

        #region Constants
        public const int MaxWorkWindowCount = 5;
        //The gap between AWS window bottom to cursor when AWS showing up
        public const double dyAwsShow = 96;
        #endregion

        #region Events
        //Invoked,when (_isMoving==true) and cursor position cross screen boundary
        public EventHandler<Screen> WorkScreenChanged;

        //Invoked when AWS Window visibility changed
        public EventHandler<bool> AwsWindowVisibilityChanged;
        #endregion

        #region Init
        public void InitInterfaces(IAgent agent, ILog log, IDeviceManagerSA devMgr, IDisplayService dispMgr, IEasyArrangeService eaService)
        {
            _agent = agent;
            _log = log;
            _deviceManagerSA = devMgr;
            _displayService = dispMgr;
            _easyArrangeService = eaService;

        }
        #endregion

        #region DCF Agent Related
        public void WriteLog(string msg, Exception? e = null)
        {
            if (_log != null)
            {
                if (e == null)
                {
                    _log.Info(msg);
                }
                else
                {
                    _log.Error(e, msg);
                }
            }
        }
        #endregion

        #region DDPM.SA Functions
        public List<MonitorInfo>? GetMonitors()
        {
            if (_deviceManagerSA == null)
            {
                WriteLog("@ArrangeVM.GetMonitors(), _deviceManagerSA is null.");
                return null;
            }
            return _deviceManagerSA.GetMonitors().Result;
        }
        public List<MonitorInfo>? GetMonitorsFromDeviceName(string deviceName)
        {
            List<MonitorInfo>? monitors = GetMonitors();
            if (monitors == null) 
                return null;

            //Find all monitors which have the same DeviceName (DisplayName)
            List<MonitorInfo> attachedMonitors = monitors.FindAll(x => x.DisplayName.Equals(deviceName, StringComparison.OrdinalIgnoreCase));
            return attachedMonitors;
        }

        public EAMonitorSettings? ReadEAMonitorSettings(MonitorInfo mi)
        {
            if (_deviceManagerSA == null)
            {
                WriteLog("@ArrangeVM.ReadEAMonitorSettings(), _deviceManagerSA is null.");
                return null;
            }
            return _deviceManagerSA?.ReadEAMonitorSettings(mi).Result;
        }

        public bool WriteEAMonitorSettings(MonitorInfo mi, EAMonitorSettings eaSettings)
        {
            if (_deviceManagerSA == null)
            {
                WriteLog("@ArrangeVM.WriteEAMonitorSettings(), _deviceManagerSA is null.");
                return false;
            }
            return _deviceManagerSA.WriteEAMonitorSettings(mi, eaSettings).Result;
        }

         #endregion

        #region Foreground Window Info
        public IntPtr hWndForeground
        {
            get => _hWndForeground;
            set => SetProperty(ref _hWndForeground, value);
        }
        public string PathNameForeground
        {
            get => _pathNameForeground;
            set => SetProperty(ref _pathNameForeground, value);
        }
        public string StartMovingMsg
        {
            get => _startMovingMsg;
            set => SetProperty(ref _startMovingMsg, value);
        }
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
        #endregion Foreground Window Info

        #region Window Moving
        public bool IsMoving
        {
            get => _isMoving;
            set
            {
                SetProperty(ref _isMoving, value);
                OnPropertyChanged("IsAwsWindowVisible");
                OnPropertyChanged("IsWorkWindowVisible");
            }
        }
        #endregion

        #region Moving Status - Cursor, Screen
        //Cursor position, will be updated since EABriket.Start()
        public int xCursor
        {
            get => _xCursor;
            set => SetProperty(ref _xCursor, value);
        }
        public int yCursor
        {
            get => _yCursor;
            set => SetProperty(ref _yCursor, value);
        }
        //The Screen Scale (1=100%, 1.25=125%, ...)
        public double ScreenScale => _screenScale;
        /// <summary>
        /// Call this method will update the ScreenScale value
        /// </summary>
        /// <returns></returns>
        public double RefreshScreenScale()
        {
            var dpiXProperty = typeof(SystemParameters).GetProperty("DpiX", BindingFlags.NonPublic | BindingFlags.Static);
            var varX = (int)dpiXProperty.GetValue(null, null);
            double dpiX = (double)varX / (double)96;
            if (dpiX >= 1.0000)
                _screenScale = dpiX;
            OnPropertyChanged("ScreenScale");
            return ScreenScale;
        }

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

        /// <summary>
        /// WorkScreen will be updated when IsMoveing is true, and cursor move cross Screen boundary.
        /// </summary>
        public Screen? WorkScreen
        {
            get => _workScreen;
            set
            {
                //We will prevent to set a null value
                if (value == null)
                    return;

                bool isEqualed = (value.Equals(_workScreen));
                SetProperty(ref _workScreen, value);
                if (!isEqualed)
                {
                    if (WorkScreenChanged != null)
                        Task.Run(() => WorkScreenChanged.Invoke(this, _workScreen));
                }
            }
        }


        public string WorkScreenName
        {
            get
            {
                if (_workScreen == null)
                    return "";
                return _workScreen.DeviceName;
            }
        }

        public void RefreshWorkScreen()
        {
            WorkScreen = GetScreenFromCursor();
        }
        #endregion

        #region Moving Support
        public void RefreshCellRects()
        {
            if (IsAwsWindowVisible)
            {
                if (_awsWindow != null)
                {
                    _awsWindow.RefreshCellRects();
                }
            }
            foreach (EAWorkWindow workWin in _workWindows)
            {
                if (workWin == null)
                    continue;
                if (!workWin.IsUsed)
                    continue;
                workWin.RefreshCellRects();
            }
            RefreshWorkWinInfos();
        }

        public CellObj? DetermineHoveringCellObj(int x, int y)
        {
            CellObj? hoveringCell = null;
            HoveringWindow = "";
            if (IsAwsWindowVisible)
            {
                if (_awsWindow != null)
                {
                    if (IsAwsWindowVisible)
                    {
                        CellObj? cellObj = _awsWindow.DetermineHoveringCellObj(x, y);
                        if (cellObj != null)
                        {
                            HoveringWindow = "aws";
                            HoveringCellObj = cellObj;
                            HoveringSplit = HoveringAwsIcon;

                            hoveringCell = cellObj;
                            WriteLog($" * HoveringCell=AWS{cellObj.Name}");
                            return cellObj;
                        }
                        else
                            HoveringWindow = "";
                    }
                }
            }
            if (IsWorkWindowVisible)
            {
                int idxWorkWin = -1;
                foreach (EAWorkWindow workWin in _workWindows)
                {
                    idxWorkWin++;
                    if (!workWin.IsUsed)
                        continue;

                    CellObj? cellObj = workWin.DetermineHoveringCellObj(x, y);
                    if (cellObj != null)
                    {
                        //HoveringScreen = workWin.ScreenDeviceName;
                        HoveringCellObj = cellObj;
                        HoveringWindow = $"w{idxWorkWin}";
                        WriteLog($" * HoveringCell=Work{cellObj.Name}");
                        return cellObj;
                    }
                }
            }
            HoveringCellObj = null;
            WriteLog($" * HoveringCell=null");
            return null;
        }

        public CellObj? HoveringCellObj
        {
            get => _hoveringCellObj;
            set
            {
                SetProperty(ref _hoveringCellObj, value);
                OnPropertyChanged("HoveringCell");
            }
        }

        public string HoveringCell
        {
            get
            {
                if (_hoveringCellObj == null)
                    return "";
                return _hoveringCellObj.Name;
            }
        }
        #endregion

        #region EzSettings
        public bool IsOnlyShift
        {
            get => _isOnlyShift;
            set 
            {
                SetProperty(ref _isOnlyShift, value);
                OnPropertyChanged("IsAwsWindowVisible");
                OnPropertyChanged("IsWorkWindowVisible");
            }
        }
        public bool IsAwsEnabled
        {
            get => _isAwsEnabled;
            set
            {
                SetProperty(ref _isAwsEnabled, value);
                OnPropertyChanged("IsAwsWindowVisible");
            }
        }
        public bool IsShiftPressed
        {
            get => _isShiftPressed;
            set
            {
                SetProperty(ref _isShiftPressed, value);
                OnPropertyChanged("IsAwsWindowVisible");
                OnPropertyChanged("IsWorkWindowVisible");
            }
        }
        public bool IsWithoutGap
        {
            get => _isWithoutGap;
            set
            {
                SetProperty(ref _isWithoutGap, value);
                OnPropertyChanged("IsWithoutGap");
            }
        }
        public bool IsSpanMultiMonitors
        {
            get => _isSpanMultiMonitors;
            set
            {
                SetProperty(ref _isSpanMultiMonitors, value);
                OnPropertyChanged("IsSpanMultiMonitors");
            }
        }


        public bool ReloadEzSettingsFromUserSettingsFile()
        {
            if (_deviceManagerSA != null)
            {
                EzSettings? ezSettings = _deviceManagerSA.ReadEzSettings().Result;
                if (ezSettings != null)
                {
                    IsOnlyShift = ezSettings.IsOnlyAllowWhenShiftKeyPressed;
                    IsAwsEnabled = ezSettings.IsAwsEnabled;
                    IsWithoutGap = ezSettings.IsWidthoutGap;
                    IsSpanMultiMonitors = ezSettings.IsSpanAcrossMultiMonitors;
                    return true;
                }
            }
            return false;
        }
        #endregion

        #region WorkWindows
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
                OnPropertyChanged("IsWorkWindowVisible");
            }
        }

        public bool IsWorkWindowVisible
        {
            get
            {
                if (!IsMoving)
                    return false;
                if (!_isWorkUIEnabled)
                    return false;

                if (IsOnlyShift)
                {
                    return IsShiftPressed;
                }
                return true;
            }
        }

        public void InitWorkWindows()
        {
            Thread thread = new Thread(() =>
            {
                try
                {
                    for (int i = 0; i < MaxWorkWindowCount; i++)
                    {
                        if (_workWindows.Count >= MaxWorkWindowCount)
                            break;

                        int idxWorkWin = _workWindows.Count;
                        WriteLog($"Before new EAWorkWindow({idxWorkWin})");
                        EAWorkWindow workWin = new EAWorkWindow(this);
                        WriteLog($"Before new EAWorkWindow({idxWorkWin})");
                        _workWindows.Add(workWin);
                        workWin.Show();
                    }
                }
                catch (Exception eW)
                {
                    WriteLog("new EAWorkWIndow causes EXCEPTION", eW);
                }
 
                System.Windows.Threading.Dispatcher.Run();
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.IsBackground = true;
            thread.Start();
        }

        public void RefreshWorkWindows()
        {
            bool isSupportNonDellMonitors = false;

            WriteLog("@ArrangeVM.RefreshWorkWindows()");
            RefreshScreenScale();

            //Clear InUsed flag for all WorkWindows
            foreach(EAWorkWindow workWindow in _workWindows)
            {
                if (workWindow != null)
                    workWindow.IsUsed = false;
            }

            //Refresh process loop
            _workWindowUsedCount = 0;
            //For each screen assign a WorkWindow work for it
            foreach (Screen scr in System.Windows.Forms.Screen.AllScreens)
            {
                //Find the attached monitor of the Screen
                List<MonitorInfo> attachedMonitors = GetMonitorsFromDeviceName(scr.DeviceName);

                //If screen has no Dell monitor attached, then we no need a WorkWindow for it
                if ((attachedMonitors == null) || (attachedMonitors.Count <= 0))
                {
                    if (!isSupportNonDellMonitors)
                        continue;
                }

                EAWorkWindow? workWindow = GetUnusedWorkWindow();
                if (workWindow == null)
                {
                    break;
                }

                workWindow.IsUsed = true;
                workWindow.SetWorkScreen(scr, attachedMonitors);
                _workWindowUsedCount++;

                //Get SelectedSplit from MonitorSettings
                EAMonitorSettings? eaSettings = ReadEAMonitorSettings(attachedMonitors[0]);
                if (eaSettings == null)
                {
                    int cellCount = 0;
                    char splitKey = 'A';
                    List<double> settings = new List<double>() { 1 };
                    workWindow.SetWorkingSplit(cellCount, splitKey, settings);
                }
                else
                {
                    int cellCount = eaSettings.SelectedSplit.CellCount;
                    char splitKey = eaSettings.SelectedSplit.SplitKey;
                    List<double> settings = eaSettings.SelectedSplit.Settings;
                    workWindow.SetWorkingSplit(cellCount, splitKey, settings);
                }

            } //foreach (Screen scr in System.Windows.Forms.Screen.AllScreens)

            OnPropertyChanged("WorkWindowUsedCount");
            RefreshWorkWinInfos();
        }

        private EAWorkWindow? GetUnusedWorkWindow()
        {
            foreach (EAWorkWindow workWin in _workWindows)
            {
                if (workWin != null)
                    if (!workWin.IsUsed)
                        return workWin;
            }
            return null;
        }

        public int WorkWindowUsedCount
        {
            get => _workWindowUsedCount;
        }
        public EAWorkWindow? FindWorkWindowByMonitor(MonitorInfo mi)
        {
            if (_workWindows == null)
                return null;

            foreach (EAWorkWindow workWin in _workWindows)
            {
                if (workWin.IsUsed)
                {
                    if (workWin.IsMyMonitor(mi))
                    {
                        return workWin;
                    }
                }
            }
            return null;
        }

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
            ObservableCollection<string> newInfo = new ObservableCollection<string>();
            int idx = 0;
            foreach (EAWorkWindow workWin in _workWindows)
            {
                if (workWin == null)
                    continue;
                if (!workWin.IsUsed)
                    continue;

                string workInfo = $"{idx}:{workWin.CellListJson}";
                newInfo.Add(workInfo);
            }
            WorkWinCellInfos = newInfo;
            OnPropertyChanged("WorkWinCellInfos");
        }

        #endregion WorkWindows

        #region AWS Window
        public void InitAwsWindow()
        {
            Thread thread = new Thread(() =>
            {
                if (_awsWindow == null)
                {
                    try
                    {
                        WriteLog("Before new AwsWindow)");
                        _awsWindow = new AwsWindow(this);
                        WriteLog("After new AwsWindow)");
                        _awsWindow.Show();
                    }
                    catch (Exception eA)
                    {
                        WriteLog("new EAWorkWIndow causes EXCEPTION", eA);
                    }
                }
 
                System.Windows.Threading.Dispatcher.Run();
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.IsBackground = true;
            thread.Start();
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
                        if (IsOnlyShift)
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

        public double xAwsWindow
        {
            get => _xAwsWindow;
            set => SetProperty(ref _xAwsWindow, value);
        }
        public double yAwsWindow
        {
            get => _yAwsWindow;
            set => SetProperty(ref _yAwsWindow, value);
        }

        public AwsWindow AwsWindow => _awsWindow;
        #endregion

        #region AWS Icons
        public ISplitCtrl AwsIcon1
        {
            get => _awsIcon1;
            set
            {
                SetProperty(ref _awsIcon1, value);
                OnPropertyChanged("AwsIcon1Info");
            }
        }
        public ISplitCtrl AwsIcon2
        {
            get => _awsIcon2;
            set
            {
                SetProperty(ref _awsIcon2, value);
                OnPropertyChanged("AwsIcon2Info");
            }
        }
        public ISplitCtrl AwsIcon3
        {
            get => _awsIcon3;
            set 
            {
                SetProperty(ref _awsIcon3, value);
                OnPropertyChanged("AwsIcon3Info");
            }
        }
        public ISplitCtrl AwsIcon4
        {
            get => _awsIcon4;
            set
            {
                SetProperty(ref _awsIcon4, value);
                OnPropertyChanged("AwsIcon4Info");
            }
        }

        public ISplitCtrl? HoveringAwsIcon
        {
            get => _hoveringAwsIcon;
            set => SetProperty(ref _hoveringAwsIcon, value);
        }

        /// <summary>
        /// Refresh AWS Icons from the specifies RecentList
        /// </summary>
        /// <param name="recentList"></param>
        /// <returns>true if the AWS Icons has been updated</returns>
        public bool RefreshAwsIconsFromRecentList(SplitJson[] recentList)
        {
            //Validation: recentList count >= 4
            if (recentList != null)
            {
                if (recentList.Length >= 4)
                {
                    AwsIcon1 = ArrangeVM.SplitCtrlFromSplitJson(recentList[0], eSplitModes.AWS);
                    AwsIcon2 = ArrangeVM.SplitCtrlFromSplitJson(recentList[1], eSplitModes.AWS);
                    AwsIcon3 = ArrangeVM.SplitCtrlFromSplitJson(recentList[2], eSplitModes.AWS);
                    AwsIcon4 = ArrangeVM.SplitCtrlFromSplitJson(recentList[3], eSplitModes.AWS);
                    return true;
                }
            }
            return false;
       }

        public bool AreAwsIconsLoaded
        {
            get
            {
                return (_awsIcon1 != null) && (_awsIcon2 != null) && (_awsIcon3 != null) && (_awsIcon4 != null);   
            }
        }
        /// <summary>
        /// Coonvert a SplitJson to ISplitCtrl
        /// </summary>
        /// <param name="spJson"></param>
        /// <param name="splitMode"></param>
        /// <returns></returns>
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

        public string AwsIcon1Info
        {
            get
            {
                if (_awsIcon1 == null)
                    return "(null)";
                return $"{_awsIcon1.FriendlyName}, Cells: {CellListText(_awsIcon1.CellList)}";
            }
        }
        public string AwsIcon2Info
        {
            get
            {
                if (_awsIcon2 == null)
                    return "(null)";
                return $"{_awsIcon2.FriendlyName}, Cells: {CellListText(_awsIcon2.CellList)}";
            }
        }
        public string AwsIcon3Info
        {
            get
            {
                if (_awsIcon3 == null)
                    return "(null)";
                return $"{_awsIcon3.FriendlyName}, Cells: {CellListText(_awsIcon3.CellList)}";
            }
        }
        public string AwsIcon4Info
        {
            get
            {
                if (_awsIcon4 == null)
                    return "(null)";
                return $"{_awsIcon4.FriendlyName}, Cells: {CellListText(_awsIcon4.CellList)}";
            }
        }

        private string CellListText(List<CellObj> cellList)
        {
            string outString = "[";
            int idx = 0;
            foreach (CellObj objCell in cellList)
            {
                if (idx > 0)
                    outString += ",";
                if (objCell.rc == Rect.Empty)
                {
                    outString += $"{{\"{objCell.Name}\": EMPTY}}";
                }
                else
                {
                    outString += $"{{\"{objCell.Name}\":{ArrangeVM.FormatRect(objCell.rc)}}}";
                }
            }
            outString += "]";
            return outString;
        }

        #endregion

        #region UI Rect Functions
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

        public static string FormatRect(System.Windows.Rect rc)
        {
            return $"({rc.Left},{rc.Top})-({rc.Right},{rc.Bottom}){rc.Width}x{rc.Height}";
        }

        public static Rect RectFromRectangle(Rectangle rectangle)
        {
            Rect rcOut = new Rect();
            rcOut.X = rectangle.Left;
            rcOut.Y = rectangle.Top;
            rcOut.Width = rectangle.Width;
            rcOut.Height = rectangle.Height;
            return rcOut;
        }

        #endregion UI Rect Functions

        #region Hovering Window
        //Values:
        //"" : no hovering Window;
        //"w0" : WorkWindows[0]; "w1" : WorkWindows[1], ...
        //"aws : AWS Window
        public string HoveringWindow { get; set; } = "";
        #endregion

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

    }
}
