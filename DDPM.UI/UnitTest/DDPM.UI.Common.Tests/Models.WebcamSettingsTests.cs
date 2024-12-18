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
    public class WebcamSettingsTests
    {
        private WebcamSettings? webcamSettings;

        [SetUp]
        public void Setup()
        {
            var resolutions = new Dictionary<string, string>();
            resolutions.Add("1", "a");
            resolutions.Add("2", "b");
            var selectedFPSs = new Dictionary<string, string>();
            selectedFPSs.Add("1", "a");
            selectedFPSs.Add("2", "b");
            webcamSettings = new WebcamSettings();
            webcamSettings.Resolutions = resolutions;
            webcamSettings.SelectedFPSs = selectedFPSs;
        }

        [Test]
        public void TestConstructor_WebcamSettings()
        {
            // Assert
            Assert.That(webcamSettings, Is.Not.Null);
        }

        [Test]
        public void TestExportWebcamSettings()
        {
            // Act
            var DeviceManagerSAMock = new Mock<IDeviceManagerSA>();
            DdpmCommonHelper.DeviceManagerSA = DeviceManagerSAMock.Object;
            DeviceManagerSAMock.Setup(x => x.WriteSerializedContentToFile(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(true));
            webcamSettings.SelectedResolution = "1";
            var result = WebcamSettings.ExportWebcamSettings(webcamSettings, "model");
            // Assert
            Assert.That(result, Is.EqualTo(true));
        }

        [Test]
        public void TestImportWebcamSettings()
        {
            // Act
            var DeviceManagerSAMock = new Mock<IDeviceManagerSA>();
            DdpmCommonHelper.DeviceManagerSA = DeviceManagerSAMock.Object;
            DeviceManagerSAMock.Setup(x => x.GetSupportedResolutions(It.IsAny<string>())).Returns(Task.FromResult(""));
            DeviceManagerSAMock.Setup(x => x.GetSelectedResolution(It.IsAny<string>())).Returns(Task.FromResult("{\"Resolution\":\"1280x720\",\"FPS\":[\"24\"]}"));
            var result = WebcamSettings.ImportWebcamSettings("model", new DeviceInfo() { SupportedResolutions="", ModelNumber = "WB5023",CustomProfiles=new Newtonsoft.Json.Linq.JArray(), PresetProfiles=new Newtonsoft.Json.Linq.JArray() });
            // Assert
            Assert.That(result, Is.Not.Null);
        }


        //class WebcamProfile
        [Test]
        public void TestId()
        {
            // Act
            var webcamProfile = new WebcamProfile();
            webcamProfile.Id = "Id";
            // Assert
            Assert.That(webcamProfile.Id, Is.EqualTo("Id"));
        }

        [Test]
        public void TestName()
        {
            // Act
            var webcamProfile = new WebcamProfile();
            webcamProfile.Name = "Name";
            // Assert
            Assert.That(webcamProfile.Name, Is.EqualTo("Name"));
        }

        [Test]
        public void TestDescription()
        {
            // Act
            var webcamProfile = new WebcamProfile();
            webcamProfile.Description = "Description";
            // Assert
            Assert.That(webcamProfile.Description, Is.EqualTo("Description"));
        }

        [Test]
        public void TestPriority()
        {
            // Act
            var webcamProfile = new WebcamProfile();
            webcamProfile.Priority = 1;
            // Assert
            Assert.That(webcamProfile.Priority, Is.EqualTo(1));
        }

        [Test]
        public void TestIsFocusOn()
        {
            // Act
            var webcamProfile = new WebcamProfile();
            webcamProfile.IsFocusOn = true;
            // Assert
            Assert.That(webcamProfile.IsFocusOn, Is.EqualTo(true));
        }

        [Test]
        public void TestFocus()
        {
            // Act
            var webcamProfile = new WebcamProfile();
            webcamProfile.Focus = 1;
            // Assert
            Assert.That(webcamProfile.Focus, Is.EqualTo(1));
        }

        [Test]
        public void TestPan()
        {
            // Act
            var webcamProfile = new WebcamProfile();
            webcamProfile.Pan = 1;
            // Assert
            Assert.That(webcamProfile.Pan, Is.EqualTo(1));
        }

        [Test]
        public void TestTilt()
        {
            // Act
            var webcamProfile = new WebcamProfile();
            webcamProfile.Tilt = 1;
            // Assert
            Assert.That(webcamProfile.Tilt, Is.EqualTo(1));
        }

        [Test]
        public void TestZoom()
        {
            // Act
            var webcamProfile = new WebcamProfile();
            //webcamProfile.Zoom = 1;
            //// Assert
            //Assert.That(webcamProfile.Zoom, Is.EqualTo(1));
        }

        [Test]
        public void TestBrightness()
        {
            // Act
            var webcamProfile = new WebcamProfile();
            webcamProfile.Brightness = 1;
            // Assert
            Assert.That(webcamProfile.Brightness, Is.EqualTo(1));
        }

        [Test]
        public void TestContrast()
        {
            // Act
            var webcamProfile = new WebcamProfile();
            webcamProfile.Contrast = 1;
            // Assert
            Assert.That(webcamProfile.Contrast, Is.EqualTo(1));
        }

        [Test]
        public void TestAntiFlicker()
        {
            // Act
            var webcamProfile = new WebcamProfile();
            webcamProfile.AntiFlicker = 1;
            // Assert
            Assert.That(webcamProfile.AntiFlicker, Is.EqualTo(1));
        }

        [Test]
        public void TestSaturation()
        {
            // Act
            var webcamProfile = new WebcamProfile();
            webcamProfile.Saturation = 1;
            // Assert
            Assert.That(webcamProfile.Saturation, Is.EqualTo(1));
        }

        [Test]
        public void TestSharpness()
        {
            // Act
            var webcamProfile = new WebcamProfile();
            webcamProfile.Sharpness = 1;
            // Assert
            Assert.That(webcamProfile.Sharpness, Is.EqualTo(1));
        }

        [Test]
        public void TestIsAutoWhiteBalanceOn()
        {
            // Act
            var webcamProfile = new WebcamProfile();
            webcamProfile.IsAutoWhiteBalanceOn = true;
            // Assert
            Assert.That(webcamProfile.IsAutoWhiteBalanceOn, Is.EqualTo(true));
        }

        [Test]
        public void TestAutoWhiteBalance()
        {
            // Act
            var webcamProfile = new WebcamProfile();
            webcamProfile.AutoWhiteBalance = 1;
            // Assert
            Assert.That(webcamProfile.AutoWhiteBalance, Is.EqualTo(1));
        }

        [Test]
        public void TestIsAutoFramingOn()
        {
            // Act
            var webcamProfile = new WebcamProfile();
            webcamProfile.IsAutoFramingOn = true;
            // Assert
            Assert.That(webcamProfile.IsAutoFramingOn, Is.EqualTo(true));
        }

        [Test]
        public void TestAutoFramingSensitivity()
        {
            // Act
            var webcamProfile = new WebcamProfile();
            webcamProfile.AutoFramingSensitivity = 1;
            // Assert
            Assert.That(webcamProfile.AutoFramingSensitivity, Is.EqualTo(1));
        }

        [Test]
        public void TestAutoFramingFrameSize()
        {
            // Act
            var webcamProfile = new WebcamProfile();
            webcamProfile.AutoFramingFrameSize = 1;
            // Assert
            Assert.That(webcamProfile.AutoFramingFrameSize, Is.EqualTo(1));
        }

        [Test]
        public void TestIsAutoFramingTransitionOn()
        {
            // Act
            var webcamProfile = new WebcamProfile();
            webcamProfile.IsAutoFramingTransitionOn = true;
            // Assert
            Assert.That(webcamProfile.IsAutoFramingTransitionOn, Is.EqualTo(true));
        }

        [Test]
        public void TestFieldOfView()
        {
            // Act
            var webcamProfile = new WebcamProfile();
            webcamProfile.FieldOfView = 1;
            // Assert
            Assert.That(webcamProfile.FieldOfView, Is.EqualTo(1));
        }

        [Test]
        public void TestIsHDROn()
        {
            // Act
            var webcamProfile = new WebcamProfile();
            webcamProfile.IsHDROn = true;
            // Assert
            Assert.That(webcamProfile.IsHDROn, Is.EqualTo(true));
        }
    }
}