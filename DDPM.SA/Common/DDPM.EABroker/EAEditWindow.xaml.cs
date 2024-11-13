using DDPM.Easy.Common;
using DDPM.SA.Common;
using DDPM.SA.Common.Display;
using DDPM.Win32Lib;
using Dell.Client.Framework.Common;
using nsWinEventHook;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
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
using static DDPM.Win32Lib.Win32;

namespace DDPM.EABroker
{
    /// <summary>
    /// Interaction logic for EAEditWindow.xaml
    /// </summary>
    public partial class EAEditWindow : Window
    {
        #region Private members
        private readonly ILog? _log;
        private string _orgFriendlyName = string.Empty;
        private List<CellJson> _cellJsons = new List<CellJson>();

        #endregion Private members

        #region ctor & Init
        public EAEditWindow(ILog? log)
        {
            InitializeComponent();
            _log = log;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //Hide window from Alt+tab
            System.Windows.Interop.WindowInteropHelper wndHelper = new System.Windows.Interop.WindowInteropHelper(this);
            Win32Lib.Win32.HideWinFromAltTab(wndHelper.Handle);
        }
        #endregion ctor & Init

        #region Hide
        public void Dispatcher_Hide()
        {
            this.Dispatcher.Invoke(() =>
            {
                //Robert_Lin, clear the previous editing SplitCtrl before Hode
                splitCtrl.Content = null;
                Hide();
            });
        }
        #endregion Close

        #region Events
        public event EventHandler<EAArgs> EditReturn;
        #endregion

        #region Log
        private void WriteLog(string msg, Exception? e=null)
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

        #region Input SplitCtrl


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
        public bool SetInputArgs(EAArgs args, Screen scr)
        {
            _inputArgs = args;

            this.Dispatcher.Invoke(() =>
            {
                //Try to create a ISplitCtrl to verify (cellCount,SplitKey) is valid
                ISplitCtrl? ispCtrl = ISplitCtrl.Create(args.CellCount, args.SplitKey);
                if (ispCtrl == null)
                {
                    //Invalidd CellCount+SplitKey, make the error messgae
                    WriteLog($"EAEditWindow.SetInputArg(), Invalid argument: {args.CellCount}{args.SplitKey}, [{SplitCtrlVM.Double_To_String(args.Settings)}]");
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

                Show();
            });
            return true;
        }

 
        public List<double> GetSettings()
        {
            return inputSplitCtrl.Settings;
        }

        public List<CellJson> GetCellJsons()
        {
            return _cellJsons;
        }

        #endregion Input SplitCtrl

        #region Show and Edit
        private ISplitCtrl inputSplitCtrl = new SplitCtrl0A();
        private EAArgs _inputArgs;
        public bool IsVertical { get; set; } = false;

        public bool ShowAndEdit(EAArgs args, Screen scr)
        {
            _inputArgs = args;
            if ((args.SplitJson.IsOverlapLayout))
            {
                this.Dispatcher.Invoke(() => { UI_ShowAndEdit_OverlapCustom(args, scr); });
            }
            else if (ISplitCtrl.IsExisted(args.SplitJson.CellCount, args.SplitJson.SplitKey))
            {
                this.Dispatcher.Invoke(() => { UI_ShowAndEdit_NonOverlapCustom(args, scr); });
            }
            else
            {
                return false;
            }
            return true;
        }

        private void UI_ShowAndEdit_NonOverlapCustom(EAArgs args, Screen scr)
        {
            //Try to create a ISplitCtrl to verify (cellCount,SplitKey) is valid
            ISplitCtrl? ispCtrl = ISplitCtrl.Create(args.SplitJson.CellCount, args.SplitJson.SplitKey);
            if (ispCtrl == null)
            {
                //Invalid CellCount+SplitKey, make the error message
                WriteLog($"EAEditWindow.SetInputArg(), Invalid argument: {args.CellCount}{args.SplitKey}, [{SplitCtrlVM.Double_To_String(args.Settings)}]");
                return;// false;
            }
            inputSplitCtrl = ispCtrl;
            inputSplitCtrl.IsEditable = true;
            inputSplitCtrl.SplitMode = eSplitModes.Edit;
            inputSplitCtrl.IsVertical = (scr.Bounds.Width < scr.Bounds.Height);

            if (args.SplitJson.Settings != null)
            {
                inputSplitCtrl.Settings = args.SplitJson.Settings;
            }

            _orgFriendlyName = args.SplitJson.CustomName;

            //Calculate the position/size of EditWindow
            double dpiX = 1.00;
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

            Show();
        }
        private void UI_ShowAndEdit_OverlapCustom(EAArgs args, Screen scr)
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
            settings.Add(screen.WorkingArea.Width);
            settings.Add(screen.WorkingArea.Height);

            double xRatio = 1.0000 / (double)screen.WorkingArea.Width;
            double yRatio = 1.0000 / (float)screen.WorkingArea.Height;

            //Enumerate all Window handle which will be fitered by IsTargetWindow()
            List<IntPtr> hWnds = Win32.GetWindowHandles(IsTargetWindow);
            WriteLog($"@ EAEditWindow.CaptureCustomLayout(), Enum candidate Window and add Borders");
            _cellJsons.Clear();
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

                WriteLog($"[{idx}] hWnd=0x{hWnd:X08}, hWndParent=0x{hWndParent:X08}, Text=[{windowText}], rcWnd=({rcWnd.Left},{rcWnd.Top}){rcWnd.Width}x{rcWnd.Height}");

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
                    if (process.MainModule != null)
                    {
                        if (!String.IsNullOrEmpty(process.MainModule.FileName))
                        {
                            pathName = process.MainModule.FileName;
                            WriteLog($"    [{idx}] PathName=[{pathName}]");
                        }
                    }
                }
                catch (Exception e1)
                {
                    msg = e1.Message;
                    WriteLog($"    [{idx}] Abandon: Get PathName from Procss causes exception, {msg}");
                }

