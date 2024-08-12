using Dell.Client.Framework.UnitTestShared.Tests;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VcpCore.Plugins.Test
{
    public class TestNodeFormatter
    {
        NodeFormatter formatter = new NodeFormatter();

        [Test]
        public void TestFormatVCPControlName()
        {
            string vcpControlName1 = "VCP Code Page";
            var expectvcpControlName1 = NodeFormatter.FormatVCPControlName("00");
            string vcpControlName2 = "Color Temperature Increment";
            var expectvcpControlName2 = NodeFormatter.FormatVCPControlName("0b");
            string? vcpControlName3 = null;
            string vcpControlName4 = "PIP/PBP Mode";
            var expectvcpControlName4 = NodeFormatter.FormatVCPControlName("e9");
            var expectvcpControlName3 = NodeFormatter.FormatVCPControlName("99");
            Assert.That(vcpControlName1, Is.EqualTo(expectvcpControlName1));
            Assert.That(vcpControlName2, Is.EqualTo(expectvcpControlName2));
            Assert.That(vcpControlName3, Is.EqualTo(expectvcpControlName3));
            Assert.That(vcpControlName4, Is.EqualTo(expectvcpControlName4));
        }

        [Test]
        public void TestFormatVCP_F8()
        {
            string vcpControlName1 = "High Resolution";
            var expectvcpControlName1 = NodeFormatter.FormatVCP_F8("F800");
            string vcpControlName2 = "High Data Speed";
            var expectvcpControlName2 = NodeFormatter.FormatVCP_F8("F801");
            string? vcpControlName3 = null;
            var expectvcpControlName3 = NodeFormatter.FormatVCP_F8("F000");

            Assert.That(vcpControlName1, Is.EqualTo(expectvcpControlName1));
            Assert.That(vcpControlName2, Is.EqualTo(expectvcpControlName2));
            Assert.That(vcpControlName3, Is.EqualTo(expectvcpControlName3));
        }

        [Test]
        public void TestFormatVCP_14()
        {
            string vcpControlName1 = "sRGB";
            var expectvcpControlName1 = NodeFormatter.FormatVCP_14("01");
            string vcpControlName2 = "5000K";
            var expectvcpControlName2 = NodeFormatter.FormatVCP_14("04");
            string? vcpControlName3 = null;
            var expectvcpControlName3 = NodeFormatter.FormatVCP_14("0g");
            string vcpControlName4 = "Custom Color 1";
            var expectvcpControlName4 = NodeFormatter.FormatVCP_14("0c");

            Assert.That(vcpControlName1, Is.EqualTo(expectvcpControlName1));
            Assert.That(vcpControlName2, Is.EqualTo(expectvcpControlName2));
            Assert.That(vcpControlName3, Is.EqualTo(expectvcpControlName3));
            Assert.That(vcpControlName4, Is.EqualTo(expectvcpControlName4));
        }

        [Test]
        public void TestFormatVCP_AA()
        {
            string vcpControlName1 = "Reserved";
            var expectvcpControlName1 = NodeFormatter.FormatVCP_AA("00");
            string vcpControlName2 = "0 degrees";
            var expectvcpControlName2 = NodeFormatter.FormatVCP_AA("01");
            string vcpControlName3 = "90 degrees";
            var expectvcpControlName3 = NodeFormatter.FormatVCP_AA("02");
            string vcpControlName4 = "180 degrees";
            var expectvcpControlName4 = NodeFormatter.FormatVCP_AA("03");
            string vcpControlName5 = "270 degrees";
            var expectvcpControlName5 = NodeFormatter.FormatVCP_AA("04");
            string? vcpControlName6 = null;
            var expectvcpControlName6 = NodeFormatter.FormatVCP_AA("07");

            Assert.That(vcpControlName1, Is.EqualTo(expectvcpControlName1));
            Assert.That(vcpControlName2, Is.EqualTo(expectvcpControlName2));
            Assert.That(vcpControlName3, Is.EqualTo(expectvcpControlName3));
            Assert.That(vcpControlName4, Is.EqualTo(expectvcpControlName4));
            Assert.That(vcpControlName5, Is.EqualTo(expectvcpControlName5));
            Assert.That(vcpControlName6, Is.EqualTo(expectvcpControlName6));
        }

        [Test]
        public void TestFormatVCP_D6()
        {
            string vcpControlName1 = "Power Normal";
            var expectvcpControlName1 = NodeFormatter.FormatVCP_D6("01");
            string vcpControlName2 = "Power Saving";
            var expectvcpControlName2 = NodeFormatter.FormatVCP_D6("04");
            string vcpControlName3 = "Power Off";
            var expectvcpControlName3 = NodeFormatter.FormatVCP_D6("05");
            string? vcpControlName4 = null;
            var expectvcpControlName4 = NodeFormatter.FormatVCP_D6("06");

            Assert.That(vcpControlName1, Is.EqualTo(expectvcpControlName1));
            Assert.That(vcpControlName2, Is.EqualTo(expectvcpControlName2));
            Assert.That(vcpControlName3, Is.EqualTo(expectvcpControlName3));
            Assert.That(vcpControlName4, Is.EqualTo(expectvcpControlName4));
        }

        [Test]
        public void TestFormatVCP_8D()
        {
            string vcpControlName1 = "Mute the Mic";
            var expectvcpControlName1 = NodeFormatter.FormatVCP_8D("01");
            string vcpControlName2 = "UnMute the Mic";
            var expectvcpControlName2 = NodeFormatter.FormatVCP_8D("02");
            string? vcpControlName3 = null;
            var expectvcpControlName3 = NodeFormatter.FormatVCP_8D("08");

            Assert.That(vcpControlName1, Is.EqualTo(expectvcpControlName1));
            Assert.That(vcpControlName2, Is.EqualTo(expectvcpControlName2));
            Assert.That(vcpControlName3, Is.EqualTo(expectvcpControlName3));
        }

        [Test]
        public void TestFormatVCP_CC()
        {
            string vcpControlName1 = "English";
            var expectvcpControlName1 = NodeFormatter.FormatVCP_CC("02");
            string vcpControlName2 = "Deutschi";
            var expectvcpControlName2 = NodeFormatter.FormatVCP_CC("04");
            string vcpControlName3 = "Portuguese";
            var expectvcpControlName3 = NodeFormatter.FormatVCP_CC("08");
            string vcpControlName4 = "Russian";
            var expectvcpControlName4 = NodeFormatter.FormatVCP_CC("09");
            string? vcpControlName5 = null;
            var expectvcpControlName5 = NodeFormatter.FormatVCP_CC("0p");

            Assert.That(vcpControlName1, Is.EqualTo(expectvcpControlName1));
            Assert.That(vcpControlName2, Is.EqualTo(expectvcpControlName2));
            Assert.That(vcpControlName3, Is.EqualTo(expectvcpControlName3));
            Assert.That(vcpControlName4, Is.EqualTo(expectvcpControlName4));
            Assert.That(vcpControlName5, Is.EqualTo(expectvcpControlName5));
        }

        [Test]
        public void TestFormatVCP_60()
        {
            string vcpControlName1 = "Composite video 1";
            var expectvcpControlName1 = NodeFormatter.FormatVCP_60("05");
            string vcpControlName2 = "Mini DisplayPort-1";
            var expectvcpControlName2 = NodeFormatter.FormatVCP_60("10");
            string vcpControlName3 = "HDMI3";
            var expectvcpControlName3 = NodeFormatter.FormatVCP_60("15");
            string vcpControlName4 = "USB-C1";
            var expectvcpControlName4 = NodeFormatter.FormatVCP_60("1b");
            string vcpControlName5 = "USB Comm from USB2 (Type-B, port 2)";
            var expectvcpControlName5 = NodeFormatter.FormatVCP_60("81");
            string? vcpControlName6 = null;
            var expectvcpControlName6 = NodeFormatter.FormatVCP_60("88");

            Assert.That(vcpControlName1, Is.EqualTo(expectvcpControlName1));
            Assert.That(vcpControlName2, Is.EqualTo(expectvcpControlName2));
            Assert.That(vcpControlName3, Is.EqualTo(expectvcpControlName3));
            Assert.That(vcpControlName4, Is.EqualTo(expectvcpControlName4));
            Assert.That(vcpControlName5, Is.EqualTo(expectvcpControlName5));
            Assert.That(vcpControlName6, Is.EqualTo(expectvcpControlName6));
        }

        [Test]
        public void TestFormatVCP_66()
        {
            string vcpControlName1 = "ALS full function";
            var expectvcpControlName1 = NodeFormatter.FormatVCP_66("00f2");
            string vcpControlName2 = "ALS full function";
            var expectvcpControlName2 = NodeFormatter.FormatVCP_66("0f02");
            string vcpControlName3 = "ALS without ALS_Primary";
            var expectvcpControlName3 = NodeFormatter.FormatVCP_66("00d2");
            string vcpControlName4 = "ALS without ALS_Primary";
            var expectvcpControlName4 = NodeFormatter.FormatVCP_66("0d02");
            string vcpControlName5 = "ALS without sensor";
            var expectvcpControlName5 = NodeFormatter.FormatVCP_66("0012");
            string vcpControlName6 = "ALS without sensor";
            var expectvcpControlName6 = NodeFormatter.FormatVCP_66("0102");
            string? vcpControlName7 = null;
            var expectvcpControlName7 = NodeFormatter.FormatVCP_66("88");

            Assert.That(vcpControlName1, Is.EqualTo(expectvcpControlName1));
            Assert.That(vcpControlName2, Is.EqualTo(expectvcpControlName2));
            Assert.That(vcpControlName3, Is.EqualTo(expectvcpControlName3));
            Assert.That(vcpControlName4, Is.EqualTo(expectvcpControlName4));
            Assert.That(vcpControlName5, Is.EqualTo(expectvcpControlName5));
            Assert.That(vcpControlName6, Is.EqualTo(expectvcpControlName6));
            Assert.That(vcpControlName7, Is.EqualTo(expectvcpControlName7));
        }

        [Test]
        public void TestFormatVCP_DC()
        {
            string vcpControlName1 = "Standard";
            var expectvcpControlName1 = NodeFormatter.FormatVCP_DC("00");
            string vcpControlName2 = "Multimedia";
            var expectvcpControlName2 = NodeFormatter.FormatVCP_DC("02");
            string vcpControlName3 = "Movie";
            var expectvcpControlName3 = NodeFormatter.FormatVCP_DC("03");
            string vcpControlName4 = "Nature";
            var expectvcpControlName4 = NodeFormatter.FormatVCP_DC("04");
            string vcpControlName5 = "Game/Game1";
            var expectvcpControlName5 = NodeFormatter.FormatVCP_DC("05");
            string vcpControlName6 = "Sport";
            var expectvcpControlName6 = NodeFormatter.FormatVCP_DC("06");
            string? vcpControlName7 = null;
            var expectvcpControlName7 = NodeFormatter.FormatVCP_DC("08");

            Assert.That(vcpControlName1, Is.EqualTo(expectvcpControlName1));
            Assert.That(vcpControlName2, Is.EqualTo(expectvcpControlName2));
            Assert.That(vcpControlName3, Is.EqualTo(expectvcpControlName3));
            Assert.That(vcpControlName4, Is.EqualTo(expectvcpControlName4));
            Assert.That(vcpControlName5, Is.EqualTo(expectvcpControlName5));
            Assert.That(vcpControlName6, Is.EqualTo(expectvcpControlName6));
            Assert.That(vcpControlName7, Is.EqualTo(expectvcpControlName7));
        }

        [Test]
        public void TestFormatVCP_F0()
        {
            string vcpControlName1 = "CAL1";
            var expectvcpControlName1 = NodeFormatter.FormatVCP_F0("05");
            string vcpControlName2 = "ComfortView";
            var expectvcpControlName2 = NodeFormatter.FormatVCP_F0("0c");
            string vcpControlName3 = "Standard HDR";
            var expectvcpControlName3 = NodeFormatter.FormatVCP_F0("30");
            string vcpControlName4 = "Desktop";
            var expectvcpControlName4 = NodeFormatter.FormatVCP_F0("34");
            string vcpControlName5 = "Display P3";
            var expectvcpControlName5 = NodeFormatter.FormatVCP_F0("a1");
            string? vcpControlName6 = null;
            var expectvcpControlName6 = NodeFormatter.FormatVCP_F0("c8");

            Assert.That(vcpControlName1, Is.EqualTo(expectvcpControlName1));
            Assert.That(vcpControlName2, Is.EqualTo(expectvcpControlName2));
            Assert.That(vcpControlName3, Is.EqualTo(expectvcpControlName3));
            Assert.That(vcpControlName4, Is.EqualTo(expectvcpControlName4));
            Assert.That(vcpControlName5, Is.EqualTo(expectvcpControlName5));
            Assert.That(vcpControlName6, Is.EqualTo(expectvcpControlName6));
        }


        [Test]
        public void TestFormatVCP_E2()
        {
            string vcpControlName1 = "Multimedia";
            var expectvcpControlName1 = NodeFormatter.FormatVCP_E2("01");
            string vcpControlName2 = "Game";
            var expectvcpControlName2 = NodeFormatter.FormatVCP_E2("04");
            string vcpControlName3 = "DICOM";
            var expectvcpControlName3 = NodeFormatter.FormatVCP_E2("09");
            string vcpControlName4 = "Custom Color";
            var expectvcpControlName4 = NodeFormatter.FormatVCP_E2("14");
            string vcpControlName5 = "ComfortView";
            var expectvcpControlName5 = NodeFormatter.FormatVCP_E2("1d");
            string vcpControlName6 = "Multiscreen Match";
            var expectvcpControlName6 = NodeFormatter.FormatVCP_E2("29");
            string? vcpControlName7 = null;
            var expectvcpControlName7 = NodeFormatter.FormatVCP_E2("9f");

            Assert.That(vcpControlName1, Is.EqualTo(expectvcpControlName1));
            Assert.That(vcpControlName2, Is.EqualTo(expectvcpControlName2));
            Assert.That(vcpControlName3, Is.EqualTo(expectvcpControlName3));
            Assert.That(vcpControlName4, Is.EqualTo(expectvcpControlName4));
            Assert.That(vcpControlName5, Is.EqualTo(expectvcpControlName5));
            Assert.That(vcpControlName6, Is.EqualTo(expectvcpControlName6));
            Assert.That(vcpControlName7, Is.EqualTo(expectvcpControlName7));
        }

        [Test]
        public void TestFormatNode()
        {
            INode node;
            var nodeMock = new Mock<INode>();
            node = nodeMock.Object;
            nodeMock.Setup(n => n.Parent).Returns(nodeMock.Object);
            nodeMock.Setup(n => n.Value).Returns("SOME_VALUE");
            var lookupTables = new Dictionary<string, Func<string, string>>
            {
                { "parentkey", value => "FORMATTED_" + value }
            };
            var result = formatter.FormatNode(nodeMock.Object);
            Assert.IsNull(result);
        }
    }
}
