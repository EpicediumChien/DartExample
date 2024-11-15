using DDPM.UI.Common;
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
    public class UnpairModalDialogTests
    {
        private UnpairModalDialog? unpairModalDialog;

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void TestConstructor_UnpairModalDialog()
        {
            unpairModalDialog = new UnpairModalDialog(eDeviceCategory.Mouse);
            // Assert
            Assert.That(unpairModalDialog, Is.Not.Null);

            unpairModalDialog = new UnpairModalDialog(eDeviceCategory.KB);
            // Assert
            Assert.That(unpairModalDialog, Is.Not.Null);

            unpairModalDialog = new UnpairModalDialog(eDeviceCategory.Pen);
            // Assert
            Assert.That(unpairModalDialog, Is.Not.Null);

            unpairModalDialog = new UnpairModalDialog(eDeviceCategory.Headset);
            // Assert
            Assert.That(unpairModalDialog, Is.Not.Null);
            Assert.That(unpairModalDialog.Height, Is.EqualTo(360));
        }
    }
}
