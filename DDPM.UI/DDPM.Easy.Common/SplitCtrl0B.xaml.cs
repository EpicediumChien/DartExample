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
    /// Interaction logic for SplitCtrl0B.xaml
    /// </summary>
    public partial class SplitCtrl0B : UserControl, ISplitCtrl
    {
        #region ctor
        public SplitCtrl0B()
        {
            InitializeComponent();
            VM.Settings = SplitCtrlVM.Double_To_GridLength(DefaultSettings);
            DataContext = vm;
            InitCellList();
            InitSplitterList();
        }

        #endregion ctor

        #region ISplitCtrl Native Members

        public string CtrlClass => nameof(SplitCtrl0B);
        public int CellCount
        {
            get
            {
                return 0;
            }
        }
        public char SplitKey => 'B';
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
            SplitCtrl0B ctrl = new SplitCtrl0B();
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
            //cellListH.Add(new CellObj("2b1", cell_2b1));
            //cellListH.Add(new CellObj("2b2", cell_2b2));

            cellListV.Clear();
            //cellListV.Add(new CellObj("2B1", cell_2B1));
            //cellListV.Add(new CellObj("2B2", cell_2B2));
        }

        public bool ApplySettingsToCellList(System.Drawing.Rectangle rcScreen)
        {
            List<double> settings = VM.Settings_Double;
            if (settings == null)
                return false;
            if (settings.Count == 0)
                return false;

            //settings[0] is BorderCount
            int borderCount = (int)settings[0];
            if (borderCount <= 0)
                return false;

            //Check the settings.Count should be (borderCount*4 + 4)
            if (settings.Count != ((borderCount + 1) * 4))
                return false;

            //settings[1] is screenScale
            double orgScreenScale = settings[1];
            //settings[1] is screenWidth
            double orgWidth = settings[2];
            //settings[2] is screenHeight
            double orgHeight = settings[3];


            int idxSettings = 0;
            for (int idx = 0; idx < borderCount; idx++)
            {
                idxSettings += 4;
                if ((idxSettings + 4) > settings.Count)
                    break;

                double left = settings[idxSettings];
                double top = settings[idxSettings + 1];

                Border border = new Border();
                border.Name = $"Cb{idx}";
                border.Style = FindResource("CellBorderStyle") as Style;
                border.Width = settings[idxSettings + 2];
                border.Height = settings[idxSettings + 3];

                canvas.Children.Add(border);
                Canvas.SetLeft(border, left);
                Canvas.SetTop(border, top);

                CellObj cell = new CellObj(border.Name, border);
                cell.rc = new Rect(left, top, border.Width, border.Height);
                cellListH.Add(cell);
            }

            return true;
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
            //VSplitterList.Add(V1);

            HSplitterList.Clear();
            //HSplitterList.Add(h1);
        }

        #endregion Splitter List

        #region Settings

        public List<double> DefaultSettings => new List<double>() { 1, 1 };

         #endregion Settings

        #region FriendlyName

        public string FriendlyName { get; set; }

        #endregion FriendlyName


    }
}
