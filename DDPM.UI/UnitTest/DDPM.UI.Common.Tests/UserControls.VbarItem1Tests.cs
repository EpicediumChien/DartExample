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
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Menu;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace DDPM.UI.Common.Tests
{
    [Apartment(ApartmentState.STA)]
    public class VbarItem1Tests
    {
        private VbarItem1? vbarItem1;
        private PrivateObject?privateObject;
        private VbarItemViewModel vm;

        [SetUp]
        public void Setup()
        {
            if (System.Windows.Application.Current == null)
            {
                new System.Windows.Application();
            }
            var resourceDictionary = new ResourceDictionary();
            resourceDictionary.Source = new Uri("pack://application:,,,/DDPM.UI.Common;component/ModuleStyle.xaml");
            System.Windows.Application.Current.Resources.MergedDictionaries.Add(resourceDictionary);
            vbarItem1 = new VbarItem1();
            privateObject=new PrivateObject(vbarItem1);
        }

        [Test]
        public void TestConstructor_VbarItem1()
        {
            // Assert
            Assert.That(vbarItem1, Is.Not.Null);
        }

        [Test]
        public void TestIndex()
        {
            vbarItem1.Index = 1;
            // Assert
            Assert.That(vbarItem1.Index, Is.EqualTo(1));
        }

        [Test]
        public void TestText()
        {
            vbarItem1.Text = "TEXT";
            // Assert
            Assert.That(vbarItem1.Text, Is.EqualTo("TEXT"));
        }

        [Test]
        public void TestIconTemplate()
        {
            var iconTemplate = new ControlTemplate();
            vbarItem1.IconTemplate = iconTemplate;
            // Assert
            Assert.That(vbarItem1.IconTemplate, Is.EqualTo(iconTemplate));
        }

        [Test]
        public void TestIconImage()
        {
            var iconImage = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Product_KB900.png");
            vbarItem1.IconImage = iconImage;
            // Assert
            Assert.That(vbarItem1.IconImage, Is.EqualTo(iconImage));
        }

        [Test]
        public void TestIsLandingMode()
        {
            vbarItem1.IsLandingMode = true;
            // Assert
            Assert.That(vbarItem1.IsLandingMode, Is.EqualTo(true));

            vbarItem1.IsLandingMode = false;
            // Assert
            Assert.That(vbarItem1.IsLandingMode, Is.EqualTo(false));
        }

        [Test]
        public void TestIsSelected()
        {
            vbarItem1.IsSelected = true;
            // Assert
            Assert.That(vbarItem1.IsSelected, Is.EqualTo(true));
        }

        [Test]
        public void TestIsLocked()
        {
            vbarItem1.IsLocked = true;
            // Assert
            Assert.That(vbarItem1.IsLocked, Is.EqualTo(true));
        }

        [Test]
        public void TestClickCommand()
        {
            var clickCommandMock = new Mock<ICommand>();
            vbarItem1.ClickCommand = clickCommandMock.Object;
            // Assert
            Assert.That(vbarItem1.ClickCommand, Is.EqualTo(clickCommandMock.Object));
        }

        [Test]
        public void TestLeaveHoverState()
        {
            try
            {
                vbarItem1.LeaveHoverState();
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }
    }
}
