using CommunityToolkit.Mvvm.ComponentModel;
using DDPM.SA.Common;
using DDPM.SA.Common.Display;
using DDPM.SA.Common.Settings;
using DDPM.UI.Common;
using DDPM.UI.Common.EAEM;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using DDPM.UI.Common.UserControls;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media;
using System.IO;
using VcpCore.Common;
using Windows.System;
using Dell.Client.Framework.Common;
using DDPM.UI.Resources.Helper;
using System.Collections.ObjectModel;
using DDPM.UI.Plugin.DdpmHomePlugin;
using Dell.Client.Framework.UX.WPF;

namespace DDPM.UI.Module.Kvm
{
    public class InputSourceList
    {
        public string Type { get; set; } = String.Empty;
        public string PathData { get; set; } = String.Empty;
        public KvmModule kvmModule { get; set; }
    }

    public class USBList
    {
        public string Type { get; set; } = String.Empty;
        public string PathData { get; set; } = string.Empty;
        public KvmModule kvmModule { get; set; }
    }

    public class PCInput
    {
        public string inputSource = string.Empty;
        public KvmModule kvmModule { get; set; }

        public string inputDisplayText
        {
            get
            {
                return inputSource;
            }
        }
    }

    public class User32_SetWindowPos
    {
        [DllImport("user32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
        public static bool _SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags)
        {
            return SetWindowPos(hWnd, hWndInsertAfter, X, Y, cx, cy, uFlags);
        }

        public static readonly IntPtr HWND_BOTTOM = new IntPtr(1);
        public static readonly IntPtr HWND_TOP = new IntPtr(0);
        public const uint SWP_NOSIZE = 0x0001;
        public const uint SWP_NOMOVE = 0x0002;
        public const uint SWP_NOACTIVATE = 0x0010;
        public const uint SWP_SHOWWINDOW = 0x0040;
        public const uint SWP_NOOWNERZORDER = 0x0200;
        public const uint SWP_NOREDRAW = 0x0008;
    }

    public struct WINDOWPOS
    {
        public IntPtr hwnd;
        public IntPtr hwndInsertAfter;
        public int x;
        public int y;
        public int cx;
        public int cy;
        public uint flags;
    }

    public class DDPMWindowPos : NativeWindow
    {
        private IntPtr _hwnd;
        private IntPtr _parent;

        private const int SWP_NOMOVE = 0x0002;
        private const int SWP_NOSIZE = 0x0001;
        private const int SWP_NOACTIVATE = 0x0010;
        private const int WM_WINDOWPOSCHANGING = 0x0046;
        private const int WM_ACTIVATE = 0x0006;
        private const int WM_NCACTIVATE = 0x0086;

