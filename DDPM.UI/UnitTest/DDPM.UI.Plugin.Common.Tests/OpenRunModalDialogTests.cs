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
    public class OpenRunModalDialogTests
    {
        private OpenRunModalDialog? openRunModalDialog;

        [SetUp]
        public void Setup()
        {
            openRunModalDialog = new OpenRunModalDialog(1.0, 2.0, new List<string>(), "Parameter\\p");
        }

        [Test]
        public void TestConstructor_OpenRunModalDialog()
        {
            // Assert
            Assert.That(openRunModalDialog, Is.Not.Null);
            Assert.That(openRunModalDialog.Width, Is.EqualTo(1.0));
            Assert.That(openRunModalDialog.Height, Is.EqualTo(2.0));
        }

        [Test]
        public void TestParameter()
        {
            Assert.That(openRunModalDialog.Parameter, Is.EqualTo("Parameter\\p"));
        }
    }
}
