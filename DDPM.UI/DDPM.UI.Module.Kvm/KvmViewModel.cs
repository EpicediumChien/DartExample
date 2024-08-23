using CommunityToolkit.Mvvm.ComponentModel;
using DDPM.SA.Common;
using DDPM.SA.Common.Display;
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
using VcpCore.Common;
using Windows.System;

namespace DDPM.UI.Module.Kvm
{
    public class InputSourceList
    {
        public string inputSource = String.Empty;
        public KvmModule kvmModule { get; set; }

        public string inputDisplayText
        {
            get
            {
                return inputSource;
            }
        }
    }

    public class USBList
    {
        public string usb = String.Empty;
        public KvmModule kvmModule { get; set; }

        public string inputDisplayText
        {
            get
            {
                return usb;
            }
        }
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
        public Visibility SupportUSBKVM { get; set; } = Visibility.Visible;
        public Visibility SupportNKVM { get; set; } = Visibility.Visible;
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
                    if (USBKVMisON)
                    {
                        isOnUSBKVM(false);
                    }
                    if (_isNKVM)
                    {
                        _isNKVM = false;
                        isOnNKVM(false);
                    }
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
                    if (_isNKVM)
                    {
                        _isNKVM = false;
                        isOnNKVM(false);
                    }
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
                    if (USBKVMisON)
                    {
                        isOnUSBKVM(false);
                    }
                    _isNoKVM = false;
                    _isNKVM = true;
                    isOnNKVM(true);
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
                SelectInputSource(_PC1selectInput.inputSource, "PC1");
            }
        }

        public InputSourceList PC2Inputs_Selected
        {
            get => _PC2selectInput;
            set
            {
                SetProperty(ref _PC2selectInput, value);
                SelectInputSource(_PC2selectInput.inputSource, "PC2");
            }
        }

        public InputSourceList PC3Inputs_Selected
        {
            get => _PC3selectInput;
            set
            {
                SetProperty(ref _PC3selectInput, value);
                SelectInputSource(_PC3selectInput.inputSource, "PC3");
            }
        }

        public InputSourceList PC4Inputs_Selected
        {
            get => _PC4selectInput;
            set
            {
                SetProperty(ref _PC4selectInput, value);
                SelectInputSource(_PC4selectInput.inputSource, "PC4");
            }
        }

        public USBList PC1USB_Selected
        {
            get => _PC1selectUSB;
            set
            {
                SetProperty(ref _PC1selectUSB, value);
                SelectUSB(_PC1selectUSB.usb, "PC1");
            }
        }

        public USBList PC2USB_Selected
        {
            get => _PC2selectUSB;
            set
            {
                SetProperty(ref _PC2selectUSB, value);
                SelectUSB(_PC2selectUSB.usb, "PC2");
            }
        }

        public USBList PC3USB_Selected
        {
            get => _PC3selectUSB;
            set
            {
                SetProperty(ref _PC3selectUSB, value);
                SelectUSB(_PC3selectUSB.usb, "PC3");
            }
        }

        public USBList PC4USB_Selected
        {
            get => _PC4selectUSB;
            set
            {
                SetProperty(ref _PC4selectUSB, value);
                SelectUSB(_PC4selectUSB.usb, "PC4");
            }
        }

        public ImageSource? PCImage
        {
            get => _PCImage;
            set => SetProperty(ref _PCImage, value);
        }

        public string? ConnectionType { get; set; }
        public double? BatteryLevel { get; set; }
        public string? BatteryStatus { get; set; }
        public bool? NoBattery { get; set; }
        public string? Text1 { get; set; }

        #region Hotkey

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

        private void saveKvmHotkeyOption()
        {
            HotkeySettings hotkeySettings = new HotkeySettings
            {
                DeviceInfo = KvmModule.SelectedHomeDevice.MonitorInfo.edid,
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
                HotkeySettings curHotkey = DdpmCommonHelper.DeviceManagerSA.ReadCurrentHotkey(KvmModule.SelectedHomeDevice.MonitorInfo.edid).Result;
                //0708 error handling for non-EE support monitor
                string swHortcutText = string.Empty;
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
                }
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

        #region UI Enable Flags

        private bool _isBusy = false;

        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        #endregion UI Enable Flags

        public void Invoke_RefreshData()
        {
            BackgroundWorker bw = new BackgroundWorker()
            {
                WorkerReportsProgress = false,
                WorkerSupportsCancellation = false
            };
            bw.DoWork += DoWork_RefreshData;
            bw.RunWorkerCompleted += RunWorkerCompleted_RefreshData;
            bw.RunWorkerAsync(); //myArg is the optional argument
            IsBusy = true;
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
                if (DdpmCommonHelper.DeviceManagerSA.isNKVMSupportMonitor(mi).Result == false)
                {
                    SupportNKVM = Visibility.Collapsed;
                }

                USBKVMisON = KvmModule.isUSBKVM;//DdpmCommonHelper.DeviceManagerSA.GetOnNKVM().Result;
                NKVMisON = DdpmCommonHelper.DeviceManagerSA.GetOnNKVM(selHomeDevice.MonitorInfo).Result;
                if (mi.CapabilityDic.ContainsKey("EE"))
                {
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

                    //If arg is specified, you can get it with below code
                    //myArgType arg = (myArgType)e.Argument;
                    inputList = new Dictionary<string, InputInfo>();
                    subInputs = new List<InputSourceObj>();
                    usbsList = new List<string>();
                    //Dictionary<string, PCsInfo> pcsList = new Dictionary<string, PCsInfo>();
                    inputList = DdpmCommonHelper.DeviceManagerSA.GetInputSourcelist(KvmModule.SelectedHomeDevice.MonitorInfo).Result;
                    subInputs = DdpmCommonHelper.DeviceManagerSA.GetSubInputs(KvmModule.SelectedHomeDevice.MonitorInfo).Result;
                    string currentinput = KvmModule.SelectedHomeDevice.MonitorInfo.inputSource;
                    pcsList = DdpmCommonHelper.DeviceManagerSA.GetUSBKVMPCsList(KvmModule.SelectedHomeDevice.MonitorInfo, inputList, subInputs).Result;
                    usbsList = DdpmCommonHelper.DeviceManagerSA.GetUSBUpstreamList(KvmModule.SelectedHomeDevice.MonitorInfo).Result;
                    original_pcsList = DdpmCommonHelper.DeviceManagerSA.GetUSBKVMPCsList(KvmModule.SelectedHomeDevice.MonitorInfo, inputList, subInputs).Result;
                    if ((inputList != null) && (pcsList.Count > 0))   // 2024-06-19 Elie, fix exception.
                    {
                        if (inputList.Count != _inputsList.Count && usbsList.Count != _usbsList.Count)
                        {
                            foreach (string item in inputList.Keys)
                            {
                                _inputsList.Add(new InputSourceList()
                                {
                                    inputSource = item,
                                    kvmModule = KvmModule
                                });
                            }
                            foreach (string str in usbsList)
                            {
                                _usbsList.Add(new USBList()
                                {
                                    usb = str,
                                    kvmModule = KvmModule
                                });
                            }
                            PCInputsList = _inputsList;
                            USBsList = _usbsList;
                            _PC1selectInput = _inputsList.Find(x => (x.inputSource == KvmModule.SelectedHomeDevice.MonitorInfo.inputSource));
                            _PC1selectUSB = _usbsList.Find(x => (x.usb == pcsList["PC1"].USBUpstream));
                            _PC2selectInput = _inputsList.Find(x => (x.inputSource == pcsList["PC2"].InputType));
                            _PC2selectUSB = _usbsList.Find(x => (x.usb == pcsList["PC2"].USBUpstream));
                            InputName1 = pcsList["PC1"].InputName;
                            InputName2 = pcsList["PC2"].InputName;
                            if (pcsList.Count >= 3)
                            {
                                _PC3selectInput = _inputsList.Find(x => (x.inputSource == pcsList["PC3"].InputType));
                                _PC3selectUSB = _usbsList.Find(x => (x.usb == pcsList["PC3"].USBUpstream));
                                InputName3 = pcsList["PC3"].InputName;
                                PC3_Visibility = Visibility.Visible;
                                if (pcsList.Count == 4)
                                {
                                    _PC4selectInput = _inputsList.Find(x => (x.inputSource == pcsList["PC4"].InputType));
                                    _PC4selectUSB = _usbsList.Find(x => (x.usb == pcsList["PC4"].USBUpstream));
                                    InputName4 = pcsList["PC4"].InputName;
                                    PC4_Visibility = Visibility.Visible;
                                    PCImage = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/USBKVM_4PCs.png");
                                }
                                else
                                {
                                    PCImage = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/USBKVM_3PCs.png");
                                }
                            }
                            else
                            {
                                PCImage = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/USBKVM_2PCs.png");
                            }
                        }
                    }
                    OnPropertyChanged("PC1Inputs_Selected");
                    OnPropertyChanged("PC2Inputs_Selected");
                    OnPropertyChanged("PC3Inputs_Selected");
                    OnPropertyChanged("PC4Inputs_Selected");
                    OnPropertyChanged("PCImage");

                    #region PIP/PIP

                    //Get the Pxp Capabilities
                    _pipPbpCaps = DdpmCommonHelper.DeviceManagerSA.GetPipPbpCapabilitiesWords(mi).Result;
                    OnPipPbpCapsChanged();

                    //Check if this monitor has Pxp mode capabilities
                    if ((_pipPbpCaps != null) && (_pipPbpCaps.Length > 0))
                    {
                        //Get current monitor's Pxp mode
                        ObjGetVCP ret = DdpmCommonHelper.DeviceManagerSA.GetPxpMode(mi).Result;
                        if (ret.result)
                        {
                            //UInt64 u64 = (UInt64)ret.result;
                            _curPxpMode = Convert.ToUInt16(ret.value);
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

                    #endregion PIP/PIP

                    //ConnectionType = KvmModule.SelectedHomeDevice.ConnectionType;
                    //BatteryLevel = KvmModule.SelectedHomeDevice.BatteryLevel;
                    //BatteryStatus = KvmModule.SelectedHomeDevice.BatteryStatus;
                    //NoBattery = KvmModule.SelectedHomeDevice.NoBattery;
                    //Text1 = KvmModule.SelectedHomeDevice.Text1;
                    //OnPropertyChanged("ConnectionType");
                    //OnPropertyChanged("BatteryLevel");
                    //OnPropertyChanged("BatteryStatus");
                    //OnPropertyChanged("NoBattery");
                    //OnPropertyChanged("Text1");
                }
                else
                {
                    SupportUSBKVM = Visibility.Collapsed;
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
                    break;
                }
            }
            pcsList[pcnum] = pcInfo;
            //bool b = DdpmCommonHelper.DeviceManagerSA.SetUSBKVMPCsList(KvmModule.SelectedHomeDevice.MonitorInfo, pcsList).Result;
            OnPropertyChanged("PC1Inputs_Selected");
            OnPropertyChanged("PC2Inputs_Selected");
            OnPropertyChanged("PC3Inputs_Selected");
            OnPropertyChanged("PC4Inputs_Selected");
        }

        private void SelectUSB(string usb, string pcnum)
        {
            pcsList[pcnum].USBUpstream = usb;
            //bool b = DdpmCommonHelper.DeviceManagerSA.SetUSBKVMPCsList(KvmModule.SelectedHomeDevice.MonitorInfo, pcsList).Result;
            OnPropertyChanged("PC1USB_Selected");
            OnPropertyChanged("PC2USB_Selected");
            OnPropertyChanged("PC3USB_Selected");
            OnPropertyChanged("PC4USB_Selected");
        }

        public void CurrentInputChange()
        {
            if (pcsList["PC1"].InputType != KvmModule.SelectedHomeDevice.MonitorInfo.inputSource)
            {
                bool b = DdpmCommonHelper.DeviceManagerSA.SetVCPCapability(KvmModule.SelectedHomeDevice.MonitorInfo, "Input Select", pcsList["PC1"].InputType).Result;
                if (b)
                {
                    KvmModule.SelectedHomeDevice.MonitorInfo.inputSource = pcsList["PC1"].InputType;
                }
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

                try
                {
                    proc.WaitForInputIdle();
                }
                catch (Exception)
                {
                }

                if (NkvmdHandle == IntPtr.Zero)
                {
                    // Get the handle of the NKVM main window
                    NkvmdHandle = proc.MainWindowHandle;
                }

                Window mainWindow = System.Windows.Application.Current.MainWindow;
                IntPtr mainWindowHandle = new WindowInteropHelper(mainWindow).Handle;

                if (NkvmdHandle != IntPtr.Zero && mainWindowHandle != IntPtr.Zero)
                {
                    //SetParent(NkvmdHandle, mainWindowHandle);
                    DDPMWindowPos windowPos = new DDPMWindowPos(NkvmdHandle, mainWindowHandle);
                    //SetWindowPos(mainWindowHandle, HWND_BOTTOM, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE | SWP_NOACTIVATE | SWP_NOOWNERZORDER | SWP_NOREDRAW);
                    //EnableWindow(mainWindowHandle, false);
                }

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
            if (ison)
            {
                DdpmCommonHelper.DeviceManagerSA.SupportedNKVMMonitors().Wait();
            }
            DdpmCommonHelper.DeviceManagerSA.SetOnNKVM(DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo, ison).Wait();
        }
    }
}