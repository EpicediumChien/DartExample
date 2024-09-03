using DDPM.SA.Common.Display;
using DDPM.UI.Common;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF.Controls;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Windows.System;

namespace DDPM.UI.Module.Brightness
{
    /// <summary>
    /// Interaction logic for DisplayHotkeyFullView.xaml
    /// </summary>
    public partial class DisplayHotkeyFullView : UserControl
    {
        private ILog? _log;
        string _strTbBrightnessMinsPreviousKey = string.Empty;
        string _strTbBrightnessAddPreviousKey = string.Empty;
        string _strTbContrastMinsPreviousKey = string.Empty;
        string _strTbContrastAddPreviousKey = string.Empty;
        string _strTbLuminanceMinsPreviousKey = string.Empty;
        string _strTbLuminanceAddPreviousKey = string.Empty;
        List<VirtualKey> BrightnessMinsNewKeys = new List<VirtualKey>();
        List<VirtualKey> BrightnessAddNewKeys = new List<VirtualKey>();
        List<VirtualKey> ContrastMinsNewKeys = new List<VirtualKey>();
        List<VirtualKey> ContrastAddNewKeys = new List<VirtualKey>();
        List<VirtualKey> LuminanceMinsNewKeys = new List<VirtualKey>();
        List<VirtualKey> LuminanceAddNewKeys = new List<VirtualKey>();

        bool updateKeys = false;
        List<VirtualKey> newKeys = new List<VirtualKey>();

        private BrightnessViewModel vm
        {
            get
            {
                return (BrightnessViewModel)DataContext;
            }
        }
        public DisplayHotkeyFullView()
        {
            InitializeComponent();
            if (DdpmCommonHelper.MyConsole != null)
            {
                _log = DdpmCommonHelper.MyConsole.CreateLog("DisplayHotkeyFullView");
                _log.Info("DisplayHotkeyFullView");
            }
        }

        private void leftArrow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DdpmCommonHelper.ModuleOwner?.CloseFullView();
        }

