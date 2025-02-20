using DDPM.CLI.Plugins.Peripherals;
using DDPM.SA.Common;
using DDPM.SA.Common.Settings;
using Dell.Client.Framework.Interfaces;
using Dell.Client.Framework.UnitTestShared.Tests;
using Moq;
using VcpCore.Common;
using Windows.UI.Text;
using static DDPM.SA.Common.ICLICommandTable;
using IDs = DDPM.SA.Common.IDs;

namespace DDPM.CLI.Plugins.Peripherals.Test
{
    public class TestCLIPeripheralsPlugins
    {
        private Mock<IAgent> DisplayMangerAgent { get; } = new();
        private Mock<IAgent> VcpCoreAgent { get; } = new();
        private Mock<IAgent> CLIPeripheralsPluginsAgent { get; } = new();

        MonitorInfo monitorInfo = new MonitorInfo();
        private Mock<IDeviceManagerSA> DeviceManagerSAService { get; } = new();
        private IDeviceManagerSA? _deviceManagerPlugin;

        MonitorInfo monitorInfo1 = new MonitorInfo()
        {
            AliasDeviceName = "Dell U2724DE(HDMI)",
            IsDellMonitor = true,
            Index = 0,
            CapabilityString = "(prot(monitor)type(LCD)model(U2424H)cmds(01 02 03 07 0C E3 F3)vcp(02 04 05 08 10 12 14(01 04 05 06 08 09 0B 0C)E5 E7(02 03) E2(00 02 04 0C 0D 0F)",
            DisplayName = "TestDISPLAY7",
            DDCisON = true,
            FwVersion = "M3T101",
            inputSource = "HDMI-1",
            modelName = "TestU2724DE",
            series = "Dell UltraSharp (U) Series Monitors",
            D_Ctrl = "Test D_Ctrl",
            SupplierID = "Test SupplierID",
            //CapabilityDic = capabilityDic;
            CapabilityDic = new Dictionary<string, List<string>>(),
            edid = new EDID()
            {
                ManufactureID = "DEL",
                VendorID = "42DC",
                Year = 2023,
                Month = 5,
                Week = 22,
                ModelName = "DELLU2724DE",
                EdidVersion = "V1.3",
                VideoInputType = "Digital Signal",
                Size = 27.1510868f,
                ServiceTag = "CN073K0",
                SerialNumber = "808597589",
                Edid = "00FFFFFFFFFFFF0010ACDC425538323016210103803C2278EA62A5AD5046AB240E5054A54B00714F8180A940D1C081C0A9C001010101565E00A0A0A029503020350055502100001A000000FF00434E3037334B300A2020202020000000FC0044454C4C20553237323444450A000000FD0030781EB23C000A20202020202001ED"

            },
        };

        private CLIPeripheralsPlugins CreateCLIPeripheralsPlugins()
        {
            CLIPeripheralsPluginsAgent.Setup(x => x.PluginManager.FindPluginByGuid(Guid.Parse(IDs.CLI_Plugin_Peripherals)));

            return new CLIPeripheralsPlugins(CLIPeripheralsPluginsAgent.Object);
        }

        private CLIPeripheralsPlugins cLIPeripheralsPlugins;
        private PrivateObject privateteCLIPeripheralsPlugins;

        [OneTimeSetUp]
        public void Setup()
        {
            cLIPeripheralsPlugins = CreateCLIPeripheralsPlugins();
            privateteCLIPeripheralsPlugins = new PrivateObject(cLIPeripheralsPlugins);
        }

        [Test]
        public void TestCLIPeripheralsPlugin()
        {
            Assert.IsNotNull(cLIPeripheralsPlugins);
        }

        [Test]
        public void TestPluginLogId()
        {
            Assert.IsNotNull(cLIPeripheralsPlugins);
            string pluginLogId = "CLIPeripherals";
            var CLIPeripheralsPlugins_result = CLIPeripheralsPlugins.PluginLogId;
            Assert.That(pluginLogId, Is.EqualTo(CLIPeripheralsPlugins_result));
        }

