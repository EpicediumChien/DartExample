using DDPM.SA.Common;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using DDPM.UI.Common;
using Moq;
using NGA.UnitTest.PrivateObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDPM.UI.Module.Kvm.Tests
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public class KvmLeftViewTests
    {
        private KvmViewModel? kvmViewModel;
        private KvmLeftView? kvmLeftView;

        [SetUp]
        public void Setup()
        {
            kvmViewModel = new KvmViewModel();
            kvmLeftView = new KvmLeftView(kvmViewModel);
        }

        [Test]
        public void TestConstructor_kvmLeftView()
        {
            Assert.That(kvmLeftView, Is.Not.Null);
            Assert.That(kvmLeftView.DataContext, Is.EqualTo(kvmViewModel));
        }
    }
}
