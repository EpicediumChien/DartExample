using DDPM.SA.Common;
using System.Windows;
using VcpCore.Common;

namespace DDPM.UI.Module.Color.Tests
{
    [Apartment(ApartmentState.STA)]
    public class ColorRightViewTests
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
            ColorRightView colorRightView = new ColorRightView();
            // Assert
            Assert.That(colorRightView, Is.Not.Null);
        }

        [Test]
        public void Testget_index_of_json_config_for_cur_monitor()
        {
            ColorRightView colorRightView = new ColorRightView();
            List<ColorPresetSettings> temp = new List<ColorPresetSettings>();
            temp.Add(new ColorPresetSettings() { ModelName = "123", SerialNumber = "111" });
            var monitorInfo = new MonitorInfo() { edid = new VcpCore.Common.EDID() { ModelName = "123", SerialNumber = "111" } };
            Test_AddAppCollectionData.GetInstance()._monitorConfigs = temp;

            var result = colorRightView.get_index_of_json_config_for_cur_monitor(monitorInfo);

            // Assert
            Assert.AreNotEqual(-1, result);
        }
    }
}