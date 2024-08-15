using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DDPM.UI.Plugin.DisplayPlugin.Views;
using DDPM.UI.Module.Color;
using static System.Net.Mime.MediaTypeNames;
using NGA.UnitTest.PrivateObject;

namespace DDPM.UI.Plugin.DisplayPlugin.Tests
{
    [Apartment(ApartmentState.STA)]
    public class DisplayDefaultLeftViewTests
    {
        private DisplayDefaultLeftView? displayDefaultLeftView;
        private PrivateObject? privateObject;

        [SetUp]
        public void Setup()
        {
            displayDefaultLeftView = new DisplayDefaultLeftView();
            privateObject = new PrivateObject(displayDefaultLeftView);
        }

        [Test]
        public void TestConstructor_InitializesComponent()
        {
            var txtRestoreText = privateObject.GetFieldOrProperty("Restore");
            // Assert
            Assert.That(displayDefaultLeftView, Is.Not.Null);
            Assert.That(txtRestoreText, Is.EqualTo("Restore to default"));
        }
    }
}