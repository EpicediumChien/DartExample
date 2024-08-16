using System.Diagnostics.CodeAnalysis;
using System.Xml.Linq;

namespace DDPM.UI.Module.PipPbp.Tests
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public class InputSourceObjTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void TestConstructor_InputSourceObj_Unused()
        {
            var inputSourceObj_Unused = new InputSourceObj_Unused(0x01, "VGA-1");
            Assert.That(inputSourceObj_Unused.Code, Is.EqualTo(0x01));
            Assert.That(inputSourceObj_Unused.Name, Is.EqualTo("VGA-1"));
        }

        [Test]
        public void TestConstructor_InputSourceObj_Unused1()
        {
            var inputSourceObj_Unused = new InputSourceObj_Unused(0x01);
            Assert.That(inputSourceObj_Unused.Code, Is.EqualTo(0x01));
            Assert.That(inputSourceObj_Unused, Is.Not.Null);
        }

        [Test]
        public void TestCode()
        {
            var inputSourceObj_Unused = new InputSourceObj_Unused();
            UInt16 code = 0x01;
            inputSourceObj_Unused.Code = code;
            Assert.That(inputSourceObj_Unused.Code, Is.EqualTo(0x01));
        }

        [Test]
        public void TestName()
        {
            var inputSourceObj_Unused = new InputSourceObj_Unused();
            string name = "VGA-1";
            inputSourceObj_Unused.Name = name;
            Assert.That(inputSourceObj_Unused.Name, Is.EqualTo("VGA-1"));
        }
    }
}