#define REMOVE_EA_SPLITTERS //Define this symbol to remove all (unused VSplitters and HSplitters)
using DDPM.SA.Resources.Helper;
using System.Windows;
using System.Windows.Controls;
using Rect = System.Windows.Rect;

namespace DDPM.Easy.Common
{
    /// <summary>
    /// Interaction logic for SplitCtrl4A.xaml
    /// </summary>
    public partial class SplitCtrl4A : UserControl, ISplitCtrl, IDisposable
    {
        #region Private members
        private bool _isDisposed = false;
        #endregion Private members

        #region ctor

        public SplitCtrl4A()
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

        public string CtrlClass => nameof(SplitCtrl4A);
        public int CellCount => 4;
        public char SplitKey => 'A';
        public UserControl UC => this;

        //SplitCtrl2? ~ 7? are predefined layout, have default value, the EAID may be changed to [1000~1004] if they are customized.
        public int EAID { get; set; } = 14;
        public string TooltipResourceName { get; } = "EATooltip_41";
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
            SplitCtrl4A ctrl = new SplitCtrl4A();
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
            cellListH.Add(new CellObj("4a1", cell_4a1) { rcRatio = new Rect(0, 0, 0.5, 0.5) });
            cellListH.Add(new CellObj("4a2", cell_4a2) { rcRatio = new Rect(0.5, 0, 0.5, 0.5) });
            cellListH.Add(new CellObj("4a3", cell_4a3) { rcRatio = new Rect(0, 0.5, 0.5, 0.5) });
            cellListH.Add(new CellObj("4a4", cell_4a4) { rcRatio = new Rect(0.5, 0.5, 0.5, 0.5) });

            cellListV.Clear();
            cellListV.Add(new CellObj("4A1", cell_4A1) { rcRatio = new Rect(0.5, 0, 0.5, 0.5) });
            cellListV.Add(new CellObj("4A2", cell_4A2) { rcRatio = new Rect(0.5, 0.5, 0.5, 0.5) });
            cellListV.Add(new CellObj("4A3", cell_4A3) { rcRatio = new Rect(0, 0, 0.5, 0.5) });
            cellListV.Add(new CellObj("4A4", cell_4A4) { rcRatio = new Rect(0, 0.5, 0.5, 0.5) });
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
            if (VM.Settings_Double.Count < 4)
                return;

            if (VM.IsVertical)
            {
                if (cellListV.Count < 4)
                    return;

                double cx = VM.Settings_Double[1] + VM.Settings_Double[0];
                double cy = VM.Settings_Double[2] + VM.Settings_Double[3];
                if ((cx > 0) && (cy > 0))
                {
                    double w = VM.Settings_Double[1] / cx;
                    double h = VM.Settings_Double[2] / cy;
                    //4A3
                    cellListV[2].rcRatio = new Rect(0, 0, w, h);
                    //4A4
                    cellListV[3].rcRatio = new Rect(0, cellListV[2].rcRatio.Bottom, w, VM.Settings_Double[3] / cy);
                    double x = cellListV[2].rcRatio.Right;
                    w = VM.Settings_Double[0] / cx;
                    //4A1
                    cellListV[0].rcRatio = new Rect(x, 0, w, VM.Settings_Double[2] / cy);
                    //4A2
                    cellListV[1].rcRatio = new Rect(x, cellListV[0].rcRatio.Bottom, w, VM.Settings_Double[3] / cy);
                }
            }
            else //Horz
            {
                if (cellListH.Count < 4)
                    return;

                double cx = VM.Settings_Double[2] + VM.Settings_Double[3];
                double cy = VM.Settings_Double[0] + VM.Settings_Double[1];
                if ((cx > 0) && (cy > 0))
                {
                    double h = VM.Settings_Double[0] / cy;
                    //4a1
                    cellListH[0].rcRatio = new Rect(0, 0, VM.Settings_Double[2] / cx, h);
                    //4a2
                    cellListH[1].rcRatio = new Rect(cellListH[0].rcRatio.Right, 0, VM.Settings_Double[3] / cx, h);
                    h = VM.Settings_Double[1] / cy;
                    double y = cellListH[0].rcRatio.Bottom;
                    //4a3
                    cellListH[2].rcRatio = new Rect(0, y, VM.Settings_Double[2] / cx, h);
                    //4a4
                    cellListH[3].rcRatio = new Rect(cellListH[2].rcRatio.Right, y, VM.Settings_Double[3] / cx, h);
                }
            }
        }

