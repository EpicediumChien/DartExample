using Dell.Client.Framework.UX.WPF;
using Moq;
using NGA.ThickClientCore;
using NGA.UnitTest.PrivateObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dell.UCA.ThickClientCore.Tests
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public class DucaSystrayDetailsTests
    {
        private DucaSystrayDetails? ducaSystrayDetails;
        private Mock<IWindowLayout>? windowLayoutMock;
        private Mock<IGearMenu>? gearMenuMock;
        private IConsole? console;
        private Mock<IShowPluginManager>? showPluginManagerMock;
        private PrivateObject? privateObject;
        private Guid? guid;


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
            guid = new Guid("3c6863f9-d8d6-4045-9403-8c3ace7df488");
            ducaSystrayDetails = new DucaSystrayDetails("C:\\Users\\zhilin\\AppData\\Local\\Dell\\Dell Display and Peripheral Manager", new Guid("3c6863f9-d8d6-4045-9403-8c3ace7df488"));
            privateObject = new PrivateObject(ducaSystrayDetails);
        }

        [Test]
        public void TestConstructor_DucaSystrayDetails()
        {
            Assert.That(ducaSystrayDetails, Is.Not.Null);
            Assert.That(ducaSystrayDetails.SystrayFullPath, Is.EqualTo("C:\\Users\\zhilin\\AppData\\Local\\Dell\\Dell Display and Peripheral Manager"));
            Assert.That(ducaSystrayDetails.SystrayUniqueId, Is.EqualTo(guid));
        }

        [Test]
        public void TestSystrayUniqueId()
        {
            Assert.That(ducaSystrayDetails.SystrayUniqueId, Is.EqualTo(guid));
        }

        [Test]
        public void TestSystrayFullPath()
        {
            Assert.That(ducaSystrayDetails.SystrayFullPath, Is.EqualTo("C:\\Users\\zhilin\\AppData\\Local\\Dell\\Dell Display and Peripheral Manager"));
        }
    }
}
