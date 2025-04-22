namespace DDPM.SA.Common
{
    public enum PopupContentPackage_Enum
    {
        Unknow,
        FWU,
        SWU
    }
    /// <summary>
    /// Popup內容包
    /// </summary>
    public class PopupContentPackage
    {
        public string Title { get; set; }
        public string Info { get; set; }
        public bool IsInfo { get; set; }
        public bool IsOnlyUpdate { get; set; }
        public bool StayOpen { get; set; }
        public int Timeout { get; set; }
        public bool IsNeedButton { get; set; }

        /// <summary>
        /// Popup事件回傳的物件
        /// </summary>
        public object Object { get; set; }
        public PopupContentPackage_Enum PopupType { get; set; }

        public PopupContentPackage()
        {
            IsInfo = true;
            IsOnlyUpdate = false;
            StayOpen = false;
            Timeout = 5;
            IsNeedButton = true;
            PopupType = PopupContentPackage_Enum.Unknow;
        }
    }
}