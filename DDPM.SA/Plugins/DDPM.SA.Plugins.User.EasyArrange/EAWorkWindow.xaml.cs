using DDPM.Easy.Common;
using DDPM.SA.Common.Settings;
using Dell.Client.Framework.Common;
using nsWinEventHook;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using VcpCore.Common;

namespace DDPM.SA.Plugins.User.EasyArrange
{
    /// <summary>
    /// Interaction logic for EAWorkWindow.xaml
    /// </summary>
    public partial class EAWorkWindow : Window
    {
        #region Private members

        private ArrangeVM VM;
        private MonitorInfo _attachedMonitor;
        private Screen _screen;
        private readonly List<MonitorInfo> _monitors;

        //Per-monitor settings
        private bool _isWidthoutGap = true;
        private bool _isOnlyAllowWhenShiftKeyPressed = false;
        private bool _IsAwsEnabled = false;
        #endregion Private members

        #region ctor

        public EAWorkWindow(ArrangeVM vm, Screen scr, List<MonitorInfo> monitors)
        {
            InitializeComponent();
            VM = vm;
            _screen = scr;
            _monitors = monitors;
            DataContext = vm;
            VM.IsMovingChanged += VM_IsMovingChanged;
        }
        public EAWorkWindow(ArrangeVM vm)
        {
            InitializeComponent();
            VM = vm;
            DataContext = vm;
            VM.IsMovingChanged += VM_IsMovingChanged;
            Left = -99999;
            Top = -99999;
            Width = 10;
            Height = 10;
        }

        private void VM_IsMovingChanged(object? sender, bool e)
        {
        }

        #endregion ctor

        #region Init

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //Hide window from Alt+tab
            System.Windows.Interop.WindowInteropHelper wndHelper = new System.Windows.Interop.WindowInteropHelper(this);
            Win32Lib.Win32.HideWinFromAltTab(wndHelper.Handle);
        }

        #endregion Init

        #region [Input] Screen
        public void SetScreen(Screen screen)
        {
            _screen = screen;
            IsVertical = (_screen.Bounds.Width < _screen.Bounds.Height);

        }
        public string ScreenDeviceName
        {
            get
            {
                if (_screen == null) return "";
                return _screen.DeviceName;
            }
        }
        #endregion [Input] Screen

        #region [Input] AttachedMonitor
        /// <summary>
        /// Used to reload settings
        /// </summary>
        public MonitorInfo AttachedMonitor
        {
            get => _attachedMonitor;
            set
            {
                _attachedMonitor = value; 
            }
        }
        #endregion

        #region [Input] Working SplitCtrl

        private ISplitCtrl? _workingSplit = null;
        private bool _isSplitCtrl0A = false;

        public bool SetWorkingSplit(int cellCount, char splitKey, List<double>? settings = null)
        {
            this.Dispatcher.Invoke(() =>
            {
                //canvasWorker.Children.Clear();
                //canvasFader.Children.Clear();

                if ((cellCount == 0) && (splitKey == 'A'))
                {
                    _workingSplit = null;
                    splitCtrl.Content = null;
                    fadeOutCtrl.Content = null;
                    return;
                }

                if ((cellCount == 0) && (splitKey == 'B'))
                {
                    SplitCtrl0B sp0B = new SplitCtrl0B();
                    _workingSplit = sp0B;
                    _workingSplit.SplitMode = eSplitModes.Work;

                    if (settings == null)
                        _workingSplit.Settings = new List<double>();
                    else
                        _workingSplit.Settings = new List<double>(settings);

                    sp0B.ApplySettingsToCellList(_screen.Bounds);

                    _workingSplit.IsEditable = false;
                    _workingSplit.IsVertical = IsVertical;
                    splitCtrl.Content = _workingSplit;
                }
                else
                {
                    _workingSplit = ISplitCtrl.Create(cellCount, splitKey);
                }
                if (_workingSplit != null)
                {
                    _workingSplit.SplitMode = eSplitModes.Work;
                    if (settings == null)
                        _workingSplit.Settings = new List<double>();
                    else
                        _workingSplit.Settings = new List<double>(settings);
                    _workingSplit.IsEditable = false;
                    _workingSplit.IsVertical = IsVertical;
                    splitCtrl.Content = _workingSplit;
                }
                else
                {
                    splitCtrl.Content = null;
                }
                ISplitCtrl? fadeSplit = ISplitCtrl.Create(cellCount, splitKey);
                if (fadeSplit != null)
                {
                    fadeSplit.SplitMode = eSplitModes.Work;
                    if (settings == null)
                        fadeSplit.Settings = new List<double>();
                    else
                        fadeSplit.Settings = new List<double>(settings);

                    if ((cellCount == 0) && (splitKey == 'B'))
                    {
                        SplitCtrl0B sp0b = fadeSplit as SplitCtrl0B;
                        sp0b.ApplySettingsToCellList(_screen.Bounds);
                    }
                    fadeSplit.IsEditable = false;
                    fadeSplit.IsVertical = IsVertical;
                    fadeOutCtrl.Content = fadeSplit;
                }
                else
                {
                    fadeOutCtrl.Content = null;
                }

                InvokeFadeOutAnimation();
            });

            return true;
        }

