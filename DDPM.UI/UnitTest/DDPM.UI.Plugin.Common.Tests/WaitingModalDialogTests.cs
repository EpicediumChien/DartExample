using DDPM.UI.Common.UserControls;
using NGA.UnitTest.PrivateObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace DDPM.UI.Plugin.Common.Tests
{
    [Apartment(ApartmentState.STA)]
    public class WaitingModalDialogTests
    {
        private WaitingModalDialog? waitingModalDialog;

        [SetUp]
        public void Setup()
        {
            //waitingModalDialog = new WaitingModalDialog("caption", "message", "alert");
        }

        [Test]
        public void TestConstructor_WaitingModalDialog()
        {
            // Assert
            Assert.That(waitingModalDialog, Is.Not.Null);
        }
    }
}
