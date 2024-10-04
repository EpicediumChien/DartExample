using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

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

        #endregion Native Properties


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
        /// </summary>
        public static List<SplitJson> DefaultRecentList = new List<SplitJson>()
        {
            //[0] SplitCtrl0A (Off)
            new  SplitJson() { CellCount = 0, SplitKey='A' },
            //[1] SplitCtrl2A
            new  SplitJson() { CellCount = 2, SplitKey='A', Settings=new List<double>() { 1, 1 } },
            //[2] SplitCtrl2C
            new  SplitJson() { CellCount = 2, SplitKey='C', Settings=new List<double>() { 7, 3 } },
            //[3] SplitCtrl3E
            new  SplitJson() { CellCount = 3, SplitKey='E', Settings=new List<double>() { 1, 1, 1, 1 } },
            //[4] SplitCtrl4A
            new  SplitJson() { CellCount = 4, SplitKey='A', Settings=new List<double>() { 1, 1, 1, 1, 1 } }

        };
        #endregion Defaul Recent List
    }
}