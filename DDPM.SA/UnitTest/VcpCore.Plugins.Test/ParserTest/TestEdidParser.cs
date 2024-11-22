using Dell.Client.Framework.UnitTestShared.Tests;
using VcpCore.Common;

namespace VcpCore.Plugins.Test.ParserTest
{
    public class TestEdidParser
    {
        //EdidParser edidparser = new EdidParser();
        private readonly string EDID_Header = "00FFFFFFFFFFFF00";

        private readonly string EDID_SerivceTag_Header = "000000FF00";

        private readonly string ModelName_Header = "000000FC00";

        private readonly int Manufacturer_ID_Len = 4;

        private readonly int VENDOR_ID_Len = 4;

        private readonly int SerialNum_Len = 8;

        private readonly int ManufactureDate_Len = 4;

        private readonly int EDIDVer_Len = 4;

        private readonly int VideoInputDef_Len = 2;

        private readonly int ScreenSize_Len = 4;

        private readonly int ModelName_Len = 13;

        private string hexString = "00FFFFFFFFFFFF0010ACDC425538323016210103803C2278EA62A5AD5046AB240E5054A54B00714F8180A940D1C081C0A9C001010101565E00A0A0A029503020350055502100001A000000FF00434E3037334B300A2020202020000000FC0044454C4C20553237323444450A000000FD0030781EB23C000A20202020202001ED";

        [Test]
        public void TestPush()
        {
            EdidParser edidparser = new EdidParser();
            byte[] blocks = { 0x01, 0x02, 0x03 };
            edidparser.Push(blocks);
            string expectedHexString = "010203";
            string actualHexString = edidparser.HexString;
            Assert.That(expectedHexString, Is.EqualTo(actualHexString));
        }

        [Test]
        public void TestCheckIsBlock0()
        {
            EdidParser edidparser = new EdidParser();
            byte[] blocks = new byte[] { 0x04, 0x05, 0x06 };
            edidparser.Push(blocks);
            string expectedHexString = "040506";
            string actualHexString = edidparser.HexString;
            Assert.That(expectedHexString, Is.EqualTo(actualHexString));//设置 HexString 不包含 EDID_Header
            Assert.IsFalse(edidparser.CheckIsBlock0());
        }

        [Test]
        public void TestGetManufacturerID()
        {
            EdidParser edidparser = new EdidParser();
            PrivateObject privateObject = new PrivateObject(edidparser);
            //var hexString = privateObject.GetField("HexString");
            privateObject.SetFieldOrProperty("HexString", hexString);
            var result = edidparser.GetManufacturerID();
            string actualManufacturerID = result;
            string ManufactureID = string.Empty;
            string result2 = "";
            string expectedManufacturerID = "DEL";
            if (hexString.Length < (EDID_Header.Length + Manufacturer_ID_Len))
            {
                Assert.That(result2, Is.EqualTo(actualManufacturerID));
            }
            string EDID = hexString.Substring(EDID_Header.Length, Manufacturer_ID_Len);
            if (EDID == "" || EDID.Length < 4)
            {
                Assert.That(result2, Is.EqualTo(actualManufacturerID));
            }

            Assert.That(expectedManufacturerID, Is.EqualTo(actualManufacturerID));
        }

        [Test]
        public void TestGetVendorID()
        {
            EdidParser edidparser = new EdidParser();
            PrivateObject privateObject = new PrivateObject(edidparser);
            privateObject.SetFieldOrProperty("HexString", hexString);
            var result = edidparser.GetVendorID();
            string expectedVendorID = "42DC";
            string actualVendorID = result;
            if (hexString.Length >= (EDID_Header.Length + Manufacturer_ID_Len + VENDOR_ID_Len + 4))
            {
                Assert.That(expectedVendorID, Is.EqualTo(actualVendorID));
            }
            Assert.That(expectedVendorID, Is.EqualTo(actualVendorID));
        }

        [Test]
        public void TestGetSerialNum()
        {
            EdidParser edidparser = new EdidParser();
            PrivateObject privateObject = new PrivateObject(edidparser);
            privateObject.SetFieldOrProperty("HexString", hexString);
            var result = edidparser.GetSerialNum();
            string expectedSerialNum = "808597589";
            string actualSerialNum = result;
            string result2 = "";
            if (hexString.Length < (EDID_Header.Length + Manufacturer_ID_Len + VENDOR_ID_Len + SerialNum_Len))
            {
                Assert.That(result2, Is.EqualTo(actualSerialNum));
            }
            string text = hexString.Substring(EDID_Header.Length + Manufacturer_ID_Len + VENDOR_ID_Len, SerialNum_Len);

            if (text.Length < 8)
            {
                Assert.That(result2, Is.EqualTo(actualSerialNum));
            }
            Assert.That(expectedSerialNum, Is.EqualTo(actualSerialNum));
        }

