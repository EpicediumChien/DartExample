using System;
using System.Collections.Generic;
using System.Text.Json;

namespace VcpCore.Common
{
    [Serializable]
    public class MonitorInfo : BaseClone<MonitorInfo>, IEquatable<MonitorInfo>
    {
        public string AliasDeviceName { get; set; } = string.Empty;
        public bool IsDellMonitor { get; set; } = false;
        public int Index { get; set; } = 0x0;
        public string CapabilityString { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public bool DDCisON { get; set; } = false;
        public EDID edid { get; set; } = new EDID();
        public string FwVersion { get; set; } = string.Empty;
        public string inputSource { get; set; } = string.Empty;
        public string inputCable { get; set; } = string.Empty;
        public Dictionary<string, List<string>> CapabilityDic { get; set; } = new Dictionary<string, List<string>>();
        public string modelName { get; set; } = string.Empty;
        public string series { get; set; } = string.Empty;
        public string MarketingName { get; set; } = string.Empty;
        public string ImageFileName { get; set; } = string.Empty;
        public string SupplierID { get; set; } = string.Empty;
        public string D_Ctrl { get; set; } = string.Empty;
        public double scalingFactor { get; set; } = 0x0;

        public MonitorInfo ShallowCopy()
        {
            return (MonitorInfo)this.MemberwiseClone();
        }

        public override MonitorInfo Clone()
        {
            var bytes = JsonSerializer.SerializeToUtf8Bytes(this);
            return JsonSerializer.Deserialize<MonitorInfo>(bytes);
        }

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
            //    bool b7 = (FwVersion == other.FwVersion);
            //    bool b8 = (inputSource == other.inputSource);
            //    bool b9 = (inputCable == other.inputCable);
            //    bool b10 = (modelName == other.modelName);
            //    bool b11 = (series == other.series);
            //    bool b12 = (MarketingName == other.MarketingName);
            //    bool b13 = (ImageFileName == other.ImageFileName);
            //    bool b14 = (SupplierID == other.SupplierID);
            //    bool b15 = (D_Ctrl == other.D_Ctrl);
            //    bool b16 = (EqualityComparer<EDID>.Default.Equals(edid, other.edid));

            //    return (b0 && b1 && b2 && b3 && b4 && b5 && b6 && b7 && b8 && b9 && b10 && b11 && b12 && b13 && b14 && b15 && b16);
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