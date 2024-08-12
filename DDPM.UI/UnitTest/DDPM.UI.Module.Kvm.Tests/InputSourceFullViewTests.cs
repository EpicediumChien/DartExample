using DDPM.UI.Common.EAEM;
using NGA.UnitTest.PrivateObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDPM.UI.Module.Kvm.Tests
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public class InputSourceFullViewTests
    {
        private InputSourceFullView? inputSourceFullView;

        [SetUp]
        public void Setup()
        {
            inputSourceFullView = new InputSourceFullView();
        }

        [Test]
        public void TestConstructor_InitializeComponent()
        {
            Assert.That(inputSourceFullView, Is.Not.Null);
        }
    }
}
