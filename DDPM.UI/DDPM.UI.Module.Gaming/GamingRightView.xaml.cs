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
        /* string _strTbDarkStabilizerTogglePreviousKey = string.Empty;
         string _strTbDualResolutionTogglePreviousKey = string.Empty;
         List<VirtualKey> DarkStabilizerToggleNewKeys = new List<VirtualKey>();
         List<VirtualKey> DualResolutionToggleNewKeys = new List<VirtualKey>();*/
        private bool alphabetKey = false;
        private string _strPreviousKey = string.Empty;
        private List<VirtualKey> newKeys = new List<VirtualKey>();
        private List<VirtualKey> BundleNewKeys = new List<VirtualKey>();
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
            KeysHelper.setUXTextBoxPreviewKey(sender, e, ref newKeys, ref BundleNewKeys, ref alphabetKey);
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
                alphabetKey = false;
                newKeys.Clear();
                _strPreviousKey = vm.DarkStabilizerToggleKey;
                //vm.DarkStabilizerToggleKey = string.Empty;
                var texBox = (sender as UXTextBox);
                texBox?.Select(vm.DarkStabilizerToggleKey.Length, 1);
            }
        }

        private void tbDarkStabilizerToggle_LostFocus(object sender, RoutedEventArgs e)
        {
            if (BundleNewKeys.Count == 0 && newKeys.Count == 0)
            {
                vm.DarkStabilizerToggleKey = _strPreviousKey;
                BundleNewKeys.Clear();
            }
            else
            {
                //for single key
                alphabetKey = false;
                newKeys.Clear();

                //save hotkey
                HotkeyInfo hotkeyInfo = new HotkeyInfo();
                hotkeyInfo.Job = HotkeyType.DarkStabilizerToggle;
                hotkeyInfo.Hotkey = BundleNewKeys.Distinct().ToList();
                hotkeyInfo.Description = "DarkStabilizerToggle";
                doLostFocus(hotkeyInfo, _strPreviousKey, vm.DarkStabilizerToggleKey, ref BundleNewKeys);
                BundleNewKeys.Clear();
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
            KeysHelper.setUXTextBoxPreviewKey(sender, e, ref newKeys, ref BundleNewKeys, ref alphabetKey);
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
                alphabetKey = false;
                newKeys.Clear();
                _strPreviousKey = vm.DualResolutionToggleKey;
                //vm.DualResolutionToggleKey = string.Empty;
                var texBox = (sender as UXTextBox);
                texBox?.Select(vm.DualResolutionToggleKey.Length, 1);
            }
        }

        private void tbDualResolutionToggle_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            e.Handled = true;
        }

        private void tbDualResolutionToggle_LostFocus(object sender, RoutedEventArgs e)
        {
            if (BundleNewKeys.Count == 0 && newKeys.Count == 0)
            {
                vm.DualResolutionToggleKey = _strPreviousKey;
                BundleNewKeys.Clear();
            }
            else
            {
                //for single key
                alphabetKey = false;
                newKeys.Clear();

                //save hotkey
                //SaveHotkeysSetting(_strTbBrightnessAddPreviousKey, vm.BrightnessAddKey, HotkeyType.BrightnessIncrease, ref BrightnessAddNewKeys, "Brightness+");
                HotkeyInfo hotkeyInfo = new HotkeyInfo();
                hotkeyInfo.Job = HotkeyType.DualResolutionToggle;
                hotkeyInfo.Hotkey = BundleNewKeys.Distinct().ToList();
                hotkeyInfo.Description = "DualResolutionToggle";
                doLostFocus(hotkeyInfo, _strPreviousKey, vm.DualResolutionToggleKey, ref BundleNewKeys);
                BundleNewKeys.Clear();
            }
            //hook
            bool isHook = DdpmCommonHelper.DeviceManagerSA.Hook().Result;
        }
    }
}