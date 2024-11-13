using DDPM.SA.Common;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common;
using DDPM.UI.Plugin.ViewModels;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using Moq;
using NGA.UnitTest.PrivateObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DDPM.UI.Plugin.Common.Tests
{
    [Apartment(ApartmentState.STA)]
    public class ConsentModalDialogTests
    {
        private ConsentModalDialog? consentModalDialog;

        [SetUp]
        public void Setup()
        {

        }

        [Test]
        public void TestConstructor_ConsentModalDialog()
        {
            consentModalDialog = new ConsentModalDialog( 1, 2);
            Assert.IsNotNull( consentModalDialog );
            Assert.That(consentModalDialog.Width,Is.EqualTo(1));
            Assert.That(consentModalDialog.Height, Is.EqualTo(2));
        }
    }
}
