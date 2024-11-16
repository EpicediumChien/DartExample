using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DDPM.SA.Common;
using DDPM.SA.Common.Display;
using DDPM.SA.Common.Settings;
using DDPM.UI.Common;
using DDPM.UI.Common.EAEM;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using DDPM.UI.Common.UserControls;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Input;
using VcpCore.Common;
using Windows.UI.ViewManagement;

namespace DDPM.UI.Module.PipPbp
{
    public class VideoSwapComboBoxInputSourceItem : InputInfo
    {
        public string InputSourceKey { get; set; } = "";
        public string DisplayName 
        {
            get
            {
                if (InputName == InputSourceKey)
                {
                    return InputSourceKey;
                }
                else
                {
                    return InputSourceKey + " - " + InputName;
                }
            }
        }

        public InputSourceObj ConvertToInputSourceObj()
        {
            return new InputSourceObj((ushort)Code, InputSourceKey);
        }
    }
    public class PipPbpViewModel : ObservableObject
    {
        public HomeDevice SelectedHomeDevice;

        public IDeviceManagerSA DeviceManagerSA;
        public IModuleOwner? ModuleOwner { get; set; }

        #region ctor

        public PipPbpViewModel()
        {
            //Setup defualts
            FullScreenClickCommand = new RelayCommand<SplitItem>(OnFullScreenClicked);
            PipSmallClickCommand = new RelayCommand<SplitItem>(OnPipSmallClicked);
            PipLargeClickCommand = new RelayCommand<SplitItem>(OnPipLargeClicked);
            PbpItemClickCommand = new RelayCommand<SplitItem>(OnPbpItemClicked);
            PipTogglePositionClickCommand = new RelayCommand(OnPipTogglePositionClicked);

            ModuleOwner = DdpmCommonHelper.ModuleOwner;
            SelectedHomeDevice = ModuleOwner.SelectedHomeDevice;
            DeviceManagerSA = (IDeviceManagerSA)DdpmCommonHelper.DeviceManagerSA;
        }

        #endregion ctor

        #region Log
        public ILog? Log { get; set; }

        private void LogInfo(string msg)
        {
            if (Log != null)
            {
                Log.Info(msg);
            }
        }
        #endregion Log

        #region Special SplitItems - need View assign it before calling RefreshData()

        public SplitItem? SplitItem_Off { get; set; }
        public SplitItem? SplitItem_PipSmall { get; set; }
        public SplitItem? SplitItem_PipLarge { get; set; }
        public SplitListView? SplitListView_Pbp { get; set; }

        #endregion Special SplitItems - need View assign it before calling RefreshData()

        #region RefreshData - Init and SelectedHomeDevice changed

        public void RefreshData()
        {
            //Please set below data (once) before calling to RefreshData
            if ((SplitItem_Off == null) || (SplitItem_PipSmall == null) || (SplitItem_PipLarge == null))
                throw new InvalidOperationException("SplitItem_Off,SplitItem_PipSmall, and SplitItem_PipLarge must be set before calling RefreshData()");

            //Update SelectedHomeDevices
            if (DdpmCommonHelper.ModuleOwner != null)
            {
                SelectedHomeDevice = DdpmCommonHelper.ModuleOwner.SelectedHomeDevice;
            }

            BackgroundWorker bw = new BackgroundWorker
            {
                WorkerReportsProgress = false,
                WorkerSupportsCancellation = false
            };
            bw.DoWork += DoWork_RefreshData;
            bw.RunWorkerCompleted += RunWorkerCompleted_RefreshData;

            LogInfo("RefresData start...");
            IsBusy = true;
            //_mainInputSource = null;
            bw.RunWorkerAsync();
        }

