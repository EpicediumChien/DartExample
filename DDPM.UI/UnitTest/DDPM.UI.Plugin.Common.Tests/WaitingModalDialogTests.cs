using DDPM.UI.Common.UserControls;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using Moq;
using NGA.UnitTest.PrivateObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace DDPM.UI.Plugin.Common.Tests
{
    [Apartment(ApartmentState.STA)]
    public class WaitingModalDialogTests
    {
        private WaitingModalDialog? waitingModalDialog;
        private Mock<IShowPluginManager>? showPluginManagerMock;
        private Mock<IConsole>? consoleMock;
        private Mock<ILog>? logMock;
        [SetUp]
        public void Setup()
        {
            showPluginManagerMock=new Mock<IShowPluginManager>();
            consoleMock=new Mock<IConsole>();
            logMock=new Mock<ILog>();
            waitingModalDialog = new WaitingModalDialog("caption", "message", "alert",new Plugin.ViewModels.AddDeviceViewModel(showPluginManagerMock.Object, consoleMock.Object, logMock.Object));
        }

        [Test]
        public void TestConstructor_WaitingModalDialog()
        {
            // Assert
            Assert.That(waitingModalDialog, Is.Not.Null);
        }
    }
}
