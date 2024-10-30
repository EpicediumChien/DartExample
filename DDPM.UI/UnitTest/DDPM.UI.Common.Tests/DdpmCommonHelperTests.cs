using DDPM.Easy.Common;
using DDPM.SA.Common;
using DDPM.SA.Common.Settings;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using DDPM.UI.Common.UserControls;
using DDPM.UI.Common.ViewModels;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using Moq;
using NGA.UnitTest.PrivateObject;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using VcpCore.Common;
using static System.Net.Mime.MediaTypeNames;

namespace DDPM.UI.Common.Tests
{
    [Apartment(ApartmentState.STA)]
    public class DdpmCommonHelperTests
    {
        private PrivateObject? privateObject;


        [SetUp]
        public void Setup()
        {

        }

        [Test]
        public void TestGetImageSourceFromCommonResource()
        {
            var result = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Brightness.png");
            // Assert
            Assert.That(result, Is.EqualTo(null));

            //string s = System.IO.Packaging.PackUriHelper.UriSchemePack;
            if (!UriParser.IsKnownScheme("pack"))
            {
                new System.Windows.Application();
            }
            result = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Brightness.png");
            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void TestMyShowPluginManager()
        {
            var MyShowPluginManagerMock = new Mock<IShowPluginManager>();
            DdpmCommonHelper.MyShowPluginManager = MyShowPluginManagerMock.Object;
            // Assert
            Assert.That(DdpmCommonHelper.MyShowPluginManager, Is.EqualTo(MyShowPluginManagerMock.Object));
        }

        [Test]
        public void TestSettings_Cache()
        {
            var settings_Cache = new DDPMSettings(new DDPMAppSettings(), new DDPMUserSettings(), new DDPMITConfig());
            DdpmCommonHelper.Settings_Cache = settings_Cache;
            // Assert
            Assert.That(DdpmCommonHelper.Settings_Cache, Is.EqualTo(settings_Cache));
        }

        //[DllImport("user32.dll", EntryPoint = "SendMessageA")]
        //public static extern int SendMessage(IntPtr hwnd, int wMsg, int wParam, int lParam);

        //[DllImport("User32.dll", EntryPoint = "FindWindow")]
        //public static extern IntPtr FindWindow(string className, string windowName);
        //public const int WM_CLOSE = 0x10;
        //[Test]
        //public void TestDDPMMesssageBox()
        //{
        //    var result = DdpmCommonHelper.DDPMMesssageBox("title", "text", null);
        //    IntPtr hwnd_win;
        //    hwnd_win = FindWindow(null, "DDPMMsgBox");
        //    SendMessage(hwnd_win, WM_CLOSE, 0, 0);
        //    // Assert
        //    Assert.That(result, Is.EqualTo(false));

        //    result = DdpmCommonHelper.DDPMMesssageBox("title", "text",new DependencyObject() );
        //    // Assert
        //    Assert.That(result, Is.EqualTo(false));
        //}



    }
}