        private void DoWork_RefreshData(object? sender, DoWorkEventArgs e)
        {
            try //2024-06-19 Elie, add try catch to get exception.
            {
                HomeDevice selHomeDevice = DdpmCommonHelper.ModuleOwner.SelectedHomeDevice;
                if (selHomeDevice == null)
                {
                    e.Result = "SelectedHomeDevice is null";
                    return;
                }

                MonitorInfo mi = selHomeDevice.MonitorInfo;
                LogInfo($"Monitor.AliasDeviceName={mi.AliasDeviceName}");

                //Get the Pxp Capabilities
                //
                //Robert_Lin, 2024-8-8 get thePIP/PBP capabilities from MonitorInfo.CapabilityString
                // instead of Subagent.User PipPbpManager (both are the same method/source)
                //OLD code:
                //Retry max 3 times
                //int maxRetries = 3;
                //LogInfo("Query PipPbpCapabilities...");
                //for (int i = 0; i < maxRetries; i++)
                //{
                //    _pipPbpCaps = DdpmCommonHelper.DeviceManagerSA.GetPipPbpCapabilitiesWords(mi).Result;
                //    LogInfo($"=> PipPbpCapabilities.Count={_pipPbpCaps.Length}");
                //    if (_pipPbpCaps.Length > 0)
                //        break;
                //    Thread.Sleep(200);
                //}
                //NEW code:
                if (!GetPipPbpCapsFromCapabilityString(mi.CapabilityString))
                {
                    LogInfo("Get PIP/PBP Capability from MonitorInfo.CapabilityString failed.");
                }

                //Get current monitor's Pxp mode
                LogInfo("@ Query CurrentPxpMode...");
                ObjGetVCP ret = DdpmCommonHelper.DeviceManagerSA.GetPxpMode(mi).Result;
                if (ret.result)
                {
                    //UInt64 u64 = (UInt64)ret.result;
                    _curPxpMode = Convert.ToUInt16(ret.value);
                    LogInfo($"  => CurrentPxpMode=0x{_curPxpMode:X02}");
                }
                else
                {
                    Log?.Info("  => CurrentPxpMode error");
                }

                //Build  VideoSwapItems
                //Robert_Lin, 2024-10-31, To fix PIMS, the comboBox display itemtext is not always InputSource name.
                //For example: If user input a custom name in InputSource Module for HDMI as "To Sony TV"
                //then the comboBox should display "HDMI - To Sony TV".
                //If user never input custom name, then is should display InputSource name, that is "HDMI"
                //
                //NEW Code:
                Worker_RefreshInputSourceList(sender, e);

                /*
                LogInfo("@ Build VideoSwapItems...");
                Dictionary<string, InputInfo> inputList = DdpmCommonHelper.DeviceManagerSA.GetInputSourcelist(mi).Result;
                Log?.Info($"  => InputList.Count={inputList.Count}");
                //Where the inputList example:
                // Key    InputInfo:InputName, USBUpstream, Code
                // HDMI   "To Sony TV", "USB1", 0xxy

                //In below code section, we will build a List<VideoSwapComboBoxInputSourceItem> as the ItemSource of ComboBoxes
                //
                List<VideoSwapComboBoxInputSourceItem> videoSwapList = new List<VideoSwapComboBoxInputSourceItem>();
                if (inputList != null)
                {
                    foreach (var inputItem in inputList)
                    {
                        VideoSwapComboBoxInputSourceItem cbItem = new VideoSwapComboBoxInputSourceItem()
                        {
                            InputSourceKey = inputItem.Key,
                            InputName = inputItem.Value.InputName,
                            USBUpstream = inputItem.Value.USBUpstream,
                            Code = inputItem.Value.Code
                        };
                        videoSwapList.Add(cbItem);
                    }
                }
                //Assign to ViewModel.VideoSwapItems
                VideoSwapItems = videoSwapList;
                */
                //OLD Code
                //LogInfo("@ Query InputSourceList...");
                //Dictionary<string, InputInfo> inputList = null;
                //inputList = DdpmCommonHelper.DeviceManagerSA.GetInputSourcelist(mi).Result;
                //Log?.Info($"  => InputSourceList.Count={inputList.Count}");

                ////Convert to the type of my ViewModel
                //List<InputSourceObj> inputSourceList = new List<InputSourceObj>();
                //string str1 = "";
                //if (inputList != null)
                //{
                //    foreach (var item in inputList)
                //    {
                //        //Robert_Lin, 2024-9-11, Fix for InputInfo class has added VCP Code by Jason.
                //        //InputSourceObj inputObj = new InputSourceObj(item.Value.InputName);
                //        InputSourceObj inputObj = new InputSourceObj((UInt16)item.Value.Code, item.Value.InputName);
                //        inputSourceList.Add(inputObj);
                //        str1 += item.Value.InputName;
                //        str1 += ", ";
                //    }
                //}
                //InputSourceList = inputSourceList;
                //str1 = str1.TrimEnd(' ');
                //str1 = str1.TrimEnd(',');
                //LogInfo($"  InputSourceList={str1}");

                //Debug
                //if ((InputSourceList == null) || (InputSourceList.Count <= 0))
                //    InputSourceList = GetFakeInputSourceList();

                /* Move to Worker_RefreshInputSourceList()
                 * 
                //Get current Main InputSource from MonitorInfo
                //
                string currentInput = mi.inputSource;
                LogInfo($"  MonitorInfo.InputSource={currentInput}");

                //Robert_Lin, 2024-10-31, Change to VideoSwapComboBoxInputSourceItem type
                if (VideoSwapItems != null)
                {
                    _mainInputSource = VideoSwapItems.Find(x => x.InputSourceKey.Equals(currentInput, StringComparison.OrdinalIgnoreCase));
                    OnPropertyChanged("MainInputSource");
                }
                if (_mainInputSource == null)
                {
                    LogInfo("  Set MainInputSource=null}");
                }
                else
                {
                    LogInfo($"  Set MainInputSource={_mainInputSource.DisplayName}");
                }

                //Get Sub inputs
                Log?.Info("@ Query SubInputSources...");

                //Get current monitor SubInputs input source
                //List<UInt16> subInputs = DdpmCommonHelper.DeviceManagerSA.GetSubInputList(mi).Result;
                List<InputSourceObj> subInputs = DdpmCommonHelper.DeviceManagerSA.GetSubInputs(mi).Result;

                if (subInputs != null)
                {
                    Log?.Info($"  * SubInputSources.Count={subInputs.Count}");
                    SubInputs = subInputs;

                    //Build SubInputs
                    if ((subInputs.Count > 0) && (VideoSwapItems != null))
                    {
                        Sub1InputSource = VideoSwapItems.Find(x => x.Code == (uint)subInputs[0].Code);
                        //Sub1InputSource = InputSourceList.Find(x => x.Code == subInputs[0].Code);
                        //_sub1InputSource = InputSourceList.Find(x => x.Code == subInputs[0].Code);
                        OnPropertyChanged("Sub1InputSource");
                        if (_sub1InputSource == null)
                        {
                            LogInfo("  Set Sub1InputSource=null}");
                        }
                        else
                        {
                            LogInfo($"  Set Sub1InputSource={_sub1InputSource.DisplayName}");
                        }

                        if (subInputs.Count > 1)
                        {
                            //Sub2InputSource = InputSourceList.Find(x => x.Code == subInputs[1].Code);
                            _sub2InputSource = VideoSwapItems.Find(x => x.Code == subInputs[1].Code);
                            OnPropertyChanged("Sub2InputSource");
                            if (_sub2InputSource == null)
                            {
                                LogInfo("  Set Sub2InputSource=null}");
                            }
                            else
                            {
                                LogInfo($"  Set Sub2InputSource={_sub2InputSource.DisplayName}");
                            }
                        }

                        if (subInputs.Count > 2)
                        {
                            //Sub3InputSource = InputSourceList.Find(x => x.Code == subInputs[2].Code);
                            _sub3InputSource = VideoSwapItems.Find(x => x.Code == subInputs[2].Code);
                            OnPropertyChanged("Sub3InputSource");
                            if (_sub3InputSource == null)
                            {
                                LogInfo("  Set Sub3InputSource=null}");
                            }
                            else
                            {
                                LogInfo($"  Set Sub3InputSource={_sub3InputSource.DisplayName}");
                            }
                        }
                    }
                }
                else
                {
                    //Not support SubInput or fail to query
                    LogInfo("=> SubInputSources error");
                }
                */
                e.Result = "OK";
            }
            catch (Exception)
            {
                ;
            }
        }

