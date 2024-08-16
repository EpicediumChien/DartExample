using System.Collections.Generic;

namespace DDPM.SA.Common.Display
{
    /// <summary>
    /// Represent the EasyArrange settings for a monitor unit.
    /// To identify a monitor unit can use {MonitorModel}+{SerialNumber}
    /// For example, the settings filename may be "{EA-{MonitorModel}_{SerialNum}.json"
    /// </summary>
    public class EAMonitorSettings
    {
        //Monitor ID, can be used to verify if the filename has been modilfied.
        public string MonitorModel { get; set; } = "(NOMODEL)";

        public string SerialNumber { get; set; } = "(NOSERIALNUM)";

        //Settings in DDPM.UI
        public SplitJson SelectedSplit { get; set; }

        public bool IsWidthoutGsp { get; set; }
        public bool IsOnlyAllowWhenShiftKeyPressed { get; set; }
        public bool IsSpanAcrossMultiMonitors { get; set; }
        public List<SplitJson> CustomList { get; set; }
        public List<SplitJson> RecentList { get; set; }

        #region Not been saved

        //Return the filename of current Monitor.
        // "EA-{MonitorModel}_{SerialNumber}.json
        public string GetFileName()
        {
            return GetFileName(MonitorModel, SerialNumber);
        }

        public static string GetFileName(string monitorModel, string serialNumber)
        {
            return $"EA-{monitorModel}_{serialNumber}.json";
        }

        #endregion Not been saved
    }
}