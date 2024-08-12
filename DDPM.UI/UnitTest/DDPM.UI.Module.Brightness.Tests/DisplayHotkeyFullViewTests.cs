using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common;
using Moq;
using NGA.UnitTest.PrivateObject;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dell.Client.Framework.UX.WPF;
using Dell.Client.Framework.Common;

namespace DDPM.UI.Module.Brightness.Tests
{
    [Apartment(ApartmentState.STA)]
    public class DisplayHotkeyFullViewTests
    {
        [SetUp]
        public void Setup()
        {

        }


        [Test]
        public void TestConstructor_InitializesComponent()
        {
            var myConsoleMock = new Mock<IConsole>();
            var myConsole = myConsoleMock.Object;
            var logMock= new Mock<ILog>();
            var log= logMock.Object;
            myConsoleMock.Setup(x=>x.CreateLog("DisplayHotkeyFullView")).Returns(log);
            DdpmCommonHelper.MyConsole = myConsole;

            var displayHotkeyFullView = new DisplayHotkeyFullView();
            var privateObject = new PrivateObject(displayHotkeyFullView);
            var result = privateObject.GetFieldOrProperty("_log");
            // Assert
            Assert.That(displayHotkeyFullView, Is.Not.Null);
            Assert.That(result, Is.Not.Null);

        }
    }
}