        private void tbBrightnessMins_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            setUXTextBoxPreviewKey(sender, e);
        }

        private void tbBrightnessMins_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            /*string swHortcutText = string.Empty;
            KeysHelper.ReSetHotKeyText(ref swHortcutText, ref BrightnessMinsNewKeys);
            vm.BrightnessMinsKey = swHortcutText;*/
            e.Handled = true;
        }

        private void tbBrightnessMins_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            e.Handled = true;
        }

        private void UserControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //UserControl uc=sender as UserControl;
            //UXTextBox tb = (UXTextBox)uc.FindName("tbCleanFocus");
            tbCleanFocus.Focus();
            Keyboard.ClearFocus();
        }

        private void tbBrightnessAdd_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            setUXTextBoxPreviewKey(sender, e);
        }

        private void tbBrightnessAdd_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            /*string swHortcutText = string.Empty;
            KeysHelper.ReSetHotKeyText(ref swHortcutText, ref BrightnessAddNewKeys);
            vm.BrightnessAddKey = swHortcutText;*/
            e.Handled = true;
        }

        private void tbBrightnessAdd_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            e.Handled = true;
        }

        private void tbBrightnessMins_GotFocus(object sender, RoutedEventArgs e)
        {
            bool isUnhook = DdpmCommonHelper.DeviceManagerSA.UnHook().Result;
            if (isUnhook)
            {
                updateKeys = false;
                newKeys.Clear();
                _strTbBrightnessMinsPreviousKey = vm.BrightnessMinsKey;
                vm.BrightnessMinsKey = string.Empty;
            }
        }

        private void tbBrightnessAdd_GotFocus(object sender, RoutedEventArgs e)
        {
            bool isUnhook = DdpmCommonHelper.DeviceManagerSA.UnHook().Result;
            if (isUnhook)
            {
                updateKeys = false;
                newKeys.Clear();
                _strTbBrightnessAddPreviousKey = vm.BrightnessAddKey;
                vm.BrightnessAddKey = string.Empty;
            }
        }
        private void tbContrastMins_GotFocus(object sender, RoutedEventArgs e)
        {
            bool isUnhook = DdpmCommonHelper.DeviceManagerSA.UnHook().Result;
            if (isUnhook)
            {
                updateKeys = false;
                newKeys.Clear();
                _strTbContrastMinsPreviousKey = vm.ContrastMinsKey;
                vm.ContrastMinsKey = string.Empty;
            }
        }

        private void tbContrastAdd_GotFocus(object sender, RoutedEventArgs e)
        {
            bool isUnhook = DdpmCommonHelper.DeviceManagerSA.UnHook().Result;
            if (isUnhook)
            {
                updateKeys = false;
                newKeys.Clear();
                _strTbContrastAddPreviousKey = vm.ContrastAddKey;
                vm.ContrastAddKey = string.Empty;
            }
        }

        private void tbLuminanceMins_GotFocus(object sender, RoutedEventArgs e)
        {
            bool isUnhook = DdpmCommonHelper.DeviceManagerSA.UnHook().Result;
            if (isUnhook)
            {
                updateKeys = false;
                newKeys.Clear();
                _strTbLuminanceMinsPreviousKey = vm.LuminanceMinsKey;
                vm.LuminanceMinsKey = string.Empty;
            }
        }

        private void tbLuminanceAdd_GotFocus(object sender, RoutedEventArgs e)
        {
            bool isUnhook = DdpmCommonHelper.DeviceManagerSA.UnHook().Result;
            if (isUnhook)
            {
                updateKeys = false;
                newKeys.Clear();
                _strTbLuminanceAddPreviousKey = vm.LuminanceAddKey;
                vm.LuminanceAddKey = string.Empty;
            }
        }

        private void tbContrastMins_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            setUXTextBoxPreviewKey(sender, e);
        }

        private void tbContrastMins_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            /*string swHortcutText = string.Empty;
            KeysHelper.ReSetHotKeyText(ref swHortcutText, ref ContrastMinsNewKeys);
            vm.ContrastMinsKey = swHortcutText;*/
            e.Handled = true;
        }

        private void tbContrastMins_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            e.Handled = true;
        }

        private void tbContrastAdd_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            setUXTextBoxPreviewKey(sender, e);
        }

        private void tbContrastAdd_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            /*string swHortcutText = string.Empty;
            KeysHelper.ReSetHotKeyText(ref swHortcutText, ref ContrastAddNewKeys);
            vm.ContrastAddKey = swHortcutText;*/
            e.Handled = true;
        }

        private void tbContrastAdd_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            e.Handled = true;
        }

        private void tbLuminanceMins_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            setUXTextBoxPreviewKey(sender, e);
        }

        private void tbLuminanceMins_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            /* string swHortcutText = string.Empty;
             KeysHelper.ReSetHotKeyText(ref swHortcutText, ref LuminanceMinsNewKeys);
             vm.LuminanceMinsKey = swHortcutText;*/
            e.Handled = true;
        }

        private void tbLuminanceMins_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            e.Handled = true;
        }

        private void tbLuminanceAdd_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            setUXTextBoxPreviewKey(sender, e);
        }

        private void tbLuminanceAdd_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            /* string swHortcutText = string.Empty;
             KeysHelper.ReSetHotKeyText(ref swHortcutText, ref LuminanceAddNewKeys);
             vm.LuminanceAddKey = swHortcutText;*/
            e.Handled = true;
        }

        private void tbLuminanceAdd_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            e.Handled = true;
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
                case "tbBrightnessMins":
                    BrightnessMinsNewKeys.AddRange(newKeys);
                    BrightnessMinsNewKeys = BrightnessMinsNewKeys.Distinct().ToList();
                    break;

                case "tbBrightnessAdd":
                    BrightnessAddNewKeys.AddRange(newKeys);
                    break;

                case "tbContrastMins":
                    ContrastMinsNewKeys.AddRange(newKeys);
                    break;

                case "tbContrastAdd":
                    ContrastAddNewKeys.AddRange(newKeys);
                    break;

                case "tbLuminanceMins":
                    LuminanceMinsNewKeys.AddRange(newKeys);
                    break;

                case "tbLuminanceAdd":
                    LuminanceAddNewKeys.AddRange(newKeys);
                    break;
            }
        }

        private void doLostFocus(HotkeyInfo hotkeyInfo, string prStr, string crStr, ref List<VirtualKey> keys)
        {
            if (KeysHelper.hotKeyConflictsCheck(hotkeyInfo))
            {
                //save hotkey
                // SaveHotkeysSetting(_strTbBrightnessMinsPreviousKey, vm.BrightnessMinsKey, HotkeyType.BrightnessReduce, ref BrightnessMinsNewKeys, "Brightness-");
                bool saveSettings = DdpmCommonHelper.DeviceManagerSA.SaveHotkeySetting(vm.SelectedHomeDevice.MonitorInfo.edid, hotkeyInfo).Result;
                vm.Invoke_RefreshHotkeySettings();
            }
            else
            {
                switch (hotkeyInfo.Job)
                {
                    case HotkeyType.BrightnessReduce:
                        vm.BrightnessMinsKey = prStr;
                        break;

                    case HotkeyType.BrightnessIncrease:
                        vm.BrightnessAddKey = prStr;
                        break;

                    case HotkeyType.ContrastReduce:
                        vm.ContrastMinsKey = prStr;
                        break;

                    case HotkeyType.ContrastIncrease:
                        vm.ContrastAddKey = prStr;
                        break;

                    case HotkeyType.LuminanceReduce:
                        vm.LuminanceMinsKey = prStr;
                        break;

                    case HotkeyType.LuminanceIncrease:
                        vm.LuminanceAddKey = prStr;
                        break;
                }
            }
            keys.Clear();
        }
        private void tbBrightnessMins_LostFocus(object sender, RoutedEventArgs e)
        {
            if (BrightnessMinsNewKeys.Count == 0)
            {
                vm.BrightnessMinsKey = _strTbBrightnessMinsPreviousKey;
            }
            else
            {
                //for single key
                updateKeys = false;
                newKeys.Clear();

                HotkeyInfo hotkeyInfo = new HotkeyInfo();
                hotkeyInfo.Job = HotkeyType.BrightnessReduce;
                hotkeyInfo.Hotkey = BrightnessMinsNewKeys.Distinct().ToList();
                hotkeyInfo.Description = "Brightness-";
                doLostFocus(hotkeyInfo, _strTbBrightnessMinsPreviousKey, vm.BrightnessMinsKey, ref BrightnessMinsNewKeys);
                BrightnessMinsNewKeys.Clear();
                /*if (hotKeyConflictsCheck(hotkeyInfo))
                {
                    //save hotkey
                   // SaveHotkeysSetting(_strTbBrightnessMinsPreviousKey, vm.BrightnessMinsKey, HotkeyType.BrightnessReduce, ref BrightnessMinsNewKeys, "Brightness-");
                 bool saveSettings = DdpmCommonHelper.DeviceManagerSA.SaveHotkeySetting(vm.SelectedHomeDevice.MonitorInfo, _strTbBrightnessMinsPreviousKey, vm.BrightnessMinsKey, HotkeyType.BrightnessReduce, BrightnessMinsNewKeys, "Brightness-").Result;
                 vm.Invoke_RefreshHotkeySettings();
                }
                else
                {
                    vm.BrightnessMinsKey = _strTbBrightnessMinsPreviousKey;
                }
                 BrightnessMinsNewKeys.Clear();
                //hook
                bool isHook = DdpmCommonHelper.DeviceManagerSA.Hook().Result;*/
            }
            //hook
            bool isHook = DdpmCommonHelper.DeviceManagerSA.Hook().Result;
        }

        private void tbBrightnessAdd_LostFocus(object sender, RoutedEventArgs e)
        {
            if (BrightnessAddNewKeys.Count == 0)
            {
                vm.BrightnessAddKey = _strTbBrightnessAddPreviousKey;
            }
            else
            {
                //for single key
                updateKeys = false;
                newKeys.Clear();

                //save hotkey
                //SaveHotkeysSetting(_strTbBrightnessAddPreviousKey, vm.BrightnessAddKey, HotkeyType.BrightnessIncrease, ref BrightnessAddNewKeys, "Brightness+");
                HotkeyInfo hotkeyInfo = new HotkeyInfo();
                hotkeyInfo.Job = HotkeyType.BrightnessIncrease;
                hotkeyInfo.Hotkey = BrightnessAddNewKeys.Distinct().ToList();
                hotkeyInfo.Description = "BrightnessReduce+";
                doLostFocus(hotkeyInfo, _strTbBrightnessAddPreviousKey, vm.BrightnessAddKey, ref BrightnessAddNewKeys);
                BrightnessAddNewKeys.Clear();
            }
            //hook
            bool isHook = DdpmCommonHelper.DeviceManagerSA.Hook().Result;
        }

        private void tbContrastMins_LostFocus(object sender, RoutedEventArgs e)
        {
            if (ContrastMinsNewKeys.Count == 0)
            {
                vm.ContrastMinsKey = _strTbContrastMinsPreviousKey;
            }
            else
            {
                //for single key
                updateKeys = false;
                newKeys.Clear();
                //save hotkey
                //SaveHotkeysSetting(_strTbContrastMinsPreviousKey, vm.ContrastMinsKey, HotkeyType.ContrastReduce, ref ContrastMinsNewKeys, "ContrastMins-");
                HotkeyInfo hotkeyInfo = new HotkeyInfo();
                hotkeyInfo.Job = HotkeyType.ContrastReduce;
                hotkeyInfo.Hotkey = ContrastMinsNewKeys.Distinct().ToList();
                hotkeyInfo.Description = "Contrast-";
                doLostFocus(hotkeyInfo, _strTbContrastMinsPreviousKey, vm.ContrastMinsKey, ref ContrastMinsNewKeys);
                ContrastMinsNewKeys.Clear();
            }
            //hook
            bool isHook = DdpmCommonHelper.DeviceManagerSA.Hook().Result;
        }

        private void tbContrastAdd_LostFocus(object sender, RoutedEventArgs e)
        {
            if (ContrastAddNewKeys.Count == 0)
            {
                vm.ContrastAddKey = _strTbContrastAddPreviousKey;
            }
            else
            {
                //for single key
                updateKeys = false;
                newKeys.Clear();
                //save hotkey
                //SaveHotkeysSetting(_strTbContrastAddPreviousKey, vm.ContrastAddKey, HotkeyType.ContrastIncrease, ref ContrastAddNewKeys, "ContrastMins+");
                HotkeyInfo hotkeyInfo = new HotkeyInfo();
                hotkeyInfo.Job = HotkeyType.ContrastIncrease;
                hotkeyInfo.Hotkey = ContrastAddNewKeys.Distinct().ToList();
                hotkeyInfo.Description = "Contrast+";
                doLostFocus(hotkeyInfo, _strTbContrastAddPreviousKey, vm.ContrastAddKey, ref ContrastAddNewKeys);
                ContrastAddNewKeys.Clear();
            }
            //hook
            bool isHook = DdpmCommonHelper.DeviceManagerSA.Hook().Result;
        }

        private void tbLuminanceMins_LostFocus(object sender, RoutedEventArgs e)
        {
            if (LuminanceMinsNewKeys.Count == 0)
            {
                vm.LuminanceMinsKey = _strTbLuminanceMinsPreviousKey;
            }
            else
            {
                //for single key
                updateKeys = false;
                newKeys.Clear();
                //save hotkey
                //SaveHotkeysSetting(_strTbLuminanceMinsPreviousKey, vm.LuminanceMinsKey, HotkeyType.LuminanceReduce, ref LuminanceMinsNewKeys, "Luminance-");
                HotkeyInfo hotkeyInfo = new HotkeyInfo();
                hotkeyInfo.Job = HotkeyType.LuminanceReduce;
                hotkeyInfo.Hotkey = LuminanceMinsNewKeys.Distinct().ToList();
                hotkeyInfo.Description = "Luminance-";
                doLostFocus(hotkeyInfo, _strTbLuminanceMinsPreviousKey, vm.LuminanceMinsKey, ref LuminanceMinsNewKeys);
                LuminanceMinsNewKeys.Clear();
            }
            //hook
            bool isHook = DdpmCommonHelper.DeviceManagerSA.Hook().Result;
        }

        private void tbLuminanceAdd_LostFocus(object sender, RoutedEventArgs e)
        {
            if (LuminanceAddNewKeys.Count == 0)
            {
                vm.LuminanceAddKey = _strTbLuminanceAddPreviousKey;
            }
            else
            {
                //for single key
                updateKeys = false;
                newKeys.Clear();
                //save hotkey
                //SaveHotkeysSetting(_strTbLuminanceAddPreviousKey, vm.LuminanceAddKey, HotkeyType.LuminanceIncrease, ref LuminanceAddNewKeys, "Luminance+");
                HotkeyInfo hotkeyInfo = new HotkeyInfo();
                hotkeyInfo.Job = HotkeyType.LuminanceIncrease;
                hotkeyInfo.Hotkey = LuminanceAddNewKeys.Distinct().ToList();
                hotkeyInfo.Description = "Luminance+";
                doLostFocus(hotkeyInfo, _strTbLuminanceAddPreviousKey, vm.LuminanceAddKey, ref LuminanceAddNewKeys);
                LuminanceAddNewKeys.Clear();
            }
            //hook
            bool isHook = DdpmCommonHelper.DeviceManagerSA.Hook().Result;
        }
    }
}