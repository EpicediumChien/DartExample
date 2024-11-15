using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DDPM.UI.Module.GamingVisionEngine.Tests
{
    [Apartment(ApartmentState.STA)]
    public class VisionEngineRightViewTests
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
        public void TestConstructor_InitializesComponent()
        {
            VisionEngineRightView visionEngineRightView = new VisionEngineRightView();
            // Assert
            Assert.That(visionEngineRightView, Is.Not.Null);
        }
    }
}
