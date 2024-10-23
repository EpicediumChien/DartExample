using System;
using System.Collections.Generic;

namespace VcpCore.Common
{
    [Serializable]
    public class MonitorInfo : IEquatable<MonitorInfo>
    {
        public string AliasDeviceName;
        public bool IsDellMonitor { get; set; }
        public int Index { get; set; }
        public string CapabilityString { get; set; }
        public string DisplayName { get; set; }
        public bool DDCisON { get; set; }
        public EDID edid { get; set; }
        public string FwVersion { get; set; }
        public string inputSource { get; set; }
        public string inputCable { get; set; }
        public Dictionary<string, List<string>> CapabilityDic;
        public string modelName { get; set; }
        public string series { get; set; }
        public string MarketingName { get; set; }
        public string ImageFileName { get; set; }
        public string SupplierID { get; set; }
        public string D_Ctrl { get; set; }

        public override bool Equals(object obj)
        {
            return Equals(obj as MonitorInfo);
        }

        public bool Equals(MonitorInfo other)
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
            //    bool b7 = (EqualityComparer<EDID>.Default.Equals(edid, other.edid));

            //    return (b0 && b1 && b2 && b3 && b4 && b5 && b6 && b7);
            //}
            //else
            //    return b0;

            return (other is not null) &&
                   //(CapabilityDic.Count == other.CapabilityDic.Count) &&
                   //(!CapabilityDic.Except(other.CapabilityDic).Any()) &&
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
                   (EqualityComparer<EDID>.Default.Equals(edid, other.edid));
        }

        //If override Equals, need to implement GetHashCode also
        public override int GetHashCode()
        {
            return CapabilityString.GetHashCode();
        }
    }
}