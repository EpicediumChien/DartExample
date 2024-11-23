using DDPM.UI.Common;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Windows.Devices.Radios;
using static DDPM.Easy.Common.CellBorder;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Brushes = System.Windows.Media.Brushes;
using Dell.Client.Framework.UX.WPF.ResourceManager;

namespace DDPM.Easy.Common.Tests
{
    [Apartment(ApartmentState.STA)]
    public class CellBorderTests
    {
        private CellBorder? cellBorder;
        private CellAppData? cellAppData;

        [SetUp]
        public void Setup()
        {
            if (System.Windows.Application.Current == null)
            {
                new System.Windows.Application();
            }
            ResourceManager res = new ResourceManager();
            var resourceDictionary = new ResourceDictionary();
            resourceDictionary.Source = new Uri("pack://application:,,,/DDPM.UI.Common;component/ModuleStyle.xaml");
            System.Windows.Application.Current.Resources.MergedDictionaries.Add(resourceDictionary);
            cellBorder = new CellBorder();
            cellAppData = new CellAppData();
        }

        [Test]
        public void TestConstructor_CellBorder()
        {
            // Assert
            Assert.That(cellBorder, Is.Not.Null);
        }

        [Test]
        public void TestConstructor_CellAppData()
        {
            // Assert
            Assert.That(cellAppData, Is.Not.Null);
        }

        [Test]
        public void TestCellName()
        {
            // Act        
            cellBorder.CellName = "A";
            // Assert
            Assert.That(cellBorder.CellName, Is.EqualTo("A"));
        }

        [Test]
        public void TestBorderThickness()
        {
            // Act
            var borderThickness = new Thickness();
            cellBorder.BorderThickness = borderThickness;
            // Assert
            Assert.That(cellBorder.BorderThickness, Is.EqualTo(borderThickness));
        }

        [Test]
        public void TestBorderBrush()
        {
            // Act
            var borderBrush = Brushes.White;
            cellBorder.BorderBrush = borderBrush;
            // Assert
            Assert.That(cellBorder.BorderBrush, Is.EqualTo(borderBrush));
        }

        [Test]
        public void TestCornerRadius()
        {
            // Act
            var cornerRadius = new CornerRadius();
            cellBorder.CornerRadius = cornerRadius;
            // Assert
            Assert.That(cellBorder.CornerRadius, Is.EqualTo(cornerRadius));
        }

        [Test]
        public void TestRadius()
        {
            // Act
            var radius = new CornerRadius();
            cellBorder.Radius = radius;
            // Assert
            Assert.That(cellBorder.Radius, Is.EqualTo(radius));
        }

        [Test]
        public void TestBkBrush()
        {
            // Act
            var bkBrush = Brushes.White;
            cellBorder.BkBrush = bkBrush;
            // Assert
            Assert.That(cellBorder.BkBrush, Is.EqualTo(bkBrush));
        }

        [Test]
        public void TestIsHover()
        {
            // Act
            cellBorder.IsHover = true;
            // Assert
            Assert.That(cellBorder.IsHover, Is.EqualTo(true));
        }

        [Test]
        public void Testrect()
        {
            // Act
            var rect = new Rect();
            cellBorder.rect = rect;
            // Assert
            Assert.That(cellBorder.rect, Is.EqualTo(rect));
        }

        [Test]
        public void TestrcRatio()
        {
            // Act
            var rect = new Rect();
            cellBorder.rcRatio = rect;
            // Assert
            Assert.That(cellBorder.rcRatio, Is.EqualTo(rect));
        }

        [Test]
        public void TestBorder()
        {
            // Act
            // Assert
            Assert.That(cellBorder.Border, Is.Not.Null);
        }

        [Test]
        public void TestDispatcher_SetIsHover()
        {
            try
            {
                cellBorder.Dispatcher_SetIsHover(true);
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }

        [Test]
        public void TestAddChild()
        {
            try
            {
                cellBorder.AddChild(new UIElement());
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }

        [Test]
        public void TestIsEmMode()
        {
            // Act
            cellBorder.IsEmMode = true;
            // Assert
            Assert.That(cellBorder.IsEmMode, Is.EqualTo(true));
        }

        [Test]
        public void TestMemoryImage()
        {
            // Act
            var ImageSource = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Product_KB900.png");
            cellBorder.MemoryImage = ImageSource;
            // Assert
            Assert.That(cellBorder.MemoryImage, Is.EqualTo(ImageSource));
        }

        [Test]
        public void TestMemoryText()
        {
            // Act
            cellBorder.MemoryText = "MemoryText";
            // Assert
            Assert.That(cellBorder.MemoryText, Is.EqualTo("MemoryText"));
        }

        [Test]
        public void TestCellNumber()
        {
            // Act
            cellBorder.CellNumber = 1;
            // Assert
            Assert.That(cellBorder.CellNumber, Is.EqualTo(1));
        }

        //class CellAppData
        [Test]
        public void TestNumber()
        {
            // Act
            cellAppData.Number = 1;
            // Assert
            Assert.That(cellAppData.Number, Is.EqualTo(1));
        }

        [Test]
        public void TestFileName()
        {
            // Act
            cellAppData.FileName = "FileName";
            // Assert
            Assert.That(cellAppData.FileName, Is.EqualTo("FileName"));
        }

        [Test]
        public void TestFilePath()
        {
            // Act
            cellAppData.FilePath = "FilePath";
            // Assert
            Assert.That(cellAppData.FilePath, Is.EqualTo("FilePath"));
        }

        [Test]
        public void TestImage()
        {
            // Act
            var BitmapImage = new System.Windows.Media.Imaging.BitmapImage();
            cellAppData.Image = BitmapImage;
            // Assert
            Assert.That(cellAppData.Image, Is.EqualTo(BitmapImage));
        }

        [Test]
        public void TestCell()
        {
            // Act
            var cell = new CellBorder();
            cellAppData.Cell = cell;
            // Assert
            Assert.That(cellAppData.Cell, Is.EqualTo(cell));
        }

        [Test]
        public void TestCellAppData()
        {
            // Act
            var cellAppData = new CellAppData(2, "fileName", "filePath",new BitmapImage(), new CellBorder());
            // Assert
            Assert.That(cellAppData, Is.Not.Null);
        }
    }
}
