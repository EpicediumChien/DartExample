using DDPM.SA.Common.Display;
using DDPM.UI.Common;
using Dell.Client.Framework.UX.WPF.Controls;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using Windows.System;

namespace DDPM.UI.Module.DisplayHotkeys
{
    /// <summary>
    /// Interaction logic for DisplayHotkeysRightView.xaml
    /// </summary>
    public partial class DisplayHotkeysRightView : UserControl
    {
        /*private string _strTbToggleInputSourcePreviousKey = string.Empty;
        private string _strTbFavoriteInputSourcePreviousKey = string.Empty;
        private string _strTbSwitchInputSourcePreviousKey = string.Empty;
        private string _strTbSwapPIPPBPInputSourcePreviousKey = string.Empty;
        private string _strTbChangePIPPositionPreviousKey = string.Empty;

        private List<VirtualKey> ToggleInputSourceNewKeys = new List<VirtualKey>();
        private List<VirtualKey> FavoriteInputSourceNewKeys = new List<VirtualKey>();
        private List<VirtualKey> SwitchInputSourceNewKeys = new List<VirtualKey>();
        private List<VirtualKey> SwapPIPPBPInputSourceNewKeys = new List<VirtualKey>();
        private List<VirtualKey> ChangePIPPositionNewKeys = new List<VirtualKey>();*/

        private bool alphabetKey = false;
        private string _strPreviousKey = string.Empty;
        private List<VirtualKey> newKeys = new List<VirtualKey>();
        private List<VirtualKey> BundleNewKeys = new List<VirtualKey>();

        private DisplayHotkeysViewModel vm
        {
            get => (DisplayHotkeysViewModel)DataContext;
        }

        public DisplayHotkeysRightView(DisplayHotkeysViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
            vm.PxPkeySettings_Visibility = vm.DisplayHotkeysModule.SelectedHomeDevice.MonitorInfo.CapabilityDic.ContainsKey("E9") ? Visibility.Visible : Visibility.Collapsed;
        }

