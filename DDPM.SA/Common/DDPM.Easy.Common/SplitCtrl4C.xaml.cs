#define REMOVE_EA_SPLITTERS //Define this symbol to remove all (unused VSplitters and HSplitters)
using DDPM.SA.Resources.Helper;
using System.Windows;
using System.Windows.Controls;
using Rect = System.Windows.Rect;

namespace DDPM.Easy.Common
{
    /// <summary>
    /// Interaction logic for SplitCtrl4C.xaml
    /// </summary>
    public partial class SplitCtrl4C : UserControl, ISplitCtrl, IDisposable
    {
        #region Private members
        private bool _isDisposed = false;
        #endregion Private members

        #region ctor

        public SplitCtrl4C()
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

        public string CtrlClass => nameof(SplitCtrl4C);
        public int CellCount => 4;
        public char SplitKey => 'C';
        public UserControl UC => this;

        //SplitCtrl2? ~ 7? are predefined layout, have default value, the EAID may be changed to [1000~1004] if they are customized.
        public int EAID { get; set; } = 16;
        public string TooltipResourceName { get; } = "EATooltip_43";
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
            SplitCtrl4C ctrl = new SplitCtrl4C();
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
            cellListH.Add(new CellObj("4c1", cell_4c1) { rcRatio = new Rect(0, 0, 0.5, 1) });
            cellListH.Add(new CellObj("4c2", cell_4c2) { rcRatio = new Rect(0.5, 0, 0.5, 1 / 3) });
            cellListH.Add(new CellObj("4c3", cell_4c3) { rcRatio = new Rect(0.5, 1/3, 0.5, 1 / 3) });
            cellListH.Add(new CellObj("4c4", cell_4c4) { rcRatio = new Rect(0.5, 2/3, 0.5, 1 / 3) });

            cellListV.Clear();
            cellListV.Add(new CellObj("4C1", cell_4C1) { rcRatio = new Rect(0, 0, 1, 0.5) });
            cellListV.Add(new CellObj("4C2", cell_4C2) { rcRatio = new Rect(2/3, 0.5, 1/3, 0.5) });
            cellListV.Add(new CellObj("4C3", cell_4C3) { rcRatio = new Rect(1/3, 0.5, 1/3, 0.5) });
            cellListV.Add(new CellObj("4C4", cell_4C4) { rcRatio = new Rect(0, 0.5, 1/3, 0.5) });
        }
        protected void ClearCellList()
        {
            if (cellListH != null)
            {
                foreach (CellObj cell in cellListH)
                {
                    cell.Dispose();
                }
                cellListH.Clear();
                cellListH = null;
            }
            if (cellListV != null)
            {
                foreach (CellObj cell in cellListV)
                {
                    cell.Dispose();
                }
                cellListV.Clear();
                cellListV = null;
            }
        }

        /// <summary>
        /// Convert ISplitCtrl.Settings to CellList[i].rcRect
        /// </summary>
        public void UpdateRatioRectsFromSettings()
        {
            if (VM.Settings_Double.Count < 5)
                return;

            if (VM.IsVertical)
            {
                if (cellListV.Count < 4)
                    return;

                double cy = VM.Settings_Double[0] + VM.Settings_Double[1];
                if (cy > 0)
                {
                    //4C1
                    cellListV[0].rcRatio = new Rect(0, 0, 1, VM.Settings_Double[0] / cy);
                }

                double cx = VM.Settings_Double[4] + VM.Settings_Double[3] + +VM.Settings_Double[2];

                // fix typo
                if ((cx > 0) &&(cy > 0))// (cx > 0))
                {
                    double y = cellListV[0].rcRatio.Bottom;
                    double h = VM.Settings_Double[1] / cy;

                    //4C4
                    cellListV[3].rcRatio = new Rect(0, y, VM.Settings_Double[4] / cx, h);
                    //4C3
                    cellListV[2].rcRatio = new Rect(cellListV[3].rcRatio.Right, y, VM.Settings_Double[3] / cx, h);
                    //4C2
                    cellListV[1].rcRatio = new Rect(cellListV[2].rcRatio.Right, y, VM.Settings_Double[2] / cx, h);
                }
            }
            else //Horz
            {
                if (cellListH.Count < 4)
                    return;

                double cx = VM.Settings_Double[0] + VM.Settings_Double[1];
                if (cx > 0)
                {
                    //4c1
                    cellListH[0].rcRatio = new Rect(0, 0, VM.Settings_Double[0] / cx, 1);
                }
                double cy = VM.Settings_Double[2] + VM.Settings_Double[3] + VM.Settings_Double[4];
                if ((cx > 0) && (cy > 0))
                {
                    double x = cellListH[0].rcRatio.Right;
                    double w = VM.Settings_Double[1] / cx;
                    //4a2
                    cellListH[1].rcRatio = new Rect(x, 0, w, VM.Settings_Double[2] / cy);
                    //4a3
                    cellListH[2].rcRatio = new Rect(x, cellListH[1].rcRatio.Bottom, w, VM.Settings_Double[3] / cy);
                    //4a4
                    cellListH[3].rcRatio = new Rect(x, cellListH[2].rcRatio.Bottom, w, VM.Settings_Double[4] / cy);
                }
            }
        }
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
        private void ClearCellBorders()
        {
            if (celBordersH != null)
            {
                foreach (CellBorder cellBd in celBordersH)
                {
                    cellBd.Dispose();
                }
                celBordersH.Clear();
                celBordersH = null;
            }
            if (celBordersV != null)
            {
                foreach (CellBorder cellBd in celBordersV)
                {
                    cellBd.Dispose();
                }
                celBordersV.Clear();
                celBordersV = null;
            }
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
            VSplitterList.Add(V1);
            VSplitterList.Add(V2);

            HSplitterList.Clear();
            HSplitterList.Add(h1);
            HSplitterList.Add(h2);
            HSplitterList.Add(H1);
        }

        private void ClearSplitterList()
        {
            if (VSplitterList != null)
            {
                VSplitterList.Clear();
                VSplitterList = null;
            }
            if (HSplitterList != null)
            {
                HSplitterList.Clear();
                HSplitterList = null;
            }
        }
        #endregion Splitter List
#endif //#if !REMOVE_EA_SPLITTERS

        #region Settings

        public List<double> DefaultSettings => new List<double>() { 1, 1, 1, 1, 1 };
        #endregion Settings

        #region FriendlyName
        private string _friendlyName = string.Empty;
        private string _defaultHorzName = //LangHelper.Instance[$"EATooltip_43H"];
        "Option 4.3: 2 columns, split equally. Column 1, no split. Column 2, split equally in 3 sections.";
        private string _defaultVertName = //LangHelper.Instance[$"EATooltip_43V"];
        "Option 4.3: 2 rows, split equally. Row 1, split equally in 3 sections. Column 2, no split.";
        //                                                                                                        Row
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
                    ClearCellList();
                    ClearCellBorders();
#if !REMOVE_EA_SPLITTERS
                    ClearSplitterList();
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
        ~SplitCtrl4C()
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