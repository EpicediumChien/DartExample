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

namespace DDPM.UI.Common.Tests
{
    [Apartment(ApartmentState.STA)]
    public class PathIconTests
    {
        private PathIcon? pathIcon;

        [SetUp]
        public void Setup()
        {
            pathIcon = new PathIcon();
        }

        [Test]
        public void TestConstructor_PathIcon()
        {
            // Assert
            Assert.That(pathIcon, Is.Not.Null);
        }

        [Test]
        public void TestPathData()
        {
            pathIcon.PathData = "PathData";
            // Assert
            Assert.That(pathIcon.PathData, Is.EqualTo("PathData"));
        }

        [Test]
        public void TestPathFill()
        {
            var PathFill = System.Windows.Media.Brushes.White;
            pathIcon.PathFill = PathFill;
            // Assert
            Assert.That(pathIcon.PathFill, Is.EqualTo(PathFill));
        }

        [Test]
        public void TestClickCommand()
        {
            var clickCommandMock = new Mock<ICommand>();
            var clickCommand = clickCommandMock.Object;
            pathIcon.ClickCommand = clickCommand;
            // Assert
            Assert.That(pathIcon.ClickCommand, Is.EqualTo(clickCommand));
        }

        [Test]
        public void TestTooltipText()
        {
            pathIcon.TooltipText = "TooltipText";
            // Assert
            Assert.That(pathIcon.TooltipText, Is.EqualTo("TooltipText"));
        }

        [Test]
        public void TestGlowEffect_Start()
        {
            try
            {
                pathIcon.GlowEffect_Start();
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }

        [Test]
        public void TestGlowEffect_Stop()
        {
            try
            {
                pathIcon.GlowEffect_Stop();
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }

    }
}
