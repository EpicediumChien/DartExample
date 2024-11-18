using DDPM.UI.Common;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using Moq;
using NGA.UnitTest.PrivateObject;
using System.Windows;

namespace DDPM.UI.Module.DisplayHotkeys.Tests
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public class DisplayHotkeysRightViewTests
    {
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
        }

        [Test]
        public void TestConstructor_DisplayHotkeysRightView()
        {
            var moduleOwnerMock = new Mock<IModuleOwner>();
            DdpmCommonHelper.ModuleOwner = moduleOwnerMock.Object;
            moduleOwnerMock.Setup(x => x.SelectedHomeDevice).Returns(new HomeDevice());
            DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo = new VcpCore.Common.MonitorInfo();
            DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo.CapabilityDic = new Dictionary<string, List<string>>() { { "JE", new List<string>() { "E9" } } };
            var vm = new DisplayHotkeysViewModel() { DisplayHotkeysModule = new DisplayHotkeysModule() { SelectedHomeDevice = new Common.Models.HomeDevice() { MonitorInfo = new VcpCore.Common.MonitorInfo() { CapabilityDic = new Dictionary<string, List<string>>() { { "JE", new List<string>() { "E9" } } } } } } };
            var displayHotkeysRightView = new DisplayHotkeysRightView(vm);

            var privateObject = new PrivateObject(displayHotkeysRightView);
            var result = privateObject.GetFieldOrProperty("vm");

            // Assert
            Assert.That(displayHotkeysRightView.DataContext, Is.EqualTo(result));
        }
    }
}