        private bool BuildAddedCustomLayout(List<double>? settings = null)
        {
            return false;
            //if (settings == null)
            //    return false;
            //if (settings.Count == 0) 
            //    return false;
            //if (_screen == null)
            //    return false;
            
            ////settings[0] is BorderCount
            //int borderCount = (int)settings[0];
            //if (borderCount <= 0)
            //    return false;

            ////Check the settings.Count should be (borderCount*4 + 4)
            //if (settings.Count != ((borderCount + 1) * 4))
            //    return false;

            ////settings[1] is screenScale
            //double orgScreenScale = settings[1];
            ////settings[1] is screenWidth
            //double orgWidth = settings[2];
            ////settings[2] is screenHeight
            //double orgHeight = settings[3];

            //for (int idx = 0; idx < borderCount; idx++)
            //{
            //    Border border = new Border();
            //}
        }

        #endregion [Input] Working SplitCtrl

        #region [Input] UI Settings
        public bool IsOnlyAllowWhenShiftKeyPressed
        {
            get => _isOnlyAllowWhenShiftKeyPressed;
            set
            {
                _isOnlyAllowWhenShiftKeyPressed = value;
            }
        }
        #endregion

        #region WorkWindow Runtime Infos

        private string _displayName = ""; //Expected example: "DISPLAY3"
        private string _monitorNames = ""; //Expected example: "U2427QE,U3427E(2)"

        //Internal used to identify a WorkWindow
        public string WindowName
        {
            get
            {
                if (String.IsNullOrEmpty(_displayName))
                {
                    if (_screen != null)
                    {
                        _displayName = _screen.DeviceName.Replace("\\", "");
                        _displayName = _displayName.Replace(".", "");
                    }
                }
                if (String.IsNullOrEmpty(_monitorNames))
                {
                    if (_monitors != null)
                    {
                        List<string> monitorNames = new List<string>();
                        foreach (MonitorInfo mi in _monitors)
                        {
                            monitorNames.Add(mi.modelName);
                        }
                        _monitorNames = String.Join(",", monitorNames);
                    }
                }
                string splitClass = "";
                if (_workingSplit != null)
                {
                    splitClass = _workingSplit.CtrlClass;
                }
                return $"{_displayName}@({_monitorNames})-{splitClass}";
            }
        }

        #endregion WorkWindow Runtime Infos

        #region ILog for EAPlugin

        public ILog Log { get; set; }

        #endregion ILog for EAPlugin

        #region Cell List

