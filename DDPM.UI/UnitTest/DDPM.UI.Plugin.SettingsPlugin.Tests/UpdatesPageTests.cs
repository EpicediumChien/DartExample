using DDPM.SA.Common;
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
    public class UpdatesPageTests
    {
        private UpdatesPage? updatesPage;
        private Mock<IDeviceManagerSA>? deviceManagerSAMock;
        private IDeviceManagerSA? deviceManagerSA;

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

            deviceManagerSAMock = new Mock<IDeviceManagerSA>();
            deviceManagerSA = deviceManagerSAMock.Object;
            DdpmCommonHelper.DeviceManagerSA = deviceManagerSA;
            updatesPage = new UpdatesPage();
        }

        [Test]
        public void TestConstructor_UpdatesPage()
        {
            Assert.That(updatesPage, Is.Not.Null);
        }
    }
}