using DDPM.UI.Common;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using Moq;
using NGA.UnitTest.PrivateObject;
using NUnit.Framework;
using System.Windows;
using System.Windows.Controls;

namespace DDPM.UI.Module.DisplayOthers.Tests
{
    [TestFixture]
    public class DisplayOthersModuleTests
    {
        private DisplayOthersModule? displayOthersModule;
        private PrivateObject? privateObject;
        private Mock<IModuleOwner>? moduleOwnerMock;

        [SetUp]
        public void SetUp()
        {
            if (System.Windows.Application.Current == null)
            {
                new System.Windows.Application();
            }
            var resourceDictionary = new ResourceDictionary();
            resourceDictionary.Source = new Uri("pack://application:,,,/DDPM.UI.Common;component/ModuleStyle.xaml");
            System.Windows.Application.Current.Resources.MergedDictionaries.Add(resourceDictionary);
            var moduleOwerMock = new Mock<IModuleOwner>();
            var moduleOwer = moduleOwerMock.Object;
            DdpmCommonHelper.ModuleOwner = moduleOwer;
            var selectedHomeDevice = new HomeDevice();
            moduleOwerMock.Setup(x => x.SelectedHomeDevice).Returns(new HomeDevice());
            displayOthersModule = new DisplayOthersModule();
            privateObject = new PrivateObject(displayOthersModule);
            moduleOwnerMock = new Mock<IModuleOwner>();
        }

        [Test]
        [Apartment(ApartmentState.STA)]
        public void TestModuleName()
        {
            // Act
            var result = displayOthersModule!.ModuleName;

            // Assert
            Assert.That(result, Is.EqualTo("DisplayOthersModule"));
        }

        [Test]
        [Apartment(ApartmentState.STA)]
        public void TestGetLeftView()
        {
            // Act
            var result = displayOthersModule!.GetLeftView();

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        [Apartment(ApartmentState.STA)]
        public void TestGetRightView()
        {
            // Act
            var result = displayOthersModule!.GetRightView();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<UserControl>());
        }

        [Test]
        [Apartment(ApartmentState.STA)]
        public void TestSelectedHomeDevice()
        {
            // Arrange
            var homeDevice = new HomeDevice();

            // Act
            displayOthersModule!.SelectedHomeDevice = homeDevice;
            var result = displayOthersModule.SelectedHomeDevice;

            // Assert
            Assert.That(result, Is.EqualTo(homeDevice));
        }

        [Test]
        [Apartment(ApartmentState.STA)]
        public void TestModuleOwner()
        {
            // Arrange
            var moduleOwner = moduleOwnerMock!.Object;

            // Act
            privateObject!.SetProperty("ModuleOwner", moduleOwner);
            var result = privateObject.GetProperty("ModuleOwner");

            // Assert
            Assert.That(result, Is.EqualTo(moduleOwner));
        }

        [Test]
        [Apartment(ApartmentState.STA)]
        public void TestViewModelInitialization()
        {
            // Act
            var viewModel = privateObject!.GetFieldOrProperty("vm") as DisplayOthersViewModel;

            // Assert
            Assert.That(viewModel, Is.Not.Null);
        }

        [Test]
        [Apartment(ApartmentState.STA)]
        public void TestOnSelectedHomeDeviceChanged()
        {
            var displayOthersModule = new DisplayOthersModule();
            displayOthersModule.OnSelectedHomeDeviceChanged();
            Assert.Pass();
        }

        [Test]
        [Apartment(ApartmentState.STA)]
        public void TestOnActivated()
        {
            var displayOthersModule = new DisplayOthersModule();
            displayOthersModule.OnActivated();
            Assert.Pass();
        }

        [Test]
        [Apartment(ApartmentState.STA)]
        public void TestOnDeactivated()
        {
            var displayOthersModule = new DisplayOthersModule();
            displayOthersModule.OnDeactivated();
            Assert.Pass("The method executed without exceptions");
        }
    }
}