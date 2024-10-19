using DDPM.Easy.Common;
using DDPM.SA.Common;
using DDPM.UI.Common.Models;
using DDPM.UI.Common.UserControls;
using Moq;
using NGA.UnitTest.PrivateObject;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using Windows.Storage;

namespace DDPM.UI.Common.Tests
{
    [Apartment(ApartmentState.STA)]
    public class RightViewHeaderCtrlTests
    {
        private RightViewHeaderCtrl? rightViewHeaderCtrl;
        private PrivateObject?privateObject;
        private Mock<IDeviceManagerSA>? deviceManagerSAMock;

        [SetUp]
        public void Setup()
        {
            deviceManagerSAMock=new Mock<IDeviceManagerSA>();
            DdpmCommonHelper.DeviceManagerSA=deviceManagerSAMock.Object;
            rightViewHeaderCtrl = new RightViewHeaderCtrl();
            privateObject = new PrivateObject(rightViewHeaderCtrl);
        }

        [Test]
        public void TestConstructor_RightViewHeaderCtrl()
        {
            // Assert
            Assert.That(rightViewHeaderCtrl, Is.Not.Null);
            Assert.That(rightViewHeaderCtrl.DataContext, Is.EqualTo(privateObject.GetFieldOrProperty("vm")));
        }

        [Test]
        public void TestSetHeaders()
        {
            var headers = new RightViewHeader[] { new RightViewHeader(1, "text1"), new RightViewHeader(2, "text2") };
            try
            {
                rightViewHeaderCtrl.SetHeaders(headers);
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }

        [Test]
        public void TestItemsSource()
        {
            var headers = new RightViewHeader[] { new RightViewHeader(1, "text1"), new RightViewHeader(2, "text2") };
            rightViewHeaderCtrl.ItemsSource = headers;
            // Assert
            Assert.That(rightViewHeaderCtrl.ItemsSource, Is.EqualTo(headers));
        }

        [Test]
        public void TestSelectedIndex()
        {
            // Act
            rightViewHeaderCtrl.SelectedIndex = 0;
            // Assert
            Assert.That(rightViewHeaderCtrl.SelectedIndex, Is.EqualTo(0));

            var rightViewHeaderCtrlViewModel = new RightViewHeaderCtrlViewModel();
            var privateobject = new PrivateObject(rightViewHeaderCtrlViewModel);
            privateobject.SetFieldOrProperty("_shownCount", 2);
            privateobject.SetFieldOrProperty("_caseNo", 2);
            privateObject.SetFieldOrProperty("vm", rightViewHeaderCtrlViewModel);
            rightViewHeaderCtrl.SelectedIndex = 2;

            // Assert
            Assert.That(rightViewHeaderCtrl.SelectedIndex, Is.EqualTo(0));
        }

    }
}
