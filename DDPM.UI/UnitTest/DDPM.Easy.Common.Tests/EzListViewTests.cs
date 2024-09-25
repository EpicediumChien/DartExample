using Moq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace DDPM.Easy.Common.Tests
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
            // Act
            var ItemsSourceMock = new Mock<IEnumerable>();
            var ItemsSource = ItemsSourceMock.Object;
            ezListView.ItemsSource = ItemsSource;
            // Assert
            Assert.That(ezListView.ItemsSource, Is.EqualTo(ItemsSource));
        }

    }
}
