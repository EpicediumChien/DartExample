using DDPM.SA.Common.Display;
using System.Collections.Generic;

namespace DDPM.SA.Common
{
    //2024-7-18 Robert_Lin added for EasyArrange commands/events
    public class EAArgs
    {
        //"EditCommand","EditError", "EditCancel"
        public string Command { get; set; }

        //Robert_Lin, 2024-10-13 Merge properties into SplitJson
        public SplitJson SplitJson { get; set; }

        public int CellCount { get; set; }
        public char SplitKey { get; set; }
        public long CustomId { get; set; }
        public List<double> Settings { get; set; }
        public List<string> CustomNames { get; set; }
        public string CustomName { get; set; }
        public bool Result { get; set; }
        public string Message { get; set; }

        #region ctor

        //Default ctor
        public EAArgs()
        {
        }

        //Copy ctor
        public EAArgs(EAArgs other)
        {
            this.Command = other.Command;
            this.CellCount = other.CellCount;
            this.SplitKey = other.SplitKey;
            this.CustomId = other.CustomId;
            this.CustomNames = other.CustomNames;
            this.CustomName = other.CustomName;
            this.Result = other.Result;
            this.Message = other.Message;
            this.Settings = new List<double>(other.Settings);
        }

        #endregion ctor
    }
}