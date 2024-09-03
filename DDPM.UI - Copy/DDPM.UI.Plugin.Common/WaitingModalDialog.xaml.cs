using System.Windows;
using System.Windows.Threading;

namespace DDPM.UI.Plugin.Common
{
    /// <summary>
    /// WaitingModalDialog.xaml 的互動邏輯
    /// </summary>
    public partial class WaitingModalDialog : Window
    {
        public WaitingModalDialog(string caption, string message, string alert)
        {
            InitializeComponent();

            txtCaption.Text = caption;
            txtMessage.Text = message;
            txtAlert.Text = alert;

            var timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(2.5)
            };
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            this.Close();
        }
    }
}