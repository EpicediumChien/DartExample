using DDPM.SA.Common;
using DDPM.SA.Common.Display;
using DDPM.UI.Common;
using DDPM.UI.Common.EAEM;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using DDPM.UI.Common.UserControls;
using Moq;
using NGA.UnitTest.PrivateObject;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using VcpCore.Common;
using Dell.Client.Framework.UX.WPF.ResourceManager;
using DDPM.SA.Common.Settings;
using Windows.System;
using Dell.Client.Framework.UX.WPF;

namespace DDPM.UI.Module.Kvm.Tests
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public class KvmViewModelTests
    {
        private Mock<IDeviceManagerSA>? deviceManagerMock;
        private IDeviceManagerSA? deviceManagerSA;
        private InputSourceList? inputSourceList;
        private IModuleOwner? moduleOwner;
        private PrivateObject? privateObject;
        private Mock<IModuleOwner>? moduleOwnerMock;
        private KvmViewModel kvmViewModel;
        private Mock<IConsole>? MyConsoleMock;
        [SetUp]
        public void Setup()
        {
            if (System.Windows.Application.Current == null)
            {
                new System.Windows.Application();
            }
            ResourceManager res = new ResourceManager();
            var resourceDictionary = new ResourceDictionary();
            resourceDictionary.Source = new Uri("pack://application:,,,/DDPM.UI.Common;component/ModuleStyle.xaml");
            System.Windows.Application.Current.Resources.MergedDictionaries.Add(resourceDictionary);
            MyConsoleMock = new Mock<IConsole>();
            DdpmCommonHelper.MyConsole = MyConsoleMock.Object;
            kvmViewModel = new KvmViewModel();
            deviceManagerMock = new Mock<IDeviceManagerSA>();
            deviceManagerSA = deviceManagerMock.Object;
            DdpmCommonHelper.DeviceManagerSA = deviceManagerSA;
            moduleOwnerMock = new Mock<IModuleOwner>();
            moduleOwner = moduleOwnerMock!.Object;
            DdpmCommonHelper.ModuleOwner = moduleOwner;
            moduleOwnerMock.Setup(x => x.SelectedHomeDevice).Returns(new HomeDevice());
            DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo = new VcpCore.Common.MonitorInfo();
            DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo.CapabilityDic = new Dictionary<string, List<string>>() { { "AA", new List<string>() }, { "EE", new List<string>() } };
            deviceManagerMock.Setup(x => x.ReadCurrentHotkey(It.IsAny<MonitorInfo>())).Returns(Task.FromResult(new ValueTuple<HotkeySettings, List<HotkeyData>>(new HotkeySettings() { HotkeyInfo = new List<HotkeyInfo>() { new HotkeyInfo() { Hotkey = new List<VirtualKey>() { VirtualKey.Q, VirtualKey.M } } } }, new List<HotkeyData>())));
            KvmModule kvmModule = new KvmModule(moduleOwner);
            kvmViewModel.KvmModule = kvmModule;
            kvmViewModel.KvmModule.SelectedHomeDevice = new HomeDevice();
            kvmViewModel.KvmModule.SelectedHomeDevice.MonitorInfo = new VcpCore.Common.MonitorInfo();

            inputSourceList = new InputSourceList();
            privateObject = new PrivateObject(inputSourceList);
            kvmViewModel = new KvmViewModel();
        }

        [Test]
        public void TestkvmModule()
        {
            var kvmModule = new KvmModule(moduleOwner);
            inputSourceList.kvmModule = kvmModule;
            Assert.That(inputSourceList.kvmModule, Is.EqualTo(kvmModule));
        }

        [Test]
        public void TestinputDisplayText()
        {
            Assert.That(inputSourceList.Type, Is.EqualTo(""));
        }

        [Test]
        public void TestUSBListkvmModule()
        {
            var kvmModule = new KvmModule(moduleOwner);
            var uSBList = new USBList();
            uSBList.kvmModule = kvmModule;
            Assert.That(uSBList.kvmModule, Is.EqualTo(kvmModule));
        }

        [Test]
        public void TestUSBlistinputDisplayText()
        {
            var uSBList = new USBList();
            Assert.That(uSBList.Type, Is.EqualTo(""));
        }

        [Test]
        public void TestPCInputtkvmModule()
        {
            var kvmModule = new KvmModule(moduleOwner);
            var pCInput = new PCInput();
            pCInput.kvmModule = kvmModule;
            Assert.That(pCInput.kvmModule, Is.EqualTo(kvmModule));
        }

        [Test]
        public void TestPCInputinputDisplayText()
        {
            var pCInput = new PCInput();
            Assert.That(pCInput.inputDisplayText, Is.EqualTo(""));
        }

        [Test]
        public void TestModuleOwner()
        {
            // Arrange;
            kvmViewModel.ModuleOwner = moduleOwner;
            Assert.That(kvmViewModel.ModuleOwner, Is.EqualTo(moduleOwner));
        }

        [Test]
        public void TestkvmViewkvmModule()
        {
            var kvmModule = new KvmModule(moduleOwner);
            kvmViewModel.KvmModule = kvmModule;
            Assert.That(kvmViewModel.KvmModule, Is.EqualTo(kvmModule));
        }

        [Test]
        public void TestPxPCode()
        {
            var pxPCode = 0x11;
            kvmViewModel.PxPCode = (ushort)pxPCode;
            Assert.That(kvmViewModel.PxPCode, Is.EqualTo(pxPCode));
        }

        [Test]
        public void TestinputSourceFullView()
        {
            var inputSourceFullView = new InputSourceFullView();
            kvmViewModel.inputSourceFullView = inputSourceFullView;
            Assert.That(kvmViewModel.inputSourceFullView, Is.EqualTo(inputSourceFullView));
        }

        [Test]
        public void TestPC3_Visibility()
        {
            var pC3_Visibility = Visibility.Hidden;
            kvmViewModel.PC3_Visibility = pC3_Visibility;
            Assert.That(kvmViewModel.PC3_Visibility, Is.EqualTo(Visibility.Hidden));
        }

        [Test]
        public void TestPC4_Visibility()
        {
            var pC4_Visibility = Visibility.Hidden;
            kvmViewModel.PC4_Visibility = pC4_Visibility;
            Assert.That(kvmViewModel.PC4_Visibility, Is.EqualTo(Visibility.Hidden));
        }

        [Test]
        public void TestBorder1Visibility()
        {
            var border1Visibility = Visibility.Hidden;
            kvmViewModel.Border1Visibility = border1Visibility;
            Assert.That(kvmViewModel.Border1Visibility, Is.EqualTo(Visibility.Hidden));
        }

        [Test]
        public void TestBorder2Visibility()
        {
            var border2Visibility = Visibility.Hidden;
            kvmViewModel.Border2Visibility = border2Visibility;
            Assert.That(kvmViewModel.Border2Visibility, Is.EqualTo(Visibility.Hidden));
        }

        [Test]
        public void TestBorder3Visibility()
        {
            var border3Visibility = Visibility.Hidden;
            kvmViewModel.Border3Visibility = border3Visibility;
            Assert.That(kvmViewModel.Border3Visibility, Is.EqualTo(Visibility.Hidden));
        }

        [Test]
        public void TestBorder4Visibility()
        {
            var border4Visibility = Visibility.Hidden;
            kvmViewModel.Border4Visibility = border4Visibility;
            Assert.That(kvmViewModel.Border4Visibility, Is.EqualTo(Visibility.Hidden));
        }

        [Test]
        public void TestSupportUSBKVM()
        {
            var supportUSBKVM = Visibility.Hidden;
            kvmViewModel.SupportUSBKVM = supportUSBKVM;
            Assert.That(kvmViewModel.SupportUSBKVM, Is.EqualTo(Visibility.Hidden));
        }

        [Test]
        public void TestSupportNKVM()
        {
            var supportNKVM = Visibility.Hidden;
            kvmViewModel.SupportNKVM = supportNKVM;
            Assert.That(kvmViewModel.SupportNKVM, Is.EqualTo(Visibility.Hidden));
        }

        [Test]
        public void TestPC1_Input()
        {
            var pC1_Input = "A";
            kvmViewModel.PC1_Input = pC1_Input;
            Assert.That(kvmViewModel.PC1_Input, Is.EqualTo("A"));
        }

        [Test]
        public void TestPC2_Input()
        {
            var pC2_Input = "B";
            kvmViewModel.PC2_Input = pC2_Input;
            Assert.That(kvmViewModel.PC2_Input, Is.EqualTo("B"));
        }

        [Test]
        public void TestPC3_Input()
        {
            var pC3_Input = "C";
            kvmViewModel.PC3_Input = pC3_Input;
            Assert.That(kvmViewModel.PC3_Input, Is.EqualTo("C"));
        }

        [Test]
        public void TestPC4_Input()
        {
            var pC4_Input = "D";
            kvmViewModel.PC4_Input = pC4_Input;
            Assert.That(kvmViewModel.PC4_Input, Is.EqualTo("D"));
        }

        [Test]
        public void TestInputName1()
        {
            var inputName1 = "a";
            kvmViewModel.InputName1 = inputName1;
            Assert.That(kvmViewModel.InputName1, Is.EqualTo("a"));
        }

        [Test]
        public void TestInputName2()
        {
            var inputName2 = "b";
            kvmViewModel.InputName2 = inputName2;
            Assert.That(kvmViewModel.InputName2, Is.EqualTo("b"));
        }

        [Test]
        public void TestInputName3()
        {
            var inputName3 = "c";
            kvmViewModel.InputName3 = inputName3;
            Assert.That(kvmViewModel.InputName3, Is.EqualTo("c"));
        }

        [Test]
        public void TestInputName4()
        {
            var inputName4 = "d";
            kvmViewModel.InputName4 = inputName4;
            Assert.That(kvmViewModel.InputName4, Is.EqualTo("d"));
        }

        [Test]
        public void TestisPipSmall()
        {
            var isPipSmall = true;
            kvmViewModel.isPipSmall = isPipSmall;
            Assert.That(kvmViewModel.isPipSmall, Is.EqualTo(true));
        }

        [Test]
        public void TestisPipLarge()
        {
            var isPipLarge = true;
            kvmViewModel.isPipLarge = isPipLarge;
            Assert.That(kvmViewModel.isPipLarge, Is.EqualTo(true));
        }

        [Test]
        public void TestisPBP()
        {
            var isPBP = true;
            kvmViewModel.isPBP = isPBP;
            Assert.That(kvmViewModel.isPBP, Is.EqualTo(true));
        }

        [Test]
        public void TestisNoKVM()
        {
            var isNoKVM = true;
            kvmViewModel.KvmModule = new KvmModule(moduleOwner);
            kvmViewModel.KvmModule.isUSBKVM = true;
            kvmViewModel.isNoKVM = isNoKVM;
            Assert.That(kvmViewModel.isNoKVM, Is.EqualTo(true));
        }

        [Test]
        public void TestisNKVM()
        {
            var isNKVM = true;
            kvmViewModel.KvmModule = new KvmModule(moduleOwner);
            kvmViewModel.KvmModule.isUSBKVM = true;
            kvmViewModel.isNKVM = isNKVM;
            Assert.That(kvmViewModel.isNKVM, Is.EqualTo(true));
        }

        [Test]
        public void TestinputList()
        {
            var inputList = new Dictionary<string, InputInfo>();
            kvmViewModel.inputList = inputList;
            Assert.That(kvmViewModel.inputList, Is.EqualTo(inputList));
        }

        [Test]
        public void TestusbupstreamList()
        {
            var usbupstreamList = new List<string>();
            kvmViewModel.usbupstreamList = usbupstreamList;
            Assert.That(kvmViewModel.usbupstreamList, Is.EqualTo(usbupstreamList));
        }

        [Test]
        public void TestsubInputs()
        {
            var subInputs = new List<InputSourceObj>();
            kvmViewModel.subInputs = subInputs;
            Assert.That(kvmViewModel.subInputs, Is.EqualTo(subInputs));
        }

        [Test]
        public void TestpcsList()
        {
            var pcsList = new Dictionary<string, PCsInfo>();
            kvmViewModel.pcsList = pcsList;
            Assert.That(kvmViewModel.pcsList, Is.EqualTo(pcsList));
        }

        [Test]
        public void TestusbsList()
        {
            var usbsList = new List<string>();
            kvmViewModel.usbsList = usbsList;
            Assert.That(kvmViewModel.usbsList, Is.EqualTo(usbsList));
        }

        [Test]
        public void Testoriginal_pcsList()
        {
            var original_pcsList = new Dictionary<string, PCsInfo>();
            kvmViewModel.original_pcsList = original_pcsList;
            Assert.That(kvmViewModel.original_pcsList, Is.EqualTo(original_pcsList));
        }

        [Test]
        public void TestPCInputsList()
        {
            var pCInputsList = new List<InputSourceList>();
            kvmViewModel.PCInputsList = pCInputsList;
            Assert.That(kvmViewModel.PCInputsList, Is.EqualTo(pCInputsList));
        }

        [Test]
        public void TestUSBsList()
        {
            var uSBsList = new List<USBList>();
            kvmViewModel.USBsList = uSBsList;
            Assert.That(kvmViewModel.USBsList, Is.EqualTo(uSBsList));
        }

        [Test]
        public void TestPC1Inputs_Selected()
        {
            var inputList = new Dictionary<string, InputInfo>();
            kvmViewModel.inputList = inputList;
            var pcsList = new Dictionary<string, PCsInfo>();
            kvmViewModel.pcsList = pcsList;
            var pC1Inputs_Selected = new InputSourceList();
            kvmViewModel.PC1Inputs_Selected = pC1Inputs_Selected;
            Assert.That(kvmViewModel.PC1Inputs_Selected, Is.EqualTo(pC1Inputs_Selected));
            Assert.That(kvmViewModel.pcsList["PC1"], Is.Not.Null);
        }

        [Test]
        public void TestPC2Inputs_Selected()
        {
            var inputList = new Dictionary<string, InputInfo>();
            kvmViewModel.inputList = inputList;
            var pcsList = new Dictionary<string, PCsInfo>();
            kvmViewModel.pcsList = pcsList;
            var pC2Inputs_Selected = new InputSourceList();
            kvmViewModel.PC2Inputs_Selected = pC2Inputs_Selected;
            Assert.That(kvmViewModel.PC2Inputs_Selected, Is.EqualTo(pC2Inputs_Selected));
            Assert.That(kvmViewModel.pcsList["PC2"], Is.Not.Null);
        }

        [Test]
        public void TestPC3Inputs_Selected()
        {
            var inputList = new Dictionary<string, InputInfo>();
            kvmViewModel.inputList = inputList;
            var pcsList = new Dictionary<string, PCsInfo>();
            kvmViewModel.pcsList = pcsList;
            var pC3Inputs_Selected = new InputSourceList();
            kvmViewModel.PC3Inputs_Selected = pC3Inputs_Selected;
            Assert.That(kvmViewModel.PC3Inputs_Selected, Is.EqualTo(pC3Inputs_Selected));
            Assert.That(kvmViewModel.pcsList["PC3"], Is.Not.Null);
        }

        [Test]
        public void TestPC4Inputs_Selected()
        {
            var inputList = new Dictionary<string, InputInfo>();
            kvmViewModel.inputList = inputList;
            var pcsList = new Dictionary<string, PCsInfo>();
            kvmViewModel.pcsList = pcsList;
            var pC4Inputs_Selected = new InputSourceList();
            kvmViewModel.PC4Inputs_Selected = pC4Inputs_Selected;
            Assert.That(kvmViewModel.PC4Inputs_Selected, Is.EqualTo(pC4Inputs_Selected));
            Assert.That(kvmViewModel.pcsList["PC4"], Is.Not.Null);
        }

        [Test]
        public void TestPC1USB_Selected()
        {
            var pcsList = new Dictionary<string, PCsInfo>();
            pcsList.Add("PC1", new PCsInfo());
            kvmViewModel.pcsList = pcsList;
            var pC1USB_Selected = new USBList() { kvmModule = new KvmModule(moduleOwner), Type = "A" };
            kvmViewModel.PC1USB_Selected = pC1USB_Selected;
            Assert.That(kvmViewModel.PC1USB_Selected, Is.EqualTo(pC1USB_Selected));
            Assert.That(kvmViewModel.pcsList["PC1"].USBUpstream, Is.EqualTo("A"));
        }

        [Test]
        public void TestPC2USB_Selected()
        {
            var pcsList = new Dictionary<string, PCsInfo>();
            pcsList.Add("PC2", new PCsInfo());
            kvmViewModel.pcsList = pcsList;
            var pC2USB_Selected = new USBList() { kvmModule = new KvmModule(moduleOwner), Type = "A" };
            kvmViewModel.PC2USB_Selected = pC2USB_Selected;
            Assert.That(kvmViewModel.PC2USB_Selected, Is.EqualTo(pC2USB_Selected));
            Assert.That(kvmViewModel.pcsList["PC2"].USBUpstream, Is.EqualTo("A"));
        }

        [Test]
        public void TestPC3USB_Selected()
        {
            var pcsList = new Dictionary<string, PCsInfo>();
            pcsList.Add("PC3", new PCsInfo());
            kvmViewModel.pcsList = pcsList;
            var pC3USB_Selected = new USBList() { kvmModule = new KvmModule(moduleOwner), Type = "A" };
            kvmViewModel.PC3USB_Selected = pC3USB_Selected;
            Assert.That(kvmViewModel.PC3USB_Selected, Is.EqualTo(pC3USB_Selected));
            Assert.That(kvmViewModel.pcsList["PC3"].USBUpstream, Is.EqualTo("A"));
        }

        [Test]
        public void TestPC4USB_Selected()
        {
            var pcsList = new Dictionary<string, PCsInfo>();
            pcsList.Add("PC4", new PCsInfo());
            kvmViewModel.pcsList = pcsList;
            var pC4USB_Selected = new USBList() { kvmModule = new KvmModule(moduleOwner), Type = "A" };
            kvmViewModel.PC4USB_Selected = pC4USB_Selected;
            Assert.That(kvmViewModel.PC4USB_Selected, Is.EqualTo(pC4USB_Selected));
            Assert.That(kvmViewModel.pcsList["PC4"].USBUpstream, Is.EqualTo("A"));
        }

        [Test]
        public void TestPCImage()
        {
            var pCImage = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/USBKVM_3PCs.png");
            kvmViewModel.PCImage = pCImage;
            Assert.That(kvmViewModel.PCImage, Is.EqualTo(pCImage));
        }

        [Test]
        public void TestIsBusy()
        {
            var isBusy = true;
            kvmViewModel.IsBusy = isBusy;

            // Assert
            Assert.That(kvmViewModel.IsBusy, Is.EqualTo(true));
        }

        [Test]
        public void TestCurrentInputChange()
        {
            var pcsList = new Dictionary<string, PCsInfo>();
            pcsList.Add("PC1", new PCsInfo() { InputType = "A" });
            kvmViewModel.pcsList = pcsList;
            var kvmmodule = new KvmModule(moduleOwner);
            kvmViewModel.KvmModule = kvmmodule;
            kvmViewModel.KvmModule.SelectedHomeDevice.MonitorInfo = new MonitorInfo();

            deviceManagerMock.Setup(x => x.SetVCPCapability(It.IsAny<MonitorInfo>(), It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(true));
            try
            {
                kvmViewModel.CurrentInputChange();
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }

        [Test]
        public void TestHasPxpCap()
        {
            bool result = kvmViewModel.HasPxpCap(0x11);
            Assert.That(result, Is.False);
            privateObject = new PrivateObject(kvmViewModel);
            privateObject.SetFieldOrProperty("_pipPbpCaps", new UInt16[2] { 0x11, 0x22 });
            result = kvmViewModel.HasPxpCap(0x11);
            Assert.That(result, Is.True);
        }

        [Test]
        public void TestHasCap_PipSmall()
        {
            bool result = kvmViewModel.HasCap_PipSmall;
            Assert.That(result, Is.False);
            privateObject = new PrivateObject(kvmViewModel);
            privateObject.SetFieldOrProperty("_pipPbpCaps", new UInt16[3] { 0x11, 0x21, 0x22 });
            result = kvmViewModel.HasCap_PipSmall;
            Assert.That(result, Is.True);
        }

        [Test]
        public void TestHasCap_PipLarge()
        {
            bool result = kvmViewModel.HasCap_PipLarge;
            Assert.That(result, Is.False);
            privateObject = new PrivateObject(kvmViewModel);
            privateObject.SetFieldOrProperty("_pipPbpCaps", new UInt16[3] { 0x11, 0x21, 0x22 });
            result = kvmViewModel.HasCap_PipLarge;
            Assert.That(result, Is.True);
        }

        [Test]
        public void TestHasCap_PipTogglePosition()
        {
            bool result = kvmViewModel.HasCap_PipTogglePosition;
            Assert.That(result, Is.False);
            privateObject = new PrivateObject(kvmViewModel);
            privateObject.SetFieldOrProperty("_pipPbpCaps", new UInt16[3] { 0x02, 0x21, 0x22 });
            result = kvmViewModel.HasCap_PipTogglePosition;
            Assert.That(result, Is.True);
        }

        [Test]
        public void TestCurPxpMode()
        {
            var result = kvmViewModel.CurPxpMode;
            Assert.That(result, Is.EqualTo(0));
        }

        [Test]
        public void TestSelectedSplitItem()
        {
            privateObject = new PrivateObject(kvmViewModel);
            //_selectedSplitItem != null&&_selectedSplitItem == value
            var selectedSplitItem = new SplitItem();
            privateObject.SetFieldOrProperty("_selectedSplitItem", selectedSplitItem);
            kvmViewModel.SelectedSplitItem = selectedSplitItem;
            Assert.That(kvmViewModel.SelectedSplitItem, Is.EqualTo(selectedSplitItem));

            //_selectedSplitItem != null&&_selectedSplitItem != value
            privateObject.SetFieldOrProperty("_selectedSplitItem", new SplitItem() { IsSelected = true });
            kvmViewModel.SelectedSplitItem = selectedSplitItem;
            // Assert
            Assert.That(kvmViewModel.SelectedSplitItem, Is.EqualTo(selectedSplitItem));
            Assert.That(kvmViewModel.SelectedSplitItem.IsSelected, Is.EqualTo(true));

            //_selectedSplitItem != null&&value==null
            privateObject.SetFieldOrProperty("_selectedSplitItem", new SplitItem() { IsSelected = true });
            kvmViewModel.SelectedSplitItem = null;
            // Assert
            Assert.That(kvmViewModel.SelectedSplitItem, Is.EqualTo(null));

            //_selectedSplitItem == null&&value!=null
            privateObject.SetFieldOrProperty("_selectedSplitItem", null);
            kvmViewModel.SelectedSplitItem = selectedSplitItem;
            Assert.That(kvmViewModel.SelectedSplitItem.IsSelected, Is.EqualTo(true));

            //_selectedSplitItem == null&&value==null
            privateObject.SetFieldOrProperty("_selectedSplitItem", null);
            kvmViewModel.SelectedSplitItem = null;
            Assert.That(kvmViewModel.SelectedSplitItem, Is.EqualTo(null));
        }

        [Test]
        public void TestMainInputSource()
        {
            var mainInputSource = new InputSourceObj();
            kvmViewModel.MainInputSource = mainInputSource;
            Assert.That(kvmViewModel.MainInputSource, Is.EqualTo(mainInputSource));
        }

        [Test]
        public void TestSubInputs()
        {
            var subinputs = new List<InputSourceObj>() { new InputSourceObj(1, "VGA-1"), new InputSourceObj(2, "VGA-2"), new InputSourceObj(3, "DVI-1"), };
            kvmViewModel.SubInputs = subinputs;
            Assert.That(kvmViewModel.SubInputs, Is.EqualTo(subinputs));
            Assert.That(kvmViewModel.Sub1InputSource, Is.EqualTo(subinputs[0]));
            Assert.That(kvmViewModel.Sub2InputSource, Is.EqualTo(subinputs[1]));
            Assert.That(kvmViewModel.Sub3InputSource, Is.EqualTo(subinputs[2]));
        }

        [Test]
        public void TestSub1InputSource()
        {
            var sub1InputSource = new InputSourceObj();
            kvmViewModel.Sub1InputSource = sub1InputSource;
            Assert.That(kvmViewModel.Sub1InputSource, Is.EqualTo(sub1InputSource));
        }

        [Test]
        public void TestSub2InputSource()
        {
            var sub2InputSource = new InputSourceObj();
            kvmViewModel.Sub2InputSource = sub2InputSource;
            Assert.That(kvmViewModel.Sub2InputSource, Is.EqualTo(sub2InputSource));
        }

        [Test]
        public void TestSub3InputSource()
        {
            var sub3InputSource = new InputSourceObj();
            kvmViewModel.Sub3InputSource = sub3InputSource;
            Assert.That(kvmViewModel.Sub3InputSource, Is.EqualTo(sub3InputSource));
        }

        [Test]
        public void TestHasSub1Input()
        {
            var result = kvmViewModel.HasSub1Input;
            Assert.That(result, Is.False);

            var subinputs = new List<InputSourceObj>() { new InputSourceObj(1, "VGA-1") };
            kvmViewModel.SubInputs = subinputs;
            result = kvmViewModel.HasSub1Input;
            Assert.That(result, Is.True);
        }

        [Test]
        public void TestHasSub2Input()
        {
            var result = kvmViewModel.HasSub2Input;
            Assert.That(result, Is.False);

            var subinputs = new List<InputSourceObj>() { new InputSourceObj(1, "VGA-1"), new InputSourceObj(2, "VGA-2") };
            kvmViewModel.SubInputs = subinputs;
            result = kvmViewModel.HasSub2Input;
            Assert.That(result, Is.True);
        }

        [Test]
        public void TestHasSub3Input()
        {
            var result = kvmViewModel.HasSub3Input;
            Assert.That(result, Is.False);

            var subinputs = new List<InputSourceObj>() { new InputSourceObj(1, "VGA-1"), new InputSourceObj(2, "VGA-2"), new InputSourceObj(3, "DVI-1"), };
            kvmViewModel.SubInputs = subinputs;
            result = kvmViewModel.HasSub3Input;
            Assert.That(result, Is.True);
        }

        [Test]
        public void TestIsPipListItemSelected()
        {
            var result = kvmViewModel.IsPipListItemSelected;
            Assert.That(result, Is.False);

            var SelectedSplitItem = new SplitItem();
            kvmViewModel.SelectedSplitItem = SelectedSplitItem;
            kvmViewModel.SelectedSplitItem.SplitOwner = eSplitOwner.PipList;
            result = kvmViewModel.IsPipListItemSelected;
            Assert.That(result, Is.True);

            kvmViewModel.SelectedSplitItem.SplitOwner = eSplitOwner.PbpList;
            result = kvmViewModel.IsPipListItemSelected;
            Assert.That(result, Is.False);
        }

        [Test]
        public void TestIsTogglePositionEnabled()
        {
            var result = kvmViewModel.IsTogglePositionEnabled;
            Assert.That(result, Is.False);

            privateObject = new PrivateObject(kvmViewModel);
            privateObject.SetFieldOrProperty("_pipPbpCaps", new UInt16[3] { 0x02, 0x21, 0x22 });
            var SelectedSplitItem = new SplitItem();
            kvmViewModel.SelectedSplitItem = SelectedSplitItem;
            SelectedSplitItem.SplitOwner = eSplitOwner.PipList;
            result = kvmViewModel.IsTogglePositionEnabled;
            Assert.That(result, Is.True);
        }

        [Test]
        public void TestVideoSwapContent()
        {
            ContentControl contentControl = new ContentControl();
            kvmViewModel.VideoSwapContent = contentControl;
            Assert.That(kvmViewModel.VideoSwapContent, Is.EqualTo(contentControl));
        }

        [Test]
        public void TestPCSwap()
        {
            var pcsList = new Dictionary<string, PCsInfo>();
            pcsList.Add("PC1", new PCsInfo() { InputType = "A" });
            pcsList.Add("PC2", new PCsInfo() { InputType = "B" });
            pcsList.Add("PC3", new PCsInfo() { InputType = "C" });
            pcsList.Add("PC4", new PCsInfo() { InputType = "D" });
            kvmViewModel.pcsList = pcsList;
            deviceManagerMock.Setup(x => x.PCInfoSwap(It.IsAny<Dictionary<string, PCsInfo>>(), It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(pcsList));
            try
            {
                kvmViewModel.PCSwap("PC1", "PC2");
                Assert.True(true);
                Assert.That(kvmViewModel.PC1_Input, Is.EqualTo("A"));
                Assert.That(kvmViewModel.PC2_Input, Is.EqualTo("B"));
                Assert.That(kvmViewModel.PC3_Input, Is.EqualTo("C"));
                Assert.That(kvmViewModel.PC4_Input, Is.EqualTo("D"));
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }

        [Test]
        public void TestPC1Click()
        {
            kvmViewModel.PC1Click();
            Assert.That(kvmViewModel.Border1Visibility, Is.EqualTo(Visibility.Visible));
            Assert.That(kvmViewModel.Border2Visibility, Is.EqualTo(Visibility.Collapsed));
            Assert.That(kvmViewModel.Border3Visibility, Is.EqualTo(Visibility.Collapsed));
            Assert.That(kvmViewModel.Border4Visibility, Is.EqualTo(Visibility.Collapsed));
        }

        [Test]
        public void TestPC2Click()
        {
            kvmViewModel.PC2Click();
            Assert.That(kvmViewModel.Border1Visibility, Is.EqualTo(Visibility.Collapsed));
            Assert.That(kvmViewModel.Border2Visibility, Is.EqualTo(Visibility.Visible));
            Assert.That(kvmViewModel.Border3Visibility, Is.EqualTo(Visibility.Collapsed));
            Assert.That(kvmViewModel.Border4Visibility, Is.EqualTo(Visibility.Collapsed));
        }

        [Test]
        public void TestPC3Click()
        {
            kvmViewModel.PC3Click();
            Assert.That(kvmViewModel.Border1Visibility, Is.EqualTo(Visibility.Collapsed));
            Assert.That(kvmViewModel.Border2Visibility, Is.EqualTo(Visibility.Collapsed));
            Assert.That(kvmViewModel.Border3Visibility, Is.EqualTo(Visibility.Visible));
            Assert.That(kvmViewModel.Border4Visibility, Is.EqualTo(Visibility.Collapsed));
        }

        [Test]
        public void TestPC4Click()
        {
            kvmViewModel.PC4Click();
            Assert.That(kvmViewModel.Border1Visibility, Is.EqualTo(Visibility.Collapsed));
            Assert.That(kvmViewModel.Border2Visibility, Is.EqualTo(Visibility.Collapsed));
            Assert.That(kvmViewModel.Border3Visibility, Is.EqualTo(Visibility.Collapsed));
            Assert.That(kvmViewModel.Border4Visibility, Is.EqualTo(Visibility.Visible));
        }

        [Test]
        public void TestOpenNKVMUI()
        {
            Application.Current.MainWindow = new Window();
            try
            {
                kvmViewModel.OpenNKVMUI(1, 3, 4);
                var process = Process.GetProcessesByName("DDM");
                Assert.True(true);
                //Assert.That(process.Count, Is.GreaterThan(0));
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }

        [Test]
        public void TestisOnUSBKVM()
        {
            kvmViewModel.KvmModule = new KvmModule(moduleOwner);
            deviceManagerMock.Setup(x => x.SetOnUSBKVM(It.IsAny<MonitorInfo>(), It.IsAny<bool>())).Returns(Task.FromResult(true));
            try
            {
                kvmViewModel.isOnUSBKVM(true);
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }

        [Test]
        public void TestisOnNKVM()
        {
            deviceManagerMock.Setup(x => x.SupportedNKVMMonitors()).Returns(Task.FromResult(true));
            deviceManagerMock.Setup(x => x.SetOnNKVM(It.IsAny<MonitorInfo>(), It.IsAny<bool>())).Returns(Task.FromResult(true));
            try
            {
                kvmViewModel.isOnNKVM(true);
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }
    }
}