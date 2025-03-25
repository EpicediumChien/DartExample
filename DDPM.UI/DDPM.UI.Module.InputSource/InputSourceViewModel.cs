using CommunityToolkit.Mvvm.ComponentModel;
using DDPM.SA.Common;
using DDPM.SA.Common.Settings;
using DDPM.UI.Common;
using DDPM.UI.Common.Interfaces;
using Dell.Client.Framework.Common;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using VcpCore.Common;

namespace DDPM.UI.Module.InputSource
{
    public class InputSourceList
    {
        public string inputSource = String.Empty;
        public string inputName = String.Empty;
        //public InputSourceModule inputSourceModule { get; set; }

        public string inputDisplayText
        {
            get
            {
                if (inputSource == inputName)
                {
                    return inputSource;
                }
                else
                {
                    return inputSource + " - " + inputName;
                }
            }
        }
    }

    public class InputSourceViewModel : ObservableObject
    {
        private InputSourceList _selectInput = new InputSourceList();
        private List<InputSourceList> _inputsList = new List<InputSourceList>();
        private string? _inputImage;
        private BackgroundWorker? bw = null;

        public readonly ILog? _log = null;
        public Guid? guid { get; set; } = Guid.NewGuid();
        public IModuleOwner? ModuleOwner { get; set; }
        public InputSourceModule InputSourceModule { get; set; }

        //public int ItemsCollection_SelectedIndex { get; set; }
        //public ObservableCollection<string> ItemsCollection { get; set; }
        public ObservableCollection<Item> items { get; set; }

        public Dictionary<string, InputInfo> inputList { get; set; }
        public List<string> usbUpstream { get; set; } = new List<string>();
        public Visibility IsUSBH { get; set; } = Visibility.Collapsed;
        public string NameHWidth { get; set; } = "0.5*";
        public string USBHWidth { get; set; } = "1.5*";
        public string NameHColumn { get; set; } = "1";
        public string InputTitle { get; set; } = Strings.InputTitle1;

        public List<InputSourceList> InputsList
        {
            get => _inputsList;
            set => SetProperty(ref _inputsList, value);
        }

        public InputSourceList Items_Selected
        {
            get => _selectInput;
            set
            {
                SetProperty(ref _selectInput, value);
                bool b = DdpmCommonHelper.DeviceManagerSA.SetVCPCapability(InputSourceModule.SelectedHomeDevice.MonitorInfo, "Input Select", _selectInput.inputSource).Result;
                if (b)
                {
                    //InputSourceModule.SelectedHomeDevice.MonitorInfo.inputSource = _selectInput.inputSource;
                    foreach (var device in ModuleOwner.HomeDevices)
                    {
                        if (device.MonitorInfo.edid.ServiceTag == InputSourceModule.SelectedHomeDevice.MonitorInfo.edid.ServiceTag &&
                            device.MonitorInfo.edid.SerialNumber == InputSourceModule.SelectedHomeDevice.MonitorInfo.edid.SerialNumber)
                        {
                            device.MonitorInfo.inputSource = _selectInput.inputSource;
                        }
                    }
                }
            }
        }

