using System.Windows;

namespace DDPM.EABroker
{
    /// <summary>
    /// Interaction logic for ScreenIdWindow.xaml
    /// </summary>
    public partial class ScreenIdWindow : Window
    {
        private int _screenId;
        private Screen _screen;
        private ArrangeVM _vm;

        public ScreenIdWindow(int screenId, Screen scr, ArrangeVM vm)
        {
            InitializeComponent();
            _screenId = screenId;
            _screen = scr;
            _vm = vm;
            DataContext = _vm;

            _vm.RefreshScreenScale();
            Left = scr.WorkingArea.Left / _vm.ScreenScale;
            Top = scr.WorkingArea.Top / _vm.ScreenScale;
            Width = scr.WorkingArea.Width / _vm.ScreenScale;
            Height = scr.WorkingArea.Height / _vm.ScreenScale;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            idText.Text=_screenId.ToString();

            //Hide window from Alt+tab
            System.Windows.Interop.WindowInteropHelper wndHelper = new System.Windows.Interop.WindowInteropHelper(this);
            Win32Lib.Win32.HideWinFromAltTab(wndHelper.Handle);
        }

        public int GetScreenId()
        {
            return _screenId;

        }

        public void SetActive(bool isActive)
        {

        }
    }
}
