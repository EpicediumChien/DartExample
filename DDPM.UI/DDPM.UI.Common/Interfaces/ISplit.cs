using DDPM.UI.Common.EAEM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace DDPM.UI.Common.Interfaces
{

    //All items in a SplitListView must be type of ISplit.
    // The Item's ItemPresenter will be ISplit.Content which is type of ContentControl
    public interface ISplit
    {
        #region Basic
        /// <summary>
        /// The type of the UserControl, it should be a SplitCtrlXX class
        /// For example: => typeof(SplitCtrl2A)
        /// </summary>
        //public Type CtrlType { get; }

        /// <summary>
        /// The content will be shown on UI, it should be the SplitCtrlXX it self
        /// </summary>
        public ContentControl? Content { get; }
        #endregion

        #region Creation
        /// <summary>
        /// Construct a new ISplit instance
        /// </summary>
        /// <returns></returns>
        public abstract ISplit New(List<double>? settings = null);

        /// <summary>
        /// Duplicate a ISplit object
        /// </summary>
        /// 
        public ISplit Duplicate()
        {
            ISplit isp = New(GetSettings());
            return isp;
        }
        #endregion

        #region Settings
        public int SettingsCount { get; }
        public abstract List<double> GetSettings();
        public abstract bool SetSettings(List<double> settings);

        #endregion

        #region Settings Helpers
        /// <summary>
        /// Format the settings (in double list) to a comma separated string
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        public static string MakeSettingsString(List<double> settings)
        {
            StringBuilder sb = new StringBuilder();
            int idx = 0;
            foreach (double v in settings)
            {
                if (idx > 0)
                    sb.Append(',');
                sb.Append(v.ToString());
                idx++;
            }
            return sb.ToString();
        }

        public static List<double> ParseSettingsFromString(string strSettings)
        {
            return Array.ConvertAll(strSettings.Split(','), Double.Parse).ToList<double>();
        }
        #endregion


        #region Used for PIP/PBP
        public UInt16 PbpCapabilityCode { get; set; }
        public string Description { get; set; }

        public static List<ISplit> PipClasses = new List<ISplit>()
        {
            //23h: PBP 2 windows h-split
            new SplitCtrl2A(new List<double>() {1,1}) { PbpCapabilityCode = (UInt16)0x0023, Description="PBP 2 windows h-split" },
            //24h: PBP 2 windows h-split, Fill
            new SplitCtrl2A(new List<double>() {1,1}) { PbpCapabilityCode = (UInt16)0x0024, Description = "PBP 2 windows h-split, Fill" },
            //25h: PBP 2 windows h-split, 3:7
            new SplitCtrl2D(new List<double>() {3,7}) { PbpCapabilityCode = (UInt16)0x0025, Description="3 - 7" },
            //26h: PBP 2 windows h-split, 7:3
            new SplitCtrl2C(new List<double>() {7,3}) { PbpCapabilityCode = (UInt16)0x0026, Description="7 - 3" },
            //27h: PBP 2 windows h-split, 2:8
            new SplitCtrl2A(new List<double>() {2,8}) { PbpCapabilityCode = (UInt16)0x0027, Description="2 - 8" },
            //28h: PBP 2 windows h-split, 8:2
            new SplitCtrl2A(new List<double>() {8,2}) { PbpCapabilityCode = (UInt16)0x0028, Description="8 - 2" },
            //29h: PBP 2 windows h-split, 25%:75%
            new SplitCtrl2A(new List<double>() {25,75}) { PbpCapabilityCode = (UInt16)0x0029, Description="25% - 75%" },
            //2Ah: PBP 2 windows h-split, 75%:25%
            new SplitCtrl2A(new List<double>() {75,25}) { PbpCapabilityCode = (UInt16)0x002A, Description="75% - 25%" },
            //2Bh: PBP 2 windows h-split, 26%:74%
            new SplitCtrl2A(new List<double>() {26,74}) { PbpCapabilityCode = (UInt16)0x002B, Description="26% - 74%" },
            //2Ch: PBP 2 windows h-split, 74%:26%
            new SplitCtrl2A(new List<double>() {74,26}) { PbpCapabilityCode = (UInt16)0x002C, Description="74% - 26%" },
            //2Dh: PBP 2 windows h-split, 33%:67%
            new SplitCtrl2A(new List<double>() {33,67}) { PbpCapabilityCode = (UInt16)0x002D, Description="33% - 67%" },
            //2Eh: PBP 2 windows h-split, 67%:33%
            new SplitCtrl2A(new List<double>() {67,33}) { PbpCapabilityCode = (UInt16)0x002E, Description="67% - 33%" },
            //2Fh: PBP 2 windows v-split
            new SplitCtrl2B(new List<double>() {1,1}) { PbpCapabilityCode = (UInt16)0x002F, Description="v-split" },

            //31h: PBP 3 windows-L1(half)/R2(up|down split)
            new SplitCtrl3E(new List<double>() {1,1,1,1}) { PbpCapabilityCode = (UInt16)0x0031, Description="PBP 3 windows-L1(half)/R2(up|down split)" },
            //32h: PBP 3 windows-L2(up|down split)/R1(half)
            new SplitCtrl3D(new List<double>() {1,1,1,1}) { PbpCapabilityCode = (UInt16)0x0032, Description="PBP 3 windows-L2(up|down split)/R1(half)" },
            //33h: PBP 3 windows-Up1(half)/Down2(left|right split)
            new SplitCtrl3H(new List<double>() {1,1,1,1 }) { PbpCapabilityCode = (UInt16)0x0033, Description="PBP 3 windows-Up1(half)/Down2(left|right split)" },
            //34h: PBP 3 windows-1row, 3column
            new SplitCtrl3B(new List<double>() {1,1,1,1}) { PbpCapabilityCode = (UInt16)0x0034, Description="PBP 3 windows-1row, 3column" },
            //35h: PBP 3 windows-Up2(left|right split)/Down1(half)
            new SplitCtrl3I(new List<double>() {1,1,1,1}) { PbpCapabilityCode = (UInt16)0x0035, Description="PBP 3 windows-Up2(left|right split)/Down1(half)" },

            //41h: PBP 4 windows-quadrant
            new SplitCtrl4A(new List<double>() {1,1,1,1}) { PbpCapabilityCode = (UInt16)0x0041, Description="PBP 4 windows-quadrant" },
            //42h: PBP 4 windows-1row, 4column
            new SplitCtrl4D(new List<double>() {1,1,1,1}) { PbpCapabilityCode = (UInt16)0x004D, Description="PBP 4 windows-1row, 4column" },

        };
        #endregion


        //public ICommand? ClickCommand { get; set; }

        //public bool IsSelected { get; set; }
    }
}