        private void RunWorkerCompleted_RefreshData(object sender, RunWorkerCompletedEventArgs e)
        {
            IsBusy = false;

            OnPropertyChanged_AllCapabilities();

            //If BackgroundWorker. WorkerSupportsCancellation is true, and you set e.Cancel=true in DoWorker
            if (e.Cancelled)
            {
                Log?.Info("** RefreshData is cancelled.");
                return;
            }
            if (e.Error != null)
            {
                //The message is e.Error.Message
                Log?.Info($"** RefreshData stopped by an exception: {e.Error.Message}");
                return;
            }
            //
            if (e.Result == null)
            {
                //In case that you never set value to e-Result
                Log?.Info("** RefreshData abnormal stopped unknown reason.");
            }
            else
            {
                Log?.Info($"** RefreshData result: {e.Result}");

                if (e.Result == "OK")
                {
                    RefreshPbpSplitListView();
                    //Result is passed.
                    UI_SetSelectedSplitItemByCurPxpMode();
                }
                else
                {
                    //Result is failed.
                }
            }
        }

        public void Invoke_RefreshInputSourceList()
        {
            BackgroundWorker bw = new BackgroundWorker
            {
                WorkerReportsProgress = false,
                WorkerSupportsCancellation = false
            };
            bw.DoWork += Worker_RefreshInputSourceList;
            bw.RunWorkerCompleted += delegate
            {
                IsBusy = false;
            };
            IsBusy = true;
            bw.RunWorkerAsync();
        }
        /// <summary>
        /// Called wheh OnActivte(), refresh for InputSource, SubInputs changed
        /// </summary>
        private void Worker_RefreshInputSourceList(object? sender, DoWorkEventArgs e)
        {
            if (SelectedHomeDevice == null)
            {
                e.Result = "SelectedHomeDevice is null";
                return;
            }
            if (SelectedHomeDevice.MonitorInfo == null)
            {
                e.Result = "SelectedHomeDevice.MonitorInfo is null";
                return;
            }

            LogInfo("@ Refresh InputSourceList...");
            Stopwatch sw = Stopwatch.StartNew();
            sw.Start();
            Dictionary<string, InputInfo> inputList = DdpmCommonHelper.DeviceManagerSA.GetInputSourcelist(SelectedHomeDevice.MonitorInfo).Result;
            Log?.Info($"  * InputList.Count={inputList.Count}");
            //Where the inputList example:
            // Key    InputInfo:InputName, USBUpstream, Code
            // HDMI   "To Sony TV", "USB1", 0xxy

            //In below code section, we will build a List<VideoSwapComboBoxInputSourceItem> as the ItemSource of ComboBoxes
            //
            List<VideoSwapComboBoxInputSourceItem> videoSwapList = new List<VideoSwapComboBoxInputSourceItem>();
            int idxSource = 0;
            if (inputList != null)
            {
                foreach (var inputItem in inputList)
                {
                    VideoSwapComboBoxInputSourceItem cbItem = new VideoSwapComboBoxInputSourceItem()
                    {
                        InputSourceKey = inputItem.Key,
                        InputName = inputItem.Value.InputName,
                        USBUpstream = inputItem.Value.USBUpstream,
                        Code = inputItem.Value.Code
                    };
                    videoSwapList.Add(cbItem);
                    LogInfo($"  [{idxSource}] {cbItem.DisplayName}");
                    idxSource++;
                }
            }
            //Assign to ViewModel.VideoSwapItems
            VideoSwapItems = videoSwapList;

            //Get current Main InputSource from MonitorInfo
            //
            string currentInput = SelectedHomeDevice.MonitorInfo.inputSource;
            LogInfo($"  * Main.InputSource={currentInput}");

            //Robert_Lin, 2024-10-31, Change to VideoSwapComboBoxInputSourceItem type
            if (VideoSwapItems != null)
            {
                _mainInputSource = VideoSwapItems.Find(x => x.InputSourceKey.Equals(currentInput, StringComparison.OrdinalIgnoreCase));
                OnPropertyChanged("MainInputSource");
            }
            if (_mainInputSource == null)
            {
                LogInfo("  Set MainInputSource=null}");
            }
            else
            {
                LogInfo($"  Set MainInputSource={_mainInputSource.DisplayName}");
            }

            //Get Sub inputs
            Log?.Info("@ Query SubInputSources...");

            //Get current monitor SubInputs input source
            //List<UInt16> subInputs = DdpmCommonHelper.DeviceManagerSA.GetSubInputList(mi).Result;
            List<InputSourceObj> subInputs = DdpmCommonHelper.DeviceManagerSA.GetSubInputs(SelectedHomeDevice.MonitorInfo).Result;

            if (subInputs != null)
            {
                Log?.Info($"  * SubInputSources.Count={subInputs.Count}");
                SubInputs = subInputs;

                //Build SubInputs
                if ((subInputs.Count > 0) && (VideoSwapItems != null))
                {
                    Sub1InputSource = VideoSwapItems.Find(x => x.Code == (uint)subInputs[0].Code);
                    //Sub1InputSource = InputSourceList.Find(x => x.Code == subInputs[0].Code);
                    //_sub1InputSource = InputSourceList.Find(x => x.Code == subInputs[0].Code);
                    OnPropertyChanged("Sub1InputSource");
                    if (_sub1InputSource == null)
                    {
                        LogInfo("  Set Sub1InputSource=null}");
                    }
                    else
                    {
                        LogInfo($"  Set Sub1InputSource={_sub1InputSource.DisplayName}");
                    }

                    if (subInputs.Count > 1)
                    {
                        //Sub2InputSource = InputSourceList.Find(x => x.Code == subInputs[1].Code);
                        _sub2InputSource = VideoSwapItems.Find(x => x.Code == subInputs[1].Code);
                        OnPropertyChanged("Sub2InputSource");
                        if (_sub2InputSource == null)
                        {
                            LogInfo("  Set Sub2InputSource=null}");
                        }
                        else
                        {
                            LogInfo($"  Set Sub2InputSource={_sub2InputSource.DisplayName}");
                        }
                    }

                    if (subInputs.Count > 2)
                    {
                        //Sub3InputSource = InputSourceList.Find(x => x.Code == subInputs[2].Code);
                        _sub3InputSource = VideoSwapItems.Find(x => x.Code == subInputs[2].Code);
                        OnPropertyChanged("Sub3InputSource");
                        if (_sub3InputSource == null)
                        {
                            LogInfo("  Set Sub3InputSource=null}");
                        }
                        else
                        {
                            LogInfo($"  Set Sub3InputSource={_sub3InputSource.DisplayName}");
                        }
                    }
                }
            }
            else
            {
                //Not support SubInput or fail to query
                LogInfo("=> SubInputSources error");
            }
            sw.Stop();
            LogInfo($"  * RefreshInputSourceList done, elapsed {sw.ElapsedMilliseconds} msec.");
            e.Result = "OK";
        }
        #endregion RefreshData - Init and SelectedHomeDevice changed

