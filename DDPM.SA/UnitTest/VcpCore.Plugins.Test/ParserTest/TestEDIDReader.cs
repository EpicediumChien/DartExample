using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VcpCore.Common;
using static VcpCore.Plugins.EDIDReader;

namespace VcpCore.Plugins.Test.ParserTest
{
    public class TestEDIDReader
    {
        [Test]
        public void TestMaximumCommonDivisor()
        {
            int num1 = 10;
            int num2 = 20;
            int num3 = 10;
            var result = EDIDReader.MaximumCommonDivisor(num1, num2);  // 最大公约数
            Assert.That(result, Is.EqualTo(num3));

            int num4 = 5;
            int num5 = 5;
            int expected = 5;
            int actual = EDIDReader.MaximumCommonDivisor(num4, num5);
            Assert.That(expected, Is.EqualTo(actual));

        }

        [Test]
        public void TestRatio()
        {
            int num1 = 10;
            int num2 = 20;
            string ratio = "1:2";
            var result = EDIDReader.Ratio(num1, num2); //计算比例
            Assert.IsNotNull(result);
            Assert.That(ratio, Is.EqualTo(result));
        }

        [Test]
        public void TestToCharByASCIIShort()
        {
            int a = 2;
            char ch1 = 'B';
            char result = EDIDReader.ToCharByASCIIShort(a); //66 B
            Assert.That(ch1, Is.EqualTo(result));
        }


        [Test]
        public void TestContains()
        {
            byte a = 0X10;
            byte b = 0X26;
            byte c = 0x3E;
            var result = EDIDReader.Contains(a, b);
            Assert.IsFalse(result);

            var result2 = EDIDReader.Contains(c, b);
            Assert.IsTrue(result2);
        }

        [Test]
        public void TestToCinch_By_ABcm()
        {
            int a = 3;
            int b = 4;
            double Cinch = 1.9685039370078741;
            var result = EDIDReader.ToCinch_By_ABcm(a, b);
            Assert.That(Cinch, Is.EqualTo(result));
        }
    }

    public class TestVendor_Product_Identification
    {
        [Test]
        public void TestMonitor_Name()
        {
            byte[] validEdid = new byte[] { 0x00, 0x01, 0x02 };
            string monitorname = "";
            Vendor_Product_Identification vendor_Product_Identification = new Vendor_Product_Identification();
            var result=Vendor_Product_Identification.Monitor_Name(validEdid);
            Assert.That(monitorname, Is.EqualTo(result));
        }

        [Test]
        public void TestMonitor_Serial_Number()
        {
            byte[] validEdid = new byte[] { 0x03, 0x04, 0x05 };
            string monitor_Serial_Number = "";
            Vendor_Product_Identification vendor_Product_Identification = new Vendor_Product_Identification();
            var result = Vendor_Product_Identification.Monitor_Serial_Number(validEdid);
            Assert.That(monitor_Serial_Number, Is.EqualTo(result));
        }

        [Test]
        public void TestManufacturer_Name()
        {
            byte[] InvalidEdid = new byte[] { 0x03, 0x04, 0x05 };
            string Manufacturer_Name1 = "";

            byte[] validEdid = new byte[128];
            validEdid[8] = 0x10;
            validEdid[9] = 0xAC;
            string Manufacturer_Name2 = "DEL";

            if (InvalidEdid == null || InvalidEdid.Length < 128)
            {
                var result = Vendor_Product_Identification.Manufacturer_Name(InvalidEdid);
                Assert.That(Manufacturer_Name1, Is.EqualTo(result));
            }

            if (validEdid != null || validEdid.Length >= 128)
            {
                var result = Vendor_Product_Identification.Manufacturer_Name(validEdid); //0X10AC=DEL
                Assert.That(Manufacturer_Name2, Is.EqualTo(result));
            }
        }

        [Test]
        public void TestManufacturer_Name_()
        {

            byte byte8 = 0x10;
            byte byte9 = 0xAC;
            string Manufacturer_Name2 = "DEL";

            var result = Vendor_Product_Identification.Manufacturer_Name(byte8, byte9);
            Assert.That(Manufacturer_Name2, Is.EqualTo(result));

        }
    }
}