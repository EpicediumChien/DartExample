using DDPM.Easy.Common;
using DDPM.SA.Common;
using Dell.Client.Framework.Common;
using System.Reflection;
using System.Windows;
using DDPM.Win32Lib;
using nsWinEventHook;
using System.Diagnostics;
using System.Windows.Controls;

namespace DDPM.SA.Plugins.User.EasyArrange
{
    //Robert_Lin, 2024-10-23 to be deleted
    /// <summary>
    /// Interaction logic for EAEditWindow.xaml
    /// </summary>
    public partial class EAEditWindow : Window
    {
        private ILog? _log;

        //private SaveCustomWindow saveCustomWindow;

        private string _orgFriendlyName = string.Empty;

        private string _lastError = string.Empty;

        #region ctor & Init
        public EAEditWindow()
        {
            InitializeComponent();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _log = EAPlugin.PluginIoc?.GetService<ILog>();
            _log?.Info($"EAEditWindow_Loaded, Input: {inputSplitCtrl.CellCount}{inputSplitCtrl.SplitKey}, [{inputSplitCtrl.SettingsString}]");

            //Hide window from Alt+tab
            System.Windows.Interop.WindowInteropHelper wndHelper = new System.Windows.Interop.WindowInteropHelper(this);
            Win32Lib.Win32.HideWinFromAltTab(wndHelper.Handle);

            //saveCustomWindow = new SaveCustomWindow();
            //saveCustomWindow.Owner = this;
            //saveCustomWindow.Left = this.Left;
            //saveCustomWindow.Top = this.Top;
            //saveCustomWindow.CustomName = _orgFriendlyName;
            //saveCustomWindow.CustomNames = _inputArgs.CustomNames;
            //saveCustomWindow.CancelButtonClick += saveCustomWidow_CancelButtonClick;
            //saveCustomWindow.SaveButtonClick += saveCustomWidow_SaveButtonClick;
            //saveCustomWindow.Show();

            //InitDragDlg();
        }
        #endregion ctor & Init

        #region Close
        public void InvokeClose()
        {
            this.Dispatcher.Invoke(() =>
            {
                Hide();
            });
        }
        #endregion Close

        #region Log
        private void LogInfo(string msg)
        {
            string logDatetime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff");
            _log?.Info($"[{logDatetime}] {msg}");
        }
        #endregion Log

        //[InfoWin solution]
        //private void OpenSaveCustomWindow()
        //{
        //    SaveCustomWindow saveCustomWindow = new SaveCustomWindow();
        //    //saveCustomWindow = new SaveCustomWindow();
        //    saveCustomWindow.Owner = this;
        //    saveCustomWindow.Left = this.Left;
        //    saveCustomWindow.Top = this.Top;
        //    saveCustomWindow.CustomName = _orgFriendlyName;
        //    saveCustomWindow.CustomNames = _inputArgs.CustomNames;
        //    saveCustomWindow.CancelButtonClick += saveCustomWidow_CancelButtonClick;
        //    saveCustomWindow.SaveButtonClick += saveCustomWidow_SaveButtonClick;
        //    saveCustomWindow.Show();
        //}

        #region Call out events

        public event EventHandler EditStarted;

        //Robert_Lin, 2024-9-13 Remove unused interfaces
        //public event EventHandler<string> EditCompleted;

        public event EventHandler<EAArgs> EditReturn;

        #endregion Call out events

        #region Input SplitCtrl

        private ISplitCtrl inputSplitCtrl = new SplitCtrl0A();
        private EAArgs _inputArgs;

        public bool SetSplitCtrl(int cellCount, char splitKey, List<double> settings, bool isVertical = false)
        {
            ISplitCtrl? isplitCtrl = ISplitCtrl.Create(cellCount, splitKey);
            if (isplitCtrl == null)
            {
                _log?.Error($"EAPlugin.EAEditWindow.SetSplitCtrl(), Invalid argument: {inputSplitCtrl.CellCount}{inputSplitCtrl.SplitKey}, [{inputSplitCtrl.SettingsString}]");
                return false;
            }

            inputSplitCtrl = isplitCtrl;
            inputSplitCtrl.IsVertical = isVertical;
            inputSplitCtrl.IsEditable = true;
            inputSplitCtrl.SplitMode = eSplitModes.Edit;

            if (settings != null)
            {
                inputSplitCtrl.Settings = settings;
            }
            splitCtrl.Visibility = Visibility.Visible;
            splitCtrl.Content = inputSplitCtrl.UC;
            return true;
        }

