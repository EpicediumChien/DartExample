using DDPM.SA.Common;
using DDPM.SA.Resources.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace DDPM.OSDs
{
    /// <summary>
    /// Interaction logic for KeyAndKeybordBatteryLowWin.xaml
    /// </summary>
    public partial class KeyAndKeybordBatteryLowWin : Window
    {
        private DispatcherTimer? animationTimer = null;
        private TimeSpan time;
        private string showString = string.Empty;
        private OSDType oSDType;
        private OSDType_Device oSDType_Device;
        private bool oSdState = false;

        public KeyAndKeybordBatteryLowWin(string Content)
        {
            InitializeComponent();
            DataContext = this;
            showString = Content;
        }

        public KeyAndKeybordBatteryLowWin(string content, OSDType type, OSDType_Device device, bool state)
        {
            InitializeComponent();
            DataContext = this;
            showString = content;
            oSDType = type;
            oSDType_Device = device;
            oSdState = state;
        }

        public bool IsCapsLockOff
        {
            get { return oSDType == OSDType.CapsLock && !oSdState; }

        }
        public bool IsCapsLockON
        {
            get { return oSDType == OSDType.CapsLock && oSdState; }

        }

        public bool IsNumLockOff
        {
            get { return oSDType == OSDType.NumLock && !oSdState; }

        }

        public bool IsNumLockON
        {
            get { return oSDType == OSDType.NumLock && oSdState; }

        }

        public bool IsScrollLockOff
        {
            get { return oSDType == OSDType.ScrollLock && !oSdState; }

        }

        public bool IsScrollLockON
        {
            get { return oSDType == OSDType.ScrollLock && oSdState; }

        }
        public bool IsKeyboard
        {
            get { return oSDType_Device == OSDType_Device.Keyboard; }

        }

        public bool IsMouse
        {
            get { return oSDType_Device == OSDType_Device.Mouse; }

        }

        public bool IsHeadset
        {
            get { return oSDType_Device == OSDType_Device.Headset; }

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
                switch (oSDType)
                {
                    case OSDType.CapsLock:
                        KeyOSDText.Text = oSdState ? LangHelper.Instance["Caps_Lock_On"] : LangHelper.Instance["Caps_Lock_Off"];
                        break;
                    case OSDType.NumLock:
                        KeyOSDText.Text = oSdState ? LangHelper.Instance["Num_Lock_On"] : LangHelper.Instance["Num_Lock_Off"];
                        break;
                    case OSDType.ScrollLock:
                        KeyOSDText.Text = oSdState ? LangHelper.Instance["Scroll_Lock_On"] : LangHelper.Instance["Scroll_Lock_Off"];
                        break;
                }
                InvokeFadeOutAnimation();

            }
            else
            {
                this.Close();
                return;
            }


            //time = TimeSpan.FromMilliseconds(1200);
            //animationTimer = new DispatcherTimer();
            //animationTimer.Interval = TimeSpan.FromMilliseconds(100);
            //animationTimer.Tick += RunTimerTick;
            //animationTimer.Start();
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

        private void Window_Closed(object sender, EventArgs e)
        {
            if (System.Windows.Threading.Dispatcher.CurrentDispatcher != null)
            {
                System.Windows.Threading.Dispatcher.CurrentDispatcher.InvokeShutdown();
            }
        }
    }
}
