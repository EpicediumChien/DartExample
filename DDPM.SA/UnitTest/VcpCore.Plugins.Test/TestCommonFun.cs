using VcpCore.Common;

namespace VcpCore.Plugins.Test
{
    public class TestCommonFun
    {
        private EDID eDID = new EDID()
        {
            ManufactureID = "DELL",
            VendorID = "42DD",
            Year = 2023,
            Month = 5,
            Week = 22,
            ModelName = "DELLU2724DD",
            EdidVersion = "V1.3",
            VideoInputType = "Digital Signal",
            Size = 27.1510868f,
            ServiceTag = "CN073K0",
            SerialNumber = "808597688",
            Edid = "00FFFFFFFFFFFF0010ACDC425538323016210103803C2278EA62A5AD5046AB240E5054A54B00714F8180A940D1C081C0A9C001010101565E00A0A0A029503020350055502100001A000000FF00434E3037334B300A2020202020000000FC0044454C4C20553237323444450A000000FD0030781EB23C000A20202020202001ED"
        };

        [Test]
        public void TestgetEDID()
        {
            bool getedid1 = false;
            string MontitorID1 = "MONITOR\\DEL42BC\\{6e4147b2-d553-535f-a3bb-9b018e8ab8f7}";  //"MONITOR\\DEL42DC\\{6e4147b2-d553-535f-a3bb-9b018e8ab8f6}"
            EDID edid1 = eDID;
            var getEDIDResult1 = CommonFun.getEDID(MontitorID1, ref edid1);
            Assert.That(getedid1, Is.EqualTo(getEDIDResult1));
            Assert.IsNotNull(edid1);
        }

        [Test]
        public void TestConvertManufacturerID()
        {
            string hexManufacturerID1 = "";
            if (string.IsNullOrEmpty(hexManufacturerID1) || hexManufacturerID1.Length < 4)
            {
                var ManufacturerIDResult1 = CommonFun.ConvertManufacturerID(hexManufacturerID1);
                Assert.That(string.IsNullOrEmpty(ManufacturerIDResult1));
                Assert.That(hexManufacturerID1, Is.EqualTo(ManufacturerIDResult1));
            }

            string hexManufacturerID2 = "AC10";
            string ConvertManufacturerID = "DEL";
            if (!string.IsNullOrEmpty(hexManufacturerID2) || hexManufacturerID2.Length >= 4)
            {
                var ManufacturerIDResult2 = CommonFun.ConvertManufacturerID(hexManufacturerID2);
                Assert.That(!string.IsNullOrEmpty(ManufacturerIDResult2));
                Assert.That(ConvertManufacturerID, Is.EqualTo(ManufacturerIDResult2));
            }
        }

        [Test]
        public void Testint2charByASCII()
        {
            int a = 1;
            string ASCII1 = "A";
            int b = 2;
            string ASCII2 = "B";
            int c = 3;
            string ASCII3 = "C";
            var ASCIIResult1 = CommonFun.int2charByASCII(a);
            Assert.That(ASCII1, Is.EqualTo(ASCIIResult1));
            var ASCIIResult2 = CommonFun.int2charByASCII(b);
            Assert.That(ASCII2, Is.EqualTo(ASCIIResult2));
            var ASCIIResult3 = CommonFun.int2charByASCII(c);
            Assert.That(ASCII3, Is.EqualTo(ASCIIResult3));
        }

        [Test]
        public void TestStringToByteArray()
        {
            byte[] bytes = { 1, 255, 255, 4, 5, 6, 7, 8, 9 };
            string hex = eDID.Edid;
            var result = CommonFun.StringToByteArray(hex);
            Assert.Greater(result.Length, bytes.Length);
            Assert.That(result[1], Is.EqualTo(bytes[1]));
            Assert.That(result[2], Is.EqualTo(bytes[2]));
        }
    }
}