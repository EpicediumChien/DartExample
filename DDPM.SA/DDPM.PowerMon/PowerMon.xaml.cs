using Dell.Client.Framework.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DDPM.PowerMon
{
    /// <summary>
    /// Interaction logic for PowerMon.xaml
    /// </summary>
    public partial class PowerMonitor : Window
    {
        private enum log_type
        {
            info = 0,
            error
        }
        private void WriteLog(string text, log_type log_type = log_type.info)
        {
            if (string.IsNullOrEmpty(text))
                text = "";

            Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss.fff")} [PowerMonitor] {text}");
            if (log != null)
            {
                if (log_type == log_type.info)
                    log.Info(text);
                else
                    log.Error(text);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct POWERBROADCAST_SETTING
        {
            public Guid PowerSetting;
            public uint DataLength;
            public byte Data;
        }

        private static Guid GUID_MONITOR_POWER_ON = new Guid("02731015-4510-4526-99E6-E5A17EBD1AEA");

        private IntPtr m_hPowerNotify = IntPtr.Zero;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr RegisterPowerSettingNotification(IntPtr hRecipient, ref Guid PowerSettingGuid, uint Flags);
        private static IntPtr _RegisterPowerSettingNotification(IntPtr hRecipient, ref Guid PowerSettingGuid, uint Flags)
        {
            return RegisterPowerSettingNotification(hRecipient, ref PowerSettingGuid, Flags);
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UnregisterPowerSettingNotification(IntPtr Handle);
        private static bool _UnregisterPowerSettingNotification(IntPtr Handle)
        {
            return UnregisterPowerSettingNotification(Handle);
        }

        public event EventHandler MonitorTurnedOn;
        //public event EventHandler MonitorTurnedOff;
        //public event EventHandler PowerSettingChanged;
        private static ILog log = null;

        public PowerMonitor(ILog Log)
        {
            InitializeComponent();
            log = Log;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            WindowInteropHelper helper = new WindowInteropHelper(this);
            HwndSource source = HwndSource.FromHwnd(helper.Handle);
            source.AddHook(WndProc);

            m_hPowerNotify = _RegisterPowerSettingNotification(helper.Handle, ref GUID_MONITOR_POWER_ON, 0);
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            if (m_hPowerNotify != IntPtr.Zero)
            {
                _UnregisterPowerSettingNotification(m_hPowerNotify);
                m_hPowerNotify = IntPtr.Zero;
            }
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_POWERBROADCAST = 0x0218;
            //const int PBT_APMRESUMEAUTOMATIC = 0x12; // Resume Automatic
            //const int PBT_APMSUSPEND = 0x04; // Suspend
            const int PBT_POWERSETTINGCHANGE = 0x8013;

            //WriteLog($"Power event {msg.ToString()}");
            switch (msg)
            {
                case WM_POWERBROADCAST:
                    /*if (wParam.ToInt32() == PBT_APMRESUMEAUTOMATIC)
                    {
                        MonitorTurnedOn?.Invoke(this, EventArgs.Empty);
                        handled = true;
                        //WriteLog($"Power event {msg.ToString()} PBT_APMRESUMEAUTOMATIC handled");
                    }
                    else if (wParam.ToInt32() == PBT_APMSUSPEND)
                    {
                        MonitorTurnedOff?.Invoke(this, EventArgs.Empty);
                        handled = true;
                        //WriteLog($"Power event {msg.ToString()} PBT_APMRESUMEAUTOMATIC handled");
                    }
                    else*/ 
                    if (wParam.ToInt32() == PBT_POWERSETTINGCHANGE)
                    {
                        POWERBROADCAST_SETTING pPwrSetting = (POWERBROADCAST_SETTING)Marshal.PtrToStructure(lParam, typeof(POWERBROADCAST_SETTING));
                        if (pPwrSetting.PowerSetting == GUID_MONITOR_POWER_ON && pPwrSetting.DataLength == sizeof(uint))
                        {
                            WriteLog($"Power event {msg.ToString()} PBT_POWERSETTINGCHANGE handled");
                            WriteLog($"Power event PBT_POWERSETTINGCHANGE data: {pPwrSetting.Data}");
                            if (pPwrSetting.Data == 0)
                            {
                                //MonitorTurnedOff?.Invoke(this, EventArgs.Empty);
                            }
                            else if (pPwrSetting.Data == 1)
                            {
                                MonitorTurnedOn?.Invoke(this, EventArgs.Empty);
                            }
                        }
                        //PowerSettingChanged?.Invoke(this, EventArgs.Empty);
                        handled = true;
                    }
                    break;
                default:
                    break;
            }
            return IntPtr.Zero;
        }
    }
}
