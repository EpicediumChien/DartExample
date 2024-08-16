#region LicenceHeader

//
// Copyright © 2022, Dell Inc., All Rights Reserved.
// This material is confidential and a trade secret.  Permission to use this
// work for any purpose must be obtained in writing from Dell Inc.
//
// IDs.cs created on 10/4/2022T3:37 PM
//

#endregion

using System;
using System.Collections.Generic;

namespace VcpCore.Common
{
    public class VCPchangedEventArgs : EventArgs
    {
        public string vcpcode { get; set; }
        public string value { get; set; }
        public MonitorInfo monitor { get; set; }
    }

    public class DisplaychangedEventArgs : EventArgs
    {
        public int count { get; set; }
        public List<MonitorInfo> monitors { get; set; }
    }

    public class DDCCIchangedEventArgs : EventArgs
    {
        public bool DDCisON { get; set; }
        public MonitorInfo monitors { get; set; }
    }
}