using System.Windows.Controls;
using Rect = System.Windows.Rect;

namespace DDPM.Easy.Common
{
    /// <summary>
    /// Interaction logic for SplitCtrl4E.xaml
    /// </summary>
    public partial class SplitCtrl4E : UserControl, ISplitCtrl
    {
        #region ctor

        public SplitCtrl4E()
        {
            InitializeComponent();
            VM.Settings = SplitCtrlVM.Double_To_GridLength(DefaultSettings);
            DataContext = vm;
            InitCellList();
            InitSplitterList();
        }

        #endregion ctor

        #region ISplitCtrl Native Members

        public string CtrlClass => nameof(SplitCtrl4E);
        public int CellCount => 4;
        public char SplitKey => 'E';
        public UserControl UC => this;

        //SplitCtrl2? ~ 7? are predefined layout, have default value, the EAID may be changed to [1000~1004] if they are customized.
        public int EAID { get; set; } = 18;
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
            SplitCtrl4E ctrl = new SplitCtrl4E();
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
            cellListH.Add(new CellObj("4e1", cell_4e1) { rcRatio = new Rect(0, 0, 1 / 3, 0.5) });
            cellListH.Add(new CellObj("4e2", cell_4e2) { rcRatio = new Rect(0, 0.5, 1 / 3, 0.5) });
            cellListH.Add(new CellObj("4e3", cell_4e3) { rcRatio = new Rect(1/3, 0, 1 / 3, 1) });
            cellListH.Add(new CellObj("4e4", cell_4e4) { rcRatio = new Rect(2/3, 0, 1 / 3, 1) });

            cellListV.Clear();
            cellListV.Add(new CellObj("4E1", cell_4E1) { rcRatio = new Rect(0.5, 0, 0.5, 1/3) });
            cellListV.Add(new CellObj("4E2", cell_4E2) { rcRatio = new Rect(0, 0, 0.5, 1/3) });
            cellListV.Add(new CellObj("4E3", cell_4E3) { rcRatio = new Rect(0, 1/3, 1, 1/3) });
            cellListV.Add(new CellObj("4E4", cell_4E4) { rcRatio = new Rect(0, 2/3, 1, 1/3) });
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
                    //4E2
                    cellListV[0].rcRatio = new Rect(0, 0, VM.Settings_Double[4] / cx, h);
                    //4E1
                    cellListV[1].rcRatio = new Rect(cellListV[0].rcRatio.Right, 0, VM.Settings_Double[3] / cx, h);

                    //4E3
                    cellListV[2].rcRatio = new Rect(0, cellListV[0].rcRatio.Bottom, 1, VM.Settings_Double[1] / cy);
                    //4E4
                    cellListV[3].rcRatio = new Rect(0, cellListV[2].rcRatio.Bottom, 1, VM.Settings_Double[2] / cy);
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
                    //4e1
                    cellListH[0].rcRatio = new Rect(0, 0, w, VM.Settings_Double[3]/cy);
                    //4e2
                    cellListH[1].rcRatio = new Rect(0, cellListH[0].rcRatio.Bottom, w, VM.Settings_Double[4] / cy);
                    //4e3
                    cellListH[2].rcRatio = new Rect(cellListH[1].rcRatio.Right, 0, VM.Settings_Double[1] / cx, 1);
                    //4e4
                    cellListH[3].rcRatio = new Rect(cellListH[2].rcRatio.Right, 0, VM.Settings_Double[2] / cx, 1);
                }
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
            VSplitterList.Add(V1);

            HSplitterList.Clear();
            HSplitterList.Add(h1);
            HSplitterList.Add(H1);
            HSplitterList.Add(H2);
        }

        #endregion Splitter List

        #region Settings

        public List<double> DefaultSettings => new List<double>() { 1, 1, 1, 1, 1 };
        #endregion Settings


        #region FriendlyName
        private string _friendlyName = string.Empty;
        private string _defaultHorzName = "Option 4.5: 3 columns split equally. Column 1, split equally. Columns 2 and 3, no split.";
        private string _defaultVertName = "Option 4.5: 3 rows, split equally.  Rows 1 and 2, no split. Row 3, split equally.";
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