        [Test]
        public void TestGetServiceTag()
        {
            EdidParser edidparser = new EdidParser();
            PrivateObject privateObject = new PrivateObject(edidparser);
            privateObject.SetFieldOrProperty("HexString", hexString);
            var result = edidparser.GetServiceTag();
            string expectedServiceTag = "CN073K0";
            string actualServiceTag = result;
            int num = hexString.IndexOf(EDID_SerivceTag_Header);
            string result2 = "";
            if (num < 0 || hexString.Length < (num + EDID_SerivceTag_Header.Length + 26))
            {
                Assert.That(result2, Is.EqualTo(actualServiceTag));
            }
            string text = hexString.Substring(num + EDID_SerivceTag_Header.Length, 26);
            if (text.Length < 26)
            {
                Assert.That(result2, Is.EqualTo(actualServiceTag));
            }
            Assert.That(expectedServiceTag, Is.EqualTo(actualServiceTag));
        }

        [Test]
        public void TestGetManufactureYearAndMonth()
        {
            EdidParser edidparser = new EdidParser();
            PrivateObject privateObject = new PrivateObject(edidparser);
            int Month_ = 10;
            int week_ = 20;
            var edid = new EDID() { Month = Month_, Week = week_, };
            privateObject.SetFieldOrProperty("HexString", hexString);
            var result = edidparser.GetManufactureYearAndMonth(ref edid);
            int expectedManufactureYear = 2023;
            int actualManufactureYear = result;
            int expectedManufactureMonth = 5;
            int expectedManufactureWeek = 22;
            int A = 0;
            int B = -1;
            if (hexString.Length < (EDID_Header.Length + Manufacturer_ID_Len + VENDOR_ID_Len + SerialNum_Len + ManufactureDate_Len + 4))
            {
                Assert.That(A, Is.EqualTo(result));
            }

            try
            {
                Assert.That(expectedManufactureYear, Is.EqualTo(actualManufactureYear));
            }
            catch (Exception)
            {
                Assert.That(B, Is.EqualTo(result));
            }
            Assert.That(expectedManufactureYear, Is.EqualTo(actualManufactureYear));
            Assert.That(expectedManufactureMonth, Is.EqualTo(Month_));
            Assert.That(expectedManufactureWeek, Is.EqualTo(week_));
        }

        [Test]
        public void TestGetModelName()
        {
            EdidParser edidparser = new EdidParser();
            PrivateObject privateObject = new PrivateObject(edidparser);
            privateObject.SetFieldOrProperty("HexString", hexString);
            var result = edidparser.GetModelName();
            string expectedModelName = "U2724DE";
            string actualModelName = result;
            string result2 = "";
            int num = hexString.IndexOf(ModelName_Header);
            if (num != -1)
            {
                Assert.That(expectedModelName, Is.EqualTo(actualModelName));
            }
            else
            {
                Assert.That(result2, Is.EqualTo(actualModelName));
            }
        }

        [Test]
        public void TestGetProductCode()
        {
            EdidParser edidparser = new EdidParser();
            string edid = "00FFFFFFFFFFFF0010ACDC425538323016210103803C2278EA62A5AD5046AB240E5054A54B00714F8180A940D1C081C0A9C001010101565E00A0A0A029503020350055502100001A000000FF00434E3037334B300A2020202020000000FC0044454C4C20553237323444450A000000FD0030781EB23C000A20202020202001ED";
            var result = edidparser.GetProductCode(edid);
            string expectedProductCode = "42DC";
            string actualProductCode = result;
            string result2 = "";
            if (edid != null && edid.Length > 24)
            {
                Assert.That(expectedProductCode, Is.EqualTo(actualProductCode));
            }
            else
            {
                Assert.That(result2, Is.EqualTo(actualProductCode));
            }
        }

        [Test]
        public void TestGetScreenSize()
        {
            EdidParser edidparser = new EdidParser();
            PrivateObject privateObject = new PrivateObject(edidparser);
            privateObject.SetFieldOrProperty("HexString", hexString);
            var result = edidparser.GetScreenSize();
            float expectedScreenSize = 27.1510868f;
            float actualScreenSize = result;
            int result2 = 0;
            if (hexString.Length < EDID_Header.Length + Manufacturer_ID_Len + VENDOR_ID_Len + SerialNum_Len + ManufactureDate_Len + EDIDVer_Len + VideoInputDef_Len + ScreenSize_Len + 4)
            {
                Assert.That(result2, Is.EqualTo(actualScreenSize));
            }
            else
            {
                Assert.That(expectedScreenSize, Is.EqualTo(actualScreenSize));
            }
        }

        [Test]
        public void TestGetExtensionFlag()
        {
            EdidParser edidparser = new EdidParser();
            PrivateObject privateObject = new PrivateObject(edidparser);
            privateObject.SetFieldOrProperty("HexString", hexString);
            var result = edidparser.GetExtensionFlag();
            int expectedExtensionFlag = 1;
            int actualExtensionFlag = result;

            int result2 = 0;
            try
            {
                Assert.That(expectedExtensionFlag, Is.EqualTo(actualExtensionFlag));
            }
            catch
            {
                Assert.That(result2, Is.EqualTo(actualExtensionFlag));
            }
        }

        [Test]
        public void Testint2charByASCII()
        {
            EdidParser edidparser = new EdidParser();
            PrivateObject privateObject = new PrivateObject(edidparser);
            var result = privateObject.Invoke("int2charByASCII", 1);
            Assert.IsNotNull(result);
            string expected = "A";
            Assert.That(expected, Is.EqualTo(result));
        }
    }
}