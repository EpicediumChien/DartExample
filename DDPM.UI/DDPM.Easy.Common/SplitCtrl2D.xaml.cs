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
    /// Interaction logic for SplitCtrl2D.xaml
    /// </summary>
    public partial class SplitCtrl2D : UserControl, ISplitCtrl
    {
        #region ctor
        public SplitCtrl2D()
        {
            InitializeComponent();
            VM.Settings = SplitCtrlVM.Double_To_GridLength(DefaultSettings);
            DataContext = vm;
            InitCellList();
            InitSplitterList();
        }
        #endregion

        #region ISplitCtrl Native Members
        public string CtrlClass => nameof(SplitCtrl2D);
        public int CellCount => 2;
        public char SplitKey => 'D';
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
            SplitCtrl2D ctrl = new SplitCtrl2D();
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
            cellListH.Add(new CellObj("2d1", cell_2d1));
            cellListH.Add(new CellObj("2d2", cell_2d2));

            cellListV.Clear();
            cellListV.Add(new CellObj("2D1", cell_2D1));
            cellListV.Add(new CellObj("2D2", cell_2D2));
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
        #endregion

        #region Settings
        public List<double> DefaultSettings => new List<double>() { 3, 7 };
        #endregion    

        #region FriendlyName
        public string FriendlyName { get; set; }
        #endregion
    }
}
