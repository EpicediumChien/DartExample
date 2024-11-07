using DDPM.SA.Common;
using DDPM.SA.Common.Display;
using DDPM.SA.Common.Settings;
using DDPM.UI.Common.EAEM;
using DDPM.UI.Common.Interfaces;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using Moq;
using System.Windows.Input;
using System.Windows;
using VcpCore.Common;
using Windows.System;
using DDPM.UI.Common.UserControls;
using Microsoft.VisualBasic.Devices;
using System.Windows.Forms;
using System.Windows.Media;
using Dell.Client.Framework.UX.WPF.Controls;

namespace DDPM.UI.Common.Tests
{
    [Apartment(ApartmentState.STA)]
    public class BlockKeysTests
    {

        [SetUp]
        public void Setup()
        {

        }

        //class BlockKeys
        [Test]
        public void TestisBlocked()
        {
            // Act
            var result = BlockKeys.isBlocked(VirtualKey.Tab);
            // Assert
            Assert.That(result, Is.EqualTo(true));

            // Act
            result = BlockKeys.isBlocked(VirtualKey.None);
            // Assert
            Assert.That(result, Is.EqualTo(false));
        }


        //class KeysHelper
        [Test]
        public void TestContainsKeyIgnoreLeftRight()
        {
            // Act
            var result = KeysHelper.ContainsKeyIgnoreLeftRight(new List<VirtualKey>() { VirtualKey.Tab, VirtualKey.V }, VirtualKey.LeftShift);
            // Assert
            Assert.That(result, Is.EqualTo(true));

            // Act
            result = KeysHelper.ContainsKeyIgnoreLeftRight(new List<VirtualKey>() { VirtualKey.Tab, VirtualKey.V }, VirtualKey.Cancel);
            // Assert
            Assert.That(result, Is.EqualTo(true));
        }

        [Test]
        public void TesthotkeyTypeToStr()
        {
            // Act
            var result = KeysHelper.hotkeyTypeToStr(HotkeyType.None);
            // Assert
            Assert.That(result, Is.EqualTo(""));

            // Act
            result = KeysHelper.hotkeyTypeToStr(HotkeyType.BrightnessReduce);
            // Assert
            Assert.That(result, Is.EqualTo("Brightness-"));

            // Act
            result = KeysHelper.hotkeyTypeToStr(HotkeyType.BrightnessIncrease);
            // Assert
            Assert.That(result, Is.EqualTo("Brightness+"));

            // Act
            result = KeysHelper.hotkeyTypeToStr(HotkeyType.ContrastReduce);
            // Assert
            Assert.That(result, Is.EqualTo("Contrast-"));

            // Act
            result = KeysHelper.hotkeyTypeToStr(HotkeyType.ContrastIncrease);
            // Assert
            Assert.That(result, Is.EqualTo("Contrast+"));

            // Act
            result = KeysHelper.hotkeyTypeToStr(HotkeyType.LuminanceReduce);
            // Assert
            Assert.That(result, Is.EqualTo("Luminance-"));

            // Act
            result = KeysHelper.hotkeyTypeToStr(HotkeyType.LuminanceIncrease);
            // Assert
            Assert.That(result, Is.EqualTo("Luminance+"));

            // Act
            result = KeysHelper.hotkeyTypeToStr(HotkeyType.ToggleInputSource);
            // Assert
            Assert.That(result, Is.EqualTo("ToggleInputSource"));

            // Act
            result = KeysHelper.hotkeyTypeToStr(HotkeyType.FavoriteInputSource);
            // Assert
            Assert.That(result, Is.EqualTo("FavoriteInputSource"));

            // Act
            result = KeysHelper.hotkeyTypeToStr(HotkeyType.SwitchInputSource);
            // Assert
            Assert.That(result, Is.EqualTo("SwitchInputSource"));

            // Act
            result = KeysHelper.hotkeyTypeToStr(HotkeyType.SwapIputPIPPBP);
            // Assert
            Assert.That(result, Is.EqualTo("SwapIputPIPPBP"));

            // Act
            result = KeysHelper.hotkeyTypeToStr(HotkeyType.ChangePIPPosition);
            // Assert
            Assert.That(result, Is.EqualTo("ChangePIPPosition"));
        }

        [Test]
        public void TesthotKeyConflictsCheck()
        {
            // Act
            var deviceManagerSAMock = new Mock<IDeviceManagerSA>();
            DdpmCommonHelper.DeviceManagerSA = deviceManagerSAMock.Object;
            deviceManagerSAMock.Setup(x => x.GetHotkeyConflicts(It.IsAny<HotkeyInfo>())).Returns(Task.FromResult(HotkeyWarning.NotConfigured));
            var result = KeysHelper.hotKeyConflictsCheck(new HotkeyInfo());
            // Assert
            Assert.That(result, Is.EqualTo(false));

            // Act
            deviceManagerSAMock.Setup(x => x.GetHotkeyConflicts(It.IsAny<HotkeyInfo>())).Returns(Task.FromResult(HotkeyWarning.None));
            result = KeysHelper.hotKeyConflictsCheck(new HotkeyInfo());
            // Assert
            Assert.That(result, Is.EqualTo(true));

            // Act
            //deviceManagerSAMock.Setup(x => x.GetHotkeyConflicts(It.IsAny<HotkeyInfo>())).Returns(Task.FromResult(HotkeyWarning.SingleKey));
            //result = KeysHelper.hotKeyConflictsCheck(new HotkeyInfo());
            // Assert
            //Assert.That(result, Is.EqualTo(false));

            //// Act
            //deviceManagerSAMock.Setup(x => x.GetHotkeyConflicts(It.IsAny<HotkeyInfo>())).Returns(Task.FromResult(HotkeyWarning.ConflictInbox));
            //result = KeysHelper.hotKeyConflictsCheck(new HotkeyInfo());
            // Assert
            //Assert.That(result, Is.EqualTo(false));
        }

