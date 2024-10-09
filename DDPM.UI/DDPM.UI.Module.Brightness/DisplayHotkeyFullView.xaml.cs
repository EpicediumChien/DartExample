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
        /*        string _strTbBrightnessMinsPreviousKey = string.Empty;
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
                List<VirtualKey> LuminanceAddNewKeys = new List<VirtualKey>();*/

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
            e.Handled = true;
        }

        private void tbLuminanceAdd_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            e.Handled = true;
        }


        private void doLostFocus(HotkeyInfo hotkeyInfo, string prStr, string crStr, ref List<VirtualKey> keys)
        {
            if (KeysHelper.hotKeyConflictsCheck(hotkeyInfo))
            {
                //save hotkey
                bool saveSettings = DdpmCommonHelper.DeviceManagerSA.SaveHotkeySetting(vm.SelectedHomeDevice.MonitorInfo, hotkeyInfo).Result;
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
            if (BundleNewKeys.Count == 0 && newKeys.Count == 0)
            {
                vm.BrightnessMinsKey = _strPreviousKey;
                BundleNewKeys.Clear();
            }
            else
            {
                //for single key
                alphabetKey = false;
                newKeys.Clear();

                HotkeyInfo hotkeyInfo = new HotkeyInfo();
                hotkeyInfo.Job = HotkeyType.BrightnessReduce;
                hotkeyInfo.Hotkey = BundleNewKeys.Distinct().ToList();
                hotkeyInfo.Description = "Brightness-";
                doLostFocus(hotkeyInfo, _strPreviousKey, vm.BrightnessMinsKey, ref BundleNewKeys);
                BundleNewKeys.Clear();

            }
            //hook
            bool isHook = DdpmCommonHelper.DeviceManagerSA.Hook().Result;
        }

        private void tbBrightnessAdd_LostFocus(object sender, RoutedEventArgs e)
        {
            if (BundleNewKeys.Count == 0 && newKeys.Count == 0)
            {
                vm.BrightnessAddKey = _strPreviousKey;
                BundleNewKeys.Clear();
            }
            else
            {
                //for single key
                alphabetKey = false;
                newKeys.Clear();

                //save hotkey
                HotkeyInfo hotkeyInfo = new HotkeyInfo();
                hotkeyInfo.Job = HotkeyType.BrightnessIncrease;
                hotkeyInfo.Hotkey = BundleNewKeys.Distinct().ToList();
                hotkeyInfo.Description = "BrightnessReduce+";
                doLostFocus(hotkeyInfo, _strPreviousKey, vm.BrightnessAddKey, ref BundleNewKeys);
                BundleNewKeys.Clear();
            }
            //hook
            bool isHook = DdpmCommonHelper.DeviceManagerSA.Hook().Result;
        }

        private void tbContrastMins_LostFocus(object sender, RoutedEventArgs e)
        {
            if (BundleNewKeys.Count == 0 && newKeys.Count == 0)
            {
                vm.ContrastMinsKey = _strPreviousKey;
                BundleNewKeys.Clear();
            }
            else
            {
                //for single key
                alphabetKey = false;
                newKeys.Clear();
                //save hotkey
                HotkeyInfo hotkeyInfo = new HotkeyInfo();
                hotkeyInfo.Job = HotkeyType.ContrastReduce;
                hotkeyInfo.Hotkey = BundleNewKeys.Distinct().ToList();
                hotkeyInfo.Description = "Contrast-";
                doLostFocus(hotkeyInfo, _strPreviousKey, vm.ContrastMinsKey, ref BundleNewKeys);
                BundleNewKeys.Clear();
            }
            //hook
            bool isHook = DdpmCommonHelper.DeviceManagerSA.Hook().Result;
        }

        private void tbContrastAdd_LostFocus(object sender, RoutedEventArgs e)
        {
            if (BundleNewKeys.Count == 0 && newKeys.Count == 0)
            {
                vm.ContrastAddKey = _strPreviousKey;
                BundleNewKeys.Clear();
            }
            else
            {
                //for single key
                alphabetKey = false;
                newKeys.Clear();
                //save hotkey
                HotkeyInfo hotkeyInfo = new HotkeyInfo();
                hotkeyInfo.Job = HotkeyType.ContrastIncrease;
                hotkeyInfo.Hotkey = BundleNewKeys.Distinct().ToList();
                hotkeyInfo.Description = "Contrast+";
                doLostFocus(hotkeyInfo, _strPreviousKey, vm.ContrastAddKey, ref BundleNewKeys);
                BundleNewKeys.Clear();
            }
            //hook
            bool isHook = DdpmCommonHelper.DeviceManagerSA.Hook().Result;
        }

        private void tbLuminanceMins_LostFocus(object sender, RoutedEventArgs e)
        {
            if (BundleNewKeys.Count == 0 && newKeys.Count == 0)
            {
                vm.LuminanceMinsKey = _strPreviousKey;
            }
            else
            {
                //for single key
                alphabetKey = false;
                newKeys.Clear();
                //save hotkey
                HotkeyInfo hotkeyInfo = new HotkeyInfo();
                hotkeyInfo.Job = HotkeyType.LuminanceReduce;
                hotkeyInfo.Hotkey = BundleNewKeys.Distinct().ToList();
                hotkeyInfo.Description = "Luminance-";
                doLostFocus(hotkeyInfo, _strPreviousKey, vm.LuminanceMinsKey, ref BundleNewKeys);
                BundleNewKeys.Clear();
            }
            //hook
            bool isHook = DdpmCommonHelper.DeviceManagerSA.Hook().Result;
        }

        private void tbLuminanceAdd_LostFocus(object sender, RoutedEventArgs e)
        {
            if (BundleNewKeys.Count == 0 && newKeys.Count == 0)
            {
                vm.LuminanceAddKey = _strPreviousKey;
            }
            else
            {
                //for single key
                alphabetKey = false;
                newKeys.Clear();
                //save hotkey
                HotkeyInfo hotkeyInfo = new HotkeyInfo();
                hotkeyInfo.Job = HotkeyType.LuminanceIncrease;
                hotkeyInfo.Hotkey = BundleNewKeys.Distinct().ToList();
                hotkeyInfo.Description = "Luminance+";
                doLostFocus(hotkeyInfo, _strPreviousKey, vm.LuminanceAddKey, ref BundleNewKeys);
                BundleNewKeys.Clear();
            }
            //hook
            bool isHook = DdpmCommonHelper.DeviceManagerSA.Hook().Result;
        }
    }
}