        /// <summary>
        /// Assign the edit arguments.
        /// Caller (EAPlugin) must call this method to assign the args before calling EAEditWindow.Show()
        /// </summary>
        /// <param name="args">The EAArgs object to specify the argument for the editing.</param>
        /// <returns>
        /// true: if the input args is accepted, and the caller (EAPlugin) can signal EditStarted to
        ///       its caller (DDPM.UI). the edit window to be display soon.        ///
        /// false: otherwise. The args is invalid, caller (EAPlugin can get the error message from EAEditWindow.LastError
        /// </returns>
        public bool SetInputArg(EAArgs args, Screen scr)
        {
            _inputArgs = args;

            this.Dispatcher.Invoke(() =>
            {
                //Try to create a ISplitCtrl to verify (cellCount,SplitKey) is valid
                ISplitCtrl? ispCtrl = ISplitCtrl.Create(args.CellCount, args.SplitKey);
                if (ispCtrl == null)
                {
                    //Invalidd CellCount+SplitKey, make the error messgae
                    _lastError = $"EAPlugin.EAEditWindow.SetInputArg(), Invalid argument: {args.CellCount}{args.SplitKey}, [{SplitCtrlVM.Double_To_String(args.Settings)}]";
                    _log?.Error(_lastError);

                    return;// false;
                }
                inputSplitCtrl = ispCtrl;
                inputSplitCtrl.IsEditable = true;
                inputSplitCtrl.SplitMode = eSplitModes.Edit;
                inputSplitCtrl.IsVertical = (scr.Bounds.Width < scr.Bounds.Height);

                if (args.Settings != null)
                {
                    inputSplitCtrl.Settings = args.Settings;
                }

                _orgFriendlyName = args.CustomName;

                //Calculate the position/size of EditWindow
                double dpiX = 1.000;
                var dpiXProperty = typeof(SystemParameters).GetProperty("DpiX", BindingFlags.NonPublic | BindingFlags.Static);
                if (dpiXProperty != null)
                {
                    var varX = (int)dpiXProperty.GetValue(null, null);
                    dpiX = (double)varX / (double)96;
                }

                splitCtrl.Content = inputSplitCtrl.UC;
                //SplitContent = inputSplitCtrl.UC;

                Left = scr.WorkingArea.Left / (double)dpiX;
                Top = scr.WorkingArea.Top / (double)dpiX;
                Width = scr.WorkingArea.Width / (double)dpiX;
                Height = scr.WorkingArea.Height / (double)dpiX;

                //[InfoWin solution] need add
                //OpenSaveCustomWindow();

                Show();
            });
            return true;
        }

        public string LastError
        { get { return _lastError; } }

        public List<double> GetSettings()
        {
            return inputSplitCtrl.Settings;
        }

        #endregion Input SplitCtrl

        #region SaveDlg Button Clicks - [InfoWin solution]
        //[InfoWin solution]
        private void saveCustomWidow_CancelButtonClick(object sender, string e)
        {
            //if (EditCompleted != null)
            //    EditCompleted(this, "");

            if (EditReturn != null)
            {
                EAArgs retArgs = new EAArgs(_inputArgs);
                retArgs.Result = false;
                retArgs.Command = "EditReturn";
                retArgs.Message = "User cancel the editing.";
                EditReturn(this, retArgs);
            }
        }

        //[InfoWin solution]
        private void saveCustomWidow_SaveButtonClick(object sender, string e)
        {
            //if (EditCompleted != null)
            //    EditCompleted(this, "");

            if (EditReturn != null)
            {
                EAArgs retArgs = new EAArgs(_inputArgs);
                retArgs.Result = true;
                retArgs.Settings = inputSplitCtrl.Settings;
                //retArgs.CustomName = saveCustomWindow.CustomName;
                retArgs.Command = "EditReturn";
                EditReturn(this, retArgs);
            }
        }

        #endregion SaveDlg Button Clicks

        #region Show and Edit
        public bool ShowAndEdit(EAArgs args, Screen scr)
        {
            _inputArgs = args;
            if ((args.CellCount == 0) && (args.SplitKey == 'B'))
            {
                this.Dispatcher.Invoke(() => { UI_ShowAndEdit_AddedCustom(args, scr); });
            }
            else if (ISplitCtrl.IsExisted(args.CellCount, args.SplitKey))
            {
                this.Dispatcher.Invoke(() => { UI_ShowAndEdit_PredefinedCustom(args, scr); });
            }
            else
            {
                return false;
            }
            return true;
        }

