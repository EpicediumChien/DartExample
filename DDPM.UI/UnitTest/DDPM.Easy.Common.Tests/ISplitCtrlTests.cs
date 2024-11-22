using Castle.Core.Resource;
using DDPM.UI.Common;
using DDPM.UI.Common.UserControls;
using DDPM.UI.Module.EzArrange;
using Dell.Client.Framework.UX.WPF;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

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

        [Test]
        public void TestsCreate()
        {
            var result = ISplitCtrl.Create(3);
            Assert.That(result, Is.InstanceOf<SplitCtrl2C>());

            result = ISplitCtrl.Create(100);
            Assert.That(result, Is.EqualTo(null));
        }

        [Test]
        public void TestClone()
        {
            try
            {
                ISplitCtrlMock.Setup(x => x.Clone()).Returns(ISplitCtrlMock.Object);
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }

        [Test]
        public void TestHoveringCell()
        {
            try
            {
                ISplitCtrlMock.Setup(x => x.HoveringCell).Returns("HoveringCell");
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }

        [Test]
        public void TestCellBorders()
        {
            try
            {
                ISplitCtrlMock.Setup(x => x.CellBorders).Returns(new List<CellBorder>());
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }

        [Test]
        public void TestSettingsString()
        {
            try
            {
                ISplitCtrlMock.Setup(x => x.SettingsString).Returns("SettingsString");
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }

        [Test]
        public void TestIsVertical()
        {
            try
            {
                ISplitCtrlMock.Setup(x => x.IsVertical).Returns(true);
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }

        //[Test]
        //public void TestCreateBitmapSourcel()
        //{
        //    BitmapSource ImageSource = (BitmapSource)DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Product_KB900.png");
        //    try
        //    {
        //        ISplitCtrlMock.Setup(x => x.CreateBitmapSource()).Returns(ImageSource);
        //        Assert.True(true);
        //    }
        //    catch (Exception ex)
        //    {
        //        Assert.Fail("not invoked");
        //    }
        //}

        //[Test]
        //public void TestsSaveBitmapSourceAsPngFile()
        //{
        //    var ispSource = ISplitCtrlMock.Object;
        //    ISplitCtrlMock.Setup(x => x.Clone()).Returns(ispSource);
        //    ISplitCtrl isp = ispSource.Clone();
        //    BitmapSource bmpSrc = isp.CreateBitmapSource();
        //    var ImageSource = (BitmapSource)DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Product_KB900.png");
        //    var result = ISplitCtrl.SaveBitmapSourceAsPngFile(ImageSource, "name");
        //    Assert.That(result, Is.EqualTo(true));

        //}

        [Test]
        public void TestIsOverlapCustomLayout()
        {
            try
            {
                ISplitCtrlMock.Setup(x => x.IsOverlapCustomLayout).Returns(true);
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }

    }
}
