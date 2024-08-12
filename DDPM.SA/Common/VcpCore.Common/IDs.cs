#region LicenceHeader
//
// Copyright © 2022, Dell Inc., All Rights Reserved.
// This material is confidential and a trade secret.  Permission to use this
// work for any purpose must be obtained in writing from Dell Inc.
//
// IDs.cs created on 10/4/2022T3:37 PM
//
#endregion

namespace VcpCore.Common
{
    public class IDs
    {
        //VcpCore
        public const string VCP_CORE_PLUGIN_ID = "{A409E0AF-E2C3-4568-A194-B2D173DA26D4}";
        public const string VCP_CORE_AGENT_ID = "{41AADF9D-EF03-43E6-A88E-AE82A100B102}";
        public const string VCP_CORE_MUTEX_ID = "{FABC8B73-9743-424B-88EF-151BBD94CDE1}";

        //DisplayManager
        public const string Display_Manager_PLUGIN_ID = "{39A9CF54-2EC0-434E-A0BF-49FF43F8C824}";
        public const string Display_Manager_AGENT_ID = "{464160F1-2E14-4724-A78E-5375D9E80C6F}";
        public const string Display_Manager_MUTEX_ID = "{96F17284-F2F3-4419-8F02-F2EC6CD26143}";

        /// <summary>
        /// UniqueId for the thick client (NGA)
        /// </summary>
        public const string ThickClientUniqueGuid = "{706B7610-0F15-4CA2-8373-DADD04E25BB4}";

        /// <summary>
        /// UniqueID for the Systray
        /// </summary>
        public const string SysTrayUniqueGuid = "{827D5FED-4299-4FF0-859B-FB624A41180E}";
    }
}
