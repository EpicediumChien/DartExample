using CommunityToolkit.Mvvm.ComponentModel;
using DDPM.SA.Common;
using DDPM.SA.Common.Settings;
using DDPM.UI.Common;
using DDPM.UI.Common.Interfaces;
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

                if (InputSourceModule.SelectedHomeDevice.MonitorInfo.CapabilityDic.ContainsKey("E7"))
                {
                    InputTitle = Strings.InputTitle0;
                    IsUSBH = Visibility.Visible;
                    NameHWidth = "1*";
                    USBHWidth = "1*";
                    NameHColumn = "0";
                    isUSB = true;
                }

                inputList = new Dictionary<string, InputInfo>();
                try
                {
                    inputList = DdpmCommonHelper.DeviceManagerSA.GetInputSourcelist(InputSourceModule.SelectedHomeDevice.MonitorInfo).Result;
                    usbUpstream = DdpmCommonHelper.DeviceManagerSA.GetUSBUpstreamList(InputSourceModule.SelectedHomeDevice.MonitorInfo).Result;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"InputSource caused crash = {ex.Message}");
                    inputList = null;
                    usbUpstream.Clear();
                }

                if (inputList != null)
                {
                    _inputsList.Clear();
                    foreach (var item in inputList)
                    {
                        _inputsList.Add(new InputSourceList()
                        {
                            inputSource = item.Key,
                            inputName = item.Value.InputName,
                            //inputSourceModule = InputSourceModule,
                        });
                    }
                    InputsList = _inputsList;
                    _selectInput = _inputsList.Find(x => (x.inputSource == InputSourceModule.SelectedHomeDevice.MonitorInfo.inputSource));
                }
                else
                    return;//temp solution 0708

                //Robert_Lin, after tmpList build completed, assign back to ViewModel
                //NEW added:
                //ItemsCollection = new ObservableCollection<string>(tmpList);

                //ListViewGridView();
                //VcpCore.Common.InputInfo inputListInfo = new VcpCore.Common.InputInfo();
                //inputList.Add("123", inputListInfo);

                items = new ObservableCollection<Item>();
                ObservableCollection<string> USBUpstream_ItemsCollection = new ObservableCollection<string>();
                if (inputList.Count != 0)
                {
                    foreach (var item in usbUpstream)
                    {
                        USBUpstream_ItemsCollection.Add(item.ToString());
                    }
                    int j = 0;
                    foreach (var input in inputList)
                    {
                        string strtUpstream = String.Empty;
                        InputSourceImage = InputTypeCommon.GetInputImage(input.Key);
                        input.Value.USBUpstream = DdpmCommonHelper.DeviceManagerSA.GetUSBUpstream(InputSourceModule.SelectedHomeDevice.MonitorInfo, input.Key).Result;
                        //if (input.Key.StartsWith("VGA"))
                        //{
                        //    InputSourceImage = "M3 0C1.34315 0 0 1.34315 0 3V17C0 18.6569 1.34314 20 3 20H46.3333C47.9902 20 49.3333 18.6569 49.3333 17V3C49.3333 1.34315 47.9902 0 46.3333 0H3ZM7.97651 2.66667C6.65025 2.66667 5.69131 3.934 6.05181 5.21032L9.06472 15.877C9.30806 16.7385 10.0942 17.3333 10.9894 17.3333H38.3437C39.2389 17.3333 40.0251 16.7385 40.2684 15.877L43.2815 5.21034C43.642 3.93402 42.6831 2.66667 41.3568 2.66667H7.97651ZM48 10C48 11.1046 47.1046 12 46 12C44.8954 12 44 11.1046 44 10C44 8.89543 44.8954 8 46 8C47.1046 8 48 8.89543 48 10ZM3.33333 12C4.4379 12 5.33333 11.1046 5.33333 10C5.33333 8.89543 4.4379 8 3.33333 8C2.22876 8 1.33333 8.89543 1.33333 10C1.33333 11.1046 2.22876 12 3.33333 12Z";
                        //}
                        //else if (input.Key.StartsWith("DVI"))
                        //{
                        //    InputSourceImage = "M2 0C0.895431 0 0 0.895431 0 2V18C0 19.1046 0.895431 20 2 20H42C43.1046 20 44 19.1046 44 18V2C44 0.895431 43.1046 0 42 0H2ZM36 4H8V16H36V4ZM5 10C5 11.1046 4.10457 12 3 12C1.89543 12 1 11.1046 1 10C1 8.89543 1.89543 8 3 8C4.10457 8 5 8.89543 5 10ZM41 12C42.1046 12 43 11.1046 43 10C43 8.89543 42.1046 8 41 8C39.8954 8 39 8.89543 39 10C39 11.1046 39.8954 12 41 12Z";
                        //}
                        //else if (input.Key.StartsWith("HDMI"))
                        //{
                        //    //InputSourceImage = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/HDMI.png");
                        //    InputSourceImage = "M2.5 0.197266C1.39543 0.197266 0.5 1.0927 0.5 2.19727V6.58877C0.5 7.27835 0.855239 7.91929 1.44 8.28477L12.0136 14.8933C12.3315 15.0919 12.6988 15.1973 13.0736 15.1973H39.9264C40.3012 15.1973 40.6685 15.0919 40.9864 14.8933L51.56 8.28477C52.1448 7.91929 52.5 7.27835 52.5 6.58877V2.19727C52.5 1.0927 51.6046 0.197266 50.5 0.197266H2.5ZM14 7.19727C13.7239 7.19727 13.5 7.42112 13.5 7.69727C13.5 7.97341 13.7239 8.19727 14 8.19727H40C40.2761 8.19727 40.5 7.97341 40.5 7.69727C40.5 7.42112 40.2761 7.19727 40 7.19727H14Z";
                        //}
                        //else if (input.Key.StartsWith("USB-C") || input.Key.StartsWith("Thunderbolt"))
                        //{
                        //    //InputSourceImage = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/USB-C.png");
                        //    InputSourceImage = "M6 0.394531C2.96243 0.394531 0.5 2.85697 0.5 5.89453C0.5 8.9321 2.96243 11.3945 6 11.3945H25C28.0376 11.3945 30.5 8.9321 30.5 5.89453C30.5 2.85697 28.0376 0.394531 25 0.394531H6ZM7 5.39453C6.72386 5.39453 6.5 5.61839 6.5 5.89453C6.5 6.17067 6.72386 6.39453 7 6.39453H24C24.2761 6.39453 24.5 6.17067 24.5 5.89453C24.5 5.61839 24.2761 5.39453 24 5.39453H7Z";
                        //}
                        //else if (input.Key.StartsWith("DisplayPort"))
                        //{
                        //    //InputSourceImage = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/DP.png");
                        //    InputSourceImage = "M2.5 0.183594C1.39543 0.183594 0.5 1.07902 0.5 2.18359V9.55121C0.5 10.2537 0.868598 10.9048 1.47101 11.2662L7.52498 14.8986C7.83581 15.0851 8.19148 15.1836 8.55397 15.1836H50.5C51.6046 15.1836 52.5 14.2882 52.5 13.1836V7.68359V2.18359C52.5 1.07902 51.6046 0.183594 50.5 0.183594H2.5ZM14 7.18359C13.7239 7.18359 13.5 7.40745 13.5 7.68359C13.5 7.95974 13.7239 8.18359 14 8.18359H40C40.2761 8.18359 40.5 7.95974 40.5 7.68359C40.5 7.40745 40.2761 7.18359 40 7.18359H14Z";
                        //}
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
                }
                OnPropertyChanged("Items_Selected");
                OnPropertyChanged("items"); //0607 Jason

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

        public InputSourceViewModel()
        {
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