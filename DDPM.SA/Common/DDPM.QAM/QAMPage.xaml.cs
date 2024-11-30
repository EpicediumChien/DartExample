using DDPM.SA.Common;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF.Controls;
using Microsoft.VisualBasic.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Policy;
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

        public event EventHandler<UpdateUINotify> UpdateUINotify;

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
        private bool ShowDDPM()
        {
            const int SW_SHOWNORMALSW_NORMAL = 1;
            string processName = "DDPM";
            bool result = false;

            if (OperatingSystem.IsWindows())
            {
                try
                {
                    Process[] processes = Process.GetProcessesByName(processName);

                    if (processes.Length > 0)
                    {
                        IntPtr mainWindowHandle = processes[0].MainWindowHandle;
                        // 將窗口最大化
                        _ShowWindow(mainWindowHandle, SW_SHOWNORMALSW_NORMAL);
                        // 顯示到前景
                        _SetForegroundWindow(mainWindowHandle);

                        result = true;
                    }
                    else
                    {
                        //string ddpmExePath = @"C:\Program Files\Dell\Dell Display and Peripheral Manager\DDPM.exe";
                        string debugPath = "D:\\DDPM\\DDPM.UI\\bin\\net8.0-windows10.0.19041.0\\DDPM.exe";

                        result = DDPM.SA.Common.Settings.DDPMFileSecurity.StartProcessSafely(
                            null,
                            new ProcessStartInfo
                            {
                                FileName = debugPath,
                                UseShellExecute = true
                            });
                    }
                }
                catch
                {
                    result = false;
                }
            }

            return result;
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

            QAMPageViewModel vm = DataContext as QAMPageViewModel;

            if (vm != null && vm.CurrentDeviceInfo != null)
                vm.IsCameraSettingSelected = true;
        }

        private void CallDDPM_Click(object sender, MouseButtonEventArgs e)
        {
            //ShowDDPM();
            //SendMessageToDDPM(10);

            if (ShowDDPM())
            {
                //Info SA that DDPM launched by QAM
                DdpmCommonHelper.DeviceManagerSA!.SetIsDDPMLaunchByQAMAsync(true);

                //should closed by SA
                //Close_Click(this, null);
            }
            else
                DdpmCommonHelper.DeviceManagerSA!.SetIsDDPMLaunchByQAMAsync(false);
        }

        private void SendMessageToDDPM(int timeout)
        {
            int i = 0;

            while (true)
            {
                Thread.Sleep(1000);
                ++i;

                //wait for homepage is available
                if (2000 == DdpmCommonHelper.DeviceManagerSA!.GetCurrentPollingRate().Result)
                {
                    Thread.Sleep(2000);
                    OnUpdateUINotify($"QAMEvent_StartPreview[{i}]");

                    break;
                }

                if (i >= 10)
                    break;
            }
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

        public void OnUpdateUINotify(string msg)
        {
            UpdateUINotify e = new UpdateUINotify();
            e.UI_Field_Name = msg;

            UpdateUINotify?.Invoke(this, e);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            CloseMyself();
        }
    }
}