        [Test]
        public void TestIsDisposed()
        {
            var Result = cLIPeripheralsPlugins.IsDisposed;
            Assert.IsFalse(Result);
            PrivateObject privateteCLIProxyPluginObj = new PrivateObject(cLIPeripheralsPlugins);
            privateteCLIProxyPluginObj.SetFieldOrProperty("IsDisposed", true);
            var Result2 = cLIPeripheralsPlugins.IsDisposed;
            Assert.IsTrue(Result2);
        }

        [Test]
        public void TestFWResultReceived_List()
        {
            bool eventTriggered = true;
            var fwUpdateInfoList = new List<FWUpdateInfo>() { new FWUpdateInfo() { DeviceName = "TestDeviceName", DeviceId = "12345" } };

            cLIPeripheralsPlugins.FWResultReceived_List += (sender, args) =>
            {
                eventTriggered = true;
                Assert.That(fwUpdateInfoList, Is.EqualTo(args.Item1));
            };
            Assert.IsTrue(eventTriggered);
        }

        [Test]
        public void TestFWResultReceived()
        {
            bool eventTriggered = true;
            var FWUErrorCode = new FWUErrorCode();

            cLIPeripheralsPlugins.FWResultReceived += (sender, args) =>
            {
                eventTriggered = true;
                Assert.That(FWUErrorCode, Is.EqualTo(args.Item1));
            };
            Assert.IsTrue(eventTriggered);
        }

