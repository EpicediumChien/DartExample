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
    public class EzListViewTests
    {
        private EzListView? ezListView;

        [SetUp]
        public void Setup()
        {
            ezListView = new EzListView();
        }

        [Test]
        public void TestConstructor_EzListView()
        {
            // Assert
            Assert.That(ezListView, Is.Not.Null);
        }

        [Test]
        public void TestItemsSource()
        {
            var itemsSourceMock = new Mock<IEnumerable>();
            var itemsSource = itemsSourceMock.Object;
            ezListView.ItemsSource=itemsSource;
            // Assert
            Assert.That(ezListView.ItemsSource, Is.EqualTo(itemsSource));
        }
    }
}