        #region FullView Demo -- Unused, to be removed
#if ProVideFullViewDemo

        private string _test1Value = "AAA";

        public string Test1Value
        {
            get { return _test1Value; }
            set => SetProperty(ref _test1Value, value);
        }

        private string _test2Value = "BBB";

        public string Test2Value
        {
            get { return _test2Value; }
            set => SetProperty(ref _test2Value, value);
        }

        private ICommand? _closeFullViewCommand;
        private ICommand? _gotoNextCommand;
        private ICommand? _gotoPrevCommand;

        public ICommand? CloseFullViewCommand
        {
            get => _closeFullViewCommand;
            set => SetProperty(ref _closeFullViewCommand, value);
        }

        public ICommand? GotoNextCommand
        {
            get => _gotoNextCommand;
            set => SetProperty(ref _gotoNextCommand, value);
        }

        public ICommand? GotoPrevCommand
        {
            get => _gotoPrevCommand;
            set => SetProperty(ref _gotoPrevCommand, value);
        }
#endif //ProVideFullViewDemo
        #endregion FullView Demo

        #region ClickCommand of SplitItems

        /// <summary>
        /// The Command when 'Fullscreen' (PIP off) is clicked
        /// </summary>
        private ICommand? _fullScreenClickCommand;

        public ICommand? FullScreenClickCommand
        {
            get => _fullScreenClickCommand;
            set => SetProperty(ref _fullScreenClickCommand, value);
        }

        /// <summary>
        /// The Command when 'PIP small' is clicked
        /// </summary>
        private ICommand? _pipSmallClickCommand;

        public ICommand? PipSmallClickCommand
        {
            get => _pipSmallClickCommand;
            set => SetProperty(ref _pipSmallClickCommand, value);
        }

        /// <summary>
        /// The Command when 'PIP large' is clicked
        /// </summary>
        private ICommand? _pipLargeClickCommand;

        public ICommand? PipLargeClickCommand
        {
            get => _pipLargeClickCommand;
            set => SetProperty(ref _pipLargeClickCommand, value);
        }

        /// <summary>
        /// The Command when a PBP SplitItem is clicked
        ///
        /// </summary>
        private ICommand? _pbpItemClickCommand;

        public ICommand? PbpItemClickCommand
        {
            get => _pbpItemClickCommand;
            set => SetProperty(ref _pbpItemClickCommand, value);
        }

        #endregion ClickCommand of SplitItems

        #region ClickCommand default Handlers

        //To be set to private when issue fix
        //
        public void OnFullScreenClicked(SplitItem spItem)
        {
            LogInfo("@ SetPxpMode=Off");
            //Update selectedSplitItem
            SelectedSplitItem = spItem;
            bool blRes =
            DdpmCommonHelper.DeviceManagerSA.SetPipModeOff(DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo).Result;

            LogInfo($"  => SetPxpMode=Off, Result={blRes}");
        }

        public void OnPipSmallClicked(SplitItem spItem)
        {
            LogInfo($"@ OnPipSmallClicked");
            SelectedSplitItem = spItem;

            BackgroundWorker bw = new BackgroundWorker()
            {
                WorkerReportsProgress = false,
                WorkerSupportsCancellation = false
            };
            bw.DoWork += delegate
            {
                LogInfo("  * SetPxpMode=Pip-Small");
                bool blRes = DdpmCommonHelper.DeviceManagerSA.SetPipModeSmall(DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo).Result;
                LogInfo($"  => SetPxpMode=Pip-Small, Result={blRes}");
            };
            bw.RunWorkerCompleted += delegate
            {
                IsBusy = false;
            };
            IsBusy = true;
            bw.RunWorkerAsync();
            //    IShowPluginManager? _showPluginManager = DDPM.UI.Plugin.DisplayPlugin.PluginIoc.GetService<IShowPluginManager>();
            //    _showPluginManager?.ShowPluginById(DDPM.UI.Common.Constants.SettingsPluginId, "1");
            DdpmCommonHelper.MyShowPluginManager?.ShowHomePage("GeHomeFirst");
        }

        public void OnPipLargeClicked(SplitItem spItem)
        {
            SelectedSplitItem = spItem;

            BackgroundWorker bw = new BackgroundWorker()
            {
                WorkerReportsProgress = false,
                WorkerSupportsCancellation = false
            };
            bw.DoWork += delegate
            {
                LogInfo("@ SetPxpMode=Pip-Large");
                bool blRes = DdpmCommonHelper.DeviceManagerSA.SetPipModeLarge(DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo).Result;
                LogInfo($"  => SetPxpMode=Pip-Large, Result={blRes}");
            };
            bw.RunWorkerCompleted += delegate
            {
                IsBusy = false;
            };
            IsBusy = true;
            bw.RunWorkerAsync();
            DdpmCommonHelper.MyShowPluginManager?.ShowHomePage("GeHomeFirst");
        }

        public void OnPbpItemClicked(SplitItem spItem)
        {
            SelectedSplitItem = spItem;

            BackgroundWorker bw = new BackgroundWorker()
            {
                WorkerReportsProgress = false,
                WorkerSupportsCancellation = false
            };
            bw.DoWork += delegate
            {
                UInt16 capCode = SelectedSplitItem.ISplit.PbpCapabilityCode;
                LogInfo($"@ SetPxpMode=0x{capCode:X}, {SelectedSplitItem.ISplit.Description}");
                bool blRes = DdpmCommonHelper.DeviceManagerSA.SetPbpMode(DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo, capCode).Result;
                LogInfo($"  => Result={blRes}");
            };
            bw.RunWorkerCompleted += delegate
            {
                IsBusy = false;
            };
            IsBusy = true;
            bw.RunWorkerAsync();
            DdpmCommonHelper.MyShowPluginManager?.ShowHomePage("GeHomeFirst");
        }

