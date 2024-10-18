using System.Windows.Controls;
using Rect = System.Windows.Rect;

namespace DDPM.Easy.Common
{
    /// <summary>
    /// Interaction logic for SplitCtrl4C.xaml
    /// </summary>
    public partial class SplitCtrl4C : UserControl, ISplitCtrl
    {
        #region ctor

        public SplitCtrl4C()
        {
            InitializeComponent();
            VM.Settings = SplitCtrlVM.Double_To_GridLength(DefaultSettings);
            DataContext = vm;
            InitCellList();
            InitSplitterList();
        }

        #endregion ctor

        #region ISplitCtrl Native Members

        public string CtrlClass => nameof(SplitCtrl4C);
        public int CellCount => 4;
        public char SplitKey => 'C';
        public UserControl UC => this;

        //SplitCtrl2? ~ 7? are predefined layout, have default value, the EAID may be changed to [1000~1004] if they are customized.
        public int EAID { get; set; } = 16;
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
            VSplitterList.Add(V2);

            HSplitterList.Clear();
            HSplitterList.Add(h1);
            HSplitterList.Add(h2);
            HSplitterList.Add(H1);
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