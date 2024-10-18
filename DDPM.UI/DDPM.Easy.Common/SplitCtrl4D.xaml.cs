using System.Windows.Controls;
using Rect = System.Windows.Rect;

namespace DDPM.Easy.Common
{
    /// <summary>
    /// Interaction logic for SplitCtrl4D.xaml
    /// </summary>
    public partial class SplitCtrl4D : UserControl, ISplitCtrl
    {
        #region ctor

        public SplitCtrl4D()
        {
            InitializeComponent();
            VM.Settings = SplitCtrlVM.Double_To_GridLength(DefaultSettings);
            DataContext = vm;
            InitCellList();
            InitSplitterList();
        }

        #endregion ctor

        #region ISplitCtrl Native Members

        public string CtrlClass => nameof(SplitCtrl4D);
        public int CellCount => 4;
        public char SplitKey => 'D';
        public UserControl UC => this;

        //SplitCtrl2? ~ 7? are predefined layout, have default value, the EAID may be changed to [1000~1004] if they are customized.
        public int EAID { get; set; } = 17;
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
            SplitCtrl4D ctrl = new SplitCtrl4D();
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
            cellListH.Add(new CellObj("4d1", cell_4d1) { rcRatio = new Rect(0, 0, 1/4, 1) });
            cellListH.Add(new CellObj("4d2", cell_4d2) { rcRatio = new Rect(1/4, 0, 1/4, 1) });
            cellListH.Add(new CellObj("4d3", cell_4d3) { rcRatio = new Rect(2/4, 0, 1 / 4, 1) });
            cellListH.Add(new CellObj("4d4", cell_4d4) { rcRatio = new Rect(3/4, 0, 1 / 4, 1) });

            cellListV.Clear();
            cellListV.Add(new CellObj("4D1", cell_4D1) { rcRatio = new Rect(0, 0, 1, 1/4) });
            cellListV.Add(new CellObj("4D2", cell_4D2) { rcRatio = new Rect(0, 1/4, 1, 1 / 4) });
            cellListV.Add(new CellObj("4D3", cell_4D3) { rcRatio = new Rect(0, 2/4, 1, 1 / 4) });
            cellListV.Add(new CellObj("4D4", cell_4D4) { rcRatio = new Rect(0, 3/4, 1, 1 / 4) });
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

            HSplitterList.Clear();
            HSplitterList.Add(H1);
            HSplitterList.Add(H2);
            HSplitterList.Add(H3);
        }

        #endregion Splitter List

        #region Settings

        public List<double> DefaultSettings => new List<double>() { 1, 1, 1, 1, 1 };
        #endregion Settings

        #region FriendlyName

        public string FriendlyName { get; set; }

        #endregion FriendlyName
    }
}