                //Filter out DDPM processes
                if (ArrangeVM.IsEAExcludedPathName(pathName))
                {
                    WriteLog($"    [{idx}] Abandon: PathName is in Excluded List");
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

                //Convert and store in CellList, rcRatio
                //CellObj cellObj = new CellObj();
                //cellObj.Name = pathName;

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
                WriteLog($"    [{idx}] Accept: Add a Border to EAEditWindow");

                CellJson cellJson = new CellJson();
                cellJson.Name = $"0b{addCount}";
                cellJson.x = (double)left * xRatio;
                cellJson.y = (double)top * yRatio;
                cellJson.w = (double)border.Width * xRatio;
                cellJson.h = (double)border.Height * yRatio;
                _cellJsons.Add(cellJson);
            }
            WriteLog($"  * Detected window count = [{addCount}]");
            settings[0] = addCount;
            inputSplitCtrl.Settings = settings;
            inputSplitCtrl.CellList.Clear();


            SplitCtrl0B spCtrl0B = (SplitCtrl0B)inputSplitCtrl;
            ///spCtrl0B.RatioRects.Clear();
            foreach (CellJson cellJson in _cellJsons)
            {
                CellBorder cellBorder = new CellBorder();
                cellBorder.CellName = cellJson.Name;
                cellBorder.rcRatio = new Rect(cellJson.x, cellJson.y, cellJson.w, cellJson.h);
                inputSplitCtrl.CellBorders.Add(cellBorder);

                CellObj cellObj = new CellObj(cellJson.Name);
                cellObj.rcRatio = new Rect(cellJson.x, cellJson.y, cellJson.w, cellJson.h);
                inputSplitCtrl.CellList.Add(cellObj);

                ///spCtrl0B.RatioRects.Add(new Rect(cellJson.x, cellJson.y, cellJson.w, cellJson.h));
            }

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
            uint uiStyles = (uint) Win32._GetWindowLong(hWnd, (int)WindowLongFlags.GWL_STYLE);
            uint uiMinimizeStyle = (uint)Win32.WindowStyles.WS_MINIMIZE;
            bool isMinimized = ((uiStyles & uiMinimizeStyle) == uiMinimizeStyle);
            if (isMinimized)
                return false;

            //Check if the window across screen boundary
            //It need Screen rect, will be check after returned

            return true;
        }
        #endregion

    }
}
