using DDPM.UI.Common.EAEM;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using DDPM.UI.Common.UserControls;
using DDPM.UI.Interfaces;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using Moq;
using NGA.UnitTest.PrivateObject;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace DDPM.UI.Common.Tests
{
    [Apartment(ApartmentState.STA)]
    public class ModuleGroupTests
    {
        private ModuleGroup? moduleGroup;
        private PrivateObject? privateObject;

        [SetUp]
        public void Setup()
        {
            moduleGroup = new ModuleGroup();
            privateObject = new PrivateObject(moduleGroup);
        }

        [Test]
        public void TestConstructor_ModuleGroup()
        {
            // Assert
            Assert.That(moduleGroup, Is.Not.Null);
        }

        [Test]
        public void TestVbarIcon()
        {
            // Act
            var vbarIcon = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Speaker_AudioSettings.png");
            moduleGroup.VbarIcon = vbarIcon;
            // Assert
            Assert.That(moduleGroup.VbarIcon, Is.EqualTo(vbarIcon));
        }

        [Test]
        public void TestIconTemplate()
        {
            // Act
            var iconTemplate = new ControlTemplate();
            moduleGroup.IconTemplate = iconTemplate;
            // Assert
            Assert.That(moduleGroup.IconTemplate, Is.EqualTo(iconTemplate));
        }

        [Test]
        public void TestHeaderCount()
        {
            // Act
            var result= moduleGroup.HeaderCount;
            // Assert
            Assert.That(result, Is.EqualTo(0));
        }

        [Test]
        public void TestAddHeader()
        {
            try
            {
                moduleGroup.AddHeader("headerText",typeof(HomeDevice));
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }

        [Test]
        public void TestFindRightViewHeaderByModuleName()
        {
            // Act
            var result = moduleGroup.FindRightViewHeaderByModuleName("PipPbpModule");
            // Assert
            Assert.That(result, Is.EqualTo(null));

            // Act
            var headers = new ObservableCollection<RightViewHeader>() { new RightViewHeader(1, "text") { ModuleType=typeof(HomeDevice)} };
            privateObject.SetFieldOrProperty("_headers", headers);
            result = moduleGroup.FindRightViewHeaderByModuleName("PipPbpModule");
            // Assert
            Assert.That(result, Is.EqualTo(null));

            // Act
            result = moduleGroup.FindRightViewHeaderByModuleName("HomeDevice");
            // Assert
            Assert.That(result, Is.EqualTo(headers[0]));

            // Act
            var ddpmModuleMock = new Mock<IDdpmModule?>();
            ddpmModuleMock.Setup(x => x.ModuleName).Returns("PipPbpModule");
            headers = new ObservableCollection<RightViewHeader>() { new RightViewHeader(1, "text") { DdpmModule = ddpmModuleMock.Object } };
            privateObject.SetFieldOrProperty("_headers", headers);
            result = moduleGroup.FindRightViewHeaderByModuleName("PipPbpModule");
            // Assert
            Assert.That(result, Is.EqualTo(headers[0]));
        }


        [TearDown]
        public void TearDown()
        {
            if (moduleGroup != null)
            {
                moduleGroup.Dispose();
                moduleGroup = null;
            }
        }

    }
}