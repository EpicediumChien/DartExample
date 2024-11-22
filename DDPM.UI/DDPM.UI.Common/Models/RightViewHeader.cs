using DDPM.UI.Interfaces;

namespace DDPM.UI.Common.Models
{
    public class RightViewHeader
    {
        public int Id { get; set; }
        public string Text { get; set; }

        // << 240603 Added by Hess to support show icon
        public string ImageFile { get; set; } = "";

        // >>

        //Module
        public IDdpmModule? DdpmModule { get; set; }

        public Type ModuleType { get; set; }

        #region ctor

        public RightViewHeader(int id, string text, IDdpmModule? mod = null)
        {
            Id = id;
            Text = text;
            DdpmModule = mod;
        }

        #endregion ctor

        //Robert_Lin, 2024-7-26, To hide the header which is not supported
        public bool IsShown { get; set; } = true;

        //Robert_Lin, 2024-11-15 add a ModuleName
        //In general, we can get ModuleName from DdpmModule.ModuleName
        //But in case of DdpmModule is null (the module has not been created/shown)
        //We did need the ModuleName when we would like to find the RightViewHeader of that Module
        public string ModuleName { get; set; } = "";
    }
}