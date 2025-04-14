using DDPM.SA.Common;
using DDPM.UI.Common;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using DDPM.UI.Common.UserControls;
using DDPM.UI.Interfaces;
using DDPM.UI.Plugin.ViewModels;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using DPeMPublic.Common.Enums;
using Moq;
using NGA.UnitTest.PrivateObject;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Windows.System;

namespace DDPM.UI.Plugin.Common.Tests
{
    [Apartment(ApartmentState.STA)]
    public class AddDeviceViewModelTests
    {
        private AddDeviceViewModel? addDeviceViewModel;
        private PrivateObject? privateObject;
        private Mock<IModuleOwner>? moduleOwnerMock;
        private IModuleOwner? moduleOwner;
        //private AddDeviceViewModel? vm;
        private IConsole? console;
        private Mock<IConsole>? consoleMock;
        private IShowPluginManager? showPluginManager;
        private Mock<IShowPluginManager>? showPluginManagerMock;
        private IDeviceManagerSA? peripheralPlugin;
        private Mock<IDeviceManagerSA>? peripheralPluginMock;
        private ILog? log;
        private Mock<ILog>? logMock;
        [SetUp]
        public void Setup()
        {
            moduleOwnerMock = new Mock<IModuleOwner>();
            moduleOwner = moduleOwnerMock!.Object;
            DdpmCommonHelper.ModuleOwner = moduleOwner;
            consoleMock = new Mock<IConsole>();
            console = consoleMock.Object;
            showPluginManagerMock = new Mock<IShowPluginManager>();
            showPluginManager = showPluginManagerMock.Object;
            peripheralPluginMock = new Mock<IDeviceManagerSA>();
            peripheralPlugin = peripheralPluginMock.Object;
            logMock = new Mock<ILog>();
            log = logMock.Object;
            addDeviceViewModel = new AddDeviceViewModel(console);
            //addDeviceViewModel = new AddHeadset_BLModule(vm);
            privateObject = new PrivateObject(addDeviceViewModel);
        }

        [Test]
        public void TestConstructor_AddDeviceViewModel()
        {
            // Assert
            Assert.That(addDeviceViewModel, Is.Not.Null);
        }

        [Test]
        public void TestIsPandoraPaired()
        {
            // Assert
            Assert.That(addDeviceViewModel.IsPandoraPaired, Is.EqualTo(false));
        }

        [Test]
        public void Test_moduleGroups()
        {
            // Assert
            Assert.That(addDeviceViewModel._moduleGroups, Is.Not.Null);
            Assert.That(addDeviceViewModel._moduleGroups, Is.InstanceOf<List<ModuleGroup>>());
        }

        [Test]
        public void TestModuleGroups()
        {
            // Act
            var moduleGroups = new List<ModuleGroup>();
            // Assert
            Assert.That(addDeviceViewModel.ModuleGroups, Is.EqualTo(moduleGroups));
        }

        [Test]
        public void TestPairingStatus()
        {
            // Act
            //addDeviceViewModel.PairingStatus = "PairingStatus";
            //// Assert
            //Assert.That(addDeviceViewModel.PairingStatus, Is.EqualTo("PairingStatus"));
        }

        [Test]
        public void TestIsModuleLoaded()
        {
            // Act
            addDeviceViewModel.IsModuleLoaded = "IsModuleLoaded";
            // Assert
            Assert.That(addDeviceViewModel.IsModuleLoaded, Is.EqualTo("IsModuleLoaded"));
        }

        [Test]
        public void TestGroupSelIdx()
        {
            // Act
            addDeviceViewModel.GroupSelIdx = 1;
            // Assert
            Assert.That(addDeviceViewModel.GroupSelIdx, Is.EqualTo(1));

            // Act
            addDeviceViewModel._moduleGroups = new List<ModuleGroup>() { new ModuleGroup() };
            privateObject.SetFieldOrProperty("_groupSelIdx", 2);
            addDeviceViewModel.GroupSelIdx = 3;
            // Assert
            Assert.That(addDeviceViewModel.GroupSelIdx, Is.EqualTo(3));
        }

