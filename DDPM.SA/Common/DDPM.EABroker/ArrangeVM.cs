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
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using System.Windows.Documents;
using System.Windows.Media.Media3D;
using static VcpCore.Common.User32;
using Rect = System.Windows.Rect;
using nsWinEventHook;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Controls;

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
        private ISettingsManagerDev? _settingsManager = null;

        //EAPlugin
        private string _eaPluginLastError = "";

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
        //private EAScreen? _workEaScreen = null;

        //EzSettings
        private bool _isOnlyShift = EzSettings.Default_IsOnlyAllowWhenShiftKeyPressed;
        private bool _isAwsEnabled = EzSettings.Default_IsAwsEnabled;
        private bool _isWithoutGap = EzSettings.Default_IsWidthoutGap;
        private bool _isSpanMultiMonitors = EzSettings.Default_IsSpanAcrossMultiMonitors;
        private bool _isShiftPressed = false;

        //Hovering
        private CellObj? _hoveringCellObj = null;
        private string _hoveringWindow = "";

        //AWS Window
        private AwsWindow? _awsWindow = null;
        //The last Visibility state of AwsWindow, used to detect when Visibility changed
        private bool _isAwsWindowVisible = true;
        //The last AwsWindow (left,top) position
        private double _xAwsWindow = 0;
        private double _yAwsWindow = 0;
        private Rect _rcAwsWindow = new Rect();
        private string _awsWindowHoverMsg = "";

        //AWS Icons
        private ISplitCtrl? _awsIcon0 = null;
        private ISplitCtrl? _awsIcon1 = null;
        private ISplitCtrl? _awsIcon2 = null;
        private ISplitCtrl? _awsIcon3 = null;
        private ISplitCtrl? _awsIcon4 = null;

        //AWS Icons Rect
        private Rect _rcIcon0 = new Rect();
        private Rect _rcIcon1 = new Rect();
        private Rect _rcIcon2 = new Rect();
        private Rect _rcIcon3 = new Rect();
        private Rect _rcIcon4 = new Rect();


        private ISplitCtrl? _hoveringAwsIcon; //Point to one of {_awsIcon1 ~ _awsIcon4 }
        private CellObj? _hoveringAwsCellObj;

        //AWS Buddy Window
        private bool _isAwsBuddyWindowVisible = true;
        private AwsBuddyWindow? _awsBuddyWindow = null;

        //WorkWindows
        private List<EAWorkWindow> _workWindows = new List<EAWorkWindow>();
        private int _workWindowUsedCount = 0;
        private bool _isWorkUIEnabled = true;
        private ObservableCollection<string> _workWinCellInfos = new ObservableCollection<string>();
        private string _workWinsInfoText = ""; //Multiple lines info

        //ScreenIdWindows
        private List<ScreenIdWindow> _screenIdWindows = new List<ScreenIdWindow>();
        private bool _isScreenIdWindowsVisible = false;

        //Span across multiple monitors
        private SpanScreen _spanScreen = new SpanScreen();
        #endregion Private members

        #region Constants
        //Derek 2025/03/31 test data
        //MaxWorkWindowCount = 5 SA開起來 UI不開 memory使用量 330M
        //MaxWorkWindowCount = 1 SA開起來 UI不開 memory使用量 300M
        //MaxWorkWindowCount = 0 SA開起來 UI不開 memory使用量 260M
        public const int MaxWorkWindowCount = 5;
        //The gap between AWS window bottom to cursor when AWS showing up
        public const double dyAwsShow = 96;
        #endregion

        #region Events
        //Invoked,when (_isMoving==true) and cursor position cross screen boundary
        public EventHandler<Screen>? WorkScreenChanged = null;

        //Invoked when AWS Window visibility changed
        public EventHandler<bool>? AwsWindowVisibilityChanged = null;

        public EventHandler<bool>? AwsBuddyWindowVisibilityChanged = null;

        public EventHandler<ISplitCtrl>? HoveringAwsIconChanged = null;

        public EventHandler<CellObj>? HoveringCellObjChanged = null;
        public EventHandler<CellObj>? HoveringAwsCellObjChanged = null;

        #endregion

        #region Init
        public void InitInterfaces(IAgent agent, ILog log, IDeviceManagerSA devMgr, IDisplayService dispMgr, IEasyArrangeService eaService, ISettingsManagerDev settingsManager)
        {
            _agent = agent;
            _log = log;
            _deviceManagerSA = devMgr;
            _displayService = dispMgr;
            _easyArrangeService = eaService;
            _settingsManager = settingsManager;

            //Derek0403 經測試可以移掉？
            WriteLog($"[ArrangeVM] ReloadEzSettingsFromUserSettingsFile by InitInterfaces"); //add for debug
            //ReloadEzSettingsFromUserSettingsFile();
        }
        #endregion

        #region Exit
        /// <summary>
        /// Called when EABroker is going to stop.
        /// </summary>
        public void Exit()
        {
            //1 Close all WorkWindows
            foreach (EAWorkWindow workWin in _workWindows)
            {
                if (workWin != null)
                {
                    workWin.Close();
                }
            }
            //2 Close AwsWindow and AwsBuddyWindow
            if (_awsWindow != null)
            {
                _awsWindow.Close();
                _awsWindow = null;
            }
            if (_awsBuddyWindow != null)
            {
                _awsBuddyWindow.Close();
                _awsBuddyWindow = null;
            }
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
        public ILog Log => _log;
        #endregion

        #region DDPM.SA Functions
        /// <summary>
        /// Call to DeviceManagerSA.GetMonitors() to get a list of MonitorInfo that current supported monitor.
        /// </summary>
        /// <returns></returns>
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

        //Derek 2025/04/01
        //public void SendNotifyToUI_SetIsSpanEnabled()
        //{

        //}

        public void Invoke_EditCommand(MonitorInfo mi, EAArgs eaArgs)
        {
            if (_easyArrangeService == null)
                return;

            Task.Factory.StartNew(() =>
            {
                _ = _easyArrangeService.EditCommand(mi, eaArgs);
            });
        }

        /// <summary>
        /// Return the DeviceManagerSA.lastUISelectedMonitor_UI from UserSettings.
        /// The returned DDPMSimpleMonitorRecord has twoproperties:
        ///  string ModelName, string ServiceTag
        ///  Can be used to find a present MonitorInfo
        /// </summary>
        /// <returns></returns>
        public DDPMSimpleMonitorRecord? ReadLastSelectedMonitorRecord()
        {
            if (_settingsManager == null)
            {
                return null;
            }
            //Read AppSettings
            DDPMSettings appSettings = _settingsManager.ReloadAppConfigData().Result;
            //Return the UserSettings.lastUISelectedMonitor
            if (appSettings != null && appSettings.UserSettings != null)
            {
                return appSettings.UserSettings.lastUISelectedMonitor;
            }
            return null;
        }

        /// <summary>
        /// Return a MonitorInfo with below logic:
        /// If (monitor of lastUISelectedMonitor_UI is found in _AllMonitors) then
        ///    return the monitor of lastUISelectedMonitor_UI;
        /// Else If (IsSpanScreenWorking) then
        ///    return SpanScreen.GetPrimaryMonitor();
        /// Else If (_AllMonitor is not empty) then
        ///    return _AllMonitors[0];
        /// else
        ///    return null;
        /// </summary>
        /// <returns></returns>
        public MonitorInfo? GetSelectedMonitorInfo()
        {
            List<MonitorInfo>? _AllMonitors = GetMonitors();
            if ((_AllMonitors == null) || (_AllMonitors.Count <= 0))
                return null;

            DDPMSimpleMonitorRecord? lastMonitorRecord = ReadLastSelectedMonitorRecord();
            if (lastMonitorRecord != null)
            {
                MonitorInfo? mi = _AllMonitors.Find(x => x.modelName.Equals(lastMonitorRecord.ModelName) && x.edid.ServiceTag.Equals(lastMonitorRecord.ServiceTag));
                if (mi != null)
                    return mi;
            }
            if (IsSpanScreenWorking)
            {
                MonitorInfo? spanMo = _spanScreen.GetPrimaryMonitor();
                if (spanMo != null)
                    return spanMo;
            }
            return _AllMonitors[0];
        }

        public bool SetEASelectedLayout(MonitorInfo monitorInfo, int eaId)
        {
            if (_deviceManagerSA != null)
            {
                return _deviceManagerSA.SetEASelectedLayout(monitorInfo, eaId).Result;
            }
            return false;
        }

        public SplitJson[] ReadCustomList()
        {
            if (_deviceManagerSA != null)
            {
                return _deviceManagerSA.ReadEACustomList().Result;
            }
            return Array.Empty<SplitJson>();
        }

        public void SendEANotifyToUI(EAArgs eAArgs)
        {
            if (_deviceManagerSA != null)
            {
                _ = _deviceManagerSA.SendEANotify(eAArgs);
            }
        }
        #endregion

        #region EAPlugin
        public string EAPluginLastError
        {
            get => _eaPluginLastError;
            set
            {
                _eaPluginLastError = value;
            }
        }
        #endregion EAPlugin

        #region System Event Handlers
        /// <summary>
        /// Called from EAPlugin, when it receive a DisplaySettings event
        /// </summary>
        //Derek 2025/04/01
        //public void HandleDisplaySettings()
        //{
        //    //1 Check if Span across multiple monitors state changed
        //    //Original state
        //    //Execute refresh

        //}
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

            if (fileName.Equals(GlobalDefinitions.DDMExeName, StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        }
        public Rect rcWndForeground { get; set; }
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
            //Robert_Lin, 2024-12-6, use the CommonFunctions
            _screenScale = CommonFunctions.GetDpiX();
            OnPropertyChanged("ScreenScale");

            //ISplitCtrl.ScreenScale = _screenScale;

            //var dpiXProperty = typeof(SystemParameters).GetProperty("DpiX", BindingFlags.NonPublic | BindingFlags.Static);
            //if (dpiXProperty != null)
            //{
            //    var varX = (int)dpiXProperty.GetValue(null, null);
            //    double dpiX = (double)varX / (double)96;
            //    if (dpiX >= 1.0000)
            //        _screenScale = dpiX;
            //    OnPropertyChanged("ScreenScale");
            //}

            return ScreenScale;
        }

        /// <summary>
        /// Return the Screen from current mouse cursor position
        /// Need to update ArrangeVM.xCursor and yCursor at first
        /// </summary>
        /// <returns></returns>
        public Screen GetScreenFromCursor()
        {
            return Screen.FromPoint(new System.Drawing.Point(xCursor, yCursor));
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
                {
                    OnPropertyChanged("WorkScreenName");
                    OnPropertyChanged("WorkScreenInfoText");
                    OnPropertyChanged("IsWorkScreenVertical");
                    return;
                }

                bool isEqualed = (value.Equals(_workScreen));
                SetProperty(ref _workScreen, value);
                OnPropertyChanged("WorkScreenName");
                OnPropertyChanged("WorkScreenInfoText");
                OnPropertyChanged("IsWorkScreenVertical");
                if (!isEqualed && WorkScreenChanged != null)
                {
                    Task.Run(() => WorkScreenChanged.Invoke(this, _workScreen));
                }
            }
        }

        //Derek 2025/04/01
        //public string WorkScreenName
        //{
        //    get
        //    {
        //        if (_workScreen == null)
        //            return "";
        //        return _workScreen.DeviceName;
        //    }
        //}

        public string WorkScreenName
        {
            get
            {
                if (_workScreen == null)
                    return "";
                return _workScreen.DeviceName;
            }
        }

        public string WorkScreenInfoText
        {
            get
            {
                if (WorkScreen == null)
                    return "(null)";
                return $"DeviceName=[{WorkScreen.DeviceName}], Bounds=[{FormatRectangle(WorkScreen.Bounds)}], WorkingArea=[{FormatRectangle(WorkScreen.WorkingArea)}]";
            }
        }

        public void RefreshWorkScreen()
        {
            WorkScreen = GetScreenFromCursor();
        }

        public bool IsWorkScreenVertical
        {
            get
            {
                if (WorkScreen != null)
                {
                    if (WorkScreen.Bounds.Height > WorkScreen.Bounds.Width)
                    {
                        ArrangeVM.cxIcon = 90;
                        ArrangeVM.cyIcon = 120;
                        return true;
                    }
                }
                ArrangeVM.cxIcon = 120;
                ArrangeVM.cyIcon = 90;
                return false;
            }
        }
        #endregion

        #region Refresh Cell Rects
        public void RefreshCellRects(bool calledByFadeFinished = false)
        {
            if (IsAwsWindowVisible && _awsWindow != null)
            {
                _awsWindow.RefreshCellRects();
            }
            if ((IsWorkWindowVisible) || (calledByFadeFinished))
            {
                foreach (EAWorkWindow workWin in _workWindows)
                {
                    if (workWin == null)
                        continue;
                    if (!workWin.IsUsed)
                        continue;
                    workWin.RefreshCellRects();
                }
            }
            RefreshWorkWinInfos();
        }
        #endregion Refresh Cell Rects

        #region Determine Hovering

        /// <summary>
        /// Walkthrough all display Cells in SplitCttrls. and determine if any Cell will be hovered.
        /// When the hovering cell is determined, the CellObj will be in Hover state, and return itself.
        /// This method must be called under a Dispatcher thread.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
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
                        hoveringCell = _awsWindow.DetermineHoverigCellObj_Icon0(x, y);
                        if (hoveringCell != null)
                        {
                            HoveringWindow = "scr";
                            HoveringCellObj = hoveringCell;
                            HoveringSplit = HoveringAwsIcon;
                            HoveringAwsCellObj = hoveringCell;

                            //IsScreenIdWindowsVisible = true;

                            //foreach (ScreenIdWindow scrIdWnd in _screenIdWindows)
                            //{
                            //    int scrId = scrIdWnd.GetScreenId();
                            //    if (scrId.ToString() == hoveringCell.Name)
                            //        scrIdWnd
                            //}
                            return hoveringCell;
                        }
                        else
                            IsScreenIdWindowsVisible = false;
                    }
                    else
                    {
                        IsScreenIdWindowsVisible = false;
                    }

                    hoveringCell = _awsWindow.DetermineHoveringCellObj(x, y);
                    if (hoveringCell != null)
                    {
                        HoveringWindow = "aws";
                        HoveringCellObj = hoveringCell;
                        HoveringSplit = HoveringAwsIcon;
                        HoveringAwsCellObj = hoveringCell;

                        if (HoveringCellObj == AwsIcon0)
                        {
                            IsScreenIdWindowsVisible = true;
                        }
                        else
                        {
                            IsScreenIdWindowsVisible = false;
                        }
                        //WriteLog($" * HoveringCell=AWS{hoveringCell.Name}");

                        if (_awsBuddyWindow != null)
                        {
                            _awsBuddyWindow.MoveToScreen(_awsWindow.HoveringScreen);
                            _awsBuddyWindow.SetWorkSplit(HoveringSplit, hoveringCell.Name);
                            //_awsBuddyWindow.RefreshCellRects();
                        }
                        return hoveringCell;
                    }
                    else
                        HoveringWindow = "";
                }
            }
            else if (IsWorkWindowVisible)
            {
                int idxWorkWin = -1;
                foreach (EAWorkWindow workWin in _workWindows)
                {
                    idxWorkWin++;
                    if (!workWin.IsUsed)
                        continue;

                    hoveringCell = workWin.DetermineHoveringCellObj(x, y);
                    if (hoveringCell != null)
                    {
                        //HoveringScreen = workWin.ScreenDeviceName;
                        HoveringCellObj = hoveringCell;
                        HoveringWindow = $"w{idxWorkWin}";
                        //WriteLog($" * HoveringCell=Work{hoveringCell.Name}");
                        return hoveringCell;
                    }
                }
            }
            HoveringCellObj = null;
            //WriteLog($" * HoveringCell=null");
            return null;
        }

        public CellObj? HoveringCellObj
        {
            get => _hoveringCellObj;
            set
            {
                //Add changed detection
                bool isChanged = (_hoveringCellObj != value);
                SetProperty(ref _hoveringCellObj, value);
                OnPropertyChanged("HoveringCell");

                if (isChanged && HoveringCellObjChanged != null)
                {
                    _ = Task.Run(() => HoveringCellObjChanged.Invoke(this, _hoveringCellObj));
                }
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
        #endregion Determine Hovering

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
                OnPropertyChanged("IsWorkWindowVisible");
                OnPropertyChanged("IsAwsWindowVisible");
            }
        }
        public bool IsShiftPressed
        {
            get => _isShiftPressed;
            set
            {
                bool isChanged = (_isShiftPressed != value);
                if (isChanged)
                {
                    SetProperty(ref _isShiftPressed, value);
                    OnPropertyChanged("IsAwsWindowVisible");
                    OnPropertyChanged("IsWorkWindowVisible");
                }
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
        /// <summary>
        /// The option in DDPM UI, Easy Arrange / Settings page is set to ON.
        /// </summary>
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

                    //To detect IsSpanMultiMonitors changed
                    bool isSpanOnOffChanged = (IsSpanMultiMonitors != ezSettings.IsSpanAcrossMultiMonitors);
                    IsSpanMultiMonitors = ezSettings.IsSpanAcrossMultiMonitors;
                    ezSettings = null;

                    if (isSpanOnOffChanged)
                    {
                        WriteLog($"[ArrangeVM] RefreshWorkWindows call from ReloadEzSettingsFromUserSettingsFile when isSpanOnOffChanged=true");
                        RefreshWorkWindows();
                    }

                    ////Robert_Lin, 2024-10-20 Debug purpose, need to comment out in release build
                    //IsAwsEnabled = true;

                    WriteLog($"@ArrangeVM.ReloadEzSettingsFromUserSettingsFile(): IsOnlyShift={IsOnlyShift}, IsAwsEnabled={IsAwsEnabled}, IsWithoutGap={IsWithoutGap}, IsSpanMultiMonitors={IsSpanMultiMonitors}, isSpanOnOffChanged={isSpanOnOffChanged}");
                    return true;
                }
            }
            else
            {
                WriteLog($"@ArrangeVM.ReloadEzSettingsFromUserSettingsFile(): IDeviceManagerSA is null");
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
                OnPropertyChanged("IsAwsWindowVisible");
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
                if (IsAwsEnabled)
                    return false;

                if (IsOnlyShift)
                {
                    return IsShiftPressed;
                }
                else
                {
                    //PIMS-317659
                    //When IsOnlySift is OFF
                    //IsShiftPress ShowWorkWindow?
                    // True        Hide (False)
                    // False       Show (True)
                    return !IsShiftPressed;
                }
            }
        }

        private void ResetWorkWindows()
        {
            SplitJson spjEmpty = new SplitJson()
            {
                EAID = 0,
                CellCount = 0,
                SplitKey = 'A',
                Settings = new List<double>()
            };
            //Clear InUsed flag for all WorkWindows
            foreach (EAWorkWindow workWindow in _workWindows)
            {
                if (workWindow != null)
                {
                    if (workWindow.IsUsed)
                    {
                        workWindow.SetWorkingSplit(spjEmpty);
                    }
                    workWindow.IsUsed = false;
                }
            }

            //Refresh process loop
            _workWindowUsedCount = 0;
            OnPropertyChanged("WorkWindowUsedCount");
        }
        public void InitWorkWindows()
        {
            int added = 0;
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
                    WriteLog("new EAWorkWindow causes EXCEPTION", eW);
                }

                added++;
                System.Windows.Threading.Dispatcher.Run();
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.IsBackground = true;
            thread.Start();

            while (added <= 0)
            {
                Task.Delay(10).Wait();
            }
        }

        //To be removed, do use and test
        //Derek 2025/04/01 due to referces = 0
        //public void RefreshWorkWindows_v1_Unused()
        //{
        //    bool isSupportNonDellMonitors = false;

        //    WriteLog("@ArrangeVM.RefreshWorkWindows()");
        //    RefreshScreenScale();

        //    //Clear InUsed flag for all WorkWindows
        //    foreach (EAWorkWindow workWindow in _workWindows)
        //    {
        //        if (workWindow != null)
        //            workWindow.IsUsed = false;
        //    }

        //    //Refresh process loop
        //    _workWindowUsedCount = 0;
        //    //For each screen assign a WorkWindow work for it
        //    foreach (Screen scr in System.Windows.Forms.Screen.AllScreens)
        //    {
        //        //Find the attached monitor of the Screen
        //        List<MonitorInfo>? attachedMonitors = GetMonitorsFromDeviceName(scr.DeviceName);

        //        //If screen has no Dell monitor attached, then we no need a WorkWindow for it
        //        if (!isSupportNonDellMonitors && ((attachedMonitors == null) || (attachedMonitors.Count <= 0)))
        //        {
        //            continue;
        //        }

        //        EAWorkWindow? workWindow = GetUnusedWorkWindow();
        //        if (workWindow == null)
        //        {
        //            break;
        //        }

        //        workWindow.IsUsed = true;
        //        workWindow.SetWorkScreen(scr, attachedMonitors);
        //        _workWindowUsedCount++;

        //        //Get SelectedSplit from MonitorSettings
        //        EAMonitorSettings? eaSettings = ReadEAMonitorSettings(attachedMonitors[0]);
        //        if (eaSettings == null)
        //        {
        //            int cellCount = 0;
        //            char splitKey = 'A';
        //            List<double> settings = new List<double>() { 1 };

        //            //workWindow.SetWorkingSplit(cellCount, splitKey, settings);

        //            SplitJson spj0A = new SplitJson();
        //            spj0A.CellCount = cellCount;
        //            spj0A.SplitKey = splitKey;
        //            spj0A.Settings = settings;
        //            workWindow.SetWorkingSplit(spj0A);
        //        }
        //        else
        //        {
        //            int cellCount = eaSettings.SelectedSplit.CellCount;
        //            char splitKey = eaSettings.SelectedSplit.SplitKey;
        //            List<double> settings = eaSettings.SelectedSplit.Settings;
        //            //workWindow.SetWorkingSplit(cellCount, splitKey, settings);
        //            workWindow.SetWorkingSplit(eaSettings.SelectedSplit);
        //        }

        //    } //foreach (Screen scr in System.Windows.Forms.Screen.AllScreens)

        //    OnPropertyChanged("WorkWindowUsedCount");
        //    RefreshWorkWinInfos();

        //    //Robert_Lin, 2024-10-18 Workaround
        //    //If ScreenCount>2, we assume it may have one Dell monitor, but no WorkWindow created, then we will
        //    //Redo this method by raise a "DisplaySettingsChanged" event
        //    if (System.Windows.Forms.Screen.AllScreens.Length >= 2 && WorkWindowUsedCount == 0)
        //    {
        //        System.Threading.Timer timer1 = new System.Threading.Timer((obj) =>
        //        {
        //            _agent?.RaiseEvent(AgentEventNames.DisplaySettingsChanged, this, new EventManagerArgs());
        //        }, null, 2000, Timeout.Infinite);
        //    }

        //}

        //(v2)Robert_Lin, 2024-11-18, new version consider when SpanScreen is ON
        public void RefreshWorkWindows(bool isInit = false)
        {
            WriteLog($"@ ArrangeVM.RefreshWorkWindows() isInit = {isInit}");
            bool isSupportNonDellMonitors = false;

            WriteLog($"[ArrangeVM] ReloadEzSettingsFromUserSettingsFile by RefreshWorkWindows"); //add for debug
            ReloadEzSettingsFromUserSettingsFile();
            RefreshScreenScale();

            //Clear InUsed flag for all WorkWindows
            ResetWorkWindows();
            //foreach (EAWorkWindow workWindow in _workWindows)
            //{
            //    if (workWindow != null)
            //        workWindow.IsUsed = false;
            //}

            //Refresh process loop
            _workWindowUsedCount = 0;

            List<MonitorInfo>? monitors = GetMonitors();
            List<EAScreen> eaScreens = EAScreen.GetEAScreens(monitors);
            WriteLog($"Count={eaScreens.Count}, {EAScreen.EAScreensToString(eaScreens)}");
            WriteLog($"EzSettings.IsSpanMultiMonitors={IsSpanMultiMonitors}, IsSpanScreenWorking={IsSpanScreenWorking}");

            //Allocate WorkWindow for SpanScreen at first
            WriteLog($"Beofre allocate for SpanScreen, WorkerWindow, UsedCount={WorkWindowUsedCount}");
            if (_spanScreen != null)
            {
                //SpanScreen is enabled and ON
                if (IsSpanScreenWorking)
                {
                    //Get an unused WorkWindow
                    EAWorkWindow? workWindow = GetUnusedWorkWindow();
                    if (workWindow == null)
                    {
                        //No more available workWindow
                        WriteLog("GetUnusedWorkWindow() return null, No more available workWindow");
                        return;
                    }

                    //
                    workWindow.SetWorkScreenToSpanScreen();

                    MonitorInfo? miPrimary = _spanScreen.GetPrimaryMonitor();
                    if (miPrimary != null)
                    {
                        //Get SelectedSplit from MonitorSettings
                        EAMonitorSettings? eaSettings = ReadEAMonitorSettings(miPrimary);
                        if (eaSettings == null)
                        {
                            int cellCount = 0;
                            char splitKey = 'A';
                            List<double> settings = new List<double>() { 1 };

                            //workWindow.SetWorkingSplit(cellCount, splitKey, settings);

                            SplitJson spj0A = new SplitJson();
                            spj0A.CellCount = cellCount;
                            spj0A.SplitKey = splitKey;
                            spj0A.Settings = settings;
                            workWindow.SetWorkingSplit(spj0A);
                        }
                        else
                        {
                            int cellCount = eaSettings.SelectedSplit.CellCount;
                            char splitKey = eaSettings.SelectedSplit.SplitKey;
                            List<double> settings = eaSettings.SelectedSplit.Settings;
                            //workWindow.SetWorkingSplit(cellCount, splitKey, settings);
                            workWindow.SetWorkingSplit(eaSettings.SelectedSplit, isInit);
                        }
                    }

                    workWindow.IsUsed = true;
                }
                else
                {
                    //Remove the WorkWindow of the SpanScreen
                    EAWorkWindow? workForSpan = _workWindows.FirstOrDefault(x => x.IsWorkForSpanScreen);
                    if (workForSpan != null)
                    {
                        workForSpan.ResetToUnused();
                    }
                }
            }

            WriteLog($"After allocate for SpanScreen, WorkerWindow, UsedCount={WorkWindowUsedCount}");

            foreach (EAScreen eaScr in eaScreens)
            {
                //Check if this eaScr is included in the SpanScreen
                if (IsSpanScreenWorking && _spanScreen.IsExistScreen(eaScr.ScreenDeviceName))
                {
                    continue;
                }
                //If the EAScreen has no a Dell monitor attached, we will not allocate a WorkWindow for it
                if (!eaScr.HasAttachedMonitor && !isSupportNonDellMonitors)
                {
                    continue;
                    //If this flag is true, then we will allow non-dell monitor to go
                }

                //Get an unused WorkWindow
                EAWorkWindow? workWindow = GetUnusedWorkWindow();
                if (workWindow == null)
                {
                    //No more available workWindow
                    break;
                }

                //Check if this EA

                workWindow.IsUsed = true;
                workWindow.SetWorkScreen(eaScr, eaScr.AttachedMonitors);
                _workWindowUsedCount++;

                if (eaScr.HasAttachedMonitor)
                {
                    //Get SelectedSplit from MonitorSettings
                    EAMonitorSettings? eaSettings = ReadEAMonitorSettings(eaScr.AttachedMonitors[0]);
                    if (eaSettings == null)
                    {
                        int cellCount = 0;
                        char splitKey = 'A';
                        List<double> settings = new List<double>() { 1 };

                        //workWindow.SetWorkingSplit(cellCount, splitKey, settings);

                        SplitJson spj0A = new SplitJson();
                        spj0A.CellCount = cellCount;
                        spj0A.SplitKey = splitKey;
                        spj0A.Settings = settings;
                        workWindow.SetWorkingSplit(spj0A);
                    }
                    else
                    {
                        int cellCount = eaSettings.SelectedSplit.CellCount;
                        char splitKey = eaSettings.SelectedSplit.SplitKey;
                        List<double> settings = eaSettings.SelectedSplit.Settings;
                        //workWindow.SetWorkingSplit(cellCount, splitKey, settings);
                        workWindow.SetWorkingSplit(eaSettings.SelectedSplit, isInit);
                    }
                }

            } //foreach

            OnPropertyChanged("WorkWindowUsedCount");
            RefreshWorkWinInfos();

            //Robert_Lin, 2025-1-8, To save WorkWindows Info to log file
            //Default is not logged, can be enabled by DevSettings
            if (DevSettings.IsDumpWorkWindowsInfoOnRefreshEnabled())
            {
                //WriteLog(WorkWinsInfoText);
                WriteLog("[RefreshWorkWindows] DevSettings/IsDumpWorkWindowsInfoOnRefreshEnabled is TRUE");
            }

            //Robert_Lin, 2024-10-18 Workaround
            //If ScreenCount>2, we assume it may have one Dell monitor, but no WorkWindow created, then we will
            //Redo this method by raise a "DisplaySettingsChanged" event
            if (System.Windows.Forms.Screen.AllScreens.Length >= 2 &&
                WorkWindowUsedCount == 0)
            {
                System.Threading.Timer? timer1 = null;
                timer1 = new System.Threading.Timer((obj) =>
                {
                    try
                    {
                        WriteLog($"[ArrangeVM] create timer and raise DisplaySettingsChanged event");
                        _agent?.RaiseEvent(AgentEventNames.DisplaySettingsChanged, this, new EventManagerArgs());
                    }
                    catch (Exception e)
                    {
                        WriteLog($"[ArrangeVM] Catch exception[{e.Message}] when Raise DisplaySettingsChanged Event");
                    }
                    finally
                    {
                        // 任务执行完成后释放定时器资源
                        timer1?.Dispose();
                        timer1 = null;
                        WriteLog($"[ArrangeVM] create timer has been Dispose");
                    }
                    //_agent?.RaiseEvent(AgentEventNames.DisplaySettingsChanged, this, new EventManagerArgs());
                }, null, 2000, Timeout.Infinite);
            }
        }

        private EAWorkWindow? GetUnusedWorkWindow()
        {
            foreach (EAWorkWindow workWin in _workWindows)
            {
                if (workWin != null && !workWin.IsUsed)
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
                if (workWin.IsUsed && workWin.IsMyMonitor(mi))
                {
                    return workWin;
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
            StringBuilder? sb = new StringBuilder();
            int idx = -1;
            foreach (EAWorkWindow workWin in _workWindows)
            {
                idx++;
                if (workWin == null)
                    continue;
                if (!workWin.IsUsed)
                    continue;


                string workInfo = $"{idx}:{workWin.CellListJson}";
                newInfo.Add(workInfo);

                if (workWin.IsWorkForSpanScreen)
                {
                    sb.AppendLine($"[{idx}] SpanScreen");
                }
                else
                {
                    sb.AppendLine($"[{idx}]");
                    sb.AppendLine($"{workWin.GetWorkScreenInfoText()}");
                }

                sb.AppendLine($"Cells: {workWin.CellListJson}");
            }
            WorkWinCellInfos = newInfo;
            OnPropertyChanged("WorkWinCellInfos");

            WorkWinsInfoText = sb.ToString();

            sb.Clear();
            sb = null;
        }

        public bool NotifyEASelectedLayoutChanged(MonitorInfo monitorInfo, SplitJson spJson)
        {
            //Check if SpanScreen is working
            if (IsSpanScreenWorking)
            {
                //Check if the monitorInfo is included in the SpanScreen
                if (SpanScreen.IsExistScreen(monitorInfo.DisplayName))
                {
                    //Yes
                    //Find the workwindow of the SpanScreen
                    EAWorkWindow? workForSpan = _workWindows.FirstOrDefault(x => x.IsWorkForSpanScreen);
                    if (workForSpan != null)
                    {
                        return workForSpan.SetWorkingSplit(spJson, true);
                    }
                }
            }

            EAWorkWindow? workWindow = FindWorkWindowByMonitor(monitorInfo);
            if (workWindow != null)
            {
                return workWindow.SetWorkingSplit(spJson, true);
            }

            return false;
        }

        public string WorkWinsInfoText
        {
            get => _workWinsInfoText;
            set => SetProperty(ref _workWinsInfoText, value);
        }

        #endregion WorkWindows

        #region AWS Window
        public void InitAwsWindow()
        {
            int added = 0;
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

                        _awsBuddyWindow = new AwsBuddyWindow(this);
                        _awsBuddyWindow.Show();

                        _awsIcon0 = new SplitCtrl0B();
                    }
                    catch (Exception eA)
                    {
                        WriteLog("new AwsWindow causes EXCEPTION", eA);
                    }
                }
                added++;

                //Robert_Lin 2025-3-19, AwsWindow.Window_Closeing() will call Dispatcher.InvokeShutdown() to exit from below Run() loop.
                System.Windows.Threading.Dispatcher.Run();
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.IsBackground = true;
            thread.Start();

            while (added <= 0)
            {
                Task.Delay(10).Wait();
            }
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
                    else if (!IsWorkUIEnabled)
                        newValue = false;
                    else
                    {
                        if (IsOnlyShift)
                        {
                            newValue = IsShiftPressed;
                        }
                        else
                        {
                            //PIMS-317659
                            //When IsOnlySift is OFF
                            //IsShiftPress AWS window?
                            // True        Hide (False)
                            // False       Show (True)
                            newValue = !IsShiftPressed;
                        }
                    }
                }

                if (newValue != _isAwsWindowVisible)
                {
                    _isAwsWindowVisible = newValue;
                    if (AwsWindowVisibilityChanged != null)
                    {
                        _ = Task.Run(() => AwsWindowVisibilityChanged.Invoke(this, newValue));
                    }
                    OnPropertyChanged("IsAwsBuddyWindowVisible");
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

        public AwsWindow? AwsWindow => _awsWindow;

        public Rect rcAwsWindow
        {
            get => _rcAwsWindow;
            set
            {
                SetProperty(ref _rcAwsWindow, value);
                OnPropertyChanged("AwsWindowRectText");
            }
        }
        public string AwsWindowRectText
        {
            get { return FormatRect(rcAwsWindow); }
        }

        public string AwsWindowHoverMsg
        {
            get => _awsWindowHoverMsg;
            set => SetProperty(ref _awsWindowHoverMsg, value);
        }
        #endregion

        #region AWS Icons
        public static double cxIcon { get; set; } = 120;
        public static double cyIcon { get; set; } = 90;

        public ISplitCtrl? AwsIcon0
        {
            get => _awsIcon0;
            set
            {
                SetProperty(ref _awsIcon0, value);
                OnPropertyChanged("AwsIcon0Info");
            }
        }

        public ISplitCtrl? AwsIcon1
        {
            get => _awsIcon1;
            set
            {
                SetProperty(ref _awsIcon1, value);
                OnPropertyChanged("AwsIcon1Info");
            }
        }
        public ISplitCtrl? AwsIcon2
        {
            get => _awsIcon2;
            set
            {
                SetProperty(ref _awsIcon2, value);
                OnPropertyChanged("AwsIcon2Info");
            }
        }
        public ISplitCtrl? AwsIcon3
        {
            get => _awsIcon3;
            set
            {
                SetProperty(ref _awsIcon3, value);
                OnPropertyChanged("AwsIcon3Info");
            }
        }
        public ISplitCtrl? AwsIcon4
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
            set
            {
                bool isChanged = (_hoveringAwsIcon != value);
                SetProperty(ref _hoveringAwsIcon, value);
                OnPropertyChanged("HoveringAwsIconText");
                if (isChanged && HoveringAwsIconChanged != null)
                {
                    _ = Task.Run(() => HoveringAwsIconChanged.Invoke(this, _hoveringAwsIcon));
                }
            }
        }

        public string HoveringAwsIconText
        {
            get
            {
                if (HoveringAwsIcon == null)
                    return "(null)";

                //Robert_Lin, 2025-2-5 To prevent null exception: FriendName will be loaded from Multilingual resource, may be null.
                //OLD:
                //return $"{HoveringAwsIcon.CtrlClass}[{HoveringAwsIcon.FriendlyName}]";
                //NEW:
                if (string.IsNullOrEmpty(HoveringAwsIcon.FriendlyName))
                    return $"{HoveringAwsIcon.CtrlClass}[]";
                else
                    return $"{HoveringAwsIcon.CtrlClass}[{HoveringAwsIcon.FriendlyName}]";
            }
        }

        //Robert_Lin, 2024-10-15 access to UI, may need move to Dispatcher thread
        /// <summary>
        /// Refresh AWS Icons from the specifies RecentList
        /// </summary>
        /// <param name="recentList"></param>
        /// <returns>true if the AWS Icons has been updated</returns>
        public bool RefreshAwsIconsFromRecentList(SplitJson[] recentList)
        {
            //Validation: recentList count >= 4
            if (recentList != null && recentList.Length >= 4)
            {
                AwsIcon1 = SplitCtrlFromSplitJson(recentList[0], eSplitModes.AWS, _rcIcon1);
                AwsIcon2 = SplitCtrlFromSplitJson(recentList[1], eSplitModes.AWS, _rcIcon2);
                AwsIcon3 = SplitCtrlFromSplitJson(recentList[2], eSplitModes.AWS, _rcIcon3);
                AwsIcon4 = SplitCtrlFromSplitJson(recentList[3], eSplitModes.AWS, _rcIcon4);
                return true;

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
        public ISplitCtrl? SplitCtrlFromSplitJson(SplitJson spJson, eSplitModes splitMode)
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
            //if ((spJson.CellCount == 0) && (spJson.SplitKey == 'B'))
            if (spJson.IsOverlapLayout)
            {
                SplitCtrl0B spctrl0B = (SplitCtrl0B)splitCtrl;
                if (splitMode == eSplitModes.AWS)
                {
                    splitMode = eSplitModes.Work;
                    spctrl0B.ApplySettingsToCellList(new Rect(0, 0, ArrangeVM.cxIcon*ScreenScale , ArrangeVM.cyIcon*ScreenScale));
                }
            }

            return splitCtrl;
        }
        public ISplitCtrl? SplitCtrlFromSplitJson(SplitJson spJson, eSplitModes splitMode, Rect rcView)
        {
            ISplitCtrl? splitCtrl = ISplitCtrl.Create(spJson.CellCount, spJson.SplitKey);

            if (splitCtrl == null)
                return null;

            if (splitMode == eSplitModes.AWS)
            {
                splitCtrl.IsVertical = IsWorkScreenVertical;
            }

            if (spJson.Settings != null)
            {
                splitCtrl.Settings = new List<double>(spJson.Settings);
            }
            splitCtrl.FriendlyName = spJson.CustomName;

            splitCtrl.SplitMode = splitMode;
            if (spJson.IsOverlapLayout)
            {
                splitCtrl.EAID = spJson.EAID;
                SplitCtrl0B spctrl0B = (SplitCtrl0B)splitCtrl;
                if (splitMode == eSplitModes.AWS)
                {
                    //splitMode = eSplitModes.Work;
                    //spctrl0B.ApplySettingsToCellList(new Rect(0, 0, ArrangeVM.cxIcon*ScreenScale , ArrangeVM.cyIcon*ScreenScale));
                    if (spJson.IsMigratedFromDdm)
                    {
                        Rect rcApply = new Rect()
                        {
                            X = rcView.Left,
                            Y = rcView.Top,
                            Width = rcView.Width / ScreenScale,
                            Height = rcView.Height / ScreenScale
                        };
                        spctrl0B.ApplySettingsToCellList(rcApply);
                    }
                    else
                    {
                        Rect rcApply = new Rect()
                        {
                            X = rcView.Left,
                            Y = rcView.Top,
                            Width = rcView.Width ,
                            Height = rcView.Height
                        };
                        spctrl0B.ApplySettingsToCellList(rcApply);
                    }

                }
            }

            return splitCtrl;
        }

        public void OnPropertyChanged_AwsIconInfos()
        {
            OnPropertyChanged("AwsIcon0Info");
            OnPropertyChanged("AwsIcon1Info");
            OnPropertyChanged("AwsIcon2Info");
            OnPropertyChanged("AwsIcon3Info");
            OnPropertyChanged("AwsIcon4Info");
        }

        public string AwsIcon0Info
        {
            get
            {
                if (_awsIcon0 == null)
                    return "(null)";

                return $" Cells: {CellListText(_awsIcon0.CellList)}";
            }
        }
        public string AwsIcon1Info
        {
            get
            {
                if (_awsIcon1 == null)
                    return "(null)";

                return $"[{_awsIcon1.EAID}], Cells: {CellListText(_awsIcon1.CellList)}";

                //if (_awsIcon1.IsAddedCustomLayout)
                //    return $"{_awsIcon1.FriendlyName}, Cells: Cells: {CellListText(_awsIcon1.CellList)}";
                //else
                //    return $"{_awsIcon1.FriendlyName}, Cells: {CellListText(_awsIcon1.CellList)}";
            }
        }
        public string AwsIcon2Info
        {
            get
            {
                if (_awsIcon2 == null)
                    return "(null)";
                return $"[{_awsIcon2.EAID}], Cells: {CellListText(_awsIcon2.CellList)}";
                //if (_awsIcon2.IsAddedCustomLayout)
                //    return $"{_awsIcon2.FriendlyName}, Cells: {CellBordersText(_awsIcon2.CellBorders)}";
                //else
                //    return $"{_awsIcon2.FriendlyName}, Cells: {CellListText(_awsIcon2.CellList)}";
            }
        }
        public string AwsIcon3Info
        {
            get
            {
                if (_awsIcon3 == null)
                    return "(null)";
                return $"[{_awsIcon3.EAID}], Cells: {CellListText(_awsIcon3.CellList)}";
                //if (_awsIcon3.IsAddedCustomLayout)
                //    return $"{_awsIcon3.FriendlyName}, Cells: {CellBordersText(_awsIcon3.CellBorders)}, Bd: ";
                //else
                //    return $"{_awsIcon3.FriendlyName}, Cells: {CellListText(_awsIcon3.CellList)}";
            }
        }
        public string AwsIcon4Info
        {
            get
            {
                if (_awsIcon4 == null)
                    return "(null)";
                return $"[{_awsIcon4.EAID}], Cells: {CellListText(_awsIcon4.CellList)}";
                //if (_awsIcon4.IsAddedCustomLayout)
                //    return $"{_awsIcon4.FriendlyName}, Cells: {CellBordersText(_awsIcon4.CellBorders)}";
                //else
                //    return $"{_awsIcon4.FriendlyName}, Cells: {CellListText(_awsIcon4.CellList)}";
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
                    outString += $"{{\"{objCell.Name}\": EMPTY}} ";
                }
                else
                {
                    outString += $"{{\"{objCell.Name}\":{FormatRect(objCell.rc)},{FormatRect(objCell.CellBd.rect)}}} ";
                }
            }
            outString += "]";
            return outString;
        }
        
        //Derek 2025/04/01
        //private string CellBordersText(List<CellBorder> cbList)
        //{
        //    string outString = "[";
        //    int idx = 0;
        //    foreach (CellBorder cb in cbList)
        //    {
        //        if (idx > 0)
        //            outString += ",";
        //        if (cb.rect == Rect.Empty)
        //        {
        //            outString += $"{{\"{cb.CellName}\": EMPTY}} ";
        //        }
        //        else
        //        {
        //            outString += $"{{\"{cb.CellName}\":{ArrangeVM.FormatRect(cb.rect)}}} ";
        //        }
        //    }
        //    outString += "]";
        //    return outString;
        //}

        public CellObj? HoveringAwsCellObj
        {
            get => _hoveringAwsCellObj;
            set
            {
                //Add changed detection
                bool isChanged = (_hoveringAwsCellObj != value);
                SetProperty(ref _hoveringAwsCellObj, value);
                OnPropertyChanged("HoveringAwsCell");

                if (isChanged && HoveringAwsCellObjChanged != null)
                {
                    _ = Task.Run(() => HoveringAwsCellObjChanged.Invoke(this, _hoveringAwsCellObj));
                }
            }
        }

        //
        // AWS Icon Rects
        public Rect rcIcont0
        {
            get => _rcIcon0;
            set
            {
                SetProperty(ref _rcIcon0, value);
                OnPropertyChanged("rcIcon0Text");
            }
        }
        public Rect rcIcont1
        {
            get => _rcIcon1;
            set
            {
                SetProperty(ref _rcIcon1, value);
                OnPropertyChanged("rcIcon1Text");
            }
        }
        public Rect rcIcont2
        {
            get => _rcIcon2;
            set
            {
                SetProperty(ref _rcIcon2, value);
                OnPropertyChanged("rcIcon2Text");
            }
        }
        public Rect rcIcont3
        {
            get => _rcIcon3;
            set
            {
                SetProperty(ref _rcIcon3, value);
                OnPropertyChanged("rcIcon3Text");
            }
        }
        public Rect rcIcont4
        {
            get => _rcIcon4;
            set
            {
                SetProperty(ref _rcIcon4, value);
                OnPropertyChanged("rcIcon4Text");
            }
        }
        public string rcIcon0Text
        {
            get { return FormatRect(_rcIcon0); }
        }
        public string rcIcon1Text
        {
            get { return FormatRect(_rcIcon1); }
        }
        public string rcIcon2Text
        {
            get { return FormatRect(_rcIcon2); }
        }
        public string rcIcon3Text
        {
            get { return FormatRect(_rcIcon3); }
        }
        public string rcIcon4Text
        {
            get { return FormatRect(_rcIcon4); }
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
            return $"({rc.Left:F2},{rc.Top:F2})-({rc.Right:F2},{rc.Bottom:F2}){rc.Width:F2}x{rc.Height:F2}";
        }

        public static string FormatRectangle(Rectangle rc)
        {
            return $"({rc.Left:F2},{rc.Top:F2})-({rc.Right:F2},{rc.Bottom:F2}){rc.Width:F2}x{rc.Height:F2}";
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

        public string HoveringWindow
        {
            get => _hoveringWindow;
            set
            {
                SetProperty(ref _hoveringWindow, value);
                OnPropertyChanged("IsAwsBuddyWindowVisible");
            }
        }
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

        public string HoveringSplitText
        {
            get
            {
                if (HoveringSplit == null)
                    return "(null)";
                //Robert_Lin, 2025-2-5 To prevent null exception: FriendName will be loaded from Multilingual resource, may be null.
                //OLD:
                //return $"{HoveringSplit.CtrlClass}[{HoveringSplit.FriendlyName}]";
                //NEW:
                if (string.IsNullOrEmpty(HoveringAwsIcon.FriendlyName))
                    return $"{HoveringAwsIcon.CtrlClass}[]";
                else
                    return $"{HoveringAwsIcon.CtrlClass}[{HoveringAwsIcon.FriendlyName}]";
            }
        }
        #endregion HoveringSplit

        #region AWS Buddy Window
        public bool IsAwsBuddyWindowVisible
        {
            get
            {
                bool newValue = _isAwsBuddyWindowVisible;

                if (!IsMoving)
                {
                    newValue = false;
                }
                else if (!_isAwsWindowVisible)
                {
                    newValue = false;
                }
                else
                {
                    newValue = (HoveringWindow == "aws");
                }
                if (newValue != _isAwsBuddyWindowVisible)
                {
                    _isAwsBuddyWindowVisible = newValue;
                    if (AwsBuddyWindowVisibilityChanged != null)
                    {
                        _ = Task.Run(() => AwsBuddyWindowVisibilityChanged.Invoke(this, newValue));
                    }
                }
                return _isAwsBuddyWindowVisible;
            }
        }

        public Rect GetHoveringRectFromAwsBuddyWindow()
        {
            int screenId = 0;
            if (int.TryParse(HoveringCell, out screenId))
            {
                if ((screenId >= 1) && (screenId <= Screen.AllScreens.Length))
                {
                    Screen targetScreen = Screen.AllScreens[screenId - 1];
                    if (!targetScreen.Equals(_workScreen))
                    {
                        double xWorkScreen = 0;
                        double yWorkScreen = 0;
                        if (WorkScreen != null)
                        {
                            xWorkScreen = WorkScreen.Bounds.X;
                            yWorkScreen = WorkScreen.Bounds.Y;
                        }
                        double dx = rcWndForeground.Left - xWorkScreen;
                        double dy = rcWndForeground.Top - yWorkScreen;
                        double x = (double)targetScreen.WorkingArea.Left;
                        if (dx > 0)
                            x += dx;
                        double y = (double)targetScreen.WorkingArea.Top;
                        if (dy > 0)
                            y += dy;
                        Rect rcOut = new Rect(x, y, rcWndForeground.Width, rcWndForeground.Height);
                        return rcOut;
                    }
                }
                return Rect.Empty;
            }

            if (_awsBuddyWindow != null)
                return _awsBuddyWindow.GetHoveringCellRect();

            return Rect.Empty;
        }
        #endregion

        #region Screen ID Window
        public void InitScreenIdWindows()
        {
            int id = 1;
            foreach (Screen scr in Screen.AllScreens)
            {
                ScreenIdWindow screenIdWindow = new ScreenIdWindow(id, scr, this);
                _screenIdWindows.Add(screenIdWindow);
                screenIdWindow.Show();
                id++;
            }
        }

        public bool IsScreenIdWindowsVisible
        {
            get => _isScreenIdWindowsVisible;
            set => SetProperty(ref _isScreenIdWindowsVisible, value);
        }
        #endregion

        #region Telemetry
        public void SendTelemetry_EasyArrangeLayout()
        {
            if (_easyArrangeService != null &&
                HoveringSplit != null)
            {
                MonitorInfo? monitorInfo = null;
                if (WorkScreen != null)
                {
                    List<MonitorInfo>? monitorInfos = GetMonitorsFromDeviceName(WorkScreen.DeviceName);
                    if ((monitorInfos != null) && (monitorInfos.Count > 0))
                    {
                        monitorInfo = monitorInfos[0];
                    }
                }
                string eventValue = GetEasyArrangeLayoutTelemetryEventValueFromISplitCtrl(HoveringSplit);
                _easyArrangeService.SendEasyArrangeLayoutTelemetry(eventValue, monitorInfo);
            }
        }
        /// <summary>
        /// CellCount SplitKey => return value
        /// 0         'B'         "custom-layout"
        /// 2                     "2-windows"
        /// 3                     "3-windows"
        /// 4                     "4-windows"
        /// 5                     "5-windows"
        /// 6                     "6-windows"
        /// 7                     "7ormore-windows"
        /// </summary>
        /// <param name="splitCtrl"></param>
        /// <returns></returns>
        private string GetEasyArrangeLayoutTelemetryEventValueFromISplitCtrl(ISplitCtrl splitCtrl)
        {
            if ((splitCtrl.CellCount == 0) && (splitCtrl.SplitKey == 'B'))
                return "custom-layout";

            if (splitCtrl.CellCount == 7)
                return "7ormore-windows";

            return $"{splitCtrl.CellCount}-windows";

        }
        #endregion Telemetry

        public void CreateCellBorderListToSplitCtrlFromCellJsons(CellJson[] cellJsons, ref ISplitCtrl ispCtrl)
        {
            if (!ispCtrl.IsAddedCustomLayout)
                return;

            SplitCtrl0B spCtrl0B = (SplitCtrl0B)ispCtrl;
            spCtrl0B.CellList.Clear();
            if (spCtrl0B.CellBorders != null)
                spCtrl0B.CellBorders.Clear();
            else
                spCtrl0B.CellBorders = new List<CellBorder>();

            foreach (CellJson cellJson in cellJsons)
            {
                CellBorder cellBorder = new CellBorder();
                cellBorder.CellName = cellJson.Name;
                cellBorder.rcRatio = new Rect(cellJson.x, cellJson.y, cellJson.w, cellJson.h);
                spCtrl0B.CellBorders.Add(cellBorder);

                CellObj cellObj = new CellObj(cellJson.Name);
                cellObj.rcRatio = new Rect(cellJson.x, cellJson.y, cellJson.w, cellJson.h);
                spCtrl0B.CellList.Add(cellObj);
            }

        }

        #region Span across multiple monitors

        public bool DetectSpanCondition()
        {
            if (_spanScreen != null)
            {
                Stopwatch sw = Stopwatch.StartNew();
                List<MonitorInfo>? monitors = GetMonitors();
                bool res = _spanScreen.DetectSpanScreens(_log, monitors);
                sw.Stop();
                WriteLog($"DetectSpanCondition, IsSpanEnabled={res}, Elapsed {sw.ElapsedMilliseconds} msec");
                OnPropertyChanged("IsSpanEnabled");
                OnPropertyChanged("IsHorzSpan");
                OnPropertyChanged("SpanWorkingArea");
                OnPropertyChanged("SpanWorkingAreaText");
                OnPropertyChanged("IsSpanScreenWorking");
                return res;
            }
            return false;
        }

        /// <summary>
        /// True if current Montor Configure is meet the requirement of Span screen.
        /// </summary>
        public bool IsSpanEnabled
        {
            get
            {
                if (_spanScreen != null)
                {
                    return _spanScreen.IsSpanEnabled;
                }
                return false;
            }
        }

        public SpanScreen SpanScreen => _spanScreen;

        public bool IsHorzSpan
        {
            get
            {
                if (_spanScreen != null)
                {
                    return _spanScreen.IsHorzSpan;
                }
                return false;
            }
        }
        public Rectangle SpanWorkingArea
        {
            get
            {
                if (_spanScreen != null)
                {
                    return _spanScreen.WorkingArea;
                }
                return Rectangle.Empty;
            }
        }
        public string SpanWorkingAreaText
        {
            get
            {
                if (SpanWorkingArea.IsEmpty)
                    return "(Empty)";
                else
                    return FormatRect(RectFromRectangle(SpanWorkingArea));
            }
        }

        /// <summary>
        /// True when "Span condition is meet"(IsSpanEnabled) and "Span option is ON"(IsSpanMultiMonitors)
        /// </summary>
        public bool IsSpanScreenWorking
        {
            get
            {
                return (IsSpanEnabled && IsSpanMultiMonitors);
            }
        }

        public System.Windows.Rect GetSpanWorkingArea(Screen scr)
        {
            Rectangle rcWorkingArea = (IsSpanScreenWorking ? scr.WorkingArea : SpanScreen.WorkingArea);
            return new System.Windows.Rect(
                (double)rcWorkingArea.Left / ScreenScale,
                (double)rcWorkingArea.Top / ScreenScale,
                (double)rcWorkingArea.Width / ScreenScale,
                (double)rcWorkingArea.Height / ScreenScale
                );
        }

        public void NotifySelectedMonitorChanged()
        {
            //Only when SpanScreen is working
            if (IsSpanScreenWorking)
            {
                //Get the last selected monitor record from UserSettings
                List<MonitorInfo>? _AllMonitors = GetMonitors();
                //If no any monitor are connected
                if ((_AllMonitors == null) || (_AllMonitors.Count <= 0))
                    return;

                DDPMSimpleMonitorRecord? lastMonitorRecord = ReadLastSelectedMonitorRecord();
                if (lastMonitorRecord != null)
                {
                    MonitorInfo? mi = _AllMonitors.Find(x => x.modelName.Equals(lastMonitorRecord.ModelName) && x.edid.ServiceTag.Equals(lastMonitorRecord.ServiceTag));
                    if (mi != null)
                    {
                        //Read the selected layout of this monitor
                        EAMonitorSettings? monitorSettings = ReadEAMonitorSettings(mi);
                        if (monitorSettings != null)
                        {
                            //If the Monitor is attached in SpanScreen
                            if (SpanScreen.IsExistScreen(mi.DisplayName))
                            {
                                //Find the workwindow of the SpanScreen
                                EAWorkWindow? workForSpan = _workWindows.FirstOrDefault(x => x.IsWorkForSpanScreen);
                                if (workForSpan != null)
                                {
                                    workForSpan.SetWorkingSplit(monitorSettings.SelectedSplit, true);
                                }
                            }
                        }
                    }
                    else
                    {
                        //The last selected monitor is not present now
                    }
                }

            }
        }
        #endregion

        #region SetEAWindowPos
        public Rect SetEAWindowPos(IntPtr hWnd, Rect rcArrange, Rectangle? workingArea = null)
        {
            if (workingArea == null && WorkScreen != null)
            {
                workingArea = WorkScreen.WorkingArea;
            }

            if (IsWithoutGap)
            {

                double extendedFrameBoundsHorz = 3;
                rcArrange.Inflate(6 + extendedFrameBoundsHorz, 6);

                if (workingArea != null)
                {
                    int scrLeft = (int)workingArea?.Left;
                    if (rcArrange.Left < scrLeft)
                    {
                        double dx = scrLeft - rcArrange.Left;
                        rcArrange.X = scrLeft;
                        rcArrange.Width -= dx;
                    }
                }
            }
            if (!rcArrange.IsEmpty)
            {
                WinEventHook.SetWindowPosition(hWnd, rcArrange);
            }
            return rcArrange;
        }
        #endregion
    }
}
