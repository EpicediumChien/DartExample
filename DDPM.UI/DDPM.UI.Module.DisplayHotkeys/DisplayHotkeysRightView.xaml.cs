using DDPM.SA.Common.Display;
using DDPM.UI.Common;
using Dell.Client.Framework.UX.WPF.Controls;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using VcpCore.Common;
using Windows.System;

namespace DDPM.UI.Module.DisplayHotkeys
{
    /// <summary>
    /// Interaction logic for DisplayHotkeysRightView.xaml
    /// </summary>
    public partial class DisplayHotkeysRightView : UserControl
    {

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
        }

        private void tbToggleInputSource_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            KeysHelper.setUXTextBoxPreviewKey(sender, e, ref newKeys, ref BundleNewKeys, ref alphabetKey);
        }

        private void SaveHotkeySettings(HotkeyInfo hotkeyInfo)
        {
            tbCleanFocus.Focus();
            DisplayHotkeysViewModel dataContext = (DisplayHotkeysViewModel)DataContext;
            if (dataContext != null)
            {
                dataContext.IsBusy = true;
                Task.Run(() =>
                {
                    if (DdpmCommonHelper.DeviceManagerSA != null)
                    {
                        bool saveSettings = DdpmCommonHelper.DeviceManagerSA.SaveHotkeySetting(dataContext.DisplayHotkeysModule.SelectedHomeDevice.MonitorInfo, hotkeyInfo).Result;
                        if (saveSettings)
                        {
                            DdpmCommonHelper.isHotkeyBypass = DdpmCommonHelper.DeviceManagerSA.ByPassHotkey(false).Result;
                        }
                    }

                }).ContinueWith((t) =>
                {
                    dataContext.IsBusy = false;
                    dataContext.Invoke_RefreshData();
                });
            }

        }
        private void tbToggleInputSource_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            e.Handled = true;
            HotkeyInfo hotkeyInfo = KeysHelper.getUXTextBoxHotkeyInfo(sender, e, ref newKeys, HotkeyType.ToggleInputSource);
            if (hotkeyInfo.Hotkey != null && hotkeyInfo.Hotkey.Count > 0)
            {
                if (KeysHelper.onlyContainModifyKeys(hotkeyInfo.Hotkey) || BundleNewKeys.Count == 0 && newKeys.Count == 0)
                {
                    vm.ToggleInputSourceKey = _strPreviousKey;
                    BundleNewKeys.Clear();
                    var texBox = (sender as UXTextBox);
                    if (texBox == null) return;
                    texBox.Text = vm.ToggleInputSourceKey;
                    texBox.Select(vm.ToggleInputSourceKey.Length, 1);
                }
                else
                {
                    //for single key
                    alphabetKey = false;
                    newKeys.Clear();

                    if (KeysHelper.hotKeyConflictsCheck(hotkeyInfo))
                    {
                        SaveHotkeySettings(hotkeyInfo);
                    }
                    else
                    {
                        vm.ToggleInputSourceKey = _strPreviousKey;
                    }

                    BundleNewKeys.Clear();

                }
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
            HotkeyInfo hotkeyInfo = KeysHelper.getUXTextBoxHotkeyInfo(sender, e, ref newKeys, HotkeyType.FavoriteInputSource);
            if (hotkeyInfo.Hotkey != null && hotkeyInfo.Hotkey.Count > 0)
            {
                if (KeysHelper.onlyContainModifyKeys(hotkeyInfo.Hotkey) || BundleNewKeys.Count == 0 && newKeys.Count == 0)
                {
                    vm.FavoriteInputSourceKey = _strPreviousKey;
                    BundleNewKeys.Clear();
                    var texBox = (sender as UXTextBox);
                    if (texBox == null) return;
                    texBox.Text = vm.FavoriteInputSourceKey;
                    texBox.Select(vm.FavoriteInputSourceKey.Length, 1);
                }
                else
                {
                    //for single key
                    alphabetKey = false;
                    newKeys.Clear();
                    if (vm.InputsList != null && vm.InputsList.Count > 0)
                    {
                        //save hotkey inputsource
                        if (vm.FavoriteInput_Selected == null)
                        {
                            vm.FavoriteInput_Selected = vm.InputsList.Single(x => x.inputSource.Equals(vm.DisplayHotkeysModule.SelectedHomeDevice.MonitorInfo.inputSource));
                            hotkeyInfo.InputSource.Add(new InputSourceObj((ushort)vm.FavoriteInput_Selected.inputCode, vm.DisplayHotkeysModule.SelectedHomeDevice.MonitorInfo.inputSource));
                        }
                        else
                        {
                            hotkeyInfo.InputSource.Add(new InputSourceObj((ushort)vm.FavoriteInput_Selected.inputCode, vm.FavoriteInput_Selected.inputSource));
                        }
                        if (KeysHelper.hotKeyConflictsCheck(hotkeyInfo))
                        {
                            SaveHotkeySettings(hotkeyInfo);
                            //vm.Invoke_RefreshData();
                        }
                        else
                        {
                            vm.FavoriteInputSourceKey = _strPreviousKey;
                        }

                        BundleNewKeys.Clear();
                    }
                    else
                    {
                        vm.FavoriteInputSourceKey = _strPreviousKey;
                        BundleNewKeys.Clear();
                        var texBox = (sender as UXTextBox);
                        if (texBox == null) return;
                        texBox.Text = vm.FavoriteInputSourceKey;
                        texBox.Select(vm.FavoriteInputSourceKey.Length, 1);
                    }
                }
            }
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
            HotkeyInfo hotkeyInfo = KeysHelper.getUXTextBoxHotkeyInfo(sender, e, ref newKeys, HotkeyType.SwitchInputSource);
            if (hotkeyInfo.Hotkey != null && hotkeyInfo.Hotkey.Count > 0)
            {
                if (KeysHelper.onlyContainModifyKeys(hotkeyInfo.Hotkey) || BundleNewKeys.Count == 0 && newKeys.Count == 0)
                {
                    vm.SwitchInputSourceKey = _strPreviousKey;
                    BundleNewKeys.Clear();
                    var texBox = (sender as UXTextBox);
                    if (texBox == null) return;
                    texBox.Text = vm.SwitchInputSourceKey;
                    texBox.Select(vm.SwitchInputSourceKey.Length, 1);
                }
                else
                {
                    //for single key
                    alphabetKey = false;
                    newKeys.Clear();

                    if (vm.InputsList != null && vm.InputsList.Count >= 2)
                    {
                        if (vm.SwitchInput1_Selected == null && vm.SwitchInput2_Selected == null)
                        {
                            //save hotkey inputsource default
                            vm.SwitchInput1_Selected = vm.InputsList.Single(x => x.inputSource.Equals(vm.DisplayHotkeysModule.SelectedHomeDevice.MonitorInfo.inputSource));
                            vm.SwitchInput2_Selected = vm.InputsList.First(x => !x.inputSource.Equals(vm.DisplayHotkeysModule.SelectedHomeDevice.MonitorInfo.inputSource));
                            hotkeyInfo.InputSource.Add(new InputSourceObj((ushort)vm.SwitchInput1_Selected.inputCode, vm.SwitchInput1_Selected.inputSource));
                            hotkeyInfo.InputSource.Add(new InputSourceObj((ushort)vm.SwitchInput2_Selected.inputCode, vm.SwitchInput2_Selected.inputSource));
                        }
                        else
                        {
                            //save hotkey inputsource
                            hotkeyInfo.InputSource.Add(new InputSourceObj((ushort)vm.SwitchInput1_Selected.inputCode, vm.SwitchInput1_Selected.inputSource));
                            hotkeyInfo.InputSource.Add(new InputSourceObj((ushort)vm.SwitchInput2_Selected.inputCode, vm.SwitchInput2_Selected.inputSource));
                        }

                        if (KeysHelper.hotKeyConflictsCheck(hotkeyInfo))
                        {
                            SaveHotkeySettings(hotkeyInfo);
                        }
                        else
                        {
                            vm.SwitchInputSourceKey = _strPreviousKey;
                        }

                        BundleNewKeys.Clear();
                    }
                    else
                    {
                        vm.SwitchInputSourceKey = _strPreviousKey;
                        BundleNewKeys.Clear();
                        var texBox = (sender as UXTextBox);
                        if (texBox == null) return;
                        texBox.Text = vm.SwitchInputSourceKey;
                        texBox.Select(vm.SwitchInputSourceKey.Length, 1);
                    }
                }
            }
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
            HotkeyInfo hotkeyInfo = KeysHelper.getUXTextBoxHotkeyInfo(sender, e, ref newKeys, HotkeyType.SwapIputPIPPBP);
            if (hotkeyInfo.Hotkey != null && hotkeyInfo.Hotkey.Count > 0)
            {
                if (KeysHelper.onlyContainModifyKeys(hotkeyInfo.Hotkey) || BundleNewKeys.Count == 0 && newKeys.Count == 0)
                {
                    vm.SwapPIPPBPInputSourceKey = _strPreviousKey;
                    BundleNewKeys.Clear();
                    var texBox = (sender as UXTextBox);
                    if (texBox == null) return;
                    texBox.Text = vm.SwapPIPPBPInputSourceKey;
                    texBox.Select(vm.SwapPIPPBPInputSourceKey.Length, 1);
                }
                else
                {
                    //for single key
                    alphabetKey = false;
                    newKeys.Clear();

                    if (KeysHelper.hotKeyConflictsCheck(hotkeyInfo))
                    {
                        SaveHotkeySettings(hotkeyInfo);
                        //vm.Invoke_RefreshData();
                    }
                    else
                    {
                        vm.SwapPIPPBPInputSourceKey = _strPreviousKey;
                    }

                    BundleNewKeys.Clear();

                }
            }
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
            HotkeyInfo hotkeyInfo = KeysHelper.getUXTextBoxHotkeyInfo(sender, e, ref newKeys, HotkeyType.ChangePIPPosition);
            if (hotkeyInfo.Hotkey != null && hotkeyInfo.Hotkey.Count > 0)
            {
                if (KeysHelper.onlyContainModifyKeys(hotkeyInfo.Hotkey) || BundleNewKeys.Count == 0 && newKeys.Count == 0)
                {
                    vm.ChangePIPPositionKey = _strPreviousKey;
                    BundleNewKeys.Clear();
                    var texBox = (sender as UXTextBox);
                    if (texBox == null) return;
                    texBox.Text = vm.ChangePIPPositionKey;
                    texBox.Select(vm.ChangePIPPositionKey.Length, 1);
                }
                else
                {
                    //for single key
                    alphabetKey = false;
                    newKeys.Clear();

                    if (KeysHelper.hotKeyConflictsCheck(hotkeyInfo))
                    {
                        SaveHotkeySettings(hotkeyInfo);
                        //vm.Invoke_RefreshData();
                    }
                    else
                    {
                        vm.ChangePIPPositionKey = _strPreviousKey;
                    }

                    BundleNewKeys.Clear();

                }
            }
            e.Handled = true;
        }

        private void tbChangePIPPosition_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            e.Handled = true;
        }

        private void tbToggleInputSource_GotFocus(object sender, RoutedEventArgs e)
        {
            alphabetKey = false;
            newKeys.Clear();
            //_strTbToggleInputSourcePreviousKey = vm.ToggleInputSourceKey;
            _strPreviousKey = vm.ToggleInputSourceKey;
            //vm.ToggleInputSourceKey = string.Empty;
            var texBox = (sender as UXTextBox);
            texBox?.Select(vm.ToggleInputSourceKey.Length, 1);
        }

        private void tbFavoriteInputSource_GotFocus(object sender, RoutedEventArgs e)
        {
            alphabetKey = false;
            newKeys.Clear();
            //_strTbFavoriteInputSourcePreviousKey = vm.FavoriteInputSourceKey;
            _strPreviousKey = vm.FavoriteInputSourceKey;
            //vm.FavoriteInputSourceKey = string.Empty;
            var texBox = (sender as UXTextBox);
            texBox?.Select(vm.FavoriteInputSourceKey.Length, 1);
        }

        private void tbSwitchInputSource_GotFocus(object sender, RoutedEventArgs e)
        {
            alphabetKey = false;
            newKeys.Clear();
            _strPreviousKey = vm.SwitchInputSourceKey;
            //vm.SwitchInputSourceKey = string.Empty;
            var texBox = (sender as UXTextBox);
            texBox?.Select(vm.FavoriteInputSourceKey.Length, 1);
        }

        private void tbSwapPIPPBPInputSource_GotFocus(object sender, RoutedEventArgs e)
        {
            alphabetKey = false;
            newKeys.Clear();
            _strPreviousKey = vm.SwapPIPPBPInputSourceKey;
            //vm.SwapPIPPBPInputSourceKey = string.Empty;
            var texBox = (sender as UXTextBox);
            texBox?.Select(vm.FavoriteInputSourceKey.Length, 1);
        }

        private void tbChangePIPPosition_GotFocus(object sender, RoutedEventArgs e)
        {
            alphabetKey = false;
            newKeys.Clear();
            _strPreviousKey = vm.ChangePIPPositionKey;
            //vm.ChangePIPPositionKey = string.Empty;
            var texBox = (sender as UXTextBox);
            texBox?.Select(vm.FavoriteInputSourceKey.Length, 1);
        }

        private void tbToggleInputSource_LostFocus(object sender, RoutedEventArgs e)
        {
            //hook
            //bool isHook = DdpmCommonHelper.DeviceManagerSA.Hook().Result;
        }

        private void tbFavoriteInputSource_LostFocus(object sender, RoutedEventArgs e)
        {
            //hook
            //bool isHook = DdpmCommonHelper.DeviceManagerSA.Hook().Result;
        }

        private void tbSwitchInputSource_LostFocus(object sender, RoutedEventArgs e)
        {
            //hook
            //bool isHook = DdpmCommonHelper.DeviceManagerSA.Hook().Result;
        }

        private void tbSwapPIPPBPInputSource_LostFocus(object sender, RoutedEventArgs e)
        {
            //hook
            //bool isHook = DdpmCommonHelper.DeviceManagerSA.Hook().Result;
        }

        private void tbChangePIPPosition_LostFocus(object sender, RoutedEventArgs e)
        {
            //hook
            //bool isHook = DdpmCommonHelper.DeviceManagerSA.Hook().Result;
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var cb = sender as ComboBox;
            if (cb != null && cb.IsDropDownOpen)
            {
                //update
                Debug.WriteLine("user changes the selected");
                InputSourceList? inputSourceList = cb.SelectedItem as InputSourceList;
                if (inputSourceList != null)
                {
                    vm.SaveHotkeySettings(new InputSourceObj((ushort)inputSourceList.inputCode, inputSourceList.inputSource), "FavoriteCombobox");
                    vm.FavoriteInput_Selected = inputSourceList;
                }
            }
            else
            {
                Debug.WriteLine("internel changes the selected");
            }

        }

        private void ComboBox_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            var cb = sender as ComboBox;
            if (cb != null && cb.IsDropDownOpen)
            {
                //update
                //Debug.WriteLine("user changes the selected");
                InputSourceList? inputSourceList = cb.SelectedItem as InputSourceList;
                if (inputSourceList != null)
                {
                    vm.SaveHotkeySettings(new InputSourceObj((ushort)inputSourceList.inputCode, inputSourceList.inputSource), "SwitchCombobox1");
                    vm.SwitchInput1_Selected = inputSourceList;
                }
            }
            else
            {
                //Debug.WriteLine("internal changes the selected");
            }
        }

        private void ComboBox_SelectionChanged_2(object sender, SelectionChangedEventArgs e)
        {
            var cb = sender as ComboBox;
            if (cb != null && cb.IsDropDownOpen)
            {
                //update
                //Debug.WriteLine("user changes the selected");
                InputSourceList? inputSourceList = cb.SelectedItem as InputSourceList;
                if (inputSourceList != null)
                {
                    vm.SaveHotkeySettings(new InputSourceObj((ushort)inputSourceList.inputCode, inputSourceList.inputSource), "SwitchCombobox2");
                    vm.SwitchInput2_Selected = inputSourceList;
                }
            }
            else
            {
                //Debug.WriteLine("internal changes the selected");
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (DdpmCommonHelper.bInputSourceRenamed)
            {
                DdpmCommonHelper.bInputSourceRenamed = false;
                vm.Invoke_RefreshData();
            }
        }

        private bool IsTextTruncated(TextBlock tb)
        {
            Typeface typeface = new Typeface(tb.FontFamily, tb.FontStyle, tb.FontWeight, tb.FontStretch);
            FormattedText formattedText = new FormattedText(tb.Text, System.Globalization.CultureInfo.CurrentCulture,
                                            tb.FlowDirection, typeface, tb.FontSize, Brushes.Black);

            if ((int)formattedText.Width > (int)tb.ActualWidth)
                return true;

            return false;
        }

        private void UXDisplayHotkeys1TextBlock_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                TextBlock? tb = sender as TextBlock;

                if (tb != null)
                {
                    if (IsTextTruncated(tb))
                        displayHotkeys1Tooltip.Visibility = Visibility.Visible;
                    else
                        displayHotkeys1Tooltip.Visibility = Visibility.Hidden;
                }
            }
            catch (Exception ex)
            {

                DdpmCommonHelper.WriteUILog($"Catch Exception[{ex.Message}] when run UXDisplayHotkeys1TextBlock_Loaded");
            } 
        }

        private void UXDisplayHotkeys3TextBlock_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                TextBlock? tb = sender as TextBlock;

                if (tb != null)
                {
                    if (IsTextTruncated(tb))
                        displayHotkeys3Tooltip.Visibility = Visibility.Visible;
                    else
                        displayHotkeys3Tooltip.Visibility = Visibility.Hidden;
                }
            }
            catch (Exception ex)
            {

                DdpmCommonHelper.WriteUILog($"Catch Exception[{ex.Message}] when run UXDisplayHotkeys3TextBlock_Loaded");
            }
        }
    }
}