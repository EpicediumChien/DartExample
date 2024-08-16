using DDPM.SA.Common;
using DDPM.SA.Common.Settings;
using DDPM.UI.Common;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDPM.UI.Plugin.SettingsPlugin.Tests
{
    [Apartment(ApartmentState.STA)]
    public class AnalyticsPageTests
    {
        private AnalyticsPage? analyticsPage;
        private Mock<IDeviceManagerSA>? DeviceManagerSAMock;
        private IDeviceManagerSA? iDeviceManagerSA;

        [SetUp]
        public void Setup()
        {
            DeviceManagerSAMock = new Mock<IDeviceManagerSA>();
            DdpmCommonHelper.DeviceManagerSA = DeviceManagerSAMock.Object;
        }

        [Test]
        public void TestConstructor_AnalyticsPage()
        {
            DeviceManagerSAMock.Setup(x => x.ReloadAppConfigData(It.IsAny<bool>())).Returns(Task.FromResult(new DDPMSettings(new DDPMAppSettings(), new DDPMUserSettings())));
            analyticsPage = new AnalyticsPage();
            Assert.That(analyticsPage, Is.Not.Null);
        }

        //internal class AnalyticsViewModel
        [Test]
        public void TeststrCheckBtnText()
        {
            AnalyticsViewModel analyticsViewModel = new AnalyticsViewModel();
            //analyticsViewModel.strCheckBtnText = "strCheckBtnText";
            //Assert.That(analyticsViewModel.strCheckBtnText, Is.EqualTo("strCheckBtnText"));
        }

        [Test]
        public void TeststrUrlBtnContent()
        {
            AnalyticsViewModel analyticsViewModel = new AnalyticsViewModel();
            //analyticsViewModel.strUrlBtnContent = "strUrlBtnContent";
            //Assert.That(analyticsViewModel.strUrlBtnContent, Is.EqualTo("strUrlBtnContent"));
        }

        [Test]
        public void TeststrPrivacyUrl()
        {
            AnalyticsViewModel analyticsViewModel = new AnalyticsViewModel();
            analyticsViewModel.strPrivacyUrl = "strPrivacyUrl";
            Assert.That(analyticsViewModel.strPrivacyUrl, Is.EqualTo("strPrivacyUrl"));
        }

        [Test]
        public void TeststrTitle()
        {
            AnalyticsViewModel analyticsViewModel = new AnalyticsViewModel();
            //analyticsViewModel.strTitle = "strTitle";
            //Assert.That(analyticsViewModel.strTitle, Is.EqualTo("strTitle"));
        }

        [Test]
        public void TeststrContent()
        {
            AnalyticsViewModel analyticsViewModel = new AnalyticsViewModel();
            //analyticsViewModel.strContent = "strContent";
            //Assert.That(analyticsViewModel.strContent, Is.EqualTo("strContent"));
        }

        [Test]
        public void TestisCheckEnable()
        {
            AnalyticsViewModel analyticsViewModel = new AnalyticsViewModel();
            analyticsViewModel.isCheckEnable = false;
            Assert.That(analyticsViewModel.isCheckEnable, Is.EqualTo(false));
        }

        [Test]
        public void TestisConsentChecked()
        {
            AnalyticsViewModel analyticsViewModel = new AnalyticsViewModel();
            analyticsViewModel.isConsentChecked = true;
            Assert.That(analyticsViewModel.isConsentChecked, Is.EqualTo(true));
        }

        [Test]
        public void TestConstructor_AnalyticsViewModel()
        {
            AnalyticsViewModel analyticsViewModel = new AnalyticsViewModel();
            Assert.That(analyticsViewModel, Is.Not.Null);
        }
    }
}