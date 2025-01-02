using System.Windows;
using System.Windows.Threading;
using DDPM.UI.Plugin.ViewModels;

namespace DDPM.UI.Plugin.Common
{
    /// <summary>
    /// WaitingModalDialog.xaml 的互動邏輯
    /// </summary>
    public partial class WaitingModalDialog : Window
    {
        AddDeviceViewModel _vm;
        public WaitingModalDialog(string caption, string message, string alert, AddDeviceViewModel vm)
        {
            InitializeComponent();
            _vm = vm;
            txtCaption.Text = caption;
            txtMessage.Text = message;
            txtAlert.Text = alert;

            var timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(2.5)
            };
            timer.Tick += Timer_Tick;
            //timer.Start();
            Loaded += WaitingModalDialog_Loaded;
            Unloaded += WaitingModalDialog_Unloaded;
        }

        private void WaitingModalDialog_Unloaded(object sender, RoutedEventArgs e)
        {
            _vm.IsPairingLoaded = false;
        }

        private void WaitingModalDialog_Loaded(object sender, RoutedEventArgs e)
        {
            _vm.IsPairingLoaded = true;
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            this.Close();
        }
        public void CloseByCaller()
        {
            Close();
        }
    }
}