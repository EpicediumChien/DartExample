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
    /// Interaction logic for SplitCtrl3C.xaml
    /// </summary>
    public partial class SplitCtrl3C : UserControl, ISplitCtrl
    {
        #region ctor
        public SplitCtrl3C()
        {
            InitializeComponent();
            VM.Settings = SplitCtrlVM.Double_To_GridLength(DefaultSettings);
            DataContext = vm;
            InitCellList();
            InitSplitterList();
        }
        #endregion ctor

        #region ISplitCtrl Native Members
        public string CtrlClass => nameof(SplitCtrl3C);
        public int CellCount => 3;
        public char SplitKey => 'C';
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
            SplitCtrl3C ctrl = new SplitCtrl3C();
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
            cellListH.Add(new CellObj("3c1", cell_3c1));
            cellListH.Add(new CellObj("3c2", cell_3c2));
            cellListH.Add(new CellObj("3c3", cell_3c3));

            cellListV.Clear();
            cellListV.Add(new CellObj("3C1", cell_3C1));
            cellListV.Add(new CellObj("3C2", cell_3C2));
            cellListV.Add(new CellObj("3C3", cell_3C3));
        }
        #endregion Cell List

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
        public List<double> DefaultSettings => new List<double>() { 3, 4, 3 };
        #endregion Settings

        #region FriendlyName
        public string FriendlyName { get; set; }
        #endregion FriendlyName

    }
}
