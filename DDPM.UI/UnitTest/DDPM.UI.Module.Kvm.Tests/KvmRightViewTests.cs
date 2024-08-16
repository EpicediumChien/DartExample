using DDPM.SA.Common;
using DDPM.UI.Common;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using Moq;

namespace DDPM.UI.Module.Kvm.Tests
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public class KvmRightViewTests
    {
        private Mock<IDeviceManagerSA>? deviceManagerMock;
        private IDeviceManagerSA? deviceManagerSA;
        private IModuleOwner? moduleOwner;
        private Mock<IModuleOwner>? moduleOwnerMock;
        private KvmViewModel? kvmViewModel;
        private KvmRightView? kvmRightView;

        [SetUp]
        public void Setup()
        {
            deviceManagerMock = new Mock<IDeviceManagerSA>();
            deviceManagerSA = deviceManagerMock.Object;
            DdpmCommonHelper.DeviceManagerSA = deviceManagerSA;

            moduleOwnerMock = new Mock<IModuleOwner>();
            moduleOwner = moduleOwnerMock!.Object;
            DdpmCommonHelper.ModuleOwner = moduleOwner;
            moduleOwnerMock.Setup(x => x.SelectedHomeDevice).Returns(new HomeDevice());

            kvmViewModel = new KvmViewModel();
            kvmRightView = new KvmRightView(kvmViewModel);
        }

        [Test]
        public void TestConstructor_KvmRightView()
        {
            Assert.That(kvmRightView, Is.Not.Null);
            Assert.That(kvmRightView.DataContext, Is.EqualTo(kvmViewModel));
        }
    }
}