        #endregion ClickCommand default Handlers

        #region Pxp VCP Code

        public const UInt16 PipMode_Off = 0;
        public const UInt16 PipMode_Small = 0x21;
        public const UInt16 PipMode_Large = 0x22;

        public const UInt16 PipMode_SizeToggle = 0x01;
        public const UInt16 PipMode_PositionToggle = 0x02;

        /// <summary>
        /// Return the split count by the specified PxpMode
        /// </summary>
        /// <param name="pxpMode"></param>
        /// <returns></returns>
        private int SplitCountFromPxpMode(UInt16 pxpMode)
        {
            switch (pxpMode)
            {
                case PipMode_Off:
                    return 1;
                case PipMode_Small:
                case PipMode_Large:
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
                    return 2;
                case 0x31:
                case 0x32:
                case 0x33:
                case 0x34:
                case 0x35:
                    return 3;
                case 0x41:
                case 0x42:
                    return 4;
                default:
                    break;
            }
            return 0;
        }

        #endregion Pxp VCP Code

        #region PIP/PBP Capabilities

        private UInt16[] _pipPbpCaps = new UInt16[] { 0 };//SDL, change to use new

        public bool HasPxpCap(UInt16 cap)
        {
            if (_pipPbpCaps.Length <= 0)
                return false;
            return Array.Exists(_pipPbpCaps, x => x == cap);
        }

        //Robert_Lin, 2024-8-8, to get PIP/PBP capabilities from MonitorInfo.CapabilityString directly
        /// <summary>
        /// Parsing and get the PIP/PBP capabilities from MonitorInfo.CapabilityString
        /// And extract PIP/PBP capabilities "E9(xx,xx,xx...)", parsing the xx,xx,xx,...
        /// And output to _pipPbpCaps[]
        /// </summary>
        /// <param name="capabilityString"></param>
        private bool GetPipPbpCapsFromCapabilityString(string capabilityString)
        {
            LogInfo("@ PibPbpViewModel.GetPipPbpCapsFromCapabilityString()");
            LogInfo($"  * capabilityString: {capabilityString}");

            //Find the start index of "E9("
            string signature = "E9(";
            int idxSignature = capabilityString.IndexOf(signature);
            if (idxSignature < 0) //Not found, no update to _pipPbpCaps[]
            {
                LogInfo($"  => Failed, cannot find signature \"{signature}\"");
                return false;
            }
            LogInfo($"  * Found signature \"{signature}\" at [{idxSignature}]");
            int idxPipPbpCapsStart = idxSignature + signature.Length;
            //Find the index of next ')' char
            int idxEnd = capabilityString.IndexOf(')', idxPipPbpCapsStart);
            if (idxEnd < 0)
            {
                LogInfo($"  => Failed, cannot find END of signature \')\'");
                return false;
            }
            int pipPbpCapsLen = idxEnd - idxPipPbpCapsStart;
            //Extract the sub string contains PIP/PBP capabilities
            string pxpCapString = capabilityString.Substring(idxPipPbpCapsStart, pipPbpCapsLen);
            LogInfo($"  * PxpCapabilitise={pxpCapString}");
            _pipPbpCaps = DdpmCommonHelper.ParsingHexStringToWords(pxpCapString);

            OnPropertyChanged_AllCapabilities();
            return true;
        }

        public void OnPropertyChanged_AllCapabilities()
        {
            OnPropertyChanged("HasCapability_PipSmall");
            OnPropertyChanged("HasCapability_PipLarge");
            OnPropertyChanged("HasCapability_PipTogglePosition");
            OnPropertyChanged("HasCapabiliy_AnyPip");
        }
        public bool HasCapability_PipSmall
        {
            get { return HasPxpCap(PipMode_Small); }
            //get { return false; }
        }
        public bool HasCapability_PipLarge
        {
            get { return HasPxpCap(PipMode_Large); }
            //get { return false; }
        }
        public bool HasCapability_PipTogglePosition
        {
            get { return HasPxpCap(PipMode_PositionToggle); }
            //get { return false; }
        }
        public bool HasCapabiliy_AnyPip
        {
            get
            {
                if (HasCapability_PipSmall)
                    return true;
                if (HasCapability_PipLarge)
                    return true;
                if (HasCapability_PipTogglePosition)
                    return true;
                return false;
            }
        }

        #endregion PIP/PBP Capabilities

        #region Current PxpMode

        private UInt16 _curPxpMode = 0;
        public UInt16 CurPxpMode
        {
            get => _curPxpMode;
            set
            {
                SetProperty(ref _curPxpMode, value);
                OnPropertyChanged("SplitCount");
            }
        }

        //Call only from UI thread
        private void UI_SetSelectedSplitItemByCurPxpMode()
        {
            if ((SplitItem_Off == null) || (SplitItem_PipSmall == null) || (SplitItem_PipLarge == null))
                return;

            if (_curPxpMode == PipMode_Off)
                SelectedSplitItem = SplitItem_Off;
            else if (_curPxpMode == PipMode_Small)
                SelectedSplitItem = SplitItem_PipSmall;
            else if (_curPxpMode == PipMode_Large)
                SelectedSplitItem = SplitItem_PipLarge;
            else
            {
                //Current is PBP mode, will be selected from View
            }
        }

        public bool IsFullscreenItemSelected
        {
            get
            {
                return (SelectedSplitItem == SplitItem_Off);
            }
        }

        //The count of split windows for CurPxpMode
        public int SplitCount
        {
            get
            {
                return SplitCountFromPxpMode(CurPxpMode);
            }
        }
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
                    OnPropertyChanged("IsPipItemSelected");

