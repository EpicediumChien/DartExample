using System.Windows.Controls;
using Rect = System.Windows.Rect;

namespace DDPM.Easy.Common
{
    /// <summary>
    /// Interaction logic for SplitCtrl4A.xaml
    /// </summary>
    public partial class SplitCtrl4A : UserControl, ISplitCtrl
    {
        #region ctor

        public SplitCtrl4A()
        {
            InitializeComponent();
            VM.Settings = SplitCtrlVM.Double_To_GridLength(DefaultSettings);
            DataContext = vm;
            InitCellList();
            InitSplitterList();
        }

        #endregion ctor

        #region ISplitCtrl Native Members

        public string CtrlClass => nameof(SplitCtrl4A);
        public int CellCount => 4;
        public char SplitKey => 'A';
        public UserControl UC => this;

        //SplitCtrl2? ~ 7? are predefined layout, have default value, the EAID may be changed to [1000~1004] if they are customized.
        public int EAID { get; set; } = 14;
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
            VSplitterList.Add(V1);

            HSplitterList.Clear();
            HSplitterList.Add(h1);
            HSplitterList.Add(H1);
        }

        #endregion Splitter List

        #region Settings

        public List<double> DefaultSettings => new List<double>() { 1, 1, 1, 1, 1 };
        #endregion Settings

        #region FriendlyName
        private string _friendlyName = string.Empty;
        private string _defaultHorzName = "Option 4.1 4 quadrant splits.";
        private string _defaultVertName = "Option 4.1 4 quadrant splits.";
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