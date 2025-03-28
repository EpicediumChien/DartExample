using System;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace DDPM.OSDs
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class DisplayChangedWin : Window
    {
        private DispatcherTimer? animationTimer = null;
        private TimeSpan time;
        private string showString = string.Empty;

        public DisplayChangedWin(string Content)
        {
            InitializeComponent();
            DataContext = this;
            showString = Content;
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

                //time = TimeSpan.FromMilliseconds(1200);
                //animationTimer = new DispatcherTimer();
                //animationTimer.Interval = TimeSpan.FromMilliseconds(100);
                //animationTimer.Tick += RunTimerTick;
                //animationTimer.Start();
            }
            else
            {
                this.Close();
                return;
            }
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
                time = time.Add(TimeSpan.FromMilliseconds(-100));

                //if (time.TotalMilliseconds < 800)
                if (time.TotalMilliseconds < 500)
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        System.Windows.Media.Brush b = this.ShowStringText.Foreground;

                        System.Windows.Media.Color MyColor = ((SolidColorBrush)b).Color;
                        int a = (int)MyColor.A - (int)60;

                        if (a < 0)
                            a = 0;
                        MyColor.A = (byte)a;

                        SolidColorBrush brush = new SolidColorBrush(MyColor);

                        this.ShowStringText.Foreground = brush;//new SolidColorBrush(System.Windows.Media.Color.FromArgb(125, 0, 0, 255));
                    });
                }
            }
        }

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

        public void StopFadeOutAnimation()
        {
            this.Dispatcher.Invoke(() =>
            {
                Storyboard? sb = Resources["FadeOut"] as Storyboard;

                if (sb == null)
                    return;

                sb.Stop();
            });
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