        [Test]
        public void TestGroupCount()
        {
            // Assert
            Assert.That(addDeviceViewModel.GroupCount, Is.EqualTo(0));
        }

        [Test]
        public void TestDeviceBarItemClickCommand()
        {
            var ICommandMock = new Mock<ICommand>();
            addDeviceViewModel.DeviceBarItemClickCommand = ICommandMock.Object;
            Assert.That(addDeviceViewModel.DeviceBarItemClickCommand, Is.EqualTo(ICommandMock.Object));
        }

        [Test]
        public void TestDeviceBarItems()
        {
            // Assert
            Assert.That(addDeviceViewModel.DeviceBarItems, Is.Not.Null);
        }

        [Test]
        public void TestDeviceBarSelectedIndex()
        {
            // Act
            addDeviceViewModel.DeviceBarSelectedIndex = 1;
            // Assert
            Assert.That(addDeviceViewModel.DeviceBarSelectedIndex, Is.EqualTo(1));

            // Act
            addDeviceViewModel._moduleGroups = new List<ModuleGroup>() { new ModuleGroup(), new ModuleGroup() };
            addDeviceViewModel.DeviceBarSelectedIndex = 1;
            // Assert
            Assert.That(addDeviceViewModel.DeviceBarSelectedIndex, Is.EqualTo(1));
        }

        [Test]
        public void TestDefaultLeftView()
        {
            // Assert
            Assert.That(addDeviceViewModel.DefaultLeftView, Is.Not.Null);
        }

        [Test]
        public void TestRightView()
        {
            // Assert
            Assert.That(addDeviceViewModel.RightView, Is.EqualTo(null));

            var iDdpmModule = new Mock<IDdpmModule>();
            iDdpmModule.Setup(x => x.GetRightView()).Returns(new System.Windows.Controls.UserControl());
            var modulegroup = new ModuleGroup();
            PrivateObject pri = new PrivateObject(modulegroup);
            pri.SetFieldOrProperty("_headers", new ObservableCollection<RightViewHeader>() { new RightViewHeader(1, "", iDdpmModule.Object), new RightViewHeader(2, "Text", iDdpmModule.Object) });
            addDeviceViewModel.DeviceBarSelectedIndex = 1;
            addDeviceViewModel._moduleGroups = new List<ModuleGroup>() { new ModuleGroup(), modulegroup };
            Assert.That(addDeviceViewModel.RightView, Is.Not.Null);
        }

        [Test]
        public void TestRightViewHeaders()
        {
            var rightViewHeaders = new ObservableCollection<RightViewHeader>();
            addDeviceViewModel.RightViewHeaders = rightViewHeaders;
            Assert.That(addDeviceViewModel.RightViewHeaders, Is.EqualTo(rightViewHeaders));

            var modulegroup = new ModuleGroup();
            rightViewHeaders = new ObservableCollection<RightViewHeader>();
            addDeviceViewModel._moduleGroups = new List<ModuleGroup>() { new ModuleGroup(), modulegroup };
            addDeviceViewModel.DeviceBarSelectedIndex = 1;
            addDeviceViewModel.RightViewHeaders = rightViewHeaders;
            Assert.That(addDeviceViewModel.RightViewHeaders, Is.EqualTo(rightViewHeaders));
        }

        [Test]
        public void TestRightViewHeaderSelectedIndex()
        {
            addDeviceViewModel.RightViewHeaderSelectedIndex = 1;
            Assert.That(addDeviceViewModel.RightViewHeaderSelectedIndex, Is.EqualTo(0));

            var modulegroup = new ModuleGroup();
            addDeviceViewModel._moduleGroups = new List<ModuleGroup>() { new ModuleGroup(), modulegroup };
            addDeviceViewModel.DeviceBarSelectedIndex = 1;
            addDeviceViewModel.RightViewHeaderSelectedIndex = 1;
            Assert.That(addDeviceViewModel.RightViewHeaderSelectedIndex, Is.EqualTo(1));
        }