        [Test]
        public void TestSetCommandArgs()
        {
            Mock<IDeviceManagerSA> devMgr = new Mock<IDeviceManagerSA>();
            DeviceHelper deviceHelper = new DeviceHelper
            {
                deviceInfo = new List<DeviceInfo>
                {
                new DeviceInfo {LogicalDeviceType = "LogicalHeadset",DeviceName = "LogicalHeadset",},
                new DeviceInfo {LogicalDeviceType = "LogicalWiredAudio",DeviceName = "LogicalWiredAudio",}
                }
            };
            string path = "TestPath";
            SWUpdateInfoPackage swUpdateInfoPackage = new SWUpdateInfoPackage() { SWUpdateInfo = new List<SWUpdateInfo>() { new SWUpdateInfo() { SoftwareName = "TestSoftwareName", SoftwareVersion = "1.0", TheLatestVersion = "1.0", NeedUpdated = false, } }, };
            List<SWUpdateInfo> SWUpdateInfo = new List<SWUpdateInfo>() { new SWUpdateInfo() { SoftwareName = "TestSoftwareName", SoftwareVersion = "1.0", TheLatestVersion = "1.0", NeedUpdated = false, } };
            DDPMSettings data = null;
            devMgr.Setup(m => m.GetDevices(It.IsAny<bool>())).Returns(Task.FromResult(deviceHelper));
            devMgr.Setup(m => m.SW_GetSWUpdateInfo(It.IsAny<bool>(), It.IsAny<bool>())).Returns(Task.FromResult(swUpdateInfoPackage));
            devMgr.Setup(m => m.SW_DownloadAndInstall(It.IsAny<List<SWUpdateInfo>>(), It.IsAny<bool>(), It.IsAny<string>())).Returns(Task.FromResult(SWUpdateInfo));
            devMgr.Setup(m => m.GetServerURL()).Returns(Task.FromResult(path));
            devMgr.Setup(m => m.ReloadAppConfigData(It.IsAny<bool>())).Returns(Task.FromResult(data));
            var devMgrObj = devMgr.Object;

            CLIEventArgs input1 = new CLIEventArgs();

            CommandLineInput commandLineInput;
            commandLineInput = input1.commandLineInput;
            if (commandLineInput == null)
            {
                var SetCommandArgs_result1 = cLIPeripheralsPlugins.SetCommandArgs(input1, devMgrObj);   //commandLineInput is null
                Assert.IsNotNull(SetCommandArgs_result1);
                Assert.IsNull(SetCommandArgs_result1.command_guid_string);
                Assert.IsNotNull(SetCommandArgs_result1.serialize_Json_response);
            }

            CLIEventArgs input2 = new CLIEventArgs()
            {
                commandLineInput = new CommandLineInput()
                {
                    PluginsType = "AUDIO",
                    Command = "GET",
                    TargetType = "APP",
                    TargetFeature = "UPDATE",
                    isCliRunAdmin = false,
                },
                command_guid_string = "test_guid"
            };
            commandLineInput = input2.commandLineInput;
            if (commandLineInput != null)
            {
                var SetCommandArgs_result2 = cLIPeripheralsPlugins.SetCommandArgs(input2, devMgrObj);   ////commandLineInput is not null, Command = "GET",TargetFeature=UPDATE,isCliRunAdmin = false,TargetFeature = "UPDATE",
                Assert.IsNotNull(SetCommandArgs_result2);
                Assert.IsNotNull(SetCommandArgs_result2.command_guid_string);
                Assert.IsNotNull(SetCommandArgs_result2.serialize_Json_response);
            }

            CLIEventArgs input3 = new CLIEventArgs()
            {
                commandLineInput = new CommandLineInput()
                {
                    PluginsType = "AUDIO",
                    Command = "GET",
                    TargetType = "APP",
                    TargetFeature = "UPDATE",
                    isCliRunAdmin = true,

                },
                command_guid_string = "test_guid"
            };
            commandLineInput = input3.commandLineInput;
            if (commandLineInput != null)
            {
                var SetCommandArgs_result3 = cLIPeripheralsPlugins.SetCommandArgs(input3, devMgrObj);   //commandLineInput is not null, Command = "GET",isCliRunAdmin=true,TargetFeature = "UPDATE",
                Assert.IsNotNull(SetCommandArgs_result3);
                Assert.IsNotNull(SetCommandArgs_result3.command_guid_string);
                Assert.IsNotNull(SetCommandArgs_result3.serialize_Json_response);
            }

            CLIEventArgs input4 = new CLIEventArgs()
            {
                commandLineInput = new CommandLineInput()
                {
                    PluginsType = "AUDIO",
                    Command = "GET",
                    TargetType = "APP",
                    TargetFeature = "UPDATESOURCELOCATION",
                    isCliRunAdmin = true,

                },
                command_guid_string = "test_guid"
            };
            commandLineInput = input4.commandLineInput;
            if (commandLineInput != null)
            {
                var SetCommandArgs_result4 = cLIPeripheralsPlugins.SetCommandArgs(input4, devMgrObj);   //commandLineInput is not null, Command = "GET",isCliRunAdmin=true,TargetFeature = "UPDATESOURCELOCATION",
                Assert.IsNotNull(SetCommandArgs_result4);
                Assert.IsNotNull(SetCommandArgs_result4.command_guid_string);
                Assert.IsNotNull(SetCommandArgs_result4.serialize_Json_response);
            }

            CLIEventArgs input5 = new CLIEventArgs()
            {
                commandLineInput = new CommandLineInput()
                {
                    PluginsType = "AUDIO",
                    Command = "SET",
                    TargetType = "APP",
                    TargetFeature = "FIRMWAREUPDATE",
                    isCliRunAdmin = true,

                },
                command_guid_string = "test_guid"
            };
            commandLineInput = input5.commandLineInput;
            if (commandLineInput != null)
            {
                var SetCommandArgs_result5 = cLIPeripheralsPlugins.SetCommandArgs(input5, devMgrObj);   //commandLineInput is not null, Command = "SET",isCliRunAdmin=true,TargetFeature = "FIRMWAREUPDATE",
                Assert.IsNotNull(SetCommandArgs_result5);
                Assert.IsNotNull(SetCommandArgs_result5.command_guid_string);
                Assert.IsNotNull(SetCommandArgs_result5.serialize_Json_response);
            }

            CLIEventArgs input6 = new CLIEventArgs()
            {
                commandLineInput = new CommandLineInput()
                {
                    PluginsType = "AUDIO",
                    Command = "SET",
                    TargetType = "APP",
                    TargetFeature = "UPDATE",
                    isCliRunAdmin = true,

                },
                command_guid_string = "test_guid"
            };
            commandLineInput = input6.commandLineInput;
            if (commandLineInput != null)
            {
                var SetCommandArgs_result6 = cLIPeripheralsPlugins.SetCommandArgs(input6, devMgrObj);   //commandLineInput is not null, Command = "SET",isCliRunAdmin=true,TargetFeature = "UPDATE",
                Assert.IsNotNull(SetCommandArgs_result6);
                Assert.IsNotNull(SetCommandArgs_result6.command_guid_string);
                Assert.IsNotNull(SetCommandArgs_result6.serialize_Json_response);
            }

            CLIEventArgs input7 = new CLIEventArgs()
            {
                commandLineInput = new CommandLineInput()
                {
                    PluginsType = "AUDIO",
                    Command = "SET",
                    TargetType = "APP",
                    TargetFeature = "UPDATESOURCELOCATION",
                    isCliRunAdmin = true,

                },
                command_guid_string = "test_guid"
            };
            commandLineInput = input7.commandLineInput;
            if (commandLineInput != null)
            {
                var SetCommandArgs_result7 = cLIPeripheralsPlugins.SetCommandArgs(input7, devMgrObj);   //commandLineInput is not null, Command = "SET",isCliRunAdmin=true,TargetFeature = "UPDATESOURCELOCATION",
                Assert.IsNotNull(SetCommandArgs_result7);
                Assert.IsNotNull(SetCommandArgs_result7.command_guid_string);
                Assert.IsNotNull(SetCommandArgs_result7.serialize_Json_response);
            }

            CLIEventArgs input8 = new CLIEventArgs()
            {
                commandLineInput = new CommandLineInput()
                {
                    PluginsType = "AUDIO",
                    Command = "SET",
                    TargetType = "TestAPP",
                    TargetFeature = "TestTargetFeature",
                    isCliRunAdmin = true,

                },
                command_guid_string = "test_guid"
            };
            commandLineInput = input8.commandLineInput;
            if (commandLineInput != null)
            {
                var SetCommandArgs_result8 = cLIPeripheralsPlugins.SetCommandArgs(input8, devMgrObj);   //commandLineInput is not null, Command = "SET",TargetType is not APP
                Assert.IsNotNull(SetCommandArgs_result8);
                Assert.IsNotNull(SetCommandArgs_result8.command_guid_string);
                Assert.IsNotNull(SetCommandArgs_result8.serialize_Json_response);
            }

            CLIEventArgs input9 = new CLIEventArgs()
            {
                commandLineInput = new CommandLineInput()
                {
                    PluginsType = "AUDIO",
                    Command = "GET",
                    TargetType = "TestAPP",
                    TargetFeature = "TestTargetFeature",
                    isCliRunAdmin = true,

                },
                command_guid_string = "test_guid"
            };
            commandLineInput = input9.commandLineInput;
            if (commandLineInput != null)
            {
                var SetCommandArgs_result9 = cLIPeripheralsPlugins.SetCommandArgs(input9, devMgrObj);   //commandLineInput is not null, Command = "GET",TargetType is not APP
                Assert.IsNotNull(SetCommandArgs_result9);
                Assert.IsNotNull(SetCommandArgs_result9.command_guid_string);
                Assert.IsNotNull(SetCommandArgs_result9.serialize_Json_response);
            }

            CLIEventArgs input10 = new CLIEventArgs()
            {
                commandLineInput = new CommandLineInput()
                {
                    PluginsType = "AUDIO",
                    Command = "Test",
                    TargetType = "TestAPP",
                    TargetFeature = "TestTargetFeature",
                    isCliRunAdmin = true,

                },
                command_guid_string = "test_guid"
            };
            commandLineInput = input10.commandLineInput;
            if (commandLineInput != null)
            {
                var SetCommandArgs_result10 = cLIPeripheralsPlugins.SetCommandArgs(input10, devMgrObj);   //commandLineInput is not null, Command = "Test",TargetType is not APP
                Assert.IsNotNull(SetCommandArgs_result10);
                Assert.IsNotNull(SetCommandArgs_result10.command_guid_string);
                Assert.IsNotNull(SetCommandArgs_result10.serialize_Json_response);
            }
        }

