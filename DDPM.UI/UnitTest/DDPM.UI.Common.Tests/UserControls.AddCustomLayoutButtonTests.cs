using DDPM.UI.Common.Models;
using DDPM.UI.Common.UserControls;
using Moq;
using System;
using System.Collections.Generic;
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
    public class AddCustomLayoutButtonTests
    {
        private AddCustomLayoutButton? addCustomLayoutButton;

        [SetUp]
        public void Setup()
        {
            addCustomLayoutButton = new AddCustomLayoutButton();
        }


        [Test]
        public void TestConstructor_AddCustomLayoutButton()
        {
            // Assert
            Assert.That(addCustomLayoutButton, Is.Not.Null);
        }

        [Test]
        public void TestClickCommand()
        {
            // Act
            var clickCommandMock = new Mock<ICommand>();
            var clickCommand = clickCommandMock.Object;
            addCustomLayoutButton.ClickCommand = clickCommand;
            // Assert
            Assert.That(addCustomLayoutButton.ClickCommand, Is.EqualTo(clickCommand));
        }
    }
}
