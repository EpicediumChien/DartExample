using DDPM.Easy.Common;
using DDPM.SA.Common;
using DDPM.Win32Lib;
using Dell.Client.Framework.Common;
using nsWinEventHook;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using Rectangle = System.Drawing.Rectangle;

namespace DDPM.EABroker
{
    /// <summary>
    /// Interaction logic for OverlapWindow.xaml
    /// </summary>
    public partial class OverlapWindow : Window
    {
        #region Private members
        private double _screenScale = 1.000;
        private readonly ILog? _log;
        private int _autoCloseTimerMsec = 3000;
        private ISplitCtrl? _splitCtrl = null;
        private SplitCtrl0B spCtrl0B;        
        #endregion

        #region Events
        public EventHandler? CaptureDone = null;

        #endregion

        #region Output
        public ISplitCtrl? SplitCtrl => _splitCtrl;
        #endregion

        #region ctor
        public OverlapWindow(ILog log)
        {
            _log = log;
            InitializeComponent();
        }
        #endregion

        #region Init
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshScreenScale();

        }
        private double RefreshScreenScale()
        {
            //Robert_Lin, 2024-12-6, use the method in CommonFunctions
            _screenScale = CommonFunctions.GetDpiX();
            return _screenScale;
            //double dpiX = 1.000;
            //var dpiXProperty = typeof(SystemParameters).GetProperty("DpiX", BindingFlags.NonPublic | BindingFlags.Static);
            //if (dpiXProperty != null)
            //{
            //    var varX = (int)dpiXProperty.GetValue(null, null);
            //    dpiX = (double)varX / (double)96;
            //}
            //_screenScale = dpiX;
            //return dpiX;
        }
        #endregion

        #region Log
        private void WriteLog(string msg, Exception? e = null)
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
        #endregion Log

        #region Capture Overlap Layout
        /// <summary>
        /// Capture the overlap layouts on the target screen
        /// </summary>
        /// <param name="scr"></param>
        /// <returns>The count of window frame (CellBorder) are captured.</returns>
        public int CaptureOverlapLayout(Screen scr)
        {
            Left = scr.Bounds.Left / _screenScale;
            Top = scr.Bounds.Top / _screenScale;
            Width = scr.Bounds.Width / _screenScale;
            Height = scr.Bounds.Height / _screenScale;

            int addCount = 0;
            try
            {
                addCount = CaptureCustomLayout_v1(scr);

                _ = Task.Delay(_autoCloseTimerMsec).ContinueWith(t => this.Dispatcher.Invoke(OnCaptureDone));
            }
            catch (Exception e)
            {
                WriteLog($"[OverlapWindow] catch exception[{e.Message}] when run CaptureOverlapLayout");
            }
            
            return addCount;
        }

        public int CaptureOverlapLayoutByWorkingArea(Rectangle workingArea)
        {
            Left = workingArea.Left / _screenScale;
            Top = workingArea.Top / _screenScale;
            Width = workingArea.Width / _screenScale;
            Height = workingArea.Height / _screenScale;

            int addCount = 0;
            try
            {
                addCount = CaptureCustomLayout_v2(workingArea);

                Task.Delay(_autoCloseTimerMsec).ContinueWith(t => this.Dispatcher.Invoke(OnCaptureDone));
            }
            catch (Exception e)
            {
                WriteLog($"[OverlapWindow] catch exception[{e.Message}] when run CaptureOverlapLayoutByWorkingArea");
            }
            
            return addCount;

        }

