using DDPM.Easy.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using VcpCore.Common;

namespace DDPM.EABroker
{
    /// <summary>
    /// Interaction logic for EAWorkWindow.xaml
    /// </summary>
    public partial class EAWorkWindow : Window
    {
        #region Private members
        private readonly ArrangeVM _vm;
        private Screen _workScreen;
        private List<MonitorInfo> _attachedMonitors = new List<MonitorInfo>();
        private bool _isVertical = false;
        private ISplitCtrl? _workingSplit = null;
        private bool _isSplitCtrl0A = false;

        #endregion

        #region Init
        public EAWorkWindow(ArrangeVM vm)
        {
            InitializeComponent();
            _vm = vm;
            DataContext = _vm;

            Left = -99999;
            Top = -99999;
            Width = 10;
            Height = 10;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //Hide window from Alt+tab
            System.Windows.Interop.WindowInteropHelper wndHelper = new System.Windows.Interop.WindowInteropHelper(this);
            Win32Lib.Win32.HideWinFromAltTab(wndHelper.Handle);
        }
        #endregion

        #region WorkScreen
        public void SetWorkScreen(Screen screen, List<MonitorInfo> attachedMonitors)
        {
            _workScreen = screen;
            _attachedMonitors = attachedMonitors;
            Dispatcher_MoveToScreen(screen);
        }


        public void Dispatcher_MoveToScreen(Screen? newWorkScreen=null)
        {
            if (newWorkScreen != null)
                _workScreen = newWorkScreen;

            this.Dispatcher.Invoke(() =>
            {
                double screenScale = _vm.ScreenScale;
                Left = _workScreen.WorkingArea.Left / screenScale;
                Top = _workScreen.WorkingArea.Top / screenScale;
                Width = _workScreen.WorkingArea.Width / screenScale;
                Height = _workScreen.WorkingArea.Height / screenScale;
                _isVertical = (Width < Height);
                if (_workingSplit != null)
                    _workingSplit.IsVertical = _isVertical;

            });
        }
        public MonitorInfo? PrimaryMonitor
        {
            get
            {
                if (_attachedMonitors != null)
                {
                    if (_attachedMonitors.Any())
                        return _attachedMonitors[0];
                }
                return null;
            }
        }

        public bool IsMyMonitor(MonitorInfo mi)
        {
            if (_workScreen == null)
                return false;

            return (mi.DisplayName.Equals(_workScreen.DeviceName));
        }
        #endregion

        #region Flags
        public bool IsUsed { get; set; } = false;
        #endregion

        #region [Input] Working SplitCtrl


