namespace DDPM.Easy.Common
{
    public class SplitJson_Unused
    {
        #region Native Properties

        public int CellCount { get; set; } = 0;
        public char SplitKey { get; set; } = 'A';
        public List<double> Settings { get; set; } = new List<double>();
        public string FriendlyName { get; set; } = "";
        public long CustomId { get; set; } = 0;
        public string Message { get; set; } = "";

        #endregion Native Properties

        //public static SplitJson CreateFromSplitItem(ISplitCtrl spCtrl)
        //{
        //    SplitJson obj = new SplitJson();
        //    obj.CellCount = spCtrl.CellCount;
        //    obj.SplitKey = spCtrl.SplitKey;
        //    obj.Settings = spCtrl.Settings;
        //    obj.FriendlyName = spCtrl.FriendlyName;
        //    return obj;
        //}
    }
}