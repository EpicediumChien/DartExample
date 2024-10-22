using System.Windows.Controls;
using Rect = System.Windows.Rect;

namespace DDPM.Easy.Common
{
    /// <summary>
    /// Interaction logic for SplitCtrl2A.xaml
    /// </summary>
    public partial class SplitCtrl2A : UserControl, ISplitCtrl
    {
        #region ctor

        public SplitCtrl2A()
        {
            InitializeComponent();
            VM.Settings = SplitCtrlVM.Double_To_GridLength(DefaultSettings);
            DataContext = vm;
            InitCellList();
            InitSplitterList();
        }

        #endregion ctor

        #region ISplitCtrl Native Members

        public string CtrlClass => nameof(SplitCtrl2A);
        public int CellCount => 2;
        public char SplitKey => 'A';
        public UserControl UC => this;
        //SplitCtrl2? ~ 7? are predefined layout, have default value, the EAID may be changed to [1000~1004] if they are customized.
        public int EAID { get; set; } = 1;

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
            SplitCtrl2A ctrl = new SplitCtrl2A();
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
            cellListH.Add(new CellObj("2a1", cell_2a1) { rcRatio = new Rect(0,0,0.5,1)});
            cellListH.Add(new CellObj("2a2", cell_2a2) { rcRatio = new Rect(0.5, 0, 0.5, 1) });

            cellListV.Clear();
            cellListV.Add(new CellObj("2A1", cell_2A1) { rcRatio = new Rect(0, 0, 1, 0.5) });
            cellListV.Add(new CellObj("2A2", cell_2A2) { rcRatio = new Rect(0, 0.5, 1, 0.5) });

            //celBordersH.Clear();
            //celBordersH.Add(cellBd_2a1);
            //celBordersH.Add(cellBd_2a2);
        }

        public void UpdateSettingsToCells()
        {
            double sum = VM.Settings_Double.Sum();
            if (sum <= 0) 
                return;

            if (CellList.Count >= 2)
            {
                CellList[0].rcRatio = new Rect(
                    0, 0, VM.Settings_Double[0] / sum, 1);
                CellList[1].rcRatio = new Rect(
                    VM.Settings_Double[0] / sum, 0, 1, 1);
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

            HSplitterList.Clear();
            HSplitterList.Add(H1);
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