using DDPM.Easy.Common;
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
using VcpCore.Common;

namespace DDPM.UI.Common.Tests
{
    [Apartment(ApartmentState.STA)]
    public class SplitItemViewModelTests
    {
        private SplitItemViewModel? splitItemViewModel;
        private PrivateObject? privateObject;


        [SetUp]
        public void Setup()
        {
            splitItemViewModel = new SplitItemViewModel();
            privateObject = new PrivateObject(splitItemViewModel);
        }

        [Test]
        public void TestConstructor_SplitItemViewModel()
        {
            // Assert
            Assert.That(splitItemViewModel, Is.Not.Null);
        }

        [Test]
        public void TestToolTipText()
        {
            // Assert
            Assert.That(splitItemViewModel.ToolTipText, Is.EqualTo(""));

            var SplitCtrlMock = new Mock<ISplitCtrl>();
            SplitCtrlMock.Setup(s => s.FriendlyName).Returns("FriendlyName");
            splitItemViewModel.SplitCtrl = SplitCtrlMock.Object;
            Assert.That(splitItemViewModel.ToolTipText, Is.EqualTo("[0]FriendlyName"));

            var _splitMock=new Mock<ISplit>();
            _splitMock.Setup(s => s.Description).Returns("Description");
            privateObject.SetFieldOrProperty("_split", _splitMock.Object);
            Assert.That(splitItemViewModel.ToolTipText, Is.EqualTo("Description"));
        }

        [Test]
        public void TestNotifyPropertyChanged_TooltipText()
        {
            try
            {
                splitItemViewModel.NotifyPropertyChanged_TooltipText();
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }

        [Test]
        public void TestCustomId()
        {
            splitItemViewModel.CustomId = 2;

            // Assert
            Assert.That(splitItemViewModel.CustomId, Is.EqualTo(2));
        }

        [Test]
        public void TestIsVertical()
        {
            splitItemViewModel.IsVertical = true;

            // Assert
            Assert.That(splitItemViewModel.IsVertical, Is.EqualTo(true));
        }

        [Test]
        public void TestIsHoverable()
        {
            splitItemViewModel.IsHoverable = true;

            // Assert
            Assert.That(splitItemViewModel.IsHoverable, Is.EqualTo(true));
        }
        

    }
}
