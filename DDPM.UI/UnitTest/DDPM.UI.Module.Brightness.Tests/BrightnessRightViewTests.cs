using DDPM.SA.Common;
using DDPM.UI.Common;
using DDPM.UI.Common.Interfaces;
using Moq;
using NGA.UnitTest.PrivateObject;
using NUnit.Framework;
using System.Windows;

namespace DDPM.UI.Module.Brightness.Tests
{
    [Apartment(ApartmentState.STA)]
    public class BrightnessRightViewTests
    {
        private BrightnessRightView? brightnessRightView;
        private PrivateObject? privateObject;
        private Mock<IModuleOwner>? moduleOwnerMock;
        private Mock<IDeviceManagerSA>? deviceManagerMock;

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
            moduleOwnerMock = new Mock<IModuleOwner>();
            var moduleOwner = moduleOwnerMock!.Object;
            DdpmCommonHelper.ModuleOwner = moduleOwner;
            //brightnessRightView = new BrightnessRightView();
            //privateObject = new PrivateObject(brightnessRightView);
        }

        [Test]
        public void TestConstructor_InitializesComponent()
        {
            brightnessRightView = new BrightnessRightView();
            privateObject = new PrivateObject(brightnessRightView);
            // Assert
            Assert.That(brightnessRightView, Is.Not.Null);
        }
    }
}