        /// <summary>
        /// Enumerate all Windows as [Alt]+[Tab], filt out ExcludeList, not in target screen,...
        /// Create CellBorders and add to SplitCtrl, and show it on the target screen
        /// </summary>
        /// <param name="screen"></param>
        /// <returns>The count of CellBorders</returns>
        private int CaptureCustomLayout_v1(Screen screen)
        {
            //Robert_Lin, 2024-12-6, use the method in CommonFunctions
            double scale = CommonFunctions.GetDpiX();
            //double scale = 1.000;
            //var dpiXProperty = typeof(SystemParameters).GetProperty("DpiX", BindingFlags.NonPublic | BindingFlags.Static);
            //if (dpiXProperty != null)
            //{
            //    var varX = (int)dpiXProperty.GetValue(null, null);
            //    scale = (double)varX / (double)96;
            //}

            //Clear CustomLayouts
            //canvas.Children.Clear();

            //Prepare for Settings
            //Format:
            // First 4 elements [0]~[3]: BorderCount | ScreenScale | ScreenBoundsWidth | ScreenBoundsHeight
            // Later: BorderRect (left, top, width, height), (left, top, width, height), ...
            List<double> settings = new List<double>();
            settings.Add(0); //BorderCount will be updated later
            settings.Add(scale);
            settings.Add(screen.WorkingArea.Width);
            settings.Add(screen.WorkingArea.Height);

            double xRatio = 1.0000 / (double)screen.WorkingArea.Width;
            double yRatio = 1.0000 / (float)screen.WorkingArea.Height;

            //Enumerate all Window handle which will be fitered by IsTargetWindow()
            List<IntPtr> hWnds = Win32.GetWindowHandles(IsTargetWindow);
            Rectangle rcScreen = screen.WorkingArea;
            WriteLog($"@ EAEditWindow.CaptureCustomLayout(screen), Screen.WorkingArea=({rcScreen.Left},{rcScreen.Top}) {rcScreen.Width}x{rcScreen.Height}");

            //_cellJsons.Clear();
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

                //Check if hWnd is in the target screen
                //
                //Get the Window Rect
                Win32.RECT rcWnd = new Win32.RECT();
                Win32._GetWindowRect(hWnd, out rcWnd);
                WriteLog($"  #{idx}[{windowText}] ({rcWnd.Left},{rcWnd.Top}){rcWnd.Width}x{rcWnd.Height}");
                //Convert RECT to Rectangle
                Rectangle rectWnd = new Rectangle(rcWnd.Left, rcWnd.Top, rcWnd.Width, rcWnd.Height);
                //Get the intersection of screen and window
                Rectangle rectIntersect = Rectangle.Intersect(rcScreen, rectWnd);
                //If they has not any intersection, then skip this window
                if (rectIntersect.IsEmpty)
                {
                    WriteLog($"    #{idx}[{windowText}] Abandon: Not in target screen.");
                    continue;
                }
                //Calculate intersection ratio
                double areaWindow = rectWnd.Width * rectWnd.Height;
                double areaIntersect = rectIntersect.Width * rectIntersect.Height;
                if (areaWindow <= 0.000)
                {
                    WriteLog($"    #{idx} Abandon: Window size is zero.");
                    continue;
                }
                double ratioIntersec = areaIntersect / areaWindow;
                WriteLog($"    #{idx} Intersection {rectIntersect.Width}x{rectIntersect.Height}, ratio={ratioIntersec}");
                if (rectIntersect.Width < 50.000)
                {
                    WriteLog($"    #{idx} Abandon: Intersec Width too small.");
                    continue;
                }
                if (rectIntersect.Height < 50.000)
                {
                    WriteLog($"    #{idx} Abandon: Intersec Height too small.");
                    continue;
                }
                if (ratioIntersec < 0.001)
                {
                    WriteLog($"    #{idx} Abandon: Intersec Ratio too small.");
                    continue;
                }
                //WriteLog($"[{idx}] hWnd=0x{hWnd:X08}, hWndParent=0x{hWndParent:X08}, Text=[{windowText}], rcWnd=({rcWnd.Left},{rcWnd.Top}){rcWnd.Width}x{rcWnd.Height}");

                //OLD: Before 2025-3-4
                /*
                //Check if the Window is in current screen
                Screen screenOfhWnd = Screen.FromHandle(hWnd);
                if (screenOfhWnd == null)
                {
                    WriteLog($"    [{idx}] Abandon: GetScreen return null.");
                    continue;
                }
                if (!screenOfhWnd.Equals(screen))
                {
                    WriteLog($"    [{idx}] Abandon: Not in target screen.");
                    continue;
                }

                //Check if the window is totally inside screen
                if (!screen.Bounds.Contains(rcWnd))
                {
                    WriteLog($"    [{idx}] Abandon: Not inside target screen (no acroess).");
                    continue;
                }
                */

                //Get the Process from hWnd
                Process process;
                string msg = "";
                if (!WinEventHook.GetProcessFromWindowHandle(hWnd, out process, out msg))
                {
                    //Fail to get the process
                    WriteLog($"    [{idx}] Abandon: GetProcessFromWindowHandle() err, {msg}");
                    continue;
                }

                //Try to get the PathName of the process
                string pathName = "";
                try
                {
                    if (process.MainModule != null && !String.IsNullOrEmpty(process.MainModule.FileName))
                    {
                        pathName = process.MainModule.FileName;
                        WriteLog($"    [{idx}] PathName=[{pathName}]");
                    }
                }
                catch (Exception e1)
                {
                    msg = e1.Message;
                    WriteLog($"    [{idx}] Abandon: Get PathName from Process causes exception, {msg}");
                }

                //Filter out DDPM processes
                if (ArrangeVM.IsEAExcludedPathName(pathName))
                {
                    WriteLog($"    [{idx}] Abandon: PathName is in Excluded List");
                    continue;
                }

                //Trim the partion out of screen
                if (rcWnd.Right > rcScreen.Right)
                    rcWnd.Right = rcScreen.Right;
                if (rcWnd.Bottom > rcScreen.Bottom)
                    rcWnd.Bottom = rcScreen.Bottom;
                if (rcWnd.Left < rcScreen.Left)
                    rcWnd.Left = rcScreen.Left;
                if (rcWnd.Top < rcScreen.Top)
                    rcWnd.Top = rcScreen.Top;

                double width = rcWnd.Width / scale;
                double height = rcWnd.Height / scale;



                //Add Border to canvas
                //Border border = new Border();
                ////#E6AC28 = (230, 172, 40)
                //border.BorderBrush = new System.Windows.Media.SolidColorBrush(
                //    System.Windows.Media.Color.FromRgb(230, 172, 40));
                //border.BorderThickness = new Thickness(6);
                //border.CornerRadius = new CornerRadius(4);
                //border.Width = rcWnd.Width / scale;
                //border.Height = rcWnd.Height / scale;

                //Convert and store in CellList, rcRatio
                //CellObj cellObj = new CellObj();
                //cellObj.Name = pathName;

                //canvas.Children.Add(border);
                addCount++;
                //Convert screen coordinate to EditWindow
                System.Windows.Point ptWindow = PointFromScreen(new System.Windows.Point(rcWnd.Left, rcWnd.Top));


                Win32.RECT lpRect = new Win32.RECT();
                Win32.RECT rect2 = rcWnd;
                Win32.RECT pvAttribute = new Win32.RECT();

                Win32._GetWindowRect(hWnd, out lpRect);

                if (Win32._DwmGetWindowAttribute(hWnd, Win32.eDwmWindowAttribute.ExtendedFrameBounds, ref pvAttribute,
                    Marshal.SizeOf(typeof(Win32.RECT))) == 0)
                {
                    double num = pvAttribute.Left - lpRect.Left;
                    double num2 = pvAttribute.Right - lpRect.Right;
                    double num3 = pvAttribute.Bottom - lpRect.Bottom;
                    rect2.Left -= (int)num;
                    rect2.Right -= (int)num2;
                    rect2.Bottom -= (int)num3;
                }

                nint wndLong = Win32._GetWindowLong(hWnd, (int)Win32.WindowLongFlags.GWL_STYLE);
                if ((wndLong & 0x40000L) == 0)
                {
                    rect2.Right = rect2.Left + (lpRect.Right - lpRect.Left);
                    rect2.Bottom = rect2.Top + (lpRect.Bottom - lpRect.Top);
                }

                double left = ptWindow.X;
                double top = ptWindow.Y;
                //Canvas.SetLeft(border, left);
                //Canvas.SetTop(border, top);

                //Reserve in screen area
                //double r = left + width;
                //double b = top + height;
                //if (r > rcScreen.Right)
                //    r = rcScreen.Right;
                //if (b > rcScreen.Bottom)
                //    b = rcScreen.Bottom;

                //if (left < rcScreen.Left)
                //    left = rcScreen.Left;
                //if (top < rcScreen.Top)
                //    top = rcScreen.Top;

                //width = r - left;
                //height = b - top;

                settings.Add(left);
                settings.Add(top);
                settings.Add(width);
                settings.Add(height);
                WriteLog($"    [{idx}] Accept: Add a Border ({left},{top}){width}x{height} to EAEditWindow");

                //CellJson cellJson = new CellJson();
                //cellJson.Name = $"0b{addCount}";
                //cellJson.x = (double)left * xRatio;
                //cellJson.y = (double)top * yRatio;
                //cellJson.w = (double)border.Width * xRatio;
                //cellJson.h = (double)border.Height * yRatio;
                //_cellJsons.Add(cellJson);

            } //foreach (IntPtr hWnd in hWnds)
            WriteLog($"  * Detected window count = [{addCount}]");
            settings[0] = addCount;
            //inputSplitCtrl.Settings = settings;
            //inputSplitCtrl.CellList.Clear();


