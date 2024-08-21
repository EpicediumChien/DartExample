using DDPM.SA.Common;
using DDPM.UI.Common;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using Moq;
using NGA.UnitTest.PrivateObject;
using VcpCore.Common;

namespace DDPM.UI.Module.Gaming.Tests
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public class GamingViewModelTests
    {
        private Mock<IModuleOwner>? moduleOwnerMock;
        private IModuleOwner? moduleOwner;
        private Mock<IDeviceManagerSA>? deviceManagerMock;
        private IDeviceManagerSA? deviceManagerSA;
        private HomeDevice? selectedHomeDevice;
        private GamingViewModel? gamingViewModel;
        private PrivateObject? privateObject;

        [SetUp]
        public void Setup()
        {
            moduleOwnerMock = new Mock<IModuleOwner>();
            moduleOwner = moduleOwnerMock!.Object;
            DdpmCommonHelper.ModuleOwner = moduleOwner;

            deviceManagerMock = new Mock<IDeviceManagerSA>();
            deviceManagerSA = deviceManagerMock.Object;
            DdpmCommonHelper.DeviceManagerSA = deviceManagerSA;
            selectedHomeDevice = new HomeDevice();
            gamingViewModel = new GamingViewModel();
            privateObject = new PrivateObject(gamingViewModel);
        }

        [Test]
        public void TestModuleOwner()
        {
            // Arrange
            gamingViewModel.ModuleOwner = moduleOwner;
            Assert.That(gamingViewModel.ModuleOwner, Is.EqualTo(moduleOwner));
        }

        [Test]
        public void TestMyModule()
        {
            var myModule = new GamingModule();
            gamingViewModel.MyModule = myModule;
            Assert.That(gamingViewModel.MyModule, Is.EqualTo(myModule));
        }

        [Test]
        public void TesResolution_ItemsCollection()
        {
            var resolution_ItemsCollection = new List<UI_Properties>();
            gamingViewModel.Resolution_ItemsCollection = resolution_ItemsCollection;
            Assert.That(gamingViewModel.Resolution_ItemsCollection, Is.EqualTo(resolution_ItemsCollection));
        }

        [Test]
        public void TestSelectedResolution()
        {
            var selectedResolution = new UI_Properties();
            gamingViewModel.SelectedResolution = selectedResolution;
            Assert.That(gamingViewModel.SelectedResolution, Is.EqualTo(selectedResolution));
        }

        [Test]
        public void TestIsBusy()
        {
            bool myIsBusy = true;
            gamingViewModel.IsBusy = myIsBusy;

            Assert.That(gamingViewModel.IsBusy, Is.EqualTo(myIsBusy));
        }

        [Test]
        public void TestRefreshUI()
        {
            var selectedResolutionBef = privateObject.GetFieldOrProperty("SelectedResolution");
            var selectedOrientationBef = privateObject.GetFieldOrProperty("SelectedOrientation");
            var resolution_ItemsCollectionBef = privateObject.GetFieldOrProperty("Resolution_ItemsCollection");
            var orientation_ItemsCollectionBef = privateObject.GetFieldOrProperty("Orientation_ItemsCollection");
            var supportedHDRBef = privateObject.GetFieldOrProperty("SupportedHDR");
            var hDRStatusBef = privateObject.GetFieldOrProperty("HDRStatus");
            var hDRStatus_StringBef = privateObject.GetFieldOrProperty("HDRStatus_String");
            var supportedUSBCPrioeitizationBef = privateObject.GetFieldOrProperty("SupportedUSBCPrioeitization");
            var isHighDataSpeedBef = privateObject.GetFieldOrProperty("IsHighDataSpeed");
            var isHighResolutionBef = privateObject.GetFieldOrProperty("IsHighResolution");

            privateObject.SetFieldOrProperty("_selectedResolution", new UI_Properties());

            moduleOwnerMock.Setup(x => x.SelectedHomeDevice).Returns(new HomeDevice());
            gamingViewModel.MyModule = new GamingModule();
            gamingViewModel.MyModule.SelectedHomeDevice = selectedHomeDevice;

            var mySelectedResolution = new UI_Properties();
            var monitorInfo = new MonitorInfo();
            monitorInfo.DisplayName = "123";
            gamingViewModel.MyModule.SelectedHomeDevice.MonitorInfo = monitorInfo;
            var myresolution_ItemsCollection = new List<UI_Properties>();
            gamingViewModel.Resolution_ItemsCollection = myresolution_ItemsCollection;

            gamingViewModel.SelectedResolution = mySelectedResolution;

            gamingViewModel.RefreshUI();
            var selectedResolutionAft = privateObject.GetFieldOrProperty("SelectedResolution");
            var selectedOrientationAft = privateObject.GetFieldOrProperty("SelectedOrientation");
            var resolution_ItemsCollectionAft = privateObject.GetFieldOrProperty("Resolution_ItemsCollection");
            var orientation_ItemsCollectionAft = privateObject.GetFieldOrProperty("Orientation_ItemsCollection");
            var supportedHDRAft = privateObject.GetFieldOrProperty("SupportedHDR");
            var hDRStatusAft = privateObject.GetFieldOrProperty("HDRStatus");
            var hDRStatus_StringAft = privateObject.GetFieldOrProperty("HDRStatus_String");
            var supportedUSBCPrioeitizationAft = privateObject.GetFieldOrProperty("SupportedUSBCPrioeitization");
            var isHighDataSpeedAft = privateObject.GetFieldOrProperty("IsHighDataSpeed");
            var isHighResolutionAft = privateObject.GetFieldOrProperty("IsHighResolution");

            Assert.That(selectedResolutionAft, Is.Not.SameAs(selectedResolutionBef));
            Assert.That(selectedOrientationAft, Is.Not.SameAs(selectedOrientationBef));
            Assert.That(resolution_ItemsCollectionAft, Is.Not.SameAs(resolution_ItemsCollectionBef));
            Assert.That(orientation_ItemsCollectionAft, Is.Not.SameAs(orientation_ItemsCollectionBef));
            Assert.That(supportedHDRAft, Is.Not.SameAs(supportedHDRBef));
            Assert.That(hDRStatusAft, Is.Not.SameAs(hDRStatusBef));
            Assert.That(hDRStatus_StringAft, Is.Not.SameAs(hDRStatus_StringBef));
            Assert.That(supportedUSBCPrioeitizationAft, Is.Not.SameAs(supportedUSBCPrioeitizationBef));
            Assert.That(isHighDataSpeedAft, Is.Not.SameAs(isHighDataSpeedBef));
            Assert.That(isHighResolutionAft, Is.Not.SameAs(isHighResolutionBef));
        }

        [Test]
        public void TestConstructor_InitializesComponent()
        {
            // Assert
            Assert.That(gamingViewModel, Is.Not.Null);
        }

        [Test]
        public void TestProperties()
        {
            UI_Properties uI_Properties = new UI_Properties();
            var myuI_Properties = new Properties();
            uI_Properties.Properties = myuI_Properties;
            // Assert
            Assert.That(uI_Properties.Properties, Is.EqualTo(myuI_Properties));
        }

        [Test]
        public void TestUI_PDisplayText()
        {
            UI_Properties uI_Properties = new UI_Properties();
            var properties = new Properties();
            uI_Properties.Properties = properties;
            var result = uI_Properties.DisplayText;
            // Assert
            Assert.That(result, Is.Not.Null);
        }

        //[Test]
        //public void TestOrientation()
        //{
        //    UI_Orientation uI_Orientation = new UI_Orientation();
        //    var myUI_Orientation = new DisplayOrientation();
        //    uI_Orientation.Orientation = myUI_Orientation;
        //    // Assert
        //    Assert.That(uI_Orientation.Orientation, Is.EqualTo(myUI_Orientation));
        //}

        //[Test]
        //public void TestUI_ODisplayText()
        //{
        //    UI_Orientation uI_Orientation = new UI_Orientation();
        //    var result = uI_Orientation.DisplayText;
        //    // Assert
        //    Assert.That(result, Is.Not.Null);
        //}
    }
}