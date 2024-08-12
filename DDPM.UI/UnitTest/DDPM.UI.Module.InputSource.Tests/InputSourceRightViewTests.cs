using DDPM.UI.Common;
using NGA.UnitTest.PrivateObject;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.ViewManagement;

namespace DDPM.UI.Module.InputSource.Tests
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public class InputSourceRightViewTests
    {
        private InputSourceRightView? inputSourceRightView;
        private PrivateObject? privateObject;
        private InputSourceViewModel? inputSourceViewModel;

        [SetUp]
        public void SetUp()
        {

            inputSourceViewModel=new InputSourceViewModel();
            inputSourceRightView = new InputSourceRightView(inputSourceViewModel);
            privateObject = new PrivateObject(inputSourceRightView);
        }

        [Test]
        public void TestConstructor_InputSourceRightView()
        {
            // Assert
            Assert.That(inputSourceRightView, Is.Not.Null);
            Assert.That(inputSourceRightView.DataContext, Is.EqualTo(inputSourceViewModel));
        }


        [Test]
        public void TestIndex()
        {
            Item item = new Item();
            int index = 2;
            item.Index = index;

            // Assert
            Assert.That(item.Index, Is.EqualTo(index));
        }

        [Test]
        public void TestInputImage()
        {
            Item item = new Item();
            var inputImage = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/HDMI.png");
            item.InputImage = inputImage;

            // Assert
            Assert.That(item.InputImage, Is.EqualTo(inputImage));
        }

        [Test]
        public void TestInputType()
        {
            Item item = new Item();
            var inputType = "11";
            item.InputType = inputType;

            // Assert
            Assert.That(item.InputType, Is.EqualTo(inputType));
        }


        [Test]
        public void TestInputName()
        {
            Item item = new Item();
            var inputName = "11";
            item.InputName = inputName;

            // Assert
            Assert.That(item.InputName, Is.EqualTo(inputName));
        }


        [Test]
        public void TestUSBUpstream()
        {
            Item item = new Item();
            var uSBUpstream = new ObservableCollection<string>();
            item.USBUpstream = uSBUpstream;
            Assert.That(item.USBUpstream, Is.EqualTo(uSBUpstream));
        }

        [Test]
        public void TestUpstreamIndex()
        {
            Item item = new Item();
            var upstreamIndex = 2;
            item.UpstreamIndex = upstreamIndex;
            Assert.That(item.UpstreamIndex, Is.EqualTo(upstreamIndex));
        }

    }
}
