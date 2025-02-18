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
using Microsoft.VisualBasic.Logging;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;
using Newtonsoft.Json.Linq;

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
        private List<InputSourceList> _inputsList1 = new List<InputSourceList>();
        private List<InputSourceList> _inputsList2 = new List<InputSourceList>();
        private List<InputSourceList> _inputsList3 = new List<InputSourceList>();
        private List<InputSourceList> _inputsList4 = new List<InputSourceList>();
        private List<USBList> _usbsList = new List<USBList>();
        private PCInput pcInput = new PCInput();
        private bool _isNoKVM = false;
        private bool _isUSBKVM = false;
        private bool _isNKVM = false;
        //private static Log _log;

        //private Dictionary<string, PCsInfo> pcsList = new Dictionary<string, PCsInfo>();
        private ImageSource? _PCImage;
        #endregion

        #region Win32
        /*[DllImport("user32.dll", EntryPoint = "SetParent", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern int SetParent(IntPtr hWndChild, IntPtr hWndNewParent);
        private static int _SetParent(IntPtr hWndChild, IntPtr hWndNewParent)
        {
            return SetParent(hWndChild, hWndNewParent);
        }*/

        /*[DllImport("user32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern bool EnableWindow(IntPtr hWnd, bool bEnable);
        private static bool _EnableWindow(IntPtr hWnd, bool bEnable)
        {
            return EnableWindow(hWnd, bEnable);
        }*/
        #endregion Win32

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
        public Visibility DisenableUSBKVM { get; set; } = Visibility.Visible;
        public Visibility EnableUSBKVM { get; set; } = Visibility.Collapsed;
        private string _pc1_Input { get; set; }
        public string PC1_Input
        {
            get => _pc1_Input;
            set
            {
                _pc1_Input = value;
                OnPropertyChanged();
            }
        }
        private string _pc2_Input { get; set; }
        public string PC2_Input
        {
            get => _pc2_Input;
            set
            {
                _pc2_Input = value;
                OnPropertyChanged();
            }
        }
        private string _pc3_Input { get; set; }
        public string PC3_Input
        {
            get => _pc3_Input;
            set
            {
                _pc3_Input = value;
                OnPropertyChanged();
            }
        }
        private string _pc4_Input { get; set; }
        public string PC4_Input
        {
            get => _pc4_Input;
            set
            {
                _pc4_Input = value;
                OnPropertyChanged();
            }
        }

        public bool IsMoreThanPC2
        {
            get
            {
                return PC3_Visibility == Visibility.Visible;
            }
        }

        public Dictionary<UInt16, System.Windows.Controls.UserControl> PxPcodeDictionary = new Dictionary<UInt16, System.Windows.Controls.UserControl>()
        {
            [0x0] = null,
            [0x21] = null,
            [0x22] = null
            //    [0x24] = new PBPSplitCtrl2A(),
            //    [0x2F] = new PBPSplitCtrl2B(),
            //    [0x26] = new PBPSplitCtrl2C(),
            //    [0x28] = new PBPSplitCtrl2C(),
            //    [0x2A] = new PBPSplitCtrl2C(),
            //    [0x2C] = new PBPSplitCtrl2C(),
            //    [0x2E] = new PBPSplitCtrl2C(),
            //    [0x25] = new PBPSplitCtrl2D(),
            //    [0x27] = new PBPSplitCtrl2D(),
            //    [0x29] = new PBPSplitCtrl2D(),
            //    [0x2B] = new PBPSplitCtrl2D(),
            //    [0x2D] = new PBPSplitCtrl2D(),
            //    [0x31] = new PBPSplitCtrl3E(),
            //    [0x32] = new PBPSplitCtrl3D(),
            //    [0x33] = new PBPSplitCtrl3H(),
            //    [0x34] = new PBPSplitCtrl3B(),
            //    [0x35] = new PBPSplitCtrl3I(),
            //    [0x41] = new PBPSplitCtrl4A(),
            //    [0x42] = new PBPSplitCtrl4D()
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
                //if (value)
                //{
                //    isOnUSBKVM(false);
                //    USBKVMisON = false;
                //    //isUSBKVM = false;
                //    //isNKVM = false;
                //}
            }
        }

        public bool isUSBKVM
        {
            get => _isUSBKVM;
            set
            {
                SetProperty(ref _isUSBKVM, value);
                //if (value)
                //{
                //    isNKVM = false;
                //    isNoKVM = false;
                //}
            }
        }

        public bool isNKVM
        {
            get => _isNKVM;
            set
            {
                SetProperty(ref _isNKVM, value);
                //if (value)
                //{
                //    isOnUSBKVM(false);
                //    USBKVMisON = false;
                //    //isUSBKVM = false;
                //    //isNoKVM = false;
                //}
            }
        }

        public List<InputSourceList> PCInputsList
        {
            get => _inputsList;
            set => SetProperty(ref _inputsList, value);
        }

        public List<InputSourceList> PC1InputsList
        {
            get => _inputsList1;
            set => SetProperty(ref _inputsList1, value);
        }

        public List<InputSourceList> PC2InputsList
        {
            get => _inputsList2;
            set => SetProperty(ref _inputsList2, value);
        }

        public List<InputSourceList> PC3InputsList
        {
            get => _inputsList3;
            set => SetProperty(ref _inputsList3, value);
        }

        public List<InputSourceList> PC4InputsList
        {
            get => _inputsList4;
            set => SetProperty(ref _inputsList4, value);
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
                if (_PC1selectInput != value)
                {
                    SetProperty(ref _PC1selectInput, value);
                    SelectInputSource(_PC1selectInput.Type, "PC1");
                }
            }
        }

        public InputSourceList PC2Inputs_Selected
        {
            get => _PC2selectInput;
            set
            {
                if (_PC2selectInput != value)
                {
                    SetProperty(ref _PC2selectInput, value);
                    SelectInputSource(_PC2selectInput.Type, "PC2");
                }
            }
        }

        public InputSourceList PC3Inputs_Selected
        {
            get => _PC3selectInput;
            set
            {
                if (_PC3selectInput != value)
                {
                    SetProperty(ref _PC3selectInput, value);
                    SelectInputSource(_PC3selectInput.Type, "PC3");
                }
            }
        }

        public InputSourceList PC4Inputs_Selected
        {
            get => _PC4selectInput;
            set
            {
                if (_PC4selectInput != value)
                {
                    SetProperty(ref _PC4selectInput, value);
                    SelectInputSource(_PC4selectInput.Type, "PC4");
                }
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

        public bool isNKVMEanble { get; set; } = true;
        public double NKVM_Opacity { get; set; } = 1;
        //public Visibility LockNKVM_Visibility { get; set; } = Visibility.Collapsed;

        public bool isUSBKVMEanble { get; set; } = true;
        public double USBKVM_Opacity { get; set; } = 1;
        public Visibility LockUSBKVM_Visibility { get; set; } = Visibility.Collapsed;
        public Visibility isPxP { get; set; } = Visibility.Collapsed;
        public Visibility NoPxP { get; set; } = Visibility.Collapsed;

        public bool isPxPFullView { get; set; } = false;

        public bool PC1USB_Enable { get; set; } = true;
        public bool PC2USB_Enable { get; set; } = true;
        public bool PC3USB_Enable { get; set; } = true;
        public bool PC4USB_Enable { get; set; } = true;

        public double PC1USB_Opacity { get; set; } = 1;
        public double PC2USB_Opacity { get; set; } = 1;
        public double PC3USB_Opacity { get; set; } = 1;
        public double PC4USB_Opacity { get; set; } = 1;

        public bool isScreenPartition = false;

        public bool LeftButtonEnable { get; set; } = true;

        #region Hotkey

        private string _kvmHotkeyTooltip = LangHelper.Instance["None"];

        public string KvmHotkeyTooltip
        {
            get => _kvmHotkeyTooltip;
            set
            {
                SetProperty(ref _kvmHotkeyTooltip, value);
                OnPropertyChanged("KvmHotkeyTooltip");
            }
        }

        private string _switchPCsKey = LangHelper.Instance["None"];

        public string SwitchPCsKey
        {
            get => _switchPCsKey;
            set
            {
                SetProperty(ref _switchPCsKey, value);
                OnPropertyChanged("SwitchPCsKey");
            }
        }

        private string _changePipKey = LangHelper.Instance["None"];

        public string ChangePipKey
        {
            get => _changePipKey;
            set
            {
                SetProperty(ref _changePipKey, value);
                OnPropertyChanged("ChangePipKey");
            }
        }

        private string _switchKbMsKey = LangHelper.Instance["None"];

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
            _log.Info("[KvmViewModel] Invoke_RefreshHotkeySettings start");
            DateTime entryUSBKVM = DateTime.Now;
            _log.Info($"[Invoke_RefreshHotkeySettings Time]:{entryUSBKVM.ToString("yyyy-MM-dd hh:mm:ss.fff")}");
            BackgroundWorker bw = new BackgroundWorker()
            {
                WorkerReportsProgress = false,
                WorkerSupportsCancellation = false
            };
            bw.DoWork += DoWork_RefreshHotkeyData;
            bw.RunWorkerCompleted += RunWorkerCompleted_RefreshHotkeyData;
            bw.RunWorkerAsync(); //myArg is the optional argument
            _log.Info("[KvmViewModel] Invoke_RefreshHotkeySettings end");
            entryUSBKVM = DateTime.Now;
            _log.Info($"[Invoke_RefreshHotkeySettings Time]:{entryUSBKVM.ToString("yyyy-MM-dd hh:mm:ss.fff")}");
        }

        private void DoWork_RefreshHotkeyData(object sender, DoWorkEventArgs e)
        {
            _log.Info("[KvmViewModel] DoWork_RefreshHotkeyData start");
            DateTime entryUSBKVM = DateTime.Now;
            _log.Info($"[DoWork_RefreshHotkeyData Time]:{entryUSBKVM.ToString("yyyy-MM-dd hh:mm:ss.fff")}");
            try
            {
                //sender is the ‘bw’ object
                BackgroundWorker bwk = (BackgroundWorker)sender;
                //load hotkey setting
                if (DdpmCommonHelper.DeviceManagerSA != null)
                {
                    HomeDevice? selectedHomeDevice = KvmModule?.SelectedHomeDevice;
                    var temp = DdpmCommonHelper.DeviceManagerSA.ReadCurrentHotkey(selectedHomeDevice?.MonitorInfo).Result;
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

                        if (selectedHomeDevice != null && selectedHomeDevice.HasCapability_PipPbp)
                        {
                            //Robert_Lin 2025-2-3 Change the displayed tooltips
                            // PxpMode          Tooltip example:
                            // Off              "Hotkeys - Switch between PCs: None"
                            // PIP or PBP mode  "Hotkeys - Switch Keyboard and Mouse: None
                            //                   Hotkeys - Change PIP position: None"
                            if (_curPxpMode == 0)
                            {
                                ObjGetVCP ret_PxP = DdpmCommonHelper.DeviceManagerSA.GetPxpMode(selectedHomeDevice.MonitorInfo).Result;
                                if (ret_PxP != null && ret_PxP.result)
                                {
                                    UInt16 curPxpMode = Convert.ToUInt16(ret_PxP.value);
                                    if (curPxpMode != 0)
                                    {
                                        //Robert_Lin 2025-2-3 
                                        //OLD:
                                        //_kvmHotkeyTooltip = $"{HeadCaption} - {SwitchPCsKeyCaption}: {SwitchPCsKey}\r\n{HeadCaption} - {SwitchKbMsKeyCaption}: {SwitchKbMsKey}\r\n{HeadCaption} - {ChangePipKeyCaption}: {ChangePipKey}";
                                        //NEW:
                                        //
                                        _kvmHotkeyTooltip = $"{HeadCaption} - {HeadCaption} - {SwitchKbMsKeyCaption}: {SwitchKbMsKey}\r\n{HeadCaption} - {ChangePipKeyCaption}: {ChangePipKey}";
                                    }
                                    else
                                    {
                                        _kvmHotkeyTooltip = $"{HeadCaption} - {SwitchPCsKeyCaption}: {SwitchPCsKey}";
                                    }
                                }
                            }
                            else
                            {
                                //Robert_Lin 2025-2-3 
                                //OLD:
                                //_kvmHotkeyTooltip = $"{HeadCaption} - {SwitchPCsKeyCaption}: {SwitchPCsKey}\r\n{HeadCaption} - {SwitchKbMsKeyCaption}: {SwitchKbMsKey}\r\n{HeadCaption} - {ChangePipKeyCaption}: {ChangePipKey}";
                                //NEW:
                                _kvmHotkeyTooltip = $"{HeadCaption} - {SwitchKbMsKeyCaption}: {SwitchKbMsKey}\r\n{HeadCaption} - {ChangePipKeyCaption}: {ChangePipKey}";
                            }
                        }
                        else
                        {
                            _kvmHotkeyTooltip = $"{HeadCaption} - {SwitchPCsKeyCaption}: {SwitchPCsKey}";
                        }
                    }
                    else
                    {
                        string StrNone = LangHelper.Instance["None"];
                        if (selectedHomeDevice != null && selectedHomeDevice.HasCapability_PipPbp)
                        {
                            if (_curPxpMode == 0)
                            {
                                ObjGetVCP ret_PxP = DdpmCommonHelper.DeviceManagerSA.GetPxpMode(selectedHomeDevice.MonitorInfo).Result;
                                if (ret_PxP != null && ret_PxP.result)
                                {
                                    UInt16 curPxpMode = Convert.ToUInt16(ret_PxP.value);
                                    if (curPxpMode != 0)
                                    {
                                        //Robert_Lin 2025-2-3 
                                        //OLD:
                                        //_kvmHotkeyTooltip = $"{HeadCaption} - {SwitchPCsKeyCaption}: {SwitchPCsKey}\r\n{HeadCaption} - {SwitchKbMsKeyCaption}: {StrNone}\r\n{HeadCaption} - {ChangePipKeyCaption}: {StrNone}";
                                        //NEW:
                                        _kvmHotkeyTooltip = $"{HeadCaption} - {SwitchKbMsKeyCaption}: {StrNone}\r\n{HeadCaption} - {ChangePipKeyCaption}: {StrNone}";
                                    }
                                    else
                                    {
                                        _kvmHotkeyTooltip = $"{HeadCaption} - {SwitchPCsKeyCaption}: {StrNone}";
                                    }
                                }
                            }
                            else
                            {
                                //Robert_Lin 2025-2-3 
                                //OLD:
                                //_kvmHotkeyTooltip = $"{HeadCaption} - {SwitchPCsKeyCaption}: {SwitchPCsKey}\r\n{HeadCaption} - {SwitchKbMsKeyCaption}: {StrNone}\r\n{HeadCaption} - {ChangePipKeyCaption}: {StrNone}";
                                //NEW:
                                _kvmHotkeyTooltip = $"{HeadCaption} - {SwitchKbMsKeyCaption}: {StrNone}\r\n{HeadCaption} - {ChangePipKeyCaption}: {StrNone}";
                            }

                        }
                        else
                        {
                            _kvmHotkeyTooltip = $"{HeadCaption} - {SwitchPCsKeyCaption}: {StrNone}";
                        }
                    }
                    if (curHotkey.HotkeyOptions.Count > 0 && 
                        curHotkey.HotkeyOptions.Any(x => x.Equals(HotkeyOption.KvmAutoApply)) && 
                        selectedHomeDevice != null && selectedHomeDevice.MonitorInfo != null)
                    {
                        if (IsPBPMode(selectedHomeDevice.MonitorInfo, _curPxpMode))
                        {
                            _autoSwitchChecked = true;
                        }
                        else
                        {
                            _autoSwitchChecked = false;
                        }
                    }
                }

                OnPropertyChanged("KvmHotkeyTooltip");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            _log.Info("[KvmViewModel] DoWork_RefreshHotkeyData end");
            entryUSBKVM = DateTime.Now;
            _log.Info($"[DoWork_RefreshHotkeyData Time]:{entryUSBKVM.ToString("yyyy-MM-dd hh:mm:ss.fff")}");
        }

        private bool IsPBPMode(MonitorInfo mo, UInt16 curPxpMode)
        {
            bool ret = false;
            UInt16 pxpModeValue = 0;
            if (!mo.CapabilityDic.ContainsKey("E9"))
                return ret;
            if (curPxpMode == 0 && 
                DdpmCommonHelper.DeviceManagerSA != null)
            {
                ObjGetVCP ret_PxP = DdpmCommonHelper.DeviceManagerSA.GetPxpMode(mo).Result;
                if (ret_PxP != null && ret_PxP.result)
                {
                    //pxpModeValue = Convert.ToUInt16(ret_PxP.value);
                    if (!ushort.TryParse(ret_PxP.value.ToString(), out pxpModeValue))
                    {
                        pxpModeValue = curPxpMode;
                    }
                }
                else
                {
                    pxpModeValue = curPxpMode;
                }
            }
            switch (pxpModeValue)
            {
                case 0x00://off
                    ret = false;
                    break;
                case 0x21://PIP small
                    ret = false;
                    break;

                case 0x22://PIP large
                    ret = false;
                    break;
                case 0x23:
                case 0x24:
                case 0x25:
                case 0x26:
                case 0x27:
                case 0x28:
                case 0x29:
                case 0x2A:
                case 0x2B:
                case 0x2C:
                case 0x2D:
                case 0x2E:
                case 0x2F:
                case 0x31:
                case 0x32:
                case 0x33:
                case 0x34:
                case 0x35:
                case 0x41:
                case 0x42:
                    ret = true;
                    break;
                default:
                    ret = false;
                    break;
            }
            return ret;
        }
        private void RunWorkerCompleted_RefreshHotkeyData(object sender, RunWorkerCompletedEventArgs e)
        {
            _log.Info("[KvmViewModel] RunWorkerCompleted_RefreshHotkeyData start");
            DateTime entryUSBKVM = DateTime.Now;
            _log.Info($"[RunWorkerCompleted_RefreshHotkeyData Time]:{entryUSBKVM.ToString("yyyy-MM-dd hh:mm:ss.fff")}");
            Debug.WriteLine("load kvm hotkey setting done");
            //Handling the result and final process
            OnPropertyChanged("IsSwitchPCsVisible");
            _log.Info("[KvmViewModel] RunWorkerCompleted_RefreshHotkeyData end");
            entryUSBKVM = DateTime.Now;
            _log.Info($"[RunWorkerCompleted_RefreshHotkeyData Time]:{entryUSBKVM.ToString("yyyy-MM-dd hh:mm:ss.fff")}");
        }

        //Robert_Lin 2025-1-20 for [PIMS-339501] [DDPM Win 2.0][R19] USB KVM Setup -It doesn't show "Switch between PCs" hotkey when set up to PIP mode finished
        //Rquirements:
        //                                    PIP mode               PBP mode              Fullscreen/Single display
        //Switch between PCs                  Collapsed              Collapsed             Visible
        //Switch keyboard and mouse           Visible                Visible               Collapsed
        //Change PIP psition                  Visible                Visible               Collased
        //[ ] Automatically ....              Visible & Disabled     Visible & Enabled     Visible & Disabled
        public bool IsSwitchPCsVisible
        {
            get
            {
                return (_curPxpMode == PipMode_Off);
            }
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

            //Robert_Lin, 2024-12-26, to handle Pip toggle positions click commnd handling
            PipTogglePositionClickCommand = new RelayCommand(OnPipTogglePositionClicked);

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

        private bool _isKVMBusy = false;

        public bool IsKVMBusy
        {
            get => _isKVMBusy;
            set => SetProperty(ref _isKVMBusy, value);
        }

        #endregion UI Enable Flags

        //public ObjGetVCP ret_PxP = new ObjGetVCP();


        public void Invoke_RefreshData()
        {
            _log.Info("[KvmViewModel] Invoke_RefreshData start");
            DateTime entryUSBKVM = DateTime.Now;
            _log.Info($"[Invoke_RefreshData Time]:{entryUSBKVM.ToString("yyyy-MM-dd hh:mm:ss.fff")}");
            BackgroundWorker bw = new BackgroundWorker()
            {
                WorkerReportsProgress = false,
                WorkerSupportsCancellation = false
            };
            if (DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo != null)
            {
                MonitorInfo mi = DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo;

                USBKVMisON = /*KvmModule.isUSBKVM;*/ DdpmCommonHelper.DeviceManagerSA.GetOnUSBKVM(mi).Result;
                if (USBKVMisON)
                {
                    LeftButtonEnable = false;
                    OnPropertyChanged("LeftButtonEnable");
                }
            }
            else
            {
                _log?.Info("MonitorInfo is null.");
            }

            bw.DoWork -= DoWork_RefreshData;
            bw.DoWork += DoWork_RefreshData;
            if (USBKVMisON)
            {
                bw.DoWork -= DoWork_USBKVM;
                bw.DoWork += DoWork_USBKVM;
                bw.RunWorkerCompleted -= RunWorkerCompleted_USBKVMisON;
                bw.RunWorkerCompleted += RunWorkerCompleted_USBKVMisON;
            }
            bw.RunWorkerCompleted -= RunWorkerCompleted_RefreshData;
            bw.RunWorkerCompleted += RunWorkerCompleted_RefreshData;
            bw.RunWorkerAsync(); //myArg is the optional argument
            IsKVMBusy = true;
            _log.Info("[KvmViewModel] Invoke_RefreshData end");
            entryUSBKVM = DateTime.Now;
            _log.Info($"[Invoke_RefreshData Time]:{entryUSBKVM.ToString("yyyy-MM-dd hh:mm:ss.fff")}");
        }

        private void DoWork_RefreshData(object sender, DoWorkEventArgs e)
        {
            _log.Info("[KvmViewModel] DoWork_RefreshData start");
            DateTime entryUSBKVM = DateTime.Now;
            _log.Info($"[DoWork_RefreshData Time]:{entryUSBKVM.ToString("yyyy-MM-dd hh:mm:ss.fff")}");
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

                var directory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

                directory = $"C:\\Program Files\\Dell\\Dell Display and Peripheral Manager";
                string strFullPath = string.Format("{0}\\Plugins\\NKVM\\{1}", directory, GlobalDefinitions.DDMExeName);

                if (mi.CapabilityDic.ContainsKey("E7"))
                {
                    SupportUSBKVM = Visibility.Visible;
                    OnPropertyChanged("SupportUSBKVM");
                    //inputList = new Dictionary<string, InputInfo>();
                    //subInputs = new List<InputSourceObj>();
                    //usbsList = new List<string>();
                    isUSBKVMEanble = true;
                    USBKVM_Opacity = 1;

                    if (mi.CapabilityDic.ContainsKey("E8"))
                    {
                        isPxP = Visibility.Visible;
                        NoPxP = Visibility.Collapsed;
                        isScreenPartition = DdpmCommonHelper.DeviceManagerSA.isScreenPartition(mi).Result;
                        if (isScreenPartition)
                        {
                            isNoKVM = true;
                            isUSBKVMEanble = false;
                            USBKVM_Opacity = 0.5;
                            _log?.Info("[KvmViewModel]isScreenPartition.");
                            OnPropertyChanged("isUSBKVMEanble");
                            OnPropertyChanged("USBKVM_Opacity");
                            //return;
                        }
                    }
                    else
                    {
                        isPxP = Visibility.Collapsed;
                        NoPxP = Visibility.Visible;
                    }
                }
                else
                {
                    SupportUSBKVM = Visibility.Collapsed;
                    OnPropertyChanged("SupportUSBKVM");
                    USBKVMisON = false;
                }

                NKVMisON = DdpmCommonHelper.DeviceManagerSA.GetOnNKVM(mi).Result;

                if (DdpmCommonHelper.DeviceManagerSA.isNKVMSupportMonitor(mi).Result && File.Exists(strFullPath))
                {
                    SupportNKVM = Visibility.Visible;
                    OnPropertyChanged("SupportNKVM");
                }
                else
                {
                    SupportNKVM = Visibility.Collapsed;
                    OnPropertyChanged("SupportNKVM");
                    NKVMisON = false;
                }

                if (USBKVMisON && !isScreenPartition)
                {
                    isUSBKVM = true;
                    //EnableUSBKVM = Visibility.Visible;
                    //DisenableUSBKVM = Visibility.Collapsed;
                }
                else if (NKVMisON)
                {
                    isNKVM = true;
                    //EnableUSBKVM = Visibility.Collapsed;
                    //DisenableUSBKVM = Visibility.Visible;
                }
                else
                {
                    isNoKVM = true;
                    //EnableUSBKVM = Visibility.Collapsed;
                    //DisenableUSBKVM = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, "[DoWork_RefreshData] exception");
            }
            _log.Info("[KvmViewModel] DoWork_RefreshData end");
            entryUSBKVM = DateTime.Now;
            _log.Info($"[DoWork_RefreshData Time]:{entryUSBKVM.ToString("yyyy-MM-dd hh:mm:ss.fff")}");
        }

        private void RunWorkerCompleted_RefreshData(object sender, RunWorkerCompletedEventArgs e)
        {
            _log.Info("[KvmViewModel] RunWorkerCompleted_RefreshData start");
            DateTime entryUSBKVM = DateTime.Now;
            _log.Info($"[RunWorkerCompleted_RefreshData Time]:{entryUSBKVM.ToString("yyyy-MM-dd hh:mm:ss.fff")}");
            IsKVMBusy = false;
            _log.Info("[KvmViewModel] RunWorkerCompleted_RefreshData End");
            entryUSBKVM = DateTime.Now;
            _log.Info($"[RunWorkerCompleted_RefreshData Time]:{entryUSBKVM.ToString("yyyy-MM-dd hh:mm:ss.fff")}");
            //Handling the result and final process
        }

        public void Invoke_USBKVM()
        {
            _log.Info("[KvmViewModel] Invoke_USBKVM start");
            DateTime entryUSBKVM = DateTime.Now;
            _log.Info($"[Invoke_USBKVM Time]:{entryUSBKVM.ToString("yyyy-MM-dd hh:mm:ss.fff")}");
            BackgroundWorker bw = new BackgroundWorker()
            {
                WorkerReportsProgress = false,
                WorkerSupportsCancellation = false
            };

            if (DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo != null)
            {
                MonitorInfo mi = DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo;

                bw.DoWork -= DoWork_USBKVM;
                bw.DoWork += DoWork_USBKVM;
                bw.RunWorkerCompleted -= RunWorkerCompleted_USBKVM;
                bw.RunWorkerCompleted += RunWorkerCompleted_USBKVM;
                bw.RunWorkerAsync(); //myArg is the optional argument
                IsKVMBusy = true;
            }
            else
            {
                _log?.Info("MonitorInfo is null.");
            }
            _log.Info("[KvmViewModel] Invoke_USBKVM end");
            entryUSBKVM = DateTime.Now;
            _log.Info($"[Invoke_USBKVM Time]:{entryUSBKVM.ToString("yyyy-MM-dd hh:mm:ss.fff")}");
        }

        private void DoWork_USBKVM(object sender, DoWorkEventArgs e)
        {
            _log.Info("[KvmViewModel] DoWork_USBKVM start");
            DateTime entryUSBKVM = DateTime.Now;
            _log.Info($"[DoWork_USBKVM Time]:{entryUSBKVM.ToString("yyyy-MM-dd hh:mm:ss.fff")}");
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
                //Update Left view Text1
                Text1 = selHomeDevice.Text1;
                OnPropertyChanged("Text1");
                //if (DdpmCommonHelper.DeviceManagerSA.isScreenPartition(mi).Result)
                //{
                //    _log?.Info("[KvmViewModel]isScreenPartition.");
                //    return;
                //}
                
                if (mi.CapabilityDic.ContainsKey("E7"))
                {
                    _log?.Info("[KvmViewModel]Have 0xEE");
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
                    if ((inputList != null && inputList.Count > 0) && (subInputList != null && subInputList.Count > 0))
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
                        string currentinput = KvmModule.SelectedHomeDevice.MonitorInfo.inputSource;
                        pcsList = DdpmCommonHelper.DeviceManagerSA.GetUSBKVMPCsList(KvmModule.SelectedHomeDevice.MonitorInfo, inputList, subInputs).Result;
                        usbsList = DdpmCommonHelper.DeviceManagerSA.GetUSBUpstreamList(KvmModule.SelectedHomeDevice.MonitorInfo).Result;
                        if (pcsList == null)
                        {
                            _log?.Info("No value return from `DdpmCommonHelper.DeviceManagerSA.GetUSBKVMPCsList` for pcsList.");
                            return;
                        }
                        if (usbsList == null)
                        {
                            _log?.Info("No value return from `DdpmCommonHelper.DeviceManagerSA.GetUSBKVMPCsList` for pcsList.");
                            return;
                        }
                        original_pcsList = pcsList.ToDictionary(entry => entry.Key, entry => entry.Value);
                        if (pcsList != null && pcsList.Count > 1)   // 2024-06-19 Elie, fix exception.
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
                                    PC1_Input = pcsList["PC1"].InputType;
                                    PC2_Input = pcsList["PC2"].InputType;
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
                                            PC3_Input = pcsList["PC3"].InputType;
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
                                                    PC4_Input = pcsList["PC4"].InputType;
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
                                                    _log?.Info("PC4 not found in pcsList.");
                                                }
                                            }
                                            else
                                            {
                                                PCImage = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/USBKVM_3PCs.png");
                                            }
                                        }
                                        else
                                        {
                                            _log?.Info("PC3 not found in pcsList.");
                                        }
                                    }
                                    else
                                    {
                                        PCImage = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/USBKVM_2PCs.png");
                                    }
                                    ModifiedPCinputList();
                                    USBDisenable();
                                }
                                else
                                {
                                    _log?.Info("PC1 or PC2 not found in pcsList.");
                                }
                            }

                        }
                        else
                        {
                            _log?.Info("inputList is null or pcsList is null or count < 2.");
                        }


                        #region PIP/PBP
                        _log?.Info("[KvmViewModel]PIP/PBP...");
                        if (mi.CapabilityDic.ContainsKey("E9"))
                        {
                            _log?.Info("[KvmViewModel]Have 0xE9");
                            if (!DdpmCommonHelper.DeviceManagerSA.isScreenPartition(mi).Result)
                            {
                                //isPxP = Visibility.Visible;
                                //NoPxP = Visibility.Collapsed;
                                //Get the Pxp Capabilities
                                //_pipPbpCaps = DdpmCommonHelper.DeviceManagerSA.GetPipPbpCapabilitiesWords(mi).Result;
                                string _pxpString = string.Empty;
                                foreach (string str in mi.CapabilityDic["E9"])
                                {
                                    if (_pxpString == string.Empty)
                                    {
                                        _pxpString = str;
                                    }
                                    else
                                    {
                                        _pxpString = _pxpString + " " + str;
                                    }
                                }
                                _pipPbpCaps = DdpmCommonHelper.ParsingHexStringToWords(_pxpString);

                                //Check if this monitor has Pxp mode capabilities
                                if ((_pipPbpCaps != null) && (_pipPbpCaps.Length > 0))
                                {
                                    foreach (UInt16 code in _pipPbpCaps)
                                    {
                                        if (!PxPcodeDictionary.ContainsKey(code))
                                        {
                                            PxPcodeDictionary.Add(code, null);
                                        }
                                    }
                                    //Get current monitor's Pxp mode
                                    ObjGetVCP ret_PxP = DdpmCommonHelper.DeviceManagerSA.GetPxpMode(mi).Result;
                                    if (ret_PxP != null && ret_PxP.result)
                                    {
                                        //UInt64 u64 = (UInt64)ret.result;
                                        _curPxpMode = Convert.ToUInt16(ret_PxP.value);
                                        //PxPCode = _curPxpMode;
                                        //VideoSwapContent = PxPcodeDictionary[_curPxpMode];
                                        //if (USBKVMisON)
                                        //{
                                        //    VideoSwapContent_Left = PxPcodeDictionary[PxPCode];
                                        //}
                                        switch (_curPxpMode)
                                        {
                                            case 0x21:
                                                isPipSmall = true;
                                                isPipLarge = false;
                                                isPBP = false;
                                                break;

                                            case 0x22:
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
                                    _log.Info("[KvmViewModel] _pipPbpCaps is null or Length not > 0");
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
                            _curPxpMode = 0x0;
                            isPipSmall = false;
                            isPipLarge = false;
                            isPBP = false;
                            //    NoPxP = Visibility.Visible;
                            //    isPxP = Visibility.Collapsed;
                        }
                        #endregion PIP/PBP
                    }
                }

            }
            catch (Exception ex)
            {
                _log.Error(ex, "[DoWork_RefreshData] exception");
            }
            _log.Info("[KvmViewModel] DoWork_USBKVM end");
            entryUSBKVM = DateTime.Now;
            _log.Info($"[DoWork_USBKVM Time]:{entryUSBKVM.ToString("yyyy-MM-dd hh:mm:ss.fff")}");
        }

        private void RunWorkerCompleted_USBKVM(object sender, RunWorkerCompletedEventArgs e)
        {
            _log.Info("[KvmViewModel] RunWorkerCompleted_USBKVM start");
            DateTime entryUSBKVM = DateTime.Now;
            _log.Info($"[RunWorkerCompleted_USBKVM Time]:{entryUSBKVM.ToString("yyyy-MM-dd hh:mm:ss.fff")}");
            //OnPropertyChanged("PC1_Input");
            //OnPropertyChanged("PC2_Input");
            //OnPropertyChanged("PC3_Input");
            //OnPropertyChanged("PC4_Input");
            //OnPropertyChanged("PC1Inputs_Selected");
            //OnPropertyChanged("PC2Inputs_Selected");
            //OnPropertyChanged("PC3Inputs_Selected");
            //OnPropertyChanged("PC4Inputs_Selected");
            //OnPropertyChanged("PC1USB_Selected");
            //OnPropertyChanged("PC2USB_Selected");
            //OnPropertyChanged("PC3USB_Selected");
            //OnPropertyChanged("PC4USB_Selected");
            //OnPropertyChanged("PCImage");
            if (DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo.CapabilityDic.ContainsKey("E9"))
            {
                PxPCode = _curPxpMode;
                if (PxPcodeDictionary.ContainsKey(_curPxpMode))
                {
                    if (PxPcodeDictionary[_curPxpMode] != null)
                    {
                        VideoSwapContent = PxPcodeDictionary[_curPxpMode];
                    }
                    else
                    {
                        PxpModeaddDic(_curPxpMode);
                        VideoSwapContent = PxPcodeDictionary[_curPxpMode];

                    }
                    if (USBKVMisON)
                    {
                        if (PxPcodeDictionary[_curPxpMode] != null)
                        {
                            VideoSwapContent_Left = PxPcodeDictionary[_curPxpMode];
                        }
                        else
                        {
                            PxpModeaddDic(_curPxpMode);
                            VideoSwapContent_Left = PxPcodeDictionary[_curPxpMode];

                        }
                    }
                }
                else
                {
                    _log?.Info("[RunWorkerCompleted_USBKVM] PxPcodeDictionary is null or PxPCode not found.");
                }
            }
            else
            {
                PxpModeaddDic(0x0);
                VideoSwapContent_Left = PxPcodeDictionary[0x0];
            }
            OnPipPbpCapsChanged();
            SetInput = Visibility.Visible;
            SetPXP = Visibility.Visible;
            EditInput = Visibility.Collapsed;
            EditPXP = Visibility.Collapsed;
            IsKVMBusy = false;
            InputSourceFullView _inputSourceFullView = new InputSourceFullView();
            _inputSourceFullView.DataContext = this;
            DdpmCommonHelper.ModuleOwner?.OpenFullView(_inputSourceFullView);
            
            _log.Info("[KvmViewModel] RunWorkerCompleted_USBKVM End");
            entryUSBKVM = DateTime.Now;
            _log.Info($"[RunWorkerCompleted_USBKVM Time]:{entryUSBKVM.ToString("yyyy-MM-dd hh:mm:ss.fff")}");
            //Handling the result and final process
        }

        private void RunWorkerCompleted_USBKVMisON(object sender, RunWorkerCompletedEventArgs e)
        {
            _log.Info("[KvmViewModel] RunWorkerCompleted_USBKVMisON start");
            DateTime entryUSBKVM = DateTime.Now;
            _log.Info($"[RunWorkerCompleted_USBKVMisON Time]:{entryUSBKVM.ToString("yyyy-MM-dd hh:mm:ss.fff")}");
            //OnPropertyChanged("PC1_Input");
            //OnPropertyChanged("PC2_Input");
            //OnPropertyChanged("PC3_Input");
            //OnPropertyChanged("PC4_Input");
            //OnPropertyChanged("PC1Inputs_Selected");
            //OnPropertyChanged("PC2Inputs_Selected");
            //OnPropertyChanged("PC3Inputs_Selected");
            //OnPropertyChanged("PC4Inputs_Selected");
            //OnPropertyChanged("PC1USB_Selected");
            //OnPropertyChanged("PC2USB_Selected");
            //OnPropertyChanged("PC3USB_Selected");
            //OnPropertyChanged("PC4USB_Selected");
            //OnPropertyChanged("PCImage");
            LeftButtonEnable = true;
            OnPropertyChanged("LeftButtonEnable");
            if (DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo.CapabilityDic.ContainsKey("E9"))
            {
                PxPCode = _curPxpMode;
                if (PxPcodeDictionary.ContainsKey(_curPxpMode))
                {
                    if (PxPcodeDictionary[_curPxpMode] != null)
                    {
                        VideoSwapContent = PxPcodeDictionary[_curPxpMode];
                    }
                    else
                    {
                        PxpModeaddDic(_curPxpMode);
                        VideoSwapContent = PxPcodeDictionary[_curPxpMode];

                    }
                    if (USBKVMisON)
                    {
                        if (PxPcodeDictionary[_curPxpMode] != null)
                        {
                            VideoSwapContent_Left = PxPcodeDictionary[_curPxpMode];
                        }
                        else
                        {
                            PxpModeaddDic(_curPxpMode);
                            VideoSwapContent_Left = PxPcodeDictionary[_curPxpMode];

                        }
                    }
                }
                else
                {
                    _log?.Info("[RunWorkerCompleted_USBKVM] PxPcodeDictionary is null or PxPCode not found.");
                }
            }
            else
            {
                PxpModeaddDic(0x0);
                VideoSwapContent_Left = PxPcodeDictionary[0x0];
            }
            OnPipPbpCapsChanged();
            SetInput = Visibility.Visible;
            SetPXP = Visibility.Visible;
            EditInput = Visibility.Collapsed;
            EditPXP = Visibility.Collapsed;
            _log.Info("[KvmViewModel] RunWorkerCompleted_USBKVM End");
            entryUSBKVM = DateTime.Now;
            _log.Info($"[RunWorkerCompleted_USBKVMisON Time]:{entryUSBKVM.ToString("yyyy-MM-dd hh:mm:ss.fff")}");
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
                    pcInfo.USBUpstream = DdpmCommonHelper.DeviceManagerSA.GetUSBUpstream(KvmModule.SelectedHomeDevice.MonitorInfo, input.Key).Result;
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
            ModifiedPCinputList();
            USBDisenable();
            //bool b = DdpmCommonHelper.DeviceManagerSA.SetUSBKVMPCsList(KvmModule.SelectedHomeDevice.MonitorInfo, pcsList).Result;
            OnPropertyChanged("PC1_Input");
            OnPropertyChanged("PC1Inputs_Selected");
            OnPropertyChanged("PC1USB_Selected");
            OnPropertyChanged("PC2_Input");
            OnPropertyChanged("PC2Inputs_Selected");
            OnPropertyChanged("PC2USB_Selected");
            OnPropertyChanged("PC3_Input");
            OnPropertyChanged("PC3Inputs_Selected");
            OnPropertyChanged("PC3USB_Selected");
            OnPropertyChanged("PC4_Input");
            OnPropertyChanged("PC4Inputs_Selected");
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
                _log?.Info(pcnum + " not found in pcsList.");
            }
        }

        public void CurrentInputChange()
        {
            if (pcsList.TryGetValue("PC1", out var pc1))
            {
                //if (pcsList["PC1"].InputType != KvmModule.SelectedHomeDevice.MonitorInfo.inputSource)
                //{
                bool b = DdpmCommonHelper.DeviceManagerSA.SetVCPCapability(KvmModule.SelectedHomeDevice.MonitorInfo, "Input Select", pcsList["PC1"].InputType).Result;
                string result = b ? "Success" : "Failed";
                _log.Info($"[KvmViewModel] MainInput PC1-{pcsList["PC1"].InputType} Done with {result}.");
                if (b)
                {
                    KvmModule.SelectedHomeDevice.MonitorInfo.inputSource = pcsList["PC1"].InputType;
                }
                //}
            }
            else
            {
                _log.Info("PC1 not found in pcsList.");
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
        public void OnPipPbpCapsChanged()
        {
            OnPropertyChanged("HasCap_PipSmall");
            OnPropertyChanged("HasCap_PipLarge");
            OnPropertyChanged("HasCap_PipTogglePosition");
            //OnPropertyChanged("PC1_Input");
            //OnPropertyChanged("PC2_Input");
            //OnPropertyChanged("PC3_Input");
            //OnPropertyChanged("PC4_Input");
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
                    //Robert_Lin, 2024-12-25 added to refresh toggle button state
                    OnPropertyChanged("IsTogglePositionEnabled");
                }
                else
                {
                    //Selection none
                    SetProperty(ref _selectedSplitItem, value);
                    //To determine if "Toggle between position" button should be enabled
                    OnPropertyChanged("IsPipListItemSelected");
                    //Robert_Lin, 2024-12-25 added to refresh toggle button state
                    OnPropertyChanged("IsTogglePositionEnabled");
                }
            }
        }

        /// <summary>
        /// Return the PxpMode code of SelectedSplitItem
        /// </summary>
        /// <returns>0x00 ~ 0xFF : The PxpMode
        /// 0xFFFF : unknown mode
        /// </returns>
        public UInt16 GetSelectedItemPxpMode()
        {
            if (SelectedSplitItem == null)
                return 0xffff;
            ISplit? isp = SelectedSplitItem.ISplit;
            if (isp == null)
                return 0xffff;
            return isp.PbpCapabilityCode;
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
                //Robert_Lin, 2024-12-26, return true only when selected item is the same with current Pxp mode
                if (IsPipListItemSelected)
                {
                    //Get selected mode
                    UInt16 selectedMode = GetSelectedItemPxpMode();
                    if (selectedMode != 0xffff)
                    {
                        return CurPxpMode == selectedMode;
                    }
                    //if (CurPxpMode == PipMode_Large || CurPxpMode == PipMode_Small)
                    //    return true;
                }
                return false;
            }
        }

        //Robert_Lin, 2024-12-26, to handle PIP toggle positions command
        /// <summary>
        /// The Command when 'Toggle position' is clicked
        /// </summary>
        private ICommand? _pipTogglePositionClickCommand;

        public ICommand? PipTogglePositionClickCommand
        {
            get => _pipTogglePositionClickCommand;
            set => SetProperty(ref _pipTogglePositionClickCommand, value);
        }

        private void OnPipTogglePositionClicked()
        {
            BackgroundWorker bw = new BackgroundWorker()
            {
                WorkerReportsProgress = false,
                WorkerSupportsCancellation = false
            };
            bw.DoWork += delegate
            {
                //UInt16 capCode = SelectedSplitItem.ISplit.PbpCapabilityCode;
                if (DdpmCommonHelper.DeviceManagerSA != null)
                {
                    DdpmCommonHelper.DeviceManagerSA.TogglePipPosition(DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo);
                }
            };
            bw.RunWorkerCompleted += delegate
            {
                IsBusy = false;
            };
            IsBusy = true;
            bw.RunWorkerAsync();
        }
        #endregion Determine if Toggle between positons button enabled/disabled

        #region VideoSwap control and content

        private ContentControl? _videoSwapContent;
        private ContentControl? _videoSwapContent_Left;

        public ContentControl? VideoSwapContent
        {
            get => _videoSwapContent;
            set => SetProperty(ref _videoSwapContent, value);
        }

        public ContentControl? VideoSwapContent_Left
        {
            get => _videoSwapContent_Left;
            set => SetProperty(ref _videoSwapContent_Left, value);
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
                USBDisenable();
            }
            OnPropertyChanged("PC1_Input");
            OnPropertyChanged("PC2_Input");
            OnPropertyChanged("PC3_Input");
            OnPropertyChanged("PC4_Input");
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
            OnPropertyChanged("PC1_Input");
            OnPropertyChanged("PC2_Input");
            OnPropertyChanged("PC3_Input");
            OnPropertyChanged("PC4_Input");
        }

        public void PC2Click()
        {
            Border1Visibility = Visibility.Collapsed;
            Border2Visibility = Visibility.Visible;
            Border3Visibility = Visibility.Collapsed;
            Border4Visibility = Visibility.Collapsed;
            OnPropertyChanged("PC1_Input");
            OnPropertyChanged("PC2_Input");
            OnPropertyChanged("PC3_Input");
            OnPropertyChanged("PC4_Input");
        }

        public void PC3Click()
        {
            Border1Visibility = Visibility.Collapsed;
            Border2Visibility = Visibility.Collapsed;
            Border3Visibility = Visibility.Visible;
            Border4Visibility = Visibility.Collapsed;
            OnPropertyChanged("PC1_Input");
            OnPropertyChanged("PC2_Input");
            OnPropertyChanged("PC3_Input");
            OnPropertyChanged("PC4_Input");
        }

        public void PC4Click()
        {
            Border1Visibility = Visibility.Collapsed;
            Border2Visibility = Visibility.Collapsed;
            Border3Visibility = Visibility.Collapsed;
            Border4Visibility = Visibility.Visible;
            OnPropertyChanged("PC1_Input");
            OnPropertyChanged("PC2_Input");
            OnPropertyChanged("PC3_Input");
            OnPropertyChanged("PC4_Input");
        }

        public void OpenNKVMUI(int index, int x, int y)
        {
            var directory = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            directory = $"C:\\Program Files\\Dell\\Dell Display and Peripheral Manager";
            string strFullPath = string.Format("{0}\\Plugins\\NKVM\\{1}", directory, GlobalDefinitions.DDMExeName);
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
                string processName = GlobalDefinitions.DDMProcessName;// "DDM";
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
                        if (process == null || process.Handle == 0)
                        {
                            continue;
                        }

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
            if (!ison)
            {
                KvmModule._leftView = null;
                DdpmCommonHelper.ModuleOwner.LoadLeftView();
            }
        }

        public void LoaddefLeftView()
        {
            KvmModule._leftView = null;
            DdpmCommonHelper.ModuleOwner.LoadLeftView();
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
            DdpmCommonHelper.DeviceManagerSA.CreatNewNamedpipe().Wait();
            //#if DEBUG
            //                DdpmCommonHelper.DeviceManagerSA.CallShowNKVM(0, 100, 100).Wait();
            //#else
            //                OpenNKVMUI(0, 100, 100);
            //#endif
            int i = 0;
            while (i < 120)
            {
                if (i == 70 && !DdpmCommonHelper.DeviceManagerSA.IsNamedpipeConnected().Result)
                {
                    _log.Info("Named pipe is not Connected, so CreatNewNamedpipe again.");
                    DdpmCommonHelper.DeviceManagerSA.CreatNewNamedpipe().Wait();
                }
                if (DdpmCommonHelper.DeviceManagerSA.IsNamedpipeConnected().Result)
                {
                    isOnNKVM(true);
                    _log.Info("NKVMOpenUI i = " + i);
                    DdpmCommonHelper.DeviceManagerSA.CallShowNKVM(0, 100, 100).Wait();
                    e.Result = true;
                    break;
                }
                i++;
                Thread.Sleep(500);
            }
            if (i == 120)
            {
                _log.Info("Named pipe is not Connected or time out");
                e.Result = false;
            }
            //if (DdpmCommonHelper.DeviceManagerSA.IsNamedpipeConnected().Result)
            //{
            //    isOnNKVM(true);
            //    _log.Info("NKVMOpenUI...");
            //    DdpmCommonHelper.DeviceManagerSA.CallShowNKVM(0, 100, 100).Wait();
            //}
            //else
            //{
            //    _log.Info("Named pipe is not Connected or time out");
            //}
        }
        private void NKVMOpenUI_Done(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                _log.Error("NKVMOpenUI error : " + e.Error.ToString());
            }
            if (e.Result != null)
            {
                Thread.Sleep(3000);
            }
            IsBusy = false;
            OnPropertyChanged("IsBusy");
        }

        #endregion

        public void OnPropertyChanged_Lock()
        {
            OnPropertyChanged("isNKVMEanble");
            OnPropertyChanged("NKVM_Opacity");
            //OnPropertyChanged("LockNKVM_Visibility");
            OnPropertyChanged("isUSBKVMEanble");
            OnPropertyChanged("USBKVM_Opacity");
            OnPropertyChanged("LockUSBKVM_Visibility");
        }

        public void FinishtoSetPCs()
        {
            //set input source
            if (pcsList != null)
            {
                // Since Profile issue PCs will be inconsistent need to rewrite each change,
                // Monitor info is only updated by monitor info updated. Originally get from profile not real monitor status
                // if (!isPCsListSame(pcsList,  original_pcsList))
                // {  

                if (pcsList.TryGetValue("PC1", out var pc1) && pcsList.TryGetValue("PC2", out var pc2))
                {
                    //Jason by U3824DW input source greyed out
                    if (pcsList["PC1"].InputType == original_pcsList["PC2"].InputType &&
                    //pcsList["PC2"].InputType == original_pcsList["PC1"].InputType) &&
                    _curPxpMode != 0x0)
                    {
                        DdpmCommonHelper.DeviceManagerSA.VideoSwap(KvmModule.SelectedHomeDevice.MonitorInfo, 0, 1);
                    }
                    else
                    {
                        CurrentInputChange();
                    }
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
                                    string result = res ? "Success" : "Failed";
                                    _log.Info($"[KvmViewModel] SubInputs PC2-{pc2input.Name}, PC3-{pc3input.Name}, PC4-{pc4input.Name} Done with {result}.");
                                    if (res)
                                    {
                                        Thread.Sleep(1000);
                                    }
                                }
                                else
                                {
                                    _log?.Info("PC4 not found in pcsList.");
                                }
                            }
                            else
                            {
                                bool res = DdpmCommonHelper.DeviceManagerSA.SetSubInputs(
                                    DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo,
                                    pc2input, pc3input, null).Result;
                                string result = res ? "Success" : "Failed";
                                _log.Info($"[KvmViewModel] SubInputs PC2-{pc2input.Name}, PC3-{pc3input.Name} Done with {result}.");
                                if (res)
                                {
                                    Thread.Sleep(1000);
                                }
                            }
                        }
                        else
                        {
                            _log?.Info("PC3 not found in pcsList.");
                        }
                    }
                    else
                    {
                        bool res = DdpmCommonHelper.DeviceManagerSA.SetSubInputs(
                                DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo,
                                pc2input, null, null).Result;
                        string result = res ? "Success" : "Failed";
                        _log.Info($"[KvmViewModel] SubInputs PC2-{pc2input.Name} Done with {result}.");
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
                        if (string.IsNullOrEmpty(pcs.Value.USBUpstream))
                        {
                            pcs.Value.USBUpstream = DdpmCommonHelper.DeviceManagerSA.GetUSBUpstream(KvmModule.SelectedHomeDevice.MonitorInfo, pcs.Value.InputType).Result;
                        }
                        else
                        {
                            //if (pcs.Value.USBUpstream != original_pcsList[pcs.Key].USBUpstream)
                            //{
                            bool bUSBuptream = DdpmCommonHelper.DeviceManagerSA.SetUSBUpstream(KvmModule.SelectedHomeDevice.MonitorInfo, pcs.Value.InputType, pcs.Value.USBUpstream).Result;
                            _log.Info($"[KvmViewModel] SetUSBUpstream {pcs.Key}-{pcs.Value.InputType}: {pcs.Value.USBUpstream} Done");
                            if (bUSBuptream)
                            {
                                Thread.Sleep(1000);
                            }
                            //}
                        }
                    }
                    USBDisenable();
                    //if (pcsList["PC1"].InputType != KvmModule.SelectedHomeDevice.MonitorInfo.inputSource)
                    //{
                    //}
                    bool bin = DdpmCommonHelper.DeviceManagerSA.SetInputSourcelist(KvmModule.SelectedHomeDevice.MonitorInfo, inputList).Result;
                    bool bpcs = DdpmCommonHelper.DeviceManagerSA.SetUSBKVMPCsList(KvmModule.SelectedHomeDevice.MonitorInfo, pcsList).Result;
                    original_pcsList = pcsList.ToDictionary(entry => entry.Key, entry => entry.Value);
                    //isOnUSBKVM(true);//bool b = DdpmCommonHelper.DeviceManagerSA.SetOnUSBKVM(true).Result;
                }
                else
                {
                    _log?.Info("PC1 or PC2 not found in pcsList.");
                }
                // }
            }
            else
            {
                _log?.Info("pcsList is null");
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
        public bool isPxpModeOn
        {
            get
            {
                return _curPxpMode != 0;
            }
        }

        public void PxpModeaddDic(UInt16 code)
        {
            switch (code)
            {
                case 0x0:
                    PxPcodeDictionary[code] = new PxPSplitCtrl0A();
                    break;
                case 0x21:
                    PxPcodeDictionary[code] = new PIPSplitCtrl1A();
                    break;
                case 0x22:
                    PxPcodeDictionary[code] = new PIPSplitCtrl1B();
                    break;
                case 0x24:
                    PxPcodeDictionary[code] = new PBPSplitCtrl2A();
                    break;
                case 0x2F:
                    PxPcodeDictionary[code] = new PBPSplitCtrl2B();
                    break;
                case 0x26:
                case 0x28:
                case 0x2A:
                case 0x2C:
                case 0x2E:
                    PxPcodeDictionary[code] = new PBPSplitCtrl2C();
                    break;
                case 0x25:
                case 0x27:
                case 0x29:
                case 0x2B:
                case 0x2D:
                    PxPcodeDictionary[code] = new PBPSplitCtrl2D();
                    break;
                case 0x31:
                    PxPcodeDictionary[code] = new PBPSplitCtrl3E();
                    break;
                case 0x32:
                    PxPcodeDictionary[code] = new PBPSplitCtrl3D();
                    break;
                case 0x33:
                    PxPcodeDictionary[code] = new PBPSplitCtrl3H();
                    break;
                case 0x34:
                    PxPcodeDictionary[code] = new PBPSplitCtrl3B();
                    break;
                case 0x35:
                    PxPcodeDictionary[code] = new PBPSplitCtrl3I();
                    break;
                case 0x41:
                    PxPcodeDictionary[code] = new PBPSplitCtrl4A();
                    break;
                case 0x42:
                    PxPcodeDictionary[code] = new PBPSplitCtrl4D();
                    break;

                default:
                    break;
            }
        }


        public bool isPxpModeOFF
        {
            get
            {
                return _curPxpMode == 0;
            }
        }
        public bool isPBPmode
        {
            get
            {
                bool ret = false;
                switch (_curPxpMode)
                {
                    case 0x00://off
                        ret = false;
                        break;
                    case 0x21://PIP small
                        ret = false;
                        break;

                    case 0x22://PIP large
                        ret = false;
                        break;
                    case 0x23:
                    case 0x24:
                    case 0x25:
                    case 0x26:
                    case 0x27:
                    case 0x28:
                    case 0x29:
                    case 0x2A:
                    case 0x2B:
                    case 0x2C:
                    case 0x2D:
                    case 0x2E:
                    case 0x2F:
                    case 0x31:
                    case 0x32:
                    case 0x33:
                    case 0x34:
                    case 0x35:
                    case 0x41:
                    case 0x42:
                        ret = true;
                        break;

                    default:
                        ret = false;
                        break;
                }
                DdpmCommonHelper.WriteUILog($"[USBKVM Hotkey] _curPxpMode={_curPxpMode},isPBPmode={ret}");
                return ret;
            }
        }

        public void ShowPxPView(ushort PbpCapabilityCode)
        {
            if (PxPcodeDictionary.ContainsKey(PbpCapabilityCode))
            {
                VideoSwapContent = PxPcodeDictionary[PbpCapabilityCode];
                VideoSwapContent_Left = PxPcodeDictionary[PbpCapabilityCode];
            }
            else
            {
                _log.Info("[PBP_MouseLeftDown]PxPcodeDictionary not find key " + PbpCapabilityCode);
            }
        }

        public void USBDisenable()
        {
            if (pcsList != null)
            {
                if (pcsList.ContainsKey("PC1"))
                {
                    if (pcsList["PC1"].InputType == "USB-C" ||
                        pcsList["PC1"].InputType == "USC-C1" ||
                        pcsList["PC1"].InputType == "Thunderbolt" ||
                        pcsList["PC1"].InputType == "Thunderbolt1")
                    {
                        PC1USB_Enable = false;
                        PC1USB_Opacity = 0.5;
                    }
                    else
                    {
                        PC1USB_Enable = true;
                        PC1USB_Opacity = 1;
                    }
                    OnPropertyChanged("PC1USB_Enable");
                    OnPropertyChanged("PC1USB_Opacity");
                }
                if (pcsList.ContainsKey("PC2"))
                {
                    if (pcsList["PC2"].InputType == "USB-C" ||
                        pcsList["PC2"].InputType == "USC-C1" ||
                        pcsList["PC2"].InputType == "Thunderbolt" ||
                        pcsList["PC2"].InputType == "Thunderbolt1")
                    {
                        PC2USB_Enable = false;
                        PC2USB_Opacity = 0.5;
                    }
                    else
                    {
                        PC2USB_Enable = true;
                        PC2USB_Opacity = 1;
                    }
                    OnPropertyChanged("PC2USB_Enable");
                    OnPropertyChanged("PC2USB_Opacity");
                }
                if (pcsList.ContainsKey("PC3"))
                {
                    if (pcsList["PC3"].InputType == "USB-C" ||
                        pcsList["PC3"].InputType == "USC-C1" ||
                        pcsList["PC3"].InputType == "Thunderbolt" ||
                        pcsList["PC3"].InputType == "Thunderbolt1")
                    {
                        PC3USB_Enable = false;
                        PC3USB_Opacity = 0.5;
                    }
                    else
                    {
                        PC3USB_Enable = true;
                        PC3USB_Opacity = 1;
                    }
                    OnPropertyChanged("PC3USB_Enable");
                    OnPropertyChanged("PC3USB_Opacity");
                }
                if (pcsList.ContainsKey("PC4"))
                {
                    if (pcsList["PC4"].InputType == "USB-C" ||
                        pcsList["PC4"].InputType == "USC-C1" ||
                        pcsList["PC4"].InputType == "Thunderbolt" ||
                        pcsList["PC4"].InputType == "Thunderbolt1")
                    {
                        PC4USB_Enable = false;
                        PC4USB_Opacity = 0.5;
                    }
                    else
                    {
                        PC4USB_Enable = true;
                        PC4USB_Opacity = 1;
                    }
                    OnPropertyChanged("PC4USB_Enable");
                    OnPropertyChanged("PC4USB_Opacity");
                }
            }
        }

        public void ModifiedPCinputList()
        {
            _inputsList1 = new List<InputSourceList>();
            _inputsList2 = new List<InputSourceList>();
            _inputsList3 = new List<InputSourceList>();
            _inputsList4 = new List<InputSourceList>();
            foreach (InputSourceList inputSource in _inputsList)
            {
                if (pcsList.ContainsKey("PC1") && inputSource.Type == pcsList["PC1"].InputType)
                {
                    _inputsList1.Add(inputSource);
                }
                else if (pcsList.ContainsKey("PC2") && inputSource.Type == pcsList["PC2"].InputType)
                {
                    _inputsList2.Add(inputSource);
                }
                else if (pcsList.ContainsKey("PC3") && inputSource.Type == pcsList["PC3"].InputType)
                {
                    _inputsList3.Add(inputSource);
                }
                else if (pcsList.ContainsKey("PC4") && inputSource.Type == pcsList["PC4"].InputType)
                {
                    _inputsList4.Add(inputSource);
                }
                else
                {
                    if (pcsList.ContainsKey("PC1"))
                    {
                        _inputsList1.Add(inputSource);
                    }
                    if (pcsList.ContainsKey("PC2"))
                    {
                        _inputsList2.Add(inputSource);
                    }
                    if (pcsList.ContainsKey("PC3"))
                    {
                        _inputsList3.Add(inputSource);
                    }
                    if (pcsList.ContainsKey("PC4"))
                    {
                        _inputsList4.Add(inputSource);
                    }
                }
            }
            OnPropertyChanged("PC1InputsList");
            OnPropertyChanged("PC2InputsList");
            OnPropertyChanged("PC3InputsList");
            OnPropertyChanged("PC4InputsList");
        }

        #region Event
        /// <summary>
        /// Catch OSD menu event
        /// </summary>
        /// <param name="sender">object type</param>
        /// <param name="e">changed event</param>
        private void OnVCPChangedEvent(object? sender, VCPchangedEventArgs e)
        {
            if (e.vcpcode.Equals("E7") && DdpmCommonHelper.DeviceManagerSA.GetOnUSBKVM(KvmModule.SelectedHomeDevice.MonitorInfo).Result)
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
                                            _log?.Info("PC4 not found in pcsList.");
                                        }
                                    }
                                }
                                else
                                {
                                    _log?.Info("PC3 not found in pcsList.");
                                }
                            }
                            OnPropertyChanged("PC1USB_Selected");
                            OnPropertyChanged("PC2USB_Selected");
                            OnPropertyChanged("PC3USB_Selected");
                            OnPropertyChanged("PC4USB_Selected");
                        }
                        else
                        {
                            _log?.Info("PC1 or PC2 not found in pcsList.");
                        }
                    }
                    else
                    {
                        _log?.Info("[KvmViewModel] usbsList is null or count is 0");
                    }
                }
                else
                {
                    _log?.Info("[KvmViewModel] inputList is null or count is 0");
                }
            }
        }
        #endregion

        private bool isPCsListSame(Dictionary<string, PCsInfo> sourceDict, Dictionary<string, PCsInfo> targetDict)
        {
            bool isSame = true;
            if (sourceDict == null && targetDict == null) return true;
            else
            {
                if (sourceDict == null || targetDict == null) return false;
                else if (sourceDict.Count != targetDict.Count) return false;
                else
                {
                    foreach (var kvp in sourceDict)
                    {
                        if (targetDict[kvp.Key].Code != kvp.Value.Code) return false;
                        if (targetDict[kvp.Key].InputName != kvp.Value.InputName) return false;
                        if (targetDict[kvp.Key].InputType != kvp.Value.InputType) return false;
                        if (targetDict[kvp.Key].USBUpstream != kvp.Value.USBUpstream) return false;
                    }
                    return true;
                }
            }
        }
    }
}