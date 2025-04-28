#define REMOVE_EA_SPLITTERS //Define this symbol to remove all (unused VSplitters and HSplitters)
using DDPM.SA.Resources.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DDPM.Easy.Common
{
    /// <summary>
    /// Interaction logic for SplitCtrl5A.xaml
    /// </summary>
    public partial class SplitCtrl5A : UserControl, ISplitCtrl, IDisposable
    {
        #region Private members
        private bool _isDisposed = false;
        #endregion Private members

        #region ctor

        public SplitCtrl5A()
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

        public string CtrlClass => nameof(SplitCtrl5A);
        public int CellCount => 5;
        public char SplitKey => 'A';
        public UserControl UC => this;

        //SplitCtrl2? ~ 7? are predefined layout, have default value, the EAID may be changed to [1000~1004] if they are customized.
        public int EAID { get; set; } = 20;
        public string TooltipResourceName { get; } = "EATooltip_51";
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
            SplitCtrl5A ctrl = new SplitCtrl5A();
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
            cellListH.Add(new CellObj("5a1", cell_5a1) { rcRatio = new Rect(0, 0, 1/3, 0.5) });
            cellListH.Add(new CellObj("5a2", cell_5a2) { rcRatio = new Rect(1/3, 0, 1/3, 0.5) });
            cellListH.Add(new CellObj("5a3", cell_5a3) { rcRatio = new Rect(2/3, 0, 1/3, 0.5) });
            cellListH.Add(new CellObj("5a4", cell_5a4) { rcRatio = new Rect(0, 0.5, 0.5, 0.5) });
            cellListH.Add(new CellObj("5a5", cell_5a5) { rcRatio = new Rect(0.5, 0.5, 0.5, 0.5) });

            cellListV.Clear();
            cellListV.Add(new CellObj("5A1", cell_5A1) { rcRatio = new Rect(0.5, 0, 0.5, 1/3) });
            cellListV.Add(new CellObj("5A2", cell_5A2) { rcRatio = new Rect(0.5, 1/3, 0.5, 1/3) });
            cellListV.Add(new CellObj("5A3", cell_5A3) { rcRatio = new Rect(0.5, 2/3, 0.5, 1/3) });
            cellListV.Add(new CellObj("5A4", cell_5A4) { rcRatio = new Rect(0, 0, 0.5, 0.5) });
            cellListV.Add(new CellObj("5A5", cell_5A5) { rcRatio = new Rect(0, 0.5, 0.5, 0.5) });
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
            if (VM.Settings_Double.Count < 7)
                return;

            if (VM.IsVertical)
            {
                if (cellListV.Count < 5)
                    return;

                double cx = VM.Settings_Double[1] + VM.Settings_Double[0];
                double cy = VM.Settings_Double[5] + VM.Settings_Double[6];
                if ((cx > 0) && (cy > 0))
                {
                    double w = VM.Settings_Double[1] / cx;
                    //5A4
                    cellListV[3].rcRatio = new Rect(0, 0, w, VM.Settings_Double[5] / cy);
                    //5A5
                    cellListV[4].rcRatio = new Rect(0, cellListV[3].rcRatio.Bottom, w, VM.Settings_Double[6] / cy);
                }

                cy = VM.Settings_Double[2] + VM.Settings_Double[3] + VM.Settings_Double[4];
                if ((cx > 0) && (cy > 0))
                {
                    double w = VM.Settings_Double[0] / cx;
                    double x = cellListV[3].rcRatio.Right;
                    //5A1
                    cellListV[0].rcRatio = new Rect(x, 0, w, VM.Settings_Double[2] / cx);
                    //5A2
                    cellListV[1].rcRatio = new Rect(x, cellListV[0].rcRatio.Bottom, w, VM.Settings_Double[3] / cx);
                    //5A3
                    cellListV[2].rcRatio = new Rect(x, cellListV[1].rcRatio.Bottom, w, VM.Settings_Double[4] / cy);
                }
            }
            else //Horz
            {
                if (cellListH.Count < 5)
                    return;

                double cx = VM.Settings_Double[2] + VM.Settings_Double[3] + VM.Settings_Double[4];
                double cy = VM.Settings_Double[0] + VM.Settings_Double[1];
                if ((cx > 0) && (cy > 0))
                {
                    double h = VM.Settings_Double[0] / cy;
                    //5a1
                    cellListH[0].rcRatio = new Rect(0, 0, VM.Settings_Double[2] / cx, h);
                    //5a2
                    cellListH[1].rcRatio = new Rect(cellListH[0].rcRatio.Right, 0, VM.Settings_Double[3] / cx, h);
                    //5a3
                    cellListH[2].rcRatio = new Rect(cellListH[1].rcRatio.Right, 0, VM.Settings_Double[4] / cx, h);
                }

                cx = VM.Settings_Double[5] + VM.Settings_Double[6];
                if ((cx > 0) && (cy > 0))
                {
                    double y = cellListH[0].rcRatio.Bottom;
                    double h = VM.Settings_Double[1] / cy;
                    //5a4
                    cellListH[3].rcRatio = new Rect(0, y, VM.Settings_Double[5] / cx, h);
                    //5a5
                    cellListH[4].rcRatio = new Rect(cellListH[3].rcRatio.Right, y, VM.Settings_Double[6] / cx, h);
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
            VSplitterList.Add(v3);
            VSplitterList.Add(V1);

            HSplitterList.Clear();
            HSplitterList.Add(h1);
            HSplitterList.Add(H1);
            HSplitterList.Add(H2);
            HSplitterList.Add(H3);
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

        public List<double> DefaultSettings => new List<double>() { 1, 1, 1, 1, 1, 1, 1 };
        #endregion Settings

        #region FriendlyName
        private string _friendlyName = string.Empty;
        private string _defaultHorzName = //LangHelper.Instance[$"EATooltip_51H"];
        "Option 5.1: 2 rows, split equally. Row 1, split equally in 3 sections. Row 2, split equally.";
        private string _defaultVertName = //LangHelper.Instance[$"EATooltip_51V"];
        "Option 5.1: 2 columns, split equallly. Column 1, split equally in 3 sections. Column 2, split equally.";
        //                                                              equally
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
        ~SplitCtrl5A()
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
