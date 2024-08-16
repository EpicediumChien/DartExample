using System.Collections.Generic;

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
    }
}