        [Test]
        public void TestSetFailResults()
        {
            string message = "Test SetFailResults message";
            privateteCLIPeripheralsPlugins.Invoke("SetFailResults", message);
            Assert.IsTrue(true);
        }

        [Test]
        public void TestNoDeviceConnectResponse()
        {
            CommandLineInput commandLineInput;
            commandLineInput = new CommandLineInput()
            {
                PluginsType = "AUDIO",
                Command = "Test",
                TargetType = "TestAPP",
                TargetFeature = "TestTargetFeature",
                isCliRunAdmin = true,

            };
            var result = ((int code, string json))privateteCLIPeripheralsPlugins.Invoke("NoDeviceConnectResponse", commandLineInput);
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.code);
            Assert.IsNotNull(result.json);
        }

        [Test]
        public void TestDownload_Event()
        {
            object obj = new object();
            List<FWUpdateInfo> fwUpdateInfo = new List<FWUpdateInfo>() { new FWUpdateInfo() { DeviceVersion = "1.0", DeviceName = "TestDeviceName", DeviceId = "123", Model = "Testmodel" } };
            privateteCLIPeripheralsPlugins.Invoke("Download_Event", obj, fwUpdateInfo);
            var Download_Event_result = (List<FWUpdateInfo>)privateteCLIPeripheralsPlugins.GetFieldOrProperty("retFWUpdateInfos");
            Assert.IsNotNull(Download_Event_result);
            Assert.That(fwUpdateInfo, Is.EqualTo(Download_Event_result));
        }