        private void UI_ShowAndEdit_PredefinedCustom(EAArgs args, Screen scr)
        {
            //Try to create a ISplitCtrl to verify (cellCount,SplitKey) is valid
            ISplitCtrl? ispCtrl = ISplitCtrl.Create(args.CellCount, args.SplitKey);
            if (ispCtrl == null)
            {
                //Invalidd CellCount+SplitKey, make the error messgae
                _lastError = $"EAPlugin.EAEditWindow.SetInputArg(), Invalid argument: {args.CellCount}{args.SplitKey}, [{SplitCtrlVM.Double_To_String(args.Settings)}]";
                _log?.Error(_lastError);

                return;// false;
            }
            inputSplitCtrl = ispCtrl;
            inputSplitCtrl.IsEditable = true;
            inputSplitCtrl.SplitMode = eSplitModes.Edit;
            inputSplitCtrl.IsVertical = (scr.Bounds.Width < scr.Bounds.Height);

            if (args.Settings != null)
            {
                inputSplitCtrl.Settings = args.Settings;
            }

            _orgFriendlyName = args.CustomName;

            //Calculate the position/size of EditWindow
            double dpiX = 1.000;
            var dpiXProperty = typeof(SystemParameters).GetProperty("DpiX", BindingFlags.NonPublic | BindingFlags.Static);
            if (dpiXProperty != null)
            {
                var varX = (int)dpiXProperty.GetValue(null, null);
                dpiX = (double)varX / (double)96;
            }

            splitCtrl.Content = inputSplitCtrl.UC;
            //SplitContent = inputSplitCtrl.UC;

            Left = scr.WorkingArea.Left / (double)dpiX;
            Top = scr.WorkingArea.Top / (double)dpiX;
            Width = scr.WorkingArea.Width / (double)dpiX;
            Height = scr.WorkingArea.Height / (double)dpiX;

            //[InfoWin solution] need add
            //OpenSaveCustomWindow();

            Show();
        }
        private void UI_ShowAndEdit_AddedCustom(EAArgs args, Screen scr)
        {
            splitCtrl.Visibility = Visibility.Collapsed;
            _orgFriendlyName = args.CustomName;

            inputSplitCtrl = new SplitCtrl0B();

            //Calculate the position/size of EditWindow
            double dpiX = 1.000;
            var dpiXProperty = typeof(SystemParameters).GetProperty("DpiX", BindingFlags.NonPublic | BindingFlags.Static);
            if (dpiXProperty != null)
            {
                var varX = (int)dpiXProperty.GetValue(null, null);
                dpiX = (double)varX / (double)96;
            }
            Left = scr.Bounds.Left / (double)dpiX;
            Top = scr.Bounds.Top / (double)dpiX;
            Width = scr.Bounds.Width / (double)dpiX;
            Height = scr.Bounds.Height / (double)dpiX;
            Show();

            CaptureCustomLayout(scr);
        }
        #endregion

