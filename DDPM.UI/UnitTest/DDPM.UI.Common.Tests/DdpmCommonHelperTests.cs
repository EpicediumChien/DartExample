using DDPM.Easy.Common;
using DDPM.SA.Common;
using DDPM.SA.Common.Settings;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using DDPM.UI.Common.UserControls;
using DDPM.UI.Common.ViewModels;
using DDPM.UI.Common.Views;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using Dell.Client.Framework.UX.WPF.Controls;
using Moq;
using NGA.UnitTest.PrivateObject;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using VcpCore.Common;
using static DDPM.UI.Common.Views.DDPMMsgBox;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Menu;

namespace DDPM.UI.Common.Tests
{
    [Apartment(ApartmentState.STA)]
    [Parallelizable(ParallelScope.All)]
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
            if (!UriParser.IsKnownScheme("pack"))
            {
                new System.Windows.Application();
            }
            var result = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Brightness.png");
            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void TestbInputSourceRenamed()
        {
            DdpmCommonHelper.bInputSourceRenamed = true;
            // Assert
            Assert.That(DdpmCommonHelper.bInputSourceRenamed, Is.EqualTo(true));
        }

        [Test]
        public void TestisHotkeyBypass()
        {
            DdpmCommonHelper.isHotkeyBypass = true;
            // Assert
            Assert.That(DdpmCommonHelper.isHotkeyBypass, Is.EqualTo(true));
        }