        private void tbToggleInputSource_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            KeysHelper.setUXTextBoxPreviewKey(sender, e, ref newKeys, ref BundleNewKeys, ref alphabetKey);
        }
        bool newAgain = false;
        private void tbToggleInputSource_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            /*string swHortcutText = string.Empty;
            KeysHelper.ReSetHotKeyText(ref swHortcutText, ref ToggleInputSourceNewKeys);
            vm.ToggleInputSourceKey = swHortcutText;*/
            //e.Handled = true;
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
            Debug.WriteLine($"tbToggleInputSource_PreviewKeyUp:{thisVirtualKey}");
            if (newKeys.Contains(thisVirtualKey))
            {

                newKeys.Remove(thisVirtualKey);
                Debug.WriteLine($"{newKeys.Count}");
            }

        }

        private void tbToggleInputSource_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            e.Handled = true;
        }

        private void tbFavoriteInputSource_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            KeysHelper.setUXTextBoxPreviewKey(sender, e, ref newKeys, ref BundleNewKeys, ref alphabetKey);
        }

        private void tbFavoriteInputSource_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            /*string swHortcutText = string.Empty;
            KeysHelper.ReSetHotKeyText(ref swHortcutText, ref FavoriteInputSourceNewKeys);
            vm.FavoriteInputSourceKey = swHortcutText;*/
            e.Handled = true;
        }

        private void tbFavoriteInputSource_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            e.Handled = true;
        }

        private void tbSwitchInputSource_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            KeysHelper.setUXTextBoxPreviewKey(sender, e, ref newKeys, ref BundleNewKeys, ref alphabetKey);
        }

        private void tbSwitchInputSource_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            /* string swHortcutText = string.Empty;
             KeysHelper.ReSetHotKeyText(ref swHortcutText, ref SwitchInputSourceNewKeys);
             vm.SwitchInputSourceKey = swHortcutText;*/
            e.Handled = true;
        }

        private void tbSwitchInputSource_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            e.Handled = true;
        }

        private void tbSwapPIPPBPInputSource_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            KeysHelper.setUXTextBoxPreviewKey(sender, e, ref newKeys, ref BundleNewKeys, ref alphabetKey);
        }

        private void tbSwapPIPPBPInputSource_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            /*string swHortcutText = string.Empty;
            KeysHelper.ReSetHotKeyText(ref swHortcutText, ref SwapPIPPBPInputSourceNewKeys);
            vm.SwapPIPPBPInputSourceKey = swHortcutText;*/
            e.Handled = true;
        }

        private void tbSwapPIPPBPInputSource_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            e.Handled = true;
        }

        private void tbChangePIPPosition_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            KeysHelper.setUXTextBoxPreviewKey(sender, e, ref newKeys, ref BundleNewKeys, ref alphabetKey);
        }

        private void tbChangePIPPosition_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            /*string swHortcutText = string.Empty;
            KeysHelper.ReSetHotKeyText(ref swHortcutText, ref ChangePIPPositionNewKeys);
            vm.ChangePIPPositionKey = swHortcutText;*/
            e.Handled = true;
        }

        private void tbChangePIPPosition_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            e.Handled = true;
        }

        private void tbToggleInputSource_GotFocus(object sender, RoutedEventArgs e)
        {
            bool isUnhook = DdpmCommonHelper.DeviceManagerSA.UnHook().Result;
            if (isUnhook)
            {
                alphabetKey = false;
                newKeys.Clear();
                //_strTbToggleInputSourcePreviousKey = vm.ToggleInputSourceKey;
                _strPreviousKey = vm.ToggleInputSourceKey;
                //vm.ToggleInputSourceKey = string.Empty;
                var texBox = (sender as UXTextBox);
                texBox?.Select(vm.ToggleInputSourceKey.Length, 1);
            }
        }

        private void tbFavoriteInputSource_GotFocus(object sender, RoutedEventArgs e)
        {
            bool isUnhook = DdpmCommonHelper.DeviceManagerSA.UnHook().Result;
            if (isUnhook)
            {
                alphabetKey = false;
                newKeys.Clear();
                //_strTbFavoriteInputSourcePreviousKey = vm.FavoriteInputSourceKey;
                _strPreviousKey = vm.FavoriteInputSourceKey;
                //vm.FavoriteInputSourceKey = string.Empty;
                var texBox = (sender as UXTextBox);
                texBox?.Select(vm.FavoriteInputSourceKey.Length, 1);
            }
        }

        private void tbSwitchInputSource_GotFocus(object sender, RoutedEventArgs e)
        {
            bool isUnhook = DdpmCommonHelper.DeviceManagerSA.UnHook().Result;
            if (isUnhook)
            {
                alphabetKey = false;
                newKeys.Clear();
                _strPreviousKey = vm.SwitchInputSourceKey;
                //vm.SwitchInputSourceKey = string.Empty;
                var texBox = (sender as UXTextBox);
                texBox?.Select(vm.FavoriteInputSourceKey.Length, 1);
            }
        }

        private void tbSwapPIPPBPInputSource_GotFocus(object sender, RoutedEventArgs e)
        {
            bool isUnhook = DdpmCommonHelper.DeviceManagerSA.UnHook().Result;
            if (isUnhook)
            {
                alphabetKey = false;
                newKeys.Clear();
                _strPreviousKey = vm.SwapPIPPBPInputSourceKey;
                //vm.SwapPIPPBPInputSourceKey = string.Empty;
                var texBox = (sender as UXTextBox);
                texBox?.Select(vm.FavoriteInputSourceKey.Length, 1);
            }
        }

        private void tbChangePIPPosition_GotFocus(object sender, RoutedEventArgs e)
        {
            bool isUnhook = DdpmCommonHelper.DeviceManagerSA.UnHook().Result;
            if (isUnhook)
            {
                alphabetKey = false;
                newKeys.Clear();
                _strPreviousKey = vm.ChangePIPPositionKey;
                //vm.ChangePIPPositionKey = string.Empty;
                var texBox = (sender as UXTextBox);
                texBox?.Select(vm.FavoriteInputSourceKey.Length, 1);
            }
        }

        private void doLostFocus(HotkeyInfo hotkeyInfo, string prStr, string crStr, ref List<VirtualKey> keys)
        {
            if (KeysHelper.hotKeyConflictsCheck(hotkeyInfo))
            {
                //save hotkey
                // SaveHotkeysSetting(_strTbBrightnessMinsPreviousKey, vm.BrightnessMinsKey, HotkeyType.BrightnessReduce, ref BrightnessMinsNewKeys, "Brightness-");
                bool saveSettings = DdpmCommonHelper.DeviceManagerSA.SaveHotkeySetting(vm.DisplayHotkeysModule.SelectedHomeDevice.MonitorInfo, hotkeyInfo).Result;
                vm.Invoke_RefreshData();
            }
            else
            {
                switch (hotkeyInfo.Job)
                {
                    case HotkeyType.ToggleInputSource:
                        vm.ToggleInputSourceKey = prStr;
                        break;

                    case HotkeyType.FavoriteInputSource:
                        vm.FavoriteInputSourceKey = prStr;
                        break;

                    case HotkeyType.SwitchInputSource:
                        vm.SwitchInputSourceKey = prStr;
                        break;

                    case HotkeyType.SwapIputPIPPBP:
                        vm.SwapPIPPBPInputSourceKey = prStr;
                        break;

                    case HotkeyType.ChangePIPPosition:
                        vm.ChangePIPPositionKey = prStr;
                        break;
                }
            }
            keys.Clear();
        }



        private void tbToggleInputSource_LostFocus(object sender, RoutedEventArgs e)
        {
            if (BundleNewKeys.Count == 0 && newKeys.Count == 0)
            {
                //vm.ToggleInputSourceKey = _strTbToggleInputSourcePreviousKey;
                vm.ToggleInputSourceKey = _strPreviousKey;
                BundleNewKeys.Clear();
            }
            else
            {
                //for single key
                alphabetKey = false;
                newKeys.Clear();

                //save hotkey
                HotkeyInfo hotkeyInfo = new HotkeyInfo();
                hotkeyInfo.Job = HotkeyType.ToggleInputSource;
                hotkeyInfo.Hotkey = BundleNewKeys.Distinct().ToList();
                hotkeyInfo.Description = "ToggleInputSource";
                doLostFocus(hotkeyInfo, _strPreviousKey, vm.ToggleInputSourceKey, ref BundleNewKeys);
                BundleNewKeys.Clear();
            }
            //hook
            bool isHook = DdpmCommonHelper.DeviceManagerSA.Hook().Result;
        }

        private void tbFavoriteInputSource_LostFocus(object sender, RoutedEventArgs e)
        {
            if (BundleNewKeys.Count == 0 && newKeys.Count == 0)
            {
                vm.FavoriteInputSourceKey = _strPreviousKey;
                BundleNewKeys.Clear();
            }
            else
            {
                //for single key
                alphabetKey = false;
                newKeys.Clear();
                //save hotkey
                HotkeyInfo hotkeyInfo = new HotkeyInfo();
                hotkeyInfo.Job = HotkeyType.FavoriteInputSource;
                hotkeyInfo.Hotkey = BundleNewKeys.Distinct().ToList();

                if (vm.FavoriteInput_Selected == null)
                {
                    if(vm.InputsList != null && vm.InputsList.Count > 0)
                    {
                        vm.FavoriteInput_Selected = vm.InputsList[0];
                    }
                }
                if (vm.FavoriteInput_Selected != null)
                {
                    hotkeyInfo.InputSource.Add(new InputSourceObj(vm.FavoriteInput_Selected.inputDisplayText));
                    hotkeyInfo.Description = "FavoriteInputSource";
                    doLostFocus(hotkeyInfo, _strPreviousKey, vm.FavoriteInputSourceKey, ref BundleNewKeys);
                    BundleNewKeys.Clear();
                }
            }
            //hook
            bool isHook = DdpmCommonHelper.DeviceManagerSA.Hook().Result;
        }

        private void tbSwitchInputSource_LostFocus(object sender, RoutedEventArgs e)
        {
            if (BundleNewKeys.Count == 0 && newKeys.Count == 0)
            {
                vm.SwitchInputSourceKey = _strPreviousKey;
                BundleNewKeys.Clear();
            }
            else
            {
                //for single key
                alphabetKey = false;
                newKeys.Clear();
                //save hotkey
                HotkeyInfo hotkeyInfo = new HotkeyInfo();
                hotkeyInfo.Job = HotkeyType.SwitchInputSource;
                hotkeyInfo.Hotkey = BundleNewKeys.Distinct().ToList();
                hotkeyInfo.InputSource.Add(new InputSourceObj(vm.SwitchInput1_Selected.inputDisplayText));
                hotkeyInfo.InputSource.Add(new InputSourceObj(vm.SwitchInput2_Selected.inputDisplayText));
                hotkeyInfo.Description = "SwitchInputSource";
                doLostFocus(hotkeyInfo, _strPreviousKey, vm.SwitchInputSourceKey, ref BundleNewKeys);
                BundleNewKeys.Clear();
            }
            //hook
            bool isHook = DdpmCommonHelper.DeviceManagerSA.Hook().Result;
        }

        private void tbSwapPIPPBPInputSource_LostFocus(object sender, RoutedEventArgs e)
        {
            if (BundleNewKeys.Count == 0 && newKeys.Count == 0)
            {
                vm.SwapPIPPBPInputSourceKey = _strPreviousKey;
                BundleNewKeys.Clear();
            }
            else
            {
                //for single key
                alphabetKey = false;
                newKeys.Clear();
                //save hotkey
                HotkeyInfo hotkeyInfo = new HotkeyInfo();
                hotkeyInfo.Job = HotkeyType.SwapIputPIPPBP;
                hotkeyInfo.Hotkey = BundleNewKeys.Distinct().ToList();
                hotkeyInfo.Description = "SwapPIPPBPInputSource";
                doLostFocus(hotkeyInfo, _strPreviousKey, vm.SwapPIPPBPInputSourceKey, ref BundleNewKeys);
                BundleNewKeys.Clear();
            }
            //hook
            bool isHook = DdpmCommonHelper.DeviceManagerSA.Hook().Result;
        }

        private void tbChangePIPPosition_LostFocus(object sender, RoutedEventArgs e)
        {
            if (BundleNewKeys.Count == 0 && newKeys.Count == 0)
            {
                vm.ChangePIPPositionKey = _strPreviousKey;
                BundleNewKeys.Clear();
            }
            else
            {
                //for single key
                alphabetKey = false;
                newKeys.Clear();
                //save hotkey
                HotkeyInfo hotkeyInfo = new HotkeyInfo();
                hotkeyInfo.Job = HotkeyType.ChangePIPPosition;
                hotkeyInfo.Hotkey = BundleNewKeys.Distinct().ToList();
                hotkeyInfo.Description = "ChangePIPPosition";
                doLostFocus(hotkeyInfo, _strPreviousKey, vm.ChangePIPPositionKey, ref BundleNewKeys);
                BundleNewKeys.Clear();
            }
            //hook
            bool isHook = DdpmCommonHelper.DeviceManagerSA.Hook().Result;
        }

    }
}