        /// <summary>
        /// Return the CellList infor with Json format, example:
        /// [{"cell1":(x1,y1)-(x2,y2)width*height},{"cell1":(x1,y1)-(x2,y2)width*height},...]
        /// </summary>
        public string CellListJson
        {
            get
            {
                if (_workingSplit == null) return "[]";

                string outString = "[";
                int idx = 0;
                foreach (CellObj objCell in _workingSplit.CellList)
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
        }

        public void Invoke_RefreshCellRects()
        {
            this.Dispatcher.Invoke(() =>
            {
                RefreshCellRects();
            });
        }

        public void RefreshCellRects()
        {
            if (_workingSplit == null)
                return;

            this.Dispatcher.Invoke(() =>
            {
                //DpiScale dpiScale = VisualTreeHelper.GetDpi(this);
                //double scale = dpiScale.PixelsPerDip;
                //VM.ScreenScale = scale;
                //Trace.WriteLine($"WorkWin.Scale={scale}");

                foreach (CellObj objCell in _workingSplit.CellList)
                {
                    if (objCell.bd == null)
                        continue;

                    objCell.rc = GetBorderRect(objCell.bd);

                    Trace.WriteLine($"Cell({objCell.Name})={ArrangeVM.FormatRect(objCell.rc)}");
                }
            });
        }

        private Rect GetBorderRect(Border ctrl)
        {
            if (ctrl == null)
                return Rect.Empty;

            if ((ctrl.ActualWidth == 0) && (ctrl.ActualHeight == 0))
                return Rect.Empty;

            PresentationSource preSrc = PresentationSource.FromVisual(ctrl);
            if (preSrc == null)
                return Rect.Empty;

            System.Windows.Point ptTopLeft = ctrl.PointToScreen(new System.Windows.Point(0, 0));
            double w = ctrl.ActualWidth * VM.ScreenScale;
            double h = ctrl.ActualHeight * VM.ScreenScale;
            Trace.WriteLine($"ctrlActual={ctrl.ActualWidth}x{ctrl.ActualHeight}; Scale={VM.ScreenScale} => {w}x{h}");
            return new Rect(ptTopLeft.X, ptTopLeft.Y, w, h);
        }

        private void TraceCellRects()
        {
            if (_workingSplit == null) return;
            foreach (CellObj objCell in _workingSplit.CellList)
            {
                Trace.WriteLine($"Cell({objCell.Name})={ArrangeVM.FormatRect(objCell.rc)}");
            }
        }

        #endregion Cell List

        #region DetermineHoveringCell

        public CellObj? DetermineHoveringCellObj(int x, int y)
        {
            if (_workingSplit == null)
                return null;
            //if (!IsShown)
            //    return null;

            DpiScale dpiScale = VisualTreeHelper.GetDpi(this);
            double scale = dpiScale.PixelsPerDip;

            _workingSplit.HoveringCell = "";

            //For AddedCustomLayout
            if (_workingSplit.IsAddedCustomLayout)
            {
                CellObj? hoverCell = null;
                foreach (CellObj objCell in _workingSplit.CellList)
                {
                    string tag = "N";
                    if (hoverCell == null)
                    {
                        if (objCell.rc.Contains(x, y))
                        {
                            hoverCell = objCell;
                            tag = "H";
                            _workingSplit.HoveringCell = objCell.Name;
                        }
                    }

                    this.Dispatcher.Invoke(() =>
                    {
                        objCell.bd.Tag = tag;
                    });

                }
                return hoverCell; ;
            }

            //For other layouts
            foreach (CellObj objCell in _workingSplit.CellList)
            {
                if (objCell.rc.Contains(x, y))
                {
                    _workingSplit.HoveringCell = objCell.Name;
                    return objCell;
                }
             }

            return null;
        }

        #endregion DetermineHoveringCell

        #region Window Status Change

        private void Window_Activated(object sender, EventArgs e)
        {
            Trace.WriteLine($"WorkWin[{WindowName}] Activated");
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            Trace.WriteLine($"WorkWin[{WindowName}] ContentRendered");
            RefreshCellRects();
        }

        private void Window_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            object newValue = e.NewValue;
            string newString = "";
            if (newValue != null)
                newString = newValue.ToString();

            Trace.WriteLine($"WorkWin[{WindowName}] IsVisibleChanged => {newString}");
        }

        #endregion Window Status Change

        #region Dispatcher Invokes

        public void ChangeWindowPos(double left, double top, double width = 0, double height = 0)
        {
            this.Dispatcher.Invoke(() =>
            {
                Left = left;
                Top = top;
                if (width != 0)
                    Width = width;
                if (height != 0)
                    Height = height;
                IsVertical = (Width < Height);

                if (_workingSplit != null)
                {
                    _workingSplit.IsVertical = IsVertical;
                }
            });
        }

        public void DispatcherClose()
        {
            this.Dispatcher.Invoke(() =>
            {
                Close();
            });
        }

        #endregion Dispatcher Invokes

        #region FadeOut Storyboard

