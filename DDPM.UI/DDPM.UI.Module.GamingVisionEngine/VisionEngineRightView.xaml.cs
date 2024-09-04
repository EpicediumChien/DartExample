using DDPM.SA.Common.Display;
using DDPM.UI.Common;
using Dell.Client.Framework.UX.WPF.Controls;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Windows.System;

namespace DDPM.UI.Module.GamingVisionEngine
{
    /// <summary>
    /// Interaction logic for VisionEngineRightView.xaml
    /// </summary>
    public partial class VisionEngineRightView : UserControl
    {
        string _strTbVisionEngineTogglePreviousKey = string.Empty;
        List<VirtualKey> VisionEngineToggleNewKeys = new List<VirtualKey>();
        bool updateKeys = false;
        List<VirtualKey> newKeys = new List<VirtualKey>();
        UI_VisionEngine tempUI_VE = null;
        public VisionEngineRightView()
        {
            InitializeComponent();

        }

        private void RefreshUI()
        {
            VisionEngineViewModel vm = (VisionEngineViewModel)DataContext;
            vm.RefreshUI();
        }
        private VisionEngineViewModel vm
        {
            get
            {
                return (VisionEngineViewModel)DataContext;
            }
        }
        private void UXCheckBox_Click(object sender, RoutedEventArgs e)
        {
            VisionEngineViewModel vm = (VisionEngineViewModel)DataContext;
            List<UI_VisionEngine> tempList = vm.VisionEngineList.FindAll(o => o.VisionEngine_Enable);
            vm.VisionEngineIsEnable = false;
            vm.RefreshUI();
            vm.SetVisionEngine();
        }
        private void UXCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as UXCheckBox;
            if (checkBox != null)
            {
                var selectedItem = checkBox.DataContext as UI_VisionEngine;
                if (selectedItem != null)
                {
                    var vm = (VisionEngineViewModel)DataContext;
                    List<UI_VisionEngine> tempList = vm.VisionEngineList.FindAll(o => o.VisionEngine_Enable);
                    if (tempList.Count <= 0)
                    {
                        checkBox.IsChecked = true;
                    }
                }
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
            }
            string swHortcutText = string.Empty;
            KeysHelper.ReSetHotKeyText(ref swHortcutText, ref newKeys);
            texBox.Text = swHortcutText;
            texBox.Select(swHortcutText.Length, 1);
            switch (texBoxName)
            {
                case "tbVisionEngineToggle":
                    VisionEngineToggleNewKeys.AddRange(newKeys);
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
                    case HotkeyType.VisionEngineToggle:
                        vm.VisionEngineToggleKey = prStr;
                        break;
                }
            }
            keys.Clear();
        }
        private void tbVisionEngineToggle_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            setUXTextBoxPreviewKey(sender, e);
        }

        private void tbVisionEngineToggle_PreviewKeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            e.Handled = true;
        }

        private void tbVisionEngineToggle_GotFocus(object sender, RoutedEventArgs e)
        {
            bool isUnhook = DdpmCommonHelper.DeviceManagerSA.UnHook().Result;
            if (isUnhook)
            {
                updateKeys = false;
                newKeys.Clear();
                _strTbVisionEngineTogglePreviousKey = vm.VisionEngineToggleKey;
                vm.VisionEngineToggleKey = string.Empty;
            }
        }

        private void tbVisionEngineToggle_LostFocus(object sender, RoutedEventArgs e)
        {
            if (VisionEngineToggleNewKeys.Count == 0)
            {
                vm.VisionEngineToggleKey = _strTbVisionEngineTogglePreviousKey;
            }
            else
            {
                //for single key
                updateKeys = false;
                newKeys.Clear();

                //save hotkey
                //SaveHotkeysSetting(_strTbBrightnessAddPreviousKey, vm.BrightnessAddKey, HotkeyType.BrightnessIncrease, ref BrightnessAddNewKeys, "Brightness+");
                HotkeyInfo hotkeyInfo = new HotkeyInfo();
                hotkeyInfo.Job = HotkeyType.VisionEngineToggle;
                hotkeyInfo.Hotkey = VisionEngineToggleNewKeys.Distinct().ToList();
                hotkeyInfo.Description = "VisionEngineToggle";
                doLostFocus(hotkeyInfo, _strTbVisionEngineTogglePreviousKey, vm.VisionEngineToggleKey, ref VisionEngineToggleNewKeys);
                VisionEngineToggleNewKeys.Clear();
            }
            //hook
            bool isHook = DdpmCommonHelper.DeviceManagerSA.Hook().Result;
        }

        private void tbVisionEngineToggle_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            e.Handled = true;
        }
    }
}