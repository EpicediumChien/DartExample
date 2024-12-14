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
using CommunityToolkit.Mvvm.DependencyInjection;
using Dell.Client.Framework.Common;
using DDPM.UI.Plugin.DdpmHomePlugin.ViewModels;

namespace DDPM.UI.Plugin.SettingsPlugin.Tests
{
    [Apartment(ApartmentState.STA)]
    public class UpdatesPageTests
    {
        private UpdatesPage? updatesPage;
        private Mock<IDeviceManagerSA>? deviceManagerSAMock;
        private IDeviceManagerSA? deviceManagerSA;
        private Mock<IServiceProvider>? PluginIocMock;


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

            var mockLog = new Mock<ILog>();
            PluginIocMock = new Mock<IServiceProvider>();
            //SettingsPlugin.PluginIoc= PluginIocMock.Object as Ioc;
            PluginIocMock.Setup(x => x.GetService(It.IsAny<Type>())).Returns(mockLog.Object);

            //updatesPage = new UpdatesPage();
        }

        //Elsa mark for PluginIoc readonly
        //[Test]
        //public void TestConstructor_UpdatesPage()
        //{
        //    Assert.That(updatesPage, Is.Not.Null);
        //}
    }
}