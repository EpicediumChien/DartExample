namespace DDPM.UI.Common
{
    [Serializable]
    public class EDID
    {
        public string ManufactureID { get; set; }
        public string VendorID { get; set; }
        public int Year { get; set; }
        public int Month;
        public string ModelName { get; set; }
        public float Size { get; set; }
        public string ServiceTag { get; set; }
        public string SerialNumber { get; set; }
        public string Edid { get; set; }
    }
}