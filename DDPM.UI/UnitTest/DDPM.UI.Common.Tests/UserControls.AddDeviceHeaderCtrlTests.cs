using DDPM.UI.Common.Models;
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

namespace DDPM.UI.Common.Tests
{
    [Apartment(ApartmentState.STA)]
    public class AddDeviceHeaderCtrlTests
    {
        private AddDeviceHeaderCtrl? addDeviceHeaderCtrl;
        private PrivateObject? privateObject;

        [SetUp]
        public void Setup()
        {
            addDeviceHeaderCtrl = new AddDeviceHeaderCtrl();
            privateObject = new PrivateObject(addDeviceHeaderCtrl);
        }

        [Test]
        public void TestConstructor_AddDeviceHeaderCtrl()
        {
            // Assert
            Assert.That(addDeviceHeaderCtrl, Is.Not.Null);
            Assert.That(addDeviceHeaderCtrl.DataContext, Is.EqualTo(privateObject.GetFieldOrProperty("vm")));
        }

        [Test]
        public void TestSetHeaders()
        {
            try
            {
                addDeviceHeaderCtrl.SetHeaders(new RightViewHeader[2] {new RightViewHeader(1,"A"),new RightViewHeader(2,"B") });
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
            // Act
            var itemsSource = new RightViewHeader[] { new RightViewHeader(1, "itemsource") };
            addDeviceHeaderCtrl.ItemsSource = itemsSource;
            // Assert
            Assert.That(addDeviceHeaderCtrl.ItemsSource, Is.EqualTo(itemsSource));
        }

        int i= 0;
        [Test]
        public void TestSelectedIndex()
        {
            // Act
            addDeviceHeaderCtrl.SelectionChanged += new RoutedEventHandler(newRoutedEventArgs);
            addDeviceHeaderCtrl.SelectedIndex = 0;
            // Assert
            Assert.That(i, Is.EqualTo(2));
            Assert.That(addDeviceHeaderCtrl.SelectedIndex, Is.EqualTo(0));

            var rightViewHeaderCtrlViewModel=new RightViewHeaderCtrlViewModel();
            var privateobject = new PrivateObject(rightViewHeaderCtrlViewModel);
            privateobject.SetFieldOrProperty("_shownCount",2);
            privateobject.SetFieldOrProperty("_caseNo", 2);
            privateObject.SetFieldOrProperty("vm", rightViewHeaderCtrlViewModel);        
            addDeviceHeaderCtrl.SelectedIndex = 2;
            
            // Assert
            Assert.That(i, Is.EqualTo(2));
            Assert.That(addDeviceHeaderCtrl.SelectedIndex, Is.EqualTo(0));
        }

        private void newRoutedEventArgs(object? sender, RoutedEventArgs e)
        {
            i = 2;
        }



    }
}
