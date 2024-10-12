using DDPM.UI.Common.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace DDPM.UI.Common.Tests
{
    [Apartment(ApartmentState.STA)]
    public class ActionButtonTests
    {
        private ActionButton? actionButton;

        [SetUp]
        public void Setup()
        {
            actionButton = new ActionButton();
        }


        [Test]
        public void TestConstructor_ActionButton()
        {
            // Assert
            Assert.That(actionButton, Is.Not.Null);
        }

        [Test]
        public void TestCaption()
        {
            // Act
            actionButton.Caption= "Caption";
            // Assert
            Assert.That(actionButton.Caption, Is.EqualTo("Caption"));
        }

        [Test]
        public void TestCornerRadius()
        {
            // Act
            var cornerRadius = new System.Windows.CornerRadius();
            actionButton.CornerRadius= cornerRadius;
            // Assert
            Assert.That(actionButton.CornerRadius, Is.EqualTo(cornerRadius));
        }

        [Test]
        public void TestBorderThickness()
        {
            // Act
            var BorderThickness = new Thickness();
            actionButton.BorderThickness = BorderThickness;
            // Assert
            Assert.That(actionButton.BorderThickness, Is.EqualTo(BorderThickness));
        }
    }
}
