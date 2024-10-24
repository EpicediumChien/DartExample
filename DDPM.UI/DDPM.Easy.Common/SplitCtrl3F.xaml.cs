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
    /// Interaction logic for SplitCtrl3F.xaml
    /// </summary>
    public partial class SplitCtrl3F : UserControl, ISplitCtrl
    {
        #region ctor
        public SplitCtrl3F()
        {
            InitializeComponent();
            VM.Settings = SplitCtrlVM.Double_To_GridLength(DefaultSettings);
            DataContext = vm;
            InitCellList();
            InitSplitterList();
        }
        #endregion ctor

        #region ISplitCtrl Native Members
        public string CtrlClass => nameof(SplitCtrl3F);
        public int CellCount => 3;
        public char SplitKey => 'F';
        public UserControl UC => this;
        //SplitCtrl2? ~ 7? are predefined layout, have default value, the EAID may be changed to [1000~1004] if they are customized.
        public int EAID { get; set; } = 10;
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
            SplitCtrl3F ctrl = new SplitCtrl3F();
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
            cellListH.Add(new CellObj("3f1", cell_3f1) { rcRatio = new Rect(0, 0, 0.3, 0.5) });
            cellListH.Add(new CellObj("3f2", cell_3f2) { rcRatio = new Rect(0, 0.5, 0.3, 0.5) });
            cellListH.Add(new CellObj("3f3", cell_3f3) { rcRatio = new Rect(0.3, 0, 0.7, 1) });

            cellListV.Clear();
            cellListV.Add(new CellObj("3F1", cell_3F1) { rcRatio = new Rect(0.5, 0, 0.5, 0.3) });
            cellListV.Add(new CellObj("3F2", cell_3F2) { rcRatio = new Rect(0, 0, 0.5, 0.3) });
            cellListV.Add(new CellObj("3F3", cell_3F3) { rcRatio = new Rect(0, 0.3, 1, 0.7) });
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
        public List<double> DefaultSettings => new List<double>() { 1, 1, 3, 7 };
        #endregion Settings

        #region FriendlyName
        private string _friendlyName = string.Empty;
        private string _defaultHorzName = "Option 3.6 2 rows, equal splits. Column 1, 30 70 perent splits. Column 2, no split.";
        private string _defaultVertName = "Option 3.6 2 rows, 70 30 percent splits. Row 1, no split. Row 2, equal splits.";
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