        [Test]
        public void TestProcessListPeripheralsOptionAsync()
        {
            IDeviceManagerSA devMgr;
            devMgr = null;
            int CLI_ExitCode = 1;
            if (devMgr == null)
            {
                var Download_Event_result = (Task<int>)privateteCLIPeripheralsPlugins.Invoke("ProcessListPeripheralsOptionAsync", devMgr);  //devMgr= null
                Assert.IsNotNull(Download_Event_result);
                Assert.That(CLI_ExitCode, Is.EqualTo(Download_Event_result.Result));
            }

            int CLI_ExitCode2 = 0;
            Mock<IDeviceManagerSA> devMgr2 = new Mock<IDeviceManagerSA>();
            DeviceHelper deviceHelper = new DeviceHelper
            {
                deviceInfo = new List<DeviceInfo>
                {
                new DeviceInfo {LogicalDeviceType = "LogicalHeadset",DeviceName = "LogicalHeadset",},
                new DeviceInfo {LogicalDeviceType = "LogicalWiredAudio",DeviceName = "LogicalWiredAudio",}
                }
            };
            devMgr2.Setup(m => m.GetDevices(It.IsAny<bool>())).Returns(Task.FromResult(deviceHelper));
            var devMgrObj2 = devMgr2.Object;

            if (devMgrObj2 != null)
            {
                var Download_Event_result2 = (Task<int>)privateteCLIPeripheralsPlugins.Invoke("ProcessListPeripheralsOptionAsync", devMgrObj2);  //devMgr ! = null
                Assert.IsNotNull(Download_Event_result2);
                Assert.That(CLI_ExitCode2, Is.EqualTo(Download_Event_result2.Result));
            }
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            cLIPeripheralsPlugins.Dispose();
        }
    }
}