        public bool IsFading
        {
            get { return (bool)GetValue(IsFadingProperty); }
            set
            {
                SetValue(IsFadingProperty, value);
            }
        }

        // Using a DependencyProperty as the backing store for IsFading.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsFadingProperty =
            DependencyProperty.Register("IsFading", typeof(bool), typeof(EAEditWindow), new PropertyMetadata(false));

        private void InvokeFadeOutAnimation()
        {
            //this.Dispatcher.Invoke(() =>
            // {
            Storyboard? sb = Resources["FadeOut"] as Storyboard;
            if (sb == null)
                return;

            sb.Completed += (o, s) =>
            {
                VM.RefreshCellRects();
                this.IsFading = false;
                fadeOutGrid.Visibility = Visibility.Collapsed;
                //Visibility = Visibility.Hidden;
                //gridSplitCtrl.Opacity = 1;
            };

            IsFading = true;
            fadeOutGrid.Visibility = Visibility.Visible;
            //gridSplitCtrl.Opacity = 1;
            sb.Begin();
            // });
        }

        public void StopFadeOutAnimation()
        {
            this.Dispatcher.Invoke(() =>
            {
                Storyboard? sb = Resources["FadeOut"] as Storyboard;
                if (sb == null)
                    return;
                sb.Stop();
                IsFading = false;
                //gridSplitCtrl.Opacity = 1;
            });
        }

        #endregion FadeOut Storyboard

        #region Screen Orientation

        private bool isVertical = false;

        public bool IsVertical
        {
            get { return isVertical; }
            set 
            {
                isVertical = value; 
                if (_workingSplit != null)
                {
                    _workingSplit.IsVertical = value;
                }
            }
        }

        #endregion Screen Orientation

        #region Used by ArrangeVM
        public bool IsUsed { get; set; } = false;
        #endregion

        //public bool IsShown { get; set; } = false;
        //public void DetermineWindowVisibility()
        //{
            //this.Dispatcher.Invoke(() =>
            //{
            //    if ((VM.IsMoving) && (VM.IsWorkUIEnabled))
            //    {
            //        bool isShiftPressed = WinEventHook.IsShiftPressed();

            //        //IsOnlyAllowWhenShiftKeyPressed | isShiftPressed | Show UI?
            //        //              N                   (Don't care)       Yes
            //        //              Y                        Y             Yes
            //        //              Y                        N              No
            //        bool isUiShow = true;

            //        if (VM.EzSettings.IsOnlyAllowWhenShiftKeyPressed && (!isShiftPressed))
            //        {
            //            isUiShow = false;
            //        }
            //        if (isUiShow)
            //        {
            //            splitGrid.Visibility = Visibility.Visible;
            //            Topmost = true;
            //            IsShown = true;
            //            //this.Visibility = Visibility.Visible;
            //            Trace.Write($"Screen[{ScreenDeviceName}] Show");
            //            return;
            //        }
            //    }
            //    splitGrid.Visibility = Visibility.Hidden;
            //    IsShown = false;
            //    //this.Visibility = Visibility.Hidden;
            //    Trace.Write($"Screen[{ScreenDeviceName}] Hide");
            //});
//        }


        //public bool ReloadMonitorSettings()
        //{
        //    if (AttachedMonitor == null)
        //        return false;
        //    EAMonitorSettings? eaSettings = VM.ReadEAMonitorSettings(AttachedMonitor);
        //    if (eaSettings == null)
        //    {
        //        //Should be never to here
        //        return false;
        //    }
 
        //    ////11 Apply settings to workWin
        //    //int cellCount = eaSettings.SelectedSplit.CellCount;
        //    //char splitKey = eaSettings.SelectedSplit.SplitKey;
        //    //List<double> settings = eaSettings.SelectedSplit.Settings;
        //    //VM.LogInfo($"  * SetWorkSplit: {eaSettings.SelectedSplit.ToString()}");
        //    //SetWorkingSplit(cellCount, splitKey, settings);

        //    _isWidthoutGap = eaSettings.IsWidthoutGap;
        //    _isOnlyAllowWhenShiftKeyPressed = eaSettings.IsOnlyAllowWhenShiftKeyPressed;
        //    _IsAwsEnabled = eaSettings.IsAwsEnabled;
        //    return true;
        //}
    }
}