        public bool SetWorkingSplit(int cellCount, char splitKey, List<double>? settings = null)
        {
            this.Dispatcher.Invoke(() =>
            {
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

                    Trace.WriteLine($"EAWorkWindow.WorkScreen:({_workScreen.Bounds.Left},{_workScreen.Bounds.Top})-({_workScreen.Bounds.Right},{_workScreen.Bounds.Bottom}){_workScreen.Bounds.Width}x{_workScreen.Bounds.Height}");
                    Rect rcScreen = new Rect();
                    rcScreen.X = _workScreen.Bounds.Left / _vm.ScreenScale;
                    rcScreen.Y = _workScreen.Bounds.Top / _vm.ScreenScale;
                    rcScreen.Width = _workScreen.Bounds.Width / _vm.ScreenScale;
                    rcScreen.Height = _workScreen.Bounds.Height / _vm.ScreenScale;
                    Trace.WriteLine($"AfterScale(/{_vm.ScreenScale}):({rcScreen.X},{rcScreen.Y})-({rcScreen.Right},{rcScreen.Bottom}){rcScreen.Width}x{rcScreen.Height}");
                    sp0B.ApplySettingsToCellList(rcScreen);

                    _workingSplit.IsEditable = false;
                    _workingSplit.IsVertical = _isVertical;
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
                    _workingSplit.IsVertical = _isVertical;
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
                        sp0b.ApplySettingsToCellList(ArrangeVM.RectFromRectangle(_workScreen.Bounds));
                    }
                    fadeSplit.IsEditable = false;
                    fadeSplit.IsVertical = _isVertical;
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

        public bool IsSameWorkSplit(ISplitCtrl? splitCtrl)
        {
            if (_workingSplit == null)
                return false;
            if (splitCtrl == null)
                return false;

            if ((_workingSplit.CellCount == splitCtrl.CellCount) && (_workingSplit.SplitKey == splitCtrl.SplitKey)
                && (_workingSplit.Settings.SequenceEqual(splitCtrl.Settings)))
            {
                return true;
            }
            return false;
        }

        public void SetWorkSplitHoveringCellName(string cellName)
        {
            if (_workingSplit == null)
                return;
            _workingSplit.HoveringCell = cellName;
        }

        #endregion [Input] Working SplitCtrl

        #region FadeOut Animation

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
                _vm.RefreshCellRects();
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

        #endregion FadeOut FadeOut Animation


        #region DetermineHoveringCell

        public CellObj? DetermineHoveringCellObj(int x, int y)
        {
            if (_workingSplit == null)
                return null;


            DpiScale dpiScale = VisualTreeHelper.GetDpi(this);
            double scale = dpiScale.PixelsPerDip;

            bool isHandled = false;

            _workingSplit.HoveringCell = "";

            /*
            if (_workingSplit.CtrlClass.Equals("SplitCtrl2C"))
            {
                SplitCtrl2C ctrl = _workingSplit as SplitCtrl2C;
                foreach (CellBorder cellBd in ctrl.CellBorders)
                {
                    if (cellBd.rect.Contains(x, y))
                    {
                        _vm.AwsIcon1.HoveringCell = cellBd.CellName;
                        cellBd.IsHover = true;
                        isHandled = true;
                    }
                    else
                    {
                        cellBd.IsHover = false;
                    }
                }
            }

            if (_workingSplit.CtrlClass.Equals("SplitCtrl2A"))
            {
                SplitCtrl2A ctrl = _workingSplit as SplitCtrl2A;
                foreach (CellBorder cellBd in ctrl.CellBorders)
                {
                    if (cellBd.rect.Contains(x, y))
                    {
                        _vm.AwsIcon1.HoveringCell = cellBd.CellName;
                        cellBd.IsHover = true;
                        isHandled = true;
                    }
                    else
                    {
                        cellBd.IsHover = false;
                    }
                }
            }
            */

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
                            isHandled = true;
                        }
                    }

                    this.Dispatcher.Invoke(() =>
                    {
                        objCell.bd.Tag = tag;
                    });

                }
                return hoverCell; ;
            }

            /*
            foreach (CellBorder cellBd in _workingSplit.CellBorders)
            {
                if (isHandled)
                {
                    cellBd.IsHover = false;
                    continue;
                }

                if (cellBd.rect.Contains(x, y))
                {
                    _vm.AwsIcon1.HoveringCell = cellBd.CellName;
                    cellBd.IsHover = true;
                    isHandled = true;
                }
                else
                {
                    cellBd.IsHover = false;
                }
            }
            */

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

        #region Cells

        #endregion

        public void RefreshCellRects()
        {
            if (_workingSplit == null)
                return;

            this.Dispatcher.Invoke(() =>
            {
                /*
                if (_workingSplit.CtrlClass.Equals("SplitCtrl2A"))
                {
                    SplitCtrl2A ctrl = _workingSplit as SplitCtrl2A;
                    foreach (CellBorder cellBd in ctrl.CellBorders)
                    {
                        cellBd.rect = _vm.GetFrameworkElementRect(cellBd);
                    }
                }
                if (_vm.AwsIcon1.CtrlClass.Equals("SplitCtrl2C"))
                {
                    SplitCtrl2C ctrl = _vm.AwsIcon1 as SplitCtrl2C;
                    foreach (CellBorder cellBd in ctrl.CellBorders)
                    {
                        cellBd.rect = _vm.GetFrameworkElementRect(cellBd);
                    }
                }
                */
                //foreach (CellBorder cellBd in _workingSplit.CellBorders)
                //{
                //    cellBd.rect = _vm.GetFrameworkElementRect(cellBd);
                //}

                foreach (CellObj objCell in _workingSplit.CellList)
                {
                    if (objCell.bd == null)
                        continue;

                    objCell.rc = _vm.GetFrameworkElementRect(objCell.bd);

                    Trace.WriteLine($"Cell({objCell.Name})={ArrangeVM.FormatRect(objCell.rc)}");
                }
            });
        }

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

    }
}
