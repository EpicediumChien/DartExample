using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDPM.UI.Module.Kvm.Tests
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public class ConnectMKFullViewTests
    {
        private ConnectMKFullView? connectMKFullView;

        [SetUp]
        public void Setup()
        {
            connectMKFullView = new ConnectMKFullView();
        }

        [Test]
        public void TestConstructor_InitializeComponent()
        {
            Assert.That(connectMKFullView, Is.Not.Null);
        }
    }
}
