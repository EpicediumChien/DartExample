using DDPM.SA.Common.Display;
using DDPM.UI.Common;
using Dell.Client.Framework.UX.WPF.Controls;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Windows.System;

namespace DDPM.UI.Module.Kvm
{
    /// <summary>
    /// Interaction logic for KVMHotkeyFullView.xaml
    /// </summary>
    public partial class KVMHotkeyFullView : UserControl
    {
        private string _strTbSwitchPCsKeyPreviousKey = string.Empty;
        private string _strTbSwitchKbMsKeyPreviousKey = string.Empty;
        private string _strTbChangePipKeyPreviousKey = string.Empty;

        private List<VirtualKey> SwitchPCsKeyNewKeys = new List<VirtualKey>();
        private List<VirtualKey> SwitchKbMsKeyNewKeys = new List<VirtualKey>();
        private List<VirtualKey> ChangePipKeyNewKeys = new List<VirtualKey>();

        private bool updateKeys = false;
        private List<VirtualKey> newKeys = new List<VirtualKey>();

        public KVMHotkeyFullView()
        {
            InitializeComponent();
        }

        private KvmViewModel vm
        {
            get
            {
                return (KvmViewModel)DataContext;
            }
        }

        private void tbSwitchPCsKey_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            setUXTextBoxPreviewKey(sender, e);
        }

