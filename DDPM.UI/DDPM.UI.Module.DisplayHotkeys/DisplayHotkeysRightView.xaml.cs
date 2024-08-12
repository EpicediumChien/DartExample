using DDPM.SA.Common;
using DDPM.SA.Common.Display;
using DDPM.UI.Common;
using Dell.Client.Framework.UX.WPF.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Windows.System;

namespace DDPM.UI.Module.DisplayHotkeys
{
    /// <summary>
    /// Interaction logic for DisplayHotkeysRightView.xaml
    /// </summary>
    public partial class DisplayHotkeysRightView : UserControl
    {
        string _strTbToggleInputSourcePreviousKey = string.Empty;
        string _strTbFavoriteInputSourcePreviousKey = string.Empty;
        string _strTbSwitchInputSourcePreviousKey = string.Empty;
        string _strTbSwapPIPPBPInputSourcePreviousKey = string.Empty;
        string _strTbChangePIPPositionPreviousKey = string.Empty;
        
        List<VirtualKey> ToggleInputSourceNewKeys = new List<VirtualKey>();
        List<VirtualKey> FavoriteInputSourceNewKeys = new List<VirtualKey>();
        List<VirtualKey> SwitchInputSourceNewKeys = new List<VirtualKey>();
        List<VirtualKey> SwapPIPPBPInputSourceNewKeys = new List<VirtualKey>();
        List<VirtualKey> ChangePIPPositionNewKeys = new List<VirtualKey>();
        

        bool updateKeys = false;
        List<VirtualKey> newKeys = new List<VirtualKey>();

        private DisplayHotkeysViewModel vm
        {
            get=> (DisplayHotkeysViewModel)DataContext;
        }
        public DisplayHotkeysRightView(DisplayHotkeysViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
        }

        private void tbToggleInputSource_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            setUXTextBoxPreviewKey(sender, e);
        }

