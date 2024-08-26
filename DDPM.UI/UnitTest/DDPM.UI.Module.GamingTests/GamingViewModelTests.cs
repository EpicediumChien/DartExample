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
        private GamingModule? myModule;

        [SetUp]
        public void Setup()
        {
            moduleOwnerMock = new Mock<IModuleOwner>();
            moduleOwner = moduleOwnerMock!.Object;
            DdpmCommonHelper.ModuleOwner = moduleOwner;

            deviceManagerMock = new Mock<IDeviceManagerSA>();
            deviceManagerSA = deviceManagerMock.Object;
            DdpmCommonHelper.DeviceManagerSA = deviceManagerSA;
            myModule = new GamingModule();
            selectedHomeDevice = new HomeDevice();
            gamingViewModel = new GamingViewModel();
            gamingViewModel.MyModule= myModule;
            gamingViewModel.MyModule.SelectedHomeDevice = selectedHomeDevice;
            gamingViewModel.MyModule.SelectedHomeDevice.MonitorInfo = new MonitorInfo() { modelName = "gx" };
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
            var selectedOrientationBef = privateObject.GetFieldOrProperty("SelectedGameEnhancementMode");
            var selectedResponseTimeBef = privateObject.GetFieldOrProperty("SelectedResponseTime");
            var selectedDarkStabilizerBef = privateObject.GetFieldOrProperty("SelectedDarkStabilizer");
            var selectedHDRTypeBef = privateObject.GetFieldOrProperty("SelectedHDRType");
            var resolution_ItemsCollectionBef = privateObject.GetFieldOrProperty("Resolution_ItemsCollection");
            var fameEnhanceMode_ItemsCollectionBef = privateObject.GetFieldOrProperty("GameEnhanceMode_ItemsCollection");
            var responseTime_ItemsCollectionBef = privateObject.GetFieldOrProperty("ResponseTime_ItemsCollection");
            var darkStabilizer_ItemsCollectionBef = privateObject.GetFieldOrProperty("DarkStabilizer_ItemsCollection");
            var hDRType_ItemsCollectionBef = privateObject.GetFieldOrProperty("HDRType_ItemsCollection");
            var isGameSeriesBef = privateObject.GetFieldOrProperty("IsGameSeries");
            var isAWSeriesBef = privateObject.GetFieldOrProperty("IsAWSeries");
            var gameEnhanceMode_IsEnableBef = privateObject.GetFieldOrProperty("GameEnhanceMode_IsEnable");
            var gameEnhanceMode_OpacityBef = privateObject.GetFieldOrProperty("GameEnhanceMode_Opacity");
            var responseTime_IsEnableBef = privateObject.GetFieldOrProperty("ResponseTime_IsEnable");
            var responseTime_OpacityBef = privateObject.GetFieldOrProperty("ResponseTime_Opacity");
            var hDRType_IsEnableBef = privateObject.GetFieldOrProperty("HDRType_IsEnable");
            var hDRType_OpacityBef = privateObject.GetFieldOrProperty("HDRType_Opacity");
            var darkStabilizer_IsEnableBef = privateObject.GetFieldOrProperty("DarkStabilizer_IsEnable");
            var darkStabilizer_OpacityBef = privateObject.GetFieldOrProperty("DarkStabilizer_Opacity");

            privateObject.SetFieldOrProperty("_selectedResolution", new UI_Properties());
            moduleOwnerMock.Setup(x => x.SelectedHomeDevice).Returns(new HomeDevice());
            var mySelectedResolution = new UI_Properties();
            var monitorInfo = new MonitorInfo();
            monitorInfo.DisplayName = "123";
            gamingViewModel.MyModule.SelectedHomeDevice.MonitorInfo = monitorInfo;
            var myresolution_ItemsCollection = new List<UI_Properties>();
            gamingViewModel.Resolution_ItemsCollection = myresolution_ItemsCollection;
            gamingViewModel.SelectedResolution = mySelectedResolution;

            gamingViewModel.RefreshUI();
            var selectedResolutionAft = privateObject.GetFieldOrProperty("SelectedResolution");
            var selectedGameEnhancementModeAft = privateObject.GetFieldOrProperty("SelectedGameEnhancementMode");
            var selectedResponseTimeAft = privateObject.GetFieldOrProperty("SelectedResponseTime");
            var selectedDarkStabilizerAft = privateObject.GetFieldOrProperty("SelectedDarkStabilizer");
            var selectedHDRTypeAft = privateObject.GetFieldOrProperty("SelectedHDRType");
            var resolution_ItemsCollectionAft = privateObject.GetFieldOrProperty("Resolution_ItemsCollection");
            var gameEnhanceMode_ItemsCollectionAft = privateObject.GetFieldOrProperty("GameEnhanceMode_ItemsCollection");
            var responseTime_ItemsCollectionAft = privateObject.GetFieldOrProperty("ResponseTime_ItemsCollection");
            var darkStabilizer_ItemsCollectionAft = privateObject.GetFieldOrProperty("DarkStabilizer_ItemsCollection");
            var hDRType_ItemsCollectionAft = privateObject.GetFieldOrProperty("HDRType_ItemsCollection");
            var isGameSeriesAft = privateObject.GetFieldOrProperty("IsGameSeries");
            var hsAWSeriesAft = privateObject.GetFieldOrProperty("IsAWSeries");
            var gameEnhanceMode_IsEnableAft = privateObject.GetFieldOrProperty("GameEnhanceMode_IsEnable");
            var gameEnhanceMode_OpacityAft = privateObject.GetFieldOrProperty("GameEnhanceMode_Opacity");
            var responseTime_IsEnableAft = privateObject.GetFieldOrProperty("ResponseTime_IsEnable");
            var responseTime_OpacityAft = privateObject.GetFieldOrProperty("ResponseTime_Opacity");
            var hDRType_IsEnableAft = privateObject.GetFieldOrProperty("HDRType_IsEnable");
            var hDRType_OpacityAft = privateObject.GetFieldOrProperty("HDRType_Opacity");
            var darkStabilizer_IsEnableAft = privateObject.GetFieldOrProperty("DarkStabilizer_IsEnable");
            var darkStabilizer_OpacityAft = privateObject.GetFieldOrProperty("DarkStabilizer_Opacity");

            Assert.That(selectedResolutionAft, Is.Not.SameAs(selectedResolutionBef));
            //Assert.That(selectedGameEnhancementModeAft, Is.Not.SameAs(selectedOrientationBef));
            //Assert.That(selectedResponseTimeAft, Is.Not.SameAs(selectedResponseTimeBef));
            //Assert.That(selectedDarkStabilizerAft, Is.Not.SameAs(selectedDarkStabilizerBef));
            //Assert.That(selectedHDRTypeAft, Is.Not.SameAs(selectedHDRTypeBef));
            //Assert.That(resolution_ItemsCollectionAft, Is.Not.SameAs(resolution_ItemsCollectionBef));
            //Assert.That(gameEnhanceMode_ItemsCollectionAft, Is.Not.SameAs(fameEnhanceMode_ItemsCollectionBef));
            //Assert.That(responseTime_ItemsCollectionAft, Is.Not.SameAs(responseTime_ItemsCollectionBef));
            //Assert.That(darkStabilizer_ItemsCollectionAft, Is.Not.SameAs(darkStabilizer_ItemsCollectionBef));
            //Assert.That(hDRType_ItemsCollectionAft, Is.Not.SameAs(hDRType_ItemsCollectionBef));
            //Assert.That(isGameSeriesAft, Is.Not.SameAs(isGameSeriesBef));
            //Assert.That(hsAWSeriesAft, Is.Not.SameAs(isAWSeriesBef));
            //Assert.That(gameEnhanceMode_IsEnableAft, Is.Not.SameAs(gameEnhanceMode_IsEnableBef));
            //Assert.That(gameEnhanceMode_OpacityAft, Is.Not.SameAs(gameEnhanceMode_OpacityBef));
            //Assert.That(responseTime_IsEnableAft, Is.Not.SameAs(responseTime_IsEnableBef));
            //Assert.That(responseTime_OpacityAft, Is.Not.SameAs(responseTime_OpacityBef));
            //Assert.That(hDRType_IsEnableAft, Is.Not.SameAs(hDRType_IsEnableBef));
            //Assert.That(hDRType_OpacityAft, Is.Not.SameAs(hDRType_OpacityBef));
            //Assert.That(darkStabilizer_IsEnableAft, Is.Not.SameAs(darkStabilizer_IsEnableBef));
            //Assert.That(darkStabilizer_OpacityAft, Is.Not.SameAs(darkStabilizer_OpacityBef));
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