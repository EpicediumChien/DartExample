using DDPM.SA.Common;
using DDPM.UI.Common;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using Dell.Client.Framework.UX.WPF;
using Moq;
using NGA.UnitTest.PrivateObject;
using System.Windows;
using VcpCore.Common;

namespace DDPM.UI.Module.DisplayProperties.Tests
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public class DisplayPropertiesViewModelTests
    {
        private Mock<IModuleOwner>? moduleOwnerMock;
        private IModuleOwner? moduleOwner;
        private Mock<IDeviceManagerSA>? deviceManagerMock;
        private IDeviceManagerSA? deviceManagerSA;
        private HomeDevice? selectedHomeDevice;
        private DisplayPropertiesViewModel? displayPropertiesViewModel;
        private PrivateObject? privateObject;

        [SetUp]
        public void Setup()
        {
            if (System.Windows.Application.Current == null)
            {
                new System.Windows.Application();
            }
            var resourceDictionary = new ResourceDictionary();
            resourceDictionary.Source = new Uri("pack://application:,,,/DDPM.UI.Common;component/ModuleStyle.xaml");
            System.Windows.Application.Current.Resources.MergedDictionaries.Add(resourceDictionary);
            moduleOwnerMock = new Mock<IModuleOwner>();
            moduleOwner = moduleOwnerMock!.Object;
            DdpmCommonHelper.ModuleOwner = moduleOwner;

            deviceManagerMock = new Mock<IDeviceManagerSA>();
            deviceManagerSA = deviceManagerMock.Object;
            DdpmCommonHelper.DeviceManagerSA = deviceManagerSA;
            selectedHomeDevice = new HomeDevice();
            displayPropertiesViewModel = new DisplayPropertiesViewModel();
            privateObject = new PrivateObject(displayPropertiesViewModel);
        }

        [Test]
        public void TestModuleOwner()
        {
            displayPropertiesViewModel.ModuleOwner = moduleOwner;
            Assert.That(displayPropertiesViewModel.ModuleOwner, Is.EqualTo(moduleOwner));
        }

        [Test]
        public void TestMyModule()
        {
            var myDisplayModule = new DisplayPropertiesModule();
            displayPropertiesViewModel.MyModule = myDisplayModule;
            Assert.That(displayPropertiesViewModel.MyModule, Is.EqualTo(myDisplayModule));
        }

        [Test]
        public void TestResolution_ItemsCollection()
        {
            var myResolution_ItemsCollection = new List<UI_Properties>();
            displayPropertiesViewModel.Resolution_ItemsCollection = myResolution_ItemsCollection;
            Assert.That(displayPropertiesViewModel.Resolution_ItemsCollection, Is.EqualTo(myResolution_ItemsCollection));
        }

        [Test]
        public void TestHDRStatus_String()
        {
            var result = displayPropertiesViewModel!.HDRStatus_String;
            Assert.That(result, Is.EqualTo("OFF"));

            deviceManagerMock.Setup(x => x.GetDisplayPropertiesInfo(It.IsAny<MonitorInfo>())).Returns(Task.FromResult(new DisplayPropertiesInfo()));
            moduleOwnerMock.Setup(x => x.SelectedHomeDevice).Returns(new HomeDevice());
            displayPropertiesViewModel.MyModule = new DisplayPropertiesModule();
            displayPropertiesViewModel.MyModule.SelectedHomeDevice = selectedHomeDevice;
            var MyConsoleMock = new Mock<IConsole>();
            var myConsole = MyConsoleMock.Object;
            DdpmCommonHelper.MyConsole = myConsole;
            var monitorInfo = new MonitorInfo();
            displayPropertiesViewModel.MyModule.SelectedHomeDevice.MonitorInfo = monitorInfo;

            displayPropertiesViewModel.HDRStatus = true;
            result = displayPropertiesViewModel!.HDRStatus_String;
            Assert.That(result, Is.EqualTo("ON"));
        }

        [Test]
        public void TestSupportedHDR()
        {
            var result = displayPropertiesViewModel!.SupportedHDR.ToString();
            Assert.That(result, Is.EqualTo("Collapsed"));

            privateObject.SetFieldOrProperty("_SupportedHDR", true);
            result = displayPropertiesViewModel!.SupportedHDR.ToString();
            Assert.That(result, Is.EqualTo("Visible"));
        }

        [Test]
        public void TestSupportedUSBCPrioeitization()
        {
            var result = displayPropertiesViewModel!.SupportedUSBCPrioeitization.ToString();
            Assert.That(result, Is.EqualTo("Collapsed"));

            privateObject.SetFieldOrProperty("_SupportedUSBCPrioeitization", true);
            result = displayPropertiesViewModel!.SupportedUSBCPrioeitization.ToString();
            Assert.That(result, Is.EqualTo("Visible"));
        }

        [Test]
        public void TestSelectedResolution()
        {
            deviceManagerMock.Setup(x => x.SetDisplayPropertiest(It.IsAny<MonitorInfo>(), It.IsAny<Properties>(), It.IsAny<DisplayOrientation>())).Returns(Task.FromResult(true));

            privateObject.SetFieldOrProperty("_selectedResolution", new UI_Properties());

            moduleOwnerMock.Setup(x => x.SelectedHomeDevice).Returns(new HomeDevice());
            displayPropertiesViewModel.MyModule = new DisplayPropertiesModule();
            displayPropertiesViewModel.MyModule.SelectedHomeDevice = selectedHomeDevice;

            var mySelectedResolution = new UI_Properties();
            var monitorInfo = new MonitorInfo();
            monitorInfo.DisplayName = "123";
            displayPropertiesViewModel.MyModule.SelectedHomeDevice.MonitorInfo = monitorInfo;
            var mySelectedOrientation = new UI_Orientation();

            displayPropertiesViewModel.SelectedOrientation = mySelectedOrientation;
            displayPropertiesViewModel.SelectedResolution = mySelectedResolution;

            Assert.That(displayPropertiesViewModel.SelectedResolution, Is.EqualTo(mySelectedResolution));
        }

        [Test]
        public void TestOrientation_ItemsCollection()
        {
            var orientation_ItemsCollection = new DisplayPropertiesViewModel();
            var myOrientation_ItemsCollection = new List<UI_Orientation>();
            orientation_ItemsCollection.Orientation_ItemsCollection = myOrientation_ItemsCollection;
            Assert.That(orientation_ItemsCollection.Orientation_ItemsCollection, Is.EqualTo(myOrientation_ItemsCollection));
        }

        [Test]
        public void TestSelectedOrientation()
        {
            deviceManagerMock.Setup(x => x.SetDisplayPropertiest(It.IsAny<MonitorInfo>(), It.IsAny<Properties>(), It.IsAny<DisplayOrientation>())).Returns(Task.FromResult(true));
            privateObject.SetFieldOrProperty("_selectedOrientation", new UI_Orientation());

            moduleOwnerMock.Setup(x => x.SelectedHomeDevice).Returns(new HomeDevice());
            displayPropertiesViewModel.MyModule = new DisplayPropertiesModule();
            displayPropertiesViewModel.MyModule.SelectedHomeDevice = selectedHomeDevice;

            var mySelectedResolution = new UI_Properties();
            var monitorInfo = new MonitorInfo();
            monitorInfo.DisplayName = "123";
            displayPropertiesViewModel.MyModule.SelectedHomeDevice.MonitorInfo = monitorInfo;
            var mySelectedOrientation = new UI_Orientation();

            displayPropertiesViewModel.SelectedResolution = mySelectedResolution;
            displayPropertiesViewModel.SelectedOrientation = mySelectedOrientation;

            Assert.That(displayPropertiesViewModel.SelectedOrientation, Is.EqualTo(mySelectedOrientation));
        }

        [Test]
        public void TestHDRStatus()
        {
            deviceManagerMock.Setup(x => x.GetDisplayPropertiesInfo(It.IsAny<MonitorInfo>())).Returns(Task.FromResult(new DisplayPropertiesInfo()));
            moduleOwnerMock.Setup(x => x.SelectedHomeDevice).Returns(new HomeDevice());
            displayPropertiesViewModel.MyModule = new DisplayPropertiesModule();
            displayPropertiesViewModel.MyModule.SelectedHomeDevice = selectedHomeDevice;
            var MyConsoleMock = new Mock<IConsole>();
            var myConsole = MyConsoleMock.Object;
            DdpmCommonHelper.MyConsole = myConsole;
            var monitorInfo = new MonitorInfo();
            displayPropertiesViewModel.MyModule.SelectedHomeDevice.MonitorInfo = monitorInfo;
            bool myHDRStatus = true;
            displayPropertiesViewModel.HDRStatus = myHDRStatus;

            Assert.That(displayPropertiesViewModel.HDRStatus, Is.EqualTo(myHDRStatus));
        }

        [Test]
        public void TestIsHighDataSpeed()
        {
            privateObject.SetFieldOrProperty("_IsHighDataSpeed", true);

            moduleOwnerMock.Setup(x => x.SelectedHomeDevice).Returns(new HomeDevice());
            displayPropertiesViewModel.MyModule = new DisplayPropertiesModule();
            displayPropertiesViewModel.MyModule.SelectedHomeDevice = selectedHomeDevice;
            var monitorInfo = new MonitorInfo();
            displayPropertiesViewModel.MyModule.SelectedHomeDevice.MonitorInfo = monitorInfo;

            bool myIsHighDataSpeed = true;
            displayPropertiesViewModel.IsHighDataSpeed = myIsHighDataSpeed;

            Assert.That(displayPropertiesViewModel.IsHighDataSpeed, Is.EqualTo(myIsHighDataSpeed));
        }

        [Test]
        public void TestIsHighResolution()
        {
            deviceManagerMock.Setup(x => x.SetUSBCPrioritizationType(It.IsAny<MonitorInfo>(), It.IsAny<USBCPrioritizationType>())).Returns(Task.FromResult(true));
            privateObject.SetFieldOrProperty("_IsHighResolution", true);

            moduleOwnerMock.Setup(x => x.SelectedHomeDevice).Returns(new HomeDevice());
            displayPropertiesViewModel.MyModule = new DisplayPropertiesModule();
            displayPropertiesViewModel.MyModule.SelectedHomeDevice = selectedHomeDevice;
            var monitorInfo = new MonitorInfo();
            displayPropertiesViewModel.MyModule.SelectedHomeDevice.MonitorInfo = monitorInfo;
            bool my_IsHighResolution = true;
            displayPropertiesViewModel.IsHighResolution = my_IsHighResolution;

            Assert.That(displayPropertiesViewModel.IsHighResolution, Is.EqualTo(my_IsHighResolution));
        }

        [Test]
        public void TestIsBusy()
        {
            bool myIsBusy = true;
            displayPropertiesViewModel.IsBusy = myIsBusy;

            Assert.That(displayPropertiesViewModel.IsBusy, Is.EqualTo(myIsBusy));
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

            deviceManagerMock.Setup(x => x.GetDisplayPropertiesInfo(It.IsAny<MonitorInfo>())).Returns(Task.FromResult(new DisplayPropertiesInfo()));
            moduleOwnerMock.Setup(x => x.SelectedHomeDevice).Returns(new HomeDevice());
            displayPropertiesViewModel.MyModule = new DisplayPropertiesModule();
            displayPropertiesViewModel.MyModule.SelectedHomeDevice = selectedHomeDevice;
            var MyConsoleMock = new Mock<IConsole>();
            var myConsole = MyConsoleMock.Object;
            DdpmCommonHelper.MyConsole = myConsole;
            var monitorInfo = new MonitorInfo();

            var mySelectedResolution = new UI_Properties();
            monitorInfo.DisplayName = "123";
            displayPropertiesViewModel.MyModule.SelectedHomeDevice.MonitorInfo = monitorInfo;
            var mySelectedOrientation = new UI_Orientation();
            var myresolution_ItemsCollection = new List<UI_Properties>();
            displayPropertiesViewModel.Resolution_ItemsCollection = myresolution_ItemsCollection;
            var myOrientation_ItemsCollection = new List<UI_Orientation>();
            displayPropertiesViewModel.Orientation_ItemsCollection = myOrientation_ItemsCollection;
            var myHDRStatus = true;
            displayPropertiesViewModel.HDRStatus = myHDRStatus;
            var myisHighDataSpeed = true;
            displayPropertiesViewModel.IsHighDataSpeed = myisHighDataSpeed;
            var myIsHighResolution = true;
            displayPropertiesViewModel.IsHighResolution = myIsHighResolution;
            privateObject.SetFieldOrProperty("_SupportedHDR", true);
            privateObject.SetFieldOrProperty("_SupportedUSBCPrioeitization", true);

            displayPropertiesViewModel.SelectedOrientation = mySelectedOrientation;
            displayPropertiesViewModel.SelectedResolution = mySelectedResolution;

            displayPropertiesViewModel.RefreshUI();
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
            DisplayPropertiesViewModel displayPropertiesViewModel = new DisplayPropertiesViewModel();
            // Assert
            Assert.That(displayPropertiesViewModel, Is.Not.Null);
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

        [Test]
        public void TestOrientation()
        {
            UI_Orientation uI_Orientation = new UI_Orientation();
            var myUI_Orientation = new DisplayOrientation();
            uI_Orientation.Orientation = myUI_Orientation;
            // Assert
            Assert.That(uI_Orientation.Orientation, Is.EqualTo(myUI_Orientation));
        }

        [Test]
        public void TestUI_ODisplayText()
        {
            UI_Orientation uI_Orientation = new UI_Orientation();
            var result = uI_Orientation.DisplayText;
            // Assert
            Assert.That(result, Is.Not.Null);
        }
    }
}