#define REMOVE_EA_SPLITTERS //Define this symbol to remove all (unused VSplitters and HSplitters)
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace DDPM.Easy.Common
{
    /// <summary>
    /// Interaction logic for SplitCtrl0B.xaml
    /// </summary>
    public partial class SplitCtrl0B : UserControl, ISplitCtrl, IDisposable
    {
        #region Private members
        private bool _isDisposed = false;
        //private CellBorder cellBorder = null; //Robert_Lin 2025-4-21 use local object instead.
        #endregion Private members

        #region ctor
        public SplitCtrl0B()
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
        //SplitCtrl0B is used fro OnScreen custom layout, EAID should be [1000~1004], no default value
        public int EAID { get; set; }

        public string TooltipResourceName { get; } = "EATooltip_0B";
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
            //cellListH.Clear();
            //cellListH.Add(new CellObj("2b1", cell_2b1));
            //cellListH.Add(new CellObj("2b2", cell_2b2));

            //cellListV.Clear();
            //cellListV.Add(new CellObj("2B1", cell_2B1));
            //cellListV.Add(new CellObj("2B2", cell_2B2));

            clearCellObj(cellListH);
            clearCellObj(cellListV);
        }

        /// <summary>
        /// Convert ISplitCtrl.Settings to CellList[i].rcRect
        /// </summary>
        public void UpdateRatioRectsFromSettings()
        {
            if (VM.Settings_Double.Count < 7)
                return;

            if (VM.IsVertical)
            {

            }
            else //Horz
            {
                //SplitCtrl0B has no default CellList, so we will create new and replace
                //Always apply to Horz CellList now

                List<Rect> listOut = new List<Rect>();

                List<double> settings = VM.Settings_Double;
                if (settings == null)
                    return;// listOut;
                if (settings.Count == 0)
                    return;// listOut;

                //settings[0] is BorderCount
                int borderCount = (int)settings[0];
                if (borderCount <= 0)
                    return;// listOut;

                //Check the settings.Count should be (borderCount*4 + 4)
                if (settings.Count != ((borderCount + 1) * 4))
                    return; // listOut;

                //settings[1] is screenScale
                double orgScreenScale = settings[1];
                //settings[2] is screenWidth
                double orgWidth = settings[2];
                //settings[3] is screenHeight
                double orgHeight = settings[3];

                if (orgWidth <= 0)
                    orgWidth = 1;
                if (orgHeight <= 0)
                    orgHeight = 1;

                Rect rcDest = new Rect(0, 0, 1, 1);
                double xRatio = rcDest.Width / orgWidth;
                double yRatio = rcDest.Height / orgHeight;

                int idxSettings = 0;
                for (int idx = 0; idx < borderCount; idx++)
                {
                    idxSettings += 4;
                    if ((idxSettings + 4) > settings.Count)
                        break;

                    double left = rcDest.Left + settings[idxSettings] * xRatio;
                    double top = rcDest.Top + settings[idxSettings + 1] * yRatio;

                    double width = settings[idxSettings + 2] * xRatio;
                    double height = settings[idxSettings + 3] * yRatio;

                    Rect rect = new Rect(left, top, width, height);
                    listOut.Add(rect);
                }

                cellListH.Clear();
                int idxCell = 0;
                foreach (Rect rcRatio in listOut)
                {
                    CellObj cellObj = new CellObj($"Cb{idxCell}");
                    cellObj.rcRatio = rcRatio;
                    cellListH.Add(cellObj);
                    idxCell++;
                }
            }

        }
        public bool ApplySettingsToCellList(Rect rcView)
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

            Trace.WriteLine($"rcView:({rcView.X},{rcView.Y})-({rcView.Right},{rcView.Bottom}){rcView.Width}x{rcView.Height}");
          
            //settings[1] is screenScale
            double orgScreenScale = settings[1];
            //settings[2] is screenWidth
            double orgWidth = settings[2];
            //settings[3] is screenHeight
            double orgHeight = settings[3];

            if (orgWidth <= 0)
                orgWidth = 1;
            if (orgHeight <= 0)
                orgHeight = 1;

            double xRatio = rcView.Width / orgWidth;
            double yRatio = rcView.Height / orgHeight;

            canvas.Children.Clear();

            int idxSettings = 0;
            for (int idx = 0; idx < borderCount; idx++)
            {
                idxSettings += 4;
                if ((idxSettings + 4) > settings.Count)
                    break;

                string cellName = $"Cb{idx+1}";
                double left = settings[idxSettings] * xRatio;
                double top =  settings[idxSettings + 1] * yRatio;
                double width = settings[idxSettings + 2] * xRatio;
                double height = settings[idxSettings + 3] * yRatio;

                //Create a CellBorder
                //

                //Robert_Lin, 2025-4-21, the cellBorder cannot be reused (cannot add to visual twice)
                //So remove class data member, and change it as local varaible below.
                CellBorder cellBorder = new CellBorder();
                //if(cellBorder == null)
                //{
                //    cellBorder = new CellBorder();
                //}
                cellBorder.CellName = cellName;
                cellBorder.Width = settings[idxSettings + 2] * xRatio;
                cellBorder.Height = settings[idxSettings + 3] * yRatio;

                //Add Borders
                //
                //Border border = new Border();
                //border.Name = $"Cb{idx}";
                //border.Style = FindResource("CellBorderStyle") as Style;
                //border.Width = settings[idxSettings + 2] * xRatio;
                //border.Height = settings[idxSettings + 3] * yRatio;

               //canvas.Children.Add(border);
                //Canvas.SetLeft(border, left);
                //Canvas.SetTop(border, top);

                CellObj cell = new CellObj(cellName, cellBorder);
                cell.rc = new Rect(left, top, width, height);
                cellListH.Add(cell);
                Trace.WriteLine($"Cell[{idx}]:({cell.rc.X},{cell.rc.Y})-({cell.rc.Right},{cell.rc.Bottom}){cell.rc.Width}x{cell.rc.Height}");

                //Add CellBorders
                //
                //CellBorder cellBorder = new CellBorder();
                //cellBorder.CellName = $"Cb{idx}";
                ////cellBorder.Style = FindResource("CellBorder0B") as Style;
                //cellBorder.Width = settings[idxSettings + 2] * xRatio;
                //cellBorder.Height = settings[idxSettings + 3] * yRatio;

                //Point topLeftCellBd = cellBorder.PointToScreen(new Point(left, top));
                //Rect rcCellBd = new Rect(topLeftCellBd.X, topLeftCellBd.Y, border.Width, border.Height);
                //cellBorder.rect = rcCellBd;


                canvas.Children.Add(cellBorder);
                Canvas.SetLeft(cellBorder, left);
                Canvas.SetTop(cellBorder, top);

                CellBorders.Add(cellBorder);

            }

            return true;
        }

        public List<Rect>? ConvertSettingsToRatioRects(Rect rcView)
        {
            List<double> settings = VM.Settings_Double;

            if (settings == null)
                return null;
            if (settings.Count == 0)
                return null;

            //settings[0] is BorderCount
            int borderCount = (int)settings[0];
            if (borderCount <= 0)
                return null;

            //Check the settings.Count should be (borderCount*4 + 4)
            if (settings.Count != ((borderCount + 1) * 4))
                return null;

            //settings[1] is screenScale
            double orgScreenScale = settings[1];
            //settings[2] is screenWidth
            double orgWidth = settings[2];
            //settings[3] is screenHeight
            double orgHeight = settings[3];

            if (orgWidth <= 0)
                orgWidth = 1;
            if (orgHeight <= 0)
                orgHeight = 1;

            double xRatio = rcView.Width / orgWidth;
            double yRatio = rcView.Height / orgHeight;

            List<Rect> listOut = new List<Rect>();

            int idxSettings = 0;
            for (int idx = 0; idx < borderCount; idx++)
            {
                idxSettings += 4;
                if ((idxSettings + 4) > settings.Count)
                    break;

                double left = settings[idxSettings] * xRatio;
                double top = settings[idxSettings + 1] * yRatio;
                double width = settings[idxSettings + 2] * xRatio;
                double height = settings[idxSettings + 3] * yRatio;

                Rect rcRatio = new Rect(left, top, width, height);
                listOut.Add(rcRatio);
            }
            return listOut;
        }

        /// <summary>
        /// Convert ISplitCtrl.Settings to list of Rects, all these Rect are the layout in a 1x1 View
        /// </summary>
        public void UpdateToCellListFromSettings()
        //public List<Rect> ConvertSettingsToRatioRects(Rect rcDest, bool isVertical = false)
        {
            //SplitCtrl0B has no default CellList, so we will create new and replace
            //Always apply to Horz CellList now

            List<Rect> listOut = new List<Rect>();

            List<double> settings = VM.Settings_Double;
            if (settings == null)
                return;// listOut;
            if (settings.Count == 0)
                return;// listOut;

            //settings[0] is BorderCount
            int borderCount = (int)settings[0];
            if (borderCount <= 0)
                return;// listOut;

            //Check the settings.Count should be (borderCount*4 + 4)
            if (settings.Count != ((borderCount + 1) * 4))
                return; // listOut;

            //settings[1] is screenScale
            double orgScreenScale = settings[1];
            //settings[2] is screenWidth
            double orgWidth = settings[2];
            //settings[3] is screenHeight
            double orgHeight = settings[3];

            if (orgWidth <= 0)
                orgWidth = 1;
            if (orgHeight <= 0)
                orgHeight = 1;

            Rect rcDest = new Rect(0,0,1,1);
            double xRatio = rcDest.Width / orgWidth;
            double yRatio = rcDest.Height / orgHeight;

            int idxSettings = 0;
            for (int idx = 0; idx < borderCount; idx++)
            {
                idxSettings += 4;
                if ((idxSettings + 4) > settings.Count)
                    break;

                double left = rcDest.Left + settings[idxSettings] * xRatio;
                double top = rcDest.Top + settings[idxSettings + 1] * yRatio;

                double width = settings[idxSettings + 2] * xRatio;
                double height = settings[idxSettings + 3] * yRatio;

                Rect rect = new Rect(left, top, width, height);
                listOut.Add(rect);
            }

            clearCellObj(cellListH);//SDL: Improper Resource Shutdown or Release

            int idxCell = 0;
            foreach(Rect rcRatio in listOut)
            {
                CellObj cellObj = new CellObj($"Cb{idxCell}");
                cellObj.rcRatio = rcRatio;
                cellListH.Add(cellObj);
                idxCell++;
            }
            
        }

        #endregion Cell List

        //SDL: Improper Resource Shutdown or Release
        private static void clearCellObj(List<CellObj> list)
        {
            if (list != null && list.Count > 0)
            {
                foreach (CellObj cell in list)
                {
                    cell.Dispose();
                }
                list.Clear();
            }
        }//SDL: End

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

        private void ClearCellBorders()
        {
            if (celBordersH != null)
            {
                foreach (CellBorder cellBd in celBordersH)
                {
                    cellBd.Dispose();
                }
                celBordersH.Clear();
                celBordersH = null;
            }
            if (celBordersV != null)
            {
                foreach (CellBorder cellBd in celBordersV)
                {
                    cellBd.Dispose();
                }
                celBordersV.Clear();
                celBordersV = null;
            }
        }
        #endregion

