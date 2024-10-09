using DDPM.SA.Common.Display;
using DDPM.UI.Common;
using Dell.Client.Framework.UX.WPF.Controls;
using System.Windows.Controls;
using Windows.System;

namespace DDPM.UI.Module.EzSettings
{
    /// <summary>
    /// Interaction logic for EzSettingsRightView.xaml
    /// </summary>
    public partial class EzSettingsRightView : UserControl
    {
        #region Private members
        private const string RecentHotkeyTooltipText = "Example: \"Alt + P\",\"Shift + F\",\"Ctrl + Shift + F\"";
        private const string ApplicationWindowSnapTooltipText = "Snap any application into a split screen layout easily by dragging into a partition";
        private readonly EzSettingsViewModel _viewModel;

        private bool alphabetKey = false;
        private string _strPreviousKey = string.Empty;
        private List<VirtualKey> newKeys = new List<VirtualKey>();
        private List<VirtualKey> BundleNewKeys = new List<VirtualKey>();
        #endregion Private members

        #region ctor
        public EzSettingsRightView(EzSettingsViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
            _viewModel = vm;

            hotkeyRecentTooltipText.Text = RecentHotkeyTooltipText;
            applicationWindowSnapTooltipText.Text = ApplicationWindowSnapTooltipText;
        }
        #endregion ctor

        #region Recent Hotkey
        private void tbRecentHotkey_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            KeysHelper.setUXTextBoxPreviewKey(sender, e, ref newKeys, ref BundleNewKeys, ref alphabetKey);
        }

        private void tbRecentHotkey_PreviewKeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            e.Handled = true;
        }

        private void tbRecentHotkey_GotFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            bool isUnhook = DdpmCommonHelper.DeviceManagerSA.UnHook().Result;
            if (isUnhook)
            {
                alphabetKey = false;
                newKeys.Clear();
                //_strTbToggleInputSourcePreviousKey = vm.ToggleInputSourceKey;
                _strPreviousKey = _viewModel.RecentHotkey;
                //vm.ToggleInputSourceKey = string.Empty;
                var texBox = (sender as UXTextBox);
                texBox?.Select(_viewModel.RecentHotkey.Length, 1);
            }
        }

        private void tbRecentHotkey_LostFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            if (BundleNewKeys.Count == 0 && newKeys.Count == 0)
            {
                //vm.ToggleInputSourceKey = _strTbToggleInputSourcePreviousKey;
                _viewModel.RecentHotkey = _strPreviousKey;
                BundleNewKeys.Clear();
            }
            else
            {
                //for single key
                alphabetKey = false;
                newKeys.Clear();

                //save hotkey
                HotkeyInfo hotkeyInfo = new HotkeyInfo();
                hotkeyInfo.Job = HotkeyType.ToggleEzRecentSetting;
                hotkeyInfo.Hotkey = BundleNewKeys.Distinct().ToList();
                hotkeyInfo.Description = "ToggleEzRecentSetting";
                doLostFocus(hotkeyInfo, _strPreviousKey, _viewModel.RecentHotkey, ref BundleNewKeys);
                BundleNewKeys.Clear();
            }
            //hook
            bool isHook = DdpmCommonHelper.DeviceManagerSA.Hook().Result;
        }

        private void tbRecentHotkey_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            e.Handled = true;
        }
        private void doLostFocus(HotkeyInfo hotkeyInfo, string prStr, string crStr, ref List<VirtualKey> keys)
        {
            if (KeysHelper.hotKeyConflictsCheck(hotkeyInfo))
            {
                //save hotkey
                // SaveHotkeysSetting(_strTbBrightnessMinsPreviousKey, vm.BrightnessMinsKey, HotkeyType.BrightnessReduce, ref BrightnessMinsNewKeys, "Brightness-");
                bool saveSettings = DdpmCommonHelper.DeviceManagerSA.SaveHotkeySetting(_viewModel._homeDevice.MonitorInfo, hotkeyInfo).Result;
                _viewModel.RefreshSettings();
            }
            else
            {
                switch (hotkeyInfo.Job)
                {
                    case HotkeyType.ToggleInputSource:
                        _viewModel.RecentHotkey = prStr;
                        break;
                }
            }
            keys.Clear();
        }
        #endregion Recent Hotkey

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (_viewModel != null)
            {
                _viewModel.RefreshSettings();
            }
        }
    }
}