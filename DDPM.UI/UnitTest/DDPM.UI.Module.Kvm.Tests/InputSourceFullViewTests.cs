using System.Windows;
using Dell.Client.Framework.UX.WPF.ResourceManager;

namespace DDPM.UI.Module.Kvm.Tests
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public class InputSourceFullViewTests
    {
        private InputSourceFullView? inputSourceFullView;

        [SetUp]
        public void Setup()
        {
            if (System.Windows.Application.Current == null)
            {
                new System.Windows.Application();
            }
            ResourceManager res = new ResourceManager();
            var resourceDictionary = new ResourceDictionary();
            resourceDictionary.Source = new Uri("pack://application:,,,/DDPM.UI.Common;component/ModuleStyle.xaml");
            System.Windows.Application.Current.Resources.MergedDictionaries.Add(resourceDictionary);
            inputSourceFullView = new InputSourceFullView();
        }

        [Test]
        public void TestConstructor_InitializeComponent()
        {
            Assert.That(inputSourceFullView, Is.Not.Null);
        }
    }
}