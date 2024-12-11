using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Windows.Media.Streaming.Adaptive;

namespace DDPM.SA.Common.Display
{
    /// <summary>
    /// The basic data class to represent a ISplit item, used by EA and EM
    /// </summary>
    public class SplitJson
    {
        #region Native Properties

        public int CellCount { get; set; } = 0;
        public char SplitKey { get; set; } = 'A';
        public List<double> Settings { get; set; } = new List<double>();
        public string CustomName { get; set; } = "";
        public long CustomId { get; set; } = 0;
        public int EAID { get; set; }
        public CellJson[] Cells { get; set; }
        #endregion Native Properties

        #region ctor and create new instance
        /// <summary>
        /// Default ctor, construct a default instances, that is, SplitCtrl0A (Off)
        /// </summary>
        public SplitJson()
        {
                
        }

        /// <summary>
        /// Clone an instance. all native propeties will be copied.
        /// </summary>
        /// <returns></returns>
        public SplitJson Clone()
        {
            SplitJson returnSplit = new SplitJson()
            {
                CellCount = this.CellCount,
                SplitKey = this.SplitKey,
                Settings = new List<double>(this.Settings),
                CustomName = this.CustomName,
                EAID = this.EAID,
                CustomId = this.CustomId
            };
            //Copy Cells if this has
            if (this.Cells != null) 
            {
                returnSplit.Cells = this.Cells.Select(x => (CellJson)x.Clone()).ToArray();
            }
            return returnSplit;
        }
        #endregion ctor and create new instance

        public string ToString()
        {
            return $"{CellCount}{SplitKey}[{Double_To_String(Settings)}],{CustomId}[{CustomName}]";
        }

        //double[] to string, format: [1,2,1,0.8,1,4.52]
        public static string Double_To_String(List<double> settings)
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

        /// <summary>
        /// Compare with other, return true if they are same layout in EasyArrange.
        /// 1 Compare CustomId, if different return false;
        /// 2 Compare (CellCount,SplitKey)
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool IsEquals(SplitJson other)
        {
            //1 Compare CustomId (0=predefined layout; others=custom layout)
            //  custom layout is d=identified with CustomId
            if (CustomId != other.CustomId)
                return false;
            //Can be a.Both are Predefined layout (CustomId=0) => need to compare with (CellCount,SplitKey)
            //    or b.Both are custom layout and are the same layout
            if (CustomId == 0)
                return (CellCount == other.CellCount) && (SplitKey == other.SplitKey);
            return true;
        }

        #region Defaul Recent List
        /// <summary>
        /// Need to copy the DefaultSettings from DDPM.Easy.Common/SplitCtrlXX.xaml.cs
        /// Never add SplitCtrl0A (Off) into the RecentList
        /// </summary>
        public static readonly List<SplitJson> DefaultRecentList = new List<SplitJson>()
        {
            //[0] SplitCtrl2A
            new  SplitJson() { CellCount = 2, SplitKey='A', Settings=new List<double>() { 1, 1 } },
            //[1] SplitCtrl2C
            new  SplitJson() { CellCount = 2, SplitKey='C', Settings=new List<double>() { 7, 3 } },
            //[2] SplitCtrl3E
            new  SplitJson() { CellCount = 3, SplitKey='E', Settings=new List<double>() { 1, 1, 1, 1 } },
            //[3] SplitCtrl4A
            new  SplitJson() { CellCount = 4, SplitKey='A', Settings=new List<double>() { 1, 1, 1, 1, 1 } },
            //[4] SplitCtrl3C
            new  SplitJson() { CellCount = 3, SplitKey='C', Settings=new List<double>() { 3, 4, 3 } }

        };
        #endregion Defaul Recent List

        #region Preset List
        public static readonly List<SplitJson> PresetList = new List<SplitJson>()
        {
            new  SplitJson() { EAID=0, CellCount = 0, SplitKey='A', Settings=new List<double>() { 1 } },
            new  SplitJson() { EAID=1, CellCount = 2, SplitKey='A', Settings=new List<double>() { 1, 1 } },
            new  SplitJson() { EAID=2, CellCount = 2, SplitKey='B', Settings=new List<double>() { 1, 1 } },
            new  SplitJson() { EAID=3, CellCount = 2, SplitKey='C', Settings=new List<double>() { 7, 3 } },
            new  SplitJson() { EAID=4, CellCount = 2, SplitKey='D', Settings=new List<double>() { 3, 7 } },

            new  SplitJson() { EAID=5, CellCount = 3, SplitKey='A', Settings=new List<double>() { 1, 1, 1 } },
            new  SplitJson() { EAID=6, CellCount = 3, SplitKey='B', Settings=new List<double>() { 1, 1, 1 } },
            new  SplitJson() { EAID=7, CellCount = 3, SplitKey='C', Settings=new List<double>() { 3, 4, 3 } },
            new  SplitJson() { EAID=8, CellCount = 3, SplitKey='D', Settings=new List<double>() { 1, 1, 1, 1 } },
            new  SplitJson() { EAID=9, CellCount = 3, SplitKey='E', Settings=new List<double>() { 1, 1, 1, 1 } },
            new  SplitJson() { EAID=10, CellCount = 3, SplitKey='F', Settings=new List<double>() { 1, 1, 3, 7 } },
            new  SplitJson() { EAID=11, CellCount = 3, SplitKey='G', Settings=new List<double>() { 1, 1, 7, 3 } },
            new  SplitJson() { EAID=12, CellCount = 3, SplitKey='H', Settings=new List<double>() { 1, 1, 1, 1 } },
            new  SplitJson() { EAID=13, CellCount = 3, SplitKey='I', Settings=new List<double>() { 1, 1, 1, 1 } },

            new  SplitJson() { EAID=14, CellCount = 4, SplitKey='A', Settings=new List<double>() { 1, 1, 1, 1 } },
            new  SplitJson() { EAID=15, CellCount = 4, SplitKey='B', Settings=new List<double>() { 1, 1, 1, 1, 1 } },
            new  SplitJson() { EAID=16, CellCount = 4, SplitKey='C', Settings=new List<double>() { 1, 1, 1, 1, 1 } },
            new  SplitJson() { EAID=17, CellCount = 4, SplitKey='D', Settings=new List<double>() { 1, 1, 1, 1 } },
            new  SplitJson() { EAID=18, CellCount = 4, SplitKey='E', Settings=new List<double>() { 1, 1, 1, 1, 1 } },
            new  SplitJson() { EAID=19, CellCount = 4, SplitKey='F', Settings=new List<double>() { 1, 1, 1, 1, 1 } }

        };
        #endregion Preset List
        public static SplitJson? CreatePresetLayoutFromEAID(int eaId)
        {
            SplitJson? presetJson = SplitJson.PresetList.Find(x => x.EAID == eaId);
            if (presetJson != null)
            {
                return presetJson.Clone();
            }
            return null;
        }

        public bool IsOverlapLayout
        {
            get { return ((CellCount == 0) && (SplitKey == 'B')); }
        }
    }
}