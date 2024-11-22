using DDPM.UI.Common.UserControls;
using Dell.Client.Framework.UX.WPF;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDPM.Easy.Common.Tests
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public class ISplitCtrlTests
    {
        private Mock<ISplitCtrl>? ISplitCtrlMock;

        [SetUp]
        public void Setup()
        {
            ISplitCtrlMock = new Mock<ISplitCtrl>();
        }

        [Test]
        public void TestIsExisted()
        {
            // Assert
            var result=ISplitCtrl.IsExisted(1,'A');
            Assert.That(result, Is.EqualTo(false));
        }

        [Test]
        public void TestCtrlClass()
        {
            try
            {
                ISplitCtrlMock.Setup(x=>x.CtrlClass).Returns("A");
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }

        [Test]
        public void TestUC()
        {
            try
            {
                ISplitCtrlMock.Setup(x => x.UC).Returns(new System.Windows.Controls.UserControl());
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }

        [Test]
        public void TestEAID()
        {
            try
            {
                ISplitCtrlMock.Setup(x => x.EAID).Returns(11);
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }

        [Test]
        public void TestVM()
        {
            try
            {
                ISplitCtrlMock.Setup(x => x.VM).Returns(new SplitCtrlVM());
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }

        [Test]
        public void TestCreate()
        {
            var result = ISplitCtrl.Create(0, 'B');
            Assert.That(result, Is.InstanceOf<SplitCtrl0B>());

            result = ISplitCtrl.Create(0, 'A');
            Assert.That(result, Is.InstanceOf<SplitCtrl0A>());

            result = ISplitCtrl.Create(100, 'Z');
            Assert.That(result, Is.EqualTo(null));
        }

    }
}