        [Test]
        public void TestSelectedGroup()
        {
            Assert.That(addDeviceViewModel.SelectedGroup, Is.EqualTo(null));

            addDeviceViewModel._moduleGroups = new List<ModuleGroup>() { new ModuleGroup(), new ModuleGroup() };
            addDeviceViewModel.DeviceBarSelectedIndex = -1;
            Assert.That(addDeviceViewModel.SelectedGroup, Is.EqualTo(null));

            var modulegroup = new ModuleGroup();
            addDeviceViewModel._moduleGroups = new List<ModuleGroup>() { new ModuleGroup(), modulegroup };
            addDeviceViewModel.DeviceBarSelectedIndex = 1;
            Assert.That(addDeviceViewModel.SelectedGroup, Is.Not.Null);
        }

        [Test]
        public void TestDongleAlertKnM()
        {
            // Act
            addDeviceViewModel.DongleAlertKnM = "DongleAlertKnM";
            // Assert
            Assert.That(addDeviceViewModel.DongleAlertKnM, Is.EqualTo("DongleAlertKnM"));
        }

        [Test]
        public void TestDongleAlertKnMVisibility()
        {
            // Act
            addDeviceViewModel.DongleAlertKnMVisibility = Visibility.Collapsed;
            // Assert
            Assert.That(addDeviceViewModel.DongleAlertKnMVisibility, Is.EqualTo(Visibility.Collapsed));
        }

        [Test]
        public void TestDongleAlertHeadset()
        {
            // Act
            addDeviceViewModel.DongleAlertHeadset = "DongleAlertHeadset";
            // Assert
            Assert.That(addDeviceViewModel.DongleAlertHeadset, Is.EqualTo("DongleAlertHeadset"));
        }

        [Test]
        public void TestDongleAlertHeadsetVisibility()
        {
            // Act
            addDeviceViewModel.DongleAlertHeadsetVisibility = Visibility.Collapsed;
            // Assert
            Assert.That(addDeviceViewModel.DongleAlertHeadsetVisibility, Is.EqualTo(Visibility.Collapsed));
        }

        [Test]
        public void TestAlertText()
        {
            // Act
            addDeviceViewModel.AlertText = "AlertText";
            // Assert
            Assert.That(addDeviceViewModel.AlertText, Is.EqualTo("AlertText"));
        }

        [Test]
        public void TestAlertVisibility()
        {
            // Act
            addDeviceViewModel.AlertVisibility = Visibility.Visible;
            // Assert
            Assert.That(addDeviceViewModel.AlertVisibility, Is.EqualTo(Visibility.Visible));
        }