        private void tbToggleInputSource_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            /*string swHortcutText = string.Empty;
            KeysHelper.ReSetHotKeyText(ref swHortcutText, ref ToggleInputSourceNewKeys);
            vm.ToggleInputSourceKey = swHortcutText;*/
            e.Handled = true;
        } 

        private void tbToggleInputSource_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            e.Handled = true;
        }

        private void tbFavoriteInputSource_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            setUXTextBoxPreviewKey(sender, e);
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
            setUXTextBoxPreviewKey(sender, e);
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
            setUXTextBoxPreviewKey(sender, e);
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
            setUXTextBoxPreviewKey(sender, e);
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
                updateKeys = false;
                newKeys.Clear();
                _strTbToggleInputSourcePreviousKey = vm.ToggleInputSourceKey;
                vm.ToggleInputSourceKey = string.Empty;
            }
        }
        private void tbFavoriteInputSource_GotFocus(object sender, RoutedEventArgs e)
        {
            bool isUnhook = DdpmCommonHelper.DeviceManagerSA.UnHook().Result;
            if (isUnhook)
            {
                updateKeys = false;
                newKeys.Clear();
                _strTbFavoriteInputSourcePreviousKey = vm.FavoriteInputSourceKey;
                vm.FavoriteInputSourceKey = string.Empty;
            }
        }
        private void tbSwitchInputSource_GotFocus(object sender, RoutedEventArgs e)
        {
            bool isUnhook = DdpmCommonHelper.DeviceManagerSA.UnHook().Result;
            if (isUnhook)
            {
                updateKeys = false;
                newKeys.Clear();
                _strTbSwitchInputSourcePreviousKey = vm.SwitchInputSourceKey;
                vm.SwitchInputSourceKey = string.Empty;
            }
        }

        private void tbSwapPIPPBPInputSource_GotFocus(object sender, RoutedEventArgs e)
        {
            bool isUnhook = DdpmCommonHelper.DeviceManagerSA.UnHook().Result;
            if (isUnhook)
            {
                updateKeys = false;
                newKeys.Clear();
                _strTbSwapPIPPBPInputSourcePreviousKey = vm.SwapPIPPBPInputSourceKey;
                vm.SwapPIPPBPInputSourceKey = string.Empty;
            }
        }
        private void tbChangePIPPosition_GotFocus(object sender, RoutedEventArgs e)
        {
            bool isUnhook = DdpmCommonHelper.DeviceManagerSA.UnHook().Result;
            if (isUnhook)
            {
                updateKeys = false;
                newKeys.Clear();
                _strTbChangePIPPositionPreviousKey = vm.ChangePIPPositionKey;
                vm.ChangePIPPositionKey = string.Empty;
            }
        }

        private void doLostFocus(HotkeyInfo hotkeyInfo, string prStr, string crStr, ref List<VirtualKey> keys)
        {
            if (KeysHelper.hotKeyConflictsCheck(hotkeyInfo))
            {
                //save hotkey
                // SaveHotkeysSetting(_strTbBrightnessMinsPreviousKey, vm.BrightnessMinsKey, HotkeyType.BrightnessReduce, ref BrightnessMinsNewKeys, "Brightness-");
                bool saveSettings = DdpmCommonHelper.DeviceManagerSA.SaveHotkeySetting(vm.DisplayHotkeysModule.SelectedHomeDevice.MonitorInfo.edid,hotkeyInfo).Result;
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


        private void SaveHotkeysSetting(string preKey,string crtKey,HotkeyType hotkeyType, ref List<VirtualKey> keys,string des)
        {
            List<HotkeyInfo> hotkeyInfoList = new List<HotkeyInfo>();
            HotkeySettings curHotkey = DdpmCommonHelper.DeviceManagerSA.ReadCurrentHotkey(vm.DisplayHotkeysModule.SelectedHomeDevice.MonitorInfo.edid).Result;
            List<HotkeySettings> List = new List<HotkeySettings>();
            List<InputSourceObj> inputSourceList= new List<InputSourceObj>();

            switch (hotkeyType)
            {
                case HotkeyType.FavoriteInputSource:
                    inputSourceList.Add(new InputSourceObj(vm.FavoriteInput_Selected.inputDisplayText));
                    break;
                case HotkeyType.SwitchInputSource:
                    inputSourceList.Add(new InputSourceObj(vm.SwitchInput1_Selected.inputDisplayText));
                    inputSourceList.Add(new InputSourceObj(vm.SwitchInput2_Selected.inputDisplayText));
                    break ;
            }

           /* _FavoriteInputSelect = vm.InputsList.Find(x => (x.inputSource == DisplayHotkeysModule.SelectedHomeDevice.MonitorInfo.inputSource));
            _swapInput1Select = _inputsList.Find(x => (x.inputSource == DisplayHotkeysModule.SelectedHomeDevice.MonitorInfo.inputSource));
            _swapInput2Select = _inputsList.Where(x => x.inputDisplayText != _swapInput1Select.inputDisplayText).First();*/

            HotkeyInfo? hotkeyInfo = curHotkey.HotkeyInfo.Find(x => x.Job.Equals(hotkeyType));
            var hotkeys = keys.Distinct().ToList();
            hotkeys = hotkeys.Count > 0 ? hotkeys : new List<VirtualKey>() { VirtualKey.None };
            if (!preKey.Equals(crtKey))
            {
                if (hotkeyInfo != null)
                {
                    hotkeyInfo.Hotkey = hotkeys;
                    hotkeyInfo.InputSource = inputSourceList;
                    List.Add(curHotkey);
                }
                else
                {
                    HotkeyInfo newhotkeyInfo = new HotkeyInfo
                    {
                        Description = $"Input Source> Hotkeys > {des}",
                        Hotkey = hotkeys,
                        Job = hotkeyType,
                        Status = HotkeyStatus.Registered,
                        InputSource = inputSourceList
                    };

                    if (curHotkey.HotkeyInfo.Count != 0) {
                        curHotkey.HotkeyInfo.Add(newhotkeyInfo);
                        List.Add(curHotkey);
                    }
                    else
                    {
                        hotkeyInfoList.Add(newhotkeyInfo);
                        HotkeySettings hotkeySettings = new HotkeySettings();
                        hotkeySettings.HotkeyInfo = hotkeyInfoList;
                        hotkeySettings.DeviceInfo = vm.DisplayHotkeysModule.SelectedHomeDevice.MonitorInfo.edid;
                        List.Add(hotkeySettings);
                    }
                }
                DdpmCommonHelper.DeviceManagerSA.WriteHotkeySettings(List);
                DdpmCommonHelper.DeviceManagerSA.ReloadHotkeyConfigData();
                Keyboard.ClearFocus();
            }
        }

        private void tbToggleInputSource_LostFocus(object sender, RoutedEventArgs e)
        {
            if (ToggleInputSourceNewKeys.Count == 0)
            {
                vm.ToggleInputSourceKey = _strTbToggleInputSourcePreviousKey;
            }
            else
            {
                //for single key
                updateKeys = false;
                newKeys.Clear();

                //save hotkey
                //SaveHotkeysSetting(_strTbFavoriteInputSourcePreviousKey, vm.ToggleInputSourceKey, HotkeyType.ToggleInputSource, ref ToggleInputSourceNewKeys, "Toggle to next input source");
                HotkeyInfo hotkeyInfo = new HotkeyInfo();
                hotkeyInfo.Job = HotkeyType.ToggleInputSource;
                hotkeyInfo.Hotkey = ToggleInputSourceNewKeys.Distinct().ToList();
                doLostFocus(hotkeyInfo, _strTbToggleInputSourcePreviousKey, vm.ToggleInputSourceKey, ref ToggleInputSourceNewKeys);
                ToggleInputSourceNewKeys.Clear();
            }
            //hook
            bool isHook = DdpmCommonHelper.DeviceManagerSA.Hook().Result;
        }
        private void tbFavoriteInputSource_LostFocus(object sender, RoutedEventArgs e)
        {
            if (FavoriteInputSourceNewKeys.Count == 0)
            {
                vm.FavoriteInputSourceKey = _strTbFavoriteInputSourcePreviousKey;
            }
            else
            {
                //for single key
                updateKeys = false;
                newKeys.Clear();
                //save hotkey
                //SaveHotkeysSetting(_strTbFavoriteInputSourcePreviousKey, vm.FavoriteInputSourceKey, HotkeyType.FavoriteInputSource, ref FavoriteInputSourceNewKeys, "Favorite input source");
                HotkeyInfo hotkeyInfo = new HotkeyInfo();
                hotkeyInfo.Job = HotkeyType.FavoriteInputSource;
                hotkeyInfo.Hotkey = FavoriteInputSourceNewKeys.Distinct().ToList();
                hotkeyInfo.InputSource.Add(new InputSourceObj(vm.FavoriteInput_Selected.inputDisplayText));
                doLostFocus(hotkeyInfo, _strTbFavoriteInputSourcePreviousKey, vm.FavoriteInputSourceKey, ref FavoriteInputSourceNewKeys);
                FavoriteInputSourceNewKeys.Clear();
            }
            //hook
            bool isHook = DdpmCommonHelper.DeviceManagerSA.Hook().Result;
        }

        private void tbSwitchInputSource_LostFocus(object sender, RoutedEventArgs e)
        {
            if (SwitchInputSourceNewKeys.Count == 0)
            {
                vm.SwitchInputSourceKey = _strTbSwitchInputSourcePreviousKey;
            }
            else
            {
                //for single key
                updateKeys = false;
                newKeys.Clear();
                //save hotkey
                //SaveHotkeysSetting(_strTbSwitchInputSourcePreviousKey, vm.SwitchInputSourceKey, HotkeyType.SwitchInputSource, ref SwitchInputSourceNewKeys, "Swap between 2 inout sources");
                HotkeyInfo hotkeyInfo = new HotkeyInfo();
                hotkeyInfo.Job = HotkeyType.SwitchInputSource;
                hotkeyInfo.Hotkey = SwitchInputSourceNewKeys.Distinct().ToList();
                hotkeyInfo.InputSource.Add(new InputSourceObj(vm.SwitchInput1_Selected.inputDisplayText));
                hotkeyInfo.InputSource.Add(new InputSourceObj(vm.SwitchInput2_Selected.inputDisplayText));
                doLostFocus(hotkeyInfo,_strTbSwitchInputSourcePreviousKey, vm.SwitchInputSourceKey, ref SwitchInputSourceNewKeys);
                SwitchInputSourceNewKeys.Clear();
            }
            //hook
            bool isHook = DdpmCommonHelper.DeviceManagerSA.Hook().Result;
        }

        private void tbSwapPIPPBPInputSource_LostFocus(object sender, RoutedEventArgs e)
        {
            if (SwapPIPPBPInputSourceNewKeys.Count == 0)
            {
                vm.SwapPIPPBPInputSourceKey = _strTbSwapPIPPBPInputSourcePreviousKey;
            }
            else
            {
                //for single key
                updateKeys = false;
                newKeys.Clear();
                //save hotkey
                //SaveHotkeysSetting(_strTbSwapPIPPBPInputSourcePreviousKey, vm.SwapPIPPBPInputSourceKey, HotkeyType.SwapIputPIPPBP, ref SwapPIPPBPInputSourceNewKeys, "swap 2 inuts of PIP/PBP");
                HotkeyInfo hotkeyInfo = new HotkeyInfo();
                hotkeyInfo.Job = HotkeyType.SwapIputPIPPBP;
                hotkeyInfo.Hotkey = SwapPIPPBPInputSourceNewKeys.Distinct().ToList();
                doLostFocus(hotkeyInfo,_strTbSwapPIPPBPInputSourcePreviousKey, vm.SwapPIPPBPInputSourceKey, ref SwapPIPPBPInputSourceNewKeys);
                SwapPIPPBPInputSourceNewKeys.Clear();
            }
            //hook
            bool isHook = DdpmCommonHelper.DeviceManagerSA.Hook().Result;
        }

        private void tbChangePIPPosition_LostFocus(object sender, RoutedEventArgs e)
        {
            if (ChangePIPPositionNewKeys.Count == 0)
            {
                vm.ChangePIPPositionKey = _strTbChangePIPPositionPreviousKey;
            }
            else
            {
                //for single key
                updateKeys = false;
                newKeys.Clear();
                //save hotkey
                //SaveHotkeysSetting(_strTbChangePIPPositionPreviousKey, vm.ChangePIPPositionKey, HotkeyType.ChangePIPPosition, ref ChangePIPPositionNewKeys, "Change PIP position");
                HotkeyInfo hotkeyInfo = new HotkeyInfo();
                hotkeyInfo.Job = HotkeyType.ChangePIPPosition;
                hotkeyInfo.Hotkey = ChangePIPPositionNewKeys.Distinct().ToList();
                doLostFocus(hotkeyInfo, _strTbChangePIPPositionPreviousKey, vm.ChangePIPPositionKey, ref ChangePIPPositionNewKeys);
                ChangePIPPositionNewKeys.Clear();
            }
            //hook
            bool isHook = DdpmCommonHelper.DeviceManagerSA.Hook().Result;
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
                case "tbToggleInputSource":
                    ToggleInputSourceNewKeys.AddRange(newKeys);
                    break;
                case "tbFavoriteInputSource":
                    FavoriteInputSourceNewKeys.AddRange(newKeys);
                    break;
                case "tbSwitchInputSource":
                    SwitchInputSourceNewKeys.AddRange(newKeys);
                    break;
                case "tbSwapPIPPBPInputSource":
                    SwapPIPPBPInputSourceNewKeys.AddRange(newKeys);
                    break;
                case "tbChangePIPPosition":
                    ChangePIPPositionNewKeys.AddRange(newKeys);
                    break;
            }
        }

    }
}
