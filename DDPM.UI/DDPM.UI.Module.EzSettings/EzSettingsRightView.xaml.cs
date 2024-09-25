using System.Windows.Controls;

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

        }

        private void tbRecentHotkey_PreviewKeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {

        }

        private void tbRecentHotkey_GotFocus(object sender, System.Windows.RoutedEventArgs e)
        {

        }

        private void tbRecentHotkey_LostFocus(object sender, System.Windows.RoutedEventArgs e)
        {

        }

        private void tbRecentHotkey_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {

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