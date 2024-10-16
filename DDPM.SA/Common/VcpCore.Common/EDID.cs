using System;

namespace VcpCore.Common
{
    [Serializable]
    public class EDID : IEquatable<EDID>
    {
        public string ManufactureID { get; set; }
        public string PID { get; set; }
        public string VendorID { get; set; }
        public int Year { get; set; }
        public int Month;
        public int Week;
        public string ModelName { get; set; }
        public string EdidVersion { get; set; }
        public string VideoInputType { get; set; }
        public float Size { get; set; }
        public string ServiceTag { get; set; }
        public string SerialNumber { get; set; }
        public string Edid { get; set; }

        public override bool Equals(object obj)
        {
            return Equals(obj as EDID);
        }

        public bool Equals(EDID other)
        {
            //bool b0 = (other is not null);
            //if (b0)
            //{
            //    bool b1 = (ManufactureID == other.ManufactureID);
            //    bool b2 = (VendorID == other.VendorID);
            //    bool b3 = (Year == other.Year);
            //    bool b4 = (Month == other.Month);
            //    bool b5 = (ModelName == other.ModelName);
            //    bool b6 = (Size == other.Size);
            //    bool b7 = (ServiceTag == other.ServiceTag);
            //    bool b8 = (SerialNumber == other.SerialNumber);
            //    bool b9 = (Edid == other.Edid);

            //    return (b0 && b1 && b2 && b3 && b4 && b5 && b6 && b7 && b8 && b9);
            //}
            //else
            //    return b0;

            return other is not null &&
                   ManufactureID == other.ManufactureID &&
                   PID == other.PID &&
                   VendorID == other.VendorID &&
                   Year == other.Year &&
                   Month == other.Month &&
                   Week == other.Week &&
                   ModelName == other.ModelName &&
                   EdidVersion == other.EdidVersion &&
                   VideoInputType == other.VideoInputType &&
                   Size == other.Size &&
                   ServiceTag == other.ServiceTag &&
                   SerialNumber == other.SerialNumber &&
                   Edid == other.Edid;
        }

        //If override Equals, need to implement GetHashCode also
        public override int GetHashCode()
        {
            return ModelName.GetHashCode();
        }
    }
}