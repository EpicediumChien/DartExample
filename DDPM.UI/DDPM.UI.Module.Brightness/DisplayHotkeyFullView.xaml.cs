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
        private bool alphabetKey = false;
        private string _strPreviousKey = string.Empty;
        private List<VirtualKey> newKeys = new List<VirtualKey>();
        private List<VirtualKey> BundleNewKeys = new List<VirtualKey>();

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
            KeysHelper.setUXTextBoxPreviewKey(sender, e, ref newKeys, ref BundleNewKeys, ref alphabetKey);
        }

        private void tbBrightnessMins_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            /*string swHortcutText = string.Empty;
            KeysHelper.ReSetHotKeyText(ref swHortcutText, ref BrightnessMinsNewKeys);
            vm.BrightnessMinsKey = swHortcutText;*/
            HotkeyInfo hotkeyInfo = KeysHelper.getUXTextBoxHotkeyInfo(sender, e, ref newKeys, HotkeyType.BrightnessReduce);
            if (hotkeyInfo.Hotkey != null && hotkeyInfo.Hotkey.Count > 0)
            {
                if (KeysHelper.onlyContainModifyKeys(hotkeyInfo.Hotkey) || BundleNewKeys.Count == 0 && newKeys.Count == 0)
                {
                    vm.BrightnessMinsKey = _strPreviousKey;
                    BundleNewKeys.Clear();
                    var texBox = (sender as UXTextBox);
                    if (texBox == null) return;
                    texBox.Text = vm.BrightnessMinsKey;
                    texBox.Select(vm.BrightnessMinsKey.Length, 1);
                }
                else
                {
                    //for single key
                    alphabetKey = false;
                    newKeys.Clear();

                    if (KeysHelper.hotKeyConflictsCheck(hotkeyInfo))
                    {
                        vm.IsBusy = true;
                        bool saveSettings = DdpmCommonHelper.DeviceManagerSA.SaveHotkeySetting(vm.SelectedHomeDevice.MonitorInfo, hotkeyInfo).Result;
                        vm.Invoke_RefreshHotkeySettings();
                        if (saveSettings)
                        {
                            DdpmCommonHelper.isHotkeyBypass = DdpmCommonHelper.DeviceManagerSA.ByPassHotkey(false).Result;
                            vm.IsBusy = false;
                        }
                    }
                    else
                    {
                        vm.BrightnessMinsKey = _strPreviousKey;
                    }

                    BundleNewKeys.Clear();

                }
            }
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
            KeysHelper.setUXTextBoxPreviewKey(sender, e, ref newKeys, ref BundleNewKeys, ref alphabetKey);
        }

        private void tbBrightnessAdd_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            /*string swHortcutText = string.Empty;
            KeysHelper.ReSetHotKeyText(ref swHortcutText, ref BrightnessAddNewKeys);
            vm.BrightnessAddKey = swHortcutText;*/
            HotkeyInfo hotkeyInfo = KeysHelper.getUXTextBoxHotkeyInfo(sender, e, ref newKeys, HotkeyType.BrightnessIncrease);
            if (hotkeyInfo.Hotkey != null && hotkeyInfo.Hotkey.Count > 0)
            {
                if (KeysHelper.onlyContainModifyKeys(hotkeyInfo.Hotkey) || BundleNewKeys.Count == 0 && newKeys.Count == 0)
                {
                    vm.BrightnessAddKey = _strPreviousKey;
                    BundleNewKeys.Clear();
                    var texBox = (sender as UXTextBox);
                    if (texBox == null) return;
                    texBox.Text = vm.BrightnessAddKey;
                    texBox.Select(vm.BrightnessAddKey.Length, 1);
                }
                else
                {
                    //for single key
                    alphabetKey = false;
                    newKeys.Clear();

                    if (KeysHelper.hotKeyConflictsCheck(hotkeyInfo))
                    {
                        vm.IsBusy = true;
                        bool saveSettings = DdpmCommonHelper.DeviceManagerSA.SaveHotkeySetting(vm.SelectedHomeDevice.MonitorInfo, hotkeyInfo).Result;
                        vm.Invoke_RefreshHotkeySettings();
                        if (saveSettings)
                        {
                            DdpmCommonHelper.isHotkeyBypass = DdpmCommonHelper.DeviceManagerSA.ByPassHotkey(false).Result;
                            vm.IsBusy = false;
                        }
                    }
                    else
                    {
                        vm.BrightnessAddKey = _strPreviousKey;
                    }

                    BundleNewKeys.Clear();

                }
            }
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
                alphabetKey = false;
                newKeys.Clear();
                _strPreviousKey = vm.BrightnessMinsKey;
                var texBox = (sender as UXTextBox);
                texBox?.Select(vm.BrightnessMinsKey.Length, 1);
                //vm.BrightnessMinsKey = string.Empty;
            }
        }

        private void tbBrightnessAdd_GotFocus(object sender, RoutedEventArgs e)
        {
            bool isUnhook = DdpmCommonHelper.DeviceManagerSA.UnHook().Result;
            if (isUnhook)
            {
                alphabetKey = false;
                newKeys.Clear();
                _strPreviousKey = vm.BrightnessAddKey;
                //vm.BrightnessAddKey = string.Empty;
                var texBox = (sender as UXTextBox);
                texBox?.Select(vm.BrightnessAddKey.Length, 1);
            }
        }
        private void tbContrastMins_GotFocus(object sender, RoutedEventArgs e)
        {
            bool isUnhook = DdpmCommonHelper.DeviceManagerSA.UnHook().Result;
            if (isUnhook)
            {
                alphabetKey = false;
                newKeys.Clear();
                _strPreviousKey = vm.ContrastMinsKey;
                //vm.ContrastMinsKey = string.Empty;
                var texBox = (sender as UXTextBox);
                texBox?.Select(vm.ContrastMinsKey.Length, 1);
            }
        }

        private void tbContrastAdd_GotFocus(object sender, RoutedEventArgs e)
        {
            bool isUnhook = DdpmCommonHelper.DeviceManagerSA.UnHook().Result;
            if (isUnhook)
            {
                alphabetKey = false;
                newKeys.Clear();
                _strPreviousKey = vm.ContrastAddKey;
                // vm.ContrastAddKey = string.Empty;
                var texBox = (sender as UXTextBox);
                texBox?.Select(vm.ContrastAddKey.Length, 1);
            }
        }

        private void tbLuminanceMins_GotFocus(object sender, RoutedEventArgs e)
        {
            bool isUnhook = DdpmCommonHelper.DeviceManagerSA.UnHook().Result;
            if (isUnhook)
            {
                alphabetKey = false;
                newKeys.Clear();
                _strPreviousKey = vm.LuminanceMinsKey;
                //vm.LuminanceMinsKey = string.Empty;
                var texBox = (sender as UXTextBox);
                texBox?.Select(vm.LuminanceMinsKey.Length, 1);
            }
        }

        private void tbLuminanceAdd_GotFocus(object sender, RoutedEventArgs e)
        {
            bool isUnhook = DdpmCommonHelper.DeviceManagerSA.UnHook().Result;
            if (isUnhook)
            {
                alphabetKey = false;
                newKeys.Clear();
                _strPreviousKey = vm.LuminanceAddKey;
                //vm.LuminanceAddKey = string.Empty;
                var texBox = (sender as UXTextBox);
                texBox?.Select(vm.LuminanceAddKey.Length, 1);
            }
        }

        private void tbContrastMins_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            KeysHelper.setUXTextBoxPreviewKey(sender, e, ref newKeys, ref BundleNewKeys, ref alphabetKey);
        }

        private void tbContrastMins_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            /*string swHortcutText = string.Empty;
            KeysHelper.ReSetHotKeyText(ref swHortcutText, ref ContrastMinsNewKeys);
            vm.ContrastMinsKey = swHortcutText;*/
            HotkeyInfo hotkeyInfo = KeysHelper.getUXTextBoxHotkeyInfo(sender, e, ref newKeys, HotkeyType.ContrastReduce);
            if (hotkeyInfo.Hotkey != null && hotkeyInfo.Hotkey.Count > 0)
            {
                if (KeysHelper.onlyContainModifyKeys(hotkeyInfo.Hotkey) || BundleNewKeys.Count == 0 && newKeys.Count == 0)
                {
                    vm.ContrastMinsKey = _strPreviousKey;
                    BundleNewKeys.Clear();
                    var texBox = (sender as UXTextBox);
                    if (texBox == null) return;
                    texBox.Text = vm.ContrastMinsKey;
                    texBox.Select(vm.ContrastMinsKey.Length, 1);
                }
                else
                {
                    //for single key
                    alphabetKey = false;
                    newKeys.Clear();

                    if (KeysHelper.hotKeyConflictsCheck(hotkeyInfo))
                    {
                        vm.IsBusy = true;
                        bool saveSettings = DdpmCommonHelper.DeviceManagerSA.SaveHotkeySetting(vm.SelectedHomeDevice.MonitorInfo, hotkeyInfo).Result;
                        vm.Invoke_RefreshHotkeySettings();
                        if (saveSettings)
                        {
                            DdpmCommonHelper.isHotkeyBypass = DdpmCommonHelper.DeviceManagerSA.ByPassHotkey(false).Result;
                            vm.IsBusy = false;
                        }
                    }
                    else
                    {
                        vm.ContrastMinsKey = _strPreviousKey;
                    }

                    BundleNewKeys.Clear();

                }
            }
            e.Handled = true;
        }

        private void tbContrastMins_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            e.Handled = true;
        }

        private void tbContrastAdd_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            KeysHelper.setUXTextBoxPreviewKey(sender, e, ref newKeys, ref BundleNewKeys, ref alphabetKey);
        }

        private void tbContrastAdd_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            /*string swHortcutText = string.Empty;
            KeysHelper.ReSetHotKeyText(ref swHortcutText, ref ContrastAddNewKeys);
            vm.ContrastAddKey = swHortcutText;*/
            HotkeyInfo hotkeyInfo = KeysHelper.getUXTextBoxHotkeyInfo(sender, e, ref newKeys, HotkeyType.ContrastIncrease);
            if (hotkeyInfo.Hotkey != null && hotkeyInfo.Hotkey.Count > 0)
            {
                if (KeysHelper.onlyContainModifyKeys(hotkeyInfo.Hotkey) || BundleNewKeys.Count == 0 && newKeys.Count == 0)
                {
                    vm.ContrastAddKey = _strPreviousKey;
                    BundleNewKeys.Clear();
                    var texBox = (sender as UXTextBox);
                    if (texBox == null) return;
                    texBox.Text = vm.ContrastAddKey;
                    texBox.Select(vm.ContrastAddKey.Length, 1);
                }
                else
                {
                    //for single key
                    alphabetKey = false;
                    newKeys.Clear();

                    if (KeysHelper.hotKeyConflictsCheck(hotkeyInfo))
                    {
                        vm.IsBusy = true;
                        bool saveSettings = DdpmCommonHelper.DeviceManagerSA.SaveHotkeySetting(vm.SelectedHomeDevice.MonitorInfo, hotkeyInfo).Result;
                        vm.Invoke_RefreshHotkeySettings();
                        if (saveSettings)
                        {
                            DdpmCommonHelper.isHotkeyBypass = DdpmCommonHelper.DeviceManagerSA.ByPassHotkey(false).Result;
                            vm.IsBusy = false;
                        }
                    }
                    else
                    {
                        vm.ContrastAddKey = _strPreviousKey;
                    }

                    BundleNewKeys.Clear();

                }
            }
            e.Handled = true;
        }

        private void tbContrastAdd_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            e.Handled = true;
        }

        private void tbLuminanceMins_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            KeysHelper.setUXTextBoxPreviewKey(sender, e, ref newKeys, ref BundleNewKeys, ref alphabetKey);
        }

        private void tbLuminanceMins_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            /* string swHortcutText = string.Empty;
             KeysHelper.ReSetHotKeyText(ref swHortcutText, ref LuminanceMinsNewKeys);
             vm.LuminanceMinsKey = swHortcutText;*/
            HotkeyInfo hotkeyInfo = KeysHelper.getUXTextBoxHotkeyInfo(sender, e, ref newKeys, HotkeyType.LuminanceReduce);
            if (hotkeyInfo.Hotkey != null && hotkeyInfo.Hotkey.Count > 0)
            {
                if (KeysHelper.onlyContainModifyKeys(hotkeyInfo.Hotkey) || BundleNewKeys.Count == 0 && newKeys.Count == 0)
                {
                    vm.LuminanceMinsKey = _strPreviousKey;
                    BundleNewKeys.Clear();
                    var texBox = (sender as UXTextBox);
                    if (texBox == null) return;
                    texBox.Text = vm.LuminanceMinsKey;
                    texBox.Select(vm.LuminanceMinsKey.Length, 1);
                }
                else
                {
                    //for single key
                    alphabetKey = false;
                    newKeys.Clear();

                    if (KeysHelper.hotKeyConflictsCheck(hotkeyInfo))
                    {
                        vm.IsBusy = true;
                        bool saveSettings = DdpmCommonHelper.DeviceManagerSA.SaveHotkeySetting(vm.SelectedHomeDevice.MonitorInfo, hotkeyInfo).Result;
                        vm.Invoke_RefreshHotkeySettings();
                        if (saveSettings)
                        {
                            DdpmCommonHelper.isHotkeyBypass = DdpmCommonHelper.DeviceManagerSA.ByPassHotkey(false).Result;
                            vm.IsBusy = false;
                        }
                    }
                    else
                    {
                        vm.LuminanceMinsKey = _strPreviousKey;
                    }

                    BundleNewKeys.Clear();

                }
            }
            e.Handled = true;
        }

        private void tbLuminanceMins_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            e.Handled = true;
        }

        private void tbLuminanceAdd_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            KeysHelper.setUXTextBoxPreviewKey(sender, e, ref newKeys, ref BundleNewKeys, ref alphabetKey);
        }

        private void tbLuminanceAdd_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            /* string swHortcutText = string.Empty;
             KeysHelper.ReSetHotKeyText(ref swHortcutText, ref LuminanceAddNewKeys);
             vm.LuminanceAddKey = swHortcutText;*/
            HotkeyInfo hotkeyInfo = KeysHelper.getUXTextBoxHotkeyInfo(sender, e, ref newKeys, HotkeyType.LuminanceIncrease);
            if (hotkeyInfo.Hotkey != null && hotkeyInfo.Hotkey.Count > 0)
            {
                if (KeysHelper.onlyContainModifyKeys(hotkeyInfo.Hotkey) || BundleNewKeys.Count == 0 && newKeys.Count == 0)
                {
                    vm.LuminanceAddKey = _strPreviousKey;
                    BundleNewKeys.Clear();
                    var texBox = (sender as UXTextBox);
                    if (texBox == null) return;
                    texBox.Text = vm.LuminanceAddKey;
                    texBox.Select(vm.LuminanceAddKey.Length, 1);
                }
                else
                {
                    //for single key
                    alphabetKey = false;
                    newKeys.Clear();

                    if (KeysHelper.hotKeyConflictsCheck(hotkeyInfo))
                    {
                        vm.IsBusy = true;
                        bool saveSettings = DdpmCommonHelper.DeviceManagerSA.SaveHotkeySetting(vm.SelectedHomeDevice.MonitorInfo, hotkeyInfo).Result;
                        vm.Invoke_RefreshHotkeySettings();
                        if (saveSettings)
                        {
                            DdpmCommonHelper.isHotkeyBypass = DdpmCommonHelper.DeviceManagerSA.ByPassHotkey(false).Result;
                            vm.IsBusy = false;
                        }
                    }
                    else
                    {
                        vm.LuminanceAddKey = _strPreviousKey;
                    }

                    BundleNewKeys.Clear();

                }
            }
            e.Handled = true;
        }

        private void tbLuminanceAdd_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            e.Handled = true;
        }


        private void tbBrightnessMins_LostFocus(object sender, RoutedEventArgs e)
        {
            //hook
            bool isHook = DdpmCommonHelper.DeviceManagerSA.Hook().Result;
        }

        private void tbBrightnessAdd_LostFocus(object sender, RoutedEventArgs e)
        {
            //hook
            bool isHook = DdpmCommonHelper.DeviceManagerSA.Hook().Result;
        }

        private void tbContrastMins_LostFocus(object sender, RoutedEventArgs e)
        {
            //hook
            bool isHook = DdpmCommonHelper.DeviceManagerSA.Hook().Result;
        }

        private void tbContrastAdd_LostFocus(object sender, RoutedEventArgs e)
        {
            //hook
            bool isHook = DdpmCommonHelper.DeviceManagerSA.Hook().Result;
        }

        private void tbLuminanceMins_LostFocus(object sender, RoutedEventArgs e)
        {
            //hook
            bool isHook = DdpmCommonHelper.DeviceManagerSA.Hook().Result;
        }

        private void tbLuminanceAdd_LostFocus(object sender, RoutedEventArgs e)
        {
            //hook
            bool isHook = DdpmCommonHelper.DeviceManagerSA.Hook().Result;
        }
    }
}