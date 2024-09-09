using DDPM.SA.Common;
using DDPM.SA.Plugins.User.DisplayManager;
using DDPM.SA.Plugins.User.PipPbpManger;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.Common.Annotations;
using Dell.Client.Framework.Interfaces;
using Dell.Client.Framework.UnitTestShared.Tests;
using Moq;
using System.Data;
using System.Threading;
using VcpCore.Common;
using VcpCore.Interfaces;
using VcpCore.Plugins;
using WinCopies;
using Windows.Media.AppBroadcasting;
using Windows.UI.ViewManagement;
using static VcpCore.Common.User32;
using DDPM.SA.Plugins.User.DisplayProperties;
using WinCopies.Util;
using static DDPM.SA.Plugins.User.Hotkey.HotkeyPlugin;
using System.Reflection.Emit;
using System.Windows.Forms;
using System.Windows.Input;
using static System.Runtime.InteropServices.JavaScript.JSType;
using DDPM.SA.Plugins.User.EasyArrange;
using System.Diagnostics;
using Microsoft.WindowsAPICodePack.Win32Native.Shell.WindowsAndMessages;

namespace DDPM.SA.Plugins.User.Hotkey.Test
{
    public class TestHotkeyPlugin
    {
        private Mock<IAgent> DisplayMangerAgent { get; } = new();
        private Mock<IAgent> VcpCoreAgent { get; } = new();
        private Mock<IAgent> HotkeyAgent { get; } = new();
        private Mock<ILog> logHotkeyAgent { get; } = new();
        //private Mock<log_type> log_typeAgent { get; } = new();
        private MonitorInfo monitorInfo = new MonitorInfo();

        private DisplayMangerPlugin CreateInitializeDisplayMangerPlugin()
        {
            DisplayMangerAgent.Setup(x => x.PluginManager.FindPluginByGuid(Guid.Parse(VcpCore.Common.IDs.Display_Manager_PLUGIN_ID)));

            return new DisplayMangerPlugin(DisplayMangerAgent.Object);
        }

        private VcpCorePlugin CreateInitializeVcpCorePlugin()
        {
            VcpCoreAgent.Setup(x => x.PluginManager.FindPluginByGuid(Guid.Parse(VcpCore.Common.IDs.VCP_CORE_PLUGIN_ID)));

            return new VcpCorePlugin(VcpCoreAgent.Object);
        }

        private HotkeyPlugin CreateInitializehotkeyPlugin()
        {
            HotkeyAgent.Setup(x => x.PluginManager.FindPluginByGuid(Guid.Parse(DDPM.SA.Common.IDs.DDPM_HOTKEY_PLUGIN_ID)));
            return new HotkeyPlugin(HotkeyAgent.Object);
        }

        private DisplayMangerPlugin displayPlugin;
        private VcpCorePlugin vcpCorePlugin;
        private Dictionary<string, Dictionary<string, string>> getstr;
        private HotkeyPlugin hotkeyPlugin;
        private delegate IntPtr keyboardHookProc(int nCode, IntPtr wParam, IntPtr lParam);

        [OneTimeSetUp]
        public void Setup()
        {
            displayPlugin = CreateInitializeDisplayMangerPlugin();
            vcpCorePlugin = CreateInitializeVcpCorePlugin();
            hotkeyPlugin = CreateInitializehotkeyPlugin();

            PrivateObject privateObject = new PrivateObject(displayPlugin);
            PrivateObject privatevcp = new PrivateObject(vcpCorePlugin);
            PrivateObject privateHotkeyObject = new PrivateObject(hotkeyPlugin);
            privateObject.SetField("_VcpCorePlugin", vcpCorePlugin as IVcpCoreService);
            privateHotkeyObject.SetFieldOrProperty("Log", logHotkeyAgent.Object);
        }

        [Test]
        public void TestHotKeyPlugin()
        {
            Assert.IsNotNull(hotkeyPlugin);
            PrivateObject privateObject = new PrivateObject(hotkeyPlugin);
            var agent2 = privateObject.GetField("_agent") as IAgent;
            Assert.That(agent2, Is.EqualTo(HotkeyAgent.Object));
        }

        [Test]
        public void Testhook()
        {
            var hookResult= hotkeyPlugin.hook();
            Assert.IsNotNull(hookResult);
            Assert.IsTrue(hookResult);
        }

        [Test]
        public void Testunhook()
        {
            var unhookResult = hotkeyPlugin.unhook();
            Assert.IsNotNull(unhookResult);
            Assert.IsTrue(unhookResult);

        }

        [Test]
        public void TesthookProc()
        {
            int code = 1;
            int wParam = 0x100;
            int wParamup = 0x101;
            int prc = 0;
            string keyDown = "KeyDown";
            string keyUp = "KeyUp";
            keyboardHookStruct lParam = new keyboardHookStruct() { vkCode = (int)Keys.A };
            hotkeyPlugin.KeyDown += HotkeyPlugin_KeyDown;
            hotkeyPlugin.KeyUp += HotkeyPlugin_KeyUp; 

            if (code >= 0)
            {
                if (wParam == 0x100)
                {
                    try
                    {
                        var hookProckResult = hotkeyPlugin.hookProc(code, wParam, ref lParam);
                    }
                    catch (Exception ex)
                    {
                        Assert.That(keyDown, Is.EqualTo(ex.Message));
                    }
                }
                if(wParamup== 0x101)
                {
                    try
                    {
                        var hookProckResult = hotkeyPlugin.hookProc(code, wParamup, ref lParam);
                    }
                    catch (Exception ex)
                    {
                        Assert.That(keyUp, Is.EqualTo(ex.Message));
                    }
                }
            }
            else 
            {
                var hookProckResult = hotkeyPlugin.hookProc(code, wParam, ref lParam);
                Assert.IsNotNull(hookProckResult);
            }

        }

        private void HotkeyPlugin_KeyUp(object? sender, System.Windows.Forms.KeyEventArgs e)
        {
            throw new NotImplementedException("KeyUp");
        }

        private void HotkeyPlugin_KeyDown(object? sender, System.Windows.Forms.KeyEventArgs e)
       
        {
            throw new Exception("KeyDown");
        }

        [Test]
        public void TestIsDisposed()
        {
            var Result = hotkeyPlugin.IsDisposed;
            Assert.IsFalse(Result);
            PrivateObject privatehotkeyPluginObject = new PrivateObject(hotkeyPlugin);
            privatehotkeyPluginObject.SetFieldOrProperty("IsDisposed", true);
            var Result2 = hotkeyPlugin.IsDisposed;
            Assert.IsTrue(Result2);
        }
		
        [Test]
        public void TestHook()
        {
            bool hooktrue;
            Thread _hookThread1;
            _hookThread1 = new Thread(() =>
            {
                hooktrue=true;
            });
            PrivateObject privatehotkeyPluginObject = new PrivateObject(hotkeyPlugin);
            privatehotkeyPluginObject.SetFieldOrProperty("_hookThread", _hookThread1);
            bool HookResult = hotkeyPlugin.Hook();
            Assert.IsFalse(HookResult);
        }


    }
}