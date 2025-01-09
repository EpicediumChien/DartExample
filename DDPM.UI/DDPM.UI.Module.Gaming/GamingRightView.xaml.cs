using DDPM.SA.Common.Display;
using DDPM.UI.Common;
using Dell.Client.Framework.Common;
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

        private bool alphabetKey = false;
        private string _strPreviousKey = string.Empty;
        private List<VirtualKey> newKeys = new List<VirtualKey>();
        private List<VirtualKey> BundleNewKeys = new List<VirtualKey>();
        public GamingRightView()
        {
            InitializeComponent();
            if (DdpmCommonHelper.DeviceManagerSA != null)
            {
                Trace.WriteLine($"GamingRightView DdpmCommonHelper.DeviceManagerSA is not null");
                DdpmCommonHelper.DeviceManagerSA.ITSettingsActionEvent += DeviceManagerSA_ITSettingsActionEvent;
            }
            else
            {
                Trace.WriteLine($"GamingRightView DdpmCommonHelper.DeviceManagerSA is null");
            }
        }
        ~GamingRightView()
        {
            if (DdpmCommonHelper.DeviceManagerSA != null)
            {
                Trace.WriteLine($"GamingRightView DdpmCommonHelper.DeviceManagerSA is not null");
                DdpmCommonHelper.DeviceManagerSA.ITSettingsActionEvent -= DeviceManagerSA_ITSettingsActionEvent;
            }
            else
            {
                Trace.WriteLine($"GamingRightView DdpmCommonHelper.DeviceManagerSA is null");
            }
        }
        private void DeviceManagerSA_ITSettingsActionEvent(object? sender, SA.Common.ITSettingEventArgs e)
        {
            bool? isLocked = DdpmCommonHelper.GetUINotifyPropertyValue_Boolean("Lock_Display_ResolutionRefreshRate", e);
            if (isLocked != null)
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    GamingViewModel localVm = (GamingViewModel)this.DataContext;
                    if (localVm != null)
                    {
                        localVm.Lock_RefreshRate = (bool)isLocked;
                        Trace.WriteLine($"[SettingsPage] DisplayProperty RefreshRate(Lock) : {isLocked}");
                    }
                }));
            }
        }
        private void CallWindowsSettings_Click(object sender, RoutedEventArgs e)
        {
            DdpmCommonHelper.DeviceManagerSA.CallWindowsDisplaySetting();
        }

        private void RefreshUI()
        {
            GamingViewModel localVm = (GamingViewModel)DataContext;
            localVm.RefreshUI();
        }

        private GamingViewModel vm
        {
            get
            {
                return (GamingViewModel)DataContext;
            }
        }

        private void tbDarkStabilizerToggle_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            KeysHelper.setUXTextBoxPreviewKey(sender, e, ref newKeys, ref BundleNewKeys, ref alphabetKey);
        }

        private void SaveHotkeySettings(HotkeyInfo hotkeyInfo)
        {
            tbCleanFocus.Focus();
            GamingViewModel dataContext = (GamingViewModel)DataContext;
            if (dataContext != null)
            {
                Common.Models.HomeDevice? selectedHomeDevice = dataContext.MyModule.SelectedHomeDevice;
                if (selectedHomeDevice?.MonitorInfo != null)
                {
                    Task.Run(() =>
                    {
                        dataContext.SaveHotkeySettings(selectedHomeDevice.MonitorInfo, hotkeyInfo);
                    });
                }
            }
        }
        private void tbDarkStabilizerToggle_PreviewKeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            HotkeyInfo hotkeyInfo = KeysHelper.getUXTextBoxHotkeyInfo(sender, e, ref newKeys, HotkeyType.DarkStabilizerToggle);
            if (hotkeyInfo.Hotkey != null && hotkeyInfo.Hotkey.Count > 0)
            {
                if (KeysHelper.onlyContainModifyKeys(hotkeyInfo.Hotkey) || BundleNewKeys.Count == 0 && newKeys.Count == 0)
                {
                    vm.DarkStabilizerToggleKey = _strPreviousKey;
                    BundleNewKeys.Clear();
                    var texBox = (sender as UXTextBox);
                    if (texBox == null) return;
                    texBox.Text = vm.DarkStabilizerToggleKey;
                    texBox.Select(vm.DarkStabilizerToggleKey.Length, 1);
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
                        vm.DarkStabilizerToggleKey = _strPreviousKey;
                    }

                    BundleNewKeys.Clear();

                }
            }
            e.Handled = true;
        }

        private void tbDarkStabilizerToggle_GotFocus(object sender, RoutedEventArgs e)
        {
            alphabetKey = false;
            newKeys.Clear();
            _strPreviousKey = vm.DarkStabilizerToggleKey;
            //vm.DarkStabilizerToggleKey = string.Empty;
            var texBox = (sender as UXTextBox);
            texBox?.Select(vm.DarkStabilizerToggleKey.Length, 1);
        }

        private void tbDarkStabilizerToggle_LostFocus(object sender, RoutedEventArgs e)
        {
            //hook
            //bool isHook = DdpmCommonHelper.DeviceManagerSA.Hook().Result;
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
            HotkeyInfo hotkeyInfo = KeysHelper.getUXTextBoxHotkeyInfo(sender, e, ref newKeys, HotkeyType.DualResolutionToggle);
            if (hotkeyInfo.Hotkey != null && hotkeyInfo.Hotkey.Count > 0)
            {
                if (KeysHelper.onlyContainModifyKeys(hotkeyInfo.Hotkey) || BundleNewKeys.Count == 0 && newKeys.Count == 0)
                {
                    vm.DualResolutionToggleKey = _strPreviousKey;
                    BundleNewKeys.Clear();
                    var texBox = (sender as UXTextBox);
                    if (texBox == null) return;
                    texBox.Text = vm.DualResolutionToggleKey;
                    texBox.Select(vm.DualResolutionToggleKey.Length, 1);
                }
                else
                {
                    //for single key
                    alphabetKey = false;
                    newKeys.Clear();

                    if (KeysHelper.hotKeyConflictsCheck(hotkeyInfo))
                    {
                        //bool saveSettings = DdpmCommonHelper.DeviceManagerSA.SaveHotkeySetting(vm.MyModule.SelectedHomeDevice.MonitorInfo, hotkeyInfo).Result;
                        //vm.Invoke_RefreshData();
                        SaveHotkeySettings(hotkeyInfo);
                    }
                    else
                    {
                        vm.DualResolutionToggleKey = _strPreviousKey;
                    }

                    BundleNewKeys.Clear();

                }
            }
            e.Handled = true;
        }

        private void tbDualResolutionToggle_GotFocus(object sender, RoutedEventArgs e)
        {
            alphabetKey = false;
            newKeys.Clear();
            _strPreviousKey = vm.DualResolutionToggleKey;
            //vm.DualResolutionToggleKey = string.Empty;
            var texBox = (sender as UXTextBox);
            texBox?.Select(vm.DualResolutionToggleKey.Length, 1);
        }

        private void tbDualResolutionToggle_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            e.Handled = true;
        }

        private void tbDualResolutionToggle_LostFocus(object sender, RoutedEventArgs e)
        {
            //hook
            //bool isHook = DdpmCommonHelper.DeviceManagerSA.Hook().Result;
        }
    }
}