        private void tbSwitchPCsKey_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            e.Handled = true;
        }

        private void tbSwitchPCsKey_GotFocus(object sender, RoutedEventArgs e)
        {
            bool isUnhook = DdpmCommonHelper.DeviceManagerSA.UnHook().Result;
            if (isUnhook)
            {
                updateKeys = false;
                newKeys.Clear();
                _strTbSwitchPCsKeyPreviousKey = vm.SwitchPCsKey;
                vm.SwitchPCsKey = string.Empty;
            }
        }

        private void tbSwitchPCsKey_LostFocus(object sender, RoutedEventArgs e)
        {
            if (SwitchPCsKeyNewKeys.Count == 0)
            {
                vm.SwitchPCsKey = _strTbSwitchPCsKeyPreviousKey;
            }
            else
            {
                //for single key
                updateKeys = false;
                newKeys.Clear();

                HotkeyInfo hotkeyInfo = new HotkeyInfo();
                hotkeyInfo.Job = HotkeyType.KvmSwitchInputSource;
                hotkeyInfo.Hotkey = SwitchPCsKeyNewKeys.Distinct().ToList();
                hotkeyInfo.Description = "kvm switch between pcs";
                //add inputsource
                string inputSource1 = vm.PC1Inputs_Selected.inputSource;
                if (!string.IsNullOrEmpty(inputSource1))
                {
                    hotkeyInfo.InputSource.Add(new InputSourceObj(inputSource1));
                }
                string inputSource2 = vm.PC2Inputs_Selected.inputSource;
                if (!string.IsNullOrEmpty(inputSource2))
                {
                    hotkeyInfo.InputSource.Add(new InputSourceObj(inputSource2));
                }
                string inputSource3 = vm.PC3Inputs_Selected.inputSource;
                if (!string.IsNullOrEmpty(inputSource3))
                {
                    hotkeyInfo.InputSource.Add(new InputSourceObj(inputSource3));
                }
                string inputSource4 = vm.PC4Inputs_Selected.inputSource;
                if (!string.IsNullOrEmpty(inputSource4))
                {
                    hotkeyInfo.InputSource.Add(new InputSourceObj(inputSource4));
                }
                doLostFocus(hotkeyInfo, _strTbSwitchPCsKeyPreviousKey, vm.SwitchPCsKey, ref SwitchPCsKeyNewKeys);
                SwitchPCsKeyNewKeys.Clear();
            }
            //hook
            bool isHook = DdpmCommonHelper.DeviceManagerSA.Hook().Result;
        }

        private void tbSwitchPCsKey_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            e.Handled = true;
        }

        private void tbSwitchKbMsKey_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            setUXTextBoxPreviewKey(sender, e);
        }

        private void tbSwitchKbMsKey_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            e.Handled = true;
        }

        private void tbSwitchKbMsKey_GotFocus(object sender, RoutedEventArgs e)
        {
            bool isUnhook = DdpmCommonHelper.DeviceManagerSA.UnHook().Result;
            if (isUnhook)
            {
                updateKeys = false;
                newKeys.Clear();
                _strTbSwitchKbMsKeyPreviousKey = vm.SwitchKbMsKey;
                vm.SwitchKbMsKey = string.Empty;
            }
        }

        private void tbSwitchKbMsKey_LostFocus(object sender, RoutedEventArgs e)
        {
            if (SwitchKbMsKeyNewKeys.Count == 0)
            {
                vm.SwitchKbMsKey = _strTbSwitchKbMsKeyPreviousKey;
            }
            else
            {
                //for single key
                updateKeys = false;
                newKeys.Clear();

                HotkeyInfo hotkeyInfo = new HotkeyInfo();
                hotkeyInfo.Job = HotkeyType.KvmSwitchKbMsKey;
                hotkeyInfo.Hotkey = SwitchKbMsKeyNewKeys.Distinct().ToList();
                hotkeyInfo.Description = "kvm switch KB MS";
                doLostFocus(hotkeyInfo, _strTbSwitchKbMsKeyPreviousKey, vm.SwitchKbMsKey, ref SwitchKbMsKeyNewKeys);
                SwitchKbMsKeyNewKeys.Clear();
            }
            //hook
            bool isHook = DdpmCommonHelper.DeviceManagerSA.Hook().Result;
        }

        private void tbSwitchKbMsKey_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            e.Handled = true;
        }

        private void tbChangePipKey_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            setUXTextBoxPreviewKey(sender, e);
        }

        private void tbChangePipKey_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            e.Handled = true;
        }

        private void tbChangePipKey_GotFocus(object sender, RoutedEventArgs e)
        {
            bool isUnhook = DdpmCommonHelper.DeviceManagerSA.UnHook().Result;
            if (isUnhook)
            {
                updateKeys = false;
                newKeys.Clear();
                _strTbChangePipKeyPreviousKey = vm.ChangePipKey;
                vm.ChangePipKey = string.Empty;
            }
        }

        private void tbChangePipKey_LostFocus(object sender, RoutedEventArgs e)
        {
            if (ChangePipKeyNewKeys.Count == 0)
            {
                vm.ChangePipKey = _strTbChangePipKeyPreviousKey;
            }
            else
            {
                //for single key
                updateKeys = false;
                newKeys.Clear();

                HotkeyInfo hotkeyInfo = new HotkeyInfo();
                hotkeyInfo.Job = HotkeyType.KvmChangePIPPosition;
                hotkeyInfo.Hotkey = ChangePipKeyNewKeys.Distinct().ToList();
                hotkeyInfo.Description = "kvm Change PIP position";
                doLostFocus(hotkeyInfo, _strTbChangePipKeyPreviousKey, vm.ChangePipKey, ref ChangePipKeyNewKeys);
                ChangePipKeyNewKeys.Clear();
            }
            //hook
            bool isHook = DdpmCommonHelper.DeviceManagerSA.Hook().Result;
        }

        private void tbChangePipKey_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            e.Handled = true;
        }

        private void LeftArrow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DdpmCommonHelper.ModuleOwner?.CloseFullView();
        }

        private void UserControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            tbCleanFocus.Focus();
            Keyboard.ClearFocus();
        }

        private void InputSourceHotkey_Click(object sender, RoutedEventArgs e)
        {
            //todo
        }

        private void setUXTextBoxPreviewKey(object sender, KeyEventArgs e)
        {
            e.Handled = true;
            newKeys = newKeys.Distinct().ToList();
            if (newKeys.Count >= 4) return;
            var texBox = (sender as UXTextBox);
            var texBoxName = texBox?.Name;
            if (string.IsNullOrEmpty(texBoxName)) return;
            Debug.WriteLine($"{texBoxName}_PreviewKeyDown---Key---{e.Key}");
            Debug.WriteLine($"{texBoxName}_PreviewKeyDown---SystemKey---{e.SystemKey}");
            VirtualKey thisVirtualKey;
            VirtualKey thisVirtualKey_system = (VirtualKey)KeyInterop.VirtualKeyFromKey(e.SystemKey);
            if (thisVirtualKey_system != VirtualKey.None)
            {
                thisVirtualKey = thisVirtualKey_system;
            }
            else
            {
                thisVirtualKey = (VirtualKey)KeyInterop.VirtualKeyFromKey(e.Key);
            }
            //if the key will be processed by an Input Method Editor (IME), then return ?
            //Object v;
            // Enum.TryParse(typeof(VirtualKey), e.Key.ToString(), out v);
            bool r = Enum.IsDefined(typeof(VirtualKey), thisVirtualKey);
            if (!r) return;
            if (BlockKeys.isBlocked(thisVirtualKey)) return;
            if (newKeys.Count > 0 && !newKeys.Any(x => (x == VirtualKey.Control) || (x == VirtualKey.Shift) || (x == VirtualKey.Menu)))
            {
                //second single key
                return;
            }
            if (newKeys.Count == 2 && newKeys.Any(x => (x == VirtualKey.Menu)) && !newKeys.Any(x => (x == VirtualKey.Control) || (x == VirtualKey.Shift)))
            {
                //second Alt+ (key)
                return;
            }
            if (thisVirtualKey == VirtualKey.LeftControl || thisVirtualKey == VirtualKey.RightControl)
            {
                thisVirtualKey = VirtualKey.Control;
            }
            else if (thisVirtualKey == VirtualKey.LeftShift || thisVirtualKey == VirtualKey.RightShift)
            {
                thisVirtualKey = VirtualKey.Shift;
            }
            else if (thisVirtualKey == VirtualKey.LeftMenu || thisVirtualKey == VirtualKey.RightMenu)
            {
                thisVirtualKey = VirtualKey.Menu;
            }
            if (!updateKeys)
            {
                newKeys.Clear();
                updateKeys = true;
                newKeys.Add(thisVirtualKey);
            }

            Debug.WriteLine($"{texBoxName}_PreviewKeyDown-NewKeys-----{string.Join(",", newKeys)}");
            if (!newKeys.Contains(thisVirtualKey))
            {
                newKeys.Add(thisVirtualKey);
                /*if ((thisVirtualKey == VirtualKey.Menu) ||
                    (thisVirtualKey == VirtualKey.Control) ||
                    (thisVirtualKey == VirtualKey.Shift) ||
                    (thisVirtualKey >= VirtualKey.Number0 && thisVirtualKey <= VirtualKey.F24))
                {
                    newKeys.Add(thisVirtualKey);
                }

                if ((thisVirtualKey >= VirtualKey.Number0 && thisVirtualKey <= VirtualKey.Number9) ||
                    (thisVirtualKey >= VirtualKey.A && thisVirtualKey <= VirtualKey.Z) ||
                    (thisVirtualKey >= VirtualKey.F1 && thisVirtualKey <= VirtualKey.F24) ||
                    (thisVirtualKey >= VirtualKey.NumberPad0 && thisVirtualKey <= VirtualKey.Divide))
                {
                    newKeys.Add(thisVirtualKey);
                }*/
            }
            string swHortcutText = string.Empty;
            KeysHelper.ReSetHotKeyText(ref swHortcutText, ref newKeys);
            texBox.Text = swHortcutText;
            texBox.Select(swHortcutText.Length, 1);
            switch (texBoxName)
            {
                case "tbSwitchPCsKey":
                    SwitchPCsKeyNewKeys.AddRange(newKeys);
                    break;

                case "tbSwitchKbMsKey":
                    SwitchKbMsKeyNewKeys.AddRange(newKeys);
                    break;

                case "tbChangePipKey":
                    ChangePipKeyNewKeys.AddRange(newKeys);
                    break;
            }
        }

        private void doLostFocus(HotkeyInfo hotkeyInfo, string prStr, string crStr, ref List<VirtualKey> keys)
        {
            if (KeysHelper.hotKeyConflictsCheck(hotkeyInfo))
            {
                //save hotkey
                // SaveHotkeysSetting(_strTbBrightnessMinsPreviousKey, vm.BrightnessMinsKey, HotkeyType.BrightnessReduce, ref BrightnessMinsNewKeys, "Brightness-");
                bool saveSettings = DdpmCommonHelper.DeviceManagerSA.SaveHotkeySetting(vm.KvmModule.SelectedHomeDevice.MonitorInfo.edid, hotkeyInfo).Result;
                vm.Invoke_RefreshHotkeySettings();
            }
            else
            {
                switch (hotkeyInfo.Job)
                {
                    case HotkeyType.KvmSwitchInputSource:
                        vm.SwitchPCsKey = prStr;
                        break;

                    case HotkeyType.KvmChangePIPPosition:
                        vm.ChangePipKey = prStr;
                        break;

                    case HotkeyType.KvmSwitchKbMsKey:
                        vm.SwitchKbMsKey = prStr;
                        break;
                }
            }
            keys.Clear();
        }
    }
}