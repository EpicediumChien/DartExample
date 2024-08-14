using DDPM.SA.Common;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common;
using Moq;
using NGA.UnitTest.PrivateObject;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDPM.UI.Module.Brightness.Tests
{
    [Apartment(ApartmentState.STA)]
    public class BrightnessLeftViewTests
    {
        private BrightnessLeftView? brightnessLeftView;

        [SetUp]
        public void Setup()
        {

        }


        [Test]
        public void TestConstructor_InitializesComponent()
        {
            brightnessLeftView = new BrightnessLeftView();
            // Assert
            Assert.That(brightnessLeftView, Is.Not.Null);
        }
    }
}
