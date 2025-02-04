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

namespace DDPM.Easy.Common
{
    /// <summary>
    /// Interaction logic for SplitCtrl3G.xaml
    /// </summary>
    public partial class SplitCtrl3G : UserControl, ISplitCtrl
    {
        #region ctor
        public SplitCtrl3G()
        {
            InitializeComponent();
            VM.Settings = SplitCtrlVM.Double_To_GridLength(DefaultSettings);
            DataContext = vm;
            InitCellList();
            InitSplitterList();
        }
        #endregion ctor

        #region ISplitCtrl Native Members
        public string CtrlClass => nameof(SplitCtrl3G);
        public int CellCount => 3;
        public char SplitKey => 'G';
        public UserControl UC => this;
        //SplitCtrl2? ~ 7? are predefined layout, have default value, the EAID may be changed to [1000~1004] if they are customized.
        public int EAID { get; set; } = 11;
        public string TooltipResourceName { get; } = "EATooltip_37";
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
            SplitCtrl3G ctrl = new SplitCtrl3G();
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
            cellListH.Add(new CellObj("3g1", cell_3g1) { rcRatio = new Rect(0, 0, 0.7, 1) });
            cellListH.Add(new CellObj("3g2", cell_3g2) { rcRatio = new Rect(0.7, 0, 0.3, 0.5) });
            cellListH.Add(new CellObj("3g3", cell_3g3) { rcRatio = new Rect(0.7, 0.5, 0.3, 0.5) });

            cellListV.Clear();
            cellListV.Add(new CellObj("3G1", cell_3G1) { rcRatio = new Rect(0, 0, 1, 0.7) });
            cellListV.Add(new CellObj("3G2", cell_3G2) { rcRatio = new Rect(0.5, 0.7, 0.5, 0.3) });
            cellListV.Add(new CellObj("3G3", cell_3G3) { rcRatio = new Rect(0, 0.7, 0.5, 0.3) });
        }
        /// <summary>
        /// Convert ISplitCtrl.Settings to CellList[i].rcRect
        /// </summary>
        public void UpdateRatioRectsFromSettings()
        {
            if (VM.Settings_Double.Count < 4)
                return;

            if (VM.IsVertical)
            {
                if (cellListV.Count < 3)
                    return;

                double cx = VM.Settings_Double[1] + VM.Settings_Double[0];
                double cy = VM.Settings_Double[2] + VM.Settings_Double[3];
                if ((cx > 0) && (cy > 0))
                {
                    //3G1
                    cellListV[0].rcRatio = new Rect(0, 0, 1, VM.Settings_Double[2] / cy);
                    double h = VM.Settings_Double[3] / cy;
                    double y = cellListV[0].rcRatio.Bottom;
                    //3G3
                    cellListV[2].rcRatio = new Rect(0, y, VM.Settings_Double[1] / cx, h);
                    //3G2
                    cellListV[1].rcRatio = new Rect(cellListV[2].rcRatio.Right, y, VM.Settings_Double[0] / cx, h);
                }
            }
            else //Horz
            {
                if (cellListH.Count < 3)
                    return;

                double cx = VM.Settings_Double[2] + VM.Settings_Double[3];
                double cy = VM.Settings_Double[0] + VM.Settings_Double[1];
                if ((cx > 0) && (cy > 0))
                {
                    //3g1
                    cellListH[0].rcRatio = new Rect(0, 0, VM.Settings_Double[2] / cx, 1);
                    double x = cellListH[0].rcRatio.Right;
                    double w = VM.Settings_Double[3] / cx;
                    //3g2
                    cellListH[1].rcRatio = new Rect(x, 0, w, VM.Settings_Double[0] / cy);
                    //3g3
                    cellListH[2].rcRatio = new Rect(x, cellListH[1].rcRatio.Bottom, w, VM.Settings_Double[1] / cy);
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
        public List<double> DefaultSettings => new List<double>() { 1, 1, 7, 3 };
        #endregion Settings

        #region FriendlyName
        private string _friendlyName = string.Empty;
        private string _defaultHorzName = //LangHelper.Instance[$"EATooltip_37H"];
        "Option 3.7: 2 rows, split equally. Column 1, split 70/30%. Column 2, no split.";
        private string _defaultVertName = //LangHelper.Instance[$"EATooltip_37V"];
        "Option 3.7: 2 rows, split 30/70%. Row 1, split equally, Row 2, no split.";
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
