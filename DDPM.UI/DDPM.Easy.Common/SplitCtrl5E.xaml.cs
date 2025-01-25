using DDPM.UI.Resources.Helper;
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
using Windows.Devices.Radios;

namespace DDPM.Easy.Common
{
    /// <summary>
    /// Interaction logic for SplitCtrl5E.xaml
    /// </summary>
    public partial class SplitCtrl5E : UserControl, ISplitCtrl
    {
        #region ctor
        public SplitCtrl5E()
        {
            InitializeComponent();
            VM.Settings = SplitCtrlVM.Double_To_GridLength(DefaultSettings);
            DataContext = vm;
            InitCellList();
            InitSplitterList();
        }
        #endregion ctor

        #region ISplitCtrl Native Members

        public string CtrlClass => nameof(SplitCtrl5E);
        public int CellCount => 5;
        public char SplitKey => 'E';
        public UserControl UC => this;

        //SplitCtrl2? ~ 7? are predefined layout, have default value, the EAID may be changed to [1000~1004] if they are customized.
        public int EAID { get; set; } = 24;
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
            SplitCtrl5E ctrl = new SplitCtrl5E();
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
            cellListH.Add(new CellObj("5e1", cell_5e1) { rcRatio = new Rect(0, 0, 1 / 2, 1 / 3) });
            cellListH.Add(new CellObj("5e2", cell_5e2) { rcRatio = new Rect(1 / 2, 0, 1 / 2, 1 / 3) });
            cellListH.Add(new CellObj("5e3", cell_5e3) { rcRatio = new Rect(0, 1 / 3, 1 / 2, 2 / 3) });
            cellListH.Add(new CellObj("5e4", cell_5e4) { rcRatio = new Rect(1 / 2, 1 / 3, 1 / 2, 1 / 3) });
            cellListH.Add(new CellObj("5e5", cell_5e5) { rcRatio = new Rect(1 / 2, 2 / 3, 1 / 2, 1 / 3) });

            cellListV.Clear();
            cellListV.Add(new CellObj("5E1", cell_5E1) { rcRatio = new Rect(2 / 3, 0, 1 / 3, 1 / 2) });
            cellListV.Add(new CellObj("5E2", cell_5E2) { rcRatio = new Rect(2 / 3, 1 / 2, 1 / 3, 1 / 2) });
            cellListV.Add(new CellObj("5E3", cell_5E3) { rcRatio = new Rect(0, 0, 2 / 3, 1 / 2) });
            cellListV.Add(new CellObj("5E4", cell_5E4) { rcRatio = new Rect(1/3, 1 / 2, 1 / 3, 1 / 2) });
            cellListV.Add(new CellObj("5E5", cell_5E5) { rcRatio = new Rect(0, 1 / 2, 1 / 3, 1 / 2) });
        }

        /// <summary>
        /// Convert ISplitCtrl.Settings to CellList[i].rcRect
        /// </summary>
        public void UpdateRatioRectsFromSettings()
        {
            if (VM.Settings_Double.Count < 6)
                return;

            if (VM.IsVertical)
            {
                if (cellListV.Count < 5)
                    return;

                double cx = VM.Settings_Double[1] + VM.Settings_Double[0];
                double cy = VM.Settings_Double[2] + VM.Settings_Double[3];
                if ((cx > 0) && (cy > 0))
                {
                    //5E3
                    cellListV[2].rcRatio = new Rect(0, 0, VM.Settings_Double[1] / cx, VM.Settings_Double[2] / cy);
                    double x = cellListV[2].rcRatio.Right;
                    double w = VM.Settings_Double[0] / cx;
                    //5E1
                    cellListV[0].rcRatio = new Rect(x, 0, w, VM.Settings_Double[2] / cy);
                    //5E2
                    cellListV[1].rcRatio = new Rect(x, cellListV[0].rcRatio.Bottom, w, VM.Settings_Double[3] / cy);
                }
                cx = (VM.Settings_Double[5] + VM.Settings_Double[4]);
                if (cx > 0)
                {
                    double xRatio = cellListV[2].rcRatio.Width;
                    double y = cellListV[2].rcRatio.Bottom;
                    double h = VM.Settings_Double[3] / cy;

                    //5E5
                    cellListV[4].rcRatio = new Rect(0, y, VM.Settings_Double[5] / cx * xRatio, h);
                    //5E4
                    cellListV[3].rcRatio = new Rect(cellListV[4].rcRatio.Right, y, VM.Settings_Double[4] / cx * xRatio, h);
                }
            }
            else //Horz
            {
                if (cellListH.Count < 5)
                    return;

                double cx = VM.Settings_Double[2] + VM.Settings_Double[3];
                double cy = VM.Settings_Double[0] + VM.Settings_Double[1];
                if ((cx > 0) && (cy > 0))
                {
                    //5e1
                    cellListH[0].rcRatio = new Rect(0, 0, VM.Settings_Double[2] / cx, VM.Settings_Double[0] / cy);
                    //5e2
                    cellListH[1].rcRatio = new Rect(cellListH[0].rcRatio.Right, 0, VM.Settings_Double[3] / cx, VM.Settings_Double[0] / cy);
                    //5e3
                    cellListH[2].rcRatio = new Rect(0, cellListH[0].rcRatio.Bottom, VM.Settings_Double[2] / cx, VM.Settings_Double[1] / cy);
                }

                cy = (VM.Settings_Double[4] + VM.Settings_Double[5]);
                if (cy > 0)
                {
                    double yRatio = cellListH[2].rcRatio.Height;

                    //5e4
                    cellListV[3].rcRatio = new Rect(cellListH[2].rcRatio.Right, cellListH[1].rcRatio.Bottom, cellListH[1].rcRatio.Width, VM.Settings_Double[4] / cx * yRatio);
                    //5e5
                    cellListV[4].rcRatio = new Rect(cellListH[2].rcRatio.Right, cellListV[3].rcRatio.Bottom, cellListH[1].rcRatio.Width, VM.Settings_Double[5] / cx * yRatio);
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
            VSplitterList.Add(V2);

            HSplitterList.Clear();
            HSplitterList.Add(h1);
            HSplitterList.Add(h2);
            HSplitterList.Add(H1);
        }

        #endregion Splitter List

        #region Settings

        public List<double> DefaultSettings => new List<double>() { 3, 7, 1, 1, 1, 1 };
        #endregion Settings

        #region FriendlyName
        private string _friendlyName = string.Empty;
        private string _defaultHorzName = LangHelper.Instance[$"EATooltip_55H"];
        //"Option 5.5: 2 rows, split 30/70%. Rows 1 and 2, each split equally. Column 1, no split. Column 2, split equally.";
        //                                                                   Row 1, split equally. Row 2, Column 1, no split. Column 2, split eaually.
        private string _defaultVertName = LangHelper.Instance[$"EATooltip_55V"];
        //"Option 5.5: 2 rows, split equally. Row 1, split equally in 3 sections. Row 2, split 30/70%.";
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
