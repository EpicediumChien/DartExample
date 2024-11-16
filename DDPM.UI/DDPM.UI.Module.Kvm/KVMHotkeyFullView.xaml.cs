using DDPM.SA.Common.Display;
using DDPM.UI.Common;
using Dell.Client.Framework.UX.WPF.Controls;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VcpCore.Common;
using Windows.System;

namespace DDPM.UI.Module.Kvm
{
    /// <summary>
    /// Interaction logic for KVMHotkeyFullView.xaml
    /// </summary>
    public partial class KVMHotkeyFullView : UserControl
    {
        /* private string _strTbSwitchPCsKeyPreviousKey = string.Empty;
         private string _strTbSwitchKbMsKeyPreviousKey = string.Empty;
         private string _strTbChangePipKeyPreviousKey = string.Empty;

         private List<VirtualKey> SwitchPCsKeyNewKeys = new List<VirtualKey>();
         private List<VirtualKey> SwitchKbMsKeyNewKeys = new List<VirtualKey>();
         private List<VirtualKey> ChangePipKeyNewKeys = new List<VirtualKey>();*/

        private bool alphabetKey = false;
        private string _strPreviousKey = string.Empty;
        private List<VirtualKey> newKeys = new List<VirtualKey>();
        private List<VirtualKey> BundleNewKeys = new List<VirtualKey>();

        public KVMHotkeyFullView()
        {
            InitializeComponent();
        }

        private KvmViewModel vm
        {
            get
            {
                return (KvmViewModel)DataContext;
            }
        }

        private void tbSwitchPCsKey_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            KeysHelper.setUXTextBoxPreviewKey(sender, e, ref newKeys, ref BundleNewKeys, ref alphabetKey);
        }

