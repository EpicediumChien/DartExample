namespace DDPM.UI.Common.EAEM
{
    /// <summary>
    /// eSplitOwner: A data member of SplitItem and SplitListView, specifys that its owner
    /// For example, It may put into PBP ListView, then its Owner will be PbpList
    /// </summary>
    ///
    public enum eSplitOwner
    {
        None = 0, PxpOff, PipList, PbpList,
        EaWin, EaRecent, EaCustom
    }
}