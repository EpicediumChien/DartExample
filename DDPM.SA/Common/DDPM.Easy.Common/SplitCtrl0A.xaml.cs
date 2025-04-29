#define REMOVE_EA_SPLITTERS //Define this symbol to remove all (unused VSplitters and HSplitters)
using DDPM.SA.Resources.Helper;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace DDPM.Easy.Common
{
    /// <summary>
    /// Interaction logic for SplitCtrl0A.xaml
    /// </summary>
    //    public partial class SplitCtrl0A : UserControl, ISplitCtrl, IDisposable

    public partial class SplitCtrl0A : UserControl, ISplitCtrl, IDisposable
    {
        #region Private members
        private bool _isDisposed = false;
        #endregion Private members

        #region ctor

        public SplitCtrl0A()
        {
            InitializeComponent();
            VM.Settings = SplitCtrlVM.Double_To_GridLength(DefaultSettings);
            DataContext = vm;
            InitCellList();
#if !REMOVE_EA_SPLITTERS
            InitSplitterList();
#endif
        }

        #endregion ctor

        #region ISplitCtrl Native Members

        public string CtrlClass => nameof(SplitCtrl0A);
        public int CellCount => 0;
        public char SplitKey => 'A';
        public UserControl UC => this;
        public int EAID { get => 0; set { } }

        public string TooltipResourceName { get; } = "EATooltip_00";
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
            SplitCtrl0A ctrl = new SplitCtrl0A();
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

            cellListV.Clear();
        }

        /// <summary>
        /// Convert ISplitCtrl.Settings to CellList[i].rcRect
        /// </summary>
        public void UpdateRatioRectsFromSettings()
        {

        }
        #endregion Cell List

        #region CellBorders 
        //CellBorders should be used in SplitCtrl0B only
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

#if !REMOVE_EA_SPLITTERS
        #region Splitter List

        public List<GridSplitter> VSplitterList { get; set; } = new List<GridSplitter>();
        public List<GridSplitter> HSplitterList { get; set; } = new List<GridSplitter>();

        public void InitSplitterList()
        {
            VSplitterList.Clear();

            HSplitterList.Clear();
        }

        #endregion Splitter List
#endif //#if !REMOVE_EA_SPLITTERS

        #region Settings

        public List<double> DefaultSettings => new List<double>() { };

        public List<double> Settings
        {
            get => DefaultSettings;
            set { }
        }

        #endregion Settings

        #region FriendlyName
        private string _friendlyName = "Empty Layout";
        public string FriendlyName
        {
            get
            {
                try
                {
                    return LangHelper.Instance[$"{TooltipResourceName}"];
                }
                catch (Exception e1)
                {
                }
                return _friendlyName;
            }
            set
            {
                _friendlyName = value;
            }
        }

        #endregion FriendlyName

        public bool IsEditable
        {
            get { return VM.IsEditable; }
            set
            {
                VM.IsEditable = value;

            }
        }
        public eSplitModes SplitMode
        {
            get => VM.SplitMode;
            set
            {
                VM.SplitMode = value;
                if (VM.SplitMode == eSplitModes.Work)
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        Opacity = 0.001;
                    });
                }
            }
        }

        #region Dispose and Destructor
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                }
                _isDisposed = true;
            }
        }
        ~SplitCtrl0A()
        {
            Dispose(false);
        }
        #endregion

    }
}