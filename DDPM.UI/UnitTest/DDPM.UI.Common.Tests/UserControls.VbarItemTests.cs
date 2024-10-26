using DDPM.Easy.Common;
using DDPM.UI.Common.Interfaces;
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
using System.Windows.Input;
using System.Windows.Media;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace DDPM.UI.Common.Tests
{
    [Apartment(ApartmentState.STA)]
    public class VbarItemTests
    {
        private VbarItem? vbarItem;
        private PrivateObject?privateObject;
        private VbarItemViewModel vm;

        [SetUp]
        public void Setup()
        {
            vbarItem = new VbarItem(1, DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Product_KB900.png"),"A");
            privateObject=new PrivateObject(vbarItem);
            vm = new VbarItemViewModel() {Text="TEXT",Id=1 };
            privateObject.SetFieldOrProperty("vm", vm);
        }

        [Test]
        public void TestConstructor_VbarItem()
        {
            // Assert
            Assert.That(vbarItem, Is.Not.Null);
        }

        [Test]
        public void TestId()
        {
            // Assert
            Assert.That(vbarItem.Id, Is.EqualTo(1));
        }

        [Test]
        public void TestText()
        {
            // Assert
            Assert.That(vbarItem.Text, Is.EqualTo("TEXT"));
        }

        [Test]
        public void TestClickCommand()
        {
            var clickCommandMock = new Mock<ICommand>();
            vbarItem.ClickCommand = clickCommandMock.Object;
            // Assert
            Assert.That(vbarItem.ClickCommand, Is.EqualTo(clickCommandMock.Object));
        }

        [Test]
        public void TestSetLadningMode()
        {
            try
            {
                vbarItem.SetLadningMode(true);
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }

            try
            {
                vbarItem.SetLadningMode(false);
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }


        [Test]
        public void TestIsSelected()
        {
            vbarItem.IsSelected = true;
            // Assert
            Assert.That(vbarItem.IsSelected, Is.EqualTo(true));
        }
         
        [Test]
        public void TestTooltipVisibility()
        {
            vbarItem.TooltipVisibility = Visibility.Visible;
            // Assert
            Assert.That(vbarItem.TooltipVisibility, Is.EqualTo(Visibility.Visible));
        }
 
    }
}
