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
using System.Globalization;

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
            bool rst = SetWindowPos(hWnd, hWndInsertAfter, X, Y, cx, cy, uFlags);

            if (!rst)
            {
#if DEBUG
                Console.WriteLine("[User32_SetWindowPos] SetWindowPos failed.");
#endif
            }

            return rst;
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
        //private Dictionary<string, PCsInfo> pcsList = new Dictionary<string, PCsInfo>();
        private ImageSource? _PCImage;
        private BackgroundWorker? bw = null;
        private bool toKVMSetPage = false;
        #endregion

        public readonly ILog? _log = null;
        public Guid? guid { get; set; } = null;
        public IModuleOwner? ModuleOwner { get; set; } = null;
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
        public bool pipSmallMode { get; set; } = false;
        public bool isPipLarge { get; set; } = false;
        public bool pipLargeMode { get; set; } = false;
        public bool isPBP { get; set; } = false;
        public bool pbpModde { get; set; } = false;

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
        public bool NoKVMisON = false;
        public bool USBKVMisON = false;
        public bool NKVMisON = false;

        public bool isNoKVM
        {
            get => _isNoKVM;
            set
            {
                SetProperty(ref _isNoKVM, value);
            }
        }

        public bool isUSBKVM
        {
            get => _isUSBKVM;
            set
            {
                SetProperty(ref _isUSBKVM, value);
            }
        }

        public bool isNKVM
        {
            get => _isNKVM;
            set
            {
                SetProperty(ref _isNKVM, value);
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
                if (value != null && _PC1selectInput != value)
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
                if (value != null && _PC2selectInput != value)
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
                if (value != null && _PC3selectInput != value)
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
                if (value != null && _PC4selectInput != value)
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

        public bool firstinKVM { get; set; } = true;

        public bool LockSendNoKVM { get; set; } = false;

        public Visibility ArrowinFullscreen { get; set; } = Visibility.Collapsed;

        public Visibility PCImage_2 {  get; set; } = Visibility.Collapsed;
        public Visibility PCImage_3 { get; set; } = Visibility.Collapsed;
        public Visibility PCImage_4 { get; set; } = Visibility.Collapsed;

        //public class PathData
        //{
        //    public string Data { get; set; }
        //    public double StrokeThickness { get; set; } = 1;
        //    public Transform RenderTransform { get; set; }
        //}

        //public ObservableCollection<PathData> PCsPaths { get; set; }

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
                    DdpmCommonHelper.isJumpFromUsbKvm = true;
                }
            }
            Invoke_RefreshHotkeySettings();
        }
        private void saveKvmHotkeyOption()
        {
            /*HotkeySettings hotkeySettings = new HotkeySettings
            {
                //DeviceInfo = KvmModule.SelectedHomeDevice.MonitorInfo.edid,
                ModelName = "DDPM",
                SerialNumber = "DDPM",
                ServiceTag = "DDPM",
                HotkeyOptions = new List<HotkeyOption> { _autoSwitchChecked ? HotkeyOption.KvmAutoApply : HotkeyOption.None }
            };
            DdpmCommonHelper.DeviceManagerSA.SaveHotkeyOptionOnly(hotkeySettings);*/
            if (DdpmCommonHelper.DeviceManagerSA != null)
            {
                MonitorInfo? monitorInfo = KvmModule?.SelectedHomeDevice?.MonitorInfo;
                if (monitorInfo != null)
                {
                    HotkeyOption hotkeyOption = _autoSwitchChecked ? HotkeyOption.KvmAutoApply : HotkeyOption.None;
                    bool result = DdpmCommonHelper.DeviceManagerSA.SaveHotkeyOptionOnly(monitorInfo, hotkeyOption).Result;
                    if (result)
                    {
                        DdpmCommonHelper.WriteUILog($"[USBKVMHotkey]saveKvmHotkeyOption success.[{monitorInfo.AliasDeviceName},{monitorInfo.edid.ServiceTag},{hotkeyOption}]");
                    }
                    else
                    {
                        DdpmCommonHelper.WriteUILog($"[USBKVMHotkey]saveKvmHotkeyOption fail.[{monitorInfo.AliasDeviceName},{monitorInfo.edid.ServiceTag},{hotkeyOption}]");
                    }

                }
                else
                {
                    DdpmCommonHelper.WriteUILog($"[USBKVMHotkey]saveKvmHotkeyOption selected monitor is null.");
                }
            }
        }

        public void Invoke_RefreshHotkeySettings()
        {
            _log?.Info("[KvmViewModel] Invoke_RefreshHotkeySettings start");
            DateTime entryUSBKVM = DateTime.Now;
            _log?.Info($"[Invoke_RefreshHotkeySettings Time]:{entryUSBKVM.ToString("yyyy-MM-dd hh:mm:ss.fff")}");
            BackgroundWorker m_bw = new BackgroundWorker()
            {
                WorkerReportsProgress = false,
                WorkerSupportsCancellation = false
            };
            m_bw.DoWork += DoWork_RefreshHotkeyData;
            m_bw.RunWorkerCompleted += RunWorkerCompleted_RefreshHotkeyData;
            m_bw.RunWorkerAsync(); //myArg is the optional argument
            _log?.Info("[KvmViewModel] Invoke_RefreshHotkeySettings end");
            entryUSBKVM = DateTime.Now;
            _log?.Info($"[Invoke_RefreshHotkeySettings Time]:{entryUSBKVM.ToString("yyyy-MM-dd hh:mm:ss.fff")}");
        }

        private void DoWork_RefreshHotkeyData(object sender, DoWorkEventArgs e)
        {
            _log?.Info("[KvmViewModel] DoWork_RefreshHotkeyData start");
            DateTime entryUSBKVM = DateTime.Now;
            _log?.Info($"[DoWork_RefreshHotkeyData Time]:{entryUSBKVM.ToString("yyyy-MM-dd hh:mm:ss.fff")}");
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
                        //03/27 testCase:Change hotkey in Input source - "Change PIP Position" will be updated in USB KVM > Hotkey > "Change PIP Position". 
                        HotkeyInfo? inputSrcHotkeyInfo = curHotkey.HotkeyInfo.SingleOrDefault(x => x.Job.Equals(HotkeyType.ChangePIPPosition));
                        if (inputSrcHotkeyInfo != null && inputSrcHotkeyInfo.KeyCode != VirtualKey.None)
                        {
                            //show key text to KvmChangePIPPosition textbox
                            List<VirtualKey> hotkeys = inputSrcHotkeyInfo.Hotkey;
                            KeysHelper.ReSetHotKeyText(ref swHortcutText, ref hotkeys);
                            hotkeys.Clear();
                            ChangePipKey = swHortcutText;
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
                    if (selectedHomeDevice != null && selectedHomeDevice.MonitorInfo != null)
                    {

                        HotkeyOption hotkeyOption = DdpmCommonHelper.DeviceManagerSA.ReadHotkeyOption(selectedHomeDevice.MonitorInfo).Result;
                        if (IsPBPMode(selectedHomeDevice.MonitorInfo, _curPxpMode))
                        {
                            _autoSwitchChecked = hotkeyOption == HotkeyOption.KvmAutoApply;
                        }
                        else
                        {
                            _autoSwitchChecked = false;
                        }
                    }
                    /*if (curHotkey.HotkeyOptions.Count > 0 &&
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
                    }*/
                }

                OnPropertyChanged("KvmHotkeyTooltip");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            _log?.Info("[KvmViewModel] DoWork_RefreshHotkeyData end");
            entryUSBKVM = DateTime.Now;
            _log?.Info($"[DoWork_RefreshHotkeyData Time]:{entryUSBKVM.ToString("yyyy-MM-dd hh:mm:ss.fff")}");
        }

        private bool IsPBPMode(MonitorInfo mo, UInt16 curPxpMode)
        {
            bool ret = false;
            UInt16 pxpModeValue = curPxpMode;
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
            _log?.Info("[KvmViewModel] RunWorkerCompleted_RefreshHotkeyData start");
            DateTime entryUSBKVM = DateTime.Now;
            _log?.Info($"[RunWorkerCompleted_RefreshHotkeyData Time]:{entryUSBKVM.ToString("yyyy-MM-dd hh:mm:ss.fff")}");
            Debug.WriteLine("load kvm hotkey setting done");
            //Handling the result and final process
            OnPropertyChanged("IsSwitchPCsVisible");
            _log?.Info("[KvmViewModel] RunWorkerCompleted_RefreshHotkeyData end");
            entryUSBKVM = DateTime.Now;
            _log?.Info($"[RunWorkerCompleted_RefreshHotkeyData Time]:{entryUSBKVM.ToString("yyyy-MM-dd hh:mm:ss.fff")}");
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
            _log?.Info("[KvmViewModel] Invoke_RefreshData start");
            DateTime entryUSBKVM = DateTime.Now;
            _log?.Info($"[Invoke_RefreshData Time]:{entryUSBKVM.ToString("yyyy-MM-dd hh:mm:ss.fff")}");
            bw = new BackgroundWorker()
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };
            guid = Guid.NewGuid();
            firstinKVM = true;
            if (DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo != null)
            {
                MonitorInfo mi = DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo;

                USBKVMisON = /*KvmModule.isUSBKVM;*/ DdpmCommonHelper.DeviceManagerSA.GetOnUSBKVM(mi).Result;
                NKVMisON = DdpmCommonHelper.DeviceManagerSA.GetOnNKVM(mi).Result;
                NoKVMisON = DdpmCommonHelper.DeviceManagerSA.GetNoKVM(mi).Result;
                if (USBKVMisON && !NKVMisON && !NoKVMisON)
                {
                    LeftButtonEnable = false;
                    OnPropertyChanged("LeftButtonEnable");
                }
            }
            else
            {
                _log?.Info("MonitorInfo is null.");
            }
            IsKVMBusy = true;
            bw.DoWork -= DoWork_RefreshData;
            bw.DoWork += DoWork_RefreshData;
            if (USBKVMisON && !NKVMisON && !NoKVMisON)
            {
                bw.DoWork -= DoWork_USBKVM;
                bw.DoWork += DoWork_USBKVM;
                bw.RunWorkerCompleted -= RunWorkerCompleted_USBKVMisON;
                bw.RunWorkerCompleted += RunWorkerCompleted_USBKVMisON;
            }
            bw.RunWorkerCompleted -= RunWorkerCompleted_RefreshData;
            bw.RunWorkerCompleted += RunWorkerCompleted_RefreshData;
            bw.RunWorkerAsync(); //myArg is the optional argument
            _log?.Info("[KvmViewModel] Invoke_RefreshData end");
            entryUSBKVM = DateTime.Now;
            _log?.Info($"[Invoke_RefreshData Time]:{entryUSBKVM.ToString("yyyy-MM-dd hh:mm:ss.fff")}");
        }

        private void DoWork_RefreshData(object sender, DoWorkEventArgs e)
        {
            _log?.Info("[KvmViewModel] DoWork_RefreshData start");
            DateTime entryUSBKVM = DateTime.Now;
            _log?.Info($"[DoWork_RefreshData Time]:{entryUSBKVM.ToString("yyyy-MM-dd hh:mm:ss.fff")}");
            LockSendNoKVM = true;
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
                isNoKVM = true;
                var directory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

                directory = $"C:\\Program Files\\Dell\\Dell Display and Peripheral Manager";
                string strFullPath = string.Format("{0}\\Plugins\\NKVM\\{1}", directory, GlobalDefinitions.DDMExeName);

                if (DdpmCommonHelper.DeviceManagerSA.isNKVMSupportMonitor(mi).Result && File.Exists(strFullPath))
                {
                    SupportNKVM = Visibility.Visible;
                    OnPropertyChanged("SupportNKVM");
                }
                else
                {
                    SupportNKVM = Visibility.Collapsed;
                    OnPropertyChanged("SupportNKVM");
                    if (NKVMisON)
                    {
                        isOnNKVM(false);
                        NKVMisON = false;
                    }
                }

                if (Cancelled_RefreshData(e, bwk))
                {
                    return;
                }

                if (mi.CapabilityDic.ContainsKey("E7"))
                {
                    SupportUSBKVM = Visibility.Visible;
                    OnPropertyChanged("SupportUSBKVM");
                    //inputList = new Dictionary<string, InputInfo>();
                    //subInputs = new List<InputSourceObj>();
                    //usbsList = new List<string>();
                    isUSBKVMEanble = true;
                    USBKVM_Opacity = 1;

                    if (Cancelled_RefreshData(e, bwk))
                    {
                        return;
                    }

                    if (mi.CapabilityDic.ContainsKey("E8"))
                    {
                        isPxP = Visibility.Visible;
                        NoPxP = Visibility.Collapsed;
                        isScreenPartition = DdpmCommonHelper.DeviceManagerSA.isScreenPartition(mi, (Guid)guid, Priority.Middle).Result;
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

                    if (Cancelled_RefreshData(e, bwk))
                    {
                        return;
                    }
                }
                else
                {
                    SupportUSBKVM = Visibility.Collapsed;
                    OnPropertyChanged("SupportUSBKVM");
                    //USBKVMisON = false;
                }

                if (Cancelled_RefreshData(e, bwk))
                {
                    return;
                }

                //if (USBKVMisON && !isScreenPartition)
                //{
                //    isUSBKVM = true;
                //}
                //if (NKVMisON)
                //{
                //    isNKVM = true;
                //}
                //else if (NoKVMisON || isScreenPartition || !USBKVMisON)
                //{
                //    isNoKVM = true;
                //}
                //else
                //{
                //    isUSBKVM = true;
                //}
                SelectKVM();

                if (Cancelled_RefreshData(e, bwk))
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                _log?.Error(ex, "[DoWork_RefreshData] exception");
            }
            _log?.Info("[KvmViewModel] DoWork_RefreshData end");
            entryUSBKVM = DateTime.Now;
            _log?.Info($"[DoWork_RefreshData Time]:{entryUSBKVM.ToString("yyyy-MM-dd hh:mm:ss.fff")}");
        }

        private void RunWorkerCompleted_RefreshData(object sender, RunWorkerCompletedEventArgs e)
        {
            _log?.Info("[KvmViewModel] RunWorkerCompleted_RefreshData start");
            DateTime entryUSBKVM = DateTime.Now;
            _log?.Info($"[RunWorkerCompleted_RefreshData Time]:{entryUSBKVM.ToString("yyyy-MM-dd hh:mm:ss.fff")}");
            IsKVMBusy = false;
            firstinKVM = false;
            _log?.Info("[KvmViewModel] RunWorkerCompleted_RefreshData End");
            entryUSBKVM = DateTime.Now;
            _log?.Info($"[RunWorkerCompleted_RefreshData Time]:{entryUSBKVM.ToString("yyyy-MM-dd hh:mm:ss.fff")}");
            //Handling the result and final process
        }

        public void Invoke_USBKVM(bool toSetPage)
        {
            _log?.Info("[KvmViewModel] Invoke_USBKVM start");
            DateTime entryUSBKVM = DateTime.Now;
            _log?.Info($"[Invoke_USBKVM Time]:{entryUSBKVM.ToString("yyyy-MM-dd hh:mm:ss.fff")}");
            bw = new BackgroundWorker()
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };
            guid = Guid.NewGuid();
            toKVMSetPage = toSetPage;
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
            _log?.Info("[KvmViewModel] Invoke_USBKVM end");
            entryUSBKVM = DateTime.Now;
            _log?.Info($"[Invoke_USBKVM Time]:{entryUSBKVM.ToString("yyyy-MM-dd hh:mm:ss.fff")}");
        }

        private void DoWork_USBKVM(object sender, DoWorkEventArgs e)
        {
            _log?.Info("[KvmViewModel] DoWork_USBKVM start");
            DateTime entryUSBKVM = DateTime.Now;
            _log?.Info($"[DoWork_USBKVM Time]:{entryUSBKVM.ToString("yyyy-MM-dd hh:mm:ss.fff")}");
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

                if (Cancelled_RefreshData(e, bwk))
                {
                    return;
                }

                //Update Left view Text1
                Text1 = selHomeDevice.Text1;
                OnPropertyChanged("Text1");
                UpdateArrow(true);
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
                    if (Cancelled_RefreshData(e, bwk))
                    {
                        return;
                    }
                    inputList = DdpmCommonHelper.DeviceManagerSA.GetInputSourcelist(KvmModule.SelectedHomeDevice.MonitorInfo).Result;
                    if (Cancelled_RefreshData(e, bwk))
                    {
                        return;
                    }
                    subInputList = DdpmCommonHelper.DeviceManagerSA.GetSubInputList(KvmModule.SelectedHomeDevice.MonitorInfo).Result;
                    if (Cancelled_RefreshData(e, bwk))
                    {
                        return;
                    }
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
                                if (Cancelled_RefreshData(e, bwk))
                                {
                                    return;
                                }
                            }
                            subInputs.Add(inputSourceObj);
                            if (Cancelled_RefreshData(e, bwk))
                            {
                                return;
                            }
                        }
                        string currentinput = KvmModule.SelectedHomeDevice.MonitorInfo.inputSource;
                        if (Cancelled_RefreshData(e, bwk))
                        {
                            return;
                        }
                        pcsList = DdpmCommonHelper.DeviceManagerSA.GetUSBKVMPCsList(KvmModule.SelectedHomeDevice.MonitorInfo, inputList, subInputs).Result;
                        if (Cancelled_RefreshData(e, bwk))
                        {
                            return;
                        }
                        usbsList = DdpmCommonHelper.DeviceManagerSA.GetUSBUpstreamList(KvmModule.SelectedHomeDevice.MonitorInfo).Result;
                        if (Cancelled_RefreshData(e, bwk))
                        {
                            return;
                        }
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
                                if (Cancelled_RefreshData(e, bwk))
                                {
                                    return;
                                }
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
                                if (Cancelled_RefreshData(e, bwk))
                                {
                                    return;
                                }
                            }
                            PCInputsList = _inputsList;
                            USBsList = _usbsList;
                            if (pcsList.TryGetValue("PC1", out var pc1) && pcsList.TryGetValue("PC2", out var pc2))
                            {
                                _PC1selectInput = _inputsList.Find(x => (x.Type == pcsList["PC1"].InputType));
                                _log?.Info("[KvmViewModel] PC1 input source : " + pcsList["PC1"].InputType);
                                _PC1selectUSB = _usbsList.Find(x => (x.Type == pcsList["PC1"].USBUpstream));
                                ModifiedPCinputList();
                                _PC2selectInput = _inputsList2.Find(x => (x.Type == pcsList["PC2"].InputType));
                                if (_PC2selectInput == null)
                                {
                                    pcsList["PC2"].InputType = _inputsList2[0].Type;
                                    pcsList["PC2"].Code = inputList[_inputsList2[0].Type].Code;
                                    pcsList["PC2"].InputName = inputList[_inputsList2[0].Type].InputName;
                                    pcsList["PC2"].USBUpstream = DdpmCommonHelper.DeviceManagerSA.GetUSBUpstream(KvmModule.SelectedHomeDevice.MonitorInfo, _inputsList2[0].Type).Result;
                                    _PC2selectInput = _inputsList2[0];
                                }
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
                                if (Cancelled_RefreshData(e, bwk))
                                {
                                    return;
                                }
                                if (pcsList.Count >= 3)
                                {
                                    ModifiedPCinputList();
                                    if (pcsList.TryGetValue("PC3", out var pc3))
                                    {
                                        _PC3selectInput = _inputsList3.Find(x => (x.Type == pcsList["PC3"].InputType));
                                        if (_PC3selectInput == null)
                                        {
                                            pcsList["PC3"].InputType = _inputsList3[0].Type;
                                            pcsList["PC3"].Code = inputList[_inputsList3[0].Type].Code;
                                            pcsList["PC3"].InputName = inputList[_inputsList3[0].Type].InputName;
                                            pcsList["PC3"].USBUpstream = DdpmCommonHelper.DeviceManagerSA.GetUSBUpstream(KvmModule.SelectedHomeDevice.MonitorInfo, _inputsList3[0].Type).Result; ;
                                            _PC3selectInput = _inputsList3[0];
                                        }
                                        _PC3selectUSB = _usbsList.Find(x => (x.Type == pcsList["PC3"].USBUpstream));

                                        PC3_Input = pcsList["PC3"].InputType;
                                        if (!USBKVMisON)
                                        {
                                            pcsList["PC3"].InputName = "PC3";
                                        }
                                        InputName3 = pcsList["PC3"].InputName;
                                        PC3_Visibility = Visibility.Visible;
                                        if (Cancelled_RefreshData(e, bwk))
                                        {
                                            return;
                                        }
                                        if (pcsList.Count == 4)
                                        {
                                            ModifiedPCinputList();
                                            if (pcsList.TryGetValue("PC4", out var pc4))
                                            {
                                                _PC4selectInput = _inputsList4.Find(x => (x.Type == pcsList["PC4"].InputType));
                                                if (_PC4selectInput == null)
                                                {
                                                    pcsList["PC4"].InputType = _inputsList4[0].Type;
                                                    pcsList["PC4"].Code = inputList[_inputsList4[0].Type].Code;
                                                    pcsList["PC4"].InputName = inputList[_inputsList4[0].Type].InputName;
                                                    pcsList["PC4"].USBUpstream = DdpmCommonHelper.DeviceManagerSA.GetUSBUpstream(KvmModule.SelectedHomeDevice.MonitorInfo, _inputsList4[0].Type).Result; ;
                                                    _PC4selectInput = _inputsList4[0];
                                                }
                                                _PC4selectUSB = _usbsList.Find(x => (x.Type == pcsList["PC4"].USBUpstream));
                                                PC4_Input = pcsList["PC4"].InputType;
                                                if (!USBKVMisON)
                                                {
                                                    pcsList["PC4"].InputName = "PC4";
                                                }
                                                InputName4 = pcsList["PC4"].InputName;
                                                PC4_Visibility = Visibility.Visible;
                                                PCImage_2 = Visibility.Collapsed;
                                                PCImage_3 = Visibility.Collapsed;
                                                PCImage_4 = Visibility.Visible;
                                                //PCImage = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/USBKVM_4PCs.png");
                                            }
                                            else
                                            {
                                                _log?.Info("PC4 not found in pcsList.");
                                            }
                                        }
                                        else
                                        {
                                            PCImage_2 = Visibility.Collapsed;
                                            PCImage_3 = Visibility.Visible;
                                            PCImage_4 = Visibility.Collapsed;
                                            //PCImage = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/USBKVM_3PCs.png");
                                        }
                                    }
                                    else
                                    {
                                        _log?.Info("PC3 not found in pcsList.");
                                    }
                                }
                                else
                                {
                                    PCImage_2 = Visibility.Visible;
                                    PCImage_3 = Visibility.Collapsed;
                                    PCImage_4 = Visibility.Collapsed;
                                    //PCImage = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/USBKVM_2PCs.png");
                                    //PCsPaths = new ObservableCollection<PathData>
                                    //{
                                    //    new PathData
                                    //    {
                                    //        Data = "M125.293 122.707C125.683 123.098 126.317 123.098 126.707 122.707L133.071 116.343C133.462 115.953 133.462 115.319 133.071 114.929C132.681 114.538 132.047 114.538 131.657 114.929L126 120.586L120.343 114.929C119.953 114.538 119.319 114.538 118.929 114.929C118.538 115.319 118.538 115.953 118.929 116.343L125.293 122.707ZM127 122V77.8526H125V122H127ZM113 63.8526H14V65.8526H113V63.8526ZM2 51.8526V0H0V51.8526H2ZM14 63.8526C7.37258 63.8526 2 58.48 2 51.8526H0C0 59.5846 6.26801 65.8526 14 65.8526V63.8526ZM127 77.8526C127 70.1207 120.732 63.8526 113 63.8526V65.8526C119.627 65.8526 125 71.2252 125 77.8526H127Z",
                                    //        StrokeThickness = 1.5,
                                    //        RenderTransform = new TranslateTransform(-75, 0)
                                    //    },
                                    //    new PathData
                                    //    {
                                    //        Data = "M1.00001 122 V77.8526 C1.00001 70.6729 6.82031 64.8526 14 64.8526H113C120.18 64.8526 126 59.0323 126 51.8526V0",
                                    //        StrokeThickness = 2,
                                    //        RenderTransform = new TranslateTransform(50, 0)
                                    //    },
                                    //    new PathData
                                    //    {
                                    //        Data = "M291.966 99H141.867C141.388 99 141 99.3918 141 99.8752V185.5C141 185.984 141.388 186.376 141.867 186.376H291.966C292.445 186.376 292.833 185.984 292.833 185.5V99.8752C292.833 99.3918 292.445 99 291.966 99Z",
                                    //        StrokeThickness = 1,
                                    //        RenderTransform = new TranslateTransform(-165, 25)
                                    //    },
                                    //    new PathData
                                    //    {
                                    //        Data = "M290.622 184.498V101.211L143.212 101.211V184.498H290.622Z",
                                    //        StrokeThickness = 0.4,
                                    //        RenderTransform = new TranslateTransform(-165, 25)
                                    //    },
                                    //    new PathData
                                    //    {
                                    //        Data = "M220.881 208.623V186.805H212.255V208.623C212.255 208.918 212.373 209.2 212.581 209.408C212.789 209.616 213.072 209.733 213.366 209.733H219.771C220.065 209.733 220.347 209.616 220.556 209.408C220.764 209.2 220.881 208.917 220.881 208.623Z",
                                    //        StrokeThickness = 0.5,
                                    //        RenderTransform = new TranslateTransform(-165, 25)
                                    //    },
                                    //    new PathData
                                    //    {
                                    //        Data = "M212.254 208.355H186.087C185.931 208.355 185.778 208.396 185.643 208.474C185.509 208.552 185.397 208.665 185.319 208.8C185.242 208.935 185.201 209.088 185.202 209.244C185.203 209.4 185.245 209.552 185.323 209.687L188.442 215.014C188.519 215.147 188.631 215.258 188.765 215.335C188.899 215.412 189.051 215.452 189.205 215.452H244.065C244.215 215.453 244.363 215.415 244.494 215.342C244.625 215.269 244.735 215.164 244.815 215.037L248.152 209.71C248.236 209.576 248.282 209.422 248.287 209.264C248.291 209.106 248.253 208.95 248.176 208.812C248.1 208.673 247.988 208.558 247.852 208.478C247.716 208.397 247.561 208.355 247.403 208.355H220.879",
                                    //        StrokeThickness = 0.4,
                                    //        RenderTransform = new TranslateTransform(-165, 25)
                                    //    },
                                    //    new PathData
                                    //    {
                                    //        Data = "M213.829 186.805V191.916C213.829 192.166 213.878 192.414 213.974 192.646C214.07 192.877 214.211 193.088 214.388 193.265C214.565 193.442 214.775 193.583 215.007 193.678C215.238 193.774 215.486 193.824 215.737 193.824H217.3C217.55 193.824 217.798 193.774 218.03 193.678C218.261 193.583 218.472 193.442 218.649 193.265C218.826 193.088 218.967 192.877 219.062 192.646C219.158 192.414 219.208 192.166 219.208 191.916V186.805",
                                    //        StrokeThickness = 0.4,
                                    //        RenderTransform = new TranslateTransform(-165, 25)
                                    //    },
                                    //    new PathData
                                    //    {
                                    //        Data = "M219.208 190.011H213.829",
                                    //        StrokeThickness = 0.4,
                                    //        RenderTransform = new TranslateTransform(-165, 25)
                                    //    },
                                    //    new PathData
                                    //    {
                                    //        Data = "M185.648 210.243L188.155 215.83C188.255 216.052 188.417 216.241 188.622 216.374C188.827 216.506 189.066 216.577 189.31 216.577H243.861C244.095 216.577 244.325 216.512 244.524 216.389C244.724 216.266 244.885 216.089 244.991 215.88L247.824 210.251",
                                    //        StrokeThickness = 0.4,
                                    //        RenderTransform = new TranslateTransform(-165, 25)
                                    //    },
                                    //    new PathData
                                    //    {
                                    //        Data = "M195.861 216.571H190.704V217H195.861V216.571Z",
                                    //        StrokeThickness = 0.4,
                                    //        RenderTransform = new TranslateTransform(-165, 25)
                                    //    },
                                    //    new PathData
                                    //    {
                                    //        Data = "M219.352 216.571H214.195V217H219.352V216.571Z",
                                    //        StrokeThickness = 0.4,
                                    //        RenderTransform = new TranslateTransform(-165, 25)
                                    //    },
                                    //    new PathData
                                    //    {
                                    //        Data = "M242.842 216.571H237.686V217H242.842V216.571Z",
                                    //        StrokeThickness = 0.4,
                                    //        RenderTransform = new TranslateTransform(-165, 25)
                                    //    },
                                    //};

                                }
                                if (Cancelled_RefreshData(e, bwk))
                                {
                                    return;
                                }
                                //ModifiedPCinputList();
                                //if (Cancelled_RefreshData(e, bwk))
                                //{
                                //    return;
                                //}
                                USBDisenable();
                                if (Cancelled_RefreshData(e, bwk))
                                {
                                    return;
                                }
                            }
                            else
                            {
                                _log?.Info("PC1 or PC2 not found in pcsList.");
                            }

                        }
                        else
                        {
                            _log?.Info("inputList is null or pcsList is null or count < 2.");
                        }

                        if (Cancelled_RefreshData(e, bwk))
                        {
                            return;
                        }

                        #region PIP/PBP
                        _log?.Info("[KvmViewModel]PIP/PBP...");
                        if (mi.CapabilityDic.ContainsKey("E9"))
                        {
                            _log?.Info("[KvmViewModel]Have 0xE9");

                            if (!isScreenPartition)
                            {
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
                                    if (Cancelled_RefreshData(e, bwk))
                                    {
                                        return;
                                    }
                                }
                                _pipPbpCaps = DdpmCommonHelper.ParsingHexStringToWords(_pxpString);
                                if (Cancelled_RefreshData(e, bwk))
                                {
                                    return;
                                }

                                //Check if this monitor has Pxp mode capabilities
                                if ((_pipPbpCaps != null) && (_pipPbpCaps.Length > 0))
                                {
                                    foreach (UInt16 code in _pipPbpCaps)
                                    {
                                        if (!PxPcodeDictionary.ContainsKey(code))
                                        {
                                            PxPcodeDictionary.Add(code, null);
                                        }
                                        if (Cancelled_RefreshData(e, bwk))
                                        {
                                            return;
                                        }
                                    }
                                    //Get current monitor's Pxp mode
                                    ObjGetVCP ret_PxP = DdpmCommonHelper.DeviceManagerSA.GetPxpMode(mi).Result;
                                    if (Cancelled_RefreshData(e, bwk))
                                    {
                                        return;
                                    }
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
                                                pipSmallMode = true;
                                                pipLargeMode = false;
                                                pbpModde = false;
                                                break;

                                            case 0x22:
                                                pipSmallMode = true;
                                                pipLargeMode = false;
                                                pbpModde = false;
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
                                                pipSmallMode = false;
                                                pipLargeMode = false;
                                                pbpModde = true;
                                                break;

                                            default:
                                                pipSmallMode = false;
                                                pipLargeMode = false;
                                                pbpModde = false;
                                                break;
                                        }
                                    }
                                }
                                else
                                {
                                    _log?.Info("[KvmViewModel] _pipPbpCaps is null or Length not > 0");
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
                            pipSmallMode = false;
                            pipLargeMode = false;
                            pbpModde = false;
                            //    NoPxP = Visibility.Visible;
                            //    isPxP = Visibility.Collapsed;
                        }
                        #endregion PIP/PBP
                    }
                }

            }
            catch (Exception ex)
            {
                _log?.Error(ex, "[DoWork_RefreshData] exception");
            }
            _log?.Info("[KvmViewModel] DoWork_USBKVM end");
            entryUSBKVM = DateTime.Now;
            _log?.Info($"[DoWork_USBKVM Time]:{entryUSBKVM.ToString("yyyy-MM-dd hh:mm:ss.fff")}");
        }

        private void RunWorkerCompleted_USBKVM(object sender, RunWorkerCompletedEventArgs e)
        {
            _log?.Info("[KvmViewModel] RunWorkerCompleted_USBKVM start");
            DateTime entryUSBKVM = DateTime.Now;
            _log?.Info($"[RunWorkerCompleted_USBKVM Time]:{entryUSBKVM.ToString("yyyy-MM-dd hh:mm:ss.fff")}");
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
            if (toKVMSetPage)
            {
                InputSourceFullView _inputSourceFullView = new InputSourceFullView();
                _inputSourceFullView.DataContext = this;
                DdpmCommonHelper.ModuleOwner?.OpenFullView(_inputSourceFullView);
            }
            _log?.Info("[KvmViewModel] RunWorkerCompleted_USBKVM End");
            entryUSBKVM = DateTime.Now;
            _log?.Info($"[RunWorkerCompleted_USBKVM Time]:{entryUSBKVM.ToString("yyyy-MM-dd hh:mm:ss.fff")}");
            //Handling the result and final process
        }

        private void RunWorkerCompleted_USBKVMisON(object sender, RunWorkerCompletedEventArgs e)
        {
            _log?.Info("[KvmViewModel] RunWorkerCompleted_USBKVMisON start");
            DateTime entryUSBKVM = DateTime.Now;
            _log?.Info($"[RunWorkerCompleted_USBKVMisON Time]:{entryUSBKVM.ToString("yyyy-MM-dd hh:mm:ss.fff")}");
            if (e.Cancelled)
            {
                return;
            }
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
            _log?.Info("[KvmViewModel] RunWorkerCompleted_USBKVM End");
            entryUSBKVM = DateTime.Now;
            _log?.Info($"[RunWorkerCompleted_USBKVMisON Time]:{entryUSBKVM.ToString("yyyy-MM-dd hh:mm:ss.fff")}");
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
                PC1_Input = pcsList["PC1"].InputType;
                _PC1selectInput = _inputsList.Find(x => (x.Type == pcsList["PC1"].InputType));
                OnPropertyChanged("PC1_Input");
                OnPropertyChanged("PC1Inputs_Selected");
            }
            else if (pcnum == "PC2")
            {
                PC2_Input = pcsList["PC2"].InputType;
                _PC2selectInput = _inputsList.Find(x => (x.Type == pcsList["PC2"].InputType));
                OnPropertyChanged("PC2_Input");
                OnPropertyChanged("PC2Inputs_Selected");
            }
            else if (pcnum == "PC3")
            {
                PC3_Input = pcsList["PC3"].InputType;
                _PC3selectInput = _inputsList.Find(x => (x.Type == pcsList["PC3"].InputType));
                OnPropertyChanged("PC3_Input");
                OnPropertyChanged("PC3Inputs_Selected");
            }
            else if (pcnum == "PC4")
            {
                PC4_Input = pcsList["PC4"].InputType;
                _PC4selectInput = _inputsList.Find(x => (x.Type == pcsList["PC4"].InputType));
                OnPropertyChanged("PC4_Input");
                OnPropertyChanged("PC4Inputs_Selected");
            }
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
            OnPropertyChanged("PC1USB_Selected");
            OnPropertyChanged("PC2USB_Selected");
            OnPropertyChanged("PC3USB_Selected");
            OnPropertyChanged("PC4USB_Selected");
        }

        private void SelectUSB(string usb, string pcnum)
        {
            _log?.Info("[SelectUSB]...");
            if (pcsList.ContainsKey(pcnum))
            {
                PCsInfo pcInfo = new PCsInfo();
                foreach (var input in inputList)
                {
                    if (pcsList[pcnum].InputType == input.Key)
                    {
                        pcInfo.InputType = input.Key;
                        pcInfo.InputName = input.Value.InputName;
                        pcInfo.USBUpstream = usb;
                        pcInfo.Code = input.Value.Code;
                        break;
                    }
                }
                pcsList[pcnum] = pcInfo;
                _log?.Info("[SelectUSB] " + pcnum + " USB is " + pcsList[pcnum].USBUpstream);
                _log?.Info("[SelectUSB] " + pcnum + " original_USB is " + original_pcsList[pcnum].USBUpstream);
                if (pcnum == "PC1")
                {
                    _PC1selectUSB = _usbsList.Find(x => (x.Type == pcsList[pcnum].USBUpstream));
                    OnPropertyChanged("PC1USB_Selected");
                }
                else if (pcnum == "PC2")
                {
                    _PC2selectUSB = _usbsList.Find(x => (x.Type == pcsList[pcnum].USBUpstream));
                    OnPropertyChanged("PC2USB_Selected");
                }
                else if (pcnum == "PC3")
                {
                    _PC3selectUSB = _usbsList.Find(x => (x.Type == pcsList[pcnum].USBUpstream));
                    OnPropertyChanged("PC3USB_Selected");
                }
                else if (pcnum == "PC4")
                {
                    _PC4selectUSB = _usbsList.Find(x => (x.Type == pcsList[pcnum].USBUpstream));
                    OnPropertyChanged("PC4USB_Selected");
                }
            }
            else
            {
                _log?.Info("[SelectUSB] " + pcnum + " not found in pcsList.");
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
                _log?.Info($"[KvmViewModel] MainInput PC1-{pcsList["PC1"].InputType} Done with {result}.");
                if (b)
                {
                    KvmModule.SelectedHomeDevice.MonitorInfo.inputSource = pcsList["PC1"].InputType;
                }
                //}
            }
            else
            {
                _log?.Info("PC1 not found in pcsList.");
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
            BackgroundWorker m_bw = new BackgroundWorker()
            {
                WorkerReportsProgress = false,
                WorkerSupportsCancellation = false
            };
            m_bw.DoWork += delegate
            {
                //UInt16 capCode = SelectedSplitItem.ISplit.PbpCapabilityCode;
                if (DdpmCommonHelper.DeviceManagerSA != null)
                {
                    DdpmCommonHelper.DeviceManagerSA.TogglePipPosition(DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo);
                }
            };
            m_bw.RunWorkerCompleted += delegate
            {
                IsBusy = false;
            };
            IsBusy = true;
            m_bw.RunWorkerAsync();
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

        public void UpdatePCList()
        {
            if (pcsList.TryGetValue("PC1", out var pc1) && pcsList.TryGetValue("PC2", out var pc2))
            {
                _PC1selectInput = _inputsList.Find(x => (x.Type == pcsList["PC1"].InputType));
                _log?.Info("[KvmViewModel][UpdatePCList] PC1 input source : " + pcsList["PC1"].InputType);
                _PC1selectUSB = _usbsList.Find(x => (x.Type == pcsList["PC1"].USBUpstream));
                OnPropertyChanged("PC1Inputs_Selected");
                ModifiedPCinputList();
                _PC2selectInput = _inputsList2.Find(x => (x.Type == pcsList["PC2"].InputType));
                if (_PC2selectInput == null)
                {
                    _PC2selectInput = _inputsList2[0];
                    pcsList["PC2"].InputType = _inputsList2[0].Type;
                    pcsList["PC2"].Code = inputList[_inputsList2[0].Type].Code;
                    pcsList["PC2"].InputName = inputList[_inputsList2[0].Type].InputName;
                    pcsList["PC2"].USBUpstream = inputList[_inputsList2[0].Type].USBUpstream;

                }
                _PC2selectUSB = _usbsList.Find(x => (x.Type == pcsList["PC2"].USBUpstream));
                OnPropertyChanged("PC2Inputs_Selected");
                PC1_Input = pcsList["PC1"].InputType;
                PC2_Input = pcsList["PC2"].InputType;
                InputName1 = pcsList["PC1"].InputName;
                InputName2 = pcsList["PC2"].InputName;
                if (pcsList.Count >= 3)
                {
                    ModifiedPCinputList();
                    if (pcsList.TryGetValue("PC3", out var pc3))
                    {
                        _PC3selectInput = _inputsList3.Find(x => (x.Type == pcsList["PC3"].InputType));
                        if (_PC3selectInput == null)
                        {
                            _PC3selectInput = _inputsList3[0];
                            pcsList["PC3"].InputType = _inputsList3[0].Type;
                            pcsList["PC3"].Code = inputList[_inputsList3[0].Type].Code;
                            pcsList["PC3"].InputName = inputList[_inputsList3[0].Type].InputName;
                            pcsList["PC3"].USBUpstream = inputList[_inputsList3[0].Type].USBUpstream;
                        }
                        _PC3selectUSB = _usbsList.Find(x => (x.Type == pcsList["PC3"].USBUpstream));
                        OnPropertyChanged("PC3Inputs_Selected");
                        PC3_Input = pcsList["PC3"].InputType;
                        if (!USBKVMisON)
                        {
                            pcsList["PC3"].InputName = "PC3";
                        }
                        InputName3 = pcsList["PC3"].InputName;
                        PC3_Visibility = Visibility.Visible;
                        if (pcsList.Count == 4)
                        {
                            ModifiedPCinputList();
                            if (pcsList.TryGetValue("PC4", out var pc4))
                            {
                                _PC4selectInput = _inputsList4.Find(x => (x.Type == pcsList["PC4"].InputType));
                                if (_PC4selectInput == null)
                                {
                                    _PC4selectInput = _inputsList4[0];
                                    pcsList["PC4"].InputType = _inputsList4[0].Type;
                                    pcsList["PC4"].Code = inputList[_inputsList4[0].Type].Code;
                                    pcsList["PC4"].InputName = inputList[_inputsList4[0].Type].InputName;
                                    pcsList["PC4"].USBUpstream = inputList[_inputsList4[0].Type].USBUpstream;
                                }
                                _PC4selectUSB = _usbsList.Find(x => (x.Type == pcsList["PC4"].USBUpstream));
                                OnPropertyChanged("PC4Inputs_Selected");
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
                OnPropertyChanged("PC1_Input");
                OnPropertyChanged("PC2_Input");
                OnPropertyChanged("PC3_Input");
                OnPropertyChanged("PC4_Input");
                USBDisenable();
                original_pcsList = pcsList.ToDictionary(entry => entry.Key, entry => entry.Value);
            }
            else
            {
                _log?.Info("PC1 or PC2 not found in pcsList.");
            }
        }

        public void CancelSetUSBKVM()
        {
            if (original_pcsList != null)
            {
                pcsList = original_pcsList.ToDictionary(entry => entry.Key, entry => entry.Value);
                UpdatePCList();
            }
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
                Task.Delay(1000).Wait();
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

        public void LoadnewLeftView(bool def)
        {
            if (!def && USBKVMisON && !NKVMisON && !NoKVMisON)
            {
                KvmModule._leftView = new KvmLeftView(this);
                KvmModule._leftView.DataContext = this;
            }
            else
            {
                KvmModule._leftView = null;
            }
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

        public void isOnNoKVM(bool ison)
        {
            //if (ison)
            //{
            //    DdpmCommonHelper.DeviceManagerSA.SupportedNKVMMonitors().Wait();
            //}
            DdpmCommonHelper.DeviceManagerSA.SetNoKVM(DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo, ison).Wait();
        }

        #region KVM Loading

        public void NKVMOpenUI()
        {
            BackgroundWorker m_bw = new BackgroundWorker()
            {
                WorkerReportsProgress = false,
                WorkerSupportsCancellation = false
            };
            m_bw.DoWork += NKVMOpenUI_Dowork;
            m_bw.RunWorkerCompleted += NKVMOpenUI_Done;
            m_bw.RunWorkerAsync();
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
                    _log?.Info("Named pipe is not Connected, so CreatNewNamedpipe again.");
                    DdpmCommonHelper.DeviceManagerSA.CreatNewNamedpipe().Wait();
                }
                if (DdpmCommonHelper.DeviceManagerSA.IsNamedpipeConnected().Result)
                {
                    isOnNKVM(true);
                    isOnNoKVM(false);
                    NoKVMisON = false;
                    NKVMisON = true;
                    _log?.Info("NKVMOpenUI i = " + i);
                    DdpmCommonHelper.DeviceManagerSA.CallShowNKVM(0, 100, 100).Wait();
                    e.Result = true;
                    break;
                }
                i++;
                Task.Delay(500).Wait();
            }
            if (i == 120)
            {
                _log?.Info("Named pipe is not Connected or time out");
                e.Result = false;
            }
            //if (DdpmCommonHelper.DeviceManagerSA.IsNamedpipeConnected().Result)
            //{
            //    isOnNKVM(true);
            //    _log?.Info("NKVMOpenUI...");
            //    DdpmCommonHelper.DeviceManagerSA.CallShowNKVM(0, 100, 100).Wait();
            //}
            //else
            //{
            //    _log?.Info("Named pipe is not Connected or time out");
            //}
        }
        private void NKVMOpenUI_Done(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                _log?.Error("NKVMOpenUI error : " + e.Error.ToString());
            }
            if (e.Result != null)
            {
                Task.Delay(3000).Wait();
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

        public void ReGetMonitorInfo()
        {
            MonitorInfo monitorInfo = new MonitorInfo();
            List<MonitorInfo> monitorList = new List<MonitorInfo>();
            monitorInfo = KvmModule.SelectedHomeDevice.MonitorInfo;
            int i = 0;
            while (i < 20)
            {
                Task.Delay(2000).Wait();
                monitorList = DdpmCommonHelper.DeviceManagerSA.GetMonitors().Result;
                if (monitorList != null && monitorList.Count > 0)
                {
                    monitorInfo = monitorList.Find(x => x.modelName == KvmModule.SelectedHomeDevice.MonitorInfo.modelName &&
                                                        x.edid.ServiceTag == KvmModule.SelectedHomeDevice.MonitorInfo.edid.ServiceTag);
                    if (monitorInfo != null)
                    {
                        KvmModule.SelectedHomeDevice.MonitorInfo = monitorInfo;
                        break;
                    }
                }
                i++;
            }
        }

        public void SetPCInput(Dictionary<string, PCsInfo> pcsList, InputSourceObj PC2input, InputSourceObj PC3input, InputSourceObj PC4input)
        {
            //MonitorInfo monitorInfo = new MonitorInfo();
            //List<MonitorInfo> monitorList = new List<MonitorInfo>();
            if (pcsList["PC1"].InputType != KvmModule.SelectedHomeDevice.MonitorInfo.inputSource)
            {
                //Jason by U3824DW input source greyed out
                if (_curPxpMode != 0x0)
                {
                    if (pcsList["PC1"].InputType == original_pcsList["PC2"].InputType)
                    {
                        _log?.Info($"[KvmViewModel] SetPCInput VideoSwap 1 <-> 2.");
                        if (!DdpmCommonHelper.DeviceManagerSA.VideoSwap(KvmModule.SelectedHomeDevice.MonitorInfo, 0, 1).Result)
                        {
                            DdpmCommonHelper.DeviceManagerSA.VideoSwap(KvmModule.SelectedHomeDevice.MonitorInfo, 0, 1).Wait();
                        }
                    }
                    else if (pcsList.ContainsKey("PC3") && pcsList["PC1"].InputType == original_pcsList["PC3"].InputType)
                    {
                        _log?.Info($"[KvmViewModel] SetPCInput VideoSwap 1 <-> 3.");
                        if (!DdpmCommonHelper.DeviceManagerSA.VideoSwap(KvmModule.SelectedHomeDevice.MonitorInfo, 0, 2).Result)
                        {
                            DdpmCommonHelper.DeviceManagerSA.VideoSwap(KvmModule.SelectedHomeDevice.MonitorInfo, 0, 2).Wait();
                        }
                    }
                    else if (pcsList.ContainsKey("PC4") && pcsList["PC1"].InputType == original_pcsList["PC4"].InputType)
                    {
                        _log?.Info($"[KvmViewModel] SetPCInput VideoSwap 1 <-> 4.");
                        if (!DdpmCommonHelper.DeviceManagerSA.VideoSwap(KvmModule.SelectedHomeDevice.MonitorInfo, 0, 3).Result)
                        {
                            DdpmCommonHelper.DeviceManagerSA.VideoSwap(KvmModule.SelectedHomeDevice.MonitorInfo, 0, 3).Wait();
                        }
                    }
                    else
                    {
                        CurrentInputChange();
                    }
                }
                else
                {
                    CurrentInputChange();
                }
                ReGetMonitorInfo();
                //int i = 0;
                //while (i < 20)
                //{
                //    Task.Delay(2000).Wait();
                //    monitorList = DdpmCommonHelper.DeviceManagerSA.GetMonitors().Result;
                //    if (monitorList != null && monitorList.Count > 0)
                //    {
                //        monitorInfo = monitorList.Find(x => x.modelName == KvmModule.SelectedHomeDevice.MonitorInfo.modelName &&
                //                                            x.edid.ServiceTag == KvmModule.SelectedHomeDevice.MonitorInfo.edid.ServiceTag);
                //        if (monitorInfo != null)
                //        {
                //            break;
                //        }
                //    }
                //    i++;
                //}
            }
            //else
            //{
            //    monitorInfo = KvmModule.SelectedHomeDevice.MonitorInfo;
            //}
            //if ((pcsList.ContainsKey("PC2") && pcsList["PC2"] != original_pcsList["PC2"]) ||
            //    (pcsList.ContainsKey("PC3") && pcsList["PC3"] != original_pcsList["PC3"]) ||
            //    (pcsList.ContainsKey("PC4") && pcsList["PC4"] != original_pcsList["PC4"]))
            //{
            bool res = DdpmCommonHelper.DeviceManagerSA.SetSubInputs(
                    KvmModule.SelectedHomeDevice.MonitorInfo,
                    PC2input, PC3input, PC4input).Result;
            string result = res ? "Success" : "Failed";
            _log?.Info($"[KvmViewModel] SubInputs PC Done with {result}.");
            //}
        }

        public void FinishtoSetPCs()
        {
            try
            {
                //set input source
                if (pcsList != null)
                {
                    // Since Profile issue PCs will be inconsistent need to rewrite each change,
                    // Monitor info is only updated by monitor info updated. Originally get from profile not real monitor status
                    InputSourceObj pc2input = new InputSourceObj();
                    InputSourceObj pc3input = new InputSourceObj();
                    InputSourceObj pc4input = new InputSourceObj();
                    if (!isPCsListSame(pcsList, original_pcsList))
                    {
                        Dictionary<string, PCsInfo> usbPCsList = new Dictionary<string, PCsInfo>();
                        usbPCsList = pcsList.ToDictionary(entry => entry.Key, entry => entry.Value);
                        foreach (var pcs in usbPCsList)
                        {
                            if (string.IsNullOrEmpty(pcs.Value.USBUpstream))
                            {
                                pcs.Value.USBUpstream = DdpmCommonHelper.DeviceManagerSA.GetUSBUpstream(KvmModule.SelectedHomeDevice.MonitorInfo, pcs.Value.InputType).Result;
                            }
                        }

                        if (usbPCsList.ContainsKey("PC3") && usbPCsList.ContainsKey("PC4"))
                        {
                            DdpmCommonHelper.DeviceManagerSA.SetAllUSBUpstream(KvmModule.SelectedHomeDevice.MonitorInfo, usbPCsList["PC1"].InputType, usbPCsList["PC1"].USBUpstream, usbPCsList["PC2"].InputType, usbPCsList["PC2"].USBUpstream,
                                                                                                                        usbPCsList["PC3"].InputType, usbPCsList["PC3"].USBUpstream, usbPCsList["PC4"].InputType, usbPCsList["PC4"].USBUpstream);
                        }
                        else if (usbPCsList.ContainsKey("PC3") && !usbPCsList.ContainsKey("PC4"))
                        {
                            DdpmCommonHelper.DeviceManagerSA.SetAllUSBUpstream(KvmModule.SelectedHomeDevice.MonitorInfo, usbPCsList["PC1"].InputType, usbPCsList["PC1"].USBUpstream, usbPCsList["PC2"].InputType, usbPCsList["PC2"].USBUpstream,
                                                                                                                        usbPCsList["PC3"].InputType, usbPCsList["PC3"].USBUpstream);
                        }
                        else
                        {
                            DdpmCommonHelper.DeviceManagerSA.SetAllUSBUpstream(KvmModule.SelectedHomeDevice.MonitorInfo, usbPCsList["PC1"].InputType, usbPCsList["PC1"].USBUpstream, usbPCsList["PC2"].InputType, usbPCsList["PC2"].USBUpstream);
                        }

                        bool bin = DdpmCommonHelper.DeviceManagerSA.SetInputSourcelist(KvmModule.SelectedHomeDevice.MonitorInfo, inputList).Result;
                        //bool bpcs = DdpmCommonHelper.DeviceManagerSA.SetUSBKVMPCsList(KvmModule.SelectedHomeDevice.MonitorInfo, usbPCsList).Result;

                        if (pcsList.TryGetValue("PC1", out var pc1) && pcsList.TryGetValue("PC2", out var pc2))
                        {
                            InputSourceObj pc1input = new InputSourceObj((UInt16)pcsList["PC1"].Code, pcsList["PC1"].InputType);
                            pc2input = new InputSourceObj((UInt16)pcsList["PC2"].Code, pcsList["PC2"].InputType);
                            inputList[pcsList["PC1"].InputType].InputName = pcsList["PC1"].InputName;
                            inputList[pcsList["PC2"].InputType].InputName = pcsList["PC2"].InputName;
                            if (pcsList.Count >= 3)
                            {
                                if (pcsList.TryGetValue("PC3", out var pc3))
                                {
                                    pc3input = new InputSourceObj((UInt16)pcsList["PC3"].Code, pcsList["PC3"].InputType);
                                    inputList[pcsList["PC3"].InputType].InputName = pcsList["PC3"].InputName;
                                    if (pcsList.Count == 4)
                                    {
                                        if (pcsList.TryGetValue("PC4", out var pc4))
                                        {
                                            pc4input = new InputSourceObj((UInt16)pcsList["PC4"].Code, pcsList["PC4"].InputType);
                                            inputList[pcsList["PC4"].InputType].InputName = pcsList["PC4"].InputName;
                                            SetPCInput(pcsList, pc2input, pc3input, pc4input);
                                        }
                                        else
                                        {
                                            _log?.Info("PC4 not found in pcsList.");
                                        }
                                    }
                                    else
                                    {
                                        SetPCInput(pcsList, pc2input, pc3input, null);
                                    }
                                }
                                else
                                {
                                    _log?.Info("PC3 not found in pcsList.");
                                }
                            }
                            else
                            {
                                SetPCInput(pcsList, pc2input, null, null);
                            }

                            original_pcsList = pcsList.ToDictionary(entry => entry.Key, entry => entry.Value);
                            //isOnUSBKVM(true);//bool b = DdpmCommonHelper.DeviceManagerSA.SetOnUSBKVM(true).Result;
                        }
                        else
                        {
                            _log?.Info("PC1 or PC2 not found in pcsList.");
                        }
                    }
                    else
                    {
                        _log?.Info("[KvmViewModel] PCsList is not same.");
                        if (pcsList.ContainsKey("PC2"))
                        {
                            _log?.Info("[KvmViewModel] PCsList has PC2.");
                            pc2input = new InputSourceObj((UInt16)pcsList["PC2"].Code, pcsList["PC2"].InputType);
                            if (pcsList.ContainsKey("PC3"))
                            {
                                _log?.Info("[KvmViewModel] PCsList has PC3.");
                                pc3input = new InputSourceObj((UInt16)pcsList["PC3"].Code, pcsList["PC3"].InputType);
                            }
                            else
                            {
                                pc3input = null;
                            }
                            if (pcsList.ContainsKey("PC4"))
                            {
                                _log?.Info("[KvmViewModel] PCsList has PC4.");
                                pc4input = new InputSourceObj((UInt16)pcsList["PC4"].Code, pcsList["PC4"].InputType);
                            }
                            else
                            {
                                pc4input = null;
                            }
                            bool res = DdpmCommonHelper.DeviceManagerSA.SetSubInputs(KvmModule.SelectedHomeDevice.MonitorInfo,
                                                                                        pc2input, pc3input, pc4input).Result;
                            string result = res ? "Success" : "Failed";
                            _log?.Info($"[KvmViewModel] SubInputs PC Done with {result}.");
                        }
                        else
                        {
                            _log?.Info("[KvmViewModel] PCsList not has PC2.");
                        }
                    }
                }
                else
                {
                    _log?.Info("pcsList is null");
                }
            }
            catch (Exception ex)
            {
                _log?.Error(ex, "[FinishtoSetPCs] exception");
            }
        }

        public void UpdateHotkeyData(MonitorInfo monitorInfo, List<InputSourceObj> inputList)
        {
            if (DdpmCommonHelper.DeviceManagerSA != null)
            {
                Task.Run(() =>
                {
                    //HomeDevice? selectedHomeDevice = KvmModule?.SelectedHomeDevice;
                    var temp = DdpmCommonHelper.DeviceManagerSA.ReadCurrentHotkey(monitorInfo).Result;
                    HotkeySettings curHotkey = temp.Item1;
                    HotkeyInfo? hotkeyInfo = curHotkey.HotkeyInfo.SingleOrDefault(x => x.Job.Equals(HotkeyType.KvmSwitchInputSource));
                    if (hotkeyInfo != null && hotkeyInfo.KeyCode != VirtualKey.None)
                    {
                        //update inputsource
                        hotkeyInfo.InputSource.Clear();
                        inputList.ForEach(x => hotkeyInfo.InputSource.Add(x));
                        bool saveSettings = DdpmCommonHelper.DeviceManagerSA.SaveHotkeySetting(monitorInfo, hotkeyInfo).Result;
                        if (!saveSettings)
                        {
                            DdpmCommonHelper.WriteUILog($"[USBKVM] USBKVM => edit input source,Update Hotkey Data fail.");
                        }
                    }
                });
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
                _log?.Info("[PBP_MouseLeftDown]PxPcodeDictionary not find key " + PbpCapabilityCode);
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

        private bool Cancelled_RefreshData(DoWorkEventArgs e, BackgroundWorker bw)
        {
            if (bw != null && bw.CancellationPending)
            {
                Debug.WriteLine("[KvmViewModel] Cancelled_RefreshData.");
                _log?.Info("[KvmViewModel] Cancelled_RefreshData.");
                e.Cancel = true;
                return true;
            }
            return false;
        }

        public void CallCancel()
        {
            if (bw != null && bw.IsBusy)
            {
                _log?.Info("[KvmViewModel] CallCancel.");
                bw.CancelAsync();
                if (guid.HasValue)
                {
                    DdpmCommonHelper.DeviceManagerSA.CancelVcpTask(guid.Value);
                }
            }
        }

        public void UpdateArrow(bool isshow)
        {
            if (isshow)
            {
                ArrowinFullscreen = Visibility.Visible;
            }
            else
            {
                ArrowinFullscreen = Visibility.Collapsed;
            }
            OnPropertyChanged("ArrowinFullscreen");
        }

        public void ChangePC(bool isNext)
        {
            MonitorInfo monitorInfo = new MonitorInfo();
            List<MonitorInfo> monitorList = new List<MonitorInfo>();
            if (pcsList != null && pcsList.Count > 0 && subInputList != null && subInputList.Count > 0)
            {
                pcsList = DdpmCommonHelper.DeviceManagerSA.ChangePC(KvmModule.SelectedHomeDevice.MonitorInfo, pcsList, subInputList, isNext).Result;
                if (pcsList != null && pcsList.ContainsKey("PC1"))
                {
                    CurrentInputChange();
                    PC1_Input = pcsList["PC1"].InputType;
                    OnPropertyChanged("PC1_Input");
                    original_pcsList = pcsList.ToDictionary(entry => entry.Key, entry => entry.Value);
                }
            }
        }

        public void SelectKVM()
        {
            if (NKVMisON)
            {
                isNKVM = true;
            }
            else if (NoKVMisON || isScreenPartition || !USBKVMisON)
            {
                isNoKVM = true;
            }
            else
            {
                isUSBKVM = true;
            }
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
            else if (e.vcpcode.Equals("input select") && DdpmCommonHelper.DeviceManagerSA.GetOnUSBKVM(KvmModule.SelectedHomeDevice.MonitorInfo).Result)
            {
                if (pcsList["PC1"].InputType != e.value)
                {
                    KvmModule.SelectedHomeDevice.MonitorInfo.inputSource = e.value;
                    subInputList = DdpmCommonHelper.DeviceManagerSA.GetSubInputList(KvmModule.SelectedHomeDevice.MonitorInfo).Result;
                    if ((inputList != null && inputList.Count > 0) && (subInputList != null && subInputList.Count > 0))
                    {
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
                        pcsList["PC1"] = original_pcsList[original_pcsList.FirstOrDefault(x => x.Value.InputType == e.value).Key];
                        pcsList["PC2"] = original_pcsList[original_pcsList.FirstOrDefault(x => x.Value.InputType == subInputs[0].Name).Key];
                        _PC1selectInput = _inputsList.Find(x => (x.Type == pcsList["PC1"].InputType));
                        _log?.Info("[KvmViewModel] PC1 input source : " + pcsList["PC1"].InputType);
                        _PC1selectUSB = _usbsList.Find(x => (x.Type == pcsList["PC1"].USBUpstream));
                        ModifiedPCinputList();
                        _PC2selectInput = _inputsList2.Find(x => (x.Type == pcsList["PC2"].InputType));
                        _PC2selectUSB = _usbsList.Find(x => (x.Type == pcsList["PC2"].USBUpstream));
                        PC1_Input = pcsList["PC1"].InputType;
                        PC2_Input = pcsList["PC2"].InputType;
                        if (subInputs.Count > 1)
                        {
                            ModifiedPCinputList();
                            pcsList["PC3"] = original_pcsList[original_pcsList.FirstOrDefault(x => x.Value.InputType == subInputs[1].Name).Key];
                            _PC3selectInput = _inputsList3.Find(x => (x.Type == pcsList["PC3"].InputType));
                            _PC3selectUSB = _usbsList.Find(x => (x.Type == pcsList["PC3"].USBUpstream));
                            PC3_Input = pcsList["PC3"].InputType;
                            if (subInputs.Count > 2)
                            {
                                ModifiedPCinputList();
                                pcsList["PC4"] = original_pcsList[original_pcsList.FirstOrDefault(x => x.Value.InputType == subInputs[2].Name).Key];
                                _PC4selectInput = _inputsList4.Find(x => (x.Type == pcsList["PC4"].InputType));
                                _PC4selectUSB = _usbsList.Find(x => (x.Type == pcsList["PC4"].USBUpstream));
                                PC4_Input = pcsList["PC4"].InputType;
                            }
                        }
                    }
                    else
                    {
                        PC1_Input = e.value;
                    }
                    OnPropertyChanged("PC1_Input");
                    OnPropertyChanged("PC2_Input");
                    OnPropertyChanged("PC3_Input");
                    OnPropertyChanged("PC4_Input");
                    OnPropertyChanged("PC1Inputs_Selected");
                    OnPropertyChanged("PC2Inputs_Selected");
                    OnPropertyChanged("PC3Inputs_Selected");
                    OnPropertyChanged("PC4Inputs_Selected");
                    USBDisenable();
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

        public Visibility CurrentCultureArrowRight {
            get {
                return CultureInfo.CurrentUICulture.Name == "ar-SA" ? Visibility.Collapsed : Visibility.Visible;
            } 
        }

        public Visibility CurrentCultureArrowLeft
        {
            get
            {
                return CurrentCultureArrowRight == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        private Visibility _fullMode { get; set; }

        public Visibility FullMode
        {
            get
            {
                return _fullMode;
            }

            set {
                _fullMode = value;
                OnPropertyChanged("FullMode");
            }
        }

        public Visibility ShrinkMode => FullMode == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
    }
}