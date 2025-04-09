using DDPM.UI.Common.EAEM;
using NGA.UnitTest.PrivateObject;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace DDPM.Easy.Common.Tests
{
    [Apartment(ApartmentState.STA)]
    public class SplitCtrlVMTests
    {
        private SplitCtrlVM? splitCtrlVM;
        private SplitCtrlVM vm;
        private PrivateObject? privateObject;

        [SetUp]
        public void Setup()
        {
            splitCtrlVM = new SplitCtrlVM();
            privateObject = new PrivateObject(splitCtrlVM);
        }

        [Test]
        public void TestConstructor_SplitCtrlVM()
        {
            // Assert
            Assert.That(splitCtrlVM, Is.Not.Null);
        }


        [Test]
        public void TestSplitMode()
        {
            // Act
            var SplitMode=new eSplitModes();
            splitCtrlVM.SplitMode = SplitMode;
            // Assert
            Assert.That(splitCtrlVM.SplitMode, Is.EqualTo(SplitMode));
        }

        [Test]
        public void TestIsEditMode()
        {
            // Act

            // Assert
            Assert.That(splitCtrlVM.IsEditMode, Is.EqualTo(false));
        }

        [Test]
        public void TestIsWorkMode()
        {
            // Act

            // Assert
            Assert.That(splitCtrlVM.IsWorkMode, Is.EqualTo(false));
        }

        [Test]
        public void TestIsIconMode()
        {
            // Act

            // Assert
            Assert.That(splitCtrlVM.IsIconMode, Is.EqualTo(true));
        }

        [Test]
        public void TestIsAwsMode()
        {
            // Act

            // Assert
            Assert.That(splitCtrlVM.IsAwsMode, Is.EqualTo(false));
        }

        [Test]
        public void TestIsEditable()
        {
            // Act
            splitCtrlVM.IsEditable = true;
            // Assert
            Assert.That(splitCtrlVM.IsEditable, Is.EqualTo(true));
        }

        [Test]
        public void TestThickBorder()
        {            
            // Act
            splitCtrlVM.ThickBorder = 1.1;
            // Assert
            Assert.That(splitCtrlVM.ThickBorder, Is.EqualTo(1.1));

        }

        [Test]
        public void TestThicknessBorder()
        {
            // Act

            // Assert
            Assert.NotNull(splitCtrlVM.ThicknessBorder);
        }

        [Test]
        public void TestThickSplitter()
        {
            // Act
            splitCtrlVM.ThickSplitter = 1.1;
            // Assert
            Assert.That(splitCtrlVM.ThickSplitter, Is.EqualTo(1.1));
        }

        [Test]
        public void TestIsVertical()
        {
            // Act
            splitCtrlVM.IsVertical = true;
            // Assert
            Assert.That(splitCtrlVM.IsVertical, Is.EqualTo(true));
        }

        [Test]
        public void TestSettings()
        {
            // Act
            var Settings = new ObservableCollection<GridLength>();
            splitCtrlVM.Settings = Settings;
            // Assert
            Assert.That(splitCtrlVM.Settings, Is.EqualTo(Settings));
        }

        [Test]
        public void TestSettings_Double()
        {
            // Act
            var Settings = new ObservableCollection<GridLength>();
            splitCtrlVM.Settings = Settings;
            // Assert
            Assert.That(splitCtrlVM.Settings_Double, Is.Not.Null);
        }

        [Test]
        public void TestSettings_String()
        {
            // Act
            var Settings = new ObservableCollection<GridLength>();
            splitCtrlVM.Settings = Settings;
            // Assert
            Assert.That(splitCtrlVM.Settings_String, Is.Not.Null);
        }


        [Test]
        public void TestGridLength_To_Double()
        {
            // Act
            var result = SplitCtrlVM.GridLength_To_Double(new ObservableCollection<GridLength>());

            // Assert
            Assert.That(result, Is.Not.Null);
        }


        [Test]
        public void TestString_To_Double()
        {
            // Act
            var result = SplitCtrlVM.String_To_Double("1,2,1.33,2,1");

            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void TestDouble_To_GridLength()
        {
            // Act
            var result = SplitCtrlVM.Double_To_GridLength(new List<double>());

            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void TestDouble_To_String()
        {
            // Act
            var result = SplitCtrlVM.Double_To_String(new List<double>() { 1, 2, 1, 0.8, 1, 4.52 });

            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void TestHoveringCell()
        {
            // Act
            splitCtrlVM.HoveringCell = "a";
            // Assert
            Assert.That(splitCtrlVM.HoveringCell, Is.EqualTo("a"));
        }



        [TearDown]
        public void TearDown()
        {
            if (splitCtrlVM != null)
            {
                splitCtrlVM.Dispose();
                splitCtrlVM = null;
            }
            if (vm != null)
            {
                vm.Dispose();
                vm = null;
            }
        }
    }
}