                    if (_selectedSplitItem.ISplit != null)
                    {
                        ISplit isp = _selectedSplitItem.ISplit;
                        CurPxpMode = isp.PbpCapabilityCode;
                    }
                }
                else
                {
                    //Selection none
                    SetProperty(ref _selectedSplitItem, value);
                    OnPropertyChanged("IsPipItemSelected");
                }

                OnPropertyChanged("IsFullscreenItemSelected");
                OnPropertyChanged("IsVideoSwapComboBoxesVisible");
                OnPropertyChanged("IsVideoSwapButtonEnabled");
                OnPropertyChanged("IsVideoSwapButtonVisible");
                OnPropertyChanged("IsUsbSwitchButtonVisible");
                OnPropertyChanged("IsUsbSwitchButtonEnabled");
            }
        }

        //private ICommand? _spItemClickCommand;
        //public ICommand? SplitItemClickCommand
        //{
        //    get => _spItemClickCommand;
        //    set => SetProperty(ref _spItemClickCommand, value);
        //}

        #endregion Selected SplitItems

        #region PIP and Toggle Position

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
                DdpmCommonHelper.DeviceManagerSA.TogglePipPosition(DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo);
            };
            bw.RunWorkerCompleted += delegate
            {
                IsBusy = false;
            };
            IsBusy = true;
            bw.RunWorkerAsync();
        }

        public bool IsPipItemSelected
        {
            get
            {
                if (SelectedSplitItem == null)
                    return false;
                if (SelectedSplitItem.SplitOwner == eSplitOwner.PipList)
                    return true;
                return false;
            }
        }

        #endregion PIP and Toggle Position

        #region InputSourceList

        //Robert_Lin, 2024-10-31 change type to VideoSwapComboBoxInputSourceItem
        //
        private List<VideoSwapComboBoxInputSourceItem> _videoSwapItems = new List<VideoSwapComboBoxInputSourceItem>();

        public List<VideoSwapComboBoxInputSourceItem> VideoSwapItems
        {
            get => _videoSwapItems;
            set
            {
                SetProperty(ref _videoSwapItems, value);
                OnPropertyChanged("VideoSwapItemCount");
            }
        }

        public int VideoSwapItemCount => _videoSwapItems.Count;

        //Robert_Lin, 2024-10-31, Unused, to be removed
        private List<InputSourceObj> _inputSourceList = new List<InputSourceObj>();

        //Robert_Lin, 2024-10-31, Unused, to be removed
        public List<InputSourceObj> InputSourceList
        {
            get => _inputSourceList;
            set
            {
                SetProperty(ref _inputSourceList, value);
                OnPropertyChanged("InputSourceCount");
                //OnPropertyChanged("IsVideoSwapComboBoxesVisible");
                //OnPropertyChanged("IsVideoSwapButtonEnabled");
                //OnPropertyChanged("IsVideoSwapButtonVisible");
                //OnPropertyChanged("IsUsbSwitchButtonVisible");
                //OnPropertyChanged("IsUsbSwitchButtonEnabled");
            }
        }


        //Robert_Lin, 2024-10-31, Unused, to be removed
        //Debug purpose
        private List<InputInfo> GetFakeInputSourceList()
        {
            List<InputInfo> fakeList = new List<InputInfo>();
            fakeList.Add(new InputInfo() { InputName = "HDMI-1" });
            fakeList.Add(new InputInfo() { InputName = "HDMI-2" });
            fakeList.Add(new InputInfo() { InputName = "USB-C1" });
            return fakeList;
        }

        //Robert_Lin, 2024-10-31, Unused, to be removed
        private void OnMainInputSourceSelectionChanged()
        {
        }

        //Robert_Lin, 2024-10-31, Unused, to be removed
        private int InputSourceCount
        {
            get
            {
                return _inputSourceList.Count;
            }
        }

        #endregion InputSourceList

        #region Main Input Source

        //Robert_Lin,2024-10-31, change type to VideoSwapComboBoxInputSourceItem
        //NEW Code:
        private VideoSwapComboBoxInputSourceItem _mainInputSource;
        public VideoSwapComboBoxInputSourceItem MainInputSource
        {
            get => _mainInputSource;
            set
            {
                bool isNeedUpdateToDevice =
                //It's  NOT the first time set value (we assume it's assigned from RefreshData())
                (_mainInputSource != null) &&
                //AND value is changed
                (_mainInputSource != value);

                SetProperty(ref _mainInputSource, value);
                if (isNeedUpdateToDevice)
                {
                    BackgroundWorker bw = new BackgroundWorker()
                    {
                        WorkerReportsProgress = false,
                        WorkerSupportsCancellation = false
                    };
                    bw.DoWork += delegate
                    {
                        if (_mainInputSource != null)
                        {
                            bool res = DdpmCommonHelper.DeviceManagerSA.SetVCPCapability(
                                 DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo,
                                 "Input Select", _mainInputSource.InputSourceKey).Result;
                            if (res)
                            {
                                DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo.inputSource =
                                _mainInputSource.InputSourceKey;
                                DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.UpdateBatteryIndicator();
                            }
                        }
                    };
                    bw.RunWorkerCompleted += delegate
                    {
                        IsBusy = false;
                    };
                    IsBusy = true;
                    bw.RunWorkerAsync();
                }
            }
        }

        //OLD Code:
        //private InputSourceObj _mainInputSource;
        //public InputSourceObj MainInputSource
        //{
        //    get => _mainInputSource;
        //    set
        //    {
        //        bool isNeedUpdateToDevice =
        //        //It's  NOT the first time set value (we assume it's assigned from RefreshData())
        //        (_mainInputSource != null) &&
        //        //AND value is changed
        //        (_mainInputSource != value);

        //        SetProperty(ref _mainInputSource, value);
        //        if (isNeedUpdateToDevice)
        //        {
        //            BackgroundWorker bw = new BackgroundWorker()
        //            {
        //                WorkerReportsProgress = false,
        //                WorkerSupportsCancellation = false
        //            };
        //            bw.DoWork += delegate
        //            {
        //                if (_mainInputSource != null)
        //                {
        //                    bool res = DdpmCommonHelper.DeviceManagerSA.SetVCPCapability(
        //                         DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo,
        //                         "Input Select", _mainInputSource.Name).Result;
        //                    if (res)
        //                    {
        //                        DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo.inputSource =
        //                        _mainInputSource.Name;
        //                        DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.UpdateBatteryIndicator();
        //                    }
        //                }
        //            };
        //            bw.RunWorkerCompleted += delegate
        //            {
        //                IsBusy = false;
        //            };
        //            IsBusy = true;
        //            bw.RunWorkerAsync();
        //        }
        //    }
        //}

        #endregion Main Input Source

        #region Sub Input Sources
        //Robert_Lin,2024-10-31, change type to VideoSwapComboBoxInputSourceItem
        //NEW Code:
        private VideoSwapComboBoxInputSourceItem? _sub1InputSource = null;

        public VideoSwapComboBoxInputSourceItem? Sub1InputSource
        {
            get => _sub1InputSource;
            set
            {
                bool isNeedUpdateToDevice =
                //It's  NOT the first time set value (we assume it's assigned from RefreshData())
                (_sub1InputSource != null) &&
                //AND value is changed
                (_sub1InputSource != value);

                SetProperty(ref _sub1InputSource, value);
                if (isNeedUpdateToDevice && (_sub1InputSource != null))
                {
                    BackgroundWorker bw = new BackgroundWorker()
                    {
                        WorkerReportsProgress = false,
                        WorkerSupportsCancellation = false
                    };
                    bw.DoWork += delegate
                    {
                        bool res = DdpmCommonHelper.DeviceManagerSA.SetSubInputs(
                            DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo,
                            _sub1InputSource.ConvertToInputSourceObj(), null, null).Result;
                    };
                    bw.RunWorkerCompleted += delegate
                    {
                        IsBusy = false;
                    };
                    IsBusy = true;
                    bw.RunWorkerAsync();
                }
            }
        }
        private VideoSwapComboBoxInputSourceItem? _sub2InputSource;
        public VideoSwapComboBoxInputSourceItem? Sub2InputSource
        {
            get => _sub2InputSource;
            set
            {
                bool isNeedUpdateToDevice =
                //It's  NOT the first time set value (we assume it's assigned from RefreshData())
                (_sub2InputSource != null) &&
                //AND value is changed
                (_sub2InputSource != value);

                SetProperty(ref _sub2InputSource, value);
                if (isNeedUpdateToDevice && (_sub2InputSource != null))
                {
                    BackgroundWorker bw = new BackgroundWorker()
                    {
                        WorkerReportsProgress = false,
                        WorkerSupportsCancellation = false
                    };
                    bw.DoWork += delegate
                    {
                        bool res = DdpmCommonHelper.DeviceManagerSA.SetSubInputs(
                            DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo,
                            null, Sub2InputSource.ConvertToInputSourceObj(), null).Result;
                    };
                    bw.RunWorkerCompleted += delegate
                    {
                        IsBusy = false;
                    };
                    IsBusy = true;
                    bw.RunWorkerAsync();
                }
            }
        }

        private VideoSwapComboBoxInputSourceItem? _sub3InputSource;
        public VideoSwapComboBoxInputSourceItem? Sub3InputSource
        {
            get => _sub3InputSource;
            set
            {
                bool isNeedUpdateToDevice =
                //It's  NOT the first time set value (we assume it's assigned from RefreshData())
                (_sub3InputSource != null) &&
                //AND value is changed
                (_sub3InputSource != value);

                SetProperty(ref _sub3InputSource, value);
                if (isNeedUpdateToDevice && (_sub3InputSource!= null))
                {
                    BackgroundWorker bw = new BackgroundWorker()
                    {
                        WorkerReportsProgress = false,
                        WorkerSupportsCancellation = false
                    };
                    bw.DoWork += delegate
                    {
                        bool res = DdpmCommonHelper.DeviceManagerSA.SetSubInputs(
                            DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo,
                            null, null, Sub3InputSource.ConvertToInputSourceObj()).Result;
                    };
                    bw.RunWorkerCompleted += delegate
                    {
                        IsBusy = false;
                    };
                    IsBusy = true;
                    bw.RunWorkerAsync();
                }
            }
        }


        //OLD Code:
        //private InputSourceObj? _sub1InputSource;

        //public InputSourceObj? Sub1InputSource
        //{
        //    get => _sub1InputSource;
        //    set
        //    {
        //        bool isNeedUpdateToDevice =
        //        //It's  NOT the first time set value (we assume it's assigned from RefreshData())
        //        (_sub1InputSource != null) &&
        //        //AND value is changed
        //        (_sub1InputSource != value);

        //        SetProperty(ref _sub1InputSource, value);
        //        if (isNeedUpdateToDevice)
        //        {
        //            BackgroundWorker bw = new BackgroundWorker()
        //            {
        //                WorkerReportsProgress = false,
        //                WorkerSupportsCancellation = false
        //            };
        //            bw.DoWork += delegate
        //            {
        //                bool res = DdpmCommonHelper.DeviceManagerSA.SetSubInputs(
        //                    DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo,
        //                    Sub1InputSource, null, null).Result;
        //            };
        //            bw.RunWorkerCompleted += delegate
        //            {
        //                IsBusy = false;
        //            };
        //            IsBusy = true;
        //            bw.RunWorkerAsync();
        //        }
        //    }
        //}

        //private InputSourceObj? _sub2InputSource;

        //public InputSourceObj? Sub2InputSource
        //{
        //    get => _sub2InputSource;
        //    set
        //    {
        //        bool isNeedUpdateToDevice =
        //        //It's  NOT the first time set value (we assume it's assigned from RefreshData())
        //        (_sub2InputSource != null) &&
        //        //AND value is changed
        //        (_sub2InputSource != value);

        //        SetProperty(ref _sub2InputSource, value);
        //        if (isNeedUpdateToDevice)
        //        {
        //            BackgroundWorker bw = new BackgroundWorker()
        //            {
        //                WorkerReportsProgress = false,
        //                WorkerSupportsCancellation = false
        //            };
        //            bw.DoWork += delegate
        //            {
        //                bool res = DdpmCommonHelper.DeviceManagerSA.SetSubInputs(
        //                    DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo,
        //                    null, Sub2InputSource, null).Result;
        //            };
        //            bw.RunWorkerCompleted += delegate
        //            {
        //                IsBusy = false;
        //            };
        //            IsBusy = true;
        //            bw.RunWorkerAsync();
        //        }
        //    }
        //}

        //private InputSourceObj? _sub3InputSource;

        //public InputSourceObj? Sub3InputSource
        //{
        //    get => _sub3InputSource;
        //    set
        //    {
        //        bool isNeedUpdateToDevice =
        //        //It's  NOT the first time set value (we assume it's assigned from RefreshData())
        //        (_sub3InputSource != null) &&
        //        //AND value is changed
        //        (_sub3InputSource != value);

        //        SetProperty(ref _sub3InputSource, value);
        //        if (isNeedUpdateToDevice)
        //        {
        //            BackgroundWorker bw = new BackgroundWorker()
        //            {
        //                WorkerReportsProgress = false,
        //                WorkerSupportsCancellation = false
        //            };
        //            bw.DoWork += delegate
        //            {
        //                bool res = DdpmCommonHelper.DeviceManagerSA.SetSubInputs(
        //                    DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo,
        //                    null, null, Sub3InputSource).Result;
        //            };
        //            bw.RunWorkerCompleted += delegate
        //            {
        //                IsBusy = false;
        //            };
        //            IsBusy = true;
        //            bw.RunWorkerAsync();
        //        }
        //    }
        //}

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
            }
        }

        public bool HasSub1Input => SubInputs.Count > 0;
        public bool HasSub2Input => SubInputs.Count > 1;
        public bool HasSub3Input => SubInputs.Count > 2;

        #endregion Sub Input Sources

        #region UI Enable Flags

        private bool _isBusy = false;

        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        #endregion UI Enable Flags

        #region Add SplitItems into SplitListView

        //Add SplitItems into SplitListView_Pbp based on PxpMode Capabilities
        //This method must be called only from UI thread
        private void RefreshPbpSplitListView()
        {
            //Must as assigned it, should be in View's ctor
            if (SplitListView_Pbp == null)
                return;

            SplitListView_Pbp.ClearList();
            bool isSelectedItemInList = false;
            foreach (ISplit isp in ISplit.PipClasses)
            {
                //Check if SelectedHomeDevice have the capability
                if (HasPxpCap(isp.PbpCapabilityCode))
                {
                    SplitItem spItem = SplitListView_Pbp.AddSplitToList(isp);
                    if (CurPxpMode == isp.PbpCapabilityCode)
                    {
                        SelectedSplitItem = spItem;
                        isSelectedItemInList = true;

                    }
                }
            }
            if (isSelectedItemInList)
            {
                SplitListView_Pbp.GotoFirstSelectedItemPage();
            }

        }

        #endregion Add SplitItems into SplitListView

        #region Video Swap

        private bool _isVideoSwapButtonEnabled = false;

        public bool IsVideoSwapComboBoxesVisible
        {
            get
            {
                //Robert_Lin, 2024-11-13, allow to set it always true from INI file for debugging
                if (DDPM.UI.Common.User32.IniReadInt("DDPMDebug", "PipPbp.VideoSwapComboBox.AlwaysVisible", 0, @"C:\temp\DDPMDebug.txt") == 1)
                {
                    return true;
                }
                //Robert_Lin, 2024-8-28 update rule:
                //If SplitCount >= 3 then IsVideoSwapComboBoxesVisible is true
                //Return true when: (InputSource.Count >= 3)
                return (SplitCountFromPxpMode(CurPxpMode) >= 3);
            }
        }

        public bool IsVideoSwapButtonVisible
        {
            get
            {
                //Robert_Lin, 2024-8-28 update rule:
                //If SplitCount == 2 then IsVideoSwapButtonVisible is true
                return (SplitCountFromPxpMode(CurPxpMode) == 2);
            }
        }

        public bool IsVideoSwapButtonEnabled
        {
            get
            {
                return (SplitCountFromPxpMode(CurPxpMode) == 2);
            }
        }

        public bool ExecuteVideoSwap()
        {
            if (DeviceManagerSA != null)
            {
                if (SelectedHomeDevice != null)
                {
                    return DeviceManagerSA.VideoSwap(SelectedHomeDevice.MonitorInfo, 0, 1).Result;
                }
            }
            return false;
        }


        #endregion Video Swap

        #region USB Switch

        public bool IsUsbSwitchButtonVisible
        {
            get
            {
                if (SelectedHomeDevice == null) return false;

                if (SelectedHomeDevice.HasCapability_UsbKvm)
                    return true;
                if (SelectedHomeDevice.HasCapability_NetworkKvm)
                {
                    if (IsNetworkKvmOn)
                        return true;
                }
                return false;
            }
        }

        public bool IsUsbSwitchButtonEnabled
        {
            get
            {
                if (IsFullscreenItemSelected)
                    return false;
                if (SelectedHomeDevice == null) return false;
                if (SelectedHomeDevice.HasCapability_UsbKvm)
                {
                    return IsUsbKvmOn;
                }
                return false;
            }
        }

        private bool IsUsbKvmOn
        {
            get
            {
                return _isUsbKvmOn;
            }
        }

        private bool IsNetworkKvmOn
        {
            get
            {
                return _isNetworkKvmOn;
            }
        }

        private bool _isUsbKvmOn = false;
        private bool _isNetworkKvmOn = false;

        private void ReadKvmSettings()
        {
            _isUsbKvmOn = DeviceManagerSA.GetOnUSBKVM(SelectedHomeDevice.MonitorInfo).Result;
            _isNetworkKvmOn = DeviceManagerSA.GetOnNKVM(SelectedHomeDevice.MonitorInfo).Result;
            OnPropertyChanged("IsUsbSwitchButtonVisible");
            OnPropertyChanged("IsUsbSwitchButtonEnabled");
        }

        public bool ExecuteUsbSwitch()
        {
            if (DeviceManagerSA != null)
            {
                if (SelectedHomeDevice != null)
                {
                    return DeviceManagerSA.UsbSwitch1(SelectedHomeDevice.MonitorInfo).Result; ;
                }
            }
            return false;
        }

        #endregion USB Switch

        public void OnActivated()
        {
            ReadKvmSettings();
        }
    }
}