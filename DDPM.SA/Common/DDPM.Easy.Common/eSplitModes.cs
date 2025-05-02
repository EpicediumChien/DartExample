namespace DDPM.Easy.Common
{
    public enum eSplitModes
    {
        /// <summary>
        /// The SplitCtrl is used as an Icon in GUI, for example, in a SplitListView.
        /// It's default mode.
        /// </summary>
        Icon,

        /// <summary>
        /// The SplitCtrl is under Editing.
        /// </summary>
        Edit,

        /// <summary>
        /// The SplitCtrl are working in WorkWindow
        /// </summary>
        Work,

        /// <summary>
        /// The SplitCtrl is unsing AWS (Application Window Snap) mode
        /// Robert_Lin, 2024-10-2 add
        /// </summary>
        AWS,

        /// <summary>
        /// The SplitCtrl is unsing EasyMemory mode
        /// </summary>
        Em
    }
}