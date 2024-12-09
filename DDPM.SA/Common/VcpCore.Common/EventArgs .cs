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
        public string vcpcode { get; set; } = string.Empty;
        public string value { get; set; } = string.Empty;
        public MonitorInfo monitor { get; set; } = new MonitorInfo();
    }

    public class DisplaychangedEventArgs : EventArgs
    {
        public int count { get; set; } = 0x0;
        public List<MonitorInfo> monitors { get; set; } = new List<MonitorInfo>();
    }

    public class DDCCIchangedEventArgs : EventArgs
    {
        public bool DDCisON { get; set; } = false;
        public MonitorInfo monitors { get; set; } = new MonitorInfo();
    }

    public class ReadWriteRequest : EventArgs
    {
        public ReadWriteRequest_Type service { get; set; } = new ReadWriteRequest_Type();
        public MonitorInfo monitor { get; set; } = new MonitorInfo();
    }

    public enum ReadWriteRequest_Type
    {
        Read,
        Write,
    }
}