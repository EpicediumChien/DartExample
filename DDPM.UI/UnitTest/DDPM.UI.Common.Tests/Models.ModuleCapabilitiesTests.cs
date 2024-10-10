using DDPM.SA.Common;
using DDPM.UI.Common.EAEM;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using DDPM.UI.Common.ViewModels;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using DPeMPublic.Common.Enums;
using Moq;
using System.Runtime.Intrinsics.X86;
using System.Security.Policy;
using System.Windows.Media;
using System.Xml.Linq;
using VcpCore.Common;
using Windows.Devices.Input;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace DDPM.UI.Common.Tests
{
    [Apartment(ApartmentState.STA)]
    public class ModuleCapabilitiesTests
    {
        private ModuleCapabilities? moduleCapabilities;

        [SetUp]
        public void Setup()
        {
            moduleCapabilities = new ModuleCapabilities();
        }

        [Test]
        public void TestConstructor_ModuleCapabilities()
        {
            // Assert
            Assert.That(moduleCapabilities, Is.Not.Null);
        }

        [Test]
        public void TestBrightnessContrast()
        {
            // Act
            moduleCapabilities.BrightnessContrast = true;
            // Assert
            Assert.That(moduleCapabilities.BrightnessContrast, Is.EqualTo(true));
        }

        [Test]
        public void TestColor()
        {
            // Act
            moduleCapabilities.Color = true;
            // Assert
            Assert.That(moduleCapabilities.Color, Is.EqualTo(true));
        }

        [Test]
        public void TestDisplayProperties()
        {
            // Act
            moduleCapabilities.DisplayProperties = true;
            // Assert
            Assert.That(moduleCapabilities.DisplayProperties, Is.EqualTo(true));
        }

        [Test]
        public void TestInputSource()
        {
            // Act
            moduleCapabilities.InputSource = true;
            // Assert
            Assert.That(moduleCapabilities.InputSource, Is.EqualTo(true));
        }

        [Test]
        public void TestPipPbp()
        {
            // Act
            moduleCapabilities.PipPbp = true;
            // Assert
            Assert.That(moduleCapabilities.PipPbp, Is.EqualTo(true));
        }


        [Test]
        public void TestDisplayHotkeys()
        {
            // Act
            moduleCapabilities.DisplayHotkeys = true;
            // Assert
            Assert.That(moduleCapabilities.DisplayHotkeys, Is.EqualTo(true));
        }

        [Test]
        public void TestEzArrange()
        {
            // Act
            moduleCapabilities.EzArrange = true;
            // Assert
            Assert.That(moduleCapabilities.EzArrange, Is.EqualTo(true));
        }

        [Test]
        public void TestEzMemeory()
        {
            // Act
            moduleCapabilities.EzMemeory = true;
            // Assert
            Assert.That(moduleCapabilities.EzMemeory, Is.EqualTo(true));
        }

        [Test]
        public void TestEzSettings()
        {
            // Act
            moduleCapabilities.EzSettings = true;
            // Assert
            Assert.That(moduleCapabilities.EzSettings, Is.EqualTo(true));
        }


        [Test]
        public void TestGaming()
        {
            // Act
            moduleCapabilities.Gaming = true;
            // Assert
            Assert.That(moduleCapabilities.Gaming, Is.EqualTo(true));
        }

        [Test]
        public void TestVisionEngine()
        {
            // Act
            moduleCapabilities.VisionEngine = true;
            // Assert
            Assert.That(moduleCapabilities.VisionEngine, Is.EqualTo(true));
        }


        [Test]
        public void TestKvm()
        {
            // Act
            moduleCapabilities.Kvm = true;
            // Assert
            Assert.That(moduleCapabilities.Kvm, Is.EqualTo(true));
        }

        [Test]
        public void TestDisplayOthers()
        {
            // Act
            moduleCapabilities.DisplayOthers = true;
            // Assert
            Assert.That(moduleCapabilities.DisplayOthers, Is.EqualTo(true));
        }
        
    }
}