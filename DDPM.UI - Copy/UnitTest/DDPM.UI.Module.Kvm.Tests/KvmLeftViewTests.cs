using DDPM.SA.Common;
using DDPM.UI.Common;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using Moq;

namespace DDPM.UI.Module.Kvm.Tests
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public class KvmLeftViewTests
    {
        private KvmViewModel? kvmViewModel;
        private KvmLeftView? kvmLeftView;

        [SetUp]
        public void Setup()
        {
            kvmViewModel = new KvmViewModel();
            var deviceManagerMock = new Mock<IDeviceManagerSA>();
            var deviceManagerSA = deviceManagerMock.Object;
            DdpmCommonHelper.DeviceManagerSA = deviceManagerSA;
            var moduleOwnerMock = new Mock<IModuleOwner>();
            var moduleOwner = moduleOwnerMock!.Object;
            DdpmCommonHelper.ModuleOwner = moduleOwner;
            moduleOwnerMock.Setup(x => x.SelectedHomeDevice).Returns(new HomeDevice());
            KvmModule kvmModule = new KvmModule(moduleOwner);
            kvmViewModel.KvmModule = kvmModule;
            kvmViewModel.KvmModule.SelectedHomeDevice = new HomeDevice();
            kvmLeftView = new KvmLeftView(kvmViewModel);
        }

        [Test]
        public void TestConstructor_kvmLeftView()
        {
            Assert.That(kvmLeftView, Is.Not.Null);
            Assert.That(kvmLeftView.DataContext, Is.EqualTo(kvmViewModel));
        }
    }
}