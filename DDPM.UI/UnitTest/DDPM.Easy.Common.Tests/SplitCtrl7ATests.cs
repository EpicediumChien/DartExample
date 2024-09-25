using NGA.UnitTest.PrivateObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace DDPM.Easy.Common.Tests
{
    [Apartment(ApartmentState.STA)]
    public class SplitCtrl7ATests
    {
        private SplitCtrl7A? splitCtrl7A;
        private PrivateObject? privateObject;

        [SetUp]
        public void Setup()
        {
            splitCtrl7A = new SplitCtrl7A();
            privateObject = new PrivateObject(splitCtrl7A);
        }

        [Test]
        public void TestConstructor_SplitCtrl7A()
        {
            // Assert
            Assert.That(splitCtrl7A, Is.Not.Null);
        }
    }
}
