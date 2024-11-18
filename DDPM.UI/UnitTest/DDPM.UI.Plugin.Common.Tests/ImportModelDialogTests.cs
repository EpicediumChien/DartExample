using DDPM.SA.Common;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common;
using DDPM.UI.Plugin.ViewModels;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using NGA.UnitTest.PrivateObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Reflection.Metadata;

namespace DDPM.UI.Plugin.Common.Tests
{
    [Apartment(ApartmentState.STA)]
    public class ImportModalDialogTests
    {
        private ImportModalDialog? importModalDialog;

        [SetUp]
        public void Setup()
        {
            importModalDialog=new ImportModalDialog("model",1.0,2.0);
        }

        [Test]
        public void TestConstructor_ImportModalDialog()
        {
            // Assert
            Assert.That(importModalDialog, Is.Not.Null);
            Assert.That(importModalDialog.Width, Is.EqualTo(1.0));
            Assert.That(importModalDialog.Height, Is.EqualTo(2.0));
        }

        [Test]
        public void TestisChecked()
        {
            importModalDialog.isChecked = true;
            Assert.That(importModalDialog.isChecked, Is.True);
        }
    }
}
