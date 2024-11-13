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
            HotkeyInfo hotkeyInfo = KeysHelper.getUXTextBoxHotkeyInfo(sender, e, ref newKeys, HotkeyType.ToggleEzRecentSetting);
            if (hotkeyInfo.Hotkey != null && hotkeyInfo.Hotkey.Count > 0)
            {
                if (KeysHelper.onlyContainModifyKeys(hotkeyInfo.Hotkey) || BundleNewKeys.Count == 0 && newKeys.Count == 0)
                {
                    _viewModel.RecentHotkey = _strPreviousKey;
                    BundleNewKeys.Clear();
                    var texBox = (sender as UXTextBox);
                    if (texBox == null) return;
                    texBox.Text = _viewModel.RecentHotkey;
                    texBox.Select(_viewModel.RecentHotkey.Length, 1);
                }
                else
                {
                    //for single key
                    alphabetKey = false;
                    newKeys.Clear();

                    if (KeysHelper.hotKeyConflictsCheck(hotkeyInfo))
                    {
                        tbCleanFocus.Focus();
                        EzSettingsViewModel dataContext = (EzSettingsViewModel)DataContext;
                        dataContext.IsBusy = true;
                        Task.Run(() =>
                        {
                            bool saveSettings = DdpmCommonHelper.DeviceManagerSA.SaveHotkeySetting(dataContext._homeDevice.MonitorInfo, hotkeyInfo).Result;
                            if (saveSettings)
                            {
                                DdpmCommonHelper.isHotkeyBypass = DdpmCommonHelper.DeviceManagerSA.ByPassHotkey(false).Result;
                            }
                        }).ContinueWith((t) => { dataContext.IsBusy = false; });
                    }
                    else
                    {
                        _viewModel.RecentHotkey = _strPreviousKey;
                    }

                    BundleNewKeys.Clear();

                }
            }
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
            //hook
            bool isHook = DdpmCommonHelper.DeviceManagerSA.Hook().Result;
        }

        private void tbRecentHotkey_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            e.Handled = true;
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