using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using DDPM.UI.Common;
using Moq;
using NGA.UnitTest.PrivateObject;
using VcpCore.Common;
using DDPM.SA.Common;

namespace DDPM.UI.Module.DisplayHotkeys.Tests
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public class DisplayHotkeysRightViewTests
    {
        private Mock<IModuleOwner>? moduleOwnerMock;

        [SetUp]
        public void Setup()
        {
 
        }

        [Test]
        public void TestConstructor_DisplayHotkeysRightView()
        {
            moduleOwnerMock = new Mock<IModuleOwner>();
            moduleOwnerMock.Setup(x => x.SelectedHomeDevice).Returns(new HomeDevice());
            var moduleOwner = moduleOwnerMock!.Object;
            DdpmCommonHelper.ModuleOwner = moduleOwner;
            DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo = new MonitorInfo();
            DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo.CapabilityDic = new Dictionary<string, List<string>>();
            var vm = new DisplayHotkeysViewModel();
            vm.DisplayHotkeysModule=new DisplayHotkeysModule(moduleOwnerMock.Object);
            vm.DisplayHotkeysModule.SelectedHomeDevice = new HomeDevice();
            vm.DisplayHotkeysModule.SelectedHomeDevice.MonitorInfo=new MonitorInfo();
            vm.DisplayHotkeysModule.SelectedHomeDevice.MonitorInfo.CapabilityDic=new Dictionary<string, List<string>>();
            var displayHotkeysRightView = new DisplayHotkeysRightView(vm);
            var privateObject = new PrivateObject(displayHotkeysRightView);
            var result = privateObject.GetFieldOrProperty("vm");

            // Assert
            Assert.That(displayHotkeysRightView.DataContext, Is.EqualTo(result));
        }
    }
}