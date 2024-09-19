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

namespace DDPM.UI.Module.InputSource
{
    public class InputSourceList
    {
        public string inputSource = String.Empty;
        public InputSourceModule inputSourceModule { get; set; }

        public string inputDisplayText
        {
            get
            {
                return inputSource;
            }
        }
    }

    public class InputSourceViewModel : ObservableObject
    {
        private InputSourceList _selectInput = new InputSourceList();
        private List<InputSourceList> _inputsList = new List<InputSourceList>();
        private ImageSource? _inputImage;

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

        public ImageSource? InputSourceImage
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

                if (InputSourceModule.SelectedHomeDevice.MonitorInfo.CapabilityDic.ContainsKey("EE"))
                {
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
                    foreach (string item in inputList.Keys)
                    {
                        _inputsList.Add(new InputSourceList()
                        {
                            inputSource = item,
                            inputSourceModule = InputSourceModule,
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
                        if (input.Key.StartsWith("HDMI"))
                        {
                            InputSourceImage = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/HDMI.png");
                        }
                        else if (input.Key.StartsWith("USB-C") || input.Key.StartsWith("Thunderbolt"))
                        {
                            InputSourceImage = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/USB-C.png");
                        }
                        else if (input.Key.StartsWith("DisplayPort"))
                        {
                            InputSourceImage = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/DP.png");
                        }
                        if (isUSB)
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
                                NameColumn = "0"
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
                                IsUSBCB = Visibility.Collapsed,
                                NameWidth = "0.5*",
                                USBWidth = "1.5*",
                                NameColumn = "1"
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
                DDPMSettings data = DdpmCommonHelper.DeviceManagerSA.ReloadAppConfigData().Result;
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
    }
}