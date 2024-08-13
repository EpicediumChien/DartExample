using DDPM.SA.Common;
using DDPM.UI.Plugin.MousePlugin;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using Moq;
using NGA.UnitTest.PrivateObject;
using System;

namespace DDPM.UI.Plugin.MousePluginTests
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public class MousePluginTests
    {
        private IPluginManager? pluginManager;
        private Mock<IPluginManager>? pluginManagerMock;
        private IConsole? console;
        private Mock<IConsole>? consoleMock;
        private Mouseplugin? mouseplugin;
        private ILog? _log;
        private Mock<ILog>? _logMock;
        private PrivateObject?privateObject;
        private Mock<IDeviceManagerSA>? IDeviceManagerSAMock;
        private IDeviceManagerSA? iDeviceManagerSA;

        [SetUp]
        public void Setup()
        {
            pluginManagerMock=new Mock<IPluginManager>();
            pluginManager=pluginManagerMock.Object;
            consoleMock=new Mock<IConsole>();
            console=consoleMock.Object;
            _logMock = new Mock<ILog>();
            _log = _logMock.Object;
            IDeviceManagerSAMock=new Mock<IDeviceManagerSA>();
            iDeviceManagerSA=IDeviceManagerSAMock.Object;
            consoleMock.Setup(x => x.CreateLog(It.IsAny<string>())).Returns(_logMock.Object);          
            mouseplugin =new Mouseplugin(pluginManager, console);
            privateObject=new PrivateObject(mouseplugin);
        }

        [Test]
        public void TestConstructor_Mouseplugin()
        {
            // Act;

            // Assert
            Assert.That(mouseplugin,Is.Not.Null);

        }

        //[Test]
        //public void TestOnShown()
        //{
        //    privateObject.SetFieldOrProperty("_deviceManagerPlugin", iDeviceManagerSA);
        //    try
        //    {
        //        mouseplugin.OnShown("xx");
        //        Assert.True(true);
        //    }
        //    catch (Exception ex)
        //    {
        //        Assert.Fail("not invoked");
        //    }
        //}

    }
}