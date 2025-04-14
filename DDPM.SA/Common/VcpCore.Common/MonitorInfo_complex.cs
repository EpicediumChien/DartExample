using System;
using System.Collections.Generic;
using static VcpCore.Common.User32;

namespace VcpCore.Common
{
    [Serializable]
    public class MonitorInfo_complex : IEquatable<MonitorInfo_complex>
    {
        public List<string> UnDefinedColorPreset = new List<string>();
        public Dictionary<string, Dictionary<string, string>> ColorPresentDescription = new Dictionary<string, Dictionary<string, string>>();
        public Dictionary<string, List<string>> CapabilityDic = new Dictionary<string, List<string>>();
        public string AliasDeviceName = string.Empty;
        public IntPtr Handle { get; set; } = new IntPtr();
        public bool IsDellMonitor { get; set; } = false;
        public int Index { get; set; } = 0x0;
        public string CapabilityString { get; set; } = string.Empty;
        public IntPtr hMonitor { get; set; } = new IntPtr();
        public IntPtr hPhysicalMonitor { get; set; } = new IntPtr();
        public string szPhysicalMonitorDescription { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public bool DDCisON { get; set; } = false;
        public int DDCCIFail { get; set; } = 0x0;
        public EDID edid { get; set; } = new EDID();
        public DISPLAY_DEVICE displaydevice { get; set; } = new DISPLAY_DEVICE();
        public MonitorInfoEx pMonitorInfoEx { get; set; } = new MonitorInfoEx();
        public DEVMODE pDevmode { get; set; } = new DEVMODE();
        public List<string> ColorPresetSupportList { get; set; } = new List<string>();
        public string FwVersion { get; set; } = string.Empty;
        public string inputSource { get; set; } = string.Empty;
        public string inputCable { get; set; } = string.Empty;
        public DISPLAYCONFIG_PATH_INFO pathInfoTarget { get; set; } = new DISPLAYCONFIG_PATH_INFO();
        public List<string> SmartHDRSupportList { get; set; } = new List<string>();
        public string modelName { get; set; } = string.Empty;
        public string series { get; set; } = string.Empty;
        public string MarketingName { get; set; } = string.Empty;
        public string ImageFileName { get; set; } = string.Empty;
        public string SupplierID { get; set; } = string.Empty;
        public string D_Ctrl { get; set; } = string.Empty;
        public double scalingFactor { get; set; } = 0x0;
        public uint cPhysicalMonitors_index { get; set; } = 0x0;
        public bool IsSupportDisplay { get; set; } = false;

        public void UpToDate(MonitorInfo_complex other)
        {
            UnDefinedColorPreset = other.UnDefinedColorPreset;
            ColorPresentDescription = other.ColorPresentDescription;
            CapabilityDic = other.CapabilityDic;
            AliasDeviceName = other.AliasDeviceName;
            Handle = other.Handle;
            IsDellMonitor = other.IsDellMonitor;
            Index = other.Index;
            CapabilityString = other.CapabilityString;
            hMonitor = other.hMonitor;
            hPhysicalMonitor = other.hPhysicalMonitor;
            szPhysicalMonitorDescription = other.szPhysicalMonitorDescription;
            DisplayName = other.DisplayName;
            DDCisON = other.DDCisON;
            DDCCIFail = other.DDCCIFail;
            edid = other.edid;
            displaydevice = other.displaydevice;
            pMonitorInfoEx = other.pMonitorInfoEx;
            pDevmode = other.pDevmode;
            ColorPresetSupportList = other.ColorPresetSupportList;
            FwVersion = other.FwVersion;
            inputSource = other.inputSource;
            inputCable = other.inputCable;
            pathInfoTarget = other.pathInfoTarget;
            SmartHDRSupportList = other.SmartHDRSupportList;
            modelName = other.modelName;
            series = other.series;
            MarketingName = other.MarketingName;
            ImageFileName = other.ImageFileName;
            SupplierID = other.SupplierID;
            D_Ctrl = other.D_Ctrl;
            scalingFactor = other.scalingFactor;
            cPhysicalMonitors_index = other.cPhysicalMonitors_index;
            IsSupportDisplay = other.IsSupportDisplay;
        }

        public MonitorInfo ToMonitorInfo()
        {
            return new MonitorInfo()
            {
                AliasDeviceName = AliasDeviceName,
                IsDellMonitor = IsDellMonitor,
                Index = Index,
                CapabilityString = CapabilityString,
                DDCisON = DDCisON,
                DisplayName = DisplayName,
                edid = edid,
                FwVersion = FwVersion,
                inputSource = inputSource,
                inputCable = inputCable,
                CapabilityDic = CapabilityDic,
                modelName = modelName,
                series = series,
                MarketingName = MarketingName,
                ImageFileName = ImageFileName,
                SupplierID = SupplierID,
                D_Ctrl = D_Ctrl,
                scalingFactor = scalingFactor,
            };
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as MonitorInfo_complex);
        }