            spCtrl0B = new SplitCtrl0B();
            _splitCtrl = spCtrl0B;
            _splitCtrl.Settings = settings;
            _splitCtrl.SplitMode = eSplitModes.Edit;
            Rect rcView = new Rect(screen.WorkingArea.Left, screen.WorkingArea.Top, screen.WorkingArea.Width, screen.WorkingArea.Height);
            spCtrl0B.ApplySettingsToCellList(rcView);

            content.Content = spCtrl0B.UC;

            //foreach (CellJson cellJson in _cellJsons)
            //{
            //    CellBorder cellBorder = new CellBorder();
            //    cellBorder.CellName = cellJson.Name;
            //    cellBorder.rcRatio = new Rect(cellJson.x, cellJson.y, cellJson.w, cellJson.h);
            //    inputSplitCtrl.CellBorders.Add(cellBorder);

            //    CellObj cellObj = new CellObj(cellJson.Name);
            //    cellObj.rcRatio = new Rect(cellJson.x, cellJson.y, cellJson.w, cellJson.h);
            //    inputSplitCtrl.CellList.Add(cellObj);

            //    ///spCtrl0B.RatioRects.Add(new Rect(cellJson.x, cellJson.y, cellJson.w, cellJson.h));
            //}

            ////Set a timeer to finished edit process
            //System.Threading.Timer timer1 = new System.Threading.Timer((obj) =>
            //{
            //    if (EditReturn != null)
            //        EditReturn(this, _inputArgs);
            //    //Hide();
            //}, null, 3000, Timeout.Infinite);

