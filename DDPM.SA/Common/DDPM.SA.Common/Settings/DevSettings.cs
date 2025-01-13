using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDPM.SA.Common.Settings
{

    //Usage:
    //1  Add key in Windows Registry
    //  [HKLM\SOFTWARE\Dell\Dell Display And Peripheral Manager\UserSettings]
    //  yourKeyName=yourKeValue
    //
    //  For example:
    // 
    //  [HKLM\SOFTWARE\Dell\Dell Display And Peripheral Manager\UserSettings]
    //  "DDPM.SA.EAPlugin.InfoWindow.IsVisible"=REG_DWORD:1
    //  or 
    //   "DDPM.SA.EAPlugin.InfoWindow.IsVisible"=REG_SZ:"1"
    //
    //2 Add an method access to the flag
    //  See IsEAInfoWindowVisible() as the example.
    //
    //3 If the flag may be access very high frequency, please read once at the dirst time
    //  Example: (this flag will be use verytime when the UserControl is hoverin)
    //  private static bool? _isShowDebugInfo = null;
    //  public string TooltipText
    //  {
    //     get
    //     {
    //          if (_isShowDebugInfo == null)
    //              _isShowDebugInfo = DevSettings.IsShowDebugInfo();
    //          if (_isShowDebugInfo)
    //             return "your debug info";
    //          else
    //             return "default info";
    //     }
    //  }

    /// <summary>
    /// The settings of development
    /// </summary>
    public class DevSettings
    {
        #region Private members
        private const string SubKey = @"SOFTWARE\Dell\Dell Display And Peripheral Manager\UserSettings";

        //Return value examples:
        // Type          Value    Return value
        // REG_DWORD     1        1
        // REG_QWORD     1        1 
        // REG_SZ        "1"      1
        // REG_SZ        "001"    0 (Causes exception inside ReadInt())
        // REG_SZ        "0x01"   0 (Causes exception inside ReadInt())
        // (keyName is not found) 0
        private static int ReadInt(string keyName, int defaultValue = 0)
        { 
            object o = DDPMRegistryHelper.ReadRegistryKey(RegistryHive.LocalMachine, SubKey, keyName);
            //If the specified keyName is not exist, then return the defaultValue
            if (o == null)
                return defaultValue;

            //Convert object o to int as return value
            try
            {
                int retValue = Convert.ToInt32(o);
                return retValue;
            }
            catch (Exception e1)
            {
                Trace.WriteLine(e1);
            }
            return defaultValue;
        }
        #endregion Private members

        #region EasyArrange
        /// <summary>
        /// A flag to show EABroker.InfoWIndow
        /// </summary>
        /// <returns></returns>
        public static bool IsEAInfoWindowVisible()
        {
            return 1 == DevSettings.ReadInt("DDPM.SA.EAPlugin.InfoWindow.IsVisible");
        }

        /// <summary>
        /// Show the "Save Icon Images..." button in DDPM.UI.Module.EzArrange.EzArrangeRightVierw.xaml
        /// It allows user to export all EA Layout icons to *.PNG files.
        /// </summary>
        /// <returns></returns>
        public static bool IsEASaveSplitCtrlsToPngFilesButtonEnabled()
        {
            return 1 == DevSettings.ReadInt("EzArrange.SaveSplitCtrlsToPngFilesButtonEnabled");
        }

        public static bool IsEAShowDebugInfoInTooltip()
        {
            return 1 == DevSettings.ReadInt("EzArrange.SplitItem.Tooltip.ShowDebugInfo");
        }

        /// <summary>
        /// Dump WorkWindows and its information to log file after RefreshWorkWindows
        /// </summary>
        /// <returns></returns>
        public static bool IsDumpWorkWindowsInfoOnRefreshEnabled()
        {
            return 1 == ReadInt("EABroker.DumpWorkWindowsInfoOnRefreshed");
        }
        #endregion EasyArrange

        #region DdpmHomePlgin

        /// <summary>
        /// When HomeDevices count is zero, will add a FakeMonitor device to HomeDevices
        /// </summary>
        /// <returns></returns>
        public static bool DdpmHomeAddFakeMonitorIfHomeDevicesEmpty()
        {
            return 1 == DevSettings.ReadInt("HomePlugin.GetDdpmDevicesAsync.AddFakeMonitorIfEmpty");
        }

        /// <summary>
        /// Show the debug toolbar at DdpmHomePage, allow engineer to add fake devices, and show the ListViewItem.Border.
        /// </summary>
        /// <returns></returns>
        public static bool DdpmHomeShowDeviceListViewToolbar()
        {
            return 1 == DevSettings.ReadInt("HomePage.ShowDeviceListViewToolbar");
        }
        #endregion DdpmHomePlgin

        #region PIP PBP
        /// <summary>
        /// Add all known PBP modes in DDPM.UI.Model.PipPbp.PipPbpRightView PBP mode ListView.
        /// It's used to check Light/Dark theme, and tooltips for all PBP modes.
        /// </summary>
        /// <returns></returns>
        public static bool IsPxpModelListViewAddAllModes()
        {
            return 1 == DevSettings.ReadInt("PipPbp.PbpModeListView.AddAllModes");
        }

        /// <summary>
        /// To show all VideoSwap ComboBoxes in DDPM.UI.Module.PipPbp.PipPbpRightView
        /// It'd used to check the ComboxBoxes UI/functions when you do not connected a supported (3~4 splits) monitors.
        /// </summary>
        /// <returns></returns>
        public static bool IsPxpVideoSwapComboBoxAlwaysVisible()
        {
            return 1 == DevSettings.ReadInt("PipPbp.VideoSwapComboBox.AlwaysVisible");
        }
        #endregion PIP PBP
    }
}