#if !REMOVE_EA_SPLITTERS
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
#endif //#if !REMOVE_EA_SPLITTERS

        #region Settings

        public List<double> DefaultSettings => new List<double>() { 1, 1 };

        #endregion Settings

        #region FriendlyName

        public string FriendlyName { get; set; } = "";

        #endregion FriendlyName

        public string HoveringCell
        {
            get => VM.HoveringCell;
            set
            {
                VM.HoveringCell = value;

                this.Dispatcher.Invoke(() =>
                {
                    foreach (CellObj objCell in CellList)
                    {
                        if (objCell.Name.Equals(value))
                        {
                            objCell.CellBd.IsHover = true;
                        }
                        else
                        {
                            objCell.CellBd.IsHover = false;
                        }
                    }
                    //IsEnabled = !isHover;
                });

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
                    // 釋放託管資源
                    if (vm != null)
                    {
                        vm.Dispose();
                        vm = null;
                    }
                    InitCellList();
                    cellListH = null;
                    cellListV = null;

                    //Robert_Lin 2025-4-21 this data member has been removed.
                    //cellBorder?.Dispose();
                    //cellBorder = null;
#if !REMOVE_EA_SPLITTERS
                    InitSplitterList();
#endif
                    if (DefaultSettings != null)
                    {
                        DefaultSettings.Clear();
                    }

                    ClearCellBorders();
                }

                // 釋放非託管資源
                //if (unmanagedResource != IntPtr.Zero)
                //{
                //    // 釋放資源
                //    unmanagedResource = IntPtr.Zero;
                //}

                _isDisposed = true;
            }
        }
        ~SplitCtrl0B()
        {
            Dispose(false);
            InitCellList();
            vm?.Dispose();//SDL: Improper Resource Shutdown or Release
        }
        #endregion

        #region UserControl event handlers
        private void uc_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.IsVisible)
            {
                ((ISplitCtrl)this).RefreshCellRects();
            }
        }
        #endregion UserControl event handlers
    }
}