        private void SaveHotkeySettings(HotkeyInfo hotkeyInfo)
        {
            tbCleanFocus.Focus();
            KvmViewModel dataContext = (KvmViewModel)DataContext;
            if (dataContext != null)
            {
                Common.Models.HomeDevice? selectedHomeDevice = dataContext.KvmModule.SelectedHomeDevice;
                if (selectedHomeDevice?.MonitorInfo != null)
                {
                    Task.Run(() =>
                    {
                        dataContext.SaveHotkeySettings(selectedHomeDevice.MonitorInfo, hotkeyInfo);
                    });
                }
            }

        }
        private void tbSwitchPCsKey_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            HotkeyInfo hotkeyInfo = KeysHelper.getUXTextBoxHotkeyInfo(sender, e, ref newKeys, HotkeyType.KvmSwitchInputSource);
            if (hotkeyInfo.Hotkey != null && hotkeyInfo.Hotkey.Count > 0)
            {
                if (KeysHelper.onlyContainModifyKeys(hotkeyInfo.Hotkey) || BundleNewKeys.Count == 0 && newKeys.Count == 0)
                {
                    vm.SwitchPCsKey = _strPreviousKey;
                    BundleNewKeys.Clear();
                    var texBox = (sender as UXTextBox);
                    if (texBox == null) return;
                    texBox.Text = vm.SwitchPCsKey;
                    texBox.Select(vm.SwitchPCsKey.Length, 1);
                }
                else
                {
                    //for single key
                    alphabetKey = false;
                    newKeys.Clear();
                    //add inputsource
                    string inputSource1 = vm.PC1Inputs_Selected.Type;
                    if (!string.IsNullOrEmpty(inputSource1))
                    {
                        hotkeyInfo.InputSource.Add(new InputSourceObj(inputSource1));
                    }
                    string inputSource2 = vm.PC2Inputs_Selected.Type;
                    if (!string.IsNullOrEmpty(inputSource2))
                    {
                        hotkeyInfo.InputSource.Add(new InputSourceObj(inputSource2));
                    }
                    string inputSource3 = vm.PC3Inputs_Selected.Type;
                    if (!string.IsNullOrEmpty(inputSource3))
                    {
                        hotkeyInfo.InputSource.Add(new InputSourceObj(inputSource3));
                    }
                    string inputSource4 = vm.PC4Inputs_Selected.Type;
                    if (!string.IsNullOrEmpty(inputSource4))
                    {
                        hotkeyInfo.InputSource.Add(new InputSourceObj(inputSource4));
                    }
                    if (KeysHelper.hotKeyConflictsCheck(hotkeyInfo))
                    {
                        SaveHotkeySettings(hotkeyInfo);
                    }
                    else
                    {
                        vm.SwitchPCsKey = _strPreviousKey;
                    }

                    BundleNewKeys.Clear();

                }
            }
            e.Handled = true;
        }

        private void tbSwitchPCsKey_GotFocus(object sender, RoutedEventArgs e)
        {

            alphabetKey = false;
            newKeys.Clear();
            _strPreviousKey = vm.SwitchPCsKey;
            //vm.SwitchPCsKey = string.Empty;
            var texBox = (sender as UXTextBox);
            texBox?.Select(vm.SwitchPCsKey.Length, 1);

        }

        private void tbSwitchPCsKey_LostFocus(object sender, RoutedEventArgs e)
        {
            //hook
            //bool isHook = DdpmCommonHelper.DeviceManagerSA.Hook().Result;
        }

        private void tbSwitchPCsKey_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            e.Handled = true;
        }

        private void tbSwitchKbMsKey_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            KeysHelper.setUXTextBoxPreviewKey(sender, e, ref newKeys, ref BundleNewKeys, ref alphabetKey);
        }

        private void tbSwitchKbMsKey_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            HotkeyInfo hotkeyInfo = KeysHelper.getUXTextBoxHotkeyInfo(sender, e, ref newKeys, HotkeyType.KvmSwitchKbMsKey);
            if (hotkeyInfo.Hotkey != null && hotkeyInfo.Hotkey.Count > 0)
            {
                if (KeysHelper.onlyContainModifyKeys(hotkeyInfo.Hotkey) || BundleNewKeys.Count == 0 && newKeys.Count == 0)
                {
                    vm.SwitchKbMsKey = _strPreviousKey;
                    BundleNewKeys.Clear();
                    var texBox = (sender as UXTextBox);
                    if (texBox == null) return;
                    texBox.Text = vm.SwitchKbMsKey;
                    texBox.Select(vm.SwitchKbMsKey.Length, 1);
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
                        vm.SwitchKbMsKey = _strPreviousKey;
                    }

                    BundleNewKeys.Clear();

                }
            }
            e.Handled = true;
        }

        private void tbSwitchKbMsKey_GotFocus(object sender, RoutedEventArgs e)
        {
            alphabetKey = false;
            newKeys.Clear();
            _strPreviousKey = vm.SwitchKbMsKey;
            //vm.SwitchKbMsKey = string.Empty;
            var texBox = (sender as UXTextBox);
            texBox?.Select(vm.SwitchKbMsKey.Length, 1);
        }

        private void tbSwitchKbMsKey_LostFocus(object sender, RoutedEventArgs e)
        {
            //hook
            //bool isHook = DdpmCommonHelper.DeviceManagerSA.Hook().Result;
        }

        private void tbSwitchKbMsKey_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            e.Handled = true;
        }

        private void tbChangePipKey_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            KeysHelper.setUXTextBoxPreviewKey(sender, e, ref newKeys, ref BundleNewKeys, ref alphabetKey);
        }

        private void tbChangePipKey_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            HotkeyInfo hotkeyInfo = KeysHelper.getUXTextBoxHotkeyInfo(sender, e, ref newKeys, HotkeyType.KvmChangePIPPosition);
            if (hotkeyInfo.Hotkey != null && hotkeyInfo.Hotkey.Count > 0)
            {
                if (KeysHelper.onlyContainModifyKeys(hotkeyInfo.Hotkey) || BundleNewKeys.Count == 0 && newKeys.Count == 0)
                {
                    vm.ChangePipKey = _strPreviousKey;
                    BundleNewKeys.Clear();
                    var texBox = (sender as UXTextBox);
                    if (texBox == null) return;
                    texBox.Text = vm.ChangePipKey;
                    texBox.Select(vm.ChangePipKey.Length, 1);
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
                        vm.ChangePipKey = _strPreviousKey;
                    }

                    BundleNewKeys.Clear();

                }
            }
            e.Handled = true;
        }

        private void tbChangePipKey_GotFocus(object sender, RoutedEventArgs e)
        {
            alphabetKey = false;
            newKeys.Clear();
            _strPreviousKey = vm.ChangePipKey;
            //vm.ChangePipKey = string.Empty;
            var texBox = (sender as UXTextBox);
            texBox?.Select(vm.ChangePipKey.Length, 1);
        }

        private void tbChangePipKey_LostFocus(object sender, RoutedEventArgs e)
        {
            //hook
            // bool isHook = DdpmCommonHelper.DeviceManagerSA.Hook().Result;
        }

        private void tbChangePipKey_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            e.Handled = true;
        }

        private void LeftArrow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DdpmCommonHelper.ModuleOwner?.CloseFullView();
        }

        private void UserControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            tbCleanFocus.Focus();
            Keyboard.ClearFocus();
        }

        private void InputSourceHotkey_Click(object sender, RoutedEventArgs e)
        {
            //todo
            if (DdpmCommonHelper.ModuleOwner != null)
            {
                DdpmCommonHelper.ModuleOwner.ShowSpecificModule(Constants.GroupName_InputSource, Constants.ModuleName_DisplayHotkeys);
            }
        }


        private void doLostFocus(HotkeyInfo hotkeyInfo, string prStr, string crStr, ref List<VirtualKey> keys)
        {
            if (KeysHelper.hotKeyConflictsCheck(hotkeyInfo))
            {
                //save hotkey
                // SaveHotkeysSetting(_strTbBrightnessMinsPreviousKey, vm.BrightnessMinsKey, HotkeyType.BrightnessReduce, ref BrightnessMinsNewKeys, "Brightness-");
                bool saveSettings = DdpmCommonHelper.DeviceManagerSA.SaveHotkeySetting(vm.KvmModule.SelectedHomeDevice.MonitorInfo, hotkeyInfo).Result;
                vm.Invoke_RefreshHotkeySettings();
            }
            else
            {
                switch (hotkeyInfo.Job)
                {
                    case HotkeyType.KvmSwitchInputSource:
                        vm.SwitchPCsKey = prStr;
                        break;

                    case HotkeyType.KvmChangePIPPosition:
                        vm.ChangePipKey = prStr;
                        break;

                    case HotkeyType.KvmSwitchKbMsKey:
                        vm.SwitchKbMsKey = prStr;
                        break;
                }
            }
            keys.Clear();
        }
    }
}