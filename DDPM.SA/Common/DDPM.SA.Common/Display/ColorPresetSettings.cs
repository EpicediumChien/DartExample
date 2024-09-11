#region LicenceHeader

//
// Copyright © 2022, Dell Inc., All Rights Reserved.
// This material is confidential and a trade secret.  Permission to use this
// work for any purpose must be obtained in writing from Dell Inc.
//
// IDs.cs created on 10/4/2022T3:37 PM
//

#endregion

using System.Collections.Generic;
using VcpCore.Common;

namespace DDPM.SA.Common
{
    public class ColorPresetSettings_AppInfo
    {
        public string ColorPresetName;

        public string IconName;

        public ColorPresetSettings_AppInfo()
        {
            ColorPresetName = "";
            IconName = "";
        }

        public ColorPresetSettings_AppInfo(string name, string iconName)
        {
            ColorPresetName = name;
            IconName = iconName;
        }
    }

    public class ColorPresetSettings
    {
        public EDID DeviceInfo { get; set; }

        // 0 is Manual
        // 1 is by AppInfo Settings.
        public int RunType { get; set; }

        //public Dictionary<string, string>? AppInfo { get; set; }
        public Dictionary<string, ColorPresetSettings_AppInfo> AppInfo { get; set; }

        //public Dictionary<string, string> AppInfo { get; set; } = new Dictionary<string, string>() { };

        public string PresetForManual { get; set; }

        // 0 is Off
        // 1 is on
        public int ColorManagement_Status { get; set; }

        // 1 is Bymonitor - automatically adjust the ICC color profile based on monitor color preset
        // 2 is Byhost - Automatically adjust the monitor color preset based on ICC color profile 
        public int ColorManagement_RunType { get; set; }


    }
}