using NGA.UnitTest.PrivateObject;
using System.Windows;

namespace DDPM.UI.Module.DisplayProperties.Tests
{
    [Apartment(ApartmentState.STA)]
    public class DisplayPropertiesRightViewTests
    {
        private DisplayPropertiesRightView? displayPropertiesRightView;
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
            displayPropertiesRightView = new DisplayPropertiesRightView();
            privateObject = new PrivateObject(displayPropertiesRightView);
        }

        [Test]
        public void TestConstructor_InitializesComponent()
        {
            // Assert
            Assert.That(displayPropertiesRightView, Is.Not.Null);
        }
    }
}