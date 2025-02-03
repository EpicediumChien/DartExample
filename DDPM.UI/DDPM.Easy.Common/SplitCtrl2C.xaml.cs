using DDPM.UI.Resources.Helper;
using System.Windows.Controls;
using Rect = System.Windows.Rect;

namespace DDPM.Easy.Common
{
    /// <summary>
    /// Interaction logic for SplitCtrl2C.xaml
    /// </summary>
    public partial class SplitCtrl2C : UserControl, ISplitCtrl
    {
        #region ctor

        public SplitCtrl2C()
        {
            InitializeComponent();
            VM.Settings = SplitCtrlVM.Double_To_GridLength(DefaultSettings);
            DataContext = vm;
            InitCellList();
            InitSplitterList();
        }

        #endregion ctor

        #region ISplitCtrl Native Members

        public string CtrlClass => nameof(SplitCtrl2C);
        public int CellCount => 2;
        public char SplitKey => 'C';
        public UserControl UC => this;
        //SplitCtrl2? ~ 7? are predefined layout, have default value, the EAID may be changed to [1000~1004] if they are customized.
        public int EAID { get; set; } = 3;

        public string TooltipResourceName { get; } = "EATooltip_23";
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
            SplitCtrl2C ctrl = new SplitCtrl2C();
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
            cellListH.Add(new CellObj("2c1", cell_2c1) { rcRatio = new Rect(0, 0, 0.7, 1) });
            cellListH.Add(new CellObj("2c2", cell_2c2) { rcRatio = new Rect(0.7, 0, 0.3, 1) });

            cellListV.Clear();
            cellListV.Add(new CellObj("2C1", cell_2C1) { rcRatio = new Rect(0, 0, 1, 0.7) });
            cellListV.Add(new CellObj("2C2", cell_2C2) { rcRatio = new Rect(0, 0.7, 1, 0.3) });
        }

        /// <summary>
        /// Convert ISplitCtrl.Settings to CellList[i].rcRect
        /// </summary>
        public void UpdateRatioRectsFromSettings()
        {
            if (VM.Settings_Double.Count < 2)
                return;

            if (VM.IsVertical)
            {
                if (cellListV.Count < 2)
                    return;

                //double cx = VM.Settings_Double[1] + VM.Settings_Double[0];
                double cy = VM.Settings_Double[0] + VM.Settings_Double[1];
                if (cy > 0)
                {
                    //double w = VM.Settings_Double[1] / cx;
                    //2C1
                    cellListV[0].rcRatio = new Rect(0, 0, 1, VM.Settings_Double[0] / cy);
                    //2C2
                    cellListV[1].rcRatio = new Rect(0, cellListV[0].rcRatio.Bottom, 1, VM.Settings_Double[1] / cy);
                }
            }
            else //Horz
            {
                if (cellListH.Count < 2)
                    return;

                double cx = VM.Settings_Double[0] + VM.Settings_Double[1];
                //double cy = VM.Settings_Double[0] + VM.Settings_Double[1];
                if (cx > 0)
                {
                    //double h = VM.Settings_Double[0] / cy;
                    //2c1
                    cellListH[0].rcRatio = new Rect(0, 0, VM.Settings_Double[0] / cx, 1);
                    //2c2
                    cellListH[1].rcRatio = new Rect(cellListH[0].rcRatio.Right, 0, VM.Settings_Double[1] / cx, 1);
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

            HSplitterList.Clear();
            HSplitterList.Add(H1);
        }

        #endregion Splitter List

        #region Settings

        public List<double> DefaultSettings => new List<double>() { 7, 3 };

        #endregion Settings

        #region FriendlyName
        private string _friendlyName = string.Empty;
        private string _defaultHorzName = //LangHelper.Instance[$"EATooltip_23H"];
        "Option 2.3: 2 columns, split 70/30%.";
        private string _defaultVertName = //LangHelper.Instance[$"EATooltip_23V"];
        "Option 2.3: 2 rows, split 30/70%.";
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
    }
}