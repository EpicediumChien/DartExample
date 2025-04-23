using IndiLogic.DPeM.Broker.WiredAudio;
using System;
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
    public class SplitJson : IDisposable
    {
        #region Native Properties

        public int CellCount { get; set; } = 0;
        public char SplitKey { get; set; } = 'A';
        public List<double> Settings { get; set; } = new List<double>();
        public string CustomName { get; set; } = "";
        /// <summary>
        /// Unused Id to identify a custom layout. Please used EAID instead.
        /// </summary>
        public long CustomId { get; set; } = 0;
        public int EAID { get; set; } = 0;
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

        public static SplitJson? CreatePresetLayoutFromEAID(int eaId)
        {
            SplitJson? presetJson = SplitJson.PresetList.Find(x => x.EAID == eaId);
            if (presetJson != null)
            {
                return presetJson.Clone();
            }
            return null;
        }

        #endregion ctor and create new instance

        #region Helper Fuctions
        /// <summary>
        /// Output the SplitJson content to string.
        /// Format: ({EAID},{CellCount}{SplitKey}[{Settings}]CustomName)
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"({EAID},{CellCount}{SplitKey}[{Double_To_String(Settings)}]{CustomName})";
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

        //Robert_Lin, 2024-12-12 General (Static) method to compare two Settings (List<double>) are the same.
        /// <summary>
        /// Compare two Settings (List of double) are equal.
        /// A. Both are null  : return true
        /// B. Both are non-null: Copmare with List.SequenceEqual() to compare if all elements in both list are the same values in sequence.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static bool AreSettingsEqual(List<double> x, List<double> y)
        {
            if ((x == null) && (y == null))
                return true;
            if (x == null) return false;
            if (y == null) return false;
            if (x.Count != y.Count)
                return false;

            return x.SequenceEqual(y);
        }
        #endregion Helper Fuctions

        #region Defaul Recent List
        //Robert_Lin, 2024-12-31 Add EAID
        /// <summary>
        /// Need to copy the DefaultSettings from DDPM.Easy.Common/SplitCtrlXX.xaml.cs
        /// Never add SplitCtrl0A (Off) into the RecentList
        /// </summary>
        public static readonly List<SplitJson> DefaultRecentList = new List<SplitJson>()
        {
            //[0] SplitCtrl2A
            new  SplitJson() { CellCount = 2, SplitKey='A', EAID=1, Settings=new List<double>() { 1, 1 } },
            //[1] SplitCtrl2C
            new  SplitJson() { CellCount = 2, SplitKey='C', EAID=3, Settings=new List<double>() { 7, 3 } },
            //[2] SplitCtrl3E
            new  SplitJson() { CellCount = 3, SplitKey='E', EAID=9, Settings=new List<double>() { 1, 1, 1, 1 } },
            //[3] SplitCtrl4A
            new  SplitJson() { CellCount = 4, SplitKey='A', EAID=14, Settings=new List<double>() { 1, 1, 1, 1, 1 } },
            //[4] SplitCtrl3C
            new  SplitJson() { CellCount = 3, SplitKey='C', EAID=7, Settings=new List<double>() { 3, 4, 3 } }

        };
        #endregion Defaul Recent List

        #region Preset List
        /// <summary>
        /// A list of all preset EA layouts used by DDPM.Saubagent.User/EABroker.
        /// It's used to create a SplitJson from EAID. used by Migration only. 
        /// And it can be considered to remove in the future.
        /// </summary>
        private static readonly List<SplitJson> PresetList = new List<SplitJson>()
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
            new  SplitJson() { EAID=19, CellCount = 4, SplitKey='F', Settings=new List<double>() { 1, 1, 1, 1, 1 } },

            new  SplitJson() { EAID=20, CellCount = 5, SplitKey='A', Settings=new List<double>() { 1, 1, 1, 1, 1, 1, 1 } },
            new  SplitJson() { EAID=21, CellCount = 5, SplitKey='B', Settings=new List<double>() { 1, 1, 1, 1, 1, 1, 1 } },
            new  SplitJson() { EAID=22, CellCount = 5, SplitKey='C', Settings=new List<double>() { 3, 7, 1, 1, 1, 1, 1 } },
            new  SplitJson() { EAID=23, CellCount = 5, SplitKey='D', Settings=new List<double>() { 7, 3, 1, 1, 1, 1, 1 } },

            new  SplitJson() { EAID=24, CellCount = 5, SplitKey='E', Settings=new List<double>() { 3, 7, 1, 1, 1, 1 } },
            new  SplitJson() { EAID=25, CellCount = 5, SplitKey='F', Settings=new List<double>() { 3, 7, 1, 1, 1, 1 } },
            new  SplitJson() { EAID=26, CellCount = 5, SplitKey='G', Settings=new List<double>() { 1, 1, 1, 1, 1, 1, 1 } },
            new  SplitJson() { EAID=27, CellCount = 5, SplitKey='H', Settings=new List<double>() { 7, 3, 7, 3, 1, 1 } },
            new  SplitJson() { EAID=28, CellCount = 5, SplitKey='I', Settings=new List<double>() { 3, 7, 7, 3, 1, 1 } },

            new  SplitJson() { EAID=29, CellCount = 6, SplitKey='A', Settings=new List<double>() { 1, 1, 1, 1, 1 } },
            new  SplitJson() { EAID=30, CellCount = 6, SplitKey='B', Settings=new List<double>() { 7, 3, 7, 3, 1, 1, 1, 1 } },
            new  SplitJson() { EAID=31, CellCount = 6, SplitKey='C', Settings=new List<double>() { 1, 1, 3, 7, 1, 1, 1, 1 } },
            new  SplitJson() { EAID=32, CellCount = 6, SplitKey='D', Settings=new List<double>() { 7, 3, 3, 7, 1, 1, 1, 1 } },

            new  SplitJson() { EAID=33, CellCount = 6, SplitKey='E', Settings=new List<double>() { 3, 7, 3, 7, 1, 1, 1, 1 } },
            new  SplitJson() { EAID=34, CellCount = 6, SplitKey='F', Settings=new List<double>() { 3, 7, 7, 3, 1, 1, 1, 1 } },
            new  SplitJson() { EAID=35, CellCount = 6, SplitKey='G', Settings=new List<double>() { 1, 1, 1, 1, 1 } },
            new  SplitJson() { EAID=36, CellCount = 6, SplitKey='H', Settings=new List<double>() { 1, 1, 1, 1, 1, 1, 1, 1 } },
            new  SplitJson() { EAID=37, CellCount = 6, SplitKey='I', Settings=new List<double>() { 1, 1, 7, 3, 1, 1, 1, 1 } },

            new  SplitJson() { EAID=38, CellCount = 6, SplitKey='J', Settings=new List<double>() { 7, 3, 1, 1, 1 } },

            new  SplitJson() { EAID=39, CellCount = 7, SplitKey='A', Settings=new List<double>() { 1, 1, 1, 3, 7, 1, 1 } },
            new  SplitJson() { EAID=40, CellCount = 7, SplitKey='B', Settings=new List<double>() { 1, 1, 1, 3, 7, 1, 1 } },
            new  SplitJson() { EAID=41, CellCount = 7, SplitKey='C', Settings=new List<double>() { 1, 1, 1, 1, 1, 1, 1, 1, 1 } },
            new  SplitJson() { EAID=42, CellCount = 7, SplitKey='D', Settings=new List<double>() { 1, 1, 1, 1, 1, 1, 1, 1, 1 } },

            new  SplitJson() { EAID=43, CellCount = 7, SplitKey='E', Settings=new List<double>() { 3, 7, 1, 2, 1, 1, 1, 1, 1, 1, 1 } },
            new  SplitJson() { EAID=44, CellCount = 7, SplitKey='F', Settings=new List<double>() { 7, 3, 1, 2, 1, 1, 1, 1, 1, 1, 1 } },
            new  SplitJson() { EAID=45, CellCount = 7, SplitKey='G', Settings=new List<double>() { 1, 1, 1, 1, 1, 1, 1 } },
            new  SplitJson() { EAID=46, CellCount = 7, SplitKey='H', Settings=new List<double>() { 1, 1, 1, 1, 1, 1, 1 } },
            new  SplitJson() { EAID=47, CellCount = 7, SplitKey='I', Settings=new List<double>() { 1, 1, 1, 1, 1, 1 } },

            new  SplitJson() { EAID=48, CellCount = 7, SplitKey='J', Settings=new List<double>() { 1, 1, 1, 1, 1, 1, 1 } },
            new  SplitJson() { EAID=49, CellCount = 7, SplitKey='K', Settings=new List<double>() { 3, 7, 1, 2, 1, 1, 1, 1, 1 } }
        };
        #endregion Preset List

        #region Layout types
        /// <summary>
        /// Return true if this SplitJson is an Overlap custom layout (SplitCrl0B).
        /// </summary>
        public bool IsOverlapLayout
        {
            get { return ((CellCount == 0) && (SplitKey == 'B')); }
        }

        /// <summary>
        /// Return true if this SplitJson is Empty layout (SplitCtrl0A).
        /// </summary>
        public bool IsOff
        {
            get { return ((CellCount == 0) && (SplitKey == 'A')); }
        }
        /// <summary>
        /// Return true if this SplitJson is a custom layout 
        /// (include Preset custom layout and Overlap custom layout)
        /// which is determined by (EAID >=1000)
        /// </summary>
        public bool IsCustomLayout
        {
            get
            {
                return (EAID >= EAEMConstants.EAID_FirstCustom); //>=1000
            }
        }

        /// <summary>
        /// For Overlap layout only. Return true if it can be comfirm it's migrated from DDM.
        /// </summary>
        public bool IsMigratedFromDdm
        {
            get
            {
                if (IsCustomLayout)
                {
                    if ((Settings != null) && (Settings.Count >= 8))
                    {
                        if ((Settings[2] == 1.000) && (Settings[3] == 1.0000))
                            return true;
                    }
                }
                return false;
            }
        }
        #endregion Layout types

        #region Destructor and Dispose
        private bool _isDisposed = false;
        private static readonly object _lockDisposePresetList = new object();
        ~SplitJson()
        {
            Dispose(false);
        }
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
                    if (Settings != null)
                    {
                        Settings.Clear();
                        Settings = null;
                    }
                    if (Cells != null)
                    {
                        Array.Clear(Cells, 0, Cells.Length);
                        Cells = null;
                    }
                }
                _isDisposed = true;
            }
        }

        public static void DisposePresetList()
        {
            lock (_lockDisposePresetList)
            {
                if ((SplitJson.PresetList != null) && (PresetList.Count > 0))
                {
                    foreach (var preset in SplitJson.PresetList)
                    {
                        preset.Dispose();
                    }
                    PresetList.Clear();
                }
            }
        }
        #endregion
    }
}