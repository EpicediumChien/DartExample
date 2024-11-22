using DDPM.UI.Common.UserControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace DDPM.UI.Plugin.Common.Tests
{
    [Apartment(ApartmentState.STA)]
    public class LoadingScreenTests
    {
        private LoadingScreen? loadingScreen;

        [SetUp]
        public void Setup()
        {
            loadingScreen = new LoadingScreen(1.0, 2.0);
        }

        [Test]
        public void TestConstructor_LoadingScreen()
        {
            // Assert
            Assert.That(loadingScreen, Is.Not.Null);
        }

        [Test]
        public void TestCloseByCaller()
        {
            try
            {
                loadingScreen.CloseByCaller();
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }

        }
    }
}
