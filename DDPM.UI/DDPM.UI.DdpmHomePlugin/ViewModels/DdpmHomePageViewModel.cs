using CommunityToolkit.Mvvm.ComponentModel;
using DDPM.SA.Common;
using DDPM.UI.Common;
using DDPM.UI.Common.Models;
using DDPM.UI.Plugin.DdpmHomePlugin.Interfaces;
using DDPM.UI.Plugin.DdpmHomePlugin.Model;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using DPeMPublic.Common.Enums;
using Microsoft;
using System.Collections.ObjectModel;
using System.Windows.Data;
using System.Windows.Documents;
using VcpCore.Common;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;
using System.ComponentModel;
using System.Windows.Forms;
using Windows.Gaming.Input;
using System.Diagnostics;

namespace DDPM.UI.Plugin.DdpmHomePlugin.ViewModels
{
    public class DdpmHomePageViewModel : ObservableObject, IDdpmHomePageViewModel
    {
        private object _LockList = new object();
        private object _LockPeripheralList = new object();
        private readonly IConsole _console;
        private readonly ILog _log;

        private ObservableCollection<HomeDevice> _homeDevices = new ObservableCollection<HomeDevice>();
        private HomeDevice? _selectedHomeDevice;

        /// <summary>
        /// Default constructor
        /// </summary>
        public DdpmHomePageViewModel(IConsole console, ILog log/*, IDeviceManagerSA devMgr*/)
        {
            Requires.NotNull(console, nameof(console));
            Requires.NotNull(log, nameof(log));
            //Requires.NotNull(deviceInfoService, nameof(deviceInfoService));

            _console = console;
            _log = log;
            _connectButtonClickCommand = new RelayCommand(HandleConnectButtonClickCommand);
        }

        public ILog Log => _log;

