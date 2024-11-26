using DDPM.SA.Common;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
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

namespace DDPM.QAM
{
    /// <summary>
    /// Interaction logic for QAM.xaml
    /// </summary>
    public partial class QAMPage : Window
    {
        CameraSetting CameraSetting;

        [DllImport("user32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private static bool _ShowWindow(IntPtr hWnd, int nCmdShow)
        {
            return ShowWindow(hWnd, nCmdShow);
        }
        [DllImport("user32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        private static bool _SetForegroundWindow(IntPtr hWnd)
        {
            return SetForegroundWindow(hWnd);
        }
        /// <summary>
        /// NotificationFWupdate 呼叫DDPM UI事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShowDDPM()
        {
            const int SW_SHOWNORMALSW_NORMAL = 1;
            string processName = "DDPM";

            if (OperatingSystem.IsWindows())
            {
                Process[] processes = Process.GetProcessesByName(processName);
                if (processes.Length > 0)
                {
                    IntPtr mainWindowHandle = processes[0].MainWindowHandle;
                    // 將窗口最大化
                    _ShowWindow(mainWindowHandle, SW_SHOWNORMALSW_NORMAL);
                    // 顯示到前景
                    _SetForegroundWindow(mainWindowHandle);
                }
            }
        }
        public QAMPage(IDeviceManagerSA deviceMangerPlugin)
        {
            InitializeComponent();
            DdpmCommonHelper.DeviceManagerSA = deviceMangerPlugin;
            DdpmCommonHelper.QAMPageViewModel = new QAMPageViewModel();
            DataContext = DdpmCommonHelper.QAMPageViewModel;

            Microsoft.Win32.SystemEvents.SessionSwitch += SystemEvents_SessionSwitch;
        }

        private void SystemEvents_SessionSwitch(object sender, Microsoft.Win32.SessionSwitchEventArgs e)
        {
            if (e.Reason == Microsoft.Win32.SessionSwitchReason.SessionLock)
            {
                Microsoft.Win32.SystemEvents.SessionSwitch -= SystemEvents_SessionSwitch;

                CloseMyself();
            }
            //else if (e.Reason == Microsoft.Win32.SessionSwitchReason.SessionUnlock)
            //{
            //}
        }

        private void Close_Click(object sender, MouseButtonEventArgs e)
        {
            CloseMyself();
        }

        private void CloseMyself()
        {
            //System.Windows.MessageBox.Show("CloseMyself");

            if (CameraSetting != null)
            {
                CameraSetting.Close();
                CameraSetting = null;
            }

            Dispatcher.Invoke(() =>
            {
                // Access UI elements or objects owned by a different thread
                this.Close();
            });
        }

        private void CameraSetting_Click(object sender, MouseButtonEventArgs e)
        {
            if (CameraSetting != null)
            {
                CameraSetting.Close();
                CameraSetting = null;
            }
            else
            {
                CameraSetting = new CameraSetting();
                CameraSetting.Left = this.Left + this.Width;
                CameraSetting.Top = this.Top;
                CameraSetting.Width = 288;
                CameraSetting.Height = 128;
                CameraSetting.Show();
            }
        }

        private void CallDDPM_Click(object sender, MouseButtonEventArgs e)
        {
            ShowDDPM();
            Close_Click(this, null);
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
                if (CameraSetting != null)
                {
                    CameraSetting.Left = this.Left + this.Width;
                    CameraSetting.Top = this.Top;
                }
            }
        }
    }
}