            return addCount;
        }

        private int CaptureCustomLayout_v2(Rectangle workingArea)
        {
            //Robert_Lin, 2024-12-6, use the method in CommonFunctions
            double scale = CommonFunctions.GetDpiX();
            //double scale = 1.000;
            //var dpiXProperty = typeof(SystemParameters).GetProperty("DpiX", BindingFlags.NonPublic | BindingFlags.Static);
            //if (dpiXProperty != null)
            //{
            //    var varX = (int)dpiXProperty.GetValue(null, null);
            //    scale = (double)varX / (double)96;
            //}

            //Prepare for Settings
            //Format:
            // First 4 elements [0]~[3]: BorderCount | ScreenScale | ScreenBoundsWidth | ScreenBoundsHeight
            // Later: BorderRect (left, top, width, height), (left, top, width, height), ...
            List<double> settings = new List<double>();
            settings.Add(0); //BorderCount will be updated later
            settings.Add(scale);
            settings.Add(workingArea.Width);
            settings.Add(workingArea.Height);

            double xRatio = 1.0000 / (double)workingArea.Width;
            double yRatio = 1.0000 / (float)workingArea.Height;

            //Enumerate all Window handle which will be fitered by IsTargetWindow()
            List<IntPtr> hWnds = Win32.GetWindowHandles(IsTargetWindow);
            WriteLog($"@ EAEditWindow.CaptureCustomLayout(), Enum candidate Window and add Borders");
            //_cellJsons.Clear();
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

                //WriteLog($"[{idx}] hWnd=0x{hWnd:X08}, hWndParent=0x{hWndParent:X08}, Text=[{windowText}], rcWnd=({rcWnd.Left},{rcWnd.Top}){rcWnd.Width}x{rcWnd.Height}");

                //Check if the Window is in current screen
                //Screen screenOfhWnd = Screen.FromHandle(hWnd);
                //if (screenOfhWnd == null)
                //{
                //    WriteLog($"    [{idx}] Abandon: GetScreen return null.");
                //    continue;
                //}
                //if (!screenOfhWnd.Equals(screen))
                //{
                //    WriteLog($"    [{idx}] Abandon: Not in target screen.");
                //    continue;
                //}

                ////Check if the window is totally inside screen
                //if (!screen.Bounds.Contains(rcWnd))
                //{
                //    WriteLog($"    [{idx}] Abandon: Not inside target screen (no acroess).");
                //    continue;
                //}

                //Get the Process from hWnd
                Process process;
                string msg = "";
                if (!WinEventHook.GetProcessFromWindowHandle(hWnd, out process, out msg))
                {
                    //Fail to get the process
                    WriteLog($"    [{idx}] Abandon: GetProcessFromWindowHandle() err, {msg}");
                    continue;
                }

                //Try to get the PathName of the process
                string pathName = "";
                try
                {
                    if (process.MainModule != null && !String.IsNullOrEmpty(process.MainModule.FileName))
                    {
                        pathName = process.MainModule.FileName;
                        //WriteLog($"    [{idx}] PathName=[{pathName}]");                        
                    }
                }
                catch (Exception e1)
                {
                    msg = e1.Message;
                    WriteLog($"    [{idx}] Abandon: Get PathName from Procss causes exception, {msg}");
                    //Robert_Lin 2025-3-19 add the missing continue
                    continue;
                }

                //Filter out DDPM processes
                if (ArrangeVM.IsEAExcludedPathName(pathName))
                {
                    WriteLog($"    [{idx}] Abandon: PathName is in Excluded List");
                    continue;
                }

                //Check if the window is totally inside screen
                Rectangle rectWnd = new Rectangle(rcWnd.Left, rcWnd.Top, rcWnd.Width, rcWnd.Height);
                if (workingArea.Contains(rectWnd))
                {
                    WriteLog($"    [{idx}] Abandon: rcWnd is not inside workingArea.");
                    continue;
                }

                double width = rcWnd.Width / scale;
                double height = rcWnd.Height / scale;

                //Add Border to canvas
                //Border border = new Border();
                ////#E6AC28 = (230, 172, 40)
                //border.BorderBrush = new System.Windows.Media.SolidColorBrush(
                //    System.Windows.Media.Color.FromRgb(230, 172, 40));
                //border.BorderThickness = new Thickness(6);
                //border.CornerRadius = new CornerRadius(4);
                //border.Width = rcWnd.Width / scale;
                //border.Height = rcWnd.Height / scale;

                //Convert and store in CellList, rcRatio
                //CellObj cellObj = new CellObj();
                //cellObj.Name = pathName;

                //canvas.Children.Add(border);
                addCount++;
                //Convert screen coordinate to EditWindow
                System.Windows.Point ptWindow = PointFromScreen(new System.Windows.Point(rcWnd.Left, rcWnd.Top));

                double left = ptWindow.X;
                double top = ptWindow.Y;
                //Canvas.SetLeft(border, left);
                //Canvas.SetTop(border, top);

                settings.Add(left);
                settings.Add(top);
                settings.Add(width);
                settings.Add(height);
                WriteLog($"    [{idx}] Accept: Add a Border to EAEditWindow");

                //CellJson cellJson = new CellJson();
                //cellJson.Name = $"0b{addCount}";
                //cellJson.x = (double)left * xRatio;
                //cellJson.y = (double)top * yRatio;
                //cellJson.w = (double)border.Width * xRatio;
                //cellJson.h = (double)border.Height * yRatio;
                //_cellJsons.Add(cellJson);

            } //foreach (IntPtr hWnd in hWnds)
            WriteLog($"  * Detected window count = [{addCount}]");
            settings[0] = addCount;
            //inputSplitCtrl.Settings = settings;
            //inputSplitCtrl.CellList.Clear();


            spCtrl0B = new SplitCtrl0B();
            _splitCtrl = spCtrl0B;
            _splitCtrl.Settings = settings;
            Rect rcView = new Rect(workingArea.Left, workingArea.Top, workingArea.Width, workingArea.Height);
            spCtrl0B.ApplySettingsToCellList(rcView);

            content.Content = spCtrl0B.UC;

            //foreach (CellJson cellJson in _cellJsons)
            //{
            //    CellBorder cellBorder = new CellBorder();
            //    cellBorder.CellName = cellJson.Name;
            //    cellBorder.rcRatio = new Rect(cellJson.x, cellJson.y, cellJson.w, cellJson.h);
            //    inputSplitCtrl.CellBorders.Add(cellBorder);

            //    CellObj cellObj = new CellObj(cellJson.Name);
            //    cellObj.rcRatio = new Rect(cellJson.x, cellJson.y, cellJson.w, cellJson.h);
            //    inputSplitCtrl.CellList.Add(cellObj);

            //    ///spCtrl0B.RatioRects.Add(new Rect(cellJson.x, cellJson.y, cellJson.w, cellJson.h));
            //}

            ////Set a timeer to finished edit process
            //System.Threading.Timer timer1 = new System.Threading.Timer((obj) =>
            //{
            //    if (EditReturn != null)
            //        EditReturn(this, _inputArgs);
            //    //Hide();
            //}, null, 3000, Timeout.Infinite);

            return addCount;
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

            //Check if the window is minimized
            uint uiStyles = (uint)Win32._GetWindowLong(hWnd, (int)Win32.WindowLongFlags.GWL_STYLE);
            uint uiMinimizeStyle = (uint)Win32.WindowStyles.WS_MINIMIZE;
            bool isMinimized = ((uiStyles & uiMinimizeStyle) == uiMinimizeStyle);
            if (isMinimized)
                return false;

            //Check if the window across screen boundary
            //It need Screen rect, will be check after returned

            return true;
        }
        #endregion

        #region Capture Done
        private void OnCaptureDone()
        {
            CaptureDone?.Invoke(this, new EventArgs());
            Close();
        }
        #endregion

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //if (System.Windows.Threading.Dispatcher.CurrentDispatcher != null)
            //{
            //     System.Windows.Threading.Dispatcher.CurrentDispatcher.InvokeShutdown();
            //}

            spCtrl0B?.InitCellList();
            content.Content = null;
        }
    }
}
