using System.Windows.Controls;
using Rect = System.Windows.Rect;

namespace DDPM.Easy.Common
{
    /// <summary>
    /// Interaction logic for SplitCtrl4B.xaml
    /// </summary>
    public partial class SplitCtrl4B : UserControl, ISplitCtrl
    {
        #region ctor

        public SplitCtrl4B()
        {
            InitializeComponent();
            VM.Settings = SplitCtrlVM.Double_To_GridLength(DefaultSettings);
            DataContext = vm;
            InitCellList();
            InitSplitterList();
        }

        #endregion ctor

        #region ISplitCtrl Native Members

        public string CtrlClass => nameof(SplitCtrl4B);
        public int CellCount => 4;
        public char SplitKey => 'B';
        public UserControl UC => this;
        //SplitCtrl2? ~ 7? are predefined layout, have default value, the EAID may be changed to [1000~1004] if they are customized.
        public int EAID { get; set; } = 15;

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
            SplitCtrl4B ctrl = new SplitCtrl4B();
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
            cellListH.Add(new CellObj("4b1", cell_4b1) { rcRatio = new Rect(0, 0, 0.5, 1/3) });
            cellListH.Add(new CellObj("4b2", cell_4b2) { rcRatio = new Rect(0, 1/3, 0.5, 1 / 3) });
            cellListH.Add(new CellObj("4b3", cell_4b3) { rcRatio = new Rect(0, 2/3, 0.5, 1 / 3) });
            cellListH.Add(new CellObj("4b4", cell_4b4) { rcRatio = new Rect(0.5, 0, 0.5, 1) });

            cellListV.Clear();
            cellListV.Add(new CellObj("4B1", cell_4B1) { rcRatio = new Rect(2/3, 0, 1/3, 0.5) });
            cellListV.Add(new CellObj("4B2", cell_4B2) { rcRatio = new Rect(1/3, 0, 1/3, 0.5) });
            cellListV.Add(new CellObj("4B3", cell_4B3) { rcRatio = new Rect(0, 0, 1/3, 0.5) });
            cellListV.Add(new CellObj("4B4", cell_4B4) { rcRatio = new Rect(0, 0.5, 1, 0.5) });
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
        private string _friendlyName = string.Empty;
        private string _defaultHorzName = "Option 4.2 2 columns, equal splits. Column 1, 3 equal splits. Column 2, no split.";
        private string _defaultVertName = "Option 4.2 2 rows, equal splits. Row 1, no split. Row 2, equal splits.";
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