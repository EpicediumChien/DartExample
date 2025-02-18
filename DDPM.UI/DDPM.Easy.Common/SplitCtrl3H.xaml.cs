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
    /// Interaction logic for SplitCtrl3H.xaml
    /// </summary>
    public partial class SplitCtrl3H : UserControl, ISplitCtrl
    {
        #region ctor
        public SplitCtrl3H()
        {
            InitializeComponent();
            VM.Settings = SplitCtrlVM.Double_To_GridLength(DefaultSettings);
            DataContext = vm;
            InitCellList();
            InitSplitterList();
        }
        #endregion ctor

        #region ISplitCtrl Native Members
        public string CtrlClass => nameof(SplitCtrl3H);
        public int CellCount => 3;
        public char SplitKey => 'H';
        public UserControl UC => this;
        //SplitCtrl2? ~ 7? are predefined layout, have default value, the EAID may be changed to [1000~1004] if they are customized.
        public int EAID { get; set; } = 12;
        public string TooltipResourceName { get; } = "EATooltip_38";
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
            SplitCtrl3H ctrl = new SplitCtrl3H();
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
            cellListH.Add(new CellObj("3h1", cell_3h1) { rcRatio = new Rect(0, 0, 1, 0.5) });
            cellListH.Add(new CellObj("3h2", cell_3h2) { rcRatio = new Rect(0, 0.5, 0.5, 0.5) });
            cellListH.Add(new CellObj("3h3", cell_3h3) { rcRatio = new Rect(0.5, 0.5, 0.5, 0.5) });

            cellListV.Clear();
            cellListV.Add(new CellObj("3H1", cell_3H1) { rcRatio = new Rect(0.5, 0, 0.5, 1) });
            cellListV.Add(new CellObj("3H2", cell_3H2) { rcRatio = new Rect(0, 0, 0.5, 0.5) });
            cellListV.Add(new CellObj("3H3", cell_3H3) { rcRatio = new Rect(0, 0.5, 0.5, 0.5) });
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
                    double w = VM.Settings_Double[1] / cx;
                    //3H2
                    cellListV[1].rcRatio = new Rect(0, 0, w, VM.Settings_Double[2] / cy);
                    //3H3
                    cellListV[2].rcRatio = new Rect(0, cellListV[1].rcRatio.Bottom, w, VM.Settings_Double[3] / cy);
                    //3H1
                    cellListV[0].rcRatio = new Rect(cellListV[1].rcRatio.Right, 0, VM.Settings_Double[0] / cx, 1);
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
                    //3h1
                    cellListH[0].rcRatio = new Rect(0, 0, 1, VM.Settings_Double[0] / cy);
                    double y = cellListH[0].rcRatio.Bottom;
                    double h = VM.Settings_Double[1] / cy;
                    //3h2
                    cellListH[1].rcRatio = new Rect(0, y, VM.Settings_Double[2] / cx, h);
                    //3h3
                    cellListH[2].rcRatio = new Rect(cellListH[1].rcRatio.Right, y, VM.Settings_Double[3] / cx, h);
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
        public List<double> DefaultSettings => new List<double>() { 1, 1, 1, 1 };
        #endregion Settings

        #region FriendlyName
        private string _friendlyName = string.Empty;
        private string _defaultHorzName = //LangHelper.Instance[$"EATooltip_38H"];
        "Option 3.8: 2 rows, split equally. Row 1, no split. Row 2, split equally.";
        private string _defaultVertName = //LangHelper.Instance[$"EATooltip_38V"];
        "Option 3.8: 2 columns, split equally.  Column 1, no split. Column 2, split equally.";
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
