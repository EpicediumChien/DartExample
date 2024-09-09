using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace DDPM.Easy.Common
{
    //LastModified: Robert_Lin 2024-9-4 16:48
    //[2024-9-4 16:48]
    //1 Add Clone()
    /// <summary>
    /// All SplitCtrlXX are inherient from this interface
    /// </summary>
    public interface ISplitCtrl
    {
        #region Collection of support SplitCtrl classes

        //All support class of EasyArrange
        public static List<ISplitCtrl> Splits_EA = new List<ISplitCtrl>()
        {
            new SplitCtrl2A(), new SplitCtrl2B(), new SplitCtrl2C(), new SplitCtrl2D(),
            //new SplitCtrl3A(), new SplitCtrl3B(), new SplitCtrl3C(), new SplitCtrl3D(), new SplitCtrl3E(), new SplitCtrl3F(),
            //new SplitCtrl3G(), new SplitCtrl3H(), new SplitCtrl3I(),
            new SplitCtrl4A(), new SplitCtrl4B(), new SplitCtrl4C(), new SplitCtrl4D(),
            new SplitCtrl4E(), new SplitCtrl4F(),
            //new SplitCtrl5A(), new SplitCtrl5B(), new SplitCtrl5C(), new SplitCtrl5D(), new SplitCtrl5E(), new SplitCtrl5F(),
            //new SplitCtrl5G(), new SplitCtrl5H(), new SplitCtrl5I(),
            //new SplitCtrl6A(), new SplitCtrl6B(), new SplitCtrl6C(), new SplitCtrl6D(), new SplitCtrl6E(),
            //new SplitCtrl6F(), new SplitCtrl6G(), new SplitCtrl6H(), new SplitCtrl6I(), new SplitCtrl6J(),
            //new SplitCtrl7A(), new SplitCtrl7B(),  new SplitCtrl7C(),  new SplitCtrl7D(),  new SplitCtrl7E(),  new SplitCtrl7F(),
            //new SplitCtrl7G(), new SplitCtrl7H(), new SplitCtrl7I(), new SplitCtrl7J(), new SplitCtrl7K(),

            new SplitCtrl0A()
        };

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
            ISplitCtrl? iSplit = ISplitCtrl.Splits_EA.Find(x => (x.CellCount == cellCount) && (x.SplitKey == splitKey));
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
            return newObj;
        }
        #endregion Create a new instance

        #region Working mode

        public eSplitModes SplitMode
        {
            get => VM.SplitMode;
            set
            {
                VM.SplitMode = value;
            }
        }

        #endregion Working mode

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

        #endregion Cell list

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

        #endregion Settings

        #region Screen Orientation

        public bool IsVertical
        {
            get { return VM.IsVertical; }
            set { VM.IsVertical = value; }
        }

        #endregion Screen Orientation


        #region Bitmap - Currently is not used in DDPM

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

        #endregion Bitmap - Currently is not used in DDPM
    }
}