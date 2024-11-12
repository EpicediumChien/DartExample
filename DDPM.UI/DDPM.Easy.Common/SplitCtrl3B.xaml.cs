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
    /// Interaction logic for SplitCtrl3B.xaml
    /// </summary>
    public partial class SplitCtrl3B : UserControl, ISplitCtrl
    {
        #region ctor
        public SplitCtrl3B()
        {
            InitializeComponent();
            VM.Settings = SplitCtrlVM.Double_To_GridLength(DefaultSettings);
            DataContext = vm;
            InitCellList();
            InitSplitterList();
        }
        #endregion ctor

        #region ISplitCtrl Native Members
        public string CtrlClass => nameof(SplitCtrl3B);
        public int CellCount => 3;
        public char SplitKey => 'B';
        public UserControl UC => this;
        //SplitCtrl2? ~ 7? are predefined layout, have default value, the EAID may be changed to [1000~1004] if they are customized.
        public int EAID { get; set; } = 6;
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
            SplitCtrl3B ctrl = new SplitCtrl3B();
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
            cellListH.Add(new CellObj("3b1", cell_3b1) { rcRatio = new Rect(0, 0, 1/3, 1) });
            cellListH.Add(new CellObj("3b2", cell_3b2) { rcRatio = new Rect(1/3, 0, 1/3, 1) });
            cellListH.Add(new CellObj("3b3", cell_3b3) { rcRatio = new Rect(2/3, 0, 1/3, 1) });

            cellListV.Clear();
            cellListV.Add(new CellObj("3B1", cell_3B1) { rcRatio = new Rect(0, 0, 1, 1 / 3) });
            cellListV.Add(new CellObj("3B2", cell_3B2) { rcRatio = new Rect(0, 1/3, 1, 1 / 3) });
            cellListV.Add(new CellObj("3B3", cell_3B3) { rcRatio = new Rect(0, 2/3, 1, 1 / 3) });
        }
        /// <summary>
        /// Convert ISplitCtrl.Settings to CellList[i].rcRect
        /// </summary>
        public void UpdateRatioRectsFromSettings()
        {
            if (VM.Settings_Double.Count < 3)
                return;

            if (VM.IsVertical)
            {
                if (cellListV.Count < 3)
                    return;

                //double cx = VM.Settings_Double[1] + VM.Settings_Double[0];
                double cy = VM.Settings_Double[0] + VM.Settings_Double[1] + VM.Settings_Double[2];
                if (cy > 0)
                {
                    //double w = VM.Settings_Double[1] / cx;
                    //3B1
                    cellListV[0].rcRatio = new Rect(0, 0, 1, VM.Settings_Double[0] / cy);
                    //3B2
                    cellListV[1].rcRatio = new Rect(0, cellListV[0].rcRatio.Bottom, 1, VM.Settings_Double[1] / cy);
                    //3B3
                    cellListV[2].rcRatio = new Rect(0, cellListV[1].rcRatio.Bottom, 1, VM.Settings_Double[2] / cy);
                }
            }
            else //Horz
            {
                if (cellListH.Count < 3)
                    return;

                double cx = VM.Settings_Double[0] + VM.Settings_Double[1] + VM.Settings_Double[2];
                //double cy = VM.Settings_Double[0] + VM.Settings_Double[1];
                if (cx > 0)
                {
                    //double h = VM.Settings_Double[0] / cy;
                    //3b1
                    cellListH[0].rcRatio = new Rect(0, 0, VM.Settings_Double[0] / cx, 1);
                    //3b2
                    cellListH[1].rcRatio = new Rect(cellListH[0].rcRatio.Right, 0, VM.Settings_Double[1] / cx, 1);
                    //3b3
                    cellListH[2].rcRatio = new Rect(cellListH[1].rcRatio.Right, 0, VM.Settings_Double[2] / cx, 1);
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

            HSplitterList.Clear();
            HSplitterList.Add(H1);
            HSplitterList.Add(H2);
        }
        #endregion Splitter List

        #region Settings
        public List<double> DefaultSettings => new List<double>() { 1, 1, 1 };
        #endregion Settings

        #region FriendlyName
        private string _friendlyName = string.Empty;
        private string _defaultHorzName = "Option 3.2: 3 columns, split equally.";
        private string _defaultVertName = "Option 3.2: 3 rows, split equally.";
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
