using Microsoft.Win32;
using System.ComponentModel;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Windows.Threading;
using static System.Windows.Forms.Design.AxImporter;
//using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Reflection;
using Dell.Client.Framework.Common;
using System.Diagnostics;
namespace DDPM.SA.Common
{
    /// Monitor NightLight on/off if changed
    public class RegistryMonitor_NightLight : IDisposable
    {
        #region P/Invoke

        [DllImport("advapi32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern int RegOpenKeyEx(IntPtr hKey, string subKey, uint options, int samDesired,
                                               out IntPtr phkResult);
        private static int _RegOpenKeyEx(IntPtr hKey, string subKey, uint options, int samDesired,
                                               out IntPtr phkResult)
        {
            int rst = RegOpenKeyEx(hKey, subKey, options, samDesired, out phkResult);

            if (rst != 0)
            {
#if DEBUG
                Console.WriteLine("[RegistryMonitor_NightLight] RegOpenKeyEx failed.");
#endif
            }

            return rst;
        }

        [DllImport("advapi32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern int RegNotifyChangeKeyValue(IntPtr hKey, bool bWatchSubtree,
                                                          RegChangeNotifyFilter dwNotifyFilter, IntPtr hEvent,
                                                          bool fAsynchronous);
        private static int _RegNotifyChangeKeyValue(IntPtr hKey, bool bWatchSubtree,
                                                  RegChangeNotifyFilter dwNotifyFilter, IntPtr hEvent,
                                                  bool fAsynchronous)
        {
            int rst = RegNotifyChangeKeyValue(hKey, bWatchSubtree, dwNotifyFilter, hEvent, fAsynchronous);

            if (rst != 0)
            {
#if DEBUG
                Console.WriteLine("[RegistryMonitor_NightLight] RegNotifyChangeKeyValue failed.");
#endif
            }

            return rst;
        }

        [DllImport("advapi32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern int RegCloseKey(IntPtr hKey);
        private static int _RegCloseKey(IntPtr hKey)
        {
            int rst = RegCloseKey(hKey);

            if (rst != 0)
            {
#if DEBUG
                Console.WriteLine("[RegistryMonitor_NightLight] RegCloseKey failed.");
#endif
            }

            return rst;
        }

        private const int KEY_QUERY_VALUE = 0x0001;
        private const int KEY_NOTIFY = 0x0010;
        private const int STANDARD_RIGHTS_READ = 0x00020000;

        private static readonly IntPtr HKEY_CLASSES_ROOT = new IntPtr(unchecked((int)0x80000000));
        private static readonly IntPtr HKEY_CURRENT_USER = new IntPtr(unchecked((int)0x80000001));
        private static readonly IntPtr HKEY_LOCAL_MACHINE = new IntPtr(unchecked((int)0x80000002));
        private static readonly IntPtr HKEY_USERS = new IntPtr(unchecked((int)0x80000003));
        private static readonly IntPtr HKEY_PERFORMANCE_DATA = new IntPtr(unchecked((int)0x80000004));
        private static readonly IntPtr HKEY_CURRENT_CONFIG = new IntPtr(unchecked((int)0x80000005));
        private static readonly IntPtr HKEY_DYN_DATA = new IntPtr(unchecked((int)0x80000006));

        #endregion P/Invoke

        #region Event handling

        public event EventHandler? RegChanged;  //SDL, add ? to syncup definitions

        protected virtual void OnRegChanged_NightLight()
        {
            EventHandler handler = RegChanged;
            if (handler != null)
                handler(this, null);
        }

        public event ErrorEventHandler? Error;

        protected virtual void OnError_NightLight(Exception e)
        {
            ErrorEventHandler handler = Error;
            if (handler != null)
                handler(this, new ErrorEventArgs(e));
        }

        #endregion Event handling

        #region Private member variables

        private IntPtr _registryHive;
        private string? _registrySubName;//SDL, add ? to syncup definitions
        private object _threadLock = new object();
        private Thread? _thread;//SDL, add ? to syncup definitions
        private Dispatcher dispatcher = Dispatcher.CurrentDispatcher;
        private bool _disposed = false;
        private ManualResetEvent _eventTerminate = new ManualResetEvent(false);

        private RegChangeNotifyFilter _regFilter = RegChangeNotifyFilter.Key | RegChangeNotifyFilter.Attribute |
                                                   RegChangeNotifyFilter.Value | RegChangeNotifyFilter.Security;

        #endregion Private member variables

        public RegistryMonitor_NightLight(RegistryKey registryKey)
        {
            InitRegistryKey(registryKey.Name);
        }

        public RegistryMonitor_NightLight(string name)
        {
            if (name == null || name.Length == 0)
                throw new ArgumentNullException("name");

            InitRegistryKey(name);
        }

        public RegistryMonitor_NightLight(RegistryHive registryHive, string subKey)
        {
            InitRegistryKey(registryHive, subKey);
        }

        public void Dispose()
        {
            Stop();
            _disposed = true;
            GC.SuppressFinalize(this);
        }

        public RegChangeNotifyFilter RegChangeNotifyFilter
        {
            get { return _regFilter; }
            set
            {
                lock (_threadLock)
                {
                    if (IsMonitoring)
                        throw new InvalidOperationException("Monitoring thread is already running");

                    _regFilter = value;
                }
            }
        }

        #region Initialization

        private void InitRegistryKey(RegistryHive hive, string name)
        {
            switch (hive)
            {
                case RegistryHive.ClassesRoot:
                    _registryHive = HKEY_CLASSES_ROOT;
                    break;

                case RegistryHive.CurrentConfig:
                    _registryHive = HKEY_CURRENT_CONFIG;
                    break;

                case RegistryHive.CurrentUser:
                    _registryHive = HKEY_CURRENT_USER;
                    break;

                case RegistryHive.LocalMachine:
                    _registryHive = HKEY_LOCAL_MACHINE;
                    break;

                case RegistryHive.PerformanceData:
                    _registryHive = HKEY_PERFORMANCE_DATA;
                    break;

                case RegistryHive.Users:
                    _registryHive = HKEY_USERS;
                    break;

                default:
                    throw new InvalidEnumArgumentException("hive", (int)hive, typeof(RegistryHive));
            }
            _registrySubName = name;
        }

        private void InitRegistryKey(string name)
        {
            string[] nameParts = name.Split('\\');

            switch (nameParts[0])
            {
                case "HKEY_CLASSES_ROOT":
                case "HKCR":
                    _registryHive = HKEY_CLASSES_ROOT;
                    break;

                case "HKEY_CURRENT_USER":
                case "HKCU":
                    _registryHive = HKEY_CURRENT_USER;
                    break;

                case "HKEY_LOCAL_MACHINE":
                case "HKLM":
                    _registryHive = HKEY_LOCAL_MACHINE;
                    break;

                case "HKEY_USERS":
                    _registryHive = HKEY_USERS;
                    break;

                case "HKEY_CURRENT_CONFIG":
                    _registryHive = HKEY_CURRENT_CONFIG;
                    break;

                default:
                    _registryHive = IntPtr.Zero;
                    throw new ArgumentException("The registry hive '" + nameParts[0] + "' is not supported", "value");
            }

            _registrySubName = String.Join("\\", nameParts, 1, nameParts.Length - 1);
        }

        #endregion Initialization

        public bool IsMonitoring
        {
            get { return _thread != null; }
        }

        public void Start()
        {
            if (_disposed)
                throw new ObjectDisposedException(null, "This instance is already disposed");

            lock (_threadLock)
            {
                if (!IsMonitoring)
                {
                    _eventTerminate.Reset();
                    _thread = new Thread(new ThreadStart(MonitorThread));
                    _thread.IsBackground = true;

                    //dispatcher.BeginInvoke((Action)delegate ()
                    //{
                    Task.Run(() =>
                    {
                        _thread.Start();
                    });
                    //});
                }
            }
        }

        public void Stop()
        {
            if (_disposed)
                throw new ObjectDisposedException(null, "This instance is already disposed");

            lock (_threadLock)
            {
                Thread thread = _thread;
                if (thread != null)
                {
                    _eventTerminate.Set();
                    thread.Join();
                }
            }
        }

        private void MonitorThread()
        {
            try
            {
                ThreadLoop();
            }
            catch (Exception e)
            {
                OnError_NightLight(e);
            }
            _thread = null;
        }

        private void ThreadLoop()
        {
            IntPtr registryKey;
            int result = _RegOpenKeyEx(_registryHive, _registrySubName, 0, STANDARD_RIGHTS_READ | KEY_QUERY_VALUE | KEY_NOTIFY,
                                      out registryKey);
            if (result != 0)
                throw new Win32Exception(result);

            try
            {
                AutoResetEvent _eventNotify = new AutoResetEvent(false);
                WaitHandle[] waitHandles = new WaitHandle[] { _eventNotify, _eventTerminate };
                while (!_eventTerminate.WaitOne(0, true))
                {
                    result = _RegNotifyChangeKeyValue(registryKey, true, _regFilter, _eventNotify.Handle, true);
                    if (result != 0)
                        throw new Win32Exception(result);

                    if (WaitHandle.WaitAny(waitHandles) == 0)
                    {
                        OnRegChanged_NightLight();
                    }
                }
            }
            finally
            {
                if (registryKey != IntPtr.Zero)
                {
                    _RegCloseKey(registryKey);
                }
            }
        }
    }

    [Flags]
    public enum RegChangeNotifyFilter
    {
        Key = 1,
        Attribute = 2,
        Value = 4,
        Security = 8,
    }

    /// Monitor Default ICC Profile if changed
    public class RegistryMonitor_ICC : IDisposable
    {
        #region P/Invoke

        [DllImport("advapi32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern int RegOpenKeyEx(IntPtr hKey, string subKey, uint options, int samDesired,
                                               out IntPtr phkResult);
        private static int _RegOpenKeyEx(IntPtr hKey, string subKey, uint options, int samDesired,
                                               out IntPtr phkResult)
        {
            int rst = RegOpenKeyEx(hKey, subKey, options, samDesired, out phkResult);

            if (rst != 0)
            {
#if DEBUG
                Console.WriteLine("[RegistryMonitor_NightLight] RegOpenKeyEx failed.");
#endif
            }

            return rst;
        }

        [DllImport("advapi32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern int RegNotifyChangeKeyValue(IntPtr hKey, bool bWatchSubtree,
                                                          RegChangeNotifyFilter dwNotifyFilter, IntPtr hEvent,
                                                          bool fAsynchronous);
        private static int _RegNotifyChangeKeyValue(IntPtr hKey, bool bWatchSubtree,
                                                          RegChangeNotifyFilter dwNotifyFilter, IntPtr hEvent,
                                                          bool fAsynchronous)
        {
            int rst = RegNotifyChangeKeyValue(hKey, bWatchSubtree, dwNotifyFilter, hEvent, fAsynchronous);

            if (rst != 0)
            {
#if DEBUG
                Console.WriteLine("[RegistryMonitor_NightLight] RegNotifyChangeKeyValue failed.");
#endif
            }

            return rst;
        }

        [DllImport("advapi32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern int RegCloseKey(IntPtr hKey);
        private static int _RegCloseKey(IntPtr hKey)
        {
            int rst = RegCloseKey(hKey);

            if (rst != 0)
            {
#if DEBUG
                Console.WriteLine("[RegistryMonitor_NightLight] RegCloseKey failed.");
#endif
            }

            return rst;
        }

        private const int KEY_QUERY_VALUE = 0x0001;
        private const int KEY_NOTIFY = 0x0010;
        private const int STANDARD_RIGHTS_READ = 0x00020000;

        private static readonly IntPtr HKEY_CLASSES_ROOT = new IntPtr(unchecked((int)0x80000000));
        private static readonly IntPtr HKEY_CURRENT_USER = new IntPtr(unchecked((int)0x80000001));
        private static readonly IntPtr HKEY_LOCAL_MACHINE = new IntPtr(unchecked((int)0x80000002));
        private static readonly IntPtr HKEY_USERS = new IntPtr(unchecked((int)0x80000003));
        private static readonly IntPtr HKEY_PERFORMANCE_DATA = new IntPtr(unchecked((int)0x80000004));
        private static readonly IntPtr HKEY_CURRENT_CONFIG = new IntPtr(unchecked((int)0x80000005));
        private static readonly IntPtr HKEY_DYN_DATA = new IntPtr(unchecked((int)0x80000006));

        #endregion P/Invoke

        #region Event handling

        public event EventHandler? RegChanged;//SDL, add ? to syncup definitions

        protected virtual void OnRegChanged_ICC()
        {
            EventHandler handler = RegChanged;
            if (handler != null)
                handler(this, null);
        }

        public event ErrorEventHandler? Error;//SDL, add ? to syncup definitions

        protected virtual void OnError_ICC(Exception e)
        {
            ErrorEventHandler handler = Error;
            if (handler != null)
                handler(this, new ErrorEventArgs(e));
        }

        #endregion Event handling

        #region Private member variables

        private IntPtr _registryHive;
        private string? _registrySubName;//SDL, add ? to syncup definitions
        private object _threadLock = new object();
        private Thread? _thread;//SDL, add ? to syncup definitions
        private Dispatcher dispatcher = Dispatcher.CurrentDispatcher;
        private bool _disposed = false;
        private ManualResetEvent _eventTerminate = new ManualResetEvent(false);

        private RegChangeNotifyFilter _regFilter = RegChangeNotifyFilter.Key | RegChangeNotifyFilter.Attribute |
                                                   RegChangeNotifyFilter.Value | RegChangeNotifyFilter.Security;

        #endregion Private member variables

        public RegistryMonitor_ICC(RegistryKey registryKey)
        {
            InitRegistryKey(registryKey.Name);
        }

        public RegistryMonitor_ICC(string name)
        {
            if (name == null || name.Length == 0)
                throw new ArgumentNullException("name");

            InitRegistryKey(name);
        }

        public RegistryMonitor_ICC(RegistryHive registryHive, string subKey)
        {
            InitRegistryKey(registryHive, subKey);
        }

        public void Dispose()
        {
            Stop();
            _disposed = true;
            GC.SuppressFinalize(this);
        }

        public RegChangeNotifyFilter RegChangeNotifyFilter
        {
            get { return _regFilter; }
            set
            {
                lock (_threadLock)
                {
                    if (IsMonitoring)
                        throw new InvalidOperationException("Monitoring thread is already running");

                    _regFilter = value;
                }
            }
        }

        #region Initialization

        private void InitRegistryKey(RegistryHive hive, string name)
        {
            switch (hive)
            {
                case RegistryHive.ClassesRoot:
                    _registryHive = HKEY_CLASSES_ROOT;
                    break;

                case RegistryHive.CurrentConfig:
                    _registryHive = HKEY_CURRENT_CONFIG;
                    break;

                case RegistryHive.CurrentUser:
                    _registryHive = HKEY_CURRENT_USER;
                    break;

                case RegistryHive.LocalMachine:
                    _registryHive = HKEY_LOCAL_MACHINE;
                    break;

                case RegistryHive.PerformanceData:
                    _registryHive = HKEY_PERFORMANCE_DATA;
                    break;

                case RegistryHive.Users:
                    _registryHive = HKEY_USERS;
                    break;

                default:
                    throw new InvalidEnumArgumentException("hive", (int)hive, typeof(RegistryHive));
            }
            _registrySubName = name;
        }

        private void InitRegistryKey(string name)
        {
            string[] nameParts = name.Split('\\');

            switch (nameParts[0])
            {
                case "HKEY_CLASSES_ROOT":
                case "HKCR":
                    _registryHive = HKEY_CLASSES_ROOT;
                    break;

                case "HKEY_CURRENT_USER":
                case "HKCU":
                    _registryHive = HKEY_CURRENT_USER;
                    break;

                case "HKEY_LOCAL_MACHINE":
                case "HKLM":
                    _registryHive = HKEY_LOCAL_MACHINE;
                    break;

                case "HKEY_USERS":
                    _registryHive = HKEY_USERS;
                    break;

                case "HKEY_CURRENT_CONFIG":
                    _registryHive = HKEY_CURRENT_CONFIG;
                    break;

                default:
                    _registryHive = IntPtr.Zero;
                    throw new ArgumentException("The registry hive '" + nameParts[0] + "' is not supported", "value");
            }

            _registrySubName = String.Join("\\", nameParts, 1, nameParts.Length - 1);
        }

        #endregion Initialization

        public bool IsMonitoring
        {
            get { return _thread != null; }
        }

        public void Start()
        {
            if (_disposed)
                throw new ObjectDisposedException(null, "This instance is already disposed");

            lock (_threadLock)
            {
                if (!IsMonitoring)
                {
                    _eventTerminate.Reset();
                    _thread = new Thread(new ThreadStart(MonitorThread));
                    _thread.IsBackground = true;

                    //dispatcher.BeginInvoke((Action)delegate ()
                    //{

                    Task.Run(() =>
                    {
                        _thread.Start();
                    });

                    //});
                }
            }
        }

        public void Stop()
        {
            if (_disposed)
                throw new ObjectDisposedException(null, "This instance is already disposed");

            lock (_threadLock)
            {
                Thread thread = _thread;
                if (thread != null)
                {
                    _eventTerminate.Set();
                    thread.Join();
                }
            }
        }

        private void MonitorThread()
        {
            try
            {
                ThreadLoop();
            }
            catch (Exception e)
            {
                OnError_ICC(e);
            }
            _thread = null;
        }

        private void ThreadLoop()
        {
            IntPtr registryKey;
            int result = _RegOpenKeyEx(_registryHive, _registrySubName, 0, STANDARD_RIGHTS_READ | KEY_QUERY_VALUE | KEY_NOTIFY,
                                      out registryKey);
            if (result != 0)
                throw new Win32Exception(result);

            try
            {
                AutoResetEvent _eventNotify = new AutoResetEvent(false);
                WaitHandle[] waitHandles = new WaitHandle[] { _eventNotify, _eventTerminate };
                while (!_eventTerminate.WaitOne(0, true))
                {
                    result = _RegNotifyChangeKeyValue(registryKey, true, _regFilter, _eventNotify.Handle, true);
                    if (result != 0)
                        throw new Win32Exception(result);

                    if (WaitHandle.WaitAny(waitHandles) == 0)
                    {
                        OnRegChanged_ICC();
                    }
                }
            }
            finally
            {
                if (registryKey != IntPtr.Zero)
                {
                    _RegCloseKey(registryKey);
                }
            }
        }
    }

    /// Monitor NightLight scheduler on/off if changed
    public class RegistryMonitor_NightLightScheduler : IDisposable
    {
        #region P/Invoke

        [DllImport("advapi32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern int RegOpenKeyEx(IntPtr hKey, string subKey, uint options, int samDesired,
                                               out IntPtr phkResult);
        private static int _RegOpenKeyEx(IntPtr hKey, string subKey, uint options, int samDesired,
                                               out IntPtr phkResult)
        {
            int rst = RegOpenKeyEx(hKey, subKey, options, samDesired, out phkResult);

            if (rst != 0)
            {
#if DEBUG
                Console.WriteLine("[RegistryMonitor_NightLight] RegOpenKeyEx failed.");
#endif
            }

            return rst;
        }

        [DllImport("advapi32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern int RegNotifyChangeKeyValue(IntPtr hKey, bool bWatchSubtree,
                                                          RegChangeNotifyFilter dwNotifyFilter, IntPtr hEvent,
                                                          bool fAsynchronous);
        private static int _RegNotifyChangeKeyValue(IntPtr hKey, bool bWatchSubtree,
                                                  RegChangeNotifyFilter dwNotifyFilter, IntPtr hEvent,
                                                  bool fAsynchronous)
        {
            int rst = RegNotifyChangeKeyValue(hKey, bWatchSubtree, dwNotifyFilter, hEvent, fAsynchronous);

            if (rst != 0)
            {
#if DEBUG
                Console.WriteLine("[RegistryMonitor_NightLight] RegNotifyChangeKeyValue failed.");
#endif
            }

            return rst;
        }

        [DllImport("advapi32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern int RegCloseKey(IntPtr hKey);
        private static int _RegCloseKey(IntPtr hKey)
        {
            int rst = RegCloseKey(hKey);

            if (rst != 0)
            {
#if DEBUG
                Console.WriteLine("[RegistryMonitor_NightLight] RegCloseKey failed.");
#endif
            }

            return rst;
        }

        private const int KEY_QUERY_VALUE = 0x0001;
        private const int KEY_NOTIFY = 0x0010;
        private const int STANDARD_RIGHTS_READ = 0x00020000;

        private static readonly IntPtr HKEY_CLASSES_ROOT = new IntPtr(unchecked((int)0x80000000));
        private static readonly IntPtr HKEY_CURRENT_USER = new IntPtr(unchecked((int)0x80000001));
        private static readonly IntPtr HKEY_LOCAL_MACHINE = new IntPtr(unchecked((int)0x80000002));
        private static readonly IntPtr HKEY_USERS = new IntPtr(unchecked((int)0x80000003));
        private static readonly IntPtr HKEY_PERFORMANCE_DATA = new IntPtr(unchecked((int)0x80000004));
        private static readonly IntPtr HKEY_CURRENT_CONFIG = new IntPtr(unchecked((int)0x80000005));
        private static readonly IntPtr HKEY_DYN_DATA = new IntPtr(unchecked((int)0x80000006));

        #endregion P/Invoke

        #region Event handling

        public event EventHandler? RegChanged;  //SDL, add ? to syncup definitions

        protected virtual void OnRegChanged_NightLightScheduler()
        {
            EventHandler handler = RegChanged;
            if (handler != null)
                handler(this, null);
        }

        public event ErrorEventHandler? Error;

        protected virtual void OnError_NightLightScheduler(Exception e)
        {
            ErrorEventHandler handler = Error;
            if (handler != null)
                handler(this, new ErrorEventArgs(e));
        }

        #endregion Event handling

        #region Private member variables

        private IntPtr _registryHive;
        private string? _registrySubName;//SDL, add ? to syncup definitions
        private object _threadLock = new object();
        private Thread? _thread;//SDL, add ? to syncup definitions
        private Dispatcher dispatcher = Dispatcher.CurrentDispatcher;
        private bool _disposed = false;
        private ManualResetEvent _eventTerminate = new ManualResetEvent(false);

        private RegChangeNotifyFilter _regFilter = RegChangeNotifyFilter.Key | RegChangeNotifyFilter.Attribute |
                                                   RegChangeNotifyFilter.Value | RegChangeNotifyFilter.Security;

        #endregion Private member variables

        public RegistryMonitor_NightLightScheduler(RegistryKey registryKey)
        {
            InitRegistryKey(registryKey.Name);
        }

        public RegistryMonitor_NightLightScheduler(string name)
        {
            if (name == null || name.Length == 0)
                throw new ArgumentNullException("name");

            InitRegistryKey(name);
        }

        public RegistryMonitor_NightLightScheduler(RegistryHive registryHive, string subKey)
        {
            InitRegistryKey(registryHive, subKey);
        }

        public void Dispose()
        {
            try
            {
                Stop();
            }
            catch (Exception ex)
            {
#if DEBUG
                Console.WriteLine("RegistryMonitor_NightLightScheduler: Dispose() " + ex.Message);
#endif
            }

            _disposed = true;
            GC.SuppressFinalize(this);
        }

        public RegChangeNotifyFilter RegChangeNotifyFilter
        {
            get { return _regFilter; }
            set
            {
                lock (_threadLock)
                {
                    if (IsMonitoring)
                        throw new InvalidOperationException("Monitoring thread is already running");

                    _regFilter = value;
                }
            }
        }

        #region Initialization

        private void InitRegistryKey(RegistryHive hive, string name)
        {
            switch (hive)
            {
                case RegistryHive.ClassesRoot:
                    _registryHive = HKEY_CLASSES_ROOT;
                    break;

                case RegistryHive.CurrentConfig:
                    _registryHive = HKEY_CURRENT_CONFIG;
                    break;

                case RegistryHive.CurrentUser:
                    _registryHive = HKEY_CURRENT_USER;
                    break;

                case RegistryHive.LocalMachine:
                    _registryHive = HKEY_LOCAL_MACHINE;
                    break;

                case RegistryHive.PerformanceData:
                    _registryHive = HKEY_PERFORMANCE_DATA;
                    break;

                case RegistryHive.Users:
                    _registryHive = HKEY_USERS;
                    break;

                default:
                    throw new InvalidEnumArgumentException("hive", (int)hive, typeof(RegistryHive));
            }
            _registrySubName = name;
        }

        private void InitRegistryKey(string name)
        {
            string[] nameParts = name.Split('\\');

            switch (nameParts[0])
            {
                case "HKEY_CLASSES_ROOT":
                case "HKCR":
                    _registryHive = HKEY_CLASSES_ROOT;
                    break;

                case "HKEY_CURRENT_USER":
                case "HKCU":
                    _registryHive = HKEY_CURRENT_USER;
                    break;

                case "HKEY_LOCAL_MACHINE":
                case "HKLM":
                    _registryHive = HKEY_LOCAL_MACHINE;
                    break;

                case "HKEY_USERS":
                    _registryHive = HKEY_USERS;
                    break;

                case "HKEY_CURRENT_CONFIG":
                    _registryHive = HKEY_CURRENT_CONFIG;
                    break;

                default:
                    _registryHive = IntPtr.Zero;
                    throw new ArgumentException("The registry hive '" + nameParts[0] + "' is not supported", "value");
            }

            _registrySubName = String.Join("\\", nameParts, 1, nameParts.Length - 1);
        }

        #endregion Initialization

        public bool IsMonitoring
        {
            get { return _thread != null; }
        }

        public void Start()
        {
            if (_disposed)
                throw new ObjectDisposedException(null, "This instance is already disposed");

            lock (_threadLock)
            {
                if (!IsMonitoring)
                {
                    _eventTerminate.Reset();
                    _thread = new Thread(new ThreadStart(MonitorThread));
                    _thread.IsBackground = true;

                    //dispatcher.BeginInvoke((Action)delegate ()
                    //{
                    Task.Run(() =>
                    {
                        _thread.Start();
                    });
                    //});
                }
            }
        }

        public void Stop()
        {
            if (_disposed)
                throw new ObjectDisposedException(null, "This instance is already disposed");

            lock (_threadLock)
            {
                Thread thread = _thread;
                if (thread != null)
                {
                    _eventTerminate.Set();
                    thread.Join();
                }
            }
        }

        private void MonitorThread()
        {
            try
            {
                ThreadLoop();
            }
            catch (Exception e)
            {
                OnError_NightLightScheduler(e);
            }
            _thread = null;
        }

        private void ThreadLoop()
        {
            IntPtr registryKey;
            int result = _RegOpenKeyEx(_registryHive, _registrySubName, 0, STANDARD_RIGHTS_READ | KEY_QUERY_VALUE | KEY_NOTIFY,
                                      out registryKey);
            if (result != 0)
                throw new Win32Exception(result);

            try
            {
                AutoResetEvent _eventNotify = new AutoResetEvent(false);
                WaitHandle[] waitHandles = new WaitHandle[] { _eventNotify, _eventTerminate };
                while (!_eventTerminate.WaitOne(0, true))
                {
                    result = _RegNotifyChangeKeyValue(registryKey, true, _regFilter, _eventNotify.Handle, true);
                    if (result != 0)
                        throw new Win32Exception(result);

                    if (WaitHandle.WaitAny(waitHandles) == 0)
                    {
                        OnRegChanged_NightLightScheduler();
                    }
                }
            }
            finally
            {
                if (registryKey != IntPtr.Zero)
                {
                    _RegCloseKey(registryKey);
                }
            }
        }
    }

    public class RegistryMonitor_Copilot : IDisposable
    {
        #region P/Invoke

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern int RegNotifyChangeKeyValue(
        IntPtr hKey,
        bool bWatchSubtree,
        uint dwNotifyFilter,
        IntPtr hEvent,
        bool fAsynchronous);

        private static class NativeMethods
        {
            public const uint REG_NOTIFY_CHANGE_NAME = 0x1;
            public const uint REG_NOTIFY_CHANGE_ATTRIBUTES = 0x2;
            public const uint REG_NOTIFY_CHANGE_LAST_SET = 0x4;
            public const uint REG_NOTIFY_CHANGE_SECURITY = 0x8;

            public const int KEY_READ = 0x20019;
        }

        #endregion P/Invoke

        #region Event handling

        public event EventHandler RegChanged;  //SDL, add ? to syncup definitions

        protected virtual void OnRegChanged_NightLightScheduler()
        {
            EventHandler handler = RegChanged;
            if (handler != null)
                handler(this, null);
        }

        #endregion Event handling

        #region Private member variables

        private readonly string _registryKey;
        private readonly bool _isDisposed;
        private Thread _monitorThread;
        private bool _stopMonitoring;
        private readonly RegistryKey _regKey;

        public event Action OnRegistryValueChanged;

        #endregion Private member variables

        public RegistryMonitor_Copilot(RegistryKey registryKey, string subKey)
        {
            if (registryKey == null)
            {
                throw new ArgumentNullException(nameof(registryKey), "RegistryKey cannot be null.");
            }

            if (string.IsNullOrEmpty(subKey))
            {
                throw new ArgumentException("SubKey cannot be null or empty.", nameof(subKey));
            }

            try
            {
                _regKey = registryKey.OpenSubKey(subKey, writable: false) ?? registryKey.CreateSubKey(subKey);
                _registryKey = subKey;
            }
            catch (Exception ex)
            {
                //EventLogHelper.WriteEventLog($"[RegistryMonitor_Copilot] RegistryMonitor_Copilot Exception: {ex.Message}", EventLogEntryType.Warning);
                //return;
                string errorMessage = $"[RegistryMonitor_Copilot] Failed to handle registry key '{subKey}' in '{registryKey.Name}' - Exception: {ex.Message}";
                EventLogHelper.WriteEventLog(errorMessage, EventLogEntryType.Warning);
                throw new ApplicationException(errorMessage, ex);
            }
        }

        public void Dispose()
        {
            Stop();
            _regKey?.Dispose();
            GC.SuppressFinalize(this);
        }

        public void Start()
        {
            if (_monitorThread != null && _monitorThread.IsAlive)
                return;

            _stopMonitoring = false;
            _monitorThread = new Thread(MonitorRegistryKey) { IsBackground = true };
            _monitorThread.Start();
        }

        private bool IsCopilotEnabled = CheckCopilotEnabled();
        private void MonitorRegistryKey()
        {
            if (_regKey == null)
                return;

            IntPtr registryKeyHandle = _regKey.Handle.DangerousGetHandle();

            while (!_stopMonitoring)
            {
                // Wait for changes in the registry key
                _ = RegNotifyChangeKeyValue(registryKeyHandle, false, NativeMethods.REG_NOTIFY_CHANGE_LAST_SET, IntPtr.Zero, false);

                var cp = CheckCopilotEnabled();
                if (cp != IsCopilotEnabled)
                {
                    if (cp)
                        RegChanged?.Invoke(this, new EventArgs());
                    else
                        RegChanged?.Invoke(this, null);
                    IsCopilotEnabled = cp;
                }
            }
        }
        private static bool CheckCopilotEnabled()
        {
            string regPath = $@"SOFTWARE\Policies\Microsoft\Windows\WindowsCopilot";
            string regKey = $"TurnOffWindowsCopilot";
            try
            {
                using (RegistryKey? key = Registry.CurrentUser.OpenSubKey(regPath))
                {
                    if (key != null)
                    {
                        // Read the value
                        object value = key.GetValue(regKey);

                        if (value != null && Convert.ToInt32(value) == 1)
                        {
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }
                    else
                    {
                        return true;
                    }
                }
            }
            catch (Exception)
            {
                return true;
            }
        }

        public void Stop()
        {
            _stopMonitoring = true;
            _monitorThread?.Join();
        }
    }
}