        public ObservableCollection<HomeDevice> HomeDevices
        {
            get => _homeDevices;
            set
            {
                SetProperty(ref _homeDevices, value);
                OnPropertyChanged("HomeDeviceCount");

                if (HomeDevicesChanged != null)
                {
                     HomeDevicesChanged.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public HomeDevice? SelectedHomeDevice
        {
            get => _selectedHomeDevice;
            set => SetProperty(ref _selectedHomeDevice, value);
        }

        public int HomeDeviceCount
        {
            get => HomeDevices.Count;
        }

        /// <summary>
        /// Input list of MonitorInfo, convert to HomeDevice and add to HomeDevices
        /// </summary>
        /// <param name="monitorInfos"></param>
        public void PrepareMonitorInfos(List<MonitorInfo> monitorInfos)
        {
            lock (_LockList)
            {
                //Robert_Lin 2024-5-16 This method should be called once, provide all
                //monitor in this call. So it will clear original list at first
                //Original code, which will append.
                //List<HomeDevice> tempList = new List<HomeDevice>(HomeDevices.ToList());
                //New code, which will replace with new list
                List<HomeDevice> tempList = new List<HomeDevice>();

                //Workaround to build a tempList then assign to ViewModel.HomeDevices
                //To avoid exception (unknown reason)

                foreach (MonitorInfo mi in monitorInfos)
                {

                    //Workaround to get InputSource of Dell Monitor
                    //
                    //byte b_vcpcode = Convert.ToByte("60", 16);
                    //string currentInput = ""; //DdpmCommonHelper.DeviceManagerSA.GetCurrentInput(mi, b_vcpcode, 0).Result;

                    HomeDevice dev = new HomeDevice()
                    {
                        DeviceName = mi.AliasDeviceName,
                        DeviceCategory = eDeviceCategory.Display,
                        MonitorInfo = mi,
                        //Text1 = currentInput,
                        DeviceImage = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Product_Display.png")
                    };

                    //2024-6-20 Robert_Lin, check if any some model already in list
                    List<HomeDevice> sameModel = tempList.FindAll(x => x.IsSameModel(dev));
                    if (sameModel.Any())
                    {
                        //Assign InstanceNo
                        int instanceNo = 1;
                        foreach (HomeDevice hd in sameModel)
                        {
                            hd.InstanceNo = instanceNo;
                            instanceNo++;
                        }
                        dev.InstanceNo = instanceNo;
                    }


                    //_homeDevices.Add(dev);
                    tempList.Add(dev);

                    //2024-5-8 Robert_Lin to validate RWD in HomePage, limit the device count=1
                    //break;
                }

                //Robert_Lin, 2024-7-10, Sort by DisplayName
                tempList.Sort((x,y) => x.DisplayName.CompareTo(y.DisplayName));

                //Assign SortOrder
                int orderBase = (int)eDeviceCategory.Display;
                int orderIndex = 0;
                const int orderMul = 10;
                foreach(HomeDevice dev in tempList)
                {
                    dev.SortOrder = orderBase + orderMul * orderIndex;
                    orderIndex++;
                }

                //OnPropertyChanged("HomeDevices");
                HomeDevices = new ObservableCollection<HomeDevice>(tempList);
                //Robert_Lin, 2024-8-7, The list has been sorted in PrepareXXX(), so should not call to RefreshCollectionView()
                //Robert_Lin, 2024-6-22, to fix the issue the WebCam not been sorted (expect arranged after monitors)
                //RefreshCollectionView();
            }
           
        }

        public void PrepareDeviceInfos(List<DeviceInfo> deviceInfos)
        {

            lock (_LockPeripheralList)
            {            
                //Robert_Lin 2024-7-10 modify for HomePage Sort and Grouping
                //Sort the deviceInfos with DeviceInfo.Name (HomeDevice.TooltipModelName)
                deviceInfos.Sort((x,y) => x.Name.CompareTo(y.Name));

                //Determine the SortOrder in the foreach loop.
                //Each Category have their index, would be in order of ModelNumber
                int idxWebcam = 0, idxKB = 0, idxMouse = 0,
                    idxPen = 0, idxHeadset = 0, idxSpeaker = 0, idxDock = 0;


                //Robert_Lin 2024-5-16 This method should be called once, provide all
                //monitor in this call. So it will clear original list at first
                //Original code, which will append.
                List<HomeDevice> tempList = new List<HomeDevice>(HomeDevices.ToList());
                //New code, which will replace with new list
                //List<HomeDevice> tempList = new List<HomeDevice>();

                foreach (DeviceInfo di in deviceInfos)
                {
                    //Check if duplicate device is existing in list already
                    HomeDevice? dupDev = tempList.Find(x => x.IsSamePeripheralDevice(di));
                    if (dupDev != null)
                    {
                        if (_log != null)
                        {
                            _log.Info($"Duplication Peripheral found! GUID=[{di.PhyscialDeviceID}], Type=[{di.Type}], Name={di.Name}");
                            _log.Info($"DeviceToBeAdded:  Name=[{di.Name}], Type=[{di.Type}], PhyscialDeviceID=[{di.PhyscialDeviceID}]");
                            _log.Info($"DeviceDuplicated: Name=[{dupDev.DeviceInfo.Name}], Type=[{dupDev.DeviceInfo.Type}], PhyscialDeviceID=[{dupDev.DeviceInfo.PhyscialDeviceID}]");
                        }
                        continue;
                    }

                    if (!di.IsConnected)
                        continue;


                    HomeDevice dev = new HomeDevice()
                    {
                        DeviceName = di.DeviceName,
                        DeviceInfo = di
                    };

                    //2024-6-20, Peripherals DeviceImage will be determined by HomeDevice internally.
                    //Upper owner just set DeviceInfo to HomeDevice can trigger it to load the product image.

                    //Apply device category
                    DeviceType devType = di.Type;
                    if (devType.ToString().Contains("Keyboard"))
                    {
                        dev.DeviceCategory = eDeviceCategory.KB;
                        //dev.DeviceImage = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Product_KB900.png");
                        dev.SortOrder = (int)dev.DeviceCategory + idxKB;
                        idxKB++;
                    }
                    else if (devType.ToString().Contains("Mouse"))
                    {
                        dev.DeviceCategory = eDeviceCategory.Mouse;
                        dev.SortOrder = (int)dev.DeviceCategory + idxMouse;
                        idxMouse++;
                        //dev.DeviceImage = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Product_Mouse.png");
                    }
          // 240722 Added by Hess to support Pen
          else if(devType.ToString().Contains("Pen")) {
            dev.DeviceCategory = eDeviceCategory.Pen;
            dev.SortOrder = (int)dev.DeviceCategory + idxPen;
            idxPen++;
          }
          //0710 Jim 修改WebCamera
          else if (devType.ToString().Contains("Webcam"))
                    {
                        string imagepath = "";
                        switch (di.ModelNumber)
                        {
                            case "WB7022":   // external webcamera
                                imagepath = "Resources/WebCamModel_WB7022_Small.png";
                                break;
                            case "WB5023":   // external webcamera
                                imagepath = "Resources/WebCamModel_WB5023_Small.png";
                                break;
                            case "WB3023":    // external webcamera
                                imagepath = "Resources/WebCamModel_WB3023_Small.png";
                                break;
                            case "U3224KB": // internal webcamera
                                imagepath = "Resources/WebCamModel_U3224KB_Small.png";
                                break;
                            case "P2424HEB": //internal webcamera
                                imagepath = "Resources/WebCamModel_P2424HEB_Small.png";
                                break;
                            case "U3223QZ": //internal webcamera
                                imagepath = "Resources/WebCamModel_U3223QZ_Small.png";
                                break;
                            default:
                                imagepath = "Resources/WebCamera.png";
                                break;
                        }

                        dev.DeviceCategory = eDeviceCategory.Webcam;
                        dev.DeviceImage = DdpmCommonHelper.GetImageSourceFromCommonResource(imagepath);
                        //dev.DeviceImage = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Product_WB7022.png");

                        //Robert_Lin, 2024-7-10, Find if any integrated Monitor (Same ModelNumber)
                        //We cannot identify which Webcam is integrated with which Monitor, so we find the first.
                        string webCamModel = di.ModelNumber;
                        HomeDevice? integratedMonitor = tempList.Find(x => (x.DeviceCategory == eDeviceCategory.Display) && (x.IsSameModel(webCamModel)));
                        if (integratedMonitor != null)
                        {
                            if (integratedMonitor.MonitorInfo != null)
                            {
                                dev.MonitorIndexOfIntegratedPeripheral = integratedMonitor.MonitorInfo.Index;
                                dev.SortOrder = integratedMonitor.SortOrder + 1 + idxWebcam;
                                idxWebcam++;
                            }
                            else
                            {
                                dev.SortOrder = (int)dev.DeviceCategory + idxWebcam;
                                idxWebcam++;
                            }
                        }
                        else
                        {
                            dev.SortOrder = (int)dev.DeviceCategory + idxWebcam;
                            idxWebcam++;
                        }

                    }
                    //0614 Bruce 新增Dock UI
                    else if (devType.ToString().ToUpper().Contains("DOCK") ||
                        (devType.ToString().ToUpper().Contains("23")) )
                    {
                        dev.DeviceCategory = eDeviceCategory.Dock;
                        //dev.DeviceImage = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/WD22TB4.png");
                        dev.SortOrder = (int)dev.DeviceCategory + idxDock;
                        idxDock++;
                    }
                    //0618 Wayn 新增HeadSet
                    else if (devType.ToString().ToUpper().Contains("HEADSET"))
                    {
                        string imagepath = "";
                        switch (di.ModelNumber)
                        {
                            case "WL7024":
                                imagepath = "Resources/HeadsetModel_WL7024-Mito.png";
                                break;
                            case "WL5024":
                                imagepath = "Resources/HeadsetModel_WL5024-Pegasus.png";
                                break;
                            case "WH5024":
                                imagepath = "Resources/HeadsetModel_WH5024-Vinflo.png";
                                break;
                            case "WL3024":
                                imagepath = "Resources/HeadsetModel_WL3024-Vaporfly.png";
                                break;
                            case "WH3024":
                                imagepath = "Resources/HeadsetModel_WH3024-Airmax.png";
                                break;
                            default:
                                imagepath = "Resources/HeadsetModel_WL7024-Mito.png";
                                break;
                        }
                        dev.DeviceCategory = eDeviceCategory.Headset;
                        dev.DeviceImage = DdpmCommonHelper.GetImageSourceFromCommonResource(imagepath);
                        dev.SortOrder = (int)dev.DeviceCategory + idxHeadset;
                        idxHeadset++;
                    }

                    //0726 Wayn 新增Soundbar/Speaker LogicalWiredAudio
                    else if (devType.ToString().ToUpper().Contains("LOGICALWIREDAUDIO"))
                    {
                        string imagepath = "";
                        switch (di.ModelNumber)
                        {
                            case "SP3022":
                                imagepath = "Resources/Speaker_SP3022.png";
                                break;
                            case "u2723qe":
                                imagepath = "Resources/Speaker_u2723qe.png";
                                break;
                            default:
                                imagepath = "Resources/Speaker_SP3022.png";
                                break;
                        }
                        dev.DeviceCategory = eDeviceCategory.Soundbar;
                        dev.DeviceImage = DdpmCommonHelper.GetImageSourceFromCommonResource(imagepath);
                        dev.SortOrder = (int)dev.DeviceCategory + idxSpeaker;
                        idxSpeaker++;
                    }

                    //Robert_Lin, 2024-8-6, assign InstanceNo for the new adding device (dev)
                    //
                    List<HomeDevice> sameModel = tempList.FindAll(x => x.IsSamePeripheralModel(dev));
                    if (sameModel.Any())
                    {
                        //Assign InstanceNo
                        int instanceNo = 1;
                        foreach (HomeDevice hd in sameModel)
                        {
                            hd.InstanceNo = instanceNo;
                            instanceNo++;
                        }
                        dev.InstanceNo = instanceNo;
                    }

                    //_homeDevices.Add(dev);
                    tempList.Add(dev);

                }
                //OnPropertyChanged("HomeDevices");

                //Sort the list with SortOrder
                tempList.Sort((x,y) =>  x.SortOrder.CompareTo(y.SortOrder));

                HomeDevices = new ObservableCollection<HomeDevice>(tempList);

                //Robert_Lin, 2024-7-10, we don't need the CollectionView, we sort in List<HomeDevice> directly.
                //Robert_Lin, 2024-6-22, to fix the issue the WebCam not been sorted (expect arranged after monitors)
                //RefreshCollectionView();
            }
            
        }

        public void ResetDevices()
        {
            HomeDevices = new ObservableCollection<HomeDevice>();
        }


        #region Refresh CollectionView
        /// <summary>
        /// Refresh the collection view, it should be called when the ListView content is changed.
        /// Robert_Lin, 2024-6-22, to fix the issue the WebCam not been sorted (expect arranged after monitors)
        /// NOT been verified, it this call will cause exception, then we should move it to UI thread. (add in a Dispatch Invoke)
        /// </summary>
        public void RefreshCollectionView()
        {
            ListCollectionView colView = (ListCollectionView)CollectionViewSource.GetDefaultView(HomeDevices);

            //DEBUG, sort by DisplayName
            //colView.SortDescriptions.Clear();
            //colView.SortDescriptions.Add(new SortDescription("DisplayName", ListSortDirection.Descending));

            colView.CustomSort = new HomeDeviceSorter();

            //colView.GroupDescriptions.Clear();
            //PropertyGroupDescription groupDesc = new PropertyGroupDescription("DisplayName");
            //colView.GroupDescriptions.Add(groupDesc);

            colView.Refresh();

            List<HomeDevice> listSorted = colView.Cast<HomeDevice>().ToList();
            //listSorted would be sorted by SortOredr
            // 

            HomeDevices = new ObservableCollection<HomeDevice>(listSorted);
        }
        #endregion

        #region HomeDevices Changed event

        /// <summary>
        /// To fix the issue: The first time startup, the HomePage RWD is not correct, for example, there are 4 devices,
        /// but the display listview is not 4 items per row.
        /// Possible root casuse: The HomeDevices are refreshed to ViewModel.HomeDevices, but View is not notified to
        /// recalculate the RWD (item width).
        /// </summary>
        /// <param name="device"></param>
        public event EventHandler HomeDevicesChanged;
        #endregion

        #region Add your first device
        private ICommand? _connectButtonClickCommand;
        public ICommand? ConnectButtonClickCommand
        {
            get => _connectButtonClickCommand;
            set => SetProperty(ref _connectButtonClickCommand, value);
        }
        private void HandleConnectButtonClickCommand()
        {
            if (_console != null)
            {
                EventManagerArgs args = new EventManagerArgs();
                _console.RaiseEvent("ShowAddDevicePlugin", this, args);
            }
        }
        public void RaiseShowAddDevicePlugin()
        {
            if (_console != null)
            {
                EventManagerArgs args = new EventManagerArgs();
                _console.RaiseEvent("ShowAddDevicePlugin", this, args);
            }
        }
        #endregion

        #region For Developer's debug
        public void AddDemoHomeDevice(HomeDevice device)
        {
            HomeDevices.Add(device);
        }

        private double _cxItem;
        public double cxItem
        {
            get => _cxItem;
            set => SetProperty(ref _cxItem, value);
        }
        #endregion

        #region Please Wait
        private bool _isPleaseWaitVisible = true;
        public bool IsPleaseWaitVisible
        {
            get => _isPleaseWaitVisible;
            set => SetProperty(ref _isPleaseWaitVisible, value);
        }

        public void Invoke_PleaseWait()
        {
            BackgroundWorker bw = new BackgroundWorker
            {
                WorkerReportsProgress = false,
                WorkerSupportsCancellation = false
            };
            bw.DoWork += DoWork_PleaseWait;
            bw.RunWorkerCompleted += RunWorkerCompleted_PleaseWait;

            IsPleaseWaitVisible = true;
            bw.RunWorkerAsync();
        }
        private void DoWork_PleaseWait(object? sender, DoWorkEventArgs e)
        {
            Thread.Sleep(1500);

            Stopwatch sw = new Stopwatch();
            sw.Start();
            int timeoutMsec = 10000;
            while (HomeDeviceCount == 0)
            {
                Thread.Sleep(500);
                if (sw.ElapsedMilliseconds > timeoutMsec)
                    break;
            }
            sw.Stop();
        }
        private void RunWorkerCompleted_PleaseWait(object sender, RunWorkerCompletedEventArgs e)
        {
            IsPleaseWaitVisible = false;
        }
        #endregion
    }
}