        public string? InputSourceImage
        {
            get => _inputImage;
            set => SetProperty(ref _inputImage, value);
        }

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
            bw = new BackgroundWorker()
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };
            guid = Guid.NewGuid();
            bw.DoWork += DoWork_RefreshData;
            bw.RunWorkerCompleted += RunWorkerCompleted_RefreshData;
            bw.RunWorkerAsync(); //myArg is the optional argument
            IsBusy = true;
        }

        private void DoWork_RefreshData(object sender, DoWorkEventArgs e)
        {
            try //2024-06-19 Elie, add try catch to get exception.
            {
                //sender is the ‘bw’ object
                BackgroundWorker bwk = (BackgroundWorker)sender;
                //If arg is specified, you can get it with below code
                //myArgType arg = (myArgType)e.Argument;

                //Robert_Lin, 2024-6-4, do not call Add() to a ObervableCoeection<T> in a background thread,
                //It will cause an exception. Use a temp list instead.
                //

                //Robert_Lin, OLD Code:
                //ItemsCollection = new ObservableCollection<string>();
                //NEW code: create a temp list
                //List<string> tmpList = new List<string>();
                bool isUSB = false;
                IsUSBH = Visibility.Collapsed;
                NameHWidth = "0.5*";
                USBHWidth = "1.5*";
                NameHColumn = "1";
                InputTitle = Strings.InputTitle1;

                if (Cancelled_RefreshData(e, bwk))
                {
                    return;
                }

                if (InputSourceModule.SelectedHomeDevice.MonitorInfo.CapabilityDic.ContainsKey("E7"))
                {
                    InputTitle = Strings.InputTitle0;
                    IsUSBH = Visibility.Visible;
                    NameHWidth = "1*";
                    USBHWidth = "1*";
                    NameHColumn = "0";
                    isUSB = true;
                }

                if (Cancelled_RefreshData(e, bwk))
                {
                    return;
                }

                inputList = new Dictionary<string, InputInfo>();
                try
                {
                    inputList = DdpmCommonHelper.DeviceManagerSA.GetInputSourcelist(InputSourceModule.SelectedHomeDevice.MonitorInfo).Result;
                    usbUpstream = DdpmCommonHelper.DeviceManagerSA.GetUSBUpstreamList(InputSourceModule.SelectedHomeDevice.MonitorInfo).Result;
                    if (Cancelled_RefreshData(e, bwk))
                    {
                        return;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"InputSource caused crash = {ex.Message}");
                    _log?.Info($"InputSource caused crash = {ex.Message}");
                    inputList = null;
                    usbUpstream.Clear();
                }

                if (inputList != null)
                {
                    //_inputsList.Clear();
                    _inputsList = new List<InputSourceList>();
                    foreach (var item in inputList)
                    {
                        _inputsList.Add(new InputSourceList()
                        {
                            inputSource = item.Key,
                            inputName = item.Value.InputName,
                            //inputSourceModule = InputSourceModule,
                        });
                        if (Cancelled_RefreshData(e, bwk))
                        {
                            return;
                        }
                    }
                    InputsList = _inputsList;
                    string currentInput = DdpmCommonHelper.DeviceManagerSA.GetCurrentInput(InputSourceModule.SelectedHomeDevice.MonitorInfo, (Guid)guid, Priority.Middle).Result;
                    if (Cancelled_RefreshData(e, bwk))
                    {
                        return;
                    }
                    Debug.WriteLine($"[InputSourceViewModel]currentInput : " + currentInput);
                    _log?.Info($"[InputSourceViewModel]currentInput : " + currentInput);
                    InputSourceModule.SelectedHomeDevice.MonitorInfo.inputSource = currentInput;
                    _selectInput = _inputsList.Find(x => (x.inputSource == currentInput)); //_inputsList.Find(x => (x.inputSource == InputSourceModule.SelectedHomeDevice.MonitorInfo.inputSource));
                }
                else
                    return;//temp solution 0708

                //Robert_Lin, after tmpList build completed, assign back to ViewModel
                //NEW added:
                //ItemsCollection = new ObservableCollection<string>(tmpList);

                //ListViewGridView();
                //VcpCore.Common.InputInfo inputListInfo = new VcpCore.Common.InputInfo();
                //inputList.Add("123", inputListInfo);
                if (Cancelled_RefreshData(e, bwk))
                {
                    return;
                }
                items = new ObservableCollection<Item>();
                ObservableCollection<string> USBUpstream_ItemsCollection = new ObservableCollection<string>();
                if (inputList.Count != 0)
                {
                    foreach (var item in usbUpstream)
                    {
                        USBUpstream_ItemsCollection.Add(item.ToString());
                        if (Cancelled_RefreshData(e, bwk))
                        {
                            return;
                        }
                    }
                    int j = 0;
                    foreach (var input in inputList)
                    {
                        string strtUpstream = String.Empty;
                        InputSourceImage = InputTypeCommon.GetInputImage(input.Key);
                        input.Value.USBUpstream = DdpmCommonHelper.DeviceManagerSA.GetUSBUpstream(InputSourceModule.SelectedHomeDevice.MonitorInfo, input.Key).Result;
                        if (isUSB)
                        {
                            if (j == 0)
                            {
                                items.Add(new Item()
                                {
                                    Index = j,
                                    InputImage = InputSourceImage,
                                    InputType = input.Key,
                                    InputName = input.Value.InputName,
                                    USBUpstream = USBUpstream_ItemsCollection,
                                    UpstreamIndex = 0,
                                    IsUSBCB = Visibility.Visible,
                                    NameWidth = "1*",
                                    USBWidth = "1*",
                                    NameColumn = "0",
                                    NoGrey = false
                                });
                            }
                            else
                            {
                                items.Add(new Item()
                                {
                                    Index = j,
                                    InputImage = InputSourceImage,
                                    InputType = input.Key,
                                    InputName = input.Value.InputName,
                                    USBUpstream = USBUpstream_ItemsCollection,
                                    UpstreamIndex = 0,
                                    IsUSBCB = Visibility.Visible,
                                    NameWidth = "1*",
                                    USBWidth = "1*",
                                    NameColumn = "0",
                                    NoGrey = true
                                });
                            }
                        }
                        else
                        {
                            items.Add(new Item()
                            {
                                Index = j,
                                InputImage = InputSourceImage,
                                InputType = input.Key,
                                InputName = input.Value.InputName,
                                USBUpstream = USBUpstream_ItemsCollection,
                                UpstreamIndex = 0,
                                IsUSBCB = Visibility.Collapsed,
                                NameWidth = "0.5*",
                                USBWidth = "1.5*",
                                NameColumn = "1",
                                NoGrey = true
                            });
                        }
                        if (Cancelled_RefreshData(e, bwk))
                        {
                            return;
                        }
                        if (!String.IsNullOrEmpty(input.Value.USBUpstream))
                        {
                            int k = 0;
                            foreach (var item in usbUpstream)
                            {
                                if (item.StartsWith(input.Value.USBUpstream))
                                {
                                    strtUpstream = input.Value.USBUpstream;
                                    items[j].UpstreamIndex = k;
                                    break;
                                }
                                k++;
                                if (Cancelled_RefreshData(e, bwk))
                                {
                                    return;
                                }
                            }
                        }
                        j++;
                        if (Cancelled_RefreshData(e, bwk))
                        {
                            return;
                        }
                    }
                }
                OnPropertyChanged("NameHWidth");
                OnPropertyChanged("USBHWidth");
                OnPropertyChanged("InputTitle");
                OnPropertyChanged("NameHColumn");
                OnPropertyChanged("IsUSBH");
                OnPropertyChanged("Items_Selected");
                OnPropertyChanged("items"); //0607 Jason
                OnPropertyChanged("InputsList");

                //Lock/unlock data init here
                DDPMSettings data = DdpmCommonHelper.ReadDDPMSettings();//DeviceManagerSA.ReloadAppConfigData().Result;
                if (data != null)
                {
                    bool isLocked_current_input = data.LockSettings.Lock_Display_ActiveInputSource;
                }
            }
            catch (Exception)
            {
                ;
            }
        }

        private void RunWorkerCompleted_RefreshData(object sender, RunWorkerCompletedEventArgs e)
        {
            //Handling the result and final process
            IsBusy = false;
        }

        private bool Cancelled_RefreshData(DoWorkEventArgs e, BackgroundWorker bw)
        {
            if (bw.CancellationPending)
            {
                Debug.WriteLine("[InputSource] Cancelled_RefreshData.");
                _log?.Info("[InputSource] Cancelled_RefreshData.");
                e.Cancel = true;
                return true;
            }
            return false;
        }

        public void CallCancel()
        {
            if (bw != null && bw.IsBusy)
            {
                _log?.Info("[InputSource] CallCancel.");
                bw.CancelAsync();
                DdpmCommonHelper.DeviceManagerSA.CancelVcpTask((Guid)guid);
            }
        }

        public InputSourceViewModel()
        {
            _log = DdpmCommonHelper.MyConsole.CreateLog("InputSourceView");
            if (DdpmCommonHelper.DeviceManagerSA != null)
            {
                //OSD/VCP control back event
                DdpmCommonHelper.DeviceManagerSA.VCPchanged += OnVCPChangedEvent;
            }
        }

        ~InputSourceViewModel()
        {
            if (DdpmCommonHelper.DeviceManagerSA != null)
            {
                //OSD/VCP control back event
                DdpmCommonHelper.DeviceManagerSA.VCPchanged -= OnVCPChangedEvent;
            }
        }

        #region OnPropertyChanged
        public Task OnInputNameChange()
        {
            //IsBusy = true;
            _inputsList.Clear();
            _inputsList = new List<InputSourceList>();
            if (inputList != null &&
                inputList.Count > 0)
            {
                foreach (var item in inputList)
                {
                    _inputsList.Add(new InputSourceList()
                    {
                        inputSource = item.Key,
                        inputName = item.Value.InputName
                    });
                }
                InputsList = _inputsList;
                _selectInput = _inputsList.Find(x => (x.inputSource == InputSourceModule.SelectedHomeDevice.MonitorInfo.inputSource));
                OnPropertyChanged("Items_Selected");
                OnPropertyChanged("InputsList");
                DdpmCommonHelper.bInputSourceRenamed = true;
            }

            return Task.CompletedTask;
        }
        #endregion

        #region Event
        /// <summary>
        /// Catch OSD menu event
        /// </summary>
        /// <param name="sender">object type</param>
        /// <param name="e">changed event</param>
        private void OnVCPChangedEvent(object? sender, VCPchangedEventArgs e)
        {
            if (e.vcpcode == null || e.value == null)
            {
                Debug.WriteLine("[InputSource] OnVCPChangedEvent: vcpcode or value is null");
                return;
            }
            Trace.WriteLine("[InputSource] OnVCPChangedEvent : " + e.vcpcode);
            if (e.vcpcode.Equals("input select")) //input source change 0x52 event
            {
                if (InputsList.Count > 0)
                {
                    int idx = InputsList.FindIndex(x => x.inputSource == e.value);
                    if (idx >= 0)
                    {
                        InputSourceList item = InputsList[idx];
                        _selectInput = item;
                        foreach (var device in ModuleOwner.HomeDevices)
                        {
                            if (device.MonitorInfo.edid.ServiceTag == e.monitor.edid.ServiceTag &&
                                device.MonitorInfo.edid.SerialNumber == e.monitor.edid.SerialNumber)
                            {
                                device.MonitorInfo.inputSource = e.value;
                            }
                        }
                        OnPropertyChanged("Items_Selected");
                    }
                }
            }
            else if (e.vcpcode.Equals("E7"))
            {
                items = new ObservableCollection<Item>();
                inputList = DdpmCommonHelper.DeviceManagerSA.GetInputSourcelist(InputSourceModule.SelectedHomeDevice.MonitorInfo).Result;
                usbUpstream = DdpmCommonHelper.DeviceManagerSA.GetUSBUpstreamList(InputSourceModule.SelectedHomeDevice.MonitorInfo).Result;
                ObservableCollection<string> USBUpstream_ItemsCollection = new ObservableCollection<string>();
                if (inputList.Count != 0)
                {
                    foreach (var item in usbUpstream)
                    {
                        USBUpstream_ItemsCollection.Add(item.ToString());
                    }
                    foreach (var item in items)
                    {
                        item.USBUpstream = USBUpstream_ItemsCollection;
                    }
                    int j = 0;
                    foreach (var input in inputList)
                    {
                        string strtUpstream = String.Empty;
                        InputSourceImage = InputTypeCommon.GetInputImage(input.Key);
                        input.Value.USBUpstream = DdpmCommonHelper.DeviceManagerSA.GetUSBUpstream(InputSourceModule.SelectedHomeDevice.MonitorInfo, input.Key).Result;
                        if (j == 0)
                        {
                            items.Add(new Item()
                            {
                                Index = j,
                                InputImage = InputSourceImage,
                                InputType = input.Key,
                                InputName = input.Value.InputName,
                                USBUpstream = USBUpstream_ItemsCollection,
                                UpstreamIndex = 0,
                                IsUSBCB = Visibility.Visible,
                                NameWidth = "1*",
                                USBWidth = "1*",
                                NameColumn = "0",
                                NoGrey = false
                            });
                        }
                        else
                        {
                            items.Add(new Item()
                            {
                                Index = j,
                                InputImage = InputSourceImage,
                                InputType = input.Key,
                                InputName = input.Value.InputName,
                                USBUpstream = USBUpstream_ItemsCollection,
                                UpstreamIndex = 0,
                                IsUSBCB = Visibility.Visible,
                                NameWidth = "1*",
                                USBWidth = "1*",
                                NameColumn = "0",
                                NoGrey = true
                            });
                        }

                        if (!String.IsNullOrEmpty(input.Value.USBUpstream))
                        {
                            int k = 0;
                            foreach (var item in usbUpstream)
                            {
                                if (item.StartsWith(input.Value.USBUpstream))
                                {
                                    strtUpstream = input.Value.USBUpstream;
                                    items[j].UpstreamIndex = k;
                                    break;
                                }
                                k++;
                            }
                        }
                        j++;
                    }
                    OnPropertyChanged("items");
                }
            }
        }
        #endregion
    }
}