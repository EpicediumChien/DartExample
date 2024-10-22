using DDPM.SA.Common;
using DDPM.UI.Common.EAEM;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using DDPM.UI.Common.ViewModels;
using DDPM.UI.Interfaces;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using DPeMPublic.Common.Enums;
using Moq;
using System.Reflection.Metadata;
using System.Runtime.Intrinsics.X86;
using System.Security.Policy;
using System.Windows.Media;
using System.Xml.Linq;
using VcpCore.Common;
using Windows.Devices.Input;
using static DDPM.UI.Common.User32;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace DDPM.UI.Common.Tests
{
    [Apartment(ApartmentState.STA)]
    public class RightViewHeaderTests
    {
        private RightViewHeader? rightViewHeader;

        [SetUp]
        public void Setup()
        {
            rightViewHeader = new RightViewHeader(1,"a",null);
        }

        [Test]
        public void TestConstructor_RightViewHeader()
        {
            // Assert
            Assert.That(rightViewHeader, Is.Not.Null);
        }

        [Test]
        public void TestId()
        {
            // Act
            rightViewHeader.Id = 1;
            // Assert
            Assert.That(rightViewHeader.Id, Is.EqualTo(1));
        }

        [Test]
        public void TestText()
        {
            // Act
            rightViewHeader.Text = "Text";
            // Assert
            Assert.That(rightViewHeader.Text, Is.EqualTo("Text"));
        }

        [Test]
        public void TestImageFile()
        {
            // Act
            rightViewHeader.ImageFile = "ImageFile";
            // Assert
            Assert.That(rightViewHeader.ImageFile, Is.EqualTo("ImageFile"));
        }

        [Test]
        public void TestDdpmModule()
        {
            // Act
            var DdpmModuleMock = new Mock<IDdpmModule>();
            rightViewHeader.DdpmModule = DdpmModuleMock.Object;
            // Assert
            Assert.That(rightViewHeader.DdpmModule, Is.EqualTo(DdpmModuleMock.Object));
        }

        [Test]
        public void TestIsShown()
        {
            // Act
            rightViewHeader.IsShown = true;
            // Assert
            Assert.That(rightViewHeader.IsShown, Is.EqualTo(true));
        }
    }
}