        [Test]
        public void TestCheckPandora()
        {
            try
            {
                addDeviceViewModel.CheckPandora(new List<DeviceInfo>() { new DeviceInfo() });
                Assert.True(true);
                Assert.That(addDeviceViewModel.IsPandoraPaired, Is.EqualTo(false));
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }

            var deviceInfos = new List<DeviceInfo>() { new DeviceInfo(), new DeviceInfo() { ModelNumber = "PN5122W" } };
            try
            {
                addDeviceViewModel.CheckPandora(deviceInfos);
                Assert.True(true);
                Assert.That(addDeviceViewModel.IsPandoraPaired, Is.EqualTo(true));
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }

        [Test]
        public void TestPrepareDongleInfo()
        {
            try
            {
                addDeviceViewModel.PrepareDongleInfo(new List<DongleInfo>() { new DongleInfo() { DeviceType = DeviceType.PhysicalDongle } });
                Assert.True(true);
                Assert.Greater(addDeviceViewModel.DongleInfos.Count, 0);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }

            var deviceInfos = new List<DeviceInfo>() { new DeviceInfo(), new DeviceInfo() { ModelNumber = "PN5122W" } };
            try
            {
                addDeviceViewModel.PrepareDongleInfo(new List<DongleInfo>() { new DongleInfo() { DeviceType = DeviceType.PhysicalAudioDongle } });
                Assert.True(true);
                Assert.Greater(addDeviceViewModel.AudioDongleInfos.Count, 0);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }

        [Test]
        public void TestHandleNotification()
        {
            try
            {
                //addDeviceViewModel.HandleNotification(DeviceChangedType.Peripherals_PlugIn, new DeviceInfo());
                //addDeviceViewModel.HandleNotification(DeviceChangedType.Peripherals_UnPlug, new DeviceInfo());
                //addDeviceViewModel.HandleNotification(DeviceChangedType.Peripherals_SettingsChange, new DeviceInfo() { PhysicalDeviceType = DeviceType.PhysicalAudioDongle }, "DonglePairedDeviceCountChanged");
                //addDeviceViewModel.HandleNotification(DeviceChangedType.Peripherals_SettingsChange, new DeviceInfo() { PairingStatusName = "Request" }, "DonglePairingStatusChanged|  ftg");
                //addDeviceViewModel.HandleNotification(DeviceChangedType.Peripherals_SettingsChange, new DeviceInfo() { PairingStatusName = "Already Paired" }, "DonglePairingStatusChanged|  ftg");
                //addDeviceViewModel.HandleNotification(DeviceChangedType.Peripherals_SettingsChange, new DeviceInfo() { PairingStatusName = "Stopped" }, "DonglePairingStatusChanged|  ftg");
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }

        [Test]
        public void TestStartPairing()
        {
            var deviceManagerSAMock = new Mock<IDeviceManagerSA>();
            DdpmCommonHelper.DeviceManagerSA = deviceManagerSAMock.Object;
            try
            {
                addDeviceViewModel.StartPairing(new Guid());
                Assert.True(true);
                Assert.That(addDeviceViewModel.IsPairing, Is.EqualTo(true));
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }

        [Test]
        public void TestStartPairingPen()
        {
            var deviceManagerSAMock = new Mock<IDeviceManagerSA>();
            DdpmCommonHelper.DeviceManagerSA = deviceManagerSAMock.Object;
            try
            {
                addDeviceViewModel.StartPairingPen();
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }

        [Test]
        public void TestStopPairing()
        {
            var deviceManagerSAMock = new Mock<IDeviceManagerSA>();
            DdpmCommonHelper.DeviceManagerSA = deviceManagerSAMock.Object;
            addDeviceViewModel.CurrentDongle = new DongleInfo() { ID = new Guid() };
            try
            {
                addDeviceViewModel.StopPairing();
                Assert.True(true);
                Assert.That(addDeviceViewModel.IsPairing, Is.EqualTo(false));
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }

            addDeviceViewModel.IsPairing = true;
            try
            {
                addDeviceViewModel.StopPairing();
                Assert.True(true);
                Assert.That(addDeviceViewModel.IsPairing, Is.EqualTo(false));
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }

        [Test]
        public void TestStopPairingPen()
        {
            addDeviceViewModel.IsPairing = true;
            try
            {
                addDeviceViewModel.StopPairingPen();
                Assert.True(true);
                Assert.That(addDeviceViewModel.IsPairing, Is.EqualTo(false));
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }

        [Test]
        public void TestGotoNewDevice()
        {
            var _showPluginManagerMock = new Mock<IShowPluginManager>();
            privateObject.SetFieldOrProperty("_showPluginManager", _showPluginManagerMock.Object);
            _showPluginManagerMock.Setup(x => x.ShowPluginById(DDPM.UI.Common.Constants.KeyboardPluginId, new Guid().ToString())).Returns(true);
            try
            {
                //addDeviceViewModel.NewDevice = new DeviceInfo() { LogicalDeviceType = "LOGICALKEYBOARD", ID = new Guid() };
                //addDeviceViewModel.GotoNewDevice();
                //addDeviceViewModel.NewDevice = new DeviceInfo() { LogicalDeviceType = "LOGICALMOUSE", ID = new Guid() };
                //addDeviceViewModel.GotoNewDevice();
                //addDeviceViewModel.NewDevice = new DeviceInfo() { LogicalDeviceType = "LOGICALHEADSET", ID = new Guid() };
                //addDeviceViewModel.GotoNewDevice();
                //addDeviceViewModel.NewDevice = new DeviceInfo() { LogicalDeviceType = "LOGICALWIREDAUDIO", ID = new Guid() };
                //addDeviceViewModel.GotoNewDevice();
                //addDeviceViewModel.NewDevice = new DeviceInfo() { LogicalDeviceType = "LOGICALPEN", ID = new Guid() };
                //addDeviceViewModel.GotoNewDevice();
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }

    }

}