        /// <summary>
        /// Convert ISplitCtrl.Settings to CellList[i].rcRect
        /// </summary>
        public void UpdateToCellListFromSettings()
        {
            if (cellListH.Count >= 4)
            {
                double w = VM.Settings_Double[2] + VM.Settings_Double[3];
                double h = VM.Settings_Double[0] + VM.Settings_Double[1];
                if ((w > 0) && (h > 0))
                {
                    //4a1
                    Rect rcRatio = (Rect)cellListH[0].rcRatio;
                    rcRatio.X = 0;
                    rcRatio.Y = 0;
                    rcRatio.Width = VM.Settings_Double[2] / w;
                    rcRatio.Height = VM.Settings_Double[0] / h;
                    cellListH[0].rcRatio = rcRatio;

                    //4a2
                    rcRatio = (Rect)cellListH[1].rcRatio;
                    rcRatio.X = VM.Settings_Double[2] / w;
                    rcRatio.Y = 0;
                    rcRatio.Width = VM.Settings_Double[3] / w;
                    rcRatio.Height = VM.Settings_Double[0] / h;
                    cellListH[1].rcRatio = rcRatio;

                    //4a3
                    rcRatio = (Rect)cellListH[2].rcRatio;
                    rcRatio.X = 0;
                    rcRatio.Y = VM.Settings_Double[0] / h; ;
                    rcRatio.Width = VM.Settings_Double[2] / w;
                    rcRatio.Height = VM.Settings_Double[1] / h;
                    cellListH[2].rcRatio = rcRatio;

                    //4a4
                    rcRatio = (Rect)cellListH[3].rcRatio;
                    rcRatio.X = VM.Settings_Double[2] / w;
                    rcRatio.Y = VM.Settings_Double[0] / h; ;
                    rcRatio.Width = VM.Settings_Double[3] / w;
                    rcRatio.Height = VM.Settings_Double[1] / h;
                    cellListH[3].rcRatio = rcRatio;
                }
            }
            if (cellListV.Count >= 4)
            {
                double w = VM.Settings_Double[0] + VM.Settings_Double[1];
                double h = VM.Settings_Double[2] + VM.Settings_Double[3];
                if ((w > 0) && (h > 0))
                {
                    //4A1
                    Rect rcRatio = (Rect)cellListV[0].rcRatio;
                    rcRatio.X = VM.Settings_Double[1] / w;
                    rcRatio.Y = 0;
                    rcRatio.Width = VM.Settings_Double[0] / w;
                    rcRatio.Height = VM.Settings_Double[2] / h;
                    cellListV[0].rcRatio = rcRatio;

                    //4A2
                    rcRatio = (Rect)cellListV[1].rcRatio;
                    rcRatio.X = VM.Settings_Double[1] / w;
                    rcRatio.Y = VM.Settings_Double[2] / h;
                    rcRatio.Width = VM.Settings_Double[0] / w;
                    rcRatio.Height = VM.Settings_Double[2] / h;
                    cellListV[1].rcRatio = rcRatio;

                    //4A3
                    rcRatio = (Rect)cellListV[2].rcRatio;
                    rcRatio.X = 0;
                    rcRatio.Y = 0;
                    rcRatio.Width = VM.Settings_Double[1] / w;
                    rcRatio.Height = VM.Settings_Double[2] / h;
                    cellListV[2].rcRatio = rcRatio;

                    //4A4
                    rcRatio = (Rect)cellListV[3].rcRatio;
                    rcRatio.X = 0;
                    rcRatio.Y = VM.Settings_Double[2] / h;
                    rcRatio.Width = VM.Settings_Double[1] / w;
                    rcRatio.Height = VM.Settings_Double[3] / h;
                    cellListV[3].rcRatio = rcRatio;
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

            HSplitterList.Clear();
            HSplitterList.Add(h1);
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
        private string _defaultHorzName = //LangHelper.Instance[$"EATooltip_41H"];
        "Option 4.1: split in 4 quadrants.";
        private string _defaultVertName = //LangHelper.Instance[$"EATooltip_41V"];
        "Option 4.1: split in 4 quadrants.";
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
        ~SplitCtrl4A()
        {
            Dispose(false);
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