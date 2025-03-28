using DDPM.SA.Resources.Helper;
using System;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace DDPM.OSDs
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class UnMuteWin : Window
    {
        /*private DispatcherTimer? animationTimer = null;
        private TimeSpan time;*/

        private string showString = string.Empty;

        public UnMuteWin(string Content)
        {
            InitializeComponent();
            DataContext = this;
            //Robert_Lin 2025-2-5, PIMS-336458 [R19_Headset]OSD language not translate to user selecting display language
            //OLD:
            //showString = Content + " is Unmuted";
            //NEW:
            showString = Content + " " + LangHelper.Instance["is_Unmuted"];
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            System.Windows.Interop.WindowInteropHelper wndHelper = new System.Windows.Interop.WindowInteropHelper(this);
            Win32Lib.Win32.HideWinFromAltTab(wndHelper.Handle);

            if (!string.IsNullOrWhiteSpace(showString))
            {
                /*this.WindowState = WindowState.Maximized;
                this.Topmost = true;*/
                this.ShowStringText.Text = showString;
                this.Left = 1;
                this.Top = 1;
                this.Width = Screen.PrimaryScreen!.WorkingArea.Width;
                this.Height = Screen.PrimaryScreen.WorkingArea.Height;
                InvokeFadeOutAnimation();

                /*time = TimeSpan.FromMilliseconds(3000);
                animationTimer = new DispatcherTimer();
                animationTimer.Interval = TimeSpan.FromMilliseconds(1000);
                animationTimer.Tick += RunTimerTick;
                animationTimer.Start();*/
            }
            else
            {
                this.Close();
                return;
            }
        }

        /*private void RunTimerTick(object sender, EventArgs e)
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
        }*/

        private void InvokeFadeOutAnimation()
        {
            this.Dispatcher.Invoke(() =>
            {
                Storyboard? sb = Resources["FadeOut"] as Storyboard;
                if (sb == null)
                    return;

                sb.Completed += (o, s) =>
                {
                    this.Close();
                };

                sb.Begin();
            });
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

        private void Window_Closed(object sender, EventArgs e)
        {
            if (System.Windows.Threading.Dispatcher.CurrentDispatcher != null)
            {
                System.Windows.Threading.Dispatcher.CurrentDispatcher.InvokeShutdown();
            }
        }
    }
}