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
            VisionEngineViewModel vm = (VisionEngineViewModel)DataContext;
            vm.RefreshUI();
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
                VisionEngineViewModel vm = (VisionEngineViewModel)DataContext;
                List<UI_VisionEngine> tempList = vm.VisionEngineList.ToList().FindAll(o => o.VisionEngine_Enable);
                vm.VisionEngineIsEnable = false;
                vm.RefreshUI();
                vm.SetVisionEngine();
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
                    var vm = (VisionEngineViewModel)DataContext;
                    List<UI_VisionEngine> tempList = vm.VisionEngineList.ToList().FindAll(o => o.VisionEngine_Enable);
                    if (tempList.Count <= 0)
                    {
                        checkBox.IsChecked = true;
                        isTrigger = false;
                    }
                }
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
                    case HotkeyType.VisionEngineToggle:
                        vm.VisionEngineToggleKey = prStr;
                        break;
                }
            }
            keys.Clear();
        }
        private void tbVisionEngineToggle_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            KeysHelper.setUXTextBoxPreviewKey(sender, e, ref newKeys, ref BundleNewKeys, ref alphabetKey);
        }

        private void tbVisionEngineToggle_PreviewKeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            e.Handled = true;
        }

        private void tbVisionEngineToggle_GotFocus(object sender, RoutedEventArgs e)
        {
            bool isUnhook = DdpmCommonHelper.DeviceManagerSA.UnHook().Result;
            if (isUnhook)
            {
                alphabetKey = false;
                newKeys.Clear();
                _strPreviousKey = vm.VisionEngineToggleKey;
                //vm.VisionEngineToggleKey = string.Empty;
                var texBox = (sender as UXTextBox);
                texBox?.Select(vm.VisionEngineToggleKey.Length, 1);
            }
        }

        private void tbVisionEngineToggle_LostFocus(object sender, RoutedEventArgs e)
        {
            if (BundleNewKeys.Count == 0 && newKeys.Count == 0)
            {
                vm.VisionEngineToggleKey = _strPreviousKey;
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
                hotkeyInfo.Job = HotkeyType.VisionEngineToggle;
                hotkeyInfo.Hotkey = BundleNewKeys.Distinct().ToList();
                hotkeyInfo.Description = "VisionEngineToggle";
                doLostFocus(hotkeyInfo, _strPreviousKey, vm.VisionEngineToggleKey, ref BundleNewKeys);
                BundleNewKeys.Clear();
            }
            //hook
            bool isHook = DdpmCommonHelper.DeviceManagerSA.Hook().Result;
        }

        private void tbVisionEngineToggle_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            e.Handled = true;
        }
    }
}