        public DDPMWindowPos(IntPtr hwnd, IntPtr parent)
        {
            this._hwnd = hwnd;
            this._parent = parent;
            this.AssignHandle(parent);
        }

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_WINDOWPOSCHANGING: // WM_WINDOWPOSCHANGING
                    {
                        if (_hwnd != IntPtr.Zero)
                        {
                            //var pos = (WINDOWPOS)Marshal.PtrToStructure(m.LParam, typeof(WINDOWPOS));
                            //pos.hwndInsertAfter = User32_SetWindowPos.HWND_BOTTOM;
                            //pos.flags |= User32_SetWindowPos.SWP_NOACTIVATE;
                            //Marshal.StructureToPtr(pos, m.LParam, true);
                            User32_SetWindowPos._SetWindowPos(_hwnd, User32_SetWindowPos.HWND_TOP, 100, 100, 100, 100, User32_SetWindowPos.SWP_NOMOVE | User32_SetWindowPos.SWP_NOSIZE);
                        }
                    }
                    break;
                    //case WM_ACTIVATE: // WM_ACTIVATE
                    //    {
                    //        if (m.WParam != IntPtr.Zero && _hwnd != IntPtr.Zero)
                    //        {
                    //            User32_SetWindowPos.SetWindowPos(this.Handle, User32_SetWindowPos.HWND_BOTTOM, 0, 0, 0, 0, User32_SetWindowPos.SWP_NOMOVE | User32_SetWindowPos.SWP_NOSIZE);
                    //        }
                    //    }
                    //    break;
                    //case WM_NCACTIVATE: // WM_NCACTIVATE
                    //    {
                    //        if (m.WParam != IntPtr.Zero && _hwnd != IntPtr.Zero)
                    //        {
                    //            User32_SetWindowPos.SetWindowPos(this.Handle, User32_SetWindowPos.HWND_BOTTOM, 0, 0, 0, 0, User32_SetWindowPos.SWP_NOMOVE | User32_SetWindowPos.SWP_NOSIZE);
                    //        }
                    //    }
                    //    break;
            }
            base.WndProc(ref m);
        }
    }

    public class KvmViewModel : ObservableObject
    {
        #region private
        private InputSourceList _PC1selectInput = new InputSourceList();
        private InputSourceList _PC2selectInput = new InputSourceList();
        private InputSourceList _PC3selectInput = new InputSourceList();
        private InputSourceList _PC4selectInput = new InputSourceList();
        private USBList _PC1selectUSB = new USBList();
        private USBList _PC2selectUSB = new USBList();
        private USBList _PC3selectUSB = new USBList();
        private USBList _PC4selectUSB = new USBList();
        private List<InputSourceList> _inputsList = new List<InputSourceList>();
        private List<USBList> _usbsList = new List<USBList>();
        private PCInput pcInput = new PCInput();
        private bool _isNoKVM = false;
        private bool _isUSBKVM = false;
        private bool _isNKVM = false;
        //private static Log _log;

        //private Dictionary<string, PCsInfo> pcsList = new Dictionary<string, PCsInfo>();
        private ImageSource? _PCImage;
        #endregion

        [DllImport("user32.dll", EntryPoint = "SetParent", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern int SetParent(IntPtr hWndChild, IntPtr hWndNewParent);
        private static int _SetParent(IntPtr hWndChild, IntPtr hWndNewParent)
        {
            return SetParent(hWndChild, hWndNewParent);
        }

        [DllImport("user32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern bool EnableWindow(IntPtr hWnd, bool bEnable);
        private static bool _EnableWindow(IntPtr hWnd, bool bEnable)
        {
            return EnableWindow(hWnd, bEnable);
        }
        //[DllImport("user32.dll", SetLastError = true)]
        //public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
        public readonly ILog _log;
        public IModuleOwner? ModuleOwner { get; set; }
        public KvmModule KvmModule { get; set; }
        public UInt16 PxPCode { get; set; } = 0;
        public InputSourceFullView inputSourceFullView { get; set; }
        public Visibility PC3_Visibility { get; set; } = Visibility.Collapsed;
        public Visibility PC4_Visibility { get; set; } = Visibility.Collapsed;
        public Visibility Border1Visibility { get; set; } = Visibility.Collapsed;
        public Visibility Border2Visibility { get; set; } = Visibility.Collapsed;
        public Visibility Border3Visibility { get; set; } = Visibility.Collapsed;
        public Visibility Border4Visibility { get; set; } = Visibility.Collapsed;
        public Visibility SupportUSBKVM { get; set; } = Visibility.Collapsed;
        public Visibility SupportNKVM { get; set; } = Visibility.Collapsed;
        public Visibility SetInput { get; set; } = Visibility.Visible;
        public Visibility SetPXP { get; set; } = Visibility.Visible;
        public Visibility EditInput { get; set; } = Visibility.Collapsed;
        public Visibility EditPXP { get; set; } = Visibility.Collapsed;
        public string PC1_Input { get; set; }
        public string PC2_Input { get; set; }
        public string PC3_Input { get; set; }
        public string PC4_Input { get; set; }

        public Dictionary<UInt16, System.Windows.Controls.UserControl> PxPcodeDictionary = new Dictionary<UInt16, System.Windows.Controls.UserControl>()
        {
            [0x0] = new PxPSplitCtrl0A(),
            [0x11] = new PIPSplitCtrl1A(),
            [0x12] = new PIPSplitCtrl1B(),
            [0x24] = new PBPSplitCtrl2A(),
            [0x2F] = new PBPSplitCtrl2B(),
            [0x26] = new PBPSplitCtrl2C(),
            [0x28] = new PBPSplitCtrl2C(),
            [0x2A] = new PBPSplitCtrl2C(),
            [0x2C] = new PBPSplitCtrl2C(),
            [0x2E] = new PBPSplitCtrl2C(),
            [0x25] = new PBPSplitCtrl2D(),
            [0x27] = new PBPSplitCtrl2D(),
            [0x29] = new PBPSplitCtrl2D(),
            [0x2B] = new PBPSplitCtrl2D(),
            [0x2D] = new PBPSplitCtrl2D(),
            [0x31] = new PBPSplitCtrl3E(),
            [0x32] = new PBPSplitCtrl3D(),
            [0x33] = new PBPSplitCtrl3H(),
            [0x34] = new PBPSplitCtrl3B(),
            [0x35] = new PBPSplitCtrl3I(),
            [0x41] = new PBPSplitCtrl4A(),
            [0x42] = new PBPSplitCtrl4D()
        };

        public string InputName1 { get; set; }
        public string InputName2 { get; set; }
        public string InputName3 { get; set; }
        public string InputName4 { get; set; }
        public bool isPipSmall { get; set; } = false;
        public bool isPipLarge { get; set; } = false;
        public bool isPBP { get; set; } = false;

        //public bool isNoKVM { get; set; } = false;
        //public bool isNKVM { get; set; } = false;
        //public ImageSource? Image_PC { get; set; }
        public Dictionary<string, InputInfo> inputList { get; set; }

        public List<string> usbupstreamList { get; set; }
        public List<InputSourceObj> subInputs { get; set; }
        public List<UInt16> subInputList = new List<UInt16>();
        public Dictionary<string, PCsInfo> pcsList { get; set; }
        public List<string> usbsList { get; set; }
        public Dictionary<string, PCsInfo> original_pcsList { get; set; }
        public bool USBKVMisON = false;
        public bool NKVMisON = false;

        public bool isNoKVM
        {
            get => _isNoKVM;
            set
            {
                SetProperty(ref _isNoKVM, value);
                if (value)
                {
                    isOnUSBKVM(false);
                    _isNKVM = false;
                    DdpmCommonHelper.DeviceManagerSA.SentKVMtoTelementry(DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo, "KVMMode", "NoKVM");
                }
            }
        }

        public bool isUSBKVM
        {
            get => _isUSBKVM;
            set
            {
                SetProperty(ref _isUSBKVM, value);
                if (value)
                {
                    _isNKVM = false;
                    _isNoKVM = false;
                }
            }
        }

        public bool isNKVM
        {
            get => _isNKVM;
            set
            {
                SetProperty(ref _isNKVM, value);
                if (value)
                {
                    isOnUSBKVM(false);
                    _isNoKVM = false;
                    _isNKVM = true;
                }
            }
        }

        public List<InputSourceList> PCInputsList
        {
            get => _inputsList;
            set => SetProperty(ref _inputsList, value);
        }

        public List<USBList> USBsList
        {
            get => _usbsList;
            set => SetProperty(ref _usbsList, value);
        }

        public InputSourceList PC1Inputs_Selected
        {
            get => _PC1selectInput;
            set
            {
                SetProperty(ref _PC1selectInput, value);
                SelectInputSource(_PC1selectInput.Type, "PC1");
            }
        }

        public InputSourceList PC2Inputs_Selected
        {
            get => _PC2selectInput;
            set
            {
                SetProperty(ref _PC2selectInput, value);
                SelectInputSource(_PC2selectInput.Type, "PC2");
            }
        }

        public InputSourceList PC3Inputs_Selected
        {
            get => _PC3selectInput;
            set
            {
                SetProperty(ref _PC3selectInput, value);
                SelectInputSource(_PC3selectInput.Type, "PC3");
            }
        }

        public InputSourceList PC4Inputs_Selected
        {
            get => _PC4selectInput;
            set
            {
                SetProperty(ref _PC4selectInput, value);
                SelectInputSource(_PC4selectInput.Type, "PC4");
            }
        }

        public USBList PC1USB_Selected
        {
            get => _PC1selectUSB;
            set
            {
                SetProperty(ref _PC1selectUSB, value);
                SelectUSB(_PC1selectUSB.Type, "PC1");
            }
        }

        public USBList PC2USB_Selected
        {
            get => _PC2selectUSB;
            set
            {
                SetProperty(ref _PC2selectUSB, value);
                SelectUSB(_PC2selectUSB.Type, "PC2");
            }
        }

        public USBList PC3USB_Selected
        {
            get => _PC3selectUSB;
            set
            {
                SetProperty(ref _PC3selectUSB, value);
                SelectUSB(_PC3selectUSB.Type, "PC3");
            }
        }

        public USBList PC4USB_Selected
        {
            get => _PC4selectUSB;
            set
            {
                SetProperty(ref _PC4selectUSB, value);
                SelectUSB(_PC4selectUSB.Type, "PC4");
            }
        }

        public ImageSource? PCImage
        {
            get => _PCImage;
            set => SetProperty(ref _PCImage, value);
        }

        public string? ConnectionType { get; set; }
        public string? Text1 { get; set; }
        public bool isUSBKVMButton { get; set; } = true;
        public double USBKVMButtonOpacity { get; set; } = 1;

        public bool isNKVMEanble { get; set; } = true;
        public double NKVM_Opacity { get; set; } = 1;
        public Visibility LockNKVM_Visibility { get; set; } = Visibility.Collapsed;

        public bool isUSBKVMEanble { get; set; } = true;
        public double USBKVM_Opacity { get; set; } = 1;
        public Visibility LockUSBKVM_Visibility { get; set; } = Visibility.Collapsed;
        public Visibility isPxP {  get; set; } = Visibility.Collapsed;
        public Visibility NoPxP {  get; set; } = Visibility.Collapsed;

        #region Hotkey

        private string _kvmHotkeyTooltip = "None";

        public string KvmHotkeyTooltip
        {
            get => _kvmHotkeyTooltip;
            set
            {
                SetProperty(ref _kvmHotkeyTooltip, value);
                OnPropertyChanged("KvmHotkeyTooltip");
            }
        }

        private string _switchPCsKey = "None";

        public string SwitchPCsKey
        {
            get => _switchPCsKey;
            set
            {
                SetProperty(ref _switchPCsKey, value);
                OnPropertyChanged("SwitchPCsKey");
            }
        }

        private string _changePipKey = "None";

        public string ChangePipKey
        {
            get => _changePipKey;
            set
            {
                SetProperty(ref _changePipKey, value);
                OnPropertyChanged("ChangePipKey");
            }
        }

        private string _switchKbMsKey = "None";

        public string SwitchKbMsKey
        {
            get => _switchKbMsKey;
            set
            {
                SetProperty(ref _switchKbMsKey, value);
                OnPropertyChanged("SwitchKbMsKey");
            }
        }

        private bool _autoSwitchChecked;

        public bool AutoSwitchChecked
        {
            get => _autoSwitchChecked;
            set
            {
                SetProperty(ref _autoSwitchChecked, value);
                OnPropertyChanged("AutoSwitchChecked");
                saveKvmHotkeyOption();
            }
        }

        public void SaveHotkeySettings(MonitorInfo monitorInfo, HotkeyInfo hotkeyInfo)
        {
            if (DdpmCommonHelper.DeviceManagerSA != null)
            {
                IsBusy = true;
                OnPropertyChanged("IsBusy");
                bool saveSettings = DdpmCommonHelper.DeviceManagerSA.SaveHotkeySetting(monitorInfo, hotkeyInfo).Result;
                if (saveSettings)
                {
                    DdpmCommonHelper.isHotkeyBypass = DdpmCommonHelper.DeviceManagerSA.ByPassHotkey(false).Result;
                    IsBusy = false;
                    OnPropertyChanged("IsBusy");
                }
            }
            Invoke_RefreshHotkeySettings();
        }
        private void saveKvmHotkeyOption()
        {
            HotkeySettings hotkeySettings = new HotkeySettings
            {
                //DeviceInfo = KvmModule.SelectedHomeDevice.MonitorInfo.edid,
                ModelName = "DDPM",
                SerialNumber = "DDPM",
                ServiceTag = "DDPM",
                HotkeyOptions = new List<HotkeyOption> { _autoSwitchChecked ? HotkeyOption.KvmAutoApply : HotkeyOption.None }
            };
            DdpmCommonHelper.DeviceManagerSA.SaveHotkeyOptionOnly(hotkeySettings);
        }

        public void Invoke_RefreshHotkeySettings()
        {
            BackgroundWorker bw = new BackgroundWorker()
            {
                WorkerReportsProgress = false,
                WorkerSupportsCancellation = false
            };
            bw.DoWork += DoWork_RefreshHotkeyData;
            bw.RunWorkerCompleted += RunWorkerCompleted_RefreshHotkeyData;
            bw.RunWorkerAsync(); //myArg is the optional argument
        }

        private void DoWork_RefreshHotkeyData(object sender, DoWorkEventArgs e)
        {
            try
            {
                //sender is the ‘bw’ object
                BackgroundWorker bwk = (BackgroundWorker)sender;
                //load hotkey setting
                if (DdpmCommonHelper.DeviceManagerSA != null)
                {
                    var temp = DdpmCommonHelper.DeviceManagerSA.ReadCurrentHotkey(KvmModule?.SelectedHomeDevice?.MonitorInfo).Result;
                    HotkeySettings curHotkey = temp.Item1;
                    //0708 error handling for non-EE support monitor
                    string swHortcutText = string.Empty;
                    string HeadCaption = LangHelper.Instance["Hotkeys"];
                    string SwitchPCsKeyCaption = LangHelper.Instance["Kvm.8"];
                    string SwitchKbMsKeyCaption = LangHelper.Instance["Kvm.9"];
                    string ChangePipKeyCaption = LangHelper.Instance["Kvm.10"];
                    if (curHotkey.HotkeyInfo.Count > 0)
                    {
                        foreach (var hotkeyInfo in curHotkey.HotkeyInfo)
                        {
                            List<VirtualKey> hotkeys = hotkeyInfo.Hotkey;
                            switch (hotkeyInfo.Job)
                            {
                                case HotkeyType.KvmSwitchInputSource:
                                    KeysHelper.ReSetHotKeyText(ref swHortcutText, ref hotkeys);
                                    hotkeys.Clear();
                                    SwitchPCsKey = swHortcutText;
                                    break;

                                case HotkeyType.KvmSwitchKbMsKey:
                                    KeysHelper.ReSetHotKeyText(ref swHortcutText, ref hotkeys);
                                    hotkeys.Clear();
                                    SwitchKbMsKey = swHortcutText;
                                    break;

                                case HotkeyType.KvmChangePIPPosition:
                                    KeysHelper.ReSetHotKeyText(ref swHortcutText, ref hotkeys);
                                    hotkeys.Clear();
                                    ChangePipKey = swHortcutText;
                                    break;
                            }
                        }
                        _kvmHotkeyTooltip = $"{HeadCaption} - {SwitchPCsKeyCaption}: {SwitchPCsKey}\r\n{HeadCaption} - {SwitchKbMsKeyCaption}: {SwitchKbMsKey}\r\n{HeadCaption} - {ChangePipKeyCaption}: {ChangePipKey}";
                    }
                    else
                    {
                        string StrNone = LangHelper.Instance["None"];
                        _kvmHotkeyTooltip = $"{HeadCaption} - {SwitchPCsKeyCaption}: {StrNone}\r\n{HeadCaption} - {SwitchKbMsKeyCaption}: {StrNone}\r\n{HeadCaption} - {ChangePipKeyCaption}: {StrNone}";
                    }
                    if (curHotkey.HotkeyOptions.Count > 0 && curHotkey.HotkeyOptions.Any(x => x.Equals(HotkeyOption.KvmAutoApply)))
                    {
                        _autoSwitchChecked = true;
                    }
                }

                OnPropertyChanged("KvmHotkeyTooltip");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private void RunWorkerCompleted_RefreshHotkeyData(object sender, RunWorkerCompletedEventArgs e)
        {
            Debug.WriteLine("load kvm hotkey setting done");
            //Handling the result and final process
        }

        #endregion Hotkey

        public KvmViewModel()
        {
            _log = DdpmCommonHelper.MyConsole.CreateLog("KvmViewModel");
            if (DdpmCommonHelper.DeviceManagerSA != null)
            {
                //OSD/VCP control back event
                DdpmCommonHelper.DeviceManagerSA.VCPchanged += OnVCPChangedEvent;
            }
        }

        ~KvmViewModel()
        {
            if (DdpmCommonHelper.DeviceManagerSA != null)
            {
                //OSD/VCP control back event
                DdpmCommonHelper.DeviceManagerSA.VCPchanged -= OnVCPChangedEvent;
            }
        }

        #region UI Enable Flags

        private bool _isBusy = false;

        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        #endregion UI Enable Flags

        //public ObjGetVCP ret_PxP = new ObjGetVCP();
        

        public void Invoke_RefreshData()
        {
            BackgroundWorker bw = new BackgroundWorker()
            {
                WorkerReportsProgress = false,
                WorkerSupportsCancellation = false
            };
            SupportNKVM = Visibility.Collapsed;
            SupportUSBKVM = Visibility.Collapsed;
            var directory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            directory = $"C:\\Program Files\\Dell\\Dell Display and Peripheral Manager";
            string strFullPath = string.Format("{0}\\Plugins\\NKVM\\DDM.exe", directory);
            
            if (DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo != null)
            {
                MonitorInfo mi = DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo;
                if (DdpmCommonHelper.DeviceManagerSA.isNKVMSupportMonitor(mi).Result && File.Exists(strFullPath))
                {
                    SupportNKVM = Visibility.Visible;
                }

                USBKVMisON = KvmModule.isUSBKVM; /*DdpmCommonHelper.DeviceManagerSA.GetOnNKVM(mi).Result;*/
                NKVMisON = DdpmCommonHelper.DeviceManagerSA.GetOnNKVM(mi).Result;
                if (mi.CapabilityDic.ContainsKey("EE"))
                {
                    SupportUSBKVM = Visibility.Visible;
                    //inputList = new Dictionary<string, InputInfo>();
                    //subInputs = new List<InputSourceObj>();
                    //usbsList = new List<string>();
                    isUSBKVMButton = true;
                    USBKVMButtonOpacity = 1;
                    //_isUSBKVM = KvmModule.isUSBKVM;
                    if (USBKVMisON)
                    {
                        _isUSBKVM = true;
                    }
                    else if (NKVMisON)
                    {
                        _isNKVM = true;
                    }
                    else
                    {
                        _isNoKVM = true;
                    }

                    if (mi.CapabilityDic.ContainsKey("E8"))
                    {
                        isPxP = Visibility.Visible;
                        NoPxP = Visibility.Collapsed;
                        if (DdpmCommonHelper.DeviceManagerSA.isScreenPartition(mi).Result)
                        {
                            isUSBKVMButton = false;
                            USBKVMButtonOpacity = 0.5;
                        }
                    }
                    else
                    {
                        isPxP = Visibility.Collapsed;
                        NoPxP = Visibility.Visible;
                    }
                }
                bw.DoWork += DoWork_RefreshData;
                bw.RunWorkerCompleted += RunWorkerCompleted_RefreshData;
                bw.RunWorkerAsync(); //myArg is the optional argument
                IsBusy = true;
            }
            else
            {
                _log?.Debug("MonitorInfo is null.");
            }
        }

        private void DoWork_RefreshData(object sender, DoWorkEventArgs e)
        {
            try // 2024-06-19 Fix exception when close Main UI or device remove.
            { 
                //sender is the ‘bw’ object
                BackgroundWorker bwk = (BackgroundWorker)sender;

                HomeDevice selHomeDevice = DdpmCommonHelper.ModuleOwner.SelectedHomeDevice;
                if (selHomeDevice == null)
                {
                    e.Result = "SelectedHomeDevice is null";
                    return;
                }
                MonitorInfo mi = selHomeDevice.MonitorInfo;
                if (mi == null)
                {
                    e.Result = "MonitorInfo is null";
                    return;
                }

                if (mi.CapabilityDic.ContainsKey("EE"))
                {
                    //If arg is specified, you can get it with below code
                    //myArgType arg = (myArgType)e.Argument;
                    inputList = new Dictionary<string, InputInfo>();
                    subInputs = new List<InputSourceObj>();
                    //List<UInt16> subInputList = new List<UInt16>();
                    usbsList = new List<string>();
                    //Dictionary<string, PCsInfo> pcsList = new Dictionary<string, PCsInfo>();
                    inputList = DdpmCommonHelper.DeviceManagerSA.GetInputSourcelist(KvmModule.SelectedHomeDevice.MonitorInfo).Result;
                    subInputList = DdpmCommonHelper.DeviceManagerSA.GetSubInputList(KvmModule.SelectedHomeDevice.MonitorInfo).Result;
                    //List<InputSourceObj> result = DdpmCommonHelper.DeviceManagerSA.GetSubInputs(KvmModule.SelectedHomeDevice.MonitorInfo).Result;
                    if (subInputList != null)
                    {
                        //subInputs.Clear();
                        subInputs = new List<InputSourceObj>();
                        foreach (UInt16 subinput in subInputList)
                        {
                            InputSourceObj inputSourceObj = new InputSourceObj();
                            foreach (var input in inputList)
                            {
                                if (input.Value.Code == (uint)subinput)
                                {
                                    inputSourceObj.Code = subinput;
                                    inputSourceObj.Name = input.Key;
                                    break;
                                }
                            }
                            subInputs.Add(inputSourceObj);
                        }
                    }
                    string currentinput = KvmModule.SelectedHomeDevice.MonitorInfo.inputSource;
                    pcsList = DdpmCommonHelper.DeviceManagerSA.GetUSBKVMPCsList(KvmModule.SelectedHomeDevice.MonitorInfo, inputList, subInputs).Result;
                    usbsList = DdpmCommonHelper.DeviceManagerSA.GetUSBUpstreamList(KvmModule.SelectedHomeDevice.MonitorInfo).Result;
                    original_pcsList = DdpmCommonHelper.DeviceManagerSA.GetUSBKVMPCsList(KvmModule.SelectedHomeDevice.MonitorInfo, inputList, subInputs).Result;
                    if ((inputList != null) && (pcsList != null && pcsList.Count > 1))   // 2024-06-19 Elie, fix exception.
                    {
                        if (inputList.Count != _inputsList.Count && usbsList.Count != _usbsList.Count)
                        {
                            string pathData = string.Empty;
                            //_inputsList.Clear();
                            _inputsList = new List<InputSourceList>();
                            foreach (string item in inputList.Keys)
                            {
                                pathData = InputTypeCommon.GetInputImage(item);
                                _inputsList.Add(new InputSourceList()
                                {
                                    PathData = pathData,
                                    Type = item,
                                    kvmModule = KvmModule
                                });
                            }
                            //_usbsList.Clear();
                            _usbsList = new List<USBList>();
                            foreach (string str in usbsList)
                            {
                                pathData = InputTypeCommon.GetInputImage(str);
                                _usbsList.Add(new USBList()
                                {
                                    Type = str,
                                    PathData = pathData,
                                    kvmModule = KvmModule
                                });
                            }
                            PCInputsList = _inputsList;
                            USBsList = _usbsList;
                            if (pcsList.TryGetValue("PC1", out var pc1) && pcsList.TryGetValue("PC2", out var pc2))
                            {
                                _PC1selectInput = _inputsList.Find(x => (x.Type == KvmModule.SelectedHomeDevice.MonitorInfo.inputSource));
                                _PC1selectUSB = _usbsList.Find(x => (x.Type == pcsList["PC1"].USBUpstream));
                                _PC2selectInput = _inputsList.Find(x => (x.Type == pcsList["PC2"].InputType));
                                _PC2selectUSB = _usbsList.Find(x => (x.Type == pcsList["PC2"].USBUpstream));
                                if (!USBKVMisON)
                                {
                                    pcsList["PC1"].InputName = "PC1";
                                    pcsList["PC2"].InputName = "PC2";
                                }
                                InputName1 = pcsList["PC1"].InputName;
                                InputName2 = pcsList["PC2"].InputName;
                                if (pcsList.Count >= 3)
                                {
                                    if (pcsList.TryGetValue("PC3", out var pc3))
                                    {
                                        _PC3selectInput = _inputsList.Find(x => (x.Type == pcsList["PC3"].InputType));
                                        _PC3selectUSB = _usbsList.Find(x => (x.Type == pcsList["PC3"].USBUpstream));
                                        if (!USBKVMisON)
                                        {
                                            pcsList["PC3"].InputName = "PC3";
                                        }
                                        InputName3 = pcsList["PC3"].InputName;
                                        PC3_Visibility = Visibility.Visible;
                                        if (pcsList.Count == 4)
                                        {
                                            if (pcsList.TryGetValue("PC4", out var pc4))
                                            {
                                                _PC4selectInput = _inputsList.Find(x => (x.Type == pcsList["PC4"].InputType));
                                                _PC4selectUSB = _usbsList.Find(x => (x.Type == pcsList["PC4"].USBUpstream));
                                                if (!USBKVMisON)
                                                {
                                                    pcsList["PC4"].InputName = "PC4";
                                                }
                                                InputName4 = pcsList["PC4"].InputName;
                                                PC4_Visibility = Visibility.Visible;
                                                PCImage = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/USBKVM_4PCs.png");
                                            }
                                            else
                                            {
                                                _log?.Debug("PC4 not found in pcsList.");
                                            }
                                        }
                                        else
                                        {
                                            PCImage = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/USBKVM_3PCs.png");
                                        }
                                    }
                                    else
                                    {
                                        _log?.Debug("PC3 not found in pcsList.");
                                    }
                                }
                                else
                                {
                                    PCImage = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/USBKVM_2PCs.png");
                                }
                            }
                            else
                            {
                                _log?.Debug("PC1 or PC2 not found in pcsList.");
                            }
                        }
                    }
                    else
                    {
                        _log?.Debug("inputList is null or pcsList is null or count < 2.");
                        isUSBKVMButton = false;
                        USBKVMButtonOpacity = 0.5;
                    }
                    OnPropertyChanged("PC1Inputs_Selected");
                    OnPropertyChanged("PC2Inputs_Selected");
                    OnPropertyChanged("PC3Inputs_Selected");
                    OnPropertyChanged("PC4Inputs_Selected");
                    OnPropertyChanged("PC1USB_Selected");
                    OnPropertyChanged("PC2USB_Selected");
                    OnPropertyChanged("PC3USB_Selected");
                    OnPropertyChanged("PC4USB_Selected");
                    OnPropertyChanged("PCImage");

                    #region PIP/PIP
                    if (mi.CapabilityDic.ContainsKey("E8"))
                    {
                        if (!DdpmCommonHelper.DeviceManagerSA.isScreenPartition(mi).Result)
                        {
                            //isPxP = Visibility.Visible;
                            //NoPxP = Visibility.Collapsed;
                            //Get the Pxp Capabilities
                            _pipPbpCaps = DdpmCommonHelper.DeviceManagerSA.GetPipPbpCapabilitiesWords(mi).Result;
                            OnPipPbpCapsChanged();

                            //Check if this monitor has Pxp mode capabilities
                            if ((_pipPbpCaps != null) && (_pipPbpCaps.Length > 0))
                            {
                                //Get current monitor's Pxp mode
                                ObjGetVCP ret_PxP = DdpmCommonHelper.DeviceManagerSA.GetPxpMode(mi).Result;
                                if (ret_PxP != null && ret_PxP.result)
                                {
                                    //UInt64 u64 = (UInt64)ret.result;
                                    _curPxpMode = Convert.ToUInt16(ret_PxP.value);
                                    PxPCode = _curPxpMode;
                                    VideoSwapContent = PxPcodeDictionary[_curPxpMode];
                                    switch (_curPxpMode)
                                    {
                                        case 0x11:
                                            isPipSmall = true;
                                            isPipLarge = false;
                                            isPBP = false;
                                            break;

                                        case 0x12:
                                            isPipSmall = true;
                                            isPipLarge = false;
                                            isPBP = false;
                                            break;

                                        case 0x24:
                                        case 0x2F:
                                        case 0x26:
                                        case 0x28:
                                        case 0x2A:
                                        case 0x2C:
                                        case 0x2E:
                                        case 0x25:
                                        case 0x27:
                                        case 0x29:
                                        case 0x2B:
                                        case 0x2D:
                                        case 0x31:
                                        case 0x32:
                                        case 0x33:
                                        case 0x34:
                                        case 0x35:
                                        case 0x41:
                                        case 0x42:
                                            isPipSmall = false;
                                            isPipLarge = false;
                                            isPBP = true;
                                            break;

                                        default:
                                            isPipSmall = false;
                                            isPipLarge = false;
                                            isPBP = false;
                                            break;
                                    }
                                }
                            }
                            else
                            {
                                isUSBKVMButton = false;
                                USBKVMButtonOpacity = 0.5;
                            }

                            //Get current Main InputSource from MonitorInfo
                            //
                            string currentInput = mi.inputSource;
                            MainInputSource = new InputSourceObj(currentInput);

                            //Get Sub inputs
                            //List<InputSourceObj> subInputs = DdpmCommonHelper.DeviceManagerSA.GetSubInputs(mi).Result;

                            if (subInputs != null)
                            {
                                SubInputs = subInputs;
                            }
                            else
                            {
                                //Not support SubInput or fail to query
                            }
                        }
                    }
                    else
                    {
                        NoPxP = Visibility.Visible;
                        isPxP = Visibility.Collapsed;
                    }
                    #endregion PIP/PIP
                }
            }
            catch (Exception)
            {
                ;
            }
        }

        private void RunWorkerCompleted_RefreshData(object sender, RunWorkerCompletedEventArgs e)
        {
            IsBusy = false;
            //Handling the result and final process
        }

        private void SelectInputSource(string inputSource, string pcnum)
        {
            PCsInfo pcInfo = new PCsInfo();
            foreach (var input in inputList)
            {
                if (inputSource == input.Key)
                {
                    pcInfo.InputType = input.Key;
                    pcInfo.InputName = input.Value.InputName;
                    pcInfo.USBUpstream = input.Value.USBUpstream;
                    pcInfo.Code = input.Value.Code;
                    break;
                }
            }
            pcsList[pcnum] = pcInfo;
            if (pcnum == "PC1")
            {
                _PC1selectUSB = _usbsList.Find(x => (x.Type == pcsList[pcnum].USBUpstream));
            }
            else if (pcnum == "PC2")
            {
                _PC2selectUSB = _usbsList.Find(x => (x.Type == pcsList[pcnum].USBUpstream));
            }
            else if (pcnum == "PC3")
            {
                _PC3selectUSB = _usbsList.Find(x => (x.Type == pcsList[pcnum].USBUpstream));
            }
            else if (pcnum == "PC4")
            {
                _PC4selectUSB = _usbsList.Find(x => (x.Type == pcsList[pcnum].USBUpstream));
            }
            //bool b = DdpmCommonHelper.DeviceManagerSA.SetUSBKVMPCsList(KvmModule.SelectedHomeDevice.MonitorInfo, pcsList).Result;
            OnPropertyChanged("PC1Inputs_Selected");
            OnPropertyChanged("PC2Inputs_Selected");
            OnPropertyChanged("PC3Inputs_Selected");
            OnPropertyChanged("PC4Inputs_Selected");
            OnPropertyChanged("PC1USB_Selected");
            OnPropertyChanged("PC2USB_Selected");
            OnPropertyChanged("PC3USB_Selected");
            OnPropertyChanged("PC4USB_Selected");
        }

        private void SelectUSB(string usb, string pcnum)
        {
            if (pcsList.TryGetValue(pcnum, out var pc))
            {
                pcsList[pcnum].USBUpstream = usb;
                //bool b = DdpmCommonHelper.DeviceManagerSA.SetUSBKVMPCsList(KvmModule.SelectedHomeDevice.MonitorInfo, pcsList).Result;
                OnPropertyChanged("PC1USB_Selected");
                OnPropertyChanged("PC2USB_Selected");
                OnPropertyChanged("PC3USB_Selected");
                OnPropertyChanged("PC4USB_Selected");
            }
            else
            {
                _log?.Debug(pcnum + " not found in pcsList.");
            }
        }

        public void CurrentInputChange()
        {
            if (pcsList.TryGetValue("PC1", out var pc1))
            {
                //if (pcsList["PC1"].InputType != KvmModule.SelectedHomeDevice.MonitorInfo.inputSource)
                //{
                    bool b = DdpmCommonHelper.DeviceManagerSA.SetVCPCapability(KvmModule.SelectedHomeDevice.MonitorInfo, "Input Select", pcsList["PC1"].InputType).Result;
                    if (b)
                    {
                        KvmModule.SelectedHomeDevice.MonitorInfo.inputSource = pcsList["PC1"].InputType;
                    }
                //}
            }
            else
            {
                _log.Debug("PC1 not found in pcsList.");
            }
        }

        #region Pxp VCP Code

        public const UInt16 PipMode_Off = 0;
        public const UInt16 PipMode_Small = 0x21;
        public const UInt16 PipMode_Large = 0x22;

        public const UInt16 PipMode_SizeToggle = 0x01;
        public const UInt16 PipMode_PositionToggle = 0x02;

        #endregion Pxp VCP Code

        #region PIP/PBP Capabilities

        private UInt16[] _pipPbpCaps = new UInt16[0];
        //public UInt16[] PipPbpCaps { get => _pipPbpCaps; }

        //Call this method when _pipPbpCaps has been changed
        private void OnPipPbpCapsChanged()
        {
            OnPropertyChanged("HasCap_PipSmall");
            OnPropertyChanged("HasCap_PipLarge");
            OnPropertyChanged("HasCap_PipTogglePosition");
            OnPropertyChanged(PC1_Input);
            OnPropertyChanged(PC2_Input);
            OnPropertyChanged(PC3_Input);
            OnPropertyChanged(PC4_Input);
        }

        public bool HasPxpCap(UInt16 cap)
        {
            if (_pipPbpCaps.Length <= 0)
                return false;
            return Array.Exists(_pipPbpCaps, x => x == cap);
        }

        public bool HasCap_PipSmall => (_pipPbpCaps.Length > 0) ? HasPxpCap(PipMode_Small) : false;
        public bool HasCap_PipLarge => (_pipPbpCaps.Length > 0) ? HasPxpCap(PipMode_Large) : false;
        public bool HasCap_PipTogglePosition => (_pipPbpCaps.Length > 0) ? HasPxpCap(PipMode_PositionToggle) : false;

        #endregion PIP/PBP Capabilities

        #region Current PxpMode

        private UInt16 _curPxpMode = 0;
        public UInt16 CurPxpMode => _curPxpMode;

        #endregion Current PxpMode

        #region Selected SplitItems

        private SplitItem? _selectedSplitItem;

        public SplitItem? SelectedSplitItem
        {
            get => _selectedSplitItem;
            set
            {
                //Unselected origainl selection
                if (_selectedSplitItem != null)
                {
                    //If selected item is the same (no changed)
                    if (_selectedSplitItem == value)
                        return;
                    //Selection changed => unselection origial item
                    _selectedSplitItem.IsSelected = false;
                }
                if (value != null)
                {
                    SetProperty(ref _selectedSplitItem, value);
                    _selectedSplitItem.IsSelected = true;
                    //To determine if "Toggle between position" button should be enabled
                    OnPropertyChanged("IsPipListItemSelected");
                }
                else
                {
                    //Selection none
                    SetProperty(ref _selectedSplitItem, value);
                    //To determine if "Toggle between position" button should be enabled
                    OnPropertyChanged("IsPipListItemSelected");
                }
            }
        }

        #endregion Selected SplitItems

        #region Main Input Source

        private InputSourceObj _mainInputSource;

        public InputSourceObj MainInputSource
        {
            get => _mainInputSource;
            set => SetProperty(ref _mainInputSource, value);
        }

        #endregion Main Input Source

        #region Sub Input Sources

        private List<InputSourceObj> _subInputs = new List<InputSourceObj>();

        public List<InputSourceObj> SubInputs
        {
            get => _subInputs;
            set
            {
                SetProperty(ref _subInputs, value);
                OnPropertyChanged("HasSub1Input");
                OnPropertyChanged("HasSub2Input");
                OnPropertyChanged("HasSub3Input");
                if (HasSub1Input)
                    Sub1InputSource = SubInputs[0];
                if (HasSub2Input)
                    Sub2InputSource = SubInputs[1];
                if (HasSub3Input)
                    Sub3InputSource = SubInputs[2];
            }
        }

        private InputSourceObj? _sub1InputSource;

        public InputSourceObj? Sub1InputSource
        {
            get => _sub1InputSource;
            set => SetProperty(ref _sub1InputSource, value);
        }

        private InputSourceObj? _sub2InputSource;

        public InputSourceObj? Sub2InputSource
        {
            get => _sub2InputSource;
            set => SetProperty(ref _sub2InputSource, value);
        }

        private InputSourceObj? _sub3InputSource;

        public InputSourceObj? Sub3InputSource
        {
            get => _sub3InputSource;
            set => SetProperty(ref _sub3InputSource, value);
        }

        public bool HasSub1Input => SubInputs.Count > 0;
        public bool HasSub2Input => SubInputs.Count > 1;
        public bool HasSub3Input => SubInputs.Count > 2;

        #endregion Sub Input Sources

        #region Determine if Toggle between positons button enabled/disabled

        public bool IsPipListItemSelected
        {
            get
            {
                if (SelectedSplitItem == null)
                    return false;
                //All SplitItems should has assign SplitOwner, to check if it's one item of PipList
                if (SelectedSplitItem.SplitOwner == eSplitOwner.PipList)
                    return true;
                return false;
            }
        }

        public bool IsTogglePositionEnabled
        {
            get
            {
                return IsPipListItemSelected && HasCap_PipTogglePosition;
            }
        }

        #endregion Determine if Toggle between positons button enabled/disabled

        #region VideoSwap control and content

        private ContentControl? _videoSwapContent;

        public ContentControl? VideoSwapContent
        {
            get => _videoSwapContent;
            set => SetProperty(ref _videoSwapContent, value);
        }

        public void PCSwap(string PC1, string PC2)
        {
            PCsInfo outpcsInfo;
            pcsList = DdpmCommonHelper.DeviceManagerSA.PCInfoSwap(pcsList, PC1, PC2).Result;
            if (pcsList.TryGetValue("PC1", out outpcsInfo) && pcsList.TryGetValue("PC2", out outpcsInfo))
            {
                PC1_Input = pcsList["PC1"].InputType;
                PC2_Input = pcsList["PC2"].InputType;
                if (pcsList.TryGetValue("PC3", out outpcsInfo))
                {
                    PC3_Input = pcsList["PC3"].InputType;
                    if (pcsList.TryGetValue("PC4", out outpcsInfo))
                    {
                        PC4_Input = pcsList["PC4"].InputType;
                    }
                }
            }
            OnPropertyChanged(PC1_Input);
            OnPropertyChanged(PC2_Input);
            OnPropertyChanged(PC3_Input);
            OnPropertyChanged(PC4_Input);
        }

        #endregion VideoSwap control and content

        private double _fromprogressValue = 0;
        public double FromProgressValue
        {
            get
            {
                return _fromprogressValue;
            }
            set
            {
                _fromprogressValue = value;
                OnPropertyChanged(nameof(FromProgressValue));
            }
        }

        private double _toprogressValue = 1;
        public double ToProgressValue
        {
            get
            {
                return _toprogressValue;
            }
            set
            {
                _toprogressValue = value;
                OnPropertyChanged(nameof(ToProgressValue));
            }
        }

        public void PC1Click()
        {
            Border1Visibility = Visibility.Visible;
            Border2Visibility = Visibility.Collapsed;
            Border3Visibility = Visibility.Collapsed;
            Border4Visibility = Visibility.Collapsed;
            OnPropertyChanged(PC1_Input);
            OnPropertyChanged(PC2_Input);
            OnPropertyChanged(PC3_Input);
            OnPropertyChanged(PC4_Input);
        }

        public void PC2Click()
        {
            Border1Visibility = Visibility.Collapsed;
            Border2Visibility = Visibility.Visible;
            Border3Visibility = Visibility.Collapsed;
            Border4Visibility = Visibility.Collapsed;
            OnPropertyChanged(PC1_Input);
            OnPropertyChanged(PC2_Input);
            OnPropertyChanged(PC3_Input);
            OnPropertyChanged(PC4_Input);
        }

        public void PC3Click()
        {
            Border1Visibility = Visibility.Collapsed;
            Border2Visibility = Visibility.Collapsed;
            Border3Visibility = Visibility.Visible;
            Border4Visibility = Visibility.Collapsed;
            OnPropertyChanged(PC1_Input);
            OnPropertyChanged(PC2_Input);
            OnPropertyChanged(PC3_Input);
            OnPropertyChanged(PC4_Input);
        }

        public void PC4Click()
        {
            Border1Visibility = Visibility.Collapsed;
            Border2Visibility = Visibility.Collapsed;
            Border3Visibility = Visibility.Collapsed;
            Border4Visibility = Visibility.Visible;
            OnPropertyChanged(PC1_Input);
            OnPropertyChanged(PC2_Input);
            OnPropertyChanged(PC3_Input);
            OnPropertyChanged(PC4_Input);
        }

        public void OpenNKVMUI(int index, int x, int y)
        {
            var directory = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            directory = $"C:\\Program Files\\Dell\\Dell Display and Peripheral Manager";
            string strFullPath = string.Format("{0}\\Plugins\\NKVM\\DDM.exe", directory);
            //Elsa Add Security
            string FileInfo;
            if (!DDPMFileSecurity.IsFilePathValid(strFullPath, out FileInfo))
            {
                Trace.WriteLine($"{nameof(OpenNKVMUI)} {FileInfo}");
            }
            Trace.WriteLine($"NKVM full path is {strFullPath}");

            try
            {
                IntPtr NkvmdHandle = IntPtr.Zero;
                string processName = "DDM";
                Process[] processes = Process.GetProcessesByName(processName);

                if (processes.Length == 0)
                {
                    Console.WriteLine("No process found with the name: " + processName);

                    Process procNew = new Process();
                    procNew.StartInfo.FileName = strFullPath;
                    procNew.Start();
                }
                else
                {
                    foreach (Process process in processes)
                    {
                        NkvmdHandle = process.Handle;
                        Console.WriteLine($"Process ID: {process.Id}, Handle: {NkvmdHandle}");
                        break;
                    }
                }

                Process proc = new Process();
                proc.StartInfo.FileName = strFullPath;
                proc.StartInfo.Arguments = $"/ShowNKVM {index} {x} {y}";
                proc.Start();

                //try
                //{
                //    proc.WaitForInputIdle();
                //}
                //catch (Exception)
                //{
                //}

                //if (NkvmdHandle == IntPtr.Zero)
                //{
                //    // Get the handle of the NKVM main window
                //    NkvmdHandle = proc.MainWindowHandle;
                //}

                //Window mainWindow = System.Windows.Application.Current.MainWindow;
                //IntPtr mainWindowHandle = new WindowInteropHelper(mainWindow).Handle;

                //if (NkvmdHandle != IntPtr.Zero && mainWindowHandle != IntPtr.Zero)
                //{
                //    //SetParent(NkvmdHandle, mainWindowHandle);
                //    DDPMWindowPos windowPos = new DDPMWindowPos(NkvmdHandle, mainWindowHandle);
                //    //SetWindowPos(mainWindowHandle, HWND_BOTTOM, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE | SWP_NOACTIVATE | SWP_NOOWNERZORDER | SWP_NOREDRAW);
                //    //EnableWindow(mainWindowHandle, false);
                //}

                //WindowInteropHelper helper = new WindowInteropHelper(mainWindow);
                //helper.Owner = NkvmdHandle;
            }
            catch (System.Exception ex)
            {
                Trace.WriteLine($"ERROR : Run NKVM ==> {ex.ToString()}");
                Thread.Sleep(1000);
            }
        }

        public void isOnUSBKVM(bool ison)
        {
            DdpmCommonHelper.DeviceManagerSA.SetOnUSBKVM(DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo, ison).Wait();
            KvmModule.isUSBKVM = ison;
        }

        public void isOnNKVM(bool ison)
        {
            //if (ison)
            //{
            //    DdpmCommonHelper.DeviceManagerSA.SupportedNKVMMonitors().Wait();
            //}
            DdpmCommonHelper.DeviceManagerSA.SetOnNKVM(DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo, ison).Wait();
        }

        #region KVM Loading

        public void NKVMOpenUI()
        {
            BackgroundWorker bw = new BackgroundWorker()
            {
                WorkerReportsProgress = false,
                WorkerSupportsCancellation = false
            };
            bw.DoWork += NKVMOpenUI_Dowork;
            bw.RunWorkerCompleted += NKVMOpenUI_Done;
            bw.RunWorkerAsync();
            IsBusy = true;
            OnPropertyChanged("IsBusy");
        }
        private void NKVMOpenUI_Dowork(object sender, DoWorkEventArgs e)
        {
            DdpmCommonHelper.DeviceManagerSA.NKVM_State(true).Wait();
            if (!DdpmCommonHelper.DeviceManagerSA.IsNamedpipeConnected().Result)
            {
                DdpmCommonHelper.DeviceManagerSA.CreatNewNamedpipe().Wait();
//#if DEBUG
                DdpmCommonHelper.DeviceManagerSA.CallShowNKVM(0, 100, 100).Wait();
//#else
//                OpenNKVMUI(0, 100, 100);
//#endif
            }
            else
            {
//#if DEBUG
                DdpmCommonHelper.DeviceManagerSA.CallShowNKVM(0, 100, 100).Wait();
//#else
//                OpenNKVMUI(0, 100, 100);
//#endif
            }

            isOnNKVM(true);
        }
        private void NKVMOpenUI_Done(object sender, RunWorkerCompletedEventArgs e)
        {
            IsBusy = false;
            OnPropertyChanged("IsBusy");
        }

#endregion

        public void OnPropertyChanged_Lock()
        {
            OnPropertyChanged("isNKVMEanble");
            OnPropertyChanged("NKVM_Opacity");
            OnPropertyChanged("LockNKVM_Visibility");
            OnPropertyChanged("isUSBKVMEanble");
            OnPropertyChanged("USBKVM_Opacity");
            OnPropertyChanged("LockUSBKVM_Visibility");
        }

        public void FinishtoSetPCs()
        {
            //set input source
            if (pcsList != null)
            {
                if (pcsList != original_pcsList)
                {
                    if (pcsList.TryGetValue("PC1", out var pc1) && pcsList.TryGetValue("PC2", out var pc2))
                    {
                        InputSourceObj pc1input = new InputSourceObj((UInt16)pcsList["PC1"].Code, pcsList["PC1"].InputType);
                        InputSourceObj pc2input = new InputSourceObj((UInt16)pcsList["PC2"].Code, pcsList["PC2"].InputType);
                        inputList[pcsList["PC1"].InputType].InputName = pcsList["PC1"].InputName;
                        inputList[pcsList["PC2"].InputType].InputName = pcsList["PC2"].InputName;
                        if (pcsList.Count >= 3)
                        {
                            if (pcsList.TryGetValue("PC3", out var pc3))
                            {
                                InputSourceObj pc3input = new InputSourceObj((UInt16)pcsList["PC3"].Code, pcsList["PC3"].InputType);
                                inputList[pcsList["PC3"].InputType].InputName = pcsList["PC3"].InputName;
                                if (pcsList.Count == 4)
                                {
                                    if (pcsList.TryGetValue("PC4", out var pc4))
                                    {
                                        InputSourceObj pc4input = new InputSourceObj((UInt16)pcsList["PC4"].Code, pcsList["PC4"].InputType);
                                        inputList[pcsList["PC4"].InputType].InputName = pcsList["PC4"].InputName;
                                        bool res = DdpmCommonHelper.DeviceManagerSA.SetSubInputs(
                                            DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo,
                                            pc2input, pc3input, pc4input).Result;
                                        if (res)
                                        {
                                            Thread.Sleep(1000);
                                        }
                                    }
                                    else
                                    {
                                        _log?.Debug("PC4 not found in pcsList.");
                                    }
                                }
                                else
                                {
                                    bool res = DdpmCommonHelper.DeviceManagerSA.SetSubInputs(
                                        DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo,
                                        pc2input, pc3input, null).Result;
                                    if (res)
                                    {
                                        Thread.Sleep(1000);
                                    }
                                }
                            }
                            else
                            {
                                _log?.Debug("PC3 not found in pcsList.");
                            }
                        }
                        else
                        {
                            bool res = DdpmCommonHelper.DeviceManagerSA.SetSubInputs(
                                    DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo,
                                    pc2input, null, null).Result;
                            if (res)
                            {
                                Thread.Sleep(1000);
                            }
                        }

                        foreach (var pcs in pcsList)
                        {
                            //if (pcs.Key == "PC1" && pcs.Value.InputType != original_pcsList["PC1"].InputType)
                            //{
                            //    CurrentInputChange();
                            //}
                            //if (pcs.Value.InputName != vm.original_pcsList[pcs.Key].InputName)
                            //{
                            //    bool binputname = DdpmCommonHelper.DeviceManagerSA.SetInputName(pcs.Value.InputType, pcs.Value.InputName).Result;
                            //}
                            if (pcs.Value.USBUpstream != original_pcsList[pcs.Key].USBUpstream)
                            {
                                bool bUSBuptream = DdpmCommonHelper.DeviceManagerSA.SetUSBUpstream(KvmModule.SelectedHomeDevice.MonitorInfo, pcs.Value.InputType, pcs.Value.USBUpstream).Result;
                                if (bUSBuptream)
                                {
                                    Thread.Sleep(1000);
                                }
                            }
                        }

                        if (pcsList["PC1"].InputType != KvmModule.SelectedHomeDevice.MonitorInfo.inputSource)
                        {
                            CurrentInputChange();
                        }
                        bool bin = DdpmCommonHelper.DeviceManagerSA.SetInputSourcelist(KvmModule.SelectedHomeDevice.MonitorInfo, inputList).Result;
                        //bool bpcs = DdpmCommonHelper.DeviceManagerSA.SetUSBKVMPCsList(KvmModule.SelectedHomeDevice.MonitorInfo, pcsList).Result;
                        //isOnUSBKVM(true);//bool b = DdpmCommonHelper.DeviceManagerSA.SetOnUSBKVM(true).Result;
                    }
                    else
                    {
                        _log?.Debug("PC1 or PC2 not found in pcsList.");
                    }
                }
            }
            else
            {
                _log?.Debug("pcsList is null");
            }
        }

        public void PCInputChange()
        {
            OnPropertyChanged("PC1Inputs_Selected");
            OnPropertyChanged("PC2Inputs_Selected");
            OnPropertyChanged("PC3Inputs_Selected");
            OnPropertyChanged("PC4Inputs_Selected");
            OnPropertyChanged("PCImage");
        }

        #region Event
        /// <summary>
        /// Catch OSD menu event
        /// </summary>
        /// <param name="sender">object type</param>
        /// <param name="e">changed event</param>
        private void OnVCPChangedEvent(object? sender, VCPchangedEventArgs e)
        {
            if (e.vcpcode.Equals("E7"))
            {
                pcsList = new Dictionary<string, PCsInfo>();
                usbsList = new List<string>();
                pcsList = DdpmCommonHelper.DeviceManagerSA.GetUSBKVMPCsList(KvmModule.SelectedHomeDevice.MonitorInfo, inputList, subInputs).Result;
                usbsList = DdpmCommonHelper.DeviceManagerSA.GetUSBUpstreamList(KvmModule.SelectedHomeDevice.MonitorInfo).Result;
                if ((inputList != null) && (pcsList.Count > 0))   // 2024-06-19 Elie, fix exception.
                {
                    string pathData = string.Empty;
                    _usbsList.Clear();
                    if (usbsList != null && usbsList.Count != 0)
                    {
                        foreach (string str in usbsList)
                        {
                            pathData = InputTypeCommon.GetInputImage(str);
                            _usbsList.Add(new USBList()
                            {
                                Type = str,
                                PathData = pathData,
                                kvmModule = KvmModule
                            });
                        }
                        USBsList = _usbsList;
                        if (pcsList.TryGetValue("PC1", out var pc1) && pcsList.TryGetValue("PC2", out var pc2))
                        {
                            _PC1selectUSB = _usbsList.Find(x => (x.Type == pcsList["PC1"].USBUpstream));
                            _PC2selectUSB = _usbsList.Find(x => (x.Type == pcsList["PC2"].USBUpstream));
                            if (pcsList.Count >= 3)
                            {
                                if (pcsList.TryGetValue("PC3", out var pc3))
                                {
                                    _PC3selectUSB = _usbsList.Find(x => (x.Type == pcsList["PC3"].USBUpstream));
                                    if (pcsList.Count == 4)
                                    {
                                        if (pcsList.TryGetValue("PC4", out var pc4))
                                        {
                                            _PC4selectUSB = _usbsList.Find(x => (x.Type == pcsList["PC4"].USBUpstream));
                                        }
                                        else
                                        {
                                            _log?.Debug("PC4 not found in pcsList.");
                                        }
                                    }
                                }
                                else
                                {
                                    _log?.Debug("PC3 not found in pcsList.");
                                }
                            }
                            OnPropertyChanged("PC1USB_Selected");
                            OnPropertyChanged("PC2USB_Selected");
                            OnPropertyChanged("PC3USB_Selected");
                            OnPropertyChanged("PC4USB_Selected");
                        }
                        else
                        {
                            _log?.Debug("PC1 or PC2 not found in pcsList.");
                        }
                    }
                    else
                    {
                        _log?.Debug("[KvmViewModel] usbsList is null or count is 0");
                    }
                }
                else
                {
                    _log?.Debug("[KvmViewModel] inputList is null or count is 0");
                }
            }
        }
        #endregion
    }
}