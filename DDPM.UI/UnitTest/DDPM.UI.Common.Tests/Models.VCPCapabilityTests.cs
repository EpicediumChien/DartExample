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
    public class VCPCapabilityTests
    {
        private VCPCapability? vCPCapability;

        [SetUp]
        public void Setup()
        {
            vCPCapability = new VCPCapability();
        }

        [Test]
        public void TestConstructor_VCPCapability()
        {
            // Assert
            Assert.That(vCPCapability, Is.Not.Null);
        }

        [Test]
        public void TestName()
        {
            // Act
            vCPCapability.Name = "Name";
            // Assert
            Assert.That(vCPCapability.Name, Is.EqualTo("Name"));
        }

        [Test]
        public void TestOptCode()
        {
            // Act
            vCPCapability.OptCode = 'O';
            // Assert
            Assert.That(vCPCapability.OptCode, Is.EqualTo('O'));
        }

        [Test]
        public void TestValue()
        {
            // Act
            vCPCapability.Value = 2;
            // Assert
            Assert.That(vCPCapability.Value, Is.EqualTo(2));
        }

        [Test]
        public void TestMaxValue()
        {
            // Act
            vCPCapability.MaxValue = 8;
            // Assert
            Assert.That(vCPCapability.MaxValue, Is.EqualTo(8));
        }

        [Test]
        public void TestgetVcpAndValue()
        {
            // Act
            var result = VcpCodeList.getVcpAndValue("Movie");
            // Assert
            Assert.That(result, Is.Not.Null);
        }
        
    }
}