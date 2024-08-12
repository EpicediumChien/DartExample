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
    /// Interaction logic for SplitCtrl4E.xaml
    /// </summary>
    public partial class SplitCtrl4E : UserControl, ISplitCtrl
    {
        #region ctor
        public SplitCtrl4E()
        {
            InitializeComponent();
            VM.Settings = SplitCtrlVM.Double_To_GridLength(DefaultSettings);
            DataContext = vm;
            InitCellList();
            InitSplitterList();
        }
        #endregion

        #region ISplitCtrl Native Members
        public string CtrlClass => nameof(SplitCtrl4E);
        public int CellCount => 4;
        public char SplitKey => 'E';
        public UserControl UC => this;
        #endregion

        #region ViewModel
        private SplitCtrlVM vm = new SplitCtrlVM();
        public SplitCtrlVM VM => vm;
        #endregion

        #region Create new instance

        /// <summary>
        /// Create a new instance, but Settings not been copied
        /// </summary>
        /// <returns></returns>
        public ISplitCtrl New()
        {
            SplitCtrl4E ctrl = new SplitCtrl4E();
            return (ISplitCtrl)ctrl;
        }
        #endregion

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
            cellListH.Add(new CellObj("4e1", cell_4e1));
            cellListH.Add(new CellObj("4e2", cell_4e2));
            cellListH.Add(new CellObj("4e3", cell_4e3));
            cellListH.Add(new CellObj("4e4", cell_4e4));

            cellListV.Clear();
            cellListV.Add(new CellObj("4E1", cell_4E1));
            cellListV.Add(new CellObj("4E2", cell_4E2));
            cellListV.Add(new CellObj("4E3", cell_4E3));
            cellListV.Add(new CellObj("4E4", cell_4E4));
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
            VSplitterList.Add(V1);

            HSplitterList.Clear();
            HSplitterList.Add(h1);
            HSplitterList.Add(H1);
            HSplitterList.Add(H2);
        }
        #endregion

        #region Settings
        public List<double> DefaultSettings => new List<double>() { 1, 1, 1, 1, 1 };
        #endregion    

        #region FriendlyName
        public string FriendlyName { get; set; }
        #endregion
    }
}
