using DDPM.QAM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace DDPM.OSDs
{
    /// <summary>
    /// Interaction logic for QAMHotKeyWin.xaml
    /// </summary>
    public partial class QAMHotKeyWin : Window
    {
        private DispatcherTimer? animationTimer = null;
        private TimeSpan time;


        public QAMHotKeyWin()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //System.Windows.MessageBox.Show("Window_Loaded");

            System.Windows.Interop.WindowInteropHelper wndHelper = new System.Windows.Interop.WindowInteropHelper(this);
            Win32Lib.Win32.HideWinFromAltTab(wndHelper.Handle);

            //if (!string.IsNullOrWhiteSpace(showString))
            {
                this.WindowState = WindowState.Maximized;
                this.Topmost = true;

                //Derek 1209 OSD don't need to auto close
                //time = TimeSpan.FromMilliseconds(5000);
                //animationTimer = new DispatcherTimer();
                //animationTimer.Interval = TimeSpan.FromMilliseconds(1000);
                //animationTimer.Tick += RunTimerTick;
                //animationTimer.Start();
            }
            //else
            //{
            //    this.Close();
            //    return;
            //}
        }

        private void RunTimerTick(object sender, EventArgs e)
        {
            if (time == TimeSpan.Zero)
            {
                animationTimer?.Stop();
                this.Dispatcher.Invoke(() =>
                {
                    this.Close();
                });
            }
            else
            {
                time = time.Add(TimeSpan.FromMilliseconds(-1000));
            }
        }

        private void close_Click(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }

        public void ShowWindow()
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(ShowWindow);
                return;
            }

            Show();
        }

        public void CloseWindow()
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(CloseWindow);
                return;
            }
            Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //System.Windows.MessageBox.Show("Button_MouseLeftButtonDown");
            try
            {
                DdpmCommonHelper.DeviceManagerSA!.SetIsWidgetSettingPageLoadedByQAMAsync(true);
            }
            catch (Exception)
            {
            }
        }
    }
}
