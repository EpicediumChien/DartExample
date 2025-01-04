using Dell.Client.Framework.UX.WPF;
using Dell.Client.Framework.UX.WPF.Controls;
using Moq;
using NGA.ThickClient.Interfaces;
using NGA.ThickClientCore;
using NGA.UnitTest.PrivateObject;
using System;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ExplorerBar;

namespace Dell.UCA.ThickClientCore.Tests
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public class CustomWindowLayoutTests
    {
#pragma warning disable NUnit1032 // An IDisposable field/property should be Disposed in a TearDown method
        private CustomWindowLayout? customWindowLayout;
#pragma warning restore NUnit1032 // An IDisposable field/property should be Disposed in a TearDown method
        private Mock<IWindowLayout>? windowLayoutMock;
        private Mock<IGearMenu>? gearMenuMock;
        private IConsole? console;
        private Mock<IShowPluginManager>? showPluginManagerMock;
        private PrivateObject? privateObject;


        [SetUp]
        public void Setup()
        {
            if (System.Windows.Application.Current == null)
            {
                new System.Windows.Application();
            }
            windowLayoutMock = new Mock<IWindowLayout>();
            gearMenuMock = new Mock<IGearMenu>();
            showPluginManagerMock = new Mock<IShowPluginManager>();
            windowLayoutMock.Setup(x => x.Masthead).Returns(new Client.Framework.UX.WPF.Controls.UXMasthead());
            windowLayoutMock.Setup(x => x.Frame).Returns(new Client.Framework.UX.WPF.Controls.UXFrame());
            customWindowLayout = new CustomWindowLayout(windowLayoutMock.Object, gearMenuMock.Object, showPluginManagerMock.Object);
            privateObject = new PrivateObject(customWindowLayout);
        }

        [Test]
        public void TestConstructor_CustomWindowLayout()
        {
            Assert.That(customWindowLayout, Is.Not.Null);
            Assert.That(privateObject.GetFieldOrProperty("_windowLayout"), Is.EqualTo(windowLayoutMock.Object));
            Assert.That(privateObject.GetFieldOrProperty("_showPluginManager"), Is.EqualTo(showPluginManagerMock.Object));
            Assert.That(windowLayoutMock.Object.Masthead.Height, Is.EqualTo(64));
        }

        [Test]
        public void TestNotificationIcon()
        {
            Assert.That(customWindowLayout.NotificationIcon.Height, Is.EqualTo(32));
            Assert.That(customWindowLayout.NotificationIcon.Width, Is.EqualTo(32));
            Assert.That(customWindowLayout.NotificationIcon.IsEnabled, Is.EqualTo(false));
        }

        [Test]
        public void TestIconComboBox()
        {
            Assert.That(customWindowLayout.IconComboBox, Is.Not.Null);
            Assert.That(customWindowLayout.IconComboBox, Is.InstanceOf<UXIconComboBox>());
        }

        [Test]
        public void TestDispose()
        {
            customWindowLayout.Dispose();
            Assert.That(privateObject.GetFieldOrProperty("_disposedValue"),Is.EqualTo(true));
        }
    }
}