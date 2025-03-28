using System;

namespace VcpCore.Common
{
    [Serializable]
    public class EDID : IEquatable<EDID>
    {
        public string ManufactureID { get; set; } = string.Empty;
        public string PID { get; set; } = string.Empty;
        public string VendorID { get; set; } = string.Empty;
        public int Year { get; set; } = 0x0;
        public int Month { get; set; } = 0x0;
        public int Week { get; set; } = 0x0;
        public string ModelName { get; set; } = string.Empty;
        public string EdidVersion { get; set; } = string.Empty;
        public string VideoInputType { get; set; } = string.Empty;
        public float Size { get; set; } = 0x0;
        public string ServiceTag { get; set; } = string.Empty;
        public string SerialNumber { get; set; } = string.Empty;
        public string Edid { get; set; } = string.Empty;

        public override bool Equals(object obj)
        {
            return Equals(obj as EDID);
        }

        public bool Equals(EDID other)
        {
            bool b0 = (other is not null);
            if (b0)
            {
                bool b1 = (ManufactureID == other.ManufactureID);
                bool b2 = (PID == other.PID);
                bool b3 = (VendorID == other.VendorID);
                bool b4 = (Year == other.Year);
                bool b5 = (Month == other.Month);
                bool b6 = (Week == other.Week);
                bool b7 = (ModelName == other.ModelName);
                bool b8 = (EdidVersion == other.EdidVersion);
                bool b9 = (VideoInputType == other.VideoInputType);
                bool b10 = (Size == other.Size);
                bool b11 = (ServiceTag == other.ServiceTag);
                bool b12 = (SerialNumber == other.SerialNumber);
                bool b13 = (Edid == other.Edid);

                return (b0 && b1 && b2 && b3 && b4 && b5 && b6 && b7 && b8 && b9 && b10 && b11 && b12 && b13);
            }
            else
                return b0;

            //return other is not null &&
            //       ManufactureID == other.ManufactureID &&
            //       PID == other.PID &&
            //       VendorID == other.VendorID &&
            //       Year == other.Year &&
            //       Month == other.Month &&
            //       Week == other.Week &&
            //       ModelName == other.ModelName &&
            //       EdidVersion == other.EdidVersion &&
            //       VideoInputType == other.VideoInputType &&
            //       Size == other.Size &&
            //       ServiceTag == other.ServiceTag &&
            //       SerialNumber == other.SerialNumber &&
            //       Edid == other.Edid;
        }

        //If override Equals, need to implement GetHashCode also
        public override int GetHashCode()
        {
            return new
            {
                ManufactureID,
                PID,
                VendorID,
                Year,
                Month,
                Week,
                ModelName,
                EdidVersion,
                VideoInputType,
                Size,
                ServiceTag,
                SerialNumber,
                Edid,
            }.GetHashCode();
        }
    }
}