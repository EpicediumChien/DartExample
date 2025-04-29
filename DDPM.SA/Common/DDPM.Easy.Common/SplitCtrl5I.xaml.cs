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
    /// Interaction logic for SplitCtrl5I.xaml
    /// </summary>
    public partial class SplitCtrl5I : UserControl, ISplitCtrl, IDisposable
    {
        #region Private members
        private bool _isDisposed = false;
        #endregion Private members

        #region ctor
        public SplitCtrl5I()
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

        public string CtrlClass => nameof(SplitCtrl5I);
        public int CellCount => 5;
        public char SplitKey => 'I';
        public UserControl UC => this;

        //SplitCtrl2? ~ 7? are predefined layout, have default value, the EAID may be changed to [1000~1004] if they are customized.
        public int EAID { get; set; } = 28;
        public string TooltipResourceName { get; } = "EATooltip_59";
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
            SplitCtrl5I ctrl = new SplitCtrl5I();
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
            cellListH.Add(new CellObj("5i1", cell_5i1) { rcRatio = new Rect(0, 0, 1 / 2, 1 / 3) });
            cellListH.Add(new CellObj("5i2", cell_5i2) { rcRatio = new Rect(1 / 2, 0, 1 / 2, 1 / 3) });
            cellListH.Add(new CellObj("5i3", cell_5i3) { rcRatio = new Rect(1 / 2, 1 / 3, 1 / 2, 2 / 3) });
            cellListH.Add(new CellObj("5i4", cell_5i4) { rcRatio = new Rect(0, 1 / 3, 1 / 2, 1 / 3) });
            cellListH.Add(new CellObj("5i5", cell_5i5) { rcRatio = new Rect(0, 2 / 3, 1 / 2, 1 / 3) });

            cellListV.Clear();
            cellListV.Add(new CellObj("5I1", cell_5I1) { rcRatio = new Rect(2 / 3, 0, 1 / 3, 1 / 2) });
            cellListV.Add(new CellObj("5I2", cell_5I2) { rcRatio = new Rect(2 / 3, 1 / 2, 1 / 3, 1 / 2) });
            cellListV.Add(new CellObj("5I3", cell_5I3) { rcRatio = new Rect(0, 1 / 2, 2 / 3, 1 / 2) });
            cellListV.Add(new CellObj("5I4", cell_5I4) { rcRatio = new Rect(1 / 3, 0, 1 / 3, 1 / 2) });
            cellListV.Add(new CellObj("5I5", cell_5I5) { rcRatio = new Rect(0, 0, 1 / 3, 1 / 2) });
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
            if (VM.Settings_Double.Count < 6)
                return;

            //if (VM.IsVertical)
            //{
            //    if (cellListV.Count < 5)
            //        return;

            //    double cx = VM.Settings_Double[1] + VM.Settings_Double[0];
            //    double cy = VM.Settings_Double[2] + VM.Settings_Double[3];
            //    if ((cx > 0) && (cy > 0))
            //    {
            //        double x = VM.Settings_Double[1] / cx;
            //        double w = VM.Settings_Double[0] / cx;
            //        //5F1
            //        cellListV[0].rcRatio = new Rect(x, 0, w, VM.Settings_Double[2] / cy);
            //        //5F2
            //        cellListV[1].rcRatio = new Rect(x, cellListV[0].rcRatio.Bottom, w, VM.Settings_Double[3] / cy);
            //        //5F3
            //        cellListV[2].rcRatio = new Rect(0, cellListV[0].rcRatio.Bottom, VM.Settings_Double[1] / cx, VM.Settings_Double[3] / cy);
            //    }

            //    cx = (VM.Settings_Double[5] + VM.Settings_Double[4]);
            //    if (cx > 0)
            //    {
            //        double h = VM.Settings_Double[2] / cy;
            //        //5F5
            //        cellListV[4].rcRatio = new Rect(0, 0, VM.Settings_Double[5] / cx * cellListV[2].rcRatio.Width, h);
            //        //5F4
            //        cellListV[3].rcRatio = new Rect(cellListV[4].rcRatio.Right, 0, VM.Settings_Double[4] / cx * cellListV[2].rcRatio.Width, h);
            //    }
            //}
            //else //Horz
            //{
            //    if (cellListH.Count < 5)
            //        return;

            //    double cx = VM.Settings_Double[2] + VM.Settings_Double[3];
            //    double cy = VM.Settings_Double[0] + VM.Settings_Double[1];
            //    if ((cx > 0) && (cy > 0))
            //    {
            //        //5f1
            //        cellListH[0].rcRatio = new Rect(0, 0, VM.Settings_Double[2] / cx, VM.Settings_Double[0] / cy);
            //        //5f2
            //        cellListH[1].rcRatio = new Rect(cellListH[0].rcRatio.Right, 0, VM.Settings_Double[3] / cx, VM.Settings_Double[0] / cy);
            //        //5f3
            //        cellListH[2].rcRatio = new Rect(cellListH[0].rcRatio.Right, cellListH[1].rcRatio.Bottom, VM.Settings_Double[3] / cx, VM.Settings_Double[1] / cy);
            //    }

            //    cy = (VM.Settings_Double[4] + VM.Settings_Double[5]);
            //    if (cy > 0)
            //    {
            //        double w = VM.Settings_Double[2] / cx;
            //        double yRatio = cellListH[2].rcRatio.Height;

            //        //5f4
            //        cellListV[3].rcRatio = new Rect(0, cellListH[0].rcRatio.Bottom, w, VM.Settings_Double[4] / cy * cellListH[2].rcRatio.Height);
            //        //5f5
            //        cellListV[4].rcRatio = new Rect(0, cellListH[3].rcRatio.Bottom, w, VM.Settings_Double[5] / cy * cellListH[2].rcRatio.Height);
            //    }
            //}
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

        public List<double> DefaultSettings => new List<double>() { 3, 7, 7, 3, 1, 1 };
        #endregion Settings

        #region FriendlyName
        private string _friendlyName = string.Empty;
        private string _defaultHorzName = //LangHelper.Instance[$"EATooltip_59H"];
        "Option 5.9: 2 rows, split 70/30%. Row 1, split 70/30%, Row 2, split equally in 3 sections.";
        //                                                                                30/70%
        private string _defaultVertName = //LangHelper.Instance[$"EATooltip_59V"];
        "Option 5.9: 2 columns, split 70/30%. Column 1, split 70/30%, Column 2, split equally in 3 sections.";
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
        ~SplitCtrl5I()
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
