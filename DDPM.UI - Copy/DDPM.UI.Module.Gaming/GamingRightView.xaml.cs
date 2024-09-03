using DDPM.SA.Common.Display;
using DDPM.UI.Common;
using Dell.Client.Framework.UX.WPF.Controls;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Windows.System;

namespace DDPM.UI.Module.Gaming
{
    /// <summary>
    /// Interaction logic for GamingRightView.xaml
    /// </summary>
    public partial class GamingRightView : UserControl
    {
        string _strTbDarkStabilizerTogglePreviousKey = string.Empty;
        string _strTbDualResolutionTogglePreviousKey = string.Empty;
        List<VirtualKey> DarkStabilizerToggleNewKeys = new List<VirtualKey>();
        List<VirtualKey> DualResolutionToggleNewKeys = new List<VirtualKey>();
        bool updateKeys = false;
        List<VirtualKey> newKeys = new List<VirtualKey>();
        public GamingRightView()
        {
            InitializeComponent();
        }

        private void CallWindowsSettings_Click(object sender, RoutedEventArgs e)
        {
            DdpmCommonHelper.DeviceManagerSA.CallWindowsDisplaySetting();
        }

        private void RefreshUI()
        {
            GamingViewModel vm = (GamingViewModel)DataContext;
            vm.RefreshUI();
        }

        private GamingViewModel vm
        {
            get
            {
                return (GamingViewModel)DataContext;
            }
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
                case "tbDarkStabilizerToggle":
                    DarkStabilizerToggleNewKeys.AddRange(newKeys);
                    break;

                case "tbDualResolutionToggle":
                    DualResolutionToggleNewKeys.AddRange(newKeys);
                    break;
            }
        }
        private void doLostFocus(HotkeyInfo hotkeyInfo, string prStr, string crStr, ref List<VirtualKey> keys)
        {
            if (KeysHelper.hotKeyConflictsCheck(hotkeyInfo))
            {
                //save hotkey
                // SaveHotkeysSetting(_strTbBrightnessMinsPreviousKey, vm.BrightnessMinsKey, HotkeyType.BrightnessReduce, ref BrightnessMinsNewKeys, "Brightness-");
                bool saveSettings = DdpmCommonHelper.DeviceManagerSA.SaveHotkeySetting(vm.MyModule.SelectedHomeDevice.MonitorInfo.edid, hotkeyInfo).Result;
                vm.Invoke_RefreshHotkeySettings();
            }
            else
            {
                switch (hotkeyInfo.Job)
                {
                    case HotkeyType.DarkStabilizerToggle:
                        vm.DarkStabilizerToggleKey = prStr;
                        break;

                    case HotkeyType.DualResolutionToggle:
                        vm.DualResolutionToggleKey = prStr;
                        break;
                }
            }
            keys.Clear();
        }
        private void tbDarkStabilizerToggle_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            setUXTextBoxPreviewKey(sender, e);
        }

        private void tbDarkStabilizerToggle_PreviewKeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            e.Handled = true;
        }

        private void tbDarkStabilizerToggle_GotFocus(object sender, RoutedEventArgs e)
        {
            bool isUnhook = DdpmCommonHelper.DeviceManagerSA.UnHook().Result;
            if (isUnhook)
            {
                updateKeys = false;
                newKeys.Clear();
                _strTbDarkStabilizerTogglePreviousKey = vm.DarkStabilizerToggleKey;
                vm.DarkStabilizerToggleKey = string.Empty;
            }
        }

        private void tbDarkStabilizerToggle_LostFocus(object sender, RoutedEventArgs e)
        {
            if (DarkStabilizerToggleNewKeys.Count == 0)
            {
                vm.DarkStabilizerToggleKey = _strTbDarkStabilizerTogglePreviousKey;
            }
            else
            {
                //for single key
                updateKeys = false;
                newKeys.Clear();

                //save hotkey
                //SaveHotkeysSetting(_strTbBrightnessAddPreviousKey, vm.BrightnessAddKey, HotkeyType.BrightnessIncrease, ref BrightnessAddNewKeys, "Brightness+");
                HotkeyInfo hotkeyInfo = new HotkeyInfo();
                hotkeyInfo.Job = HotkeyType.DarkStabilizerToggle;
                hotkeyInfo.Hotkey = DarkStabilizerToggleNewKeys.Distinct().ToList();
                hotkeyInfo.Description = "DarkStabilizerToggle";
                doLostFocus(hotkeyInfo, _strTbDarkStabilizerTogglePreviousKey, vm.DarkStabilizerToggleKey, ref DarkStabilizerToggleNewKeys);
                DarkStabilizerToggleNewKeys.Clear();
            }
            //hook
            bool isHook = DdpmCommonHelper.DeviceManagerSA.Hook().Result;
        }

        private void tbDarkStabilizerToggle_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            e.Handled = true;
        }

        private void tbDualResolutionToggle_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            setUXTextBoxPreviewKey(sender, e);
        }

        private void tbDualResolutionToggle_PreviewKeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            e.Handled = true;
        }

        private void tbDualResolutionToggle_GotFocus(object sender, RoutedEventArgs e)
        {
            bool isUnhook = DdpmCommonHelper.DeviceManagerSA.UnHook().Result;
            if (isUnhook)
            {
                updateKeys = false;
                newKeys.Clear();
                _strTbDualResolutionTogglePreviousKey = vm.DualResolutionToggleKey;
                vm.DualResolutionToggleKey = string.Empty;
            }
        }

        private void tbDualResolutionToggle_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            e.Handled = true;
        }

        private void tbDualResolutionToggle_LostFocus(object sender, RoutedEventArgs e)
        {
            if (DualResolutionToggleNewKeys.Count == 0)
            {
                vm.DualResolutionToggleKey = _strTbDualResolutionTogglePreviousKey;
            }
            else
            {
                //for single key
                updateKeys = false;
                newKeys.Clear();

                //save hotkey
                //SaveHotkeysSetting(_strTbBrightnessAddPreviousKey, vm.BrightnessAddKey, HotkeyType.BrightnessIncrease, ref BrightnessAddNewKeys, "Brightness+");
                HotkeyInfo hotkeyInfo = new HotkeyInfo();
                hotkeyInfo.Job = HotkeyType.DualResolutionToggle;
                hotkeyInfo.Hotkey = DualResolutionToggleNewKeys.Distinct().ToList();
                hotkeyInfo.Description = "DualResolutionToggle";
                doLostFocus(hotkeyInfo, _strTbDualResolutionTogglePreviousKey, vm.DualResolutionToggleKey, ref DualResolutionToggleNewKeys);
                DualResolutionToggleNewKeys.Clear();
            }
            //hook
            bool isHook = DdpmCommonHelper.DeviceManagerSA.Hook().Result;
        }
    }
}