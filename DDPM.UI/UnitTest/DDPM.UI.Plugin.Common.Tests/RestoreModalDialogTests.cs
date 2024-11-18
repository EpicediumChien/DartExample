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
    public class RestoreModalDialogTests
    {
        private RestoreModalDialog? restoreModalDialog;

        [SetUp]
        public void Setup()
        {
            restoreModalDialog = new RestoreModalDialog();
        }

        [Test]
        public void TestConstructor_RestoreModalDialog()
        {
            // Assert
            Assert.That(restoreModalDialog, Is.Not.Null);
        }
    }
}
