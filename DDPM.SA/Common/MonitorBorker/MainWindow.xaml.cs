using DDPM.ColorApp;
using DDPM.ShowOSD;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Pipes;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Windows.UI.Notifications;
using static System.Net.Mime.MediaTypeNames;
using System.DirectoryServices.ActiveDirectory;
using WinCopies.Util;
using System.Windows.Forms;
using DDPM.SA.Common;
using VcpCore.Common;

namespace DDPM.MonitorBorker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private static MonitorWin? ColorPresetWin = null;
        private ShowOSDWin? OsdWin = null;
        private IntPtr windowHandle = IntPtr.Zero;

        private IDeviceManagerSA ddmLib;// Dean 0626 SAST issue
        private MonitorInfo Mi;// Dean 0626 SAST issue

        // jim add 20240605
        private bool b_AUTO_ColorPresetConfig = true;// Dean 0626 SAST issue. change to private without static

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
        public void Set_AUTO_ColorPresetConfig(bool blAUTO)
        {
            b_AUTO_ColorPresetConfig = blAUTO;

            ColorPresetWin.Set_AUTO_ColorPresetConfig(blAUTO);
        }

        // jim add 20240620
        public void Notify_refresh_app_list()
        {
            ColorPresetWin.Notify_refresh_app_list();
        }

    }
}
