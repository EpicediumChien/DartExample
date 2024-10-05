using DDPM.ColorApp;
using DDPM.SA.Common;
using System;
using System.Windows;
using VcpCore.Common;

namespace DDPM.MonitorBorker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static MonitorWin? ColorPresetWin = null;
        //private ShowOSDWin? OsdWin = null;
        //private IntPtr windowHandle = IntPtr.Zero;

        private IDeviceManagerSA ddmLib;// Dean 0626 SAST issue
        private MonitorInfo Mi;// Dean 0626 SAST issue

        // jim add 20240605
        private bool b_AUTO_ColorPresetConfig = true;// Dean 0626 SAST issue. change to private without static

        private bool b_SmartHDR_ON= false;

        public MainWindow(IDeviceManagerSA _ddmLib, MonitorInfo m)
        {
            InitializeComponent();

            ddmLib = _ddmLib;
            Mi = m;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.ShowInTaskbar = false;

            this.Hide();

            // jim modify 20240605
            if (ColorPresetWin == null)
            {
                ColorPresetWin = new MonitorWin(ddmLib, Mi);
                ColorPresetWin.Owner = this;

                ColorPresetWin.Show();
            }
        }

        private void WindowRendered(object sender, EventArgs e)
        {
            // jim modify 20240605
            if (ColorPresetWin == null)
            {
                ColorPresetWin = new MonitorWin(ddmLib, Mi);
                ColorPresetWin.Owner = this;

                ColorPresetWin.Show();
            }
        }

        // jim add 20240605
        public void Set_AUTO_ColorPresetConfig(bool blAUTO, bool blSmartHDR_ON)
        {
            b_AUTO_ColorPresetConfig = blAUTO;
            b_SmartHDR_ON = blSmartHDR_ON;

            if (ColorPresetWin != null) // 20240809 jim add
                ColorPresetWin.Set_AUTO_ColorPresetConfig(b_AUTO_ColorPresetConfig, b_SmartHDR_ON);
        }

        // jim add 20240620
        public void Notify_refresh_app_list()
        {
            if (ColorPresetWin != null) // 20240809 jim add
                ColorPresetWin.Notify_refresh_app_list();
        }
    }
}