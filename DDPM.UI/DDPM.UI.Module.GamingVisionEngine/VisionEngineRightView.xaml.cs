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
        private bool alphabetKey = false;
        private string _strPreviousKey = string.Empty;
        private List<VirtualKey> newKeys = new List<VirtualKey>();
        private List<VirtualKey> BundleNewKeys = new List<VirtualKey>();
        UI_VisionEngine tempUI_VE = null;
        bool isTrigger = true;
        public VisionEngineRightView()
        {
            InitializeComponent();

        }

        private void RefreshUI()
        {
            VisionEngineViewModel localVm = (VisionEngineViewModel)DataContext;
            localVm.RefreshUI();
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
            if (isTrigger)
            {
                VisionEngineViewModel localVm = (VisionEngineViewModel)DataContext;
                List<UI_VisionEngine> tempList = localVm.VisionEngineList.ToList().FindAll(o => o.VisionEngine_Enable);
                localVm.VisionEngineIsEnable = false;
                localVm.RefreshUI();
                localVm.SetVisionEngine();
            }
            isTrigger = true;
        }
        private void UXCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as UXCheckBox;
            if (checkBox != null)
            {
                var selectedItem = checkBox.DataContext as UI_VisionEngine;
                if (selectedItem != null)
                {
                    var localVm = (VisionEngineViewModel)DataContext;
                    List<UI_VisionEngine> tempList = localVm.VisionEngineList.ToList().FindAll(o => o.VisionEngine_Enable);
                    if (tempList.Count <= 0)
                    {
                        checkBox.IsChecked = true;
                        isTrigger = false;
                    }
                }
            }

        }
        private void tbVisionEngineToggle_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            KeysHelper.setUXTextBoxPreviewKey(sender, e, ref newKeys, ref BundleNewKeys, ref alphabetKey);
        }

        private void SaveHotkeySettings(HotkeyInfo hotkeyInfo)
        {
            tbCleanFocus.Focus();
            VisionEngineViewModel dataContext = (VisionEngineViewModel)DataContext;
            if (dataContext != null)
            {
                dataContext.IsBusy = true;
                Task.Run(() =>
                {
                    if (DdpmCommonHelper.DeviceManagerSA != null)
                    {
                        bool saveSettings = DdpmCommonHelper.DeviceManagerSA.SaveHotkeySetting(dataContext.MyModule.SelectedHomeDevice.MonitorInfo, hotkeyInfo).Result;
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
        private void tbVisionEngineToggle_PreviewKeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            e.Handled = true;
            HotkeyInfo hotkeyInfo = KeysHelper.getUXTextBoxHotkeyInfo(sender, e, ref newKeys, HotkeyType.VisionEngineToggle);
            if (hotkeyInfo.Hotkey != null && hotkeyInfo.Hotkey.Count > 0)
            {
                if (KeysHelper.onlyContainModifyKeys(hotkeyInfo.Hotkey) || BundleNewKeys.Count == 0 && newKeys.Count == 0)
                {
                    vm.VisionEngineToggleKey = _strPreviousKey;
                    BundleNewKeys.Clear();
                    var texBox = (sender as UXTextBox);
                    if (texBox == null) return;
                    texBox.Text = vm.VisionEngineToggleKey;
                    texBox.Select(vm.VisionEngineToggleKey.Length, 1);
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
                        vm.VisionEngineToggleKey = _strPreviousKey;
                    }

                    BundleNewKeys.Clear();

                }
            }
        }

        private void tbVisionEngineToggle_GotFocus(object sender, RoutedEventArgs e)
        {
            alphabetKey = false;
            newKeys.Clear();
            //_strTbToggleInputSourcePreviousKey = vm.ToggleInputSourceKey;
            _strPreviousKey = vm.VisionEngineToggleKey;
            //vm.ToggleInputSourceKey = string.Empty;
            var texBox = (sender as UXTextBox);
            texBox?.Select(vm.VisionEngineToggleKey.Length, 1);
        }

        private void tbVisionEngineToggle_LostFocus(object sender, RoutedEventArgs e)
        {
            //hook
            //bool isHook = DdpmCommonHelper.DeviceManagerSA.Hook().Result;
        }

        private void tbVisionEngineToggle_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            e.Handled = true;
        }
    }
}