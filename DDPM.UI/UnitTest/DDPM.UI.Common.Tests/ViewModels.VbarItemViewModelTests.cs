using DDPM.SA.Common;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using DDPM.UI.Common.UserControls;
using DDPM.UI.Common.ViewModels;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using Moq;
using NGA.UnitTest.PrivateObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using VcpCore.Common;

namespace DDPM.UI.Common.Tests
{
    [Apartment(ApartmentState.STA)]
    public class VbarItemViewModelTests
    {
        private VbarItemViewModel? vbarItemViewModel;
        private PrivateObject? privateObject;


        [SetUp]
        public void Setup()
        {
            vbarItemViewModel = new VbarItemViewModel();
            privateObject = new PrivateObject(vbarItemViewModel);
        }

        [Test]
        public void TestConstructor_VbarItemViewModel()
        {
            // Assert
            Assert.That(vbarItemViewModel, Is.Not.Null);
        }

        [Test]
        public void TestId()
        {
            vbarItemViewModel.Id = 2;

            // Assert
            Assert.That(vbarItemViewModel.Id, Is.EqualTo(2));
        }

        [Test]
        public void TestIcon()
        {
            var icon = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Product_KB900.png");

            vbarItemViewModel.Icon = icon;

            // Assert
            Assert.That(vbarItemViewModel.Icon, Is.EqualTo(icon));
        }

        [Test]
        public void TestText()
        {
            vbarItemViewModel.Text = "Text";

            // Assert
            Assert.That(vbarItemViewModel.Text, Is.EqualTo("Text"));
        }

        [Test]
        public void TestVisibility()
        {
            vbarItemViewModel.Visibility = Visibility.Visible;

            // Assert
            Assert.That(vbarItemViewModel.Visibility, Is.EqualTo(Visibility.Visible));
        }

        [Test]
        public void TestItemClickCommand()
        {
            var itemClickCommandMock = new Mock<ICommand>();
            vbarItemViewModel.ItemClickCommand = itemClickCommandMock.Object;

            // Assert
            Assert.That(vbarItemViewModel.ItemClickCommand, Is.EqualTo(itemClickCommandMock.Object));
        }

        [Test]
        public void TestIsLandingMode()
        {
            vbarItemViewModel.IsLandingMode = true;

            // Assert
            Assert.That(vbarItemViewModel.IsLandingMode, Is.EqualTo(true));
        }

        [Test]
        public void TestIsSelected()
        {
            vbarItemViewModel.IsSelected = true;

            // Assert
            Assert.That(vbarItemViewModel.IsSelected, Is.EqualTo(true));
        }

        [Test]
        public void TestTooltipVisibility()
        {
            vbarItemViewModel.TooltipVisibility = Visibility.Visible;

            // Assert
            Assert.That(vbarItemViewModel.TooltipVisibility, Is.EqualTo(Visibility.Visible));
        }
    }
}
