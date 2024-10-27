using DDPM.UI.Module.DisplayOthers;
using NGA.UnitTest.PrivateObject;
using NUnit.Framework;
using System.Windows;

namespace DDPM.UI.Module.DisplayProperties.Tests
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public class DisplayOthersRightViewTests
    {
        private DisplayOthersRightView? displayOthersRightView;
        private PrivateObject? privateObject;

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
            DisplayOthersViewModel vm = new DisplayOthersViewModel();
            displayOthersRightView = new DisplayOthersRightView();
            privateObject = new PrivateObject(displayOthersRightView);
        }

        [Test]
        public void TestConstructor_InitializesComponent()
        {
            // Assert
            Assert.That(displayOthersRightView, Is.Not.Null);
        }
    }
}