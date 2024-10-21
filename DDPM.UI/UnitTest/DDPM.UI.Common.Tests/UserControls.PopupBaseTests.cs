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

namespace DDPM.UI.Common.Tests
{
    [Apartment(ApartmentState.STA)]
    public class PopupBaseTests
    {
        private PopupBase? popupBase;

        [SetUp]
        public void Setup()
        {
            popupBase = new PopupBase(true,true, "HeaderText", "SubHeaderText");
        }

        [Test]
        public void TestConstructor_PopupBase()
        {
            // Assert
            Assert.That(popupBase, Is.Not.Null);
            Assert.That(popupBase.Header.Text, Is.EqualTo("HeaderText"));
            Assert.That(popupBase.SubHeader.Text, Is.EqualTo("SubHeaderText"));
            Assert.That(popupBase.SubHeader.Visibility, Is.EqualTo(Visibility.Visible));
            Assert.That(popupBase.ButtonPanel.Visibility, Is.EqualTo(Visibility.Visible));
            Assert.That(popupBase.LeftButton.Content, Is.EqualTo(""));
            Assert.That(popupBase.RightButton.Content, Is.EqualTo(""));
        }


    }
}
