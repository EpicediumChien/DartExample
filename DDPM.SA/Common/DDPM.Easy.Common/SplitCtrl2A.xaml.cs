#define REMOVE_EA_SPLITTERS //Define this symbol to remove all (unused VSplitters and HSplitters)
using DDPM.SA.Resources.Helper;
using System.Windows;
using System.Windows.Controls;
using Rect = System.Windows.Rect;

namespace DDPM.Easy.Common
{
    /// <summary>
    /// Interaction logic for SplitCtrl2A.xaml
    /// </summary>
    public partial class SplitCtrl2A : UserControl, ISplitCtrl, IDisposable
    {
        #region Private members
        private bool _isDisposed = false;
        #endregion Private members

        #region ctor

        public SplitCtrl2A()
        {
            InitializeComponent();
            VM.Settings = SplitCtrlVM.Double_To_GridLength(DefaultSettings);
            DataContext = vm;
            InitCellList();
#if !REMOVE_EA_SPLITTERS
            InitSplitterList();
#endif
        }

        #endregion ctor

        #region ISplitCtrl Native Members

        public string CtrlClass => nameof(SplitCtrl2A);
        public int CellCount => 2;
        public char SplitKey => 'A';
        public UserControl UC => this;
        //SplitCtrl2? ~ 7? are predefined layout, have default value, the EAID may be changed to [1000~1004] if they are customized.
        public int EAID { get; set; } = 1;

        public string TooltipResourceName { get; } = "EATooltip_21";
        #endregion ISplitCtrl Native Members

        #region ViewModel

        private SplitCtrlVM vm = new SplitCtrlVM();
        public SplitCtrlVM VM => vm;

        #endregion ViewModel

        #region Create new instance

        /// <summary>
        /// Create a new instance, but Settings not been copied
        /// </summary>
        /// <returns></returns>
        public ISplitCtrl New()
        {
            SplitCtrl2A ctrl = new SplitCtrl2A();
            return (ISplitCtrl)ctrl;
        }

        #endregion Create new instance

        #region Cell List

        private List<CellObj> cellListH = new List<CellObj>();
        private List<CellObj> cellListV = new List<CellObj>();

        public List<CellObj> CellList
        {
            get
            {
                if (VM.IsVertical)
                    return cellListV;
                else
                    return cellListH;
            }
            set { }
        }

        //public List<CellObj> CellList { get; set; } = new List<CellObj>();
        public void InitCellList()
        {
            cellListH.Clear();
            cellListH.Add(new CellObj("2a1", cell_2a1) { rcRatio = new Rect(0,0,0.5,1)});
            cellListH.Add(new CellObj("2a2", cell_2a2) { rcRatio = new Rect(0.5, 0, 0.5, 1) });

            cellListV.Clear();
            cellListV.Add(new CellObj("2A1", cell_2A1) { rcRatio = new Rect(0, 0, 1, 0.5) });
            cellListV.Add(new CellObj("2A2", cell_2A2) { rcRatio = new Rect(0, 0.5, 1, 0.5) });
        }

        /// <summary>
        /// Convert ISplitCtrl.Settings to CellList[i].rcRect
        /// </summary>
        public void UpdateRatioRectsFromSettings()
        {
            if (VM.Settings_Double.Count < 2)
                return;

            if (VM.IsVertical)
            {
                if (cellListV.Count < 2)
                    return;

                //double cx = VM.Settings_Double[1] + VM.Settings_Double[0];
                double cy = VM.Settings_Double[0] + VM.Settings_Double[1];
                if (cy > 0)
                {
                    //double w = VM.Settings_Double[1] / cx;
                    //2A1
                    cellListV[0].rcRatio = new Rect(0, 0, 1, VM.Settings_Double[0] / cy);
                    //2A2
                    cellListV[1].rcRatio = new Rect(0, cellListV[0].rcRatio.Bottom, 1, VM.Settings_Double[1] / cy);
                }
            }
            else //Horz
            {
                if (cellListH.Count < 2)
                    return;

                double cx = VM.Settings_Double[0] + VM.Settings_Double[1];
                //double cy = VM.Settings_Double[0] + VM.Settings_Double[1];
                if (cx > 0)
                {
                    //double h = VM.Settings_Double[0] / cy;
                    //2a1
                    cellListH[0].rcRatio = new Rect(0, 0, VM.Settings_Double[0] / cx, 1);
                    //2a2
                    cellListH[1].rcRatio = new Rect(cellListH[0].rcRatio.Right, 0, VM.Settings_Double[1] / cx, 1);
                }
            }
        }

