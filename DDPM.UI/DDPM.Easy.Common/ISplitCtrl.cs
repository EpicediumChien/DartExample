using System.Configuration;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace DDPM.Easy.Common
{
    //LastModified: Robert_Lin 2024-9-4 16:48
    //[2025-4-15 13:41] for SDL and memory usage
    //1 Change Splits_EA to private and add 'readonly'
    //2 Add a AllSplitCtrls for enumeation usage (for exmaple, foreach)
    //3 Add DisposeAllSplitCtrls() to dispose allocated SplitCtrls in the list
    //[2024-9-4 16:48]
    //1 Add Clone()
    /// <summary>
    /// All SplitCtrlXX are inherient from this interface
    /// </summary>
    public interface ISplitCtrl
    {
        #region Collection of support SplitCtrl classes

        //EasyArrange preset layouts
        //public static List<ISplitCtrl> Splits_EA = new List<ISplitCtrl>()
        private static readonly List<ISplitCtrl> _allSplitCtrls = new List<ISplitCtrl>
        {
            // 2 Windows
            new SplitCtrl2A(), new SplitCtrl2B(), new SplitCtrl2C(), new SplitCtrl2D(),
            //3 Windows
            new SplitCtrl3A(), new SplitCtrl3B(), new SplitCtrl3C(), new SplitCtrl3D(), new SplitCtrl3E(), new SplitCtrl3F(),
            new SplitCtrl3G(), new SplitCtrl3H(), new SplitCtrl3I(),
            //4 Windows
            new SplitCtrl4A(), new SplitCtrl4B(), new SplitCtrl4C(), new SplitCtrl4D(), new SplitCtrl4E(), new SplitCtrl4F(),
            //5 Windows
            new SplitCtrl5A(), new SplitCtrl5B(), new SplitCtrl5C(), new SplitCtrl5D(), new SplitCtrl5E(), new SplitCtrl5F(),
            new SplitCtrl5G(), new SplitCtrl5H(), new SplitCtrl5I(),
            //6 Windows
            new SplitCtrl6A(), new SplitCtrl6B(), new SplitCtrl6C(), new SplitCtrl6D(), new SplitCtrl6E(),
            new SplitCtrl6F(), new SplitCtrl6G(), new SplitCtrl6H(), new SplitCtrl6I(), new SplitCtrl6J(),
            //7 or more Windows
            new SplitCtrl7A(), new SplitCtrl7B(), new SplitCtrl7C(), new SplitCtrl7D(), new SplitCtrl7E(), new SplitCtrl7F(),
            new SplitCtrl7G(), new SplitCtrl7H(), new SplitCtrl7I(), new SplitCtrl7J(), new SplitCtrl7K(),

            new SplitCtrl0A()
        };

        public static bool IsExisted(int cellCount, char splitKey)
        {
            //ISplitCtrl? iSplit = ISplitCtrl.Splits_EA.Find(x => (x.CellCount == cellCount) && (x.SplitKey == splitKey));
            ISplitCtrl? iSplit = ISplitCtrl._allSplitCtrls.Find(x => (x.CellCount == cellCount) && (x.SplitKey == splitKey));
            return (iSplit != null);
        }

        /// <summary>
        /// Check if the specified eaID is an existed (valid) EAID of Preset Layout 
        /// </summary>
        /// <param name="eaId"></param>
        /// <returns></returns>
        public static bool IsExistedPresetEAID(int eaId)
        {
            return (eaId >= 0) && (eaId <= 49);
        }

        //Robert_Lin 2025-4-15 new added for enumeration
        //Usage:
        //  foreach (ISplitCtrl sp in ISplitCtrl.AllSplitCtrls)
        public static IEnumerable<ISplitCtrl> AllSplitCtrls
        {
            get
            {
                foreach (ISplitCtrl sp in _allSplitCtrls)
                {
                    yield return sp;
                }
            }
        }
        #endregion Collection of support SplitCtrl classes

        #region Native members - value not be changed once created

        /// <summary>
        /// It's used to identify the instance is implement in what class
        /// For example, in SplitCtrl2B this propery will be =nameof(SplitCtrl2B)
        /// </summary>
        public string CtrlClass { get; }

        /// <summary>
        /// The number of CellObj in this SplitCtrl, it's also used to know with list will be added.
        /// For example, for the SplitCtrl3X classes, their CellCount=3, and will be added to '3 Windows' list.
        /// </summary>
        public int CellCount { get; }

        /// <summary>
        /// The identify of a SplitCtrl in a 'group'. For example, SplitCtrl3X have 9 different layouts (classes),
        /// each one is identified with 'key' which is 'A','B','C',....
        /// </summary>
        public char SplitKey { get; }

        /// <summary>
        /// Return the SplitCtrl itselft. it's also will be assign to a Content control on UI to display.
        /// </summary>
        public UserControl UC { get; }

        public string FriendlyName { get; set; }

        //Robert_Lin, 2024-10-12 added
        /// <summary>
        /// The ID number to spefiied a layout. this ID has the same definetion wil DDM 2
        /// which is used to migrate settings from DDM, and used for CLI command.
        /// 0 = Off = SplitCtrl0A
        /// 1~999 = Predefind layout (current used are [1~49])
        /// 1000~1004 = Custom layout
        /// </summary>
        public int EAID { get; set; }

        //public CellJson[] Cells { get; set; }

        public string TooltipResourceName { get; }

        #endregion Native members - value not be changed once created

        #region ViewModel

        public SplitCtrlVM VM { get; }

        #endregion ViewModel

        #region Create a new instance

        /// <summary>
        /// It's used to create an instance.
        /// </summary>
        /// <returns></returns>
        public abstract ISplitCtrl New();

        public static ISplitCtrl? Create(int cellCount, char splitKey)
        {
            if ((cellCount == 0) && (splitKey == 'B'))
            {
                return new SplitCtrl0B();
            }
            //ISplitCtrl? iSplit = ISplitCtrl.Splits_EA.Find(x => (x.CellCount == cellCount) && (x.SplitKey == splitKey));
            ISplitCtrl? iSplit = ISplitCtrl._allSplitCtrls.Find(x => (x.CellCount == cellCount) && (x.SplitKey == splitKey));
            if (iSplit == null)
                return null;
            return iSplit.New();
        }

        /// <summary>
        /// Create a Preset Layout ISplitCtrl by EAID
        /// </summary>
        /// <param name="eaId">Range [1~48] cannot used to create custom layout (which EAID=[1000~1004]</param>
        /// <returns></returns>
        public static ISplitCtrl? Create(int eaId)
        {
            //ISplitCtrl? iSplit = ISplitCtrl.Splits_EA.Find(x => (x.EAID == eaId));
            ISplitCtrl? iSplit = ISplitCtrl._allSplitCtrls.Find(x => (x.EAID == eaId));
            if (iSplit == null)
                return null;
            return iSplit.New();
        }

        /// <summary>
        /// Create a new ISplitCtrl and clone settings, but have different UserControl
        /// </summary>
        /// <returns></returns>
        public ISplitCtrl Clone()
        {
            //Construct a new instance, class Native members are clone.
            ISplitCtrl newObj = New();
            //Clone settings
            newObj.Settings = new List<double>(Settings);
            newObj.EAID = EAID;
            //Robert_Lin 2025-2-5 prevent null exception, tepmoratry comment-out
            //if (!string.IsNullOrEmpty(FriendlyName))
            //    newObj.FriendlyName = FriendlyName;
            return newObj;
        }
        #endregion Create a new instance

        #region Split mode

        public eSplitModes SplitMode
        {
            get => VM.SplitMode;
            set
            {
                VM.SplitMode = value;
            }
        }

        #endregion Split mode

        #region IsEditable

        public bool IsEditable
        {
            get => VM.IsEditable;
            set { VM.IsEditable = value; }
        }

        #endregion IsEditable

        #region HoveringCell

        public string HoveringCell
        {
            get => VM.HoveringCell;
            set => VM.HoveringCell = value;
        }

        #endregion HoveringCell

        #region Cell list

        /// <summary>
        /// SplitCtrl will get the cells from UI, and store in CellList.
        /// EAWindow need them to update te coordinates (Rect) of cells to check if current inside a cell,
        /// and use the Rect to arrange target window.
        /// </summary>
        public List<CellObj> CellList { get; set; }

        public void UpdateRatioRectsFromSettings();

        public void RefreshCellRects()
        {
            if (SplitMode == eSplitModes.AWS)
            {
                UC.Dispatcher.InvokeAsync(() => {
                    foreach (CellObj cellObj in CellList)
                    {
                        Point ptTopLeft = cellObj.CellBd.PointToScreen(new Point(0, 0));
                        double w = cellObj.CellBd.ActualWidth * ScreenScale;
                        double h = cellObj.CellBd.ActualHeight * ScreenScale;
                        cellObj.rc = new Rect(ptTopLeft.X, ptTopLeft.Y, w, h);
                    }
                }, System.Windows.Threading.DispatcherPriority.Loaded);
            }
        }
        //Used for RefreshCellRects()
        public static double ScreenScale { get; set; } = 1.000;
        #endregion Cell list

        #region CellBorders
        public List<CellBorder> CellBorders { get; set; }
        #endregion

        #region Settings

        //1 Settings are not stored in a SplitCtrl class member, instead, it apply to UI directly.
        //2 Each SplitCtrl has it's individual DefaultSettings.
        //3 When user (from UI) change the Default settings (apply to UI directly), the SpltCtrl will be
        //  a custom layout.
        //4 The settings of custom layout should be stored in per-user per-monitor settings file.
        //
        public List<double> DefaultSettings { get; }

        public List<double> Settings
        {
            get
            {
                if (VM.Settings == null)
                    return DefaultSettings;
                else
                    return VM.Settings_Double;
            }

            set
            {
                VM.Settings = SplitCtrlVM.Double_To_GridLength(value);
            }
        }

        //Settings in string type
        public string SettingsString
        {
            get
            {
                return SplitCtrlVM.Double_To_String(Settings);
            }
            set
            {
                List<double> newSettings = SplitCtrlVM.String_To_Double(value);
                if ((newSettings != null) && (newSettings.Count > 0))
                    Settings = newSettings;
                //If the input value is invalid, the Settings will not be changed
            }
        }

        //public void UpdateRatioRectsFromSettings();
        #endregion Settings

        #region Screen Orientation

        public bool IsVertical
        {
            get { return VM.IsVertical; }
            set { VM.IsVertical = value; }
        }

        #endregion Screen Orientation

        #region Bitmap - Currently is not used in DDPM
        //To save a ISplitCtrl to a .PNG image file: (Need to run under UI thread)
        // ISplitCtr isp must be created and shown on UI
        // BitmapSource bmpSrc = isp.CreateBitmapSource();
        // SaveBitmapSourceAsPngFile(bmpSrc, pathName);

        /// <summary>
        /// To create a BitmatSouce from current SplitCtrl. The return BitmapSource can be used to
        /// 1) Display an Image on GUI, 2) Save as a .PNG file
        /// </summary>
        /// <returns></returns>
        public BitmapSource CreateBitmapSource()
        {
            double pxWidth = UC.ActualWidth + 1;
            double pxHeight = UC.ActualHeight + 1;

            if ((pxWidth <= 0) && (pxHeight <= 0))
                return null;

            UC.Measure(new Size(pxWidth, pxHeight));
            UC.Arrange(new Rect(new Size(pxWidth, pxHeight)));

            RenderTargetBitmap rtb = new RenderTargetBitmap((int)pxWidth, (int)pxHeight,
                96d, 96d, System.Windows.Media.PixelFormats.Default);

            rtb.Render(UC);
            return rtb;
        }

        public static bool SaveBitmapSourceAsPngFile(BitmapSource bmpSrc, string pathName)
        {
            BitmapFrame bmpFrame = BitmapFrame.Create(bmpSrc);
            PngBitmapEncoder pngEnc = new PngBitmapEncoder();
            pngEnc.Frames.Add(bmpFrame);

            using(FileStream fs = new FileStream(pathName, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                pngEnc.Save(fs);
            }
            
            return true;
        }
        #endregion Bitmap - Currently is not used in DDPM

        #region Added Custom Layout
        //IsAddedCustomLayout will be removed, please use IsOverlapCustomLayout instead
        public bool IsAddedCustomLayout
        {
            get { return ((CellCount==0) && (SplitKey=='B')); }
        }
        public bool IsOverlapCustomLayout
        {
            get { return ((CellCount == 0) && (SplitKey == 'B')); }
        }
        #endregion

        #region Dispose
        private static readonly object _lockDisposeAll = new object();
        public static void DisposeAll()
        {
            lock (_lockDisposeAll)
            {
                foreach (var spCtrl in _allSplitCtrls)
                {
                    try
                    {
                        if (spCtrl != null)
                        {
                            if (spCtrl is IDisposable disposable)
                                disposable.Dispose();
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }
        #endregion Dispose
    }
}