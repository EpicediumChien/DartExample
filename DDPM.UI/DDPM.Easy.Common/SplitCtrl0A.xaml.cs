using System.Windows.Controls;

namespace DDPM.Easy.Common
{
    /// <summary>
    /// Interaction logic for SplitCtrl0A.xaml
    /// </summary>
    public partial class SplitCtrl0A : UserControl, ISplitCtrl
    {
        #region ctor

        public SplitCtrl0A()
        {
            InitializeComponent();
            VM.Settings = SplitCtrlVM.Double_To_GridLength(DefaultSettings);
            DataContext = vm;
            InitCellList();
            InitSplitterList();
        }

        #endregion ctor

        #region ISplitCtrl Native Members

        public string CtrlClass => nameof(SplitCtrl0A);
        public int CellCount => 0;
        public char SplitKey => 'A';
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
            SplitCtrl0A ctrl = new SplitCtrl0A();
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

            cellListV.Clear();
        }

        #endregion Cell List

        #region Splitter List

        public List<GridSplitter> VSplitterList { get; set; } = new List<GridSplitter>();
        public List<GridSplitter> HSplitterList { get; set; } = new List<GridSplitter>();

        public void InitSplitterList()
        {
            VSplitterList.Clear();

            HSplitterList.Clear();
        }

        #endregion Splitter List

        #region Settings

        public List<double> DefaultSettings => new List<double>() { };

        public List<double> Settings
        {
            get => DefaultSettings;
            set { }
        }

        #endregion Settings

        #region FriendlyName

        public string FriendlyName { get; set; }

        #endregion FriendlyName
    }
}