        public void UpdateSettingsToCells()
        {
            double sum = VM.Settings_Double.Sum();
            if (sum <= 0) 
                return;

            if (CellList.Count >= 2)
            {
                CellList[0].rcRatio = new Rect(
                    0, 0, VM.Settings_Double[0] / sum, 1);
                CellList[1].rcRatio = new Rect(
                    VM.Settings_Double[0] / sum, 0, 1, 1);
            }
        }

        //public void RefreshCellsRect()
        //{
        //    Dispatcher.InvokeAsync(() => {
        //        foreach (CellObj cellObj in CellList)
        //        {
        //            Point ptTopLeft = cellObj.CellBd.PointFromScreen(new Point(0, 0));
        //            double w = cellObj.CellBd.ActualWidth;
        //            double h = cellObj.CellBd.ActualHeight;
        //            cellObj.rc = new Rect(ptTopLeft.X, ptTopLeft.Y, w, h);
        //        }
        //    }, System.Windows.Threading.DispatcherPriority.Loaded);
        //}
        #endregion Cell List

        #region CellBorders
        //CellBorders should be used in SplitCtrl0B only
        private List<CellBorder> celBordersH = new List<CellBorder>();
        private List<CellBorder> celBordersV = new List<CellBorder>();

        public List<CellBorder> CellBorders
        {
            get
            {
                if (VM.IsVertical)
                    return celBordersV;
                else
                    return celBordersH;
            }
            set { }
        }
        #endregion

#if !REMOVE_EA_SPLITTERS
        #region Splitter List

        public List<GridSplitter> VSplitterList { get; set; } = new List<GridSplitter>();
        public List<GridSplitter> HSplitterList { get; set; } = new List<GridSplitter>();

        public void InitSplitterList()
        {
            VSplitterList.Clear();
            VSplitterList.Add(v1);

            HSplitterList.Clear();
            HSplitterList.Add(H1);
        }

        #endregion Splitter List
#endif //#if !REMOVE_EA_SPLITTERS

        #region Settings

        public List<double> DefaultSettings => new List<double>() { 1, 1 };
        #endregion Settings

        #region FriendlyName
        private string _friendlyName = string.Empty;
        private string _defaultHorzName = //LangHelper.Instance[$"EATooltip_21H"];
        "Option 2.1: 2 columns, split equally.";
        private string _defaultVertName = //LangHelper.Instance[$"EATooltip_21V"];
        "Option 2.1: 2 rows, split equally.";
        public string FriendlyName 
        { 
            get
            {
                if (String.IsNullOrEmpty(_friendlyName))
                {
                    if (VM.IsVertical)
                    {
                        try
                        {
                            return LangHelper.Instance[$"{TooltipResourceName}V"];
                        }
                        catch (Exception e1)
                        {
                        }
                        return _defaultVertName;
                    }
                    else
                    {
                        try
                        {
                            return LangHelper.Instance[$"{TooltipResourceName}H"];
                        }
                        catch (Exception e1)
                        {
                        }
                        return _defaultHorzName;
                    }
                }
                return _friendlyName;
            }
            set
            {
                _friendlyName = value;
            }
        }
        #endregion FriendlyName

        #region Dispose and Destructor
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    // 釋放託管資源
                    if (vm != null)
                    {
                        vm.Dispose();
                        vm = null;
                    }
                    InitCellList();
                    cellListH = null;
                    cellListV = null;

#if !REMOVE_EA_SPLITTERS
                    InitSplitterList();
#endif
                    if (DefaultSettings != null)
                    {
                        DefaultSettings.Clear();
                    }

                }

                // 釋放非託管資源
                //if (unmanagedResource != IntPtr.Zero)
                //{
                //    // 釋放資源
                //    unmanagedResource = IntPtr.Zero;
                //}

                _isDisposed = true;
            }
        }
        ~SplitCtrl2A()
        {
            Dispose(false);
            vm?.Dispose();//SDL: Improper Resource Shutdown or Release
        }
        #endregion

        #region UserControl event handlers
        private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.IsVisible)
            {
                ((ISplitCtrl)this).RefreshCellRects();
            }
        }
        #endregion UserControl event handlers
    }
}