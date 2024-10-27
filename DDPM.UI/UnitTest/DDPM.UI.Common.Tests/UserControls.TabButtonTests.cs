using DDPM.Easy.Common;
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
    public class TabButtonTests
    {
        private TabButton? tabButton;

        [SetUp]
        public void Setup()
        {
            tabButton = new TabButton();
        }

        [Test]
        public void TestConstructor_TabButton()
        {
            // Assert
            Assert.That(tabButton, Is.Not.Null);
        }


        [Test]
        public void TestMainImageSource()
        {
            var imageSource= DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Product_KB900.png");
            tabButton.MainImageSource = imageSource;
            // Assert
            Assert.That(tabButton.MainImageSource, Is.EqualTo(imageSource));
        }

        [Test]
        public void TestCaption()
        {
            tabButton.Caption = "Caption";
            // Assert
            Assert.That(tabButton.Caption, Is.EqualTo("Caption"));
        }

        [Test]
        public void TestTabInfo()
        {
            tabButton.TabInfo = "TabInfo";
            // Assert
            Assert.That(tabButton.TabInfo, Is.EqualTo("TabInfo"));
        }

        [Test]
        public void TestFocused()
        {
            tabButton.Focused = true;
            // Assert
            Assert.That(tabButton.Focused, Is.EqualTo(true));
        }

        [Test]
        public void TestHasInfoIcon()
        {
            tabButton.HasInfoIcon = true;
            // Assert
            Assert.That(tabButton.HasInfoIcon, Is.EqualTo(true));
        }

        [Test]
        public void TestCommand()
        {
            var commandMock = new Mock<ICommand>();
            tabButton.Command = commandMock.Object;
            // Assert
            Assert.That(tabButton.Command, Is.EqualTo(commandMock.Object));
        }

        [Test]
        public void TestCaptionSize()
        {
            tabButton.CaptionSize = 2;
            // Assert
            Assert.That(tabButton.CaptionSize, Is.EqualTo(2));
        }

        [Test]
        public void TestBorderThickness()
        {
            var borderThickness = new Thickness();
            tabButton.BorderThickness = borderThickness;
            // Assert
            Assert.That(tabButton.BorderThickness, Is.EqualTo(borderThickness));
        }

        [Test]
        public void TestCornerRadius()
        {
            var cornerRadius = new CornerRadius();
            tabButton.CornerRadius = cornerRadius;
            // Assert
            Assert.That(tabButton.CornerRadius, Is.EqualTo(cornerRadius));
        }

        [Test]
        public void TestBackground()
        {
            var background = new System.Windows.Media.Color();
            tabButton.Background = background;
            // Assert
            Assert.That(tabButton.Background, Is.EqualTo(background));
        }

        [Test]
        public void TestTooltipOffsetH()
        {
            tabButton.TooltipOffsetH = 2;
            // Assert
            Assert.That(tabButton.TooltipOffsetH, Is.EqualTo(2));
        }
    }
}
