using DDPM.SA.Common;
using DDPM.UI.Common.Models;
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
    public class RightViewHeaderCtrlViewModelTests
    {
        private RightViewHeaderCtrlViewModel? rightViewHeaderCtrlViewModel;
        private PrivateObject? privateObject;


        [SetUp]
        public void Setup()
        {
            rightViewHeaderCtrlViewModel = new RightViewHeaderCtrlViewModel();
            privateObject = new PrivateObject(rightViewHeaderCtrlViewModel);
        }

        [Test]
        public void TestConstructor_RightViewHeaderCtrlViewModel()
        {
            // Assert
            Assert.That(rightViewHeaderCtrlViewModel, Is.Not.Null);
        }

        [Test]
        public void TestText1()
        {
            rightViewHeaderCtrlViewModel.Text1 = "Text1";

            // Assert
            Assert.That(rightViewHeaderCtrlViewModel.Text1, Is.EqualTo("Text1"));
        }

        [Test]
        public void TestText2()
        {
            rightViewHeaderCtrlViewModel.Text2 = "Text2";

            // Assert
            Assert.That(rightViewHeaderCtrlViewModel.Text2, Is.EqualTo("Text2"));
        }

        [Test]
        public void TestText3()
        {
            rightViewHeaderCtrlViewModel.Text3 = "Text3";

            // Assert
            Assert.That(rightViewHeaderCtrlViewModel.Text3, Is.EqualTo("Text3"));
        }

        [Test]
        public void TestLocker1()
        {
            rightViewHeaderCtrlViewModel.Locker1 = Visibility.Visible;

            // Assert
            Assert.That(rightViewHeaderCtrlViewModel.Locker1, Is.EqualTo(Visibility.Visible));
        }

        [Test]
        public void TestLocker2()
        {
            rightViewHeaderCtrlViewModel.Locker2 = Visibility.Visible;

            // Assert
            Assert.That(rightViewHeaderCtrlViewModel.Locker2, Is.EqualTo(Visibility.Visible));
        }

        [Test]
        public void TestLocker3()
        {
            rightViewHeaderCtrlViewModel.Locker3 = Visibility.Visible;

            // Assert
            Assert.That(rightViewHeaderCtrlViewModel.Locker3, Is.EqualTo(Visibility.Visible));
        }

        [Test]
        public void TestSetHeaders()
        {
            //_itemCount = 1
            var headers = new RightViewHeader[] { new RightViewHeader(0, "A") };
            try
            {
                rightViewHeaderCtrlViewModel.SetHeaders(headers);
                Assert.True(true);
                Assert.That(rightViewHeaderCtrlViewModel.CtrlVisibility, Is.EqualTo(Visibility.Collapsed));
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
            //_itemCount == 3,_shownCount == 2
            headers = new RightViewHeader[] { new RightViewHeader(0, "A") { IsShown = true }, new RightViewHeader(1, "B") { IsShown = true }, new RightViewHeader(2, "C") { IsShown = false } };
            try
            {
                rightViewHeaderCtrlViewModel.SetHeaders(headers);
                Assert.True(true);
                Assert.That(rightViewHeaderCtrlViewModel.CtrlVisibility, Is.EqualTo(Visibility.Visible));
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
            //_itemCount == 3,_shownCount == 3
            headers = new RightViewHeader[] { new RightViewHeader(0, "A") { IsShown = true }, new RightViewHeader(1, "B") { IsShown = true }, new RightViewHeader(2, "C") { IsShown = true } };
            try
            {
                rightViewHeaderCtrlViewModel.SetHeaders(headers);
                Assert.True(true);
                Assert.That(rightViewHeaderCtrlViewModel.CtrlVisibility, Is.EqualTo(Visibility.Visible));
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
            //!headers[1].IsShown
            headers = new RightViewHeader[] { new RightViewHeader(0, "A") { IsShown = true }, new RightViewHeader(1, "B") { IsShown = false }, new RightViewHeader(2, "C") { IsShown = true } };
            try
            {
                rightViewHeaderCtrlViewModel.SetHeaders(headers);
                Assert.True(true);
                Assert.That(rightViewHeaderCtrlViewModel.CtrlVisibility, Is.EqualTo(Visibility.Visible));
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
            //!headers[2].IsShown
            headers = new RightViewHeader[] { new RightViewHeader(0, "A") { IsShown = true }, new RightViewHeader(1, "B") { IsShown = true }, new RightViewHeader(2, "C") { IsShown = false } };
            try
            {
                rightViewHeaderCtrlViewModel.SetHeaders(headers);
                Assert.True(true);
                Assert.That(rightViewHeaderCtrlViewModel.CtrlVisibility, Is.EqualTo(Visibility.Visible));
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
            //_itemCount == 2,_shownCount == 1
            headers = new RightViewHeader[] { new RightViewHeader(0, "A") { IsShown = true }, new RightViewHeader(1, "B") { IsShown = false } };
            try
            {
                rightViewHeaderCtrlViewModel.SetHeaders(headers);
                Assert.True(true);
                Assert.That(rightViewHeaderCtrlViewModel.CtrlVisibility, Is.EqualTo(Visibility.Collapsed));
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
            //_itemCount == 2,_shownCount == 1
            headers = new RightViewHeader[] { new RightViewHeader(0, "A") { IsShown = true }, new RightViewHeader(1, "B") { IsShown = true } };
            try
            {
                rightViewHeaderCtrlViewModel.SetHeaders(headers);
                Assert.True(true);
                Assert.That(rightViewHeaderCtrlViewModel.CtrlVisibility, Is.EqualTo(Visibility.Visible));
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }

        [Test]
        public void TestCol1Width()
        {
            var col1Width = new GridLength(10);
            rightViewHeaderCtrlViewModel.Col1Width = col1Width;

            // Assert
            Assert.That(rightViewHeaderCtrlViewModel.Col1Width, Is.EqualTo(col1Width));
        }

        [Test]
        public void TestCtrlVisibility()
        {
            rightViewHeaderCtrlViewModel.CtrlVisibility = Visibility.Visible;

            // Assert
            Assert.That(rightViewHeaderCtrlViewModel.CtrlVisibility, Is.EqualTo(Visibility.Visible));
        }

        [Test]
        public void TestInternalSelectedIndex()
        {
            rightViewHeaderCtrlViewModel.InternalSelectedIndex = 2;

            // Assert
            Assert.That(rightViewHeaderCtrlViewModel.InternalSelectedIndex, Is.EqualTo(2));
        }

        [Test]
        public void TestSelectedIndex()
        {
            privateObject.SetFieldOrProperty("_shownCount", 1);
            rightViewHeaderCtrlViewModel.SelectedIndex = 0;
            Assert.That(rightViewHeaderCtrlViewModel.InternalSelectedIndex, Is.EqualTo(0));
            Assert.That(rightViewHeaderCtrlViewModel.SelectedIndex, Is.EqualTo(0));
            //
            privateObject.SetFieldOrProperty("_shownCount", 2);
            privateObject.SetFieldOrProperty("_caseNo", 2);
            rightViewHeaderCtrlViewModel.SelectedIndex = 0;
            Assert.That(rightViewHeaderCtrlViewModel.InternalSelectedIndex, Is.EqualTo(0));
            rightViewHeaderCtrlViewModel.SelectedIndex = 1;
            Assert.That(rightViewHeaderCtrlViewModel.InternalSelectedIndex, Is.EqualTo(2));

            privateObject.SetFieldOrProperty("_internalSelectedIndex", 1);
            privateObject.SetFieldOrProperty("_caseNo", 3);
            rightViewHeaderCtrlViewModel.SelectedIndex = 0;
            Assert.That(rightViewHeaderCtrlViewModel.InternalSelectedIndex, Is.EqualTo(0));
            rightViewHeaderCtrlViewModel.SelectedIndex = 2;
            Assert.That(rightViewHeaderCtrlViewModel.InternalSelectedIndex, Is.EqualTo(2));
            Assert.That(rightViewHeaderCtrlViewModel.SelectedIndex, Is.EqualTo(0));

            privateObject.SetFieldOrProperty("_internalSelectedIndex", 2);
            privateObject.SetFieldOrProperty("_caseNo", 4);
            rightViewHeaderCtrlViewModel.SelectedIndex = 1;
            Assert.That(rightViewHeaderCtrlViewModel.InternalSelectedIndex, Is.EqualTo(0));
            rightViewHeaderCtrlViewModel.SelectedIndex = 2;
            Assert.That(rightViewHeaderCtrlViewModel.InternalSelectedIndex, Is.EqualTo(2));
            Assert.That(rightViewHeaderCtrlViewModel.SelectedIndex, Is.EqualTo(0));

            privateObject.SetFieldOrProperty("_caseNo", 8);
            rightViewHeaderCtrlViewModel.SelectedIndex = 0;
            Assert.That(rightViewHeaderCtrlViewModel.InternalSelectedIndex, Is.EqualTo(0));
            rightViewHeaderCtrlViewModel.SelectedIndex = 1;
            Assert.That(rightViewHeaderCtrlViewModel.InternalSelectedIndex, Is.EqualTo(2));

            privateObject.SetFieldOrProperty("_caseNo", 1);
            rightViewHeaderCtrlViewModel.SelectedIndex = 0;
            Assert.That(rightViewHeaderCtrlViewModel.InternalSelectedIndex, Is.EqualTo(2));
        }
    }
}