        public bool Equals(MonitorInfo_complex other)
        {
            //bool b0 = (other is not null);
            //if (b0)
            //{
            //    bool b1 = (AliasDeviceName == other.AliasDeviceName);
            //    bool b2 = (IsDellMonitor == other.IsDellMonitor);
            //    bool b3 = (Index == other.Index);
            //    bool b4 = (CapabilityString == other.CapabilityString);
            //    bool b5 = (DisplayName == other.DisplayName);
            //    bool b6 = (DDCisON == other.DDCisON);
            //    bool b7 = (FwVersion == other.FwVersion);
            //    bool b8 = (inputSource == other.inputSource);
            //    bool b9 = (inputCable == other.inputCable);
            //    bool b10 = (modelName == other.modelName);
            //    bool b11 = (series == other.series);
            //    bool b12 = (MarketingName == other.MarketingName);
            //    bool b13 = (ImageFileName == other.ImageFileName);
            //    bool b14 = (SupplierID == other.SupplierID);
            //    bool b15 = (D_Ctrl == other.D_Ctrl);
            //    bool b16 = (edid.Equals(other.edid));
            //    bool b17 = (IsSupportDisplay == other.IsSupportDisplay);

            //    return (b0 && b1 && b2 && b3 && b4 && b5 && b6 && b7 && b8 && b9 && b10 && b11 && b12 && b13 && b14 && b15 && b16 && b17);
            //}
            //else
            //    return b0;

            return (other is not null) &&
                   (AliasDeviceName == other.AliasDeviceName) &&
                   (IsDellMonitor == other.IsDellMonitor) &&
                   (Index == other.Index) &&
                   (CapabilityString == other.CapabilityString) &&
                   (DisplayName == other.DisplayName) &&
                   (DDCisON == other.DDCisON) &&
                   (FwVersion == other.FwVersion) &&
                   (inputSource == other.inputSource) &&
                   (inputCable == other.inputCable) &&
                   (modelName == other.modelName) &&
                   (series == other.series) &&
                   (MarketingName == other.MarketingName) &&
                   (ImageFileName == other.ImageFileName) &&
                   (SupplierID == other.SupplierID) &&
                   (D_Ctrl == other.D_Ctrl) &&
                   (IsSupportDisplay == other.IsSupportDisplay) &&
                   (edid.Equals(other.edid));
        }

        //If override Equals, need to implement GetHashCode also
        public override int GetHashCode()
        {
            return new
            {
                AliasDeviceName,
                IsDellMonitor,
                Index,
                CapabilityString,
                DisplayName,
                DDCisON,
                FwVersion,
                inputSource,
                inputCable,
                modelName,
                series,
                MarketingName,
                ImageFileName,
                SupplierID,
                D_Ctrl,
                edid,
                IsSupportDisplay,
            }.GetHashCode();
        }
    }
}