        [Test]
        public void TestSplashPath()
        {
            DdpmCommonHelper.SplashPath = "SplashPath";
            // Assert
            Assert.That(DdpmCommonHelper.SplashPath, Is.EqualTo("SplashPath"));
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

        [Test]
        public void TestThemeSwitchFlag()
        {
            DdpmCommonHelper.ThemeSwitchFlag = true;
            // Assert
            Assert.That(DdpmCommonHelper.ThemeSwitchFlag, Is.EqualTo(true));
        }

        [Test]
        public void TestUIDebugModeFlag()
        {
            DdpmCommonHelper.UIDebugModeFlag = true;
            // Assert
            Assert.That(DdpmCommonHelper.UIDebugModeFlag, Is.EqualTo(true));
        }

        [DllImport("User32.dll", EntryPoint = "FindWindow")]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern IntPtr FindWindow(string className, string windowName);
        private static IntPtr _FindWindow(string className, string windowName)
        {
            return FindWindow(className, windowName);
        }
        public const int WM_CLOSE = 0x10;

        [Test, NonParallelizable]
        public void TestDDPMMesssageBox()
        {
            IntPtr hwnd_win;
            hwnd_win = _FindWindow(null, "title");
            Thread t = new Thread(() => DdpmCommonHelper.DDPMMesssageBox("title", "text", null));
            t.ApartmentState = ApartmentState.STA;
            t.Start();
            while (hwnd_win == 0)
            {
                //Thread.Sleep(1000);
                hwnd_win = _FindWindow(null, "DDPMMsgBox");
            }
        }

        [Test, NonParallelizable]
        public void TestDDPMPureMesssageBox()
        {
            IntPtr hwnd_win;
            hwnd_win = _FindWindow(null, "title");
            Thread t = new Thread(() => DdpmCommonHelper.DDPMPureMesssageBox("title", "text", true, null));
            t.ApartmentState = ApartmentState.STA;
            t.Start();
            while (hwnd_win == 0)
            {
                //Thread.Sleep(1000);
                hwnd_win = _FindWindow(null, "DDPMMsgBox");
            }
        }

        [Test, NonParallelizable]
        public void TestDDPMEzMesssageBox()
        {
            IntPtr hwnd_win;
            hwnd_win = _FindWindow(null, "title");
            Thread t = new Thread(() => DdpmCommonHelper.DDPMEzMesssageBox("title", "text", true, null, 10, 10, new Thickness(), new Thickness()));
            t.ApartmentState = ApartmentState.STA;
            t.Start();
            while (hwnd_win == 0)
            {
                Thread.Sleep(1000);
                hwnd_win = _FindWindow(null, "DDPMMsgBox");
            }
        }

        [Test]
        public void TestParsingHexStringToWords()
        {
            var result = DdpmCommonHelper.ParsingHexStringToWords("");
            // Assert
            Assert.That(result.Length, Is.EqualTo(0));

            result = DdpmCommonHelper.ParsingHexStringToWords("02 04 05 08 10 12");
            // Assert
            Assert.That(result.Length, Is.EqualTo(6));

            result = DdpmCommonHelper.ParsingHexStringToWords("xfdae2 er");
            // Assert
            Assert.That(result, Is.EqualTo(null));
        }

        [Test]
        public void TestGetUINotifyPropertyValue_Boolean()
        {
            var result = DdpmCommonHelper.GetUINotifyPropertyValue_Boolean("Lock_Display_ColorPreset", null);
            // Assert
            Assert.That(result, Is.EqualTo(null));

            result = DdpmCommonHelper.GetUINotifyPropertyValue_Boolean("Lock_Display_ColorPreset", new ITSettingEventArgs() { IT_Feature_TriggerList = new List<string>(), target_object = new DDPMITConfig() { Lock_Settings_Updates = true } });
            // Assert
            Assert.That(result, Is.EqualTo(null));

            result = DdpmCommonHelper.GetUINotifyPropertyValue_Boolean("Lock_Display_ColorPreset", new ITSettingEventArgs() { IT_Feature_TriggerList = new List<string>() { "", "Lock_Display_ColorPreset" }, target_object = new DDPMITConfig() { Lock_Settings_Updates = true } });
            // Assert
            Assert.That(result, Is.EqualTo(false));
        }

        [Test]
        public void TestGetUINotify_IsSynchronizeBetweenMonitors_Locked()
        {
            var result = DdpmCommonHelper.GetUINotify_IsSynchronizeBetweenMonitors_Locked(null);
            // Assert
            Assert.That(result, Is.EqualTo(false));

            result = DdpmCommonHelper.GetUINotify_IsSynchronizeBetweenMonitors_Locked(new DDPMSettings(new DDPMAppSettings(), new DDPMUserSettings(), new DDPMITConfig()));
            // Assert
            Assert.That(result, Is.EqualTo(false));

            result = DdpmCommonHelper.GetUINotify_IsSynchronizeBetweenMonitors_Locked(new DDPMSettings(new DDPMAppSettings(), new DDPMUserSettings(), new DDPMITConfig()) { LockSettings = new DDPMITConfig() { Lock_Display_BriCont = true, Lock_Display_ColorPreset = false, Lock_Display_AutoBriTemp = true } });
            // Assert
            Assert.That(result, Is.EqualTo(true));
        }

        [Test]
        public void TestGetUINotifyPropertyValue_isAnyLocked()
        {
            var result = DdpmCommonHelper.GetUINotifyPropertyValue_isAnyLocked(null);
            // Assert
            Assert.That(result, Is.EqualTo(false));

            result = DdpmCommonHelper.GetUINotifyPropertyValue_isAnyLocked(new DDPMSettings(new DDPMAppSettings(), new DDPMUserSettings(), new DDPMITConfig()), null);
            // Assert
            Assert.That(result, Is.EqualTo(false));

            result = DdpmCommonHelper.GetUINotifyPropertyValue_isAnyLocked(new DDPMSettings(new DDPMAppSettings(), new DDPMUserSettings(), new DDPMITConfig()) { LockSettings = new DDPMITConfig() { Lock_Display_BriCont = true, Lock_Display_ColorPreset = false, Lock_Display_AutoBriTemp = true } });
            // Assert
            Assert.That(result, Is.EqualTo(true));
        }

        [Test]
        public void TestApplyRestoreFactoryDefaultsEventData()
        {
            var result = DdpmCommonHelper.ApplyRestoreFactoryDefaultsEventData(new ITSettingEventArgs(), "Lock_Display_RestoreFactoryDefaults");
            Assert.That(result.isEnabled, Is.EqualTo(false));
            Assert.That(result.isLocked, Is.EqualTo(Visibility.Collapsed));

            var deviceManagerSAMock = new Mock<IDeviceManagerSA>();
            DdpmCommonHelper.DeviceManagerSA = deviceManagerSAMock.Object;
            result = DdpmCommonHelper.ApplyRestoreFactoryDefaultsEventData(new ITSettingEventArgs(), "Lock_Display_RestoreFactoryDefaults");
            Assert.That(result.isEnabled, Is.EqualTo(false));
            Assert.That(result.isLocked, Is.EqualTo(Visibility.Collapsed));

            DdpmCommonHelper.Settings_Cache = new DDPMSettings(new DDPMAppSettings() { }, new DDPMUserSettings(), new DDPMITConfig()) { LockSettings = new DDPMITConfig() { Lock_Display_RestoreFactoryDefaults = true } };
            result = DdpmCommonHelper.ApplyRestoreFactoryDefaultsEventData(new ITSettingEventArgs() { IT_Feature_TriggerList = new List<string>() { "", "Lock_Display_RestoreFactoryDefaults" }, target_object = new DDPMITConfig() { Lock_Settings_Updates = true } }, "Lock_Display_RestoreFactoryDefaults");
            Assert.That(result.isEnabled, Is.EqualTo(true));
            Assert.That(result.isLocked, Is.EqualTo(Visibility.Collapsed));

            DdpmCommonHelper.Settings_Cache = new DDPMSettings(new DDPMAppSettings() { }, new DDPMUserSettings(), new DDPMITConfig()) { LockSettings = new DDPMITConfig() { Lock_Setting_RestoreDefaults = true } };
            result = DdpmCommonHelper.ApplyRestoreFactoryDefaultsEventData(new ITSettingEventArgs() { IT_Feature_TriggerList = new List<string>() { "", "Lock_Display_RestoreFactoryDefaults" }, target_object = new DDPMITConfig() { Lock_Settings_Updates = true } }, "Lock_Display_RestoreFactoryDefaults");
            Assert.That(result.isEnabled, Is.EqualTo(false));
            Assert.That(result.isLocked, Is.EqualTo(Visibility.Visible));

            DdpmCommonHelper.Settings_Cache = new DDPMSettings(new DDPMAppSettings() { }, new DDPMUserSettings(), new DDPMITConfig()) { LockSettings = new DDPMITConfig() { Lock_Display_RestoreFactoryDefaults = true } };
            result = DdpmCommonHelper.ApplyRestoreFactoryDefaultsEventData(new ITSettingEventArgs() { IT_Feature_TriggerList = new List<string>() { "", "Lock_Setting_RestoreDefaults" }, target_object = new DDPMITConfig() { Lock_Settings_Updates = true } }, "Lock_Display_RestoreFactoryDefaults");
            Assert.That(result.isEnabled, Is.EqualTo(false));
            Assert.That(result.isLocked, Is.EqualTo(Visibility.Visible));

            DdpmCommonHelper.Settings_Cache = new DDPMSettings(new DDPMAppSettings() { }, new DDPMUserSettings(), new DDPMITConfig()) { LockSettings = new DDPMITConfig() { Lock_Audio_RestoreFactoryDefaults = true } };
            result = DdpmCommonHelper.ApplyRestoreFactoryDefaultsEventData(new ITSettingEventArgs() { IT_Feature_TriggerList = new List<string>() { "", "Lock_Setting_RestoreDefaults" }, target_object = new DDPMITConfig() { Lock_Settings_Updates = true } }, "Lock_Audio_RestoreFactoryDefaults");
            Assert.That(result.isEnabled, Is.EqualTo(false));
            Assert.That(result.isLocked, Is.EqualTo(Visibility.Visible));

            DdpmCommonHelper.Settings_Cache = new DDPMSettings(new DDPMAppSettings() { }, new DDPMUserSettings(), new DDPMITConfig()) { LockSettings = new DDPMITConfig() { Lock_Webcam_RestoreFactoryDefaults = false } };
            result = DdpmCommonHelper.ApplyRestoreFactoryDefaultsEventData(new ITSettingEventArgs() { IT_Feature_TriggerList = new List<string>() { "", "Lock_Setting_RestoreDefaults" }, target_object = new DDPMITConfig() { Lock_Settings_Updates = true } }, "Lock_Webcam_RestoreFactoryDefaults");
            Assert.That(result.isEnabled, Is.EqualTo(true));
            Assert.That(result.isLocked, Is.EqualTo(Visibility.Collapsed));

            DdpmCommonHelper.Settings_Cache = new DDPMSettings(new DDPMAppSettings() { }, new DDPMUserSettings(), new DDPMITConfig()) { LockSettings = new DDPMITConfig() { Lock_Keyboard_RestoreFactoryDefaults = false } };
            result = DdpmCommonHelper.ApplyRestoreFactoryDefaultsEventData(new ITSettingEventArgs() { IT_Feature_TriggerList = new List<string>() { "", "Lock_Setting_RestoreDefaults" }, target_object = new DDPMITConfig() { Lock_Settings_Updates = true } }, "Lock_Keyboard_RestoreFactoryDefaults");
            Assert.That(result.isEnabled, Is.EqualTo(true));
            Assert.That(result.isLocked, Is.EqualTo(Visibility.Collapsed));

            DdpmCommonHelper.Settings_Cache = new DDPMSettings(new DDPMAppSettings() { }, new DDPMUserSettings(), new DDPMITConfig()) { LockSettings = new DDPMITConfig() { Lock_Mouse_RestoreFactoryDefaults = true } };
            result = DdpmCommonHelper.ApplyRestoreFactoryDefaultsEventData(new ITSettingEventArgs() { IT_Feature_TriggerList = new List<string>() { "", "Lock_Setting_RestoreDefaults" }, target_object = new DDPMITConfig() { Lock_Settings_Updates = true } }, "Lock_Mouse_RestoreFactoryDefaults");
            Assert.That(result.isEnabled, Is.EqualTo(false));
            Assert.That(result.isLocked, Is.EqualTo(Visibility.Visible));

            DdpmCommonHelper.Settings_Cache = new DDPMSettings(new DDPMAppSettings() { }, new DDPMUserSettings(), new DDPMITConfig()) { LockSettings = new DDPMITConfig() { Lock_Pen_RestoreFactoryDefaults = true } };
            result = DdpmCommonHelper.ApplyRestoreFactoryDefaultsEventData(new ITSettingEventArgs() { IT_Feature_TriggerList = new List<string>() { "", "Lock_Setting_RestoreDefaults" }, target_object = new DDPMITConfig() { Lock_Settings_Updates = true } }, "Lock_Pen_RestoreFactoryDefaults");
            Assert.That(result.isEnabled, Is.EqualTo(false));
            Assert.That(result.isLocked, Is.EqualTo(Visibility.Visible));
        }

        [Test]
        public void TestWriteDDPMSettings()
        {
            var result = DdpmCommonHelper.WriteDDPMSettings(null);
            // Assert
            Assert.That(result, Is.EqualTo(false));

            result = DdpmCommonHelper.WriteDDPMSettings(new DDPMSettings(new DDPMAppSettings(), new DDPMUserSettings(), new DDPMITConfig()));
            // Assert
            Assert.That(result, Is.EqualTo(false));

            var deviceManagerSAMock = new Mock<IDeviceManagerSA>();
            DdpmCommonHelper.DeviceManagerSA = deviceManagerSAMock.Object;
            deviceManagerSAMock.Setup(x => x.SetAppConfigData(It.IsAny<DDPMSettings>())).Returns(Task.FromResult(true));
            result = DdpmCommonHelper.WriteDDPMSettings(new DDPMSettings(new DDPMAppSettings(), new DDPMUserSettings(), new DDPMITConfig()));
            // Assert
            Assert.That(result, Is.EqualTo(true));
        }

        [Test]
        public void TestReadDDPMSettings()
        {
            var result = DdpmCommonHelper.ReadDDPMSettings(false);
            //Assert
            Assert.That(result, Is.Not.Null);

            DdpmCommonHelper.DeviceManagerSA = null;
            result = DdpmCommonHelper.ReadDDPMSettings(true);
            // Assert
            Assert.That(result, Is.Not.Null);

            var deviceManagerSAMock = new Mock<IDeviceManagerSA>();
            DdpmCommonHelper.DeviceManagerSA = deviceManagerSAMock.Object;
            deviceManagerSAMock.Setup(x => x.ReloadAppConfigData(It.IsAny<bool>())).Returns(Task.FromResult(new DDPMSettings(new DDPMAppSettings(), new DDPMUserSettings(), new DDPMITConfig())));
            result = DdpmCommonHelper.ReadDDPMSettings(true);
            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void TestupdateMergedDictionarie()
        {
            UXSystemParameters.Instance.OSTheme = OSThemeEnum.Dark;
            try
            {
                //new Dell.Client.Framework.UX.WPF.ResourceManager.ResourceManager()
                //Dell.Client.Framework.UX.WPF.ResourceManager
                //Fix build
                DdpmCommonHelper.updateMergedDictionaries(null);
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }

        [Test]
        public void TestisDarkMode()
        {
            UXSystemParameters.Instance.OSTheme = OSThemeEnum.Light;
            var result = DdpmCommonHelper.isDarkMode();
            // Assert
            Assert.That(result, Is.EqualTo(false));

            UXSystemParameters.Instance.OSTheme = OSThemeEnum.Dark;
            result = DdpmCommonHelper.isDarkMode();
            // Assert
            Assert.That(result, Is.EqualTo(true));
        }

        [Test]
        public void TestGetBreakPoints()
        {
            var result = DdpmCommonHelper.GetBreakPoints();
            Assert.That(result, Is.EqualTo(1050));
        }

        [Test]
        public void TestCreateBitmapSource()
        {
            var result = DdpmCommonHelper.CreateBitmapSource(new FrameworkElement() { });
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void TestSaveBitmapSourceAsPngFile()
        {
            var bmpSrc = DdpmCommonHelper.CreateBitmapSource(new FrameworkElement() { });
            var result = DdpmCommonHelper.SaveBitmapSourceAsPngFile(bmpSrc, "PathName");
            Assert.That(result, Is.EqualTo(true));
        }

        [Test]
        public void TestDeterminePeripheralProductImageFileName()
        {
            var result = DdpmCommonHelper.DeterminePeripheralProductImageFileName(new DeviceInfo() { ModelNumber = "KB740", ColorCode = 1 });
            Assert.That(result, Is.EqualTo("KB740_1"));
            result = DdpmCommonHelper.DeterminePeripheralProductImageFileName(new DeviceInfo() { ModelNumber = "KB7120W", ColorCode = 1 });
            Assert.That(result, Is.EqualTo("KB740_1"));
            result = DdpmCommonHelper.DeterminePeripheralProductImageFileName(new DeviceInfo() { ModelNumber = "KB500", ColorCode = 1 });
            Assert.That(result, Is.EqualTo("KB500_1"));
            result = DdpmCommonHelper.DeterminePeripheralProductImageFileName(new DeviceInfo() { ModelNumber = "KB3121W", ColorCode = 1 });
            Assert.That(result, Is.EqualTo("KB500_1"));
            result = DdpmCommonHelper.DeterminePeripheralProductImageFileName(new DeviceInfo() { ModelNumber = "KB700", ColorCode = 1 });
            Assert.That(result, Is.EqualTo("KB700_1"));
            result = DdpmCommonHelper.DeterminePeripheralProductImageFileName(new DeviceInfo() { ModelNumber = "KB7221W", ColorCode = 1 });
            Assert.That(result, Is.EqualTo("KB700_1"));
            result = DdpmCommonHelper.DeterminePeripheralProductImageFileName(new DeviceInfo() { ModelNumber = "MS300", ColorCode = 1 });
            Assert.That(result, Is.EqualTo("MS300_1"));
            result = DdpmCommonHelper.DeterminePeripheralProductImageFileName(new DeviceInfo() { ModelNumber = "MS3121W", ColorCode = 1 });
            Assert.That(result, Is.EqualTo("MS300_1"));
            //Default
            result = DdpmCommonHelper.DeterminePeripheralProductImageFileName(new DeviceInfo() { ModelNumber = "MS3121", ColorCode = 1 });
            Assert.That(result, Is.EqualTo("MS3121_1"));
        }

    }
}
