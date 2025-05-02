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
    public class TabHeadersTests
    {
        private TabHeaders? tabHeaders;

        [SetUp]
        public void Setup()
        {
            tabHeaders = new TabHeaders();
        }

        [Test]
        public void TestConstructor_TabHeaders()
        {
            // Assert
            Assert.That(tabHeaders, Is.Not.Null);
        }

        [Test]
        public void TestItemsSource()
        {
            var itemsSource = new List<ITabHeader>();
            tabHeaders.ItemsSource = itemsSource;
            // Assert
            Assert.That(tabHeaders.ItemsSource, Is.EqualTo(itemsSource));
        }

        [Test]
        public void TestText1()
        {
            tabHeaders.Text1 = "Text1";
            // Assert
            Assert.That(tabHeaders.Text1, Is.EqualTo("Text1"));
        }

        [Test]
        public void TestText2()
        {
            tabHeaders.Text2 = "Text2";
            // Assert
            Assert.That(tabHeaders.Text2, Is.EqualTo("Text2"));
        }

        [Test]
        public void TestText3()
        {
            tabHeaders.Text3 = "Text3";
            // Assert
            Assert.That(tabHeaders.Text3, Is.EqualTo("Text3"));
        }

        [Test]
        public void TestSelectedIndex()
        {
            var privateobject = new PrivateObject(tabHeaders);
            privateobject.SetFieldOrProperty("_itemCount",3);
            tabHeaders.SelectedIndex = 2;
            // Assert
            Assert.That(tabHeaders.SelectedIndex, Is.EqualTo(2));
            Assert.That(tabHeaders.HightlightHeaderIndex, Is.EqualTo(2));

            privateobject.SetFieldOrProperty("_itemCount", 2);
            tabHeaders.SelectedIndex = 2;
            // Assert
            Assert.That(tabHeaders.SelectedIndex, Is.EqualTo(2));
            Assert.That(tabHeaders.HightlightHeaderIndex, Is.EqualTo(2));

            tabHeaders.SelectedIndex = 1;
            // Assert
            Assert.That(tabHeaders.SelectedIndex, Is.EqualTo(1));
            Assert.That(tabHeaders.HightlightHeaderIndex, Is.EqualTo(2));
        }

        [Test]
        public void TestHightlightHeaderIndex()
        {
            tabHeaders.HightlightHeaderIndex = 2;
            // Assert
            Assert.That(tabHeaders.HightlightHeaderIndex, Is.EqualTo(2));
        }

        [Test]
        public void TestClickCommand()
        {
            var clickCommandMock=new Mock<ICommand>();
            tabHeaders.ClickCommand = clickCommandMock.Object;
            // Assert
            Assert.That(tabHeaders.ClickCommand, Is.EqualTo(clickCommandMock.Object));
        }
    }
}
