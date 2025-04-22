using DDPM.SA.Common;
using DDPM.UI.Common;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using Moq;
using NGA.UnitTest.PrivateObject;
using System.Windows;
using VcpCore.Common;
using static DDPM.UI.Module.Gaming.UI_HDRType;
using Dell.Client.Framework.UX.WPF.ResourceManager;
using System.Globalization;
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
            if (System.Windows.Application.Current == null)
            {
                new System.Windows.Application();
            }
            ResourceManager res = new ResourceManager();
            var resourceDictionary = new ResourceDictionary();
            resourceDictionary.Source = new Uri("pack://application:,,,/DDPM.UI.Common;component/ModuleStyle.xaml");
            System.Windows.Application.Current.Resources.MergedDictionaries.Add(resourceDictionary);
            moduleOwnerMock = new Mock<IModuleOwner>();
            moduleOwnerMock.Setup(m => m.SelectedHomeDevice).Returns(new HomeDevice() { MonitorInfo=new MonitorInfo()});
            moduleOwner = moduleOwnerMock!.Object;
            DdpmCommonHelper.ModuleOwner = moduleOwner;


            deviceManagerMock = new Mock<IDeviceManagerSA>();
            deviceManagerSA = deviceManagerMock.Object;
            deviceManagerMock.Setup(x => x.GetGamingProperties_SupportedList(It.IsAny<MonitorInfo>())).Returns(Task.FromResult(new GamingDisplayPropertiesInfo()));
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
        public void TestGameEnhanceMode_ItemsCollection()
        {
            var gameEnhanceMode_ItemsCollection = new List<UI_GameEnhancementMode>();
            gamingViewModel.GameEnhanceMode_ItemsCollection = gameEnhanceMode_ItemsCollection;
            Assert.That(gamingViewModel.GameEnhanceMode_ItemsCollection, Is.EqualTo(gameEnhanceMode_ItemsCollection));
        }

        [Test]
        public void TestSelectedGameEnhancementMode()
        {
            var selectedGameEnhancementMode = new UI_GameEnhancementMode();
            gamingViewModel.SelectedGameEnhancementMode = selectedGameEnhancementMode;
            Assert.That(gamingViewModel.SelectedGameEnhancementMode, Is.EqualTo(selectedGameEnhancementMode));
        }

        [Test]
        public void TestResponseTime_ItemsCollection()
        {
            var responseTime_ItemsCollection = new List<UI_ResponseTime>();
            gamingViewModel.ResponseTime_ItemsCollection = responseTime_ItemsCollection;
            Assert.That(gamingViewModel.ResponseTime_ItemsCollection, Is.EqualTo(responseTime_ItemsCollection));
        }

        [Test]
        public void TestSelectedResponseTime()
        {
            var selectedResponseTime = new UI_ResponseTime();
            gamingViewModel.SelectedResponseTime = selectedResponseTime;
            Assert.That(gamingViewModel.SelectedResponseTime, Is.EqualTo(selectedResponseTime));
        }

        [Test]
        public void TestDarkStabilizer_ItemsCollection()
        {
            var darkStabilizer_ItemsCollection = new List<UI_DarkStabilizer>();
            gamingViewModel.DarkStabilizer_ItemsCollection = darkStabilizer_ItemsCollection;
            Assert.That(gamingViewModel.DarkStabilizer_ItemsCollection, Is.EqualTo(darkStabilizer_ItemsCollection));
        }

        [Test]
        public void TestSelectedDarkStabilizer()
        {
            var selectedDarkStabilizer = new UI_DarkStabilizer();
            gamingViewModel.SelectedDarkStabilizer = selectedDarkStabilizer;
            Assert.That(gamingViewModel.SelectedDarkStabilizer, Is.EqualTo(selectedDarkStabilizer));
        }

        [Test]
        public void TestHDRType_ItemsCollection()
        {
            var HDRType_ItemsCollection = new List<UI_HDRType>();
            gamingViewModel.HDRType_ItemsCollection = HDRType_ItemsCollection;
            Assert.That(gamingViewModel.HDRType_ItemsCollection, Is.EqualTo(HDRType_ItemsCollection));
        }

        [Test]
        public void TestSelectedHDRType()
        {
            var selectedHDRType = new UI_HDRType();
            gamingViewModel.SelectedHDRType = selectedHDRType;
            Assert.That(gamingViewModel.SelectedHDRType, Is.EqualTo(selectedHDRType));
        }

        [Test]
        public void TestDualResolution_ItemsCollection()
        {
            var dualResolution_ItemsCollection = new List<UI_DualResolution>();
            gamingViewModel.DualResolution_ItemsCollection = dualResolution_ItemsCollection;
            Assert.That(gamingViewModel.DualResolution_ItemsCollection, Is.EqualTo(dualResolution_ItemsCollection));
        }

        [Test]
        public void TestSelectedDualResolution()
        {
            var selectedDualResolution = new UI_DualResolution();
            gamingViewModel.SelectedDualResolution = selectedDualResolution;
            Assert.That(gamingViewModel.SelectedDualResolution, Is.EqualTo(selectedDualResolution));
        }

        [Test]
        public void TestIsGameSeries()
        {
            var isGameSeries = Visibility.Visible;
            gamingViewModel.IsGameSeries = isGameSeries;
            Assert.That(gamingViewModel.IsGameSeries, Is.EqualTo(Visibility.Visible));
        }

        [Test]
        public void TestIsAWSeries()
        {
            var isAWSeries = Visibility.Visible;
            gamingViewModel.IsAWSeries = isAWSeries;
            Assert.That(gamingViewModel.IsAWSeries, Is.EqualTo(Visibility.Visible));
        }

        [Test]
        public void TestGameEnhanceMode_IsEnable()
        {
            gamingViewModel.GameEnhanceMode_IsEnable = true;
            Assert.That(gamingViewModel.GameEnhanceMode_IsEnable, Is.EqualTo(true));
        }

        [Test]
        public void TestGameEnhanceMode_Opacity()
        {
            var result = gamingViewModel.GameEnhanceMode_Opacity;
            Assert.That(result, Is.EqualTo("0.5"));

            gamingViewModel.GameEnhanceMode_IsEnable = true;
            result = gamingViewModel.GameEnhanceMode_Opacity;
            Assert.That(result, Is.EqualTo("1.0"));
        }
      
        [Test]
        public void TestResponseTime_IsEnable()
        {
            gamingViewModel.ResponseTime_IsEnable = true;
            Assert.That(gamingViewModel.ResponseTime_IsEnable, Is.EqualTo(true));
        }

        [Test]
        public void TestResponseTime_Opacity()
        {
            var result = gamingViewModel.ResponseTime_Opacity;
            Assert.That(result, Is.EqualTo("0.5"));

            gamingViewModel.ResponseTime_IsEnable = true;
            result = gamingViewModel.ResponseTime_Opacity;
            Assert.That(result, Is.EqualTo("1.0"));
        }

        [Test]
        public void TestHDRType_IsEnable()
        {
            gamingViewModel.HDRType_IsEnable = true;
            Assert.That(gamingViewModel.HDRType_IsEnable, Is.EqualTo(true));
        }

        [Test]
        public void TestHDRType_Opacity()
        {
            var result = gamingViewModel.HDRType_Opacity;
            Assert.That(result, Is.EqualTo("0.5"));

            gamingViewModel.HDRType_IsEnable = true;
            result = gamingViewModel.HDRType_Opacity;
            Assert.That(result, Is.EqualTo("1.0"));
        }

        [Test]
        public void TestDarkStabilizer_IsEnable()
        {
            gamingViewModel.DarkStabilizer_IsEnable = true;
            Assert.That(gamingViewModel.DarkStabilizer_IsEnable, Is.EqualTo(true));
        }

        [Test]
        public void TestDarkStabilizer_Opacity()
        {
            var result = gamingViewModel.DarkStabilizer_Opacity;
            Assert.That(result, Is.EqualTo("0.5"));

            gamingViewModel.DarkStabilizer_IsEnable = true;
            result = gamingViewModel.DarkStabilizer_Opacity;
            Assert.That(result, Is.EqualTo("1.0"));
        }

        [Test]
        public void TestIsSupported_GameEnhanceMode()
        {
            var isSupported_GameEnhanceMode = Visibility.Visible;
            gamingViewModel.IsSupported_GameEnhanceMode = isSupported_GameEnhanceMode;
            Assert.That(gamingViewModel.IsSupported_GameEnhanceMode, Is.EqualTo(Visibility.Visible));
        }

        [Test]
        public void TestIsSupported_ResponseTime()
        {
            var isSupported_ResponseTime = Visibility.Visible;
            gamingViewModel.IsSupported_ResponseTime = isSupported_ResponseTime;
            Assert.That(gamingViewModel.IsSupported_ResponseTime, Is.EqualTo(Visibility.Visible));
        }

        [Test]
        public void TestIsSupported_DarkStabilizer()
        {
            var isSupported_DarkStabilizer = Visibility.Visible;
            gamingViewModel.IsSupported_DarkStabilizer = isSupported_DarkStabilizer;
            Assert.That(gamingViewModel.IsSupported_DarkStabilizer, Is.EqualTo(Visibility.Visible));
        }

        [Test]
        public void TestIsSupported_DualResolution()
        {
            var isSupported_DualResolution = Visibility.Visible;
            gamingViewModel.IsSupported_DualResolution = isSupported_DualResolution;
            Assert.That(gamingViewModel.IsSupported_DualResolution, Is.EqualTo(Visibility.Visible));
        }




        [Test]
        public void TestIsBusy()
        {
            bool myIsBusy = true;
            gamingViewModel.IsBusy = myIsBusy;

            Assert.That(gamingViewModel.IsBusy, Is.EqualTo(myIsBusy));
        }

        [Test]
        public void TestGamingParamChang()
        {
            var e = new GamingDisplayPropertiesInfo();
            gamingViewModel.GameEnhanceMode_ItemsCollection = new List<UI_GameEnhancementMode>() { new UI_GameEnhancementMode() { GameEnhancementMode = Gaming_GameEnhancementMode.Disable } };
            try
            {
                gamingViewModel.GamingParamChang(null,e);
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }
        

        [Test]
        public void TestRefreshUI()
        {
            var selectedResolutionBef = privateObject.GetFieldOrProperty("SelectedResolution");
            var selectedOrientationBef = privateObject.GetFieldOrProperty("SelectedGameEnhancementMode");
            var selectedResponseTimeBef = privateObject.GetFieldOrProperty("SelectedResponseTime");
            var selectedDarkStabilizerBef = privateObject.GetFieldOrProperty("SelectedDarkStabilizer");
            var selectedHDRTypeBef = privateObject.GetFieldOrProperty("SelectedHDRType");
            var selectedDualResolutionBef = privateObject.GetFieldOrProperty("_selectedDualResolution");
            var resolution_ItemsCollectionBef = gamingViewModel.Resolution_ItemsCollection;
            var gameEnhanceMode_ItemsCollectionBef = gamingViewModel.GameEnhanceMode_ItemsCollection;
            var responseTime_ItemsCollectionBef = gamingViewModel.ResponseTime_ItemsCollection;
            var darkStabilizer_ItemsCollectionBef = gamingViewModel.DarkStabilizer_ItemsCollection;

            var hDRType_ItemsCollectionBef = gamingViewModel.HDRType_ItemsCollection;
            var dualResolution_ItemsCollectionBef = gamingViewModel.DualResolution_ItemsCollection;
            var isGameSeriesBef = gamingViewModel.IsGameSeries;
            var isAWSeriesBef = gamingViewModel.IsAWSeries;
            var gameEnhanceMode_IsEnableBef = gamingViewModel.GameEnhanceMode_IsEnable;
            var gameEnhanceMode_OpacityBef = gamingViewModel.GameEnhanceMode_Opacity;
            var responseTime_IsEnableBef = gamingViewModel.ResponseTime_IsEnable;
            var responseTime_OpacityBef = gamingViewModel.ResponseTime_Opacity;
            var hDRType_IsEnableBef = gamingViewModel.HDRType_IsEnable;
            var hDRType_OpacityBef = gamingViewModel.HDRType_Opacity;

            var darkStabilizer_IsEnableBef = gamingViewModel.DarkStabilizer_IsEnable;
            var darkStabilizer_OpacityBef = gamingViewModel.DarkStabilizer_Opacity;
            var isSupported_GameEnhanceModeBef = gamingViewModel.IsSupported_GameEnhanceMode;
            var isSupported_ResponseTimeBef = gamingViewModel.IsSupported_ResponseTime;
            var isSupported_DarkStabilizerBef = gamingViewModel.IsSupported_DarkStabilizer;
            var isSupported_DualResolutionBef = gamingViewModel.IsSupported_DualResolution;

            privateObject.SetFieldOrProperty("_selectedResolution", new UI_Properties());
            privateObject.SetFieldOrProperty("_selectedGameEnhancementMode", new UI_GameEnhancementMode());
            privateObject.SetFieldOrProperty("_selectedResponseTime", new UI_ResponseTime());
            privateObject.SetFieldOrProperty("_selectedDarkStabilizer", new UI_DarkStabilizer());
            privateObject.SetFieldOrProperty("_selectedHDRType", new UI_HDRType());
            privateObject.SetFieldOrProperty("_selectedDualResolution", new UI_DualResolution());
            gamingViewModel.Resolution_ItemsCollection = new List<UI_Properties>();
            gamingViewModel.GameEnhanceMode_ItemsCollection=new List<UI_GameEnhancementMode>();
            gamingViewModel.ResponseTime_ItemsCollection=new List<UI_ResponseTime>();
            gamingViewModel.DarkStabilizer_ItemsCollection = new List<UI_DarkStabilizer>();

            gamingViewModel.HDRType_ItemsCollection=new List<UI_HDRType>();
            gamingViewModel.DualResolution_ItemsCollection=new List<UI_DualResolution>();
            gamingViewModel.IsGameSeries = Visibility.Visible;
            gamingViewModel.IsAWSeries = Visibility.Visible;
            gamingViewModel.GameEnhanceMode_IsEnable=true;
            //gamingViewModel.GameEnhanceMode_Opacity;
            gamingViewModel.ResponseTime_IsEnable=true;
            //gamingViewModel.ResponseTime_Opacity;
            gamingViewModel.HDRType_IsEnable=true;
            //gamingViewModel.HDRType_Opacity;

            gamingViewModel.DarkStabilizer_IsEnable = true;
            //gamingViewModel.DarkStabilizer_Opacity;
            gamingViewModel.IsSupported_GameEnhanceMode = Visibility.Visible;
            gamingViewModel.IsSupported_ResponseTime=Visibility.Visible;
            gamingViewModel.IsSupported_DarkStabilizer=Visibility.Visible;
            gamingViewModel.IsSupported_DualResolution=Visibility.Visible;

            var selectedResolutionAft = privateObject.GetFieldOrProperty("SelectedResolution");
            var selectedGameEnhancementModeAft = privateObject.GetFieldOrProperty("SelectedGameEnhancementMode");
            var selectedResponseTimeAft = privateObject.GetFieldOrProperty("SelectedResponseTime");
            var selectedDarkStabilizerAft = privateObject.GetFieldOrProperty("SelectedDarkStabilizer");
            var selectedHDRTypeAft = privateObject.GetFieldOrProperty("SelectedHDRType");
            var selectedDualResolutionAft = privateObject.GetFieldOrProperty("SelectedDualResolution");
            var resolution_ItemsCollectionAft = gamingViewModel.Resolution_ItemsCollection;
            var gameEnhanceMode_ItemsCollectionAft = gamingViewModel.GameEnhanceMode_ItemsCollection;
            var responseTime_ItemsCollectionAft = gamingViewModel.ResponseTime_ItemsCollection;
            var darkStabilizer_ItemsCollectionAft = gamingViewModel.DarkStabilizer_ItemsCollection;

            var hDRType_ItemsCollectionAft = gamingViewModel.HDRType_ItemsCollection;
            var dualResolution_ItemsCollectionAft = gamingViewModel.DualResolution_ItemsCollection ;
            var isGameSeriesAft = gamingViewModel.IsGameSeries;
            var isAWSeriesAft = gamingViewModel.IsAWSeries;
            var gameEnhanceMode_IsEnableAft = gamingViewModel.GameEnhanceMode_IsEnable ;
            var gameEnhanceMode_OpacityAft = gamingViewModel.GameEnhanceMode_Opacity;
            var responseTime_IsEnableAft = gamingViewModel.ResponseTime_IsEnable;
            var responseTime_OpacityAft = gamingViewModel.ResponseTime_Opacity;
            var hDRType_IsEnableAft = gamingViewModel.HDRType_IsEnable ;
            var hDRType_OpacityAft = gamingViewModel.HDRType_Opacity;

            var darkStabilizer_IsEnableAft = gamingViewModel.DarkStabilizer_IsEnable;
            var darkStabilizer_OpacityAft = gamingViewModel.DarkStabilizer_Opacity;
            var isSupported_GameEnhanceModeAft = gamingViewModel.IsSupported_GameEnhanceMode ;
            var isSupported_ResponseTimeAft = gamingViewModel.IsSupported_ResponseTime ;
            var isSupported_DarkStabilizerAft = gamingViewModel.IsSupported_DarkStabilizer ;
            var isSupported_DualResolutionAft = gamingViewModel.IsSupported_DualResolution;

            gamingViewModel.RefreshUI();

            Assert.That(selectedResolutionAft, Is.Not.SameAs(selectedResolutionBef));
            Assert.That(selectedGameEnhancementModeAft, Is.Not.SameAs(selectedOrientationBef));
            Assert.That(selectedResponseTimeAft, Is.Not.SameAs(selectedResponseTimeBef));
            Assert.That(selectedDarkStabilizerAft, Is.Not.SameAs(selectedDarkStabilizerBef));
            Assert.That(selectedHDRTypeAft, Is.Not.SameAs(selectedHDRTypeBef));
            Assert.That(selectedDualResolutionAft, Is.Not.SameAs(resolution_ItemsCollectionBef));
            Assert.That(resolution_ItemsCollectionAft, Is.Not.SameAs(selectedResolutionBef));
            Assert.That(gameEnhanceMode_ItemsCollectionAft, Is.Not.SameAs(gameEnhanceMode_ItemsCollectionBef));
            Assert.That(responseTime_ItemsCollectionAft, Is.Not.SameAs(responseTime_ItemsCollectionBef));
            Assert.That(darkStabilizer_ItemsCollectionAft, Is.Not.SameAs(darkStabilizer_ItemsCollectionBef));

            Assert.That(hDRType_ItemsCollectionAft, Is.Not.SameAs(hDRType_ItemsCollectionBef));
            Assert.That(dualResolution_ItemsCollectionAft, Is.Not.SameAs(dualResolution_ItemsCollectionBef));
            Assert.That(isGameSeriesAft, Is.Not.EqualTo(isGameSeriesBef));
            Assert.That(isAWSeriesAft, Is.Not.EqualTo(isAWSeriesBef));
            Assert.That(gameEnhanceMode_IsEnableAft, Is.Not.EqualTo(gameEnhanceMode_IsEnableBef));
            Assert.That(gameEnhanceMode_OpacityAft, Is.Not.SameAs(gameEnhanceMode_OpacityBef));
            Assert.That(responseTime_IsEnableAft, Is.Not.EqualTo(responseTime_IsEnableBef));
            Assert.That(responseTime_OpacityAft, Is.Not.SameAs(responseTime_OpacityBef));
            Assert.That(hDRType_IsEnableAft, Is.Not.EqualTo(hDRType_IsEnableBef));
            Assert.That(hDRType_OpacityAft, Is.Not.SameAs(hDRType_OpacityBef));

            Assert.That(darkStabilizer_IsEnableAft, Is.Not.EqualTo(darkStabilizer_IsEnableBef));
            Assert.That(darkStabilizer_OpacityAft, Is.Not.SameAs(darkStabilizer_OpacityBef));
            Assert.That(isSupported_GameEnhanceModeAft, Is.Not.EqualTo(isSupported_GameEnhanceModeBef));
            Assert.That(isSupported_ResponseTimeAft, Is.Not.EqualTo(isSupported_ResponseTimeBef));
            Assert.That(isSupported_DarkStabilizerAft, Is.Not.EqualTo(isSupported_DarkStabilizerBef));
            Assert.That(isSupported_DualResolutionAft, Is.Not.EqualTo(isSupported_DualResolutionBef));

        }

        [Test]
        public void TestConstructor_InitializesComponent()
        {
            // Assert
            Assert.That(gamingViewModel, Is.Not.Null);
        }

        //class UI_Properties
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

        //class UI_GameEnhancementMode
        [Test]
        public void TestGameEnhancementMode()
        {
            UI_GameEnhancementMode uI_GameEnhancementMode = new UI_GameEnhancementMode();
            var gameEnhancementMode = new Gaming_GameEnhancementMode();
            uI_GameEnhancementMode.GameEnhancementMode = gameEnhancementMode;
            // Assert
            Assert.That(uI_GameEnhancementMode.GameEnhancementMode, Is.EqualTo(gameEnhancementMode));
        }

        [Test]
        public void TestDisplayText()
        {
            if (CultureInfo.CurrentCulture.Name == "es-US")
            {
                UI_GameEnhancementMode uI_GameEnhancementMode = new UI_GameEnhancementMode();
                uI_GameEnhancementMode.GameEnhancementMode = new Gaming_GameEnhancementMode();
                var result = uI_GameEnhancementMode.DisplayText;
                // Assert
                Assert.That(result, Is.EqualTo("OFF"));
            }
        }

        //class UI_ResponseTime
        [Test]
        public void TestResponseTime()
        {
            UI_ResponseTime uI_ResponseTime = new UI_ResponseTime();
            var responseTime = new Gaming_ResponseTime();
            uI_ResponseTime.ResponseTime = responseTime;
            // Assert
            Assert.That(uI_ResponseTime.ResponseTime, Is.EqualTo(responseTime));
        }

       [Test]
        public void TestDisplayTextx()
        {
            if (CultureInfo.CurrentCulture.Name == "es-US")
            {
                UI_ResponseTime uI_ResponseTime = new UI_ResponseTime();
                uI_ResponseTime.ResponseTime = new Gaming_ResponseTime();
                var result = uI_ResponseTime.DisplayText;
                // Assert
                Assert.That(result, Is.EqualTo("Extreme"));
            }
        }

        //class UI_DarkStabilizer
        [Test]
        public void TestDarkStabilizer()
        {
            UI_DarkStabilizer uI_DarkStabilizer = new UI_DarkStabilizer();
            var darkStabilizer = new Gaming_DarkStabilizer();
            uI_DarkStabilizer.DarkStabilizer = darkStabilizer;
            // Assert
            Assert.That(uI_DarkStabilizer.DarkStabilizer, Is.EqualTo(darkStabilizer));
        }

        [Test]
        public void TestDisplayTexty()
        {
            if (CultureInfo.CurrentCulture.Name == "es-US")
            {
                UI_DarkStabilizer uI_DarkStabilizer = new UI_DarkStabilizer();
                uI_DarkStabilizer.DarkStabilizer = new Gaming_DarkStabilizer();
                var result = uI_DarkStabilizer.DisplayText;
                // Assert
                Assert.That(result, Is.EqualTo("Level 0"));
            }
        }

        //class UI_HDRType
        [Test]
        public void TestHDRType()
        {
            UI_HDRType uI_HDRType = new UI_HDRType();
            var hDRType = new Gaming_HDRType();
            uI_HDRType.HDRType = hDRType;
            // Assert
            Assert.That(uI_HDRType.HDRType, Is.EqualTo(hDRType));
        }

        [Test]
        public void TestDisplayTextq()
        {
            if (CultureInfo.CurrentCulture.Name == "es-US")
            {
                UI_HDRType uI_HDRType = new UI_HDRType();
                uI_HDRType.HDRType = new Gaming_HDRType();
                var result = uI_HDRType.DisplayText;
                // Assert
                Assert.That(result, Is.EqualTo("OFF"));
            }
        }

        //class UI_DualResolution
        [Test]
        public void TestDualResolutionType()
        {
            UI_DualResolution uI_DualResolution = new UI_DualResolution();
            var dualResolutionType = new Gaming_DualResolutionType();
            uI_DualResolution.DualResolutionType = dualResolutionType;
            // Assert
            Assert.That(uI_DualResolution.DualResolutionType, Is.EqualTo(dualResolutionType));
        }

        [Test]
        public void TestDisplayTextz()
        {
            UI_DualResolution uI_DualResolution = new UI_DualResolution();
            uI_DualResolution.DualResolutionType = Gaming_DualResolutionType._4K;
            var result = uI_DualResolution.DisplayText;
            // Assert
            Assert.That(result, Is.EqualTo("4K"));
        }
    }
}