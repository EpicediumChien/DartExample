using DDPM.Easy.Common;
using DDPM.SA.Common.Display;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media.Animation;
using VcpCore.Common;
using Rectangle = System.Drawing.Rectangle;
using Window = System.Windows.Window;

namespace DDPM.EABroker
{
    /// <summary>
    /// Interaction logic for EAWorkWindow.xaml
    /// </summary>
    public partial class EAWorkWindow : Window
    {
        #region Private members
        private readonly ArrangeVM _vm;
        private Screen? _workScreen = null;
        private EAScreen? _workEaScreen = null;
        private List<MonitorInfo> _attachedMonitors = new List<MonitorInfo>();
        private bool _isVertical = false;
        private ISplitCtrl? _workingSplit = null;
        //private bool _isSplitCtrl0A = false;
        private readonly bool _isAwsBuddy;
        private bool _isWorkForSpanScreen = false;
        #endregion

        #region Init
        public EAWorkWindow(ArrangeVM vm, bool isAwsBuddy=false)
        {
            InitializeComponent();
            _vm = vm;
            DataContext = _vm;
            _isAwsBuddy = isAwsBuddy;

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
        public void SetWorkScreen(EAScreen screen, List<MonitorInfo> attachedMonitors)
        {
            _workEaScreen = screen;
            _workScreen = _workEaScreen.FormsScreen;
            _attachedMonitors = attachedMonitors;
            Dispatcher_MoveToScreen(_workEaScreen.FormsScreen);
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
                if (_attachedMonitors != null && _attachedMonitors.Any())
                {
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

        public void SetWorkScreenToSpanScreen()
        {
            if (!_vm.IsSpanEnabled)
                return;
            MonitorInfo? miPrimary = _vm.SpanScreen.GetPrimaryMonitor();

            _isWorkForSpanScreen = true;
            
            //Set the Primary screen
            _attachedMonitors = new List<MonitorInfo>();
            _attachedMonitors.Add(miPrimary);

            Dispatcher.BeginInvoke(new Action(() =>
            {
                double screenScale = _vm.ScreenScale;
                Rectangle rcSpan = _vm.SpanScreen.WorkingArea;

                Left = (double)rcSpan.Left / screenScale;
                Top = rcSpan.Top / screenScale;
                Width = rcSpan.Width / screenScale;
                Height = rcSpan.Height / screenScale;
                _isVertical = (Width < Height);
                if (_workingSplit != null)
                    _workingSplit.IsVertical = _isVertical;

            }));

        }

        public string GetWorkScreenInfoText()
        {
            if (_workEaScreen != null)
            {
                return $"EAScreen: {_workEaScreen.ToString()}";
            }
            return $"EAScreen: None";
        }

        #endregion

        #region Flags
        public bool IsUsed { get; set; } = false;
        #endregion

        #region [Input] Working SplitCtrl
        private void ReleaseSplitCtrls()
        {
            if (splitCtrl.Content != null)
            {
                // 释放资源
                if (splitCtrl.Content is FrameworkElement contentElement)
                {
                    // 这里可以添加更多资源释放逻辑，例如取消事件订阅等
                }

                // 清空 ContentControl 的内容
                splitCtrl.Content = null;
            }

            if (fadeOutCtrl.Content != null)
            {
                // 释放资源
                if (fadeOutCtrl.Content is FrameworkElement contentElement)
                {
                    // 这里可以添加更多资源释放逻辑，例如取消事件订阅等
                }
                // 清空 ContentControl 的内容
                fadeOutCtrl.Content = null;
            }

            //GC.Collect();
            //GC.WaitForPendingFinalizers();
        }

        public bool SetWorkingSplit(SplitJson splitJson, bool showFadeOut=false)
        {
            this.Dispatcher.Invoke(() =>
            {
                Rect rcScreen = new Rect();
                int cellCount = splitJson.CellCount;
                char splitKey = splitJson.SplitKey;

                //Derek 2025/03/28 release previous resource
                ReleaseSplitCtrls();

                //if ((cellCount == 0) && (splitKey == 'A'))
                if (splitJson.IsOff)
                {
                    _workingSplit = null;
                    splitCtrl.Content = null;
                    fadeOutCtrl.Content = null;
                    return;
                }

                //if ((cellCount == 0) && (splitKey == 'B'))
                if (splitJson.IsOverlapLayout)
                {
                    SplitCtrl0B sp0B = new SplitCtrl0B();
                    _workingSplit = sp0B;
                    _workingSplit.SplitMode = eSplitModes.Work;

                    if (splitJson.Settings == null)
                        _workingSplit.Settings = new List<double>();
                    else
                        _workingSplit.Settings = new List<double>(splitJson.Settings);


                    //_vm.CreateCellBorderListToSplitCtrlFromCellJsons(splitJson.Cells, ref _workingSplit);

                    Trace.WriteLine($"EAWorkWindow.WorkScreen:({_workScreen.WorkingArea.Left},{_workScreen.WorkingArea.Top})-({_workScreen.WorkingArea.Right},{_workScreen.WorkingArea.Bottom}){_workScreen.WorkingArea.Width}x{_workScreen.WorkingArea.Height}");

                    double scale = 1.000;
                    //Robert_Lin 2025-3-17, change to use WorkingArea instead of Bounds
                    //rcScreen.X = _workScreen.Bounds.Left / scale;
                    //rcScreen.Y = _workScreen.Bounds.Top / scale;
                    //rcScreen.Width = _workScreen.Bounds.Width / scale;
                    //rcScreen.Height = _workScreen.Bounds.Height / scale;

                    //Robert_Lin 2025-4-22 The display layout too large if it's mirgrated from DDM
                    //OLD:
                    //NEW:
                    if (splitJson.IsMigratedFromDdm)
                    {
                        rcScreen.X = _workScreen.WorkingArea.Left ;
                        rcScreen.Y = _workScreen.WorkingArea.Top ;
                        rcScreen.Width = _workScreen.WorkingArea.Width / _vm.ScreenScale;
                        rcScreen.Height = _workScreen.WorkingArea.Height / _vm.ScreenScale;
                    }
                    else
                    {
                        rcScreen.X = _workScreen.WorkingArea.Left;
                        rcScreen.Y = _workScreen.WorkingArea.Top;
                        rcScreen.Width = _workScreen.WorkingArea.Width / scale;
                        rcScreen.Height = _workScreen.WorkingArea.Height/scale;
                    }

                    Trace.WriteLine($"AfterScale(/{_vm.ScreenScale}):({rcScreen.X},{rcScreen.Y})-({rcScreen.Right},{rcScreen.Bottom}){rcScreen.Width}x{rcScreen.Height}");

                    //sp0B.UI_CreateCellBordersFromRatioRects(rcScreen);
                    sp0B.ApplySettingsToCellList(rcScreen);

                    _workingSplit.IsEditable = false;
                    _workingSplit.IsVertical = _isVertical;
                    splitCtrl.Content = _workingSplit;
                }
                else
                {
                    _workingSplit = ISplitCtrl.Create(cellCount, splitKey);

                    if (_workingSplit != null)
                    {
                        _workingSplit.SplitMode = eSplitModes.Work;
                        if (splitJson.Settings == null)
                            _workingSplit.Settings = new List<double>();
                        else
                            _workingSplit.Settings = new List<double>(splitJson.Settings);
                        _workingSplit.IsEditable = false;
                        _workingSplit.IsVertical = _isVertical;
                        splitCtrl.Content = _workingSplit;
                    }
                    else
                    {
                        splitCtrl.Content = null;
                    }
                }
                //AwsBuddy Window do not support FadeOut
                if (_isAwsBuddy)
                {

                    return;
                }
                if (!showFadeOut)
                    return;

                ISplitCtrl? fadeSplit = ISplitCtrl.Create(cellCount, splitKey);
                if (fadeSplit != null)
                {
                    fadeSplit.SplitMode = eSplitModes.Work;
                    if (splitJson.Settings == null)
                        fadeSplit.Settings = new List<double>();
                    else
                        fadeSplit.Settings = new List<double>(splitJson.Settings);

                    //_vm.CreateCellBorderListToSplitCtrlFromCellJsons(splitJson.Cells, ref fadeSplit);

                    if ((cellCount == 0) && (splitKey == 'B'))
                    {
                        SplitCtrl0B sp0b = fadeSplit as SplitCtrl0B;
                        // sp0b.UI_CreateCellBordersFromRatioRects(rcScreen);
                        sp0b.ApplySettingsToCellList(rcScreen);

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


        public bool SetWorkingSplit(int cellCount, char splitKey, List<double>? settings = null)
        {
            this.Dispatcher.Invoke(() =>
            {
                Rect rcScreen = new Rect();

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
                    //Rect rcScreen = new Rect();
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

                //AwsBuddy Window do not support FadeOut
                if (_isAwsBuddy)
                {

                    return;
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
                        SplitCtrl0B? sp0b = fadeSplit as SplitCtrl0B;
                        sp0b?.ApplySettingsToCellList(rcScreen);
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
                _vm.RefreshCellRects(true);
                this.IsFading = false;
                fadeOutGrid.Visibility = Visibility.Collapsed;
                //Visibility = Visibility.Hidden;
                //gridSplitCtrl.Opacity = 1;
                sb = null; //Derek 2025/04/01
            };

            IsFading = true;
            fadeOutGrid.Visibility = Visibility.Visible;
            //gridSplitCtrl.Opacity = 1;
            sb.Begin();
            // });
        }

        //Derek 2025/04/01
        //public void StopFadeOutAnimation()
        //{
        //    this.Dispatcher.Invoke(() =>
        //    {
        //        Storyboard? sb = Resources["FadeOut"] as Storyboard;

        //        if (sb == null)
        //            return;

        //        sb.Stop();
        //        IsFading = false;
        //        //gridSplitCtrl.Opacity = 1;
        //    });
        //}

        #endregion FadeOut FadeOut Animation


        #region DetermineHoveringCell

        public CellObj? DetermineHoveringCellObj(int x, int y)
        {
            //if (!Dispatcher.CheckAccess())
            //    return null;

            if (_workingSplit == null)
                return null;

            CellObj? hoverCell = null;
            ISplitCtrl localSplit = _workingSplit as ISplitCtrl;
            if (localSplit == null)
                return null;

            //For Overlap layout (SplitCtrl0B), the hover state is set to IsHover.
            //And need to refresh once changed
            if (localSplit.IsOverlapCustomLayout)
            {
                foreach (CellObj objCell in localSplit.CellList)
                {
                    //If the hoverCell is not determined now
                    if (hoverCell == null)
                    {
                        //Check if cursor(x,y) is inside this CellBorder
                        if (objCell.rc.Contains(x, y))
                        {
                            //Yes, this cell will be the HoveringCell
                            //Set this CellBorder to Hover
                            //objCell.CellBd.IsHover = true;
                            objCell.CellBd.Dispatcher_SetIsHover(true);
                            //Store it in the return object
                            hoverCell = objCell;
                        }
                        else
                        {
                            //No, set this CellBorder.IsHover to false
                            //objCell.CellBd.IsHover = false;
                            objCell.CellBd.Dispatcher_SetIsHover(false);
                        }
                    }
                    else
                    {
                        //hoverCell has been determined, so the other CellBorder will set IsHover to false
                        //objCell.CellBd.IsHover = false;
                        objCell.CellBd.Dispatcher_SetIsHover(false);
                    }
                } //foreach
                return hoverCell;
            }
            //Else: Not Overlap layout, will hover/unhover the cell by DataTrigger with HoveringCell property

            foreach (CellObj objCell in localSplit.CellList)
            {
                //Check if cursor(x,y) is inside this CellBorder
                if (objCell.rc.Contains(x, y))
                {
                    //Trigger it to Hover state by HoveringCell property (Cell.Name)
                    localSplit.HoveringCell = objCell.Name;
                    //Store it in the return object
                    hoverCell = objCell;
                    //We can return immediately, all other CellObjs not been triggerd will be non-Hover
                    return hoverCell;
                }

            } //foreach

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
                bool _areCellRectsRefreshed = true;
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
                if (_workingSplit.IsAddedCustomLayout)
                {
                    SplitCtrl0B sp0B = (SplitCtrl0B)_workingSplit;
                    foreach (CellBorder cellBd in _workingSplit.CellBorders)
                    {
                        cellBd.rect = _vm.GetFrameworkElementRect(cellBd);

                        CellObj? cellObj = _workingSplit.CellList.Find(x => x.Name.Equals(cellBd.CellName));
                        if (cellObj != null)
                        {
                            cellObj.rc = cellBd.rect;

                            if (cellObj.rc.IsEmpty)
                                _areCellRectsRefreshed = false;
                        }
                    }
                }
                else // if (_workingSplit.CellCount==5)
                {
                    foreach(CellObj objCell in _workingSplit.CellList)
                    {
                        if (objCell.CellBd == null)
                            continue;

                        objCell.rc = _vm.GetFrameworkElementRect(objCell.CellBd);

                        if (objCell.rc.IsEmpty)
                            _areCellRectsRefreshed = false;
                    }
                }
                //else
                //{
                //    //foreach (CellObj objCell in _workingSplit.CellList)
                //    //{
                //    //    if (objCell.bd == null)
                //    //        continue;

                //    //    objCell.rc = _vm.GetFrameworkElementRect(objCell.bd);

                //    //    Trace.WriteLine($"Cell({objCell.Name})={ArrangeVM.FormatRect(objCell.rc)}");

                //    //    if (objCell.rc.IsEmpty)
                //    //        _areCellRectsRefreshed = false;
                //    //}

                //}
                if (!_areCellRectsRefreshed)
                {
                    //System.Threading.Timer timer1 = new System.Threading.Timer(refreshCellRects_TimerCallback, null, 100, Timeout.Infinite);
                    System.Threading.Timer? timer1 = null;
                    try
                    {
                        timer1 = new System.Threading.Timer((obj) => { RefreshCellRects(); }, null, 100, Timeout.Infinite);
                    }
                    catch (Exception e)
                    {
                        _vm.WriteLog($"[EAWindow] create timer for RefreshCellRects() exception: {e.Message}");
                    }
                    finally
                    {
                        timer1?.Dispose();
                        timer1 = null;
                    }
                }
                else
                {
                    string d = "";
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

        #region SpanScreen
        public bool IsWorkForSpanScreen => _isWorkForSpanScreen;
        #endregion Span

        #region InUse
        public void ResetToUnused()
        {
            //Change the _working split
            SetWorkingSplit(0, 'A');
            //Clear InUse flag
            IsUsed = false;
        }

        #endregion InUse

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //System.Windows.MessageBox.Show("Window_Closing");
            if (System.Windows.Threading.Dispatcher.CurrentDispatcher != null)
            {
                //System.Windows.MessageBox.Show("Window_Closing1");
                System.Windows.Threading.Dispatcher.CurrentDispatcher.InvokeShutdown();
            }
        }
    }
}
