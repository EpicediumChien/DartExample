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
}