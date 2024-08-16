using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using VcpCore.Common;
using static VcpCore.Plugins.EDIDReader;
using static VcpCore.Plugins.EDIDReader.Monitor_Range_Limit;

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
        public void TestInformation()
        {
            byte[] validEdid = new byte[128];
            validEdid[71] = 0x91;

            validEdid[21] = 0x60; //960mm
            validEdid[22] = 0x90; //1440mm
            string Max_Display_Size_CH2 = "68.1(寸)";

            validEdid[8] = 0x10;
            validEdid[9] = 0xAC;
            string Manufacturer_Name2 = "DEL";

            validEdid[17] = 0x21;
            string Year_Of_Manufacture2 = "2023";

            validEdid[16] = 0x16;
            string Week_Of_Manufacture2 = "22";

            validEdid[56] = 10;
            validEdid[58] = 20;
            validEdid[59] = 30;
            validEdid[61] = 40;
            string Active_Ratio2 = "133:271";

            validEdid[21] = 0x60; //960mm
            string Max_Horizontal_Image_Size2 = "960 mm";

            validEdid[22] = 0x90; //1440mm
            string Max_Vertical_Image_Size2 = "1440 mm";

            string Information1 = "2023年22周; DEL; 68.1(寸)(960 mm,1440 mm); 133:271;\r\n";
            var result = EDIDReader.Information(validEdid);
            Assert.That(Information1, Is.EqualTo(result));

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
            var result = Vendor_Product_Identification.Monitor_Name(validEdid);
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

        [Test]
        public void TestProduct_Id()
        {
            byte[] InvalidEdid = new byte[] { 0x03, 0x04, 0x05 };
            string Product_Id1 = "";

            byte[] validEdid = new byte[128];
            validEdid[10] = 0xDC;
            validEdid[11] = 0x42;
            string Product_Id2 = "42DC";

            if (InvalidEdid == null || InvalidEdid.Length < 128)
            {
                var result = Vendor_Product_Identification.Product_Id(InvalidEdid);
                Assert.That(Product_Id1, Is.EqualTo(result));
            }

            if (validEdid != null || validEdid.Length >= 128)
            {
                var result = Vendor_Product_Identification.Product_Id(validEdid);
                Assert.That(Product_Id2, Is.EqualTo(result));
            }
        }

        [Test]
        public void TestSerial_Number()
        {
            byte[] InvalidEdid = new byte[] { 0x03, 0x04, 0x05 };
            string Serial_Number1 = "";

            byte[] validEdid = new byte[128];
            validEdid[12] = 0x58;
            validEdid[13] = 0x97;
            validEdid[14] = 0x85;
            validEdid[15] = 0x80;
            string Serial_Number2 = "80859758";

            if (InvalidEdid == null || InvalidEdid.Length < 128)
            {
                var result = Vendor_Product_Identification.Serial_Number(InvalidEdid);
                Assert.That(Serial_Number1, Is.EqualTo(result));
            }

            if (validEdid != null || validEdid.Length >= 128)
            {
                var result = Vendor_Product_Identification.Serial_Number(validEdid);
                Assert.That(Serial_Number2, Is.EqualTo(result));
            }
        }

        [Test]
        public void TestWeek_Of_Manufacture()
        {
            byte[] InvalidEdid = new byte[] { 0x03, 0x04, 0x05 };
            string Week_Of_Manufacture1 = "";

            byte[] validEdid = new byte[128];
            validEdid[16] = 0x16;

            string Week_Of_Manufacture2 = "22";

            if (InvalidEdid == null || InvalidEdid.Length < 128)
            {
                var result = Vendor_Product_Identification.Week_Of_Manufacture(InvalidEdid);
                Assert.That(Week_Of_Manufacture1, Is.EqualTo(result));
            }

            if (validEdid != null || validEdid.Length >= 128)
            {
                var result = Vendor_Product_Identification.Week_Of_Manufacture(validEdid);  //0x16=22
                Assert.That(Week_Of_Manufacture2, Is.EqualTo(result));
            }
        }

        [Test]
        public void TestYear_Of_Manufacture()
        {
            byte[] InvalidEdid = new byte[] { 0x03, 0x04, 0x05 };
            string Year_Of_Manufacture1 = "";

            byte[] validEdid = new byte[128];
            validEdid[17] = 0x21;

            string Year_Of_Manufacture2 = "2023";

            if (InvalidEdid == null || InvalidEdid.Length < 128)
            {
                var result = Vendor_Product_Identification.Year_Of_Manufacture(InvalidEdid);
                Assert.That(Year_Of_Manufacture1, Is.EqualTo(result));
            }

            if (validEdid != null || validEdid.Length >= 128)
            {
                var result = Vendor_Product_Identification.Year_Of_Manufacture(validEdid);  //0x21=33,33+1990=2023
                Assert.That(Year_Of_Manufacture2, Is.EqualTo(result));
            }
        }

        [Test]
        public void TestEDIDVersion()
        {
            byte[] InvalidEdid = new byte[] { 0x03, 0x04, 0x05 };
            string EDIDVersion1 = "";

            byte[] validEdid = new byte[128];
            validEdid[18] = 1;
            validEdid[19] = 3;
            string EDIDVersion2 = "V1.3";

            if (InvalidEdid == null || InvalidEdid.Length < 128)
            {
                var result = Vendor_Product_Identification.EDIDVersion(InvalidEdid);
                Assert.That(EDIDVersion1, Is.EqualTo(result));
            }

            if (validEdid != null || validEdid.Length >= 128)
            {
                var result = Vendor_Product_Identification.EDIDVersion(validEdid);  //V1.3
                Assert.That(EDIDVersion2, Is.EqualTo(result));
            }
        }

        [Test]
        public void TestNumber_Of_Extension_Flag()
        {
            byte[] InvalidEdid = new byte[] { 0x03, 0x04, 0x05 };
            string Number_Of_Extension_Flag1 = "0";

            byte[] validEdid = new byte[256];

            string Number_Of_Extension_Flag2 = "1";

            if (InvalidEdid == null || InvalidEdid.Length < 256)
            {
                var result = Vendor_Product_Identification.Number_Of_Extension_Flag(InvalidEdid);
                Assert.That(Number_Of_Extension_Flag1, Is.EqualTo(result));
            }

            if (validEdid != null || validEdid.Length >= 128)
            {
                var result = Vendor_Product_Identification.Number_Of_Extension_Flag(validEdid);
                Assert.That(Number_Of_Extension_Flag2, Is.EqualTo(result));
            }
        }
    }

    public class TestDisplay_Parameters
    {
        [Test]
        public void TestVideo_Input_Definition()
        {
            byte[] InvalidEdid = new byte[] { 0x01, 0x04, 0x05 };
            string Video_Input_Definition1 = "";

            byte[] validEdid = new byte[128];
            validEdid[20] = 0x80;
            string Video_Input_Definition2 = "Digital Signal";
            string Video_Input_Definition3 = "Analog Signal";
            if (InvalidEdid == null || InvalidEdid.Length < 128)
            {
                var result = Display_Parameters.Video_Input_Definition(InvalidEdid);
                Assert.That(Video_Input_Definition1, Is.EqualTo(result));
            }

            if (validEdid != null || validEdid.Length >= 128)
            {
                if ((validEdid[20] & 0x80) == 0x80)
                {
                    var result = Display_Parameters.Video_Input_Definition(validEdid);
                    Assert.That(Video_Input_Definition2, Is.EqualTo(result));
                }
                else
                {
                    var result = Display_Parameters.Video_Input_Definition(validEdid);
                    Assert.That(Video_Input_Definition3, Is.EqualTo(result));
                }
            }
        }

        [Test]
        public void TestDFP1X_Compatible_Interface()
        {
            byte[] InvalidEdid = new byte[] { 0x01, 0x04, 0x05 };
            string DFP1X_Compatible_Interface1 = "";

            byte[] validEdid = new byte[128];
            validEdid[20] = 0x81;
            string DFP1X_Compatible_Interface2 = "True";
            string DFP1X_Compatible_Interface3 = "False";
            string DFP1X_Compatible_Interface4 = "Invalid";
            if (InvalidEdid == null || InvalidEdid.Length < 128)
            {
                var result = Display_Parameters.DFP1X_Compatible_Interface(InvalidEdid);
                Assert.That(DFP1X_Compatible_Interface1, Is.EqualTo(result));
            }

            if (validEdid != null || validEdid.Length >= 128)
            {
                if ((validEdid[20] & 0x80) == 0x80)
                {
                    if ((validEdid[20] & 0x01) == 0x01)
                    {
                        var result = Display_Parameters.DFP1X_Compatible_Interface(validEdid);
                        Assert.That(DFP1X_Compatible_Interface2, Is.EqualTo(result));
                    }
                    else
                    {
                        var result = Display_Parameters.DFP1X_Compatible_Interface(validEdid);
                        Assert.That(DFP1X_Compatible_Interface3, Is.EqualTo(result));
                    }
                }
                else
                {
                    var result = Display_Parameters.DFP1X_Compatible_Interface(validEdid);
                    Assert.That(DFP1X_Compatible_Interface4, Is.EqualTo(result));
                }
            }
        }

        [Test]
        public void TestVideo_White_and_Sync_Levels()
        {
            byte[] InvalidEdid = new byte[] { 0x01, 0x04, 0x05 };
            string Video_White_and_Sync_Levels1 = "";

            byte[] validEdid = new byte[128];
            validEdid[20] = 0x81;

            string Video_White_and_Sync_Levels2 = "Invalid";
            string Video_White_and_Sync_Levels3 = "+0.7/0 V";
            string Video_White_and_Sync_Levels4 = "+1.0/−0.4 V";
            string Video_White_and_Sync_Levels5 = "+0.714/−0.286 V";
            string Video_White_and_Sync_Levels6 = "+0.7/−0.3 V";

            if (InvalidEdid == null || InvalidEdid.Length < 128)
            {
                var result = Display_Parameters.Video_White_and_Sync_Levels(InvalidEdid);
                Assert.That(Video_White_and_Sync_Levels1, Is.EqualTo(result));
            }

            if (validEdid != null || validEdid.Length >= 128)
            {
                if ((validEdid[20] & 0x80) == 0x80)
                {
                    var result = Display_Parameters.Video_White_and_Sync_Levels(validEdid);
                    Assert.That(Video_White_and_Sync_Levels2, Is.EqualTo(result));

                }
                else if ((validEdid[20] & 0x60) == 0x60)//11
                {
                    var result = Display_Parameters.Video_White_and_Sync_Levels(validEdid);
                    Assert.That(Video_White_and_Sync_Levels3, Is.EqualTo(result));
                }
                else if ((validEdid[20] & 0x40) == 0x40)//10
                {
                    var result = Display_Parameters.Video_White_and_Sync_Levels(validEdid);
                    Assert.That(Video_White_and_Sync_Levels4, Is.EqualTo(result));
                }
                else if ((validEdid[20] & 0x20) == 0x20)//01
                {
                    var result = Display_Parameters.Video_White_and_Sync_Levels(validEdid);
                    Assert.That(Video_White_and_Sync_Levels5, Is.EqualTo(result));
                }
                else
                {
                    var result = Display_Parameters.Video_White_and_Sync_Levels(validEdid);
                    Assert.That(Video_White_and_Sync_Levels6, Is.EqualTo(result));
                }
            }
        }

        [Test]
        public void TestBlank_To_Black_Setup()
        {
            byte[] InvalidEdid = new byte[] { 0x01, 0x04, 0x05 };
            string Blank_To_Black_Setup1 = "";

            byte[] validEdid = new byte[128];
            validEdid[20] = 0x80;

            byte[] validEdid2 = new byte[128];
            validEdid2[20] = 0x60;
            string Blank_To_Black_Setup2 = "True";
            string Blank_To_Black_Setup3 = "False";
            string Blank_To_Black_Setup4 = "Invalid";
            if (InvalidEdid == null || InvalidEdid.Length < 128)
            {
                var result = Display_Parameters.Blank_To_Black_Setup(InvalidEdid);
                Assert.That(Blank_To_Black_Setup1, Is.EqualTo(result));
            }

            if (validEdid != null || validEdid.Length >= 128)
            {
                if ((validEdid[20] & 0x80) == 0x80)
                {
                    var result = Display_Parameters.Blank_To_Black_Setup(validEdid);
                    Assert.That(Blank_To_Black_Setup4, Is.EqualTo(result));
                }
            }
            if (validEdid2 != null || validEdid2.Length >= 128)
            {
                if ((validEdid2[20] & 0x10) == 0x60)
                {
                    var result = Display_Parameters.Blank_To_Black_Setup(validEdid2);
                    Assert.That(Blank_To_Black_Setup2, Is.EqualTo(result));
                }
                else
                {
                    var result = Display_Parameters.Blank_To_Black_Setup(validEdid2);
                    Assert.That(Blank_To_Black_Setup3, Is.EqualTo(result));
                }
            }
        }

        [Test]
        public void TestSeparate_Sync()
        {
            byte[] InvalidEdid = new byte[] { 0x01, 0x04, 0x05 };
            string Separate_Sync1 = "";

            byte[] validEdid = new byte[128];
            validEdid[20] = 0x80;

            byte[] validEdid2 = new byte[128];
            validEdid2[20] = 0x08;
            string Separate_Sync2 = "True";
            string Separate_Sync3 = "False";
            string Separate_Sync4 = "Invalid";
            if (InvalidEdid == null || InvalidEdid.Length < 128)
            {
                var result = Display_Parameters.Separate_Sync(InvalidEdid);
                Assert.That(Separate_Sync1, Is.EqualTo(result));
            }

            if (validEdid != null || validEdid.Length >= 128)
            {
                if ((validEdid[20] & 0x80) == 0x80)
                {
                    var result = Display_Parameters.Separate_Sync(validEdid);
                    Assert.That(Separate_Sync4, Is.EqualTo(result));
                }
            }
            if (validEdid2 != null || validEdid2.Length >= 128)
            {
                if ((validEdid2[20] & 0x08) == 0x08)
                {
                    var result = Display_Parameters.Separate_Sync(validEdid2);
                    Assert.That(Separate_Sync2, Is.EqualTo(result));
                }
                else
                {
                    var result = Display_Parameters.Separate_Sync(validEdid2);
                    Assert.That(Separate_Sync3, Is.EqualTo(result));
                }
            }
        }

        [Test]
        public void TestHSync_Composite_Syncc()
        {
            byte[] InvalidEdid = new byte[] { 0x01, 0x04, 0x05 };
            string HSync_Composite_Sync1 = "";

            byte[] validEdid = new byte[128];
            validEdid[20] = 0x80;

            byte[] validEdid2 = new byte[128];
            validEdid2[20] = 0x04;
            string HSync_Composite_Sync2 = "True";
            string HSync_Composite_Sync3 = "False";
            string HSync_Composite_Sync4 = "Invalid";
            if (InvalidEdid == null || InvalidEdid.Length < 128)
            {
                var result = Display_Parameters.HSync_Composite_Sync(InvalidEdid);
                Assert.That(HSync_Composite_Sync1, Is.EqualTo(result));
            }

            if (validEdid.Length >= 128)
            {
                if ((validEdid[20] & 0x80) == 0x80)
                {
                    var result = Display_Parameters.HSync_Composite_Sync(validEdid);
                    Assert.That(HSync_Composite_Sync4, Is.EqualTo(result));
                }
            }
            if (validEdid2.Length >= 128)
            {
                if ((validEdid2[20] & 0x04) == 0x04)
                {
                    var result = Display_Parameters.HSync_Composite_Sync(validEdid2);
                    Assert.That(HSync_Composite_Sync2, Is.EqualTo(result));
                }
                else
                {
                    var result = Display_Parameters.HSync_Composite_Sync(validEdid2);
                    Assert.That(HSync_Composite_Sync3, Is.EqualTo(result));
                }
            }
        }

        [Test]
        public void TestSOG()
        {
            byte[] InvalidEdid = new byte[] { 0x01, 0x04, 0x05 };
            string SOG1 = "";

            byte[] validEdid = new byte[128];
            validEdid[20] = 0x80;

            byte[] validEdid2 = new byte[128];
            validEdid2[20] = 0x02;
            string SOG2 = "True";
            string SOG3 = "False";
            string SOG4 = "Invalid";
            if (InvalidEdid == null || InvalidEdid.Length < 128)
            {
                var result = Display_Parameters.SOG(InvalidEdid);
                Assert.That(SOG1, Is.EqualTo(result));
            }

            if (validEdid.Length >= 128)
            {
                if ((validEdid[20] & 0x80) == 0x80)
                {
                    var result = Display_Parameters.SOG(validEdid);
                    Assert.That(SOG4, Is.EqualTo(result));
                }
            }
            if (validEdid2.Length >= 128)
            {
                if ((validEdid2[20] & 0x02) == 0x02)
                {
                    var result = Display_Parameters.SOG(validEdid2);
                    Assert.That(SOG2, Is.EqualTo(result));
                }
                else
                {
                    var result = Display_Parameters.SOG(validEdid2);
                    Assert.That(SOG3, Is.EqualTo(result));
                }
            }
        }


        [Test]
        public void TestVSync_Pulse_Must_Be_Serrated()
        {
            byte[] InvalidEdid = new byte[] { 0x01, 0x04, 0x05 };
            string VSync_Pulse_Must_Be_Serrated1 = "";

            byte[] validEdid = new byte[128];
            validEdid[20] = 0x80;

            byte[] validEdid2 = new byte[128];
            validEdid2[20] = 0x03;
            string VSync_Pulse_Must_Be_Serrated2 = "True";
            string VSync_Pulse_Must_Be_Serrated3 = "False";
            string VSync_Pulse_Must_Be_Serrated4 = "Invalid";
            string VSync_Pulse_Must_Be_Serrated5 = "Invalid. VSync pulse must be serrated when composite or sync-on-green is used.";
            if (InvalidEdid == null || InvalidEdid.Length < 128)
            {
                var result = Display_Parameters.VSync_Pulse_Must_Be_Serrated(InvalidEdid);
                Assert.That(VSync_Pulse_Must_Be_Serrated1, Is.EqualTo(result));
            }

            if (validEdid.Length >= 128)
            {
                if ((validEdid[20] & 0x80) == 0x80)
                {
                    var result = Display_Parameters.VSync_Pulse_Must_Be_Serrated(validEdid);
                    Assert.That(VSync_Pulse_Must_Be_Serrated4, Is.EqualTo(result));
                }
            }
            if (validEdid2.Length >= 128)
            {
                if (((validEdid2[20] & 0x02) == 0x02) || ((validEdid2[20] & 0x04) == 0x04))
                {
                    if ((validEdid2[20] & 0x01) == 0x01)
                    {
                        var result = Display_Parameters.VSync_Pulse_Must_Be_Serrated(validEdid2);
                        Assert.That(VSync_Pulse_Must_Be_Serrated2, Is.EqualTo(result));
                    }
                    else
                    {
                        var result = Display_Parameters.VSync_Pulse_Must_Be_Serrated(validEdid2);
                        Assert.That(VSync_Pulse_Must_Be_Serrated3, Is.EqualTo(result));
                    }
                }
                else
                {
                    var result = Display_Parameters.VSync_Pulse_Must_Be_Serrated(validEdid2);
                    Assert.That(VSync_Pulse_Must_Be_Serrated5, Is.EqualTo(result));
                }
            }
        }

        [Test]
        public void TestMax_Horizontal_Image_Size()
        {
            byte[] InvalidEdid = new byte[] { 0x01, 0x04, 0x05 };
            string Max_Horizontal_Image_Size1 = "";

            byte[] validEdid = new byte[128];
            validEdid[21] = 0x20;
            string Max_Horizontal_Image_Size2 = "320 mm";

            if (InvalidEdid == null || InvalidEdid.Length < 128)
            {
                var result = Display_Parameters.Max_Horizontal_Image_Size(InvalidEdid);
                Assert.That(Max_Horizontal_Image_Size1, Is.EqualTo(result));
            }

            if (validEdid.Length >= 128)
            {
                var result = Display_Parameters.Max_Horizontal_Image_Size(validEdid);
                Assert.That(Max_Horizontal_Image_Size2, Is.EqualTo(result));
            }
        }

        [Test]
        public void TestMax_Vertical_Image_Size()
        {
            byte[] InvalidEdid = new byte[] { 0x01, 0x04, 0x05 };
            string Max_Vertical_Image_Size1 = "";

            byte[] validEdid = new byte[128];
            validEdid[22] = 0x60;
            string Max_Vertical_Image_Size2 = "960 mm";

            if (InvalidEdid == null || InvalidEdid.Length < 128)
            {
                var result = Display_Parameters.Max_Vertical_Image_Size(InvalidEdid);
                Assert.That(Max_Vertical_Image_Size1, Is.EqualTo(result));
            }

            if (validEdid.Length >= 128)
            {
                var result = Display_Parameters.Max_Vertical_Image_Size(validEdid);
                Assert.That(Max_Vertical_Image_Size2, Is.EqualTo(result));
            }
        }

        [Test]
        public void TestImage_Size_Ratio()
        {
            byte[] InvalidEdid = new byte[] { 0x01, 0x04, 0x05 };
            string Image_Size_Ratio1 = "";

            byte[] validEdid = new byte[128];
            validEdid[21] = 0x60; //960mm
            validEdid[22] = 0x90; //1440mm
            string Image_Size_Ratio2 = "2:3";

            if (InvalidEdid == null || InvalidEdid.Length < 128)
            {
                var result = Display_Parameters.Image_Size_Ratio(InvalidEdid);
                Assert.That(Image_Size_Ratio1, Is.EqualTo(result));
            }

            if (validEdid.Length >= 128)
            {
                var result = Display_Parameters.Image_Size_Ratio(validEdid);
                Assert.That(Image_Size_Ratio2, Is.EqualTo(result));
            }
        }


        [Test]
        public void TestMax_Display_Size()
        {
            byte[] InvalidEdid = new byte[] { 0x01, 0x04, 0x05 };
            string Max_Display_Size1 = "";

            byte[] validEdid = new byte[128];
            validEdid[21] = 0x60; //960mm
            validEdid[22] = 0x90; //1440mm
            string Max_Display_Size2 = "68.1 inches";

            if (InvalidEdid == null || InvalidEdid.Length < 128)
            {
                var result = Display_Parameters.Max_Display_Size(InvalidEdid);
                Assert.That(Max_Display_Size1, Is.EqualTo(result));
            }

            if (validEdid.Length >= 128)
            {
                var result = Display_Parameters.Max_Display_Size(validEdid);
                Assert.That(Max_Display_Size2, Is.EqualTo(result));
            }
        }

        [Test]
        public void TestMax_Display_Size_CH()
        {
            byte[] InvalidEdid = new byte[] { 0x01, 0x04, 0x05 };
            string Max_Display_Size_CH1 = "";

            byte[] validEdid = new byte[128];
            validEdid[21] = 0x60; //960mm
            validEdid[22] = 0x90; //1440mm
            string Max_Display_Size_CH2 = "68.1(寸)";

            if (InvalidEdid == null || InvalidEdid.Length < 128)
            {
                var result = Display_Parameters.Max_Display_Size_CH(InvalidEdid);
                Assert.That(Max_Display_Size_CH1, Is.EqualTo(result));
            }

            if (validEdid.Length >= 128)
            {
                var result = Display_Parameters.Max_Display_Size_CH(validEdid);
                Assert.That(Max_Display_Size_CH2, Is.EqualTo(result));
            }
        }

    }

    public class TestPower_Management_and_Features
    {
        [Test]
        public void TestStandby()
        {
            byte[] validEdid = new byte[128];
            validEdid[24] = 0x81;//128

            byte[] InvalidEdid = new byte[128];
            InvalidEdid[24] = 0x41; //65

            string Standbysupport = "Supported";
            string StandbyNosupport = "Not Supported";
            Power_Management_and_Features power_Management_and_Features = new Power_Management_and_Features();

            if ((validEdid[24] & 0x80) == 0x80)
            {
                var result = Power_Management_and_Features.Standby(validEdid);
                Assert.That(Standbysupport, Is.EqualTo(result));
            }
            if ((InvalidEdid[24] & 0x80) != 0x80)
            {
                var result = Power_Management_and_Features.Standby(InvalidEdid);
                Assert.That(StandbyNosupport, Is.EqualTo(result));
            }

        }

        [Test]
        public void TestSuspend()
        {
            byte[] validEdid = new byte[128];
            validEdid[24] = 0x41;//65

            byte[] InvalidEdid = new byte[128];
            InvalidEdid[24] = 0x81; //129

            string Suspendsupport = "Supported";
            string SuspendNosupport = "Not Supported";
            Power_Management_and_Features power_Management_and_Features = new Power_Management_and_Features();

            if ((validEdid[24] & 0x40) == 0x40)
            {
                var result = Power_Management_and_Features.Suspend(validEdid);
                Assert.That(Suspendsupport, Is.EqualTo(result));
            }
            if ((InvalidEdid[24] & 0x40) != 0x40)
            {
                var result = Power_Management_and_Features.Suspend(InvalidEdid);
                Assert.That(SuspendNosupport, Is.EqualTo(result));
            }

        }

        [Test]
        public void TestActiveOff()
        {
            byte[] validEdid = new byte[128];
            validEdid[24] = 0x21;//33

            byte[] InvalidEdid = new byte[128];
            InvalidEdid[24] = 0x81; //129

            string ActiveOffsupport = "Supported";
            string ActiveOffNosupport = "Not Supported";
            Power_Management_and_Features power_Management_and_Features = new Power_Management_and_Features();

            if ((validEdid[24] & 0x20) == 0x20)
            {
                var result = Power_Management_and_Features.ActiveOff(validEdid);
                Assert.That(ActiveOffsupport, Is.EqualTo(result));
            }
            if ((InvalidEdid[24] & 0x20) != 0x20)
            {
                var result = Power_Management_and_Features.ActiveOff(InvalidEdid);
                Assert.That(ActiveOffNosupport, Is.EqualTo(result));
            }

        }

        [Test]
        public void TestVideo_Input_Display_Type()
        {
            byte[] validEdid = new byte[128];
            validEdid[24] = 0x18;//24

            byte[] InvalidEdid = new byte[] { 0x01, 0x04, 0x05 };
            string Video_Input_Display_Type1 = "";
            string Video_Input_Display_Type2 = "3";

            if (InvalidEdid == null || InvalidEdid.Length < 128)
            {
                var result = Power_Management_and_Features.Video_Input_Display_Type(InvalidEdid);
                Assert.That(Video_Input_Display_Type1, Is.EqualTo(result));
            }

            if (validEdid.Length >= 128)
            {
                var result = Power_Management_and_Features.Video_Input_Display_Type(validEdid);
                Assert.That(Video_Input_Display_Type2, Is.EqualTo(result));
            }
        }

        [Test]
        public void TestVideo_Input()
        {
            byte[] validEdid = new byte[128];
            validEdid[24] = 0x80;//128

            byte[] InvalidEdid = new byte[] { 0x01, 0x04, 0x05 };
            string Video_Input1 = "";
            string Video_Input2 = "1";

            if (InvalidEdid == null || InvalidEdid.Length < 128)
            {
                var result = Power_Management_and_Features.Video_Input(InvalidEdid);
                Assert.That(Video_Input1, Is.EqualTo(result));
            }

            if (validEdid.Length >= 128)
            {
                var result = Power_Management_and_Features.Video_Input(validEdid);
                Assert.That(Video_Input2, Is.EqualTo(result));
            }
        }


        [Test]
        public void TestsRGB_Default_ColorSpace()
        {
            byte[] validEdid = new byte[128];
            validEdid[24] = 0x05;//5

            byte[] InvalidEdid = new byte[128];
            InvalidEdid[24] = 0x09; //129

            string sRGB_Default_ColorSpace1 = "True";
            string sRGB_Default_ColorSpace2 = "False";
            Power_Management_and_Features power_Management_and_Features = new Power_Management_and_Features();

            if ((validEdid[24] & 0x04) == 0x04)
            {
                var result = Power_Management_and_Features.sRGB_Default_ColorSpace(validEdid);
                Assert.That(sRGB_Default_ColorSpace1, Is.EqualTo(result));
            }
            if ((InvalidEdid[24] & 0x40) != 0x40)
            {
                var result = Power_Management_and_Features.sRGB_Default_ColorSpace(InvalidEdid);
                Assert.That(sRGB_Default_ColorSpace2, Is.EqualTo(result));
            }

        }

        [Test]
        public void TestDefault_GTF()
        {
            byte[] validEdid = new byte[128];
            validEdid[24] = 0x02;//5

            byte[] InvalidEdid = new byte[128];
            InvalidEdid[24] = 0x04; //129

            string Default_GTF1 = "Supported";
            string Default_GTF2 = "Not Supported";

            if ((validEdid[24] & 0x02) == 0x02)
            {
                var result = Power_Management_and_Features.Default_GTF(validEdid);
                Assert.That(Default_GTF2, Is.EqualTo(result));
            }
            if ((InvalidEdid[24] & 0x02) != 0x02)
            {
                var result = Power_Management_and_Features.Default_GTF(InvalidEdid);
                Assert.That(Default_GTF1, Is.EqualTo(result));
            }

        }


        [Test]
        public void TestPrefered_Timing_Mode()
        {
            byte[] validEdid = new byte[128];
            validEdid[24] = 0x02;//5

            byte[] InvalidEdid = new byte[128];
            InvalidEdid[24] = 0x04; //129

            string Prefered_Timing_Mode1 = "True";
            string Prefered_Timing_Mode2 = "False";

            if ((validEdid[24] & 0x02) == 0x02)
            {
                var result = Power_Management_and_Features.Prefered_Timing_Mode(validEdid);
                Assert.That(Prefered_Timing_Mode1, Is.EqualTo(result));
            }
            if ((InvalidEdid[24] & 0x02) != 0x02)
            {
                var result = Power_Management_and_Features.Prefered_Timing_Mode(InvalidEdid);
                Assert.That(Prefered_Timing_Mode2, Is.EqualTo(result));
            }

        }

    }

    public class TestGamma_Color_and_Etablished_Timings
    {
        [Test]
        public void TestDisplay_Gamma()
        {
            byte[] validEdid = new byte[128];
            validEdid[23] = 0x64;//100

            byte[] InvalidEdid = new byte[] { 0x01, 0x04, 0x05 };
            string Display_Gamma1 = "";
            string Display_Gamma2 = "2.00";

            Gamma_Color_and_Etablished_Timings gamma_Color_And_Etablished_Timings = new Gamma_Color_and_Etablished_Timings();

            if (InvalidEdid == null || InvalidEdid.Length < 128)
            {
                var result = Gamma_Color_and_Etablished_Timings.Display_Gamma(InvalidEdid);
                Assert.That(Display_Gamma1, Is.EqualTo(result));
            }

            if (validEdid.Length >= 128)
            {
                var result = Gamma_Color_and_Etablished_Timings.Display_Gamma(validEdid);
                Assert.That(Display_Gamma2, Is.EqualTo(result));
            }
        }

        [Test]
        public void TestRed()
        {
            byte[] validEdid = new byte[128];
            validEdid[25] = 0x3F;
            validEdid[27] = 0xFF;
            validEdid[28] = 0xFF;

            byte[] InvalidEdid = new byte[] { 0x01, 0x04, 0x05 };
            string Red1 = "";
            string Red2 = "(x,y)(0.9961,0.9990)";

            Gamma_Color_and_Etablished_Timings gamma_Color_And_Etablished_Timings = new Gamma_Color_and_Etablished_Timings();

            if (InvalidEdid == null || InvalidEdid.Length < 128)
            {
                var result = Gamma_Color_and_Etablished_Timings.Red(InvalidEdid);
                Assert.That(Red1, Is.EqualTo(result));
            }

            if (validEdid.Length >= 128)
            {
                var result = Gamma_Color_and_Etablished_Timings.Red(validEdid);
                Assert.That(Red2, Is.EqualTo(result));
            }
        }

        [Test]
        public void TestGreen()
        {
            byte[] validEdid = new byte[128];
            validEdid[25] = 0x4F;
            validEdid[29] = 0xEF;
            validEdid[30] = 0xEF;

            byte[] InvalidEdid = new byte[] { 0x01, 0x04, 0x05 };
            string Green1 = "";
            string Green2 = "(x,y)(0.9365,0.9365)";

            Gamma_Color_and_Etablished_Timings gamma_Color_And_Etablished_Timings = new Gamma_Color_and_Etablished_Timings();

            if (InvalidEdid == null || InvalidEdid.Length < 128)
            {
                var result = Gamma_Color_and_Etablished_Timings.Green(InvalidEdid);
                Assert.That(Green1, Is.EqualTo(result));
            }

            if (validEdid.Length >= 128)
            {
                var result = Gamma_Color_and_Etablished_Timings.Green(validEdid);
                Assert.That(Green2, Is.EqualTo(result));
            }
        }

        [Test]
        public void TestBlue()
        {
            byte[] validEdid = new byte[128];
            validEdid[25] = 0x5F;
            validEdid[26] = 0xDF;
            validEdid[31] = 0xFF;
            validEdid[32] = 0xFF;

            byte[] InvalidEdid = new byte[] { 0x01, 0x04, 0x05 };
            string Blue1 = "";
            string Blue2 = "(x,y)(0.9971,0.9971)";

            Gamma_Color_and_Etablished_Timings gamma_Color_And_Etablished_Timings = new Gamma_Color_and_Etablished_Timings();

            if (InvalidEdid == null || InvalidEdid.Length < 128)
            {
                var result = Gamma_Color_and_Etablished_Timings.Blue(InvalidEdid);
                Assert.That(Blue1, Is.EqualTo(result));
            }

            if (validEdid.Length >= 128)
            {
                var result = Gamma_Color_and_Etablished_Timings.Blue(validEdid);
                Assert.That(Blue2, Is.EqualTo(result));
            }
        }

        [Test]
        public void TestWhite()
        {
            byte[] validEdid = new byte[128];
            validEdid[25] = 0x6F;
            validEdid[26] = 0xCF;
            validEdid[33] = 0xEF;
            validEdid[34] = 0xEF;

            byte[] InvalidEdid = new byte[] { 0x01, 0x04, 0x05 };
            string White1 = "";
            string White2 = "(x,y)(0.9365,0.9365)";

            Gamma_Color_and_Etablished_Timings gamma_Color_And_Etablished_Timings = new Gamma_Color_and_Etablished_Timings();

            if (InvalidEdid == null || InvalidEdid.Length < 128)
            {
                var result = Gamma_Color_and_Etablished_Timings.White(InvalidEdid);
                Assert.That(White1, Is.EqualTo(result));
            }

            if (validEdid.Length >= 128)
            {
                var result = Gamma_Color_and_Etablished_Timings.White(validEdid);
                Assert.That(White2, Is.EqualTo(result));
            }
        }

        [Test]
        public void TestEtablished_Timings()
        {
            byte[] validEdid = new byte[128];
            validEdid[35] = 0x80;
            validEdid[36] = 0x40;
            validEdid[37] = 0x20;

            byte[] InvalidEdid = new byte[] { 0x01, 0x04, 0x05 };
            string Etablished_Timings1 = "";
            string Etablished_Timings2 = "\r\n\t\t720×400 @ 70 Hz\r\n\t\t800×600 @ 75 Hz\r\n\t\tOther manufacturer-specific display modes 2 | 0010 0000\r\n";

            Gamma_Color_and_Etablished_Timings gamma_Color_And_Etablished_Timings = new Gamma_Color_and_Etablished_Timings();

            if (InvalidEdid == null || InvalidEdid.Length < 128)
            {
                var result = Gamma_Color_and_Etablished_Timings.Etablished_Timings(InvalidEdid);
                Assert.That(Etablished_Timings1, Is.EqualTo(result));
            }

            if (validEdid.Length >= 128)
            {
                var result = Gamma_Color_and_Etablished_Timings.Etablished_Timings(validEdid);
                Assert.That(Etablished_Timings2, Is.EqualTo(result));
            }
        }

        [Test]
        public void TestDisplay_Type()
        {
            byte[] InvalidEdid = new byte[] { 0x01, 0x04, 0x05 };

            byte[] validEdid = new byte[128];
            validEdid[20] = 0x80;
            validEdid[24] = 0x18;

            byte[] validEdid2 = new byte[128];
            validEdid2[20] = 0x80;
            validEdid2[24] = 0x10;

            byte[] validEdid3 = new byte[128];
            validEdid3[20] = 0x80;
            validEdid3[24] = 0x08;

            byte[] validEdid4 = new byte[128];
            validEdid4[20] = 0x80;
            validEdid4[24] = 0x00;

            byte[] validEdid5 = new byte[128];
            validEdid5[20] = 0x41;
            validEdid5[24] = 0x18;

            byte[] validEdid6 = new byte[128];
            validEdid6[20] = 0x41;
            validEdid6[24] = 0x10;

            byte[] validEdid7 = new byte[128];
            validEdid7[20] = 0x41;
            validEdid7[24] = 0x08;

            byte[] validEdid8 = new byte[128];
            validEdid8[20] = 0x41;
            validEdid8[24] = 0x00;

            string Display_Type1 = "";
            string Display_Type2 = "(Digital) RGB 4:4:4 + YCrCb 4:4:4 + YCrCb 4:2:2";
            string Display_Type3 = "(Digital) RGB 4:4:4 + YCrCb 4:2:2";
            string Display_Type4 = "(Digital) RGB 4:4:4 + YCrCb 4:4:4";
            string Display_Type5 = "(Digital) RGB 4:4:4";
            string Display_Type6 = "(Analog) Undefined";
            string Display_Type7 = "(Analog) Non-RGB color";
            string Display_Type8 = "(Analog) RGB color";
            string Display_Type9 = "(Analog) Monochrome or Grayscale";
            Gamma_Color_and_Etablished_Timings gamma_Color_And_Etablished_Timings = new Gamma_Color_and_Etablished_Timings();

            if (InvalidEdid == null || InvalidEdid.Length < 128)
            {
                var result = Gamma_Color_and_Etablished_Timings.Display_Type(InvalidEdid);
                Assert.That(Display_Type1, Is.EqualTo(result));
            }

            if (validEdid.Length >= 128)
            {
                if ((validEdid[20] & 0x80) == 0x80)
                {
                    if ((validEdid[24] & 0x18) == 0x18)
                    {
                        var result = Gamma_Color_and_Etablished_Timings.Display_Type(validEdid);
                        Assert.That(Display_Type2, Is.EqualTo(result));
                    }
                    if ((validEdid2[24] & 0x10) == 0x10)
                    {
                        var result = Gamma_Color_and_Etablished_Timings.Display_Type(validEdid2);
                        Assert.That(Display_Type3, Is.EqualTo(result));
                    }

                    if ((validEdid3[24] & 0x08) == 0x08)
                    {
                        var result = Gamma_Color_and_Etablished_Timings.Display_Type(validEdid3);
                        Assert.That(Display_Type4, Is.EqualTo(result));
                    }

                    if ((validEdid4[24] & 0x08) != 0x08)
                    {
                        var result = Gamma_Color_and_Etablished_Timings.Display_Type(validEdid4);
                        Assert.That(Display_Type5, Is.EqualTo(result));
                    }
                }

                if ((validEdid5[20] & 0x80) != 0x80)
                {
                    if ((validEdid5[24] & 0x18) == 0x18)
                    {
                        var result = Gamma_Color_and_Etablished_Timings.Display_Type(validEdid5);
                        Assert.That(Display_Type6, Is.EqualTo(result));
                    }

                    if ((validEdid6[24] & 0x10) == 0x10)
                    {
                        var result = Gamma_Color_and_Etablished_Timings.Display_Type(validEdid6);
                        Assert.That(Display_Type7, Is.EqualTo(result));
                    }
                    if ((validEdid7[24] & 0x08) == 0x08)
                    {
                        var result = Gamma_Color_and_Etablished_Timings.Display_Type(validEdid7);
                        Assert.That(Display_Type8, Is.EqualTo(result));
                    }
                    if ((validEdid8[24] & 0x08) != 0x08)
                    {
                        var result = Gamma_Color_and_Etablished_Timings.Display_Type(validEdid8);
                        Assert.That(Display_Type9, Is.EqualTo(result));
                    }
                }
            }
        }



        [Test]
        public void TestDisplay_Type_()
        {
            byte[] InvalidEdid = new byte[] { 0x01, 0x04, 0x05 };

            byte videoInputType = 0x80;
            byte videoInputType2 = 0x41;

            byte[] validEdid = new byte[128];
            validEdid[24] = 0x18;

            byte[] validEdid2 = new byte[128];
            validEdid2[24] = 0x10;

            byte[] validEdid3 = new byte[128];
            validEdid3[24] = 0x08;

            byte[] validEdid4 = new byte[128];
            validEdid4[24] = 0x00;

            byte[] validEdid5 = new byte[128];
            validEdid5[24] = 0x18;

            byte[] validEdid6 = new byte[128];
            validEdid6[24] = 0x10;

            byte[] validEdid7 = new byte[128];
            validEdid7[24] = 0x08;

            byte[] validEdid8 = new byte[128];
            validEdid8[24] = 0x00;

            string Display_Type1 = "";
            string Display_Type2 = "(Digital) RGB 4:4:4 + YCrCb 4:4:4 + YCrCb 4:2:2";
            string Display_Type3 = "(Digital) RGB 4:4:4 + YCrCb 4:2:2";
            string Display_Type4 = "(Digital) RGB 4:4:4 + YCrCb 4:4:4";
            string Display_Type5 = "(Digital) RGB 4:4:4";
            string Display_Type6 = "(Analog) Undefined";
            string Display_Type7 = "(Analog) Non-RGB color";
            string Display_Type8 = "(Analog) RGB color";
            string Display_Type9 = "(Analog) Monochrome or Grayscale";
            Gamma_Color_and_Etablished_Timings gamma_Color_And_Etablished_Timings = new Gamma_Color_and_Etablished_Timings();

            if (InvalidEdid == null || InvalidEdid.Length < 128)
            {
                var result = Gamma_Color_and_Etablished_Timings.Display_Type(InvalidEdid, videoInputType);
                Assert.That(Display_Type1, Is.EqualTo(result));
            }

            if (validEdid.Length >= 128)
            {
                if ((videoInputType & 0x80) == 0x80)
                {
                    if ((validEdid[24] & 0x18) == 0x18)
                    {
                        var result = Gamma_Color_and_Etablished_Timings.Display_Type(validEdid, videoInputType);
                        Assert.That(Display_Type2, Is.EqualTo(result));
                    }
                    if ((validEdid2[24] & 0x10) == 0x10)
                    {
                        var result = Gamma_Color_and_Etablished_Timings.Display_Type(validEdid2, videoInputType);
                        Assert.That(Display_Type3, Is.EqualTo(result));
                    }

                    if ((validEdid3[24] & 0x08) == 0x08)
                    {
                        var result = Gamma_Color_and_Etablished_Timings.Display_Type(validEdid3, videoInputType);
                        Assert.That(Display_Type4, Is.EqualTo(result));
                    }

                    if ((validEdid4[24] & 0x08) != 0x08)
                    {
                        var result = Gamma_Color_and_Etablished_Timings.Display_Type(validEdid4, videoInputType);
                        Assert.That(Display_Type5, Is.EqualTo(result));
                    }
                }

                if ((videoInputType2 & 0x80) != 0x80)
                {
                    if ((validEdid5[24] & 0x18) == 0x18)
                    {
                        var result = Gamma_Color_and_Etablished_Timings.Display_Type(validEdid5, videoInputType2);
                        Assert.That(Display_Type6, Is.EqualTo(result));
                    }

                    if ((validEdid6[24] & 0x10) == 0x10)
                    {
                        var result = Gamma_Color_and_Etablished_Timings.Display_Type(validEdid6, videoInputType2);
                        Assert.That(Display_Type7, Is.EqualTo(result));
                    }
                    if ((validEdid7[24] & 0x08) == 0x08)
                    {
                        var result = Gamma_Color_and_Etablished_Timings.Display_Type(validEdid7, videoInputType2);
                        Assert.That(Display_Type8, Is.EqualTo(result));
                    }
                    if ((validEdid8[24] & 0x08) != 0x08)
                    {
                        var result = Gamma_Color_and_Etablished_Timings.Display_Type(validEdid8, videoInputType2);
                        Assert.That(Display_Type9, Is.EqualTo(result));
                    }
                }
            }
        }

    }

    public class TestPreferred_Detailed_Timing
    {
        [Test]
        public void TestActive_Ratio()
        {
            byte[] validEdid = new byte[128];
            validEdid[56] = 10;
            validEdid[58] = 20;
            validEdid[59] = 30;
            validEdid[61] = 40;

            byte[] InvalidEdid = new byte[] { 0x01, 0x04, 0x05 };
            string Active_Ratio1 = "";
            string Active_Ratio2 = "133:271";

            Preferred_Detailed_Timing preferred_Detailed_Timing = new Preferred_Detailed_Timing();

            if (InvalidEdid == null || InvalidEdid.Length < 128)
            {
                var result = Preferred_Detailed_Timing.Active_Ratio(InvalidEdid);
                Assert.That(Active_Ratio1, Is.EqualTo(result));
            }

            if (validEdid.Length >= 128)
            {
                var result = Preferred_Detailed_Timing.Active_Ratio(validEdid);
                Assert.That(Active_Ratio2, Is.EqualTo(result));
            }
        }

        [Test]
        public void TestPixel_Clock()
        {
            byte[] validEdid = new byte[128];
            validEdid[54] = 60;
            validEdid[55] = 70;

            byte[] InvalidEdid = new byte[] { 0x01, 0x04, 0x05 };
            string Pixel_Clock1 = "";
            string Pixel_Clock2 = "179.80MHz";

            Preferred_Detailed_Timing preferred_Detailed_Timing = new Preferred_Detailed_Timing();

            if (InvalidEdid == null || InvalidEdid.Length < 128)
            {
                var result = Preferred_Detailed_Timing.Pixel_Clock(InvalidEdid);
                Assert.That(Pixel_Clock1, Is.EqualTo(result));
            }

            if (validEdid.Length >= 128)
            {
                var result = Preferred_Detailed_Timing.Pixel_Clock(validEdid);
                Assert.That(Pixel_Clock2, Is.EqualTo(result));
            }
        }

        [Test]
        public void TestHorizontal_Active()
        {
            byte[] validEdid = new byte[128];
            validEdid[56] = 40;
            validEdid[58] = 80;

            byte[] InvalidEdid = new byte[] { 0x01, 0x04, 0x05 };
            string Horizontal_Active1 = "";
            string Horizontal_Active2 = "1320 pixels";

            if (InvalidEdid == null || InvalidEdid.Length < 128)
            {
                var result = Preferred_Detailed_Timing.Horizontal_Active(InvalidEdid);
                Assert.That(Horizontal_Active1, Is.EqualTo(result));
            }

            if (validEdid.Length >= 128)
            {
                var result = Preferred_Detailed_Timing.Horizontal_Active(validEdid);
                Assert.That(Horizontal_Active2, Is.EqualTo(result));
            }
        }

        [Test]
        public void TestHorizontal_Blanking()
        {
            byte[] validEdid = new byte[128];
            validEdid[57] = 30;
            validEdid[58] = 70;

            byte[] InvalidEdid = new byte[] { 0x01, 0x04, 0x05 };
            string Horizontal_Blanking1 = "";
            string Horizontal_Blanking2 = "1566 pixels";

            if (InvalidEdid == null || InvalidEdid.Length < 128)
            {
                var result = Preferred_Detailed_Timing.Horizontal_Blanking(InvalidEdid);
                Assert.That(Horizontal_Blanking1, Is.EqualTo(result));
            }

            if (validEdid.Length >= 128)
            {
                var result = Preferred_Detailed_Timing.Horizontal_Blanking(validEdid);
                Assert.That(Horizontal_Blanking2, Is.EqualTo(result));
            }
        }

        [Test]
        public void TestHorizontal_Sync_Offset()
        {
            byte[] validEdid = new byte[128];
            validEdid[62] = 60;
            validEdid[65] = 40;

            byte[] InvalidEdid = new byte[] { 0x01, 0x04, 0x05 };
            string Horizontal_Sync_Offset1 = "";
            string Horizontal_Sync_Offset2 = "60 pixels";

            if (InvalidEdid == null || InvalidEdid.Length < 128)
            {
                var result = Preferred_Detailed_Timing.Horizontal_Sync_Offset(InvalidEdid);
                Assert.That(Horizontal_Sync_Offset1, Is.EqualTo(result));
            }

            if (validEdid.Length >= 128)
            {
                var result = Preferred_Detailed_Timing.Horizontal_Sync_Offset(validEdid);
                Assert.That(Horizontal_Sync_Offset2, Is.EqualTo(result));
            }
        }


        [Test]
        public void TestHorizontal_Sync_Pulse_Width()
        {
            byte[] validEdid = new byte[128];
            validEdid[63] = 20;
            validEdid[65] = 80;

            byte[] InvalidEdid = new byte[] { 0x01, 0x04, 0x05 };
            string Horizontal_Sync_Pulse_Width1 = "";
            string Horizontal_Sync_Pulse_Width2 = "276 pixels";

            if (InvalidEdid == null || InvalidEdid.Length < 128)
            {
                var result = Preferred_Detailed_Timing.Horizontal_Sync_Pulse_Width(InvalidEdid);
                Assert.That(Horizontal_Sync_Pulse_Width1, Is.EqualTo(result));
            }

            if (validEdid.Length >= 128)
            {
                var result = Preferred_Detailed_Timing.Horizontal_Sync_Pulse_Width(validEdid);
                Assert.That(Horizontal_Sync_Pulse_Width2, Is.EqualTo(result));
            }
        }

        [Test]
        public void TestHorizontal_Border()
        {
            byte[] validEdid = new byte[128];
            validEdid[69] = 200;
            string Horizontal_Border2 = "200 pixels";
            var result = Preferred_Detailed_Timing.Horizontal_Border(validEdid);
            Assert.That(Horizontal_Border2, Is.EqualTo(result));

        }

        [Test]
        public void TestHorizontal_Size()
        {
            byte[] validEdid = new byte[128];
            validEdid[66] = 40;
            validEdid[68] = 80;

            byte[] InvalidEdid = new byte[] { 0x01, 0x04, 0x05 };
            string Horizontal_Size1 = "";
            string Horizontal_Size2 = "1320 mm";

            if (InvalidEdid == null || InvalidEdid.Length < 128)
            {
                var result = Preferred_Detailed_Timing.Horizontal_Size(InvalidEdid);
                Assert.That(Horizontal_Size1, Is.EqualTo(result));
            }

            if (validEdid.Length >= 128)
            {
                var result = Preferred_Detailed_Timing.Horizontal_Size(validEdid);
                Assert.That(Horizontal_Size2, Is.EqualTo(result));
            }
        }

        [Test]
        public void TestVertical_Active()
        {
            byte[] validEdid = new byte[128];
            validEdid[59] = 30;
            validEdid[61] = 60;

            byte[] InvalidEdid = new byte[] { 0x01, 0x04, 0x05 };
            string Vertical_Active1 = "";
            string Vertical_Active2 = "798 lines";

            if (InvalidEdid == null || InvalidEdid.Length < 128)
            {
                var result = Preferred_Detailed_Timing.Vertical_Active(InvalidEdid);
                Assert.That(Vertical_Active1, Is.EqualTo(result));
            }

            if (validEdid.Length >= 128)
            {
                var result = Preferred_Detailed_Timing.Vertical_Active(validEdid);
                Assert.That(Vertical_Active2, Is.EqualTo(result));
            }
        }

        [Test]
        public void TestVertical_Blanking()
        {
            byte[] validEdid = new byte[128];
            validEdid[60] = 20;
            validEdid[61] = 90;

            byte[] InvalidEdid = new byte[] { 0x01, 0x04, 0x05 };
            string Vertical_Blanking1 = "";
            string Vertical_Blanking2 = "2580 lines";

            if (InvalidEdid == null || InvalidEdid.Length < 128)
            {
                var result = Preferred_Detailed_Timing.Vertical_Blanking(InvalidEdid);
                Assert.That(Vertical_Blanking1, Is.EqualTo(result));
            }

            if (validEdid.Length >= 128)
            {
                var result = Preferred_Detailed_Timing.Vertical_Blanking(validEdid);
                Assert.That(Vertical_Blanking2, Is.EqualTo(result));
            }
        }


        [Test]
        public void TestVertical_Sync_Offset()
        {
            byte[] validEdid = new byte[128];
            validEdid[64] = 30;
            validEdid[65] = 80;

            byte[] InvalidEdid = new byte[] { 0x01, 0x04, 0x05 };
            string Vertical_Sync_Offset1 = "";
            string Vertical_Sync_Offset2 = "1 lines";

            if (InvalidEdid == null || InvalidEdid.Length < 128)
            {
                var result = Preferred_Detailed_Timing.Vertical_Sync_Offset(InvalidEdid);
                Assert.That(Vertical_Sync_Offset1, Is.EqualTo(result));
            }

            if (validEdid.Length >= 128)
            {
                var result = Preferred_Detailed_Timing.Vertical_Sync_Offset(validEdid);
                Assert.That(Vertical_Sync_Offset2, Is.EqualTo(result));
            }
        }

        [Test]
        public void TestVertical_Sync_Pulse_Width()
        {
            byte[] validEdid = new byte[128];
            validEdid[64] = 25;
            validEdid[65] = 90;

            byte[] InvalidEdid = new byte[] { 0x01, 0x04, 0x05 };
            string Vertical_Sync_Pulse_Width1 = "";
            string Vertical_Sync_Pulse_Width2 = "41 lines";

            if (InvalidEdid == null || InvalidEdid.Length < 128)
            {
                var result = Preferred_Detailed_Timing.Vertical_Sync_Pulse_Width(InvalidEdid);
                Assert.That(Vertical_Sync_Pulse_Width1, Is.EqualTo(result));
            }

            if (validEdid.Length >= 128)
            {
                var result = Preferred_Detailed_Timing.Vertical_Sync_Pulse_Width(validEdid);
                Assert.That(Vertical_Sync_Pulse_Width2, Is.EqualTo(result));
            }
        }

        [Test]
        public void TestVertical_Border()
        {
            byte[] validEdid = new byte[128];
            validEdid[70] = 125;
            string Vertical_Border1 = "125 lines";
            var result = Preferred_Detailed_Timing.Vertical_Border(validEdid);
            Assert.That(Vertical_Border1, Is.EqualTo(result));
        }


        [Test]
        public void TestVertical_Size()
        {
            byte[] validEdid = new byte[128];
            validEdid[67] = 30;
            validEdid[68] = 95;

            byte[] InvalidEdid = new byte[] { 0x01, 0x04, 0x05 };
            string Vertical_Size1 = "";
            string Vertical_Size2 = "3870 mm";

            if (InvalidEdid == null || InvalidEdid.Length < 128)
            {
                var result = Preferred_Detailed_Timing.Vertical_Size(InvalidEdid);
                Assert.That(Vertical_Size1, Is.EqualTo(result));
            }

            if (validEdid.Length >= 128)
            {
                var result = Preferred_Detailed_Timing.Vertical_Size(validEdid);
                Assert.That(Vertical_Size2, Is.EqualTo(result));
            }
        }

        [Test]
        public void TestInput_Type_Sync_type()
        {
            byte[] validEdid1 = new byte[128];
            validEdid1[71] = 0x18;

            byte[] validEdid2 = new byte[128];
            validEdid2[71] = 0x10;

            byte[] validEdid3 = new byte[128];
            validEdid3[71] = 0x08;

            byte[] validEdid4 = new byte[128];
            validEdid4[71] = 0x00;

            byte[] validEdid5 = new byte[128];
            validEdid5[71] = 0x04;

            byte[] InvalidEdid = new byte[] { 0x01, 0x04, 0x05 };

            string Input_Type_Sync_type0 = ""; //0x04
            string Input_Type_Sync_type1 = "Digital separate";   //0x18  3
            string Input_Type_Sync_type2 = "Digital composite (on HSync)"; //0x10  2
            string Input_Type_Sync_type3 = "Bipolar analog composite"; //0x08  1
            string Input_Type_Sync_type4 = "Analog composite";  //0x00  0

            if (InvalidEdid == null || InvalidEdid.Length < 128)
            {
                var result = Preferred_Detailed_Timing.Input_Type_Sync_type(InvalidEdid);
                Assert.That(Input_Type_Sync_type0, Is.EqualTo(result));
            }

            if (validEdid1.Length >= 128)
            {
                if (((validEdid1[71] >> 3) & 0x0003) == 3)
                {
                    var result = Preferred_Detailed_Timing.Input_Type_Sync_type(validEdid1);
                    Assert.That(Input_Type_Sync_type1, Is.EqualTo(result));
                }

                if (((validEdid2[71] >> 3) & 0x0003) == 2)
                {
                    var result = Preferred_Detailed_Timing.Input_Type_Sync_type(validEdid2);
                    Assert.That(Input_Type_Sync_type2, Is.EqualTo(result));
                }

                if (((validEdid3[71] >> 3) & 0x0003) == 1)
                {
                    var result = Preferred_Detailed_Timing.Input_Type_Sync_type(validEdid3);
                    Assert.That(Input_Type_Sync_type3, Is.EqualTo(result));
                }

                if (((validEdid4[71] >> 3) & 0x0003) == 0)
                {
                    var result = Preferred_Detailed_Timing.Input_Type_Sync_type(validEdid4);
                    Assert.That(Input_Type_Sync_type4, Is.EqualTo(result));
                }

                if (((validEdid5[71] >> 3) & 0x0003) != 1)
                {
                    var result = Preferred_Detailed_Timing.Input_Type_Sync_type(validEdid5);
                    Assert.That(Input_Type_Sync_type4, Is.EqualTo(result));
                }
            }
        }

        [Test]
        public void TestInterlaced()
        {
            byte[] validEdid = new byte[128];
            validEdid[71] = 0x80;

            byte[] validEdid2 = new byte[128];
            validEdid2[71] = 0x41;

            byte[] InvalidEdid = new byte[] { 0x01, 0x04, 0x05 };
            string Interlaced1 = "";
            string Interlaced2 = "True";
            string Interlaced3 = "False";

            if (InvalidEdid == null || InvalidEdid.Length < 128)
            {
                var result = Preferred_Detailed_Timing.Interlaced(InvalidEdid);
                Assert.That(Interlaced1, Is.EqualTo(result));
            }

            if (validEdid.Length >= 128)
            {
                if (Contains(validEdid[71], 0x80))
                {
                    var result = Preferred_Detailed_Timing.Interlaced(validEdid);
                    Assert.That(Interlaced2, Is.EqualTo(result));
                }
                if (Contains(validEdid2[71], 0x80) == false)
                {
                    var result = Preferred_Detailed_Timing.Interlaced(validEdid2);
                    Assert.That(Interlaced3, Is.EqualTo(result));
                }
            }
        }

        [Test]
        public void TestVerticalPolarity()
        {
            byte[] validEdid = new byte[128];
            validEdid[71] = 0x04;

            byte[] validEdid2 = new byte[128];
            validEdid2[71] = 0x08;

            byte[] InvalidEdid = new byte[] { 0x01, 0x04, 0x05 };
            string VerticalPolarity1 = "";
            string VerticalPolarity2 = "True";
            string VerticalPolarity3 = "False";

            if (InvalidEdid == null || InvalidEdid.Length < 128)
            {
                var result = Preferred_Detailed_Timing.VerticalPolarity(InvalidEdid);
                Assert.That(VerticalPolarity1, Is.EqualTo(result));
            }

            if (validEdid.Length >= 128)
            {
                if (Contains(validEdid[71], 0x04))
                {
                    var result = Preferred_Detailed_Timing.VerticalPolarity(validEdid);
                    Assert.That(VerticalPolarity2, Is.EqualTo(result));
                }
                if (Contains(validEdid2[71], 0x04) == false)
                {
                    var result = Preferred_Detailed_Timing.VerticalPolarity(validEdid2);
                    Assert.That(VerticalPolarity3, Is.EqualTo(result));
                }
            }
        }

        [Test]
        public void TestHorizontalPolarity()
        {
            byte[] validEdid = new byte[128];
            validEdid[71] = 0x02;

            byte[] validEdid2 = new byte[128];
            validEdid2[71] = 0x09;

            byte[] InvalidEdid = new byte[] { 0x01, 0x04, 0x05 };
            string HorizontalPolarity1 = "";
            string HorizontalPolarity2 = "True";
            string HorizontalPolarity3 = "False";

            if (InvalidEdid == null || InvalidEdid.Length < 128)
            {
                var result = Preferred_Detailed_Timing.HorizontalPolarity(InvalidEdid);
                Assert.That(HorizontalPolarity1, Is.EqualTo(result));
            }

            if (validEdid.Length >= 128)
            {
                if (Contains(validEdid[71], 0x02))
                {
                    var result = Preferred_Detailed_Timing.HorizontalPolarity(validEdid);
                    Assert.That(HorizontalPolarity2, Is.EqualTo(result));
                }
                if (Contains(validEdid2[71], 0x02) == false)
                {
                    var result = Preferred_Detailed_Timing.HorizontalPolarity(validEdid2);
                    Assert.That(HorizontalPolarity3, Is.EqualTo(result));
                }
            }
        }
    }

    public class TestDetailed_Timing_Sharp2
    {
        [Test]
        public void TestPixel_Clock_()
        {
            byte[] validEdid = new byte[128];
            string Pixel_Clock1 = "";

            var result = Detailed_Timing_Sharp2.Pixel_Clock(validEdid);    // 	114.46 Mhz
            Assert.That(Pixel_Clock1, Is.EqualTo(result));
        }

        [Test]
        public void TestHorizontal_Active_()
        {
            byte[] validEdid = new byte[128];
            string Horizontal_Active1 = "";

            var result = Detailed_Timing_Sharp2.Horizontal_Active(validEdid); //1920 pixels
            Assert.That(Horizontal_Active1, Is.EqualTo(result));
        }

        [Test]
        public void TestHorizontal_Blanking_()
        {
            byte[] validEdid = new byte[128];
            string Horizontal_Blanking1 = "";

            var result = Detailed_Timing_Sharp2.Horizontal_Blanking(validEdid); //244 pixels
            Assert.That(Horizontal_Blanking1, Is.EqualTo(result));
        }

        [Test]
        public void TestHorizontal_Sync_Offset_()
        {
            byte[] validEdid = new byte[128];
            string Horizontal_Sync_Offset1 = "";

            var result = Detailed_Timing_Sharp2.Horizontal_Sync_Offset(validEdid); // 48 pixels
            Assert.That(Horizontal_Sync_Offset1, Is.EqualTo(result));
        }

        [Test]
        public void TestHorizontal_Sync_Pulse_Width_()
        {
            byte[] validEdid = new byte[128];
            string Horizontal_Sync_Pulse_Width1 = "";

            var result = Detailed_Timing_Sharp2.Horizontal_Sync_Pulse_Width(validEdid); //32 pixels
            Assert.That(Horizontal_Sync_Pulse_Width1, Is.EqualTo(result));
        }

        [Test]
        public void TestHorizontal_Border_()
        {
            byte[] validEdid = new byte[128];
            string Horizontal_Border1 = "";

            var result = Detailed_Timing_Sharp2.Horizontal_Border(validEdid); //0 pixels
            Assert.That(Horizontal_Border1, Is.EqualTo(result));
        }

        [Test]
        public void TestHorizontal_Size_()
        {
            byte[] validEdid = new byte[128];
            string Horizontal_Size1 = "";

            var result = Detailed_Timing_Sharp2.Horizontal_Size(validEdid); // 	344 mm
            Assert.That(Horizontal_Size1, Is.EqualTo(result));
        }


        [Test]
        public void TestVertical_Active_()
        {
            byte[] validEdid = new byte[128];
            string Vertical_Active1 = "";

            var result = Detailed_Timing_Sharp2.Vertical_Active(validEdid); //1080 lines
            Assert.That(Vertical_Active1, Is.EqualTo(result));
        }

        [Test]
        public void TestVertical_Blanking_()
        {
            byte[] validEdid = new byte[128];
            string Vertical_Blanking1 = "";

            var result = Detailed_Timing_Sharp2.Vertical_Blanking(validEdid); // 22 lines
            Assert.That(Vertical_Blanking1, Is.EqualTo(result));
        }

        [Test]
        public void TestVertical_Sync_Offset_()
        {
            byte[] validEdid = new byte[128];
            string Vertical_Sync_Offset1 = "";

            var result = Detailed_Timing_Sharp2.Vertical_Sync_Offset(validEdid); // 3 lines
            Assert.That(Vertical_Sync_Offset1, Is.EqualTo(result));
        }

        [Test]
        public void TestVertical_Sync_Pulse_Width_()
        {
            byte[] validEdid = new byte[128];
            string Vertical_Sync_Pulse_Width1 = "";

            var result = Detailed_Timing_Sharp2.Vertical_Sync_Pulse_Width(validEdid); // 5 lines
            Assert.That(Vertical_Sync_Pulse_Width1, Is.EqualTo(result));
        }

        [Test]
        public void TestVertical_Border_()
        {
            byte[] validEdid = new byte[128];
            string Vertical_Border1 = "";

            var result = Detailed_Timing_Sharp2.Vertical_Border(validEdid); // 0 lines
            Assert.That(Vertical_Border1, Is.EqualTo(result));
        }

        [Test]
        public void TestVertical_Size_()
        {
            byte[] validEdid = new byte[128];
            string Vertical_Size1 = "";

            var result = Detailed_Timing_Sharp2.Vertical_Size(validEdid); // 194 mm
            Assert.That(Vertical_Size1, Is.EqualTo(result));
        }

        [Test]
        public void TestInput_Type_()
        {
            byte[] validEdid = new byte[128];
            string Input_Type1 = "";

            var result = Detailed_Timing_Sharp2.Input_Type(validEdid); // Digital Separate
            Assert.That(Input_Type1, Is.EqualTo(result));
        }

        [Test]
        public void TestInterlaced_()
        {
            byte[] validEdid = new byte[128];
            string Interlaced1 = "";

            var result = Detailed_Timing_Sharp2.Interlaced(validEdid); // False
            Assert.That(Interlaced1, Is.EqualTo(result));
        }

        [Test]
        public void TestVerticalPolarity_()
        {
            byte[] validEdid = new byte[128];
            string VerticalPolarity1 = "";

            var result = Detailed_Timing_Sharp2.VerticalPolarity(validEdid); // False
            Assert.That(VerticalPolarity1, Is.EqualTo(result));
        }

        [Test]
        public void TestHorizontalPolarity_()
        {
            byte[] validEdid = new byte[128];
            string HorizontalPolarity1 = "";

            var result = Detailed_Timing_Sharp2.HorizontalPolarity(validEdid); // True
            Assert.That(HorizontalPolarity1, Is.EqualTo(result));
        }
    }

    public class TestMonitor_Range_Limit
    {
        [Test]
        public void TestMaximum_Vertical_Frequency()
        {
            byte[] validEdid = new byte[128];
            validEdid[96] = 0x72;  //114

            byte[] InvalidEdid = new byte[] { 0x01, 0x04, 0x05 };
            string Maximum_Vertical_Frequency1 = "";
            string Maximum_Vertical_Frequency2 = "114 Hz";

            if (InvalidEdid == null || InvalidEdid.Length < 128)
            {
                var result = Monitor_Range_Limit.Maximum_Vertical_Frequency(InvalidEdid);
                Assert.That(Maximum_Vertical_Frequency1, Is.EqualTo(result));
            }

            if (validEdid.Length >= 128)
            {
                var result = Monitor_Range_Limit.Maximum_Vertical_Frequency(validEdid);
                Assert.That(Maximum_Vertical_Frequency2, Is.EqualTo(result));
            }
        }
    }
}