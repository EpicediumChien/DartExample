using DDPM.SA.Common;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common;
using DDPM.UI.Plugin.ViewModels;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using Moq;
using NGA.UnitTest.PrivateObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Dell.Client.Framework.UX.WPF.ResourceManager;
using NUnit.Framework.Interfaces;
using System.Globalization;

namespace DDPM.UI.Module.KeyCustomization.Tests
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public class KeyCustomizationRightViewTests
    {
        private KeyCustomizationRightView? keyCustomizationRightView;
        private PrivateObject? privateObject;
        private Mock<IDeviceManagerSA>? deviceManagerMock;
        private IDeviceManagerSA? deviceManager;
        private Mock<IConsole>? consoleMock;
        private IConsole? console;
        private Mock<ILog>? logMock;
        private ILog? log;
        private KeyboardViewModel? vm;

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
            deviceManagerMock = new Mock<IDeviceManagerSA>();
            deviceManager = deviceManagerMock.Object;
            DdpmCommonHelper.DeviceManagerSA = deviceManager;
            consoleMock = new Mock<IConsole>();
            console = consoleMock.Object;
            DdpmCommonHelper.MyConsole = console;
            logMock = new Mock<ILog>();
            log = logMock.Object;
            vm = new KeyboardViewModel(console, log);
            keyCustomizationRightView = new KeyCustomizationRightView(vm);
            privateObject = new PrivateObject(keyCustomizationRightView);
        }

        [Test]
        public void TestConstructor_KeyCustomizationRightView()
        {
            if (CultureInfo.CurrentCulture.Name == "es-US")
            {
                var txtMessageText = Strings.KeyCustomizeMessage;
                var txtRestoreText = Strings.KeyCustomizeRestoreCaption;
                var txtSuggestedActionsText = Strings.SuggestedActionsCaption;
                var txtProductivityActionsText = Strings.ProductivityActionsCaption;
                var txtWindowsActionsText = Strings.WindowsActionsCaption;
                var txtMultimediaActionsText = Strings.MultimediaActionsCaption;
                var txtSearchResultText = Strings.SearchResultsCaption;

                // Assert
                Assert.That(keyCustomizationRightView, Is.Not.Null);
                Assert.That(privateObject!.GetFieldOrProperty("_vm"), Is.EqualTo(vm));
                Assert.That(txtMessageText, Is.EqualTo("To customize a key, click one of the outlined keys on the image to the left"));
                Assert.That(txtRestoreText, Is.EqualTo("Restore all actions to default"));
                Assert.That(txtSuggestedActionsText, Is.EqualTo("Suggested Actions"));
                Assert.That(txtProductivityActionsText, Is.EqualTo("Productivity Actions"));
                Assert.That(txtWindowsActionsText, Is.EqualTo("Windows Actions"));
                Assert.That(txtMultimediaActionsText, Is.EqualTo("Multimedia Actions"));
                Assert.That(txtSearchResultText, Is.EqualTo("Search Results"));
            }
        }

        [Test]
        public void TestInitialize()
        {
            keyCustomizationRightView.Initialize();
            var txtCaptionText = Strings.KeyCustomizeCaptionCaption;
            var imgBackVisibility = Visibility.Collapsed;
            var Section1Visibility = Visibility.Visible;
            // Assert

            Assert.That(imgBackVisibility, Is.EqualTo(Visibility.Collapsed));
            Assert.That(Section1Visibility, Is.EqualTo(Visibility.Visible));
            Assert.That(privateObject.GetFieldOrProperty("SelectedActionID"), Is.EqualTo(-1));
            if (CultureInfo.CurrentCulture.Name == "es-US")
            { 
                Assert.That(txtCaptionText, Is.EqualTo("Key Customization")); 
            }
        }

        [Test]
        public void TestInitializea()
        {
            vm = new KeyboardViewModel(console, log) {SelectedKey="3",KeyboardAction= new KeyboardActions("KB7221W") };
            keyCustomizationRightView = new KeyCustomizationRightView(vm);
            keyCustomizationRightView.Initialize();
            // Assert
            Assert.That(privateObject.GetFieldOrProperty("SelectedActionID"), Is.EqualTo(-1));
        }
    }
}
