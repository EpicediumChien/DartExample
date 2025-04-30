#define REMOVE_EA_SPLITTERS //Define this symbol to remove all (unused VSplitters and HSplitters)
using DDPM.SA.Resources.Helper;
using System.Windows;
using System.Windows.Controls;
using Rect = System.Windows.Rect;

namespace DDPM.Easy.Common
{
    /// <summary>
    /// Interaction logic for SplitCtrl4F.xaml
    /// </summary>
    public partial class SplitCtrl4F : UserControl, ISplitCtrl, IDisposable
    {
        #region Private members
        private bool _isDisposed = false;
        #endregion Private members

        #region ctor

        public SplitCtrl4F()
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

        public string CtrlClass => nameof(SplitCtrl4F);
        public int CellCount => 4;
        public char SplitKey => 'F';
        public UserControl UC => this;

        //SplitCtrl2? ~ 7? are predefined layout, have default value, the EAID may be changed to [1000~1004] if they are customized.
        public int EAID { get; set; } = 19;
        public string TooltipResourceName { get; } = "EATooltip_46";
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
            SplitCtrl4F ctrl = new SplitCtrl4F();
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
            cellListH.Add(new CellObj("4f1", cell_4f1) { rcRatio = new Rect(0, 0, 1 / 3, 1) });
            cellListH.Add(new CellObj("4f2", cell_4f2) { rcRatio = new Rect(1/3, 0, 1 / 3, 1) });
            cellListH.Add(new CellObj("4f3", cell_4f3) { rcRatio = new Rect(2/3, 0, 1 / 3, 0.5) });
            cellListH.Add(new CellObj("4f4", cell_4f4) { rcRatio = new Rect(2/3, 0.5, 1 / 3, 0.5) });

            cellListV.Clear();
            cellListV.Add(new CellObj("4F1", cell_4F1) { rcRatio = new Rect(0, 0, 1, 1/3) });
            cellListV.Add(new CellObj("4F2", cell_4F2) { rcRatio = new Rect(0, 1/3, 1, 1/3) });
            cellListV.Add(new CellObj("4F3", cell_4F3) { rcRatio = new Rect(0.5, 2/3, 0.5, 1/3) });
            cellListV.Add(new CellObj("4F4", cell_4F4) { rcRatio = new Rect(0, 2/3, 0.5, 1/3) });
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

                double cx = VM.Settings_Double[3] + VM.Settings_Double[4];
                double cy = VM.Settings_Double[0] + VM.Settings_Double[1] + VM.Settings_Double[2];
                if ((cy > 0) && (cx > 0))
                {
                    double h = VM.Settings_Double[0] / cy;
                    //4F1
                    cellListV[0].rcRatio = new Rect(0, 0, 1, VM.Settings_Double[0] / cy);
                    //4F2
                    cellListV[1].rcRatio = new Rect(0, cellListV[0].rcRatio.Bottom, 1, VM.Settings_Double[1] / cy);

                    //4F4
                    cellListV[4].rcRatio = new Rect(0, cellListV[2].rcRatio.Bottom, VM.Settings_Double[4] / cx, VM.Settings_Double[2] / cy);
                    //4F3
                    cellListV[2].rcRatio = new Rect(cellListV[4].rcRatio.Right, cellListV[2].rcRatio.Bottom, VM.Settings_Double[3] / cy, VM.Settings_Double[2] / cy);
                }
            }
            else //Horz
            {
                if (cellListH.Count < 4)
                    return;

                double cy = VM.Settings_Double[3] + VM.Settings_Double[4];
                double cx = VM.Settings_Double[0] + VM.Settings_Double[1] + VM.Settings_Double[2];
                if ((cy > 0) && (cx > 0))
                {
                    double w = VM.Settings_Double[0] / cx;
                    //4f1
                    cellListH[0].rcRatio = new Rect(0, 0, VM.Settings_Double[0] / cx, 1);
                    //4f2
                    cellListH[1].rcRatio = new Rect(cellListH[0].rcRatio.Right, 0, VM.Settings_Double[1] / cx, 1);
                    //4f3
                    cellListH[2].rcRatio = new Rect(cellListH[1].rcRatio.Right, 0, VM.Settings_Double[2] / cx, VM.Settings_Double[3] / cy);
                    //4f4
                    cellListH[3].rcRatio = new Rect(cellListH[1].rcRatio.Right, cellListH[2].rcRatio.Bottom, VM.Settings_Double[2] / cx, VM.Settings_Double[4] / cy);
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
            VSplitterList.Add(v2);
            VSplitterList.Add(V1);

            HSplitterList.Clear();
            HSplitterList.Add(h1);
            HSplitterList.Add(H1);
            HSplitterList.Add(H2);
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
        private string _defaultHorzName = //LangHelper.Instance[$"EATooltip_46H"];
        "Option 4.6: 3 columns, split equally. Columns 1 and 2, no split. Column 3, split equally.";
        private string _defaultVertName = //LangHelper.Instance[$"EATooltip_46V"];
        "Option 4.6: 3 rows, split equally, Row 1, split equally, Rows 2 and 3, no split.";
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
        ~SplitCtrl4F()
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