        [Test]
        public void TestgetUXTextBoxHotkeyInfo()
        {
            // Act
            var keyboard = new TestKeyboardDevice(InputManager.Current);
            var Presentation = new TestPresentationSource();
            var newKeys = new List<VirtualKey>() { VirtualKey.LeftMenu };
            var hotkeyType = new HotkeyType();
            var e = new System.Windows.Input.KeyEventArgs(keyboard, Presentation, 1, Key.K);
            var result = KeysHelper.getUXTextBoxHotkeyInfo(null, e, ref newKeys, hotkeyType);
            //Assert
            Assert.That(result, Is.Not.Null);

            // Act
            e = new System.Windows.Input.KeyEventArgs(keyboard, Presentation, 1, Key.LeftCtrl);
            result = KeysHelper.getUXTextBoxHotkeyInfo(null, e, ref newKeys, hotkeyType);
            // Assert
            Assert.That(result, Is.Not.Null);

            // Act
            newKeys = new List<VirtualKey>() {};
            e = new System.Windows.Input.KeyEventArgs(keyboard, Presentation, 1, Key.LeftShift);
            result = KeysHelper.getUXTextBoxHotkeyInfo(new UXTextBox() { Text= " Ctrl" } , e, ref newKeys, hotkeyType);
            // Assert
            Assert.That(result.Hotkey.Count, Is.EqualTo(1));

            // Act
            result = KeysHelper.getUXTextBoxHotkeyInfo(new UXTextBox() { Text = " 3" }, e, ref newKeys, hotkeyType);
            // Assert
            Assert.That(result.Hotkey.Count, Is.EqualTo(1));

        }

        [Test]
        public void TestonlyContainModifyKeys()
        {
            // Act
            var result = KeysHelper.onlyContainModifyKeys(new List<VirtualKey>() { VirtualKey.Control, VirtualKey.Menu, VirtualKey.Shift });
            // Assert
            Assert.That(result, Is.EqualTo(true));

            // Act
            result = KeysHelper.onlyContainModifyKeys(new List<VirtualKey>() { VirtualKey.Control, VirtualKey.Menu, VirtualKey.Shift, VirtualKey.V });
            // Assert
            Assert.That(result, Is.EqualTo(false));
        }

        //Test setUXTextBoxPreviewKey()
        [Test]
        public void TestsetUXTextBoxPreviewKey()
        {
            // Act
            var keyboard = new TestKeyboardDevice(InputManager.Current);
            var Presentation = new TestPresentationSource();
            var newKeys = new List<VirtualKey>() { VirtualKey.LeftMenu };
            var BundleNewKeys = new List<VirtualKey>();
            var e = new System.Windows.Input.KeyEventArgs(keyboard, Presentation, 1, Key.K);
            var alphabetKey = true;
            try
            {
                KeysHelper.setUXTextBoxPreviewKey(null, e, ref newKeys, ref BundleNewKeys, ref alphabetKey);
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }



        }

        [Test]
        public void TestReSetHotKeyText()
        {
            var str = "strShortCutText";
            var newHotKeys = new List<VirtualKey>() { };
            try
            {
                KeysHelper.ReSetHotKeyText(ref str, ref newHotKeys);
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }

            newHotKeys = new List<VirtualKey>() { VirtualKey.Control, VirtualKey.LeftControl, VirtualKey.Menu, VirtualKey.LeftMenu, VirtualKey.Shift, VirtualKey.LeftShift };
            try
            {
                KeysHelper.ReSetHotKeyText(ref str, ref newHotKeys);
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }

        //class FullWidthCharactersHandler
        [Test]
        public void TestisHalfWidthString()
        {
            // Act
            var result = FullWidthCharactersHandler.isHalfWidthString("０");
            // Assert
            Assert.That(result, Is.EqualTo(false));

            // Act
            result = FullWidthCharactersHandler.isHalfWidthString("fadfdfdgsfgsd");
            // Assert
            Assert.That(result, Is.EqualTo(true));
        }

        [Test]
        public void TestconvertFullWidthToHalfWidth()
        {
            // Act
            var result = FullWidthCharactersHandler.convertFullWidthToHalfWidth("０１２");
            // Assert
            Assert.That(result, Is.EqualTo("012"));
        }


    }

    public class TestKeyboardDevice : KeyboardDevice
    {
        public TestKeyboardDevice(InputManager inputManager) : base(inputManager)
        {
        }

        protected override KeyStates GetKeyStatesFromSystem(Key key)
        {
            throw new NotImplementedException();
        }
    }

    public class TestPresentationSource : PresentationSource
    {
        public override bool IsDisposed => throw new NotImplementedException();

        public override Visual RootVisual { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        protected override CompositionTarget GetCompositionTargetCore()
        {
            throw new NotImplementedException();
        }
    }
}