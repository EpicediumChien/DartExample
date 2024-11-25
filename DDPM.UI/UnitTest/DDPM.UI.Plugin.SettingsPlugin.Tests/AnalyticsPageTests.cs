using DDPM.SA.Common;
using DDPM.SA.Common.Settings;
using DDPM.UI.Common;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Dell.Client.Framework.UX.WPF.ResourceManager;

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
            if (System.Windows.Application.Current == null)
            {
                new System.Windows.Application();
            }
            ResourceManager res = new ResourceManager();
            var resourceDictionary = new ResourceDictionary();
            resourceDictionary.Source = new Uri("pack://application:,,,/DDPM.UI.Common;component/ModuleStyle.xaml");
            System.Windows.Application.Current.Resources.MergedDictionaries.Add(resourceDictionary);
            DeviceManagerSAMock = new Mock<IDeviceManagerSA>();
            DdpmCommonHelper.DeviceManagerSA = DeviceManagerSAMock.Object;
        }

        [Test]
        public void TestConstructor_AnalyticsPage()
        {
            DeviceManagerSAMock.Setup(x => x.ReloadAppConfigData(It.IsAny<bool>())).Returns(Task.FromResult(new DDPMSettings(new DDPMAppSettings(), new DDPMUserSettings(),new DDPMITConfig())));
            analyticsPage = new AnalyticsPage();
            Assert.That(analyticsPage, Is.Not.Null);
        }

        //internal class AnalyticsViewModel
        [Test]
        public void TeststrPrivacyUrl()
        {
            AnalyticsViewModel analyticsViewModel = new AnalyticsViewModel();
            analyticsViewModel.strPrivacyUrl = "strPrivacyUrl";
            Assert.That(analyticsViewModel.strPrivacyUrl, Is.EqualTo("strPrivacyUrl"));
        }

        [Test]
        public void TestShowLockMask()
        {
            AnalyticsViewModel analyticsViewModel = new AnalyticsViewModel();
            analyticsViewModel.ShowLockMask = true;
            Assert.That(analyticsViewModel.ShowLockMask, Is.EqualTo(true));
            Assert.That(analyticsViewModel.LockMaskVisible, Is.EqualTo(Visibility.Visible));
        }

        [Test]
        public void TestLockMaskVisible()
        {
            AnalyticsViewModel analyticsViewModel = new AnalyticsViewModel();
            analyticsViewModel.LockMaskVisible = Visibility.Visible;
            Assert.That(analyticsViewModel.LockMaskVisible, Is.EqualTo(Visibility.Visible));
        }

        [Test]
        public void TestisTabStoppable()
        {
            AnalyticsViewModel analyticsViewModel = new AnalyticsViewModel();
            analyticsViewModel.isTabStoppable = false;
            Assert.That(analyticsViewModel.isTabStoppable, Is.EqualTo(false));
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