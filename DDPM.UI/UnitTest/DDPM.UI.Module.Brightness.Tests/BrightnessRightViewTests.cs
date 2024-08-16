using DDPM.SA.Common;
using DDPM.UI.Common;
using DDPM.UI.Common.Interfaces;
using Moq;
using NGA.UnitTest.PrivateObject;
using NUnit.Framework;

namespace DDPM.UI.Module.Brightness.Tests
{
    [Apartment(ApartmentState.STA)]
    public class BrightnessRightViewTests
    {
        private BrightnessRightView? brightnessRightView;
        private PrivateObject? privateObject;
        private Mock<IModuleOwner>? moduleOwnerMock;
        private Mock<IDeviceManagerSA>? deviceManagerMock;

        [SetUp]
        public void Setup()
        {
            moduleOwnerMock = new Mock<IModuleOwner>();
            var moduleOwner = moduleOwnerMock!.Object;
            DdpmCommonHelper.ModuleOwner = moduleOwner;
            //brightnessRightView = new BrightnessRightView();
            //privateObject = new PrivateObject(brightnessRightView);
        }

        [Test]
        public void TestConstructor_InitializesComponent()
        {
            brightnessRightView = new BrightnessRightView();
            privateObject = new PrivateObject(brightnessRightView);
            // Assert
            Assert.That(brightnessRightView, Is.Not.Null);
        }
    }
}