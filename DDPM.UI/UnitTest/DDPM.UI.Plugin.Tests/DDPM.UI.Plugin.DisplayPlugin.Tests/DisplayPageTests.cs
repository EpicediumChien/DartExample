using DDPM.UI.Plugin.DisplayPlugin.Views;
using NGA.UnitTest.PrivateObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Display.Core;

namespace DDPM.UI.Plugin.DisplayPlugin.Tests
{
    [Apartment(ApartmentState.STA)]
    public class DisplayPageTests
    {
        private DisplayPage? displayPage;
        private PrivateObject? privateObject;

        [SetUp]
        public void Setup()
        {
            displayPage = new DisplayPage();
            privateObject = new PrivateObject(displayPage);
        }


    }
}
