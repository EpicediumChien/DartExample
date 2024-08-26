using DDPM.Easy.Common;
using Dell.Client.Framework.Common;
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
        private MonitorInfo _mi;
        private readonly Screen _scr;
        private readonly List<MonitorInfo> _monitors;
        #endregion private members

        #region ctor
        public EAWorkWindow(ArrangeVM vm, Screen scr, List<MonitorInfo> monitors)
        {
            InitializeComponent();
            VM = vm;
            _scr = scr;
            _monitors = monitors;
            DataContext = vm;
            VM.IsMovingChanged += VM_IsMovingChanged;
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

        #region Working SplitCtrl

        private ISplitCtrl? _workingSplit = null;
        private bool _isSplitCtrl0A = false;

        public bool SetWorkingSplit(int cellCount, char splitKey, List<double>? settings = null)
        {
            this.Dispatcher.Invoke(() =>
            {
                if ((cellCount == 0) && (splitKey == 'A'))
                {
                    _workingSplit = null;
                }
                else
                {
                    _workingSplit = ISplitCtrl.Create(cellCount, splitKey);
                }
                if (_workingSplit != null)
                {
                    _workingSplit.SplitMode = eSplitModes.Work;
                    _workingSplit.IsEditable = false;
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
                    fadeSplit.IsEditable = false;
                    fadeOutCtrl.Content = fadeSplit;
                }

                InvokeFadeOutAnimation();
            });

            return true;

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
                    if (_scr != null)
                    {
                        _displayName = _scr.DeviceName.Replace("\\", "");
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

        #endregion Working SplitCtrl

        #region ILog for EAPlugin
        public ILog Log { get; set; }
        #endregion

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

            DpiScale dpiScale = VisualTreeHelper.GetDpi(this);
            double scale = dpiScale.PixelsPerDip;
            VM.ScreenScale = scale;
            Trace.WriteLine($"WorkWin.Scale={scale}");

            foreach (CellObj objCell in _workingSplit.CellList)
            {
                if (objCell.bd == null)
                    continue;

                objCell.rc = GetBorderRect(objCell.bd);

                Trace.WriteLine($"Cell({objCell.Name})={ArrangeVM.FormatRect(objCell.rc)}");
            }
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

            DpiScale dpiScale = VisualTreeHelper.GetDpi(this);
            double scale = dpiScale.PixelsPerDip;

            foreach (CellObj objCell in _workingSplit.CellList)
            {
                if (objCell.rc.Contains(x, y))
                {
                    _workingSplit.HoveringCell = objCell.Name;
                    return objCell;
                }
            }
            _workingSplit.HoveringCell = "";
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
                    //Visibility = Visibility.Hidden;
                    //gridSplitCtrl.Opacity = 1;
                };

                IsFading = true;
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
    }
}