        #region Capture Added Custom Layout
        private void CaptureCustomLayout(Screen screen)
        {
            double scale = 1.000;
            var dpiXProperty = typeof(SystemParameters).GetProperty("DpiX", BindingFlags.NonPublic | BindingFlags.Static);
            if (dpiXProperty != null)
            {
                var varX = (int)dpiXProperty.GetValue(null, null);
                scale = (double)varX / (double)96;
            }

            //Clear CustomLayouts
            canvas.Children.Clear();

            //Prepare for Settings
            //Format:
            // First 4 elements [0]~[3]: BorderCount | ScreenScale | ScreenBoundsWidth | ScreenBoundsHeight
            // Later: BorderRect (left, top, width, height), (left, top, width, height), ...
            List<double> settings = new List<double>();
            settings.Add(0); //BorderCount will be updated later
            settings.Add(scale);
            settings.Add(screen.Bounds.Width);
            settings.Add(screen.Bounds.Height);

            //Enumerate all Window handle which will be fitered by IsTargetWindow()
            List<IntPtr> hWnds = Win32.GetWindowHandles(IsTargetWindow);
            LogInfo($"@ EAEditWindow.CaptureCustomLayout(), Enum candidate Window and add Borders");
            int idx = -1;
            int addCount = 0;
            //Second phase to filter out from the hWnd
            foreach (IntPtr hWnd in hWnds)
            {
                idx++;
                //Get the basic info of hWnd
                //
                string windowText = Win32._GetWindowText(hWnd);

                IntPtr hWndParent = Win32._GetParent(hWnd);

                Win32.RECT rcWnd = new Win32.RECT();
                Win32._GetWindowRect(hWnd, out rcWnd);

                LogInfo($"[{idx}] hWnd=0x{hWnd:X08}, hWndParent=0x{hWndParent:X08}, Text=[{windowText}], rcWnd=({rcWnd.Left},{rcWnd.Top}){rcWnd.Width}x{rcWnd.Height}");

                //Check if the Window is in current screen
                Screen screenOfhWnd = Screen.FromHandle(hWnd);
                if (screenOfhWnd == null)
                {
                    LogInfo($"    [{idx}] Abandon: GetScreen return null.");
                    continue;
                }
                if (!screenOfhWnd.Equals(screen))
                {
                    LogInfo($"    [{idx}] Abandon: Not in target screen.");
                    continue;
                }

                //Get the Process from hWnd
                Process process;
                string msg = "";
                if (!WinEventHook.GetProcessFromWindowHandle(hWnd, out process, out msg))
                {
                    //Fail to get the process
                    LogInfo($"    [{idx}] Abandon: GetProcessFromWindowHandle() err, {msg}");
                    continue;
                }

                //Try to get the PathName of the process
                string pathName = "";
                try
                {
                    if (process.MainModule != null)
                    {
                        if (!String.IsNullOrEmpty(process.MainModule.FileName))
                        {
                            pathName = process.MainModule.FileName;
                            LogInfo($"    [{idx}] PathName=[{pathName}]");
                        }
                    }
                }
                catch (Exception e1)
                {
                    msg = e1.Message;
                    LogInfo($"    [{idx}] Abandon: Get PathName from Procss causes exception, {msg}");
                }

                //Filter out DDPM processes
                if (ArrangeVM.IsEAExcludedPathName(pathName))
                {
                    LogInfo($"    [{idx}] Abandon: PathName is in Excluded List");
                    continue;
                }

                //Add Border to canvas
                Border border = new Border();
                //#E6AC28 = (230, 172, 40)
                border.BorderBrush = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(230, 172, 40));
                border.BorderThickness = new Thickness(6);
                border.CornerRadius = new CornerRadius(4);
                border.Width = rcWnd.Width / scale;
                border.Height = rcWnd.Height / scale;

                canvas.Children.Add(border);
                addCount++;
                //Convert screen coordinate to EditWindow
                System.Windows.Point ptWindow = PointFromScreen(new System.Windows.Point(rcWnd.Left, rcWnd.Top));

                double left = ptWindow.X;
                double top = ptWindow.Y;
                Canvas.SetLeft(border, left);
                Canvas.SetTop(border, top);

                settings.Add(left);
                settings.Add(top);
                settings.Add(border.Width);
                settings.Add(border.Height);
                LogInfo($"    [{idx}] Accept: Add a Border to EAEditWindow");
            }
            LogInfo($"  * Detected window count = [{addCount}]");
            settings[0] = addCount;
            inputSplitCtrl.Settings = settings;
        }

        //Reference: https://stackoverflow.com/questions/210504/enumerate-windows-like-alt-tab-does
        //Try to get the Windows like [Alt]+[Tab] key
        private bool IsTargetWindow(IntPtr hWnd, IntPtr lParam)
        {
            //1 The window must be visible
            if (!Win32._IsWindowVisible(hWnd))
                return false;

            //2 The window must not be a toolwindow
            uint winStyle = (uint)Win32._GetWindowLong(hWnd, (int)Win32.WindowLongFlags.GWL_EXSTYLE);
            if ((winStyle & (uint)Win32.WindowStylesEx.WS_EX_TOOLWINDOW) != 0)
            {
                return false;
            }

            if (Win32._GetAncestor(hWnd, Win32.eGaFlags.GA_ROOTOWNER) != hWnd)
            {
                return false;
            }

            uint cloaked;
            Win32._DwmGetWindowAttribute(hWnd, Win32.eDwmWindowAttribute.Cloaked, out cloaked, sizeof(uint));
            if (cloaked == Win32.DWM_CLOAKED_SHELL)
            {
                return false;
            }
            return true;
        }
        #endregion


        public bool IsVertical { get; set; } = false;
    }
}