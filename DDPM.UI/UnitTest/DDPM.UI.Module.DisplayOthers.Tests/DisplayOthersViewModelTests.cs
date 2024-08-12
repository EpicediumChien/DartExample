using DDPM.UI.Common.Interfaces;
using Moq;
using NGA.UnitTest.PrivateObject;
using NUnit.Framework;

namespace DDPM.UI.Module.DisplayOthers.Tests
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public class DisplayOthersViewModelTests
    {
        private DisplayOthersViewModel? viewModel;
        private PrivateObject? privateObject;
        private Mock<IModuleOwner>? moduleOwnerMock;

        [SetUp]
        public void SetUp()
        {
            viewModel = new DisplayOthersViewModel();
            privateObject = new PrivateObject(viewModel);
            moduleOwnerMock = new Mock<IModuleOwner>();
        }

        [Test]
        public void TestModuleOwner()
        {
            // Arrange
            var expectedModuleOwner = moduleOwnerMock!.Object;

            // Act
            privateObject!.SetProperty("ModuleOwner", expectedModuleOwner);
            var result = privateObject.GetProperty("ModuleOwner");

            // Assert
            Assert.That(result, Is.EqualTo(expectedModuleOwner));
        }
    }
}
