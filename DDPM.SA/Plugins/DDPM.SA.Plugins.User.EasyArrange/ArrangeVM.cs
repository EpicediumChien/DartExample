using CommunityToolkit.Mvvm.ComponentModel;
using DDPM.Easy.Common;
using DDPM.SA.Common;
using Dell.Client.Framework.Common;
using VcpCore.Common;

namespace DDPM.SA.Plugins.User.EasyArrange
{
    public class ArrangeVM : ObservableObject
    {
        private readonly object _lockObject = new();
        private IDisplayService? _displayManagerPlugin;

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
                return (_isWorkUIEnabled && IsMoving);
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
            foreach (KeyValuePair<string, EAWorkWindow> keyValuePair in _workWindows)
            {
                EAWorkWindow workWin = keyValuePair.Value;
                CellObj? cellObj = workWin.DetermineHoveringCellObj(x, y);
                if (cellObj != null)
                {
                    HoveringScreen = keyValuePair.Key;
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

        #region WorkWindows

        private Dictionary<string, EAWorkWindow> _workWindows = new Dictionary<string, EAWorkWindow>();
        private List<string> _workWinCellInfos = new List<string>();

        public Dictionary<string, EAWorkWindow> WorkWindows
        {
            get => _workWindows;
            set
            {
                SetProperty(ref _workWindows, value);
            }
        }

        public void AddWorkWindow(string key, EAWorkWindow workWin)
        {
            _workWindows.Add(key, workWin);

            OnPropertyChanged("WorkWindows");
            OnPropertyChanged("WorkWindowCount");
            //RefreshWorkWinInfos();
        }

        public int WorkWindowCount
        {
            get { return WorkWindows.Count; }
        }

        public void ClearWorkWindows()
        {
            foreach (KeyValuePair<string, EAWorkWindow> keyValuePair in _workWindows)
            {
                keyValuePair.Value.DispatcherClose();
            }
            _workWindows.Clear();
            RefreshWorkWinInfos();
        }

        public void RemoveWorkWindow(string key)
        {
            EAWorkWindow workWindow;
            if (WorkWindows.TryGetValue(key, out workWindow))
            {
                workWindow.DispatcherClose();
                WorkWindows.Remove(key);
                RefreshWorkWinInfos();
            }
        }

        public List<string> WorkWinCellInfos
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
                List<string> newInfo = new List<string>();
                foreach (KeyValuePair<string, EAWorkWindow> kv in _workWindows)
                {
                    EAWorkWindow workWin = kv.Value as EAWorkWindow;
                    if (workWin != null)
                    {
                        string workInfo = $"{workWin.WindowName}={workWin.CellListJson}";
                        newInfo.Add(workInfo);
                    }
                }
                WorkWinCellInfos = newInfo;
            }
        }

        public void RefreshCellRects()
        {
            foreach (KeyValuePair<string, EAWorkWindow> keyValuePair in _workWindows)
            {
                EAWorkWindow workWin = keyValuePair.Value;
                workWin.Invoke_RefreshCellRects();
            }
            RefreshWorkWinInfos();
        }

        //Find the WorkWindow in WorkWindows by DisplayName, for exmaple "\\.\DISPLAY1"
        public EAWorkWindow? FindWorkWindowByDisplayName(string displayName)
        {
            if (_workWinCellInfos == null)
                return null;

            EAWorkWindow workWindow = null;
            if (_workWindows.TryGetValue(displayName, out workWindow))
            {
                return workWindow;
            }
            return null;
        }

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

        public IDisplayService? DisplayManager
        {
            get => _displayManagerPlugin;
            set => _displayManagerPlugin = value;
        }

        public List<MonitorInfo>? GetMonitors()
        {
            if (_displayManagerPlugin == null)
                return null;

            return _displayManagerPlugin.GetMonitors().Result;
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
    }
}