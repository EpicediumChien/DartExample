using System.Windows.Controls;

namespace DDPM.Easy.Common
{
    /// <summary>
    /// Interaction logic for SplitCtrl2B.xaml
    /// </summary>
    public partial class SplitCtrl2B : UserControl, ISplitCtrl
    {
        #region ctor

        public SplitCtrl2B()
        {
            InitializeComponent();
            VM.Settings = SplitCtrlVM.Double_To_GridLength(DefaultSettings);
            DataContext = vm;
            InitCellList();
            InitSplitterList();
        }

        #endregion ctor

        #region ISplitCtrl Native Members

        public string CtrlClass => nameof(SplitCtrl2B);
        public int CellCount => 2;
        public char SplitKey => 'B';
        public UserControl UC => this;

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
            SplitCtrl2B ctrl = new SplitCtrl2B();
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
            cellListH.Add(new CellObj("2b1", cell_2b1));
            cellListH.Add(new CellObj("2b2", cell_2b2));

            cellListV.Clear();
            cellListV.Add(new CellObj("2B1", cell_2B1));
            cellListV.Add(new CellObj("2B2", cell_2B2));
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
            VSplitterList.Add(V1);

            HSplitterList.Clear();
            HSplitterList.Add(h1);
        }

        #endregion Splitter List

        #region Settings

        public List<double> DefaultSettings => new List<double>() { 1, 1 };

        #endregion Settings

        #region FriendlyName

        public string FriendlyName { get; set; }

        #endregion FriendlyName
    }
}