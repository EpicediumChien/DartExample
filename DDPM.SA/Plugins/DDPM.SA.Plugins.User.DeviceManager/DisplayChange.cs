using Dell.Client.Framework.Common;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using VcpCore.Common;
using Application = System.Windows.Forms.Application;

namespace DDPM.SA.Plugins.User.DeviceManager
{
    internal class DisplayChange
    {
        private static Logs _logs;

        public event EventHandler DisplayChange_Event;

        private const int WM_DISPLAYCHANGE = 0x001A;
        private const int WM_SETTINGCHANGE = 0x007E;
        private const int WM_DEVICECHANGE = 0x0219;

        // Windows API Imports
        [DllImport("user32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern IntPtr RegisterDeviceNotification(IntPtr hRecipient, IntPtr NotificationFilter, uint Flags);

        private static IntPtr _RegisterDeviceNotification(IntPtr hRecipient, IntPtr NotificationFilter, uint Flags)
        {
            return RegisterDeviceNotification(hRecipient, NotificationFilter, Flags);
        }

        [DllImport("user32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern bool UnregisterDeviceNotification(IntPtr Handle);

        private static bool _UnregisterDeviceNotification(IntPtr Handle)
        {
            return UnregisterDeviceNotification(Handle);
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct DEV_BROADCAST_DEVICEINTERFACE
        {
            public uint dbcc_size;
            public uint dbcc_devicetype;
            public uint dbcc_reserved;
            public Guid dbcc_classguid;
            public ushort dbcc_name;
        }

        public DisplayChange(ILog log)
        {
            _logs ??= new Logs(log, "DisplayChange");
        }

        public void Initialize_DisplayChangeEvent()
        {
            NotificationForm notificationForm = new NotificationForm();
            notificationForm.DisplayChange_Event += DisplayChangeEvent;
            Application.Run(notificationForm);
        }

        private void DisplayChangeEvent(object o, EventArgs e)
        {
            DisplayChange_Event?.Invoke(o, e);
        }

        private class NotificationForm : Form
        {
            public event EventHandler DisplayChange_Event;

            private IntPtr _notificationHandle;
            private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
            private bool displayInOut = false;

            public NotificationForm()
            {
                // Register for device notifications
                var dbi = new DEV_BROADCAST_DEVICEINTERFACE
                {
                    dbcc_size = (uint)Marshal.SizeOf(typeof(DEV_BROADCAST_DEVICEINTERFACE)),
                    dbcc_devicetype = 0x00000005, // DBT_DEVTYP_DEVICEINTERFACE
                    dbcc_classguid = Guid.Empty
                };

                IntPtr filter = Marshal.AllocHGlobal(Marshal.SizeOf(dbi));
                Marshal.StructureToPtr(dbi, filter, false);

                _notificationHandle = _RegisterDeviceNotification(this.Handle, filter, 0);

                if (_notificationHandle == IntPtr.Zero)
                {
                    _logs.DebugMsg_1("Failed to register for device notifications.");
                }

                Marshal.FreeHGlobal(filter);

                // Set up a message loop
                this.Load += (sender, e) =>
                {
                    this.Visible = false;
                    this.ShowInTaskbar = false;
                    this.WindowState = FormWindowState.Minimized;
                };
            }

            protected override void WndProc(ref Message m)
            {
                base.WndProc(ref m);
                switch (m.Msg)
                {
                    case WM_DISPLAYCHANGE:
                        _logs.DebugMsg_1($"WM_DISPLAYCHANGE");
                        lock (this)
                        {
                            displayInOut = false;
                            // Cancel any previous delay task
                            _cancellationTokenSource.Cancel();
                            _cancellationTokenSource.Dispose();
                            _cancellationTokenSource = new CancellationTokenSource();
                            var token = _cancellationTokenSource.Token;

                            // Process display change with a delay if no immediate state change
                            _ = Task.Factory.StartNew(async () =>
                            {
                                var delayTask = Task.Delay(TimeSpan.FromSeconds(10), token); // Wait up to 10 seconds

                                while (!token.IsCancellationRequested)
                                {
                                    if (displayInOut)
                                    {
                                        DisplayChange_Event?.Invoke(this, new EventArgs());
                                        return;
                                    }

                                    // Wait for a short interval before checking again
                                    var delayRemaining = TimeSpan.FromSeconds(0.5); // Check every 0.5 seconds
                                    if (await Task.WhenAny(delayTask, Task.Delay(delayRemaining, token)) == delayTask)
                                    {
                                        // The delayTask has completed, no need to continue
                                        break;
                                    }
                                }

                                // After exiting the loop, check if displayInOut is still false
                                if (!displayInOut)
                                {
                                    Console.WriteLine($"No change detected within 10 seconds {DateTime.Now}");
                                }
                            }, token);
                            break;
                        }
                    case WM_SETTINGCHANGE:
                        _logs.DebugMsg_1($"WM_SETTINGHANGE");
                        break;

                    case WM_DEVICECHANGE:
                        _logs.DebugMsg_1($"WM_DEVICECHANGE");
                        displayInOut = true;
                        break;
                }
            }

            protected override void OnFormClosed(FormClosedEventArgs e)
            {
                // Unregister device notifications
                if (_notificationHandle != IntPtr.Zero)
                {
                    _UnregisterDeviceNotification(_notificationHandle);
                }

                base.OnFormClosed(e);
            }
        }
    }
}