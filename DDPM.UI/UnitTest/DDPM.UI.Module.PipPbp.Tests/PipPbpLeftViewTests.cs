using DDPM.UI.Common;
using DDPM.UI.Common.Interfaces;
using NGA.UnitTest.PrivateObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDPM.UI.Module.PipPbp.Tests
{
    [Apartment(ApartmentState.STA)]
    public class PipPbpLeftViewTests
    {

        private PipPbpLeftView? pipPbpLeftView;
        private PrivateObject? privateObject;



        [SetUp]
        public void Setup()
        {

            pipPbpLeftView = new PipPbpLeftView();
            privateObject = new PrivateObject(pipPbpLeftView);
        }

        [Test]

        public void TestPipPbpLeftViewInitialization()
        {
            // Assert
            Assert.That(pipPbpLeftView, Is.Not.Null);
        }


    }
}
