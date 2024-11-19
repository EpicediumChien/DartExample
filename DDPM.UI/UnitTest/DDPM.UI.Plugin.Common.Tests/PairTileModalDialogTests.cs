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
    public class PairTileModalDialogTests
    {
        private PairTileModalDialog? pairTileModalDialog;

        [SetUp]
        public void Setup()
        {
            pairTileModalDialog = new PairTileModalDialog(1.0, 2.0);
        }

        [Test]
        public void TestConstructor_PairTileModalDialog()
        {
            // Assert
            Assert.That(pairTileModalDialog, Is.Not.Null);
            Assert.That(pairTileModalDialog.Width, Is.EqualTo(1.0));
            Assert.That(pairTileModalDialog.Height, Is.EqualTo(2.0));
        }

    }
}
