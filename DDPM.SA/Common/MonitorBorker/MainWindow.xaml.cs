using DDPM.ColorApp;
using DDPM.SA.Common;
using System;
using System.Collections.Generic;
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
        private bool b_Is_Game_DeviceName = false; // jim add 20241207

        private List<string> _supported_preset = new List<string>();

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
        public void Set_AUTO_ColorPresetConfig(bool blAUTO, bool Is_Game_DeviceName, bool blSmartHDR_ON, List<string> ColorPresetSupportList)
        {
            b_AUTO_ColorPresetConfig = blAUTO;
            b_SmartHDR_ON = blSmartHDR_ON;
            b_Is_Game_DeviceName = Is_Game_DeviceName; // jim add 20241207
            _supported_preset = ColorPresetSupportList;

            if (ColorPresetWin != null) // 20240809 jim add , 20241207 modify for The DDPM color profile can not be applied by DDPM on Smart HDR mode.(Gaming monitor ex: AW2724DM)
                ColorPresetWin.Set_AUTO_ColorPresetConfig(b_AUTO_ColorPresetConfig, b_Is_Game_DeviceName ,b_SmartHDR_ON, _supported_preset);
        }

        // jim add 20240620
        public void Notify_refresh_app_list()
        {
            if (ColorPresetWin != null) // 20240809 jim add
                ColorPresetWin.Notify_refresh_app_list();
        }

        // For PIMS-326072
        public void Set_Active_Monitor(MonitorInfo m)
        {
            if (ColorPresetWin != null) 
                ColorPresetWin.Set_Active_Monitor(m);
        }
    }
}