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
    /// Interaction logic for SplitCtrl7J.xaml
    /// </summary>
    public partial class SplitCtrl7J : UserControl, ISplitCtrl
    {
        #region ctor
        public SplitCtrl7J()
        {
            InitializeComponent();
            VM.Settings = SplitCtrlVM.Double_To_GridLength(DefaultSettings);
            DataContext = vm;
            InitCellList();
            InitSplitterList();
        }
        #endregion ctor

        #region ISplitCtrl Native Members

        public string CtrlClass => nameof(SplitCtrl7J);
        public int CellCount => 7;
        public char SplitKey => 'J';
        public UserControl UC => this;

        //SplitCtrl2? ~ 7? are predefined layout, have default value, the EAID may be changed to [1000~1004] if they are customized.
        public int EAID { get; set; } = 48;
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
            SplitCtrl7J ctrl = new SplitCtrl7J();
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
            cellListH.Add(new CellObj("7j1", cell_7j1) { rcRatio = new Rect(0, 0, 1 / 3, 1 / 2) });
            cellListH.Add(new CellObj("7j2", cell_7j2) { rcRatio = new Rect(1 / 3, 0, 1 / 3, 1 / 2) });
            cellListH.Add(new CellObj("7j3", cell_7j3) { rcRatio = new Rect(2 / 3, 0, 1 / 3, 1 / 2) });
            cellListH.Add(new CellObj("7j4", cell_7j4) { rcRatio = new Rect(0, 1 / 2, 1 / 3, 1 / 2) });
            cellListH.Add(new CellObj("7j5", cell_7j5) { rcRatio = new Rect(1 / 3, 1 / 2, 1 / 3, 1 / 2) });
            cellListH.Add(new CellObj("7j6", cell_7j6) { rcRatio = new Rect(2 / 3, 1 / 2, 1 / 3, 1 / 2) });
            cellListH.Add(new CellObj("7j7", cell_7j7) { rcRatio = new Rect(2 / 3, 1 / 2, 1 / 3, 1 / 2) });
            cellListH.Add(new CellObj("7j8", cell_7j8) { rcRatio = new Rect(2 / 3, 1 / 2, 1 / 3, 1 / 2) });
            cellListH.Add(new CellObj("7j9", cell_7j9) { rcRatio = new Rect(2 / 3, 1 / 2, 1 / 3, 1 / 2) });
            cellListH.Add(new CellObj("7jA", cell_7jA) { rcRatio = new Rect(2 / 3, 1 / 2, 1 / 3, 1 / 2) });
            cellListH.Add(new CellObj("7jB", cell_7jB) { rcRatio = new Rect(2 / 3, 1 / 2, 1 / 3, 1 / 2) });
            cellListH.Add(new CellObj("7jC", cell_7jC) { rcRatio = new Rect(2 / 3, 1 / 2, 1 / 3, 1 / 2) });

            cellListV.Clear();
            cellListV.Add(new CellObj("7J1", cell_7J1) { rcRatio = new Rect(0, 2 / 3, 1 / 2, 1 / 3) });
            cellListV.Add(new CellObj("7J2", cell_7J2) { rcRatio = new Rect(0, 1 / 3, 1 / 2, 1 / 3) });
            cellListV.Add(new CellObj("7J3", cell_7J3) { rcRatio = new Rect(0, 0, 1 / 2, 1 / 3) });
            cellListV.Add(new CellObj("7J4", cell_7J4) { rcRatio = new Rect(1 / 2, 2 / 3, 1 / 2, 1 / 3) });
            cellListV.Add(new CellObj("7J5", cell_7J5) { rcRatio = new Rect(1 / 2, 1 / 3, 1 / 2, 1 / 3) });
            cellListV.Add(new CellObj("7J6", cell_7J6) { rcRatio = new Rect(1 / 2, 0, 1 / 2, 1 / 3) });
            cellListV.Add(new CellObj("7J7", cell_7J7) { rcRatio = new Rect(1 / 2, 0, 1 / 2, 1 / 3) });
            cellListV.Add(new CellObj("7J8", cell_7J8) { rcRatio = new Rect(1 / 2, 0, 1 / 2, 1 / 3) });
            cellListV.Add(new CellObj("7J9", cell_7J9) { rcRatio = new Rect(1 / 2, 0, 1 / 2, 1 / 3) });
            cellListV.Add(new CellObj("7JA", cell_7JA) { rcRatio = new Rect(1 / 2, 0, 1 / 2, 1 / 3) });
            cellListV.Add(new CellObj("7JB", cell_7JB) { rcRatio = new Rect(1 / 2, 0, 1 / 2, 1 / 3) });
            cellListV.Add(new CellObj("7JC", cell_7JC) { rcRatio = new Rect(1 / 2, 0, 1 / 2, 1 / 3) });
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
                if (cellListV.Count < 6)
                    return;

                //double cx = VM.Settings_Double[1] + VM.Settings_Double[0];
                //double cy = VM.Settings_Double[2] + VM.Settings_Double[3];
                //if ((cx > 0) && (cy > 0))
                //{
                //    double x = VM.Settings_Double[1] / cx;
                //    double w = VM.Settings_Double[0] / cx;
                //    5F1
                //    cellListV[0].rcRatio = new Rect(x, 0, w, VM.Settings_Double[2] / cy);
                //    5F2
                //    cellListV[1].rcRatio = new Rect(x, cellListV[0].rcRatio.Bottom, w, VM.Settings_Double[3] / cy);
                //    5F3
                //    cellListV[2].rcRatio = new Rect(0, cellListV[0].rcRatio.Bottom, VM.Settings_Double[1] / cx, VM.Settings_Double[3] / cy);
                //}

                //cx = (VM.Settings_Double[5] + VM.Settings_Double[4]);
                //if (cx > 0)
                //{
                //    double h = VM.Settings_Double[2] / cy;
                //    5F5
                //    cellListV[4].rcRatio = new Rect(0, 0, VM.Settings_Double[5] / cx * cellListV[2].rcRatio.Width, h);
                //    5F4
                //    cellListV[3].rcRatio = new Rect(cellListV[4].rcRatio.Right, 0, VM.Settings_Double[4] / cx * cellListV[2].rcRatio.Width, h);
                //}
            }
            else //Horz
            {
                if (cellListH.Count < 6)
                    return;

                //double cx = VM.Settings_Double[2] + VM.Settings_Double[3];
                //double cy = VM.Settings_Double[0] + VM.Settings_Double[1];
                //if ((cx > 0) && (cy > 0))
                //{
                //    //5f1
                //    cellListH[0].rcRatio = new Rect(0, 0, VM.Settings_Double[2] / cx, VM.Settings_Double[0] / cy);
                //    //5f2
                //    cellListH[1].rcRatio = new Rect(cellListH[0].rcRatio.Right, 0, VM.Settings_Double[3] / cx, VM.Settings_Double[0] / cy);
                //    //5f3
                //    cellListH[2].rcRatio = new Rect(cellListH[0].rcRatio.Right, cellListH[1].rcRatio.Bottom, VM.Settings_Double[3] / cx, VM.Settings_Double[1] / cy);
                //}

                //cy = (VM.Settings_Double[4] + VM.Settings_Double[5]);
                //if (cy > 0)
                //{
                //    double w = VM.Settings_Double[2] / cx;
                //    double yRatio = cellListH[2].rcRatio.Height;

                //    //5f4
                //    cellListV[3].rcRatio = new Rect(0, cellListH[0].rcRatio.Bottom, w, VM.Settings_Double[4] / cy * cellListH[2].rcRatio.Height);
                //    //5f5
                //    cellListV[4].rcRatio = new Rect(0, cellListH[3].rcRatio.Bottom, w, VM.Settings_Double[5] / cy * cellListH[2].rcRatio.Height);
                //}
            }
        }

        #endregion Cell List

        #region CellBorders
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
            VSplitterList.Add(V2);

            HSplitterList.Clear();
            HSplitterList.Add(h1);
            HSplitterList.Add(h2);
            HSplitterList.Add(H1);
            HSplitterList.Add(H2);
            HSplitterList.Add(H3);
        }

        #endregion Splitter List

        #region Settings

        public List<double> DefaultSettings => new List<double>() { 1, 1, 1, 1, 1, 1, 1 };
        #endregion Settings

        #region FriendlyName
        private string _friendlyName = string.Empty;
        private string _defaultHorzName = "Option 7.10: 4 columns, split equally, Columns 1, 2, 3, and 4, each split equally in 3 sections.";
        private string _defaultVertName = "Option 7.10: 4 columns, split equally, Columns 1, 2, 3, and 4, each split equally in 3 sections.";
        public string FriendlyName
        {
            get
            {
                if (String.IsNullOrEmpty(_friendlyName))
                {
                    if (VM.IsVertical)
                        return _defaultVertName;
                    else
                        return _defaultHorzName;
                }
                return _friendlyName;
            }
            set
            {
                _friendlyName = value;
            }
        }
        #endregion FriendlyName      
    }
}
